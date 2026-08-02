namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x16, Helper type
    UnknownActor = 0x4C24, // R1.000, x2
    GreenHead1 = 0x4C12, // R15.000, x1
    BlueHead1 = 0x4C13, // R15.000, x1
    BlueHead2 = 0x4C15, // R1.000, x1
    TwoHeadedAevis = 0x4C11, // R18.000, x1
    Actor1ea1a1 = 0x1EA1A1, // R0.500-2.000, x3, EventObj type
    GreenHead2 = 0x4C14, // R1.000, x1
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
    Ability_StormsBreath = 47616, // Helper->location, 8.0s cast, ???
    Ability_StormsBreath1 = 47613, // GreenHead1->self, 8.0s cast, single-target
    Weaponskill_Aethersplit = 48642, // GreenHead2->BlueHead2, no cast, single-target
    AutoAttack_ = 47754, // BlueHead2->player, no cast, single-target
    AutoAttack_1 = 47753, // GreenHead2->player, no cast, single-target
    Ability_2 = 50710, // Helper->player, no cast, single-target
    Ability_3 = 50709, // Helper->player, no cast, single-target
    Ability_ThunderfrostTempest = 47735, // GreenHead1/BlueHead1->self, 5.0s cast, single-target
    Ability_4 = 47736, // TwoHeadedAevis->self, 5.0s cast, single-target
    Ability_ThunderfrostTempest1 = 47737, // Helper->self, no cast, ???
    Ability_ThunderfrostTempest2 = 47738, // Helper->self, no cast, ???
    Ability_5 = 50656, // TwoHeadedAevis->self, 5.0s cast, single-target
    Ability_TwoTerrors = 50655, // GreenHead1/BlueHead1->self, 6.0s cast, single-target
    Ability_TwoTerrors1 = 50658, // Helper->self, 6.0s cast, range 40 width 10 rect
    Ability_6 = 50657, // TwoHeadedAevis->self, 5.0s cast, single-target
    Ability_HissingReprise = 49722, // GreenHead1/BlueHead1->self, 3.0s cast, single-target
    Ability_7 = 49723, // TwoHeadedAevis->self, 3.0s cast, single-target
    Ability_Buffet = 49725, // Helper->self, no cast, ???
    Ability_Buffet1 = 49724, // Helper->self, no cast, ???
    Ability_Summon = 47704, // GreenHead1/BlueHead1->self, 3.0s cast, single-target
    Ability_8 = 47705, // TwoHeadedAevis->self, 3.0s cast, single-target
    Ability_9 = 47643, // TwoHeadedAevis->self, 7.4s cast, single-target
    Ability_IceCluster = 47645, // BlueHead2->location, 8.0s cast, single-target
    Ability_LightningCluster = 50697, // Helper->location, 8.0s cast, range 15 circle
    Ability_LightningCluster1 = 47642, // GreenHead1->self, 8.0s cast, single-target
    Ability_IceCluster1 = 50698, // Helper->location, 8.0s cast, range 15 circle
    Ability_LightningCluster2 = 47644, // GreenHead2->location, 8.0s cast, single-target
    Ability_IceCluster2 = 48220, // BlueHead1->self, 8.0s cast, single-target
    Ability_HypothermalCombustion = 47707, // SwirlingOrb->self, 2.0s cast, range 15 circle
    Ability_Shock = 47706, // BallLightning->self, 2.0s cast, range 15 circle
    Ability_10 = 47656, // TwoHeadedAevis->self, 5.3s cast, single-target
    Ability_Blaze = 47659, // BlueHead2->location, 6.0s cast, single-target
    Ability_Blaze1 = 50703, // Helper->location, 6.0s cast, range 5 circle
    Ability_Blazeloop = 47654, // BlueHead1->self, 6.0s cast, single-target
    Ability_Blazeloop1 = 47660, // Helper->self, 2.5s cast, range 5-60 donut
    Ability_Blaze2 = 47663, // BlueHead2->location, 6.0s cast, single-target
    Ability_Blaze3 = 50704, // Helper->location, 6.0s cast, range 5 circle
    Ability_Blazeloop2 = 47661, // BlueHead1->self, 6.0s cast, single-target
    Ability_Blazeloop3 = 47662, // GreenHead1->self, 5.3+0.7s cast, single-target
    Ability_11 = 47657, // TwoHeadedAevis->self, no cast, single-target
    Ability_Blaze4 = 47664, // GreenHead2->location, 6.0s cast, single-target
    Ability_Blaze5 = 50705, // Helper->location, 6.0s cast, range 5 circle
    Ability_ArcaneRevelation = 49716, // GreenHead1/BlueHead1->self, 3.0s cast, single-target
    Ability_12 = 49717, // TwoHeadedAevis->self, 3.0s cast, single-target
    Ability_ArcaneBeacon = 49720, // ArcaneFont->self, 4.0s cast, range 60 width 5 rect
    Ability_13 = 47615, // TwoHeadedAevis->self, 7.2+0.8s cast, single-target
    Ability_PoisonBreath = 47617, // Helper->location, 8.0s cast, range 18 circle
    Ability_PoisonBreath1 = 50715, // BlueHead1->self, 8.0s cast, single-target
}

public enum SID : uint
{
    UnknownStatus = 2552, // none->TwoHeadedAevis, extra=0x470/0x471
    VulnerabilityUp = 2347, // Helper/SwirlingOrb/BallLightning->player, extra=0x1/0x2/0x3/0x4/0x5/0x6
    EasterlyReprise = 5403, // none->player, extra=0x0
    WesterlyReprise = 5404, // none->player, extra=0x0

}

public enum IconID : uint
{
    Icon_m0811trg02t0a1 = 585, // player->self
}

public enum TetherID : uint
{
    Tether_chn_m0560_0t2 = 411, // GreenHead2/BlueHead2->UnknownActor
    Tether_chn_m0560_ice_0t2 = 413, // BlueHead2->UnknownActor
    Tether_chn_m0560_elc_0t2 = 412, // GreenHead2->UnknownActor
}
