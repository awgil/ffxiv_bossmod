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

    _Gen_ManaSphere = 0x4B05, // R0.600, x0 (spawn during fight)
    _Gen_ManaSphere1 = 0x4B06, // R0.600, x0 (spawn during fight)
    _Gen_ManaSphere2 = 0x4B07, // R0.600, x0 (spawn during fight)
    _Gen_ManaSphere3 = 0x4B08, // R0.600, x0 (spawn during fight)
    _Gen_ManaSphere4 = 0x4B09, // R0.600, x0 (spawn during fight)
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
    _Weaponskill_ = 46306, // Helper->self, no cast, single-target
    FirefallSplashCast = 46307, // Boss->self, 5.7s cast, single-target
    FirefallSplash = 46308, // Boss->location, no cast, range 5 circle
    _Spell_ScaldingWaves = 46309, // Helper->self, no cast, range 50 10-degree cone
    _Weaponskill_ManaBurst = 46310, // Boss/Lindschrat->self, no cast, single-target
    _Weaponskill_ManaBurst1 = 46311, // Helper->location, no cast, range 20 circle
    _Weaponskill_HeavySlam = 46312, // Lindschrat->location, no cast, range 5 circle
    _Weaponskill_Grotesquerie = 46313, // Lindschrat->self, no cast, single-target
    _Weaponskill_Grotesquerie1 = 46314, // Helper->player, no cast, single-target
    _Weaponskill_HemorrhagicProjection = 46315, // Helper->self, no cast, range 50 50-degree cone
    _Weaponskill_UnmitigatedImpact = 46320, // Helper->self, no cast, range 60 circle

    _Weaponskill_Reenactment = 46316, // Boss->self, 3.0s cast, single-target
    _Weaponskill_FirefallSplash = 46317, // Lindschrat->Understudy, no cast, range 5 circle
    _Weaponskill_ManaBurst2 = 46318, // Lindschrat->self, no cast, single-target
    _Weaponskill_NetherwrathNear = 46382, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ManaBurst3 = 48099, // Helper->Understudy, no cast, range 20 circle
    _Weaponskill_TimelessSpite = 46384, // Helper->players, no cast, range 6 circle
    _Spell_ScaldingWaves1 = 47329, // Helper->self, no cast, range 50 ?-degree cone
    _Weaponskill_Grotesquerie2 = 46321, // Lindschrat->self, no cast, single-target
    _Weaponskill_HemorrhagicProjection1 = 47394, // Helper->self, no cast, range 50 ?-degree cone
    _Weaponskill_HeavySlam1 = 46319, // Lindschrat->Understudy, no cast, single-target
    _Weaponskill_HeavySlam2 = 48733, // Helper->self, no cast, range 5 circle
    _Weaponskill_MutatingCells = 46341, // Boss->self, 3.0s cast, single-target
    _Weaponskill_NetherwrathFar = 46383, // Boss->self, 5.0s cast, single-target
    _Weaponskill_5 = 46342, // Helper->player, no cast, single-target
    _Weaponskill_BloodMana = 46331, // Boss->self, 3.0s cast, single-target
    _Spell_ = 48304, // _Gen_ManaSphere->self, no cast, single-target
    _Spell_1 = 46333, // _Gen_ManaSphere4/_Gen_ManaSphere2/_Gen_ManaSphere1/_Gen_ManaSphere3->location, no cast, single-target
    _Spell_UnmitigatedExplosion = 46344, // Helper->player, no cast, single-target
    _Spell_DramaticLysis = 46343, // Helper->player, no cast, single-target
    _Spell_2 = 46335, // _Gen_ManaSphere4/_Gen_ManaSphere2/_Gen_ManaSphere1/_Gen_ManaSphere3->_Gen_ManaSphere, no cast, single-target
    _Spell_BloodyBurst = 46334, // Helper->players, no cast, range 5 circle
    _Weaponskill_BloodWakening = 46336, // Boss->self, 3.0s cast, single-target
    _Spell_3 = 46332, // _Gen_ManaSphere->self, no cast, single-target
    _Spell_LindwurmsAeroIII = 46338, // Helper->self, no cast, range ?-60 donut
    _Spell_LindwurmsWaterIII = 46337, // Helper->self, no cast, range 8 circle
    _Spell_SidewaysFireII = 46340, // Helper->self, no cast, range 40 ?-degree cone
    _Spell_StraightforwardThunderII = 46339, // Helper->self, no cast, range 40 ?-degree cone
}

public enum SID : uint
{
    _Gen_FireResistanceDownII = 2937, // Lindschrat->player, extra=0x0
    _Gen_DarkResistanceDownII = 3323, // Helper->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Boss/Helper/Lindschrat->player, extra=0x0
    _Gen_SustainedDamage = 4149, // Helper->player, extra=0x2/0x1
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
