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
    WildfiresFury = 23865, // Damage
    HarnessFire = 23864, // Boss->self, 3.0s cast, single-target
    HeartOfNature = 23844, // Boss->self, 3.0s cast, range 80 circle
    CagedHeartOfNature = 23837, // Boss->self, 3.0s cast, range 10 circle
    NaturesPulse1 = 23845, // Helper->self, 4.0s cast, range 10 circle
    NaturesPulse2 = 23846, // Helper->self, 5.5s cast, range 10-20 donut
    NaturesPulse3 = 23847, // Helper->self, 7.0s cast, range 20-30 donut
    TasteOfBlood = 23843, // Boss->self, 4.0s cast, range 40 180-degree cone
    SoulAflame = 23852,
    FlamesMeet1 = 23853, // VermillionFlame->self, 6.5s cast; makes the orb light up
    FlamesMeet2 = 23854, // VermillionFlame->self, 11s cast; actual AOE
    HeavenAndEarthCW = 23855,
    HeavenAndEarthCCW = 24554,
    HeavenAndEarthRotate = 23856, // Unused by module
    HeavenAndEarthStart = 23857,
    HeavenAndEarthMove = 23858,
    MoveMountains1 = 23859, // Boss->self, 5s cast, first attack
    MoveMountains2 = 23860, // Boss->self, no cast, second attack
    MoveMountains3 = 23861, // Helper->self, 5s cast, first line
    MoveMountains4 = 23862, // Helper->self, no cast, second line
    WindsPeak1 = 23850, // Boss->self, 3.0s cast, range 5 circle
    WindsPeak2 = 23851, // Helper->self, 4.0s cast, range 50 circle
    NaturesBlood1 = 23848, // Helper->self, 7.5s cast, range 4 circle
    NaturesBlood2 = 23849, // Helper->self, no cast, range 4 circle
    SplittingRage = 23863, // Boss->self, 3.0s cast, range 50 circle
    DuelOrDie = 23836,
    WildfireCrucible = 23836, //enrage, 25s cast time
}

public enum SID : uint
{
    OnFire = 2547, // Boss->Boss
    TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
    DuelOrDie = 2545, // Boss/Helper->player
}
