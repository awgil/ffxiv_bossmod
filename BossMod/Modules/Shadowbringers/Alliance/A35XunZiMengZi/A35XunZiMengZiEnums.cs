namespace BossMod.Shadowbringers.Alliance.A35XunZiMengZi;

public enum OID : uint
{
    MengZi = 0x3196, // R15.000, x1
    XunZi = 0x3195, // R15.000, x1
    Helper = 0x233C, // R0.500, x4, 523 type
    Ally2B = 0x31A8, // R0.512, x1
    Ally9S = 0x31A9, // R0.512, x1
    Energy = 0x3197, // R1.000, x24
    SerialJointedModel = 0x3199, // R2.400, x4
    SmallFlyer = 0x3198, // R1.320, x0 (spawn during fight)
}

public enum AID : uint
{
    BladeFlurry1 = 23788, // Ally2B->XunZi, no cast, single-target
    BladeFlurry2 = 23789, // Ally2B->XunZi, no cast, single-target
    DancingBlade = 23790, // Ally2B->XunZi, no cast, width 2 rect charge
    BalancedEdge = 23791, // Ally2B->self, 2.0s cast, range 5 circle
    WhirlingAssault = 23792, // Ally2B->self, 2.0s cast, range 40 width 4 rect

    DeployArmaments1 = 23552, // XunZi/MengZi->self, 6.0s cast, range 50 width 18 rect
    DeployArmaments2 = 23553, // MengZi->self, 7.0s cast, range 50 width 18 rect
    DeployArmaments3 = 23554, // Helper->self, 6.7s cast, range 50 width 18 rect
    DeployArmaments4 = 23555, // XunZi/MengZi->self, 6.0s cast, range 50 width 18 rect
    DeployArmaments5 = 23556, // MengZi/XunZi->self, 7.0s cast, range 50 width 18 rect
    DeployArmaments6 = 23557, // Helper->self, 7.0s cast, range 50 width 18 rect
    DeployArmaments7 = 24696, // Helper->self, 7.7s cast, range 50 width 18 rect
    DeployArmaments8 = 24697, // Helper->self, 8.0s cast, range 50 width 18 rect

    HighPoweredLaser = 23561, // SerialJointedModel->self, no cast, range 70 width 4 rect
    UniversalAssault = 23558, // XunZi/MengZi->self, 5.0s cast, range 50 width 50 rect
    LowPoweredOffensive = 23559, // SmallFlyer->self, 2.0s cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/SerialJointedModel->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
    UnknownStatus = 2056, // none->SerialJointedModel, extra=0x87
}

public enum IconID : uint
{
    Icon_164 = 164, // player
}