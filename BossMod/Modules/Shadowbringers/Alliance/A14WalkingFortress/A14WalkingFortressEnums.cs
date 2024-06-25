namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

public enum OID : uint
{
    Boss = 0x2C74, // R6.500, x1
    Helper = 0x233C, // R0.500, x24 (spawn during fight), 523 type
    Marx1 = 0x2C0B, // R0.700, x1
    Marx2 = 0x2C0C, // R0.700, x1
    Helper2P = 0x2C66, // R0.512, x1
    FlightUnit9S = 0x2C75, // R2.800, x1
    Marx3 = 0x2C78, // R12.000, x0 (spawn during fight)
    GoliathTank = 0x2C77, // R9.600, x0 (spawn during fight)
    SerialJointedServiceModel = 0x2C76, // R3.360, x0 (spawn during fight)
}

public enum AID : uint
{
    BladeFlurry1 = 18608, // Helper2P->Boss/GoliathTank/SerialJointedServiceModel, no cast, single-target
    BladeFlurry2 = 18609, // Helper2P->Boss/GoliathTank/SerialJointedServiceModel, no cast, single-target
    BossAutoAttack = 18884, // Boss->player, no cast, single-target
    DancingBlade = 19048, // Helper2P->Boss/GoliathTank/SerialJointedServiceModel, no cast, width 2 rect charge
    BalancedEdge = 19049, // Helper2P->self, 2.0s cast, range 5 circle
    Neutralization = 18677, // Boss->player, 4.0s cast, single-target
    LaserSaturation = 18678, // Boss->self, 4.0s cast, range 85 circle
    WhirlingAssault = 19050, // Helper2P->self, 2.0s cast, range 40 width 4 rect
    LaserTurret2 = 18679, // Boss->self, 4.0s cast, single-target
    LaserTurret3 = 19060, // Helper->self, 4.3s cast, range 90 width 8 rect
    GroundToGroundMissile = 18680, // Boss->self, no cast, single-target
    BallisticImpact1 = 18804, // Helper->location, 3.5s cast, range 6 circle
    BallisticImpact2 = 18681, // Helper->players, 5.0s cast, range 6 circle
    ForeHindCannons = 18655, // Boss->self, 5.0s cast, single-target
    LaserSuppression = 18656, // Helper->self, 5.0s cast, range 60 ?-degree cone
    DualFlankCannons = 18654, // Boss->self, 5.0s cast, single-target
    EngageMarxSupport = 18643, // Boss->self, 3.0s cast, single-target
    MarxImpact = 18644, // Marx3->self, 5.0s cast, range 22 circle
    Undock1 = 19255, // Boss->self, 4.0s cast, single-target
    Undock2 = 19038, // Boss->self, no cast, single-target
    UnknownAbiility1 = 19037, // FlightUnit9S->self, no cast, single-target
    UnknownAbiility2 = 18683, // Helper2P->location, no cast, single-target
    UnknownWeaponskill1 = 18647, // FlightUnit9S->self, no cast, single-target
    UnknownWeaponskill2 = 18649, // Helper->self, 1.0s cast, range 60 width 20 rect
    BallisticImpact3 = 18652, // Helper->self, no cast, range 15 width 20 rect
    UnknownWeaponskill3 = 18650, // Helper->self, 3.5s cast, range 60 width 20 rect
    UnknownWeaponskill4 = 18651, // Helper->self, 6.0s cast, range 60 width 20 rect
    UnknownWeaponskill5 = 19223, // FlightUnit9S->self, no cast, single-target
    UnknownAbiility3 = 18653, // Boss->self, no cast, single-target
    UnknownWeaponskill6 = 18659, // Helper->self, no cast, single-target
    AntiPersonnelMissile1 = 18657, // Boss->self, 6.0s cast, single-target
    BallisticImpact4 = 18660, // Helper->self, no cast, range 15 width 15 rect
    EngageGoliathTankSupport = 18661, // Boss->self, 3.0s cast, single-target
    LaserTurret1 = 18662, // GoliathTank->self, no cast, range 85 width 10 rect
    HackGoliathTank = 18663, // Boss->GoliathTank, 10.0s cast, single-target
    ConvenientSelfDestruction1 = 18664, // GoliathTank->self, 10.0s cast, range 85 circle
    ConvenientSelfDestruction2 = 18665, // GoliathTank->self, 8.0s cast, range 22 circle
    UnknownWeaponskill7 = 18666, // Boss->self, no cast, single-target
    SJSMAutoAttack = 872, // SerialJointedServiceModel->player, no cast, single-target
    ClangingBlow = 18672, // SerialJointedServiceModel->player, 4.0s cast, single-target
    ShrapnelImpact = 18675, // Helper->players, 5.0s cast, range 6 circle
    CentrifugalSpin1 = 19076, // SerialJointedServiceModel->self, 6.0s cast, single-target
    CentrifugalSpin2 = 19077, // Helper->self, 6.3s cast, range 30 width 8 rect
    DeployDefenses = 18671, // Helper2P->self, no cast, single-target
    TotalAnnihilationManeuver1 = 18667, // Boss->self, 10.0s cast, single-target
    UnknownWeaponskill8 = 18766, // 2C88->self, no cast, single-target
    UnknownWeaponskill9 = 18764, // 2C86->self, no cast, single-target
    UnknownWeaponskill10 = 18765, // 2C87->self, no cast, single-target
    TotalAnnihilationManeuver2 = 18668, // Helper->self, 13.0s cast, range 80 circle
    AntiPersonnelMissile2 = 18658, // Boss->self, 9.0s cast, single-target
    Undock3 = 18645, // Boss->self, 4.0s cast, single-target
    UnknownAbiility4 = 18646, // FlightUnit9S->self, 4.0s cast, single-target
    UnknownWeaponskill11 = 18648, // FlightUnit9S->self, no cast, single-target
    AntiPersonnelMissile3 = 19217, // Boss->self, 5.0s cast, single-target
    SidestrikingSpin1 = 19079, // Helper->self, 6.3s cast, range 12 width 30 rect
    SidestrikingSpin2 = 19078, // SerialJointedServiceModel->self, 6.0s cast, single-target
    Unknown1 = 19032, // 2CC8->self, no cast, single-target
    Unknown2 = 19031, // 2CC7->self, no cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 2213, // Helper/Marx3->player, extra=0x1/0x2/0x3/0x4/0x5/0x7/0x6
    UnknownStatus1 = 2056, // Boss->Marx3/Boss/GoliathTank/2C86/2C87/2C88, extra=0x92/0x91/0x90/0x94/0x8C
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Preoccupied = 1619, // none->player/Helper2P, extra=0x0
    Invincibility = 775, // none->Boss, extra=0x0
    DamageUp = 2220, // none->Boss, extra=0x1/0x2
    Invincibility2 = 1570, // none->player, extra=0x0
    UnknownStatus2 = 2193, // none->2C86/2C87/2C88, extra=0x8D
    VulnerabilityDown = 350, // none->player, extra=0x0
    UnknownStatus3 = 2160, // none->Helper2P, extra=0x1C94
}

public enum IconID : uint
{
    Icon198 = 198, // player
    Icon139 = 139, // player
    Icon199 = 199, // Helper
    Icon164 = 164, // player
    Icon200 = 200, // GoliathTank
    Icon161 = 161, // player
}

public enum TetherID : uint
{
    Tether100 = 100, // GoliathTank->Boss
    Tether54 = 54, // Boss->GoliathTank
}
