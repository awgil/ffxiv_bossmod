namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

public enum OID : uint
{
    Boss = 0x38FA, // R5.997, x1
    Helper = 0x233C, // R0.500, x30
};

public enum AID : uint
{
    AutoAttack = 28888, // Boss->player, no cast, single-target
    DestructiveStrike = 28889, // Boss->player, 5.0s cast, range 13 ?-degree cone, tankbuster
    DestructiveCharge = 28890, // Boss->self, no cast, single-target, visual (lighting orbs)
    DestructiveChargeAOE = 28891, // Helper->self, no cast, range 25 90-degree cone
    Boltloop = 28892, // Boss->self, 2.0s cast, single-target, visual (expanding circles)
    BoltloopAOE1 = 28893, // Helper->self, 3.0s cast, range 10 circle
    BoltloopAOE2 = 28894, // Helper->self, 5.0s cast, range 10-20 donut
    BoltloopAOE3 = 28895, // Helper->self, 7.0s cast, range 20-30 donut
    DestructiveStatic = 28896, // Boss->self, 8.0s cast, range 50 180-degree cone
    LightningBolt = 28897, // Boss->self, 3.0s cast, single-target, visual (puddles)
    LightningBoltAOE = 28898, // Helper->location, 3.0s cast, range 6 circle puddle
    BoltsFromTheBlue = 28899, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    BoltsFromTheBlueAOE = 28900, // Helper->self, no cast, range 25 circle, raidwide
};
