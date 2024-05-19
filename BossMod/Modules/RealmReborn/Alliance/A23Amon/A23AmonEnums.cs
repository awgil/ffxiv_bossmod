namespace BossMod.RealmReborn.Alliance.A23Amon;

public enum OID : uint
{
    Boss = 0xBE0, // R4.000, x?
    AmonHelper = 0x1B2, // R0.500, x?
    KumKum = 0xBE7, // R1.000-2.000, x?
    IceCage = 0xBE1, // R1.800, x?
    Kichiknebik = 0xBE6, // R1.200-2.400, x?
    DimensionalCompression = 0xBE2, // R1.000, x?
    ExperimentalByProduct66 = 0xBE4, // R1.000, x?
    ExperimentalByProduct33 = 0xBE3, // R0.500-1.000, x?
}

public enum AID : uint
{
    AutoAttack1k = 872, // Boss->player, no cast, single-target
    Coloratura = 2379, // Boss->self, no cast, range 8+R ?-degree cone
    BlizzagaForte = 2382, // Boss->self, 3.0s cast, range 6+R circle
    Darkness = 1875, // KumKum->self, 1.0s cast, range 6+R ?-degree cone
    FiragaForte = 2380, // Boss->self, no cast, range 60 circle
    Mini = 2371, // DimensionalCompression->player/ExperimentalByProduct33/KumKum/Kichiknebik, no cast, single-target
    RanineComedy = 2386, // Boss->self, no cast, ???
    TheLastSong = 2374, // ExperimentalByProduct33->self, no cast, range 60 circle
    MidwinterTragedy = 2385, // Boss->self, no cast, ???
    AutoAttack2 = 870, // Kichiknebik->player, no cast, single-target
    CurtainCall = 2441, // Boss->self, 15.0s cast, ???
    TheLastBout = 2372, // ExperimentalByProduct66->self, no cast, range 60 circle
    ThundagaForte1 = 2384, // AmonHelper->location, 3.5s cast, range 6 circle
    ThundagaForte2 = 2383, // Boss->location, 3.5s cast, range 6 circle
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    Heavy = 14, // Boss->player, extra=0x32
    Minimum = 438, // DimensionalCompression->player/ExperimentalByProduct33/KumKum/Kichiknebik, extra=0x32
    FireToad = 511, // Boss->player, extra=0x2
    Silence = 7, // ExperimentalByProduct33->player, extra=0x0
    DeepFreeze = 487, // Boss->player, extra=0x1
    Pacification = 6, // ExperimentalByProduct66->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon27 = 27, // player
    Icon3 = 3, // player
}

public enum TetherID : uint
{
    Tether1 = 1, // DimensionalCompression->player
}