namespace BossMod.Stormblood.Ultimate.UCOB;

public enum OID : uint
{
    Twintania = 0x1FDF, // R3.960, x1
    Oviform = 0x1FE0, // R1.000, spawn during fight (hatch orb)

    NaelDeusDarnus = 0x1FE1, // R2.550, x1
    NaelGeminus = 0x1FE2, // R2.550, x4, 523 type (helper that for meteor streams)
    Firehorn = 0x1FE3, // R4.000, spawn during fight (fire dragon)
    Iceclaw = 0x1FE4, // R4.000, spawn during fight (ice dragon)
    Thunderwing = 0x1FE5, // R4.000, spawn during fight (thunder dragon)
    TailOfDarkness = 0x1FE6, // R4.000, spawn during fight (dark dragon)
    FangOfLight = 0x1FE7, // R4.000, spawn during fight (light dragon)

    BahamutPrime = 0x1FE8, // R4.200, x1
    Phoenix = 0x1FE9, // R2.800, x1

    Helper = 0x18D6, // R0.500, x42, mixed types
    EventHelper = 0x1EA1A1, // R2.000, x6, EventObj type

    VoidzoneTwister = 0x1E8910, // R0.500, EventObj type, spawn during fight
    VoidzoneLiquidHell = 0x1E88FE, // R0.500, EventObj type, spawn during fight
    Neurolink = 0x1E88FF, // R0.500, EventObj type, spawn during fight
    VoidzoneSalvation = 0x1E91D4, // R0.500, EventObj type, spawn during fight
    VoidzoneHypernova = 0x1E91C1, // R0.500, EventObj type, spawn during fight
    BahamutMoon = 0x1EA7E5, // R0.500, EventObj type, spawn during fight
    VoidzoneEarthShaker = 0x1E9663, // R0.500, EventObj type, spawn during fight

    //_Gen_Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    //_Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttackP1 = 9895, // Twintania->player, no cast, single-target
    Plummet = 9896, // Twintania->self, no cast, range 8+R ?-degree cone
    DeathSentence = 9897, // Twintania->player, 4.0s cast, single-target, tankbuster
    Twister = 9898, // Twintania->self, 2.0s cast, single-target, visual (spawn twister)
    TwisterAOE = 9899, // Helper->self, no cast, ???, kill target and knockback 50 players in radius 8-9
    Fireball = 9900, // Twintania->players, no cast, range 4 circle stack
    LiquidHell = 9901, // Twintania->location, no cast, range 6 circle voidzone
    Generate = 9902, // Twintania->self, 3.0s cast, single-target, visual (spawn hatch)
    Hatch = 9903, // Oviform->self, no cast, range 8 circle
    DeepHatch = 9904, // Helper->self, no cast, range 80 circle (hatch fail)

    AutoAttackP2 = 9908, // NaelDeusDarnus->player, no cast, single-target
    BahamutsClaw = 9909, // NaelDeusDarnus->player, no cast, single-target, 5-hit tankbuster
    Ravensbeak = 9910, // NaelDeusDarnus->player, 4.0s cast, single-target, tankbuster forcing swap
    Heavensfall = 9912, // Helper->self, no cast, range 80 circle, knockback 11 from center
    ThermionicBurst = 9913, // Helper->self, 3.0s cast, range 24+R 22.5-degree cone
    IronChariot = 9915, // NaelDeusDarnus->self, no cast, range 6+R circle aoe
    LunarDynamo = 9916, // NaelDeusDarnus->self, no cast, range ?-22 donut aoe
    ThermionicBeam = 9917, // NaelDeusDarnus->players, no cast, range 4 circle stack
    RavenDive = 9918, // NaelDeusDarnus->players, no cast, range 3 circle, knockback 50 on secondary targets
    Hypernova = 9919, // NaelDeusDarnus->location, no cast, range 5 circle voidzone spawn
    MeteorStream = 9920, // NaelGeminus->players, no cast, range 4 circle
    DalamudDive = 9921, // NaelDeusDarnus->location, no cast, range 5 circle tankbuster
    BahamutsFavor = 9922, // NaelDeusDarnus->self, 3.0s cast, single-target, visual (damage-up on self, spawn dragons)
    FireballP2 = 9925, // Firehorn->players, no cast, range 4 circle, applies firescorched on targets
    Iceball = 9926, // Iceclaw->player, no cast, single-target, applies icebitten on 1 player
    ChainLightning = 9927, // Thunderwing->self, no cast, ???, applies thunderstruck on 2 players
    ChainLightningAOE = 9928, // Helper->self, no cast, ???, paralysis on targets within radius 5 except main target
    Deathstorm = 9929, // TailOfDarkness->self, no cast, ???, applies dooms on 2-3 players
    WingsOfSalvation = 9930, // FangOfLight->location, 3.0s cast, range 4 circle aoe that leaves cleanse voidzone
    Cauterize1 = 9931, // Firehorn->self, 4.0s cast, range 48+R width 20 rect
    Cauterize2 = 9932, // Iceclaw->self, 4.0s cast, range 48+R width 20 rect
    Cauterize3 = 9933, // Thunderwing->self, 4.0s cast, range 48+R width 20 rect
    Cauterize4 = 9934, // TailOfDarkness->self, 4.0s cast, range 48+R width 20 rect
    Cauterize5 = 9935, // FangOfLight->self, 4.0s cast, range 48+R width 20 rect

