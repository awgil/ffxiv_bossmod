#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

public enum OID : uint
{
    Boss = 0x4866,
    Helper = 0x233C,
    Triton = 0x4786, // R4.000, x1 (spawn during fight)
    Nereid = 0x4787, // R4.000, x1 (spawn during fight)
    Phobos = 0x4788, // R4.000, x1 (spawn during fight)
    LiquifiedTriton = 0x478B, // R4.000, x0 (spawn during fight)
    LiquifiedNereid = 0x478C, // R4.000, x0 (spawn during fight)
    DeadStars = 0x478D, // R15.000, x0 (spawn during fight)
    GaseousNereid = 0x4814, // R5.000, x0 (spawn during fight)
    GaseousPhobos = 0x4815, // R5.000, x0 (spawn during fight)
    FrozenTriton = 0x4816, // R5.000, x0 (spawn during fight)
    FrozenPhobos = 0x4817, // R5.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Triton/Phobos/Nereid->player, no cast, single-target
    _Ability_ = 42420, // Phobos/Nereid/Triton->location, no cast, single-target
    _Ability_DecisiveBattle2 = 42490, // Triton->self, 6.0s cast, range 35 circle
    _Ability_DecisiveBattle1 = 42491, // Nereid->self, 6.0s cast, range 35 circle
    _Ability_DecisiveBattle = 42492, // Phobos->self, 6.0s cast, range 35 circle
    _Ability_1 = 42504, // Boss->self, no cast, range ?-40 donut
    _Ability_SliceNDice = 42498, // Helper->player, 5.0s cast, range 70 ?-degree cone
    _Ability_SliceNDice1 = 42497, // Phobos/Nereid/Triton->player, 5.0s cast, single-target
    _Ability_ThreeBodyProbl = 42421, // Phobos/Nereid/Triton->location, 5.3+0.7s cast, single-target
    _Ability_ThreeBodyProblem = 43453, // Nereid->self, 6.5s cast, single-target
    _Ability_ThreeBodyProblem1 = 42425, // Triton->self, 6.5s cast, single-target
    _Ability_NoisomeNuisance = 42551, // Helper->self, 7.0s cast, range 6 circle
    _Spell_PrimordialChaos = 42457, // Phobos->self, 5.0s cast, single-target
    _Spell_PrimordialChaos1 = 42458, // LiquifiedNereid/LiquifiedTriton->self, no cast, single-target
    _Spell_PrimordialChaos2 = 42460, // Helper->self, no cast, ???
    FrozenFalloutFireCast = 42463, // Helper->self, 2.5s cast, range 22 circle
    FrozenFalloutIceCast = 42464, // Helper->self, 2.5s cast, range 22 circle
    _Spell_FrozenFallout2 = 42461, // Phobos->self, 10.0s cast, single-target
    _Spell_FrozenFallout3 = 42466, // LiquifiedNereid->location, no cast, single-target
    _Spell_FrozenFallout4 = 42465, // LiquifiedTriton->location, no cast, single-target
    _Spell_ = 42459, // Helper->self, 1.4s cast, range 22 circle
    _Spell_FrozenFallout5 = 42468, // Helper->self, 1.4s cast, ???
    _Spell_FrozenFallout6 = 42467, // Helper->self, 1.4s cast, ???
    _Spell_FrozenFallout7 = 42462, // Phobos->self, no cast, single-target
    _Spell_NoxiousNova = 42469, // Phobos->self, 5.0s cast, single-target
    _Spell_NoxiousNova1 = 42470, // Helper->self, 5.8s cast, ???
    _Ability_2 = 42427, // Nereid/Triton/Phobos->self, no cast, single-target
    _Ability_3 = 42428, // Nereid/Triton/Phobos->self, no cast, single-target
    _Spell_VengefulBlizzardIII = 42430, // Nereid->self, 5.5s cast, range 60 120-degree cone
    _Spell_VengefulFireIII = 42429, // Triton->self, 5.5s cast, range 60 120-degree cone
    _Ability_4 = 42426, // Phobos/Nereid/Triton->self, no cast, single-target
    _Spell_ExcruciatingEquilibrium = 42488, // Phobos/Nereid/Triton->self, no cast, single-target
    _Spell_ExcruciatingEquilibrium1 = 42489, // Helper->Nereid/Triton/Phobos, 0.5s cast, single-target
    _Spell_DeltaAttack = 42495, // Helper->self, 5.0s cast, single-target
    _Spell_DeltaAttack1 = 42493, // Phobos/Nereid/Triton->self, 5.0s cast, single-target
    _Spell_DeltaAttack2 = 42558, // Helper->self, 5.5s cast, ???
    _Spell_DeltaAttack3 = 42496, // Helper->self, no cast, single-target
    _Spell_DeltaAttack4 = 42494, // Phobos/Nereid/Triton->self, no cast, single-target
    _Spell_DeltaAttack5 = 42559, // Helper->self, 0.5s cast, ???
    _Ability_Firestrike = 42500, // Helper->player, no cast, single-target
    _Ability_Firestrike1 = 42499, // Phobos/Nereid/Triton->self, 5.0s cast, single-target
    _Ability_Firestrike2 = 42502, // Phobos/Triton/Nereid->players, no cast, range 70 width 10 rect
    _Ability_ThreeBodyProblem2 = 42424, // Phobos/Triton->self, 6.5s cast, single-target
    _Ability_IceboundBuffoon = 42550, // Helper->self, 7.0s cast, range 6 circle
    _Ability_SnowBoulder = 42447, // Helper->location, 7.0s cast, width 10 rect charge
    _Ability_SnowballFlight = 42446, // Nereid->self, 7.0s cast, single-target
    _Ability_SnowBoulder1 = 42448, // FrozenTriton/FrozenPhobos->location, no cast, width 10 rect charge
    _Spell_ChillingCollision = 42451, // Helper->self, 5.0s cast, range 40 circle
    _Spell_ChillingCollision1 = 42422, // Nereid->self, 5.0s cast, single-target
    _Spell_ChillingCollision2 = 42452, // Helper->self, no cast, ???
    _Ability_Avalaunch = 43162, // FrozenTriton/FrozenPhobos->self, 5.0s cast, single-target
    _Ability_Avalaunch1 = 42450, // FrozenTriton/FrozenPhobos->location, no cast, range 8 circle
    _Ability_Avalaunch2 = 42449, // Helper->player, 8.0s cast, single-target
    _Spell_ToTheWinds = 42453, // Nereid->self, 13.0s cast, single-target
    _Spell_VengefulBioIII = 42431, // Phobos->self, 5.5s cast, range 60 120-degree cone
    _Ability_Firestrike3 = 42503, // Helper->player, no cast, single-target
    _Ability_SliceNStrike = 42501, // Phobos/Nereid/Triton->player, 5.0s cast, single-target
    _Ability_ThreeBodyProblem3 = 42423, // Phobos/Nereid->self, 6.5s cast, single-target
    _Ability_BlazingBelligerent = 42549, // Helper->self, 7.0s cast, range 6 circle
    _Spell_ElementalImpact = 42432, // GaseousPhobos/GaseousNereid->self, 3.0s cast, range 5 circle
    _Spell_FireSpread = 43272, // Helper->players, no cast, range 60 ?-degree cone
    _Spell_ElementalImpact1 = 42433, // GaseousPhobos/GaseousNereid->self, 3.0s cast, range 5 circle
    _Spell_GeothermalRupture = 42441, // Triton->self, 5.0s cast, single-target
    _Spell_FlameThrower = 42444, // Helper->player, no cast, single-target
    _Spell_GeothermalRupture1 = 42442, // Helper->location, 3.0s cast, range 8 circle
    _Spell_FlameThrower1 = 42443, // Triton->self, 5.0s cast, single-target
    _Spell_FlameThrower2 = 42445, // Helper->self, no cast, range 40 width 8 rect
    _Spell_ElementalImpact2 = 42434, // GaseousPhobos/GaseousNereid->self, 5.0s cast, range 5 circle
    _Ability_SixHandedFistfight = 42471, // Phobos/Nereid/Triton->location, 9.4+0.6s cast, single-target
    _Ability_SixHandedFistfight1 = 42472, // Helper->self, 10.0s cast, range 12 circle
    _Ability_SixHandedFistfight2 = 42473, // Helper->self, 10.5s cast, ???
    _Ability_Bodied = 42474, // Helper->self, no cast, range 12 circle
    _Ability_5 = 42475, // Triton/Phobos/Nereid->self, no cast, single-target
    _Ability_6 = 42476, // Nereid/Phobos/Triton->self, no cast, single-target
    _Spell_CollateralGasJet = 42480, // Helper->self, 5.0s cast, range 40 60-degree cone
    _Spell_CollateralColdJet = 42479, // Helper->self, 5.0s cast, range 40 60-degree cone
    _Spell_CollateralHeatJet = 42478, // Helper->self, 5.0s cast, range 40 60-degree cone
    _Ability_CollateralDamage = 42477, // DeadStars->self, 5.0s cast, single-target
    _Spell_CollateralFireball = 42481, // Helper->player, no cast, range 4 circle
    _Spell_CollateralIceball = 42482, // Helper->player, no cast, range 4 circle
    _Spell_CollateralBioball = 42483, // Helper->player, no cast, range 4 circle
    _Ability_7 = 42486, // Phobos/Nereid/Triton->location, no cast, single-target
    _System_Return = 42487, // Phobos/Nereid/Triton->self, 6.0s cast, single-target
    _Spell_SelfDestruct = 42454, // FrozenTriton/FrozenPhobos->self, 13.0s cast, single-target
}

