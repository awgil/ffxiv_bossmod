namespace BossMod.Shadowbringers.Ultimate.TEA;

public enum OID : uint
{
    BossP1 = 0x2C47, // R3.500, x1 living liquid
    LiquidHand = 0x2C48, // R1.600, spawn during fight
    LiquidRage = 0x2C49, // R4.000, spawn during fight (tornado in P1 and P2)
    JagdDoll = 0x2C4A, // R0.800, spawn during fight
    Embolus = 0x2C4B, // R1.000, spawn during fight
    VoidzoneLiquidRage = 0x1E9998, // R0.500, EventObj type, spawn during fight together with LiquidRage

    BruteJustice = 0x2C4C, // R5.400, x1
    CruiseChaser = 0x2C4E, // R4.998, x1
    SteamChakram = 0x2C4D, // R3.000, spawn during fight
    PlasmaShield = 0x2C4F, // R2.550, Unknown type, spawn during fight
    GelidGaol = 0x2C81, // R2.880, spawn during fight (frozen tornado)
    VoidzoneEarthMissileBaited = 0x1E958C, // R0.500, EventObj type, spawn during fight
    VoidzoneEarthMissileIceSmall = 0x1E958D, // R0.500, EventObj type, spawn during fight
    VoidzoneEarthMissileIceLarge = 0x1E958E, // R0.500, EventObj type, spawn during fight

    AlexanderPrime = 0x2C53, // R7.200, x1
    Plasmasphere = 0x2C50, // R1.200-3.000, spawn during fight
    Shanoa = 0x2C51, // R2.000, spawn during fight
    TrueHeart = 0x2C52, // R1.000, spawn during fight
    JudgmentCrystal = 0x2C54, // R1.190, spawn during fight
    VoidzoneJudgmentCrystal = 0x1E9E3C, // R0.500, EventObj type, spawn during fight
    VoidzonePlasmasphere = 0x1EA1C9, // R0.500, EventObj type, spawn during fight
    Wormhole1 = 0x1EA1DF, // R0.500, EventObj type, spawn during fight
    Wormhole2 = 0x1EA1E0, // R0.500, EventObj type, spawn during fight
    Wormhole3 = 0x1EA1E1, // R0.500, EventObj type, spawn during fight

    PerfectAlexander = 0x2C55, // R10.800, x1, and more spawn during fight
    FateProjection = 0x2C56, // R0.500, spawn during fight

    Helper = 0x233C, // R0.500, x17
    CruiseChaserCutscene = 0x2C9D, // R0.850, x1
    AlexanderPrimeCutscene = 0x2C9E, // R7.200, x1
    PerfectAlexanderCutscene = 0x2C9F, // R10.800, x1
    Alexander = 0x18D6, // R0.500, x1

