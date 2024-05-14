namespace BossMod.Stormblood.Trial.T08Suzaku;

public enum OID : uint
{
    Boss = 0x2459, // R2.800-6.930, x1
    Helper = 0x233C, // R0.500, x8, 523 type

    Actor1ea9ff = 0x1EA9FF, // R0.500, x0 (spawn during fight), EventObj type
    RapturousEcho = 0x245C, // R1.500, x0 (spawn during fight)
    ScarletLady = 0x245A, // R1.120, x4
    ScarletPlume = 0x245B, // R1.500, x0 (spawn during fight)
    SongOfDurance = 0x2463, // R1.500, x0 (spawn during fight)
    SongOfFire = 0x2460, // R1.500, x0 (spawn during fight)
    SongOfOblivion = 0x2462, // R1.500, x0 (spawn during fight)
    SongOfSorrow = 0x2461, // R1.500, x0 (spawn during fight)
    Suzaku1 = 0x245F, // R2.100, x4
    Suzaku2 = 0x2570, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    ScarletLadyAuto = 14065, // ScarletLady->player, no cast, single-target
    AutoAttack2 = 12863, // Boss->player, no cast, single-target

    AshesToAshes = 12831, // ScarletLady->self, 3.0s cast, range 40 circle
    Burn = 12861, // Helper->self, no cast, range 4 circle
    Cremate = 12832, // Boss->player, 3.0s cast, single-target
    EternalFlame = 12834, // Boss->self, 3.0s cast, range 80 circle
    FleetingSummer = 12835, // Boss->self, 3.0s cast, range 40 90-degree cone
    Hotspot = 12856, // Helper->self, 0.9s cast, range 21 ?-degree cone
    IncandescentInterlude = 12860, // Boss->self, 4.0s cast, single-target

    PhantomFlurry1 = 12849, // Boss->self, 4.0s cast, single-target
    PhantomFlurry2 = 12850, // Helper->players, no cast, single-target
    PhantomFlurry3 = 12851, // Helper->self, 6.0s cast, range 41 180-degree cone

    PhoenixDown = 12836, // Boss->self, 3.0s cast, single-target
    Rekindle = 12853, // Helper->player, no cast, range 6 circle
    RuthlessRefrain = 12848, // Boss->self, 4.0s cast, range 41 circle
    ScarletFever = 12844, // Helper->self, 7.0s cast, range 41 circle

    ScarletHymn1 = 12840, // RapturousEcho->player, no cast, single-target
    ScarletHymn2 = 12855, // Boss->self, no cast, single-target

    ScreamsOfTheDamned = 12833, // Boss->self, 3.0s cast, range 40 circle
    SouthronStar = 12852, // Boss->self, 4.0s cast, range 41 circle
    Swoop = 12859, // Suzaku1->self, 3.0s cast, range 55 width 6 rect
    UnknownSpell = 12846, // Boss->location, no cast, single-target

    UnknownWeaponskill1 = 12838, // Boss->self, no cast, single-target
    UnknownWeaponskill2 = 12841, // RapturousEcho->Suzaku2, no cast, single-target
    UnknownWeaponskill3 = 12857, // Boss->self, no cast, single-target
    UnknownWeaponskill4 = 12858, // Boss->self, no cast, single-target
    UnknownWeaponskill5 = 13445, // Boss->self, no cast, single-target
    UnknownWeaponskill6 = 13446, // Boss->self, no cast, single-target

    WellOfFlame = 12854, // Boss->self, 4.0s cast, range 41 width 20 rect
    WingAndAPrayer = 12837, // ScarletPlume->self, 15.0s cast, range 9 circle
}

public enum SID : uint
{
    PrimaryTarget = 1689, // ScarletLady->player, extra=0x0
    Burns = 530, // ScarletLady->player, extra=0x1
    HPBoost = 586, // none->ScarletLady, extra=0x1/0x2
    VulnerabilityUp = 202, // Boss/Helper->player, extra=0x1
    Suppuration = 375, // ScarletLady->player, extra=0x1
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    LovesTrueForm = 1630, // Boss->Boss, extra=0xC6
    DamageUp = 505, // RapturousEcho->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
    Stun = 149, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Icon_381 = 381, // player
    Icon_139 = 139, // player
}

public enum TetherID : uint
{
    Tether_79 = 79, // Suzaku1->Boss
}