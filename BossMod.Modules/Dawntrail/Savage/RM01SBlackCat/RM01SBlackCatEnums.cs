namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

public enum OID : uint
{
    Boss = 0x4329, // R3.993, x1
    Helper = 0x233C, // R0.500, x50, Helper type
    CopyCat = 0x432A, // R3.993, x1, mouser clone
    LeapTarget = 0x432B, // R1.000, x0 (spawn during fight)
    Soulshade = 0x432C, // R5.610, x3, boss clone
}

public enum AID : uint
{
    AutoAttack = 39152, // Boss->player, no cast, single-target
    Teleport = 37640, // Boss->location, no cast, single-target
    BiscuitMaker = 38037, // Boss->player, 5.0s cast, single-target, tankbuster
    BiscuitMakerSecond = 38038, // Boss->player, no cast, single-target, tankbuster second hit
    BloodyScratch = 38036, // Boss->self, 5.0s cast, range 100 circle, raidwide
    NineLives = 37985, // Boss->self, 3.0s cast, single-target, visual (clones that repeat mechanics)
    Soulshade = 37986, // Boss->self, 3.0s cast, single-target, visual (create clones)
    SoulshadeActivate = 37987, // Soulshade->self, no cast, single-target, visual (clone activation)

    QuadrupleCrossingFirst = 37948, // Boss->self, 5.0s cast, single-target, visual (proteans)
    QuadrupleCrossingMid = 37949, // Boss->self, 1.0s cast, single-target, visual (intermediate set)
    QuadrupleCrossingLast = 37950, // Boss->self, 1.0s cast, single-target, visual (last set)
    QuadrupleCrossingProtean = 37951, // Helper->self, no cast, range 100 45-degree cone (protean 1/2 sets)
    QuadrupleCrossingAOE = 37952, // Helper->self, 1.6s cast, range 100 45-degree cone (cone 3/4 sets)
    LeapingQuadrupleCrossingBossR = 38959, // Boss->self, 5.0s cast, single-target, visual (jump right + proteans)
    LeapingQuadrupleCrossingBossL = 37975, // Boss->self, 5.0s cast, single-target, visual (jump left + proteans)
    LeapingQuadrupleCrossingBossFirst = 37976, // Boss->self, no cast, single-target, visual (first set)
    LeapingQuadrupleCrossingBossMid = 37977, // Boss->self, 1.0s cast, single-target, visual
    LeapingQuadrupleCrossingBossLast = 37978, // Boss->self, 1.0s cast, single-target, visual
    LeapingQuadrupleCrossingBossProtean = 37979, // Helper->self, no cast, range 100 45-degree cone
    LeapingQuadrupleCrossingBossAOE = 37980, // Helper->self, 1.6s cast, range 100 45-degree cone
    LeapingQuadrupleCrossingShadeR = 38995, // Soulshade->self, 5.0s cast, single-target, visual
    LeapingQuadrupleCrossingShadeL = 38009, // Soulshade->self, 5.0s cast, single-target, visual
    LeapingQuadrupleCrossingShadeFirst = 38010, // Soulshade->self, no cast, single-target, visual
    LeapingQuadrupleCrossingShadeMid = 38011, // Soulshade->self, 1.0s cast, single-target, visual
    LeapingQuadrupleCrossingShadeLast = 38012, // Soulshade->self, 1.0s cast, single-target, visual
    LeapingQuadrupleCrossingShadeProtean = 38013, // Helper->self, no cast, range 100 45-degree cone
    LeapingQuadrupleCrossingShadeAOE = 38014, // Helper->self, 1.6s cast, range 100 45-degree cone

