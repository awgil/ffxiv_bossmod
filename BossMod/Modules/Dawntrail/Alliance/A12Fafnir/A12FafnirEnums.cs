namespace BossMod.Dawntrail.Alliance.A12Fafnir;

public enum OID : uint
{
    Boss = 0x41E5, // R17.000, x1
    Helper = 0x233C, // R0.500, x43, Helper type
    DragonBreath = 0x1EBCCB, // R0.500, x4, EventObj type
    RavagingWind = 0x41E7, // R3.000, x3
    BitingWind = 0x41E8, // R1.000, x6
    Darter = 0x41E9, // R2.250, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttackBoss = 40860, // Boss->player, no cast, single-target
    ShudderingEarth = 40857, // Boss->location, no cast, single-target, teleport?
    DarkMatterBlast = 40854, // Boss->self, 5.0s cast, range 70 circle, raidwide
    OffensivePostureSpikeFlail = 40811, // Boss->self, 8.0+1.0s cast, single-target, visual (back cleave)
    SpikeFlail = 41114, // Helper->self, 9.0s cast, range 80 270-degree cone
    OffensivePostureDragonBreath = 40814, // Boss->self, 8.0+1.1s cast, single-target, visual (donut with lingering fires)
    DragonBreath = 40815, // Helper->self, no cast, range ?-30 donut
    OffensivePostureTouchdown = 40816, // Boss->self, 8.0+1.2s cast, single-target, visual (out)
    Touchdown = 41116, // Helper->self, 9.2s cast, range 24 circle
    BalefulBreath = 39922, // Boss->self, 8.0s cast, single-target, visual (4-hit line stack)
    BalefulBreathAOEFirst = 40852, // Boss->self, no cast, range 70 width 6 rect, first hit
    BalefulBreathAOERest = 40853, // Boss->self, no cast, range 70 width 6 rect, hits 2-4
    BalefulBreathEnd = 39798, // Boss->self, no cast, single-target, visual (mechanic end)
    SharpSpike = 40855, // Boss->self, 5.0+1.0s cast, single-target, visual (tankbusters)
    SharpSpikeAOE = 40856, // Helper->player, no cast, range 4 circle tankbuster

    AutoAttackAdd = 40633, // Darter->player, no cast, single-target
    PestilentSphere = 40859, // Darter->player, 5.0s cast, single-target, single-target damage
    Venom = 40858, // Darter->self, 4.0s cast, range 30 120-degree cone
    HurricaneWingRaidwide = 40817, // Boss->self, 3.0+2.5s cast, single-target, visual (multi-hit raidwide)
    HurricaneWingRaidwideAOE1 = 40818, // Helper->self, no cast, range 70 circle
    HurricaneWingRaidwideAOE2 = 40819, // Helper->self, no cast, range 70 circle
    HurricaneWingRaidwideAOE3 = 40820, // Helper->self, no cast, range 70 circle
    HurricaneWingRaidwideAOE4 = 40821, // Helper->self, no cast, range 70 circle
    HurricaneWingRaidwideAOE5 = 40822, // Helper->self, no cast, range 70 circle
    HurricaneWingRaidwideAOE6 = 41302, // Helper->self, no cast, range 70 circle
    HurricaneWingRaidwideAOE7 = 41303, // Helper->self, no cast, range 70 circle
    HurricaneWingRaidwideAOE8 = 41304, // Helper->self, no cast, range 70 circle
    HurricaneWingRaidwideAOE9 = 41305, // Helper->self, no cast, range 70 circle
    HurricaneWingLongShrinking1 = 40824, // Helper->self, 5.0s cast, range 23-30 donut
    HurricaneWingLongShrinking2 = 40825, // Helper->self, 7.0s cast, range 16-23 donut
    HurricaneWingLongShrinking3 = 40826, // Helper->self, 9.0s cast, range 9-16 donut
    HurricaneWingLongShrinking4 = 40827, // Helper->self, 11.0s cast, range 9 circle
    HurricaneWingLongExpanding1 = 40829, // Helper->self, 5.0s cast, range 9 circle
    HurricaneWingLongExpanding2 = 40830, // Helper->self, 7.0s cast, range 9-16 donut
    HurricaneWingLongExpanding3 = 40831, // Helper->self, 9.0s cast, range 16-23 donut
    HurricaneWingLongExpanding4 = 40832, // Helper->self, 11.0s cast, range 23-30 donut
    HurricaneWingShortShrinking1 = 40834, // Helper->self, 4.0s cast, range 23-30 donut
    HurricaneWingShortShrinking2 = 40835, // Helper->self, 6.0s cast, range 16-23 donut
    HurricaneWingShortShrinking3 = 40836, // Helper->self, 8.0s cast, range 9-16 donut
    HurricaneWingShortShrinking4 = 40837, // Helper->self, 10.0s cast, range 9 circle
    HurricaneWingShortExpanding1 = 40839, // Helper->self, 4.0s cast, range 9 circle
    HurricaneWingShortExpanding2 = 40840, // Helper->self, 6.0s cast, range 9-16 donut
    HurricaneWingShortExpanding3 = 40841, // Helper->self, 8.0s cast, range 16-23 donut
    HurricaneWingShortExpanding4 = 40842, // Helper->self, 10.0s cast, range 23-30 donut
    GreatWhirlwindLarge = 39873, // RavagingWind->self, 4.5s cast, range 10 circle
    GreatWhirlwindSmall = 39874, // BitingWind->self, 4.5s cast, range 3 circle
    GreatWhirlwindLargeAOE = 40843, // Helper->self, no cast, range 9 circle
    GreatWhirlwindSmallAOE = 40844, // Helper->self, no cast, range 3 circle
    HorridRoar = 40849, // Boss->self, 3.0s cast, single-target, visual (puddles + spread)
    HorridRoarPuddle = 40850, // Helper->location, 4.0s cast, range 4 circle puddle
    HorridRoarSpread = 40851, // Helper->player, 5.0s cast, range 8 circle spread
    BalefulNail1 = 40812, // Boss->location, no cast, single-target, visual (teleport?)
    BalefulNail2 = 40813, // Boss->location, no cast, single-target, visual (teleport?)
    AbsoluteTerror = 40845, // Boss->self, 6.0+1.4s cast, single-target, visual (center)
    AbsoluteTerrorAOE = 40846, // Helper->self, 7.4s cast, range 70 width 20 rect
    WingedTerror = 40847, // Boss->self, 6.0+1.4s cast, single-target, visual (sides)
    WingedTerrorAOE = 40848, // Helper->self, 7.4s cast, range 70 width 25 rect
}

public enum IconID : uint
{
    BalefulBreath = 568, // Boss->player
    SharpSpike = 342, // player->self
    HorridRoar = 499, // player->self
}
