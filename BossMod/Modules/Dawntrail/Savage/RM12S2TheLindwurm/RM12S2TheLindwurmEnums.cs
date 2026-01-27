#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

public enum OID : uint
{
    Boss = 0x4B02, // R5.000, x1
    _Gen_Lindwurm = 0x4AFE, // R0.000, x2, Part type
    Helper = 0x233C, // R0.500, x30, Helper type
    Luzzelwurm = 0x4B03, // R2.100, x0 (spawn during fight), lil snake
    Lindschrat = 0x4B04, // R5.000, x0 (spawn during fight), clone
    Understudy = 0x4B0A, // R1.000, x0 (spawn during fight), transforms into copy of player
    LindschratSplit = 0x4B29, // R7.500, x0 (spawn during fight), plays clone split animation
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
    _Spell_ScaldingWaves = 46309, // Helper->self, no cast, range 50 ?-degree cone
    _Weaponskill_ManaBurst = 46310, // Boss/Lindschrat->self, no cast, single-target
    _Weaponskill_ManaBurst1 = 46311, // Helper->location, no cast, range 20 circle
    _Weaponskill_HeavySlam = 46312, // Lindschrat->location, no cast, range 5 circle
    _Weaponskill_Grotesquerie = 46313, // Lindschrat->self, no cast, single-target
    _Weaponskill_Grotesquerie1 = 46314, // Helper->player, no cast, single-target
    _Weaponskill_HemorrhagicProjection = 46315, // Helper->self, no cast, range 50 ?-degree cone
    _Weaponskill_UnmitigatedImpact = 46320, // Helper->self, no cast, range 60 circle
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
    _Gen_Tether_chn_x6rc_fr_fan01x = 367, // Lindschrat->player
    _Gen_Tether_chn_x6rc_fr_tgae01x = 368, // Lindschrat->player
    _Gen_Tether_chn_x6rc_fr_share01x = 369, // Lindschrat->player
    _Gen_Tether_chn_tergetfix1f = 373, // 4B0A/4B04/Boss->player
    _Gen_Tether_chn_teke01h = 374, // Boss->player
}
