namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

public enum OID : uint
{
    SwordDancer = 0x4D76, // R6.000, x1
    Helper = 0x233C, // R0.500, x29, Helper type
    DancingSwordCyclosword = 0x4D79, // R2.000, x3
    DancingSwordSurgesword = 0x4D7C, // R2.000, x16
    SwordDanceMarker = 0x1EC033, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 50925, // SwordDancer->player, no cast, single-target
    SwordStormCast = 49617, // SwordDancer->self, 5.0s cast, ???
    SwordStorm = 49684, // DancingSword5->self, no cast, ???
    ThrowingSwords = 49559, // SwordDancer->self, 2.0+1.0s cast, single-target
    Rush1 = 50525, // DancingSword4->location, 3.0s cast, width 7 rect charge
    Rush2 = 50526, // DancingSword4->location, 3.0s cast, width 7 rect charge
    TurnInner1 = 49575, // DancingSword5->self, 3.5s cast, range 9-14 donut
    TurnOuter1 = 49577, // DancingSword5->self, 3.5s cast, range 19-24 donut
    TurnInner2 = 49578, // DancingSword5->self, 3.5s cast, range 9-14 donut
    TurnOuter2 = 49580, // DancingSword5->self, 3.5s cast, range 19-24 donut
    TurnaboutInner = 49883, // DancingSword5->self, 3.5s cast, range ?-14 donut
    TurnaboutOuter = 49889, // DancingSword5->self, 3.5s cast, range ?-24 donut

    MartialMystique = 49585, // DancingSword5->self, 5.5s cast, range 48 width 96 rect

    CycloswordsUnsheathed = 49586, // SwordDancer->self, 3.0s cast, single-target
    Cycloswords = 49587, // SwordDancer->self, 3.0s cast, single-target
    Spin = 49589, // DancingSword3->self, 1.0s cast, range 15-60 donut
    Spin1 = 49592, // DancingSword3->self, 1.0s cast, range 15 circle
    Spin2 = 49593, // DancingSword3->self, 1.0s cast, range 20 circle

    SwordDanceCast = 49609, // SwordDancer->self, 4.4+0.6s cast, single-target
    SwordDance = 49614, // DancingSword5->self, 1.5s cast, range 60 width 20 rect
    LeapingLift = 49594, // SwordDancer->self, 3.0s cast, single-target
    Pierce = 49595, // DancingSword2->self, 3.6s cast, range 5 circle

    Swordpointe = 49685, // SwordDancer->self, 2.0+1.0s cast, single-target
    Steelsbreath = 50359, // DancingSword5->self, 2.0s cast, ???
    Steelsbreath1 = 49599, // DancingSword2->self, 2.0s cast, ???

    SurgeswordsUnsheathed = 49615, // SwordDancer->self, 3.0s cast, single-target
    RushSurgesword = 49616, // DancingSword->self, 4.0s cast, range 30 width 6 rect
}
public enum SID : uint
{
    Cyclosword = 3558, // none->DancingSword3, extra=0x46E/0x46F, cyclosword spin
    LeapingLift = 2056, // none->SwordDancer/DancingSword2, extra=0x47A/0x47B
}
public enum TetherID : uint
{
    Tether_chn_sworddancer_r01t1 = 423, // DancingSword4->SwordDancer
    Tether_chn_sworddancer_l01t1 = 424, // DancingSword4->SwordDancer
}
