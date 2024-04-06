namespace BossMod.Endwalker.Alliance.A32Llymlaen;

public enum OID : uint
{
    Boss = 0x4024, // R7.000, x1
    Helper = 0x233C, // R0.500, x25, 523 type
    Thalaos = 0x4027, // R6.300, x1
    Perykos = 0x4026, // R6.300, x1
    SeaFoam = 0x4029, // R1.500, spawn during fight
    Trident = 0x4025, // R3.000, spawn during fight
};

public enum AID : uint
{
    AutoAttack = 871, // Llymlaen->player, no cast, single-target

    Tempest = 34827, // Llymlaen->self, 5.0s cast, range 100 circle
    Landing = 34843, // Trident->self, no cast, range 80 circle, 6x raidwide when tridents spawn
    Godsbane = 35948, // LlymlaenHelper->self, 7.0s cast, range 100 circle

    SeafoamSpiral = 34829, // Llymlaen->self, 6.0s cast, range 6-70 donut

    WindRose = 34828, // Llymlaen->self, 6.0s cast, range 12 circle

    DireStraitsVisual = 34825, // Llymlaen->location, no cast, single-target
    DireStraitsVisual2 = 36043, // Llymlaen->self, no cast, single-target
    DireStraitsVisual3 = 36044, // Llymlaen->self, no cast, single-target
    DireStraitsVisual4 = 36047, // Llymlaen->self, no cast, single-target

    DireStraitTelegraph1 = 36045, // LlymlaenHelper->self, 2.5s cast, range 40 width 80 rect
    DireStraitTelegraph2 = 36046, // LlymlaenHelper->self, 4.5s cast, range 40 width 80 rect
    DireStraitsRectAOE1 = 34831, // LlymlaenHelper->self, 7.5s cast, range 40 width 80 rect
    DireStraitsRectAOE2 = 34832, // LlymlaenHelper->self, 9.3s cast, range 40 width 80 rect

    NavigatorsTridentVisual1 = 34859, // Llymlaen->self, no cast, single-target      
    NavigatorsTridentVisual2 = 34830, // Llymlaen->self, 6.5s cast, single-target
    NavigatorsTridentVisual3 = 36048, // Llymlaen->self, no cast, single-target
    NavigatorsTridentRectAOE = 34833, // LlymlaenHelper->self, 7.0s cast, range 60 width 60 rect, damage fall off rectangle, knockback 20 left/right, depending on player position

    SurgingWaveVisual = 34834, // Llymlaen->location, 9.0s cast, single-target
    SurgingWaveAOE = 34835, // LlymlaenHelper->self, 10.0s cast, range 6 circle, instant kill
    Shockwave = 34836, // LlymlaenHelper->self, 10.2s cast, range 100 circle, knockback 68, away from source
    SphereShatter = 34861, // SeaFoam->player, no cast, single-target

    FrothingSeaRectAOE = 34826, // LlymlaenHelper->self, no cast, range 25 width 100 rect, knockback 15, dir forward

    RightStraitCone = 34898, // Llymlaen->self, 6.0s cast, range 60 180-degree cone
    LeftStraitCone = 34897, // Llymlaen->self, 6.0s cast, range 60 180-degree cone

    DeepDiveStack1 = 34841, // Llymlaen->players, 5.0s cast, range 6 circle, Stack
    DeepDiveStack2 = 34868, // Llymlaen->players, 9.0s cast, range 6 circle, Stack
    HardWaterStack1 = 34869, // Perykos->players, 5.0s cast, range 6 circle, Stack
    HardWaterStack2 = 34870, // Thalaos->players, 7.0s cast, range 6 circle, Stack
    StormwindsSpread = 34847, // LlymlaenHelper->players, 5.0s cast, range 6 circle, spread

    TorrentialTridents = 34842, // Llymlaen->self, 4.0s cast, single-target
    LandingCircle = 34844, // Trident->self, 6.0s cast, range 18 circle
    StormySeas = 34845, // Llymlaen->self, no cast, single-target
    Stormwhorl = 34846, // LlymlaenHelper->location, 4.0s cast, range 6 circle

    DenizensOfTheDeep = 34848, // Llymlaen->self, 4.0s cast, single-target, summons adds

    SerpentsTideRectAOE1 = 34855, // Perykos->self, no cast, range 80 width 20 rect
    SerpentsTideRectAOE2 = 34857, // Thalaos->self, no cast, range 80 width 20 rect
    SerpentsTideRectAOE3 = 34854, // Perykos->self, no cast, range 80 width 20 rect
    SerpentsTideRectAOE4 = 34856, // Thalaos->self, no cast, range 80 width 20 rect
    SerpentsTideVisual = 34853, // LlymlaenHelper->self, 8.0s cast, range 80 width 20 rect

    Maelstrom = 34858, // LlymlaenHelper->location, 4.0s cast, range 6 circle
    Godsbane1 = 34852, // LlymlaenHelper->self, 2.0s cast, single-target
    Godsbane2 = 34849, // Llymlaen->self, 5.0s cast, single-target
    Godsbane3 = 34850, // Perykos->self, 5.0s cast, single-target
    Godsbane4 = 34851, // Thalaos->self, 5.0s cast, single-target

    ToTheLastVisual = 34837, // Llymlaen->self, 5.0s cast, single-target
    ToTheLastVisual2 = 34839, // Llymlaen->self, no cast, single-target
    ToTheLastTelegraph = 34838, // LlymlaenHelper->self, 1.0s cast, range 80 width 10 rect
    ToTheLastAOE = 34840, // LlymlaenHelper->self, 6.0s cast, range 80 width 10 rect
    //these only happen if a player uses the blowkiss emote on Llymlaen, stun+heavy dmg, not our fault if people do that
    BlowkissPunishment = 34874, // Llymlaen->player, no cast, single-target, knockback 10 away from source + down for the count
    NavigatorsDagger = 34875, // Llymlaen->player, no cast, single-target
};
