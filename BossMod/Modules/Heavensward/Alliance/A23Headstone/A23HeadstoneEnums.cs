namespace BossMod.Heavensward.Alliance.A23Headstone;

public enum OID : uint
{
    Boss = 0x166F, // R4.500, x?
    VoidFire = 0x1671, // R0.600-0.900, x?
    Parthenope = 0x1670, // R2.000, x?
}

public enum AID : uint
{
    AutoAttack = 872, // Parthenope->player, no cast, single-target
    FieryEpigraph = 6227, // Boss->player, no cast, single-target
    Epigraph = 6226, // Boss->self, no cast, range 100+R width 8 rect
    TremblingEpigraph = 6229, // Boss->self, 1.0s cast, range 100 circle
    FlaringEpigraph = 6230, // Boss->self, 30.0s cast, range 81 circle
    BigBurst = 6233, // VoidFire->self, 30.0s cast, range 100 circle
}

public enum SID : uint
{
    BrinkOfDeath = 44, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0
    VulnerabilityDown = 350, // none->player, extra=0x0
}
