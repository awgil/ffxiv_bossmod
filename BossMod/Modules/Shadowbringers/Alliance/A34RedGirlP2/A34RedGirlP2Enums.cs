namespace BossMod.Shadowbringers.Alliance.A34RedGirlP2;

public enum OID : uint
{
    Boss = 0x32BD, // R12.250, x1
    Helper = 0x233C, // R0.500, x16, 523 type
    RedGirl1 = 0x32BE, // R12.250, x3
    RedGirl2 = 0x32BC, // R2.250, x0 (spawn during fight)
    RedGirlP1 = 0x32BB, // R7.500, x1
    Ally2B = 0x31A8, // R0.512, x1
    BlackPylon = 0x32E5, // R1.000, x0 (spawn during fight)
    WhiteLance = 0x32E3, // R1.000, x0 (spawn during fight)
    BlackLance = 0x32E4, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    BladeFlurry1 = 23788, // Ally2B->Boss, no cast, single-target
    BladeFlurry2 = 23789, // Ally2B->Boss, no cast, single-target
    DancingBlade = 23790, // Ally2B->Boss, no cast, width 2 rect charge
    BalancedEdge = 23791, // Ally2B->self, 2.0s cast, range 5 circle
    Cruelty1 = 24595, // Boss->self, 5.0s cast, single-target
    Cruelty2 = 24596, // Helper->location, no cast, range 75 circle
    BossAutoAttack = 24597, // Helper->player, no cast, single-target
    WhirlingAssault = 23792, // Ally2B->self, 2.0s cast, range 40 width 4 rect
    ChildsPlay1 = 24612, // Boss/RedGirl1->self, 10.0s cast, single-target
    Explosion = 24614, // BlackPylon->self, 15.0s cast, range 9 circle
    Shockwave = 24590, // Boss->self, 2.0s cast, single-target
    ShockBlack = 24593, // Helper->players, no cast, range 5 circle
    GenerateBarrier1 = 24583, // Helper->self, 4.0s cast, range 12 width 3 rect
    GenerateBarrier2 = 24582, // Helper->self, 4.0s cast, range 6 width 3 rect
    GenerateBarrier3 = 24581, // Boss->self, 4.0s cast, single-target
    GenerateBarrier4 = 25361, // Helper->self, no cast, range 12 width 3 rect
    GenerateBarrier5 = 25360, // Helper->self, no cast, range 6 width 3 rect
    PointWhite1 = 24607, // WhiteLance->self, no cast, range 50 width 6 rect
    PointBlack1 = 24610, // BlackLance->self, no cast, range 24 width 6 rect
    PointBlack2 = 24608, // BlackLance->self, no cast, range 50 width 6 rect
    GenerateBarrier6 = 24585, // Helper->self, 4.0s cast, range 24 width 3 rect
    GenerateBarrier7 = 25363, // Helper->self, no cast, range 24 width 3 rect
    PointWhite2 = 24609, // WhiteLance->self, no cast, range 24 width 6 rect
    RecreateMeteor = 24903, // Boss->self, 2.0s cast, single-target
    UnknownAbility = 18683, // Ally2B->location, no cast, single-target
    WipeWhite = 24588, // Helper->self, 13.0s cast, range 75 circle
    WipeBlack = 24589, // Helper->self, 13.0s cast, range 75 circle
    Replicate = 24587, // Boss->self, 3.0s cast, single-target
    DiffuseEnergy1 = 24611, // RedGirl2->self, 5.0s cast, range 12 120-degree cone
    DiffuseEnergy2 = 24662, // RedGirl2->self, no cast, range 12 ?-degree cone
    ManipulateEnergy1 = 24601, // Boss->self, 4.0s cast, single-target
    ManipulateEnergy2 = 24602, // Helper->player, no cast, range 3 circle
    ChildsPlay2 = 24613, // Boss->self, 10.0s cast, single-target
    ManipulateEnergy3 = 24600, // Boss->self, 4.0s cast, single-target
}

public enum SID : uint
{
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    Invincibility = 1570, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    VulnerabilityUp = 1789, // player/BlackPylon/WhiteLance/BlackLance/RedGirl2->player, extra=0x1/0x2
    MeatAndMead = 360, // none->player, extra=0xA
    ProperCare = 362, // none->player, extra=0x14
    BrinkOfDeath = 44, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    PayingThePiper = 1681, // none->player, extra=0x2/0x4
    UnknownStatus = 2160, // none->Ally2B, extra=0x1C94
    HelpingHand = 368, // none->player, extra=0xA
}

public enum IconID : uint
{
    Icon_263 = 263, // player
    Icon_168 = 168, // RedGirl2
    Icon_167 = 167, // RedGirl2
    Tankbuster = 218, // player
}

public enum TetherID : uint
{
    Tether_149 = 149, // player/RedGirl2->Boss/RedGirl1
}
