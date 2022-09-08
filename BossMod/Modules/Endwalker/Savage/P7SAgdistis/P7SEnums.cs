namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    public enum OID : uint
    {
        Boss = 0x3A2D, // x1
        Helper = 0x233C, // x22
        ImmatureIo = 0x3A31, // x3, bull
        ImmatureStymphalide = 0x3A32, // x4, bird
        ImmatureMinotaur = 0x3A33, // x6, minotaur
        BridgeDestroyer = 0x3A34, // x3
        //_Gen_ForbiddenFruit = 0x3A30, // x6
        //_Gen_Actor3ad9 = 0x3AD9, // x9
        //_Gen_ForbiddenFruit = 0x3A2F, // x4
        //_Gen_ForbiddenFruit = 0x3A2E, // x3
        //_Gen_Actor1ea1a1 = 0x1EA1A1, // x1, EventObj type
        //_Gen_Actor1eb793 = 0x1EB793, // EventObj type, spawn during fight
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
        ImmortalsObolAOE = 30733, // Helper->self, 7.5s cast, raidwide
        ForbiddenFruit = 30736, // Boss->self, 4.0s cast, single-target, visual
        ForbiddenFruitInvis = 30737, // Boss->self, 3.0s cast, single-target, visual (followup, invisible cast bar)
        HemitheosHoly = 30778, // Boss->self, 3.0s cast, single-target, visual
        HemitheosHolyAOE = 30779, // Helper->players, 6.0s cast, range 6 circle stack
        BoughOfAttisBack = 30758, // Boss->self, 6.2s cast, single-target, visual
        BoughOfAttisBackAOE = 30759, // Helper->self, 7.0s cast, range 25 circle aoe on S platform
        BoughOfAttisFront = 30753, // Boss->self, 5.8s cast, single-target, visual
        BoughOfAttisFrontAOE = 30754, // Helper->self, 7.0s cast, range 19 circle aoe on NW and NE platforms
        BoughOfAttisSide = 30755, // Boss->self, 4.0s cast, single-target, visual (invisible cast bar)
        BoughOfAttisSideAOE = 30757, // Helper->self, 5.0s cast, range 50 width 25 rect knockback 60
        InviolateBonds = 30746, // Boss->self, 4.0s cast, single-target, visual
        HemitheosAeroExpire = 30747, // Helper->location, no cast, range 7 circle
        HemitheosHolyExpire = 30748, // Helper->location, no cast, range 6 circle
        RootsOfAttis = 30734, // Boss->self, 3.0s cast, single-target, visual
        RootsOfAttisDestroy = 30735, // BridgeDestroyer->self, no cast, single-target, visual
        Multicast = 31221, // Boss->self, 3.0s cast, single-target, visual
        HemitheosAeroKnockback = 31243, // Helper->self, 6.0s cast, raidwide knockback ?
        HemitheosHolySpread = 30770, // Helper->player, 5.0s cast, range 6 circle spread

        StaticMoon = 30738, // ImmatureIo->self, 3.0s cast, range 10 circle aoe
        StymphalianStrike = 30741, // ImmatureStymphalide->self, 3.0s cast, range 60 width 8 rect aoe

        //_Gen_BoughOfAttis = 30756, // Boss->self, 4.0s cast, single-target
        //_Gen_BullishSwipe = 30744, // ImmatureMinotaur->self, 2.5s cast, single-target
        //_Gen_BullishSlash = 30743, // ImmatureMinotaur->self, no cast, range 60 ?-degree cone
        //_Gen_StaticPath = 30739, // ImmatureIo->self, no cast, single-target
        //_Gen_StaticPath = 30740, // Helper->self, no cast, range 60 width 8 rect
        //_Gen_BullishSwipe = 30745, // Helper->self, 3.0s cast, range 60 90-degree cone
        //_Gen_HemitheossAeroIV = 30772, // Helper->self, 6.0s cast, range 60 circle
        //_Gen_ShadowOfAttis = 30763, // Boss->self, no cast, single-target
        //_Gen_BronzeBellows = 30742, // ImmatureStymphalide->self, no cast, range 60 width 8 rect
        //_Gen_Burst = 30764, // Helper->self, no cast, range 5 circle
        //_Gen_InviolatePurgation = 30750, // Boss->self, 4.0s cast, single-target
        //_Gen_HemitheossTornado = 30751, // Helper->self, 14.0s cast, range 25 circle
        //_Gen_HemitheossGlareIII = 30752, // Helper->self, 14.0s cast, range ?-30 donut
        //_Gen_LightOfLife = 30946, // Boss->self, 26.0s cast, range 60 circle
        //_Gen_HemitheossHolyIV = 30749, // Helper->self, no cast, range 60 circle
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
}
