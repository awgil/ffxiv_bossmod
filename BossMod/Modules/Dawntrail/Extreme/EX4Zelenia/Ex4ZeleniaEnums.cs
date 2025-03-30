namespace BossMod.Modules.Dawntrail.Extreme.EX4Zelenia;

public enum OID : uint
{
    ZeleniasShade = 0x47CA, // R5.500, x0-7
    RosebloodDrop = 0x485B, // R1.000, x0-1
    BriarThorn = 0x47C7, // R1.000, x0-6
    Boss = 0x47C6, // R5.500, x0-1
    Helper = 0x233C, // R0.500, x18-35, Helper type
    RosebloodDrop1 = 0x47C2 // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 43242, // Boss->player, no cast, single-target
    ThornedCatharsis = 43166, // Boss->self, 5.0s cast, range 50 circle
    _ = 43054, // Boss->location, no cast, single-target
    Shock = 43169, // Boss/47CA->self, 3.0s cast, single-target
    AlexandrianHoly = 43200, // Boss/47CA->self, 3.0s cast, single-target
    Shock1 = 43178, // Helper->self, no cast, range ?-6 donut
    Shock2 = 43171, // Helper->self, no cast, range 4 circle
    Shock3 = 43177, // Helper->self, no cast, single-target
    Shock4 = 43170, // Helper->self, no cast, single-target
    Shock5 = 43179, // Helper->self, no cast, range ?-6 donut
    Shock6 = 43172, // Helper->self, no cast, range 4 circle
    Explosion = 43226, // Helper->self, 7.0s cast, range 3 circle
    Shock7 = 43173, // Helper->self, no cast, range 4 circle
    Shock8 = 43174, // Helper->self, no cast, range 4 circle
    Shock9 = 43175, // Helper->self, no cast, range 4 circle
    Shock10 = 43176, // Helper->self, no cast, range 4 circle
    SpecterOfTheLost = 43167, // Boss->self, 7.0s cast, single-target
    SpecterOfTheLostHelper = 43168, // Helper->players, 0.7s cast, range 48 ?-degree cone
    EscelonsFall1 = 43181, // Boss->self, 13.0s cast, single-target
    EscelonsFall1Helper = 43183, // Helper->player, no cast, range 24 ?-degree cone
    EscelonsFall2 = 43182, // Boss->self, no cast, single-target
    StockBreak = 43221, // Boss->self, 7.0s cast, single-target
    StockBreakHelper1 = 43222, // Helper->location, no cast, range 6 circle
    StockBreakHelper2 = 43223, // Helper->location, no cast, range 6 circle
    StockBreakHelper3 = 43224, // Helper->location, no cast, range 6 circle
    StockBreakHelper4 = 43225, // Helper->location, no cast, range 6 circle
    BlessedBarricade = 43189, // Boss->self, 3.0s cast, single-target
    SpearpointPush = 43187, // 47CA->location, 1.5+0.7s cast, range 33 width 74 rect
    Explosion1 = 43068, // Helper->self, 6.0s cast, range 3 circle
    SpearpointPush1 = 43188, // 47CA->location, 1.5+0.7s cast, range 33 width 74 rect
    UnmitigatedExplosion = 43069, // Helper->self, no cast, range 50 circle
    PerfumedQuietus = 43191, // Boss->self, 3.0+6.2s cast, range 50 circle
    PerfumedQuietus1 = 43213, // Helper->self, 9.2s cast, range 50 circle
    RosebloodBloom = 43193, // Boss->self, 2.6+0.4s cast, single-target
    QueensCrusade = 43194, // Helper->self, 3.7s cast, range 2 circle
    AlexandrianThunderII = 43198, // Boss->self, 5.0s cast, single-target
    AlexandrianThunderIIHelper = 43199, // Helper->self, 5.7s cast, ???
    AlexandrianThunderII1 = 43064, // Helper->self, no cast, ???
    AlexandrianThunderIII = 43235, // Boss->self, 4.3s cast, single-target
    AlexandrianThunderIII1 = 43236, // Helper->player, 5.0s cast, ???
    Roseblood2NdBloom = 43540, // Boss->self, 2.6+0.4s cast, single-target
    ThunderSlash = 43448, // Boss->self, 6.0+0.7s cast, single-target
    ThunderSlashHelper = 43216, // Helper->self, 6.7s cast, range 24 60-degree cone
    AlexandrianThunderIV = 43450, // Helper->self, 6.7s cast, ???
    AlexandrianThunderIV1 = 43451, // Helper->self, 6.7s cast, ???
    Roseblood3RdBloom = 43541, // Boss->self, 2.6+0.4s cast, single-target
    BudOfValor = 43186, // Boss->self, 3.0s cast, single-target
    Emblazon = 43195, // Boss->self, 3.0s cast, single-target
    EmblazonHelper = 43040, // Helper->player, no cast, single-target
    Explosion2 = 43201, // Helper->self, 13.0s cast, ???
    AlexandrianBanishII = 43217, // 47CA->self, 5.0s cast, single-target
    AlexandrianBanishII1 = 43218, // Helper->players, no cast, ???
    Roseblood4ThBloom = 43542, // Boss->self, 2.6+0.4s cast, single-target
    AlexandrianThunderIII2 = 43238, // Helper->location, 7.5s cast, ???
    EncirclingThorns = 43203, // Boss->self, 4.0s cast, single-target
    AlexandrianBanishIII = 43240, // Boss->self, 4.0s cast, single-target
    AlexandrianBanishIII1 = 43241, // Helper->players, no cast, ???
    PowerBreak = 43184, // 47CA->self, 6.0s cast, range 24 width 64 rect
    PowerBreak1 = 43185, // 47CA->self, 6.0s cast, range 24 width 64 rect
    Roseblood5ThBloom = 43543, // Boss->self, 2.6+0.4s cast, single-target
    ValorousAscension = 43206, // Boss->self, 3.0s cast, single-target
    ValorousAscension1 = 43207, // Helper->self, 3.7s cast, range 50 circle
    ValorousAscension2 = 43208, // Helper->self, no cast, range 50 circle
    ValorousAscension3 = 43209, // Helper->self, no cast, range 50 circle
    ValorousAscension4 = 43210, // 47C7->self, 5.0s cast, range 40 width 8 rect
    Roseblood6ThBloom = 43544, // Boss->self, 2.6+0.4s cast, single-target
    HolyHazard = 43233, // Boss->self, 7.0+0.7s cast, single-target
    HolyHazardHelper = 43234, // Helper->self, 7.7s cast, range 24 ?-degree cone
    HolyHazard1 = 43065, // Boss->self, no cast, single-target
}
public enum SID : uint
{
    LightResistanceDownII = 4164, // Helper->player, extra=0x0
    _W = 2970, // none->Boss, extra=0x2F6/0x2F7
    SlashingResistanceDown = 3130, // Helper->player, extra=0x0
    VulnerabilityUp = 1789, // 47CA/Helper/47C7->player, extra=0x1/0x2/0x3
    Bleeding1 = 4137, // none->player, extra=0x0
    Bleeding2 = 4138, // none->player, extra=0x0
    LightningResistanceDownII = 2998, // Helper->player, extra=0x0
    MagicVulnerabilityUp = 3414, // Helper->player, extra=0x0
    ThornyVine = 445, // none->player, extra=0x0

}
public enum IconID : uint
{
    Icon_580 = 580, // player->self
    Icon_581 = 581, // player->self
    Icon_590 = 590, // player->self
    Icon_23 = 23, // player->self
    Icon_168 = 168, // Boss->self
    Icon_596 = 596, // player->self
    Icon_592 = 592, // player->self
    Icon_93 = 93, // player->self
    Icon_12 = 12, // player->self
    Icon_597 = 597, // player->self
}
public enum TetherID : uint
{
    Tether_89 = 89, // player->Boss
    Tether_17 = 17, // 47CA->player
    Tether_18 = 18, // player->player
}
