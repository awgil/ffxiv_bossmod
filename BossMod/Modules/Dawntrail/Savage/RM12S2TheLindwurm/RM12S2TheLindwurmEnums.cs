#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

public enum OID : uint
{
    Boss = 0x4B02, // R5.000, x1
    Helper = 0x233C, // R0.500, x30, Helper type
    Luzzelwurm = 0x4B03, // R2.100, x0 (spawn during fight), lil snake
    Lindschrat = 0x4B04, // R5.000, x0 (spawn during fight), clone
    Understudy = 0x4B0A, // R1.000, x0 (spawn during fight), transforms into copy of player
    LindschratSplit = 0x4B29, // R7.500, x0 (spawn during fight), plays clone split animation

    ManaSphereBlackHole = 0x4B05, // R0.600, x0 (spawn during fight)
    ManaSphereBlueSphere = 0x4B06, // R0.600, x0 (spawn during fight)
    ManaSphereGreenDonut = 0x4B07, // R0.600, x0 (spawn during fight)
    ManaSpherePurpleBowtie = 0x4B08, // R0.600, x0 (spawn during fight)
    ManaSphereOrangeBowtie = 0x4B09, // R0.600, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 46367, // Boss->player, no cast, single-target
    ArcadiaAflame = 46376, // Boss->self, 5.0s cast, range 60 circle
    SnakingKick = 46375, // Boss->self, 5.0s cast, range 40 180-degree cone
    Dash = 46297, // Boss/Lindschrat->location, no cast, single-target

    Replication = 46296, // Boss->self, 3.0s cast, single-target
    WingedScourgeCastVertical = 46298, // Lindschrat->self, 3.0s cast, single-target
    WingedScourgeCastHorizontal = 46299, // Lindschrat->self, 3.0s cast, single-target
    WingedScourge = 46300, // Helper->self, 4.0s cast, range 50 30-degree cone
    TopTierSlamCast = 46301, // Lindschrat->self, 3.0s cast, single-target
    TopTierSlamStack = 46302, // Lindschrat->location, no cast, range 5 circle
    MightyMagicCast = 46303, // Lindschrat->self, 3.0s cast, single-target
    MightyMagicSpread = 46304, // Helper->players, no cast, range 5 circle
    DoubleSobatBoss1 = 46368, // Boss->self, 5.0s cast, single-target
    DoubleSobatBuster1 = 46369, // Boss->self, no cast, range 40 180-degree cone
    DoubleSobatBuster2 = 46370, // Boss->self, no cast, range 40 180-degree cone
    DoubleSobatBuster3 = 46371, // Boss->self, no cast, range 40 180-degree cone
    DoubleSobatBuster4 = 46372, // Boss->self, no cast, range 40 180-degree cone
    DoubleSobatRepeat = 46373, // Helper->self, 4.6s cast, range 40 180-degree cone
    EsotericFinisher = 46374, // Helper->players, no cast, range 10 circle

    Staging = 46305, // Boss->self, 3.0s cast, single-target
    UnderstudyTransform = 46306, // Helper->self, no cast, single-target, just guessing what this does
    FirefallSplashCast = 46307, // Boss->self, 5.7s cast, single-target
    FirefallSplash = 46308, // Boss->location, no cast, range 5 circle
    ScaldingWaves = 46309, // Helper->self, no cast, range 50 10-degree cone
    ManaBurstVisual = 46310, // Boss/Lindschrat->self, no cast, single-target
    ManaBurstAOE = 46311, // Helper->location, no cast, range 20 circle
    HeavySlam = 46312, // Lindschrat->location, no cast, range 5 circle
    UnmitigatedImpact = 46320, // Helper->self, no cast, range 60 circle, Heavy Slam "tower" failure
    GrotesquerieVisual = 46313, // Lindschrat->self, no cast, single-target
    GrotesquerieHelper = 46314, // Helper->player, no cast, single-target
    HemorrhagicProjection = 46315, // Helper->self, no cast, range 50 50-degree cone

    Reenactment = 46316, // Boss->self, 3.0s cast, single-target
    FirefallSplashReplay = 46317, // Lindschrat->Understudy, no cast, range 5 circle
    ManaBurstReplayVisual = 46318, // Lindschrat->self, no cast, single-target
    ManaBurstReplay = 48099, // Helper->Understudy, no cast, range 20 circle
    NetherwrathNear = 46382, // Boss->self, 5.0s cast, single-target
    NetherwrathFar = 46383, // Boss->self, 5.0s cast, single-target
    TimelessSpite = 46384, // Helper->players, no cast, range 6 circle
    ScaldingWavesReplay = 47329, // Helper->self, no cast, range 50 10-degree cone
    GrotesquerieReplayVisual = 46321, // Lindschrat->self, no cast, single-target
    HemorrhagicProjectionReplay = 47394, // Helper->self, no cast, range 50 50-degree cone
    HeavySlamReplayVisual = 46319, // Lindschrat->Understudy, no cast, single-target
    HeavySlamReplay = 48733, // Helper->self, no cast, range 5 circle

