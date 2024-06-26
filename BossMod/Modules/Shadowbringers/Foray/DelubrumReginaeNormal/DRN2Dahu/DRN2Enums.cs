namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN2Dahu;

public enum OID : uint
{
    Dahu = 0x233C, // R0.500, x?, 523 type
    Boss = 0x30A6, // R4.600, x?
    Marchosias = 0x30A7, // R2.500, x?
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss/CrownedMarchosias->player, no cast, single-target
    ReverberatingRoar = 22363, // Boss->self, no cast, single-target, visual (falling rocks)
    FallingRock = 22364, // Helper->location, 3.0s cast, range 4 circle puddle
    HotCharge = 22372, // Boss->location, 3.0s cast, width 8 rect charge
    Firebreathe = 22373, // Boss->self, 5.0s cast, range 60 90-degree cone
    AddAppear = 22360, // Marchosias/CrownedMarchosias->self, no cast, single-target, visual
    HeadDown = 22358, // Marchosias->location, 3.0s cast, width 4 rect charge
    LeftSidedShockwaveFirst = 22368, // Boss->self, 3.0s cast, range 15 180-degree cone
    RightSidedShockwaveFirst = 22369, // Boss->self, 3.0s cast, range 15 180-degree cone
    LeftSidedShockwaveSecond = 22370, // Boss->self, 1.0s cast, range 15 180-degree cone
    RightSidedShockwaveSecond = 22371, // Boss->self, 1.0s cast, range 15 180-degree cone
    FeralHowl = 22357, // Boss->self, 5.0s cast, single-target, visual (knockback)
    HuntersClaw = 22359, // Marchosias->self, 8.5s cast, range 8 circle puddle
    FirebreatheRotating = 22361, // Boss->self, 5.0s cast, single-target, visual (rotating cone)
    FirebreatheRotatingAOE = 22362, // Boss->self, 0.5s cast, range 60 90-degree cone
}

public enum SID : uint
{
    Staggered = 715, // Boss->player, extra=0xECA
    TwiceComeRuin = 2485, // Boss->player, extra=0x1
}

public enum IconID : uint
{
    FirebreatheCW = 167, // Boss
    FirebreatheCCW = 168, // Boss
}
