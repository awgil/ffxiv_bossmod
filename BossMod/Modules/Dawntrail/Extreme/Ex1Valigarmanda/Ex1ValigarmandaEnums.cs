namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

public enum OID : uint
{
    Boss = 0x4118, // R20.000, x1
    Helper = 0x233C, // R0.500, x20, 523 type

    Valigarmanda1 = 0x417C, // R0.000-0.500, x1, Part type
    Valigarmanda2 = 0x417B, // R0.000-0.500, x1, Part type

    IceBoulder1 = 0x4492, // R0.500, x0 (spawn during fight)
    IceBoulder2 = 0x411A, // R1.260, x0 (spawn during fight)

    FlameKissedBeacon = 0x438E, // R4.800, x0 (spawn during fight)
    ThunderousBeacon = 0x438D, // R4.800, x0 (spawn during fight)
    GlacialBeacon = 0x438F, // R4.800, x0 (spawn during fight)
    FeatherOfRuin = 0x4119, // R2.680, x0 (spawn during fight)
    ArcaneSphere1 = 0x411B, // R1.000, x0 (spawn during fight)
    ArcaneSphere2 = 0x4182, // R1.000, x0 (spawn during fight)
    Actor1e8d9b = 0x1E8D9B, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 36892, // Boss->player, no cast, single-target

    ArcaneLightning = 39002, // ArcaneSphere2->self, 1.0s cast, range 50 width 5 rect

    BlightedBolt1 = 36831, // Boss->self, 5.0+0.8s cast, single-target
    BlightedBolt2 = 36832, // Helper->player, no cast, range 3 circle
    BlightedBolt3 = 36833, // Helper->FeatherOfRuin, 5.8s cast, range 8 circle

    CalamitousCry1 = 36866, // Boss->self, 5.1+0.9s cast, single-target
    CalamitousCry2 = 36867, // Boss->self, no cast, single-target
    CalamitousCry3 = 36868, // Helper->self, no cast, range 80 width 6 rect
    CalamitousCry4 = 36869, // Helper->self, no cast, range 80 width 6 rect

    CalamitousEcho = 36870, // Helper->self, 5.0s cast, range 40 20.000-degree cone

    CharringCataclysm = 36804, // Helper->players, no cast, range 4 circle

    ChillingCataclysm1 = 36802, // 411B->self, 1.0s cast, single-target
    ChillingCataclysm2 = 36803, // Helper->self, 1.5s cast, range 40 width 5 cross

    CracklingCataclysm = 36801, // Helper->location, 3.0s cast, range 6 circle

    DisasterZone1 = 36821, // Boss->self, 3.0+0.8s cast, ???
    DisasterZone2 = 36822, // Helper->self, 3.8s cast, range 80 circle
    DisasterZone3 = 36823, // Boss->self, 3.0+0.8s cast, ???
    DisasterZone4 = 36824, // Helper->self, 3.8s cast, range 80 circle
    DisasterZone5 = 36825, // Boss->self, 3.0+0.8s cast, ???
    DisasterZone6 = 36826, // Helper->self, 3.8s cast, range 80 circle

    FreezingDust = 36848, // Boss->self, 5.0+0.8s cast, range 80 circle

    HailOfFeathers1 = 36829, // Boss->self, 4.0+2.0s cast, single-target
    HailOfFeathers2 = 36830, // FeatherOfRuin->self, no cast, single-target
    HailOfFeathers3 = 36893, // Helper->self, 6.0s cast, range 80 circle
    HailOfFeathers4 = 36894, // Helper->self, 9.0s cast, range 80 circle
    HailOfFeathers5 = 36895, // Helper->self, 12.0s cast, range 80 circle
    HailOfFeathers6 = 36896, // Helper->self, 15.0s cast, range 80 circle
    HailOfFeathers7 = 36897, // Helper->self, 18.0s cast, range 80 circle
    HailOfFeathers8 = 36898, // Helper->self, 21.0s cast, range 80 circle

    IceTalon1 = 36858, // Boss->self, 4.0+1.0s cast, single-target
    IceTalon2 = 36859, // Valigarmanda2/Valigarmanda1->player, no cast, range 6 circle

    MountainFire1 = 36876, // Boss->self, 4.0s cast, single-target
    MountainFire2 = 36883, // Boss->self, no cast, single-target
    MountainFire3 = 36884, // Boss->self, no cast, single-target
    MountainFire4 = 36885, // Boss->self, no cast, single-target
    MountainFire5 = 36889, // Helper->self, 3.5s cast, range 3 circle
    MountainFire6 = 36890, // Helper->self, no cast, range 40 ?-degree cone

