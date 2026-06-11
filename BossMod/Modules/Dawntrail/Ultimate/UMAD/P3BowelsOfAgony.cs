namespace BossMod.Dawntrail.Ultimate.UMAD;

class P3AeroIIIAssault(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Ability_AeroIIIAssault, 15, true);

class P3ThunderIII(BossModule module) : Components.StandardAOEs(module, AID._Ability_ThunderIII, 14.8f);

class P3Firewall(BossModule module) : Components.GenericInvincible(module)
{
    enum Color
    {
        None,
        Epic, // chaos
        Fated // exdeath
    }

    readonly Color[] _colors = new Color[8];

    Actor? _epicBoss;
    Actor? _fatedBoss;

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var otherBoss = _colors[slot] switch
        {
            Color.Fated => _epicBoss,
            Color.Epic => _fatedBoss,
            _ => null
        };

        if (otherBoss != null)
            yield return otherBoss;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_EpicHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    _colors[slot] = Color.Epic;
                break;
            case SID._Gen_FatedHero:
                if (Raid.TryFindSlot(actor, out var slot2))
                    _colors[slot2] = Color.Fated;
                break;
            case SID._Gen_EpicVillain:
                _epicBoss = actor;
                break;
            case SID._Gen_FatedVillain:
                _fatedBoss = actor;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_EpicHero:
            case SID._Gen_FatedHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    _colors[slot] = default;
                break;
            case SID._Gen_EpicVillain:
                _epicBoss = null;
                break;
            case SID._Gen_FatedVillain:
                _fatedBoss = null;
                break;
        }
    }
}

class P3BowelsOfAgony(BossModule module) : Components.RaidwideCast(module, AID._Ability_BowelsOfAgony);

class P3Crystals(BossModule module) : BossComponent(module)
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var f in Module.Enemies(OID.FireCrystal))
            Arena.AddCircleFilled(f.Position, 1, ArenaColor.Enemy);
        foreach (var f in Module.Enemies(OID.WaterCrystal))
            Arena.AddCircleFilled(f.Position, 1, 0xFFFF8080);
        foreach (var f in Module.Enemies(OID.WindCrystal))
            Arena.AddCircleFilled(f.Position, 1, 0xFF80FF00);
    }
}

enum Element
{
    None,
    Fire,
    Water
}

class P3EntropyFluid(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public static readonly AOEShape Circle = new AOEShapeCircle(5);
    public static readonly AOEShape Donut = new AOEShapeDonut(4, 10);

    readonly List<Bait> _restBaits = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_Entropy:
                CurrentBaits.Add(new(actor, actor, Circle, status.ExpireAt));
                break;
            case SID._Gen_DynamicFluid:
                CurrentBaits.Add(new(actor, actor, Donut, status.ExpireAt));
                break;
        }

        if (CurrentBaits.Count == 4)
        {
            var allBaits = CurrentBaits.OrderBy(c => c.Activation).ToList();
            CurrentBaits.Clear();
            CurrentBaits.AddRange(allBaits.Take(2));
            _restBaits.AddRange(allBaits.Skip(2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_StrayFlames:
            case AID._Ability_StraySpray:
                NumCasts++;
                CurrentBaits.RemoveAll(b => b.Target.InstanceID == spell.MainTargetID);
                break;
            case AID._Ability_Inferno:
            case AID._Ability_Tsunami:
                if (CurrentBaits.Count == 0)
                {
                    CurrentBaits.AddRange(_restBaits);
                    _restBaits.Clear();
                }
                break;
        }
    }

    // draw other baits with "imminent danger" color to visually distinguish them from crystal baits
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in ActiveBaitsNotOn(pc))
            b.Shape.Draw(Arena, b.Target, ArenaColor.Danger);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => CurrentBaits.Concat(_restBaits).Any(b => b.Target == player) ? PlayerPriority.Danger : PlayerPriority.Normal;

    public override void Update()
    {
        CurrentBaits.RemoveAll(b => b.Target.IsDead);
    }
}

class P3InfernoTsunami(BossModule module) : Components.GenericBaitAway(module)
{
    readonly P3EntropyFluid _fireWater = module.FindComponent<P3EntropyFluid>()!;

    Element _imminent;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_StrayFlames:
                _imminent = Element.Fire;
                break;
            case AID._Ability_StraySpray:
                _imminent = Element.Water;
                break;
            case AID._Ability_Inferno:
            case AID._Ability_Tsunami:
                NumCasts++;
                _imminent = Element.None;
                break;
        }
    }

    // for the sake of visual clarity, we draw self bait the same way other baits are drawn, so the player can distinguish between elemental debuff and crystal
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        foreach (var b in ActiveBaitsOn(pc))
            b.Shape.Draw(Arena, pc, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }

    public override void Update()
    {
        var nextElement = _imminent;
        DateTime nextActivation = default;

        CurrentBaits.Clear();

        if (_imminent == Element.None && _fireWater.CurrentBaits.Count > 0)
        {
            nextElement = _fireWater.CurrentBaits[0].Shape is AOEShapeCircle ? Element.Fire : Element.Water;
            nextActivation = _fireWater.CurrentBaits[0].Activation.AddSeconds(1);
        }

        var source = nextElement switch
        {
            Element.Fire => Module.Enemies(OID.FireCrystal).FirstOrDefault(),
            Element.Water => Module.Enemies(OID.WaterCrystal).FirstOrDefault(),
            _ => null
        };

        if (source == null)
            return;

        var targets = Raid.WithoutSlot().SortedByRange(source.Position).Take(2);
        CurrentBaits.AddRange(targets.Select(t => new Bait(t, t, nextElement == Element.Fire ? P3EntropyFluid.Donut : P3EntropyFluid.Circle, nextActivation)));
    }
}
