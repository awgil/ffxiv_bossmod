namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

public enum OID : uint
{
    DaryaTheSeaMaid = 0x4A99, // R5.000, x1
    Helper = 0x233C, // R0.500, x35, Helper type
    SeabornSteed = 0x4A9A, // R2.200, x0 (spawn during fight)
    SeabornShrike = 0x4A9E, // R1.600, x0 (spawn during fight)
    SeabornSoldier = 0x4A9C, // R2.200, x0 (spawn during fight)
    SeabornServant = 0x4A9D, // R1.600, x0 (spawn during fight)
    SeabornSteward = 0x4A9B, // R2.200, x0 (spawn during fight)
    BlueOrb = 0x1EBF1B, // R0.500, x0 (spawn during fight), EventObj type
    BlueSphere = 0x1EBF1C,// R0.500, x0 (spawn during fight), EventObj type
    DonutSphere = 0x1EBF1D, // R0.500, x0 (spawn during fight), EventObj type
    WaterTile = 0x1EBF1E, // R0.500, x0 (spawn during fight), EventObj type - bad tile
}

public enum STATE : uint
{ // States for BlueSphere & DonutSphere
    FirstState = 1048608,
    SecondState = 4194432,
    ThirdState = 262152,
}

public enum AID : uint
{
    AutoAttack = 45838, // DaryaTheSeaMaid->player, no cast, single-target
    Ability = 45770, // DaryaTheSeaMaid->location, no cast, single-target

    PiercingPlunge = 45870, // DaryaTheSeaMaid->self, 5.0s cast, range 70 circle

    FamiliarCall = 45771, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    EchoedSerenade = 45773, // DaryaTheSeaMaid->self, 8.5+0.5s cast, range 60 circle
    EchoedReprise = 45844, // Boss->self, 4.0+0.5s cast, range 60 circle
    Watersong = 45843, // 4A9E->self, 1.0s cast, range 45 ?-degree cone
    Watersong1 = 45839, // 4A9A->self, 1.0s cast, range 40 width 8 rect
    Watersong2 = 45841, // 4A9C->self, 1.0s cast, range 40 width 8 rect
    Watersong3 = 45842, // 4A9D->self, 1.0s cast, range 20 ?-degree cone
    Watersong4 = 45840, // 4A9B->self, 1.0s cast, range 40 width 8 rect

    Hydrobullet = 45847, // DaryaTheSeaMaid->self, no cast, single-target
    Hydrobullet1 = 45848, // Helper->player, 3.0s cast, range 15 circle

    SurgingCurrent = 45865, // DaryaTheSeaMaid->self, 5.0+1.0s cast, single-target
    SurgingCurrent1 = 45866, // Helper->self, 6.0s cast, range 60 ?-degree cone

    AlluringOrder = 45861, // DaryaTheSeaMaid->self, 4.0s cast, range 70 circle

    SwimmingInTheAir = 45845, // DaryaTheSeaMaid->self, 4.0s cast, single-target
    Hydrofall = 45846, // Helper->location, 1.0s cast, range 12 circle
    Tidalspout = 45858, // Helper->players, no cast, range 6 circle

    CeaselessCurrent = 45862, // DaryaTheSeaMaid->self, 4.0+1.0s cast, single-target
    CeaselessCurrent1 = 45863, // Helper->self, 5.0s cast, range 8 width 40 rect
    CeaselessCurrent2 = 45864, // Helper->self, no cast, range 8 width 40 rect
    CrossCurrent = 45860, // Helper->self, no cast, range 36 width 8 cross

    SunkenTreasure = 45849, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target

    AquaSpear = 45852, // Boss->self, 3.0s cast, single-target
    WaterZone1 = 45853, // Helper->self, 6.0s cast, range 8 width 8 rect
    WaterZone2 = 45855, // Helper->self, no cast, range 8 width 8 rect
    WaterZone3 = 45854, // Helper->player, 6.0s cast, single-target

    AquaBall = 45867, // Boss->self, 2.0+1.0s cast, single-target
    AquaBall1 = 45868, // Helper->location, 3.0s cast, range 5 circle

    Spell = 45859, // Helper->player, 6.0s cast, single-target
    SphereShatter = 45850, // Helper->self, no cast, range 18 circle
    SphereShatter1 = 45851, // Helper->self, no cast, range ?-20 donut

    SeaShackles = 45856, // DaryaTheSeaMaid->self, 4.0+1.0s cast, range 70 circle
    Hydrobullet2 = 47088, // Helper->players, no cast, range 15 circle
    Explosion = 45857, // Helper->player, no cast, single-target

    PiercingPlungeEnrage = 45872, // Boss->self, 10.0s cast, range 70 circle
    PiercingPlungeEnrage1 = 45873, // Boss->self, no cast, range 70 circle
}

public enum SID : uint
{
    ForcedMarch = 1257, // none->player, extra=0x1/0x2/0x8
    ForwardMarch = 2161, // none->player, extra=0x0
    AboutFace = 2162, // none->player, extra=0x0
    LeftFace = 2163, // none->player, extra=0x0
    RightFace = 2164, // none->player, extra=0x0
    TidalspoutTarget = 4726, // none->player, extra=0x0
    NearShoreShackles = 4724, // none->player, extra=0x0
    FarShoreShackles = 4725, // none->player, extra=0x0
    HydrobulletTarget = 4968, // none->player, extra=0x0
}

public enum IconID : uint
{
    CrossCurrent = 20, // player->self
    Hydrobullet = 22, // player->self
    YellowTile = 185, // player->self
}

public enum TetherID : uint
{
    Far = 129, // player->player
    Near = 130, // player->player
    Safe = 3, // player->player
}