namespace BossMod.Shadowbringers.Alliance.A22FlightUnits;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x?, 523 type

    ALphaHelper = 0x2EF9, // R0.500, x?
    FlightUnitALpha = 0x2E10, // R6.000, x?

    BEtaHelper = 0x2EFA, // R0.500, x?
    FlightUnitBEta = 0x2E11, // R6.000, x?

    CHiHelper = 0x2EFB, // R0.500, x?
    FlightUnitCHi = 0x2E12, // R6.000, x?
}

public enum AID : uint
{
    BossAutoAttack = 21423, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->player, no cast, single-target
    ApplyShieldProtocol1 = 20390, // FlightUnitALpha->self, 5.0s cast, single-target
    ApplyShieldProtocol2 = 20392, // FlightUnitCHi->self, 5.0s cast, single-target
    ApplyShieldProtocol3 = 20391, // FlightUnitBEta->self, 5.0s cast, single-target

    SharpTurnAlpha1 = 20393, // FlightUnitALpha->self, 9.0s cast, single-target
    SharpTurnAlpha2 = 20394, // FlightUnitALpha->self, 9.0s cast, single-target
    FormationSharpTurn = 20395, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 3.0s cast, single-target

    SlidingSwipeAlpha1 = 20396, // FlightUnitALpha->self, 6.0s cast, single-target
    SlidingSwipeAlpha2 = 20397, // FlightUnitALpha->self, 6.0s cast, single-target
    FormationSlidingSwipe = 20398, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target

    IncendiaryBarrage = 20399, // Helper->location, 9.0s cast, range 27 circle
    FormationAirRaid = 20400, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target

    StandardSurfaceMissile1 = 20401, // Helper->location, 5.0s cast, range 10 circle
    StandardSurfaceMissile2 = 20402, // Helper->location, 5.0s cast, range 10 circle

    LethalRevolution = 20403, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, range 15 circle

    ManeuverHighPoweredLaser1 = 20404, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target
    ManeuverHighPoweredLaser2 = 20405, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->players, no cast, range 80 width 14 rect

    UnknownAbility = 20406, // Helper->player, no cast, single-target
    ManeuverAreaBombardment = 20407, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target

    GuidedMissile = 20408, // Helper->location, 3.0s cast, range 4 circle
    IncendiaryBombing = 20409, // Helper->location, 5.0s cast, range 8 circle
    SurfaceMissile = 20410, // Helper->location, 3.0s cast, range 6 circle
    AntiPersonnelMissile = 20411, // Helper->player, 5.0s cast, range 6 circle

    SuperiorMobility = 20412, // FlightUnitBEta/FlightUnitCHi/FlightUnitALpha->location, no cast, single-target
    ManeuverMissileCommand = 20413, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 4.0s cast, single-target
    BarrageImpact = 20414, // Helper->self, no cast, range 50 circle

    ManeuverIncendiaryBombing = 20419, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target
    ManeuverPrecisionGuidedMissile = 20420, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 4.0s cast, single-target
    PrecisionGuidedMissile = 20421, // Helper->players, 4.0s cast, range 6 circle // Tankbuster

    SharpTurn1 = 20589, // Helper->self, no cast, range 110 width 30 rect
    SharpTurn2 = 20590, // Helper->self, no cast, range 110 width 30 rect
    SlidingSwipe1 = 20591, // Helper->self, no cast, range 130 width 30 rect
    SlidingSwipe2 = 20592, // Helper->self, no cast, range 130 width 30 rect

    SlidingSwipeBeta = 21774, // FlightUnitBEta->self, 6.0s cast, single-target
    SlidingSwipeChi = 21775, // FlightUnitCHi->self, 6.0s cast, single-target
    SharpTurnBeta1 = 21777, // FlightUnitBEta->self, 9.0s cast, single-target
    SharpTurnBeta2 = 21778, // FlightUnitBEta->self, 9.0s cast, single-target
    SharpTurnChi1 = 21779, // FlightUnitCHi->self, 9.0s cast, single-target
    SharpTurnChi2 = 21780, // FlightUnitCHi->self, 9.0s cast, single-target

    UnknownWeaponskill = 26807, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->location, no cast, single-target
}

public enum SID : uint
{
    ShieldProtocolA = 2288, // none->player, extra=0x0
    ShieldProtocolC = 2290, // none->player, extra=0x0
    ShieldProtocolB = 2289, // none->player, extra=0x0
    ProcessOfEliminationC = 2411, // none->FlightUnitCHi, extra=0x0
    ProcessOfEliminationA = 2409, // none->FlightUnitALpha, extra=0x0
    ProcessOfEliminationB = 2410, // none->FlightUnitBEta, extra=0x0
    Burns1 = 2194, // none->player, extra=0x0
    MagicVulnerabilityUp = 2091, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper/FlightUnitBEta->player, extra=0x1/0x2
    Burns2 = 2401, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Spreadmarker1 = 23, // player
    Spreadmarker2 = 139, // player
    Icon198 = 198, // player
}

public enum TetherID : uint
{
    Tether7 = 7, // player->FlightUnitCHi/FlightUnitALpha/FlightUnitBEta
    Tether54 = 54, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->FlightUnitBEta/FlightUnitALpha/FlightUnitCHi
}
