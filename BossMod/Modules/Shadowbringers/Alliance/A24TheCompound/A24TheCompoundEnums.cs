namespace BossMod.Shadowbringers.Alliance.A24TheCompound;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x24 (spawn during fight), 523 type
    Boss = 0x2EC4, // R5.290, x1
    ThePuppets1 = 0x2EC5, // R5.290, x0 (spawn during fight)
    ThePuppets2 = 0x2FA5, // R0.500, x0 (spawn during fight)
    ThePuppets3 = 0x2FA4, // R1.000, x0 (spawn during fight)
    Compound2P = 0x2EC6, // R6.000, x0 (spawn during fight)
}

public enum AID : uint
{
    BossAutoAttack = 21450, // Boss->player, no cast, single-target
    MechanicalLaceration1 = 20920, // Boss->self, 5.0s cast, range 100 circle
    MechanicalDissection = 20915, // Boss->self, 6.0s cast, range 85 width 11 rect
    MechanicalDecapitation = 20916, // Boss->self, 6.0s cast, range ?-43 donut
    MechanicalContusionVisual = 20917, // Boss->self, 3.0s cast, single-target
    MechanicalContusionGround = 20919, // Helper->location, 4.0s cast, range 6 circle
    MechanicalContusionSpread = 20918, // Helper->player, 5.0s cast, range 6 circle
    IncongruousSpin1 = 20913, // ThePuppets1->self, 8.0s cast, single-target
    IncongruousSpinAOE = 20914, // Helper->self, 8.5s cast, range 80 width 150 rect
    MechanicalLaceration2 = 21461, // Boss->self, no cast, range 100 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Boss->player, extra=0x1
    MagicVulnerabilityUp = 2091, // Helper->player, extra=0x0
    DownForTheCount = 2408, // none->player, extra=0xEC7

}

public enum IconID : uint
{
    Spreadmarker = 139, // player
}
