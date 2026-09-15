namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

public enum OID : uint
{
    Boss = 0x42B5, // R10.050, x1
    Helper = 0x233C, // R0.500, x18 (spawn during fight), mixed types
    FangVollokSmall = 0x42B7, // R1.000, x0 (spawn during fight), 1-cell aoes
    FangVollokLarge = 0x42B8, // R2.000, x0 (spawn during fight)
    FangBlade = 0x42AB, // R1.000, x0 (spawn during fight), sword aoes
    ForgedTrackHelper = 0x19A, // R0.500, x0 (spawn during fight)
    ProjectionOfTriumphCircle = 0x4156, // R1.000, x0 (spawn during fight), projection of triumph line
    ProjectionOfTriumphDonut = 0x4157, // R1.000, x0 (spawn during fight), projection of triumph line
    ProjectionOfTurmoil = 0x42BD, // R1.000, x0 (spawn during fight), projection of turmoil line
    BitingWind = 0x42BE, // R0.800, x0 (spawn during fight), knockback zone
    HalfCircuitHelper = 0x42B9, // R10.050, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 37799, // Boss->player, no cast, single-target
    Teleport = 37717, // Boss->location, no cast, single-target
    Actualize = 37784, // Boss->self, 5.0s cast, range 100 circle

    MultidirectionalDivide = 37794, // Boss->self, 5.0s cast, range 30 width 4 cross
    MultidirectionalDivideMain = 37795, // Helper->self, 16.0s cast, range 30 width 8 cross
    MultidirectionalDivideExtra = 37796, // Helper->self, 16.0s cast, range 40 width 4 rect
    ForwardHalfR = 37755, // Boss->self, 8.0+1.0s cast, single-target, visual (jump forward, turn around, cleave right)
    ForwardHalfL = 37756, // Boss->self, 8.0+1.0s cast, single-target, visual (jump forward, turn around, cleave left)
    BackwardHalfR = 37757, // Boss->self, 8.0+1.0s cast, single-target, visual (jump back, cleave right)
    BackwardHalfL = 37758, // Boss->self, 8.0+1.0s cast, single-target, visual (jump back, cleave left)
    ForwardEdge = 37759, // Helper->self, 1.0s cast, range 60 width 60 rect
    BackwardEdge = 39282, // Helper->self, 1.0s cast, range 60 width 60 rect
    HalfFullShortAOE = 37760, // Helper->self, 1.0s cast, range 60 width 120 rect
    RegicidalRage = 39227, // Boss->self, 8.0s cast, single-target
    RegicidalRageAOE = 39228, // Helper->players, no cast, range 8 circle tankbuster tether

    DawnOfAnAge = 37783, // Boss->self, 7.0s cast, range 100 circle, raidwide + arena transition
    VollokSmall = 37719, // Boss->self, 4.0s cast, single-target, visual (spawn 16 small fangs)
    Sync = 37721, // Boss->self, 5.0s cast, single-target, visual (mirror fangs to main arena)
    ChasmOfVollokFangSmall = 37785, // FangVollokSmall->self, 8.0s cast, range 5 width 5 rect, outside main area
    ChasmOfVollokFangSmallAOE = 37786, // Helper->self, 1.0s cast, range 5 width 5 rect, mirrored to main area
    HalfFullR = 37736, // Boss->self, 6.0s cast, single-target, visual (cleave right)
    HalfFullL = 37737, // Boss->self, 6.0s cast, single-target, visual (cleave left)
    HalfFullLongAOE = 37790, // Helper->self, 6.3s cast, range 60 width 120 rect
    GreaterGateway = 37761, // Boss->self, 4.0s cast, single-target, visual (create portals)
    BladeWarp = 37726, // Boss->self, 4.0s cast, single-target, visual (create swords)
    ForgedTrack = 37727, // Boss->self, 4.0s cast, single-target, visual (tether swords)
    ForgedTrackVisual = 37728, // Helper->ForgedTrackHelper, no cast, single-target, ???
    ForgedTrackPreview = 37788, // FangBlade->self, 10.9s cast, range 20 width 5 rect
    ForgedTrackAOE = 37789, // FangBlade->self, no cast, range 20 width 5 rect (narrow line)
    FieryEdge = 37762, // FangBlade->self, no cast, single-target, visual (wide/triple line)
    FieryEdgeAOECenter = 37763, // Helper->self, no cast, range 20 width 5 rect, central aoe
    FieryEdgeAOESide = 37764, // Helper->self, no cast, range 20 width 5 rect, side aoe
    StormyEdge = 37765, // FangBlade->self, no cast, single-target, visual (knockback line)
    StormyEdgeAOE = 37766, // Helper->self, 0.5s cast, range 20 width 5 rect, central aoe
    StormyEdgeKnockback = 37767, // Helper->self, 0.5s cast, range 10 width 20 rect, knockback 7
    ChasmOfVollokPlayer = 37769, // Helper->self, no cast, ???, aoe in player's cell

