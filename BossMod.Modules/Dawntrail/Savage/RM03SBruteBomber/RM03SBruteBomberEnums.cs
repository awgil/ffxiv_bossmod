namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

public enum OID : uint
{
    Boss = 0x42C6, // R5.016-6.270, x1
    Helper = 0x233C, // R0.500, x35 (spawn during fight), Helper type
    LitFuse = 0x42C7, // R1.200, x8
    LariatComboHelper = 0x42C9, // R1.000, x4
    Refbot = 0x42C8, // R3.360, x0 (spawn during fight)
    BruteDistortion = 0x42CB, // R5.016, x0 (spawn during fight), clone with chains
    TagTeamHelper = 0x43F5, // R0.100, x0 (spawn during fight), chain hand?..
    SinisterSpark = 0x42CA, // R1.000, x0 (spawn during fight), Helper type
}

public enum AID : uint
{
    AutoAttack = 39554, // Boss->player, no cast, single-target
    Teleport = 37876, // Boss->location, no cast, single-target
    BrutalImpact = 37925, // Boss->self, 5.0s cast, range 60 circle, raidwide (first hit)
    BrutalImpactAOE = 37926, // Boss->self, no cast, range 60 circle, raidwide (subsequent hits)
    KnuckleSandwich = 37923, // Boss->players, 5.0s cast, range 6 circle, shared tankbuster (first hit)
    KnuckleSandwichAOE = 37924, // Boss->players, no cast, range 6 circle, shared tankbuster (subsequent hits)
    DopingDraught1 = 37895, // Boss->self, 4.0s cast, single-target, visual (drink first beer)
    DopingDraught2 = 37927, // Boss->self, 4.0s cast, single-target, visual (drink second beer)
    DopingDraught3 = 39320, // Boss->self, 4.0s cast, single-target, visual (drink third beer)

    OctupleLariatOut = 37848, // Boss->self, 6.2+0.8s cast, single-target, visual (out+spread)
    OctupleLariatOutAOE = 37864, // Helper->self, 7.0s cast, range 10 circle
    OctupleLariatIn = 37849, // Boss->self, 6.2+0.8s cast, single-target, visual (in+spread)
    OctupleLariatInAOE = 37865, // Helper->self, 7.0s cast, range 10-60 donut
    QuadrupleLariatOut = 37850, // Boss->self, 6.2+0.8s cast, single-target, visual (out+pairs)
    QuadrupleLariatOutAOE = 37866, // Helper->self, 7.0s cast, range 10 circle
    QuadrupleLariatIn = 37851, // Boss->self, 6.2+0.8s cast, single-target, visual (in+pairs)
    QuadrupleLariatInAOE = 37867, // Helper->self, 7.0s cast, range 10-60 donut
    BlazingLariatSpread = 37852, // Helper->self, no cast, range 40 45-degree cone
    BlazingLariatPair = 37853, // Helper->self, no cast, range 40 20-degree cone

    OctoboomDiveProximity = 37854, // Boss->location, 7.2+0.8s cast, single-target, visual (proximity+spread)
    OctoboomDiveProximityAOE = 37868, // Helper->self, 8.0s cast, range 60 circle with ? falloff
    OctoboomDiveKnockback = 37855, // Boss->location, 7.2+0.8s cast, single-target, visual (knockback+spread)
    OctoboomDiveKnockbackAOE = 37869, // Helper->self, 8.0s cast, range 60 circle, knockback 25
    QuadroboomDiveProximity = 37856, // Boss->location, 7.2+0.8s cast, single-target, visual (proximity+pairs)
    QuadroboomDiveProximityAOE = 37877, // Helper->self, 8.0s cast, range 60 circle with ? falloff
    QuadroboomDiveKnockback = 37857, // Boss->location, 7.2+0.8s cast, single-target, visual (knockback+pairs)
    QuadroboomDiveKnockbackAOE = 37878, // Helper->self, 8.0s cast, range 60 circle, knockback 25
    DiveboomSpread = 37858, // Helper->players, no cast, range 5 circle
    DiveboomPair = 37859, // Helper->players, no cast, range 5 circle

