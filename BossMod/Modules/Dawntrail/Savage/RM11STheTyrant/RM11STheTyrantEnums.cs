namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

public enum OID : uint
{
    Boss = 0x4AEC, // R5.000, x1
    Helper = 0x233C, // R0.500, x25-36, Helper type
    TheTyrant = 0x4AEA, // R3.000, x3-4
    Comet = 0x4AED, // R2.160, x8
    Axe = 0x4AF0, // R1.000, x2
    Scythe = 0x4AF1, // R1.000, x2
    Sword = 0x4AF2, // R1.000, x2
    Maelstrom = 0x4AEF, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 46085, // Boss->player, no cast, single-target
    _Weaponskill_CrownOfArcadia = 46086, // Boss->self, 5.0s cast, range 60 circle
    RawSteelTrophyAxe = 46114, // Boss->self, 2.0+4.0s cast, single-target
    RawSteelTrophyScythe = 46115, // Boss->self, 2.0+4.0s cast, single-target
    _Weaponskill_RawSteel = 46090, // Boss->self, no cast, single-target
    _Weaponskill_RawSteel1 = 46091, // Boss->players, no cast, range 6 circle
    _Weaponskill_Impact = 46092, // Helper->players, no cast, range 6 circle
    _Ability_ = 46084, // Boss->location, no cast, single-target

    _Weaponskill_RawSteel4 = 46095, // Helper->self, no cast, range 60 90-degree cone, tankbuster
    _Weaponskill_HeavyHitter = 46096, // Helper->self, no cast, range 60 45-degree cone, party stack

    _Weaponskill_TrophyWeapons = 46102, // Boss->self, 3.0s cast, single-target
    _Weaponskill_AssaultEvolved = 46103, // Boss->self, 6.0s cast, single-target
    _Weaponskill_AssaultEvolved1 = 46405, // Boss->location, no cast, single-target
    _Weaponskill_SharpTaste = 46109, // Helper->self, no cast, range 60 width 6 rect
    _Weaponskill_AssaultEvolved2 = 46106, // Helper->self, 2.0s cast, range 40 width 10 cross
    _Weaponskill_AssaultEvolved3 = 46403, // Boss->location, no cast, single-target
    _Weaponskill_HeavyWeight = 46107, // Helper->players, no cast, range 6 circle
    _Weaponskill_AssaultEvolved4 = 46104, // Helper->self, 2.0s cast, range 8 circle
    _Weaponskill_AssaultEvolved5 = 46404, // Boss->location, no cast, single-target
    _Weaponskill_SweepingVictory = 46108, // Helper->self, no cast, range 60 30-degree cone
    _Weaponskill_AssaultEvolved6 = 46105, // Helper->self, 2.0s cast, range 5-60 donut
    _Spell_VoidStardust = 46098, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_Cometite = 46099, // Helper->location, 3.0s cast, range 6 circle
    _Spell_Comet = 46100, // Helper->players, 5.0s cast, range 6 circle
    _Spell_CrushingComet = 46101, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_DanceOfDominationTrophy = 47035, // Boss->self, 2.0+4.0s cast, single-target
    _Weaponskill_DanceOfDomination = 46110, // Boss->self, no cast, single-target
    _Weaponskill_DanceOfDomination1 = 46111, // Helper->self, 0.5s cast, range 60 circle
    _Weaponskill_DanceOfDomination2 = 46113, // Helper->self, no cast, range 60 circle
    _Weaponskill_DanceOfDomination3 = 47082, // Helper->self, no cast, range 60 circle
    _Spell_EyeOfTheHurricane = 46116, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_Explosion = 46112, // Helper->self, 5.0s cast, range 60 width 10 rect
    _Weaponskill_Explosion1 = 47036, // Helper->self, 5.5s cast, range 60 width 10 rect, visual only? doesn't deal damage
    _Weaponskill_RawSteel2 = 46093, // Boss->self, no cast, single-target
    _Weaponskill_RawSteel3 = 46094, // Boss->self, no cast, single-target
    _Spell_Charybdistopia = 46117, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_UltimateTrophyWeapons = 47085, // Boss->self, 3.0s cast, single-target
    _Weaponskill_AssaultApex = 47086, // Boss->self, 5.0s cast, single-target
    _Spell_PowerfulGust = 46119, // Helper->self, no cast, range 60 90-degree cone
    _Ability_ImmortalReign = 46120, // Boss->self, 3.0+1.0s cast, single-target
    _Spell_Charybdis = 46118, // Helper->player, no cast, single-target
    _Weaponskill_OneAndOnly = 46122, // Helper->self, 9.0s cast, range 60 circle
    _Weaponskill_OneAndOnly1 = 46121, // Boss->self, 6.0+2.0s cast, single-target

