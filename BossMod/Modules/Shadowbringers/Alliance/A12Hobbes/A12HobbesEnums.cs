namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

public enum OID : uint
{
    Boss = 0x2C2B, // R25.000, x?
    Helper2P = 0x2C66, // R0.512, x?
    Hobbes1 = 0x2C73, // R1.000, x?, Part type
    Hobbes2 = 0x2C2D, // R1.000, x?
    Hobbes3 = 0x2C2C, // R1.000, x?
    HobbesHelper = 0x233C, // R0.500, x?, 523 type
    Anogg = 0x2C83, // R0.500, x?
    Konogg = 0x2C82, // R0.500, x?
    SmallExploder = 0x2C62, // R0.960, x?
}

public enum AID : uint
{
    Attack = 18430, // Hobbes1->player, no cast, single-target
    BalancedEdge = 19049, // Helper2P->self, 2.0s cast, range 5 circle
    BladeFlurry1 = 18608, // Helper2P->Boss, no cast, single-target
    BladeFlurry2 = 18609, // Helper2P->Boss, no cast, single-target
    ConvenientSelfDestruction = 18460, // SmallExploder->self, no cast, range 5 circle
    DancingBlade = 19048, // Helper2P->Boss, no cast, width 2 rect charge
    ElectromagneticPulse = 18457, // HobbesHelper->self, no cast, range 40 width 5 rect

    FireResistanceTest1 = 18454, // HobbesHelper->self, no cast, range 70 ?-degree cone
    FireResistanceTest2 = 18455, // HobbesHelper->self, no cast, range 12 width 38 rect
    FireResistanceTest3 = 18456, // HobbesHelper->self, no cast, range 22 width 42 rect

    Impact = 18450, // HobbesHelper->self, 7.7s cast, range 20 circle

    LaserResistanceTest1 = 18437, // Boss->self, 4.0s cast, range 50 circle
    LaserResistanceTest2 = 18438, // Boss->self, no cast, range 50 circle

    LaserSight1 = 18440, // HobbesHelper->player, no cast, single-target
    LaserSight2 = 18439, // Boss->self, 8.0s cast, single-target
    LaserSight3 = 18441, // HobbesHelper->self, no cast, range 65 width 8 rect

    OilWell = 18459, // HobbesHelper->self, no cast, ???

    RingLaser1 = 18431, // Boss->self, 3.4s cast, single-target
    RingLaser2 = 18432, // Boss->self, 1.0s cast, single-target
    RingLaser3 = 18434, // HobbesHelper->self, 4.0s cast, range ?-20 donut
    RingLaser4 = 18433, // Boss->self, 1.0s cast, single-target
    RingLaser5 = 18435, // HobbesHelper->self, 4.0s cast, range ?-15 donut
    RingLaser6 = 18436, // HobbesHelper->self, 4.0s cast, range ?-10 donut

    ShockingDischarge = 18443, // HobbesHelper->self, 2.0s cast, range 5 circle
    ShortRangeMissile1 = 18452, // HobbesHelper->self, no cast, single-target
    ShortRangeMissile2 = 18453, // HobbesHelper->players, 8.0s cast, range 8 circle
    Towerfall = 18451, // HobbesHelper->self, no cast, range 20 width 8 rect

    UnknownAbility1 = 18707, // Hobbes2->self, no cast, single-target
    UnknownAbility2 = 18710, // Hobbes2->self, no cast, single-target
    UnknownAbility3 = 18702, // Hobbes3->self, no cast, single-target
    UnknownAbility4 = 18706, // Hobbes3->self, no cast, single-target
    UnknownAbility5 = 18704, // Hobbes3->self, no cast, single-target
    UnknownAbility6 = 18711, // Hobbes2->self, no cast, single-target
    UnknownAbility7 = 18701, // Hobbes3->self, no cast, single-target
    UnknownAbility8 = 18703, // Hobbes3->self, no cast, single-target
    UnknownAbility9 = 18705, // Hobbes3->self, no cast, single-target
    UnknownAbility10 = 18683, // Helper2P->location, no cast, single-target
    UnknownAbility11 = 18712, // Hobbes2->self, no cast, single-target
    UnknownAbility12 = 18709, // Hobbes2->self, no cast, single-target

    UnknownWeaponskill1 = 18442, // Hobbes3->self, no cast, single-target
    UnknownWeaponskill2 = 18444, // Hobbes3->self, no cast, single-target

    UnwillingCargo = 18458, // HobbesHelper->self, no cast, range 40 width 7 rect

    VariableCombatTest1 = 18446, // Hobbes3->self, 5.0s cast, single-target
    VariableCombatTest2 = 18885, // HobbesHelper->self, 5.7s cast, range 20 ?-degree cone
    VariableCombatTest3 = 18887, // HobbesHelper->self, 5.7s cast, range ?-19 donut
    VariableCombatTest4 = 18886, // HobbesHelper->self, 5.7s cast, range 2 circle

    VariableCombatTest5 = 18446, // Hobbes3->self, 5.0s cast, single-target
    VariableCombatTest6 = 18447, // HobbesHelper->self, 2.0s cast, range 20 ?-degree cone
    VariableCombatTest7 = 18449, // HobbesHelper->self, 2.0s cast, range ?-19 donut
    VariableCombatTest8 = 18448, // HobbesHelper->self, 2.0s cast, range 2 circle
    VariableCombatTest9 = 18445, // Hobbes3->self, 5.0s cast, single-target

    WhirlingAssault = 19050, // Helper2P->self, 2.0s cast, range 40 width 4 rect
}

public enum SID : uint
{
    Unknown = 2056, // Hobbes3->Hobbes3, extra=0x8F
    Burns = 2199, // HobbesHelper->player, extra=0x1/0x2/0x3
    PhysicalVulnerabilityUp = 2090, // HobbesHelper->player, extra=0x0
    MagicVulnerabilityUp = 2091, // HobbesHelper->player, extra=0x0
    Electrocution = 2200, // HobbesHelper->player, extra=0x1/0x2
    Oil = 2157, // HobbesHelper->player, extra=0x32
}

public enum IconID : uint
{
    Icon_168 = 168, // Hobbes3
    Icon_167 = 167, // Hobbes3
    Icon_196 = 196, // player
}

public enum TetherID : uint
{
    Tether_99 = 99, // 18D6->18D6
    Tether_84 = 84, // SmallExploder->player
}
