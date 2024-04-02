namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

public enum OID : uint
{
    Boss = 0x30A8, // R4.600, x1
    Marchosias = 0x30A9, // R2.500, spawn during fight
    CrownedMarchosias = 0x30AA, // R2.500, spawn during fight
    Helper = 0x233C, // R0.500, x20
};

public enum AID : uint
{
    AutoAttack = 6497, // Boss/CrownedMarchosias->player, no cast, single-target
    ReverberatingRoar = 22381, // Boss->self, no cast, single-target, visual (falling rocks)
    FallingRock = 22382, // Helper->location, 3.0s cast, range 4 circle puddle
    HotCharge = 22387, // Boss->location, 3.0s cast, width 8 rect charge
    Firebreathe = 22388, // Boss->self, 5.0s cast, range 60 90-degree cone
    AddAppear = 22378, // Marchosias/CrownedMarchosias->self, no cast, single-target, visual
    HeadDown = 22376, // Marchosias->location, 3.0s cast, width 4 rect charge
    SpitFlame = 22390, // Boss->self, 8.0s cast, single-target, visual (icons)
    SpitFlameAOE = 22391, // Boss->players, no cast, range 4 circle
    LeftSidedShockwaveFirst = 22383, // Boss->self, 3.0s cast, range 15 180-degree cone
    RightSidedShockwaveFirst = 22384, // Boss->self, 3.0s cast, range 15 180-degree cone
    LeftSidedShockwaveSecond = 22385, // Boss->self, 1.0s cast, range 15 180-degree cone
    RightSidedShockwaveSecond = 22386, // Boss->self, 1.0s cast, range 15 180-degree cone
    FeralHowl = 22375, // Boss->self, 5.0s cast, single-target, visual (knockback)
    FeralHowlAOE = 23349, // Helper->self, no cast, range 50 circle, raidwide knockback 30
    HuntersClaw = 22377, // Marchosias->self, 8.5s cast, range 8 circle puddle
    FirebreatheRotating = 22379, // Boss->self, 5.0s cast, single-target, visual (rotating cone)
    FirebreatheRotatingAOE = 22380, // Boss->self, 0.5s cast, range 60 90-degree cone
    HystericAssault = 22392, // Boss->self, 5.0s cast, single-target, visual (knockback)
    HystericAssaultAOE = 23363, // Helper->self, no cast, range 50 circle, raidwide knockback 30
    Burn = 21603, // Helper->players, no cast, range 45 circle with ? falloff
};

public enum IconID : uint
{
    SpitFlame1 = 79, // player
    SpitFlame2 = 80, // player
    SpitFlame3 = 81, // player
    SpitFlame4 = 82, // player
    FirebreatheCW = 167, // Boss
    FirebreatheCCW = 168, // Boss
    Burn = 87, // player
};
