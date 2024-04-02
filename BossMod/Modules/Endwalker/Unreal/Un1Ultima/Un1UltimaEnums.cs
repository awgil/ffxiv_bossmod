namespace BossMod.Endwalker.Unreal.Un1Ultima;

public enum OID : uint
{
    Boss = 0x385B,
    Ifrit = 0x385C, // x1
    Titan = 0x385D, // x1
    Garuda = 0x385E, // x1
    Aetheroplasm = 0x385F, // spawn mid fight, orbs that can be kited
    Ultimaplasm = 0x3860, // spawn mid fight, aetheric boom orbs
    MagitekBit = 0x3861, // spawn mid fight
    Helper = 0x3867, // x19
    AetheroplasmHelper = 0x3868, // x3
};

public enum AID : uint
{
    AutoAttack = 28426, // cleave range 2
    ViscousAetheroplasm = 28398, // Boss->mt, no cast, stacking debuff, range 2 cleave
    ViscousAetheroplasmKill = 28422, // Boss->target, no cast, if debuff reaches 5 stacks
    CeruleumVent = 28399, // Boss->self, no cast, range 8 aoe (around self)
    HomingLasers = 28400, // Boss->target, no cast, range 4 aoe (around random target)
    DiffractiveLaser = 28401, // Boss->self, no cast, range 12 120-degree (?) cone (should only hit mt)
    MagitekRayCenter = 28402, // Helper->self, 2.2s cast, range 40 half-width 3 rect
    MagitekRayRight = 28403, // Helper->self, 2.2s cast, range 40 half-width 3 rect
    MagitekRayLeft = 28404, // Helper->self, 2.2s cast, range 40 half-width 3 rect
    TankPurge = 28409, // Boss->self, 3.5s cast, raidwide

    MistralSong = 28412, // Garuda->self, 3s cast, range 20 150-degree (?) cone
    VulcanBurst = 28413, // Boss->self, no cast, knockback 30
    EyeOfTheStorm = 28414, // Helper->self, 3s cast, range 25 donut aoe, 15? inner
    Geocrush = 28415, // Titan->self, 4s cast, range 25 aoe with ? falloff

    RadiantPlume = 28416, // Helper->location, 3s cast, range 8 aoe
    WeightOfTheLand = 28417, // Helper->location, 3s cast, range 6 aoe

    Eruption = 28419, // Helper->location, 3s cast, range 8 aoe
    CrimsonCyclone = 28420, // Ifrit->self, 3s cast, range 38 half-width 6 rect

    AethericBoom = 28406, // Boss->self, 4s cast, knockback 30
    AetheroplasmBoom = 28407, // Ultimaplasm->self, no cast, range 8 shared-damage aoe
    FusionBurst = 28408, // Ultimaplasm->self, no cast, wipe if orbs touch
    OrbFixate = 28423, // AetheroplasmHelper->target, no cast, target select for fixate
    AetheroplasmFixated = 28405, // Aetheroplasm->self, no cast, range 6 aoe

    AssaultCannon = 28421, // MagitekBit->self, 2.5s cast, range 45 half-width 1 rect
    Detonation = 28410, // Helper->self, no cast, range 40 aoe with ? falloff (TODO: don't know how to detect in advance...)
};

public enum SID : uint
{
    None = 0,
    ViscousAetheroplasm = 369,
}

public enum TetherID : uint
{
    None = 0,
}

public enum IconID : uint
{
    None = 0,
}
