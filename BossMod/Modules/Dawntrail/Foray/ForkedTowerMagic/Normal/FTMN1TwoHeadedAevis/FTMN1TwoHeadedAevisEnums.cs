namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x16, Helper type
    UnknownActor = 0x4C24, // R1.000, x2
    GreenHead = 0x4C12, // R15.000, x1
    BlueHead = 0x4C13, // R15.000, x1
    BlueHead1 = 0x4C15, // R1.000, x1
    TwoHeadedAevis = 0x4C11, // R18.000, x1
    Actor1ea1a1 = 0x1EA1A1, // R0.500-2.000, x3, EventObj type
    GreenHead1 = 0x4C14, // R1.000, x1
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    SwirlingOrb = 0x4C17, // R2.800, x0 (spawn during fight)
    BallLightning = 0x4C16, // R2.400, x0 (spawn during fight)
    ArcaneFont = 0x4B73, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    Ability_ = 47614, // TwoHeadedAevis->self, 7.2+0.8s cast, single-target
    Ability_1 = 48243, // Helper->location, 8.0s cast, range 30 circle
    Buffet = 49726, // BlueHead/GreenHead->self, 5.0s cast, single-target
    PoisonBreath = 47617, // Helper->location, 8.0s cast, range 18 circle
    Ability_PoisonBreath1 = 50715, // BlueHead->self, 8.0s cast, single-target
    StormsBreathCast = 47613, // GreenHead->self, 8.0s cast, single-target
    StormsBreath = 47616, // Helper->location, 8.0s cast, ???
    Weaponskill_Aethersplit = 48642, // GreenHead1->BlueHead1, no cast, single-target
    AutoAttack_ = 47754, // BlueHead1->player, no cast, single-target
    AutoAttack_1 = 47753, // GreenHead1->player, no cast, single-target
    Ability_2 = 50710, // Helper->player, no cast, single-target
    Ability_3 = 50709, // Helper->player, no cast, single-target
    ThunderfrostTempest = 47735, // GreenHead/BlueHead->self, 5.0s cast, single-target
    Ability_4 = 47736, // TwoHeadedAevis->self, 5.0s cast, single-target
    Ability_ThunderfrostTempest1 = 47737, // Helper->self, no cast, ???
    Ability_ThunderfrostTempest2 = 47738, // Helper->self, no cast, ???
    Ability_5 = 50656, // TwoHeadedAevis->self, 5.0s cast, single-target
    Ability_TwoTerrors = 50655, // GreenHead/BlueHead->self, 6.0s cast, single-target
    TwoTerrors = 50658, // Helper->self, 6.0s cast, range 40 width 10 rect
    Ability_6 = 50657, // TwoHeadedAevis->self, 5.0s cast, single-target
    HissingReprise = 49722, // GreenHead/BlueHead->self, 3.0s cast, single-target
    Ability_7 = 49723, // TwoHeadedAevis->self, 3.0s cast, single-target
    BuffetEastern= 49724, // Helper->self, no cast, ???
    BuffetWestern = 49725, // Helper->self, no cast, ???
    Ability_Summon = 47704, // GreenHead/BlueHead->self, 3.0s cast, single-target
    Ability_8 = 47705, // TwoHeadedAevis->self, 3.0s cast, single-target
    Ability_9 = 47643, // TwoHeadedAevis->self, 7.4s cast, single-target
    Ability_IceCluster = 47645, // BlueHead1->location, 8.0s cast, single-target
    LightningCluster = 50697, // Helper->location, 8.0s cast, range 15 circle
    Ability_LightningCluster1 = 47642, // GreenHead->self, 8.0s cast, single-target
    IceCluster = 50698, // Helper->location, 8.0s cast, range 15 circle
    Ability_LightningCluster2 = 47644, // GreenHead1->location, 8.0s cast, single-target
    Ability_IceCluster2 = 48220, // BlueHead->self, 8.0s cast, single-target
    HypothermalCombustion = 47707, // SwirlingOrb->self, 2.0s cast, range 15 circle
    Shock = 47706, // BallLightning->self, 2.0s cast, range 15 circle
    Ability_10 = 47656, // TwoHeadedAevis->self, 5.3s cast, single-target
    Ability_Blaze = 47659, // BlueHead1->location, 6.0s cast, single-target
    Blaze1 = 50703, // Helper->location, 6.0s cast, range 5 circle
    Blaze2 = 50704, // Helper->location, 6.0s cast, range 5 circle
    Blaze3 = 50705, // Helper->location, 6.0s cast, range 5 circle
    Ability_Blazeloop = 47654, // BlueHead->self, 6.0s cast, single-target
    Blazeloop = 47660, // Helper->self, 2.5s cast, range 5-60 donut
    Ability_Blaze2 = 47663, // BlueHead1->location, 6.0s cast, single-target
    Ability_Blazeloop2 = 47661, // BlueHead->self, 6.0s cast, single-target
    Ability_Blazeloop3 = 47662, // GreenHead->self, 5.3+0.7s cast, single-target
    Ability_11 = 47657, // TwoHeadedAevis->self, no cast, single-target
    Ability_Blaze4 = 47664, // GreenHead1->location, 6.0s cast, single-target
    Ability_ArcaneRevelation = 49716, // GreenHead/BlueHead->self, 3.0s cast, single-target
    Ability_12 = 49717, // TwoHeadedAevis->self, 3.0s cast, single-target
    ArcaneBeacon = 49720, // ArcaneFont->self, 4.0s cast, range 60 width 5 rect
    Ability_13 = 47615, // TwoHeadedAevis->self, 7.2+0.8s cast, single-target
    _Ability_ = 49727, // TwoHeadedAevis->self, 5.0s cast, single-target
    _Ability_1 = 47655, // TwoHeadedAevis->self, 5.3s cast, single-target
    _Ability_2 = 47658, // TwoHeadedAevis->self, no cast, single-target
    _Ability_Archaeofury = 47745, // BlueHead/GreenHead->self, 5.0s cast, single-target
    Archaeofury1 = 47747, // Helper->player, 5.0s cast, range 6 circle
    Archaeofury2 = 47748, // Helper->player, 5.0s cast, range 6 circle
    _Ability_3 = 47746, // TwoHeadedAevis->self, 5.0s cast, single-target
}

public enum SID : uint
{
    UnknownStatus = 2552, // none->TwoHeadedAevis, extra=0x470/0x471
    EpicHero = 4192, // none->player, extra=0x0
    EpicVillain = 5400, // none->GreenHead, extra=0x0
    FatedHero = 4194, // none->player, extra=0x0
    FatedVillain = 5401, // none->BlueHead, extra=0x0
    EasterlyReprise = 5403, // none->player, extra=0x0
    WesterlyReprise = 5404, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockonae_6m_5s_01t = 344, // player->self
    Icon_m0811trg02t0a1 = 585, // player->self
}

public enum TetherID : uint
{
    Tether_chn_m0560_0t2 = 411, // GreenHead1/BlueHead1->UnknownActor
    Tether_chn_m0560_elc_0t2 = 412, // GreenHead1->UnknownActor
    Tether_chn_m0560_ice_0t2 = 413, // BlueHead1->UnknownActor
    Buffet = 429, // player->BlueHead1/GreenHead1
}