    OneTwoPawBossRL = 37942, // Boss->self, 5.0s cast, single-target, visual (two cleaves)
    OneTwoPawBossAOERFirst = 37943, // Helper->self, 6.0s cast, range 100 180-degree cone
    OneTwoPawBossAOELSecond = 37944, // Helper->self, 9.0s cast, range 100 180-degree cone
    OneTwoPawBossLR = 37945, // Boss->self, 5.0s cast, single-target, visual (two cleaves)
    OneTwoPawBossAOELFirst = 37947, // Helper->self, 6.0s cast, range 100 180-degree cone
    OneTwoPawBossAOERSecond = 37946, // Helper->self, 9.0s cast, range 100 180-degree cone
    OneTwoPawShadeRL = 37988, // Soulshade->self, 5.0s cast, single-target, visual (two cleaves)
    OneTwoPawShadeAOERFirst = 37989, // Helper->self, 6.0s cast, range 100 180-degree cone
    OneTwoPawShadeAOELSecond = 37990, // Helper->self, 9.0s cast, range 100 180-degree cone
    OneTwoPawShadeLR = 37991, // Soulshade->self, 5.0s cast, single-target, visual (two cleaves)
    OneTwoPawShadeAOELFirst = 37993, // Helper->self, 6.0s cast, range 100 180-degree cone
    OneTwoPawShadeAOERSecond = 37992, // Helper->self, 9.0s cast, range 100 180-degree cone
    LeapingOneTwoPawBossLRL = 37965, // Boss->self, 5.0s cast, single-target, visual (jump left, cleave right then left)
    LeapingOneTwoPawBossLLR = 37966, // Boss->self, 5.0s cast, single-target, visual (jump left, cleave left then right)
    LeapingOneTwoPawBossRRL = 37967, // Boss->self, 5.0s cast, single-target, visual (jump right, cleave right then left)
    LeapingOneTwoPawBossRLR = 37968, // Boss->self, 5.0s cast, single-target, visual (jump right, cleave left then right)
    LeapingOneTwoPawBossJumpRL = 37969, // Boss->self, no cast, single-target, visual (RL after jump)
    LeapingOneTwoPawBossAOERFirst = 37970, // Helper->self, 0.8s cast, range 100 180-degree cone
    LeapingOneTwoPawBossAOELSecond = 37971, // Helper->self, 2.8s cast, range 100 180-degree cone
    LeapingOneTwoPawBossJumpLR = 37972, // Boss->self, no cast, single-target, visual (LR after jump)
    LeapingOneTwoPawBossAOELFirst = 37974, // Helper->self, 0.8s cast, range 100 180-degree cone
    LeapingOneTwoPawBossAOERSecond = 37973, // Helper->self, 2.8s cast, range 100 180-degree cone
    LeapingOneTwoPawShadeLRL = 37999, // Soulshade->self, 5.0s cast, single-target, visual (jump left, cleave right then left)
    LeapingOneTwoPawShadeLLR = 38000, // Soulshade->self, 5.0s cast, single-target, visual (jump left, cleave left then right)
    LeapingOneTwoPawShadeRRL = 38001, // Soulshade->self, 5.0s cast, single-target, visual (jump right, cleave right then left)
    LeapingOneTwoPawShadeRLR = 38002, // Soulshade->self, 5.0s cast, single-target, visual (jump right, cleave left then right)
    LeapingOneTwoPawShadeJumpRL = 38003, // Soulshade->self, no cast, single-target, visual (RL after jump)
    LeapingOneTwoPawShadeAOERFirst = 38004, // Helper->self, 0.8s cast, range 100 180-degree cone
    LeapingOneTwoPawShadeAOELSecond = 38005, // Helper->self, 2.8s cast, range 100 180-degree cone
    LeapingOneTwoPawShadeJumpLR = 38006, // Soulshade->self, no cast, single-target, visual (LR after jump)
    LeapingOneTwoPawShadeAOELFirst = 38008, // Helper->self, 0.8s cast, range 100 180-degree cone
    LeapingOneTwoPawShadeAOERSecond = 38007, // Helper->self, 2.8s cast, range 100 180-degree cone

    QuadrupleSwipeBoss = 37981, // Boss->self, 4.0+1.0s cast, single-target, visual (pair stacks)
    QuadrupleSwipeBossAOE = 37982, // Helper->players, 5.0s cast, range 4 circle 2-man stack
    DoubleSwipeBoss = 37983, // Boss->self, 4.0+1.0s cast, single-target, visual (light party stacks)
    DoubleSwipeBossAOE = 37984, // Helper->players, 5.0s cast, range 5 circle 4-man stack
    QuadrupleSwipeShade = 38015, // Soulshade->self, 4.0+1.0s cast, single-target, visual (pair stacks)
    QuadrupleSwipeShadeAOE = 38016, // Helper->players, 5.0s cast, range 4 circle 2-man stack
    DoubleSwipeShade = 38017, // Soulshade->self, 4.0+1.0s cast, single-target, visual (light party stacks)
    DoubleSwipeShadeAOE = 38018, // Helper->players, 5.0s cast, range 5 circle 4-man stack

    TempestuousTearTargetSelect = 34722, // Helper->player, no cast, single-target
    TempestuousTear = 38019, // Boss->self, 5.0+1.0s cast, single-target
    TempestuousTearAOE = 38020, // Helper->players, no cast, range 100 width 6 rect
    Nailchipper = 38021, // Boss->self, 7.0+1.0s cast, single-target, visual (spreads)
    NailchipperAOE = 38022, // Helper->players, 8.0s cast, range 5 circle spread

