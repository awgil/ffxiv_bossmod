namespace BossMod.Dawntrail.Ultimate.FRU;

public enum OID : uint
{
    BossP1 = 0x459B, // R5.004, x1
    Helper = 0x233C, // R0.500, x24, Helper type
    FatebreakersImage = 0x459C, // R5.004, x15
    FatebreakersImageHelper = 0x45B0, // R1.800, x8
    HaloOfFlame = 0x459D, // R1.000, x0 (spawn during fight)
    HaloOfLevin = 0x459E, // R1.000, x0 (spawn during fight)

    BossP2 = 0x459F, // R6.125, x0 (spawn during fight)
    OraclesReflection = 0x45A0, // R6.125, x0 (spawn during fight)
    FrozenMirror = 0x45A1, // R1.000, x0 (spawn during fight)
    HolyLight = 0x45A2, // R2.000, x0 (spawn during fight) (light rampant orb)
    SinboundHolyVoidzone = 0x1EBC4F, // R0.500, x0 (spawn during fight), EventObj type

    CrystalOfLight = 0x45A3, // R1.500, x0 (spawn during fight)
    CrystalOfDarkness = 0x45A4, // R1.500, x0 (spawn during fight)
    IceVeil = 0x45A5, // R5.000, x0 (spawn during fight)
    Gaia = 0x45A6, // R1.000, x0 (spawn during fight)
    //_Gen_CrystalOfLight = 0x464F, // R1.000, x0 (spawn during fight)
    HiemalRayVoidzone = 0x1EA1CB, // R0.500, x0 (spawn during fight), EventObj type
    //_Gen_EternalIceFragment = 0x1EBBF8, // R0.500, x0 (spawn during fight), EventObj type

    BossP3 = 0x45A7, // R7.040, x0 (spawn during fight)
    DelightsHourglass = 0x45A8, // R1.000, x0 (spawn during fight)
    ApocalypseLight = 0x1EB0FF, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    // P1
    AutoAttackP1 = 40116, // BossP1->player, no cast, single-target
    TeleportP1 = 40173, // BossP1->location, no cast, ???

    PowderMarkTrail = 40168, // BossP1->player, 5.0s cast, single-target
    BurnMark = 40169, // Helper->self, no cast, range 10 circle, spread on tankbuster target and closest player
    BurnishedGlory = 40170, // BossP1->self, 5.0s cast, range 40 circle, raidwide with bleed

    CyclonicBreakBossStack = 40144, // BossP1->self, 6.5s cast, single-target, visual (proteans + pairs)
    CyclonicBreakBossSpread = 40148, // BossP1->self, 6.5s cast, single-target, visual (proteans + spread)
    CyclonicBreakImageStack = 40329, // FatebreakersImage->self, 7.0s cast, single-target, visual (proteans + pairs)
    CyclonicBreakImageSpread = 40330, // FatebreakersImage->self, 7.0s cast, single-target, visual (proteans + spread)
    CyclonicBreakAOEFirst = 40145, // Helper->self, no cast, range 60 ?-degree cone
    CyclonicBreakAOERest = 40146, // Helper->self, no cast, range 50 ?-degree cone
    CyclonicBreakSinsmoke = 40147, // Helper->players, no cast, range 6 circle, 2-man stack
    CyclonicBreakSinsmite = 40149, // Helper->players, no cast, range 6 circle spread

    UtopianSkyStack = 40154, // BossP1->self, 4.0s cast, single-target, visual (3 lines + stack)
    UtopianSkySpread = 40155, // BossP1->self, 4.0s cast, single-target, visual (3 lines + spread)
    BlastingZoneAOE = 40157, // FatebreakersImage->self, no cast, range 50 width 16 rect
    BlastingZone = 40158, // FatebreakersImageHelper->self, 10.0s cast, single-target, visual (line aoe)
    SinboundFire = 40159, // Helper->players, no cast, range 6 circle 4-man stack
    SinboundThunder = 40160, // Helper->players, no cast, range 5 circle spread

