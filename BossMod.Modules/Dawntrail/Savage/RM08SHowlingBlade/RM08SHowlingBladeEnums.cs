namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

public enum OID : uint
{
    Boss = 0x4727, // R5.005, x1
    Helper = 0x233C, // R0.500, x25 (spawn during fight), Helper type
    WolfOfWindDecay = 0x472A, // R1.500, x9
    GleamingFangCrossing = 0x472D, // R1.400, x2
    GleamingFangCharge = 0x472C, // R1.400, x8
    MoonlitShadow = 0x4729, // R5.005, x4
    Shadow = 0x4728, // R4.235, x5
    BossP2 = 0x472E, // R19.000, x1
    WolfOfWindUseless1 = 0x47B2, // R1.500, x0 (spawn during fight)
    WolfOfStoneUseless1 = 0x47B3, // R1.500, x0 (spawn during fight)
    FontOfWindAether = 0x4755, // R1.000-1.500, x0 (spawn during fight)
    FontOfEarthAether = 0x4756, // R1.000-1.500, x0 (spawn during fight)
    WolfOfWindTactical = 0x472B, // R1.500, x0 (spawn during fight)
    WolfOfStoneTactical = 0x4731, // R1.500, x0 (spawn during fight)
    WolfOfStoneWeal = 0x484B, // R1.500, x0 (spawn during fight)
    WolfOfWindRoaring = 0x485F, // R1.500, x0 (spawn during fight)
    WolfOfWindUseless2 = 0x486E, // R1.500, x0 (spawn during fight)
    WolfOfStoneUseless2 = 0x486F, // R1.500, x0 (spawn during fight)
    HowlingBladePart = 0x4733, // R0.000, x0 (spawn during fight), Part type
    GleamingFangBeam = 0x472F, // R3.000, x0 (spawn during fight)
    GleamingFangInout = 0x4730, // R3.000, x0 (spawn during fight)
    TwofoldVoidzone = 0x1EBD8F, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 42222, // Boss->player, no cast, single-target
    ExtraplanarPursuitVisual = 41946, // Boss->self, 1.6+2.4s cast, single-target
    ExtraplanarPursuit = 42831, // Helper->self, 4.0s cast, range 40 circle

    WindfangCards = 41885, // Boss->self, 6.0s cast, range 15 width 6 cross
    WindfangIntercards = 41886, // Boss->self, 6.0s cast, range 15 width 6 cross
    WindfangDonut = 41887, // Helper->self, 6.0s cast, range 8-20 donut
    WindfangProtean = 41888, // Helper->self, no cast, range 40 25-degree cone
    StonefangCards = 41889, // Boss->self, 6.0s cast, range 15 width 6 cross
    StonefangIntercards = 41890, // Boss->self, 6.0s cast, range 15 width 6 cross
    StonefangCircle = 41904, // Helper->self, 6.0s cast, range 9 circle
    StonefangProtean = 41905, // Helper->self, no cast, range 40 25(?)-degree cone

    EminentReignVisual1 = 43281, // Boss->self, 5.1s cast, single-target
    EminentReignVisual2 = 43282, // Boss->self, 5.1s cast, single-target
    RevolutionaryReignVisual1 = 43283, // Boss->self, 5.1s cast, single-target
    RevolutionaryReignVisual2 = 43284, // Boss->self, 5.1s cast, single-target
    EminentReignDash = 43297, // Boss->location, no cast, single-target
    RevolutionaryReignDash = 43298, // Boss->location, no cast, single-target
    WolvesReignCloneVisual1 = 43305, // Shadow->self, 6.4s cast, single-target
    WolvesReignCloneVisual2 = 43306, // Shadow->self, 6.5s cast, single-target
    WolvesReignCloneVisual3 = 43307, // Shadow->self, 6.6s cast, single-target
    WolvesReignClone1 = 43308, // Helper->self, 6.7s cast, range 6 circle
    WolvesReignClone2 = 43309, // Helper->self, 6.8s cast, range 6 circle
    WolvesReignClone3 = 43310, // Helper->self, 6.9s cast, range 6 circle
    EminentReignJump = 43312, // Helper->self, 7.0s cast, range 6 circle
    RevolutionaryReignJump = 43313, // Helper->self, 7.0s cast, range 6 circle
    WolvesReignRect1 = 43369, // Helper->self, 1.5s cast, range 28 width 10 rect
    WolvesReignRect2 = 43370, // Helper->self, 1.5s cast, range 28 width 10 rect
    WolvesReignRectJump1 = 41880, // Boss->location, 1.0+0.5s cast, single-target
    WolvesReignRectJump2 = 42927, // Boss->location, 1.0+0.5s cast, single-target
    WolvesReignRectVisual1 = 41881, // Boss->self, 1.0+0.5s cast, single-target
    WolvesReignRectVisual2 = 41882, // Boss->self, 1.0+0.5s cast, single-target
    WolvesReignCone = 42929, // Helper->self, 1.5s cast, range 40 120-degree cone
    WolvesReignCircle = 42930, // Helper->self, 1.5s cast, range 14 circle
    SovereignScar = 41884, // Helper->self, no cast, range 40 30-degree cone
    ReignsEnd = 41883, // Helper->self, no cast, range 40 60-degree cone

