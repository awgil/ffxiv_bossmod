#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x28, Helper type
    _Gen_ = 0x4947, // R1.000, x3
    Boss = 0x4918, // R9.000, x1
    UltimaTheFeared = 0x4919, // R16.000, x1
    _Gen_UltimaTheFeared = 0x49AF, // R0.000, x1, Part type
    _Gen_EnergyOrb = 0x491A, // R1.000, x2, Helper type

    ManaScreenNESW = 0x1EBE8B,
    ManaScreenNWSE = 0x1EBE8C,
}

public enum AID : uint
{
    _AutoAttack_ = 44317, // _Gen_UltimaTheFeared->player, no cast, single-target
    _AutoAttack_Attack = 45308, // Boss->player, no cast, single-target
    _Weaponskill_IonEfflux = 44331, // Boss->self, 6.5s cast, range 65 circle
    _Weaponskill_Antimatter = 44304, // UltimaTheFeared->self, 5.0s cast, single-target
    _Spell_Antimatter = 44305, // Helper->player, 6.0s cast, single-target
    _Weaponskill_EnergyOrb = 44296, // UltimaTheFeared->self, 3.0s cast, single-target
    _Ability_ = 44332, // Boss->location, no cast, single-target
    _Spell_EnergizingEquilibrium = 44316, // UltimaTheFeared->Boss, no cast, single-target
    _Spell_EnergyRay = 44338, // _Gen_EnergyOrb->self, 5.0s cast, range 40 width 16 rect
    _Weaponskill_ForeToAftFire = 44325, // Boss->self, 6.0s cast, single-target
    _Spell_OmegaBlaster = 44329, // Helper->self, 6.5s cast, range 50 180-degree cone
    _Weaponskill_AftwardBlaster = 44328, // Boss->self, no cast, single-target
    _Spell_OmegaBlaster1 = 44330, // Helper->self, 8.8s cast, range 50 180-degree cone
    _Weaponskill_TractorBeam = 44294, // UltimaTheFeared->self, 10.0s cast, range 40 width 48 rect
    _Weaponskill_AntiPersonnelMissile = 45191, // Boss->self, no cast, single-target
    _Ability_Crash = 44295, // Helper->self, 10.5s cast, range 40 width 24 rect
    _Spell_TractorBeam = 45190, // Helper->self, 10.5s cast, range 40 width 24 rect
    _Weaponskill_AntiPersonnelMissile1 = 45192, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_SurfaceMissile = 45173, // Boss->self, 9.0s cast, single-target
    _Spell_SurfaceMissile = 45174, // Helper->self, 1.0s cast, range 12 width 20 rect
    _Weaponskill_ManaScreen = 44297, // UltimaTheFeared->self, 3.0s cast, single-target
    _Spell_EnergyRay1 = 44300, // Helper->self, no cast, range 48 width 20 rect
    _Weaponskill_TrajectoryProjection = 44323, // Boss->self, 3.5s cast, single-target
    _Spell_GuidedMissile = 44324, // Helper->self, 1.0s cast, range 6 circle
    _Weaponskill_TractorField = 44306, // UltimaTheFeared->self, 5.0s cast, single-target
    _Spell_TractorField = 44307, // Helper->self, 5.5s cast, range 50 circle
    _Weaponskill_MultiMissile = 45035, // Boss->self, 2.1s cast, single-target
    _Spell_CitadelSiege = 44308, // UltimaTheFeared->self, no cast, single-target
    _Weaponskill_MultiMissile1 = 45037, // Helper->self, 4.0s cast, range 6 circle
    _Weaponskill_MultiMissile2 = 45036, // Helper->self, 4.1s cast, range 10 circle
    _Spell_CitadelSiege1 = 44309, // UltimaTheFeared->self, no cast, single-target
    _Spell_CitadelSiege2 = 44312, // Helper->self, 5.0s cast, range 48 width 10 rect
    _Spell_CitadelSiege3 = 44310, // UltimaTheFeared->self, no cast, single-target
    _Spell_CitadelSiege4 = 44311, // UltimaTheFeared->self, no cast, single-target
    _Weaponskill_CitadelBuster = 44315, // UltimaTheFeared->location, 6.0s cast, range 50 circle
    _Weaponskill_HyperPulse = 44335, // Boss->self, 5.0s cast, range 50 circle
    _Spell_EnergyRay2 = 44301, // Helper->self, no cast, range 48 width 20 rect
    _Spell_EnergyRay3 = 44299, // Helper->self, no cast, range 40 width 16 rect
    _Spell_EnergizingEquilibrium1 = 44336, // Boss->UltimaTheFeared, no cast, single-target
    _Weaponskill_ChemicalBomb = 44302, // UltimaTheFeared->self, 6.5s cast, single-target
    _Spell_ChemicalBomb = 44303, // Helper->self, 7.0s cast, range 50 circle
    _Weaponskill_AftToForeFire = 44327, // Boss->self, 6.0s cast, single-target
    _Weaponskill_ForewardBlaster = 44326, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon02k1 = 218, // player->self
    _Gen_Icon_loc06sp_05ak1 = 466, // player->self
    _Gen_Icon_z6r2_b2_lock_c0w = 616, // Helper->self
    _Gen_Icon_z6r2_trg_w_t0w = 618, // player->self
    _Gen_Icon_z6r2_trg_s_t0w = 619, // player->self
    _Gen_Icon_z6r2_trg_n_t0w = 620, // player->self
    _Gen_Icon_z6r2_trg_e_t0w = 617, // player->self
}
