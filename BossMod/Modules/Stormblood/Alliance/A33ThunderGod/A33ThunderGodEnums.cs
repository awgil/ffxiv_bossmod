namespace BossMod.Stormblood.Alliance.A33ThunderGod;

public enum OID : uint
{
    Boss = 0x25D7, // R19.880, x?
    Helper = 0x233C, // R0.500, x39, mixed types

    Actor1eaa54 = 0x1EAA54, // R0.500, x0 (spawn during fight), EventObj type
    Actor262c = 0x262C, // R0.500, x1
    EphemeralKnight = 0x25D8, // R7.100, x3
    Fran = 0x273E, // R0.595, x1
    Icewolf = 0x25D9, // R2.000, x0 (spawn during fight)
    Montblanc = 0x239D, // R0.300, x1
    Unknown = 0x261E, // R14.000, x3
}

public enum AID : uint
{
    BossAutoAttack = 14151, // Boss->self, no cast, single-target
    HelperAutoAttack = 14152, // Helper->player, no cast, single-target
    EphemeralAutoAttack = 870, // EphemeralKnight->player, no cast, single-target

    BalanceAsunder1 = 14186, // Boss->self, no cast, single-target
    BalanceAsunder2 = 14187, // Helper->self, no cast, range 40 circle

    Burst = 14172, // Icewolf->self, 24.0s cast, range 70 circle
    CleansingStrike = 14161, // Boss->self, 3.0s cast, range 40 circle
    Colosseum = 14178, // Boss->self, no cast, range 40 circle
    CrushAccessory1 = 14170, // Boss->self, 8.0s cast, single-target
    CrushAccessory2 = 14171, // Helper->self, 8.0s cast, range ?-20 donut

    CrushArmor1 = 14168, // Boss->self, 5.0s cast, single-target
    CrushArmor2 = 14169, // Helper->player, no cast, single-target

    CrushHelm1 = 14162, // Boss->self, 6.0s cast, single-target
    CrushHelm2 = 14163, // Helper->player, no cast, single-target
    CrushHelm3 = 14164, // Helper->player, no cast, single-target

    CrushWeapon1 = 14165, // Boss->self, 4.0s cast, single-target
    CrushWeapon2 = 14166, // Helper->self, 4.0s cast, range 6 circle
    CrushWeapon3 = 14167, // Helper->self, no cast, range 6 circle

    DivineRuination1 = 14158, // Helper->self, no cast, range 16 circle
    DivineRuination2 = 14179, // EphemeralKnight->self, 5.0s cast, single-target
    DivineRuination3 = 14180, // Helper->self, no cast, range 70 width 6 rect

    Duskblade = 14177, // Boss->self, 10.0s cast, range 70 circle

    HallowedBolt1 = 14155, // Helper->self, no cast, range 15 circle
    HallowedBolt2 = 14181, // EphemeralKnight->self, 5.0s cast, single-target
    HallowedBolt3 = 14182, // Helper->self, 5.0s cast, range 15 circle
    HallowedBolt4 = 14183, // Helper->self, 5.0s cast, range ?-30 donut
    HallowedBolt5 = 14184, // Helper->players, no cast, range 6 circle

    JudgmentBlade = 15060, // Helper->self, no cast, range ?-35 donut
    NorthswainsStrike = 15059, // Helper->self, no cast, range 26 circle
    Shadowblade1 = 14173, // Boss->self, 8.0s cast, single-target
    Shadowblade2 = 14174, // Helper->players, no cast, range 5 circle
    Shadowblade3 = 14175, // Helper->self, no cast, range 1 circle
    Shadowblade4 = 14176, // Helper->self, no cast, range 70 circle

    TGHolySword1 = 14153, // Boss->self, 6.0s cast, single-target
    TGHolySword2 = 14154, // Boss->self, 6.0s cast, single-target
    TGHolySword3 = 14156, // Boss->self, 8.0s cast, single-target
    TGHolySword4 = 14157, // Boss->self, 8.0s cast, single-target
    TGHolySword5 = 14159, // Boss->self, 6.0s cast, single-target
    TGHolySword6 = 14160, // Boss->self, 6.0s cast, single-target

    Unknown1 = 14059, // Helper->self, no cast, single-target
    Unknown2 = 14372, // Helper->self, no cast, single-target
}

public enum SID : uint
{
    Bleeding = 643, // Helper->player, extra=0x4
    BrinkOfDeath = 44, // none->player, extra=0x0
    Doom = 1769, // Boss->player, extra=0x0
    Electrocution = 271, // Helper->player, extra=0x0
    Frostbite = 285, // none->player, extra=0x0
    MagicVulnerabilityUp = 1138, // Helper->player, extra=0x0
    PhysicalVulnerabilityUp1 = 200, // Helper->player, extra=0x1/0x2/0x3/0x4
    PhysicalVulnerabilityUp2 = 695, // Helper->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 202, // Helper/Icewolf->player, extra=0x1/0x2/0x4/0x3
    Weakness = 43, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_170 = 170, // player
    Nox = 197, // player chasing AOE icon
    Icon_23 = 23, // player
    Icon_62 = 62, // player
    Icon_110 = 110, // player
}

public enum TetherID : uint
{
    Tether_50 = 50, // EphemeralKnight->player
    Tether_84 = 84, // Helper->player
}
