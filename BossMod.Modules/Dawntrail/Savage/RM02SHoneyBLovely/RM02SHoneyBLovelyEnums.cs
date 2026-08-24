namespace BossMod.Dawntrail.Savage.RM02SHoneyBLovely;

public enum OID : uint
{
    Boss = 0x422D, // R5.004, x1
    Helper = 0x233C, // R0.500, x28, Helper type
    Groupbee = 0x422E, // R1.500, x0 (spawn during fight) - charging bee
    Sweetheart = 0x422F, // R1.000, x0 (spawn during fight) - small heart moving in straight line
    PoisonCloud = 0x4231, // R1.000, x0 (spawn during fight)
    PoisonStingVoidzone = 0x1EBAA1, // R0.500, x0 (spawn during fight), EventObj type
    LoveMeTenderTower1 = 0x1EBAA2, // R0.500, x0 (spawn during fight), EventObj type
    LoveMeTenderTower2 = 0x1EBAA3, // R0.500, x0 (spawn during fight), EventObj type (this one plays eobjanim on enter/exit)
}

public enum AID : uint
{
    AutoAttack = 37320, // Boss->player, no cast, single-target
    Teleport = 37219, // Boss->location, no cast, single-target
    CallMeHoney = 37251, // Boss->self, 5.0s cast, range 60 circle, raidwide
    StingingSlash = 37275, // Boss->self, 4.0+1.0s cast, single-target, visual (tankbusters on two tanks)
    StingingSlashAOE = 37277, // Helper->self, no cast, range 50 90-degree cone
    KillerSting = 37276, // Boss->self, 4.0+1.0s cast, single-target, visual (shared tankbuster)
    KillerStingAOE = 37278, // Helper->players, no cast, range 6 circle

    SplashOfVenom = 37252, // Boss->self, 5.0s cast, single-target, visual (prepare spreads)
    SplashOfVenomAOE = 37257, // Helper->player, no cast, range 6 circle spread
    DropOfVenom = 37253, // Boss->self, 5.0s cast, single-target, visual (prepare pairs)
    DropOfVenomAOE = 37258, // Helper->players, no cast, range 6 circle 2-man stack
    HoneyBeeline = 37254, // Boss->self, 5.5+0.7s cast, single-target
    HoneyBeelineAOE = 39625, // Helper->self, 6.2s cast, range 60 width 14 rect
    TemptingTwist = 37255, // Boss->self, 5.5+0.7s cast, single-target
    TemptingTwistAOE = 39626, // Helper->self, 6.2s cast, range ?-30 donut
    PoisonCloudAppear = 37229, // PoisonCloud->location, no cast, single-target
    PoisonCloudSplinter = 37256, // PoisonCloud->self, 3.3s cast, range 8 circle

    HoneyBLiveBeat1 = 39972, // Boss->self, 2.0+6.3s cast, single-target, visual (mechanic start + raidwide)
    HoneyBLiveBeat1AOE = 39551, // Helper->self, no cast, range 60 circle, raidwide
    HoneyBLiveFail = 37262, // Helper->Boss, no cast, single-target, heal + damage up if someone gets 4 hearts
    CenterstageCombo = 37292, // Boss->self, 5.0+1.0s cast, single-target, visual (in->cross->out)
    OuterstageCombo = 37293, // Boss->self, 5.0+1.0s cast, single-target, visual (out->cross->in)
    StageComboIn = 37294, // Boss->self, no cast, single-target, visual
    StageComboCross = 37295, // Boss->self, no cast, single-target, visual
    StageComboOut = 37296, // Boss->self, no cast, single-target, visual
    LacerationOut = 37297, // Helper->self, no cast, range 7 circle
    LacerationCross = 37298, // Helper->self, no cast, range 30 width 14 cross
    LacerationCone = 37299, // Helper->self, no cast, range 30 45-degree cone
    LacerationIn = 37300, // Helper->self, no cast, range 7-30 donut
    SweetheartTouch = 37285, // Sweetheart->player, no cast, single-target, visual (extra heart if touched)
    LoveMeTender = 37279, // Boss->self, 4.0s cast, single-target, visual (heart towers)
    Fracture = 37283, // Helper->self, 8.0s cast, range 4 circle tower
    FractureBigBurst = 37284, // Helper->self, no cast, range 60 circle, tower fail
    Loveseeker = 39805, // Boss->self, 3.0+1.0s cast, single-target, visual (out)
    LoveseekerAOE = 39806, // Helper->self, 4.0s cast, range 10 circle
    Heartsick = 37280, // Helper->players, no cast, range 6 circle, stack that gives out 4 hearts
    HoneyBFinale = 37263, // Boss->self, 5.0s cast, range 60 circle, raidwide + beat end

