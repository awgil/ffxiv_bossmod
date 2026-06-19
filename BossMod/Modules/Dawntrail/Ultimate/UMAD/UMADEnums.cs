namespace BossMod.Dawntrail.Ultimate.UMAD;

public enum OID : uint
{
    BossP1 = 0x4C30, // R6.000, x1
    GravenImage = 0x4C31, // R0.500, x9
    Gravitas = 0x1EC022, // blue puddle, R5.000
    TelePortent = 0x1EC023, // arrow, R2.000

    BossP2 = 0x4C32, // R6.020, x1
    CloneP2 = 0x4C39, // R3.500, x0 (spawn during fight)

    ChaosP3 = 0x4C34, // R6.000, x0 (spawn during fight)
    ExdeathP3 = 0x4C35, // R3.800, x0 (spawn during fight)
    // dunno who these guys are. probably related to limit cut
    CloneP3 = 0x4BFB, // R2.700, x0 (spawn during fight), casts Ultima Blaster
    OtherKefkaP3 = 0x482B, // R6.000, x0 (spawn during fight)
    BlackHoleP3 = 0x4C38, // R1.000, x0 (spawn during fight)

    FireCrystal = 0x1EC03A,
    WaterCrystal = 0x1EC03B,
    WindCrystal = 0x1EC03C,

    Helper = 0x233C, // R0.500, x37, Helper type
}

public enum AID : uint
{
    AutoAttackP1 = 49746, // BossP1->player, no cast, single-target
    RevoltingRuinIIIFirst = 50179, // BossP1->self/player, 5.0s cast, range 100 120-degree cone
    RevoltingRuinIIISecond = 50401, // BossP1->self, no cast, range 100 120-degree cone
    TeleportP1 = 50173, // BossP1->location, no cast, single-target
    GravenImage = 48370, // BossP1->self, 3.0s cast, single-target
    MysteryMagic = 47764, // BossP1->self, 5.0s cast, single-target

    PulseWave = 47785, // GravenImage->player, no cast, single-target, knockback. application delay is about 0.5s
    BlizzardIIIBlowoutCast = 47765, // BossP1->self, 5.0s cast, single-target
    BlizzardIIIBlowout1 = 47768, // Helper->self, 5.0s cast, range 40 90-degree cone
    BlizzardIIIBlowoutFake = 47771, // Helper->self, 5.0s cast, range 40 90-degree cone, does nothing
    BlizzardIIIBlowout2 = 47774, // Helper->self, 5.0s cast, range 40 90-degree cone
    ThrummingThunderIII1 = 47775, // Helper->self, 5.0s cast, range 40 width 10 rect
    ThrummingThunderIIIFake = 47776, // Helper->self, 5.0s cast, range 40 width 10 rect, does nothing
    ThrummingThunderIII2 = 47777, // Helper->self, 5.0s cast, range 40 width 10 rect
    FlagrantFireIIISpread = 47778, // Helper->players, no cast, range 5 circle, spread
    FlagrantFireIIIStack = 47779, // Helper->players, no cast, range 6 circle, stack

    DoubleTroubleTrapCast = 47782, // BossP1->self, 3.0s cast, single-target
    DoubleTroubleTrapStack = 47783, // Helper->players, no cast, range 6 circle
    WaveCannon = 47784, // GravenImage->self, no cast, range 100 width 6 rect
    ExplosionP1 = 47786, // Helper->self, 3.0s cast, range 4 circle, tower
    UnmitigatedExplosionP1 = 47787, // Helper->self, no cast, range 100 circle

    LightOfJudgmentP1 = 50722, // BossP1->self, 5.0s cast, range 100 circle, raidwide
    Hyperdrive = 49739, // BossP1->players, no cast, range 5 circle, 3 hit tankbuster

