namespace BossMod.Endwalker.Alliance.A33Oschon;

public enum OID : uint
{
    BossP1 = 0x406D, // R8.000, x1
    Avatar = 0x406E, // R8.000, x4
    BossP2 = 0x406F, // R24.990, spawn during fight
    Helper = 0x233C, // R0.500, x40, 523 type
}

public enum AID : uint
{
    // P1
    AutoAttackP1 = 35906, // BossP1->player, no cast, single-target
    Teleport = 35224, // BossP1->location, no cast, single-target
    SuddenDownpour = 35225, // BossP1->self, 4.0s cast, single-target, visual (raidwide)
    SuddenDownpourAOE = 36026, // Helper->self, 5.0s cast, range 60 circle, raidwide

    TrekShotN = 35213, // BossP1->self, 6.0s cast, single-target, visual (cone shifted to the north)
    TrekShotS = 35214, // BossP1->self, 6.0s cast, single-target, visual (cone shifted to the south)
    TrekShotNShiftIndicator = 34862, // Helper->self, no cast, single-target, visual (origin shift indicator)
    TrekShotSShiftIndicator = 34863, // Helper->self, no cast, single-target, visual (origin shift indicator)
    TrekShotNAOE = 35215, // Helper->self, 9.5s cast, range 65 120-degree cone
    TrekShotSAOE = 35908, // Helper->self, 9.5s cast, range 65 120-degree cone

    Reproduce = 35209, // BossP1->self, 3.0s cast, single-target, visual (summons an Avatar adds for Swinging Draws)
    SwingingDrawCW = 35210, // Avatar->self, 7.0s cast, single-target, visual (cone shifted CW)
    SwingingDrawCCW = 35211, // Avatar->self, 7.0s cast, single-target, visual (cone shifted CCW)
    SwingingDrawCWShiftIndicator = 34864, // Helper->self, no cast, single-target, visual (origin shift indicator)
    SwingingDrawCCWShiftIndicator = 34865, // Helper->self, no cast, single-target, visual (origin shift indicator)
    SwingingDrawAOE = 35212, // Avatar->self, 2.0s cast, range 65 120-degree cone

    FlintedFoehnP1 = 35235, // BossP1->self, 4.5s cast, single-target, visual (multihit stack)
    FlintedFoehnP1AOE = 35237, // Helper->players, no cast, range 6 circle, stack x6
    SoaringMinuet1 = 36110, // BossP1->self, 5.0s cast, range 65 270-degree cone (standalone version)
    SoaringMinuet2 = 35220, // BossP1->self, 5.0s cast, range 65 270-degree cone (after downhill/climbing shot)
    ArrowP1 = 35227, // BossP1->self, 4.0s cast, single-target, visual (tankbusters)
    ArrowP1AOE = 35229, // Helper->players, 5.0s cast, range 6 circle, aoe tankbuster

    DownhillP1 = 35231, // BossP1->self, 3.0s cast, single-target, visual (puddles)
    DownhillP1AOE = 35233, // Helper->location, 8.5s cast, range 6 circle
    ClimbingShot1 = 35216, // BossP1->self, 5.0s cast, range 80 circle, knockback 20 (not sure about difference between versions)
    ClimbingShot2 = 35217, // BossP1->self, 5.0s cast, range 80 circle, knockback 20 (not sure about difference between versions)
    ClimbingShot3 = 35218, // BossP1->self, 5.0s cast, range 80 circle, knockback 20 (not sure about difference between versions)
    ClimbingShot4 = 35219, // BossP1->self, 5.0s cast, range 80 circle, knockback 20 (not sure about difference between versions)

    // intermission
    LoftyPeaks = 35239, // BossP1->self, 5.0s cast, single-target, visual (untargetable)
    MovingMountains = 36067, // Helper->self, no cast, range 60 circle, raidwide x3
    PeakPeril = 36068, // Helper->self, no cast, range 60 circle, raidwide
    Shockwave = 35240, // Helper->self, 8.4s cast, range 60 circle, raidwide

    // P2
    AutoAttackP2 = 35907, // BossP2->player, no cast, single-target
    PitonPullNW = 35241, // BossP2->self, 8.0s cast, single-target, visual (NW/SE aoes)
    PitonPullNE = 35242, // BossP2->self, 8.0s cast, single-target, visual (NE/SW aoes)
    PitonPullAOE = 35243, // Helper->location, 8.5s cast, range 22 circle
    Altitude = 35247, // BossP2->location, 6.0s cast, single-target, visual (puddles with staggered telegraphs)
    AltitudeVisual = 35248, // Helper->location, 2.0s cast, range 6 circle, visual (single puddle telegraph)
    AltitudeAOE = 35249, // Helper->location, 7.0s cast, range 6 circle
    FlintedFoehnP2 = 35236, // BossP2->self, 4.5s cast, single-target, visual (multihit stack)
    FlintedFoehnP2AOE = 35238, // Helper->players, no cast, range 8 circle, stack x6
    WanderingShotN = 36086, // BossP2->self, 7.0s cast, range 40 width 40 rect, visual (huge circle N)
    WanderingShotS = 36087, // BossP2->self, 7.0s cast, range 40 width 40 rect, visual (huge circle S)
    GreatWhirlwind = 35246, // Helper->location, 3.6s cast, range 23 circle
    ArrowP2 = 35228, // BossP2->self, 6.0s cast, single-target, visual (tankbusters)
    ArrowP2AOE = 35230, // Helper->player, 7.0s cast, range 10 circle, aoe tankbuster

    ArrowTrail = 35250, // BossP2->self, 3.0s cast, single-target, visual (exaflares)
    ArrowTrailHint = 35251, // Helper->self, 2.0s cast, range 40 width 10 rect, visual (next exaflare lane)
    ArrowTrailAOE = 35252, // Helper->self, no cast, range 10 width 10 rect, exaflare
    ArrowTrailDownhill = 35909, // Helper->location, 3.0s cast, range 6 circle

    WanderingVolleyDownhill = 35232, // BossP2->self, 3.0s cast, single-target, visual (puddles)
    WanderingVolleyDownhillAOE = 35234, // Helper->location, 14.0s cast, range 8 circle
    WanderingVolleyN = 35244, // BossP2->self, 10.0s cast, range 40 width 40 rect, knockback 12 left/right + huge circle N
    WanderingVolleyS = 35245, // BossP2->self, 10.0s cast, range 40 width 40 rect, knockback 12 left/right + huge circle S
}

public enum IconID : uint
{
    FlintedFoehn = 316, // player
    ArrowP1 = 344, // player
    TankbusterP2 = 500, // player
}
