namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

public enum OID : uint
{
    SwordDancer = 0x4D76, // R6.000, x1
    SwordDancer2 = 0x4D7D, // R1.000, x1

    DancingSword = 0x4D7C, // R2.000, x16
    DancingSword1 = 0x4D7B, // R2.000, x2
    DancingSword2 = 0x4D7A, // R1.000, x5
    DancingSword3 = 0x4D79, // R2.000, x3
    DancingSword4 = 0x4D77, // R2.000, x4
    DancingSword5 = 0x233C, // R0.500, x29, Helper type

    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x0 (spawn during fight), EventObj type
    Actor1ec032 = 0x1EC032, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ec033 = 0x1EC033, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 50925, // SwordDancer->player, no cast, single-target
    SwordStorm1 = 49617, // SwordDancer->self, 5.0s cast, ???
    SwordStorm2 = 49684, // DancingSword5->self, no cast, ???
    UnknownAbility = 49558, // SwordDancer->location, no cast, single-target
    UnknownAbility1 = 49557, // SwordDancer2->self, no cast, range ?-30 donut
    ThrowingSwords = 49559, // SwordDancer->self, 2.0+1.0s cast, single-target
    Rush = 50525, // DancingSword4->location, 3.0s cast, width 7 rect charge
    Rush1 = 50526, // DancingSword4->location, 3.0s cast, width 7 rect charge
    Turn = 49563, // DancingSword4->location, 3.5s cast, ???
    Turn1 = 49575, // DancingSword5->self, 3.5s cast, range ?-14 donut
    Turn2 = 49577, // DancingSword5->self, 3.5s cast, range ?-24 donut
    Turn3 = 49568, // DancingSword4->location, 3.5s cast, ???
    Turn4 = 49569, // DancingSword4->location, 3.5s cast, ???
    Turn5 = 49578, // DancingSword5->self, 3.5s cast, range ?-14 donut
    MartialMystique1 = 49583, // SwordDancer->self, 4.0+1.5s cast, single-target
    MartialMystique2 = 49585, // DancingSword5->self, 5.5s cast, range 48 width 96 rect
    Turnabout = 49889, // DancingSword5->self, 3.5s cast, range ?-24 donut
    Turn6 = 49574, // DancingSword4->location, 3.5s cast, ???
    MartialMystique3 = 49584, // SwordDancer->self, 4.0+1.5s cast, single-target
    CycloswordsUnsheathed = 49586, // SwordDancer->self, 3.0s cast, single-target
    Cycloswords = 49587, // SwordDancer->self, 3.0s cast, single-target
    Spin = 49589, // DancingSword3->self, 1.0s cast, range 5-60 donut
    Spin1 = 49592, // DancingSword3->self, 1.0s cast, range 15 circle
    SwordDance1 = 49609, // SwordDancer->self, 4.4+0.6s cast, single-target
    SwordDance2 = 49610, // DancingSword5->self, 5.0s cast, ???
    SwordDance3 = 49611, // DancingSword5->self, no cast, ???
    SwordDance4 = 49612, // DancingSword5->self, no cast, ???
    SwordDance5 = 49613, // DancingSword5->self, no cast, ???
    SwordDance6 = 49614, // DancingSword5->self, 1.5s cast, range 60 width 20 rect
    LeapingLift = 49594, // SwordDancer->self, 3.0s cast, single-target
    Pierce = 49595, // DancingSword2->self, 3.6s cast, range 5 circle
    LeapingLift1 = 49596, // SwordDancer->location, no cast, ???
    LeapingLift2 = 49597, // SwordDancer->location, no cast, single-target
    LeapingLift3 = 49598, // SwordDancer->location, no cast, ???
    Swordpointe = 49685, // SwordDancer->self, 2.0+1.0s cast, single-target
    Steelsbreath = 50359, // DancingSword5->self, 2.0s cast, ???
    Steelsbreath1 = 49599, // DancingSword2->self, 2.0s cast, ???
    SurgeswordsUnsheathed = 49615, // SwordDancer->self, 3.0s cast, single-target
    Rush2 = 49616, // DancingSword->self, 4.0s cast, range 30 width 6 rect
    ThrowingSwords1 = 49560, // SwordDancer->self, no cast, single-target
    Spin2 = 49593, // DancingSword3->self, 1.0s cast, range 20 circle
}
public enum SID : uint
{
    VulnerabilityUp = 2347, // DancingSword5/DancingSword/DancingSword3/DancingSword4->player, extra=0x1/0x2/0x4/0x3
    UnknownStatus1 = 3558, // none->DancingSword3, extra=0x46E/0x46F
    UnknownStatus2 = 2056, // none->SwordDancer/DancingSword2, extra=0x47A/0x47B

}
public enum TetherID : uint
{
    Tether_chn_sworddancer_r01t1 = 423, // DancingSword4->SwordDancer
    Tether_chn_sworddancer_l01t1 = 424, // DancingSword4->SwordDancer
}