    Gravitas = 47788, // GravenImage->players, no cast, range 5 circle
    GravitationalExplosion = 47789, // Helper->self, no cast, range 100 circle, triggered by stepping on Gravity or hitting it with Vitrophyre
    Vitrophyre = 47792, // GravenImage->players, no cast, range 5 circle
    GravitationalWave = 47793, // GravenImage->self, no cast, range 100 180-degree cone
    IntemperateWill = 47794, // GravenImage->self, no cast, range 100 180-degree cone
    GravityIII = 47791, // Helper->self, no cast, range 5 circle, puddle explosion

    TeleTrouncingCast = 47801, // BossP1->self, 5.0s cast, single-target
    TeleTrouncingArrowSpawn = 47802, // Helper->players, no cast, range 2 circle
    Unk1BossP1 = 50516, // BossP1->self, 3.0s cast, single-target
    Unk2BossP1 = 50517, // BossP1->self, no cast, single-target
    IndulgentWill = 47797, // GravenImage->player, no cast, single-target, applies confusion
    IdyllicWill = 47798, // GravenImage->players, no cast, range 5 circle, applies sleep
    AveMaria = 47795, // GravenImage->self, no cast, range 100 circle, inverted gaze
    IndolentWill = 47796, // GravenImage->self, no cast, range 100 circle, gaze
    LightOfJudgmentP1Enrage = 47803, // BossP1->self, 5.0s cast, range 100 circle

    UltimateEmbrace = 49740, // BossP2->players, 5.0s cast, range 5 circle, stack buster
    Forsaken = 47804, // BossP2->self, 7.0s cast, range 100 circle
    ThePathOfLight = 47806, // Helper->self, no cast, range 4 circle, tower
    TheRiverOfLight = 47807, // Helper->self, no cast, range 100 circle, tower failure
    Spelldriver = 47808, // Helper->players, no cast, range 5 circle, stack
    Spellscatter = 47809, // Helper->players, no cast, range 5 circle, spread
    Spellwave = 47810, // Helper->self, no cast, range 40 90-degree cone, cone
    SpellsTrouble = 47811, // Helper->player, no cast, single-target, kills players with debuff stacks remaining at end of forsaken
    FuturesEndCast = 47826, // BossP2-self, 6.4s cast, single-target
    PastsEndCast = 47827, // BossP2->self, 6.4s cast, single-target
    FuturesEndBossAOE = 47830, // BossP2->players, no cast, range 5 circle
    PastsEndBossAOE = 47831, // BossP2->players, no cast, range 5 circle
    FuturesEndCloneAOE = 47832, // CloneP2->players, no cast, range 5 circle
    PastsEndCloneAOE = 47833, // CloneP2->players, no cast, range 5 circle
    AllThingsEnding1 = 47836, // BossP2/CloneP2->self, 5.0s cast, range 100 180-degree cone
    AllThingsEnding2 = 47837, // BossP2/CloneP2->self, 5.0s cast, range 100 180-degree cone

    LightOfJudgmentP2 = 47805, // BossP2->self, 5.0s cast, range 100 circle
    TrineCast = 47839, // BossP2->self, 3.0s cast, single-target
    Trine = 47840, // Helper->self, no cast, range 6 circle
    WingsOfDestructionL = 47821, // BossP2->self, 4.0s cast, range 80 width 40 rect
    WingsOfDestructionR = 47822, // BossP2->self, 4.0s cast, range 80 width 40 rect
    WingsOfDestructionBusterCast = 50311, // BossP2->self, 4.0s cast, single-target
    WingsOfDestructionBuster = 47823, // Helper->players, no cast, range 7 circle
    LightOfJudgmentP2Enrage = 47841, // BossP2->self, 5.0s cast, range 100 circle

