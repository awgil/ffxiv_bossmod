namespace BossMod.Stormblood.Quest.ARequiemForHeroes;

public enum OID : uint
{
    BossP1 = 0x268A,
    BossP2 = 0x268C,
    Helper = 0x233C,
    AmeNoHabakiri = 0x2692, // R3.000, x0 (spawn during fight)
    TheStorm = 0x2760, // R3.000, x0 (spawn during fight)
    TheSwell = 0x275F, // R3.000, x0 (spawn during fight)
    DarkAether = 0x2694, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    FloodOfDarkness = 14808, // Helper->self, 3.5s cast, range 6 circle
    VeinSplitter = 14839, // Boss->self, 4.0s cast, range 10 circle
    LightlessSpark = 14838, // Boss->self, 4.0s cast, range 40+R 90-degree cone
    LightlessSparkAdds = 14824, // 268D->self, 4.0s cast, range 40+R 90-degree cone
    ArtOfTheSwell = 14812, // Boss->self, 4.0s cast, range 33 circle
    TheSwellUnbound = 14813, // Helper->self, 8.0s cast, range 8-20 donut
    ArtOfTheSword1 = 14819, // Helper->self, 4.0s cast, range 40+R width 6 rect
    ArtOfTheSword2 = 14818, // Helper->self, 6.0s cast, range 40+R width 6 rect
    ArtOfTheSword3 = 14820, // Helper->self, 2.0s cast, range 40+R width 6 rect
    ArtOfTheStorm = 14814, // Boss->self, 4.0s cast, range 8 circle
    TheStormUnboundCast = 14815, // Helper->self, 3.0s cast, range 5 circle
    TheStormUnboundRepeat = 14816, // Helper->self, no cast, range 5 circle
    EntropicFlame = 14833, // Helper->self, 4.0s cast, range 50+R width 8 rect
}
