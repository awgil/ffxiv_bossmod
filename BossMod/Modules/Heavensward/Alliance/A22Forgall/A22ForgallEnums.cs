namespace BossMod.Heavensward.Alliance.A22Forgall;

public enum OID : uint
{
    Boss = 0x1627, // R2.600, x?
    ShriveledTalon1 = 0x1629, // R1.000, x?
    ShriveledTalon2 = 0x162A, // R1.000, x?
    ShriveledTalon3 = 0x162B, // R1.000, x?
    ShriveledTalon4 = 0x1628, // R1.000, x1
    Forgall = 0x1631, // R0.500, x?, 523 type
    PoisonMist = 0x1630, // R1.500, x?
    SummonedDahak = 0x162E, // R5.000, x?
    SummonedSuccubus = 0x162D, // R2.200, x?
    SummonedHaagenti = 0x162F, // R2.800, x?
}

public enum AID : uint
{
    AutoAttack = 6101, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // 1628/ShriveledTalon2/ShriveledTalon3/ShriveledTalon1/SummonedDahak/SummonedHaagenti/SummonedSuccubus->player, no cast, single-target

    Necropurge = 6103, // ShriveledTalon1/ShriveledTalon3/1628/ShriveledTalon2->location, 2.0s cast, range 10 circle

    MegiddoFlame1 = 6080, // Boss->self, 3.0s cast, single-target
    MegiddoFlame2 = 6081, // Forgall->self, 3.0s cast, range 50+R width 8 rect

    Necropurge1 = 6078, // Boss->self, 3.0s cast, single-target
    Necropurge2 = 6079, // Forgall->location, 12.5s cast, range 8 circle

    BrandOfTheFallen = 6092, // Boss->self, 6.0s cast, ???
    DarkEruption1 = 6082, // Boss->self, 2.0s cast, single-target
    DarkEruption2 = 6083, // Forgall->location, 2.0s cast, range 6 circle
    EvilMist = 6085, // Boss->self, 3.0s cast, range 60+R circle
    HellWind = 6091, // Boss->self, 5.0s cast, range 80+R circle
    ManaExplosion = 6088, // Forgall->self, 1.7s cast, range 60 circle
    MegaDeath = 6090, // Boss->self, 11.0s cast, range 80+R circle

    MortalRay = 6100, // SummonedHaagenti->self, 4.0s cast, range 38+R ?-degree cone
    Mow = 6098, // SummonedHaagenti->self, 3.0s cast, range 11+R 120-degree cone

    Necropurge3 = 6104, // PoisonMist->location, 2.0s cast, range 10 circle
    PunishingRay = 6084, // Forgall->self, no cast, range 60 circle
    RottenBreath = 6096, // SummonedDahak->self, no cast, range 5+R ?-degree cone

    TailDrive = 6097, // SummonedDahak->self, 2.5s cast, range 25+R 90-degree cone

    Unknown = 6087, // Forgall->self, no cast, ???
    Voidblood = 6099, // SummonedHaagenti->players, 5.0s cast, range 6 circle
    VoidCall = 6086, // Boss->location, 8.0s cast, range 15 circle
}

public enum SID : uint
{
    GradualZombification = 1045, // ShriveledTalon2/ShriveledTalon4/PoisonMist/Forgall->player, extra=0x1
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    Invincibility = 1570, // none->player, extra=0x0
    VulnerabilityUp = 202, // Forgall->player, extra=0x1/0x2
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Zombification = 371, // Forgall/ShriveledTalon2->player, extra=0x0
    Bleeding = 320, // none->player, extra=0x0
    BrandOfTheFallen = 1065, // Boss->player, extra=0x0
    Toad = 439, // Boss->player, extra=0x1
    Poison = 559, // SummonedHaagenti->player, extra=0x0
    Concussion = 266, // SummonedDahak->player, extra=0x0
    Doom = 910, // SummonedHaagenti->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    SweetSteel = 6093, // SummonedSuccubus->self, 3.0s cast, range 14+R 120-degree cone
}

public enum IconID : uint
{
    DoritoStack = 55, // player
    Icon23 = 23, // player
    Icon25 = 25, // player
}

public enum TetherID : uint
{
    Tether37 = 37, // ShriveledTalon1/ShriveledTalon3/ShriveledTalon4/ShriveledTalon2->Boss
}
