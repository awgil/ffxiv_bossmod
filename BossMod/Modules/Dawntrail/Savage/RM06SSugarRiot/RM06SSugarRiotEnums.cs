#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

public enum OID : uint
{
    Boss = 0x479F, // R4.000, x0-1
    Helper = 0x233C, // R0.500, x17-35, Helper type
    _Gen_ = 0x47B4, // R1.000, x0-1
    _Gen_HeavenBomb = 0x47A1, // R0.800, x0 (spawn during fight)
    _Gen_PaintBomb = 0x47A0, // R0.800, x0 (spawn during fight)
    _Gen_CandiedSuccubus = 0x47A5, // R2.500, x0 (spawn during fight)
    _Gen_MouthwateringMorbol = 0x47A4, // R4.550, x0 (spawn during fight)
    _Gen_Yan = 0x47A8, // R1.000, x0 (spawn during fight)
    _Gen_Mu = 0x47A7, // R1.800, x0 (spawn during fight)
    _Gen_StickyPudding = 0x47A6, // R1.200, x0 (spawn during fight)
    _Gen_GimmeCat = 0x47AB, // R1.650, x0 (spawn during fight)
    _Gen_Jabberwock = 0x47A9, // R3.000, x0 (spawn during fight)
    _Gen_FeatherRay = 0x47AA, // R1.600, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 42932, // Boss->player, no cast, single-target
    _Spell_MousseMural = 42684, // Boss->self, 5.0s cast, range 100 circle
    _Weaponskill_ColorRiot = 42642, // Boss->self, 5.0+2.0s cast, single-target
    _Spell_CoolBomb = 42643, // Helper->players, no cast, range 4 circle
    _Spell_WarmBomb = 42644, // Helper->players, no cast, range 4 circle
    _Ability_ = 42611, // Boss->location, no cast, single-target
    _Weaponskill_Wingmark = 42614, // Boss->self, 4.0+0.9s cast, single-target
    _Weaponskill_ColorClash = 42635, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ = 42636, // Boss->self, no cast, single-target
    _Weaponskill_DoubleStyle = 42624, // Boss->self, 12.0+0.9s cast, single-target
    _Weaponskill_Burst = 42619, // _Gen_HeavenBomb->location, 1.0s cast, range 15 circle
    _Spell_DarkMist = 42629, // _Gen_CandiedSuccubus->self, 1.0s cast, range 30 circle
    _Spell_ColorClash = 42640, // Helper->players, no cast, range 6 circle
    _Weaponskill_StickyMousse = 42645, // Boss->self, 5.0+0.6s cast, single-target
    _Weaponskill_ColorRiot1 = 42641, // Boss->self, 5.0+2.0s cast, single-target
    _Weaponskill_DoubleStyle1 = 42623, // Boss->self, 12.0+0.9s cast, single-target
    _Weaponskill_Burst1 = 42617, // _Gen_PaintBomb->self, 1.0s cast, range 15 circle
    _Spell_StickyMousse = 42646, // Helper->players, no cast, range 4 circle
    _Spell_Burst = 42647, // Helper->location, no cast, range 4 circle
    _Weaponskill_Sugarscape = 42600, // Boss->self, 1.0+7.0s cast, single-target
    _Weaponskill_Layer = 42602, // Boss->self, 1.0+6.0s cast, single-target
    _Weaponskill_SprayPain = 42657, // Helper->self, 7.0s cast, range 10 circle
    _Spell_Brulee = 42658, // Helper->location, no cast, range 15 circle
    _Spell_CrowdBrulee = 39469, // Helper->location, no cast, range 6 circle
    _Weaponskill_Layer1 = 42604, // Boss->self, 1.0+6.0s cast, single-target
    _Weaponskill_SprayPain1 = 39468, // Helper->self, 8.5s cast, range 10 circle
    _Spell_PuddingGraf = 42677, // Boss->self, 3.0s cast, single-target
    _Weaponskill_DoubleStyle2 = 42627, // Boss->self, 8.0+0.9s cast, single-target
    _Spell_PuddingGraf1 = 42678, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_ColorClash1 = 42637, // Boss->self, 3.0s cast, single-target
    _Weaponskill_1 = 42638, // Boss->self, no cast, single-target
    _Weaponskill_DoubleStyle3 = 42621, // Boss->self, 12.0+0.9s cast, single-target
    _Weaponskill_BadBreath = 42628, // _Gen_MouthwateringMorbol->self, 1.0s cast, range 50 100-degree cone
    _Spell_ColorClash1 = 42639, // Helper->players, no cast, range 6 circle
    _Weaponskill_DoubleStyle4 = 42625, // Boss->self, 12.0+0.9s cast, single-target
    _Weaponskill_DoubleStyle5 = 37834, // Boss->self, 12.0+0.9s cast, single-target
    _Weaponskill_DoubleStyle6 = 42626, // Boss->self, 12.0+0.9s cast, single-target
    _Weaponskill_DoubleStyle7 = 42622, // Boss->self, 12.0+0.9s cast, single-target
    _Weaponskill_2 = 42620, // _Gen_HeavenBomb->location, 1.0s cast, single-target
    _Spell_SoulSugar = 42661, // Boss->self, 3.0s cast, single-target
    _Weaponskill_LivePainting = 42662, // Boss->self, 4.0s cast, single-target
    _Weaponskill_UnlimitedCraving = 39479, // _Gen_GimmeCat->self, no cast, single-target
    _Ability_1 = 42672, // _Gen_GimmeCat->location, no cast, single-target
    _AutoAttack_1 = 37919, // _Gen_Mu->player, no cast, single-target
    _AutoAttack_2 = 37920, // _Gen_Yan->player, no cast, single-target
    _Weaponskill_ICraveViolence = 42673, // _Gen_GimmeCat->self, 3.0s cast, range 6 circle
    _Weaponskill_LivePainting1 = 42663, // Boss->self, 4.0s cast, single-target
    _Spell_WaterIII = 37831, // _Gen_FeatherRay->self, 3.0s cast, single-target
    _Spell_WaterIII1 = 42671, // _Gen_FeatherRay->players, no cast, range 8 circle
    _Weaponskill_LivePainting2 = 42664, // Boss->self, 4.0s cast, single-target
    _Ability_RallyingCheer = 42667, // _Gen_Mu->_Gen_Yan, no cast, single-target
    _Weaponskill_ManxomeWindersnatch = 42669, // _Gen_Jabberwock->player, no cast, single-target
    _Weaponskill_DoubleStyle8 = 37896, // Boss->self, 12.0+0.9s cast, single-target
    _Weaponskill_ReadyOreNot = 42666, // Boss->self, 7.0s cast, range 100 circle
    _Weaponskill_LivePainting3 = 42665, // Boss->self, 4.0s cast, single-target
    _Weaponskill_SlayousSnickerSnack = 42670, // _Gen_Jabberwock->player, no cast, single-target
    _Weaponskill_OreRigato = 42668, // _Gen_Mu->self, 5.0s cast, range 100 circle
    _Weaponskill_HangryHiss = 42674, // _Gen_GimmeCat->self, 5.0s cast, range 100 circle
}

