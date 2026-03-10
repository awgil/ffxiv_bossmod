namespace BossMod.Dawntrail.Variant.V02LoneSwordmaster;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x26, Helper type
    Boss = 0x4B17, // R5.000, x1
    _Gen_ForceOfWill = 0x4B1E, // R1.000, x0 (spawn during fight)
    _Gen_ForceOfWill1 = 0x4B19, // R0.500, x0 (spawn during fight)
    _Gen_ForceOfWill2 = 0x4C3D, // R2.000, x0 (spawn during fight)
    _Gen_ForceOfWill3 = 0x4B18, // R1.250, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 45128, // Boss->player, no cast, single-target
    _Weaponskill_SteelsbreathRelease = 48136, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_MaleficQuartering = 46647, // Boss->self, 3.0s cast, single-target
    _Weaponskill_MaleficInfluence = 46651, // Helper->player, no cast, single-target
    _Weaponskill_MaleficInfluence1 = 46650, // Helper->player, no cast, single-target
    _Weaponskill_MaleficInfluence2 = 46649, // Helper->player, no cast, single-target
    _Weaponskill_MaleficInfluence3 = 46648, // Helper->player, no cast, single-target
    _Weaponskill_MaleficPortent = 46652, // Boss->self, 7.0s cast, single-target
    _Weaponskill_MaleficPortent1 = 46653, // Helper->player, no cast, single-target
    _Ability_ = 46607, // Boss->location, no cast, single-target
    _Weaponskill_LashOfLight = 46655, // Helper->self, 4.0s cast, range 40 90-degree cone
    _Weaponskill_ShiftingHorizon = 46654, // Boss->self, 4.0s cast, single-target
    _Weaponskill_UnyieldingWill = 48652, // 4B19->4B1E, 6.9s cast, width 4 rect charge
    _Weaponskill_UnyieldingWill1 = 46656, // 4B19->location, no cast, width 4 rect charge
    _Weaponskill_UnyieldingWill2 = 46657, // 4B19->player, no cast, width 4 rect charge
    _Weaponskill_FarFromHeaven = 47565, // Boss->self, 7.0s cast, single-target
    _Weaponskill_HeavensConfluence = 46663, // Helper->self, 7.0s cast, range 8-60 donut
    _Weaponskill_FarFromHeaven1 = 46661, // Boss->players, no cast, range 5 circle
    _Weaponskill_HeavensConfluence1 = 46664, // Helper->self, 9.0s cast, range 8 circle
    _Weaponskill_EchoingHeat = 46666, // Boss->self, 5.0s cast, single-target
    _Weaponskill_EchoingHush = 46747, // Helper->location, 7.5s cast, range 8 circle
    _Weaponskill_WolfsCrossing = 46668, // Boss->self, 4.0s cast, single-target
    _Weaponskill_WolfsCrossing1 = 46669, // Helper->self, 5.0s cast, range 40 width 8 cross
    _Weaponskill_EchoingEight = 46670, // Boss->self, 5.0s cast, single-target
    _Weaponskill_EchoingHush1 = 46667, // Helper->location, 4.0s cast, range 8 circle
    _Weaponskill_EchoingEight1 = 46671, // Helper->self, 3.0s cast, range 40 width 8 cross
    _Weaponskill_FarFromHeaven2 = 47563, // Boss->self, 7.0s cast, single-target
    _Weaponskill_FarFromHeaven3 = 46659, // Boss->players, no cast, range 5 circle
    _Weaponskill_StingOfTheScorpion = 46646, // Boss->player, 5.0s cast, single-target
    _Weaponskill_MaleficAlignment = 46672, // Boss->self, 3.0+1.0s cast, single-target
    _Weaponskill_MaleficAlignment1 = 46673, // Helper->self, 4.0s cast, range 40 ?-degree cone
    _Weaponskill_CardinalHorizons = 46674, // Boss->self, 4.0s cast, single-target
    _Weaponskill_WillOfTheUnderworld = 47762, // 4C3D->self, 6.0s cast, range 40 width 20 rect
    _Weaponskill_WaitingWounds = 46676, // Helper->location, 6.0s cast, range 10 circle
    _Weaponskill_WaitingWounds1 = 46675, // Boss->self, 6.0s cast, single-target
    _Weaponskill_SilentEight = 46677, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ResoundingSilence = 46678, // Helper->players, no cast, range 8 circle
    _Weaponskill_MawOfTheWolf = 46679, // Boss->self, 3.4+1.6s cast, single-target
    _Weaponskill_MawOfTheWolf1 = 46680, // Helper->self, 5.0s cast, range 80 width 80 rect
    _Weaponskill_SteelsbreathRelease1 = 46681, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_VanishingHorizon = 46682, // Boss->self, 4.0s cast, single-target
    _Weaponskill_SteelsbreathBonds = 46684, // Boss->self, no cast, single-target
    _Weaponskill_WillOfTheUnderworld1 = 46683, // 4B18->self, 6.0s cast, range 40 width 10 rect
    _Weaponskill_NearToHeaven = 47564, // Boss->self, 7.0s cast, single-target
    _Weaponskill_HeavensConfluence2 = 46662, // Helper->self, 7.0s cast, range 8 circle
    _Weaponskill_NearToHeaven1 = 46660, // Boss->players, no cast, range 5 circle
    _Weaponskill_HeavensConfluence3 = 46665, // Helper->self, 9.0s cast, range 8-60 donut
    _Weaponskill_NearToHeaven2 = 47562, // Boss->self, 7.0s cast, single-target
    _Weaponskill_NearToHeaven3 = 46658, // Boss->player, no cast, range 5 circle
    _Weaponskill_SteelScream = 46685, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    _Gen_MaleficE = 4773, // none->player, extra=0x0
    _Gen_MaleficW = 4774, // none->player, extra=0x0
    _Gen_MaleficEW = 4775, // none->player, extra=0x0
    _Gen_MaleficS = 4776, // none->player, extra=0x0
    _Gen_MaleficSE = 4777, // none->player, extra=0x0
    _Gen_MaleficSW = 4778, // none->player, extra=0x0
    _Gen_MaleficSEW = 4779, // none->player, extra=0x0
    _Gen_MaleficN = 4780, // none->player, extra=0x0
    _Gen_MaleficNE = 4781, // none->player, extra=0x0
    _Gen_MaleficNW = 4782, // none->player, extra=0x0
    _Gen_MaleficNEW = 4783, // none->player, extra=0x0
    _Gen_MaleficNS = 4784, // none->player, extra=0x0
    _Gen_MaleficNSE = 4785, // none->player, extra=0x0
    _Gen_MaleficNSW = 4786, // none->player, extra=0x0
    _Gen_MaleficNSEW = 4787, // none->player, extra=0x0
    _Gen_Bind = 2518, // none->player, extra=0x0
    _Gen_PhysicalVulnerabilityUp = 2940, // Helper->player, extra=0x0
    _Gen_Bleeding = 3077, // none->player, extra=0x0
    _Gen_Bleeding1 = 3078, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0489trg_a0c = 136, // player->self
    _Gen_Icon_lockon7_line_1p = 648, // player->self
    _Gen_Icon_tank_lockon02k1 = 218, // player->self
    _Gen_Icon_loc08sp_05a_se_c2 = 499, // player->self
    _Gen_Icon_d1086_f_cn_t0p = 653, // player->self
    _Gen_Icon_m0489trg_b0c = 137, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_ambd_s_p = 360, // player->Boss
    _Gen_Tether_chn_ambd_n_p = 357, // player->Boss
    _Gen_Tether_chn_d1086_gkn_p = 371, // _Gen_ForceOfWill1/_Gen_ForceOfWill->_Gen_ForceOfWill/player
    _Gen_Tether_chn_m0731_0m1 = 163, // player->player
    _Gen_Tether_chn_ambd_e_p = 358, // player->Boss
    _Gen_Tether_chn_ambd_w_p = 359, // player->Boss
}

class V02LoneSwordmasterStates : StateMachineBuilder
{
    public V02LoneSwordmasterStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14323, DevOnly = true)]
public class V02LoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(170, -815), new ArenaBoundsSquare(20));

