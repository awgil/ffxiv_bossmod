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
    Actor1eb214 = 0x1EB214, // R0.500, x0 (spawn during fight), EventObj type
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
    FourthSanctifiedQuakeIII = 24351, // FourthLegionAugur2->self, 5.0s cast, single-target
    FourthSanctifiedQuakeIIIHelper = 24352, // Helper->self, 5.6s cast, ???
    FourthVoidCall = 24350, // FourthLegionAugur2->self, 3.0s cast, single-target
    FourthTurbine = 24341, // Helper->self, 5.0s cast, ???
    Turbine = 24340, // FlameborneZirnitra->self, 5.0s cast, range 40 circle
    FourthFlamingCyclone = 24345, // StormborneZirnitra->self, 7.5s cast, range 10 circle
    Fourth74Degrees = 24343, // WaveborneZirnitra->players, 8.0s cast, range ?-8 donut
    FourthPyroplexy = 24347, // FourthLegionAugur2->self, 12.0s cast, single-target
    FourthPyroclysm = 24349, // Helper->location, no cast, range 40 circle
    FourthPyroplexyHelper = 24348, // Helper->location, no cast, range 4 circle
    FourthStormcall = 24358, // TamedAlkonost->self, 3.0s cast, single-target
    FourthSouthWind = 24354, // TamedCarrionCrow->self, 8.0s cast, single-target
    FourthStormcallOrb = 24359, // VorticalOrb/VorticalOrb2->self, 2.0s cast, range 35 circle
    FourthUnknown = 24355, // Helper->self, 8.0s cast, range 60 width 60 rect
    FourthSouthWindHelper = 24815, // Helper->self, no cast, ???
    FourthPainStorm = 24363, // TamedAlkonost->self, 6.0s cast, range 35 130-degree cone
    FourthShadowsCast = 24369, // TamedAlkonostsShadow->self, 6.0s cast, single-target
    FourthFrigidPulse = 24362, // TamedAlkonost->self, 6.0s cast, range 8-25 donut
    FourthNorthWind = 24353, // TamedCarrionCrow->self, 8.0s cast, single-target
    FourthNorthWindHelper = 24814, // Helper->self, no cast, ???
    FourthForeshadowing = 24368, // TamedAlkonost->self, 11.0s cast, single-target
    FourthFrigidPulseShadow = 24365, // TamedAlkonostsShadow->self, 11.0s cast, range 8-25 donut
    FourthPainStormShadow = 24366, // TamedAlkonostsShadow->self, 11.0s cast, range 35 130-degree cone
    FourthNihilitysSong = 24360, // TamedAlkonost->self, 5.0s cast, single-target
    FourthNihilitysSongHelper = 24361, // Helper->self, 5.6s cast, ???
    FourthBroadsideBarrage = 24357, // TamedCarrionCrow->self, 5.0s cast, range 40 width 40 rect
    FourthPainfulGust = 24364, // TamedAlkonost->self, 6.0s cast, range 20 circle
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
    Unk = 288, // player->self
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

class SuppressiveMagitekRays(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SuppressiveMagitekRays));

class FourthLegionAugurStates : StateMachineBuilder
{
    public FourthLegionAugurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeastTracker>()
            .ActivateOnEnter<SuppressiveMagitekRays>()
            .Raw.Update = () => module.FindComponent<BeastTracker>()!.BothDead;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10207)]
public class FourthLegionAugur(WorldState ws, Actor primary) : BossModule(ws, primary, new(222, -689), new ArenaBoundsRect(29.5f, 29.5f))
{
    // smaller arena during bird phase is 24 radius square

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    public override bool DrawAllPlayers => true;
}