    TurnOfHeavensFire = 40150, // FatebreakersImage->self, 7.0s cast, single-target, visual (large fire)
    TurnOfHeavensLightning = 40151, // FatebreakersImage->self, 7.0s cast, single-target, visual (large lightning)
    TurnOfHeavensBurntStrikeFire = 40161, // FatebreakersImage->self, 8.0s cast, range 80 width 10 rect, followed by knockback
    TurnOfHeavensBlastburn = 40162, // Helper->self, 10.0s cast, range 80 width 50 rect, knockback 15 to the side
    TurnOfHeavensBurntStrikeLightning = 40163, // FatebreakersImage->self, 8.0s cast, range 80 width 10 rect, followed by wide aoe
    TurnOfHeavensBurnout = 40164, // Helper->self, 9.7s cast, range 80 width 20 rect
    BrightfireSmall = 40152, // HaloOfFlame/HaloOfLevin->self, 8.0s cast, range 5 circle
    BrightfireLarge = 40153, // HaloOfFlame/HaloOfLevin->self, 8.0s cast, range 10 circle
    BoundOfFaith = 40165, // FatebreakersImage->self, 10.0s cast, single-target, visual (tethers for stacks)
    FloatingFetters = 40171, // FatebreakersImage/BossP1->player, no cast, single-target, apply floating on stack targets
    TurnOfHeavensSolemnCharge = 40166, // FatebreakersImage->player, no cast, single-target, visual (stack)
    BoundOfFaithSinsmoke = 40167, // Helper->players, no cast, range 6 circle, 4-man stack
    FatedBurnMark = 40331, // Helper->location, no cast, range 100 circle, raidwide if tether target dies

    FallOfFaithFire = 40137, // Boss/FatebreakersImage->self, 9.0s cast, single-target, visual (fire tether -> shared cone)
    FallOfFaithLightning = 40140, // Boss/FatebreakersImage->self, 9.0s cast, single-target, visual (lightning tether -> proteans)
    FallOfFaithApply = 40172, // Helper->player, no cast, single-target, ??? (attract to current position, right before applying fetters)
    FallOfFaithSolemnChargeFire = 40138, // FatebreakersImage/BossP1->player, no cast, single-target, visual (tether resolve)
    FallOfFaithSolemnChargeLightning = 40141, // Boss/FatebreakersImage->player, no cast, single-target, visual (tether resolve)
    FallOfFaithSinsmite = 40142, // Helper->player, no cast, single-target, primary target hit
    FallOfFaithBowShock = 40143, // Helper->self, no cast, range 60 120-degree cone, protean
    FallOfFaithSinblaze = 40156, // Helper->self, no cast, range 60 90-degree cone, 4-man stack

    ExplosionBurntStrikeFire = 40129, // BossP1->self, 6.5s cast, range 80 width 10 rect, followed by knockback
    ExplosionBlastburn = 40130, // Helper->self, 8.5s cast, range 80 width 50 rect, knockback 15 to the side
    ExplosionBurntStrikeLightning = 40133, // BossP1->self, 6.5s cast, range 80 width 10 rect, followed by wide aoe
    ExplosionBurnout = 40134, // Helper->self, 8.2s cast, range 80 width 20 rect
    Explosion11 = 40135, // Helper->self, 10.5s cast, range 4 circle, 1-man tower
    Explosion12 = 40131, // Helper->self, 10.5s cast, range 4 circle, 1-man tower
    Explosion21 = 40125, // Helper->self, 10.5s cast, range 4 circle, 2-man tower
    Explosion22 = 40122, // Helper->self, 10.5s cast, range 4 circle, 2-man tower
    Explosion31 = 40126, // Helper->self, 10.5s cast, range 4 circle, 3-man tower
    Explosion32 = 40123, // Helper->self, 10.5s cast, range 4 circle, 3-man tower
    Explosion41 = 40124, // Helper->self, 10.5s cast, range 4 circle, 4-man tower
    Explosion42 = 40127, // Helper->self, 10.5s cast, range 4 circle, 4-man tower
    UnmitigatedExplosion1 = 40132, // Helper->self, no cast, range 40 circle, raidwide if not enough soakers
    UnmitigatedExplosion2 = 40136, // Helper->self, no cast, range 40 circle, raidwide if not enough soakers (what's the difference?)

