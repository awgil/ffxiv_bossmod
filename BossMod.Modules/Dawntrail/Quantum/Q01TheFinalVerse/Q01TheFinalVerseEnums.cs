namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

public enum OID : uint
{
    Boss = 0x48EE, // R28.500, x0-1
    Helper = 0x233C, // R0.500, x7-47, Helper type
    DevouredEater = 0x48EF, // R15.000, x0-1, Part type
    EminentGrief = 0x486C, // R1.000, x1-2
    VodorigaMinion = 0x48F0, // R1.200, x0 (spawn during fight)
    ArcaneFont = 0x48F4, // R2.000, x0 (spawn during fight)
    ScourgingBlaze = 0x1EBE70,
    Flameborn = 0x48F3, // R2.600-5.200, x0 (spawn during fight)

    Unk = 0x48F2, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttackVisual = 44135, // Boss->self, no cast, single-target
    EaterAttackVisual = 44136, // DevouredEater->self, no cast, single-target
    AutoAttack1 = 44137, // Helper->player, 0.8s cast, single-target
    AutoAttack2 = 44820, // EminentGrief->player, 0.5s cast, single-target
    AutoAttack3 = 44814, // Helper->player, 0.8s cast, single-target
    AutoAttack4 = 44802, // EminentGrief->player, 0.5s cast, single-target

    CrystalAppear = 44115, // Helper->location, no cast, single-target

    ScourgingBlazeHorizontalFirst = 44797, // Boss->self, 3.0s cast, single-target
    ScourgingBlazeVerticalFirst = 44798, // Boss->self, 3.0s cast, single-target
    ScourgingBlazeHorizontalSecond = 44799, // Boss->self, no cast, single-target
    ScourgingBlazeVerticalSecond = 44800, // Boss->self, no cast, single-target

    BoundsOfSinBossCast = 44120, // DevouredEater->self, 3.3+0.7s cast, single-target
    BoundsOfSinBind = 44121, // Helper->self, 4.0s cast, range 40 circle
    BoundsOfSinIcicleDrop = 44122, // Helper->self, 3.0s cast, range 3 circle
    BoundsOfSinJailInside = 44123, // Helper->self, no cast, range 8 circle
    BoundsOfSinJailOutside = 44124, // Helper->self, no cast, range 8-30 donut

    BigBurstTower1 = 44141, // Helper->self, no cast, range 60 circle, tower failure?
    NeutralizeExplosion = 44142, // Helper->player, no cast, single-target, light/dark failure
    BigBurstTower2 = 44143, // Helper->self, no cast, range 60 circle, tower failure?
    AbyssalSun = 44139, // Helper->self, no cast, range 30 circle, raidwide after towers, applies bleed

    ScourgingBlazeFirst = 44118, // Helper->location, 7.0s cast, range 5 circle
    ScourgingBlazeRest = 44119, // Helper->location, no cast, range 5 circle

    ChainsOfCondemnationCastFast = 44099, // Boss->location, 4.3+0.7s cast, single-target
    ChainsOfCondemnationFast = 44100, // Helper->location, 5.0s cast, range 30 circle, move-only pyretic
    ChainsOfCondemnationCastSlow = 44106, // Boss->location, 7.3+0.7s cast, single-target
    ChainsOfCondemnationSlow = 44107, // Helper->location, 8.0s cast, range 30 circle, move-only pyretic

    BladeOfFirstLightInsideFast = 44102, // DevouredEater->self, 4.2+0.8s cast, single-target
    BladeOfFirstLightOutsideFast = 44103, // DevouredEater->self, 4.2+0.8s cast, single-target
    BladeOfFirstLightFast = 44104, // Helper->self, 5.0s cast, range 30 width 15 rect

    BladeOfFirstLightInsideSlow = 44108, // DevouredEater->self, 7.2+0.8s cast, single-target
    BladeOfFirstLightOutsideSlow = 44109, // DevouredEater->self, 7.2+0.8s cast, single-target
    BladeOfFirstLightSlow = 44110, // Helper->self, 8.0s cast, range 30 width 15 rect

    BallOfFireCastFast = 44097, // Boss->self, 5.0s cast, single-target
    BallOfFireCastSlow = 44105, // Boss->self, 8.0s cast, single-target

    BallOfFirePuddle = 44098, // Helper->location, 2.1s cast, range 6 circle

    SearingChainsCross = 44144, // Helper->self, no cast, range 50 width 6 cross

    SpinelashVisual1 = 44125, // Boss->self, 2.0s cast, single-target
    SpinelashVisual2 = 44126, // Boss->self, 2.2+0.8s cast, single-target
    Spinelash = 45119, // Helper->self, 3.0s cast, range 60 width 8 rect

    VodorigaAuto = 45197, // VodorigaMinion->player, no cast, single-target
    BloodyClaw = 45116, // VodorigaMinion->player, no cast, single-target
    TerrorEye = 45117, // VodorigaMinion->location, 3.0s cast, range 6 circle

    ShacklesOfGreaterSanctity = 44801, // DevouredEater->self, 3.0s cast, single-target
    ShacklesOfSanctityHealer = 44147, // Helper->player, no cast, single-target
    ShacklesOfSanctityDPS = 44148, // Helper->player, no cast, single-target
    ShackleHealerExplosion = 44149, // Helper->players, no cast, range 21 circle, triggered on healing spell
    ShackleDPSExplosion = 44150, // Helper->players, no cast, range 8 circle, triggered on ability use