    //_Gen_Actor1EA1A1 = 0x1EA1A1, // R0.500-2.000, x8, EventObj type
    //_Gen_Actor1E8536 = 0x1E8536, // R0.500-2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttackP1Boss = 18808, // BossP1->player, no cast, single-target
    AutoAttackP1Hand = 18809, // LiquidHand->player, no cast, single-target
    AutoAttackP1Doll = 19278, // JagdDoll->player, no cast, single-target
    FluidSwing = 18864, // BossP1->self, no cast, range 8+R 90-degree cone aoe tankbuster
    FluidStrike = 18871, // LiquidHand->self, no cast, range 10+R 90-degree cone aoe tankbuster
    Cascade = 18470, // BossP1->self, 4.0s cast, raidwide
    HandOfPrayer = 18475, // LiquidHand->self, no cast, raidwide (damage depends on distance to boss)
    HandOfParting = 18476, // LiquidHand->self, no cast, raidwide (damage depends on distance to boss)
    HandOfPain = 18477, // LiquidHand->self, 3.0s cast, raidwide (huge damage if hp diff is large)
    ProteanWaveTornadoVis = 18869, // Helper->self, 3.0s cast, range 40 30-degree cone (visible/avoidable, baited to closest)
    ProteanWaveTornadoInvis = 18870, // Helper->self, no cast, range 40 30-degree cone (invisible/unavoidable, baited to closest)
    Exhaust = 18462, // JagdDoll->self, no cast, range 8+R circle aoe
    ReducibleComplexity = 18464, // JagdDoll->self, no cast, raidwide on feeding (huge damage if >25%)
    ReducibleComplexityFail = 18465, // JagdDoll->self, no cast, insta kill
    Pressurize = 18473, // LiquidRage->self, no cast, spawns embolus
    Outburst = 18474, // Embolus->self, no cast, raidwide if touched
    ProteanWaveLiquidVisBoss = 18466, // BossP1->self, 3.0s cast, range 40 30-degree cone
    ProteanWaveLiquidVisHelper = 18468, // Helper->self, 3.0s cast, range 40 30-degree cone
    ProteanWaveLiquidInvisBoss = 18467, // BossP1->self, no cast, range 40 30-degree cone (not baited - "shadow" wave)
    ProteanWaveLiquidInvisHelper = 18469, // Helper->self, no cast, range 40 30-degree cone (baited to 4 closest)
    Sluice = 18865, // Helper->location, 3.0s cast, range 5 puddle (baited to 4 farthest)
    Splash = 18866, // BossP1->self, no cast, raidwide - 6 hits in succession
    DrainageP1 = 18471, // LiquidRage->players, no cast, range 6 aoe tankbuster on tethered target
    LiquidGaol = 18472, // LiquidRage->self, no cast, single-target (related to throttle debuffs)
    Enrage = 18867, // BossP1->self, 4.0s cast

    HawkBlasterIntermission = 18480, // Helper->location, no cast, range 10 aoe
    AlphaSwordP2 = 18484, // CruiseChaser->self, no cast, range 25+R 90-degree cone, knockback 5
    SuperBlasstyChargeP2 = 19279, // CruiseChaser->self, no cast, range 50+R width 10 rect, knockback 20
    JKick = 18516, // BruteJustice->self, no cast, raidwide

    AutoAttackCC = 18810, // CruiseChaser->player, no cast, single-target
    AutoAttackBJ = 18811, // BruteJustice->player, no cast, single-target
    Whirlwind = 18882, // CruiseChaser->self, 4.0s cast, raidwide
    JudgmentNisi = 18494, // BruteJustice->self, 4.0s cast, single-target, visual for nisi application
    LinkUp = 18495, // BruteJustice->self, 3.0s cast, single-target, visual for compressed water/lightning application
    PunishingWave = 18498, // Helper->self, no cast, raidwide on compressed water death
    PunishingThunder = 18499, // Helper->self, no cast, raidwide on compressed lightning death
    EyeOfTheChakram = 18517, // SteamChakram->self, 6.0s cast, range 70+R width 6 rect
    OpticalSight = 18479, // CruiseChaser->self, 2.0s cast, single-target, visual
    HawkBlasterP2 = 18481, // Helper->location, 5.0s cast, range 10 circle aoe
    Photon = 18486, // CruiseChaser->self, 3.0s cast, single-target, visual
    PhotonAOE = 18487, // Helper->player, no cast, single-target, set hp to 1 for each player
    SpinCrusher = 19058, // CruiseChaser->self, 3.0s cast, range 5+R 90-degree cone
    CrashingWave = 18496, // Helper->self, no cast, range 8 circle 3+-person stack (water)
    CrashingThunder = 18497, // Helper->self, no cast, range 8 circle 2-person stack (lighting)
    DrainageP2 = 18500, // LiquidRage->players, no cast, range 6 circle knockback 10 on players standing too near to tornado
    MissileCommand = 18509, // BruteJustice->self, 3.0s cast, single-target, visual (missiles + mines + enumeration mechanic start)
    EarthMissileBaited = 18510, // Helper->self, 3.0s cast, range 5 circle aoe (baited on two targets farthest from BJ)
    EarthMissileIce = 18511, // Helper->players, no cast, range 6 circle
    Enumeration = 18512, // Helper->self, no cast, range ?, 3-player stack (does nothing on success, kills everyone in range on failure)
    HiddenMinefield = 18513, // Helper->self, 3.0s cast, range 5 circle baited aoe
    HiddenMine = 18514, // Helper->self, no cast, range 8 circle mine soak
    HiddenMineShrapnel = 18515, // Helper->self, no cast, range 80+R circle (explosion when no one soaks the mine)
    Verdict = 18491, // BruteJustice->self, 4.0s cast, single-target
    LimitCutP2 = 18483, // CruiseChaser->self, 2.0s cast, single-target, invulnerabitily
    FlarethrowerP2 = 18501, // BruteJustice->self, 3.9s cast, single-target, visual
    FlarethrowerP2AOE = 18502, // BruteJustice->self, no cast, range 100 ?-degree cone aoe
    PropellerWind = 18482, // CruiseChaser->self, 6.0s cast, range 50 circle LOSable aoe
    Gavel = 18492, // BruteJustice->self, 5.0s cast, ??? raidwide, nisi resolve
    DoubleRocketPunch = 18503, // BruteJustice->player, 4.0s cast, range 3 circle shared tankbuster
    SuperJump = 18505, // BruteJustice->self, 3.9s cast, single-target, visual
    SuperJumpAOE = 18506, // BruteJustice->location, no cast, range 10 circle aoe baited on farthest
    ApocalypticRay = 18507, // BruteJustice->self, no cast, single-target, visual
    ApocalypticRayAOE = 18508, // Helper->self, no cast, range 25+R ?-degree cone aoe
    FinalSentence = 18518, // BruteJustice->self, 8.0s cast, single-target, visual (enrage)
    EternalDarknessP2 = 18490, // CruiseChaser->location, 8.0s cast, range 100 circle, ??? (enrage)
    //SevereContamination = 18891, // LiquidRage->self, no cast, range 80 circle

