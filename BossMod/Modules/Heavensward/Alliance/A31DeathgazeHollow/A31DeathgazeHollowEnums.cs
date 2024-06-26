namespace BossMod.Heavensward.Alliance.A31DeathgazeHollow;

public enum OID : uint
{
    Boss = 0x190F, // R26.000, x1
    Helper = 0x1911, // R0.500, x9
    FierceWind = 0x1912, // R1.000, x0 (spawn during fight)
    VoidSprite = 0x1910, // R2.000, x0 (spawn during fight)
    Actor1ea2e7 = 0x1EA2E7, // R0.500, x0 (spawn during fight), EventObj type

    Actor1ea273 = 0x1EA273, // R2.000, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    Benediction = 6844, // Helper->location, 5.0s cast, range 6 circle

    BitingWind = 7317, // FierceWind->self, no cast, range 4 circle

    BoltOfDarkness1 = 7302, // Boss->self, 6.0s cast, single-target
    BoltOfDarkness2 = 7303, // Boss->self, 6.0s cast, single-target
    BoltOfDarkness3 = 7315, // Helper->self, 6.8s cast, range 31+R width 20 rect

    DarkII = 7318, // VoidSprite->self, 2.5s cast, range 50+R 60-degree cone

    Doomsay = 7301, // Boss->self, 3.0s cast, range 100 circle
    Doomsay2 = 7300, // Boss->self, 3.0s cast, range 100 circle

    Ruin = 2214, // VoidSprite->player, 1.0s cast, single-target

    SpikeOfDarkness = 7761, // Boss->player, no cast, single-target

    VoidAeroII = 7308, // Helper->players, no cast, range 5 circle
    VoidAeroII3 = 7288, // Boss->self, 5.0s cast, single-target

    VoidAeroIII1 = 7290, // Boss->self, 3.0s cast, single-target
    VoidAeroIII2 = 7291, // Boss->self, 3.0s cast, single-target
    VoidAeroIIIAOE = 7309, // Helper->self, 6.0s cast, range 10 circle

    VoidAeroIIV = 7289, // Boss->self, 5.0s cast, single-target

    VoidAeroIVKB1 = 7292, // Boss->self, 2.5s cast, single-target
    VoidAeroIVKB2 = 7293, // Boss->self, 2.5s cast, single-target

    Unknown3 = 7310, // Helper->self, 6.0s cast, range 40 circle kb 20
    VoidAeroIV2 = 7311, // Helper->self, 3.0s cast, range 31+R width 60 rect kb 37

    VoidBlizzardIII1 = 7284, // Boss->self, 2.5s cast, single-target
    VoidBlizzardIII2 = 7285, // Boss->self, 2.5s cast, single-target
    VoidBlizzardIIIAOE = 7306, // Helper->self, 3.0s cast, range 40+R ?-degree cone

    VoidBlizzardIV1 = 7286, // Boss->self, 3.0s cast, single-target
    VoidBlizzardIV2 = 7287, // Boss->self, 3.0s cast, single-target
    VoidBlizzardIVAOE = 7307, // Helper->location, 5.0s cast, range 40 circle

    VoidDeath = 7312, // Helper->self, 9.0s cast, range 10 circle
    VoidDeathKB = 7313, // Helper->self, no cast, range 40 circle
    VoidDeathKB2 = 7314, // Helper->self, no cast, range 40 circle

    VoidDeath3 = 7294, // Boss->self, 8.4s cast, single-target
    Unknown4 = 7297, // Boss->self, no cast, single-target
    VoidDeathIV1 = 7298, // Boss->self, 3.0s cast, single-target
    VoidDeathIV2 = 7299, // Boss->self, 3.0s cast, single-target
    VoidDeathVisual = 7295, // Boss->self, 8.4s cast, single-target
}

public enum SID : uint
{
    Frostbite = 268, // Helper->player, extra=0x0
    Doom = 910, // Boss->player, extra=0x0
    Windburn = 269, // FierceWind->player, extra=0x0
    Bleeding = 273, // Helper->player, extra=0x0
    Heavy = 1107, // VoidSprite->player, extra=0x32
}

public enum IconID : uint
{
    WindSpread = 70,
}
