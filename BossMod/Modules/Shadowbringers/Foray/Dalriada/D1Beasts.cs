namespace BossMod.Shadowbringers.Foray.Dalriada.D1Beasts;

public enum OID : uint
{
    Boss = 0x32BF, // R3.640, x1
    Helper = 0x233C, // R0.500, x12, mixed types
    FourthLegionInfantry = 0x32C0, // R1.000, x0 (spawn during fight)
    TerminusEst = 0x32C1, // R1.000, x0 (spawn during fight)
    FourthLegionAugur = 0x33E2, // R0.500, x0 (spawn during fight)
    FourthLegionAugur2 = 0x32C8, // R0.550, x0 (spawn during fight)
    StormborneZirnitra = 0x32C7, // R2.800, x0 (spawn during fight)
    WaveborneZirnitra = 0x32C6, // R2.800, x0 (spawn during fight)
    FlameborneZirnitra = 0x32C5, // R2.800, x0 (spawn during fight)
    PyroplexyTower = 0x1EB214, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb213 = 0x1EB213, // R0.500, x0 (spawn during fight), EventObj type
    TamedAlkonost = 0x32CB, // R7.500, x0 (spawn during fight)
    TamedCarrionCrow = 0x32CA, // R4.320, x0 (spawn during fight)
    FourthLegionBeastmaster = 0x32CF, // R0.500, x0 (spawn during fight)
    VorticalOrb = 0x32CD, // R0.500, x0 (spawn during fight)
    VorticalOrb2 = 0x32CC, // R0.500, x0 (spawn during fight)
    TamedAlkonostsShadow = 0x32CE, // R3.750-7.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 25126, // Boss->player, no cast, single-target
    SuppressiveMagitekRaysBoss = 24338, // Boss->self, 5.0s cast, single-target
    SuppressiveMagitekRays = 24339, // Helper->self, 5.6s cast, ???
    AntiPersonnelMissile = 24333, // Boss->self, 10.0s cast, single-target
    BallisticImpact = 24334, // Helper->self, no cast, range 24 width 24 rect
    ReadOrdersFieldSupport = 24332, // Boss->self, 3.0s cast, single-target
    TerminusEst = 24323, // FourthLegionInfantry->self, no cast, single-target
    Analysis = 24335, // Boss->self, 3.0s cast, single-target
    TerminusEst2 = 24327, // TerminusEst->self, 5.0s cast, range 50 width 8 rect
    SurfaceMissileBoss = 24336, // Boss->self, 3.0s cast, single-target
    SurfaceMissile = 24337, // Helper->location, 3.0s cast, range 6 circle
    SanctifiedQuakeIII = 24351, // FourthLegionAugur2->self, 5.0s cast, single-target
    SanctifiedQuakeIIIHelper = 24352, // Helper->self, 5.6s cast, ???
    VoidCall = 24350, // FourthLegionAugur2->self, 3.0s cast, single-target
    TurbineHelper = 24341, // Helper->self, 5.0s cast, ???
    Turbine = 24340, // FlameborneZirnitra->self, 5.0s cast, range 40 circle
    FlamingCyclone = 24345, // StormborneZirnitra->self, 7.5s cast, range 10 circle
    A74Degrees = 24343, // WaveborneZirnitra->players, 8.0s cast, range ?-8 donut
    Pyroplexy = 24347, // FourthLegionAugur2->self, 12.0s cast, single-target
    Pyroclysm = 24349, // Helper->location, no cast, range 40 circle
    PyroplexyHelper = 24348, // Helper->location, no cast, range 4 circle
    Stormcall = 24358, // TamedAlkonost->self, 3.0s cast, single-target
    SouthWind = 24354, // TamedCarrionCrow->self, 8.0s cast, single-target
    StormcallOrb = 24359, // VorticalOrb/VorticalOrb2->self, 2.0s cast, range 35 circle
    Unknown = 24355, // Helper->self, 8.0s cast, range 60 width 60 rect
    SouthWindHelper = 24815, // Helper->self, no cast, ???
    PainStorm = 24363, // TamedAlkonost->self, 6.0s cast, range 35 130-degree cone
    ShadowsCast = 24369, // TamedAlkonostsShadow->self, 6.0s cast, single-target
    FrigidPulse = 24362, // TamedAlkonost->self, 6.0s cast, range 8-25 donut
    NorthWind = 24353, // TamedCarrionCrow->self, 8.0s cast, single-target
    NorthWindHelper = 24814, // Helper->self, no cast, ???
    Foreshadowing = 24368, // TamedAlkonost->self, 11.0s cast, single-target
    FrigidPulseShadow = 24365, // TamedAlkonostsShadow->self, 11.0s cast, range 8-25 donut
    PainStormShadow = 24366, // TamedAlkonostsShadow->self, 11.0s cast, range 35 130-degree cone
    NihilitysSong = 24360, // TamedAlkonost->self, 5.0s cast, single-target
    NihilitysSongHelper = 24361, // Helper->self, 5.6s cast, ???
    BroadsideBarrage = 24357, // TamedCarrionCrow->self, 5.0s cast, range 40 width 40 rect
    PainfulGust = 24364, // TamedAlkonost->self, 6.0s cast, range 20 circle
}

