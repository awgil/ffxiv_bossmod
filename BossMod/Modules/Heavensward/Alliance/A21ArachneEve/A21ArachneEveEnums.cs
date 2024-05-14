namespace BossMod.Heavensward.Alliance.A21ArachneEve;

public enum OID : uint
{
    Boss = 0x1637, // R7.500, x?
    ArachneEveHelper = 0x1677, // R0.500, x?, 523 type
    Keyknot = 0x1676, // R1.500, x?
    Webmaiden = 0x1672, // R3.600, x?
    EarthAether = 0x1675, // R1.000, x?
    DeepEarthAether = 0x167B, // R2.000, x?
    SpittingSpider = 0x1673, // R3.000, x?
    SkitteringSpider = 0x1674, // R3.000, x?
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss/SpittingSpider/SkitteringSpider->player, no cast, single-target
    AutoAttack2 = 870, // Webmaiden->player, no cast, single-target

    ArachneWeb = 6238, // Boss->location, no cast, range 6 circle
    BitterBile = 6214, // SpittingSpider->self, no cast, single-target

    DarkSpike = 6179, // Boss->player, 2.5s cast, single-target

    DigestiveFluid = 6208, // SpittingSpider->player, 1.0s cast, single-target
    FrondAffeared = 6202, // Boss->self, 3.0s cast, range 60 circle
    Hatch = 6209, // EarthAether->self, no cast, range 4+R circle
    Implosion = 6195, // Boss->self, 10.0s cast, range 80 circle
    Pitfall1 = 6181, // Boss->self, 5.5s cast, range 60 circle
    Pitfall2 = 6213, // Boss->self, no cast, range 60 circle
    RealmShaker = 6206, // Webmaiden->self, no cast, range 6+R circle

    ShadowBurst = 6200, // Boss->players, 4.0s cast, range 8 circle
    SilkenSpray = 6180, // Boss->self, 2.5s cast, range 17+R 60-degree cone

    Silkscreen = 6204, // Webmaiden->self, 2.5s cast, range 15+R width 4 rect
    SpiderThread = 6201, // SkitteringSpider->player, 5.0s cast, range 6 circle
    StickyWicket1 = 6215, // Boss->self, no cast, single-target
    StickyWicket2 = 6207, // ArachneEveHelper->players, no cast, ???
    TheWidowsEmbrace = 6280, // ArachneEveHelper->self, no cast, range 60 circle
    TheWidowsKiss = 6210, // ArachneEveHelper->self, no cast, range 60 circle
    Tremblor1 = 6199, // ArachneEveHelper->self, 2.5s cast, range 10+R circle
    Tremblor2 = 6198, // ArachneEveHelper->self, 2.5s cast, range 20+R circle
    Tremblor3 = 6197, // ArachneEveHelper->self, 2.5s cast, range 30+R circle
    Unknown = 6281, // ArachneEveHelper->self, 4.0s cast, range 60+R circle
}

public enum SID : uint
{
    Heavy = 14, // Boss->player, extra=0x1E
    DigestiveFluid = 1073, // none->player, extra=0x1E/0x32
    Seized = 961, // ArachneEveHelper->player, extra=0x0
    MeatAndMead = 360, // none->player, extra=0xA
    ProperCare = 362, // none->player, extra=0x14
    Digesting = 645, // SpittingSpider->player, extra=0x1
    VulnerabilityUp = 202, // ArachneEveHelper->player, extra=0x1/0x2
    Hysteria = 296, // Boss->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    DamageUp = 443, // none->ArachneEveHelper/Boss, extra=0x1/0x2

}

public enum IconID : uint
{
    Icon_23 = 23, // player
    Icon_60 = 60, // player
    Icon_13 = 13, // player
    Icon_62 = 62, // player
}

public enum TetherID : uint
{
    Tether_44 = 44, // player->player
    Tether_49 = 49, // SkitteringSpider->player
}