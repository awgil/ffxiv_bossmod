namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

public enum OID : uint
{
    NBoss = 0x405D, // R3.960, x1
    NHomingPattern = 0x405E, // R1.000, spawn during fight
    NNeedle = 0x405F, // R1.000, spawn during fight
    NBallOfFire = 0x4060, // R2.250, spawn during fight
    NBomb = 0x4061, // R1.000, spawn during fight
    NSurprisingMissile = 0x4062, // R1.000, spawn during fight
    NSurprisingStaff = 0x4063, // R1.000, spawn during fight
    NSurprisingClaw = 0x4064, // R1.200, spawn during fight

    SBoss = 0x4065, // R3.960, x1
    SHomingPattern = 0x4066, // R1.000, spawn during fight
    SNeedle = 0x4067, // R1.000, spawn during fight
    SBallOfFire = 0x4068, // R2.250, spawn during fight
    SBomb = 0x4069, // R1.000, spawn during fight
    SSurprisingMissile = 0x406A, // R1.000, spawn during fight
    SSurprisingStaff = 0x406B, // R1.000, spawn during fight
    SSurprisingClaw = 0x406C, // R1.200, spawn during fight

    Helper = 0x233C, // R0.500, x20, 523 type
    ConeSlice = 0x40C6, // R1.000, x6
    Dart = 0x1EB931, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    NAutoAttack = 35143, // NBoss->player, no cast, single-target
    SAutoAttack = 35172, // SBoss->player, no cast, single-target
    Teleport = 35111, // NBoss->location, no cast, single-target
    NShockingAbandon = 35144, // NBoss->player, 5.0s cast, single-target, tankbuster
    NAero = 35145, // NBoss->self, 5.0s cast, range 60 circle, raidwide
    SShockingAbandon = 35173, // SBoss->player, 5.0s cast, single-target, tankbuster
    SAero = 35174, // SBoss->self, 5.0s cast, range 60 circle, raidwide

    NTrickReload = 35146, // NBoss->self, 4.0s cast, single-target, visual (stack/spread on a cone slice)
    STrickReload = 35175, // SBoss->self, 4.0s cast, single-target, visual (stack/spread on a cone slice)
    LockedAndLoaded = 35109, // *Boss->self, no cast, single-target, visual (successful load)
    Misload = 35110, // *Boss->self, no cast, single-target, visual (failed load)
    NTrapshooting1 = 36122, // NBoss->self, 4.0s cast, single-target, visual (first mechanic)
    NTrapshooting2 = 35161, // NBoss->self, 4.0s cast, single-target, visual (second mechanic)
    NTrapshootingStack = 35162, // Helper->players, no cast, range 6 circle stack
    NTrapshootingSpread = 35163, // Helper->player, no cast, range 6 circle spread
    STrapshooting1 = 36124, // SBoss->self, 4.0s cast, single-target, visual (first mechanic)
    STrapshooting2 = 35190, // SBoss->self, 4.0s cast, single-target, visual (second mechanic)
    STrapshootingStack = 35191, // Helper->players, no cast, range 6 circle stack
    STrapshootingSpread = 35192, // Helper->player, no cast, range 6 circle spread
    NTriggerHappy = 35147, // NBoss->self, 4.3s cast, single-target, visual (5/6 cones)
    NTriggerHappyAOE = 35148, // Helper->self, 5.0s cast, range 40 60-degree cone aoe
    NTriggerHappyFake = 35207, // Helper->self, 5.0s cast, range 40 60-degree cone no-effect
    STriggerHappy = 35176, // SBoss->self, 4.3s cast, single-target, visual (5/6 cones)
    STriggerHappyAOE = 35177, // Helper->self, 5.0s cast, range 40 60-degree cone aoe
    STriggerHappyFake = 35208, // Helper->self, 5.0s cast, range 40 60-degree cone no-effect
    NRingARingOExplosions = 35164, // NBoss->self, 3.0s cast, single-target, visual (bombs)
    NBombBurst = 35165, // NBomb->self, 1.5s cast, range 12 circle aoe
    SRingARingOExplosions = 35193, // SBoss->self, 3.0s cast, single-target, visual (bombs)
    SBombBurst = 35194, // SBomb->self, 1.5s cast, range 12 circle aoe
    MoveBomb = 35434, // Helper->NBomb, no cast, single-target, attract on bomb

