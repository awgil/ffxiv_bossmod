#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x28, Helper type
    Unk1 = 0x4947, // R1.000, x3
    Boss = 0x4918, // R9.000, x1
    UltimaTheFeared = 0x4919, // R16.000, x1
    UltimaTheFearedAuto = 0x49AF, // R0.000, x1, Part type
    EnergyOrb = 0x491A, // R1.000, x2, Helper type

    ManaScreenNESW = 0x1EBE8B,
    ManaScreenNWSE = 0x1EBE8C,
}

public enum AID : uint
{
    AutoUltima = 44317, // UltimaTheFearedAuto->player, no cast, single-target
    AutoOmega = 45308, // Boss->player, no cast, single-target
    IonEfflux = 44331, // Boss->self, 6.5s cast, range 65 circle
    AntimatterCast = 44304, // UltimaTheFeared->self, 5.0s cast, single-target
    Antimatter = 44305, // Helper->player, 6.0s cast, single-target
    EnergyOrb = 44296, // UltimaTheFeared->self, 3.0s cast, single-target
    OmegaJump = 44332, // Boss->location, no cast, single-target
    EnergizingEquilibriumUltima = 44316, // UltimaTheFeared->Boss, no cast, single-target, bosses share HP
    EnergizingEquilibriumOmega = 44336, // Boss->UltimaTheFeared, no cast, single-target
    EnergyRay1 = 44338, // EnergyOrb->self, 5.0s cast, range 40 width 16 rect
    EnergyRay2 = 44298, // Helper->self, no cast, range 40 width 16 rect
    EnergyRay3 = 44299, // Helper->self, no cast, range 40 width 16 rect
    EnergyRay4 = 44300, // Helper->self, no cast, range 48 width 20 rect
    EnergyRay5 = 44301, // Helper->self, no cast, range 48 width 20 rect
    ForeToAftFire = 44325, // Boss->self, 6.0s cast, single-target
    ForewardBlaster = 44326, // Boss->self, no cast, single-target
    AftToForeFire = 44327, // Boss->self, 6.0s cast, single-target
    AftwardBlaster = 44328, // Boss->self, no cast, single-target
    OmegaBlasterFirst = 44329, // Helper->self, 6.5s cast, range 50 180-degree cone
    OmegaBlasterSecond = 44330, // Helper->self, 8.8s cast, range 50 180-degree cone
    TractorBeamVisual = 44294, // UltimaTheFeared->self, 10.0s cast, range 40 width 48 rect
    TractorBeam = 45190, // Helper->self, 10.5s cast, range 40 width 24 rect
    AntiPersonnelMissileVisual = 45191, // Boss->self, no cast, single-target
    AntiPersonnelMissile = 45192, // Helper->players, 5.0s cast, range 6 circle
    Crash = 44295, // Helper->self, 10.5s cast, range 40 width 24 rect
    SurfaceMissileCast = 45173, // Boss->self, 9.0s cast, single-target
    SurfaceMissile = 45174, // Helper->self, 1.0s cast, range 12 width 20 rect
    ManaScreen = 44297, // UltimaTheFeared->self, 3.0s cast, single-target
    TrajectoryProjection = 44323, // Boss->self, 3.5s cast, single-target
    GuidedMissile = 44324, // Helper->self, 1.0s cast, range 6 circle
    TractorFieldCast = 44306, // UltimaTheFeared->self, 5.0s cast, single-target
    TractorField = 44307, // Helper->self, 5.5s cast, range 50 circle
    MultiMissile = 45035, // Boss->self, 2.1s cast, single-target
    MultiMissileBig = 45036, // Helper->self, 4.1s cast, range 10 circle
    MultiMissileSmall = 45037, // Helper->self, 4.0s cast, range 6 circle
    CitadelSiege1 = 44308, // UltimaTheFeared->self, no cast, single-target
    CitadelSiege2 = 44309, // UltimaTheFeared->self, no cast, single-target
    CitadelSiege3 = 44310, // UltimaTheFeared->self, no cast, single-target
    CitadelSiege4 = 44311, // UltimaTheFeared->self, no cast, single-target
    CitadelSiegeRect = 44312, // Helper->self, 5.0s cast, range 48 width 10 rect
    CitadelBuster = 44315, // UltimaTheFeared->location, 6.0s cast, range 50 circle
    HyperPulse = 44335, // Boss->self, 5.0s cast, range 50 circle
    ChemicalBombCast = 44302, // UltimaTheFeared->self, 6.5s cast, single-target
    ChemicalBomb = 44303, // Helper->self, 7.0s cast, range 50 circle
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
    Spread = 466, // player->self
    SurfaceMissile = 616, // Helper->self
    BaitEast = 617, // player->self
    BaitWest = 618, // player->self
    BaitSouth = 619, // player->self
    BaitNorth = 620, // player->self
}