public enum SID : uint
{
    MagicalAversion = 2370, // none->FourthLegionInfantry, extra=0x0
    LeftUnseen = 1708, // none->player, extra=0xEA
    RightUnseen = 1707, // none->player, extra=0xE9
    BackUnseen = 1709, // none->player, extra=0xE8
    PhysicalAversion = 2369, // none->FourthLegionAugur2/StormborneZirnitra/WaveborneZirnitra/FlameborneZirnitra/TamedAlkonost/TamedCarrionCrow, extra=0x0
    Unk = 2234, // none->VorticalOrb2, extra=0x2B
    Transfiguration = 705, // TamedAlkonostsShadow->TamedAlkonostsShadow, extra=0x1A4
    Bleeding = 642, // none->player, extra=0x0
}

public enum IconID : uint
{
    BallisticImpact = 261, // Helper->self
    Degrees = 288, // player->self
}

class BeastTracker(BossModule module) : BossComponent(module)
{
    public bool CrowDead;
    public bool AlkonostDead;
    public bool BothDead => CrowDead && AlkonostDead;

    public override void Update()
    {
        if (!CrowDead)
            CrowDead |= Module.Enemies(OID.TamedCarrionCrow).Any(x => x.IsDead);
        if (!AlkonostDead)
            AlkonostDead |= Module.Enemies(OID.TamedAlkonost).Any(x => x.IsDead);
    }
}

class SuppressiveMagitekRays(BossModule module) : Components.RaidwideCast(module, AID.SuppressiveMagitekRays);
class SanctifiedQuake(BossModule module) : Components.RaidwideCast(module, AID.SanctifiedQuakeIIIHelper);

class BallisticImpact(BossModule module) : Components.GenericAOEs(module, AID.BallisticImpact)
{
    private readonly List<WPos> Impacts = [];
    private DateTime Activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Impacts.Take(2).Select(t => new AOEInstance(new AOEShapeRect(12, 12, 12), t, Activation: Activation));

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BallisticImpact)
        {
            if (Impacts.Count == 0)
                Activation = WorldState.FutureTime(11.9f);

            Impacts.Add(actor.Position);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            Impacts.RemoveAt(0);
            if (Impacts.Count > 0)
                Activation = WorldState.FutureTime(3.1f);
            else
                Activation = default;
        }
    }
}

class TerminusEst(BossModule module) : Components.CastWeakpoint(module, AID.TerminusEst2, new AOEShapeRect(60, 4), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);
class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID.SurfaceMissile, 6);

class Turbine(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TurbineHelper, 15)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && Casters.Count == 0)
            Casters.Add(caster);
    }
}
class FlamingCyclone(BossModule module) : Components.StandardAOEs(module, AID.FlamingCyclone, new AOEShapeCircle(10));
class A74Degrees(BossModule module) : Components.BaitAwayCast(module, AID.A74Degrees, new AOEShapeDonut(4, 8), centerAtTarget: true, endsOnCastEvent: true);
class Pyroplexy(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.PyroplexyTower && state == 0x00010002)
            Towers.Add(new(actor.Position, 4, maxSoakers: int.MaxValue, activation: WorldState.FutureTime(8.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Pyroclysm or AID.PyroplexyHelper)
            Towers.RemoveAll(t => t.Position.AlmostEqual(spell.TargetXZ, 1));
    }
}

