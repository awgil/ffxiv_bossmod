#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x36, Helper type
    _Gen_ = 0x4824, // R2.000, x2
    _Gen_1 = 0x49E2, // R5.000, x1
    Boss = 0x460F, // R7.000, x1
    _Gen_OrbitalWind = 0x493B, // R1.000, x0 (spawn during fight)
    _Gen_OrbitalLevin = 0x493A, // R1.500, x0 (spawn during fight)
    _Gen_OrbitalFlame = 0x4939, // R1.300, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 44359, // Boss->player, no cast, single-target
    _Spell_UranosCascade = 44369, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_UranosCascade1 = 44370, // Helper->player, 5.0s cast, range 6 circle
    _Spell_CronosSling = 44361, // Boss->self, 7.0s cast, single-target
    _Spell_CronosSling1 = 44365, // Helper->self, 7.5s cast, range 9 circle
    _Spell_CronosSling2 = 44367, // Helper->self, 13.3s cast, range 70 width 136 rect
    _Spell_EmpyrealVortex = 44397, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_EmpyrealVortex1 = 44398, // Helper->self, 4.3s cast, single-target
    _Spell_EmpyrealVortex2 = 44399, // Helper->self, no cast, range 75 circle
    _Spell_EmpyrealVortex3 = 44400, // Helper->players, 5.0s cast, range 5 circle
    _Spell_EmpyrealVortex4 = 44401, // Helper->location, 5.0s cast, range 6 circle
    _Spell_CronosSling3 = 44364, // Boss->self, 7.0s cast, single-target
    _Spell_CronosSling4 = 44366, // Helper->self, 7.5s cast, range 6-70 donut
    _Spell_CronosSling5 = 44368, // Helper->self, 13.3s cast, range 70 width 136 rect
    _Ability_ = 44375, // Helper->self, 4.0s cast, range 6 circle
    _Ability_Warp = 44374, // Boss->self, 4.0s cast, single-target
    _Ability_1 = 44360, // Boss->location, no cast, single-target
    _Spell_Sleepga = 44376, // Boss->self, 3.0s cast, range 70 width 70 rect
    _Spell_GaeaStream = 44371, // Boss->self, 3.0+1.0s cast, single-target
    _Spell_GaeaStream1 = 44372, // Helper->self, 4.0s cast, range 4 width 24 rect
    _Spell_GaeaStream2 = 44373, // Helper->self, 2.0s cast, range 4 width 24 rect
    _Spell_OmegaJavelin = 44380, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_OmegaJavelin1 = 44381, // Helper->location, no cast, range 6 circle
    _Spell_OmegaJavelin2 = 44382, // Helper->location, 4.5s cast, range 6 circle
    _Spell_Duplicate = 44407, // Boss->self, 2.0+1.0s cast, single-target
    _Spell_Duplicate1 = 44408, // Helper->self, 0.7s cast, range 16 width 16 rect
    _Spell_Excelsior = 45120, // Boss->self, 7.0s cast, range 35 circle
    _Spell_ = 44823, // Helper->self, 7.5s cast, range 100 circle
    _Spell_PhaseShift = 44377, // Boss->self, 3.0s cast, single-target
    _Spell_PhaseShift1 = 44334, // Boss->self, no cast, single-target
    _Ability_2 = 44379, // Helper->self, 13.0s cast, range 16 width 16 rect
    _Spell_PhaseShift2 = 44378, // Helper->self, 22.0s cast, range 100 circle
    _Spell_VisionsOfParadise = 44405, // Boss->self, 7.0+1.0s cast, single-target
    _Spell_Duplicate2 = 44406, // Helper->self, 1.0s cast, range 16 width 16 rect
    _Spell_1 = 44413, // Helper->player, 4.0s cast, single-target
    _Spell_StellarBurst = 44402, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_StellarBurst1 = 44403, // Helper->player, 5.0s cast, single-target
    _Spell_StellarBurst2 = 44404, // Helper->location, 5.0s cast, range 24 circle
    _Spell_Quake = 44388, // Helper->self, 6.0s cast, range 16 width 48 rect
    _Spell_Tornado = 44395, // Helper->self, 6.0s cast, range 35 circle
    _Spell_AncientTriad = 44383, // Boss->self, 6.0s cast, single-target
    _Spell_Tornado1 = 44341, // Helper->self, 7.0s cast, range 5 circle
    _Spell_Burst = 44386, // Helper->location, 7.0s cast, range 5 circle
    _Spell_Tornado2 = 44396, // Helper->self, no cast, range 3 circle
    _Spell_Shock = 44387, // _Gen_OrbitalLevin->player, no cast, single-target
    _Spell_CronosSling6 = 44362, // Boss->self, 7.0s cast, single-target
    _Spell_Freeze = 44389, // Helper->self, 6.0s cast, range 16 width 48 rect
    _Spell_Flare = 44384, // Helper->location, 7.0s cast, range 5 circle
    _Spell_Flare1 = 44385, // _Gen_OrbitalFlame->location, 4.0s cast, range 70 width 6 rect
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockonae_6m_5s_01t = 344, // player->self
    _Gen_Icon_loc05sp_05a_se_p = 376, // player->self
    _Gen_Icon_loc06sp_05ak1 = 466, // player->self
    _Gen_Icon_lockon5_t0h = 23, // player->self
    _Gen_Icon_d1084_share_24m_s6_0k2 = 608, // _Gen_1->self
    _Gen_Icon_tar_ring1bp = 210, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_d1084_change_0k2 = 352, // _Gen_->_Gen_
    _Gen_Tether_chn_thunder1f = 6, // _Gen_OrbitalLevin->player
}