    BarbarousBarrage = 37883, // Boss->self, 4.0s cast, single-target, visual (knockback towers)
    BarbarousBarrageExplosion4 = 38542, // Helper->self, no cast, range 4 circle, 4-man tower, knockback 23
    BarbarousBarrageExplosion2 = 37884, // Helper->self, no cast, range 4 circle, 2-man tower, knockback 19
    BarbarousBarrageExplosion8 = 38543, // Helper->self, no cast, range 4 circle, 8-man tower, knockback 15
    BarbarousBarrageUnmitigatedExplosion = 37885, // Helper->self, no cast, range 60 circle, failed tower
    BarbarousBarrageMurderousMist = 37886, // Boss->self, 6.0s cast, range 40 270-degree cone
    BarbarousBarrageLariatComboFirstRR = 39656, // Boss->location, 4.9+1.2s cast, single-target
    BarbarousBarrageLariatComboFirstRL = 39657, // Boss->location, 4.9+1.2s cast, single-target
    BarbarousBarrageLariatComboFirstLL = 39658, // Boss->location, 4.9+1.2s cast, single-target
    BarbarousBarrageLariatComboFirstLR = 39659, // Boss->location, 4.9+1.2s cast, single-target
    BarbarousBarrageLariatComboSecondRR = 39660, // Boss->location, 3.0s cast, single-target
    BarbarousBarrageLariatComboSecondRL = 39661, // Boss->location, 3.0s cast, single-target
    BarbarousBarrageLariatComboSecondLL = 39662, // Boss->location, 3.0s cast, single-target
    BarbarousBarrageLariatComboSecondLR = 39663, // Boss->location, 3.0s cast, single-target
    BarbarousBarrageLariatComboFirstRAOE = 39664, // Helper->self, 6.1s cast, range 70 width 34 rect
    BarbarousBarrageLariatComboFirstLAOE = 39665, // Helper->self, 6.1s cast, range 70 width 34 rect
    BarbarousBarrageLariatComboSecondRAOE = 39666, // Helper->self, 3.1s cast, range 50 width 34 rect
    BarbarousBarrageLariatComboSecondLAOE = 39667, // Helper->self, 3.1s cast, range 50 width 34 rect

    TagTeam = 37863, // Boss->self, 4.0s cast, single-target, visual (clones with chains)
    ChainDeathmatch = 37861, // Boss->self, 7.0s cast, single-target, visual (chains)
    ChainDeathmatchVisual = 37862, // BruteDistortion->self, 7.0s cast, single-target, visual (chains)
    TagTeamLariatComboVisual1 = 39741, // LariatComboHelper->self, 4.9s cast, single-target, visual (???)
    TagTeamLariatComboVisual2 = 39809, // LariatComboHelper->self, 4.9s cast, single-target, visual (???)
    TagTeamLariatComboVisualEnd = 39742, // LariatComboHelper->self, no cast, single-target, visual (mechanic end)
    TagTeamLariatComboFirstR = 39724, // BruteDistortion->location, 4.9+1.2s cast, single-target, visual (charge)
    TagTeamLariatComboFirstL = 39726, // BruteDistortion->location, 4.9+1.2s cast, single-target, visual (charge)
    TagTeamLariatComboFirstRAOE = 39732, // Helper->self, 6.1s cast, range 70 width 34 rect
    TagTeamLariatComboFirstLAOE = 39733, // Helper->self, 6.1s cast, range 70 width 34 rect
    TagTeamLariatComboSecondR = 39728, // BruteDistortion->location, 3.0s cast, single-target, visual (back charge)
    TagTeamLariatComboSecondL = 39730, // BruteDistortion->location, 3.0s cast, single-target, visual (back charge)
    TagTeamLariatComboSecondRAOE = 39734, // Helper->self, 3.1s cast, range 50 width 34 rect
    TagTeamLariatComboSecondLAOE = 39735, // Helper->self, 3.1s cast, range 50 width 34 rect
    PunishingChain = 39886, // Helper->player, no cast, single-target, kill on chain deathmatch failure

