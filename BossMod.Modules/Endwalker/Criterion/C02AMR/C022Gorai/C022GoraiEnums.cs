namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

public enum OID : uint
{
    NBoss = 0x3F61, // R7.000, x1
    NBallOfLevin = 0x3F62, // R0.690-2.300, spawn during fight - lightning orb

    SBoss = 0x3F63, // R7.000, x1
    SBallOfLevin = 0x3F64, // R0.690-2.300, spawn during fight - lightning orb

    Helper = 0x233C, // R0.500, spawn during fight
    FlameAndSulphurFlame = 0x1EB893, // R0.500, EventObj type, spawn during fight
    FlameAndSulphurRock = 0x1EB894, // R0.500, EventObj type, spawn during fight
    OrangeTower1 = 0x1EB895, // R0.500, EventObj type, spawn during fight
    OrangeTower2 = 0x1EB906, // R0.500, EventObj type, spawn during fight
    BlueTower1 = 0x1EB896, // R0.500, EventObj type, spawn during fight
    BlueTower2 = 0x1EB897, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 34050, // *Boss->player, no cast, single-target
    Teleport = 34003, // *Boss->location, no cast, single-target

    Unenlightenment = 34100, // *Boss->self, 5.0s cast, single-target, visual (raidwide)
    NUnenlightenmentAOE = 34101, // Helper->self, no cast, range 60 circle, raidwide
    SUnenlightenmentAOE = 34133, // Helper->self, no cast, range 60 circle, raidwide

    SealOfScurryingSparks = 34051, // *Boss->self, 4.0s cast, single-target, visual (2-man stacks)
    SealOfScurryingSparksAOE = 34052, // Helper->player, no cast, single-target, visual (applies stack debuff)
    NGreaterBallOfFire = 34053, // Helper->players, no cast, range 6 circle, 2-man stack
    NGreatBallOfFire = 34054, // Helper->players, no cast, range 10 circle spread
    SGreaterBallOfFire = 34105, // Helper->players, no cast, range 6 circle, 2-man stack
    SGreatBallOfFire = 34106, // Helper->players, no cast, range 10 circle spread
    FlameAndSulphur = 34056, // *Boss->self, 3.0s cast, single-target, visual (create rocks)
    BrazenBalladExpanding = 34057, // *Boss->self, 5.0s cast, single-target, visual (expanding aoes)
    BrazenBalladSplitting = 34058, // *Boss->self, 5.0s cast, single-target, visual (splitting aoes)
    NFireSpreadExpand = 34059, // Helper->self, no cast, range 46 width 10 rect
    NFireSpreadSplit = 34060, // Helper->self, no cast, range 46 width 5 rect
    NFallingRockExpand = 34062, // Helper->self, no cast, range 11 circle
    NFallingRockSplit = 34063, // Helper->self, no cast, range 6-16 donut
    SFireSpreadExpand = 34108, // Helper->self, no cast, range 46 width 10 rect
    SFireSpreadSplit = 34109, // Helper->self, no cast, range 46 width 5 rect
    SFallingRockExpand = 34111, // Helper->self, no cast, range 11 circle
    SFallingRockSplit = 34112, // Helper->self, no cast, range 6-16 donut

    ImpurePurgation = 34095, // *Boss->self, 3.6s cast, single-target, visual (proteans)
    NImpurePurgationBait = 34096, // Helper->self, no cast, range 60 45-degree cone (baited)
    NImpurePurgationAOE = 34097, // Helper->self, 2.0s cast, range 60 45-degree cone (casted)
    SImpurePurgationBait = 34130, // Helper->self, no cast, range 60 45-degree cone (baited)
    SImpurePurgationAOE = 34131, // Helper->self, 2.0s cast, range 60 45-degree cone (casted)

    Thundercall = 34080, // *Boss->self, 3.0s cast, single-target, visual (create lightning orbs)
    HumbleHammer = 34084, // *Boss->self, 5.0s cast, single-target, visual (reduce size)
    NHumbleHammerAOE = 34085, // Helper->players, 5.0s cast, range 3 circle
    SHumbleHammerAOE = 34123, // Helper->players, 5.0s cast, range 3 circle
    ShockVisual = 34081, // *BallOfLevin->self, 7.0s cast, range 8 circle, visual
    NShockSmall = 34082, // NBallOfLevin->self, no cast, range 8 circle, small aoe
    NShockLarge = 34083, // NBallOfLevin->self, no cast, range 8+10 circle, large aoe
    SShockSmall = 34121, // SBallOfLevin->self, no cast, range 8 circle, small aoe
    SShockLarge = 34122, // SBallOfLevin->self, no cast, range 8+10 circle, large aoe
    Flintlock = 34086, // *Boss->self, no cast, single-target, visual (wild charge)
    NFlintlockAOE = 34087, // Helper->self, no cast, range 50 width 8 rect wild charge
    SFlintlockAOE = 34124, // Helper->self, no cast, range 50 width 8 rect wild charge

    TorchingTorment = 34098, // *Boss->player, 5.0s cast, single-target, visual (tankbuster)
    NTorchingTormentAOE = 34099, // Helper->player, no cast, range 6 circle tankbuster
    STorchingTormentAOE = 34132, // Helper->player, no cast, range 6 circle tankbuster

