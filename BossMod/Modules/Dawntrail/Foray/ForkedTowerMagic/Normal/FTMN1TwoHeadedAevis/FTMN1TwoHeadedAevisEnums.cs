namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

public enum OID : uint
{
    TwoHeadedAevis = 0x4C11, // R18.000, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    GreenHead = 0x4C12, // R15.000, x1
    BlueHead = 0x4C13, // R15.000, x1
    GreenHead1 = 0x4C14, // R1.000, x1
    BlueHead1 = 0x4C15, // R1.000, x1
    SwirlingOrb = 0x4C17, // R2.800, x0 (spawn during fight)
    BallLightning = 0x4C16, // R2.400, x0 (spawn during fight)
    ArcaneFont = 0x4B73, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    Buffet = 49726, // BlueHead/GreenHead->self, 5.0s cast, single-target
    PoisonBreath = 47617, // Helper->location, 8.0s cast, range 18 circle
    StormsBreathCast = 47613, // GreenHead->self, 8.0s cast, single-target
    StormsBreath = 47616, // Helper->location, 8.0s cast, ???
    TwoTerrors = 50658, // Helper->self, 6.0s cast, range 40 width 10 rect

    HissingReprise = 49722, // GreenHead/BlueHead->self, 3.0s cast, single-target
    BuffetEastern = 49724, // Helper->self, no cast, ???
    BuffetWestern = 49725, // Helper->self, no cast, ???

    LightningCluster = 50697, // Helper->location, 8.0s cast, range 15 circle
    IceCluster = 50698, // Helper->location, 8.0s cast, range 15 circle
    HypothermalCombustion = 47707, // SwirlingOrb->self, 2.0s cast, range 15 circle
    ThunderfrostTempest = 47735, // GreenHead/BlueHead->self, 5.0s cast, single-target
    Shock = 47706, // BallLightning->self, 2.0s cast, range 15 circle

    Blaze1 = 50703, // Helper->location, 6.0s cast, range 5 circle
    Blaze2 = 50704, // Helper->location, 6.0s cast, range 5 circle
    Blaze3 = 50705, // Helper->location, 6.0s cast, range 5 circle
    Blazeloop = 47660, // Helper->self, 2.5s cast, range 5-60 donut

    ArcaneBeacon = 49720, // ArcaneFont->self, 4.0s cast, range 60 width 5 rect

    Archaeofury1 = 47747, // Helper->player, 5.0s cast, range 6 circle
    Archaeofury2 = 47748, // Helper->player, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    EpicHero = 4192, // none->player, extra=0x0
    EpicVillain = 5400, // none->GreenHead, extra=0x0
    FatedHero = 4194, // none->player, extra=0x0
    FatedVillain = 5401, // none->BlueHead, extra=0x0
    EasterlyReprise = 5403, // none->player, extra=0x0
    WesterlyReprise = 5404, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 344, // player->self
    KnockbackTimer = 585, // player->self
}

public enum TetherID : uint
{
    Tether_chn_m0560_0t2 = 411, // GreenHead1/BlueHead1->UnknownActor
    Tether_chn_m0560_elc_0t2 = 412, // GreenHead1->UnknownActor
    Tether_chn_m0560_ice_0t2 = 413, // BlueHead1->UnknownActor
    Buffet = 429, // player->BlueHead1/GreenHead1
}
