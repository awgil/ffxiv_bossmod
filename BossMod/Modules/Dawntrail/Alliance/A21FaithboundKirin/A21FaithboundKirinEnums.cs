#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

public enum OID : uint
{
    Boss = 0x493C, // R7.990, x1
    _Gen_SculptedArm = 0x4941, // R8.400, x1
    _Gen_SculptedArm1 = 0x4942, // R8.400, x1
    Helper = 0x233C, // R0.500, x34, Helper type
    _Gen_DawnboundSeiryu = 0x493D, // R3.200, x0-1 (spawn during fight)
    _Gen_MoonboundGenbu = 0x493E, // R8.750, x0-1 (spawn during fight)
    _Gen_SunboundSuzaku = 0x493F, // R6.000, x1
    _Gen_DuskboundByakko = 0x4940, // R6.000, x0-1 (spawn during fight)
    _Gen_ChiseledArm = 0x4A20, // R15.000, x0 (spawn during fight)
    _Gen_ChiseledArm1 = 0x4944, // R24.000, x0 (spawn during fight)
    _Gen_ChiseledArm2 = 0x4A21, // R15.000, x0 (spawn during fight)
    _Gen_ChiseledArm3 = 0x4943, // R24.000, x0 (spawn during fight)
    _Gen_BallOfFire = 0x4946, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 45306, // Boss->player, no cast, single-target
    _Spell_StonegaIV = 44490, // Boss->self, 5.0s cast, range 60 circle
    _Ability_ = 44493, // Boss->location, no cast, single-target
    _Ability_WroughtArms = 44433, // Boss->self, 3.5s cast, single-target
    _Ability_SynchronizedStrike = 44437, // Boss->self, 4.4+0.6s cast, single-target
    _Ability_CrimsonRiddle = 45044, // Boss->self, 5.0s cast, range 30 180-degree cone
    _Ability_CrimsonRiddle1 = 45045, // Boss->self, 5.0s cast, range 30 180-degree cone
    _Ability_SummonShijin = 44414, // Boss->self, 7.0s cast, single-target
    _Ability_1 = 44495, // Helper->self, 2.0s cast, range 5 width 16 rect
    _Ability_2 = 44430, // 4940->location, no cast, single-target
    _Ability_GloamingGleam = 44431, // 4940->location, 3.0s cast, range 50 width 12 rect
    _Ability_RazorFang = 44432, // Helper->self, 4.5s cast, range 20 circle
    _Ability_3 = 44429, // 4940->location, no cast, single-target
    _Spell_Quake = 45179, // Boss->self, 3.0s cast, single-target
    _Spell_Quake1 = 45180, // Helper->self, 5.0s cast, range 10 circle
    _Ability_DeadWringer1 = 44440, // 4942->self, 5.0s cast, single-target

    // tutorial mechanic
    SynchronizedStrikeSlow = 44438, // Helper->self, 5.0s cast, range 60 width 10 rect
    SynchronizedSmiteRightSlow = 44443, // 4941->self, 5.0s cast, range 60 width 32 rect
    SynchronizedSmiteLeftSlow = 44444, // 4942->self, 5.0s cast, range 60 width 32 rect

    WringerSlow = 44434, // Boss->self, 5.0s cast, range 14 circle
    StrikingRightBoss = 44435, // Boss->self, 5.0s cast, range 10 circle
    StrikingLeftBoss = 44436, // Boss->self, 5.0s cast, range 10 circle
    DeadWringerSlow = 44439, // 4941->self, 5.0s cast, range 14-30 donut
    SmitingRightSlow = 44441, // 4941->self, 5.0s cast, range 30 circle
    SmitingLeftSlow = 44442, // 4942->self, 5.0s cast, range 30 circle

    // memory mechanic
    WringerFast = 44450, // Boss->self, 2.0s cast, range 14 circle
    StrikingRightFast = 44451, // Boss->self, 2.0s cast, range 10 circle
    StrikingLeftFast = 44452, // Boss->self, 2.0s cast, range 10 circle
    SmitingRightFast = 44457, // 4941->self, 2.0s cast, range 30 circle
    SmitingLeftFast = 44458, // 4942->self, 2.0s cast, range 30 circle

    WringerTelegraph = 44461, // Helper->self, 3.0s cast, range 14 circle
    StrikingRightTelegraph = 44462, // Helper->self, 3.0s cast, range 10 circle
    StrikingLeftTelegraph = 44463, // Helper->self, 3.0s cast, range 10 circle
    SynchronizedStrikeTelegraph = 44464, // Helper->self, 3.0s cast, range 60 width 10 rect