public enum TetherID : uint
{
    _Gen_Tether_324 = 324, // player/_Gen_StickyPudding->Boss
    _Gen_Tether_319 = 319, // _Gen_HeavenBomb/_Gen_PaintBomb/_Gen_MouthwateringMorbol/_Gen_CandiedSuccubus/player->Boss
    _Gen_Tether_320 = 320, // _Gen_CandiedSuccubus/player/_Gen_MouthwateringMorbol->Boss
    _Gen_Tether_337 = 337, // _Gen_->Boss
    _Gen_Tether_17 = 17, // _Gen_FeatherRay->player
}

public enum IconID : uint
{
    _Gen_Icon_23 = 23, // player->self
}

public enum SID : uint
{
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_WarmTint = 4451, // Helper->player, extra=0x0
    _Gen_CoolTint = 4452, // Helper->player, extra=0x0
    _Gen_ = 2056, // none->_Gen_HeavenBomb, extra=0x36B
    _Gen_Wingmark = 4450, // none->player, extra=0x0
    _Gen_Stun = 4163, // none->player, extra=0x0
    _Gen_MousseMine = 4453, // Helper->player, extra=0x0
    _Gen_Sweltering = 4449, // none->player, extra=0x0
    _Gen_BurningUp = 4448, // none->player, extra=0x0
    _Gen_HeatingUp = 4454, // none->player, extra=0x0
    _Gen_FireResistanceDownII = 4383, // Helper->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_SixFulmsUnder = 567, // none->player, extra=0x300/0x361/0x362/0x363
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_HuffyCat = 4457, // none->_Gen_GimmeCat, extra=0x1/0x2/0x3
    _Gen_VulnerabilityUp = 3361, // _Gen_FeatherRay->player, extra=0x0
    _Gen_Dropsy = 3075, // none->player, extra=0x0
    _Gen_Dropsy1 = 3076, // none->player, extra=0x0
    _Gen_DamageUp = 3129, // _Gen_Mu->_Gen_Yan, extra=0x0
    _Gen_Bind = 3625, // _Gen_Jabberwock->player, extra=0x0
    _Gen_SustainedDamage = 4149, // _Gen_Mu->player, extra=0x1
}
