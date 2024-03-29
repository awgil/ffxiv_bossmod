namespace BossMod.Endwalker.Savage.P9SKokytos;

class Charibdys : Components.PersistentVoidzoneAtCastTarget
{
    public Charibdys() : base(6, ActionID.MakeSpell(AID.CharybdisAOE), m => m.Enemies(OID.Charybdis).Where(v => v.EventState != 7), 0.6f) { }
}

class Comet : Components.Adds
{
    public static bool IsActive(Actor c) => c.ModelState.AnimState2 == 1;
    public static bool IsFinished(Actor c) => c.IsDead || c.CastInfo != null;

    public Comet() : base((uint)OID.Comet) { }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var c in Actors.Where(a => !IsFinished(a)))
        {
            arena.Actor(c, IsActive(c) ? ArenaColor.Enemy : ArenaColor.Object, true);
        }
    }
}

class CometImpact : Components.SelfTargetedAOEs
{
    public CometImpact() : base(ActionID.MakeSpell(AID.CometImpact), new AOEShapeCircle(10)) { } // TODO: verify falloff
}

class CometBurst : Components.SelfTargetedAOEs
{
    public CometBurst() : base(ActionID.MakeSpell(AID.CometBurstLong), new AOEShapeCircle(10)) { }
}

class BeastlyBile : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }
    private Comet? _comet;
    private DateTime _activation;
    private BitMask _forbiddenPlayers;

    public BeastlyBile() : base(6, 0, 4) { }

    public override void Init(BossModule module)
    {
        _comet = module.FindComponent<Comet>();
        _activation = module.WorldState.CurrentTime.AddSeconds(15); // assuming component is activated after proximity
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (_comet != null && IsStackTarget(actor))
        {
            if (_comet.Actors.Any(c => !Comet.IsActive(c) && c.Position.InCircle(actor.Position, StackRadius)))
                hints.Add("GTFO from normal comets!");
            if (!_comet.Actors.Any(c => Comet.IsActive(c) && c.Position.InCircle(actor.Position, StackRadius)))
                hints.Add("Bait to glowing comet!");
        }
    }

    public override void Update(BossModule module)
    {
        Stacks.Clear();
        var target = NumCasts < 2 ? module.Raid.WithoutSlot().Farthest(module.PrimaryActor.Position) : null;
        if (target != null)
            AddStack(target, _activation, _forbiddenPlayers);
        base.Update(module);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BeastlyBileAOE)
        {
            ++NumCasts;
            _activation = module.WorldState.CurrentTime.AddSeconds(6);
            foreach (var t in spell.Targets)
                _forbiddenPlayers.Set(module.Raid.FindSlot(t.ID));
        }
    }
}

class Thunderbolt : Components.GenericBaitAway
{
    private Comet? _comet;

    private static readonly AOEShapeCone _shape = new(40, 22.5f.Degrees());

    public Thunderbolt() : base(ActionID.MakeSpell(AID.ThunderboltAOE)) { }

    public override void Init(BossModule module)
    {
        _comet = module.FindComponent<Comet>();
    }

    public override void Update(BossModule module)
    {
        CurrentBaits.Clear();
        foreach (var p in module.Raid.WithoutSlot().SortedByRange(module.PrimaryActor.Position).Take(4))
            CurrentBaits.Add(new(module.PrimaryActor, p, _shape));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        foreach (var b in ActiveBaitsOn(actor))
            if (_comet?.Actors.Any(c => IsClippedBy(c, b)) ?? false)
                hints.Add("Aim away from comets!");
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(module.Raid.FindSlot(t.ID));
        }
    }
}

class EclipticMeteor : Components.GenericLineOfSightAOE
{
    public EclipticMeteor() : base(ActionID.MakeSpell(AID.EclipticMeteorAOE), 60, false) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.EclipticMeteor)
        {
            Modify(actor.Position, module.Enemies(OID.Comet).Where(c => c != actor && !Comet.IsFinished(c)).Select(c => (c.Position, c.HitboxRadius)));
        }
    }
}
