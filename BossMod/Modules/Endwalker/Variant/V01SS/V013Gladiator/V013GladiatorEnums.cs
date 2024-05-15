namespace BossMod.Endwalker.Variant.V01SS.V013Gladiator;

public enum OID : uint
{
    Boss = 0x399F, // R6.500, x1
    Helper = 0x233C, // R0.500, x18, 523 type
    Regret = 0x39A1, // R1.000, x0 (spawn during fight)
    WhirlwindSafe = 0x39A2, // R2.000, x0 (spawn during fight)
    WhirlwindBad = 0x3AEC, // R1.330, x0 (spawn during fight)
    AntiqueBoulder = 0x39A3, // R1.800, x0 (spawn during fight)
    HatefulVisage = 0x39A0, // R2.250, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 30265, // Boss->location, no cast, single-target

    BitingWindSafe = 30285, // Helper->self, 4.0s cast, range 6 circle
    BitingWindKnockup = 30286, // Helper->player, no cast, single-target
    BitingWindBad = 31222, // Helper->self, 4.0s cast, range 4 circle

    FlashOfSteel1 = 30284, // Boss->self, 5.0s cast, range 60 circle raidwide
    FlashOfSteel2 = 30294, // Boss->self, 5.0s cast, range 60 circle raidwide
    MightySmite = 30295, // Boss->player, 5.0s cast, single-target tankbuster

    WrathOfRuin = 30277, // Boss->self, 3.0s cast, single-target, visual
    RackAndRuin = 30278, // Regret->location, 4.0s cast, range 40 width 5 rect

    RingOfMightVisual = 30655, // Helper->self, 10.0s cast, range 18 circle
    RingOfMight1Out = 30271, // Boss->self, 10.0s cast, range 8 circle
    RingOfMight2Out = 30272, // Boss->self, 10.0s cast, range 13 circle
    RingOfMight3Out = 30273, // Boss->self, 10.0s cast, range 18 circle
    RingOfMight1In = 30274, // Helper->self, 12.0s cast, range 8-30 donut
    RingOfMight2In = 30275, // Helper->self, 12.0s cast, range 13-30 donut
    RingOfMight3In = 30276, // Helper->self, 12.0s cast, range 18-30 donut

    FlashOfSteelMeteor = 30287, // Boss->self, 5.0s cast, range 60 circle
    Landing = 30288, // AntiqueBoulder->self, 7.0s cast, range 50 circle

    RushOfMight1 = 30266, // Boss->location, 10.0s cast, range 25 width 3 rect aoe
    RushOfMight2 = 30267, // Boss->location, 10.0s cast, range 25 width 3 rect aoe
    RushOfMight3 = 30268, // Boss->location, 10.0s cast, range 25 width 3 rect aoe
    RushOfMightFront = 30269, // Helper->self, 10.5s cast, range 60 180-degree cone
    RushOfMightBack = 30270, // Helper->self, 12.5s cast, range 60 180-degree cone

    SculptorsPassion = 30282, // Boss->self, 5.0s cast, range 60 width 8 rect shared ///
    ShatteringSteel = 30283, // Boss->self, 12.0s cast, range 60 circle raidwide

    SunderedRemainsVisual = 30280, // Boss->self, 3.0s cast, single-target
    SunderedRemains = 30281, // Helper->self, 9.0s cast, range 10 circle aoe

    //11
    HatefulVisage = 30289, // Boss->self, 3.0s cast, single-target
    GoldenFlame = 30290, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect

    //12
    SilverFlameFirst1 = 30292, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect
    SilverFlameFirst2 = 30291, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect
    SilverFlameRest = 30293, // HatefulVisage->self, no cast, range 60 width 10 rect
}

public enum SID : uint
{
    UnknownStatus = 2056, // none->Boss/Whirlwind1, extra=0x1D8/0x1D9/0x1F2/0x1D7
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
    Liftoff = 3262, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
}