    MillennialDecay = 41906, // Boss->self, 5.0s cast, range 40 circle

    AeroIII = 41912, // Helper->self, 5.0s cast, range 40 circle
    AeroIIIVisual = 41911, // Boss->self, 5.0s cast, single-target

    BreathOfDecay = 41908, // WolfOfWindDecay->self, 8.0s cast, range 40 width 8 rect
    Gust = 41907, // Helper->players, no cast, range 5 circle
    ProwlingGale1 = 41910, // Helper->location, 7.0s cast, range 2 circle
    WindsOfDecay = 41909, // WolfOfWindDecay->self, no cast, range 40 30(?)-degree cone
    GreatWhirlwind = 41957, // Helper->self, no cast, range 40 circle, tower explosion

    TrackingTremorsVisual = 41913, // Boss->self, 5.0s cast, single-target
    TrackingTremors = 41915, // Helper->players, no cast, range 6 circle
    GreatDivide = 41944, // Boss->self/players, 5.0s cast, range 60 width 6 rect

    TerrestrialTitansVisual = 41924, // Boss->self, 4.0s cast, single-target
    TerrestrialTitans = 41925, // Helper->self, 4.0s cast, range 5 circle
    TitanicPursuitVisual = 41927, // Boss->self, 1.6+2.4s cast, range 40 circle
    TitanicPursuit = 42833, // Helper->self, 4.0s cast, range 40 circle
    Towerfall = 41926, // Helper->self, 8.0s cast, range 30 width 10 rect
    FangedCrossing = 41943, // GleamingFangCrossing->self, 4.0s cast, range 21 width 7 cross

    BareFangs1 = 42187, // Boss->self, no cast, single-target
    BareFangs2 = 42188, // Boss->self, no cast, single-target
    Unk1 = 42886, // Boss->self, no cast, single-target, cast after certain reign mechanics but not all, probably some visual
    Unk2 = 41871, // Boss->location, no cast, single-target, dash?
    TacticalPackBossDisappear = 41929, // Boss->self, no cast, single-target
    P2TransitionStun = 43053, // Helper->self, no cast, range 60 circle

    TacticalPack = 41928, // Boss->self, 3.0s cast, single-target

    AutoAttackWind = 42225, // WolfOfWindTactical->player, no cast, single-target
    AutoAttackStone = 42226, // WolfOfStoneTactical->player, no cast, single-target
    HowlingHavocWind = 41947, // WolfOfWindTactical->self, 5.0s cast, single-target
    HowlingHavocStone = 41948, // WolfOfStoneTactical->self, 5.0s cast, single-target
    HowlingHavoc = 41949, // Helper->self, 5.0s cast, range 40 circle
    PackPredation = 41932, // WolfOfWindTactical/WolfOfStoneTactical->self, 5.0s cast, single-target

    AlphaWind = 41933, // Helper->self, no cast, range 40 ?-degree cone
    AlphaStone = 41954, // Helper->self, no cast, range 40 ?-degree cone
    StalkingWind = 41935, // Helper->self, no cast, range 40 width 6 rect
    StalkingStone = 41956, // Helper->self, no cast, range 40 width 6 rect

