namespace BossMod.Endwalker.Alliance.A32Llymlaen;

public enum OID : uint
{
    Boss = 0x4024, // R7.000, x1
    Perykos = 0x4026, // R6.300, x1
    Thalaos = 0x4027, // R6.300, x1
    Helper = 0x233C, // R0.500, x25, 523 type
    SeaFoam = 0x4029, // R1.500, spawn during fight
    Trident = 0x4025, // R3.000, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    Teleport = 34825, // Boss->location, no cast, single-target
    Tempest = 34827, // Boss->self, 5.0s cast, range 100 circle
    WindRose = 34828, // Boss->self, 6.0s cast, range 12 circle
    SeafoamSpiral = 34829, // Boss->self, 6.0s cast, range 6-70 donut
    DeepDiveNormal = 34841, // Boss->players, 5.0s cast, range 6 circle, stack

    NavigatorsTrident = 34830, // Boss->self, 6.5s cast, single-target, visual (left/right cleave, then knockback to the sides)
    NavigatorsTridentAOE = 34833, // Helper->self, 7.0s cast, range 60 width 60 rect, knockback 20 to the sides
    NavigatorsTridentSpawnSeaFoam1 = 36043, // Boss->self, no cast, single-target, visual (spawn seafoam adds)
    NavigatorsTridentSpawnSeaFoam2 = 36044, // Boss->self, no cast, single-target, visual (spawn seafoam adds) - no idea what's the difference between these two
    NavigatorsTridentTeleportStart1 = 36047, // Boss->self, no cast, single-target, visual (start moving to the side)
    NavigatorsTridentTeleportStart2 = 34859, // Boss->self, no cast, single-target, visual (start moving to the side) - no idea what's the difference between these two
    NavigatorsTridentTeleportEnd = 36048, // Boss->self, no cast, single-target, visual (finish moving to the side)
    DireStraitsVisualFirst = 36045, // Helper->self, 2.5s cast, range 40 width 80 rect, visual (telegraph for first cleave)
    DireStraitsVisualSecond = 36046, // Helper->self, 4.5s cast, range 40 width 80 rect, visual (telegraph for second cleave)
    DireStraitsAOEFirst = 34831, // Helper->self, 7.5s cast, range 40 width 80 rect
    DireStraitsAOESecond = 34832, // Helper->self, 9.3s cast, range 40 width 80 rect
    SurgingWave = 34834, // Boss->location, 9.0s cast, single-target, visual (knockback to the side corridor)
    SurgingWaveAOE = 34835, // Helper->self, 10.0s cast, range 6 circle, instant kill
    SurgingWaveShockwave = 34836, // Helper->self, 10.2s cast, range 100 circle, knockback 68, away from source
    SurgingWaveFrothingSea = 34826, // Helper->self, no cast, range 25 width 100 rect, knockback 15, dir forward
    SphereShatter = 34861, // SeaFoam->player, no cast, single-target, when seafoam orb is touched
    LeftStrait = 34897, // Boss->self, 6.0s cast, range 60 180-degree cone, half-arena cleave
    RightStrait = 34898, // Boss->self, 6.0s cast, range 60 180-degree cone, half-arena cleave
    ToTheLast = 34837, // Boss->self, 5.0s cast, single-target, visual (side cleaves sequence)
    ToTheLastRepeat = 34839, // Boss->self, no cast, single-target, visual (subsequent cleaves)
    ToTheLastVisual = 34838, // Helper->self, 1.0s cast, range 80 width 10 rect, visual (telegraph)
    ToTheLastAOE = 34840, // Helper->self, 6.0s cast, range 80 width 10 rect

    TorrentialTridents = 34842, // Boss->self, 4.0s cast, single-target, visual (staggered tridents)
    TorrentialTridentLanding = 34843, // Trident->self, no cast, range 80 circle, 6x raidwide when tridents spawn
    TorrentialTridentAOE = 34844, // Trident->self, 6.0s cast, range 18 circle
    StormySeas = 34845, // Boss->self, no cast, single-target, visual (puddles + spreads)
    Stormwhorl = 34846, // Helper->location, 4.0s cast, range 6 circle
    Stormwinds = 34847, // Helper->players, 5.0s cast, range 6 circle, spread

    DenizensOfTheDeep = 34848, // Boss->self, 4.0s cast, single-target, visual (summons adds)
    SerpentsTide = 34853, // Helper->self, 8.0s cast, range 80 width 20 rect, visual (aoe)
    SerpentsTideAOEPerykosNS = 34854, // Perykos->self, no cast, range 80 width 20 rect
    SerpentsTideAOEPerykosEW = 34855, // Perykos->self, no cast, range 80 width 20 rect
    SerpentsTideAOEThalaosNS = 34856, // Thalaos->self, no cast, range 80 width 20 rect
    SerpentsTideAOEThalaosEW = 34857, // Thalaos->self, no cast, range 80 width 20 rect
    Maelstrom = 34858, // Helper->location, 4.0s cast, range 6 circle
    Godsbane = 34849, // Boss->self, 5.0s cast, single-target, visual (raidwide + bleed)
    GodsbaneAOE = 35948, // Helper->self, 7.0s cast, range 100 circle, raidwide
    GodsbaneVisualPerykos = 34850, // Perykos->self, 5.0s cast, single-target, visual (animation)
    GodsbaneVisualThalaos = 34851, // Thalaos->self, 5.0s cast, single-target, visual (animation)
    GodsbaneVisual = 34852, // Helper->self, 2.0s cast, single-target, visual (animation)
    DeepDiveHardWater = 34868, // Boss->players, 9.0s cast, range 6 circle, stack
    HardWaterPerykos = 34869, // Perykos->players, 5.0s cast, range 6 circle, stack
    HardWaterThalaos = 34870, // Thalaos->players, 7.0s cast, range 6 circle, stack

    BlowkissPunishment = 34874, // Boss->player, no cast, single-target, knockback 10 away from source + down for the count, used on players using specific emotes on boss
    NavigatorsDagger = 34875, // Boss->player, no cast, single-target, used on players using specific emotes on boss
}

public enum IconID : uint
{
    DeepDive = 161, // player
    Stormwinds = 139, // player
    HardWater = 305, // player
}
