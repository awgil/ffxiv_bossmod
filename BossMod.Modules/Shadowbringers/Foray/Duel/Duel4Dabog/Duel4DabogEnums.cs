namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

public enum OID : uint
{
    Boss = 0x3261, // R4.500, x1
    Helper = 0x233C, // R0.500, x14
    Deathwall = 0x18D6, // R0.500, spawn during fight
    DabogFragment = 0x3262, // R4.500, spawn during fight
    AtomicSphere = 0x3263, // R1.000, spawn during fight
    AtomicSphereVoidzone = 0x1EB1A4, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Deathwall = 24246, // Deathwall->self, no cast, range 20-30 donut deathwall
    Enrage = 24247, // Boss->self, 10.0s cast, enrage
    LeftArmLimitCut = 24249, // Boss->self, 4.0s cast, single-target, buff for next left-arm attack
    RightArmLimitCut = 24250, // Boss->self, 4.0s cast, single-target, buff for next right-arm attack
    LeftArmMetalCutter = 24251, // Boss->self, 5.0s cast, single-target, visual (triple cones)
    LeftArmMetalCutterSecond = 24252, // Boss->self, no cast, single-target, visual (second set)
    LeftArmMetalCutterAOE1 = 24253, // Helper->self, 6.0s cast, range 40 90-degree cone aoe
    LeftArmMetalCutterAOE2 = 24254, // Helper->self, 1.0s cast, range 40 90-degree cone aoe
    LeftArmMetalCutterKnockbackShort = 24255, // Helper->self, no cast, range 40 ?-degree cone knockback 5
    LeftArmMetalCutterKnockbackLong = 24256, // Helper->self, no cast, range 40 ?-degree cone knockback 15
    RightArmComet = 24257, // Boss->self, 5.0s cast, single-target, visual (create tower)
    RightArmCometKnockbackShort = 24258, // Helper->self, 7.5s cast, range 5 circle tower with knockback 12
    RightArmCometKnockbackLong = 24259, // Helper->self, 7.5s cast, range 5 circle tower with knockback 25
    ArmUnit = 24261, // Boss->self, 5.0s cast, single-target, visual (cutter + comet combo)
    ArmUnitSecond = 24262, // Boss->self, no cast, single-target, visual (second set of cones)
    CellDivision = 24263, // Boss->self, 4.0s cast, single-target, visual (spawn fragments)
    FragmentAppear = 24264, // DabogFragment->self, no cast, single-target, visual (cast at spawn)
    RightArmBlasterFragment = 24265, // DabogFragment->self, 4.0s cast, range 100 width 6 rect
    RightArmBlasterBoss = 24266, // Boss->self, 4.0s cast, range 100 width 6 rect aoe
    LeftArmSlash = 24267, // Boss->self, 3.0s cast, range 10 ?-degree cone
    LeftArmWave = 24268, // Boss->self, 5.0s cast, single-target, visual (baited large aoe)
    LeftArmWaveAOE = 24269, // Helper->location, 3.0s cast, range 24 circle baited aoe
    RightArmRayNormal = 24270, // Boss->self, 4.0s cast, single-target, visual (triple circles)
    RightArmRayNormalAOE = 24271, // Helper->self, 11.0s cast, range 10 circle aoe
    RightArmRayBuffed = 24272, // Boss->self, 4.0s cast, single-target, visual (rotating crosses)
    RightArmRayAOEFirst = 24273, // Helper->self, 10.0s cast, range 16 width 6 cross aoe
    RightArmRayAOERest = 24274, // Helper->self, no cast, range 16 width 6 cross aoe
    RightArmRayVoidzone = 24275, // Helper->player, no cast, range 5 circle aoe that leaves voidzone
}

public enum IconID : uint
{
    LeftArmMetalCutter = 14, // player
    RightArmBlasterBait = 42, // player
    LeftArmWaveBait = 23, // player
    AtomicSphereCW = 156, // AtomicSphere
    AtomicSphereCCW = 157, // AtomicSphere
}
