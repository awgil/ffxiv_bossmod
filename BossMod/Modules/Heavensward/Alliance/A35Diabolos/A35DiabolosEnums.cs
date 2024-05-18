namespace BossMod.Heavensward.Alliance.A35Diabolos;

public enum OID : uint
{
    Boss = 0x1907, // R1.500, x1
    Helper = 0x19A, // R0.500, x1, 523 type
    Deathgate = 0x190A, // R3.200, x0 (spawn during fight)
    Lifegate = 0x190C, // R5.440, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 7180, // Boss->players, no cast, range 9+R ?-degree cone

    Camisado = 7181, // Boss->player, no cast, single-target

    Nightmare = 7182, // Boss->self, 4.0s cast, range 50 circle
    NightTerror = 7187, // Boss->players, no cast, range 10 circle
    Noctoshield = 7183, // Boss->self, no cast, single-target
    RuinousOmen1 = 7184, // Boss->self, 15.0s cast, range 50 circle
    RuinousOmen2 = 7185, // Helper->self, 16.0s cast, range 30 circle
    UltimateTerror = 7186, // Boss->self, 4.0s cast, range 18+R circle
    UnknownAbility = 7190, // Boss->self, no cast, single-target
    VoidCall = 7188, // Deathgate->self, 9.0s cast, single-target
}

public enum SID : uint
{
    Nightmare = 423, // Boss->player, extra=0x0
    Noctoshield = 426, // Boss->Helper/Boss, extra=0x0
    DiabolicCurse = 424, // Boss->player, extra=0x1
    HPBoost = 586, // none->Lifegate, extra=0x1/0x2/0x3/0x4
}

public enum IconID : uint
{
    Icon62 = 62, // player
}
