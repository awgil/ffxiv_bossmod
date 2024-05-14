namespace BossMod.Stormblood.Alliance.A21Famfrit;

public enum OID : uint
{
    Boss = 0x225A, // R6.480, x?
    RamzaBasLexentale = 0x23E, // R0.500, x?
    Montblanc = 0x239D, // R0.300, x?
    FamfritTheDarkeningCloud = 0x18D6, // R0.500, x?, 523 type
    DarkEwer = 0x225B, // R2.400, x?
    DarkRain = 0x225C, // R2.200, x?
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    BrinyCannonade1 = 11329, // Boss->self, 5.0s cast, single-target
    BrinyCannonade2 = 11333, // FamfritTheDarkeningCloud->player, no cast, range 6 circle
    DarkCannonade1 = 11330, // Boss->self, 6.0s cast, single-target
    DarkCannonade2 = 11332, // FamfritTheDarkeningCloud->player, no cast, single-target
    DarkeningDeluge = 11350, // FamfritTheDarkeningCloud->location, 3.0s cast, range 8 circle
    DarkeningRainfall = 11327, // Boss->self, 3.0s cast, single-target
    DarkEwer = 11331, // Boss->self, 3.0s cast, single-target
    DarkRain1 = 11328, // Boss->self, 4.0s cast, single-target
    DarkRain2 = 11334, // FamfritTheDarkeningCloud->self, 3.5s cast, range 8 circle
    Explosion = 11349, // DarkRain->self, 20.0s cast, range 80 circle
    Jet = 11348, // FamfritTheDarkeningCloud->self, no cast, range 6 circle
    Materialize = 11347, // DarkEwer->self, 3.0s cast, range 6 circle
    TidePod = 11326, // Boss->player, 4.0s cast, single-target
    Tsunami1 = 11336, // Boss->self, 3.0s cast, single-target
    Tsunami2 = 11338, // Boss->self, 3.0s cast, single-target
    Tsunami3 = 11339, // Boss->self, 3.0s cast, single-target
    Tsunami4 = 11340, // Boss->self, no cast, single-target
    Tsunami5 = 11341, // Boss->self, no cast, single-target
    Tsunami6 = 11342, // Boss->self, no cast, single-target
    Tsunami7 = 11343, // Boss->self, no cast, single-target
    Tsunami8 = 11344, // FamfritTheDarkeningCloud->self, 5.0s cast, range 40 circle
    Tsunami9 = 11345, // FamfritTheDarkeningCloud->self, no cast, range 36+R ?-degree cone
    Tsunami10 = 11346, // FamfritTheDarkeningCloud->self, no cast, range 36+R ?-degree cone
    UnknownAbility = 11335, // Boss->location, no cast, ???
    WaterIV = 11325, // Boss->self, 4.0s cast, range 80 circle
}

public enum SID : uint
{
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    Invincibility = 1570, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    MagicVulnerabilityUp = 658, // DarkRain->player, extra=0x1
    Dropsy = 289, // none->player, extra=0x0

}

public enum IconID : uint
{
    Icon_198 = 198, // player
    Icon_139 = 139, // player
    Icon_55 = 55, // player
}
