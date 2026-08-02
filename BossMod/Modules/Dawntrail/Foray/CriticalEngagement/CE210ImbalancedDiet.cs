namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE210ImbalancedDiet;

public enum OID : uint
{
    AlgolHelper = 0x233C, // R0.500, x32, Helper type
    CrescentLorelei = 0x4E17, // R2.640, x7
    CrescentTomato = 0x4E10, // R1.050, x6
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    Algol2 = 0x4D87, // R6.000, x5
    CrescentOnion = 0x4C4D, // R0.900, x4
    CrescentOnion1 = 0x4E11, // R1.050, x6
    CrescentVinegaroon = 0x4E12, // R3.500, x5 (spawn during fight)
    Algol = 0x4C4B, // R7.500, x1
    Actor1ec021 = 0x1EC021, // R0.500, x1, EventObj type
    CrescentTomato1 = 0x4C4C, // R0.900, x4
    UnknownActor = 0x4C4E, // R1.000, x0 (spawn during fight)
    CrescentZu = 0x4E0F, // R3.600, x0 (spawn during fight)
    CrescentWorm = 0x4E0E, // R3.900, x0 (spawn during fight)
    CrescentSaltSwallow = 0x4E19, // R3.200, x0 (spawn during fight)
}

public enum AID : uint
{
    UnknownWeaponskill1 = 48118, // Algol2->self, no cast, range ?-30 donut
    RottenOnion = 48112, // AlgolHelper->self, 2.0s cast, range 60 30.000-degree cone
    ShrillPeal = 50426, // Algol->self, 3.0s cast, ???
    ShrillPeal1 = 50427, // AlgolHelper->self, 4.0s cast, ???
    Inhale = 48101, // Algol->self, 2.0+1.0s cast, single-target
    Inhale1 = 48102, // Algol->self, no cast, single-target
    Inhale2 = 48104, // Algol2->self, 3.5s cast, range 60 30.000-degree cone
    Inhale3 = 48103, // AlgolHelper->CrescentTomato1/CrescentOnion, 0.7s cast, single-target
    Devour = 50469, // AlgolHelper->self, 6.8s cast, range 8 120.000-degree cone
    Regurgitomato = 48106, // Algol->location, no cast, single-target
    RottenTomato = 48109, // AlgolHelper->self, 4.0s cast, range 50 width 6 rect
    RottenTomato1 = 48111, // AlgolHelper->self, 2.0s cast, range 50 width 6 rect
    CursedScreech = 48100, // Algol->self, 5.0s cast, ???
    CursedScreech1 = 48971, // AlgolHelper->self, 6.0s cast, ???
    SpinningInhale = 48113, // Algol->self, 5.0s cast, range 30 30.000-degree cone
    SpinningInhale1 = 50942, // Algol2->self, no cast, range ?-30 donut
    SpinningInhale2 = 48114, // Algol2->self, no cast, range ?-30 donut
    SpinningInhale3 = 48249, // AlgolHelper->self, no cast, range 7 ?-degree cone
    UnknownWeaponskill2 = 48115, // Algol->self, no cast, single-target
    Devour1 = 48105, // Algol->self, no cast, range 12 ?-degree cone
    Devour2 = 50422, // AlgolHelper->self, 3.0s cast, range 12 120.000-degree cone
    Devour3 = 50467, // AlgolHelper->self, 3.0s cast, range 12 120.000-degree cone
    DigestedJuice = 48116, // Algol->self, 4.0s cast, range 40 width 50 rect
    DigestedJuice1 = 50423, // Algol->self, no cast, single-target
    DigestedJuice2 = 50424, // AlgolHelper->self, 4.0s cast, range 40 width 50 rect
    Malady = 48117, // Algol->self, no cast, range 12 circle
    Malady1 = 50425, // AlgolHelper->self, 3.0s cast, range 11 circle
    AutoAttack = 50644, // Algol->player, no cast, single-target
    Regurgitonion = 48107, // Algol->location, no cast, single-target
    RottenOnion1 = 48110, // AlgolHelper->self, 4.0s cast, range 60 30.000-degree cone
}

public enum SID : uint
{
    VulnerabilityUp = 2347, // AlgolHelper/Algol2->player, extra=0x2/0x3/0x1/0x5/0x4/0x6/0x7/0x8
    Incapacitated = 5408, // none->CrescentTomato1/CrescentOnion, extra=0x0
    QuickerStep = 4799, // none->player, extra=0x0
    UnknownStatus = 2552, // Algol->Algol, extra=0x424
    Stun1 = 5411, // Algol2->player, extra=0xEC7
    DirectionalDisregard = 3808, // none->Algol, extra=0x0
    Stun2 = 2656, // Algol2->player, extra=0xEC7
}

public enum IconID : uint
{
    Icon_m0005sp_11o0t = 13, // CrescentTomato1/CrescentOnion->self
    Icon_d1004turning_right_c0p = 167, // Algol->self
}

[SkipLocalsInit]
sealed class ImbalancedDietStates : StateMachineBuilder
{
    public ImbalancedDietStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(ImbalancedDietStates),
    ConfigType = null, // replace null with typeof(ImbalancedDietConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Algol,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14790u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class ImbalancedDiet(WorldState ws, Actor primary) : BossModule(ws, primary, new(764f, 0f), new ArenaBoundsCircle(23.9f));
