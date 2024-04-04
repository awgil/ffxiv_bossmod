namespace BossMod.Endwalker.Alliance.A33Oschon;

public enum OID : uint
{
    OschonP1 = 0x406D, // R8.000, x1
    OschonP2 = 0x406F, // R24.990, spawn during fight
    OschonsAvatar = 0x406E, // R8.000, x4
    OschonHelper = 0x233C, // R0.500, x40, 523 type
    Unknown = 0x400E, // R0.500, x1
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    PedestalOfPassage = 0x1EB91A, // R0.500, x1, EventObj type
    ExitToTheOmphalos = 0x1EB91E, // R0.500, x1, EventObj type
};

public enum AID : uint
{
    AutoAttack = 35906, // Oschon->player, no cast, single-target

    Teleport = 35224, // Oschon->location, no cast, single-target, boss teleports mid
    Ability2 = 34863, // OschonHelper->self, no cast, single-target
    Ability3 = 34864, // OschonHelper->self, no cast, single-target
    Ability4 = 34862, // OschonHelper->self, no cast, single-target
    Ability5 = 34865, // OschonHelper->self, no cast, single-target

    TrekShot1 = 35214, // Oschon->self, 6.0s cast, single-target
    TrekShot2 = 35908, // OschonHelper->self, 9.5s cast, range 65 120-degree cone
    TrekShot3 = 35213, // Oschon->self, 6.0s cast, single-target
    TrekShot4 = 35215, // OschonHelper->self, 9.5s cast, range 65 120-degree cone

    SwingingDraw1 = 35210, // OschonsAvatar->self, 7.0s cast, single-target
    SwingingDraw2 = 35212, // OschonsAvatar->self, 2.0s cast, range 65 120-degree cone
    SwingingDraw3 = 35211, // OschonsAvatar->self, 7.0s cast, single-target
    Reproduce = 35209, // Oschon->self, 3.0s cast, single-target,s ummons an OschonsAvatar add - add will use Swinging Draw

    SuddenDownpour1 = 35225, // Oschon->self, 4.0s cast, single-target
    SuddenDownpour2 = 36026, // OschonHelper->self, 5.0s cast, range 60 circle, raidwide 

    Downhill1 = 35231, // Oschon->self, 3.0s cast, single-target
    Downhill2 = 35233, // OschonHelper->location, 8.5s cast, range 6 circle
    ClimbingShot = 35217, // Oschon->self, 5.0s cast, range 80 circle, knockback 20, dir away from source
    ClimbingShot2 = 35216, // Oschon->self, 5.0s cast, range 80 circle, knockback 20, dir away from source
    ClimbingShot3 = 35219, // Oschon->self, 5,0s cast, range 80 circle, knockback 20, dir away from source
    ClimbingShot4 = 35218, // Boss->self, 5,0s cast, range 80 circle, knockback 20, dir away from source

    SoaringMinuet1 = 36110, // Oschon->self, 5.0s cast, range 65 270-degree cone
    SoaringMinuet2 = 35220, // Oschon->self, 5.0s cast, range 65 270-degree cone

    FlintedFoehnVisual = 35235, // Oschon->self, 4.5s cast, single-target
    FlintedFoehnStack = 35237, // OschonHelper->players, no cast, range 6 circle, stack 6 times

    TheArrowVisual = 35227, // Oschon->self, 4.0s cast, single-target
    TheArrow = 35229, // OschonHelper->players, 5.0s cast, range 6 circle, tankbusters

    //on phase change
    LoftyPeaks = 35239, // Oschon->self, 5.0s cast, single-target, applies status effect 2970
    MovingMountains = 36067, // OschonHelper->self, no cast, range 60 circle, raidwide x3
    PeakPeril = 36068, // OschonHelper->self, no cast, range 60 circle, raidwide
    Shockwave = 35240, // OschonHelper->self, 8.4s cast, range 60 circle, raidwide

    _AutoAttack_ = 35907, // OschonBig->player, no cast, single-target
    PitonPull1Visual = 35241, // OschonBig->self, 8.0s cast, single-target // NW and ES Visual
    PitonPull3Visual2 = 35242, // OschonBig->self, 8.0s cast, single-target // NE and SW Visual
    PitonPullAOE = 35243, // OschonHelper->location, 8.5s cast, range 22 circle // Massive AOEs

    AltitudeVisual1 = 35247, // OschonBig->location, 6.0s cast, single-target // Visual
    AltitudeVisual2 = 35248, // OschonHelper->location, 2.0s cast, range 6 circle
    AltitudeAOE = 35249, // OschonHelper->location, 7.0s cast, range 6 circle // Multiple AOEs

    //For the life of me couldnt figure out why this would not appear
    FlintedFoehnVisualP2 = 35236, // OschonBig->self, 4.5s cast, single-target // Visual
    FlintedFoehnStackP2 = 35238, // OschonHelper->players, no cast, range 8 circle // Multihit party stack

    WanderingShotVisual = 36087, // OschonBig->self, 7.0s cast, range 40 width 40 rect
    WanderingShot2 = 36086, // OschonBig->self, 7.0s cast, range 40 width 40 rect

    GreatWhirlwindAOE = 35246, // OschonHelper->location, 3.6s cast, range 23 circle // Massive AOE

    TheArrowVisualP2 = 35228, // OschonBig->self, 6.0s cast, single-target
    TheArrowP2 = 35230, // OschonHelper->player, 7.0s cast, range 10 circle // Tankbuster


    //Not finished
    ArrowTrailVisual = 35250, // OschonBig->self, 3.0s cast, single-target
    ArrowTrailAOE = 35252, // OschonHelper->self, no cast, range 10 width 10 rect // Arrows will travel down several columns in the arena, telegraphed by red areas
    ArrowTrailRectAOE = 35251, // OschonHelper->self, 2.0s cast, range 40 width 10 rect // telegraph for ArrowTrailAOE

    DownhillVisual = 35232, // OschonBig->self, 3.0s cast, single-target
    DownhillSmallAOE = 35909, // OschonHelper->location, 3.0s cast, range 6 circle
    DownhillBigAOE = 35234, // OschonHelper->location, 14.0s cast, range 8 circle

    WanderingVolley = 35245, // OschonBig->self, 10.0s cast, range 40 width 40 rect
    WanderingVolley2 = 35244, // OschonBig->self, 10.0s cast, range 40 width 40 rect
};

public enum IconID : uint
{
    FlintedFoehnStack = 316, // player
    TankbusterP1 = 344, // player
    TankbusterP2 = 500, // player
};