    RousingReincarnation = 34066, // *Boss->self, 5.0s cast, single-target, visual (towers)
    NRousingReincarnationAOE = 34067, // Helper->player, no cast, single-target, damage + debuffs
    SRousingReincarnationAOE = 34180, // Helper->player, no cast, single-target, damage + debuffs
    MalformedPrayer = 34072, // *Boss->self, 4.0s cast, single-target, visual (towers)
    PointedPurgation = 34077, // *Boss->self, 8.0s cast, single-target, visual (proteans on tethers)
    PointedPurgationRest = 34078, // *Boss->self, no cast, single-target, visual (proteans on tethers)
    NPointedPurgationAOE = 34079, // Helper->self, no cast, range 60 ?-degree cone
    SPointedPurgationAOE = 34120, // Helper->self, no cast, range 60 ?-degree cone
    NBurstOrange = 34073, // Helper->self, no cast, range 4 circle tower
    NDramaticBurstOrange = 34074, // Helper->self, no cast, range 60 circle unsoaked tower
    NBurstBlue = 34075, // Helper->self, no cast, range 4 circle tower
    NDramaticBurstBlue = 34076, // Helper->self, no cast, range 60 circle unsoaked tower
    SBurstOrange = 34116, // Helper->self, no cast, range 4 circle tower
    SDramaticBurstOrange = 34117, // Helper->self, no cast, range 60 circle unsoaked tower
    SBurstBlue = 34118, // Helper->self, no cast, range 4 circle tower
    SDramaticBurstBlue = 34119, // Helper->self, no cast, range 60 circle unsoaked tower
    //_Spell_OdderFodder = 34071, // Helper->self, no cast, range 60 circle

    CloudToGround = 34088, // *Boss->self, 6.2s cast, single-target, visual (exaflares)
    NCloudToGroundAOEFirst = 34089, // Helper->self, 7.0s cast, range 6 circle
    NCloudToGroundAOERest = 34090, // Helper->self, no cast, range 6 circle
    SCloudToGroundAOEFirst = 34125, // Helper->self, 7.0s cast, range 6 circle
    SCloudToGroundAOERest = 34126, // Helper->self, no cast, range 6 circle

    FightingSpirits = 34091, // *Boss->self, 5.0s cast, single-target, visual (limit cut)
    NFightingSpiritsAOE = 34092, // Helper->self, 6.2s cast, range 30 circle, knockback 16
    SFightingSpiritsAOE = 34127, // Helper->self, 6.2s cast, range 30 circle, knockback 16
    NWorldlyPursuitJump = 34093, // NBoss->location, no cast, single-target
    NWorldlyPursuitAOE = 34061, // Helper->self, 0.6s cast, range 60 width 20 cross
    SWorldlyPursuitJump = 34128, // SBoss->location, no cast, single-target
    SWorldlyPursuitAOE = 34110, // Helper->self, 0.6s cast, range 60 width 20 cross

    MalformedReincarnation = 34068, // *Boss->self, 5.0s cast, single-target, visual (last towers)
    NMalformedReincarnationAOE = 34069, // Helper->player, no cast, single-target, damage + debuffs
    SMalformedReincarnationAOE = 34181, // Helper->player, no cast, single-target, damage + debuffs
    FlickeringFlame = 34064, // *Boss->self, 3.0s cast, single-target, visual (criss-cross)
    NFireSpreadCross = 34065, // Helper->self, 3.5s cast, range 46 width 5 rect
    SFireSpreadCross = 34113, // Helper->self, 3.5s cast, range 46 width 5 rect

    LivingHell = 34102, // *Boss->self, 10.0s cast, single-target, enrage
}

public enum SID : uint
{
    LiveBrazier = 3607, // none->player, extra=0x0, stack
    LiveCandle = 3608, // none->player, extra=0x0, spread
    RodentialRebirth1 = 3597, // none->player, extra=0x0 (orange 1)
    RodentialRebirth2 = 3598, // none->player, extra=0x0 (orange 2)
    RodentialRebirth3 = 3599, // none->player, extra=0x0 (orange 3)
    RodentialRebirth4 = 3600, // none->player, extra=0x0 (orange 4)
    OdderIncarnation1 = 3601, // none->player, extra=0x0 (blue 1)
    OdderIncarnation2 = 3602, // none->player, extra=0x0 (blue 2)
    OdderIncarnation3 = 3603, // none->player, extra=0x0 (blue 3)
    OdderIncarnation4 = 3604, // none->player, extra=0x0 (blue 4)
    SquirrellyPrayer = 3605, // none->player, extra=0x0 (orange)
    OdderPrayer = 3606, // none->player, extra=0x0 (blue)
}

public enum IconID : uint
{
    HumbleHammer = 27, // player
    TorchingTorment = 344, // player
    Order1 = 336, // player
    Order2 = 337, // player
    Order3 = 338, // player
    Order4 = 339, // player
}

public enum TetherID : uint
{
    RousingReincarnation = 248, // player->NBoss
    PointedPurgation = 89, // player->NBoss
}
