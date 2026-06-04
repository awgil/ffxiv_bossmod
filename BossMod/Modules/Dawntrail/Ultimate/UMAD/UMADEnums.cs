#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Ultimate.UMAD;

public enum OID : uint
{
    BossP1 = 0x4C30, // R6.000, x1
    GravenImage = 0x4C31, // R0.500, x9
    Gravitas = 0x1EC022, // blue puddle, R5.000
    TelePortent = 0x1EC023, // arrow, R2.000

    BossP2 = 0x4C32, // R6.020, x1

    Helper = 0x233C, // R0.500, x37, Helper type
}

public enum AID : uint
{
    _AutoAttack_ = 49746, // Boss->player, no cast, single-target
    _Ability_RevoltingRuinIII = 50179, // Boss->self/player, 5.0s cast, range 100 120-degree cone
    _Ability_RevoltingRuinIII1 = 50401, // Boss->self, no cast, range 100 120-degree cone
    _Ability_ = 50173, // Boss->location, no cast, single-target
    _Ability_GravenImage = 48370, // Boss->self, 3.0s cast, single-target
    _Ability_MysteryMagic = 47764, // Boss->self, 5.0s cast, single-target

    _Ability_BlizzardIIIBlowout3 = 47765, // Boss->self, 5.0s cast, single-target
    _Ability_BlizzardIIIBlowout2 = 47768, // Helper->self, 5.0s cast, range 40 90-degree cone, real aoe
    _Ability_BlizzardIIIBlowout = 47771, // Helper->self, 5.0s cast, range 40 90-degree cone, visual
    _Ability_BlizzardIIIBlowout1 = 47774, // Helper->self, 5.0s cast, range 40 90-degree cone, real aoe
    _Ability_ThrummingThunderIII2 = 47775, // Helper->self, 5.0s cast, range 40 width 10 rect, real
    _Ability_ThrummingThunderIII1 = 47776, // Helper->self, 5.0s cast, range 40 width 10 rect, fake
    _Ability_ThrummingThunderIII = 47777, // Helper->self, 5.0s cast, range 40 width 10 rect, real
    _Ability_FlagrantFireIII1 = 47778, // Helper->players, no cast, range 5 circle, spread
    _Ability_FlagrantFireIII = 47779, // Helper->players, no cast, range 6 circle, stack

    _Ability_DoubleTroubleTrap = 47782, // Boss->self, 3.0s cast, single-target
    _Ability_DoubleTroubleTrap1 = 47783, // Helper->players, no cast, range 6 circle
    _Ability_WaveCannon = 47784, // _Gen_GravenImage->self, no cast, range 100 width 6 rect
    _Ability_PulseWave = 47785, // _Gen_GravenImage->player, no cast, single-target
    _Ability_Explosion = 47786, // Helper->self, 3.0s cast, range 4 circle
    _Ability_UnmitigatedExplosion = 47787, // Helper->self, no cast, range 100 circle

    _Ability_Gravitas = 47788, // _Gen_GravenImage->players, no cast, range 5 circle
    _Ability_GravitationalExplosion = 47789, // Helper->self, no cast, range 100 circle

    _Ability_LightOfJudgment = 50722, // Boss->self, 5.0s cast, range 100 circle
    _Ability_Hyperdrive = 49739, // Boss->players, no cast, range 5 circle
    _Ability_Vitrophyre = 47792, // GravenImageP1->players, no cast, range 5 circle
    _Ability_GravitationalWave = 47793, // GravenImageP1->self, no cast, range 100 ?-degree cone
    _Ability_IntemperateWill = 47794, // _Gen_GravenImage->self, no cast, range 100 ?-degree cone

    _Ability_GravityIII = 47791, // Helper->self, no cast, range 5 circle
    _Ability_TeleTrouncing = 47801, // BossP1->self, 5.0s cast, single-target
    _Ability_TeleTrouncing1 = 47802, // Helper->players, no cast, range 2 circle
    _Weaponskill_ = 50516, // BossP1->self, 3.0s cast, single-target
    _Ability_IndulgentWill = 47797, // GravenImageP1->player, no cast, single-target, applies confusion
    _Ability_IdyllicWill = 47798, // GravenImageP1->players, no cast, range 5 circle, applies sleep
    _Weaponskill_1 = 50517, // BossP1->self, no cast, single-target
    _Ability_AveMaria = 47795, // GravenImageP1->self, no cast, range 100 circle, inverted gaze
    _Ability_IndolentWill = 47796, // GravenImageP1->self, no cast, range 100 circle, gaze
    P1LightOfJudgmentEnrage = 47803, // BossP1->self, 5.0s cast, range 100 circle

    _Ability_UltimateEmbrace = 49740, // BossP2->players, 5.0s cast, range 5 circle
    _Ability_Forsaken = 47804, // BossP2->self, 7.0s cast, range 100 circle
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper/_Gen_GravenImage->player, extra=0x0
    DoubleTroubleTrap = 5078, // none->player, extra=0x0

    // players are always assigned one of the 1 set; if the second status is from the 2 set, the two arrows differ by 90 degrees
    TelePortentN1 = 4876, // Helper->player, extra=0x0
    TelePortentS1 = 4877, // Helper->player, extra=0x0
    TelePortentE1 = 4878, // Helper->player, extra=0x0
    TelePortentW1 = 4879, // Helper->player, extra=0x0
    TelePortentN2 = 5079, // Helper->player, extra=0x0
    TelePortentS2 = 5080, // Helper->player, extra=0x0
    TelePortentE2 = 5081, // Helper->player, extra=0x0
    TelePortentW2 = 5082, // Helper->player, extra=0x0

    Sleep = 4894, // GravenImageP1->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    Confused = 1283, // GravenImageP1->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon02k1 = 218, // player->self
    _Gen_Icon_m0462trg_a0c = 127, // player->self, spread
    _Gen_Icon_m0462trg_b0c = 128, // player->self, stack
    _Gen_Icon_m0462trg_c01c = 673, // Boss->self, fake fire
    _Gen_Icon_m0462trg_c02c = 674, // Boss->self, real fire
    _Gen_Icon_m0462trg_c03c = 675, // Boss->self, fake ice
    _Gen_Icon_m0462trg_c04c = 676, // Boss->self, real ice
    _Gen_Icon_m0462trg_c05c = 677, // Boss->self, fake lightning
    _Gen_Icon_m0462trg_c06c = 678, // Boss->self, real lightning
}

public enum TetherID : uint
{
    _Gen_Tether_chn_elem0f = 45, // _Gen_GravenImage->player
}
