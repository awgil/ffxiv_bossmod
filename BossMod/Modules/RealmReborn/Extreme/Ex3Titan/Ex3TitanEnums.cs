namespace BossMod.RealmReborn.Extreme.Ex3Titan;

public enum OID : uint
{
    Boss = 0xF8, // R5.250, x1
    Helper = 0x1B2, // R0.500, x14
    GaolMarkerHelper = 0x8EE, // R0.500, x1
    BombBoulder = 0x5E0, // R1.300, spawn during fight
    GraniteGaol = 0x5E2, // R1.800, spawn during fight
    GraniteGaoler = 0x8F2, // R1.900, spawn during fight
    TitansHeart = 0x5E5, // R5.250, Part type, spawn during fight
    GaolerVoidzone = 0x1E8F8F, // R0.500, EventObj type, spawn during fight (voidzone left by killed gaolers)
};

public enum AID : uint
{
    AutoAttackBoss = 872, // Boss->player, no cast, single-target
    RockBuster = 1463, // Boss->self, no cast, range 6+R ?-degree cone cleave (during heart phase)
    MountainBuster = 1464, // Boss->self, no cast, range 16+R ?-degree cone cleave applying stacking vuln (outside heart phase)
    TumultBoss = 1465, // Boss->self, no cast, raidwide (x4)
    Upheaval = 1466, // Boss->self, 5.0s cast, raidwide, knockback 13
    LandslideBoss = 1467, // Boss->self, 2.2s cast, range 35+R width 6 rect aoe, knockback 15
    LandslideHelper = 1468, // Helper->self, 2.2s cast, range 35+R width 6 rect aoe, knockback 15
    WeightOfTheLand = 1469, // Boss->self, 2.0s cast, single-target, visual
    WeightOfTheLandAOE = 1470, // Helper->location, 2.5s cast, range 6 circle baited puddle
    Geocrush = 1472, // Boss->self, 3.0s cast, raidwide with ? falloff (on phase switch)
    EarthenFury = 1473, // Boss->self, no cast, raidwide (after heart phase)
    Enrage = 1474, // Boss->self, 10.0s cast, range 100 circle

    GaolMarkerHealer = 1652, // GaolMarkerHelper->player, no cast, single-target, visual (indicates player will be gaoled)
    RockThrow = 645, // Boss->player, no cast, single-target, visual (indicates player will be gaoled)
    GraniteSepulchre = 1477, // GraniteGaol->self, 15.0s cast, kills gaoled player if not killed in time

    Bury = 1051, // BombBoulder->self, no cast, range 3+R circle aoe
    Burst = 1471, // BombBoulder->self, 5.0s cast, range 5+R circle aoe

    AutoAttackGaoler = 1651, // GraniteGaoler->player, no cast, single-target
    LandslideGaoler = 1475, // GraniteGaoler->self, 2.2s cast, range 35+R width 6 rect aoe, knockback 15
    TumultGaoler = 1476, // GraniteGaoler->self, no cast, raidwide
};

public enum SID : uint
{
    PhysicalVulnerabilityUp = 126, // Boss->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
    Fetters = 292, // none->player, extra=0x0
};

public enum TetherID : uint
{
    Gaol = 7, // player->player
};
