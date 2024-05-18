namespace BossMod.Heavensward.Alliance.A14Echidna;

public enum OID : uint
{
    Boss = 0x13F3, // R5.000, x?
    EchidnaHelper = 0x1B2, // R0.500, x?, mixed types
    Dexter = 0x1464, // R2.400, x?
    Sinister = 0x13F5, // R2.400, x?
    Echidna = 0x13F4, // R2.400, x?
    Bloodguard = 0x13F6, // R2.000, x?
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/Bloodguard->player, no cast, single-target
    AutoAttack2 = 1207, // Dexter/Sinister->self, no cast, range 6+R ?-degree cone
    AutoAttack3 = 5148, // Echidna->player, no cast, range 8+R ?-degree cone

    AbyssalReaper = 5144, // Boss->self, 4.0s cast, range 14 circle
    BloodyHarvest = 5147, // EchidnaHelper->location, 4.0s cast, range 12 circle
    Deathstrike = 5195, // Bloodguard->self, 3.0s cast, range 60+R width 6 rect
    DemonicDescent = 5146, // Boss->self, no cast, range 10 circle
    FlameWreath = 5149, // Echidna->location, 6.0s cast, range 18 circle
    Gehenna = 5152, // Boss->self, no cast, range 100 circle
    NemesisBurst = 5145, // Boss->self, no cast, range 6+R circle
    Petrifaction1 = 5154, // Boss->self, 3.0s cast, range 100 circle
    Petrifaction2 = 5374, // Echidna->self, 3.0s cast, range 60 circle
    SerpentineStrike = 5150, // Sinister/Dexter->self, 3.0s cast, range 20 circle
    SickleSlash1 = 5142, // Boss->self, 4.0s cast, single-target
    SickleSlash2 = 5143, // EchidnaHelper->self, 4.0s cast, range 18+R width 60 rect
    SickleSlash3 = 5341, // EchidnaHelper->self, 4.0s cast, range 18+R width 60 rect
    SickleStrike = 5153, // Boss->player, 3.5s cast, single-target
    Unknown = 5151, // EchidnaHelper->EchidnaHelper, no cast, single-target
}

public enum SID : uint
{
    TheRoadTo80 = 1411, // none->player, extra=0x0
    ReducedRates = 364, // none->player, extra=0x1E
    TheHeatOfBattle = 365, // none->player, extra=0xA
    Invincibility = 1570, // none->player, extra=0x0
    VulnerabilityUp = 202, // Boss/EchidnaHelper/Echidna->player, extra=0x1/0x2/0x3/0x4
    VulnerabilityDown = 406, // none->Dexter/Sinister, extra=0x2/0x1/0x5/0x4/0x3
    MagicVulnerabilityUp = 60, // Sinister/Dexter->player, extra=0x0
    Petrification = 610, // Boss/Echidna->player, extra=0x0
}

public enum IconID : uint
{
    Icon63 = 63, // player
}

public enum TetherID : uint
{
    Tether2 = 2, // Sinister->Dexter
}
