namespace BossMod.Stormblood.Ultimate.UCOB;

class Quote(BossModule module) : BossComponent(module)
{
    public Actor? Source;
    public List<AID> PendingMechanics = [];
    public DateTime NextActivation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (PendingMechanics.Count > 0)
        {
            hints.Add(string.Join(" > ", PendingMechanics.Select(aid => aid switch
            {
                AID.IronChariot => "Out",
                AID.LunarDynamo => "In",
                AID.ThermionicBeam => "Stack",
                AID.RavenDive or AID.MeteorStream => "Spread",
                AID.DalamudDive => "Tankbuster",
                _ => "???"
            })));
        }
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        List<AID> aids = id switch
        {
            6492 => [AID.LunarDynamo, AID.IronChariot],
            6493 => [AID.LunarDynamo, AID.ThermionicBeam],
            6494 => [AID.ThermionicBeam, AID.IronChariot],
            6495 => [AID.ThermionicBeam, AID.LunarDynamo],
            6496 => [AID.RavenDive, AID.IronChariot],
            6497 => [AID.RavenDive, AID.LunarDynamo],
            6500 => [AID.MeteorStream, AID.DalamudDive],
            6501 => [AID.DalamudDive, AID.ThermionicBeam],
            6502 => [AID.RavenDive, AID.LunarDynamo, AID.MeteorStream],
            6503 => [AID.LunarDynamo, AID.RavenDive, AID.MeteorStream],
            6504 => [AID.IronChariot, AID.ThermionicBeam, AID.RavenDive],
            6505 => [AID.IronChariot, AID.RavenDive, AID.ThermionicBeam],
            6506 => [AID.LunarDynamo, AID.RavenDive, AID.ThermionicBeam],
            6507 => [AID.LunarDynamo, AID.IronChariot, AID.RavenDive],
            _ => []
        };
        if (aids.Count > 0)
        {
            Source = actor;
            PendingMechanics = aids;
            NextActivation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (PendingMechanics.Count > 0 && (AID)spell.Action.ID == PendingMechanics[0])
        {
            PendingMechanics.RemoveAt(0);
            NextActivation = WorldState.FutureTime(3.1f);
        }
    }
}

class QuoteIronChariotLunarDynamo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    private static readonly AOEShapeCircle _shapeChariot = new(8.55f);
    private static readonly AOEShapeDonut _shapeDynamo = new(6, 22); // TODO: verify inner radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        AOEShape? shape = _quote != null && _quote.PendingMechanics.Count > 0 ? _quote.PendingMechanics[0] switch
        {
            AID.IronChariot => _shapeChariot,
            AID.LunarDynamo => _shapeDynamo,
            _ => null
        } : null;
        if (shape != null && _quote?.Source != null)
            yield return new(shape, _quote.Source.Position, default, _quote.NextActivation);
    }
}

class QuoteThermionicBeam(BossModule module) : Components.UniformStackSpread(module, 4, 0, 8)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    public override void Update()
    {
        bool stackImminent = _quote != null && _quote.PendingMechanics.Count > 0 && _quote.PendingMechanics[0] == AID.ThermionicBeam;
        if (stackImminent && Stacks.Count == 0 && Raid.Player() is var target && target != null) // note: target is random
            AddStack(target, _quote!.NextActivation);
        else if (!stackImminent && Stacks.Count > 0)
            Stacks.Clear();
        base.Update();
    }
}

class QuoteRavenDive(BossModule module) : Components.UniformStackSpread(module, 0, 3, alwaysShowSpreads: true)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    public override void Update()
    {
        bool spreadImminent = _quote != null && _quote.PendingMechanics.Count > 0 && _quote.PendingMechanics[0] == AID.RavenDive;
        if (spreadImminent && Spreads.Count == 0)
            AddSpreads(Raid.WithoutSlot(true), _quote!.NextActivation);
        else if (!spreadImminent && Spreads.Count > 0)
            Spreads.Clear();
        base.Update();
    }
}

class QuoteMeteorStream(BossModule module) : Components.UniformStackSpread(module, 0, 4, alwaysShowSpreads: true)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    public override void Update()
    {
        bool spreadImminent = _quote != null && _quote.PendingMechanics.Count > 0 && _quote.PendingMechanics[0] == AID.MeteorStream;
        if (spreadImminent && Spreads.Count == 0)
            AddSpreads(Raid.WithoutSlot(true), _quote!.NextActivation);
        else if (!spreadImminent && Spreads.Count > 0)
            Spreads.Clear();
        base.Update();
    }
}

class QuoteDalamudDive(BossModule module) : Components.GenericBaitAway(module, AID.DalamudDive, true, true)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    private static readonly AOEShapeCircle _shape = new(5);

    public override void Update()
    {
        bool imminent = _quote != null && _quote.PendingMechanics.Count > 0 && _quote.PendingMechanics[0] == AID.DalamudDive;
        if (imminent && CurrentBaits.Count == 0 && Module.Enemies(OID.NaelDeusDarnus).FirstOrDefault() is var source && WorldState.Actors.Find(source?.TargetID ?? 0) is var target && target != null)
            CurrentBaits.Add(new(target, target, _shape));
        else if (!imminent && CurrentBaits.Count > 0)
            CurrentBaits.Clear();
    }
}
