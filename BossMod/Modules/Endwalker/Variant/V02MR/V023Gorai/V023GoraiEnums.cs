namespace BossMod.Endwalker.Variant.V02MR.V023Gorai;

public enum OID : uint
{
    Boss = 0x3F5C, // R3.450, x1
    Helper = 0x233C, // R0.500, x13, 523 type
    FlameAndSulphurFlame = 0x1EB893, // R0.500, EventObj type, spawn during fight
    FlameAndSulphurRock = 0x1EB894, // R0.500, EventObj type, spawn during fight
    ShishuWhiteBaboon = 0x3F5D, // R1.020, x0 (spawn during fight)
    BallOfLevin = 0x3F60, // R0.690-2.300, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 34004, // Boss->player, no cast, single-target

    FlickeringFlame = 34005, // Boss->self, 3.0s cast, single-target // summons flames
    SulphuricStone = 34006, // Boss->self, 3.0s cast, single-target // summons stones
    FlameAndSulphur = 34007, // Boss->self, 3.0s cast, single-target // summons both

    BrazenBalladExpanding = 34009, // Boss->self, 5.0s cast, single-target // modifies stones and flames, moves flame rect out, turns stones to donut aoe
    FallingRockExpand = 34014, // Helper->self, no cast, range ?-16 donut
    FireSpreadExpand = 34011, // Helper->self, no cast, range 46 width 5 rect

    BrazenBalladSplitting = 34008, // Boss->self, 5.0s cast, single-target // mofifies stones and flames, extends flame rect width, turns stones into large aoe
    FireSpreadSplit = 34010, // Helper->self, no cast, range 46 width 10 rect
    FallingRockSplit = 34013, // Helper->self, no cast, range 11 circle

    Unenlightenment = 34048, // Boss->self, 5.0s cast, single-target
    UnenlightenmentAOE = 34049, // Helper->self, no cast, range 60 circle

    Burst = 34018, // Helper->self, no cast, range 4 circle
    DramaticBurst = 34019, // Helper->self, no cast, range 60 circle // raidwide

    ImpurePurgation = 34022, // Boss->self, 3.6s cast, single-target
    ImpurePurgationFirst = 34023, // Helper->self, 4.0s cast, range 60 45-degree cone
    ImpurePurgationSecond = 34024, // Helper->self, 6.0s cast, range 60 45-degree cone

    RatTower = 34003, // Boss->location, no cast, single-target
    MalformedPrayer = 34017, // Boss->self, 4.0s cast, single-target //Sequentially summons three orange towers
    RousingReincarnation1 = 34015, // Boss->self, 5.0s cast, single-target
    RousingReincarnation2 = 34016, // Helper->player, no cast, single-target

    SpikeOfFlameVisual = 34020, // Boss->self, 2.6s cast, single-target //Baited circle AoEs on each player
    SpikeOfFlameAOE = 34021, // Helper->location, 3.0s cast, range 5 circle

    StringSnapVisual = 34025, // Boss->self, 2.6s cast, single-target
    StringSnapDonut1 = 34026, // Helper->self, 3.0s cast, range 10 circle
    StringSnapDonut2 = 34027, // Helper->self, 5.0s cast, range ?-20 donut
    StringSnapDonut3 = 34028, // Helper->self, 7.0s cast, range ?-30 donut

    TorchingTormentVisual = 34046, // Boss->player, 5.0s cast, single-target //AoE tankbuster
    TorchingTormentAOE = 34047, // Helper->player, no cast, range 6 circle

    //Route 5
    PureShockVisual = 34029, // Boss->self, 2.5s cast, single-target
    PureShock = 34030, // Helper->self, 3.0s cast, range 40 circle
    SelfDestruct = 34033, // ShishuWhiteBaboon->self, 30.0s cast, range 10 circle
    UnknownSpell = 34031, // Helper->player, no cast, single-target
    WilyWall = 34032, // ShishuWhiteBaboon->self, 7.0s cast, single-target

    //Route 6
    HumbleHammerVisual = 34038, // Boss->self, 5.0s cast, single-target
    HumbleHammerAOE = 34039, // Helper->location, 3.0s cast, range 3 circle
    ShockAOE = 34012, // BallOfLevin->self, 6.0s cast, range 8 circle
    ShockVisual2 = 34035, // BallOfLevin->self, 17.0s cast, range 8 circle
    ShockSmall = 34036, // BallOfLevin->self, no cast, range 8 circle
    ShockLarge = 34037, // BallOfLevin->self, no cast, range 8 circle
    Thundercall = 34034, // Boss->self, 3.0s cast, single-target

    //Route 7
    WorldlyPursuitFirst = 34042, // Boss->self, 5.0s cast, range 60 width 20 cross
    WorldlyPursuitRest = 34044, // Boss->self, 1.5s cast, range 60 width 20 cross
    FightingSpiritsVisual = 34040, // Boss->self, 5.0s cast, single-target
    FightingSpiritsRaidwide = 34041, // Helper->self, 6.2s cast, range 30 circle
    BiwaBreakerFirst = 34045, // Boss->self, 4.0s cast, range 30 circle
    BiwaBreakerRest = 34513, // Boss->self, no cast, range 30 circle
}

public enum SID : uint
{
    DrunkWithPower = 3595, // Boss->Boss, extra=0x0
    RodentialRebirth1 = 3597, // Helper->player, extra=0x0
    RodentialRebirth2 = 3598, // Helper->player, extra=0x0
    RodentialRebirth3 = 3599, // Helper->player, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
    Transfiguration = 1608, // none->player, extra=0x1F7
    Stun = 2656, // Helper->player, extra=0x0
    AreaOfInfluenceUp = 618, // none->BallOfLevin, extra=0xA
    Unknown = 2970, // Helper->BallOfLevin, extra=0x261

}

public enum IconID : uint
{
    Icon168 = 168, // Boss
    Icon344 = 344, // player
}

public enum TetherID : uint
{
    Tether248 = 248, // player->Boss
    Tether80 = 80, // ShishuWhiteBaboon->player
}
