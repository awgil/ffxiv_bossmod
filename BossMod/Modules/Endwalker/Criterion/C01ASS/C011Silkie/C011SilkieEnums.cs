namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie
{
    public enum OID : uint
    {
        NBoss = 0x39F2, // R6.000, x1
        NSilkenPuff = 0x39F3, // R1.000, x15
        NEasternEwer = 0x39F4, // R2.400, x5

        SBoss = 0x39F5, // R6.000, x1
        SSilkenPuff = 0x39F6, // R1.000, x15
        SEasternEwer = 0x39F7, // R2.400, x5

        Helper = 0x233C, // R0.500, x15
    };

    public enum AID : uint
    {
        AutoAttack = 870, // NBoss/SBoss->player, no cast, single-target

        NCarpetBeater = 30543, // NBoss->player, 5.0s cast, single-target tankbuster
        NTotalWash = 30544, // NBoss->self, 5.0s cast, raidwide
        NDustBluster = 30572, // NBoss->location, 5.0s cast, range 60 circle knockback 16
        SCarpetBeater = 30578, // SBoss->player, 5.0s cast, single-target tankbuster
        STotalWash = 30579, // SBoss->self, 5.0s cast, raidwide
        SDustBluster = 30607, // SBoss->location, 5.0s cast, range 60 circle knockback 16

        NBracingSuds = 30551, // NBoss->self, 3.0s cast, single-target, applies green status
        NChillingSuds = 30552, // NBoss->self, 3.0s cast, single-target, applies blue status
        NFizzlingSuds = 30553, // NBoss->self, 3.0s cast, single-target, applies yellow status
        NSoapsUp = 30554, // NBoss->self, 4.0s cast, single-target, removes color statuses
        NFizzlingDusterAOE = 30557, // Helper->self, 5.0s cast, range 60 45-degree cone aoe
        NFreshPuff = 30566, // NBoss->self, 4.0s cast, single-target, visual (statuses on puffs)
        NSoapingSpreeBoss = 30567, // NBoss->self, 5.0s cast, single-target, visual
        NSoapingSpreePuff = 30571, // NSilkenPuff->self, 5.0s cast, single-target, removes color statuses
        SBracingSuds = 30586, // SBoss->self, 3.0s cast, single-target, applies green status
        SChillingSuds = 30587, // SBoss->self, 3.0s cast, single-target, applies blue status
        SFizzlingSuds = 30588, // SBoss->self, 3.0s cast, single-target, applies yellow status
        SSoapsUp = 30589, // SBoss->self, 4.0s cast, single-target, removes color statuses
        SFizzlingDusterAOE = 30592, // Helper->self, 5.0s cast, range 60 45-degree cone aoe
        SFreshPuff = 30601, // SBoss->self, 4.0s cast, single-target, visual (statuses on puffs)
        SSoapingSpreeBoss = 30602, // SBoss->self, 5.0s cast, single-target, visual
        SSoapingSpreePuff = 30606, // SSilkenPuff->self, 5.0s cast, single-target, removes color statuses

        NSqueakyCleanE = 30545, // NBoss->self, 4.5s cast, single-target, visual
        NSqueakyCleanW = 30546, // NBoss->self, 4.5s cast, single-target, visual
        NSqueakyCleanAOE1 = 30547, // Helper->self, 6.0s cast, range 60 45-degree cone
        NSqueakyCleanAOE2 = 30548, // Helper->self, 7.7s cast, range 60 45-degree cone
        NSqueakyCleanAOE3E = 30549, // Helper->self, 9.2s cast, range 60 225-degree cone
        NSqueakyCleanAOE3W = 30550, // Helper->self, 9.2s cast, range 60 225-degree cone
        SSqueakyCleanE = 30580, // SBoss->self, 4.5s cast, single-target, visual
        SSqueakyCleanW = 30581, // SBoss->self, 4.5s cast, single-target, visual
        SSqueakyCleanAOE1 = 30582, // Helper->self, 6.0s cast, range 60 45-degree cone
        SSqueakyCleanAOE2 = 30583, // Helper->self, 7.7s cast, range 60 45-degree cone
        SSqueakyCleanAOE3E = 30584, // Helper->self, 9.2s cast, range 60 225-degree cone
        SSqueakyCleanAOE3W = 30585, // Helper->self, 9.2s cast, range 60 225-degree cone

        SlipperySoapTargetSelection = 31227, // Helper->player, no cast, single-target, cast at the beginning of visual cast
        NSlipperySoap = 30558, // NBoss->self, 5.0s cast, single-target, visual
        NSlipperySoapAOEBlue = 30560, // NBoss->players, no cast, width 10 rect charge aoe
        NSlipperySoapAOEGreen = 30561, // NBoss->players, no cast, width 10 rect charge aoe + knockback 15 forward
        NSlipperySoapAOEYellow = 30562, // NBoss->players, no cast, width 10 rect charge aoe + apply forked lightning status
        NSoapsudStatic = 30701, // Helper->self, no cast, range 5 spread (forked lightning)
        NChillingDusterBoss = 30563, // Helper->self, no cast, range 60 width 10 cross aoe
        NBracingDusterBoss = 30564, // Helper->self, no cast, range 5-60 donut
        NFizzlingDusterBoss = 30565, // Helper->self, no cast, range 60 45-degree cone
        SSlipperySoap = 30593, // SBoss->self, 5.0s cast, single-target, visual
        SSlipperySoapAOEBlue = 30595, // SBoss->players, no cast, width 10 rect charge aoe
        SSlipperySoapAOEGreen = 30596, // SBoss->players, no cast, width 10 rect charge aoe + knockback 15 forward
        SSlipperySoapAOEYellow = 30597, // SBoss->players, no cast, width 10 rect charge aoe + apply forked lightning status
        SSoapsudStatic = 30702, // Helper->self, no cast, range 5 spread (forked lightning)
        SChillingDusterBoss = 30598, // Helper->self, no cast, range 60 width 10 cross aoe
        SBracingDusterBoss = 30599, // Helper->self, no cast, range 5-60 donut
        SFizzlingDusterBoss = 30600, // Helper->self, no cast, range 60 45-degree cone

        NChillingDusterPuff = 30568, // Helper->self, 5.0s cast, range 60 width 10 cross aoe
        NBracingDusterPuff = 30569, // Helper->self, 5.0s cast, range 5-60 donut aoe
        NFizzlingDusterPuff = 30570, // Helper->self, 5.0s cast, range 60 45-degree cone
        SChillingDusterPuff = 30603, // Helper->self, 5.0s cast, range 60 width 10 cross aoe
        SBracingDusterPuff = 30604, // Helper->self, 5.0s cast, range 5-60 donut aoe
        SFizzlingDusterPuff = 30605, // Helper->self, 5.0s cast, range 60 45-degree cone

        NPuffAndTumbleMove = 30576, // NSilkenPuff->location, no cast, single-target
        NPuffAndTumbleAOE = 30577, // Helper->location, 1.6s cast, range 4 circle aoe
        SPuffAndTumbleMove = 30611, // SSilkenPuff->location, no cast, single-target
        SPuffAndTumbleAOE = 30612, // Helper->location, 1.6s cast, range 4 circle aoe

        NEasternEwers = 30573, // NBoss->self, 4.0s cast, single-target, visual
        NBrimOver = 30574, // NEasternEwer->self, 3.0s cast, range 4 circle ? (initial cast for exaflare)
        NRinse = 30575, // Helper->self, no cast, range 4 circle exaflare
        SEasternEwers = 30608, // SBoss->self, 4.0s cast, single-target, visual
        SBrimOver = 30609, // SEasternEwer->self, 3.0s cast, range 4 circle ? (initial cast for exaflare)
        SRinse = 30610, // Helper->self, no cast, range 4 circle exaflare

        Enrage = 31200, // NBoss->self, 10.0s cast, wipe
        Enrage2 = 31225, // NBoss->self, no cast, range 100 circle ???
        NBuffetedPuffs = 30697, // NSilkenPuff->self, no cast, single-target, visual (puffs merge if too close)
        NBuffetedPuffsAOE = 30698, // Helper->self, no cast, raidwide wipe
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