    WindSurge = 41965, // FontOfWindAether->self, no cast, ???
    SandSurge = 41966, // FontOfEarthAether->self, no cast, ???
    GaleSurge = 41967, // FontOfWindAether->self, no cast, range 40 circle
    StoneSurge = 41968, // FontOfEarthAether->self, no cast, range 40 circle
    WindSurgeFinal = 43137, // FontOfWindAether->self, no cast, ???
    SandSurgeFinal = 43138, // FontOfEarthAether->self, no cast, ???
    WindSurgeFinalEarly = 43519, // FontOfWindAether->self, no cast, triggers if add is already dead, was an enrage before 7.2
    SandSurgeFinalEarly = 43520, // FontOfEarthAether->self, no cast, see above
    ForlornWind = 41936, // WolfOfWindTactical->self, 8.0s cast, single-target, enrage
    ForlornWindVisual = 41937, // WolfOfWindTactical->self, no cast, single-target
    ForlornWindEnrage = 41938, // Helper->self, no cast, range 40 circle
    ForlornStoneCast = 41939, // WolfOfStoneTactical->self, 8.0s cast, single-target, enrage
    ForlornStoneVisual = 41940, // WolfOfStoneTactical->self, no cast, single-target
    ForlornStoneEnrage = 41941, // Helper->self, no cast, range 40 circle

    RavenousSaber1 = 42825, // Helper->self, 3.4s cast, range 40 circle
    RavenousSaber2 = 42826, // Helper->self, 3.6s cast, range 40 circle
    RavenousSaber3 = 42827, // Helper->self, 3.9s cast, range 40 circle
    RavenousSaber4 = 43518, // Helper->self, 6.0s cast, range 40 circle
    RavenousSaber5 = 41931, // Helper->self, 7.3s cast, range 40 circle

    TerrestrialRage = 41918, // Boss->self, 3.0s cast, single-target
    FangedCharge = 41942, // GleamingFangCharge->self, 4.0s cast, range 30 width 6 rect
    SuspendedStone = 41919, // Helper->players, no cast, range 6 circle
    Heavensearth = 41920, // Helper->players, no cast, range 6 circle
    ShadowchaseCast = 41916, // Boss->self, 3.0s cast, single-target
    Shadowchase = 41917, // Shadow->self, 2.0s cast, range 40 width 8 rect
    RoaringWindCast = 42889, // WolfOfWindRoaring->self, 2.5s cast, single-target
    RoaringWind = 42890, // Helper->self, 2.5s cast, range 40 width 8 rect

    WealOfStone1Cast = 42893, // WolfOfStoneWeal->self, 2.5s cast, single-target
    WealOfStone1 = 42894, // Helper->self, 2.5s cast, range 40 width 6 rect
    WealOfStone2Cast = 42897, // WolfOfStoneWeal->self, 2.5s cast, single-target
    WealOfStone2 = 42898, // Helper->self, 2.5s cast, range 40 width 6 rect
    BeckonMoonlight = 41921, // Boss->self, 3.0s cast, single-target
    MoonbeamPreLeft = 41953, // MoonlitShadow->location, no cast, single-target
    MoonbeamPreRight = 41952, // MoonlitShadow->location, no cast, single-target
    MoonbeamsBiteLeft = 41923, // MoonlitShadow->self, 9.0s cast, range 40 width 20 rect
    MoonbeamsBiteRight = 41922, // MoonlitShadow->self, 9.0s cast, range 40 width 20 rect

    ExtraplanarFeast = 41969, // Boss->self, 4.0s cast, range 40 circle, P1 enrage

    P2AutoAttack = 42227, // BossP2->player, no cast, single-target
    P2AutoAttackOfftank = 42228, // HowlingBladePart->player, no cast, single-target

    QuakeIIICast = 42074, // BossP2->self, 5.0s cast, single-target
    QuakeIIIStack = 42075, // Helper->self, no cast, hits a specific platform (circle vfx)
    GleamingBeam = 42078, // GleamingFangBeam->self, 4.0s cast, range 31 width 8 rect
    HerosBlow1Cast = 42079, // BossP2->self, 6.0+1.0s cast, single-target
    HerosBlow1 = 42080, // Helper->self, 7.0s cast, range 40 180-degree cone
    HerosBlow2Cast = 42081, // BossP2->self, 6.0+1.0s cast, single-target
    HerosBlow2 = 42082, // Helper->self, 7.0s cast, range 40 180-degree cone
    FangedMaw = 42083, // GleamingFangInout->self, 7.0s cast, range 22 circle
    FangedPerimeter = 42084, // GleamingFangInout->self, 7.0s cast, range 15-30 donut

