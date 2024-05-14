namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

public enum OID : uint
{
    Boss = 0x2EC6, // R6.000, x1
    Helper = 0x233C, // R0.500, x31 (spawn during fight), 523 type
    CompoundPod = 0x2EC8, // R1.360, x0 (spawn during fight)
    Puppet2P = 0x2EC7, // R6.000, x0 (spawn during fight)
    ThePuppets1 = 0x2FA5, // R0.500, x4
    ThePuppets2 = 0x2FA4, // R1.000, x1
    TheCompound = 0x2EC4, // R5.290, x1
}

public enum AID : uint
{
    BossAutoAttack = 21490, // Boss->player, no cast, single-target
    CentrifugalSlice = 20912, // Boss->self, 5.0s cast, range 100 circle
    RelentlessSpiral1 = 20905, // Boss->self, 3.5s cast, single-target
    RelentlessSpiral2 = 20906, // Helper->location, 3.5s cast, range 8 circle
    PrimeBlade1 = 21535, // Boss->self, 7.0s cast, range 20 circle
    PrimeBlade2 = 21536, // Boss->self, 7.0s cast, range 85 width 20 rect
    PrimeBlade3 = 21537, // Boss/Puppet2P->self, 7.0s cast, range ?-43 donut
    UnknownWeaponskill1 = 20899, // Boss->location, no cast, single-target
    UnknownWeaponskill2 = 21347, // Helper->location, no cast, single-target
    UnknownWeaponskill3 = 21456, // Helper->self, no cast, single-target
    RelentlessSpiral3 = 20939, // Helper->self, 1.0s cast, range 8 circle
    PrimeBlade = 20890, // Boss->self, 10.0s cast, range ?-43 donut
    ThreePartsDisdain1 = 20891, // Boss->players, 6.0s cast, range 6 circle
    ThreePartsDisdain2 = 20892, // Boss->players, no cast, range 6 circle
    ThreePartsDisdain3 = 20893, // Boss->players, no cast, range 6 circle
    CompoundPodR012 = 20907, // Boss->self, 3.0s cast, single-target
    R012Laser1 = 20908, // CompoundPod->self, no cast, single-target
    R012Laser2 = 20911, // Helper->location, 4.0s cast, range 6 circle
    R012Laser3 = 20910, // Helper->players, 5.0s cast, range 6 circle
    R012Laser4 = 20909, // Helper->player, 5.0s cast, range 6 circle
    FourPartsResolve1 = 20894, // Boss->self, 8.0s cast, single-target
    FourPartsResolve2 = 20895, // Boss->players, no cast, range 6 circle
    FourPartsResolve3 = 20896, // Boss->self, no cast, range 85 width 12 rect
    Reproduce = 20897, // Boss->self, 4.0s cast, single-target
    UnknownWeaponskill4 = 21457, // Helper->self, no cast, single-target
    EnergyCompression = 20902, // Boss->self, 4.0s cast, single-target
    Explosion = 20903, // Helper->self, no cast, range 5 circle
    ForcedTransfer1 = 20898, // Boss->self, 6.5s cast, single-target
    CompoundPodR011 = 20900, // Boss->self, 4.0s cast, single-target
    ForcedTransfer2 = 21562, // Boss->self, 8.5s cast, single-target
    R011Laser5 = 20901, // CompoundPod->self, 11.5s cast, single-target
    R011Laser6 = 21531, // Helper->self, 1.5s cast, range 70 width 15 rect
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/Boss->player, extra=0x1/0x2
    Electrocution = 2086, // none->player, extra=0x0
    MagicVulnerabilityUp = 2091, // Helper->player, extra=0x0
    UnknownStatus = 2056, // none->CompoundPod, extra=0xB6
}

public enum IconID : uint
{
    Icon_62 = 62, // player
    Tankbuster = 218, // player
    Icon_139 = 139, // player
    Icon_79 = 79, // player
    Icon_80 = 80, // player
    Icon_81 = 81, // player
    Icon_82 = 82, // player
}

public enum TetherID : uint
{
    Tether_117 = 117, // Helper->Helper
    Tether_116 = 116, // Helper->Helper
    Tether_41 = 41, // player->Boss
    Tether_54 = 54, // Puppet2P->Boss
}
