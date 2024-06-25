namespace BossMod.Endwalker.Variant.V02MR.V022Moko;

public enum OID : uint
{
    Boss = 0x3F81, // R6.000, x1 //Path 1, 3, and 4
    BossPath2 = 0x3FB3, // R3.450, x1 //Path 2
    Helper = 0x233C, // R0.500, x20, 523 type
    AshigaruSohei1 = 0x3F82, // R1.000, x12
    AshigaruKyuhei = 0x3F84, // R1.000, x6
    AncientKatana = 0x3F88, // R1.500, x15
    AshigaruSohei2 = 0x3F83, // R1.000, x8
    OnisClaw = 0x3F85, // R13.200, x6
    IllComeTengu = 0x3F86, // R2.000, x8
    Spiritflame = 0x3F87, // R2.400, x12
}

public enum AID : uint
{
    AutoAttack = 34698, // Boss->player, no cast, single-target
    Teleport = 34223, // Boss->location, no cast, single-target

    KenkiRelease = 34221, // Boss->self, 5.0s cast, range 60 circle raidwide
    AzureAuspice = 34204, // Boss->self, 5.0s cast, range 6-40 donut

    UpwellFirst = 34207, // Helper->self, 8.0s cast, range 60 width 10 rect
    UpwellRest = 34208, // Helper->self, 1.0s cast, range 60 width 5 rect

    BoundlessAzure = 34205, // Boss->self, 2.4s cast, single-target
    BoundlessAzureAOE = 34206, // Helper->self, 3.0s cast, range 60 width 10 rect

    IaiKasumiGiri1 = 34183, // Boss->self, 5.0s cast, range 60 270-degree cone
    IaiKasumiGiri2 = 34184, // Boss->self, 5.0s cast, range 60 270-degree cone
    IaiKasumiGiri3 = 34185, // Boss->self, 5.0s cast, range 60 270-degree cone
    IaiKasumiGiri4 = 34186, // Boss->self, 5.0s cast, range 60 270-degree cone

    DoubleKasumiGiriFirst1 = 34187, // Boss->self, 11.0s cast, range 60 270-degree cone
    DoubleKasumiGiriFirst2 = 34188, // Boss->self, 11.0s cast, range 60 270-degree cone
    DoubleKasumiGiriFirst3 = 34189, // Boss->self, 11.0s cast, range 60 270-degree cone
    DoubleKasumiGiriFirst4 = 34190, // Boss->self, 11.0s cast, range 60 270-degree cone

    DoubleKasumiGiriRest1 = 34191, // Boss->self, 1.5s cast, range 60 270-degree cone
    DoubleKasumiGiriRest2 = 34192, // Boss->self, 1.5s cast, range 60 270-degree cone
    DoubleKasumiGiriRest3 = 34193, // Boss->self, 1.5s cast, range 60 270-degree cone
    DoubleKasumiGiriRest4 = 34194, // Boss->self, 1.5s cast, range 60 270-degree cone

    SoldiersOfDeath = 34195, // Boss->self, 3.0s cast, single-target
    IronRain = 34196, // AshigaruKyuhei->location, 8.0s cast, range 10 circle

    //Route 1
    UntemperedSword = 34216, // Boss->self, 3.0s cast, single-target
    Unsheathing = 34217, // AncientKatana->location, 2.0s cast, range 3 circle
    VeilSever = 34218, // AncientKatana->self, 5.0s cast, range 40 width 5 rect

    //Route 2
    ScarletAuspice = 34200, // Boss->self, 5.0s cast, range 6 circle
    MoonlessNight = 34219, // Boss->self, 3.0s cast, range 60 circle
    Clearout = 34220, // OnisClaw->self, 5.0s cast, range 22 180-degree cone
    BoundlessScarletVisual = 34201, // Boss->self, 2.4s cast, single-target
    BoundlessScarlet = 34202, // Helper->self, 3.0s cast, range 60 width 10 rect
    Explosion = 34203, // Helper->self, 10.0s cast, range 60 width 30 rect

    //Route 3
    TenguYobi = 34209, // Boss->self, 3.0s cast, single-target
    YamaKagura = 34210, // IllComeTengu->self, 9.0s cast, range 40 width 5 rect
    GhastlyGraspVisual = 34211, // Boss->self, 11.0s cast, single-target
    GhastlyGrasp = 34212, // Helper->location, 11.0s cast, range 5 circle

    //Route 4
    Spiritspark = 34213, // Boss->self, 3.0s cast, single-target
    Spiritflame = 34214, // Helper->location, 4.0s cast, range 6 circle

    //Unfinished below
    SpearmansOrders = 34197, // Boss->self, 5.5s cast, single-target
    SpearmansOrdersAOE = 34198, // Helper->self, 5.5s cast, range 40 width 40 rect
    SpearpointPush1 = 34199, // Helper->self, no cast, range 3 width 4 rect
    SpearpointPush2 = 34514, // Helper->self, no cast, range 2 width 4 rect
    UnknownWeaponskill2 = 34588, // Helper->self, no cast, single-target
}

public enum SID : uint
{
    Giri = 2970, // none->Boss, extra=0x248/0x24A/0x249/0x24B
    DangerMarch = 2056, // none->AshigaruKyuhei/AshigaruSohei1/AshigaruSohei2/Boss/Spiritflame, extra=0x1E8/0x26E/0x26B
}