    EnrageP1 = 40128, // BossP1->self, 10.0s cast, range 40 circle, enrage

    // P2
    AutoAttackP2 = 40176, // BossP2->player, no cast, single-target
    TeleportP2 = 40175, // BossP2/OraclesReflection->location, no cast, single-target
    QuadrupleSlapFirst = 40191, // BossP2->player, 5.0s cast, single-target, tankbuster with vuln
    QuadrupleSlapSecond = 40192, // BossP2->player, 2.5s cast, single-target, tankbuster second hit

    MirrorImage = 40180, // BossP2->self, 3.0s cast, single-target, visual (show clones)
    DiamondDust = 40197, // BossP2->self, 5.0s cast, range 40 circle, raidwide
    AxeKick = 40202, // OraclesReflection->self, 6.0s cast, range 16 circle
    ScytheKick = 40203, // OraclesReflection/BossP2->self, 6.0s cast, range 4-20 donut
    HouseOfLight = 40206, // Helper->self, no cast, range 60 30-degree cone, baited on 4 closest
    FrigidStone = 40199, // Helper->location, no cast, range 5 circle, baited on icons
    IcicleImpact = 40198, // Helper->location, 9.0s cast, range 10 circle, circles at cardinals/intercardinals
    FrigidNeedleCircle = 40200, // Helper->self, 5.0s cast, range 5 circle
    FrigidNeedleCross = 40201, // Helper->self, 5.0s cast, range 40 width 5 cross
    HeavenlyStrike = 40207, // BossP2->self, no cast, range 40 circle, knockback 12
    SinboundHoly = 40208, // OraclesReflection->self, 5.0s cast, single-target, visual (multi-hit light party stacks)
    SinboundHolyAOE = 40209, // Helper->location, no cast, range 6 circle, 4-man stack on healers
    ShiningArmor = 40185, // Helper->self, no cast, range 40 circle, gaze (stun + damage down)
    FrostArmor = 40184, // Helper->self, no cast, single-target, visual (thin ice)
    TwinStillnessFirst = 40193, // OraclesReflection->self, 3.5s cast, range 30 270-degree cone (front)
    TwinStillnessSecond = 40196, // OraclesReflection->self, no cast, range 40 90-degree cone (back)
    TwinSilenceFirst = 40194, // OraclesReflection->self, 3.5s cast, range 40 90-degree cone (back)
    TwinSilenceSecond = 40195, // OraclesReflection->self, no cast, range 30 270-degree cone (front)

    HallowedRay = 40210, // BossP2->self, 5.0s cast, single-target, visual (line stack)
    HallowedRayAOE = 40211, // BossP2->self, no cast, range 65 width 6 rect, line stack

    MirrorMirror = 40179, // BossP2->self, 3.0s cast, single-target, visual (mechanic start)
    ReflectedScytheKickBlue = 40204, // FrozenMirror->self, no cast, range 4-20 donut
    ReflectedScytheKickRed = 40205, // FrozenMirror->self, 10.0s cast, range 4-20 donut
    BanishStack = 40220, // BossP2->self, 5.0s cast, single-target, visual (pairs)
    BanishStackAOE = 40222, // Helper->location, no cast, range 5 circle
    BanishSpread = 40221, // BossP2->self, 5.0s cast, single-target, visual (spread)
    BanishSpreadAOE = 40223, // Helper->location, no cast, range 5 circle

    LightRampant = 40212, // BossP2->self, 5.0s cast, range 40 circle, raidwide + mechanic start
    LuminousHammer = 40218, // Helper->player, no cast, range 6 circle, baited puddle
    BrightHunger = 40213, // Helper->location, no cast, range 4 circle, tower
    InescapableIllumination = 40214, // Helper->location, no cast, range 40 circle, raidwide when player with lightsteeped dies?
    RefulgentFate = 40215, // Helper->location, no cast, range 40 circle, raidwide with damage down when tether breaks incorrectly (eg a target dies)
    Lightsteep = 40216, // Helper->players, no cast, range 40 circle, raidwide on reaching 5+ stacks of lightsteeped
    HolyLightBurst = 40219, // HolyLight->self, 5.0s cast, range 11 circle
    PowerfulLight = 40217, // Helper->players, no cast, range 5 circle stack
    HouseOfLightBoss = 40189, // BossP2->self, 5.0s cast, single-target, visual (proteans)
    HouseOfLightBossAOE = 40188, // Helper->self, no cast, range 60 ?-degree cone

