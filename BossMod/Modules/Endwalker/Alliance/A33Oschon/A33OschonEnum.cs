namespace BossMod.Endwalker.Alliance.A33Oschon;

public enum OID : uint
{
    OschonP1 = 0x406D, // R8.000, x1
    OschonP2 = 0x406F, // R24.990, spawn during fight
    OschonsAvatar = 0x406E, // R8.000, x4
    Helper = 0x233C, // R0.500, x40, 523 type
};

public enum AID : uint
{
    AutoAttack = 35906, // Oschon->player, no cast, single-target

    Teleport = 35224, // Oschon->location, no cast, single-target, boss teleports mid
    TrekShotVisual1 = 34863, // OschonHelper->self, no cast, single-target
    TrekShotVisual2 = 34862, // OschonHelper->self, no cast, single-target
    TrekShotVisual3 = 35214, // Oschon->self, 6.0s cast, single-target
    TrekShotVisual4 = 35213, // Oschon->self, 6.0s cast, single-target
    TrekShot = 35908, // OschonHelper->self, 9.5s cast, range 65 120-degree cone
    TrekShot2 = 35215, // OschonHelper->self, 9.5s cast, range 65 120-degree cone

    Reproduce = 35209, // Oschon->self, 3.0s cast, single-target, summons an OschonsAvatar adds for Swinging Draws
    SwingingDrawCW = 35210, // OschonsAvatar->self, 7.0s cast, single-target
    SwingingDrawCCW = 35211, // OschonsAvatar->self, 7.0s cast, single-target
    SwingingDrawVisualCW = 34864, // OschonHelper->self, no cast, single-target
    SwingingDrawVisualCCW = 34865, // OschonHelper->self, no cast, single-target
    SwingingDraw = 35212, // OschonsAvatar->self, 2.0s cast, range 65 120-degree cone

    SuddenDownpour1 = 35225, // Oschon->self, 4.0s cast, single-target
    SuddenDownpour2 = 36026, // OschonHelper->self, 5.0s cast, range 60 circle, raidwide 

    DownhillVisual = 35231, // Oschon->self, 3.0s cast, single-target
    Downhill = 35233, // OschonHelper->location, 8.5s cast, range 6 circle
    ClimbingShot = 35217, // Oschon->self, 5.0s cast, range 80 circle, knockback 20, dir away from source
    ClimbingShot2 = 35216, // Oschon->self, 5.0s cast, range 80 circle, knockback 20, dir away from source
    ClimbingShot3 = 35219, // Oschon->self, 5,0s cast, range 80 circle, knockback 20, dir away from source
    ClimbingShot4 = 35218, // Oschon->self, 5,0s cast, range 80 circle, knockback 20, dir away from source

    SoaringMinuet1 = 36110, // Oschon->self, 5.0s cast, range 65 270-degree cone
    SoaringMinuet2 = 35220, // Oschon->self, 5.0s cast, range 65 270-degree cone

    FlintedFoehnVisual = 35235, // Oschon->self, 4.5s cast, single-target
    FlintedFoehnStack = 35237, // OschonHelper->players, no cast, range 6 circle, stack 6 times

    TheArrowVisual = 35227, // Oschon->self, 4.0s cast, single-target
    TheArrow = 35229, // OschonHelper->players, 5.0s cast, range 6 circle, tankbusters

    //during phase change
    LoftyPeaks = 35239, // Oschon->self, 5.0s cast, single-target, applies status effect 2970
    MovingMountains = 36067, // OschonHelper->self, no cast, range 60 circle, raidwide x3
    PeakPeril = 36068, // OschonHelper->self, no cast, range 60 circle, raidwide
    Shockwave = 35240, // OschonHelper->self, 8.4s cast, range 60 circle, raidwide

    //P2
    AutoAttackP2 = 35907, // OschonBig->player, no cast, single-target

    PitonPull1Visual = 35241, // OschonBig->self, 8.0s cast, single-target, NW and ES visual
    PitonPull3Visual2 = 35242, // OschonBig->self, 8.0s cast, single-target, NE and SW visual
    PitonPull = 35243, // OschonHelper->location, 8.5s cast, range 22 circle

    AltitudeVisual1 = 35247, // OschonBig->location, 6.0s cast, single-target
    AltitudeVisual2 = 35248, // OschonHelper->location, 2.0s cast, range 6 circle
    Altitude = 35249, // OschonHelper->location, 7.0s cast, range 6 circle

    FlintedFoehnVisualP2 = 35236, // OschonBig->self, 4.5s cast, single-target
    FlintedFoehnStackP2 = 35238, // OschonHelper->players, no cast, range 8 circle, stack 6 times

    WanderingShotVisual = 36087, // OschonBig->self, 7.0s cast, range 40 width 40 rect
    WanderingShotVisual2 = 36086, // OschonBig->self, 7.0s cast, range 40 width 40 rect
    GreatWhirlwind = 35246, // OschonHelper->location, 3.6s cast, range 23 circle

    TheArrowVisualP2 = 35228, // OschonBig->self, 6.0s cast, single-target
    TheArrowP2 = 35230, // OschonHelper->player, 7.0s cast, range 10 circle, tankbuster

    ArrowTrailVisual = 35250, // OschonBig->self, 3.0s cast, single-target
    ArrowTrailExa = 35252, // OschonHelper->self, no cast, range 10 width 10 rect
    ArrowTrailTelegraph = 35251, // OschonHelper->self, 2.0s cast, range 40 width 10 rect

    DownhillVisualP2 = 35232, // OschonBig->self, 3.0s cast, single-target
    DownhillSmall = 35909, // OschonHelper->location, 3.0s cast, range 6 circle
    DownhillBig = 35234, // OschonHelper->location, 14.0s cast, range 8 circle

    WanderingVolley = 35245, // OschonBig->self, 10.0s cast, range 40 width 40 rect, damage fall off raidwide, knockback 12 left/right
    WanderingVolley2 = 35244, // OschonBig->self, 10.0s cast, range 40 width 40 rect, damage fall off raidwide, knockback 12 left/right
};

public enum IconID : uint
{
    FlintedFoehnStack = 316, // player
    TankbusterP1 = 344, // player
    TankbusterP2 = 500, // player
};
