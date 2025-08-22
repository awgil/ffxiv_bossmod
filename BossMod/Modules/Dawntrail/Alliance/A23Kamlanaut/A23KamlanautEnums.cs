namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

public enum OID : uint
{
    Boss = 0x48F8, // R6.000, x1
    Helper = 0x233C, // R0.500, x30, Helper type
    SublimeEstoc = 0x48FA, // R1.000, x14
    Unk = 0x49C7, // R1.000, x8
    Clone = 0x48F9, // R6.000, x3

    ProvingGround = 0x1EBEEF,
}

public enum AID : uint
{
    AutoAttack = 44876, // Boss->player, no cast, single-target
    EnspiritedSwordplay = 44221, // Boss->self, 5.0s cast, range 60 circle
    BossJump = 44225, // Boss->location, no cast, single-target
    ProvingGround = 45065, // Boss->self, 3.0s cast, range 5 circle
    ElementalBladeFast = 44177, // Boss->self, 8.0s cast, single-target
    ElementalBladeSlow = 44178, // Boss->self, 11.0s cast, single-target
    SublimeElements = 45066, // Boss->self, 8.0+1.0s cast, single-target

    SublimeFireSmall = 44179, // Helper->self, 9.0s cast, range 40 22-degree cone
    SublimeEarthSmall = 44180, // Helper->self, 9.0s cast, range 40 22-degree cone
    SublimeWaterSmall = 44181, // Helper->self, 9.0s cast, range 40 22-degree cone
    SublimeIceSmall = 44182, // Helper->self, 9.0s cast, range 40 22-degree cone
    SublimeLightningSmall = 44183, // Helper->self, 9.0s cast, range 40 22-degree cone
    SublimeWindSmall = 44184, // Helper->self, 9.0s cast, range 40 22-degree cone

    SublimeFireLarge = 44185, // Helper->self, 9.0s cast, range 40 98-degree cone
    SublimeEarthLarge = 44186, // Helper->self, 9.0s cast, range 40 98-degree cone
    SublimeWaterLarge = 44187, // Helper->self, 9.0s cast, range 40 98-degree cone
    SublimeIceLarge = 44188, // Helper->self, 9.0s cast, range 40 98-degree cone
    SublimeLightningLarge = 44189, // Helper->self, 9.0s cast, range 40 98-degree cone
    SublimeWindLarge = 44190, // Helper->self, 9.0s cast, range 40 98-degree cone

    FireBladeSmall = 44191, // Helper->self, 9.0s cast, range 80 width 5 rect
    EarthBladeSmall = 44192, // Helper->self, 9.0s cast, range 80 width 5 rect
    WaterBladeSmall = 44193, // Helper->self, 9.0s cast, range 80 width 5 rect
    IceBladeSmall = 44194, // Helper->self, 9.0s cast, range 80 width 5 rect
    LightningBladeSmall = 44195, // Helper->self, 9.0s cast, range 80 width 5 rect
    WindBladeSmall = 44196, // Helper->self, 9.0s cast, range 80 width 5 rect

    FireBladeLarge = 44197, // Helper->self, 9.0s cast, range 80 width 20 rect
    EarthBladeLarge = 44198, // Helper->self, 9.0s cast, range 80 width 20 rect
    WaterBladeLarge = 44199, // Helper->self, 9.0s cast, range 80 width 20 rect
    IceBladeLarge = 44200, // Helper->self, 9.0s cast, range 80 width 20 rect
    LightningBladeLarge = 44201, // Helper->self, 9.0s cast, range 80 width 20 rect
    WindBladeLarge = 44202, // Helper->self, 9.0s cast, range 80 width 20 rect

    PrincelyBlowCast = 44219, // Boss->self, 7.2+0.8s cast, single-target
    PrincelyBlow = 44220, // Helper->self, no cast, range 60 width 10 rect
    LightBlade = 44203, // Boss->self, 3.0s cast, range 120 width 13 rect
    SublimeEstoc = 44204, // _Gen_SublimeEstoc->self, 3.0s cast, range 40 width 5 rect
    GreatWheel1 = 44205, // Boss->self, 3.0s cast, range 10 circle
    GreatWheel2 = 44206, // Boss->self, 3.0s cast, range 10 circle
    GreatWheel3 = 44207, // Boss->self, 3.0s cast, range 10 circle
    GreatWheel4 = 44208, // Boss->self, 3.0s cast, range 10 circle
    GreatWheelCleave = 44209, // Helper->self, 5.8s cast, range 80 180-degree cone
    EsotericScrivening = 44210, // Boss->self, 6.0s cast, single-target
    Shockwave = 44211, // Helper->self, 5.2s cast, range 60 circle
    TranscendentUnionCast = 44212, // Boss->self, 5.0s cast, single-target
    ElementalEdge = 44289, // Helper->self, no cast, range 60 circle
    TranscendentUnion = 44213, // Helper->self, no cast, range 60 circle
    EsotericPalisade = 44214, // Boss->self, 3.0s cast, single-target
    CrystallineResonance = 44215, // Boss->self, 3.0s cast, single-target
    ElementalResonance = 44216, // Helper->self, 7.0s cast, range 18 circle
    EmpyrealBanishIII = 44223, // Helper->players, 5.0s cast, range 5 circle
    IllumedFacet = 44217, // Boss->self, 3.0s cast, single-target
    IllumedEstoc = 44218, // _Gen_Kamlanaut->self, 8.0s cast, range 120 width 13 rect
    ShieldBash = 44222, // Boss->self, 7.0s cast, range 60 circle
    EmpyrealBanishIVCast = 44907, // Boss->self, 4.0+1.0s cast, single-target
    EmpyrealBanishIV = 44224, // Helper->players, 5.0s cast, range 5 circle

    Unk1 = 44409, // Boss->self, no cast, single-target
    Unk2 = 44410, // Boss->self, no cast, single-target
    Unk3 = 41727, // Helper->Boss, no cast, single-target
}

public enum IconID : uint
{
    TankbusterKnockback = 613, // Boss->player
    Spread = 376, // player->self
    Stack = 93, // player->self
}