    AutoAttackP3 = 9936, // BahamutPrime->player, no cast, single-target
    SeventhUmbralEra = 9937, // Helper->self, no cast, range 80 circle raidwide with ? falloff, knockback 11
    CalamitousFlame = 9938, // Helper->self, no cast, range 80 circle raidwide with ? falloff
    CalamitousBlaze = 9939, // Helper->self, no cast, range 80 circle raidwide with ? falloff, requires tank LB2+
    FlareBreath = 9940, // BahamutPrime->self, no cast, range 25+R ?-degree cone cleave
    Flatten = 9941, // BahamutPrime->player, 4.0s cast, single-target tankbuster
    Gigaflare = 9942, // BahamutPrime->self, 6.0s cast, range 80+R circle, raidwide
    QuickmarchTrio = 9954, // BahamutPrime->self, 4.0s cast, single-target, visual (trio 1 start)
    TwistingDive = 9906, // Twintania->self, 4.0s cast, range 60+R width 8 rect
    LunarDive = 9923, // NaelDeusDarnus->self, 4.0s cast, range 60+R width 8 rect
    MegaflareDive = 9953, // BahamutPrime->self, 4.0s cast, range 60+R width 12 rect
    MegaflareSpread = 9948, // Helper->players, no cast, range 5 circle spread
    MegaflarePuddle = 9949, // Helper->location, 3.0s cast, range 6 circle puddle
    MegaflareStack = 9950, // Helper->player, no cast, range ? enumeration stack
    EarthShaker = 9945, // BahamutPrime->self, no cast, single-target, visual (baited cones)
    EarthShakerAOE = 9946, // Helper->self, no cast, range 60+R 90-degree cone
    TempestWing = 9943, // BahamutPrime->self, no cast, tankbusters on tether targets
    TempestWingAOE = 9944, // Helper->self, no cast, knockback 50 in radius 5 around target
    BlackfireTrio = 9955, // BahamutPrime->self, 4.0s cast, single-target, visual (trio 2 start)
    MegaflareTower = 9951, // Helper->location, 8.0s cast, range 3 circle tower
    MegaflareStrike = 9952, // Helper->self, no cast, range 80 circle, tower fail
    FellruinTrio = 9956, // BahamutPrime->self, 4.0s cast, single-target, visual (trio 3 start)
    AethericProfusion = 9905, // Twintania->self, 5.0s cast, range 80+R circle, deadly raidwide unless in neurolink
    HeavensfallTrio = 9957, // BahamutPrime->self, 4.0s cast, single-target, visual (trio 4 start)
    HeavensfallVisual = 9911, // NaelDeusDarnus->self, 3.0s cast, single-target, visual (knockback)
    TenstrikeTrio = 9958, // BahamutPrime->self, 4.0s cast, single-target, visual (trio 5 start)
    GrandOctet = 9959, // BahamutPrime->self, 4.0s cast, single-target, visual (trio 6 start)

    BahamutsFavorP4 = 9960, // Helper->self, no cast, ???, visual (buff twin/nael with damage up)
    MegaflareRaidwide = 9914, // NaelDeusDarnus->self, 5.0s cast, range 80+R circle, raidwide
    TwinFury = 9907, // Twintania->self, 3.0s cast, single-target, enrage
    WhiteFury = 9924, // NaelDeusDarnus->self, 3.0s cast, single-target, enrage

    Teraflare = 9961, // BahamutPrime->self, no cast, ???, kills everyone during phase transition
    FlamesOfRebirth = 9970, // Phoenix->self, no cast, range 80+R circle, revives and buffs everyone during phase transition
    BecomeGolden = 9991, // BahamutPrime->location, no cast, single-target, visual (phase transition)
    MornAfah = 9964, // BahamutPrime->players, 6.0s cast, ???, stack
    AkhMorn = 9962, // BahamutPrime->players, 4.0s cast, range 4 circle, tankbuster first hit
    AkhMornAOE = 9963, // BahamutPrime->players, no cast, range 4 circle, tankbuster successive hits
    Exaflare = 9967, // BahamutPrime->self, 4.0s cast, single-target, visual (exaflares)
    ExaflareFirst = 9968, // Helper->self, 4.0s cast, range 6 circle
    ExaflareRest = 9969, // Helper->self, no cast, range 6 circle
    Enrage = 9965, // BahamutPrime->player, 10.0s cast, range 4 circle, enrage spread
    EnrageAOE = 9966, // BahamutPrime->player, no cast, range 4 circle, enrage spread second+ casts
}

public enum SID : uint
{
    Doom = 210, // none->player, extra=0x0
    Firescorched = 464, // Firehorn->player, extra=0x0
    Icebitten = 465, // Iceclaw->player, extra=0x0
    Thunderstruck = 466, // none->player, extra=0x0
    DownForTheCount = 783, // none->player, extra=0xEC7
}

public enum TetherID : uint
{
    Fireball = 5, // Firehorn->player
    TempestWing = 4, // player->BahamutPrime
}

public enum IconID : uint
{
    Fireball = 117, // player
    Generate = 118, // player
    Cauterize = 20, // player
    MegaflareStack = 39, // player
    Earthshaker = 40, // player
    MegaflareDive = 41, // player
    TwistingDive = 42, // player
    LunarDive = 119, // player
}
