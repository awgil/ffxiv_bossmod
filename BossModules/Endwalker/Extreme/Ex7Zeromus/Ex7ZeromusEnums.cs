namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

public enum OID : uint
{
    Boss = 0x40AD, // R20.000, x1
    Comet = 0x40AE, // R2.250, x22
    ToxicBubble = 0x40AF, // R1.700, spawn during fight
    FlowOfTheAbyss = 0x4110, // R1.000, x1
    Helper = 0x233C, // R0.500, x30, 523 type
    FlareTower = 0x1EB94E, // R0.500, EventObj type, spawn during fight
    FlareRay = 0x1EB94F, // R0.500, EventObj type, spawn during fight
    BlackHole = 0x1EB94C, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 35913, // Boss->player, no cast, single-target

    AbyssalNox = 35647, // Boss->self, 5.0s cast, range 60 circle, 1hp/doom with 5s application delay
    AbyssalEchoesVisualCardinal = 35648, // Helper->self, 9.0s cast, single-target, visual (line in cardinal direction)
    AbyssalEchoesVisualIntercardinal = 35649, // Helper->self, 9.0s cast, single-target, visual (line in diagonal direction)
    AbyssalEchoesVisualCenter = 36139, // Helper->self, 9.0s cast, single-target, visual (central spot)
    AbyssalEchoes = 35650, // Helper->self, 16.0s cast, range 12 circle

    SableThread = 35640, // Boss->self, 5.0s cast, single-target, visual (wild charge)
    SableThreadTarget = 35567, // Helper->player, no cast, single-target, visual (target selection)
    SableThreadAOE = 35643, // Helper->self, no cast, range 60 width 12 rect wild charge
    SableThreadVisualHitIntermediate = 35641, // Boss->self, no cast, single-target, visual (before intermediate hits)
    SableThreadVisualHitLast = 35642, // Boss->self, no cast, single-target, visual (before last hit)

    DarkMatter = 35715, // Boss->self, 4.0s cast, single-target, visual (tankbusters)
    DarkMatterAOE = 35716, // Helper->player, no cast, range 8 circle tankbuster

    VisceralWhirlR = 35651, // Boss->self, 8.0s cast, single-target, visual (two of lines with right safespot)
    VisceralWhirlRAOE1 = 35652, // Helper->self, 8.8s cast, range 29 width 28 rect
    VisceralWhirlRAOE2 = 35653, // Helper->self, 8.8s cast, range 60 width 28 rect
    VisceralWhirlL = 35654, // Boss->self, 8.0s cast, single-target, visual  (two of lines with left safespot)
    VisceralWhirlLAOE1 = 35655, // Helper->self, 8.8s cast, range 29 width 28 rect
    VisceralWhirlLAOE2 = 35656, // Helper->self, 8.8s cast, range 60 width 28 rect
    MiasmicBlast = 35657, // Helper->self, 8.0s cast, range 60 width 10 cross
    MiasmicBlastVisual = 36088, // Helper->self, 8.0s cast, range 60 width 10 cross, visual (animation)

    Flare = 35677, // Boss->self, 7.0s cast, single-target, visual (towers)
    FlareAOE = 35680, // Helper->self, 8.0s cast, range 5 circle tower
    FlareScald = 35766, // Helper->self, no cast, range 5 circle (tower aftereffect, damage + vuln)
    FlareKill = 35682, // Helper->self, 5.0s cast, range 5 circle (tower aftereffect, kill)
    ProminenceSpine = 35683, // Helper->self, 5.0s cast, range 60 width 10 rect (tower aftereffect, ray)

    VoidBio = 35686, // Boss->self, 5.0s cast, single-target, visual (spawn toxic bubbles)
    VoidBioBurst = 35687, // ToxicBubble->player, no cast, single-target (bubble touch, damage + vuln + dot)

