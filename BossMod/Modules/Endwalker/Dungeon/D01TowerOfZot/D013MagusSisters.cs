namespace BossMod.Endwalker.Dungeon.D01TheTowerOfZot.D013MagusSisters;

public enum OID : uint
{
    Boss = 0x33F1, // R2.2
    Sanduruva = 0x33F2, // R=2.5
    Minduruva = 0x33F3, // R=2.04
    BerserkerSphere = 0x33F0, // R=1.5-2.5
    Helper = 0x233C,
    Helper2 = 0x3610,
};

public enum AID : uint
{
    AutoAttack = 871, // Sanduruva->player, no cast, single-target
    Teleport = 25254, // Sanduruva->location, no cast, single-target
    DeltaAttack = 25260, // Minduruva->Boss, 5.0s cast, single-target
    DeltaAttack1 = 25261, // Minduruva->Boss, 5.0s cast, single-target
    DeltaAttack2 = 25262, // Minduruva->Boss, 5.0s cast, single-target
    DeltaBlizzardIII1 = 25266, // Helper->self, 3.0s cast, range 40+R 20-degree cone
    DeltaBlizzardIII2 = 25267, // Helper->self, 3.0s cast, range 44 width 4 rect
    DeltaBlizzardIII3 = 25268, // Helper->location, 5.0s cast, range 40 circle
    DeltaFireIII1 = 25263, // Helper->self, 4.0s cast, range 5-40 donut
    DeltaFireIII2 = 25264, // Helper->self, 3.0s cast, range 44 width 10 rect
    DeltaFireIII3 = 25265, // Helper->player, 5.0s cast, range 6 circle, spread
    DeltaThunderIII1 = 25269, // Helper->location, 3.0s cast, range 3 circle
    DeltaThunderIII2 = 25270, // Helper->location, 3.0s cast, range 5 circle
    DeltaThunderIII3 = 25271, // Helper->self, 3.0s cast, range 40 width 10 rect
    DeltaThunderIII4 = 25272, // Helper->player, 5.0s cast, range 5 circle, stack
    Dhrupad = 25281, // Minduruva->self, 4.0s cast, single-target, after this each of the non-tank players get hit once by a single-target spell (ManusyaBlizzard1, ManusyaFire1, ManusyaThunder1)
    IsitvaSiddhi = 25280, // Sanduruva->player, 4.0s cast, single-target, tankbuster
    ManusyaBlizzard1 = 25283, // Minduruva->player, no cast, single-target
    ManusyaBlizzard2 = 25288, // Minduruva->player, 2.0s cast, single-target
    ManusyaFaith = 25258, // Sanduruva->Minduruva, 4.0s cast, single-target
    ManusyaFire1 = 25282, // Minduruva->player, no cast, single-target
    ManusyaFire2 = 25287, // Minduruva->player, 2.0s cast, single-target
    ManusyaGlare = 25274, // Boss->none, no cast, single-target
    ManusyaReflect = 25259, // Boss->self, 4.2s cast, range 40 circle
    ManusyaThunder1 = 25284, // Minduruva->player, no cast, single-target
    ManusyaThunder2 = 25289, // Minduruva->player, 2.0s cast, single-target
    PraptiSiddhi = 25275, // Sanduruva->self, 2.0s cast, range 40 width 4 rect
    Samsara = 25273, // Boss->self, 3.0s cast, range 40 circle
    ManusyaBio = 25290, // Minduruva->player, 4,0s cast, single-target
    ManusyaBerserk = 25276, // Sanduruva->self, 3,0s cast, single-target
    ExplosiveForce = 25277, // Sanduruva->self, 2,0s cast, single-target
    SphereShatter = 25279, // BerserkerSphere->self, 1,5s cast, range 15 circle
    PrakamyaSiddhi = 25278, // Sanduruva->self, 4,0s cast, range 5 circle
    ManusyaBlizzardIII = 25285, // Minduruva->self, 4,0s cast, single-target
    ManusyaBlizzardIII2 = 25286, // Helper->self, 4,0s cast, range 40+R 20-degree cone
};

public enum SID : uint
{
    Poison = 18, // Boss->player, extra=0x0
    Burns = 2082, // Minduruva->player, extra=0x0
    Frostbite = 2083, // Minduruva->player, extra=0x0
    Electrocution = 2086, // Minduruva->player, extra=0x0
};

class Dhrupad : BossComponent
{
    private int NumCasts;
    private bool active;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Dhrupad)
            active = true;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ManusyaFire1 or AID.ManusyaBlizzard1 or AID.ManusyaThunder1)
        {
            ++NumCasts;
            if (NumCasts == 3)
            {
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (active)
            hints.Add("3 single target hits + DoTs");
    }
}

class ManusyaBio : Components.SingleTargetCast
{
    public ManusyaBio() : base(ActionID.MakeSpell(AID.ManusyaBio), "Tankbuster + cleansable poison") { }
}

class Poison : BossComponent
{
    private List<Actor> _poisoned = [];

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Poison)
            _poisoned.Add(actor);
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Poison)
            _poisoned.Remove(actor);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_poisoned.Contains(actor) && !(actor.Role == Role.Healer || actor.Class == Class.BRD)) //theoretically only the tank can ge poisoned, this is just in here incase of bad tanks
            hints.Add("You were poisoned! Get cleansed fast.");
        if (_poisoned.Contains(actor) && (actor.Role == Role.Healer || actor.Class == Class.BRD))
            hints.Add("Cleanse yourself! (Poison).");
        foreach (var c in _poisoned)
            if (!_poisoned.Contains(actor) && (actor.Role == Role.Healer || actor.Class == Class.BRD))
                hints.Add($"Cleanse {c.Name} (Poison)");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        foreach (var c in _poisoned)
        {
            if (_poisoned.Count > 0 && actor.Role == Role.Healer)
                hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Esuna), c, 1, false));
            if (_poisoned.Count > 0 && actor.Class == Class.BRD)
                hints.PlannedActions.Add((ActionID.MakeSpell(BRD.AID.WardensPaean), c, 1, false));
        }
    }
}