    HellishEarthCast = 44151, // Boss->location, 5.0+1.0s cast, single-target
    HellishEarthPull = 44152, // Helper->self, 6.0s cast, distance 10 attract on non-tethered players
    HellishEarthPullTether = 44153, // Helper->self, 6.0s cast, distance 60 attract on tethered (furthest) player
    HellishTormentTowerFailure = 44154, // Helper->self, no cast, range 50 circle

    Eruption = 44156, // Helper->location, 3.0s cast, range 6 circle

    ManifoldLashingsCast1 = 44157, // Boss->self, 5.0+1.3s cast, single-target
    ManifoldLashingsCast2 = 44158, // Boss->self, 5.0+1.3s cast, single-target
    ManifoldLashingsTower = 44159, // Helper->self, 6.3s cast, range 2 circle
    ManifoldLashingsTowerRepeat = 44160, // Helper->self, no cast, range 2 circle
    ManifoldLashingsTail = 44161, // Helper->self, 2.3s cast, range 42 width 9 rect

    BurningChains = 44145, // Helper->self, no cast, ???
    UnholyDarknessVisual = 44163, // DevouredEater->self, 6.0+0.7s cast, single-target
    UnholyDarkness = 44164, // Helper->self, 6.7s cast, range 30 circle, bleed raidwide

    CrimeAndPunishmentCast = 44165, // DevouredEater->self, 6.0+0.7s cast, single-target
    CrimeAndPunishment = 44166, // Helper->player, no cast, single-target, apply Sin Bearer
    Explosion = 44167, // Helper->players, no cast, range 4 circle, triggers on Sin Bearer expiration
    SinBurst = 44168, // Helper->player, no cast, range 60 circle, triggered on failed Sin Bearer pass

    DrainAetherLightFast = 44129, // Boss->self, 7.0s cast, range 50 width 50 rect
    DrainAetherLightSlow = 44130, // Boss->self, 12.0s cast, range 50 width 50 rect
    DrainAetherDarkFastCast = 44131, // DevouredEater->self, 6.0+1.0s cast, single-target
    DrainAetherDarkFast = 44132, // EminentGrief->self, 7.0s cast, range 50 width 50 rect
    DrainAetherDarkSlowCast = 44133, // DevouredEater->self, 11.0+1.0s cast, single-target
    DrainAetherDarkSlow = 44134, // EminentGrief->self, 12.0s cast, range 50 width 50 rect

    FeveredFlame = 44170, // Boss->self, 4.0s cast, single-target
    SelfDestruct = 44171, // 48F3->self, 2.0s cast, range 60 circle
    Fuse = 44172, // 48F3->48F3, no cast, single-target

    UnholyDarknessEnrageCast = 44175, // DevouredEater->self, 9.0+0.7s cast, single-target
    UnholyDarknessEnrage = 44176, // Helper->self, 9.7s cast, range 30 circle
    HellishEarthEnrage = 44174, // Boss->location, 27.0s cast, range 60 circle

    Unk1 = 44173, // Helper->self, no cast, ???
    Unk2 = 44127, // Boss->self, no cast, single-target
    UnkExplosion = 44140, // Helper->player, no cast, single-target
    UnkHellishTorment = 44155, // Helper->player, no cast, single-target
    UnkBigBurst = 44162, // Helper->self, no cast, range 80 circle
}

public enum SID : uint
{
    Bind = 4510, // Helper->player, extra=0x0
    DarkVengeance = 4559, // none->player, extra=0x0
    LightVengeance = 4560, // none->player, extra=0x0
    ChainsOfCondemnation = 4562, // Helper->player, extra=0x0
    SearingChains = 4563, // none->player, extra=0x0
    FireResistanceDownII = 2937, // Helper->player, extra=0x0
    Burns = 2082, // Helper->player, extra=0x0

    // param changes when adds merge, 0x399 is base size
    UnkAura = 3913, // Boss->Boss/48F3, extra=0x3C6/0x399/0x39A/0x39B

    Suppuration = 4512, // Helper->player, extra=0x4/0x2/0x1
    ShackledAbilities = 4565, // none->player, extra=0x0
    ShackledHealing = 4564, // none->player, extra=0x0
    HellishEarth = 4566, // none->player, extra=0x0
    HPBoost = 586, // none->ArcaneFont, extra=0x8
    Petrification = 1, // none->player, extra=0x0
    Poison = 3462, // Helper->player, extra=0x1/0x2/0x3
    BleedingTower = 2922, // none->player, extra=0x0
    Bleeding = 2951, // Helper->player, extra=0x0
    DamageDown = 3304, // Helper->player, extra=0x1/0x2/0x3/0x4
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Bleeding3 = 2088, // Helper->player, extra=0x0
    SinBearer = 4567, // Helper->player, extra=1-16
    Doom = 4594, // none->player, extra=0x0
    WithoutSin = 4569, // none->player, extra=0x0
    SumOfAllSins = 4568, // none->player, extra=0xEC7
    Unk1 = 4685, // none->player, extra=0x0
}

public enum IconID : uint
{
    RingLight = 77, // player->self
    RingDark = 78, // player->self
    SearingChains = 97, // player->self
    Target = 23, // player->self
    LineStack = 527, // Boss->player
}

public enum TetherID : uint
{
    HellishEarth = 5, // Boss->player
    SearingChains = 9, // player->player
}