    AeroIIIAssault = 50167, // BossP1->self, 3.0s cast, range 40 circle
    Trance = 49878, // BossP1->self, 3.9s cast, single-target
    DefinitionOfInsanity = 47842, // BossP1->self, 4.0s cast, single-target
    DecisiveBattleA = 49890, // ChaosP3->self, 3.0s cast, single-target
    DecisiveBattleB = 49891, // ExdeathP3->self, 3.0s cast, single-target
    AutoExdeath = 49744, // ExdeathP3->player, no cast, single-target
    BowelsOfAgony = 47858, // ChaosP3->self, 5.0s cast, range 100 circle

    ThunderIIICircle = 47890, // ExdeathP3->self, 7.0s cast, range 11+R circle, bigass aoe
    StrayFlames = 47859, // Helper->player, no cast, range 5 circle, Entropy expiration
    Inferno = 47860, // Helper->player, no cast, range 4-10 donut, fire crystal bait
    StraySpray = 47862, // Helper->player, no cast, range 4-10 donut, Fluid expiration
    Tsunami = 47861, // Helper->players, no cast, range 5 circle, water crystal bait
    StrayGusts = 47863, // Helper->player, no cast, single-target, kills player if they failed the "gaze"
    Cyclone = 47864, // Helper->player, no cast, range 6 circle, wind stack (requires at least 2 players)
    ThunderIIIBusterCast = 47881, // ExdeathP3->self, 5.0s cast, single-target
    ThunderIIIBuster = 47884, // Helper->player, no cast, range 5 circle
    LongitudinalImplosion = 47869, // ChaosP3->self, 5.0+0.8s cast, single-target
    LatitudinalImplosion = 47870, // ChaosP3->self, 5.0+0.8s cast, single-target
    LatLongShockwave = 47871, // Helper->self, no cast, range 40 90-degree cone

    UltimaBlasterRaidwide = 47843, // CloneP3->self, no cast, range 100 circle
    UmbraSmash = 47872, // ChaosP3->location, 5.0s cast, range 100 circle
    VacuumWave = 47891, // ExdeathP3->self, 8.0s cast, range 100 circle
    Aetherlink1 = 49892, // ChaosP3->self, no cast, single-target
    Aetherlink2 = 49893, // ExdeathP3->self, no cast, single-target
    UltimaBlasterCharge = 47844, // CloneP3->self, no cast, range 100 width 6 rect

    Max = 47845, // BossP1->self, 5.0s cast, single-target, big kefka
    EarthquakeRaidwideCast = 50545, // ChaosP3->self, 5.0s cast, single-target
    EarthquakeRaidwide = 50546, // Helper->self, 5.0s cast, range 100 circle
    EarthquakeInstant = 47866, // Helper->self, no cast, range 100 circle
    SlapHappyRightHand = 47846, // BossP1->self, 5.0s cast, single-target, followed by party stack
    SlapHappyLeftHand = 47847, // BossP1->self, 5.0s cast, single-target, followed by role spread
    SlapHappyBig = 47848, // Helper->self, no cast, range 13 circle
    SlapHappySmall = 47849, // Helper->self, 1.5s cast, range 6 circle
    SlapHappyShockingImpact = 47850, // Helper->self, no cast, range 100 45?-degree cone, party stack, TODO verify angle
    SlapHappyShockwave = 47851, // Helper->self, no cast, range 100 45?-degree cone, role spread, TODO verify angle
    BlackHole = 47867, // ExdeathP3->self, 3.0s cast, single-target
    Nothingness = 47868, // BlackHoleP3->self, no cast, range 125 width 6 rect
    DamningEdict = 47873, // ChaosP3->self, 5.0s cast, range 60 width 80 rect
    BlackSpark = 48333, // Helper->player, no cast, single-target, triggered by touching black hole, applies dd
    LookUponMeAndDespairCast = 47852, // BossP1->self, 4.0+1.0s cast, single-target
    LookUponMeAndDespair = 47854, // Helper->self, 5.0s cast, range 100 width 16 rect
    UnkP3 = 50362, // BossP1->self, no cast, single-target
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper/GravenImage->player, extra=0x0
    DoubleTroubleTrap = 5078, // none->player, extra=0x0

