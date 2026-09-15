namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

public enum OID : uint
{
    Boss = 0x48E5, // R7.800, x1
    Helper = 0x233C, // R0.500, x25 (spawn during fight), Helper type
    SmallCrystal = 0x48E6, // R0.700, x0 (spawn during fight)
    BigCrystal = 0x48E7, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 43342, // Boss->player, no cast, single-target

    Roar1 = 43950, // Boss->self, 5.0s cast, range 60 circle
    Roar2 = 45202, // Boss->self, 5.0s cast, range 60 circle
    Roar3 = 43951, // Boss->self, 5.0s cast, range 60 circle

    ChainbladeBossCast1 = 43887, // Boss->self, 5.0s cast, single-target
    ChainbladeBossCast2 = 43888, // Boss->self, 5.0s cast, single-target
    ChainbladeBossCast3 = 45081, // Boss->self, 4.0s cast, single-target
    ChainbladeBossCast4 = 45082, // Boss->self, 4.0s cast, single-target

    ChainbladeTail1 = 43889, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeTail2 = 43890, // Helper->self, 6.6s cast, range 40 width 4 rect
    ChainbladeTail3 = 45077, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeTail4 = 45078, // Helper->self, 6.6s cast, range 40 width 4 rect
    ChainbladeTail5 = 45083, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeTail6 = 45084, // Helper->self, 5.6s cast, range 40 width 4 rect
    ChainbladeTail7 = 45086, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeTail8 = 45087, // Helper->self, 5.6s cast, range 40 width 4 rect

    ChainbladeSide1 = 43891, // Helper->self, 7.2s cast, range 80 width 28 rect
    ChainbladeSide2 = 43892, // Helper->self, 7.2s cast, range 80 width 28 rect
    ChainbladeSide3 = 45085, // Helper->self, 6.2s cast, range 80 width 28 rect
    ChainbladeSide4 = 45088, // Helper->self, 6.2s cast, range 80 width 28 rect

    ChainbladeBossRepeat1 = 43893, // Boss->self, no cast, single-target
    ChainbladeBossRepeat2 = 43894, // Boss->self, no cast, single-target
    ChainbladeBossRepeat3 = 45089, // Boss->self, no cast, single-target
    ChainbladeBossRepeat4 = 45090, // Boss->self, no cast, single-target

    ChainbladeTailFast1 = 43895, // Helper->self, 1.2s cast, range 40 width 4 rect
    ChainbladeTailFast2 = 43896, // Helper->self, 1.6s cast, range 40 width 4 rect
    ChainbladeTailFast3 = 45079, // Helper->self, 1.2s cast, range 40 width 4 rect
    ChainbladeTailFast4 = 45080, // Helper->self, 1.6s cast, range 40 width 4 rect
    ChainbladeTailFast5 = 45091, // Helper->self, 1.2s cast, range 40 width 4 rect
    ChainbladeTailFast6 = 45092, // Helper->self, 1.6s cast, range 40 width 4 rect
    ChainbladeTailFast7 = 45094, // Helper->self, 1.2s cast, range 40 width 4 rect
    ChainbladeTailFast8 = 45095, // Helper->self, 1.6s cast, range 40 width 4 rect

    ChainbladeSideFast1 = 43897, // Helper->self, 2.2s cast, range 80 width 28 rect
    ChainbladeSideFast2 = 43898, // Helper->self, 2.2s cast, range 80 width 28 rect
    ChainblideSideFast3 = 45093, // Helper->self, 2.2s cast, range 80 width 28 rect
    ChainbladeSideFast4 = 45096, // Helper->self, 2.2s cast, range 80 width 28 rect

    WhiteFlash = 43906, // Helper->players, 8.0s cast, range 6 circle
    Dragonspark = 43907, // Helper->players, 8.0s cast, range 6 circle

    BossFlight1 = 43899, // Boss->location, 5.0s cast, range 40 width 4 rect
    BossFlight2 = 43902, // Boss->location, 5.0s cast, range 40 width 4 rect
    BossFlight3 = 45097, // Boss->location, 4.0s cast, range 40 width 4 rect
    BossFlight4 = 45098, // Boss->location, 4.0s cast, range 40 width 4 rect