    FusesOfFury = 37887, // Boss->self, 4.0s cast, single-target, visual (bombs + chains)
    FusesOfFuryMurderousMist = 39895, // Boss->self, 8.0s cast, range 40 270-degree cone
    FusesOfFuryLariatComboVisual1 = 39986, // LariatComboHelper->self, 6.9s cast, single-target
    FusesOfFuryLariatComboVisual2 = 39984, // LariatComboHelper->self, 6.9s cast, single-target
    FusesOfFuryLariatComboVisualEnd = 39985, // LariatComboHelper->self, no cast, single-target
    FusesOfFuryLariatComboFirstR = 39896, // BruteDistortion->location, 6.9+1.2s cast, single-target
    FusesOfFuryLariatComboFirstL = 39898, // BruteDistortion->location, 6.9+1.2s cast, single-target
    FusesOfFuryLariatComboFirstRAOE = 39917, // Helper->self, 8.1s cast, range 70 width 34 rect
    FusesOfFuryLariatComboFirstLAOE = 39918, // Helper->self, 8.1s cast, range 70 width 34 rect
    FusesOfFuryLariatComboSecondR = 39913, // BruteDistortion->location, 3.0s cast, single-target
    FusesOfFuryLariatComboSecondL = 39915, // BruteDistortion->location, 3.0s cast, single-target
    FusesOfFuryLariatComboSecondRAOE = 39919, // Helper->self, 3.1s cast, range 50 width 34 rect
    FusesOfFuryLariatComboSecondLAOE = 39920, // Helper->self, 3.1s cast, range 50 width 34 rect

    FinalFusedown = 37894, // Boss->self, 4.0s cast, single-target, visual (bombs)
    FinalFusedownSelfDestructShort = 37889, // LitFuse->self, 5.0s cast, range 8 circle
    FinalFusedownSelfDestructLong = 37890, // LitFuse->self, 10.0s cast, range 8 circle
    FinalFusedownExplosionShort = 37892, // Helper->location, no cast, range 6 circle
    FinalFusedownExplosionLong = 37893, // Helper->location, no cast, range 6 circle

    Fusefield = 37870, // Boss->self, 4.0s cast, single-target, visual (long/short fuses)
    BombarianFlame = 37871, // Boss->self, 3.0s cast, single-target, visual (start moving fuses)
    FusefieldVisualStart = 37872, // Boss->self, no cast, single-target, visual (activate center voidzone)
    ManaExplosion = 37875, // SinisterSpark->self, no cast, range 60 circle, raidwide with magic vuln
    ManaExplosionKill = 39527, // SinisterSpark->players, no cast, range 60 circle, kill player who touches spark without buff, raidwide with magic vuln on others
    Fuseflare = 37874, // Helper->self, no cast, range 60 circle, wipe if spark reaches the target
    FusefieldVisualEnd = 37873, // Boss->self, no cast, single-target, visual (finish mechanic)

    OctoboomBombarianSpecial = 38738, // Boss->self, 6.0s cast, single-target, visual (multiple raidwides + spread)
    QuadroboomBombarianSpecial = 37898, // Boss->self, 6.0s cast, single-target, visual (multiple raidwides + pairs)
    BombarianSpecialRaidwide1 = 37899, // Helper->self, 7.0s cast, range 60 circle
    BombarianSpecialRaidwide2 = 37900, // Helper->self, 8.5s cast, range 60 circle
    BombarianSpecialRaidwide3 = 37901, // Helper->self, 10.5s cast, range 60 circle
    BombarianSpecialRaidwide4 = 37902, // Helper->self, 12.7s cast, range 60 circle
    BombarianSpecialRaidwide5 = 37903, // Helper->self, 14.0s cast, range 60 circle
    BombarianSpecialOut = 37904, // Helper->self, 15.8s cast, range 10 circle
    BombarianSpecialIn = 37905, // Helper->self, 18.3s cast, range 6-40 donut
    BombarianSpecialRaidwide6 = 37906, // Helper->self, 20.0s cast, range 60 circle
    BombarianSpecialAOE = 37907, // Helper->self, 26.5s cast, range 8 circle
    BombarianSpecialKnockback = 37908, // Helper->self, 26.5s cast, range 60 circle, knockback 10
    BombariboomSpread = 38739, // Helper->player, no cast, range 5 circle
    BombariboomPair = 38740, // Helper->players, no cast, range 5 circle

