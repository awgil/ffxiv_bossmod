namespace BossMod.RealmReborn.Extreme.Ex2Garuda
{
    public enum OID : uint
    {
        Boss = 0xF5, // R1.700, x1
        Monolith = 0xF3, // R2.300, x4
        EyeOfTheStorm = 0x626, // R0.500, x1
        Helper = 0x1B2, // R0.500, x18
        RazorPlume = 0xF4, // R0.500, spawn during fight
        SpinyPlume = 0x600, // R0.500, spawn during fight
        SatinPlume = 0x601, // R0.500, spawn during fight
        Chirada = 0x620, // R1.360, spawn during fight
        Suparna = 0x621, // R1.360, spawn during fight
        Whirlwind = 0x625, // R0.500, spawn during fight
        Monolith1 = 0x1E8706, // R2.000, x1, EventObj type
        Monolith2 = 0x1E8707, // R2.000, x1, EventObj type
        Monolith3 = 0x1E8708, // R2.000, x1, EventObj type
        Monolith4 = 0x1E8709, // R2.000, x1, EventObj type
        SpinyShield = 0x1E8F68, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss/Chirada/Suparna->player, no cast, single-target
        Downburst1 = 1380, // Boss/Suparna->self, no cast, range 10+R ?-degree cone cleave
        Downburst2 = 1553, // Chirada->self, no cast, range 10+R ?-degree cone cleave
        WickedWheel = 1552, // Boss/Suparna->self, no cast, range 7+R circle cleave
        Slipstream = 1382, // Boss/Chirada/Suparna->self, 2.5s cast, range 10+R 90-degree cone aoe
        FrictionBoss = 1548, // Boss->location, no cast, range 5 circle (at random target?)
        FrictionAdds = 1549, // Chirada/Suparna->location, 3.0s cast, range 5 circle aoe
        EyeOfTheStorm = 1671, // EyeOfTheStorm->self, 2.0s cast, range 12-25 donut aoe
        FeatherRain = 1550, // Helper->location, 1.0s cast, range 3 circle aoe
        MistralSong = 1761, // Boss->self, 1.0s cast, range 30+R ?-degree cone LOSable aoe
        AerialBlast = 1554, // Boss->self, 4.0s cast, raidwide
        MistralShriek = 1384, // Boss->self, 3.0s cast, raidwide (should be mitigated by standing in spiny shield)
        GreatWhirlwind = 1777, // Whirlwind->location, 3.0s cast, range 8 circle, knockback 15

        Featherlance = 1763, // RazorPlume->self, no cast, range 8 circle, suicide attack if not killed in ~30s
        ThermalTumult = 1551, // SatinPlume->self, no cast, raidwide sleep, happens ~25s after spawn (and repeats every ~25s after that)
        Gigastorm = 1555, // SpinyPlume->self, 3.0s cast, range 6+R circle aoe on death; after cast end shield spawns and stays active for ~10s
        Cyclone = 1556, // SpinyPlume->player, no cast, single-target, applies thermal low
        SuperCyclone = 1557, // SpinyPlume->location, no cast, deadly raidwide if thermal low reaches 3 stacks
    };

    public enum SID : uint
    {
        ThermalLow = 379, // SpinyPlume->player, extra=0x1/0x2/0x3
    };
}
