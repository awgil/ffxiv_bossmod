namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    public enum OID : uint
    {
        Boss = 0x3A2D, // x1
        Helper = 0x233C, // x22
        ForbiddenFruitBull = 0x3A2E, // x3
        ForbiddenFruitBird = 0x3A2F, // x4
        ForbiddenFruitMinotaur = 0x3A30, // x6
        ImmatureIo = 0x3A31, // x3, bull
        ImmatureStymphalide = 0x3A32, // x4, bird
        ImmatureMinotaur = 0x3A33, // x6, minotaur
        BridgeDestroyer = 0x3A34, // x3
        BullTetherSource = 0x3AD9, // x9
        Tower = 0x1EB793, // EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackVisual = 30947, // Boss->self, no cast, single-target, visual
        AutoAttackReal = 30949, // Helper->player, no cast, single-target, on 2 highest aggro targets
        SparkOfLife = 30777, // Boss->self, 5.0s cast, raidwide

        DispersedAero = 30773, // Boss->self, 7.0s cast, single-target, visual
        DispersedAeroAOE = 30775, // Helper->player, no cast, range 8 circle aoe tankbuster that hits 2 highest aggro targets
        CondensedAero = 30774, // Boss->self, 7.0s cast, single-target, visual
        CondensedAeroAOE = 30776, // Helper->player, no cast, range 6 circle shared tankbuster

        BladesOfAttis = 30766, // Boss->self, 3.0s cast, single-target, visual
        BladesOfAttisFirst = 30767, // Helper->self, 7.5s cast, range 7 circle exaflare
        BladesOfAttisRest = 30768, // Helper->self, no cast, range 7 circle exaflare
        ImmortalsObol = 30732, // Boss->self, 5.5s cast, single-target, visual
        ImmortalsObolAOE = 30733, // Helper->self, 7.5s cast, aoe with ~15 falloff
        ForbiddenFruit = 30736, // Boss->self, 4.0s cast, single-target, visual
        ForbiddenFruitInvis = 30737, // Boss->self, 3.0s cast, single-target, visual (followup, invisible cast bar)
        HemitheosHoly = 30778, // Boss->self, 3.0s cast, single-target, visual
        HemitheosHolyAOE = 30779, // Helper->players, 6.0s cast, range 6 circle stack
        BoughOfAttisBack = 30758, // Boss->self, 6.2s cast, single-target, visual
        BoughOfAttisBackAOE = 30759, // Helper->self, 7.0s cast, range 25 circle aoe on S platform
        BoughOfAttisFront = 30753, // Boss->self, 5.8s cast, single-target, visual
        BoughOfAttisFrontAOE = 30754, // Helper->self, 7.0s cast, range 19 circle aoe on NW and NE platforms
        BoughOfAttisSideW = 30755, // Boss->self, 4.0s cast, single-target, visual (invisible cast bar)
        BoughOfAttisSideE = 30756, // Boss->self, 4.0s cast, single-target, visual (invisible cast bar)
        BoughOfAttisSideAOE = 30757, // Helper->self, 5.0s cast, range 50 width 25 rect knockback 60
        InviolateBonds = 30746, // Boss->self, 4.0s cast, single-target, visual
        HemitheosAeroExpire = 30747, // Helper->location, no cast, range 7 circle
        HemitheosHolyExpire = 30748, // Helper->location, no cast, range 6 circle
        HemitheosHolyExpireFail = 30749, // Helper->self, no cast, range 60 circle ???
        RootsOfAttis = 30734, // Boss->self, 3.0s cast, single-target, visual
        RootsOfAttisDestroy = 30735, // BridgeDestroyer->self, no cast, single-target, visual
        Multicast = 31221, // Boss->self, 3.0s cast, single-target, visual
        HemitheosAeroKnockback1 = 31243, // Helper->self, 6.0s cast, raidwide knockback 16?
        HemitheosAeroKnockback2 = 30772, // Helper->self, 6.0s cast, raidwide knockback 16
        HemitheosHolySpread = 30770, // Helper->player, 5.0s cast, range 6 circle spread
        ShadowOfAttis = 30763, // Boss->self, no cast, single-target, visual (towers spawn?)
        Burst = 30764, // Helper->self, no cast, range 5 circle tower, soaked
        BigBurst = 30765, // Helper->self, no cast, raidwide (tower soak fail)
        InviolatePurgation = 30750, // Boss->self, 4.0s cast, single-target, visual
        LightOfLife = 30946, // Boss->self, 26.0s cast, raidwide
        HemitheosTornado = 30751, // Helper->self, 14.0s cast, range 25 circle aoe (lingering explosion from spreads)
        HemitheosGlareMine = 30752, // Helper->self, 14.0s cast, range ?-30 donut aoe (lingering explosion from stack)
        HemitheosGlare = 30760, // Boss->self, 5.0s cast, single-target, visual (chasing aoes)
        HemitheosGlareFirst = 30761, // Helper->self, 5.0s cast, range 6 circle
        HemitheosGlareRest = 30762, // Helper->self, no cast, range 6 circle
        FaminesHarvest = 31311, // Boss->self, 4.0s cast, single-target, visual
        DeathsHarvest = 31312, // Boss->self, 4.0s cast, single-target, visual
        WarsHarvest = 31313, // Boss->self, 4.0s cast, single-target, visual
        Enrage = 30783, // Boss->self, 10.0s cast, enrage

        StaticMoon = 30738, // ImmatureIo->self, 3.0s cast, range 10 circle aoe (untethered bull)
        StaticPath = 30739, // ImmatureIo->self, no cast, single-target, visual (tethered bull)
        StaticPathAOE = 30740, // Helper->self, no cast, range 60 width 8 rect aoe (tethered bull)
        StymphalianStrike = 30741, // ImmatureStymphalide->self, 3.0s cast, range 60 width 8 rect aoe (untethered bird)
        BronzeBellows = 30742, // ImmatureStymphalide->self, no cast, range 60 width 8 rect aoe (tethered bird)
        BullishSlash = 30743, // ImmatureMinotaur->self, no cast, range 60 45-degree cone (tethered minotaur)
        BullishSwipe = 30744, // ImmatureMinotaur->self, 2.5s cast, single-target, visual (untethered minotaur)
        BullishSwipeAOE = 30745, // Helper->self, 3.0s cast, range 60 90-degree cone aoe (baited from untethered minotaur)
    };

    public enum SID : uint
    {
        InviolateWinds1 = 3308, // none->player, extra=0x0
        InviolateWinds2 = 3397, // none->player, extra=0x0
        HolyBonds1 = 3309, // none->player, extra=0x0
        HolyBonds2 = 3398, // none->player, extra=0x0
        PurgatoryWinds1 = 3310, // none->player, extra=0x0
        PurgatoryWinds2 = 3391, // none->player, extra=0x0
        PurgatoryWinds3 = 3392, // none->player, extra=0x0
        PurgatoryWinds4 = 3393, // none->player, extra=0x0
        HolyPurgation1 = 3311, // none->player, extra=0x0
        HolyPurgation2 = 3394, // none->player, extra=0x0
        HolyPurgation3 = 3395, // none->player, extra=0x0
        HolyPurgation4 = 3396, // none->player, extra=0x0
    };

    public enum TetherID : uint
    {
        Bull = 6, // BullTetherSource->player
        MinotaurClose = 57, // ImmatureMinotaur->player
        MinotaurFar = 1, // ImmatureMinotaur->player
        Bird = 17, // ImmatureStymphalide->player
    };
}
