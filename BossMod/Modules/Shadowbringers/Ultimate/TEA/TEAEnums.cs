namespace BossMod.Shadowbringers.Ultimate.TEA
{
    public enum OID : uint
    {
        BossP1 = 0x2C47, // x1 living liquid
        LiquidHand = 0x2C48, // spawn during fight
        LiquidRage = 0x2C49, // spawn during fight
        JagdDoll = 0x2C4A, // spawn during fight
        Embolus = 0x2C4B, // spawn during fight

        BruteJustice = 0x2C4C, // x1
        CruiseChaser = 0x2C4E, // x1
        SteamChakram = 0x2C4D, // spawn during fight

        AlexanderPrime = 0x2C53, // x1
        PerfectAlexander = 0x2C55, // x1
        CruiseChaser2 = 0x2C9D, // x1
        AlexanderPrime2 = 0x2C9E, // x1, and more spawn during fight ?
        PerfectAlexander2 = 0x2C9F, // x1
        Alexander = 0x18D6, // x1, and more spawn during fight ?

        Helper = 0x233C, // x17, and more spawn during fight ?

        //_Gen_Actor_1EA1A1 = 0x1EA1A1, // x8, EventObj type
        //_Gen_Actor_1E8536 = 0x1E8536, // x1, EventObj type
        //_Gen_Actor_1E9998 = 0x1E9998, // EventObj type, spawn during fight together with LiquidRage
    };

    public enum AID : uint
    {
        AutoAttackP1Boss = 18808, // BossP1->target, no cast
        AutoAttackP1Hand = 18809, // LiquidHand->target, no cast
        AutoAttackP1Doll = 19278, // JagdDoll->target, no cast
        FluidSwing = 18864, // BossP1->self, no cast, range 11.5 90-degree cone aoe tankbuster
        FluidStrike = 18871, // LiquidHand->self, no cast, range 11.6 90-degree cone aoe tankbuster
        Cascade = 18470, // BossP1->self, 4.0s cast, raidwide
        HandOfPrayer = 18475, // LiquidHand->self, no cast, raidwide (damage depends on distance to boss)
        HandOfParting = 18476, // LiquidHand->self, no cast, raidwide (damage depends on distance to boss)
        HandOfPain = 18477, // LiquidHand->self, 3.0s cast, raidwide (huge damage if hp diff is large)
        ProteanWaveTornadoVis = 18869, // Helper->self, 3.0s cast, range 40 30-degree cone (visible/avoidable, baited to closest)
        ProteanWaveTornadoInvis = 18870, // Helper->self, no cast, range 40 30-degree cone (invisible/unavoidable, baited to closest)
        Exhaust = 18462, // JagdDoll->self, no cast, range 8.8 aoe
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
        LiquidGaol = 18472, // LiquidRage->self, no cast, ??? (related to throttle debuffs)
        Enrage = 18867, // BossP1->self, 4.0s cast

        HawkBlasterIntermission = 18480, // Helper->location, no cast, range 10 aoe
        AlphaSword = 18484, // CruiseChaser->self, no cast, range 25 ?-degree cone, knockback 5
        SuperBlasstyCharge = 19279, // CruiseChaser->self, no cast, range 50 width 10 rect, knockback 20
        JKick = 18516, // BruteJustice->self, no cast, raidwide

        AutoAttackCruiseChaser = 18810, // CruiseChaser->player, no cast
        AutoAttackBruteJustice = 18811, // BruteJustice->player, no cast
        Whirlwind = 18882, // CruiseChaser->self, 4.0s cast, raidwide
        JudgmentNisi = 18494, // BruteJustice->self, 4.0s cast, visual for nisi application
        LinkUp = 18495, // BruteJustice->self, 3.0s cast, visual for compressed water/lightning application
        PunishingWave = 18498, // Helper->self, no cast, raidwide on compressed water death
        PunishingThunder = 18499, // Helper->self, no cast, raidwide on compressed lightning death
        EyeOfTheChakram = 18517, // 2C4D->self, 6.0s cast, range 70 width 6 rect
        OpticalSight = 18479, // CruiseChaser->self, 2.0s cast, visual
        HawkBlasterP2 = 18481, // Helper->location, 5.0s cast, range 10 aoe
        Photon = 18486, // CruiseChaser->self, 3.0s cast, visual
        PhotonAOE = 18487, // Helper->player, no cast, set hp to 1 for each player
        SpinCrusher = 19058, // CruiseChaser->self, 3.0s cast, range 5 ?-degree cone
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