public enum SID : uint
{
    _Gen_TritonicGravity = 4438, // none->player, extra=0x0
    _Gen_PhobosicGravity = 4440, // none->player, extra=0x0
    _Gen_NereidicGravity = 4439, // none->player, extra=0x0
    _Gen_1 = 4505, // none->Triton, extra=0x334
    _Gen_2 = 4506, // none->Nereid, extra=0x37E
    _Gen_3 = 4507, // none->Phobos, extra=0x37F
    _Gen_NovaOoze = 4441, // none->player, extra=0x2/0x3/0x1
    _Gen_IceOoze = 4442, // none->player, extra=0x2/0x1/0x3
    _Gen_PredictionOfCleansing = 4266, // none->player, extra=0x6/0xF
    _Gen_MagicVulnerabilityUp = 2941, // Helper/Triton/Phobos/Nereid/GaseousPhobos/GaseousNereid->player, extra=0x0
    _Gen_ResurrectionRestricted = 4262, // none->player, extra=0x2/0x1
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_ = 2193, // Nereid/Triton/Phobos->Nereid/Triton/FrozenTriton/FrozenPhobos/Phobos, extra=0x375/0x373/0x374
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_PredictionOfJudgment = 4265, // none->player, extra=0x5/0xB
    _Gen_PredictionOfStarfall = 4268, // none->player, extra=0xD/0x9/0xA
    _Gen_IceboundBuffoonery = 4443, // none->FrozenTriton/FrozenPhobos, extra=0x4/0x3/0x2/0x1
    _Gen_ResurrectionDenied = 4263, // none->player, extra=0x0
    _Gen_PhysicalVulnerabilityUp = 2940, // FrozenPhobos/FrozenTriton->player, extra=0x0
    _Gen_ThriceComeRuin = 3478, // Helper->player, extra=0x1/0x2
    _Gen_PredictionOfBlessing = 4267, // none->player, extra=0x5
    _Gen_FalsePrediction = 4269, // none->player, extra=0x0
    _Gen_Doom = 2519, // Helper->player, extra=0x0
    _Gen_BattleHigh = 4229, // none->player, extra=0x0
}