class Wind(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> winds = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => winds;

    public WDir KnockbackDir(int slot) => winds.Where(s => !IsImmune(slot, s.Activation)).Select(s => s.Direction.ToDirection() * s.Distance).FirstOrDefault();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SouthWind)
        {
            winds.Add(new(new WPos(222, -665), 40, Module.CastFinishAt(spell, 0.7f), null, 180.Degrees(), Kind.DirForward));
        }

        if ((AID)spell.Action.ID == AID.NorthWind)
        {
            winds.Add(new(new WPos(222, -713), 40, Module.CastFinishAt(spell, 0.7f), null, 0.Degrees(), Kind.DirForward));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SouthWindHelper or AID.NorthWindHelper)
            winds.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            if (!IsImmune(slot, src.Activation))
            {
                var badZ = src.Origin.Z < Arena.Center.Z ? src.Origin.Z + 48 : src.Origin.Z - 48;
                var badWall = new WPos(src.Origin.X, badZ);
                hints.AddForbiddenZone(ShapeContains.Rect(badWall, src.Direction + 180.Degrees(), 40, 0, 40), src.Activation);
            }
        }
    }
}

// fast orb is 13.85s later
// slow orb is 20.95s later
class Stormcall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes.Take(1);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.VorticalOrb or OID.VorticalOrb2)
        {
            var offset = actor.Position.Z < Arena.Center.Z ? new WDir(0, 48) : new WDir(0, -48);
            aoes.Add(new(new AOEShapeCircle(35), actor.Position + offset, Activation: WorldState.FutureTime(actor.OID == (uint)OID.VorticalOrb ? 13.85f : 20.95f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StormcallOrb && aoes.Count > 0)
            aoes.RemoveAt(0);
    }
}

class KBAware(BossModule module, Enum action, AOEShape shape) : Components.StandardAOEs(module, action, shape)
{
    private Wind? windComponent;

    public override void Update()
    {
        windComponent ??= Module.FindComponent<Wind>();
    }

    private WDir KnockbackDir(int slot) => windComponent?.KnockbackDir(slot) ?? default;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (KnockbackDir(slot) == default)
            return base.ActiveAOEs(slot, actor);
        else
            return base.ActiveAOEs(slot, actor).Select(a => a with { Risky = false });
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var dir = KnockbackDir(slot);
        foreach (var aoe in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(aoe.Shape, aoe.Origin - dir, aoe.Rotation, aoe.Activation);
    }
}

class PainStorm(BossModule module) : KBAware(module, AID.PainStorm, new AOEShapeCone(35, 65.Degrees()));
class PainStorm2(BossModule module) : KBAware(module, AID.PainStormShadow, new AOEShapeCone(35, 65.Degrees()));
class FrigidPulse(BossModule module) : KBAware(module, AID.FrigidPulse, new AOEShapeDonut(8, 25));
class FrigidPulse2(BossModule module) : KBAware(module, AID.FrigidPulseShadow, new AOEShapeDonut(8, 25));
class BroadsideBarrage(BossModule module) : KBAware(module, AID.BroadsideBarrage, new AOEShapeRect(40, 20));
class PainfulGust(BossModule module) : KBAware(module, AID.PainfulGust, new AOEShapeCircle(20));

class FourthLegionAugurStates : StateMachineBuilder
{
    public FourthLegionAugurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeastTracker>()
            .ActivateOnEnter<SuppressiveMagitekRays>()
            .ActivateOnEnter<BallisticImpact>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<SanctifiedQuake>()
            .ActivateOnEnter<Turbine>()
            .ActivateOnEnter<FlamingCyclone>()
            .ActivateOnEnter<A74Degrees>()
            .ActivateOnEnter<Pyroplexy>()
            .ActivateOnEnter<Wind>()
            .ActivateOnEnter<Stormcall>()
            .ActivateOnEnter<PainStorm>()
            .ActivateOnEnter<PainStorm2>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<FrigidPulse2>()
            .ActivateOnEnter<BroadsideBarrage>()
            .ActivateOnEnter<PainfulGust>()
            .Raw.Update = () => module.FindComponent<BeastTracker>()!.BothDead;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10207)]
public class FourthLegionAugur(WorldState ws, Actor primary) : BossModule(ws, primary, new(222, -689), new ArenaBoundsRect(24, 24))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    public override bool DrawAllPlayers => true;
}
