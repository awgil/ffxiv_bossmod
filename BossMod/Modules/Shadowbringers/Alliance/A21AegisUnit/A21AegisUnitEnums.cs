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

    ManeuverBeamCannons = 20595, // Boss->self, 12.0s cast, single-target //Six evenly-spaced red cone AoEs
    BeamCannons1 = 20596, // AegisUnitHelper->self, no cast, range 40 ?-degree cone
    BeamCannons2 = 20597, // AegisUnitHelper->self, no cast, range 40 ?-degree cone
    BeamCannons3 = 20598, // AegisUnitHelper->self, no cast, range 40 ?-degree cone

    UnknownAbility1 = 20599, // Boss->self, no cast, single-target
    UnknownAbility2 = 20600, // Boss->self, no cast, single-target
    UnknownAbility3 = 20601, // Boss->self, no cast, single-target
    UnknownAbility4 = 20602, // Boss->self, no cast, single-target

    ManeuverColliderCannons1 = 20603, // Boss->self, 7.0s cast, single-target
    ManeuverColliderCannons2 = 20605, // Boss->self, 8.0s cast, single-target
    ColliderCannons = 20606, // AegisUnitHelper->self, no cast, range 40 ?-degree cone

    ManeuverRefractionCannons1 = 20607, // Boss->self, 6.0s cast, single-target
    ManeuverRefractionCannons2 = 20608, // Boss->self, 6.0s cast, single-target
    RefractionCannons = 20609, // AegisUnitHelper->self, no cast, range 40 ?-degree cone

    FlightPath = 20620, // FlightUnit->self, 3.0s cast, range 60 width 10 rect
    FiringOrderAntiPersonnelLaser = 20621, // Boss->self, 3.0s cast, single-target // AOE Tankbuster
    FiringOrderSurfaceLaser = 20622, // Boss->self, 3.0s cast, single-target
    FiringOrderHighPoweredLaser = 20623, // Boss->self, 3.0s cast, single-target
    AntiPersonnelLaser = 20624, // AegisUnitHelper->players, no cast, range 3 circle
    SurfaceLaser1 = 20625, // AegisUnitHelper->location, no cast, single-target
    SurfaceLaser2 = 20626, // AegisUnitHelper->location, no cast, range 4 circle
    HighPoweredLaser = 20627, // AegisUnitHelper->players, no cast, range 6 circle

    ManeuverSaturationBombing = 20631, // FlightUnit->self, 25.0s cast, range 60 circle
    ManeuverDiffusionCannon = 20633, // Boss->self, 6.0s cast, range 60 circle

    AerialSupportSwoop = 20690, // Boss->self, 3.0s cast, single-target
    AerialSupportBombardment = 20691, // Boss->self, 3.0s cast, single-target

    UnknownAbility5 = 21426, // AegisUnitHelper->self, no cast, single-target
    LifesLastSong = 21427, // AegisUnitHelper->self, 7.5s cast, range 30 ?-degree cone
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // AegisUnitHelper/FlightUnit->player, extra=0x1/0x2
    MagicVulnerabilityUp = 2091, // AegisUnitHelper->player, extra=0x0
    Hover = 2390, // none->Boss, extra=0x1F4
}

public enum IconID : uint
{
    SpreadMarker = 23, // player
    Stackmarker = 62, // player
    Icon198 = 198, // player
}
