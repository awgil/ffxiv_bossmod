namespace BossMod.Shadowbringers.Alliance.A33RedGirlP1;

public enum OID : uint
{
    Boss = 0x32BB, // R7.500, x1
    Helper = 0x233C, // R0.500, x16 (spawn during fight), 523 type
    Ally2B = 0x31A8, // R0.512, x1
    WhiteLance = 0x32E3, // R1.000, x0 (spawn during fight)
    BlackLance = 0x32E4, // R1.000, x0 (spawn during fight)
    RedGirl1 = 0x32BC, // R2.250, x0 (spawn during fight)
    WallbossRedGirl = 0x32BD, // R12.250, x0 (spawn during fight)
    RedGirl3 = 0x32BE, // R12.250, x3
}

public enum AID : uint
{
    BladeFlurry1 = 23788, // Ally2B->Boss/RedGirl2, no cast, single-target
    BladeFlurry2 = 23789, // Ally2B->Boss/RedGirl2, no cast, single-target
    AutoAttack = 24597, // Helper->player, no cast, single-target
    DancingBlade = 23790, // Ally2B->Boss/RedGirl2, no cast, width 2 rect charge
    BalancedEdge = 23791, // Ally2B->self, 2.0s cast, range 5 circle
    Cruelty1 = 24594, // Boss->self, 5.0s cast, single-target
    Cruelty2 = 24596, // Helper->location, no cast, range 75 circle
    Shockwave = 24590, // Boss->self, 2.0s cast, single-target
    GenerateBarrier1 = 24585, // Helper->self, 4.0s cast, range 24 width 3 rect
    GenerateBarrier2 = 24580, // Boss->self, 4.0s cast, single-target
    GenerateBarrier3 = 25363, // Helper->self, no cast, range 24 width 3 rect
    WhirlingAssault = 23792, // Ally2B->self, 2.0s cast, range 40 width 4 rect
    ShockWhite1 = 24591, // Helper->players, no cast, range 5 circle
    ShockWhite2 = 24592, // Helper->location, 4.0s cast, range 5 circle
    PointWhite1 = 24607, // WhiteLance->self, no cast, range 50 width 6 rect
    PointWhite2 = 24609, // WhiteLance->self, no cast, range 24 width 6 rect
    ShockBlack1 = 24972, // Helper->location, 4.0s cast, range 5 circle
    ShockBlack2 = 24593, // Helper->players, no cast, range 5 circle
    PointBlack1 = 24608, // BlackLance->self, no cast, range 50 width 6 rect
    PointBlack2 = 24610, // BlackLance->self, no cast, range 24 width 6 rect
    Vortex = 24599, // Helper->location, no cast, ???
    UnknownAbility = 18683, // Ally2B->location, no cast, single-target
    GenerateBarrier4 = 24584, // Helper->self, 4.0s cast, range 18 width 3 rect
    GenerateBarrier5 = 25362, // Helper->self, no cast, range 18 width 3 rect
    RecreateMeteor = 24903, // Boss->self, 2.0s cast, single-target
    WipeWhite = 24588, // Helper->self, 13.0s cast, range 75 circle
    ManipulateEnergy1 = 24600, // Boss->self, 4.0s cast, single-target
    ManipulateEnergy2 = 24602, // Helper->players, no cast, range 3 circle
    Replicate = 24586, // Boss->self, 3.0s cast, single-target
    DiffuseEnergy1 = 24611, // RedGirl1->self, 5.0s cast, range 12 120-degree cone
    DiffuseEnergy2 = 24662, // RedGirl1->self, no cast, range 12 ?-degree cone
    SublimeTranscendence1 = 25098, // Boss->self, 5.0s cast, single-target
    SublimeTranscendence2 = 25099, // Helper->location, no cast, range 75 circle
    UnknownWeaponskill = 24605, // Helper->location, no cast, single-target
}

public enum SID : uint
{
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    Invincibility = 1570, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 1789, // WhiteLance/BlackLance/RedGirl1->player, extra=0x1/0x2/0x3
    Stun = 2656, // none->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    ProgramFFFFFFF = 2632, // none->player, extra=0x1AB
    Program000000 = 2633, // none->player, extra=0x1AC
}

public enum IconID : uint
{
    Icon_262 = 262, // player
    Icon_263 = 263, // player
    Icon_264 = 264, // player
    Tankbuster = 218, // player
    Icon_167 = 167, // RedGirl1
    Icon_168 = 168, // RedGirl1
}
