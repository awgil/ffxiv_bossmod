namespace BossMod.Endwalker.Quest.Endwalker;

public enum OID : uint
{
    Helpers = 0x233C, // R0.500, x28, 523 type
    AvatarVisual = 0x3366, // R0.920, x1
    ZenosP1 = 0x3364, // R2.001, x1
    ZenosP2 = 0x3365, // R2.001, x1
    Puddles = 0x1E950D, // R0.500, EventObj type, spawn during fight
};

public enum AID : uint
{
    AutoAttackBoss = 27767, // ZenosP1/ZenosP2->player, no cast, single-target
    TidalWave = 26917, // ZenosP1->self, 10.0s cast, single-target
    TidalWaveVisual = 26918, // Helpers->self, 10.0s cast, range 60 width 60 rect
    Megaflare = 26919, // Helpers->location, 7.5s cast, range 6 circle
    JudgementBolt = 26920, // ZenosP1->self, 6.0s cast, single-target
    JudgementBoltVisual = 26921, // Helpers->self, 6.0s cast, range 60 circle
    Hellfire = 26922, // ZenosP1->self, 6.0s cast, single-target
    HellfireVisual = 26923, // Helpers->self, 6.0s cast, range 60 circle
    AkhMorn = 26924, // ZenosP1/ZenosP2->self, 7.0s cast, single-target
    AkhMornVisual = 26925, // Helpers->player, no cast, range 4 circle
    Teleport = 26926, // ZenosP1/ZenosP2->location, no cast, single-target
    ComeHither = 26927, // ZenosP1->self, 4.0s cast, single-target
    StarBeyondStars = 26928, // ZenosP1->self, 4.0s cast, single-target
    StarBeyondStarsHelper = 26929, // Helpers->self, 5.0s cast, range 50 30-degree cone
    TheEdgeUnbound = 26962, // AvatarVisual->self, 4.0s cast, range 10 circle
    WyrmsTongue = 26930, // ZenosP1/ZenosP2->self, no cast, single-target
    WyrmsTongueHelper = 26931, // Helpers->self, 3.0s cast, range 40 60-degree cone
    NineNightsAvatar = 26963, // AvatarVisual->self, 9.0s cast, range 10 circle
    NineNightsHelpers = 26964, // Helpers->self, 9.0s cast, range 10 circle
    VeilAsunder = 26932, // ZenosP1/ZenosP2->self, no cast, single-target
    VeilAsunderHelper = 26933, // Helpers->location, 4.0s cast, range 6 circle
    Exaflare = 26934, // ZenosP1->self, 4.0s cast, single-target
    ExaflareFirstHit = 26935, // Helpers->self, 4.0s cast, range 6 circle
    ExaflareRest = 26936, // Helpers->self, no cast, range 6 circle
    MortalCoil = 26965, // AvatarVisual->self, 6.0s cast, single-target
    MortalCoilVisual = 26966, // Helpers->self, 6.0s cast, range ?-20 donut
    DiamondDust = 26937, // ZenosP1->self, 5.0s cast, single-target
    DiamondDustVisual = 26938, // Helpers->self, 5.0s cast, range 60 circle
    DeadGaze = 26967, // AvatarVisual->self, 7.0s cast, single-target
    DeadGazeVisual = 26968, // Helpers->self, 7.0s cast, range 60 circle
    GapClose = 28303, // ZenosP1/ZenosP2->player, no cast, single-target
    StarBeyondStarsAvatar = 26969, // AvatarVisual->self, 4.0s cast, single-target
    TidalWave2 = 26939, // ZenosP1->self, 3.0s cast, single-target
    TidalWaveVisual2 = 26940, // Helpers->self, 3.0s cast, range 60 width 60 rect
    SwiftAsShadowIntermission = 26941, // ZenosP2->player, no cast, single-target
    Thorns = 26942, // ZenosP2->player, no cast, single-target, intermission ability
    AetherialRay = 26944, // ZenosP2->self, 5.0s cast, single-target
    AetherialRayVisual = 26945, // Helpers->self, no cast, range 100 width 10 rect
    SilveredEdge = 26946, // ZenosP2->self, 3.0s cast, single-target
    SilveredEdgeVisual = 26947, // Helpers->self, no cast, range 40 width 6 rect
    SwiftAsShadow = 26948, // ZenosP2->location, 2.5s cast, width 2 rect charge
    CandlewickPointBlank = 26949, // ZenosP2->self, 5.0s cast, range 10 circle
    CandlewickDonut = 26950, // Helpers->self, 5.0s cast, range ?-30 donut
    Exconcentrativity = 26953, // ZenosP2->self, no cast, single-target
    ExconcentrativityHelper = 26954, // Helpers->self, no cast, range 60 circle
    Extinguishment = 26951, // ZenosP2->self, 4.0s cast, single-target
    ExtinguishmentVisual = 26952, // Helpers->self, 4.0s cast, range ?-30 donut
    UnmovingDvenadkatik = 26955, // ZenosP2->self, 5.0s cast, single-target
    UnmovingDvenadkatikVisual = 26956, // Helpers->self, 6.0s cast, range 50 30-degree cone
    TheEdgeUnbound2 = 26957, // ZenosP2->self, 4.0s cast, range 10 circle
};

public enum SID : uint
{
    FireResistanceUp = 520, // none->player, extra=0x0
    LightningResistanceDownII = 1260, // none->player, extra=0x0
    ThinIce = 1579, // Helpers->player, extra=0xFA
    Petrification = 610, // Helpers->player, extra=0x0
    DownForTheCount = 2961, // Helpers->player, extra=0xEC7
    _Gen_Unk01 = 2785, // none->ZenosP2, extra=0x0
    _Gen_Unk02 = 2892, // ZenosP2->player, extra=0x2110
    Unlimited = 2781, // none->player, extra=0x0
    _Gen_Unk03 = 2850, // none->player, extra=0x0
    SparkOfHope = 2786, // none->player, extra=0x5/0x4/0x3/0x2/0x1
    _Gen_Unk04 = 2881, // none->player, extra=0x0
};