    AutoAttackP3 = 18812, // AlexanderPrime->player, no cast, single-target
    TemporalStasis = 18522, // AlexanderPrime->self, 8.0s cast, single-target, visual (debuffs & baits)
    PlaintOfSeverity = 18529, // Helper->player, no cast, single-target, small damage + vulns on aggravated assault targets
    PlaintOfObloquy = 18528, // Helper->self, no cast, range 30 circle, p4 defamation
    PlaintOfSolidarity = 18530, // Helper->players, no cast, range 4 circle 3-person stack
    PlaintOfSurety = 18531, // Helper->self, no cast, ???, 1 damage or kill on close/far tethers
    AlphaSwordP3 = 18539, // CruiseChaser->self, no cast, range 25+R ?-degree cone on 3 closest
    FlarethrowerP3 = 18540, // BruteJustice->self, no cast, range 100 ?-degree cone on 2 closest
    ChasteningHeat = 19072, // AlexanderPrime->player, 5.0s cast, range 5 circle tankbuster inflicting vuln
    DivineSpear = 19074, // AlexanderPrime->self, no cast, range 17+R ?-degree cone tankbuster
    InceptionFormation = 18543, // AlexanderPrime->self, 4.0s cast, single-target, visual (mechanic start)
    InceptionVisual = 18545, // CruiseChaser->self, no cast, single-target, visual (???)
    JudgmentCrystal = 18523, // AlexanderPrime->self, 3.0s cast, single-target, visual
    JudgmentCrystalAOE = 18524, // Helper->location, no cast, range 5 circle baited aoe
    UndyingAffection = 19020, // Shanoa->self, no cast, single-target, visual (spawn true heart)
    Aetheroplasm = 18546, // Plasmasphere->self, no cast, range 6 circle (orb explosion)
    ElectromagneticBurst = 18548, // Helper->self, no cast, range 50 circle, raidwide if aetheroplasm explosions intersect
    Tetrashatter = 19080, // JudgmentCrystal->self, no cast, range 80+R circle (light raidwide, if mechanic succeeded)
    TetrashatterFail = 18525, // JudgmentCrystal->self, 5.5s cast, range 80+R circle
    Inception = 18526, // AlexanderPrime->self, 5.0s cast, range 100 width 16 cross, ???
    SacramentInception = 18527, // AlexanderPrime->self, no cast, range 100 width 16 cross aoe
    //_Weaponskill_ = 19023, // Helper->self, no cast, single-target, ??? visual?
    TrueHeartSuccess = 19024, // TrueHeart->self, no cast, range 80 circle, applies Enigma Codex to whole raid (on success?)
    WormholeFormation = 18542, // AlexanderPrime->self, 4.0s cast, single-target, visual (mechanic start)
    LimitCutP3 = 19215, // CruiseChaser->self, 2.0s cast, single-target, visual
    VoidOfRepentance = 18534, // AlexanderPrime->self, 3.0s cast, single-target, visual
    SuperBlasstyChargeP3 = 18883, // CruiseChaser->self, no cast, range 50+R width 10 rect
    SacramentWormhole = 18519, // AlexanderPrime->self, 6.0s cast, range 100 width 16 cross
    Repentance3 = 18535, // Helper->self, no cast, ???, third wormhole soak
    Repentance2 = 18536, // Helper->self, no cast, ???, second wormhole soak
    Repentance1 = 18537, // Helper->self, no cast, ???, first wormhole soak
    Confession = 18538, // Helper->self, no cast, range 80+R circle, unsoaked wormhole
    IncineratingHeat = 19025, // AlexanderPrime->players, 5.0s cast, range 5 circle stack
    MegaHoly = 19075, // AlexanderPrime->self, 4.0s cast, range 80+R circle
    SummonAlexander = 19029, // AlexanderPrime->self, 3.0s cast, single-target, visual
    JStorm = 18550, // BruteJustice->self, no cast, range 80+R circle
    JWave = 18551, // Helper->self, no cast, range 80 circle
    EternalDarknessP3 = 18549, // CruiseChaser->self, 25.0s cast, enrage
    DivineJudgment = 18552, // AlexanderPrime->self, 75.0s cast, enrage?
    DivineJudgmentSuccess = 18553, // AlexanderPrime->self, no cast, single-target, visual (if enrage is interrupted by killing)
    P4TransitionVisual = 19034, // Helper->location, 5.0s cast, range 80 circle, visual?
    DivineJudgmentRaidwide = 18554, // Alexander->self, no cast, raidwide requiring tank LB3