    GeotemporalBlast = 42089, // Helper->self, no cast, range 6 circle
    AerotemporalBlast = 42088, // Helper->self, no cast, range 16 circle
    HuntersHarvestCast = 42092, // BossP2->self, no cast, single-target
    HuntersHarvest = 42093, // Helper->self, 1.0s cast, range 40 200-degree cone
    UltraviolentRayCast = 42076, // BossP2->self, 6.0s cast, single-target
    UltraviolentRay = 42077, // Helper->self, no cast, hits a specific platform (rect vfx)
    TwinbiteCast = 42189, // BossP2->self, 7.0s cast, single-target
    Twinbite = 42190, // Helper->self, no cast, tankbuster, hits a specific platform (circle vfx)
    MooncleaverCast = 42085, // BossP2->self, 4.0+1.0s cast, single-target
    Mooncleaver = 42086, // Helper->self, 5.0s cast, range 8 circle
    ElementalPurge = 42087, // BossP2->self, 5.0s cast, single-target

    ProwlingGale2Cast = 42094, // BossP2->self, 3.0s cast, single-target
    ProwlingGale2 = 42095, // Helper->self, 8.0s cast, range 2 circle

    RiseOfTheHowlingWind = 43050, // BossP2->self, 7.0s cast, single-target
    TwofoldTempestCast = 42097, // BossP2->self, 7.0s cast, single-target
    TwofoldTempestStack = 42098, // Helper->players, no cast, range 6 circle
    TwofoldTempestLine = 42099, // Helper->self, no cast, hits a specific platform (rect vfx)
    TwofoldTempestVisual = 42100, // BossP2->self, no cast, single-target
    WindSurgeVoidzone = 43153, // Helper->self, no cast, range 4.5-9 circle

    BareFangs3 = 42101, // BossP2->self, 4.0s cast, single-target
    GleamingBarrage = 42102, // GleamingFangBeam->self, 2.8s cast, range 31 width 8 rect
    ChampionsCircuitCW = 42103, // BossP2->self, 7.3+0.7s cast, single-target
    ChampionsCircuitCCW = 42104, // BossP2->self, 7.3+0.7s cast, single-target
    ChampionsCircuitCWVisual = 42145, // BossP2->self, no cast, single-target
    ChampionsCircuitCCWVisual = 42146, // BossP2->self, no cast, single-target

    ChampionsCircuitRectFirst = 42105, // Helper->self, 8.0s cast, range 30 width 12 rect
    ChampionsCircuitDonutSmallFirst = 42106, // Helper->self, 8.0s cast, range 4-13 donut
    ChampionsCircuitDonutLarge1First = 42107, // Helper->self, 8.0s cast, range 15.85-28 donut, drawn as 60 degree sector
    ChampionsCircuitConeFirst = 42108, // Helper->self, 8.0s cast, range 22 60-degree cone
    ChampionsCircuitDonutLarge2First = 42109, // Helper->self, 8.0s cast, range 15.85-28 donut, drawn as 60 degree sector
    ChampionsCircuitRectRest = 42110, // Helper->self, 0.7s cast, range 30 width 12 rect
    ChampionsCircuitDonutSmallRest = 42111, // Helper->self, 0.7s cast, range 4-13 donut
    ChampionsCircuitDonutLarge1Rest = 42112, // Helper->self, 0.7s cast, range 15.85-28 donut
    ChampionsCircuitConeRest = 42113, // Helper->self, 0.7s cast, range 22 60-degree cone
    ChampionsCircuitDonutLarge2Rest = 42114, // Helper->self, 0.7s cast, range 15.85-28 donut

    RiseOfTheHuntersBlade = 43052, // BossP2->self, 7.0s cast, single-target
    LoneWolfsLament = 42115, // BossP2->self, 3.0s cast, single-target
    UnmitigatedExplosion = 42116, // Helper->player, no cast, single-target
    ProwlingGale3Cast = 42117, // BossP2->self, 3.0s cast, single-target
    ProwlingGaleOneTower = 42118, // Helper->self, 8.0s cast, range 2 circle
    ProwlingGaleTwoTower = 42119, // Helper->self, 8.0s cast, range 2 circle
    ProwlingGaleThreeTower = 42120, // Helper->self, 8.0s cast, range 2 circle
    WolfsLamentGreatWhirlwind = 42121, // Helper->self, no cast, range 60 circle, tower failure