class IsitvaSiddhi : Components.SingleTargetCast
{
    public IsitvaSiddhi() : base(ActionID.MakeSpell(AID.IsitvaSiddhi)) { }
}

class Samsara : Components.RaidwideCast
{
    public Samsara() : base(ActionID.MakeSpell(AID.Samsara)) { }
}

class DeltaThunderIII1 : Components.LocationTargetedAOEs
{
    public DeltaThunderIII1() : base(ActionID.MakeSpell(AID.DeltaThunderIII1), 3) { }
}

class DeltaThunderIII2 : Components.LocationTargetedAOEs
{
    public DeltaThunderIII2() : base(ActionID.MakeSpell(AID.DeltaThunderIII2), 5) { }
}

class DeltaThunderIII3 : Components.SelfTargetedAOEs
{
    public DeltaThunderIII3() : base(ActionID.MakeSpell(AID.DeltaThunderIII3), new AOEShapeRect(40, 5)) { }
}

class DeltaThunderIII4 : Components.StackWithCastTargets
{
    public DeltaThunderIII4() : base(ActionID.MakeSpell(AID.DeltaThunderIII4), 5) { }
}

class DeltaBlizzardIII1 : Components.SelfTargetedAOEs
{
    public DeltaBlizzardIII1() : base(ActionID.MakeSpell(AID.DeltaBlizzardIII1), new AOEShapeCone(40.5f, 10.Degrees())) { }
}

class DeltaBlizzardIII2 : Components.SelfTargetedAOEs
{
    public DeltaBlizzardIII2() : base(ActionID.MakeSpell(AID.DeltaBlizzardIII2), new AOEShapeRect(44, 2)) { }
}

class DeltaBlizzardIII3 : Components.SelfTargetedAOEs
{
    public DeltaBlizzardIII3() : base(ActionID.MakeSpell(AID.DeltaBlizzardIII3), new AOEShapeCircle(15)) { }
}

class DeltaFireIII1 : Components.SelfTargetedAOEs
{
    public DeltaFireIII1() : base(ActionID.MakeSpell(AID.DeltaFireIII1), new AOEShapeDonut(5, 40)) { }
}

class DeltaFireIII2 : Components.SelfTargetedAOEs
{
    public DeltaFireIII2() : base(ActionID.MakeSpell(AID.DeltaFireIII2), new AOEShapeRect(44, 5)) { }
}

class DeltaFireIII3 : Components.SpreadFromCastTargets
{
    public DeltaFireIII3() : base(ActionID.MakeSpell(AID.DeltaFireIII3), 6) { }
}

class PraptiSiddhi : Components.SelfTargetedAOEs
{
    public PraptiSiddhi() : base(ActionID.MakeSpell(AID.PraptiSiddhi), new AOEShapeRect(40, 2)) { }
}

class SphereShatter : Components.GenericAOEs
{
    private DateTime _activation;
    private readonly List<Actor> _casters = [];
    private static readonly AOEShapeCircle circle = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_casters.Count > 0)
            foreach (var c in _casters)
                yield return new(circle, c.Position, activation: _activation);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.BerserkerSphere)
        {
            _casters.Add(actor);
            _activation = module.WorldState.CurrentTime.AddSeconds(7.3f);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SphereShatter)
        {
            _casters.Remove(caster);
            ++NumCasts;
        }
    }
}

class PrakamyaSiddhi : Components.SelfTargetedAOEs
{
    public PrakamyaSiddhi() : base(ActionID.MakeSpell(AID.PrakamyaSiddhi), new AOEShapeCircle(5)) { }
}

class ManusyaBlizzardIII : Components.SelfTargetedAOEs
{
    public ManusyaBlizzardIII() : base(ActionID.MakeSpell(AID.ManusyaBlizzardIII2), new AOEShapeCone(40.5f, 10.Degrees())) { }
}

class D013MagusSistersStates : StateMachineBuilder
{
    public D013MagusSistersStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IsitvaSiddhi>()
            .ActivateOnEnter<ManusyaBio>()
            .ActivateOnEnter<Poison>()
            .ActivateOnEnter<Samsara>()
            .ActivateOnEnter<ManusyaBlizzardIII>()
            .ActivateOnEnter<PrakamyaSiddhi>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<PraptiSiddhi>()
            .ActivateOnEnter<DeltaFireIII1>()
            .ActivateOnEnter<DeltaFireIII2>()
            .ActivateOnEnter<DeltaFireIII3>()
            .ActivateOnEnter<DeltaThunderIII1>()
            .ActivateOnEnter<DeltaThunderIII2>()
            .ActivateOnEnter<DeltaThunderIII3>()
            .ActivateOnEnter<DeltaThunderIII4>()
            .ActivateOnEnter<Dhrupad>()
            .ActivateOnEnter<DeltaBlizzardIII1>()
            .ActivateOnEnter<DeltaBlizzardIII2>()
            .ActivateOnEnter<DeltaBlizzardIII3>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Sanduruva).All(e => e.IsDead) && module.Enemies(OID.Minduruva).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783, NameID = 10265)]
class D013MagusSisters : BossModule
{
    public D013MagusSisters(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-27.5f, -49.5f), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Minduruva))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Sanduruva))
            Arena.Actor(s, ArenaColor.Enemy);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 3,
                OID.Minduruva => 2,
                OID.Sanduruva => 1,
                _ => 0
            };
        }
    }
}
