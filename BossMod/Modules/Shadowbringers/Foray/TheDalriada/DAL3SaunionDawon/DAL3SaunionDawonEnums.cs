namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

public enum OID : uint
{
    Boss = 0x31DD, // R7.875, x1
    Dawon = 0x31DE, // R6.900, x1
    Helper = 0x233C, // R0.500, x24, 523 type

    VerdantPlume = 0x31E1, // R0.500, x0 (spawn during fight)
    VermilionFlame = 0x31E0, // R0.750, x0 (spawn during fight)
    Actor1eb217 = 0x1EB217, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb216 = 0x1EB216, // R0.500, x0 (spawn during fight), EventObj type
    Actor33df = 0x33DF, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    AutoAttackDawon = 6497, // Dawon->player, no cast, single-target

    AetherialDistribution1 = 25060, // Boss->Dawon, no cast, single-target
    AetherialDistribution2 = 25061, // Dawon->Boss, no cast, single-target

    AntiPersonnelMissile = 24002, // Helper->players, 5.0s cast, range 6 circle
    Explosion = 24015, // VerdantPlume->self, 1.5s cast, range ?-12 donut
    FireBrand = 24012, // Dawon->self, no cast, range 50 width 14 cross

    FrigidPulse = 24011, // Dawon->self, no cast, range ?-60 donut
    FrigidPulseAOE = 24701, // Dawon->self, 5.0s cast, range ?-60 donut

    HighPoweredMagitekRay = 24005, // Boss->player, 5.0s cast, range 5 circle // tankbuster
    MagitekHalo = 23989, // Boss->self, 5.0s cast, range ?-60 donut // Hitbox donut
    MagitekCrossray = 23991, // Boss->self, 5.0s cast, range 60 width 19 cross // A large cross AoE

    MissileCommand = 24001, // Boss->self, 4.0s cast, single-target // Fires AoEs targeted at specific players followed by a stack marker on another player
    MissileSalvo = 24003, // Helper->players, 5.0s cast, range 6 circle

    MobileCrossrayAOE = 23992, // Boss->self, no cast, range 60 width 19 cross
    MobileCrossray1 = 23997, // Boss->self, 7.0s cast, single-target
    MobileCrossray2 = 23998, // Boss->self, 7.0s cast, single-target
    MobileCrossray3 = 23999, // Boss->self, 7.0s cast, single-target
    MobileCrossray4 = 24000, // Boss->self, 7.0s cast, single-target

    MobileHaloAOE = 23990, // Boss->self, no cast, range ?-60 donut
    MobileHalo1 = 23993, // Boss->self, 7.0s cast, single-target
    MobileHalo2 = 23994, // Boss->self, 7.0s cast, single-target
    MobileHalo3 = 23995, // Boss->self, 7.0s cast, single-target
    MobileHalo4 = 23996, // Boss->self, 7.0s cast, single-target

    Obey = 24009, // Dawon->self, 11.0s cast, single-target

    Pentagust = 24017, // Dawon->self, 5.0s cast, single-target
    PentagustAOE = 24018, // Helper->self, 5.0s cast, range 50 20-degree cone//

    RawHeat = 24014, // VermilionFlame->self, 1.5s cast, range 10 circle

    SpiralScourge = 23986, // Boss->self, 7.0s cast, single-target
    SpiralScourgeAOE = 23987, // Helper->location, no cast, range 13 circle

    SurfaceMissile = 24004, // Helper->location, 5.0s cast, range 6 circle//

    SwoopingFrenzy = 24010, // Dawon->location, no cast, range 12 circle
    SwoopingFrenzyAOE = 24016, // Dawon->location, 4.0s cast, range 12 circle

    ToothAndTalon = 24020, // Dawon->player, 5.0s cast, single-target // Tankbuster

    Touchdown1 = 24006, // Dawon->self, 6.0s cast, ???
    Touchdown2 = 24007, // Helper->self, no cast, ???
    Touchdown3 = 24912, // Helper->self, 6.0s cast, range 60 circle

    UnknownAbility = 24023, // Boss->location, no cast, single-target
    UnknownWeaponskill = 23988, // Boss->self, no cast, single-target

    WildfireWinds1 = 24013, // Dawon->self, 5.0s cast, ???
    WildfireWinds2 = 24772, // Helper->self, no cast, ???
}

public enum SID : uint
{
    UnknownStatus = 2056, // none->Boss, extra=0x12E/0x12D
    DownForTheCount = 2408, // Dawon/Helper->player, extra=0xEC7
    OneMind = 2553, // none->Boss/Dawon, extra=0x0
    VulnerabilityUp = 1789, // VerdantPlume/Helper/Dawon->player, extra=0x1/0x2/0x3/0x4/0x5
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Spreadmarker = 139, // player
    Stackmarker = 62, // player
}

public enum TetherID : uint
{
    Tether12 = 12, // Boss->Dawon
}
