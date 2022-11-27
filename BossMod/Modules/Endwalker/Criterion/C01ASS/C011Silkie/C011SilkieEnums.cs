namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie
{
    public enum OID : uint
    {
        Boss = 0x39F2, // R6.000, x1
        Helper = 0x233C, // R0.500, x15
        SilkenPuff = 0x39F3, // R1.000, x15
        EasternEwer = 0x39F4, // R2.400, x5
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        CarpetBeater = 30543, // Boss->player, 5.0s cast, single-target tankbuster
        TotalWash = 30544, // Boss->self, 5.0s cast, raidwide
        DustBluster = 30572, // Boss->location, 5.0s cast, range 60 circle knockback 16

        BracingSuds = 30551, // Boss->self, 3.0s cast, single-target, appplies green status
        ChillingSuds = 30552, // Boss->self, 3.0s cast, single-target, applies blue status
        FizzlingSuds = 30553, // Boss->self, 3.0s cast, single-target, applies yellow status
        SoapsUp = 30554, // Boss->self, 4.0s cast, single-target, removes color statuses
        FizzlingDusterAOE = 30557, // Helper->self, 5.0s cast, range 60 45-degree cone aoe
        FreshPuff = 30566, // Boss->self, 4.0s cast, single-target, visual (statuses on puffs)
        SoapingSpreeBoss = 30567, // Boss->self, 5.0s cast, single-target, visual
        SoapingSpreePuff = 30571, // SilkenPuff->self, 5.0s cast, single-target, removes color statuses

        SqueakyCleanE = 30545, // Boss->self, 4.5s cast, single-target, visual
        SqueakyCleanW = 30546, // Boss->self, 4.5s cast, single-target
        SqueakyCleanAOE1 = 30547, // Helper->self, 6.0s cast, range 60 45-degree cone
        SqueakyCleanAOE2 = 30548, // Helper->self, 7.7s cast, range 60 45-degree cone
        SqueakyCleanAOE3E = 30549, // Helper->self, 9.2s cast, range 60 225-degree cone
        SqueakyCleanAOE3W = 30550, // Helper->self, 9.2s cast, range 60 225-degree cone

        SlipperySoapTargetSelection = 31227, // Helper->player, no cast, single-target, cast at the beginning of visual cast
        SlipperySoap = 30558, // Boss->self, 5.0s cast, single-target, visual
        SlipperySoapAOEBlue = 30560, // Boss->players, no cast, width 10 rect charge aoe
        SlipperySoapAOEGreen = 30561, // Boss->players, no cast, width 10 rect charge aoe + knockback 15 forward
        SlipperySoapAOEYellow = 30562, // Boss->players, no cast, width 10 rect charge aoe + apply forked lightning status
        SoapsudStatic = 30701, // Helper->self, no cast, range 5 spread (forked lightning)
        ChillingDusterBoss = 30563, // Helper->self, no cast, range 60 width 10 cross aoe
        BracingDusterBoss = 30564, // Helper->self, no cast, range 5-60 donut
        FizzlingDusterBoss = 30565, // Helper->self, no cast, range 60 45-degree cone

        ChillingDusterPuff = 30568, // Helper->self, 5.0s cast, range 60 width 10 cross aoe
        BracingDusterPuff = 30569, // Helper->self, 5.0s cast, range 5-60 donut aoe
        FizzlingDusterPuff = 30570, // Helper->self, 5.0s cast, range 60 45-degree cone

        PuffAndTumbleMove = 30576, // SilkenPuff->location, no cast, single-target
        PuffAndTumbleAOE = 30577, // Helper->location, 1.6s cast, range 4 circle aoe

        EasternEwers = 30573, // Boss->self, 4.0s cast, single-target, visual
        BrimOver = 30574, // EasternEwer->self, 3.0s cast, range 4 circle ? (initial cast for exaflare)
        Rinse = 30575, // Helper->self, no cast, range 4 circle exaflare

        Enrage = 31200, // Boss->self, 10.0s cast, wipe
        Enrage2 = 31225, // Boss->self, no cast, range 100 circle ???
        BuffetedPuffs = 30697, // SilkenPuff->self, no cast, single-target, visual (puffs merge if too close)
        BuffetedPuffsAOE = 30698, // Helper->self, no cast, raidwide wipe
    };

    public enum SID : uint
    {
        BracingSudsBoss = 3297, // Boss->Boss, extra=0x0
        ChillingSudsBoss = 3298, // Boss->Boss, extra=0x0
        FizzlingSudsBoss = 3299, // Boss->Boss, extra=0x0

        BracingSudsPuff = 3305, // none->SilkenPuff, extra=0x0
        ChillingSudsPuff = 3306, // none->SilkenPuff, extra=0x0
        FizzlingSudsPuff = 3307, // none->SilkenPuff, extra=0x0

        ForkedLightning = 587, // Boss->player, extra=0x0, spread
    };
}
