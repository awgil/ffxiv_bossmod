namespace BossMod.Endwalker.Savage.P4S1Hesperos;

public enum OID : uint
{
    Boss = 0x35FD, // first phase boss
    Pinax = 0x35FE, // '', 4x exist at start at [90/110, 90/110]
    Orb = 0x35FF, // orbs spawned by Belone Bursts
    Helper = 0x233C, // 38 exist at start
};

public enum AID : uint
{
    ElegantEviscerationSecond = 26649, // Boss->target, no cast, second hit
    SettingTheScene = 27083, // Boss->Boss
    ShiftStart = 27086, // Boss->Boss, no cast, tp to center, sword glowing (?)
    Pinax = 27087, // Boss->Boss
    MekhaneAcid = 27088, // Helper->target, no cast
    MekhaneLava = 27089, // Helper->target, no cast
    MekhaneWell = 27090, // Helper->Helper, no cast, affects arena
    MekhaneLevinstrike = 27091, // Helper->Helper, no cast, affects arena
    PinaxAcid = 27092, // Helper->Helper, affects corner
    PinaxLava = 27093, // Helper->Helper, affects corner
    PinaxWell = 27094, // Helper->Helper, affects corner
    PinaxLevinstrike = 27095, // Helper->Helper, affects corner
    Bloodrake = 27096, // Boss->Boss
    BeloneBursts = 27097, // Boss->Boss
    BeloneBurstsAOETank = 27098, // Orb->target, no cast
    BeloneBurstsAOEHealer = 27099, // Orb->target, no cast
    BeloneBurstsAOEDPS = 27100, // Orb->target, no cast
    BeloneCoils = 27101, // Boss->Boss
    BeloneCoilsDPS = 27102, // Helper->Helper, role towers ('no heals/tanks' variant)
    BeloneCoilsTH = 27103, // Helper->Helper, role towers ('no dps' variant)
    DirectorsBelone = 27110, // Boss->Boss
    DirectorsBeloneDebuffs = 27111, // Helper->target, no cast, just applies Role Call debuffs
    CursedCasting2 = 27112, // Helper->target, no cast, during second director's belone, does something bad if no role call?..
    CursedCasting1 = 27113, // Helper->target, no cast, during first director's belone, does something bad if no role call?..
    AethericChlamys = 27116, // Boss->Boss
    InversiveChlamys = 27117, // Boss->Boss
    InversiveChlamysAOE = 27119, // Helper->target, no cast, damage to tethered targets
    ElementalBelone = 27122, // Boss->Boss
    Periaktoi = 27124, // Boss->Boss
    PeriaktoiSafeAcid = 27125, // Helper->Helper (unconfirmed)
    PeriaktoiSafeLava = 27126, // Helper->Helper
    PeriaktoiSafeWell = 27127, // Helper->Helper
    PeriaktoiSafeLevinstrike = 27128, // Helper->Helper
    PeriaktoiDangerAcid = 27129, // Helper->Helper
    PeriaktoiDangerLava = 27130, // Helper->Helper
    PeriaktoiDangerWell = 27131, // Helper->Helper
    PeriaktoiDangerLevinstrike = 27132, // Helper->Helper
    NortherlyShiftCloak = 27133, // Boss->Boss
    SoutherlyShiftCloak = 27134, // Boss->Boss (unconfirmed)
    EasterlyShiftCloak = 27135, // Boss->Boss (unconfirmed)
    WesterlyShiftCloak = 27136, // Boss->Boss (unconfirmed)
    ShiftingStrikeCloak = 27137, // Helper->Helper
    NortherlyShiftSword = 27138, // Boss->Boss
    SoutherlyShiftSword = 27139, // Boss->Boss
    EasterlyShiftSword = 27140, // Boss->Boss
    WesterlyShiftSword = 27141, // Boss->Boss (unconfirmed)
    ShiftingStrikeSword = 27142, // Helper->Helper, sword attack
    ElegantEvisceration = 27144, // Boss->target
    Decollation = 27145, // Boss->Boss
    VengefulBelone = 28194, // Boss->Boss
    InversiveChlamysAOE2 = 28437, // Helper->target, no cast, damage to tethered targets (during belone coils)
};

public enum SID : uint
{
    OrbRole = 2056,
    ThriceComeRuin = 2530,
    RoleCall = 2802,
    Miscast = 2803,
    ActingDPS = 2925,
    ActingHealer = 2926,
    ActingTank = 2927,
}

public enum TetherID : uint
{
    ExplosiveAether = 17,
    Chlamys = 89,
    Bloodrake = 165,
}