    AutoAttackP4 = 19083, // PerfectAlexander->player, no cast, single-target
    FinalWord = 18557, // PerfectAlexander->self, 4.0s cast, single-target, visual (mechanic start)
    OrdainedMotion = 18558, // PerfectAlexander->self, 4.0s cast, single-target, visual
    OrdainedMotionSuccess = 19039, // Helper->player, no cast, single-target, always misses
    OrdainedMotionFail = 18560, // Helper->player, no cast, single-target, kills target
    OrdainedStillness = 18559, // PerfectAlexander->self, 4.0s cast, single-target, visual
    OrdainedStillnessSuccess = 19040, // Helper->player, no cast, single-target, always misses
    OrdainedStillnessFail = 18561, // Helper->player, no cast, single-target, kills target
    OpticalSightSpread = 18570, // PerfectAlexander->self, 3.0s cast, single-target, visual (spreads)
    OpticalSightStack = 18571, // PerfectAlexander->self, 3.0s cast, single-target, visual (stacks)
    IndividualReprobation = 18572, // Helper->player, no cast, range 6 circle spread
    CollectiveReprobation = 18573, // Helper->players, no cast, range 6 circle stack
    Faithlessness = 19073, // Helper->self, no cast, range 100 circle, ??? (wipe if something failed)
    DivineRetribution = 18563, // Helper->player, no cast, single-target, kill if distance check failed
    FateProjectionPrepare = 18581, // PerfectAlexander->location, no cast, single-target, visual (???)
    FateProjectionAlpha = 18555, // PerfectAlexander->self, 5.0s cast, single-target, visual (spawn clones)
    FateCalibrationAlpha = 18556, // PerfectAlexander->self, 22.0s cast, single-target, visual (execute mechanics on clones)
    FateCalibrationAlphaOrdainedMotionStillness1 = 18584, // PerfectAlexander[clone]->self, 1.0s cast, single-target (common for both mechanics)
    FateCalibrationAlphaOrdainedMotion1 = 19213, // Helper->FateProjection, no cast, single-target, visual (motion fx)
    FateCalibrationAlphaOrdainedStillness1 = 19214, // Helper->FateProjection, no cast, single-target, visual (stillness fx)
    FateCalibrationAlphaDefamation = 18596, // Helper->self, no cast, single-target, visual (fx near defamation target)
    FateCalibrationAlphaAggravatedAssault = 18597, // Helper->FateProjection, no cast, single-target (kills clones by aggravated assault)
    FateCalibrationAlphaSharedSentence = 18598, // Helper->FateProjection, no cast, single-target (kills clone by shared sentence)
    FateCalibrationAlphaOrdainedMotionStillness2 = 18858, // PerfectAlexander[clone]->self, 1.0s cast, single-target (common for both mechanics)
    FateCalibrationAlphaOrdainedMotion2 = 18585, // Helper->FateProjection, no cast, single-target, kill (motion fx)
    // FateCalibrationAlphaOrdainedStillness2 = ???
    FateCalibrationAlphaSacrament = 18591, // PerfectAlexander[clone]->self, no cast, single-target, sacrament fx
    FateCalibrationAlphaResolveOrdainedMotion = 18859, // PerfectAlexander->self, no cast, single-target
    FateCalibrationAlphaResolveOrdainedStillness = 18860, // PerfectAlexander->self, no cast, single-target
    FateCalibrationAlphaResolveSacrament = 18569, // PerfectAlexander->self, no cast, range 100 width 16 cross
    OrdainedCapitalPunishment = 18578, // PerfectAlexander->self, 3.0s cast, single-target, visual (tankbuster sequence)
    OrdainedCapitalPunishmentAOE = 18579, // Helper->players, no cast, range 4 circle, shared 3-hit tankbuster
    OrdainedPunishment = 18577, // PerfectAlexander->player, 5.0s cast, range 5 circle aoe tankbuster
    FateProjectionBeta = 19219, // PerfectAlexander->self, 5.0s cast, single-target, visual (spawn clones)
    FateCalibrationBeta = 19220, // PerfectAlexander->self, 35.0s cast, single-target, visual (execute mechanics on clones)
    FateCalibrationBetaKillDarkTethered = 18587, // Helper->FateProjection, no cast, single-target (kills tethered dark clones)
    FateCalibrationBetaKillLightTethered = 18588, // Helper->FateProjection, no cast, single-target (kills tethered light clones)
    FateCalibrationBetaJump = 18589, // PerfectAlexander[clone]->location, no cast, single-target, visual (baited jumps)
    FateCalibrationBetaDonut = 18590, // PerfectAlexander[clone]->self, no cast, single-target
    FateCalibrationBetaOpticalSightSpread = 18592, // PerfectAlexander[clone]->self, no cast, single-target, visual (spread fx)
    FateCalibrationBetaOpticalSightStack = 18593, // PerfectAlexander[clone]->self, no cast, single-target, visual (stack fx)
    FateCalibrationBetaKillBeaconSpread = 18594, // Helper->FateProjection, no cast, single-target (kills beacon clones)
    FateCalibrationBetaKillBeaconStack = 18595, // Helper->FateProjection, no cast, single-target (kills beacon clones)
    //_Weaponskill_DivineRetribution = 18562, // Helper->player, no cast, single-target
    FateCalibrationBetaJJump = 18565, // PerfectAlexander[clone]->location, no cast, range 10 circle baited jump
    FateCalibrationBetaResolveOpticalSightSpread = 18861, // PerfectAlexander->self, no cast, single-target, visual (spread fx)
    FateCalibrationBetaResolveOpticalSightStack = 18862, // PerfectAlexander->self, no cast, single-target, visual (stack fx)
    FateCalibrationBetaRadiantSacrament = 18566, // PerfectAlexander->self, no cast, range 8-60 donut
    AlmightyJudgment = 18574, // PerfectAlexander->self, 3.0s cast, single-target, visual (exatrines mechanic start)
    AlmightyJudgmentVisual = 18575, // Helper->self, 1.5s cast, range 6 circle, visual
    AlmightyJudgmentAOE = 18576, // Helper->self, no cast, range 6 circle aoe
    IrresistibleGrace = 18580, // PerfectAlexander->players, 5.0s cast, range 6 circle stack
    TemporalInterference = 18582, // PerfectAlexander->self, 5.0s cast, single-target, visual (enrage sequence start)
    TemporalPrison = 18583, // PerfectAlexander->self, 42.0s cast, range 100 circle, enrage
}

