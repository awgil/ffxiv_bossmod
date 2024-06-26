namespace BossMod.Heavensward.Alliance.A25Calofisteri;

public enum OID : uint
{
    Boss = 0x1661, // R5.500, x?
    CalofisteriHelper1 = 0x1B2, // R0.500, x?, 523 type
    CalofisteriHelper2 = 0x138F, // R0.500, x?, 523 type
    LurkingLock = 0x1667, // R1.000, x?
    Entanglement = 0x1668, // R5.920, x?
    GrandBijou = 0x1666, // R1.430, x?
    Bijou1 = 0x1665, // R0.660, x?
    Bijou2 = 0x1682, // R0.330, x?
    LivingLock1 = 0x1663, // R3.600, x?
    LivingLock2 = 0x1664, // R3.600, x?
    LivingLock3 = 0x1662, // R3.600, x?
}

public enum AID : uint
{
    AuraBurst = 6177, // Boss->self, no cast, range 60 circle
    BloodiedNail = 6175, // Boss->self, no cast, range 10+R ?-degree cone

    CoifChange1 = 6154, // Boss->self, no cast, single-target
    CoifChange2 = 6158, // Boss->self, no cast, single-target

    DancingMad = 6168, // Boss->self, no cast, single-target
    DancingMad1 = 6170, // Boss->self, no cast, range 60 circle
    DancingMad2 = 6171, // CalofisteriHelper2->self, no cast, range 60 circle

    DepthCharge = 6176, // Boss->location, 5.0s cast, width 10 rect charge

    EvilCurl = 6166, // LivingLock1->self, no cast, range 15 circle
    EvilSwitch = 6165, // LivingLock3->self, no cast, range 60+R width 18 rect
    EvilTress = 6167, // LivingLock2->self, no cast, range 12 circle

    Extension1 = 6162, // Boss->self, 3.0s cast, single-target
    Extension2 = 6163, // CalofisteriHelper1->location, 3.0s cast, range 6 circle

    FeintParticleBeam1 = 6439, // CalofisteriHelper1->location, 3.5s cast, range 3 circle
    FeintParticleBeam2 = 6440, // CalofisteriHelper1->location, no cast, range 3 circle

    Garrotte1 = 6407, // Entanglement->self, no cast, ???
    Garrotte2 = 6174, // Entanglement->self, 7.0s cast, ???

    Graft = 6172, // LurkingLock->self, 3.0s cast, range 5 circle

    Haircut1 = 6155, // Boss->self, 2.0s cast, range 60+R 180-degree cone
    Haircut2 = 6159, // Boss->self, 2.0s cast, range 60+R 180-degree cone

    ManaDrain = 6169, // Boss->self, no cast, single-target
    Penetration = 6178, // Boss->self, 5.0s cast, range 60 circle //in kb

    Recharge1 = 6437, // Bijou1->Boss, 30.0s cast, single-target
    Recharge2 = 6438, // GrandBijou->Boss, 60.0s cast, single-target

    SplitEnd1 = 6156, // Boss->self, no cast, range 20+R ?-degree cone
    SplitEnd2 = 6160, // Boss->self, no cast, range 20+R ?-degree cone

    Unknown1 = 6157, // Boss->self, no cast, single-target
    Unknown2 = 6173, // Entanglement->self, no cast, ???
    Unknown3 = 6161, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    Fetters = 668, // Entanglement->player, extra=0x0
    FleshWound = 264, // Boss->player, extra=0x0
    VulnerabilityUp1 = 202, // CalofisteriHelper1->player, extra=0x1/0x2
    VulnerabilityUp2 = 714, // LivingLock1/LivingLock2/LivingLock3->player, extra=0x1/0x2/0x4/0x3/0x5
    Stun = 149, // Boss->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Nox = 197, // player Nox Chasing AOE icon
}
