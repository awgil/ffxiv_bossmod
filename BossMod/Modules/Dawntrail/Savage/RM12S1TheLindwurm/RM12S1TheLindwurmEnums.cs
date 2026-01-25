namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x22-43, Helper type
    Boss = 0x4AFB, // R13.800, x1
    RedOrb = 0x4B00, // R1.800, x0 (spawn during fight)
    GreenOrb = 0x4B01, // R1.800, x0 (spawn during fight)
    HelperUnknown = 0x4B02, // R5.000, x1, called "Lindwurm" but does nothing (maybe heart?)
    AutoAttacker = 0x4AFE, // R0.000-0.500, x2, Part type
    BloodVessel = 0x4AFF, // R0.800, x0 (spawn during fight)
    FloorSnake1 = 0x4AFD, // R1.000, x0 (spawn during fight), Helper type
    FloorSnake2 = 0x4B27, // R1.000, x0 (spawn during fight), Helper type
    FloorSnake3 = 0x4B28, // R1.000, x0 (spawn during fight), Helper type
    SnakeHead = 0x4AFC, // R4.000, x0 (spawn during fight), Helper type
}

public enum AID : uint
{
    AutoAttack = 46291, // AutoAttacker->player, no cast, single-target
    TheFixer = 46295, // Boss->self, 5.0s cast, range 60 circle

    MortalSlayerBoss = 46229, // Boss->self, 12.0s cast, single-target
    MortalSlayerSpread = 46230, // GreenOrb->players, no cast, range 6 circle
    MortalSlayerTank = 46232, // RedOrb->players, no cast, range 6 circle

    GrotesquerieAct1 = 48829, // Boss->self, 3.0s cast, single-target
    PhagocyteSpotlightPlayer = 46238, // Helper->location, 3.0s cast, range 5 circle
    RavenousReachBoss = 46185, // Boss->self, 1.0+10.7s cast, single-target
    RavenousReachHead1 = 46953, // SnakeHead->self, no cast, single-target
    RavenousReachHead2 = 46954, // SnakeHead->self, no cast, single-target
    RavenousReachCone = 46237, // Helper->self, 10.6s cast, range 35 120-degree cone
    BurstingGrotesquerieDramaticLysis = 46250, // Helper->location, no cast, range 6 circle
    SharedGrotesquerieFourthWallFusion = 46254, // Helper->location, no cast, range 6 circle
    HemorrhagicProjection = 46255, // Helper->self, no cast, range 60 30-degree cone

    Burst = 46239, // Helper->location, 2.5s cast, range 12 circle
    VisceralBurst = 46294, // Helper->players, no cast, range 6 circle, tankbuster
    Act1FourthWallFusion = 47545, // Helper->players, no cast, range 6 circle, stack

    GrotesquerieAct2 = 48830, // Boss->self, 3.0s cast, single-target
    PhagocyteSpotlightFixed = 46262, // Helper->location, 3.0s cast, range 5 circle
    CruelCoilBoss1 = 46264, // Boss->location, 3.0s cast, single-target
    CruelCoilBoss2 = 46265, // Boss->location, 3.0s cast, single-target
    CruelCoilBoss3 = 46266, // Boss->location, 3.0s cast, single-target
    CruelCoilBoss4 = 46267, // Boss->location, 3.0s cast, single-target
    CruelCoilBind = 46194, // Helper->self, no cast, range 60 circle
    SkinsplitterBoss = 46268, // Boss->self, no cast, single-target
    SkinsplitterDonut = 46398, // Helper->self, no cast, range 9?-13 donut
    DramaticLysisTetherBreak = 46260, // Helper->location, no cast, range 4 circle
    DramaticLysisTetherFail = 46261, // Helper->self, no cast, ???
    RoilingMass1 = 46259, // BloodVessel->self, 3.0s cast, range 3 circle
    RoilingMass2 = 46263, // BloodVessel->self, 3.0s cast, range 3 circle
    UnmitigatedExplosion = 46258, // Helper->self, no cast, range 60 circle
    ConstrictorBoss1 = 46269, // Boss->location, no cast, single-target
    ConstrictorBoss2 = 46270, // Boss->location, no cast, single-target
    ConstrictorBoss3 = 46271, // Boss->location, no cast, single-target
    ConstrictorBoss4 = 46272, // Boss->location, no cast, single-target
    ConstrictorUnk = 46273, // Helper->self, 3.0s cast, range 9 circle
    ConstrictorKill = 46274, // Helper->self, 1.0s cast, range 13 circle
    SplattershedBoss1 = 47555, // Boss->self, 3.0+2.1s cast, single-target
    SplattershedBoss2 = 47556, // Boss->self, 3.0+2.1s cast, single-target
    SplattershedRaidwide = 47558, // Helper->self, no cast, range 60 circle

