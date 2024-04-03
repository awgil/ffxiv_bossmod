namespace BossMod.Endwalker.Alliance.A32Llymlaen;

public enum OID : uint
{
    Boss = 0x4024, // R7.000, x1
    LlymlaenHelper = 0x233C, // R0.500, x25, 523 type
    Thalaos = 0x4027, // R6.300, x1
    Perykos = 0x4026, // R6.300, x1
    SeaFoam = 0x4029, // R1.500, spawn during fight
    Trident = 0x4025, // R3.000, spawn during fight
    OschonsAvatar = 0x406E, // R8.000, spawn during fight
    Unknown = 0x400E, // R0.500, x1
};

public enum AID : uint
{
    AutoAttack = 871, // Llymlaen->player, no cast, single-target

    TempestRaidwide = 34827, // Llymlaen->self, 5.0s cast, range 100 circle
    ShockwaveRaidwide = 34836, // LlymlaenHelper->self, 10.2s cast, range 100 circle
    LandingRaidwide = 34843, // Trident->self, no cast, range 80 circle
    GodsbaneRaidwide = 35948, // LlymlaenHelper->self, 7.0s cast, range 100 circle

    SeafoamSpiralDonut = 34829, // Llymlaen->self, 6.0s cast, range 6-70 donut

    WindRoseAOE = 34828, // Llymlaen->self, 6.0s cast, range 12 circle

    DireStraitAltVisual = 34825, // Llymlaen->location, no cast, single-target
    DireStraitAltRectAOE1 = 36045, // LlymlaenHelper->self, 2.5s cast, range 40 width 80 rect
    DireStraitAltRectAOE2 = 36046, // LlymlaenHelper->self, 4.5s cast, range 40 width 80 rect

    DireStraitsVisual = 36044, // Llymlaen->self, no cast, single-target
    DireStraitsRectAOE1 = 34831, // LlymlaenHelper->self, 7.5s cast, range 40 width 80 rect
    DireStraitsRectAOE2 = 34832, // LlymlaenHelper->self, 9.3s cast, range 40 width 80 rect
    DireStraits4 = 36043, // Llymlaen->self, no cast, single-target

    NavigatorsTridentVisual1 = 34859, // Llymlaen->self, no cast, single-target      
    NavigatorsTridentVisual2 = 34830, // Llymlaen->self, 6.5s cast, single-target
    NavigatorsTridentVisual3 = 36048, // Llymlaen->self, no cast, single-target
    NavigatorsTridentRectAOE = 34833, // LlymlaenHelper->self, 7.0s cast, range 60 width 60 rect, knockback 20 left/right, depending on playeer position

    SurgingWaveKnockback = 34834, // Llymlaen->location, 9.0s cast, single-target // Knockback
    SurgingWaveAOE = 34835, // LlymlaenHelper->self, 10.0s cast, range 6 circle // CircleAOE
    SphereShatter = 34861, // SeaFoam->player, no cast, single-target // Sphere pop

    FrothingSeaRectAOE = 34826, // LlymlaenHelper->self, no cast, range 25 width 100 rect // RectAOE

    // one these are cast after Llymlaen shoves everyone back after parting the water
    RightStraitCone = 34898, // Llymlaen->self, 6.0s cast, range 60 180-degree cone // ConeCleaves
    LeftStraitCone = 34897, // Llymlaen->self, 6.0s cast, range 60 180-degree cone // ConeCleaves

    DeepDiveStack1 = 34841, // Llymlaen->players, 5.0s cast, range 6 circle // Stack
    DeepDiveStack2 = 34868, // Llymlaen->players, 9.0s cast, range 6 circle // Stack
    HardWaterStack1 = 34869, // Perykos->players, 5.0s cast, range 6 circle // Stack
    HardWaterStack2 = 34870, // Thalaos->players, 7.0s cast, range 6 circle // Stack

    TorrentialTridents = 34842, // Llymlaen->self, 4.0s cast, single-target
    LandingAOE = 34844, // Trident->self, 6.0s cast, range 18 circle // CircleAOE
    StormySeas = 34845, // Llymlaen->self, no cast, single-target
    StormwhorlLocAOE = 34846, // LlymlaenHelper->location, 4.0s cast, range 6 circle // LocationAOE
    StormwindsSpread = 34847, // LlymlaenHelper->players, 5.0s cast, range 6 circle // Spread

    DenizensOfTheDeep = 34848, // Llymlaen->self, 4.0s cast, single-target // Summons adds

    SerpentsTideRectAOE1 = 34855, // Perykos->self, no cast, range 80 width 20 rect // RectAOE
    SerpentsTideRectAOE2 = 34857, // Thalaos->self, no cast, range 80 width 20 rect // RectAOE
    SerpentsTideRectAOE3 = 34854, // Perykos->self, no cast, range 80 width 20 rect // RectAOE
    SerpentsTideRectAOE4 = 34856, // Thalaos->self, no cast, range 80 width 20 rect // RectAOE
    SerpentsTideRectAOE5 = 34853, // LlymlaenHelper->self, 8.0s cast, range 80 width 20 rect // RectAOE
    SerpentsTideRectAOE6 = 34838, // LlymlaenHelper->self, 1.0s cast, range 80 width 10 rect // RectAOE // This one does not fully cover the section of the arena, this is intentional to allow a safespot if youre willing to use antiknock

    MaelstromLocAOE = 34858, // LlymlaenHelper->location, 4.0s cast, range 6 circle // LocationAOE
    Godsbane1 = 34852, // LlymlaenHelper->self, 2.0s cast, single-target
    Godsbane2 = 34849, // Llymlaen->self, 5.0s cast, single-target
    Godsbane3 = 34850, // Perykos->self, 5.0s cast, single-target
    Godsbane4 = 34851, // Thalaos->self, 5.0s cast, single-target

    _Ability6_ = 36047, // Llymlaen->self, no cast, single-target
    ToTheLast1 = 34837, // Llymlaen->self, 5.0s cast, single-target
    ToTheLast2 = 34839, // Llymlaen->self, no cast, single-target
    ToTheLastRectAOE = 34840, // LlymlaenHelper->self, 6.0s cast, range 80 width 10 rect // RectAOE

    BlowkissPunishment = 34874, // Llymlaen->player, no cast, single-target, knockback 10 away from source + down for the count
    NavigatorsDagger = 34875, // Llymlaen->player, no cast, single-target, damage after /blowkiss emote
};

public enum SID : uint
{
    SeafoamStatus = 2234, // none->SeaFoam, extra=0x14
    DownForTheCount = 783, // LlymlaenHelper->player, extra=0xEC7
    Liftoff = 3262, // SeaFoam->player, extra=0x0
    WindResistanceDownII = 2096, // LlymlaenHelper->player, extra=0x0
    Dropsy1 = 3777, // none->player, extra=0x0
    Dropsy2 = 3778, // none->player, extra=0x0
    Dropsy3 = 2087, // LlymlaenHelper->player, extra=0x0
};

public enum IconID : uint
{
    DiveStack = 161, // player
    StormwindBait = 139, // player
    HardWaterStack = 305, // player
};
