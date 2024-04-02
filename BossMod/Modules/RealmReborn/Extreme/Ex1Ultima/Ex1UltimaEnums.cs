namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

public enum OID : uint
{
    Boss = 0x90F, // R6.000, x1
    UltimaIfrit = 0x910, // R5.000, x1
    UltimaTitan = 0x911, // R5.250, x1
    UltimaGaruda = 0x912, // R3.400, x1
    Aetheroplasm = 0x913, // R1.000, more spawn during fight, orbs that can be kited
    Ultimaplasm = 0x914, // R1.000, more spawn during fight, aetheric boom orbs
    MagitekBit = 0x915, // R0.600, more spawn during fight
    Helper = 0x1B2, // R0.500, x19
    AetheroplasmHelper = 0x8EE, // R0.500, x3
};

public enum AID : uint
{
    AutoAttack = 1420, // Boss->player, no cast, range 2 circle cleave
    ViscousAetheroplasm = 1503, // Boss->player, no cast, range 2 circle cleave, stacking debuff
    ViscousAetheroplasmKill = 1546, // Helper->player, no cast, single-target, if debuff reaches 5 stacks

    CeruleumVent = 1504, // Boss->self, no cast, range 8+R circle aoe
    HomingLasers = 1505, // Boss->player, no cast, range 4 circle aoe around random target
    DiffractiveLaser = 1506, // Boss->self, no cast, range 12+R 120-degree cone cleave (after primal phases end)
    MagitekRayCenter = 1507, // Helper->self, 2.2s cast, range 40+R width 6 rect
    MagitekRayRight = 1508, // Helper->self, 2.2s cast, range 40+R width 6 rect
    MagitekRayLeft = 1509, // Helper->self, 2.2s cast, range 40+R width 6 rect
    TankPurge = 1514, // Boss->self, 3.5s cast, raidwide
    Ultima = 1516, // Boss->self, 11.0s cast, enrage

    MistralSong = 1517, // UltimaGaruda->self, 3.0s cast, range 20+R 150-degree cone
    VulcanBurst = 1518, // Boss->self, no cast, range 16+R circle, knockback 30
    EyeOfTheStorm = 1519, // Helper->self, 3.0s cast, range 12-25 donut aoe
    Geocrush = 1520, // UltimaTitan->self, 4.0s cast, range 25 circle aoe with ? falloff

    RadiantPlume = 1521, // Helper->location, 3.0s cast, range 8 circle aoe
    WeightOfTheLand = 1522, // Helper->location, 3.0s cast, range 6 circle aoe

    Eruption = 1524, // Helper->location, 3.0s cast, range 8 circle aoe
    CrimsonCyclone = 1525, // UltimaIfrit->self, 3.0s cast, range 38+R width 12 rect aoe

    AethericBoom = 1511, // Boss->self, 4.0s cast, range 40 circle knockback 30
    AetheroplasmBoom = 1512, // Ultimaplasm->self, no cast, range 8 circle shared aoe
    FusionBurst = 1513, // Ultimaplasm->self, no cast, wipe if orbs touch
    OrbFixate = 1650, // AetheroplasmHelper->player, no cast, single-target, orb fixate icon
    AetheroplasmFixated = 1510, // Aetheroplasm->self, no cast, range 6 circle aoe

    AssaultCannon = 1526, // MagitekBit->self, 2.5s cast, range 45+R width 2 rect aoe
    Freefire = 1515, // Helper->self, no cast, range 40 circle aoe with 15 falloff
};

public enum SID : uint
{
    ViscousAetheroplasm = 369, // Boss->player, extra=0x1/0x2/0x3/0x4/0x5
}
