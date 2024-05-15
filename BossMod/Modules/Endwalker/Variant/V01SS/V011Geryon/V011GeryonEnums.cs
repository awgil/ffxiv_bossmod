namespace BossMod.Endwalker.Variant.V01SS.V011Geryon;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x10, 523 type
    Boss = 0x398B, // R9.600, x1
    RustyWinch = 0x1EB7CD, // R0.500, x1, EventObj type
    CisternSwitch = 0x1EB7D2, // R2.000, x0 (spawn during fight), EventObj type
    PowderKegRed = 0x39C9, // R1.000, x0 (spawn during fight)
    PowderKegBlue = 0x398C, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    ColossalCharge1 = 29900, // Boss->location, 8.0s cast, width 14 rect charge
    ColossalCharge2 = 29901, // Boss->location, 8.0s cast, width 14 rect charge
    ColossalLaunch = 29896, // Boss->self, 5.0s cast, range 40 width 40 rect
    ColossalSlam = 29904, // Boss->self, 6.0s cast, range 60 60-degree cone
    ColossalStrike = 29903, // Boss->player, 5.0s cast, single-target
    ColossalSwing = 29905, // Boss->self, 5.0s cast, range 60 180-degree cone
    ExplodingCatapult = 29895, // Boss->self, 5.0s cast, range 60 circle
    ExplosionAOE = 29908, // PowderKegBlue/PowderKegRed->self, 2.5s cast, range 15 circle
    ExplosionDonut = 29909, // PowderKegRed/PowderKegBlue->self, 2.5s cast, range 5-17 donut
    GigantomillFirst = 29897, // Boss->self, 8.0s cast, range 72 width 10 cross
    GigantomillRest = 29899, // Boss->self, no cast, range 72 width 10 cross
    SubterraneanShudder = 29906, // Boss->self, 5.0s cast, range 60 circle raidwide
    Unknown1 = 29894, // Boss->location, no cast, single-target
    Unknown2 = 31260, // PowderKegRed->self, no cast, single-target
    Unknown3 = 29907, // PowderKegBlue->self, no cast, single-target

    Intake = 29913, // Helper->self, no cast, range 40 width 10 rect

    RunawayRunoff = 29911, // Helper->self, 9.0s cast, range 60 circle
    Gigantomill = 29898, // Boss->self, 8.0s cast, range 72 width 10 cross

    RollingBoulder = 29914, // Helper->self, no cast, range 10 width 10 rect

    //route 3
    RunawaySludge = 29910, // Helper->self, 5.0s cast, range 9 circle
    Shockwave = 29902, // Boss->self, 5.0s cast, range 40 width 40 rect
}

public enum SID : uint
{
    Sludge = 2948, // none->player, extra=0x0 route 3
}

public enum IconID : uint
{
    Icon_218 = 218, // player
    Icon_156 = 156, // Boss
    Icon_157 = 157, // Boss
}
