namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

public enum OID : uint
{
    Boss = 0x4118, // R20.000, x1
    Helper = 0x233C, // R0.500, x20, 523 type
    IceTalon1 = 0x417B, // R0.000, x1, Part type
    IceTalon2 = 0x417C, // R0.000, x1, Part type
    IceBoulder = 0x4492, // R0.500, x0 (spawn during fight), spikesicle
    ScourgeOfFireVoidzone = 0x1E8D9B, // R0.500, x0 (spawn during fight), EventObj type
    ThunderousBeacon = 0x438D, // R4.800, x0 (spawn during fight), add
    FlameKissedBeacon = 0x438E, // R4.800, x0 (spawn during fight), add
    GlacialBeacon = 0x438F, // R4.800, x0 (spawn during fight), add
    FeatherOfRuin = 0x4119, // R2.680, x0 (spawn during fight)
    IceBoulderJail = 0x411A, // R1.260, x0 (spawn during fight), when player is hit by ice mechanics
    ArcaneSphere = 0x4182, // R1.000, x0 (spawn during fight), thunderous breath lines
    ChillingCataclysmArcaneSphere = 0x411B, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 36892, // Boss->player, no cast, single-target

    SpikesicleStart = 36849, // Boss->self, no cast, single-target, visual (right before cast start)
    Spikesicle = 36850, // Boss->self, 10.0+0.5s cast, single-target, visual (curved aoes)
    SpikesicleRepeat = 36851, // Boss->self, no cast, single-target, visual (second+ curve)
    SpikesicleEnd = 36852, // Boss->self, no cast, single-target, visual (mechanic end)
    SpikesicleAOE1 = 36853, // Helper->self, 1.7s cast, range ?-25 donut
    SpikesicleAOE2 = 36854, // Helper->self, 1.7s cast, range ?-30 donut
    SpikesicleAOE3 = 36855, // Helper->self, 1.7s cast, range ?-35 donut
    SpikesicleAOE4 = 36856, // Helper->self, 1.7s cast, range ?-40 donut
    SpikesicleAOE5 = 36857, // Helper->self, 1.7s cast, range 40 width 5 rect
    SphereShatter = 39261, // IceBoulder->self, 1.5s cast, range 13 circle

    SusurrantBreathThunder = 36805, // Boss->self, 6.5s cast, single-target, visual (cone in thunder phase)
    SusurrantBreathIce = 36806, // Boss->self, 6.5s cast, single-target, visual (cone in ice phase)
    SusurrantBreathFire = 36807, // Boss->self, 6.5s cast, single-target, visual (cone in fire phase)
    SusurrantBreathAOE = 36808, // Helper->self, 7.3s cast, range 50 ?-degree cone
    SlitheringStrikeThunder = 36809, // Boss->self, 6.5s cast, single-target, visual (out in thunder phase)
    SlitheringStrikeIce = 36810, // Boss->self, 6.5s cast, single-target, visual (out in ice phase)
    SlitheringStrikeFire = 36811, // Boss->self, 6.5s cast, single-target, visual (out in fire phase)
    SlitheringStrikeAOE = 36812, // Helper->self, 7.3s cast, range 24 180-degree cone
    StranglingCoilThunder = 36813, // Boss->self, 6.5s cast, single-target, visual (in in thunder phase)
    StranglingCoilIce = 36814, // Boss->self, 6.5s cast, single-target, visual (in in ice phase)
    StranglingCoilFire = 36815, // Boss->self, 6.5s cast, single-target, visual (in in fire phase)
    StranglingCoilAOE = 36816, // Helper->self, 7.3s cast, range 8-30 donut
    CharringCataclysm = 36804, // Helper->players, no cast, range 4 circle, pairs in fire phase
    ChillingCataclysm = 36802, // ChillingCataclysmArcaneSphere->self, 1.0s cast, single-target, visual (cross)
    ChillingCataclysmAOE = 36803, // Helper->self, 1.5s cast, range 40 width 5 cross
    CracklingCataclysm = 36801, // Helper->location, 3.0s cast, range 6 circle, puddle

    SkyruinFire = 38340, // Boss->self, 6.0+5.3s cast, single-target, visual (switch to fire phase)
    SkyruinFireAOE = 36820, // Helper->self, 4.5s cast, range 80 circle, raidwide
    SkyruinIce = 36817, // Boss->self, 6.0+5.3s cast, single-target, visual (switch to thunder phase)
    SkyruinIceAOE = 36818, // Helper->self, 4.5s cast, range 80 circle, raidwide
    SkyruinThunder = 38339, // Boss->self, 6.0+5.3s cast, single-target, visual (switch to ice phase)
    SkyruinThunderAOE = 36819, // Helper->self, 4.5s cast, range 80 circle, raidwide
    DisasterZoneFire = 36825, // Boss->self, 3.0+0.8s cast, ???, visual (raidwide + fire phase end)
    DisasterZoneFireAOE = 36826, // Helper->self, 3.8s cast, range 80 circle, raidwide
    DisasterZoneIce = 36821, // Boss->self, 3.0+0.8s cast, ???, visual (raidwide + ice phase end)
    DisasterZoneIceAOE = 36822, // Helper->self, 3.8s cast, range 80 circle, raidwide
    DisasterZoneThunder = 36823, // Boss->self, 3.0+0.8s cast, ???, visual (raidwide + thunder phase end)
    DisasterZoneThunderAOE = 36824, // Helper->self, 3.8s cast, range 80 circle, raidwide

