namespace BossMod.Endwalker.Variant.V01SS.V014ZelessGah;
public enum OID : uint
{
    Boss = 0x39A9, // R9.000, x1
    Helper = 0x233C, // R0.500, x32, 523 type

    InfernBrand1 = 0x39AD, // R2.000, x4
    InfernBrand2 = 0x39AB, // R2.000, x6
    MyrrhIncenseBurner = 0x1EB7CF, // R2.000, x1, EventObj type
    ForebodingDoor = 0x1EB7CE, // R0.500, x1, EventObj type
    BallOfFire = 0x39AF, // R1.000, x0 (spawn during fight)
    ArcaneFont = 0x39B1, // R0.500-1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Burn = 29839, // BallOfFire->self, 1.5s cast, range 12 circle

    CastShadow = 29850, // Boss->self, 4.8s cast, single-target
    CastShadowFirst = 29851, // Helper->self, 5.5s cast, range 65 ?-degree cone
    CastShadowNext = 29853, // Helper->self, 7.5s cast, range 65 ?-degree cone

    CrypticPortal1 = 29842, // Boss->self, 5.0s cast, single-target
    CrypticPortal2 = 29843, // Boss->self, 8.0s cast, single-target

    FiresteelFracture = 29868, // Boss->self/player, 5.0s cast, range 40 ?-degree cone
    InfernBrand = 29841, // Boss->self, 4.0s cast, single-target

    BlazingBenifice = 29861, // ArcaneFont->self, 1.5s cast, range 100 width 10 rect

    InfernGale1 = 29858, // Boss->self, 4.0s cast, single-target
    InfernGale2 = 29859, // Helper->player, no cast, single-target

    InfernWard = 29846, // Boss->self, 4.0s cast, single-target
    PureFire1 = 29855, // Boss->self, 3.0s cast, single-target
    PureFire2 = 29856, // Helper->location, 3.0s cast, range 6 circle
    ShowOfStrength = 29870, // Boss->self, 5.0s cast, range 65 circle

    Unknown1 = 29845, // Helper->InfernBrand1/InfernBrand2, no cast, single-target
    Unknown2 = 29847, // Helper->self, no cast, single-target
    Unknown3 = 29886, // Boss->location, no cast, single-target

    //route 7
    InfernWell1 = 29863, // Boss->self, 4.0s cast, single-target
    UnknownAbility1 = 29864, // Helper->player, no cast, single-target
    InfernWell2 = 29866, // Helper->self, 1.5s cast, range 8 circle
    UnknownAbility2 = 29890, // Helper->self, no cast, range 60 width 100 rect
    TrespassersPyre = 29848, // Helper->player, 1.0s cast, single-target
}

public enum SID : uint
{
    UnknownStatus = 2397, // none->InfernBrand1/InfernBrand2, extra=0x1CA/0x1C1/0x1F3
    ForbiddenPassage = 3278, // none->player, extra=0x0
    VulnerabilityUp = 1789, // BallOfFire->player, extra=0x1
    Stun = 2656, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_230 = 230, // player
}
