namespace BossMod.Endwalker.Variant.V01SS.V015ThorneKnight;

public enum OID : uint
{
    Boss = 0x3917, // R7.200, x1
    Helper = 0x233C, // R0.500, x23, 523 type

    AmaljaaArtilleryCarriage = 0x394A, // R0.500, x12
    BallOfFire = 0x3989, // R1.500, x0 (spawn during fight)
    MagickedPuppet = 0x3949, // R0.500, x12
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    BlazingBeacon1 = 28921, // Helper->self, 8.5s cast, range 50 width 16 rect
    BlazingBeacon2 = 28926, // Helper->self, 6.5s cast, range 50 width 16 rect
    BlazingBeacon3 = 28928, // Helper->self, 8.5s cast, range 50 width 16 rect

    BlisteringBlow = 28906, // Boss->player, 5.0s cast, single-target //tankbuster
    Cogwheel = 28907, // Boss->self, 5.0s cast, range 50 circle
    Explosion = 28925, // BallOfFire->self, 7.0s cast, range 50 width 6 cross
    ForeHonor = 28908, // Boss->self, 6.0s cast, range 50 180-degree cone

    MagicCannon = 28919, // MagickedPuppet->self, no cast, range 45 width 6 rect
    AmaljaaArtillery = 28920, // AmaljaaArtilleryCarriage->self, no cast, range 45 width 6 rect

    SacredFlay1 = 28922, // Helper->self, 8.5s cast, range 50 ?-degree cone
    SacredFlay2 = 28927, // Helper->self, 6.5s cast, range 50 ?-degree cone
    SacredFlay3 = 28929, // Helper->self, 8.5s cast, range 50 ?-degree cone

    BlazeOfGlory = 28916, // Boss->self, 3.0s cast, single-target
    SignalFlare1 = 28917, // Boss->self, 5.5s cast, single-target
    SignalFlareAOE = 28923, // Helper->self, 7.0s cast, range 10 circle

    Slashburn1 = 28911, // Boss->self, 8.5s cast, single-target
    Slashburn2 = 28912, // Boss->self, 8.5s cast, single-target
    Slashburn3 = 28913, // Boss->self, 8.5s cast, single-target
    Slashburn4 = 28930, // Boss->self, 8.5s cast, single-target

    SpringToLife1 = 28909, // Boss->self, 7.5s cast, single-target
    SpringToLife2 = 28910, // Boss->self, 7.5s cast, single-target

    UnknownAbility1 = 28966, // MagickedPuppet->self, no cast, single-target
    UnknownAbility2 = 28967, // MagickedPuppet->self, no cast, single-target
}

public enum SID : uint
{
    UnknownStatus = 2397, // none->Boss, extra=0x1BF/0x1C0
    VulnerabilityUp = 1789, // BallOfFire/AmaljaaArtilleryCarriage->player, extra=0x2/0x4

}

public enum IconID : uint
{
    Icon218 = 218, // player
    Icon363 = 363, // Boss
}
