namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

public enum OID : uint
{
    NBoss = 0x409C, // R8.000, x1
    NSpringCrystalSphere = 0x409D, // R4.200, spawn during fight
    NSpringCrystalRect = 0x409E, // R4.200, spawn during fight
    NAiryBubbleTyphoon = 0x409F, // R1.200, spawn during fight
    NAiryBubbleExaflare = 0x40A0, // R1.300, spawn during fight
    NZaratan = 0x40A1, // R1.120, spawn during fight (snake)

    SBoss = 0x40A3, // R8.000, x1
    SSpringCrystalSphere = 0x40A4, // R4.200, spawn during fight
    SSpringCrystalRect = 0x40A5, // R4.200, spawn during fight
    SAiryBubbleTyphoon = 0x40A6, // R1.200, spawn during fight
    SAiryBubbleExaflare = 0x40A7, // R1.300, spawn during fight
    SZaratan = 0x40A8, // R1.120, spawn during fight (snake)

    Helper = 0x233C, // R0.500, 523 type, spawn during fight
    KnockbackHelper = 0x40B2, // R0.500, spawn during fight
    BubbleStrewer = 0x1EB936, // R0.500, EventObj type, spawn during fight
    BubbleBlower = 0x1EB937, // R0.500, EventObj type, spawn during fight
};

public enum AID : uint
{
    AutoAttack = 35495, // *Boss->player, no cast, single-target
    Teleport = 35447, // *Boss->location, no cast, single-target

    TidalRoar = 35540, // *Boss->self, 5.0s cast, single-target, visual (raidwide)
    NTidalRoarAOE = 35541, // Helper->self, no cast, range 100 circle, raidwide
    STidalRoarAOE = 35565, // Helper->self, no cast, range 100 circle, raidwide

    SpringCrystals = 35496, // *Boss->self, 2.2s cast, single-target, visual (create crystals)
    NSpringCrystalSphereAppear = 35497, // NSpringCrystalSphere->self, no cast, single-target, visual
    NSpringCrystalRectAppear = 35498, // NSpringCrystalRect->self, no cast, single-target, visual
    SSpringCrystalSphereAppear = 35545, // SSpringCrystalSphere->self, no cast, single-target, visual
    SSpringCrystalRectAppear = 35546, // SSpringCrystalRect->self, no cast, single-target, visual
    NSaturateRect = 35500, // NSpringCrystalRect->self, 1.0s cast, range 76 width 10 rect aoe
    SSaturateRect = 35548, // SSpringCrystalRect->self, 1.0s cast, range 76 width 10 rect aoe
    BubbleNet1 = 35501, // *Boss->self, 4.1s cast, single-target, visual (raidwide + debuffs)
    NBubbleNet1AOE = 35502, // Helper->self, 5.0s cast, range 65 circle, raidwide
    SBubbleNet1AOE = 35549, // Helper->self, 5.0s cast, range 65 circle, raidwide
    Hydrofall = 35508, // *Boss->self, 4.0s cast, single-target, visual (stacks)
    HydrofallSecond = 35509, // *Boss->self, no cast, single-target, visual (stacks after spreads)
    HydrofallApply = 35510, // Helper->player, no cast, single-target, visual (apply debuff)
    NHydrofallAOE = 35511, // Helper->players, no cast, range 6 circle stack
    SHydrofallAOE = 35550, // Helper->players, no cast, range 6 circle stack
    Hydrobullet = 35512, // *Boss->self, 4.0s cast, single-target, visual (spreads)
    HydrobulletSecond = 35949, // *Boss->self, no cast, single-target, visual (spreads after stacks)
    HydrobulletApply = 35513, // Helper->player, no cast, single-target, visual (apply debuff)
    NHydrobulletAOE = 35514, // Helper->players, no cast, range 15 circle spread
    SHydrobulletAOE = 35551, // Helper->players, no cast, range 15 circle spread
    FlukeGale = 35505, // *Boss->self, 3.0s cast, single-target, visual (knockbacks)
    FlukeGaleAOE1 = 35506, // Helper->self, 8.0s cast, range 20 width 20 rect, first knockback
    FlukeGaleAOE2 = 35507, // Helper->self, 10.0s cast, range 20 width 20 rect, second knockback

    BlowingBubbles = 35517, // *Boss->self, 4.2s cast, single-target, visual (exaflares)
    BlowingBubblesRiptide = 35518, // *AiryBubbleExaflare->player, no cast, single-target, attract to center
    BlowingBubblesFetters = 35519, // *AiryBubbleExaflare->player, no cast, single-target, kill?
    Hydrobomb = 35536, // *Boss->self, 2.2s cast, single-target, visual (puddles)
    NHydrobombAOE = 35537, // Helper->location, 3.0s cast, range 5 circle puddle
    SHydrobombAOE = 35563, // Helper->location, 3.0s cast, range 5 circle puddle