    ProjectionOfTriumph = 37770, // Boss->self, 5.0s cast, single-target, visual (lines with circles and donuts)
    SiegeOfVollok = 37771, // FangVollokSmall->self, 0.5s cast, range 3-8 donut
    WallsOfVollok = 37772, // FangVollokSmall->self, 0.5s cast, range 4 circle
    ProjectionOfTurmoil = 39560, // Boss->self, 5.0s cast, single-target, visual (line with stacks)
    MightOfVollok = 37773, // Helper->players, no cast, range 8 circle, shared damage with vuln
    BitterWhirlwind = 39229, // Boss->self, 5.0s cast, single-target, visual (3-hit aoe tankbuster with swaps/invuln)
    BitterWhirlwindAOEFirst = 39230, // Helper->player, 5.0s cast, range 5 circle, aoe tankbuster
    BitterWhirlwindRest = 39231, // Boss->self, no cast, single-target, visual (second+ hits)
    BitterWhirlwindAOERest = 39232, // Helper->player, no cast, range 5 circle, aoe tankbuster

    DrumOfVollok = 37774, // Boss->self, 7.4+0.6s cast, single-target, visual (enumerations)
    DrumOfVollokAOE = 37775, // Helper->players, 8.0s cast, range 4 circle, 2-man stack, partner gets knockback 25
    VollokLarge = 37778, // Boss->self, 5.0s cast, single-target, visual (spawn 2 large fangs)
    VollokLargeAOE = 37779, // FangVollokLarge->self, 8.0s cast, range 10 width 10 rect, aoe under large swords
    ChasmOfVollokFangLargeAOE = 37780, // Helper->self, 1.0s cast, range 10 width 10 rect, mirrored to other platform
    AeroIII = 37776, // Boss->self, 5.0s cast, single-target, visual (knockback zones)
    AeroIIIAOE = 37777, // BitingWind->self, no cast, range 4 circle, voidzone with knockback
    ForwardHalfLongR = 39322, // Boss->self, 9.0+1.0s cast, single-target, visual (jump forward, turn around, cleave right)
    ForwardHalfLongL = 39323, // Boss->self, 9.0+1.0s cast, single-target, visual (jump forward, turn around, cleave left)
    BackwardHalfLongR = 39324, // Boss->self, 9.0+1.0s cast, single-target, visual (jump back, cleave right)
    BackwardHalfLongL = 39325, // Boss->self, 9.0+1.0s cast, single-target, visual (jump back, cleave left)
    DutysEdge = 37748, // Boss->self, 4.9s cast, single-target, visual (line stack)
    DutysEdgeTarget = 35567, // Helper->player, no cast, single-target, visual (target selection for line stack)
    DutysEdgeRepeat = 37749, // Boss->self, no cast, single-target, visual (repeated hits)
    DutysEdgeAOE = 38055, // Helper->self, no cast, range 100 width 8 rect
    BurningChains = 37781, // Boss->self, 5.0s cast, single-target, visual (chains)
    BurningChainsAOE = 37782, // Helper->self, no cast, ???

    HalfCircuitCircle = 37739, // Boss->self, 7.0s cast, single-target, visual (circle + side cleave)
    HalfCircuitDonut = 37740, // Boss->self, 7.0s cast, single-target, visual (donut + side cleave)
    HalfCircuitAOERect = 37791, // Helper->self, 7.3s cast, range 60 width 120 rect
    HalfCircuitAOEDonut = 37792, // Helper->self, 7.0s cast, range 10-30 donut
    HalfCircuitAOECircle = 37793, // Helper->self, 7.0s cast, range 10 circle
    SmitingCircuitDonut = 37732, // HalfCircuitHelper->self, no cast, single-target, visual
    SmitingCircuitCircle = 37733, // HalfCircuitHelper->self, no cast, single-target, visual

    Enrage = 39274, // Boss->self, 10.0s cast, range 100 circle
}

public enum SID : uint
{
    //_Gen_ = 2397, // none->FangVollokSmall/HalfCircuitHelper, extra=0x2C7/0x2CD
    Projection = 4047, // none->player, extra=0x0
    //_Gen_Sprint = 481, // none->ProjectionOfTurmoil, extra=0x32
    //_Gen_WindResistanceDownII = 2096, // Helper/BitingWind->player, extra=0x0
    //_Gen_Liftoff = 3262, // BitingWind->player, extra=0x0
    //_Gen_BurningChains = 769, // none->player, extra=0x0
}

public enum IconID : uint
{
    ChasmOfVollok = 185, // player
    BitterWhirlwind = 343, // player
    DrumOfVollok = 539, // player
}

public enum TetherID : uint
{
    RegicidalRage = 89, // player->Boss
    ForgedTrack = 86, // FangBlade->Boss
    BurningChains = 128, // player->player
}
