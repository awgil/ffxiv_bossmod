#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

public enum OID : uint
{
    RedHot = 0x4B57, // R4.000, x1
    DeepBlue = 0x4B58, // R4.000, x1
    Helper = 0x233C, // R0.500, x20-24, Helper type
    _Gen_ = 0x4AE7, // R1.000, x4
    _Gen_TheXtremes = 0x4BDF, // R1.000, x2
    SickSwell = 0x4B5A, // R1.000, x1
    XtremeAether = 0x4B59, // R1.500, x28
    WateryGrave = 0x4AC0, // R1.000, x4
    _Gen_WateryGrave = 0x4B5C, // R4.000, x0 (spawn during fight)

    FlameFloater = 0x1EBF30, // fire puddle, range 60 width 8 rect
    FlamePuddle5 = 0x1EBF31, // fire puddle, range 5 circle
    FlamePuddle6 = 0x1EBF32, // fire puddle, range 6 circle
    CutbackBlaze = 0x1EBF33, // fire puddle, range 60 330-degree cone
    FlameCone = 0x1EBFCE, // fire puddle, range 60 45?-degree cone
}

public enum AID : uint
{
    _AutoAttack_ = 48639, // RedHot->player, no cast, single-target
    _Weaponskill_HotImpact = 46518, // RedHot->players, 5.0s cast, range 6 circle
    _Ability_ = 46456, // RedHot->location, no cast, single-target
    _Weaponskill_FlameFloater = 46522, // RedHot->self, 7.0s cast, single-target
    _Weaponskill_FlameFloater1 = 46523, // RedHot->location, no cast, range 60 width 8 rect
    _Weaponskill_FlameFloater2 = 46524, // RedHot->location, no cast, range 60 width 8 rect
    _Weaponskill_FlameFloater3 = 46525, // RedHot->location, no cast, range 60 width 8 rect
    _Weaponskill_FlameFloater4 = 46526, // RedHot->location, no cast, range 60 width 8 rect
    _Ability_1 = 46460, // RedHot->self, no cast, single-target
    _Weaponskill_AlleyOopInferno = 46528, // RedHot->self, 4.3+0.7s cast, single-target
    _Weaponskill_AlleyOopInferno1 = 46529, // Helper->player, 5.0s cast, range 5 circle
    _Weaponskill_CutbackBlaze = 46537, // RedHot->self, 4.3+0.7s cast, single-target
    _Weaponskill_CutbackBlaze1 = 46538, // Helper->self, no cast, range 60 330-degree cone
    _Weaponskill_Pyrotation = 46530, // RedHot->self, 4.3+0.7s cast, single-target
    _Weaponskill_Pyrotation1 = 46531, // Helper->players, no cast, range 6 circle
    _Weaponskill_DiversDare = 46520, // RedHot->self, 5.0s cast, range 60 circle
    _AutoAttack_1 = 48640, // DeepBlue->player, no cast, single-target
    _Weaponskill_SickSwell = 46539, // DeepBlue->self, 3.0s cast, single-target
    _Weaponskill_SickestTakeOff = 46541, // DeepBlue->self, 4.0s cast, single-target
    _Weaponskill_SickestTakeOff1 = 46542, // Helper->self, 7.0s cast, range 50 width 15 rect
    _Weaponskill_SickSwell1 = 46540, // Helper->self, 7.0s cast, range 50 width 50 rect
    _Weaponskill_AwesomeSplash = 46543, // Helper->players, no cast, range 5 circle
    _Weaponskill_AlleyOopDoubleDip = 46557, // DeepBlue->self, 5.0s cast, single-target
    _Weaponskill_AlleyOopDoubleDip1 = 46558, // Helper->self, no cast, range 60 30-degree cone
    _Weaponskill_AlleyOopDoubleDip2 = 46559, // Helper->self, no cast, range 60 15-degree cone
    _Weaponskill_DeepImpact = 46519, // DeepBlue->self, 4.9s cast, single-target
    _Weaponskill_DeepImpact1 = 44486, // DeepBlue->players, no cast, range 6 circle
    _Weaponskill_DiversDare1 = 46521, // DeepBlue->self, 5.0s cast, range 60 circle
    _Ability_2 = 46457, // DeepBlue->location, no cast, single-target
    _Weaponskill_XtremeSpectacular1 = 46553, // RedHot->self, 4.0s cast, single-target
    _Weaponskill_XtremeSpectacular = 46554, // DeepBlue->self, 4.0s cast, single-target
    _Weaponskill_XtremeSpectacular2 = 46500, // _Gen_TheXtremes->self, 7.4s cast, range 50 width 40 rect
    _Weaponskill_XtremeSpectacular3 = 46556, // _Gen_TheXtremes->self, no cast, range 60 circle
    _Weaponskill_XtremeSpectacular4 = 47050, // _Gen_TheXtremes->self, no cast, range 60 circle
    _Ability_EpicBrotherhood1 = 46458, // RedHot->DeepBlue, no cast, single-target
    _Ability_EpicBrotherhood = 46459, // DeepBlue->RedHot, no cast, single-target
    _Weaponskill_InsaneAir1 = 47255, // RedHot->self, 5.9+1.5s cast, single-target
    _Weaponskill_InsaneAir = 47256, // DeepBlue->self, 5.9+1.5s cast, single-target
    _Weaponskill_VerticalBlast = 46583, // RedHot->self, no cast, single-target
    _Weaponskill_PlungingSnap = 46576, // DeepBlue->self, no cast, single-target
    _Weaponskill_PlungingSnap1 = 46578, // Helper->self, no cast, range 60 45-degree cone
    _Weaponskill_VerticalBlast1 = 46585, // Helper->players, no cast, range 6 circle
    _Weaponskill_InsaneAir3 = 47257, // RedHot->self, 3.9+1.5s cast, single-target
    _Weaponskill_InsaneAir2 = 47258, // DeepBlue->self, 3.9+1.5s cast, single-target
    _Weaponskill_BlastingSnap = 46575, // RedHot->self, no cast, single-target
    _Weaponskill_ReEntryPlunge = 46580, // DeepBlue->self, no cast, single-target
    _Weaponskill_BlastingSnap1 = 46577, // Helper->self, no cast, range 60 45-degree cone
    _Weaponskill_ReEntryPlunge1 = 46582, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_ReEntryBlast = 46579, // RedHot->self, no cast, single-target
    _Weaponskill_ReEntryBlast1 = 46581, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_VerticalPlunge = 46584, // DeepBlue->self, no cast, single-target
    _Weaponskill_VerticalPlunge1 = 46586, // Helper->player, no cast, range 6 circle
    _Ability_3 = 46461, // DeepBlue->self, no cast, single-target
    _Weaponskill_Firesnaking = 45953, // RedHot->self, 5.0s cast, range 60 circle
    _Weaponskill_Watersnaking = 45954, // DeepBlue->self, 5.0s cast, range 60 circle
    _Weaponskill_ReverseAlleyOop = 46560, // DeepBlue->self, 5.0s cast, single-target
    _Weaponskill_ReverseAlleyOop1 = 46561, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_ReverseAlleyOop2 = 46562, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_HotImpact1 = 46464, // RedHot->players, 5.0s cast, range 6 circle
    _Weaponskill_DeepVarial = 47249, // DeepBlue->location, 5.3+1.0s cast, ???
    _Weaponskill_DeepVarial1 = 46547, // Helper->self, 6.8s cast, range 60 120-degree cone
    _Weaponskill_AwesomeSplash1 = 46551, // Helper->player, no cast, range 5 circle
    _Weaponskill_SteamBurst = 46587, // XtremeAether->self, 3.0s cast, range 9 circle
    _Weaponskill_HotAerial = 46532, // RedHot->self, 5.0s cast, single-target
    _Weaponskill_HotAerial1 = 47389, // RedHot->player, no cast, single-target
    _Weaponskill_HotAerial2 = 47390, // Helper->player, no cast, range 6 circle
    _Weaponskill_HotAerial3 = 47391, // Helper->player, no cast, range 6 circle
    _Weaponskill_HotAerial4 = 47392, // Helper->player, no cast, range 6 circle
    _Weaponskill_HotAerial5 = 47393, // Helper->players, no cast, range 6 circle
    _Weaponskill_AwesomeSlab1 = 46544, // Helper->players, no cast, range 6 circle
    _Weaponskill_AwesomeSlab = 46552, // Helper->players, no cast, range 6 circle
    _Weaponskill_DeepAerial = 46563, // DeepBlue->location, 5.0s cast, single-target
    _Weaponskill_DeepAerial1 = 46564, // Helper->self, 6.0s cast, range 6 circle
    _Weaponskill_ = 46570, // WateryGrave->player, no cast, single-target
    _Weaponskill_XtremeWave = 46533, // RedHot->self, 4.9s cast, single-target
    _Weaponskill_XtremeWave1 = 46534, // DeepBlue->self, 4.9s cast, single-target
    _Weaponskill_XtremeWave2 = 46545, // RedHot->location, no cast, range 60 width 8 rect
    _Weaponskill_XtremeWave3 = 46546, // DeepBlue->location, no cast, range 60 width 8 rect
    _Weaponskill_ScathingSteam = 44487, // 4B5C->self, 1.0s cast, range 60 circle
    _Weaponskill_XtremeWave4 = 46536, // DeepBlue->self, 4.9s cast, single-target
    _Weaponskill_XtremeWave5 = 46535, // RedHot->self, 4.9s cast, single-target
    _Weaponskill_ImpactZone = 46572, // 4B5C->self, no cast, range 60 circle
    _Weaponskill_ImpactZone1 = 46571, // WateryGrave->self, no cast, range 60 circle
    _Weaponskill_FlameFloater5 = 46548, // RedHot->self, 5.0s cast, single-target
    _Weaponskill_FlameFloater6 = 46527, // RedHot->location, no cast, range 60 width 8 rect
    _Weaponskill_FreakyPyrotation = 46486, // RedHot->self, 4.3+0.7s cast, single-target
    _Weaponskill_FreakyPyrotation1 = 46487, // Helper->player, no cast, range 6 circle
    _Weaponskill_XtremeFiresnaking = 46510, // RedHot->self, 5.0s cast, range 60 circle
    _Weaponskill_XtremeWatersnaking = 46511, // DeepBlue->self, 5.0s cast, range 60 circle
    _Weaponskill_InsaneAir4 = 46566, // RedHot->self, 6.9+1.5s cast, single-target
    _Weaponskill_InsaneAir5 = 46567, // DeepBlue->self, 6.9+1.5s cast, single-target
    _Weaponskill_Bailout = 46513, // Helper->players, 1.0s cast, range 15 circle
    _Weaponskill_Bailout1 = 46512, // Helper->players, 1.0s cast, range 15 circle
    _Weaponskill_InsaneAir6 = 46568, // RedHot->self, 6.9+1.5s cast, single-target
    _Weaponskill_InsaneAir7 = 46569, // DeepBlue->self, 6.9+1.5s cast, single-target
    _Weaponskill_UnmitigatedExplosion = 46565, // Helper->self, no cast, range 60 circle
    _Weaponskill_OverTheFalls = 46588, // RedHot->self, 9.0s cast, range 60 circle
    _Weaponskill_OverTheFalls1 = 46589, // DeepBlue->self, 9.0s cast, range 60 circle
}