    Triscourge = 36839, // Boss->self, 3.0s cast, range 80 circle, raidwide with debuffs
    FireScourgeOfFire = 36847, // Helper->players, no cast, range 5 circle, 3-hit 4-man stack that leaves voidzone
    IceScourgeOfFire = 36846, // Helper->players, no cast, range 5 circle, 3-man stack
    IceScourgeOfIce = 36844, // Helper->player, no cast, range 16 circle, defamation
    FireIceScourgeOfThunder = 36842, // Helper->players, no cast, range 5 circle, spread
    ThunderScourgeOfFire = 36845, // Helper->players, no cast, range 5 circle, 4-man stack
    ThunderScourgeOfThunderFail = 36840, // Helper->player, no cast, single-target, vuln up + paralysis
    ThunderScourgeOfThunder = 36841, // Helper->players, no cast, range 8 circle, spread

    VolcanicDropPuddle = 36836, // Helper->location, 2.5s cast, range 2 circle
    VolcanicDropVisual = 36837, // Helper->location, no cast, range 20 circle, visual (volcano)
    VolcanicDropAOE = 36838, // Helper->location, no cast, range 20 circle, voidzone
    MountainFire = 36876, // Boss->self, 4.0s cast, single-target, visual (tankbuster tower)

    MountainFireMoveNToNW = 36877, // Boss->self, no cast, single-target
    MountainFireMoveNToNE = 36878, // Boss->self, no cast, single-target
    MountainFireMoveNWToN = 36879, // Boss->self, no cast, single-target
    MountainFireMoveNWToNE = 36880, // Boss->self, no cast, single-target
    MountainFireMoveNEToN = 36881, // Boss->self, no cast, single-target
    MountainFireMoveNEToNW = 36882, // Boss->self, no cast, single-target
    MountainFireConeN = 36883, // Boss->self, no cast, single-target, visual (cone from N)
    MountainFireConeNW = 36884, // Boss->self, no cast, single-target, visual (cone from NW)
    MountainFireConeNE = 36885, // Boss->self, no cast, single-target, visual (cone from NE)
    MountainFireTower = 36889, // Helper->self, 3.5s cast, range 3 circle, tower
    UnmitigatedExplosion = 36891, // Helper->self, no cast, range 80 circle, tower fail
    MountainFireConeAOE = 36890, // Helper->self, no cast, range 40 ?-degree cone
    MountainFireEndN = 36886, // Boss->self, no cast, single-target, visual (mechanic end)
    MountainFireEndNW = 36887, // Boss->self, no cast, single-target, visual (mechanic end)
    MountainFireEndNE = 36888, // Boss->self, no cast, single-target, visual (mechanic end)

    NorthernCrossR = 36827, // Helper->self, 3.0s cast, range 60 width 25 rect (avalanche right)
    NorthernCrossL = 36828, // Helper->self, 3.0s cast, range 60 width 25 rect (avalanche left)
    FreezingDust = 36848, // Boss->self, 5.0+0.8s cast, range 80 circle, apply freezing
    IceTalon = 36858, // Boss->self, 4.0+1.0s cast, single-target, aoe tankbusters at both tanks
    IceTalonAOE = 36859, // IceTalon1/IceTalon2->player, no cast, range 6 circle tankbuster

