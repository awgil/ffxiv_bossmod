namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

public enum OID : uint
{
    Boss = 0x31AB, // R9.000, x1
    Helper = 0x233C, // R0.500, x25, 523 type
    MarsakTheStalwart = 0x326F, // R0.500, x1
    BajsaljenTheRighteous = 0x326E, // R0.500, x1
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    AmbientPulsation = 23693, // Boss->self, 5.0s cast, single-target
    AmbientPulsationAOE = 23694, // Helper->self, 8.0s cast, range 12 circle

    BurgeoningDread1 = 23688, // Boss->self, 5.0s cast, ???
    BurgeoningDread2 = 23689, // Helper->self, 5.0s cast, ???

    FellFlow1 = 23691, // Boss->self, 5.0s cast, range 50 120-degree cone
    FellFlow2 = 23692, // Helper->self, no cast, range 50 ?-degree cone

    FleshyNecromass1 = 23682, // Boss->self, 8.0s cast, single-target // visual
    FleshyNecromass2 = 23683, // Boss->location, no cast, single-target // teleport
    FleshyNecromass3 = 23684, // Helper->self, no cast, range 12 circle
    FleshyNecromass4 = 23685, // Helper->self, no cast, range 12 circle
    FleshyNecromass5 = 24953, // Boss->location, no cast, single-target // teleport

    GhastlyAura1 = 24909, // Boss->self, 5.0s cast, ???
    GhastlyAura2 = 24910, // Helper->self, 5.0s cast, ???

    MightOfMalice = 23698, // Boss->player, 5.0s cast, single-target

    NecroticBillow = 23686, // Boss->self, 5.0s cast, single-target
    NecroticBillowAOE = 23687, // Helper->self, 4.0s cast, range 8 circle

    PutrifiedSoul1 = 23695, // Boss->self, 5.0s cast, ???
    PutrifiedSoul2 = 23696, // Helper->self, 5.0s cast, ???
}

public enum SID : uint
{
    AboutFace = 2162, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    ForwardMarch = 2161, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x2/0x8/0x1/0x4
    Gelatinous = 2543, // none->player, extra=0x1AD
    Bleeding = 642, // none->player, extra=0x0
    Infirmity = 172, // none->player, extra=0x0
    DownForTheCount = 783, // Helper->player, extra=0xEC7
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2/0x3/0x4
    LeftFace = 2163, // Boss->player, extra=0x0
    TemporaryMisdirection = 1422, // Helper/Boss->player, extra=0x2D0
    RayOfFortitude = 2625, // none->player, extra=0x3/0x8/0xA/0x5/0x1/0x6
    RayOfValor = 2626, // none->player, extra=0x6/0xA/0x8/0x1
    RayOfSuccor = 2627, // none->player, extra=0x2/0x4/0xA/0x3/0x1
    HoofingIt = 1778, // none->player, extra=0x0
    DutiesAsAssigned = 2415, // none->player, extra=0x0
    TheEcho = 42, // none->player, extra=0x0
    MeatAndMead = 360, // none->player, extra=0xA
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    PhysicalAversion = 2369, // none->Boss, extra=0x0
}

public enum IconID : uint
{
    Icon40 = 40, // player
}
