namespace BossMod.Heavensward.Alliance.A13Cuchulainn;

public enum OID : uint
{
    Boss = 0x1413, // R6.875, x?
    CuchulainnHelper = 0x1418, // R0.500, x?
    Gyrtower = 0x1414, // R2.000, x?
    PlanarFissure = 0x1416, // R2.000, x?
    Foobar = 0x1417, // R2.400, x?
    BlackPhlegm = 0x1415, // R1.500, x?
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Foobar->player, no cast, single-target
    Beckon = 5184, // Boss->self, 7.0s cast, range 30+R 60-degree cone
    BileBelow = 5181, // Boss->self, 6.0s cast, ???
    BileBombardment = 5177, // CuchulainnHelper->location, 3.0s cast, range 8 circle
    BlackLung = 5182, // Boss->self, 3.0s cast, range 80+R circle
    CorrosiveBile1 = 5174, // Boss->self, 2.0s cast, single-target
    CorrosiveBile2 = 5175, // CuchulainnHelper->self, no cast, range 18+R ?-degree cone
    Corruption = 5191, // BlackPhlegm->self, no cast, range 5+R circle
    Devour = 5185, // Boss->self, no cast, ???
    Expel = 5186, // Boss->self, no cast, single-target
    FlailingTentacles1 = 5178, // Boss->self, 5.0s cast, single-target
    FlailingTentacles2 = 5179, // CuchulainnHelper->self, 5.0s cast, range 32+R width 7 rect
    GrandCorruption = 5192, // BlackPhlegm->self, no cast, range 80+R circle
    IdolOfImpurity = 5180, // Boss->self, 3.0s cast, single-target
    Malaise = 5176, // Boss->self, no cast, single-target
    Pestilence = 5188, // Boss->self, 4.0s cast, range 80+R circle
    VoidPact = 5183, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    Bleeding = 940, // none->player, extra=0x1/0x5/0x4/0x3/0x2
    Poison1 = 560, // CuchulainnHelper->player, extra=0x1/0x2/0x3/0x4/0x5
    Concussion = 266, // CuchulainnHelper->player, extra=0x0
    VulnerabilityDown = 350, // none->CuchulainnHelper/Boss, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    StoneskinPhysical = 152, // none->CuchulainnHelper/Boss, extra=0x0
    StoneskinMagical = 153, // none->CuchulainnHelper/Boss, extra=0x0
    Poison2 = 18, // none->player, extra=0x0
    MagicVulnerabilityUp = 658, // BlackPhlegm->player, extra=0x1/0x2/0x3/0x4/0x5/0x6
    OutOfTheAction = 939, // BlackPhlegm->player, extra=0x0
    Bind = 13, // BlackPhlegm->player, extra=0x0
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
}