    HailOfFeathers = 36829, // Boss->self, 4.0+2.0s cast, single-target, visual (proximity feathers)
    HailOfFeathersVisual = 36830, // FeatherOfRuin->self, no cast, single-target, visual (feather appear)
    HailOfFeathersAOE1 = 36893, // Helper->self, 6.0s cast, range 80 circle with ? falloff
    HailOfFeathersAOE2 = 36894, // Helper->self, 9.0s cast, range 80 circle with ? falloff
    HailOfFeathersAOE3 = 36895, // Helper->self, 12.0s cast, range 80 circle with ? falloff
    HailOfFeathersAOE4 = 36896, // Helper->self, 15.0s cast, range 80 circle with ? falloff
    HailOfFeathersAOE5 = 36897, // Helper->self, 18.0s cast, range 80 circle with ? falloff
    HailOfFeathersAOE6 = 36898, // Helper->self, 21.0s cast, range 80 circle with ? falloff
    BlightedBolt = 36831, // Boss->self, 5.0+0.8s cast, single-target, visual (feather explosion)
    BlightedBolt2 = 36832, // Helper->player, no cast, range 3 circle
    BlightedBoltAOE = 36833, // Helper->FeatherOfRuin, 5.8s cast, range 8 circle
    ThunderousBreath = 36834, // Boss->self, 7.0+0.9s cast, single-target, visual (lines + require levitation)
    ThunderousBreathAOE = 36835, // Helper->self, 7.9s cast, range 50 ?-degree cone
    ArcaneLightning = 39002, // ArcaneSphere->self, 1.0s cast, range 50 width 5 rect
    Ruinfall = 36860, // Boss->self, 4.0+1.6s cast, single-target, visual (shared tankbuster tower + knockbacks)
    RuinfallTower = 36861, // Helper->self, 5.6s cast, range 6 circle, 2-man tankbuster tower
    RuinfallKnockback = 36863, // Helper->self, 8.0s cast, range 40 width 40 rect, knockback 25
    RuinfallAOE = 39130, // Helper->location, 9.5s cast, range 6 circle puddle

    RuinForetold = 38546, // Boss->self, 5.0s cast, range 80 circle, raidwide
    CalamitousCryFirst = 36866, // Boss->self, 5.1+0.9s cast, single-target, first damage up
    CalamitousCryRest = 36867, // Boss->self, no cast, single-target, second+ damage up
    CalamitousEcho = 36870, // Helper->self, 5.0s cast, range 40 20-degree cone
    CalamitousCryTargetFirst = 34722, // Helper->player, no cast, single-target, target select for first wild charge
    CalamitousCryTargetRest = 26708, // Helper->player, no cast, single-target, target select for second+ wild charges
    CalamitousCryEnrage = 36868, // Helper->self, no cast, range 80 width 6 rect
    CalamitousCryAOE = 36869, // Helper->self, no cast, range 80 width 6 rect, wild charge
    RuinForetoldBeaconsDead1 = 38245, // FlameKissedBeacon->Boss, no cast, single-target, visual (all 3 beacons are dead)
    RuinForetoldBeaconsDead2 = 38246, // ThunderousBeacon->Boss, no cast, single-target, visual (all 3 beacons are dead)
    RuinForetoldBeaconsDead3 = 38247, // GlacialBeacon->Boss, no cast, single-target, visual (all 3 beacons are dead)
    RuinForetoldEnd = 38323, // Boss->self, no cast, single-target, visual (mechanic end)
    Tulidisaster = 36872, // Boss->self, 7.0+3.0s cast, single-target, visual (triple raidwide)
    TulidisasterAOE1 = 36874, // Helper->self, no cast, range 80 circle
    TulidisasterAOE2 = 36875, // Helper->self, no cast, range 80 circle
    TulidisasterAOE3 = 36873, // Helper->self, no cast, range 80 circle, raidwide with dot
    WrathUnfurled = 36871, // Boss->self, 4.0+3.3s cast, single-target, visual (raidwide)
    WrathUnfurledAOE = 39237, // Helper->self, 7.3s cast, range 80 circle, raidwide
    TulidisasterEnrage = 37452, // Boss->self, 7.0+3.0s cast, single-target, visual (enrage sequence)
    TulidisasterEnrageAOE1 = 37454, // Helper->self, no cast, range 80 circle
    TulidisasterEnrageAOE2 = 37455, // Helper->self, no cast, range 80 circle
    TulidisasterEnrageAOE3 = 37453, // Helper->self, no cast, range 80 circle
}

public enum SID : uint
{
    //_Gen_ = 2552, // none->Boss/player, extra=0x2AE/0x21
    //CalamitysFlames = 3817, // none->player, extra=0x0
    //CalamitysInferno = 3818, // none->player, extra=0x0
    //CalamitysEmbers = 3819, // none->player, extra=0x0
    //CalamitysFrost = 3820, // none->player, extra=0x0
    //CalamitysBite = 3821, // none->player, extra=0x0
    //CalamitysChill = 3822, // none->player, extra=0x0
    //CalamitysBolt = 3823, // none->player, extra=0x0
    //CalamitysFulgur = 3824, // none->player, extra=0x0
    //Prey = 2939, // none->player, extra=0x0
    //PerpetualConflagration = 4122, // none->player, extra=0x0
    FreezingUp = 3523, // Boss->player, extra=0x0
    Levitate = 3974, // none->player, extra=0xD7
}

public enum IconID : uint
{
    CalamitysBolt = 507, // player
    CalamitysInferno = 508, // player
    CalamitysChill = 509, // player
    IceTalon = 344, // player
}

public enum TetherID : uint
{
    ArcaneLightning = 6, // ArcaneSphere->Boss
}
