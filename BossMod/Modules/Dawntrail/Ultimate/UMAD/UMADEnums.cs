namespace BossMod.Dawntrail.Ultimate.UMAD;

public enum OID : uint
{
    BossP1 = 0x4C30, // R6.000, x1
    GravenImageP1 = 0x4C31, // R0.500, x9
    GravitasP1 = 0x1EC022, // blue puddle, R5.000

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
}

// intemperate will notes
// EObjAnim: 0x1EBFBD = 00400080: east (right) side cleave 5.2s later

public enum SID : uint
{
    _Gen_DamageDown = 2911, // Helper/_Gen_GravenImage->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper/_Gen_GravenImage->player, extra=0x0
    _Gen_DoubleTroubleTrap = 5078, // none->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
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