    BigBang = 35660, // Boss->self, 10.0s cast, range 60 circle, raidwide with dot
    BigBangAOE = 35662, // Helper->location, 6.0s cast, range 5 circle puddle
    BigBangSpread = 35806, // Helper->player, 5.0s cast, range 5 circle spread
    BigCrunch = 35661, // Boss->self, 10.0s cast, range 60 circle, raidwide with dot
    BigCrunchAOE = 36145, // Helper->location, 6.0s cast, range 5 circle puddle
    BigCrunchSpread = 36146, // Helper->player, 5.0s cast, range 5 circle spread

    VoidMeteor = 35671, // Boss->self, 4.8s cast, single-target, visual (initial meteors)
    MeteorImpactProximity = 35676, // Comet->self, 6.0s cast, range 40 circle with ? falloff
    MeteorImpact = 35670, // Boss->self, 11.0s cast, single-target, visual (tethered meteors)
    MeteorImpactChargeNormal = 35672, // Comet->location, no cast, width 4 rect charge (non-clipping, fatal damage if distance is less than ~25, otherwise small damage)
    MeteorImpactChargeClipping = 35673, // Comet->location, no cast, width 4 rect charge (clipping, fatal damage followed by raidwide and vuln)
    MeteorImpactAppearNormal = 36025, // Helper->location, no cast, range 2 circle (???)
    MeteorImpactAppearClipping = 35674, // Helper->location, no cast, range 60 circle, raidwide with vuln (followed by wipe)
    MeteorImpactMassiveExplosion = 35675, // Comet->self, no cast, range 60 circle (wipe if meteor is clipped)
    MeteorImpactExplosion = 36148, // Comet->self, 5.0s cast, range 10 circle

    DarkBinds = 35669, // Helper->self, no cast, damage + vuln if chains were not broken in 5s
    DarkDivides = 35666, // Helper->players, no cast, range 5 circle spread
    ForkedLightning = 35668, // Helper->self, no cast, range 5 circle spread
    DarkBeckons = 36154, // Helper->players, no cast, range 6 circle 4-man stack
    // AccelerationBomb = 35663 player->player

    BlackHole = 35689, // Boss->self, 5.0s cast, single-target, visual (placeable black hole + laser)
    FracturedEventideWE = 35644, // Boss->self, 10.0s cast, single-target, visual (laser W to E)
    FracturedEventideEW = 35645, // Boss->self, 10.0s cast, single-target, visual (laser E to W)
    FracturedEventideAOEFirst = 35646, // Helper->self, 10.5s cast, range 60 width 8 rect
    FracturedEventideAOERest = 35762, // Helper->self, no cast, range 60 width 8 rect

    SparkingFlare = 35678, // Boss->self, 7.0s cast, single-target, visual (towers + spreads)
    BrandingFlare = 35679, // Boss->self, 7.0s cast, single-target, visual (towers + pairs)
    SparkingFlareAOE = 35684, // Helper->player, 10.0s cast, range 4 circle spread
    BrandingFlareAOE = 35685, // Helper->players, 10.0s cast, range 4 circle 2-man stack
    Nox = 36136, // Boss->self, 4.0s cast, single-target, visual (chasing aoe)
    NoxAOEFirst = 36138, // Helper->self, 5.0s cast, range 10 circle
    NoxAOERest = 36132, // Helper->self, no cast, range 10 circle

    RendTheRift = 35853, // Boss->self, 6.0s cast, range 60 circle, raidwide
    NostalgiaDimensionalSurge = 35710, // Helper->location, 4.0s cast, range 5 circle puddle
    Nostalgia = 35691, // Boss->self, 5.0s cast, single-target, visual (multiple raidwides)
    NostalgiaBury1 = 35693, // Helper->self, 0.7s cast, range 60 circle
    NostalgiaBury2 = 35694, // Helper->self, 1.7s cast, range 60 circle
    NostalgiaBury3 = 35695, // Helper->self, 2.7s cast, range 60 circle
    NostalgiaBury4 = 35696, // Helper->self, 3.7s cast, range 60 circle
    NostalgiaRoar1 = 35697, // Helper->self, 5.7s cast, range 60 circle
    NostalgiaRoar2 = 35698, // Helper->self, 6.7s cast, range 60 circle
    NostalgiaPrimalRoar = 35699, // Helper->self, 9.7s cast, range 60 circle

