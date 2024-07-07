namespace BossMod.Dawntrail.Trial.T02Zoraal;

public enum OID : uint
{
    Boss = 0x42A9, // R2.500, x?
    Helper = 0x233C, // R0.500, x?, mixed types
    Fang1 = 0x42AA, // R1.000, x?
    Fang2 = 0x42B6, // R1.000, x?
    ShadowOfTural1 = 0x43A8, // R0.500, x?
    ShadowOfTural2 = 0x42AC, // R1.000, x?
    ShadowOfTural3 = 0x42AD, // R1.000, x?
    UnknownActor1 = 0x42B9, // R10.050, x0 (spawn during fight)
    UnknownActor2 = 0x19A, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    //Phase1
    AutoAttack = 6497, // Boss->player, no cast, single-target

    SoulOverflow1 = 37707, // Boss->self, 5.0s cast, range 100 circle
    SoulOverflow2 = 37744, // Boss->self, 7.0s cast, range 100 circle

    UnknownAbility = 39137, // Boss->location, no cast, single-target
    DoubleEdgedSwords1 = 37713, // Boss->self, 4.4+0.6s cast, single-target
    DoubleEdgedSwords2 = 37714, // Helper->self, 5.0s cast, range 30 180.000-degree cone

    PatricidalPique = 37715, // Boss->player, 5.0s cast, single-target
    CalamitysEdge = 37708, // Boss->self, 5.0s cast, range 100 circle
    Burst = 37709, // ShadowOfTural1->self, 8.0s cast, range 8 circle

    VorpalTrail1 = 37710, // Boss->self, 3.7+0.3s cast, single-target
    VorpalTrail2 = 38183, // Fang1->location, no cast, width 4 rect charge
    VorpalTrail3 = 38184, // Helper->location, 4.3s cast, width 4 rect charge
    VorpalTrail4 = 37711, // Fang1->location, 1.0s cast, width 4 rect charge
    VorpalTrail5 = 37712, // Helper->location, 2.3s cast, width 4 rect charge
}

public enum SID : uint
{
    //Phase1
    DamageUp = 2550, // Boss->Boss, extra=0x1/0x2/0x3
    Haunt1 = 1542, // none->ShadowOfTural2/ShadowOfTural3, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2/0x3/0x4
    Haunt2 = 1543, // none->42B3/42B0/42B1/42B2, extra=0x0
    DownForTheCount = 2408, // Boss->player, extra=0xEC7
    UnknownStatus = 3409, // Boss->Boss, extra=0x2BF2
    Bleeding1 = 3077, // none->player, extra=0x0
    Bleeding2 = 3078, // none->player, extra=0x0
}

public enum IconID : uint
{
    //Phase1
    Icon218 = 218, // player
}