    AbsoluteZero = 40224, // BossP2->self, 10.0s cast, single-target, visual (raidwide + intermission start)
    AbsoluteZeroAOE = 40333, // Helper->self, no cast, range 100 circle, raidwide
    AbsoluteZeroEnrage = 40334, // Helper->self, no cast, range 100 circle, enrage if boss is at >20%
    SwellingFrost = 40225, // Helper->self, no cast, range 40 circle, knockback 10 + freeze
    EndlessIceAge = 40259, // IceVeil->self, 40.0s cast, range 100 circle, enrage
    SinboundBlizzard = 40258, // CrystalOfDarkness->self, 3.3s cast, single-target, visual (baited cone)
    SinboundBlizzardAOE = 40262, // Helper->self, 3.3s cast, range 50 ?-degree cone
    HiemalStorm = 40255, // CrystalOfLight->self, no cast, single-target, visual (baited puddle)
    HiemalStormAOE = 40256, // Helper->location, 3.3s cast, range 7 circle
    HiemalRay = 40257, // Helper->player, no cast, range 4 circle

    // P3
    Junction = 40226, // Helper->self, no cast, range 40 circle, raidwide
    HellsJudgment = 40265, // BossP3->self, 4.0s cast, range 100 circle, maxhp-1 raidwide
    AutoAttackP3 = 40264, // BossP3->player, no cast, single-target
    TeleportP3 = 40117, // BossP3->location, no cast, single-target
    ShockwavePulsar = 40282, // BossP3->self, 5.0s cast, range 40 circle, raidwide
    BlackHalo = 40290, // BossP3->self/player, 5.0s cast, range 60 ?-degree cone, shared tankbuster

    UltimateRelativity = 40266, // BossP3->self, 10.0s cast, range 100 circle, raidwide + mechanic start
    UltimateRelativitySpeed = 40293, // BossP3->self, 5.5s cast, single-target, visual (change hourglass resolve speed)
    UltimateRelativityQuicken = 40294, // Helper->DelightsHourglass, no cast, single-target, visual (resolve early)
    UltimateRelativitySlow = 40295, // Helper->DelightsHourglass, no cast, single-target, visual (resolve late)
    UltimateRelativityDarkFire = 40276, // Helper->players, no cast, range 8 circle spread
    UltimateRelativityUnholyDarkness = 40277, // Helper->players, no cast, range 6 circle stack
    UltimateRelativitySinboundMeltdown = 40291, // DelightsHourglass->self, 4.0s cast, single-target, visual (baited laser)
    UltimateRelativitySinboundMeltdownAOEFirst = 40235, // Helper->self, no cast, range 60 width 5 rect
    UltimateRelativitySinboundMeltdownAOERest = 40292, // Helper->self, no cast, range 50 width 5 rect
    UltimateRelativityDarkBlizzard = 40279, // Helper->player, no cast, range ?-12 donut
    UltimateRelativityShadoweye = 40278, // Helper->self, no cast, gaze
    DarkEruption = 40274, // Helper->player, no cast, range 6 circle
    DarkWater = 40271, // Helper->players, no cast, range 6 circle, 4-man stack
    ShellCrusher = 40286, // BossP3->self, 3.0s cast, single-target, visual (stack)
    ShellCrusherAOE = 40287, // BossP3->players, no cast, range 6 circle stack