    AlarmPheromones = 37245, // Boss->self, 3.0s cast, single-target, visual (mechanic start)
    BlindingLoveBait = 37272, // Groupbee->self, 6.3+0.7s cast, single-target
    BlindingLoveBaitAOE = 39629, // Helper->self, 7.0s cast, range 50 width 8 rect, damage down + knockback 15
    BlindingLoveCharge1 = 37264, // Groupbee->location, 5.3+0.7s cast, width 10 rect charge, visual (? difference between 1 and 2)
    BlindingLoveCharge1AOE = 39627, // Helper->self, 6.0s cast, range 45 width 10 rect
    BlindingLoveCharge2 = 37265, // Groupbee->location, 5.3+0.7s cast, width 10 rect charge, visual
    BlindingLoveCharge2AOE = 39628, // Helper->self, 6.0s cast, range 45 width 10 rect
    PoisonSting = 37268, // Boss->self, 4.7+1.6s cast, single-target, visual (dropped voidzone)
    PoisonStingRest = 37269, // Boss->self, no cast, single-target, visual (second+)
    PoisonStingAOE = 37267, // Helper->player, 6.0s cast, range 6 circle, drops voidzone
    BeeSting = 37288, // Boss->self, 4.0+1.0s cast, single-target, visual (stack)
    BeeStingAOE = 37289, // Helper->players, 5.0s cast, range 6 circle 4-man stack

    HoneyBLiveBeat2 = 39973, // Boss->self, 2.0+6.3s cast, single-target, visual (mechanic start + raidwide)
    HoneyBLiveBeat2AOE = 39975, // Helper->self, no cast, range 60 circle, raidwide
    SpreadLove = 39688, // Boss->self, 5.0s cast, single-target, visual (prepare spreads)
    SpreadLoveAOE = 39694, // Helper->player, no cast, range 6 circle
    DropOfLove = 39689, // Boss->self, 5.0s cast, single-target, visual (prepare pairs)
    DropOfLoveAOE = 39695, // Helper->players, no cast, range 6 circle
    HeartStruck = 37287, // Helper->location, 3.0s cast, range 6 circle puddle
    Heartsore = 37281, // Helper->player, no cast, range 6 circle spread
    HoneyBeelineBeat = 39692, // Boss->self, 5.5+0.7s cast, single-target
    HoneyBeelineBeatAOE = 39696, // Helper->self, 6.2s cast, range 60 width 14 rect
    TemptingTwistBeat = 39693, // Boss->self, 5.5+0.7s cast, single-target
    TemptingTwistBeatAOE = 39697, // Helper->self, 6.2s cast, range ?-30 donut
    SweetheartAppear = 39690, // Sweetheart->location, no cast, single-target
    SweetheartSplinter = 39691, // Sweetheart->self, 3.3s cast, range 8 circle

    HoneyBLiveBeat3 = 39974, // Boss->self, 2.0+6.3s cast, single-target, visual (mechanic start + raidwide)
    HoneyBLiveBeat3AOE = 39976, // Helper->self, no cast, range 60 circle, raidwide
    HoneyBLiveBeat3BigBurst = 37302, // Helper->player, no cast, range 14 circle spread

    RottenHeart = 37290, // Boss->self, 1.0+4.0s cast, single-target, visual (mechanic start)
    RottenHeartAOE = 37330, // Helper->self, no cast, range 60 circle, raidwide
    RottenHeartBigBurst = 37291, // Helper->self, no cast, range 60 circle, raidwide on touch

    SheerHeartAttack = 37303, // Boss->self, 10.0s cast, range 60 circle, visual (enrage)
    SheerHeartAttackHeal = 38659, // Helper->Boss, no cast, single-target, heal for gaining 4 hearts due to enrage
    SheerHeartAttackHealHoneyBFinale = 37304, // Boss->self, no cast, range 60 circle, enrage
}

public enum SID : uint
{
    Hearts0 = 3922, // none->player, extra=0x2DA
    Hearts1 = 3923, // none->player, extra=0x2DB
    Hearts2 = 3924, // none->player, extra=0x2DC
    Hearts3 = 3925, // none->player, extra=0x2DD
    Hearts4 = 3926, // none->player, extra=0x2DE
    PoisonNPop = 3934, // none->player, extra=0x0
    BeelovedVenomA = 3932, // none->player, extra=0x0
    BeelovedVenomB = 3933, // none->player, extra=0x0
}

public enum IconID : uint
{
    StingingSlash = 471, // player
    KillerSting = 259, // player
    Heartsick = 517, // player
    Heartsore = 515, // player
    PoisonSting = 234, // player
    BeeSting = 161, // player
}

public enum TetherID : uint
{
    RottenHeart = 224, // player->player
}
