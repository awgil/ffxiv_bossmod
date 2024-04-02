namespace BossMod.Endwalker.Savage.P12S1Athena;

public enum OID : uint
{
    Boss = 0x3F2B, // R10.000, x1
    Helper = 0x233C, // R0.500, x29
    Anthropos = 0x3F2C, // R2.000, x10
    Wing = 0x3F65, // R1.000, spawn during fight (also: destroyable platforms)
    LogouIdea = 0x3F2D, // R1.000, spawn during fight
    ThymouIdea = 0x3F2E, // R1.000, spawn during fight
    EpithymiasIdea = 0x3F2F, // R1.000, spawn during fight
    SuperchainOrigin = 0x3F30, // R0.500, spawn during fight
    SuperchainCircle = 0x3F31, // R0.600, spawn during fight
    SuperchainDonut = 0x3F32, // R0.600, spawn during fight
    SuperchainSpread = 0x3F33, // R0.600, spawn during fight
    SuperchainPairs = 0x3F34, // R0.600, spawn during fight
    PalladionVoidzone = 0x1E8FEA, // R0.500, EventObj type, spawn during fight
};

public enum AID : uint
{
    AutoAttack = 34423, // Boss->player, no cast, single-target
    TeleportAdd = 33556, // ThymouIdea/EpithymiasIdea/LogouIdea->location, no cast, single-target
    TeleportBoss = 33557, // Boss->location, no cast, single-target
    OnTheSoul = 33540, // Boss->self, 5.0s cast, range 60 circle, raidwide
    Glaukopis = 33532, // Boss->self/player, 5.0s cast, range 60 width 5 rect tankbuster
    GlaukopisSecond = 33533, // Boss->self, no cast, range 60 width 5 rect tankbuster (second hit)

    Paradeigma = 33517, // Boss->self, 3.0s cast, single-target, visual (spawn ideas)
    WhiteFlame = 33519, // Anthropos->self, no cast, single-target, visual (two baits)
    WhiteFlameAOE = 33520, // Helper->self, no cast, range 100 width 4 rect on two closest
    TrinityOfSoulsDirectTR = 33505, // Boss->self, 10.0s cast, range 60 180-degree cone (direct order, first top hit, right hand)
    TrinityOfSoulsDirectTL = 33506, // Boss->self, 10.0s cast, range 60 180-degree cone (direct order, first top hit, left hand)
    TrinityOfSoulsDirectMR = 33507, // Boss->self, no cast, range 60 180-degree cone (direct order, second middle hit, right hand)
    TrinityOfSoulsDirectML = 33508, // Boss->self, no cast, range 60 180-degree cone (direct order, second middle hit, left hand)
    TrinityOfSoulsDirectBR = 33509, // Boss->self, no cast, range 60 180-degree cone (direct order, last bottom hit, right hand)
    TrinityOfSoulsDirectBL = 33510, // Boss->self, no cast, range 60 180-degree cone (direct order, last bottom hit, left hand)
    TrinityOfSoulsInvertBR = 33511, // Boss->self, 10.0s cast, range 60 180-degree cone (inverse order, first bottom hit, right hand)
    TrinityOfSoulsInvertBL = 33512, // Boss->self, 10.0s cast, range 60 180-degree cone (inverse order, first bottom hit, left hand)
    TrinityOfSoulsInvertMR = 33513, // Boss->self, no cast, range 60 180-degree cone (inverse order, second middle hit, right hand)
    TrinityOfSoulsInvertML = 33514, // Boss->self, no cast, range 60 180-degree cone (inverse order, second middle hit, left hand)
    TrinityOfSoulsInvertTR = 33515, // Boss->self, no cast, range 60 180-degree cone (inverse order, last top hit, right hand)
    TrinityOfSoulsInvertTL = 33516, // Boss->self, no cast, range 60 180-degree cone (inverse order, last top hit, left hand)

    EngravementOfSouls = 33541, // Boss->self, 4.0s cast, single-target, visual (debuffs)
    PolarizedGlow = 33616, // Boss->self, no cast, single-target, visual (??? together with aoe resolve)
    SearingRadiance = 33521, // Anthropos->self/players, 9.0s cast, range 60 width 6 rect (light ray, gives umbral = light tilt)
    Shadowsear = 33522, // Anthropos->self/players, 9.0s cast, range 60 width 6 rect (dark ray, gives astral = dark tilt)
    UmbralGlow = 33548, // Helper->player, no cast, range 3 circle (aoe that creates light tower and gives umbral = light tilt; can be soaked by opposite color)
    AstralGlow = 33549, // Helper->player, no cast, range 3 circle (aoe that creates dark tower and gives astral = dark tilt; can be soaked by opposite color)
    UmbralAdvance = 33550, // Anthropos->location, 2.0s cast, range 3 circle (light tower, should be soaked by astral = dark tilt)
    AstralAdvance = 33551, // Anthropos->location, 2.0s cast, range 3 circle (dark tower, should be soaked by umbral = ligth tilt)
    UmbralUnmitigatedExplosion = 33552, // Helper->location, no cast, range 60 circle (unsoaked tower)
    AstralUnmitigatedExplosion = 33553, // Helper->location, no cast, range 60 circle (unsoaked tower)
    Electrify = 33555, // Helper->location, no cast, range 60 circle (wipe if tower debuff dies)
    RayOfLight = 33518, // Anthropos->self, 5.0s cast, range 60 width 10 rect

