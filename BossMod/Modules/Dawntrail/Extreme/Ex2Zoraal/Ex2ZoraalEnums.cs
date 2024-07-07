namespace BossMod.Dawntrail.Extreme.Ex2Zoraal;

public enum OID : uint
{
    Boss = 0x42B5, // R10.050, x1
    Helper = 0x233C, // R0.500, x18 (spawn during fight), mixed types
    BitingWind = 0x42BE, // R0.800, x0 (spawn during fight)
    Fang1 = 0x42AB, // R1.000, x0 (spawn during fight)
    Fang2 = 0x42B7, // R1.000, x0 (spawn during fight)
    Fang3 = 0x42B8, // R2.000, x0 (spawn during fight)
    UnknownActor1 = 0x19A, // R0.500, x0 (spawn during fight)
    UnknownActor2 = 0x4156, // R1.000, x0 (spawn during fight)
    UnknownActor3 = 0x4157, // R1.000, x0 (spawn during fight)
    UnknownActor4 = 0x42B9, // R10.050, x0 (spawn during fight)
    UnknownActor5 = 0x42BD, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 37799, // Boss->player, no cast, single-target
    Teleport = 37717, // Boss->location, no cast, single-target

    Actualize1 = 37784, // Boss->self, 5.0s cast, range 100 circle
    Actualize2 = 39274, // Boss->self, 10.0s cast, range 100 circle

    AeroIII1 = 37776, // Boss->self, 5.0s cast, single-target
    AeroIII2 = 37777, // BitingWind->self, no cast, range 4 circle

    BackwardEdge = 39282, // Helper->self, 1.0s cast, range 60 width 60 rect

    BackwardHalf1 = 37757, // Boss->self, 8.0+1.0s cast, single-target
    BackwardHalf2 = 37758, // Boss->self, 8.0+1.0s cast, single-target
    BackwardHalf3 = 39324, // Boss->self, 9.0+1.0s cast, single-target
    BackwardHalf4 = 39325, // Boss->self, 9.0+1.0s cast, single-target

    BitterWhirlwind1 = 39229, // Boss->self, 5.0s cast, single-target
    BitterWhirlwind2 = 39230, // Helper->player, 5.0s cast, range 5 circle
    BitterWhirlwind3 = 39231, // Boss->self, no cast, single-target
    BitterWhirlwind4 = 39232, // Helper->player, no cast, range 5 circle

    BladeWarp = 37726, // Boss->self, 4.0s cast, single-target

    BurningChains1 = 37781, // Boss->self, 5.0s cast, single-target
    BurningChains2 = 37782, // Helper->self, no cast, ???

    ChasmOfVollok1 = 37769, // Helper->self, no cast, ???
    ChasmOfVollok2 = 37780, // Helper->self, 1.0s cast, range 10 width 10 rect
    ChasmOfVollok3 = 37785, // Fang2->self, 8.0s cast, range 5 width 5 rect
    ChasmOfVollok4 = 37786, // Helper->self, 1.0s cast, range 5 width 5 rect

    DawnOfAnAge = 37783, // Boss->self, 7.0s cast, range 100 circle

    DrumOfVollok1 = 37774, // Boss->self, 7.4+0.6s cast, single-target
    DrumOfVollok2 = 37775, // Helper->players, 8.0s cast, range 4 circle

    DutysEdge1 = 37748, // Boss->self, 4.9s cast, single-target
    DutysEdge2 = 37749, // Boss->self, no cast, single-target
    DutysEdge3 = 38055, // Helper->self, no cast, range 100 width 8 rect

    FieryEdge1 = 37762, // Fang1->self, no cast, single-target
    FieryEdge2 = 37763, // Helper->self, no cast, range 20 width 5 rect
    FieryEdge3 = 37764, // Helper->self, no cast, range 20 width 5 rect

    ForgedTrack1 = 37727, // Boss->self, 4.0s cast, single-target
    ForgedTrack2 = 37788, // Fang1->self, 10.9s cast, range 20 width 5 rect
    ForgedTrack3 = 37789, // Fang1->self, no cast, range 20 width 5 rect

    ForwardEdge = 37759, // Helper->self, 1.0s cast, range 60 width 60 rect

