namespace BossMod.Shadowbringers.Alliance.A11CommandModel;

public enum OID : uint
{
    Boss = 0x2C61, // R5.600, x1
    BossHelper = 0x233C, // R0.500, x16, 523 type
    SJSM1 = 0x2C63, // R2.400, x12
    Helper2P = 0x2C66, // R0.512, x1
    SJSM2 = 0x2C65, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    BladeFlurry1 = 18608, // Helper2P->Boss, no cast, single-target
    BladeFlurry2 = 18609, // Helper2P->Boss, no cast, single-target
    DancingBlade = 19048, // Helper2P->Boss, no cast, width 2 rect charge
    SystematicSiege = 18610, // Boss->self, 2.5s cast, single-target
    BalancedEdge = 19049, // Helper2P->self, 2.0s cast, range 5 circle
    UnknownWeaponskill1 = 19249, // SJSM1->self, no cast, single-target
    UnknownWeaponskill2 = 18611, // SJSM1->self, 1.0s cast, single-target
    ClangingBlow = 18638, // Boss->player, 4.0s cast, single-target
    EnergyBomb = 18612, // SJSM2->player/Helper2P, no cast, single-target
    WhirlingAssault = 19050, // Helper2P->self, 2.0s cast, range 40 width 4 rect
    EnergyBombardment1 = 18615, // Boss->self, 3.0s cast, single-target
    EnergyBombardment2 = 18616, // BossHelper->location, 3.0s cast, range 4 circle
    ForcefulImpact = 18639, // Boss->self, 4.0s cast, range 100 circle
    EnergyAssault1 = 18613, // Boss->self, 5.0s cast, single-target
    EnergyAssault2 = 18614, // BossHelper->self, no cast, range 30 ?-degree cone
    UnknownWeaponskill3 = 18960, // Boss->self, no cast, single-target
    SystematicTargeting = 18628, // Boss->self, 2.5s cast, single-target
    HighPoweredLaser = 18629, // SJSM1->self, no cast, range 70 width 4 rect
    SidestrikingSpin1 = 18634, // Boss->self, 6.0s cast, single-target
    SidestrikingSpin2 = 18636, // BossHelper->self, 6.3s cast, range 30 width 12 rect
    SidestrikingSpin3 = 18635, // BossHelper->self, 6.3s cast, range 30 width 12 rect
    SystematicAirstrike = 18617, // Boss->self, 2.5s cast, single-target
    UnknownWeaponskill4 = 19250, // SJSM1->self, no cast, single-target
    AirToSurfaceEnergy = 18618, // BossHelper->self, no cast, range 5 circle
    Shockwave = 18627, // Boss->self, 5.0s cast, range 100 circle
    EnergyRing1 = 18619, // Boss->self, 3.5s cast, single-target
    EnergyRing2 = 18620, // BossHelper->self, 4.7s cast, range 12 circle
    EnergyRing3 = 18621, // Boss->self, no cast, single-target
    EnergyRing4 = 18622, // BossHelper->self, 6.7s cast, range ?-24 donut
    EnergyRing5 = 18623, // Boss->self, no cast, single-target
    EnergyRing6 = 18624, // BossHelper->self, 8.7s cast, range ?-36 donut
    EnergyRing7 = 18625, // Boss->self, no cast, single-target
    EnergyRing8 = 18626, // BossHelper->self, 10.7s cast, range ?-48 donut
    SystematicSuppression = 18630, // Boss->self, 2.5s cast, single-target
    HighCaliberLaser1 = 18631, // SJSM1->self, 7.0s cast, single-target
    HighCaliberLaser2 = 18682, // BossHelper->self, 7.0s cast, range 70 width 24 rect
    CentrifugalSpin1 = 18633, // BossHelper->self, 6.3s cast, range 30 width 8 rect
    CentrifugalSpin2 = 18632, // Boss->self, 6.0s cast, single-target
}

public enum SID : uint
{
    Unknown1 = 2193, // SJSM1/Boss->SJSM1/Boss, extra=0x89
    Unknown2 = 2056, // Boss/SJSM1->Boss/SJSM1, extra=0x8E/0x87/0x88
    VulnerabilityUp = 1789, // SJSM2/BossHelper/SJSM1->player, extra=0x1/0x2/0x3/0x4/0x5
}

public enum IconID : uint
{
    Icon198 = 198, // player
    Icon164 = 164, // player
}