    FlightCast1 = 43900, // Helper->self, 6.5s cast, range 40 width 8 rect
    FlightCast2 = 43903, // Helper->self, 6.5s cast, range 40 width 8 rect
    FlightCast3 = 45099, // Helper->self, 5.5s cast, range 40 width 8 rect
    FlightCast4 = 45101, // Helper->self, 5.5s cast, range 40 width 8 rect

    FlightResonance1 = 43901, // Helper->self, 10.0s cast, range 40 width 16 rect
    FlightResonance2 = 45100, // Helper->self, 9.0s cast, range 40 width 16 rect
    FlightResonanceVisual1 = 45125, // Helper->self, 7.2s cast, range 40 width 8 rect
    FlightResonanceVisual2 = 45111, // Helper->self, 7.2s cast, range 40 width 8 rect
    FlightResonanceVisual3 = 45126, // Helper->self, 6.2s cast, range 40 width 8 rect
    FlightResonanceVisual4 = 45104, // Helper->self, 6.2s cast, range 40 width 8 rect

    FlightRadiance1 = 43904, // Helper->self, 10.0s cast, range 40 width 18 rect
    FlightRadiance2 = 43905, // Helper->self, 10.0s cast, range 40 width 18 rect
    FlightRadiance3 = 45102, // Helper->self, 9.0s cast, range 40 width 18 rect
    FlightRadiance4 = 45103, // Helper->self, 9.0s cast, range 40 width 18 rect

    AethericResonance = 43919, // Boss->self, 9.7+1.3s cast, single-target
    GuardianResonancePuddle = 43923, // Helper->location, 3.0s cast, range 6 circle
    GuardianResonanceTowerSmall = 43920, // Helper->location, 11.0s cast, range 2 circle
    GuardianResonanceTowerLarge = 43921, // Helper->location, 11.0s cast, range 4 circle

    WyvernsRadiancePuddleSmall = 43942, // Helper->location, 5.0s cast, range 6 circle
    WyvernsRadiancePuddleLarge = 43933, // Helper->location, 5.0s cast, range 12 circle

    WyvernsRadianceQuake1 = 43911, // Helper->self, 7.5s cast, range 8 circle
    WyvernsRadianceQuake2 = 43912, // Helper->self, 9.5s cast, range 8-14 donut
    WyvernsRadianceQuake3 = 43913, // Helper->self, 11.5s cast, range 14-20 donut
    WyvernsRadianceQuake4 = 43914, // Helper->self, 13.5s cast, range 20-26 donut

    RushCast = 43909, // Boss->location, 6.0s cast, width 12 rect charge
    RushInstant = 43910, // Boss->location, no cast, width 12 rect charge

    RushTelegraph = 43908, // Helper->location, 2.5s cast, width 12 rect charge
    QuakeTelegraph = 45110, // Helper->self, 3.5s cast, range 8 circle

    WyvernsRadianceExawaveFirst = 43940, // Helper->self, 2.5s cast, range 8 width 40 rect
    WyvernsRadianceExawaveRest = 43941, // Helper->self, 1.0s cast, range 8 width 40 rect

    OurobladeCast1 = 43915, // Boss->self, 6.0+1.5s cast, single-target
    OurobladeCast2 = 43917, // Boss->self, 6.0+1.5s cast, single-target
    OurobladeCast3 = 45105, // Boss->self, 5.0+1.5s cast, single-target
    OurobladeCast4 = 45106, // Boss->self, 5.0+1.5s cast, single-target

    Ouroblade1 = 43918, // Helper->self, 7.0s cast, range 40 180-degree cone
    Ouroblade2 = 43916, // Helper->self, 7.0s cast, range 40 180-degree cone
    Ouroblade3 = 45108, // Helper->self, 6.0s cast, range 40 180-degree cone
    Ouroblade4 = 45107, // Helper->self, 6.0s cast, range 40 180-degree cone

    WildEnergy = 43932, // Helper->players, 8.0s cast, range 6 circle

