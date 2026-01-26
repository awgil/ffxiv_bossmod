#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

public enum OID : uint
{
    Boss = 0x4B02, // R5.000, x1
    _Gen_Lindwurm = 0x4AFE, // R0.000, x2, Part type
    Helper = 0x233C, // R0.500, x30, Helper type
    _Gen_Luzzelwurm = 0x4B03, // R2.100, x0 (spawn during fight), snake guy
    _Gen_Lindschrat1 = 0x4B04, // R5.000, x0 (spawn during fight), clone
    _Gen_Lindschrat = 0x4B29, // R7.500, x0 (spawn during fight), plays clone split animation
    _Gen_Understudy = 0x4B0A, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 46367, // Boss->player, no cast, single-target
    _Weaponskill_ArcadiaAflame = 46376, // Boss->self, 5.0s cast, range 60 circle
    CloneJump = 46297, // Boss/4B04->location, no cast, single-target
    _Weaponskill_Replication = 46296, // Boss->self, 3.0s cast, single-target
    _Weaponskill_WingedScourge = 46298, // 4B04->self, 3.0s cast, single-target
    _Weaponskill_MightyMagic = 46303, // 4B04->self, 3.0s cast, single-target
    _Weaponskill_TopTierSlam = 46301, // 4B04->self, 3.0s cast, single-target
    _Weaponskill_TopTierSlam1 = 46302, // 4B04->location, no cast, range 5 circle
    _Weaponskill_WingedScourge1 = 46300, // Helper->self, 4.0s cast, range 50 30-degree cone
    _Weaponskill_MightyMagic1 = 46304, // Helper->players, no cast, range 5 circle
    _Weaponskill_SnakingKick = 46375, // Boss->self, 5.0s cast, range 40 180-degree cone
    _Weaponskill_WingedScourge2 = 46299, // 4B04->self, 3.0s cast, single-target

    DoubleSobatBoss1 = 46368, // Boss->self, 5.0s cast, single-target
    DoubleSobatBuster1 = 46369, // Boss->self, no cast, range 40 180-degree cone
    DoubleSobatBuster2 = 46370, // Boss->self, no cast, range 40 180-degree cone
    DoubleSobatBuster3 = 46371, // Boss->self, no cast, range 40 180-degree cone
    DoubleSobatBuster4 = 46372, // Boss->self, no cast, range 40 180-degree cone
    DoubleSobatRepeat = 46373, // Helper->self, 4.6s cast, range 40 180-degree cone

    _Weaponskill_EsotericFinisher = 46374, // Helper->players, no cast, range 10 circle
    _Weaponskill_Staging = 46305, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ = 46306, // Helper->self, no cast, single-target
    _Weaponskill_FirefallSplash = 46307, // Boss->self, 5.7s cast, single-target
    _Weaponskill_FirefallSplash1 = 46308, // Boss->location, no cast, range 5 circle
    _Spell_ScaldingWaves = 46309, // Helper->self, no cast, range 50 ?-degree cone
    _Weaponskill_ManaBurst = 46310, // Boss/4B04->self, no cast, single-target
    _Weaponskill_ManaBurst1 = 46311, // Helper->location, no cast, range 20 circle
    _Weaponskill_HeavySlam = 46312, // 4B04->location, no cast, range 5 circle
    _Weaponskill_Grotesquerie = 46313, // 4B04->self, no cast, single-target
    _Weaponskill_Grotesquerie1 = 46314, // Helper->player, no cast, single-target
    _Weaponskill_HemorrhagicProjection = 46315, // Helper->self, no cast, range 50 ?-degree cone
    _Weaponskill_UnmitigatedImpact = 46320, // Helper->self, no cast, range 60 circle
}

public enum SID : uint
{
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_FireResistanceDownII = 2937, // 4B04->player, extra=0x0
    _Gen_DamageDown = 2911, // Helper/Boss->player, extra=0x0
    _Gen_DarkResistanceDownII = 3323, // Helper->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Boss/Helper/4B04->player, extra=0x0
    _Gen_SustainedDamage = 4149, // Helper->player, extra=0x2/0x1
}

public enum IconID : uint
{
    _Gen_Icon_sharelaser2tank5sec_c0k1 = 598, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_tergetfix1f = 373, // 4B0A/4B04/Boss->player
    _Gen_Tether_chn_x6rc_fr_fan01x = 367, // 4B04->player
    _Gen_Tether_chn_x6rc_fr_share01x = 369, // 4B04->player
    _Gen_Tether_chn_teke01h = 374, // Boss->player
    _Gen_Tether_chn_x6rc_fr_tgae01x = 368, // 4B04->player
}