public enum SID : uint
{
    _Gen_DirectionalDisregard = 3808, // none->RedHot/DeepBlue, extra=0x0
    _Gen_FirstInLine = 3004, // none->player, extra=0x0
    _Gen_SecondInLine = 3005, // none->player, extra=0x0
    _Gen_ThirdInLine = 3006, // none->player, extra=0x0
    _Gen_FourthInLine = 3451, // none->player, extra=0x0
    _Gen_FireResistanceDownII = 2937, // RedHot/Helper->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper/RedHot/DeepBlue->player, extra=0x0
    _Gen_ = 2056, // none->DeepBlue, extra=0x3ED/0x3EE/0x3EF/0x3F0/0x435
    _Gen_Watersnaking = 4975, // none->player, extra=0x0
    _Gen_Firesnaking = 4974, // none->player, extra=0x0
    _Gen_Burns = 3065, // none->player, extra=0x0
    _Gen_Burns2 = 3066, // none->player, extra=0x0
    _Gen_Stun = 2656, // none->player, extra=0x0
    _Gen_WateryGrave = 4829, // none->player, extra=0x12C
    _Gen_VulnerabilityDown = 929, // none->_Gen_WateryGrave, extra=0x0
    _Gen_SustainedDamage = 4149, // _Gen_WateryGrave/WateryGrave->player, extra=0x1/0x3
    _Gen_XtremeFiresnaking = 4827, // none->player, extra=0x0
    _Gen_XtremeWatersnaking = 4828, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0676trg_tw_d0t1p = 259, // player->self
    _Gen_Icon_target_ae_5m_s5_fire0c = 660, // player->self
    _Gen_Icon_m0982trg_g0c = 666, // player->self
    _Gen_Icon_m0982trg_c1c = 636, // player->self
    _Gen_Icon_m0982trg_c0c = 635, // player->self
    _Gen_Icon_com_share_fire01s5_0c = 659, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0982_3c = 378, // _Gen_/player->player
    _Gen_Tether_chn_m0982_4c = 379, // player/_Gen_->player/_Gen_
    _Gen_Tether_chn_m0982_2c = 372, // DeepBlue->_Gen_
    _Gen_Tether_chn_arrow01f = 57, // _Gen_->player
    _Gen_Tether_chn_tergetfix1f = 17, // _Gen_->player
}
