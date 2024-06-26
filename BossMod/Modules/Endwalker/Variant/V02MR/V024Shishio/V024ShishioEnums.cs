namespace BossMod.Endwalker.Variant.V02MR.V024Shishio;

public enum OID : uint
{
    Boss = 0x3F40, // R3.450, x1
    Helper = 0x233C, // R0.500, x13, 523 type
    Iwakura = 0x1EB8BD, // R0.500, x1, EventObj type
    Raiun = 0x3F41, // R0.800, x0 (spawn during fight)
    Rairin = 0x3F42, // R1.000, x0 (spawn during fight)
    VenomousThrallSnakes = 0x3F45, // R1.440, x0 (spawn during fight)
    FeralThrallTigers = 0x3F44, // R3.250, x0 (spawn during fight)
    CleverThrallBaboons = 0x3F43, // R1.800, x0 (spawn during fight)
    DevilishThrall = 0x3F46, // R2.000, x0 (spawn during fight)
    HauntingThrall = 0x3F47, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 34526, // Boss->player, no cast, single-target

    StormcloudSummons = 33751, // Boss->self, 3.0s cast, single-target

    SmokeaterFirst = 33752, // Boss->self, 2.5s cast, single-target
    SmokeaterRest = 33753, // Boss->self, no cast, single-target
    SmokeaterAbsorb = 33754, // Raiun->Boss, no cast, single-target

    OnceOnRokujo = 33755, // Boss->self, 7.5s cast, single-target
    OnceOnRokujoAOE = 33757, // Helper->self, 8.0s cast, range 60 width 14 rect

    LeapingLevin1 = 33758, // Raiun->self, 1.0s cast, range 8 circle
    LeapingLevin2 = 33759, // Raiun->self, 1.5s cast, range 12 circle
    LeapingLevin3 = 33760, // Raiun->self, 2.5s cast, range 23 circle

    ThunderOnefold1 = 33761, // Boss->self, 3.0s cast, single-target
    ThunderOnefold2 = 33762, // Helper->location, 4.0s cast, range 6 circle

    CloudToCloud1 = 33763, // Raiun->self, 2.5s cast, range 100 width 2 rect
    CloudToCloud2 = 33764, // Raiun->self, 3.0s cast, range 100 width 6 rect
    CloudToCloud3 = 33765, // Raiun->self, 4.0s cast, range 100 width 12 rect
    NoblePursuit = 33766, // Boss->location, 8.0s cast, width 12 rect charge
    Levinburst = 33767, // Rairin->self, no cast, range 10 width 40 rect // Ring burst on Noble Pursuit, no cast time need to be tied to something else

    Enkyo = 33781, // Boss->self, 5.0s cast, range 60 circle //raidwide
    SplittingCry = 33782, // Boss->self/player, 5.0s cast, range 60 width 14 rect
    UnknownWeaponskill1 = 33783, // Boss->location, no cast, single-target

    TwiceOnRokujo1 = 34723, // Boss->self, 7.5s cast, single-target
    TwiceOnRokujo2 = 34724, // Boss->self, no cast, single-target
    TwiceOnRokujo3 = 34725, // Helper->self, 8.0s cast, range 60 width 14 rect

    ThriceOnRokujo1 = 34726, // Boss->self, 7.5s cast, single-target
    ThriceOnRokujo2 = 34731, // Boss->self, no cast, single-target
    ThriceOnRokujo3 = 34732, // Helper->self, 8.0s cast, range 60 width 14 rect

    ThunderTwofold1 = 34733, // Boss->self, 3.0s cast, single-target
    ThunderTwofold2 = 34809, // Helper->location, 4.0s cast, range 6 circle

    ThunderThreefold1 = 34810, // Boss->self, 3.0s cast, single-target
    ThunderThreefold2 = 34811, // Helper->location, 4.0s cast, range 6 circle

    //Route 8
    HauntingCry = 33768, // Boss->self, 3.0s cast, single-target // Summons Ads
    ThunderVortex = 33780, // Boss->self, 5.0s cast, range 6-30 donut
    UnsagelySpin = 33773, // VenomousThrallSnakes->self, 4.0s cast, range 6 circle
    Rush = 33774, // FeralThrallTigers->location, 6.0s cast, width 8 rect charge
    Vasoconstrictor = 33775, // VenomousThrall->location, 3.0s cast, range 5 circle

    //Route 9
    FocusedTremor = 33769, // Helper->self, 5.0s cast, range 60 circle
    Yoki1 = 33770, // Boss->self, 3.0s cast, single-target
    Yoki2 = 33771, // Helper->self, 4.0s cast, range 6 circle
    YokiUzu = 33772, // Boss->self, 8.0s cast, range 23 circle

    //Route 10
    RightSwipe = 33776, // DevilishThrall->self, 8.0s cast, range 40 180-degree cone
    LeftSwipe = 33777, // DevilishThrall->self, 8.0s cast, range 40 180-degree cone

    //Route 11
    Reisho = 33778, // Helper->self, no cast, range 6 circle
    ReishoTether = 34603, // HauntingThrall->self, 4.0s cast, single-target
    ReishoAOE = 34604, // Helper->self, 4.0s cast, range 6 circle
}

public enum SID : uint
{
    Unknown = 2056, // none->3F46, extra=0xE1
    VulnerabilityUp = 1789, // 3F46/Raiun->player, extra=0x1/0x2/0x3
}

public enum IconID : uint
{
    TankbusterCleave = 471, // player
}

public enum TetherID : uint
{
    Tether1 = 1, // 3F47->player
}