    StrewnBubbles = 35515, // *Boss->self, 2.2s cast, single-target, visual (half-arena lines)
    NSphereShatter = 35516, // Helper->self, no cast, range 20 width 10 rect
    SSphereShatter = 35552, // Helper->self, no cast, range 20 width 10 rect
    NRecedingTwintides = 35532, // NBoss->self, 5.0s cast, range 14 circle
    NFarTide = 35535, // NBoss->self, 1.0s cast, range 8-60 donut
    NEncroachingTwintides = 35534, // NBoss->self, 5.0s cast, range 8-60 donut
    NNearTide = 35533, // NBoss->self, 1.0s cast, range 14 circle
    SRecedingTwintides = 35559, // SBoss->self, 5.0s cast, range 14 circle
    SFarTide = 35562, // SBoss->self, 1.0s cast, range 8-60 donut
    SEncroachingTwintides = 35561, // SBoss->self, 5.0s cast, range 8-60 donut
    SNearTide = 35560, // SBoss->self, 1.0s cast, range 14 circle

    Roar = 35524, // *Boss->self, 3.0s cast, single-target, visual (spawn snakes)
    BubbleNet2 = 35525, // *Boss->self, 4.1s cast, single-target, visual (raidwide + debuffs)
    NBubbleNet2AOE = 35526, // Helper->self, 5.0s cast, range 65 circle, raidwide
    SBubbleNet2AOE = 35556, // Helper->self, 5.0s cast, range 65 circle, raidwide
    Updraft = 35527, // *Boss->self, 4.2s cast, single-target, visual (raise bubbles)
    UpdraftApply = 35528, // Helper->self, 5.0s cast, range 35 circle, visual (apply raise bubbles animation)
    HundredLashingsVisual = 35530, // *Zaratan->self, 2.1s cast, single-target, visual (remove bubble)
    NHundredLashingsNormal = 35529, // NZaratan->self, 3.2s cast, range 60 180-degree cone
    NHundredLashingsBubble = 35531, // Helper->self, 3.2s cast, range 60 180-degree cone
    SHundredLashingsNormal = 35557, // SZaratan->self, 3.2s cast, range 60 180-degree cone
    SHundredLashingsBubble = 35558, // Helper->self, 3.2s cast, range 60 180-degree cone

    AngrySeas = 35520, // *Boss->self, 4.2s cast, single-target, visual (knockback to the sides)
    NAngrySeasAOE = 35521, // Helper->self, 5.0s cast, range 46 width 50 rect, knockback 12
    SAngrySeasAOE = 35553, // Helper->self, 5.0s cast, range 46 width 50 rect, knockback 12
    FlukeTyphoon = 35503, // *Boss->self, 3.0s cast, single-target, visual (knockback across arena)
    FlukeTyphoonAOE = 35504, // Helper->self, 5.0s cast, range 40 width 40 rect, knockback 20 on targets in bubbles
    FlukeTyphoonRiptide = 35458, // *AiryBubbleTyphoon->player, no cast, single-target, attract to center and bind
    FlukeTyphoonFetters = 35459, // *AiryBubbleTyphoon->player, no cast, single-target, move with the bubble
    NSaturateSphere = 35499, // NSpringCrystalSphere->self, 1.0s cast, range 8 circle
    SSaturateSphere = 35547, // SSpringCrystalSphere->self, 1.0s cast, range 8 circle
    NBurst = 35522, // Helper->self, no cast, range 4 circle tower
    NBigBurst = 35523, // Helper->self, no cast, range 60 circle tower fail
    SBurst = 35554, // Helper->self, no cast, range 4 circle tower
    SBigBurst = 35555, // Helper->self, no cast, range 60 circle tower fail

    Enrage = 35542, // *Boss->self, 10.0s cast, single-target, enrage
};

public enum SID : uint
{
    BubbleWeave = 3743, // none->player, extra=0x0
    FoamyFetters = 3788, // none->player, extra=0x0
    HydrobulletTarget = 3748, // none->player, extra=0x0
    HydrofallTarget = 3747, // none->player, extra=0x0
    Bubble = 3745, // none->*SpringCrystal1/*Zaratan, extra=0xC8
};

public enum IconID : uint
{
    Order1 = 336, // KnockbackHelper
    Order2 = 337, // KnockbackHelper
};
