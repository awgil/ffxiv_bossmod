namespace BossMod.Stormblood.Alliance.A35UltimaP2;

public enum OID : uint
{
    Boss = 0x25A9, // R8.000, x1
    Helper = 0x233C, // R0.500, x41
    AuraciteShard = 0x2639, // R1.250, x0 (spawn during fight)
    Dominion1 = 0x25AE, // R1.700, x0 (spawn during fight)
    Dominion2 = 0x25B6, // R1.700, x0 (spawn during fight)
    Dominion3 = 0x2689, // R1.000, x0 (spawn during fight)
    UltimaP1Boss = 0x2604, // R10.400, x1
}

public enum AID : uint
{
    AutoAttack = 14501, // Boss->player, no cast, single-target

    Auralight1 = 14504, // Helper->self, 2.5s cast, range 50 width 10 rect
    Auralight2 = 14505, // Helper->self, 2.5s cast, range 25 width 10 rect

    Bombardment = 14526, // Helper->location, 2.5s cast, range 6 circle
    Cataclysm = 14500, // Boss->self, no cast, single-target
    DemiVirgo = 14530, // Boss->self, 3.0s cast, single-target

    EastwardMarch1 = 14494, // Boss->self, 4.0s cast, single-target
    EastwardMarch2 = 14496, // Helper->self, no cast, single-target

    Embrace1 = 14520, // 25AE->self, 3.0s cast, single-target
    Embrace2 = 14521, // Helper->location, 3.0s cast, range 3 circle
    Embrace3 = 14522, // Helper->self, no cast, range 7 circle

    FlareIV = 14493, // Helper->players, 5.5s cast, range 80 circle
    UltimateFlare = 14525, // Dominion2->location, no cast, range 80 circle

    GrandCross1 = 14510, // 2639->self, 4.0s cast, range 60 width 15 cross
    GrandCross2 = 14536, // Boss->self, 3.0s cast, single-target

    Holy = 14507, // Helper->location, 2.5s cast, range 2 circle

    HolyIV1 = 14490, // Helper->location, 3.0s cast, range 6 circle
    HolyIV2 = 14491, // Helper->players, 5.5s cast, range 6 circle
    HolyIV3 = 14534, // Boss->self, 3.0s cast, single-target

    LifeDrain = 14523, // Helper->player, no cast, single-target
    Penultima = 14524, // 25B6->player, no cast, single-target
    Plummet = 14509, // 2639->self, 3.5s cast, range 15 width 15 rect

    RayOfLight1 = 14518, // 25AE->self, no cast, single-target
    RayOfLight2 = 14519, // Helper->self, no cast, range 70 width 10 rect

    Redemption = 14506, // Boss->player, 4.0s cast, single-target
    Shockwave = 14484, // Helper->self, no cast, range 80 circle

    WestwardMarch1 = 14497, // Helper->self, no cast, single-target
    WestwardMarch2 = 14503, // Boss->self, 4.0s cast, single-target
}

public enum SID : uint
{
    AccelerationBomb = 1072, // none->player, extra=0x0
    Bind = 564, // Helper->player, extra=0x0
    Clashing = 1271, // none->player, extra=0x17E3
    Fearless = 1747, // 25B6->player, extra=0x0
    OutOfTheAction = 1284, // none->player, extra=0x63E
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 202, // 2639->player, extra=0x2/0x1
    Weakness = 43, // none->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    Bleeding = 320, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon63 = 63, // player
    Icon75 = 75, // player
    Icon87 = 87, // player
    Icon127 = 127, // player
    Icon139 = 139, // player
    Icon186 = 186, // player
    Icon198 = 198, // player
}

public enum TetherID : uint
{
    Tether84 = 84, // 25AE->player
}
