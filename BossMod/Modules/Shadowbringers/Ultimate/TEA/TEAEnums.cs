namespace BossMod.Shadowbringers.Ultimate.TEA
{
    public enum OID : uint
    {
        BossP1 = 0x2C47, // R3.500, x1 living liquid
        LiquidHand = 0x2C48, // R1.600, spawn during fight
        LiquidRage = 0x2C49, // R4.000, spawn during fight
        JagdDoll = 0x2C4A, // R0.800, spawn during fight
        Embolus = 0x2C4B, // R1.000, spawn during fight

        BruteJustice = 0x2C4C, // R5.400, x1
        CruiseChaser = 0x2C4E, // R4.998, x1
        SteamChakram = 0x2C4D, // R3.000, spawn during fight

        AlexanderPrime = 0x2C53, // R7.200, x1
        PerfectAlexander = 0x2C55, // R10.800, x1
        CruiseChaser2 = 0x2C9D, // R0.850, x1
        AlexanderPrime2 = 0x2C9E, // R7.200, x1
        PerfectAlexander2 = 0x2C9F, // R10.800, x1
        Alexander = 0x18D6, // R0.500, x1

        Helper = 0x233C, // R0.500, x17

        //_Gen_GelidGaol = 0x2C81, // R2.880, spawn during fight
        //_Gen_PlasmaShield = 0x2C4F, // R2.550, Unknown type, spawn during fight
        //_Gen_Actor_1EA1A1 = 0x1EA1A1, // R0.500-2.000, x8, EventObj type
        //_Gen_Actor_1E8536 = 0x1E8536, // R0.500-2.000, x1, EventObj type
        //_Gen_Actor_1E9998 = 0x1E9998, // R0.500, EventObj type, spawn during fight together with LiquidRage
        //_Gen_Actor1e958c = 0x1E958C, // R0.500, EventObj type, spawn during fight
        //_Gen_Actor1e958d = 0x1E958D, // R0.500, EventObj type, spawn during fight
        //_Gen_Actor1e958e = 0x1E958E, // R0.500, EventObj type, spawn during fight
    };

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
        Drainage = 18471, // LiquidRage->players, no cast, range 6 aoe tankbuster on tethered target
        LiquidGaol = 18472, // LiquidRage->self, no cast, single-target (related to throttle debuffs)
        Enrage = 18867, // BossP1->self, 4.0s cast

        HawkBlasterIntermission = 18480, // Helper->location, no cast, range 10 aoe
        AlphaSword = 18484, // CruiseChaser->self, no cast, range 25+R 90-degree cone, knockback 5
        SuperBlasstyCharge = 19279, // CruiseChaser->self, no cast, range 50+R width 10 rect, knockback 20
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
        //_Weaponskill_CrashingThunder = 18497, // Helper->self, no cast, range 8 circle
        //_Weaponskill_CrashingWave = 18496, // Helper->self, no cast, range 8 circle
        //_Weaponskill_MissileCommand = 18509, // BruteJustice->self, 3.0s cast, single-target
        //_Weaponskill_EarthMissile = 18510, // Helper->self, 3.0s cast, range 5 circle
        //_Weaponskill_HiddenMinefield = 18513, // Helper->self, 3.0s cast, range 5 circle
        //_Weaponskill_Enumeration = 18512, // Helper->self, no cast, ???
        //_Weaponskill_EarthMissile = 18511, // Helper->players, no cast, range 6 circle
        //_Weaponskill_HiddenMine = 18514, // Helper->self, no cast, range 8 circle
        //_Weaponskill_Drainage = 18500, // LiquidRage->players, no cast, range 6 circle
        //_Weaponskill_HiddenMineShrapnel = 18515, // Helper->self, no cast, range 80+R circle
        //_Weaponskill_Verdict = 18491, // BruteJustice->self, 4.0s cast, single-target
        //_Ability_LimitCut = 18483, // CruiseChaser->self, 2.0s cast, single-target
        //_Weaponskill_Flarethrower = 18501, // BruteJustice->self, 3.9s cast, single-target
        //_Weaponskill_Flarethrower = 18502, // BruteJustice->self, no cast, range 100 ?-degree cone
    };

    public enum SID : uint
    {
        None = 0,
        Throttle = 700, // none->player, extra=0x0
        FinalDecreeNisiAlpha = 2222, // none->player, extra=0x0
        FinalDecreeNisiBeta = 2223, // none->player, extra=0x0
        FinalDecreeNisiGamma = 2137, // none->player, extra=0x0
        FinalDecreeNisiDelta = 2138, // none->player, extra=0x0
        CompressedWater = 2142, // none->player, extra=0x0
        CompressedLightning = 2143, // none->player, extra=0x0
    }

    public enum TetherID : uint
    {
        None = 0,
        Drainage = 3, // LiquidRage->player
    }

    public enum IconID : uint
    {
        None = 0,
        CompressedWater = 68, // player
        CompressedLightning = 69, // player
    }
}
