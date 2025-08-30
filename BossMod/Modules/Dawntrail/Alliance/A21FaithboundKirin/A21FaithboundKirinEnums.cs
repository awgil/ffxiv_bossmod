namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

public enum OID : uint
{
    Boss = 0x493C, // R7.990, x1
    Helper = 0x233C, // R0.500, x34, Helper type
    DawnboundSeiryu = 0x493D, // R3.200, x0-1 (spawn during fight)
    MoonboundGenbu = 0x493E, // R8.750, x0-1 (spawn during fight)
    SunboundSuzaku = 0x493F, // R6.000, x1
    DuskboundByakko = 0x4940, // R6.000, x0-1 (spawn during fight)
    SculptedArm1 = 0x4941, // R8.400, x1
    SculptedArm2 = 0x4942, // R8.400, x1
    ChiseledArm1 = 0x4943, // R24.000, x0 (spawn during fight)
    ChiseledArm2 = 0x4944, // R24.000, x0 (spawn during fight)
    BallOfFire = 0x4946, // R1.000, x0 (spawn during fight)
    ChiseledArm3 = 0x4A20, // R15.000, x0 (spawn during fight)
    ChiseledArm4 = 0x4A21, // R15.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45306, // Boss->player, no cast, single-target
    StonegaIV = 44490, // Boss->self, 5.0s cast, range 60 circle
    BossJump = 44493, // Boss->location, no cast, single-target
    WroughtArms = 44433, // Boss->self, 3.5s cast, single-target
    SynchronizedStrikeCast = 44437, // Boss->self, 4.4+0.6s cast, single-target
    CrimsonRiddleFront = 45044, // Boss->self, 5.0s cast, range 30 180-degree cone
    CrimsonRiddleBack = 45045, // Boss->self, 5.0s cast, range 30 180-degree cone
    SummonShijin = 44414, // Boss->self, 7.0s cast, single-target

    SeiryuJump1 = 44415, // DawnboundSeiryu->location, no cast, single-target
    SeiryuJump2 = 45112, // DawnboundSeiryu->location, no cast, single-target
    EastwindWheel1 = 44416, // DawnboundSeiryu->self, 8.0s cast, range 60 width 18 rect
    EastwindWheel2 = 44417, // DawnboundSeiryu->self, 8.0s cast, range 60 width 18 rect
    EastwindWheelRepeat = 44418, // Helper->self, no cast, range 60 width 18 rect
    EastwindWheelCone = 44419, // Helper->self, no cast, range 60 ?-degree cone
    StonegaIIISeiryu = 43287, // Helper->players, 8.0s cast, range 6 circle
    QuakeSeiryu = 43288, // Helper->self, 8.0s cast, range 6 circle

    ShatteringStomp = 44420, // MoonboundGenbu->self, 3.0s cast, range 35 circle
    MoontideFontFast = 44421, // Helper->self, 8.0s cast, range 9 circle
    MoontideFontSlow = 44422, // Helper->self, 8.2s cast, range 9 circle
    MidwinterMarchCast = 44423, // MoonboundGenbu->location, 6.0+1.0s cast, single-target
    NorthernCurrent = 44424, // MoonboundGenbu->self, 3.0s cast, range 12-60 donut
    MidwinterMarch = 44337, // Helper->location, 7.0s cast, range 12 circle

    SunPowder = 44425, // SunboundSuzaku->location, no cast, single-target
    VermilionFlightCast = 44426, // SunboundSuzaku->self, 4.5+3.5s cast, single-target
    ArmOfPurgatory = 44794, // Helper->self, 11.0s cast, range 3 circle
    VermilionFlight = 44795, // Helper->self, 8.0s cast, range 60 width 20 rect
    ArmOfPurgatoryCast = 44796, // BallOfFire->self, 5.5s cast, single-target
    StonegaIIIBoss = 45177, // Boss->self, 3.0s cast, single-target
    StonegaIIISuzaku = 45178, // Helper->players, 8.0s cast, range 6 circle

    ByakkoWallSpawn = 44495, // Helper->self, 2.0s cast, range 5 width 16 rect
    ByakkoJump1 = 44429, // DuskboundByakko->location, no cast, single-target
    ByakkoJump2 = 44430, // DuskboundByakko->location, no cast, single-target
    GloamingGleam = 44431, // DuskboundByakko->location, 3.0s cast, range 50 width 12 rect
    RazorFang = 44432, // Helper->self, 4.5s cast, range 20 circle
    QuakeCast = 45179, // Boss->self, 3.0s cast, single-target
    QuakeByakko = 45180, // Helper->self, 5.0s cast, range 10 circle

