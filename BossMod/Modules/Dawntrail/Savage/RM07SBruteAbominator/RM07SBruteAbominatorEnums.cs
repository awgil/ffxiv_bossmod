#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

public enum OID : uint
{
    Boss = 0x4783, // R7.000-19.712, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    _Gen_ = 0x4785, // R7.000, x5
    _Gen_BruteAbombinator = 0x481C, // R0.000-0.500, x1, Part type
    _Gen_BloomingAbomination = 0x4784, // R3.400, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 42330, // Boss->player, no cast, single-target
    _Weaponskill_BrutalImpact = 42331, // Boss->self, 5.0s cast, single-target
    _Weaponskill_BrutalImpact1 = 42332, // Boss->self, no cast, range 60 circle
    StoneringerClub = 42333, // Boss->self, 2.0+3.5s cast, single-target
    StoneringerSword = 42334, // Boss->self, 2.0+3.5s cast, single-target
    StoneringerClubP2 = 42367, // Boss->self, 2.0+3.5s cast, single-target
    StoneringerSwordP2 = 42368, // Boss->self, 2.0+3.5s cast, single-target
    _Weaponskill_SmashThere = 42336, // Boss->self, 3.0+1.0s cast, single-target
    _Weaponskill_BrutishSwing2 = 42340, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing3 = 42338, // Helper->self, 4.0s cast, range 9-60 donut
    _Weaponskill_BrutalSmash1 = 42342, // Boss->players, no cast, range 6 circle
    _Weaponskill_SporeSac = 42345, // Boss->self, 3.0s cast, single-target
    _Ability_ = 42262, // Boss->location, no cast, single-target
    _Spell_SporeSac = 42346, // Helper->location, 4.0s cast, range 8 circle
    _Weaponskill_SinisterSeeds = 42349, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_Pollen = 42347, // Helper->location, 4.0s cast, range 8 circle
    _Spell_SinisterSeeds = 42353, // Helper->location, 3.0s cast, range 7 circle
    _Spell_SinisterSeeds1 = 42350, // Helper->players, 7.0s cast, range 6 circle
    _Spell_Impact = 42356, // Boss->self, no cast, single-target
    _Weaponskill_Impact = 42355, // Helper->players, no cast, range 6 circle
    _Spell_TendrilsOfTerror = 42352, // Helper->self, 3.0s cast, range 60 width 4 cross
    _Spell_TendrilsOfTerror1 = 42351, // Helper->self, 3.0s cast, range 10 circle
    _AutoAttack_Attack = 872, // _Gen_BloomingAbomination->player, no cast, single-target
    _Spell_RootsOfEvil = 42354, // Helper->location, 3.0s cast, range 12 circle
    _Ability_WindingWildwinds = 43277, // _Gen_BloomingAbomination->self, 7.0s cast, range 5-60 donut
    _Ability_CrossingCrosswinds = 43278, // _Gen_BloomingAbomination->self, 7.0s cast, range 50 width 10 cross
    _Weaponskill_BrutishSwing = 42339, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing1 = 42337, // Helper->self, 4.0s cast, range 12 circle
    _Weaponskill_BrutalSmash = 42341, // Boss->players, no cast, range 6 circle
    _Weaponskill_QuarrySwamp = 42357, // Boss->self, 4.0s cast, range 60 circle
    _Spell_Explosion = 42358, // Helper->location, 9.0s cast, range 60 circle
    _Weaponskill_SmashHere = 42335, // Boss->self, 3.0+1.0s cast, single-target
    _Weaponskill_PulpSmash = 42359, // Boss->self, 3.0+2.0s cast, single-target
    _Weaponskill_PulpSmash1 = 42360, // Boss->player, no cast, single-target
    _Weaponskill_PulpSmash2 = 42361, // Helper->players, no cast, range 6 circle
    _Spell_ItCameFromTheDirt = 42362, // Helper->location, 2.0s cast, range 6 circle
    _Spell_TheUnpotted = 42363, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_NeoBombarianSpecial = 42364, // Boss->self, 8.0s cast, range 60 circle
    _Weaponskill_GrapplingIvy = 42365, // Boss->location, no cast, single-target
    _AutoAttack_1 = 43157, // _Gen_BruteAbombinator->player, no cast, single-target
    _Weaponskill_BrutishSwing7 = 42381, // Boss->location, 4.0+3.8s cast, single-target
    _Weaponskill_BrutishSwing5 = 42389, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing6 = 42387, // Helper->self, 8.1s cast, range ?-88 donut
    _Weaponskill_GlowerPower = 42373, // Boss->self, 2.7+1.3s cast, single-target
    _Spell_ElectrogeneticForce = 42374, // Helper->players, no cast, range 6 circle
    _Weaponskill_GlowerPower1 = 43340, // Helper->self, 4.0s cast, range 65 width 14 rect
    _Weaponskill_RevengeOfTheVines = 42375, // Boss->self, 5.0s cast, range 60 circle
    _Spell_ThornyDeathmatch = 42376, // Boss->self, 3.0s cast, single-target
    _Weaponskill_AbominableBlink = 42377, // Boss->self, 5.3+1.3s cast, single-target
    _Ability_AbominableBlink = 43156, // Helper->players, no cast, range 60 circle
    _Weaponskill_Sporesplosion = 42378, // Boss->self, 4.0s cast, single-target
    _Spell_Sporesplosion = 42379, // Helper->location, 5.0s cast, range 8 circle
    _Weaponskill_BrutishSwing4 = 42385, // Boss->location, 4.0+3.8s cast, single-target
    _Weaponskill_BrutishSwing8 = 42388, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing9 = 42386, // Helper->self, 8.1s cast, range 25 180-degree cone
    _Weaponskill_HurricaneForce = 42348, // _Gen_BloomingAbomination->self, 6.0s cast, range 60 circle
    _Spell_DemolitionDeathmatch = 42390, // Boss->self, 3.0s cast, single-target
    _Weaponskill_StrangeSeeds = 42391, // Boss->self, 4.0s cast, single-target
    _Spell_StrangeSeeds = 42392, // Helper->players, 5.0s cast, range 6 circle
    _Spell_TendrilsOfTerror2 = 42394, // Helper->self, 3.0s cast, range 60 width 4 cross
    _Spell_TendrilsOfTerror3 = 42393, // Helper->self, 3.0s cast, range 10 circle
    _Spell_KillerSeeds = 42395, // Helper->players, 5.0s cast, range 6 circle
    _Spell_TendrilsOfTerror4 = 42397, // Helper->self, 3.0s cast, range 60 width 4 cross
    _Spell_TendrilsOfTerror5 = 42396, // Helper->self, 3.0s cast, range 10 circle
    _Weaponskill_BrutishSwing10 = 42383, // Boss->location, 4.0+3.8s cast, single-target
    _Weaponskill_Powerslam = 42398, // Boss->location, 6.0s cast, range 60 circle
    _Weaponskill_BrutishSwing41 = 42380, // Boss->location, 4.0+3.8s cast, single-target
    _Weaponskill_BrutishSwing11 = 42384, // Boss->location, 4.0+3.8s cast, single-target
    _Weaponskill_Stoneringer2Stoneringers = 42401, // Boss->self, 2.0+3.5s cast, single-target
    _Weaponskill_BrutishSwing12 = 42402, // Boss->location, 3.0+3.8s cast, single-target
    _Weaponskill_BrutishSwing13 = 42557, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing14 = 42405, // Helper->self, 6.7s cast, range ?-88 donut
    _Weaponskill_RevengeOfTheVines1 = 42553, // Boss->self, no cast, range 60 circle
    _Weaponskill_LashingLariat = 42407, // Boss->location, 3.5+0.5s cast, single-target
    _Weaponskill_LashingLariat1 = 42408, // Helper->self, 4.0s cast, range 70 width 32 rect
    _Weaponskill_BrutishSwing15 = 42412, // Boss->location, 3.0+3.8s cast, single-target
    _Weaponskill_BrutishSwing16 = 42556, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing17 = 42403, // Helper->self, 6.7s cast, range 25 180-degree cone
    _Weaponskill_GlowerPower2 = 43338, // Boss->self, 0.7+1.3s cast, single-target
    _Weaponskill_GlowerPower3 = 43358, // Helper->self, 2.0s cast, range 65 width 14 rect
    _Weaponskill_Slaminator = 42413, // Boss->location, 4.0+1.0s cast, single-target
    _Weaponskill_Slaminator1 = 42414, // Helper->self, 5.0s cast, range 8 circle
    _Spell_DebrisDeathmatch = 42416, // Boss->self, 3.0s cast, single-target
    _Weaponskill_BrutishSwing18 = 42411, // Boss->location, 3.0+3.8s cast, single-target
    _Weaponskill_Stoneringer2Stoneringers1 = 42400, // Boss->self, 2.0+3.5s cast, single-target
    _Weaponskill_BrutishSwing19 = 42404, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing20 = 42406, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing21 = 42382, // Boss->location, 4.0+3.8s cast, single-target
    _Weaponskill_LashingLariat2 = 42409, // Boss->location, 3.5+0.5s cast, single-target
    _Weaponskill_LashingLariat3 = 42410, // Helper->self, 4.0s cast, range 70 width 32 rect
}

