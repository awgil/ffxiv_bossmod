namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

public enum OID : uint
{
    Boss = 0x47C6,
    Helper = 0x233C,
    ZeleniasShade = 0x47CA, // R5.500, x?
    RosebloodDrop = 0x485B, // R1.000, x?
    BriarThorn = 0x47C7, // R1.000, x?
    RosebloodDrop1 = 0x47C2, // R1.000, x?
    Unknown = 0x47C8, // R1.000, x?
}

public enum AID : uint
{
    AutoAttack = 43242, // Boss->player, no cast, single-target
    ThornedCatharsis = 43166, // Boss->self, 5.0s cast, range 50 circle
    Dash = 43054, // Boss->location, no cast, single-target
    ShockVisual = 43169, // Boss/ZeleniasShade->self, 3.0s cast, single-target
    AlexandrianHolyVisual = 43200, // Boss/ZeleniasShade->self, 3.0s cast, single-target
    ShockDonut1 = 43178, // Helper->self, no cast, range 1-6 donut
    ShockDonut2 = 43179, // Helper->self, no cast, range 1-6 donut
    ShockCircle1 = 43171, // Helper->self, no cast, range 4 circle
    ShockCircle2 = 43172, // Helper->self, no cast, range 4 circle
    ShockCircle3 = 43173, // Helper->self, no cast, range 4 circle
    ShockCircle4 = 43174, // Helper->self, no cast, range 4 circle
    ShockCircle5 = 43175, // Helper->self, no cast, range 4 circle
    ShockCircle6 = 43176, // Helper->self, no cast, range 4 circle
    UnkShock1 = 43170, // Helper->self, no cast, single-target
    UnkShock2 = 43177, // Helper->self, no cast, single-target
    P1Explosion = 43226, // Helper->self, 7.0s cast, range 3 circle, tower
    SpecterOfTheLostVisual = 43167, // Boss->self, 7.0s cast, single-target
    SpecterOfTheLost = 43168, // Helper->players, 0.7s cast, range 48 60-degree cone
    EscelonsFallVisual = 43181, // Boss->self, 13.0s cast, single-target
    EscelonsFallVisualRepeat = 43182, // Boss->self, no cast, single-target
    EscelonsFall = 43183, // Helper->players, no cast, range 24 45-degree cone

    StockBreak = 43221, // Boss->self, 7.0s cast, single-target
    StockBreakHelper = 43222, // Helper->location, no cast, range 6 circle
    StockBreak1 = 43223, // Helper->location, no cast, range 6 circle
    StockBreak2 = 43224, // Helper->location, no cast, range 6 circle
    StockBreak3 = 43225, // Helper->location, no cast, range 6 circle
    BlessedBarricade = 43189, // Boss->self, 3.0s cast, single-target
    SpearpointPushSide1 = 43187, // ZeleniasShade->location, 1.5+0.7s cast, range 33 width 74 rect
    SpearpointPushSide2 = 43188, // ZeleniasShade->location, 1.5+0.7s cast, range 33 width 74 rect
    AddsExplosion = 43068, // Helper->self, 6.0s cast, range 3 circle, tower

    PerfumedQuietusVisual = 43191, // Boss->self, 3.0+6.2s cast, range 50 circle
    PerfumedQuietus = 43213, // Helper->self, 9.2s cast, range 50 circle

    RosebloodBloom = 43193, // Boss->self, 2.6+0.4s cast, single-target
    QueensCrusade = 43194, // Helper->self, 3.7s cast, range 2 circle, not sure what this does, probably visual only
    AlexandrianThunderIIVisual = 43198, // Boss->self, 5.0s cast, single-target
    AlexandrianThunderIIStart = 43199, // Helper->self, 5.7s cast, range 24 45-degree cone
    AlexandrianThunderIIRepeat = 43064, // Helper->self, no cast, range 24 45-degree cone
    AlexandrianThunderIIIVisual = 43235, // Boss->self, 4.3s cast, single-target, visual
    AlexandrianThunderIII = 43236, // Helper->players, 5.0s cast, range 4 circle, travels through active tiles
    AlexandrianThunderIIIGroundTarget = 43238, // Helper->location, 7.5s cast, range 4 circle, travels through active tiles

    Roseblood2ndBloom = 43540, // Boss->self, 2.6+0.4s cast, single-target
    ThunderSlashVisual1 = 43448, // Boss->self, 6.0+0.7s cast, single-target
    ThunderSlashVisual2 = 43449, // Boss->self, 6.0+0.7s cast, single-target
    ThunderSlash = 43216, // Helper->self, 6.7s cast, range 24 60-degree cone
    AlexandrianThunderIVCircle = 43450, // Helper->self, 6.7s cast, range 8 circle
    AlexandrianThunderIVDonut = 43451, // Helper->self, 6.7s cast, range 8-16 donut