    GrotesquerieAct3 = 48831, // Boss->self, 3.0s cast, single-target
    FeralFission = 48649, // Boss->self, 2.0s cast, single-target
    GrandEntranceAppear1 = 46240, // FloorSnake1->self, 3.0s cast, range 2 circle
    GrandEntranceAppear2 = 46241, // FloorSnake2->self, 3.0s cast, range 2 circle
    GrandEntranceAppear3 = 46242, // FloorSnake3->self, 3.0s cast, range 2 circle
    GrandEntranceDisappear = 46243, // Helper->self, 3.5s cast, range 2 circle
    BringDownTheHouseBoss = 48650, // Boss->self, no cast, single-target
    BringDownTheHouseLarge = 46244, // Helper->self, 1.0s cast, range 10 width 20 rect
    BringDownTheHouseMedium = 46245, // Helper->self, 1.0s cast, range 10 width 15 rect
    BringDownTheHouseSmall = 46246, // Helper->self, 1.0s cast, range 10 width 10 rect
    MitoticPhaseDramaticLysis = 46256, // Helper->location, no cast, range 9 circle
    MetamitosisTowerSpawn = 46257, // Helper->location, 1.5s cast, single-target
    MetamitosisTower = 47395, // Helper->self, 1.8s cast, range 3 circle
    SplitScourgeVisual = 46247, // SnakeHead->self, no cast, single-target
    SplitScourge = 46251, // Helper->self, no cast, range 60 width 10 rect
    VenomousScourge = 46248, // Helper->players, no cast, range 5 circle

    GrotesquerieCurtainCall = 48832, // Boss->self, 3.0s cast, single-target
    CellShedding = 46252, // Helper->players, no cast, range 6 circle
    CellDeath = 46253, // Helper->player, no cast, single-target, triggered by uncleansed debuff

    SlaughtershedBoss1 = 46275, // Boss->self, 3.0+2.1s cast, single-target
    SlaughtershedBoss2 = 46278, // Boss->self, 3.0+2.1s cast, single-target
    SlaughtershedRaidwide = 44489, // Helper->self, no cast, range 60 circle
    SerpentineScourgeWestEast = 46283, // Boss->self, no cast, single-target
    RaptorKnucklesWestEast = 46284, // Boss->self, no cast, single-target
    SerpentineScourgeEastWest = 46285, // Boss->self, no cast, single-target
    RaptorKnucklesEastWest = 46286, // Boss->self, no cast, single-target
    CurtainCallDramaticLysis = 46292, // Helper->players, no cast, range 6 circle
    CurtainCallFourthWallFusion = 46293, // Helper->players, no cast, range 6 circle
    SerpentineScourgeBoss1 = 46289, // Boss->self, no cast, single-target
    SerpentineScourgeBoss2 = 46290, // Boss->self, no cast, single-target
    SerpentineScourgeRect = 47548, // Helper->self, 1.0s cast, range 30 width 20 rect
    RaptorKnucklesBoss1 = 46287, // Boss->self, no cast, single-target
    RaptorKnucklesBoss2 = 46288, // Boss->self, no cast, single-target
    RaptorKnucklesKB = 47559, // Helper->self, 0.8s cast, range 60 circle

    TheFixerEnrage = 45767, // Boss->self, 5.0s cast, range 60 circle

    UnkBoss1 = 47044, // Boss->self, no cast, single-target
    UnkBoss2 = 47045, // Boss->self, no cast, single-target
    UnkBoss3 = 47579, // Boss->self, no cast, single-target
    UnkBoss4 = 46190, // Boss->self, no cast, single-target
    UnkBoss5 = 46201, // Boss->self, no cast, single-target

    UnkSnake1 = 46234, // SnakeHead->self, no cast, single-target
    UnkSnake2 = 46235, // SnakeHead->self, no cast, single-target

    UnkFloor1 = 46447, // FloorSnake1->self, no cast, single-target
    UnkFloor2 = 46448, // FloorSnake2->self, no cast, single-target
    UnkFloor3 = 46449, // FloorSnake3->self, no cast, single-target
}

public enum SID : uint
{
    UnkOrb = 3792, // Boss->GreenOrb/RedOrb, extra=0x12/0x16/0x13/0x17/0x14/0x15/0x76/0x18
    UnkBoss = 3913, // none->Boss, extra=0x444

    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    PoisonResistanceDownII = 3935, // GreenOrb/RedOrb/BloodVessel->player, extra=0x0

    DirectedGrotesquerie = 4976, // none->player, extra=0x0
    DirectedGrotesquerieVisual = 3558, // none->player, extra=0x40F/0x40D/0x40E/0x40C/0x436/0x437/0x438/0x439
    BurstingGrotesquerie = 4761, // Helper->player, extra=0x0
    SharedGrotesquerie = 4762, // none->player, extra=0x0
    RottingFlesh = 4763, // none->player, extra=0x0
    MitoticPhase = 4764, // none->player, extra=0x0

    BondsA = 4752, // none->player, extra=0x0
    UnbreakableA = 4753, // none->player, extra=0x0
    BondsB = 4754, // none->player, extra=0x0
    UnbreakableB = 4755, // none->player, extra=0x0
    FirstInLine = 3004, // none->player, extra=0x0
    SecondInLine = 3005, // none->player, extra=0x0
    ThirdInLine = 3006, // none->player, extra=0x0
    FourthInLine = 3451, // none->player, extra=0x0

    Bind = 2518, // none->player, extra=0x0
    FateOfTheWurm = 4772, // none->player, extra=0x0, disables jump
}

public enum IconID : uint
{
    Share = 161, // player->self
    Tankbuster = 344, // player->self
    CellChainSpawn = 657, // player->self
    SlaughtershedSpread = 375, // player->self
    SlaughtershedStack = 317, // player->self
}

public enum TetherID : uint
{
    Cell = 366, // player->player
}
