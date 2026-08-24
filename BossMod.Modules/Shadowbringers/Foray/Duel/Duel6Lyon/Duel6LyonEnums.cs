namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

public enum OID : uint
{
    Boss = 0x31C1,
    Helper = 0x233C,
    VermillionFlame = 0x2E8F,
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    WildfiresFury = 0x5D39, // Damage
    HarnessFire = 0x5D38, // Boss->self, 3.0s cast, single-target
    HeartOfNature = 0x5D24, // Boss->self, 3.0s cast, range 80 circle
    CagedHeartOfNature = 0x5D1D, // Boss->self, 3.0s cast, range 10 circle
    NaturesPulse1 = 0x5D25, // Helper->self, 4.0s cast, range 10 circle
    NaturesPulse2 = 0x5D26, // Helper->self, 5.5s cast, range 10-20 donut
    NaturesPulse3 = 0x5D27, // Helper->self, 7.0s cast, range 20-30 donut
    TasteOfBlood = 0x5D23, // Boss->self, 4.0s cast, range 40 180-degree cone
    SoulAflame = 0x5D2C,
    FlamesMeet1 = 0x5D2D, // VermillionFlame->self, 6.5s cast; makes the orb light up
    FlamesMeet2 = 0x5D2E, // VermillionFlame->self, 11s cast; actual AOE
    HeavenAndEarthCW = 0x5D2F,
    HeavenAndEarthCCW = 0x5FEA,
    HeavenAndEarthRotate = 0x5D30, // Unused by module
    HeavenAndEarthStart = 0x5D31,
    HeavenAndEarthMove = 0x5D32,
    MoveMountains1 = 0x5D33, // Boss->self, 5s cast, first attack
    MoveMountains2 = 0x5D34, // Boss->self, no cast, second attack
    MoveMountains3 = 0x5D35, // Helper->self, 5s cast, first line
    MoveMountains4 = 0x5D36, // Helper->self, no cast, second line
    WindsPeak1 = 0x5D2A, // Boss->self, 3.0s cast, range 5 circle
    WindsPeak2 = 0x5D2B, // Helper->self, 4.0s cast, range 50 circle
    NaturesBlood1 = 0x5D28, // Helper->self, 7.5s cast, range 4 circle
    NaturesBlood2 = 0x5D29, // Helper->self, no cast, range 4 circle
    SplittingRage = 0x5D37, // Boss->self, 3.0s cast, range 50 circle
    DuelOrDie = 0x5D1C,
    WildfireCrucible = 0x5D3B, //enrage, 25s cast time
}

public enum SID : uint
{
    OnFire = 0x9F3, // Boss->Boss
    TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
    DuelOrDie = 0x9F1, // Boss/Helper->player
}
