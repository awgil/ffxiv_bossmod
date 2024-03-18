namespace BossMod.Modules.Endwalker.SoloDuty.Endwalker;
public enum OID : uint
{
    Helpers = 0x233C, // R0.500, x28, 523 type
    AvatarVisual = 0x3366, // R0.920, x1
    Phase2Zenos = 0x3365, // R2.001, x1
    Phase1Zenos = 0x3364, // R2.001, x1
    Puddles = 0x1E950D, // R0.500, EventObj type, spawn during fight
};

public enum AID : uint
{
    AutoAttackBoss = 27767, // 3364/3365->player, no cast, single-target
    TidalWave = 26917, // 3364->self, 10.0s cast, single-target
    TidalWaveVisual = 26918, // 233C->self, 10.0s cast, range 60 width 60 rect
    Megaflare = 26919, // 233C->location, 7.5s cast, range 6 circle
    JudgementBolt = 26920, // 3364->self, 6.0s cast, single-target
    JudgementBoltVisual = 26921, // 233C->self, 6.0s cast, range 60 circle
    Hellfire = 26922, // 3364->self, 6.0s cast, single-target
    HellfireVisual = 26923, // 233C->self, 6.0s cast, range 60 circle
    AkhMorn = 26924, // 3364/3365->self, 7.0s cast, single-target, 6x hitting tb
    AkhMornVisual = 26925, // 233C->player, no cast, range 4 circle
    _Weaponskill_Unk01 = 26926, // 3364/3365->location, no cast, single-target
    ComeHither = 26927, // 3364->self, 4.0s cast, single-target, summons avatar
    StarBeyondStars = 26928, // 3364->self, 4.0s cast, single-target
    StarBeyondStarsHelper = 26929, // 233C->self, 5.0s cast, range 50 30-degree cone
    TheEdgeUnbound = 26962, // 3366->self, 4.0s cast, range 10 circle
    WyrmsTongue = 26930, // 3364/3365->self, no cast, single-target
    WyrmsTongueHelper = 26931, // 233C->self, 3.0s cast, range 40 60-degree cone
    NineNightsAvatar = 26963, // 3366->self, 9.0s cast, range 10 circle, 1x shiva circle
    NineNightsHelpers = 26964, // 233C->self, 9.0s cast, range 10 circle, 8x shiva circles
    VeilAsunder = 26932, // 3364/3365->self, no cast, single-target
    VeilAsunderHelper = 26933, // 233C->location, 4.0s cast, range 6 circle
    Exaflare = 26934, // 3364->self, 4.0s cast, single-target
    ExaflareFirstHit = 26935, // 233C->self, 4.0s cast, range 6 circle
    ExaflareRest = 26936, // 233C->self, no cast, range 6 circle
    MortalCoil = 26965, // 3366->self, 6.0s cast, single-target
    MortalCoilVisual = 26966, // 233C->self, 6.0s cast, range ?-20 donut
    DiamondDust = 26937, // Phase1Zenos->self, 5.0s cast, single-target
    DiamondDustVisual = 26938, // Helpers->self, 5.0s cast, range 60 circle
    DeadGaze = 26967, // AvatarVisual->self, 7.0s cast, single-target
    DeadGazeVisual = 26968, // Helpers->self, 7.0s cast, range 60 circle
    StarBeyondStarsAvatar = 26969, // AvatarVisual->self, 4.0s cast, single-target
    TidalWave2 = 26939, // 3364->self, 3.0s cast, single-target
    TidalWaveVisual2 = 26940, // 233C->self, 3.0s cast, range 60 width 60 rect
    SwiftAsShadowIntermission = 26941, // 3365->player, no cast, single-target, intermission cutscene attack
    Thorns = 26942, // 3365->player, no cast, single-target, intermission cutscene attack
    AetherialRay = 26944, // 3365->self, 5.0s cast, single-target
    AetherialRayVisual = 26945, // 233C->self, no cast, range 100 width 10 rect
    SilveredEdge = 26946, // 3365->self, 3.0s cast, single-target
    SilveredEdgeVisual = 26947, // 233C->self, no cast, range 40 width 6 rect
    _Weaponskill_Unk02 = 28303, // 3365->player, no cast, single-target
    SwiftAsShadow = 26948, // 3365->location, 2.5s cast, width 2 rect charge
    CandlewickPointBlank = 26949, // 3365->self, 5.0s cast, range 10 circle
    CandlewickDonut = 26950, // 233C->self, 5.0s cast, range ?-30 donut
    Exconcentrativity = 26953, // 3365->self, no cast, single-target
    ExconcentrativityHelper = 26954, // 233C->self, no cast, range 60 circle, raidwide. Can't write hint?
    Extinguishment = 26951, // 3365->self, 4.0s cast, single-target
    ExtinguishmentVisual = 26952, // 233C->self, 4.0s cast, range ?-30 donut
    UnmovingDvenadkatik = 26955, // 3365->self, 5.0s cast, single-target
    UnmovingDvenadkatikVisual = 26956, // 233C->self, 6.0s cast, range 50 30-degree cone
    TheEdgeUnbound2 = 26957, // 3365->self, 4.0s cast, range 10 circle
    Tryst = 26958, // 3365->self, no cast, single-target
    Tryst2 = 26959, // 233C->self, no cast, range 60 circle
    Thorns2 = 26943, // 3365->player, no cast, single-target, intermission cutscene attack
    _Weaponskill_UltimateEdge = 26960, // 3365->self, no cast, single-target
};

public enum SID : uint
{
    _Gen_FireResistanceUp = 520, // none->player, extra=0x0
    _Gen_LightningResistanceDownII = 1260, // none->player, extra=0x0
    _Gen_ThinIce = 1579, // Helpers->player, extra=0xFA
    _Gen_Petrification = 610, // Helpers->player, extra=0x0
    _Gen_DownForTheCount = 2961, // Helpers->player, extra=0xEC7
    _Gen_Unk01 = 2785, // none->Phase2Zenos, extra=0x0
    _Gen_Unk02 = 2892, // Phase2Zenos->player, extra=0x2110
    _Gen_Unlimited = 2781, // none->player, extra=0x0
    _Gen_Unk03 = 2850, // none->player, extra=0x0
    _Gen_SparkOfHope = 2786, // none->player, extra=0x5/0x4/0x3/0x2/0x1
    _Gen_Unk04 = 2881, // none->player, extra=0x0

};