    NDartboardOfDancingExplosives = 36029, // NBoss->self, 3.0s cast, single-target, visual (dartboard)
    NUncommonGroundSuccess = 35156, // Helper->self, no cast, ???, raidwide
    NUncommonGroundFail = 36034, // Helper->self, no cast, ???, wipe if mechanic was not resolved
    SDartboardOfDancingExplosives = 36032, // SBoss->self, 3.0s cast, single-target, visual (dartboard)
    SUncommonGroundSuccess = 35185, // Helper->self, no cast, ???, raidwide
    SUncommonGroundFail = 36035, // Helper->self, no cast, ???, wipe if mechanic was not resolved
    NSurpriseBalloon = 35149, // NBoss->self, 4.0s cast, single-target, visual (spawn balloons)
    NPop = 35150, // Helper->self, 2.3s cast, range 60 circle, knockback 13
    NSurpriseNeedle = 35151, // NNeedle->self, 1.5s cast, range 40 width 2 rect (pop balloon - technically it's an aoe that hits for minor damage)
    NBeguilingGlitter = 35171, // NBoss->self, 4.0s cast, range 60 circle, visual (apply forced march)
    SSurpriseBalloon = 35178, // SBoss->self, 4.0s cast, single-target, visual (spawn balloons)
    SPop = 35179, // Helper->self, 2.3s cast, range 60 circle, knockback 13
    SSurpriseNeedle = 35180, // SNeedle->self, 1.5s cast, range 40 width 2 rect (pop balloon - technically it's an aoe that hits for minor damage)
    SBeguilingGlitter = 35200, // SBoss->self, 4.0s cast, range 60 circle, visual (apply forced march)

    NPresentBox = 35157, // NBoss->self, 3.0s cast, single-target, visual (staffs)
    NFaerieRing = 35158, // NSurprisingStaff->self, 11.0s cast, range 6?-12 donut
    NFireworks = 35166, // NBoss->self, 3.0s cast, single-target, visual (missiles/claws/chains/etc)
    NFireworksStack = 35167, // Helper->players, no cast, range 3 circle enumeration stack
    NFireworksSpread = 35168, // Helper->players, no cast, range 20 circle spread
    NMissileBurst = 35159, // NSurprisingMissile->player, no cast, single-target, kill if missile reaches
    NDeathByClaw = 35160, // NSurprisingClaw->player, no cast, single-target, kill if claw reaches
    NBurningChains = 36030, // Helper->self, no cast, ???, fail to break
    SPresentBox = 35186, // SBoss->self, 3.0s cast, single-target, visual (staffs)
    SFaerieRing = 35187, // SSurprisingStaff->self, 11.0s cast, range 6?-12 donut
    SFireworks = 35195, // SBoss->self, 3.0s cast, single-target, visual (missiles/claws/chains/etc)
    SFireworksStack = 35196, // Helper->players, no cast, range 3 circle enumeration stack
    SFireworksSpread = 35197, // Helper->players, no cast, range 20 circle spread
    SMissileBurst = 35188, // SSurprisingMissile->player, no cast, single-target, kill if missile reaches
    SDeathByClaw = 35189, // SSurprisingClaw->player, no cast, single-target, kill if claw reaches
    SBurningChains = 36033, // Helper->self, no cast, ???, fail to break

    NPinwheelingDartboard = 36028, // NBoss->self, 3.0s cast, single-target, visual (dartboard)
    SPinwheelingDartboard = 36031, // SBoss->self, 3.0s cast, single-target, visual (dartboard)
    NFireSpread = 35202, // NBallOfFire->self, 8.0s cast, single-target, visual (rotating fire wall)
    NFireSpreadFirst = 35154, // Helper->self, 8.0s cast, range 12 width 5 rect
    NFireSpreadRest = 35321, // Helper->self, no cast, range 12 width 5 rect (x11, advance by 10 degrees every time)
    SFireSpread = 35203, // SBallOfFire->self, 8.0s cast, single-target, visual (rotating fire wall)
    SFireSpreadFirst = 35183, // Helper->self, 8.0s cast, range 12 width 5 rect
    SFireSpreadRest = 35323, // Helper->self, no cast, range 12 width 5 rect (x11, advance by 10 degrees every time)
    Enrage = 35875, // *Boss->self, 10.0s cast, range 60 circle, enrage
    EnrageAOE = 35876, // *Boss->self, no cast, range 60 circle, enrage repeat (every 3s after enrage)
}

public enum SID : uint
{
    BullsEye = 3742, // none->player, extra=0x0
    ForwardMarch = 3538, // none->player, extra=0x0
    AboutFace = 3539, // none->player, extra=0x0
    LeftFace = 3540, // none->player, extra=0x0
    RightFace = 3541, // none->player, extra=0x0
    ForcedMarch = 1257, // none->player, extra=0x2/0x1/0x4/0x8
    BurningChains = 769, // none->player, extra=0x0
}

public enum IconID : uint
{
    Order1 = 390, // ConeSlice
    Order2 = 391, // ConeSlice
    Order3 = 392, // ConeSlice
    Order4 = 393, // ConeSlice
    Order5 = 394, // ConeSlice
    Order6 = 395, // ConeSlice
    FireworksSpread = 97, // player, indicates chain partners
    FireworksEnumeration = 347, // player
    BurningChains = 220, // player
    ShockingAbandon = 218, // player
    RotateCW = 156, // *BallOfFire
    RotateCCW = 157, // *BallOfFire
}

public enum TetherID : uint
{
    BombsLink = 54, // *Bomb->*Bomb
    Follow = 17, // *SurprisingMissile/*SurprisingClaw->player
    BurningChains = 9, // player->player
}

public enum SplitType : byte
{
    Undefined = 0x0,
    Lateral = 0x1,
    Vertical = 0x2,
}