public enum SID : uint
{
    None = 0,
    Throttle = 700, // none->player, extra=0x0
    FinalDecreeNisiAlpha = 2222, // none->player, extra=0x0
    FinalDecreeNisiBeta = 2223, // none->player, extra=0x0
    FinalDecreeNisiGamma = 2137, // none->player, extra=0x0
    FinalDecreeNisiDelta = 2138, // none->player, extra=0x0
    FinalJudgmentNisiAlpha = 2224, // none->player, extra=0x0
    FinalJudgmentNisiBeta = 2225, // none->player, extra=0x0
    FinalJudgmentNisiGamma = 2139, // none->player, extra=0x0
    FinalJudgmentNisiDelta = 2140, // none->player, extra=0x0
    CompressedWater = 2142, // none->player, extra=0x0
    CompressedLightning = 2143, // none->player, extra=0x0
    //FinalJudgment:PenaltyIII = 1035, // none->player, extra=0x0
    Invincibility = 775, // none->CruiseChaser, extra=0x0
    DirectionalInvincibility = 1125, // none->PlasmaShield, extra=0x0
    TemporalDisplacement = 1119, // none->player, extra=0x0
    AggravatedAssault = 1121, // none->player, extra=0x0
    SharedSentence = 1122, // none->player, extra=0x0
    HouseArrest = 1123, // none->player, extra=0x0
    RestrainingOrder = 1124, // none->player, extra=0x0
    FinalWordContactProhibition = 2152, // none->player, extra=0x0
    FinalWordContactRegulation = 2153, // none->player, extra=0x0
    FinalWordEscapeProhibition = 2154, // none->player, extra=0x0
    FinalWordEscapeDetection = 2155, // none->player, extra=0x0
    ContactProhibitionOrdained = 3055, // player->player, extra=0x0
    ContactRegulationOrdained = 3056, // none->player, extra=0x0
    EscapeProhibitionOrdained = 3057, // player->player, extra=0x0
    EscapeDetectionOrdained = 3058, // none->player, extra=0x0
    FateCalibrationBeta = 2195, // none->player, extra=0x84 (light) / 0x85 (dark)
    TemporalPrison = 2165, // none->player, extra=0x0