    MooncleaverEnrageCast = 42828, // BossP2->self, 3.0+1.0s cast, single-target
    MooncleaverEnrage = 42829, // Helper->self, 4.0s cast, range 8 circle
    HowlingEightFirstCast = 43522, // BossP2->location, 8.0+1.0s cast, single-target
    HowlingEightFirst1 = 43523, // Helper->self, 9.1s cast, range 8 circle
    HowlingEightFirst2 = 43524, // Helper->self, 10.1s cast, range 8 circle
    HowlingEightFirst3 = 43525, // Helper->self, 11.0s cast, range 8 circle
    HowlingEightFirst4 = 43526, // Helper->self, 11.8s cast, range 8 circle
    HowlingEightFirst5 = 43527, // Helper->self, 12.4s cast, range 8 circle
    HowlingEightFirst6 = 43528, // Helper->self, 12.9s cast, range 8 circle
    HowlingEightFirst7 = 43529, // Helper->self, 13.3s cast, range 8 circle
    HowlingEightFirst8 = 43530, // Helper->self, 15.1s cast, range 8 circle
    HowlingEightRestCast = 42132, // BossP2->location, 5.0+1.0s cast, single-target
    HowlingEightRest1 = 42133, // Helper->self, 6.1s cast, range 8 circle
    HowlingEightRest2 = 42134, // Helper->self, 7.1s cast, range 8 circle
    HowlingEightRest3 = 42135, // Helper->self, 8.0s cast, range 8 circle
    HowlingEightRest4 = 42136, // Helper->self, 8.8s cast, range 8 circle
    HowlingEightRest5 = 42137, // Helper->self, 9.4s cast, range 8 circle
    HowlingEightRest6 = 42138, // Helper->self, 9.9s cast, range 8 circle
    HowlingEightRest7 = 42139, // Helper->self, 10.3s cast, range 8 circle
    HowlingEightRest8 = 42140, // Helper->self, 12.1s cast, range 8 circle
    HowlingAgony = 43140, // Helper->self, no cast, range 100 circle, howling eight tower failure

    StarcleaverVisual = 42141, // BossP2->self, 10.0s cast, single-target
    Starcleaver = 42142, // Helper->self, 11.0s cast, range 8 circle, enrage
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper/WolfOfWindDecay/FontOfWindAether/FontOfEarthAether->player, extra=0x0
    PhysicalVulnerabilityUp = 2940, // Helper->player, extra=0x0
    Unk1 = 4465, // none->Shadow, extra=0x32
    Unk2 = 2234, // none->FontOfEarthAether/FontOfWindAether, extra=0x3E
    Bind = 2518, // none->player, extra=0x0
    Windpack = 4389, // none->FontOfWindAether/player, extra=0x0
    Stonepack = 4390, // none->FontOfEarthAether/player, extra=0x0
    EarthborneEnd = 4391, // none->player, extra=0x0
    WindborneEnd = 4392, // none->player, extra=0x0
    HungryForVictory = 4517, // none->Boss, extra=0x0
    DownForTheCount = 2408, // Helper->player, extra=0xEC7
    InEvent = 2999, // none->player, extra=0x0
    TerrestrialChains = 4393, // none->player, extra=0x0
    ImmobileSuit = 2578, // none->player, extra=0x0
    PatienceOfStone = 4395, // none->player, extra=0x0
    PatienceOfWind = 4394, // none->player, extra=0x0
    VulnerabilityUp = 3361, // Helper->player, extra=0x0
    LamentOfTheClose = 4396, // none->player, extra=0x0
    LamentOfTheDistant = 4397, // none->player, extra=0x0
    DamageUp = 2550, // BossP2->BossP2, extra=0x1/0x2/0x3/0x4
}

public enum IconID : uint
{
    Gust = 376, // player->self
    MultiStack = 316, // player->self
    TankShared = 598, // player->self
    Target = 23, // player->self
    Divebomb = 14, // player->self
    Stack = 93, // player->self
    Spread = 139, // player->self
    Unk501 = 501, // BossP2->self
}

public enum TetherID : uint
{
    Generic = 1, // WolfOfWindDecay->player
    Danger = 57, // WolfOfWindDecay->player
    Stone = 335, // player->WolfOfStoneTactical
    Wind = 336, // player->WolfOfWindTactical
    GenericPassable = 84, // BossP2->player
    LamentClose = 317, // player->player
    LamentDistant = 318, // player->player
}