    _Weaponskill_GreatWallOfFire = 46123, // Boss->self, 5.0s cast, single-target
    _Weaponskill_GreatWallOfFire1 = 46124, // Boss->self, no cast, range 60 width 6 rect
    _Weaponskill_GreatWallOfFire2 = 46125, // Helper->self, no cast, single-target
    _Weaponskill_Explosion2 = 46126, // Helper->self, 3.0s cast, range 60 width 6 rect
    _Spell_OrbitalOmen = 46130, // Boss->self, 5.0s cast, single-target
    _Weaponskill_FireAndFury = 46127, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_OrbitalOmen1 = 46131, // Helper->self, 6.0s cast, range 60 width 10 rect
    _Weaponskill_FireAndFury1 = 46129, // Helper->self, 5.0s cast, range 60 ?-degree cone
    _Weaponskill_FireAndFury2 = 46128, // Helper->self, 5.0s cast, range 60 ?-degree cone
    _Spell_Meteorain = 46132, // Boss->self, 5.0s cast, single-target
    _Weaponskill_FearsomeFireball = 46137, // Boss->self, 5.0s cast, single-target
    _Weaponskill_FearsomeFireball1 = 46138, // Boss->self, no cast, range 60 width 6 rect
    _Ability_CosmicKiss = 46133, // _Gen_Comet->self, no cast, range 4 circle
    _Spell_ForegoneFatality = 46134, // _Gen_TheTyrant->player/_Gen_Comet, no cast, single-target
    _Ability_UnmitigatedExplosion = 46135, // _Gen_Comet->self, no cast, range 60 circle
    _Ability_Explosion = 46136, // _Gen_Comet->self, 3.0s cast, range 8 circle
    _Weaponskill_TripleTyrannhilation = 46139, // Boss->self, no cast, single-target
    _Weaponskill_TripleTyrannhilation1 = 46140, // Boss->self, 7.0+1.0s cast, single-target
    _Weaponskill_Shockwave = 46141, // Helper->self, no cast, range 60 circle
    _Ability_1 = 46142, // _Gen_Comet->self, no cast, single-target
    _Weaponskill_ = 46176, // Boss->self, no cast, single-target
    _Weaponskill_Flatliner = 46143, // Boss->self, 4.0+2.0s cast, single-target
    _Weaponskill_Flatliner1 = 47760, // Helper->self, 6.0s cast, range 60 circle
    _Spell_MajesticMeteor = 46144, // Boss->self, 5.0s cast, single-target
    _Spell_Explosion = 46148, // Helper->self, 10.0s cast, range 4 circle
    _Spell_UnmitigatedExplosion = 46149, // Helper->self, no cast, range 60 circle
    _Spell_MajesticMeteor1 = 46145, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_FireBreath = 46150, // Boss->self, 8.0+1.0s cast, single-target
    _Weaponskill_FireBreath1 = 46151, // Helper->self, no cast, range 60 width 6 rect
    _Spell_MajesticMeteowrath = 46147, // Helper->self, no cast, range 60 width 10 rect
    _Spell_MajesticMeteorain = 46146, // Helper->self, no cast, range 60 width 10 rect
    _Spell_MassiveMeteor = 46152, // Boss->self, 5.0s cast, single-target
    _Spell_MassiveMeteor1 = 46153, // Helper->players, no cast, range 6 circle
    _Weaponskill_ArcadionAvalanche = 46160, // Boss->self, 6.0+9.5s cast, single-target
    _Weaponskill_ArcadionAvalanche1 = 46161, // Helper->self, 15.5s cast, range 40 width 40 rect
    _Weaponskill_ArcadionAvalanche2 = 46156, // Boss->self, 6.0+9.5s cast, single-target
    _Weaponskill_ArcadionAvalanche3 = 46157, // Helper->self, 15.5s cast, range 40 width 40 rect
}

public enum SID : uint
{
    _Gen_PhysicalVulnerabilityUp = 2940, // Boss/Helper/_Gen_Comet->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper/Boss/_Gen_TheTyrant->player/_Gen_Comet, extra=0x0
    _Gen_DamageDown = 2911, // Helper/_Gen_Comet->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_ = 3913, // none->_Gen_1/_Gen_/Boss/_Gen_2, extra=0x3EB/0x3EC/0x0/0x432
    _Gen_FireResistanceDownII = 2937, // none->player, extra=0x0
    _Gen_DirectionalDisregard = 3808, // none->_Gen_TheTyrant/Boss, extra=0x0
    _Gen_1 = 4435, // none->_Gen_TheTyrant/Boss, extra=0xA
    _Gen_SustainedDamage = 4149, // none->player, extra=0x2/0x4/0x3/0x5/0x1
}

public enum IconID : uint
{
    _Gen_Icon_target_ae_s5f = 139, // player->self
    _Gen_Icon_com_share3t = 161, // player->self
    _Gen_Icon_lockon8_t0w = 244, // player->self
    _Gen_Icon_share_laser_5sec_0t = 525, // Boss->player
    _Gen_Icon_com_share4a1 = 305, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_teke_tank01k1 = 356, // _Gen_TheTyrant->_Gen_Comet/player
    _Gen_Tether_chn_arrow01f = 57, // _Gen_TheTyrant->player
    _Gen_Tether_chn_tergetfix2k1 = 249, // _Gen_TheTyrant->player
}
