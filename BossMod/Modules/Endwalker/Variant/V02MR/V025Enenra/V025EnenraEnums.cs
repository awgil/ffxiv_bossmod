namespace BossMod.Endwalker.Variant.V02MR.V025Enenra;

public enum OID : uint
{
    Boss = 0x3EAD,
    EnenraClone = 0x3EAE, // R2.800, x1
    Helper = 0x233C, // R0.500, x25, 523 type
    UnknownActor = 0x3FD0, // R3.000, x1
    Actor1eb891 = 0x1EB891, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb890 = 0x1EB890, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb88f = 0x1EB88F, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss/EnenraClone->player, no cast, single-target

    KiseruClamor = 32840, // Boss/EnenraClone->location, 3.0s cast, range 6 circle //cascading earthquake AoEs
    BedrockUplift1 = 32841, // Helper->self, 5.0s cast, range 6-12 donut
    BedrockUplift2 = 32842, // Helper->self, 7.0s cast, range 12-18 donut
    BedrockUplift3 = 32843, // Helper->self, 9.0s cast, range 18-24 donut

    ClearingSmoke = 32866, // EnenraClone/Boss->self, 8.0s cast, single-target
    ClearingSmokeKB = 32850, // Helper->self, 11.5s cast, range 21 circle //Knockback
    SmokeRings = 32867, // EnenraClone/Boss->self, 8.0s cast, single-target //AOE
    SmokeRingsAOE = 32851, // Helper->self, 11.5s cast, range 16 circle //AOE

    FlagrantCombustion = 32834, // Boss/EnenraClone->self, 5.0s cast, range 50 circle //raidwide

    OutOfTheSmoke = 32844, // Boss/EnenraClone->self, 12.0s cast, single-target
    IntoTheFire = 32845, // Boss/EnenraClone->self, 1.0s cast, single-target
    IntoTheFireAOE = 32856, // Helper->self, 1.5s cast, range 50 width 50 rect //frontal cleave

    PipeCleaner = 32852, // Boss->self, 5.0s cast, single-target
    PipeCleanerAOE = 32853, // Boss->self, no cast, range 50 width 6 rect // Tethers one player and hits them with a line AoE.

    SmokeAndMirrors = 32835, // Boss->self, 2.5s cast, single-target

    UnknownAbility3 = 32837, // EnenraClone->location, no cast, single-target
    SmokeStackBoss = 32838, // Boss->location, 2.0s cast, single-target // recombine
    SmokeStackClone = 32839, // EnenraClone->location, 2.0s cast, single-target // recombine

    Smoldering = 32848, // Helper->self, 7.0s cast, range 8 circle
    UnknownAbility2 = 32846, // Boss/EnenraClone->location, no cast, single-target
    SmolderingDamnation = 32847, // Boss->self, 4.0s cast, single-target

    Snuff = 32854, // Boss->player, 5.0s cast, range 6 circle //Telegraphed AoE tankbuster and baited AOE
    Uplift = 32855, // Helper->location, 3.5s cast, range 6 circle

    UnknownAbility1 = 33986, // Boss/EnenraClone->location, no cast, single-target
}

public enum SID : uint
{
    Unknown1 = 2397, // none->Boss/EnenraClone, extra=0x260
    VulnerabilityUp = 1789, // Helper->player, extra=0x1
    MirroredSmoke = 3520, // none->Boss, extra=0x0
    Unknown2 = 2056, // none->UnknownActor, extra=0x245/0x246
    Liftoff = 3262, // Helper->player, extra=0x0
}
public enum IconID : uint
{
    Icon_344 = 344, // player
}
public enum TetherID : uint
{
    Tether_244 = 244, // EnenraClone->Boss
    PipeCleaner = 17, // player->Boss
}