public enum IconID : uint
{
    SporeSac = 375, // player->self
    SinisterSeed = 466, // player->self
    PulpSmash = 161, // player->self
    KillerSeed = 93, // player->self
    Flare = 327, // player->self
}

public enum TetherID : uint
{
    WallTether = 326, // _Gen_->Boss
    ThornsOfDeathTank = 338, // Boss->player
    ThornsOfDeath = 325, // Boss/_Gen_->player
    ThornsOfDeathPre = 84, // _Gen_->player
    _Gen_Tether_339 = 339, // _Gen_->Boss
}

public enum SID : uint
{
    _Gen_ = 2056, // none->Boss, extra=0x38A/0x388/0x377/0x389
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_DamageDown = 2911, // Helper/_Gen_BloomingAbomination->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_1 = 4434, // none->Boss, extra=0x1
    _Gen_2 = 4435, // none->Boss, extra=0x16
    _Gen_DirectionalDisregard = 3808, // none->Boss, extra=0x0
    _Gen_ThornsOfDeathI = 4499, // none->player, extra=0x0
    _Gen_ThornsOfDeathII = 4500, // none->player, extra=0x0
    _Gen_ThornsOfDeathIII = 4501, // none->player, extra=0x0
    _Gen_ThornsOfDeathIV = 4502, // none->player, extra=0x0
    _Gen_ThornsOfDeathI2 = 4466, // none->player, extra=0x0
    _Gen_ThornsOfDeathII2 = 4467, // none->player, extra=0x0
    _Gen_ThornsOfDeathIII2 = 4468, // none->player, extra=0x0
    _Gen_ThornsOfDeathIV2 = 4469, // none->player, extra=0x0
    _Gen_StoneCurse = 4537, // Boss->_Gen_BloomingAbomination/player, extra=0x0

}
