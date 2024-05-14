namespace BossMod.Heavensward.Alliance.A36DiabolosHollow;

public enum OID : uint
{
    Boss = 0x1908, // R3.000, x1
    Helper = 0x19A, // R0.500, x13, 523 type
    Deathgate = 0x190A, // R3.200, x0 (spawn during fight)
    DiabolicGate = 0x190B, // R3.200, x0 (spawn during fight)
    Shadowsphere = 0x19AF, // R1.500, x0 (spawn during fight)
    NightHound = 0x190E, // R5.850, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 7742, // Boss->player, no cast, single-target

    Blindside = 7208, // Boss->self, no cast, range 9+R ?-degree cone
    CriticalGravity = 7767, // Shadowsphere->self, no cast, range 8+R circle
    DeepFlow = 7189, // DiabolicGate->self, 28.0s cast, range 40 circle
    DoubleEdge = 7211, // Boss->self, 3.0s cast, single-target

    EarthShaker1 = 7209, // Boss->self, 4.0s cast, single-target
    EarthShaker2 = 7210, // Helper->self, no cast, range 60+R ?-degree cone

    HollowCamisado = 7193, // Boss->player, 3.0s cast, single-target
    HollowNight = 7197, // Boss->players, no cast, range 8 circle
    HollowNightmare = 7200, // Boss->self, 4.0s cast, range 50 circle
    HollowOmen1 = 7202, // Boss->self, 15.0s cast, range 50 circle
    HollowOmen2 = 7203, // Helper->self, 20.0s cast, range 30 circle
    Hollowshield = 7198, // Boss->self, no cast, single-target

    NoxAOEFirst = 7196, // Helper->location, 5.0s cast, range 10 circle
    NoxAOERest = 7195, // Helper->location, no cast, range 10 circle

    ParticleBeam1 = 7204, // Helper->location, no cast, range 60 circle
    ParticleBeam2 = 7205, // Helper->location, no cast, range 5 circle
    ParticleBeam3 = 7206, // Helper->location, no cast, range 60 circle
    ParticleBeam4 = 7207, // Helper->location, no cast, range 5 circle

    PavorInanis = 7199, // Boss->self, 4.0s cast, range 40 circle

    Shadethrust = 7194, // Boss->location, 3.0s cast, range 40+R width 5 rect
    UnknownAbility = 7192, // Boss->location, no cast, ???
    VoidCall = 7188, // Deathgate->self, 9.0s cast, single-target

    Attack = 870, // NightHound->player, no cast, single-target
    RavenousBite = 7687, // NightHound->player, no cast, single-target

}

public enum SID : uint
{
    BrinkOfDeath = 44, // none->player, extra=0x0
    DiabolicCurse = 424, // Boss->player, extra=0x1/0x2
    Doom = 910, // Boss->player, extra=0x0
    FleshWound = 264, // Boss->player, extra=0x0
    Hysteria = 296, // Boss->player, extra=0x0
    Invincibility1 = 325, // none->DiabolicGate, extra=0x0
    Invincibility2 = 776, // none->player, extra=0x0
    KeenEdge = 1145, // Boss->Boss, extra=0x0
    MeatAndMead = 360, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    TheHeatOfBattle = 365, // none->player, extra=0xA
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityDown = 929, // Boss->Helper/Boss, extra=0x0
    Weakness = 43, // none->player, extra=0x0
}

public enum IconID : uint
{
    Nox = 197, // player chasing AOE icon
    Icon_91 = 91, // player
    Icon_93 = 93, // player
    Icon_40 = 40, // player
}