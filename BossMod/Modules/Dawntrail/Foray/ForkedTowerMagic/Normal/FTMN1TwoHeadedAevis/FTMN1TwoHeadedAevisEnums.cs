namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

public enum OID : uint {
    TwoHeadedAevis = 0x4C11, // R18.000, x1
    GreenHead1 = 0x4C12, // R15.000, x1
    GreenHead2 = 0x4C14, // R1.000, x1
    BlueHead1 = 0x4C13, // R15.000, x1
    BlueHead2 = 0x4C15, // R1.000, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    SwirlingOrb = 0x4C17, // R2.800, x0 (spawn during fight)
    BallLightning = 0x4C16, // R2.400, x0 (spawn during fight)
    ArcaneFont = 0x4B73, // R1.000, x0 (spawn during fight)

    UnknownActor = 0x4C24, // R1.000, x2
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R0.500-2.000, x3, EventObj type
}

public enum AID : uint {
    AutoAttackBlueHead = 47754, // BlueHead2->player, no cast, single-target
    AutoAttackGreenHead = 47753, // GreenHead2->player, no cast, single-target
    Buffet = 49726, // BlueHead1/GreenHead1->self, 5.0s cast, single-target - Applies debuffs of hero
    Buffet1 = 49727, // TwoHeadedAevis->self, 5.0s cast, single-target
    TwoHeadedAevisPoisonBreath = 47615, // TwoHeadedAevis->self, 7.2+0.8s cast, single-target
    PoisonBreath = 47617, // Helper->location, 8.0s cast, range 18 circle
    PoisonBreath1 = 50715, // BlueHead1->self, 8.0s cast, single-target

    TwoHeadedAevisStormsBreath = 47614, // TwoHeadedAevis->self, 7.2+0.8s cast, single-target
    StormsBreath = 47616, // Helper->location, 8.0s cast, ???
    StormsBreath1 = 47613, // GreenHead1->self, 8.0s cast, single-target
    StormsBreath2 = 48243, // Helper->location, 8.0s cast, range 30 circle

    TwoHeadedAevisThunderfrostTempest= 47736, // TwoHeadedAevis->self, 5.0s cast, single-target
    ThunderfrostTempest = 47735, // GreenHead1/BlueHead1->self, 5.0s cast, single-target
    ThunderfrostTempestVisual = 47737, // Helper->self, no cast, ???
    ThunderfrostTempestVisual1 = 47738, // Helper->self, no cast, ???

    TwoHeadedAevisTwoTerrors = 50656, // TwoHeadedAevis->self, 5.0s cast, single-target
    TwoHeadedAevisTwoTerrors1 = 50657, // TwoHeadedAevis->self, 5.0s cast, single-target
    TwoTerrorsCast = 50655, // GreenHead1/BlueHead1->self, 6.0s cast, single-target
    TwoTerrors = 50658, // Helper->self, 6.0s cast, range 40 width 10 rect

    TwoHeadedAevisHissingReprise = 49723, // TwoHeadedAevis->self, 3.0s cast, single-target
    HissingReprise = 49722, // GreenHead1/BlueHead1->self, 3.0s cast, single-target - Applies EasterlyReprise/WesterlyReprise
    HissingRepriseBuffet = 49725, // Helper->self, no cast, ???
    HissingRepriseBuffet1 = 49724, // Helper->self, no cast, ???

    TwoHeadedAevisSummon = 47705, // TwoHeadedAevis->self, 3.0s cast, single-target
    Summon = 47704, // GreenHead1/BlueHead1->self, 3.0s cast, single-target

    TwoHeadedAevisCluster = 47643, // TwoHeadedAevis->self, 7.4s cast, single-target
    IceCluster = 48220, // BlueHead1->self, 8.0s cast, single-target
    IceClusterTeleport = 50698, // Helper->location, 8.0s cast, range 15 circle
    IceClusterTeleport1 = 47645, // BlueHead2->location, 8.0s cast, single-target
    HypothermalCombustion = 47707, // SwirlingOrb->self, 2.0s cast, range 15 circle

    LightningCluster = 47642, // GreenHead1->self, 8.0s cast, single-target
    LightningClusterTeleport = 50697, // Helper->location, 8.0s cast, range 15 circle
    LightningClusterTeleport1 = 47644, // GreenHead2->location, 8.0s cast, single-target
    Shock = 47706, // BallLightning->self, 2.0s cast, range 15 circle

    TwoHeadedAevisBlaze = 47656, // TwoHeadedAevis->self, 5.3s cast, single-target
    Blaze = 47659, // BlueHead2->location, 6.0s cast, single-target
    Blaze1 = 47663, // BlueHead2->location, 6.0s cast, single-target
    Blaze2 = 47664, // GreenHead2->location, 6.0s cast, single-target
    BlazeInner = 50703, // Helper->location, 6.0s cast, range 5 circle
    BlazeInner1 = 50704, // Helper->location, 6.0s cast, range 5 circle
    BlazeInner2 = 50705, // Helper->location, 6.0s cast, range 5 circle
    Blazeloop = 47654, // BlueHead1->self, 6.0s cast, single-target
    Blazeloop2 = 47661, // BlueHead1->self, 6.0s cast, single-target
    Blazeloop3 = 47662, // GreenHead1->self, 5.3+0.7s cast, single-target
    BlazeloopOuter = 47660, // Helper->self, 2.5s cast, range 5-60 donut

    TwoHeadedAevisArcaneRevelation = 49717, // TwoHeadedAevis->self, 3.0s cast, single-target
    ArcaneRevelation = 49716, // GreenHead1/BlueHead1->self, 3.0s cast, single-target
    ArcaneBeacon = 49720, // ArcaneFont->self, 4.0s cast, range 60 width 5 rect

    // TODO visual stuff / instant casts from actors most likely
    Weaponskill_Aethersplit = 48642, // GreenHead2->BlueHead2, no cast, single-target
    Ability_2 = 50710, // Helper->player, no cast, single-target
    Ability_3 = 50709, // Helper->player, no cast, single-target
    Ability_11 = 47657, // TwoHeadedAevis->self, no cast, single-target
}

public enum SID : uint {
    EpicHero = 4192, // none->player, extra=0x0
    FatedHero = 4194, // none->player, extra=0x0
    FatedVillain = 5401, // none->BlueHead1, extra=0x0
    EpicVillain = 5400, // none->GreenHead1, extra=0x0
    EasterlyReprise = 5403, // none->player, extra=0x0
    WesterlyReprise = 5404, // none->player, extra=0x0
    UnknownStatus = 2552, // none->TwoHeadedAevis, extra=0x470/0x471
}

public enum IconID : uint {
    KnockbackTimer = 585, // player->self
}

public enum TetherID : uint {
    Tether_chn_m0560_0t2 = 411, // GreenHead2/BlueHead2->UnknownActor
    Tether_chn_m0560_ice_0t2 = 413, // BlueHead2->UnknownActor
    Tether_chn_m0560_elc_0t2 = 412, // GreenHead2->UnknownActor
    _Gen_Tether_chn_tergetfix2k1 = 429, // player->GreenHead2/BlueHead2

}
