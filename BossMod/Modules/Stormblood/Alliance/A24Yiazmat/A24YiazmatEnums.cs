namespace BossMod.Stormblood.Alliance.A24Yiazmat;

public enum OID : uint
{
    Boss = 0x2253, // R10.000, x1
    Helper = 0x18D6, // R0.500, x24, 523 type
    Actor1ea961 = 0x1EA961, // R0.500, x0 (spawn during fight), EventObj type
    Actor2256 = 0x2256, // R1.000, x0 (spawn during fight)
    Archaeodemon = 0x2257, // R1.950, x0 (spawn during fight)
    Cyclone = 0x2259, // R1.000, x0 (spawn during fight)
    HeartOfTheDragon = 0x2254, // R3.000, x0 (spawn during fight), Part type
    RamzaBasLexentale = 0x23E, // R0.500, x1
    WindAzer = 0x2255, // R0.800, x0 (spawn during fight)
}

public enum AID : uint
{
    BossAutoAttack = 870, // Boss->player, no cast, single-target
    MobAutoAttack = 872, // Archaeodemon->player, no cast, single-target
    AncientAero = 11320, // WindAzer->self, 2.5s cast, range 40+R width 6 rect
    Cyclone1 = 11299, // Boss->self, 7.0s cast, range 30 circle
    Cyclone2 = 11481, // Helper->player, no cast, single-target
    DeathStrike1 = 11315, // Helper->players, no cast, range 5 circle
    DeathStrike2 = 11316, // Boss->self, 5.0s cast, single-target
    DustStorm1 = 11317, // Helper->location, no cast, range 30 circle
    DustStorm2 = 11318, // Boss->self, 3.5s cast, single-target
    FaceOff = 4488, // Archaeodemon->player, no cast, single-target
    GrowingThreat = 11314, // Boss->self, 6.0s cast, single-target
    GustFront = 11300, // Helper->location, 3.5s cast, range 8 circle
    Karma = 9842, // Archaeodemon->self, 3.0s cast, range 30 90-degree cone
    MagneticGenesis1 = 11307, // Helper->self, no cast, single-target
    MagneticGenesis2 = 11322, // Helper->self, no cast, ???
    MagneticGenesis3 = 11323, // Helper->self, no cast, ???
    MagneticLysis = 11306, // Boss->location, 3.5s cast, range 30 circle
    Rake1 = 11302, // Boss->player, 4.0s cast, single-target
    Rake2 = 11303, // Boss->location, no cast, range 5 circle
    Rake3 = 11304, // Boss->location, no cast, range 5 circle
    Rake4 = 11324, // Boss->location, 4.0s cast, range 10 circle
    Rake5 = 11598, // Helper->player, 4.5s cast, range 5 circle
    Rake6 = 11599, // Helper->location, no cast, range 10 circle
    Rake7 = 11826, // Helper->self, 4.5s cast, range 10 circle
    SolarStorm1 = 11308, // Boss->self, 5.0s cast, range 30 circle
    SolarStorm2 = 11474, // Boss->self, no cast, range 31 circle
    StoneBreath = 11305, // Boss->self, 7.0s cast, range 60+R ?-degree cone
    Summon = 11319, // Boss->self, 3.0s cast, single-target
    Turbulence = 11312, // Helper->self, no cast, range 4 circle
    UnholyDarkness = 9843, // Archaeodemon->location, 3.5s cast, range 8 circle
    UnknownAbility = 11614, // Helper->self, no cast, single-target
    WhiteBreath = 11313, // Boss->self, 5.0s cast, range ?-60 donut
}

public enum SID : uint
{
    BorneHeart = 1554, // none->Boss, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    DamageDown = 696, // Helper->player, extra=0x0
    DamageUp = 290, // Boss->Helper/Boss, extra=0x0
    Heartless = 1555, // none->Boss, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0
    MagneticLevitation = 1552, // none->player, extra=0x64
    MagneticLysis1 = 1551, // none->player, extra=0x0
    MagneticLysis2 = 1550, // none->player, extra=0x0
    MeatAndMead = 360, // none->player, extra=0xA
    Petrification = 610, // Boss->player, extra=0x0
    ProperCare = 362, // none->player, extra=0x14
    ReducedRates = 364, // none->player, extra=0x1E
    Slow = 561, // Boss->player, extra=0x0
    TheHeatOfBattle = 365, // none->player, extra=0xA
    TheOneDragon = 1553, // none->Boss, extra=0x0
    Threatened = 1556, // Boss->Boss, extra=0x64
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityDown = 350, // none->Archaeodemon, extra=0x0
    VulnerabilityUp = 202, // Boss/Helper->player, extra=0x1/0x2
    Weakness = 43, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_133 = 133, // Boss
    Icon_134 = 134, // Boss
    Icon_415 = 415, // player
    Icon_62 = 62, // player
}

public enum TetherID : uint
{
    Tether_1 = 1, // Archaeodemon->Archaeodemon
    Tether_4 = 4, // WindAzer->player
}