    SteeltailCast1 = 43949, // Boss->self, 4.0s cast, range 60 width 6 rect
    SteeltailCast2 = 45109, // Boss->self, 3.0s cast, range 60 width 6 rect
    SteeltailThrust1 = 44805, // Helper->self, 4.6s cast, range 60 width 6 rect
    SteeltailThrust2 = 44806, // Helper->self, 3.6s cast, range 60 width 6 rect

    ChainbladeChargeCast = 43947, // Boss->self, 6.0s cast, single-target
    ChainbladeChargeVisual = 43948, // Boss->player, no cast, single-target
    ChainbladeChargeStack = 44812, // Helper->location, no cast, range 6 circle

    WyvernsVengeanceFirst = 43926, // Helper->self, 5.0s cast, range 6 circle
    WyvernsVengeanceRest = 43927, // Helper->location, no cast, range 6 circle

    SmallCrystalVisual = 43924, // SmallCrystal->self, 0.5s cast, range 6 circle
    BigCrystalVisual = 43925, // BigCrystal->self, 0.5s cast, range 12 circle
    SmallCrystalExplosion = 44809, // Helper->self, 1.0s cast, range 6 circle
    BigCrystalExplosion = 44810, // Helper->self, 1.0s cast, range 12 circle

    ForgedFuryCast = 43934, // Boss->self, 5.0s cast, single-target
    ForgedFuryHit1 = 43935, // Helper->self, 7.0s cast, range 60 circle
    ForgedFuryHit2 = 44792, // Helper->self, 7.8s cast, range 60 circle
    ForgedFuryHit3 = 44793, // Helper->self, 10.2s cast, range 60 circle

    ClamorousChaseLeftWing = 43955, // Boss->self, 8.0s cast, single-target, left wing
    ClamorousChaseRightWing = 43958, // Boss->self, 8.0s cast, single-target, right wing
    ClamorousJump1 = 43956, // Boss->location, no cast, range 6 circle
    ClamorousJump2 = 43959, // Boss->location, no cast, range 6 circle
    ClamorousCleave1 = 43957, // Helper->self, 1.0s cast, range 60 ?-degree cone
    ClamorousCleave2 = 43960, // Helper->self, 1.0s cast, range 60 ?-degree cone

    WyvernsWealBossCast = 43936, // Boss->self, 8.0s cast, single-target
    WyvernsWealBossVisual = 43872, // Boss->self, no cast, single-target
    WyvernsWealSlow = 43937, // Helper->self, 2.0s cast, range 60 width 6 rect
    WyvernsWealFast = 43938, // Helper->self, 0.5s cast, range 60 width 6 rect

    WrathfulRattle = 43943, // Boss->self, 1.0+2.5s cast, single-target

    WyveCannonMiddle = 43944, // Helper->self, 3.5s cast, range 40 width 8 rect
    WyveCannonRepeat = 43945, // Helper->self, 1.0s cast, range 40 width 4 rect
    WyveCannonEdge = 43946, // Helper->self, 2.0s cast, range 40 width 4 rect

    WyvernsVengeanceLineFirst = 43952, // Helper->self, 8.0s cast, range 6 circle
    WyvernsVengeanceLineRest = 43953, // Helper->location, no cast, range 6 circle

    ForgedFuryEnrageCast = 43954, // Boss->self, 5.0s cast, range 60 circle
    ForgedFuryEnrage = 44878, // Helper->self, 10.2s cast, range 60 circle

    WyvernsRattle = 43939, // Boss->self, no cast, single-target
    GreaterResonance = 43922, // Helper->location, no cast, range 60 circle
    Unk0 = 45175, // Boss->location, no cast, single-target
    Unk1 = 43827, // Boss->location, no cast, single-target
    Unk2 = 43859, // Boss->self, no cast, single-target
    Unk3 = 43873, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    Share = 100, // player->self
    Spread = 101, // player->self
    LimitCut1 = 404, // player->self
    LimitCut2 = 405, // player->self
    LimitCut3 = 406, // player->self
    LimitCut4 = 407, // player->self
    LimitCut5 = 408, // player->self
    LimitCut6 = 409, // player->self
    LimitCut7 = 410, // player->self
    LimitCut8 = 411, // player->self
    LineTarget = 470, // player->self
}