    Mouser = 37953, // Boss->self, 10.0s cast, single-target, visual (break tiles)
    MouserPrepareJump1 = 37955, // Helper->self, 1.0s cast, range 10 width 10 rect, visual (damage tile)
    MouserPrepareJump2 = 39276, // Helper->self, 1.0s cast, range 10 width 10 rect, visual (destroy tile)
    MouserJump = 37956, // Helper->location, no cast, single-target (jump before hit)
    MouserTileDamage = 38054, // Helper->self, no cast, range 10 width 10 rect (actual aoe)
    MouserJumpsEnd = 37954, // Boss->self, no cast, single-target, visual (stop jumps)
    Copycat = 37957, // Boss->self, 3.0s cast, single-target, visual (activate clone)
    ElevateAndEviscerateKnockback = 37958, // CopyCat->self, 5.6s cast, single-target, visual (hit with knockback)
    ElevateAndEviscerateKnockbackAOE = 37959, // CopyCat->player, no cast, single-target, visual
    ElevateAndEviscerateHit = 37960, // CopyCat->self, 5.6s cast, single-target, visual (hit without knockback)
    ElevateAndEviscerateHitAOE = 37961, // CopyCat->player, no cast, single-target, visual
    ElevateAndEviscerateShockwave = 37962, // Helper->self, no cast, range 60 width 10 cross
    ElevateAndEviscerateImpactKnockback = 39251, // Helper->self, no cast, range 10 width 10 rect, aoe after knockback on landing cell
    ElevateAndEviscerateImpactHit = 39252, // Helper->self, no cast, range 10 width 10 rect, aoe after no-knockback on landing cell
    GrimalkinGale = 39811, // Boss->self, no cast, single-target, visual (start repairs + knockbacks + spreads)
    GrimalkinGaleShockwave = 37963, // Boss->self, 6.0+1.0s cast, single-target, visual (knockback)
    GrimalkinGaleShockwaveAOE = 37964, // Helper->self, 7.0s cast, range 30 circle knockback 21
    GrimalkinGaleSpreadAOE = 39812, // Helper->players, 5.0s cast, range 5 circle
    OvershadowTargetSelect = 26708, // Helper->player, no cast, single-target, visual (target selection)
    Overshadow = 38039, // Boss->player, 5.0s cast, single-target, visual (line stack)
    OvershadowAOE = 38040, // Boss->players, no cast, range 100 width 5 rect
    SplinteringNails = 38041, // Boss->self, 5.0s cast, single-target, visual (role cones)
    SplinteringNailsAOE = 38042, // Helper->self, no cast, range 100 ?-degree cone

    RainingCatsFirst = 39611, // Boss->self, 6.0s cast, single-target, visual (limit cut)
    RainingCatsMid = 39612, // Boss->self, 5.0s cast, single-target, visual (sets 2/3)
    RainingCatsLast = 39613, // Boss->self, 5.0s cast, single-target, visual (set 4)
    RainingCatsTether = 38045, // Helper->self, no cast, range 100 ?-degree cone
    RainingCatsStack = 38047, // Helper->players, no cast, range 4 circle stack on closest/farthest target
    PredaceousPounceMove1 = 38026, // Helper->location, 2.0s cast, width 6 rect charge, visual (first jump)
    PredaceousPounceMove2 = 38027, // Helper->self, 3.0s cast, range 11 circle, visual
    PredaceousPounceMove3 = 38028, // Helper->location, 4.0s cast, width 6 rect charge, visual
    PredaceousPounceMove4 = 38029, // Helper->self, 5.0s cast, range 11 circle, visual
    PredaceousPounceMove5 = 38030, // Helper->location, 6.0s cast, width 6 rect charge, visual
    PredaceousPounceMove6 = 38031, // Helper->self, 7.0s cast, range 11 circle, visual
    PredaceousPounceMove7 = 38032, // Helper->location, 8.0s cast, width 6 rect charge, visual
    PredaceousPounceMove8 = 38033, // Helper->self, 9.0s cast, range 11 circle, visual
    PredaceousPounceMove9 = 38034, // Helper->location, 10.0s cast, width 6 rect charge, visual
    PredaceousPounceMove10 = 38035, // Helper->self, 11.0s cast, range 11 circle, visual
    PredaceousPounceMove11 = 39632, // Helper->location, 12.0s cast, width 6 rect charge, visual
    PredaceousPounceMove12 = 39633, // Helper->self, 13.0s cast, range 11 circle, visual
    PredaceousPounceJumpFirst = 39635, // CopyCat->location, 13.0s cast, single-target
    PredaceousPounceJumpRest = 38024, // CopyCat->location, no cast, single-target
    PredaceousPounceChargeAOEFirst = 39704, // Helper->location, 13.5s cast, width 6 rect charge
    PredaceousPounceImpactAOEFirst = 39709, // Helper->self, 14.0s cast, range 11 circle
    PredaceousPounceChargeAOERest = 39270, // Helper->location, 1.0s cast, width 6 rect charge
    PredaceousPounceImpactAOERest = 38025, // Helper->self, 1.5s cast, range 11 circle

    MouserEnrage = 39822, // Boss->self, 8.0s cast, single-target
}

public enum IconID : uint
{
    BiscuitMaker = 218, // player
    ElevateAndEviscerate = 538, // player
    GrimalkinGale = 376, // player
    Nailchipper = 244, // player
}

public enum TetherID : uint
{
    Soulshade = 102, // Soulshade->Boss
    Leap = 12, // Boss->LeapTarget
    RainingCats = 89, // player->Boss
}
