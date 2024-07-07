namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x20, 523 type
    Boss = 0x4115, // R20.000, x1

    Valigarmanda1 = 0x417A, // R0.000, x1, Part type
    Valigarmanda2 = 0x4179, // R0.000, x1, Part type

    FeatherOfRuin = 0x4116, // R2.680, x0 (spawn during fight)

    ArcaneSphere1 = 0x4181, // R1.000, x0 (spawn during fight)
    ArcaneSphere2 = 0x4493, // R1.000, x0 (spawn during fight)

    IceBoulder = 0x4117, // R1.260, x0 (spawn during fight)
    FlameKissedBeacon = 0x438B, // R4.800, x0 (spawn during fight)
    GlacialBeacon = 0x438C, // R4.800, x0 (spawn during fight)
    ThunderousBeacon = 0x438A, // R4.800, x0 (spawn during fight)

    Actor1ea1a1 = 0x1EA1A1, // R0.500-2.000, x1, EventObj type
}

public enum AID : uint
{
    Attack = 36899, // Boss->player, no cast, single-target

    StranglingCoil1 = 36159, // Boss->self, 6.5s cast, single-target
    StranglingCoil2 = 36160, // Helper->self, 7.3s cast, range ?-30 donut 

    SlitheringStrike1 = 36157, // Boss->self, 6.5s cast, single-target
    SlitheringStrike2 = 36158, // Helper->self, 7.3s cast, range 24 180.000-degree cone // Point blank AOE, out of melee range safe

    SusurrantBreath1 = 36155, // Boss->self, 6.5s cast, single-target
    SusurrantBreath2 = 36156, // Helper->self, 7.3s cast, range 50 ?-degree cone // Corners of arena safe

    Skyruin1 = 36161, // Boss->self, 6.0+5.3s cast, single-target
    Skyruin2 = 36162, // Helper->self, 4.5s cast, range 80 circle // Raidwide
    Skyruin3 = 36163, // Helper->self, 4.5s cast, range 80 circle // Raidwide
    Skyruin4 = 38338, // Boss->self, 6.0+5.3s cast, single-target

    ThunderousBreath1 = 36175, // Boss->self, 7.0+0.9s cast, single-target
    ThunderousBreath2 = 36176, // Helper->self, 7.9s cast, range 50 135.000-degree cone // Raidwide, mitigated by standing on levitation pads

    HailOfFeathers1 = 36170, // Boss->self, 4.0+2.0s cast, single-target
    HailOfFeathers2 = 36171, // FeatherOfRuin->self, no cast, single-target
    HailOfFeathers3 = 36361, // Helper->self, 6.0s cast, range 80 circle // Raidwide

    BlightedBolt1 = 36172, // Boss->self, 7.0+0.8s cast, single-target
    BlightedBolt2 = 36173, // Helper->player, no cast, range 3 circle // Player-targeted AOE, mitigated by leaving the levitation pads
    BlightedBolt3 = 36174, // Helper->FeatherOfRuin, 7.8s cast, range 7 circle // Feather-targeted AOE

    ArcaneLightning = 39001, // ArcaneSphere1->self, 1.0s cast, range 50 width 5 rect

    DisasterZone1 = 36164, // Boss->self, 3.0+0.8s cast, ???
    DisasterZone2 = 36165, // Helper->self, 3.8s cast, range 80 circle // Raidwide
    DisasterZone3 = 36166, // Boss->self, 3.0+0.8s cast, ???
    DisasterZone4 = 36167, // Helper->self, 3.8s cast, range 80 circle // Raidwide

    Ruinfall1 = 36186, // Boss->self, 4.0+1.6s cast, single-target
    RuinfallTower = 36187, // Helper->self, 5.6s cast, range 6 circle
    Ruinfall3 = 36189, // Helper->self, 8.0s cast, range 40 width 40 rect // Knockback 21 DirForward
    RuinfallAOE = 39129, // Helper->location, 9.7s cast, range 6 circle

    NorthernCross1 = 36168, // Helper->self, 3.0s cast, range 60 width 25 rect
    NorthernCross2 = 36169, // Helper->self, 3.0s cast, range 60 width 25 rect

    FreezingDust = 36177, // Boss->self, 5.0+0.8s cast, range 80 circle // Move or be frozen

    ChillingCataclysm1 = 39264, // ArcaneSphere2->self, 1.0s cast, single-target
    ChillingCataclysm2 = 39265, // Helper->self, 1.5s cast, range 40 width 5 cross

    RuinForetold = 38545, // Boss->self, 5.0s cast, range 80 circle // Raidwide, summons 3 adds that must be destroyed

    UnknownAbility1 = 34722, // Helper->player, no cast, single-target

    CalamitousCry1 = 36192, // Boss->self, 5.1+0.9s cast, single-target
    CalamitousCry2 = 36193, // Boss->self, no cast, single-target
    CalamitousCry3 = 36194, // Helper->self, no cast, range 80 width 6 rect

    CalamitousEcho = 36195, // Helper->self, 5.0s cast, range 40 20.000-degree cone // telegraphed conal AoEs

    UnknownAbility2 = 26708, // Helper->player, no cast, single-target
    UnknownWeaponskill1 = 38245, // FlameKissedBeacon->Boss, no cast, single-target
    UnknownWeaponskill2 = 38247, // GlacialBeacon->Boss, no cast, single-target
    UnknownWeaponskill3 = 38246, // ThunderousBeacon->Boss, no cast, single-target
    UnknownWeaponskill4 = 38323, // Boss->self, no cast, single-target

    Tulidisaster1 = 36197, // Boss->self, 7.0+3.0s cast, single-target // Repeat Raidwides, wipe if adds not killed
    Tulidisaster2 = 36199, // Helper->self, no cast, range 80 circle
    Tulidisaster3 = 36200, // Helper->self, no cast, range 80 circle
    Tulidisaster4 = 36198, // Helper->self, no cast, range 80 circle

    Eruption1 = 36190, // Boss->self, 3.0s cast, single-target
    Eruption2 = 36191, // Helper->location, 3.0s cast, range 6 circle // Baited AOEs

    IceTalonVisual = 36184, // Boss->self, 4.0+1.0s cast, single-target
    IceTalonTankbuster = 36185, // Valigarmanda1/Valigarmanda2->player, no cast, range 6 circle // AOE Tankbuster
}

public enum SID : uint
{
    SustainedDamage = 2935, // Helper->player, extra=0x0
    Trauma = 3796, // Helper->player, extra=0x1
    DamageDown = 628, // Helper->player, extra=0x1
    Concussion = 997, // Helper->player, extra=0xF43
    Electrocution = 2086, // Helper->player, extra=0x0
    Levitate = 3974, // none->player, extra=0xD7
    UnknownStatus = 2552, // none->player/FeatherOfRuin, extra=0x21/0x2AD/0x2BA
    Frostbite = 2083, // Helper->player, extra=0x0
    FreezingUp = 3523, // Boss->player, extra=0x0
    DeepFreeze = 4150, // Helper->player, extra=0x0
    DamageUp = 3975, // Boss->Boss, extra=0x1/0x2/0x3/0x4
    PerpetualConflagration = 4122, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1
    Paralysis = 3463, // ArcaneSphere1->player, extra=0x0

}

public enum IconID : uint
{
    Icon225 = 225, // player
    Icon344 = 344, // player
}

public enum TetherID : uint
{
    Tether6 = 6, // ArcaneSphere1->Boss
}