    PhysicalVulnerabilityUp = 695, // none->player
    //_Gen_EnigmaCodex = 2147, // none->TrueHeart, extra=0x171
    //_Gen_EnigmaCodex = 2146, // TrueHeart->player, extra=0x0
    //_Gen_DamageDown = 1016, // none->Plasmasphere, extra=0x1/0x2/0x3/0x4
    //_Gen_VulnerabilityDown = 406, // none->TrueHeart, extra=0x1/0x2/0x3/0x4
    //_Gen_DamageUp = 290, // none->AlexanderPrime/CruiseChaser, extra=0x0
    //_Gen_VulnerabilityDown = 350, // none->AlexanderPrime, extra=0x0
    //_Gen_Sludge = 287, // none->player, extra=0x0
}

public enum TetherID : uint
{
    None = 0,
    Drainage = 3, // LiquidRage->player
    HouseArrest = 28, // player->player (stay at range <= 5; 4.93 is ok, 5.34 is fatal)
    RestrainingOrder = 29, // player->player (stay at range >= 30 or so; 25.45 is fatal, 31.25 is ok)
    Plasmasphere = 12, // Plasmasphere->player
    FateProjection = 98, // player->FateProjection (note that it appears before target is created)
}

public enum IconID : uint
{
    None = 0,
    Icon1 = 79, // player
    Icon2 = 80, // player
    Icon3 = 81, // player
    Icon4 = 82, // player
    Icon5 = 83, // player
    Icon6 = 84, // player
    Icon7 = 85, // player
    Icon8 = 86, // player
    CompressedWater = 68, // player
    CompressedLightning = 69, // player
    Enumeration = 65, // player
    EarthMissileIce = 67, // player
    JudgmentCrystal = 96, // player
    IncineratingHeat = 93, // player
    OpticalSightSpread = 139, // player
    OpticalSightStack = 62, // player
}