    NorthernCross1 = 36827, // Helper->self, 3.0s cast, range 60 width 25 rect
    NorthernCross2 = 36828, // Helper->self, 3.0s cast, range 60 width 25 rect

    Ruinfall1 = 36860, // Boss->self, 4.0+1.6s cast, single-target
    Ruinfall2 = 36861, // Helper->self, 5.6s cast, range 6 circle
    Ruinfall3 = 36863, // Helper->self, 8.0s cast, range 40 width 40 rect
    Ruinfall4 = 39130, // Helper->location, 9.5s cast, range 6 circle

    RuinForetold = 38546, // Boss->self, 5.0s cast, range 80 circle

    ScourgeOfFire1 = 36845, // Helper->players, no cast, range 5 circle
    ScourgeOfFire2 = 36846, // Helper->players, no cast, range 5 circle
    ScourgeOfFire3 = 36847, // Helper->players, no cast, range 5 circle

    ScourgeOfIce = 36844, // Helper->player, no cast, range 16 circle

    ScourgeOfThunder1 = 36840, // Helper->player, no cast, single-target
    ScourgeOfThunder2 = 36841, // Helper->players, no cast, range 8 circle
    ScourgeOfThunder3 = 36842, // Helper->player, no cast, range 5 circle

    Skyruin1 = 36817, // Boss->self, 6.0+5.3s cast, single-target
    Skyruin2 = 36818, // Helper->self, 4.5s cast, range 80 circle
    Skyruin3 = 36819, // Helper->self, 4.5s cast, range 80 circle
    Skyruin4 = 38339, // Boss->self, 6.0+5.3s cast, single-target
    Skyruin5 = 36820, // Helper->self, 4.5s cast, range 80 circle
    Skyruin6 = 38340, // Boss->self, 6.0+5.3s cast, single-target

    SlitheringStrike1 = 36809, // Boss->self, 6.5s cast, single-target
    SlitheringStrike2 = 36810, // Boss->self, 6.5s cast, single-target
    SlitheringStrike3 = 36811, // Boss->self, 6.5s cast, single-target
    SlitheringStrike4 = 36812, // Helper->self, 7.3s cast, range 24 180.000-degree cone

    SphereShatter1 = 36843, // IceBoulder2->self, no cast, range 80 circle
    SphereShatter2 = 39261, // IceBoulder->self, 1.5s cast, range 13 circle

    Spikesicle1 = 36850, // Boss->self, 10.0+0.5s cast, single-target
    Spikesicle2 = 36851, // Boss->self, no cast, single-target
    Spikesicle3 = 36853, // Helper->self, 1.7s cast, range ?-25 donut
    Spikesicle4 = 36854, // Helper->self, 1.7s cast, range ?-30 donut
    Spikesicle5 = 36855, // Helper->self, 1.7s cast, range ?-35 donut
    Spikesicle6 = 36856, // Helper->self, 1.7s cast, range ?-40 donut
    Spikesicle7 = 36857, // Helper->self, 1.7s cast, range 40 width 5 rect

    StranglingCoil1 = 36813, // Boss->self, 6.5s cast, single-target
    StranglingCoil2 = 36814, // Boss->self, 6.5s cast, single-target
    StranglingCoil3 = 36815, // Boss->self, 6.5s cast, single-target
    StranglingCoil4 = 36816, // Helper->self, 7.3s cast, range ?-30 donut 

    SusurrantBreath1 = 36805, // Boss->self, 6.5s cast, single-target
    SusurrantBreath2 = 36806, // Boss->self, 6.5s cast, single-target
    SusurrantBreath3 = 36807, // Boss->self, 6.5s cast, single-target
    SusurrantBreath4 = 36808, // Helper->self, 7.3s cast, range 50 ?-degree cone

    ThunderousBreath1 = 36834, // Boss->self, 7.0+0.9s cast, single-target
    ThunderousBreath2 = 36835, // Helper->self, 7.9s cast, range 50 ?-degree cone

    Triscourge = 36839, // Boss->self, 3.0s cast, range 80 circle

    Tulidisaster1 = 36872, // Boss->self, 7.0+3.0s cast, single-target
    Tulidisaster2 = 36873, // Helper->self, no cast, range 80 circle
    Tulidisaster3 = 36874, // Helper->self, no cast, range 80 circle
    Tulidisaster4 = 36875, // Helper->self, no cast, range 80 circle
    Tulidisaster5 = 37452, // Boss->self, 7.0+3.0s cast, single-target
    Tulidisaster6 = 37453, // Helper->self, no cast, range 80 circle
    Tulidisaster7 = 37454, // Helper->self, no cast, range 80 circle
    Tulidisaster8 = 37455, // Helper->self, no cast, range 80 circle

