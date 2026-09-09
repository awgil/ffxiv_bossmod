namespace BossMod.Dawntrail.Quest.TheProtectorAndTheDestroyer;

public enum OID : uint
{
    Boss = 0x4342,
    Helper = 0x233C,
    BossP2 = 0x4349,
    BallOfLevin = 0x434A, // R1.500, x0 (spawn during fight)
    SuperchargedLevin = 0x39C4, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    ThrownFlames = 38205, // 4345->self, 6.0s cast, range 8 circle
    BastionBreaker = 38198, // Helper->players/4144/4339, 6.0s cast, range 6 circle
    SearingSlash = 38197, // Boss->self, 6.0s cast, range 8 circle
    StormlitShockwave = 38202, // Boss->self, 5.0s cast, range 40 circle
    Electrobeam = 38207, // 4346->self, 6.0s cast, range 40 width 4 rect
    HolyBlade = 38199, // Helper->4339, 6.0s cast, range 6 circle
    SteadfastWill = 38201, // Boss->4144, 5.0s cast, single-target
    Rush = 38209, // 4347->location, 5.0s cast, width 5 rect charge
    ValorousAscension = 38203, // Boss->self, 8.0s cast, range 40 circle
    RendPower = 38200, // Helper->self, 4.5s cast, range 40 30-degree cone

    CracklingHowl = 38211, // BossP2->self, 4.3+0.7s cast, single-target
    VioletVoltageVisual = 38220, // Helper->self, 2.5s cast, range 20 180-degree cone
    VioletVoltage3X = 38214, // BossP2->self, 8.3+0.7s cast, single-target
    VioletVoltage4X = 38215, // BossP2->self, 10.3+0.7s cast, single-target
    VioletVoltageAOE = 38221, // Helper->self, no cast, range 20 ?-degree cone
    RollingThunder = 38224, // Helper->self, 5.0s cast, range 20 ?-degree cone
    UntamedCurrent1 = 38233, // 3A5E->location, 3.1s cast, range 5 circle
    UntamedCurrent2 = 19718, // 3A5E->location, 3.2s cast, range 5 circle
    UntamedCurrent3 = 19719, // 3A5E->location, 3.3s cast, range 5 circle
    UntamedCurrent4 = 19999, // 3A5E->location, 3.0s cast, range 5 circle
    UntamedCurrent5 = 38234, // 3A5E->location, 3.1s cast, range 5 circle
    UntamedCurrent6 = 19720, // 3A5E->location, 3.2s cast, range 5 circle
    UntamedCurrent7 = 19721, // 3A5E->location, 3.3s cast, range 5 circle
    UnnamedCurrent1 = 19179, // 3A5E->location, 3.1s cast, range 5 circle
    UnnamedCurrent2 = 19728, // 3A5E->location, 3.3s cast, range 5 circle
    UntamedCurrentSpread = 19181, // Helper->players/4146/4339, 5.0s cast, range 5 circle
}
