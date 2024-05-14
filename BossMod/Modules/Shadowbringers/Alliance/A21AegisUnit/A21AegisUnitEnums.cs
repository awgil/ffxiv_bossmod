namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

public enum OID : uint
{
    Boss = 0x2EA2, // R17.100, x1
    AegisUnitHelper = 0x233C, // R0.500, x18, 523 type
    FlightUnit = 0x2ECB, // R2.800, x6
}

public enum AID : uint
{
    Attack = 20924, // AegisUnitHelper->player, no cast, single-target
    FiringOrderAntiPersonnelLaser = 20621, // Boss->self, 3.0s cast, single-target
    AntiPersonnelLaser = 20624, // AegisUnitHelper->players, no cast, range 3 circle
    UnknownAbility1 = 20601, // Boss->self, no cast, single-target
    ManeuverBeamCannons = 20595, // Boss->self, 12.0s cast, single-target
    BeamCannons1 = 20597, // AegisUnitHelper->self, no cast, range 40 ?-degree cone
    BeamCannons2 = 20598, // AegisUnitHelper->self, no cast, range 40 ?-degree cone
    BeamCannons3 = 20596, // AegisUnitHelper->self, no cast, range 40 ?-degree cone
    ManeuverColliderCannons1 = 20603, // Boss->self, 7.0s cast, single-target
    ManeuverColliderCannons2 = 20605, // Boss->self, 8.0s cast, single-target
    ColliderCannons = 20606, // AegisUnitHelper->self, no cast, range 40 ?-degree cone
    FiringOrderSurfaceLaser = 20622, // Boss->self, 3.0s cast, single-target
    AerialSupportSwoop = 20690, // Boss->self, 3.0s cast, single-target
    SurfaceLaser1 = 20626, // AegisUnitHelper->location, no cast, range 4 circle
    SurfaceLaser2 = 20625, // AegisUnitHelper->location, no cast, single-target
    FlightPath = 20620, // FlightUnit->self, 3.0s cast, range 60 width 10 rect
    ManeuverRefractionCannons1 = 20607, // Boss->self, 6.0s cast, single-target
    ManeuverRefractionCannons2 = 20608, // Boss->self, 6.0s cast, single-target
    RefractionCannons = 20609, // AegisUnitHelper->self, no cast, range 40 ?-degree cone
    ManeuverDiffusionCannon = 20633, // Boss->self, 6.0s cast, range 60 circle
    AerialSupportBombardment = 20691, // Boss->self, 3.0s cast, single-target
    FiringOrderHighPoweredLaser = 20623, // Boss->self, 3.0s cast, single-target
    HighPoweredLaser = 20627, // AegisUnitHelper->players, no cast, range 6 circle
    UnknownAbility2 = 21426, // AegisUnitHelper->self, no cast, single-target
    LifesLastSong = 21427, // AegisUnitHelper->self, 7.5s cast, range 30 ?-degree cone
    UnknownAbility3 = 20602, // Boss->self, no cast, single-target
    ManeuverSaturationBombing = 20631, // FlightUnit->self, 25.0s cast, range 60 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // AegisUnitHelper/FlightUnit->player, extra=0x1/0x2
    MagicVulnerabilityUp = 2091, // AegisUnitHelper->player, extra=0x0
    Hover = 2390, // none->Boss, extra=0x1F4
}

public enum IconID : uint
{
    Icon_198 = 198, // player
    Icon_23 = 23, // player
    Icon_62 = 62, // player
}
