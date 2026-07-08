namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

public enum OID : uint {
    DaryaSeaMaid = 0x4A94,
    Helper = 0x233C,
    SeabornStalwart = 0x4A96, // R2.000, x0 (spawn during fight)
    SeabornSteed = 0x4A95, // R2.200, x0 (spawn during fight) - Horse
    SeabornSteward = 0x4A97, // R2.200, x0 (spawn during fight) - Turtle
    SeabornSoldier = 0x4A98, // R2.200, x0 (spawn during fight) - Crab
    SwimmingInTheAirOrb = 0x1EBF1B, // R0.500, x0 (spawn during fight), EventObj type
    BlueSphere = 0x1EBF1C,// R0.500, x0 (spawn during fight), EventObj type
    DonutSphere = 0x1EBF1D, // R0.500, x0 (spawn during fight), EventObj type
    AquaSpearTile = 0x1EBF1E, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint {
    AutoAttack = 45769, // Boss->player, no cast, single-target
    Teleport = 45770, // Boss->location, no cast, single-target - Boss teleports to the middle of the map

    PiercingPlunge = 45837, // Boss->self, 5.0s cast, range 70 circle

    FamiliarCall = 45771, // Boss->self, 3.0+1.0s cast, single-target
    EchoedSerenade = 45773, // Boss->self, 8.5+0.5s cast, range 60 circle
    Watersong = 45808, // 4A98->self, 1.0s cast, range 40 width 8 rect
    Watersong1 = 45807, // 4A97->self, 1.0s cast, range 40 width 8 rect
    Watersong2 = 45806, // 4A96->self, 1.0s cast, range 40 width 8 rect
    Watersong3 = 45805, // 4A95->self, 1.0s cast, range 40 width 8 rect

    SunkenTreasure = 45812, // Boss->self, 3.0+1.0s cast, single-target
    SphereShatter = 45814, // Helper->self, no cast, range ?-20 donut
    SphereShatter1 = 45813, // Helper->self, no cast, range 18 circle
    HydrobulletCast = 45815, // Boss->self, 3.0+1.0s cast, single-target
    HydrobulletSpread = 45816, // Helper->player, 5.0s cast, range 15 circle - used during SunkenTreasure

    Hydrocannon = 45801, // Boss->self, 4.0+1.0s cast, single-target
    Hydrocannon1 = 45836, // Helper->self/player, 5.0s cast, range 70 width 6 rect

    AquaSpear = 45817, // Boss->self, 4.0s cast, single-target
    AquaSpear1 = 45818, // Helper->self, 3.0s cast, range 8 width 8 rect
    SeaShackles = 45821, // Boss->self, 4.0+1.0s cast, range 70 circle
    TidalWave = 45819, // Boss->self, 4.0+1.0s cast, single-target
    TidalWave1 = 45820, // Helper->self, 6.0s cast, range 60 width 60 rect

    SwimmingInTheAir = 45809, // Boss->self, 4.0s cast, single-target
    Hydrofall = 45810, // Helper->location, 1.0s cast, range 12 circle
    HydrobulletSpread2 = 45811, // Helper->player, 6.0s cast, range 15 circle - used during SwimmingInTheAir

    CeaselessCurrentBoss = 45823, // Boss->self, 4.0+1.0s cast, single-target
    CeaselessCurrentFirst = 45824, // Helper->self, 5.0s cast, range 8 width 40 rect
    CeaselessCurrentRest = 45825, // Helper->self, no cast, range 8 width 40 rect

    SurgingCurrent = 45826, // Boss->self, 7.0+1.0s cast, single-target
    SurgingCurrent1 = 45827, // Helper->self, 8.0s cast, range 60 ?-degree cone

    AlluringOrder = 47090, // Boss->self, 4.0s cast, range 70 circle
    AquaBall = 45834, // Boss->self, 2.0+1.0s cast, single-target
    AquaBall1 = 45835, // Helper->location, 3.0s cast, range 5 circle

    RecedingTwinTides = 45828, // Boss->self, 3.0+1.0s cast, single-target
    NearTide = 45829, // Helper->location, 4.0s cast, range 10 circle
    FarTide = 45833, // Helper->location, no cast, range ?-40 donut
}

public enum SID : uint {
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 1789, // 4A96->player, extra=0x1

    LeftFace = 2163, // Boss->player, extra=0x0
    ForwardMarch = 2161, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x1/0x4

    NearShoreShackles = 4724, // none->player, extra=0x0
    Dropsy = 3797, // none->player, extra=0x0
}

public enum VfxID : uint {
    Horse = 2741,
    Stalwart = 2742,
    Turtle = 2743,
    Crab = 2744
}

public enum IconID : uint {
    Hydrobullet = 23, // player->self
    Hydrocannon = 471, // player->self
    Hydrobullet2 = 658, // player->self
}

public enum TetherID : uint {
    SeaShackles = 376, // player->player
    SeaShacklesSafe = 377, // player->player
}