    // players are always assigned one of the 1 set; if the second status is from the 2 set, the two arrows differ by 90 degrees
    TelePortentN1 = 4876, // Helper->player, extra=0x0
    TelePortentS1 = 4877, // Helper->player, extra=0x0
    TelePortentE1 = 4878, // Helper->player, extra=0x0
    TelePortentW1 = 4879, // Helper->player, extra=0x0
    TelePortentN2 = 5079, // Helper->player, extra=0x0
    TelePortentS2 = 5080, // Helper->player, extra=0x0
    TelePortentE2 = 5081, // Helper->player, extra=0x0
    TelePortentW2 = 5082, // Helper->player, extra=0x0

    Sleep = 4894, // GravenImage->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    Confused = 1283, // GravenImage->player, extra=0x0

    SpellsTrouble = 5083, // none->player, extra=0x4/0x3/0x2/0x1
    // these statuses are invisible, shape is temporarily indicated by an icon appearing over the player (at start of mechanic or after taking tower)
    ForsakenStack = 5084, // none->player, extra=0x0
    ForsakenSpread = 5085, // none->player, extra=0x0
    ForsakenCone = 5086, // none->player, extra=0x0

    DownForTheCount = 774, // BossP1->player, extra=0xEC7
    InEvent = 1268, // none->player, extra=0x0

    EpicHero = 4192, // none->player, extra=0x0
    EpicVillain = 4193, // none->ChaosP3, extra=0x0
    FatedHero = 4194, // none->player, extra=0x0
    FatedVillain = 4195, // none->ExdeathP3, extra=0x0
    Entropy = 1600, // none->player, extra=0x0
    DynamicFluid = 1601, // none->player, extra=0x0
    Headwind = 1602, // none->player, extra=0x0
    Tailwind = 1603, // none->player, extra=0x0
    WindResistanceDownII = 1052, // Helper->player, extra=0x0
    LightningResistanceDownII = 2998, // Helper->player, extra=0x0
    UnkBoss = 2273, // BossP1->BossP1, extra=0x1FF/0x22B
    P3Max = 2536, // none->BossP1, extra=0x1FA

    Accretion = 1604, // none->player, extra=0x0
    PrimordialCrust = 5454, // none->player, extra=0x0
    FirstInLine = 3004, // none->player, extra=0x0
    SecondInLine = 3005, // none->player, extra=0x0
    ThirdInLine = 3006, // none->player, extra=0x0
    EarthResistanceDownII = 3372, // Helper->player, extra=0x0
    Unbecoming = 5452, // BlackHoleP3->player, extra=0x1
    MeanestExistence = 5453, // BlackHoleP3->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
    FireSpread = 127, // player->self
    FireStack = 128, // player->self
    MysteryMagicFireLie = 673, // BossP1->self
    MysteryMagicFireTruth = 674, // BossP1->self
    MysteryMagicIceLie = 675, // BossP1->self
    MysteryMagicIceTruth = 676, // BossP1->self
    MysteryMagicThunderLie = 677, // BossP1->self
    MysteryMagicThunderTruth = 678, // BossP1->self

    SharedBuster = 259, // player->self
    ForsakenStack = 715, // player->self
    ForsakenSpread = 716, // player->self
    ForsakenCone = 717, // player->self

    Blaster1 = 336, // player->self
    Blaster2 = 337, // player->self
    Blaster3 = 338, // player->self
    Blaster4 = 339, // player->self
    Blaster5 = 437, // player->self
    Blaster6 = 438, // player->self
    Blaster7 = 439, // player->self
    Blaster8 = 440, // player->self
}

public enum TetherID : uint
{
    GravenImage = 45, // GravenImage->player, used for every statue-related P1 mechanic
    ExdeathThunder = 64, // ExdeathP3->GravenImage
    BlackHole = 84, // BlackHoleP3->player
}
