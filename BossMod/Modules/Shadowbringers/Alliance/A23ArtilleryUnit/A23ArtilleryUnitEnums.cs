namespace BossMod.Shadowbringers.Alliance.A23ArtilleryUnit;

public enum OID : uint
{
    Helper905P = 0x2EFC, // R0.500, x?
    Boss = 0x2E6A, // R4.000, x?
    Helper = 0x233C, // R0.500, x?, 523 type
    ArtilleryUnit = 0x18D6, // R0.500, x?
    Energy = 0x2E6C, // R1.000, x?
    Pod = 0x2E6D, // R0.800, x?
}

public enum AID : uint
{
    BossAutoAttack = 21561, // Boss->player, no cast, single-target
    ManeuverVoltArray = 20486, // Boss->self, 4.0s cast, range 60 circle

    LowerLaser1 = 20614, // Helper->self, 1.8s cast, range 30 ?-degree cone
    UpperLaser1 = 20615, // Helper->self, 1.8s cast, range 16 ?-degree cone
    UpperLaser2 = 20616, // Helper->self, 4.8s cast, range ?-23 donut
    UpperLaser3 = 20617, // Helper->self, 7.8s cast, range ?-30 donut
    LowerLaser2 = 20470, // Helper->self, no cast, range 60 ?-degree cone
    UpperLaser4 = 20471, // Helper->self, no cast, range 16 ?-degree cone
    UpperLaser5 = 20472, // Helper->self, no cast, range ?-23 donut
    UpperLaser6 = 20473, // Helper->self, no cast, range ?-30 donut

    EnergyBomb = 20474, // Energy->player, no cast, single-target
    EnergyBombardment1 = 20475, // Boss->self, 2.0s cast, single-target
    EnergyBombardment2 = 20476, // Helper->location, 3.0s cast, range 4 circle
    ManeuverImpactCrusher1 = 20477, // Boss->self, 4.0s cast, single-target
    ManeuverImpactCrusher2 = 20478, // Boss->location, no cast, range 8 circle
    UnknownWeaponskill = 20479, // Helper->location, 4.0s cast, range 8 circle
    ManeuverRevolvingLaser = 20480, // Boss->self, 3.0s cast, range ?-60 donut

    ManeuverHighPoweredLaser1 = 20481, // Boss->self, 5.0s cast, single-target
    ManeuverHighPoweredLaser2 = 20482, // Boss->self, no cast, range 60 width 8 rect
    ManeuverUnconventionalVoltage = 20483, // Boss->self, 6.0s cast, single-target
    UnconventionalVoltage = 20484, // Helper->self, no cast, range 60 ?-degree cone
    UnknownAbility = 20485, // Helper->player, no cast, single-target

    OperationSynthesizeCompound = 20460, // Boss->self, 3.0s cast, single-target
    OperationActivateLaserTurret = 20461, // Boss->self, 6.0s cast, single-target
    OperationActivateSuppressiveUnit = 20462, // Boss->self, 6.0s cast, single-target
    OperationAccessSelfConsciousnessData = 20463, // Boss->self, 8.0s cast, single-target
    R010Laser = 20464, // Pod->self, 10.0s cast, range 60 width 12 rect
    R030Hammer = 20465, // Pod->self, 10.0s cast, range 18 circle

    ChemicalBurn = 20468, // Helper->self, no cast, range 3 circle

    SupportPod = 20457, // Boss->self, 2.0s cast, single-target
    OperationPodProgram = 20458, // Boss->self, 6.0s cast, single-target
}

public enum SID : uint
{
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper/Boss/Energy->player, extra=0x2/0x1
    BrinkOfDeath = 44, // none->player, extra=0x0

}

public enum IconID : uint
{
    Icon230 = 230, // player
    Icon172 = 172, // player
}

public enum TetherID : uint
{
    Tether122 = 122, // Boss->ArtilleryUnit
    Tether123 = 123, // ArtilleryUnit->Pod/Helper
}