    _Ability_SynchronizedSequence = 44448, // Boss->self, 9.4+0.6s cast, single-target
    _Ability_SynchronizedSequence1 = 44449, // Helper->self, 10.0s cast, range 60 width 10 rect
    _Ability_MightyGrip = 44465, // Boss->self, 7.0s cast, single-target
    _Ability_Shockwave = 44480, // Helper->self, no cast, range 60 circle
    _Ability_DeadlyHold = 44466, // Boss->self, 12.5s cast, single-target
    _Ability_DeadlyHold1 = 44928, // 4A20/4A21->self, 12.5s cast, single-target
    _Ability_DeadlyHold2 = 44470, // 4944->self, 14.0s cast, single-target
    _Ability_DeadlyHold3 = 44469, // 4943->self, 14.0s cast, single-target
    _Ability_4 = 44472, // 4944->self, no cast, single-target
    _Ability_5 = 44471, // 4943->self, no cast, single-target
    _Ability_6 = 44474, // 4944->self, no cast, single-target
    _Ability_7 = 44473, // 4943->self, no cast, single-target
    _Ability_Bury = 44479, // Helper->player, no cast, single-target
    _Ability_8 = 44468, // Boss->self, no cast, single-target
    _Ability_SmitingRightSequence = 44446, // Boss->self, 10.0s cast, range 10 circle
    _Ability_SmitingLeftSequence = 44447, // Boss->self, 10.0s cast, range 10 circle
    _Ability_KirinCaptivator = 44467, // Boss->self, 30.0s cast, single-target
    _Ability_KirinCaptivator1 = 44929, // 4A20/4A21->self, 30.0s cast, single-target
    _Ability_KirinCaptivator2 = 44478, // 4944->self, 31.5s cast, range 60 circle
    _Ability_KirinCaptivator3 = 44477, // 4943->self, 31.5s cast, range 60 circle
    _Ability_9 = 44415, // 493D->location, no cast, single-target
    _Ability_EastwindWheel = 44417, // 493D->self, 8.0s cast, range 60 width 18 rect
    _Ability_EastwindWheel1 = 44418, // Helper->self, no cast, range 60 width 18 rect
    _Ability_EastwindWheel2 = 44419, // Helper->self, no cast, range 60 ?-degree cone
    _Ability_10 = 45112, // 493D->location, no cast, single-target
    _Ability_EastwindWheel3 = 44416, // 493D->self, 8.0s cast, range 60 width 18 rect
    _Ability_DoubleCast = 43286, // Boss->self, 3.0s cast, single-target
    _Spell_StonegaIII = 43287, // Helper->players, 8.0s cast, range 6 circle
    _Spell_Quake2 = 43288, // Helper->self, 8.0s cast, range 6 circle
    _Ability_DeadWringer2 = 44456, // 4942->self, 2.0s cast, single-target
    _Ability_DeadWringer3 = 44455, // 4941->self, 2.0s cast, range 14-30 donut
    _Ability_DoubleWringer = 44445, // Boss->self, 10.0s cast, range 14 circle
    _Ability_SunPowder = 44425, // 493F->location, no cast, single-target
    _Ability_VermilionFlight = 44426, // 493F->self, 4.5+3.5s cast, single-target
    _Ability_ArmOfPurgatory = 44796, // 4946->self, 5.5s cast, single-target
    _Ability_VermilionFlight1 = 44795, // Helper->self, 8.0s cast, range 60 width 20 rect
    _Ability_ArmOfPurgatory1 = 44794, // Helper->self, 11.0s cast, range 3 circle
    _Spell_StonegaIII1 = 45177, // Boss->self, 3.0s cast, single-target
    _Spell_StonegaIII2 = 45178, // Helper->players, 8.0s cast, range 6 circle
    _Ability_ShatteringStomp = 44420, // 493E->self, 3.0s cast, range 35 circle
    _Ability_MoontideFont = 44421, // Helper->self, 8.0s cast, range 9 circle
    _Ability_MoontideFont1 = 44422, // Helper->self, 8.2s cast, range 9 circle
    _Ability_MidwinterMarch = 44423, // 493E->location, 6.0+1.0s cast, single-target
    _Ability_MidwinterMarch1 = 44337, // Helper->location, 7.0s cast, range 12 circle
    _Ability_NorthernCurrent = 44424, // 493E->self, 3.0s cast, range 12-60 donut
    _Ability_SynchronizedStrike3 = 44453, // Boss->self, 1.4+0.6s cast, single-target
    _Ability_SynchronizedStrike4 = 44454, // Helper->self, 2.0s cast, range 60 width 10 rect
    _Ability_SynchronizedSmite2 = 44460, // 4942->self, 2.0s cast, range 60 width 32 rect
    _Ability_SynchronizedSmite3 = 44459, // 4941->self, 2.0s cast, range 60 width 32 rect
}

public enum IconID : uint
{
    _Gen_Icon_m0851_turnleft_c0g = 502, // _Gen_DawnboundSeiryu->self
    _Gen_Icon_m0851_turnright_c0g = 501, // _Gen_DawnboundSeiryu->self
    _Gen_Icon_m0074g01ai = 101, // player->self
}