    MutatingCells = 46341, // Boss->self, 3.0s cast, single-target
    MutatingCellsVisual = 46342, // Helper->player, no cast, single-target
    BloodMana = 46331, // Boss->self, 3.0s cast, single-target
    BlackHoleUnk = 48304, // _Gen_ManaSphere->self, no cast, single-target, not sure, some animation
    ManaSphereSpawn = 46333, // _Gen_ManaSphere4/_Gen_ManaSphere2/_Gen_ManaSphere1/_Gen_ManaSphere3->location, no cast, single-target
    BloodyBurst = 46334, // Helper->players, no cast, range 5 circle
    BlackHoleAbsorb = 46335, // _Gen_ManaSphere4/_Gen_ManaSphere2/_Gen_ManaSphere1/_Gen_ManaSphere3->_Gen_ManaSphere, no cast, single-target
    MutatingUnmitigatedExplosion = 46344, // Helper->player, no cast, single-target
    MutatingDramaticLysis = 46343, // Helper->player, no cast, single-target
    BloodWakening = 46336, // Boss->self, 3.0s cast, single-target
    BlackHoleActivate = 46332, // _Gen_ManaSphere->self, no cast, single-target, triggers after Blood Wakening and immediately before stored aoes
    LindwurmsWaterIII = 46337, // Helper->self, no cast, range 8 circle
    LindwurmsAeroIII = 46338, // Helper->self, no cast, range 5-60 donut
    StraightforwardThunderII = 46339, // Helper->self, no cast, range 40 120-degree cone
    SidewaysFireII = 46340, // Helper->self, no cast, range 40 120-degree cone
    NetherworldNear = 46379, // Boss->self, 5.0s cast, single-target
    NetherworldFar = 46380, // Boss->self, 5.0s cast, single-target
    WailingWave = 46381, // Helper->players, no cast, range 6 circle

    _Weaponskill_IdyllicDream = 46345, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_TwistedVision = 48098, // Boss->self, 4.0s cast, single-target
    _Weaponskill_PowerGusher = 46354, // Helper->self, 5.0s cast, range 60 90-degree cone
    _Weaponskill_PowerGusher1 = 46352, // Lindschrat->self, 5.0s cast, single-target
    _Weaponskill_SnakingKick = 46353, // Lindschrat->self, 5.0s cast, range 10 circle
    _Weaponskill_PowerGusher2 = 46351, // Lindschrat->self, 5.0s cast, single-target
    _Weaponskill_PowerGusher3 = 46356, // Lindschrat->self, no cast, single-target
    _Weaponskill_SnakingKick1 = 46357, // Lindschrat->self, no cast, single-target
    _Weaponskill_PowerGusher4 = 46355, // Lindschrat->self, no cast, single-target
    _Weaponskill_SnakingKick2 = 48789, // Helper->self, no cast, range 10 circle
    _Weaponskill_PowerGusher5 = 46358, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_LindwurmsMeteor = 46322, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_Downfall = 46323, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ArcadianArcanum = 46377, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ArcadianArcanum1 = 47577, // Helper->players, no cast, range 6 circle
    _Weaponskill_HeavySlam = 46361, // Lindschrat->location, no cast, range 5 circle
    _Weaponskill_ManaBurst = 46359, // Lindschrat->self, no cast, single-target
    _Weaponskill_ManaBurst1 = 46360, // Helper->players, no cast, range 20 circle
}

public enum SID : uint
{
    _Gen_FireResistanceDownII = 2937, // Lindschrat->player, extra=0x0
    _Gen_DarkResistanceDownII = 3323, // Helper->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Boss/Helper/Lindschrat->player, extra=0x0
    _Gen_SustainedDamage = 4149, // Helper->player, extra=0x2/0x1
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_ = 2056, // none->Understudy, extra=0x442
    _Gen_MutationA = 4769, // none->player, extra=0x0
    _Gen_MutatingCells = 4770, // none->player, extra=0x0
    _Gen_MutationB = 4771, // none->player, extra=0x0
    _Gen_DirectionalDisregard = 3808, // none->Boss, extra=0x0
    _Gen_1 = 4435, // none->Boss, extra=0x9
    _Gen_LightResistanceDownII = 4164, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_sharelaser2tank5sec_c0k1 = 598, // player->self
}

public enum TetherID : uint
{
    RepCone = 367, // Lindschrat->player
    RepSpread = 368, // Lindschrat->player
    RepStack = 369, // Lindschrat->player
    Fixed = 373, // 4B0A/4B04/Boss->player
    RepBoss = 374, // Boss->player
}