    UnknownAbility1 = 26708, // Helper->player, no cast, single-target
    UnknownAbility2 = 34722, // Helper->player, no cast, single-target

    UnknownWeaponskill1 = 36849, // Boss->self, no cast, single-target
    UnknownWeaponskill10 = 36887, // Boss->self, no cast, single-target
    UnknownWeaponskill11 = 36888, // Boss->self, no cast, single-target
    UnknownWeaponskill12 = 38245, // FlameKissedBeacon->Boss, no cast, single-target
    UnknownWeaponskill13 = 38246, // ThunderousBeacon->Boss, no cast, single-target
    UnknownWeaponskill14 = 38247, // GlacialBeacon->Boss, no cast, single-target
    UnknownWeaponskill15 = 38323, // Boss->self, no cast, single-target
    UnknownWeaponskill2 = 36852, // Boss->self, no cast, single-target
    UnknownWeaponskill3 = 36877, // Boss->self, no cast, single-target
    UnknownWeaponskill4 = 36878, // Boss->self, no cast, single-target
    UnknownWeaponskill5 = 36879, // Boss->self, no cast, single-target
    UnknownWeaponskill6 = 36880, // Boss->self, no cast, single-target
    UnknownWeaponskill7 = 36881, // Boss->self, no cast, single-target
    UnknownWeaponskill8 = 36882, // Boss->self, no cast, single-target
    UnknownWeaponskill9 = 36886, // Boss->self, no cast, single-target

    UnmitigatedExplosion = 36891, // Helper->self, no cast, range 80 circle

    VolcanicDrop1 = 36836, // Helper->location, 2.5s cast, range 2 circle
    VolcanicDrop2 = 36837, // Helper->location, no cast, range 20 circle
    VolcanicDrop3 = 36838, // Helper->location, no cast, range 20 circle

    WrathUnfurled1 = 36871, // Boss->self, 4.0+3.3s cast, single-target
    WrathUnfurled2 = 39237, // Helper->self, 7.3s cast, range 80 circle
}

public enum SID : uint
{
    Burns1 = 2082, // Helper->player, extra=0x0
    Burns2 = 2945, // Helper->player, extra=0x0
    Burns3 = 3065, // none->player, extra=0x0
    Burns4 = 3066, // none->player, extra=0x0
    Burns5 = 3826, // Helper->player, extra=0x1/0x2

    CalamitysBite = 3821, // none->player, extra=0x0
    CalamitysBolt = 3823, // none->player, extra=0x0
    CalamitysChill = 3822, // none->player, extra=0x0
    CalamitysEmbers = 3819, // none->player, extra=0x0
    CalamitysFlames = 3817, // none->player, extra=0x0
    CalamitysFrost = 3820, // none->player, extra=0x0
    CalamitysFulgur = 3824, // none->player, extra=0x0
    CalamitysInferno = 3818, // none->player, extra=0x0

    Concussion = 997, // Helper->player, extra=0xF43
    DamageDown = 628, // Helper/IceBoulder->player, extra=0x1/0x2/0x3/0x4
    DamageUp = 3975, // Boss->Boss, extra=0x1/0x2/0x3
    DeepFreeze = 4150, // Helper/Boss->player, extra=0x0
    Electrocution = 2086, // Helper->player, extra=0x0
    FireResistanceDownII = 2937, // Helper->player, extra=0x0
    FreezingUp = 3523, // none->player, extra=0x0
    Frostbite = 2083, // Helper/Valigarmanda2->player, extra=0x0
    Fury = 3805, // Boss->Boss, extra=0x0
    Levitate = 3974, // none->player, extra=0xD7
    LightningResistanceDownII = 2998, // Helper->player, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    Paralysis = 3463, // Helper/4182->player, extra=0x0
    PerpetualConflagration = 4122, // none->player, extra=0x0
    Prey = 2939, // none->player, extra=0x0
    SustainedDamage = 2935, // Helper->player, extra=0x0
    Trauma = 3796, // Helper->player, extra=0x1
    UnknownStatus = 2552, // none->Boss, extra=0x2AE
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
}

public enum IconID : uint
{
    Icon344 = 344, // player
    Icon507 = 507, // player
    Icon508 = 508, // player
    Icon509 = 509, // player
}

public enum TetherID : uint
{
    Tether8 = 8, // 411A->411A
    Tether6 = 6, // 4182->Boss
}