    // tutorial mechanic
    SynchronizedStrikeSlow = 44438, // Helper->self, 5.0s cast, range 60 width 10 rect
    SynchronizedSmiteRightSlow = 44443, // SculptedArm1self, 5.0s cast, range 60 width 32 rect
    SynchronizedSmiteLeftSlow = 44444, // SculptedArm2->self, 5.0s cast, range 60 width 32 rect

    WringerSlow = 44434, // Boss->self, 5.0s cast, range 14 circle
    StrikingRightBoss = 44435, // Boss->self, 5.0s cast, range 10 circle
    StrikingLeftBoss = 44436, // Boss->self, 5.0s cast, range 10 circle
    DeadWringerSlow = 44439, // SculptedArm1->self, 5.0s cast, range 14-30 donut
    SmitingRightSlow = 44441, // SculptedArm1->self, 5.0s cast, range 30 circle
    SmitingLeftSlow = 44442, // SculptedArm2->self, 5.0s cast, range 30 circle

    // memory mechanic
    WringerFast = 44450, // Boss->self, 2.0s cast, range 14 circle
    StrikingRightFast = 44451, // Boss->self, 2.0s cast, range 10 circle
    StrikingLeftFast = 44452, // Boss->self, 2.0s cast, range 10 circle
    SmitingRightFast = 44457, // SculptedArm1->self, 2.0s cast, range 30 circle
    SmitingLeftFast = 44458, // SculptedArm2->self, 2.0s cast, range 30 circle

    WringerTelegraph = 44461, // Helper->self, 3.0s cast, range 14 circle
    StrikingRightTelegraph = 44462, // Helper->self, 3.0s cast, range 10 circle
    StrikingLeftTelegraph = 44463, // Helper->self, 3.0s cast, range 10 circle
    SynchronizedStrikeTelegraph = 44464, // Helper->self, 3.0s cast, range 60 width 10 rect

    DeadWringerSlowVisual = 44440, // SculptedArm2->self, 5.0s cast, single-target
    DoubleWringer = 44445, // Boss->self, 10.0s cast, range 14 circle
    SmitingRightSequence = 44446, // Boss->self, 10.0s cast, range 10 circle
    SmitingLeftSequence = 44447, // Boss->self, 10.0s cast, range 10 circle
    SynchronizedSequenceCast = 44448, // Boss->self, 9.4+0.6s cast, single-target
    SynchronizedSequence = 44449, // Helper->self, 10.0s cast, range 60 width 10 rect
    SynchronizedStrikeFastVisual = 44453, // Boss->self, 1.4+0.6s cast, single-target
    SynchronizedStrikeFast = 44454, // Helper->self, 2.0s cast, range 60 width 10 rect
    SynchronizedSmite1Fast = 44459, // SculptedArm1->self, 2.0s cast, range 60 width 32 rect
    SynchronizedSmite2Fast = 44460, // SculptedArm2->self, 2.0s cast, range 60 width 32 rect
    MightyGrip = 44465, // Boss->self, 7.0s cast, single-target
    Shockwave = 44480, // Helper->self, no cast, range 60 circle
    DeadlyHoldCast = 44466, // Boss->self, 12.5s cast, single-target
    DeadlyHold1 = 44928, // ChiseledArm3/ChiseledArm4->self, 12.5s cast, single-target
    UnkBoss = 44468, // Boss->self, no cast, single-target
    DeadlyHold2 = 44469, // ChiseledArm1->self, 14.0s cast, single-target
    DeadlyHold3 = 44470, // ChiseledArm2->self, 14.0s cast, single-target
    Unk1Arm = 44471, // ChiseledArm1->self, no cast, single-target
    Unk2Arm = 44472, // ChiseledArm2->self, no cast, single-target
    Unk3Arm = 44473, // ChiseledArm1->self, no cast, single-target
    Unk4Arm = 44474, // ChiseledArm2->self, no cast, single-target
    Bury = 44479, // Helper->player, no cast, single-target
    KirinCaptivatorBoss = 44467, // Boss->self, 30.0s cast, single-target
    KirinCaptivatorArmVisual = 44929, // ChiseledArm3/ChiseledArm4->self, 30.0s cast, single-target
    KirinCaptivatorArm1 = 44477, // ChiseledArm1->self, 31.5s cast, range 60 circle
    KirinCaptivatorArm2 = 44478, // ChiseledArm2->self, 31.5s cast, range 60 circle
    DoubleCast = 43286, // Boss->self, 3.0s cast, single-target
    DeadWringerFastVisual = 44456, // SculptedArm2->self, 2.0s cast, single-target
    DeadWringerFast = 44455, // SculptedArm1->self, 2.0s cast, range 14-30 donut
}

public enum IconID : uint
{
    TurnLeft = 502, // DawnboundSeiryu->self
    TurnRight = 501, // DawnboundSeiryu->self
    Spread = 101, // player->self
}
