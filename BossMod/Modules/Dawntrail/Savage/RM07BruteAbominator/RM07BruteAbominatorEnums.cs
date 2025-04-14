namespace BossMod.Dawntrail.Savage.RM07BruteAbominator;

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
    _Spell_SporeSac = 42346, // Helper->location, 4.0s cast, range 8 circle
    _Weaponskill_SinisterSeeds = 42349, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_Pollen = 42347, // Helper->location, 4.0s cast, range 8 circle
    _Spell_SinisterSeeds = 42353, // Helper->location, 3.0s cast, range 7 circle
    _Spell_SinisterSeeds1 = 42350, // Helper->players, 7.0s cast, range 6 circle
    _Spell_Impact = 42356, // Boss->self, no cast, single-target
    _Weaponskill_Impact = 42355, // Helper->players, no cast, range 6 circle
    _Spell_TendrilsOfTerror = 42352, // Helper->self, 3.0s cast, range 60 width 4 cross
    _Spell_TendrilsOfTerror1 = 42351, // Helper->self, 3.0s cast, range 10 circle - i don't know what this is, it gets cast from where the first seeds are dropped in P1, hits players but deals no damage (only a StartActionCombo effect)
    _AutoAttack_Attack = 872, // _Gen_BloomingAbomination->player, no cast, single-target
    _Spell_RootsOfEvil = 42354, // Helper->location, 3.0s cast, range 12 circle
    _AutoAttack_ = 42330, // Boss->player, no cast, single-target
    _Ability_CrossingCrosswinds = 43278, // _Gen_BloomingAbomination->self, 7.0s cast, range 50 width 10 cross
    _Ability_WindingWildwinds = 43277, // _Gen_BloomingAbomination->self, 7.0s cast, range 5-60 donut
    _Weaponskill_HurricaneForce = 42348, // _Gen_BloomingAbomination->self, 6.0s cast, range 60 circle
    _Weaponskill_BrutalImpact = 42331, // Boss->self, 5.0s cast, single-target
    _Weaponskill_BrutalImpact1 = 42332, // Boss->self, no cast, range 60 circle
    StoneringerClub = 42333, // Boss->self, 2.0+3.5s cast, single-target
    StoneringerSword = 42334, // Boss->self, 2.0+3.5s cast, single-target
    _Weaponskill_SmashHere = 42335, // Boss->self, 3.0+1.0s cast, single-target
    _Weaponskill_BrutishSwing = 42339, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing1 = 42337, // Helper->self, 4.0s cast, range 12 circle
    _Weaponskill_BrutalSmash = 42341, // Boss->players, no cast, range 6 circle
    _Weaponskill_SporeSac = 42345, // Boss->self, 3.0s cast, single-target
    _Ability_ = 42262, // Boss->location, no cast, single-target
    _Weaponskill_BrutishSwing2 = 42340, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing3 = 42338, // Helper->self, 4.0s cast, range 9-60 donut
    _Weaponskill_BrutalSmash1 = 42342, // Boss->players, no cast, range 6 circle
    _Weaponskill_QuarrySwamp = 42357, // Boss->self, 4.0s cast, range 60 circle
    _Spell_Explosion = 42358, // Helper->location, 9.0s cast, range 60 circle
    _Weaponskill_PulpSmash = 42359, // Boss->self, 3.0+2.0s cast, single-target
    _Weaponskill_PulpSmash1 = 42360, // Boss->player, no cast, single-target
    _Weaponskill_PulpSmash2 = 42361, // Helper->players, no cast, range 6 circle
    _Spell_TheUnpotted = 42363, // Helper->self, no cast, range 60 ?-degree cone
    _Spell_ItCameFromTheDirt = 42362, // Helper->location, 2.0s cast, range 6 circle
    _Weaponskill_SmashThere = 42336, // Boss->self, 3.0+1.0s cast, single-target
    _Weaponskill_NeoBombarianSpecial = 42364, // Boss->self, 8.0s cast, range 60 circle
    _Weaponskill_GrapplingIvy = 42365, // Boss->location, no cast, single-target
    _AutoAttack_1 = 43157, // _Gen_BruteAbombinator->player, no cast, single-target
    _Weaponskill_Stoneringer2 = 42368, // Boss->self, 2.0+3.5s cast, single-target
    _Weaponskill_BrutishSwing4 = 42380, // Boss->location, 4.0+3.8s cast, single-target
    _Weaponskill_BrutishSwing5 = 42389, // Boss->self, no cast, single-target
    _Weaponskill_BrutishSwing6 = 42387, // Helper->self, 8.1s cast, range ?-88 donut
    _Weaponskill_GlowerPower = 42373, // Boss->self, 2.7+1.3s cast, single-target
    _Spell_ElectrogeneticForce = 42374, // Helper->players, no cast, range 6 circle
    _Weaponskill_GlowerPower1 = 43340, // Helper->self, 4.0s cast, range 65 width 14 rect
    _Weaponskill_RevengeOfTheVines = 42375, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_BrutishSwing7 = 42381, // Boss->location, 4.0+3.8s cast, single-target
    _Spell_ThornyDeathmatch = 42376, // Boss->self, 3.0s cast, single-target
    _Weaponskill_AbominableBlink = 42377, // Boss->self, 5.3+1.3s cast, single-target
    _Ability_AbominableBlink = 43156, // Helper->players, no cast, range 60 circle
    _Weaponskill_BrutalSmash2 = 42343, // Boss->location, no cast, range 6 circle
}

public enum IconID : uint
{
    Spread = 375, // player->self
    Stack = 161, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_326 = 326, // _Gen_->Boss
    _Gen_Tether_338 = 338, // Boss->player
    _Gen_Tether_325 = 325, // Boss->player
}
