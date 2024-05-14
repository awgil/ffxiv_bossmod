namespace BossMod.Stormblood.Trial.T04Tsukuyomi;

public enum OID : uint
{
    Boss = 0x2210, // R3.250, x1
    Specter = 0x18D6, // R0.500, x1 (spawn during fight), mixed types
    DancingFan = 0x2241, // R1.600, x0 (spawn during fight)
    MidnightHaze = 0x2242, // R1.000, x0 (spawn during fight)
    SpecterOfGosetsu = 0x2248, // R0.600, x0 (spawn during fight)
    Yotsuyu = 0x2249, // R0.600, x0 (spawn during fight)
    SpecterOfZenos = 0x2247, // R1.152, x0 (spawn during fight)
    SpecterOfThePatriarch = 0x2244, // R0.600, x0 (spawn during fight)
    SpecterOfTheMatriarch = 0x2245, // R0.600, x0 (spawn during fight)
    SpecterOfTheEmpire = 0x224B, // R0.600, x0 (spawn during fight)
    SpecterOfTheHomeland = 0x224A, // R0.600, x0 (spawn during fight)
    SpecterOfAsahi = 0x2246, // R0.600, x0 (spawn during fight)
    Moonlight = 0x2278, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 11523, // Boss->player, no cast, single-target
    SpecterAutoAttack = 11542, // SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfTheEmpire->player/Yotsuyu, no cast, single-target
    SpecterOfTheHomelandAuto = 11543, // SpecterOfTheHomeland->player, no cast, single-target
    SpecterOfAsahiAuto = 11858, // SpecterOfAsahi->Yotsuyu, no cast, single-target
    BossAutoAttack = 870, // Boss->player, no cast, single-target

    Antitwilight = 11256, // Boss->self, 5.0s cast, range 100 circle
    Concentrativity = 11247, // SpecterOfZenos->self, no cast, range 100 circle

    DanceOfTheDead1 = 11551, // Specter->self, no cast, single-target
    DanceOfTheDead2 = 11897, // Boss->self, no cast, range 100 circle

    DarkBlade = 11257, // Boss->self, 3.0s cast, range 40+R 210-degree cone
    Dispersivity = 11248, // Specter->self, no cast, range 100 circle
    LeadOfTheUnderworld = 11238, // Boss->self/players, 5.0s cast, range 40+R width 8 rect

    Lunacy1 = 11259, // Boss->players, 5.0s cast, range 6 circle
    Lunacy2 = 11260, // Boss->players, no cast, range 6 circle

    LunarHalo = 11379, // Moonlight->self, 4.0s cast, range ?-15 donut

    MidnightHaze1 = 11240, // Boss->location, 4.0s cast, single-target
    MidnightHaze2 = 11241, // Boss->location, no cast, single-target

    Nightbloom1 = 11246, // Boss->self, 6.0s cast, range 100 circle
    Nightbloom2 = 11438, // Yotsuyu->self, no cast, single-target
    Nightbloom3 = 11440, // Specter->self, 4.0s cast, range 60 circle

    Nightfall1 = 11236, // Boss->self, 4.0s cast, single-target
    Nightfall2 = 11237, // Boss->self, 4.0s cast, single-target

    Reprimand = 11234, // Boss->self, 4.0s cast, range 100 circle
    Selenomancy = 11249, // Boss->self, 4.0s cast, single-target
    SteelOfTheUnderworld = 11239, // Boss->self, 3.0s cast, range 40+R 90-degree cone
    ToAshes = 11243, // MidnightHaze->self, 20.0s cast, range 100 circle

    TormentUntoDeath1 = 11235, // Boss->self/player, 4.0s cast, range 15+R ?-degree cone
    TormentUntoDeath2 = 11955, // Boss->self/player, 4.0s cast, range 15+R ?-degree cone

    TsukiNoMaiogi = 11245, // DancingFan->self, 5.0s cast, range 10 circle
    UnknownAbility = 11441, // Specter->player, no cast, single-target

    UnknownWeaponskill1 = 11200, // Boss->self, no cast, single-target
    UnknownWeaponskill2 = 11210, // SpecterOfZenos->self, 3.0s cast, single-target
    UnknownWeaponskill3 = 11211, // SpecterOfGosetsu->location, no cast, width 8 rect charge
    UnknownWeaponskill4 = 11261, // Boss->self, no cast, single-target
    UnknownWeaponskill5 = 11471, // Boss->self, no cast, single-target
    UnknownWeaponskill6 = 11478, // SpecterOfGosetsu->location, no cast, ???

    UnmovingTroika1 = 11435, // SpecterOfZenos->self, no cast, range 9+R ?-degree cone
    UnmovingTroika2 = 11436, // Specter->self, 1.7s cast, range 9+R ?-degree cone
    UnmovingTroika3 = 11437, // Specter->self, 2.1s cast, range 9+R ?-degree cone

    ZashikiAsobi = 11244, // Boss->self, 4.0s cast, single-target

    Perilune = 11255, // Boss->self, 5.0s cast, range 100 circle
    BrightBlade = 11258, // Boss->self, 3.0s cast, range 40+R 210-degree cone
}

public enum SID : uint
{
    VulnerabilityUp = 202, // DancingFan/Boss->player, extra=0x1/0x2
    DownForTheCount = 783, // Boss->player, extra=0xEC7
    Haunt1 = 1542, // none->SpecterOfZenos/SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfTheEmpire/SpecterOfTheHomeland/SpecterOfAsahi, extra=0x0
    Grudge = 1573, // none->Yotsuyu, extra=0x0
    Stun = 149, // SpecterOfZenos->player, extra=0x0
    Haunt2 = 1543, // none->SpecterOfGosetsu, extra=0x0
    Moonshadowed = 1539, // none->player, extra=0x1/0x2/0x3/0x4
    Moonlit = 1538, // none->player, extra=0x1/0x2/0x3/0x4/0x5
    Doom = 210, // none->player, extra=0x0
    Bleeding = 642, // none->player, extra=0x0
    BloodMoon = 1537, // Boss->Boss, extra=0x58
}

public enum IconID : uint
{
    Icon_230 = 230, // player
    Icon_305 = 305, // player
}

public enum TetherID : uint
{
    Tether_12 = 12, // MidnightHaze->MidnightHaze
    Tether_17 = 17, // SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfAsahi->Yotsuyu
}