    FuseOrFoe = 37891, // Boss->self, 4.0s cast, single-target, visual (pantokrator)
    InfernalSpinFirstCW = 37916, // Boss->self, 5.5+0.5s cast, range 40 60-degree cone
    InfernalSpinFirstCCW = 37917, // Boss->self, 5.5+0.5s cast, range 40 60-degree cone
    InfernalSpinFirstAOE = 39855, // Helper->self, 6.0s cast, range 40 60-degree cone
    InfernalSpinRest = 37918, // Boss->self, no cast, range 40 60-degree cone
    InfernalSpinRestAOE = 39856, // Helper->self, 0.5s cast, range 40 60-degree cone
    ExplosiveRain11 = 37910, // Helper->self, 6.0s cast, range 8 circle
    ExplosiveRain12 = 37911, // Helper->self, 10.0s cast, range 8-16 donut
    ExplosiveRain13 = 37912, // Helper->self, 14.0s cast, range 16-24 donut
    ExplosiveRain21 = 37913, // Helper->self, 6.0s cast, range 8 circle
    ExplosiveRain22 = 37914, // Helper->self, 10.0s cast, range 8-16 donut
    ExplosiveRain23 = 37915, // Helper->self, 14.0s cast, range 16-24 donut

    SpecialBombarianSpecial = 37931, // Boss->self, 6.0s cast, single-target, visual (multiple raidwides + enrage)
    SpecialBombarianSpecialRaidwide1 = 37932, // Helper->self, 7.0s cast, range 60 circle
    SpecialBombarianSpecialRaidwide2 = 37933, // Helper->self, 8.5s cast, range 60 circle
    SpecialBombarianSpecialRaidwide3 = 37934, // Helper->self, 10.5s cast, range 60 circle
    SpecialBombarianSpecialRaidwide4 = 37935, // Helper->self, 12.7s cast, range 60 circle
    SpecialBombarianSpecialRaidwide5 = 37936, // Helper->self, 14.0s cast, range 60 circle
    SpecialBombarianSpecialOut = 37937, // Helper->self, 15.8s cast, range 10 circle
    SpecialBombarianSpecialIn = 37938, // Helper->self, 18.3s cast, range 6-40 donut
    SpecialBombarianSpecialRaidwide6 = 37939, // Helper->self, 20.0s cast, range 60 circle
    SpecialBombarianSpecialEnrage = 37940, // Helper->self, 26.5s cast, single-target, visual
    SpecialBombarianSpecialEnrageAOE = 37941, // Helper->self, 26.5s cast, range 60 circle, enrage
}

public enum SID : uint
{
    FinalFusedownFutureExplosionShort = 4024, // none->player, extra=0x2E3
    FinalFusedownFutureExplosionLong = 4025, // none->player, extra=0x2E4
    FinalFusedownImminentExplosionShort = 4026, // none->player, extra=0x2E5
    FinalFusedownImminentExplosionLong = 4027, // none->player, extra=0x2E6
    FinalFusedownFutureSelfDestructShort = 4015, // none->LitFuse, extra=0x2DF
    FinalFusedownFutureSelfDestructLong = 4016, // none->LitFuse, extra=0x2E0
    FinalFusedownImminentSelfDestructShort = 4017, // none->LitFuse, extra=0x2E1
    FinalFusedownImminentSelfDestructLong = 4018, // none->LitFuse, extra=0x313
    Bombarium = 4020, // none->player, extra=0x0
}

public enum IconID : uint
{
    KnuckleSandwich = 467, // player
    InfernalSpinCW = 546, // Boss
    InfernalSpinCCW = 547, // Boss
}

public enum TetherID : uint
{
    ChainDeathmatch = 274, // TagTeamHelper->player
    Fusefield = 273, // SinisterSpark->TagTeamHelper
    FusesOfFury = 275, // player->Boss
}