    SuperchainTheory1 = 33498, // Boss->self, 5.0s cast, single-target, visual (mechanic start)
    SuperchainBurst = 33499, // Helper->self, 1.0s cast, range 7 circle
    SuperchainCoil = 33500, // Helper->self, 1.0s cast, range 6-70 donut
    SuperchainRadiation = 33501, // Helper->self, no cast, range 100 30?-degree cone (spreads)
    SuperchainEmission = 33502, // Helper->self, no cast, range 100 40?-degree cone (pairs)
    PolarizedRay = 33543, // Boss->self, no cast, single-target, visual (rays)
    UmbralImpact = 33544, // Helper->self, no cast, range 100 width 6 rect (light ray)
    AstralImpact = 33545, // Helper->self, no cast, range 100 width 6 rect (dark ray)
    TheosHoly = 33542, // Helper->players, no cast, range 6 circle (spread on heavensflame targets)

    Apodialogos = 33534, // Boss->self, 5.0s cast, single-target, visual (tankbuster farthest + stack closest)
    Peridialogos = 33535, // Boss->self, 5.0s cast, single-target, visual (tankbuster closest + stack farthest)
    ApodialogosAOE = 33536, // Boss->players, no cast, range 6 circle tankbuster
    PeridialogosAOE = 33537, // Boss->player, no cast, range 6 circle tankbuster
    Dialogos = 33538, // Helper->players, no cast, range 6 circle stack

    Shock = 33554, // Helper->location, 4.0s cast, range 3 circle tower
    UnnaturalEnchainment = 33503, // Boss->self, 7.0s cast, single-target, visual (destroy platforms)
    Sample = 33504, // Helper->self, no cast, range 10 width 20 rect (destroy platforms)
    TheosCross = 33546, // Helper->self, 3.0s cast, range 40 width 6 cross (+)
    TheosSaltire = 33547, // Helper->self, 3.0s cast, range 40 width 6 cross (x)

    UltimaBlade = 33523, // Boss->self, no cast, single-target, visual (limit cut)
    UltimaBladeAOE = 33524, // Helper->self, no cast, range 60 circle, raidwide
    Palladion = 33525, // Boss->self, 8.0s cast, single-target, visual (start jumps)
    PalladionAOE = 33526, // Boss->location, no cast, range 6 circle (2-man stack at impact position)
    Shockwave = 33527, // Helper->players, no cast, width 4 rect charge (to next number)
    PalladionVisualEnd = 33528, // Boss->self, no cast, single-target, visual (???)
    PalladionDestroyPlatforms = 33529, // Helper->self, no cast, range 20 width 40 rect
    TheosUltima = 33530, // Boss->self, 7.0s cast, range 60 circle, raidwide
    ClearCut = 33531, // Anthropos->self, no cast, range 4 ?-degree cone (dark add aoe)

    SuperchainTheory2A = 34554, // Boss->self, 5.0s cast, single-target, visual (mechanic start)
    SuperchainTheory2B = 34555, // Boss->self, 5.0s cast, single-target, visual (mechanic start)
    Parthenos = 33539, // Boss->self, 5.0s cast, range 120 width 16 rect, middle cleave

    TheosUltimaEnrage = 34134, // Boss->self, 7.0s cast, range 60 circle, enrage
    TransitionVisual = 33611, // Helper->self, 5.0s cast, range 10 width 40 rect, ??? (is interrupted 2s after start)
    TransitionStun = 33612, // Helper->location, no cast, range 60 circle (attract 60 + apply down for the count)
};

public enum SID : uint
{
    ActiveWing = 3572, // Boss->Wing, extra=0x13/0x14/0x15/0x16/0x17/0x18
    UmbralTilt = 3576, // Anthropos/Helper->player, extra=0x0 (light guy)
    AstralTilt = 3577, // Anthropos/Helper->player, extra=0x0 (dark guy)
    UmbralbrightSoul = 3579, // none->player, extra=0x0 (light tower)
    AstralbrightSoul = 3580, // none->player, extra=0x0 (dark tower)
    HeavensflameSoul = 3578, // none->player, extra=0x0 (spread)
    UmbralstrongSoul = 3581, // none->player, extra=0x0 (light ray)
    AstralstrongSoul = 3582, // none->player, extra=0x0 (dark ray)
    QuarteredSoul = 3583, // none->player, extra=0x0 (plus)
    XMarkedSoul = 3584, // none->player, extra=0x0 (x)
    //_Gen_ = 2056, // none->SuperchainOrigin, extra=0x247
    //_Gen_Concussion = 2944, // Boss->player, extra=0x0
    //_Gen_EnchainedSoul = 3585, // none->player, extra=0x0
};

public enum IconID : uint
{
    WingTLFirst = 421, // Wing
    WingTRFirst = 422, // Wing
    WingML = 423, // Wing
    WingMR = 424, // Wing
    WingBLFirst = 425, // Wing
    WingBRFirst = 426, // Wing
    WingTLLast = 431, // Wing
    WingTRLast = 432, // Wing
    WingBLLast = 433, // Wing
    WingBRLast = 434, // Wing

    Glaukopis = 471, // player

    Palladion1 = 336, // player
    Palladion2 = 337, // player
    Palladion3 = 338, // player
    Palladion4 = 339, // player
    Palladion5 = 437, // player
    Palladion6 = 438, // player
    Palladion7 = 439, // player
    Palladion8 = 440, // player
};

public enum TetherID : uint
{
    LightNear = 233, // Anthropos->player
    DarkNear = 234, // Anthropos->player
    LightFar = 250, // Anthropos->player
    DarkFar = 251, // Anthropos->player
    SuperchainCircle = 228, // SuperchainCircle->SuperchainOrigin
    SuperchainDonut = 229, // SuperchainDonut->SuperchainOrigin
    SuperchainSpread = 230, // SuperchainSpread->SuperchainOrigin
    SuperchainPairs = 231, // SuperchainPairs->SuperchainOrigin
    UnnaturalEnchainment = 232, // Wing->Boss (destroyed platform)
};