    SpellInWaitingRefrain = 40269, // BossP3->self, 2.0s cast, single-target, visual (next dark water is staggered)
    ApocalypseDarkWater = 40270, // BossP3->self, 5.0s cast, single-target, visual (apply staggered stacks)
    ApocalypseDarkWaterVisual = 40272, // Helper->player, no cast, single-target, visual (player stacks)
    Apocalypse = 40296, // BossP3->self, 4.0s cast, single-target, visual (exploding lights)
    ApocalypseAOE = 40297, // Helper->self, no cast, range 9 circle
    SpiritTaker = 40288, // BossP3->self, 3.0s cast, single-target, visual (jump on random target)
    SpiritTakerAOE = 40289, // BossP3->player, no cast, range 5 circle, jump on random target, knockback 40 on everyone else in aoe
    ApocalypseDarkEruption = 40273, // BossP3->self, 4.0+1.0s cast, single-target, visual (spread)
    DarkestDance = 40181, // BossP3->self, 5.0s cast, single-target, visual (baited tankbuster)
    DarkestDanceBait = 40182, // BossP3->players, no cast, range 8 circle, baited tankbuster
    DarkestDanceKnockback = 40183, // BossP3->self, no cast, range 40 circle, knockback 21

    MemorysEnd = 40300, // BossP3->self, 10.0s cast, single-target, visual (enrage)
    MemorysEndAOE = 40336, // Helper->self, no cast, range 100 circle, enrage
}

public enum SID : uint
{
    PowderMarkTrail = 4166, // BossP1->player, extra=0x0
    Concealed = 1621, // none->FatebreakersImage, extra=0x1
    Prey = 1051, // none->player, extra=0x0
    FatedBurnMark = 4165, // none->player, extra=0x0
    FloatingFetters = 2304, // FatebreakersImage/BossP1->player, extra=0xC8
    MarkOfMortality = 4372, // Helper->player, extra=0x1
    ChainsOfEverlastingLight = 4157, // none->player, extra=0x0, light rampant first tether
    CurseOfEverlastingLight = 4158, // none->player, extra=0x0, light rampant second tether
    WeightOfLight = 4159, // none->player, extra=0x0, light rampant stack
    Lightsteeped = 2257, // Helper/HolyLight->player, extra=0x1/0x2/0x3/0x4/0x5
    SpellInWaitingUnholyDarkness = 2454, // none->player, extra=0x0, stack
    SpellInWaitingDarkFire = 2455, // none->player, extra=0x0, large spread
    SpellInWaitingShadoweye = 2456, // none->player, extra=0x0, delayed gaze
    SpellInWaitingDarkEruption = 2460, // none->player, extra=0x0, delayed small spread
    SpellInWaitingDarkWater = 2461, // none->player, extra=0x0, delayed stack
    SpellInWaitingDarkBlizzard = 2462, // none->player, extra=0x0, donut
    SpellInWaitingReturn = 2464, // none->player, extra=0x0
    DelightsHourglassRotation = 2970, // none->DelightsHourglass, extra=0x10D (ccw)/0x15C (cw)
    Return = 2452, // none->player, extra=0x0
    Stun = 4163, // none->player, extra=0x0
    //SpellInWaitingRefrain = 4373, // BossP3->BossP3, extra=0x0
    //_Gen_ = 2458, // none->player, extra=0x0
}

public enum IconID : uint
{
    PowderMarkTrail = 218, // player->self
    FrigidStone = 345, // player->self
    HallowedRay = 525, // BossP2->player
    LuminousHammer = 375, // player->self
    BlackHalo = 259, // player->self
    DelayedDarkWater = 62, // player->self
    DarkWater = 184, // player->self
    DarkEruption = 139, // player->self
}

public enum TetherID : uint
{
    Fire = 249, // Boss/FatebreakersImage->player
    Lightning = 287, // Boss/FatebreakersImage->player
    LightRampantChains = 110, // player->player
    LightRampantCurse = 111, // player->player
    IntermissionGaia = 112, // Gaia->IceVeil
    IntermissionCrystal = 8, // CrystalOfLight/CrystalOfDarkness->IceVeil
    HiemalRay = 84, // CrystalOfLight->player
    UltimateRelativitySlow = 133, // DelightsHourglass->BossP3
    UltimateRelativityQuicken = 134, // DelightsHourglass->BossP3
}