    Roseblood3rdBloom = 43541, // Boss->self, 2.6+0.4s cast, single-target
    BudOfValor = 43186, // Boss->self, 3.0s cast, single-target
    EmblazonVisual = 43195, // Boss->self, 3.0s cast, single-target
    Emblazon = 43040, // Helper->player, no cast, single-target
    TileExplosion = 43201, // Helper->self, 13.0s cast, travels through caster's tile and adjacent active tiles
    AlexandrianBanishIIVisual = 43217, // ZeleniasShade->self, 5.0s cast, single-target
    AlexandrianBanishIIStack = 43218, // Helper->players, no cast, range 5 circle

    Roseblood4thBloom = 43542, // Boss->self, 2.6+0.4s cast, single-target
    EncirclingThornsVisual = 43203, // Boss->self, 4.0s cast, single-target
    AlexandrianBanishIIIVisual = 43240, // Boss->self, 4.0s cast, single-target
    AlexandrianBanishIIIStack = 43241, // Helper->players, no cast, range 4 circle, travels through active tiles
    PowerBreak1 = 43184, // ZeleniasShade->self, 6.0s cast, range 24 width 64 rect
    PowerBreak2 = 43185, // ZeleniasShade->self, 6.0s cast, range 24 width 64 rect

    Roseblood5thBloom = 43543, // Boss->self, 2.6+0.4s cast, single-target
    ValorousAscensionVisual = 43206, // Boss->self, 3.0s cast, single-target
    ValorousAscension1 = 43207, // Helper->self, 3.7s cast, range 50 circle
    ValorousAscension2 = 43208, // Helper->self, no cast, range 50 circle
    ValorousAscension3 = 43209, // Helper->self, no cast, range 50 circle
    ValorousAscensionRect = 43210, // BriarThorn->self, 5.0s cast, range 40 width 8 rect

    Roseblood6thBloom = 43544, // Boss->self, 2.6+0.4s cast, single-target
    HolyHazardVisual1 = 43233, // Boss->self, 7.0+0.7s cast, single-target
    HolyHazardVisual2 = 43231, // Boss->self, 7.0+0.7s cast, single-target
    HolyHazardVisualInstant = 43065, // Boss->self, no cast, single-target
    HolyHazard = 43234, // Helper->self, 7.7s cast, range 24 135-degree cone
    EncirclingThornsDamage = 43204, // Helper->self, no cast, triggered on unstretched tethers after delay
    PerfumedQuietusEnrageVisual = 43192, // Boss->self, 3.0+6.2s cast, range 50 circle
    PerfumedQuietusEnrage = 43214, // Helper->self, 9.2s cast, range 50 circle

    UnmitigatedExplosion = 43069, // Helper->self, no cast, range 50 circle
    UnmitigatedExplosion1 = 43202, // Helper->self, no cast, range 50 circle
    UnmitigatedExplosion2 = 43227, // Helper->self, no cast, range 50 circle
    MagicBreakInner = 43196, // Helper->self, no cast, range 8 circle, instant kill mechanic, triggered when a rose is baited onto an active tile
    MagicBreakOuter = 43197, // Helper->self, no cast, range 8-16 donut, see above
}

public enum IconID : uint
{
    ShockDonut = 580, // player->self
    ShockCircle = 581, // player->self
    StockBreak = 590, // player->self
    ThunderCW = 167, // Boss->self
    ThunderCCW = 168, // Boss->self
    LockOn = 23, // player->self, on tethered players during adds phase
    AlexandrianThunderIII = 596, // player->self, spread
    Rose = 592, // player->self, marks occupied tile on activation
    AlexandrianBanishII = 93, // player->self, stack
    Thorns = 12, // player->self
    AlexandrianBanishIII = 597, // player->self
}

public enum SID : uint
{
    LightResistanceDownII = 4164, // Helper->player, extra=0x0
    WitchHunt = 2970, // none->Boss, extra=0x2F7/0x2F6
    SlashingResistanceDown = 3130, // Helper->player, extra=0x0
    LightningResistanceDownII = 2998, // Helper->player, extra=0x0
    MagicVulnerabilityUp = 3414, // Helper->player, extra=0x0
    ThornyVine = 445, // none->player, extra=0x0
    Unk1 = 2056, // none->47CA/Boss, extra=0x382
    Bleeding1 = 4137, // none->player, extra=0x0
    Bleeding2 = 4138, // none->player, extra=0x0
}

public enum TetherID : uint
{
    SpecterOfTheLost = 89, // player->Boss
    SpearpointPush = 17, // ZeleniasShade->player
    Thorns = 18, // player->player, break distance ~20
}