    FlowOfTheAbyss = 36090, // Boss->self, 7.0s cast, single-target, visual (spread/stack + unsafe line)
    FlowOfTheAbyssDimensionalSurge = 35714, // Helper->self, 9.0s cast, range 60 width 14 rect
    AkhRhaiStart = 35700, // Helper->self, no cast, single-target
    AkhRhaiAOE = 35701, // Helper->self, no cast, range 5 circle spread
    UmbralRays = 35702, // Helper->players, 5.0s cast, range 6 circle, 8-man stack
    UmbralPrism = 35703, // Helper->players, 5.0s cast, range 5 circle, 2-man enumeration stack
    ChasmicNails = 35704, // Boss->self, 7.0s cast, single-target, visual (pizzas)
    ChasmicNailsAOE1 = 35705, // Helper->self, 7.7s cast, range 60 40-degree cone
    ChasmicNailsAOE2 = 35706, // Helper->self, 8.4s cast, range 60 40-degree cone
    ChasmicNailsAOE3 = 35707, // Helper->self, 9.1s cast, range 60 40-degree cone
    ChasmicNailsAOE4 = 35708, // Helper->self, 9.8s cast, range 60 40-degree cone
    ChasmicNailsAOE5 = 35709, // Helper->self, 10.5s cast, range 60 40-degree cone
    ChasmicNailsVisual1 = 35623, // Helper->self, 1.5s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual2 = 35624, // Helper->self, 3.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual3 = 35625, // Helper->self, 4.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual4 = 35626, // Helper->self, 5.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual5 = 35627, // Helper->self, 6.0s cast, range 60 40-degree cone, visual (telegraph)

    Enrage = 35870, // Boss->self, 10.0s cast, range 60 circle, enrage
}

public enum SID : uint
{
    //Doom = 1769, // Boss->player, extra=0x0
    //HPPenalty = 1089, // none->player, extra=0x0
    //AccelerationBomb = 2657, // none->player, extra=0x0
    DivisiveDark = 3762, // none->player, extra=0x0
    //BigBang = 3760, // none->Boss, extra=0x0
    //BeckoningDark = 3794, // none->player, extra=0x0
    ForkedLightning = 3799, // none->player, extra=0x0
    //BigBounce = 3761, // Boss->player, extra=0x0
    //Bind = 2518, // none->player, extra=0x0
    //BluntResistanceDown = 2248, // Comet->player, extra=0x0
    //BondsOfDarkness = 3767, // none->player, extra=0x0
    //Pollen = 1507, // ToxicBubble->player, extra=0x0
    //FireResistanceDownII = 2098, // Helper->player, extra=0x0
    // = 2056, // Boss->Boss, extra=0x286
    //FleshWound = 264, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    DarkMatter = 364, // player
    BigBang = 376, // player
    Chain = 326, // player
    AccelerationBomb = 267, // player
    DarkBeckonsUmbralRays = 62, // player
    BlackHole = 330, // player
    Nox = 197, // player
    AkhRhai = 23, // player
    UmbralPrism = 211, // player
}

public enum TetherID : uint
{
    VoidMeteorCloseClipping = 252, // Comet->player
    VoidMeteorCloseGood = 253, // Comet->player
    VoidMeteorStretchedClipping = 254, // Comet->player
    VoidMeteorStretchedGood = 255, // Comet->player
    BondsOfDarkness = 163, // player->player
    FlowOfTheAbyss = 265, // FlowOfTheAbyss->Boss
}