    ForwardHalf1 = 37755, // Boss->self, 8.0+1.0s cast, single-target
    ForwardHalf2 = 37756, // Boss->self, 8.0+1.0s cast, single-target
    ForwardHalf3 = 39322, // Boss->self, 9.0+1.0s cast, single-target
    ForwardHalf4 = 39323, // Boss->self, 9.0+1.0s cast, single-target

    GreaterGateway = 37761, // Boss->self, 4.0s cast, single-target

    HalfCircuit1 = 37739, // Boss->self, 7.0s cast, single-target
    HalfCircuit2 = 37740, // Boss->self, 7.0s cast, single-target
    HalfCircuit3 = 37791, // Helper->self, 7.3s cast, range 60 width 120 rect
    HalfCircuit4 = 37792, // Helper->self, 7.0s cast, range ?-30 donut
    HalfCircuit5 = 37793, // Helper->self, 7.0s cast, range 10 circle

    HalfFull1 = 37736, // Boss->self, 6.0s cast, single-target
    HalfFull2 = 37737, // Boss->self, 6.0s cast, single-target
    HalfFull3 = 37760, // Helper->self, 1.0s cast, range 60 width 120 rect
    HalfFull4 = 37790, // Helper->self, 6.3s cast, range 60 width 120 rect

    MightOfVollok = 37773, // Helper->players, no cast, range 8 circle

    MultidirectionalDivide1 = 37794, // Boss->self, 5.0s cast, range 30 width 4 cross
    MultidirectionalDivide2 = 37795, // Helper->self, 16.0s cast, range 30 width 8 cross
    MultidirectionalDivide3 = 37796, // Helper->self, 16.0s cast, range 40 width 4 rect

    ProjectionOfTriumph = 37770, // Boss->self, 5.0s cast, single-target
    ProjectionOfTurmoil = 39560, // Boss->self, 5.0s cast, single-target

    RegicidalRage1 = 39227, // Boss->self, 8.0s cast, single-target
    RegicidalRage2 = 39228, // Helper->player, no cast, range 8 circle

    SiegeOfVollok = 37771, // Fang2->self, 0.5s cast, range ?-8 donut

    SmitingCircuit1 = 37732, // UnknownActor4->self, no cast, single-target
    SmitingCircuit2 = 37733, // UnknownActor4->self, no cast, single-target

    StormyEdge1 = 37765, // Fang1->self, no cast, single-target
    StormyEdge2 = 37766, // Helper->self, 0.5s cast, range 20 width 5 rect
    StormyEdge3 = 37767, // Helper->self, 0.5s cast, range 10 width 20 rect

    Sync = 37721, // Boss->self, 5.0s cast, single-target
    UnknownAbility = 37728, // Helper->UnknownActor1, no cast, single-target
    UnknownSpell = 35567, // Helper->player, no cast, single-target

    Vollok1 = 37719, // Boss->self, 4.0s cast, single-target
    Vollok2 = 37778, // Boss->self, 5.0s cast, single-target
    Vollok3 = 37779, // Fang3->self, 8.0s cast, range 10 width 10 rect

    WallsOfVollok = 37772, // Fang2->self, 0.5s cast, range 4 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/Fang2->player, extra=0x1/0x2/0x3
    MagicVulnerabilityUp1 = 2941, // Helper->player, extra=0x0
    UnknownStatus = 2397, // none->Fang2/UnknownActor4, extra=0x2C7/0x2CD
    Projection = 4047, // none->player, extra=0x0
    Sprint = 481, // none->UnknownActor5, extra=0x32
    Bleeding1 = 3077, // none->player, extra=0x0
    Bleeding2 = 3078, // none->player, extra=0x0
    WindResistanceDownII = 2096, // Helper/BitingWind->player, extra=0x0
    Liftoff = 3262, // BitingWind->player, extra=0x0
    MagicVulnerabilityUp2 = 3516, // Helper->player, extra=0x1/0x2/0x3/0x4
    BurningChains = 769, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon185 = 185, // player
    Icon343 = 343, // player
    Icon539 = 539, // player
}

public enum TetherID : uint
{
    Tether89 = 89, // player->Boss
    Tether86 = 86, // Fang1->Boss
    BurningChains = 128, // player->player
}
