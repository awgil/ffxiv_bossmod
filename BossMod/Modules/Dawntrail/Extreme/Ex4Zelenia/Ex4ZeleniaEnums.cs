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
    _AutoAttack_ = 43242, // Boss->player, no cast, single-target
    _Weaponskill_ThornedCatharsis = 43166, // Boss->self, 5.0s cast, range 50 circle
    _Ability_ = 43054, // Boss->location, no cast, single-target
    _Weaponskill_Shock = 43169, // Boss/47CA->self, 3.0s cast, single-target
    _Spell_AlexandrianHoly = 43200, // Boss/47CA->self, 3.0s cast, single-target
    _Weaponskill_ShockDonut1 = 43178, // Helper->self, no cast, range ?-6 donut
    _Weaponskill_ShockDonut2 = 43179, // Helper->self, no cast, range ?-6 donut
    _Weaponskill_ShockCircle1 = 43171, // Helper->self, no cast, range 4 circle
    _Weaponskill_ShockCircle2 = 43172, // Helper->self, no cast, range 4 circle
    _Weaponskill_ShockCircle3 = 43173, // Helper->self, no cast, range 4 circle
    _Weaponskill_ShockCircle4 = 43174, // Helper->self, no cast, range 4 circle
    _Weaponskill_ShockCircle5 = 43175, // Helper->self, no cast, range 4 circle
    _Weaponskill_ShockCircle6 = 43176, // Helper->self, no cast, range 4 circle
    _Weaponskill_Shock2 = 43170, // Helper->self, no cast, single-target
    _Weaponskill_Shock4 = 43177, // Helper->self, no cast, single-target
    _Spell_Explosion = 43226, // Helper->self, 7.0s cast, range 3 circle
    _Weaponskill_SpecterOfTheLost = 43167, // Boss->self, 7.0s cast, single-target
    _Ability_SpecterOfTheLost = 43168, // Helper->players, 0.7s cast, range 48 60-degree cone
    _Weaponskill_EscelonsFall = 43181, // Boss->self, 13.0s cast, single-target
    _Ability_EscelonsFall = 43183, // Helper->players, no cast, range 24 45-degree cone
    _Weaponskill_EscelonsFall1 = 43182, // Boss->self, no cast, single-target
    _Weaponskill_StockBreak = 43221, // Boss->self, 7.0s cast, single-target
    _Ability_StockBreak = 43222, // Helper->location, no cast, range 6 circle
    _Ability_StockBreak1 = 43223, // Helper->location, no cast, range 6 circle
    _Ability_StockBreak2 = 43224, // Helper->location, no cast, range 6 circle
    _Ability_StockBreak3 = 43225, // Helper->location, no cast, range 6 circle
    _Weaponskill_BlessedBarricade = 43189, // Boss->self, 3.0s cast, single-target
    _Weaponskill_SpearpointPush = 43188, // 47CA->location, 1.5+0.7s cast, range 33 width 74 rect
    _Spell_Explosion1 = 43068, // Helper->self, 6.0s cast, range 3 circle
    _Weaponskill_SpearpointPush1 = 43187, // 47CA->location, 1.5+0.7s cast, range 33 width 74 rect
    _Weaponskill_PerfumedQuietus = 43191, // Boss->self, 3.0+6.2s cast, range 50 circle
    _Weaponskill_PerfumedQuietus1 = 43213, // Helper->self, 9.2s cast, range 50 circle
    _Weaponskill_RosebloodBloom = 43193, // Boss->self, 2.6+0.4s cast, single-target
    _Ability_QueensCrusade = 43194, // Helper->self, 3.7s cast, range 2 circle
    _Weaponskill_AlexandrianThunderII = 43198, // Boss->self, 5.0s cast, single-target
    _Ability_AlexandrianThunderII = 43199, // Helper->self, 5.7s cast, range 24 45-degree cone
    _Ability_AlexandrianThunderII1 = 43064, // Helper->self, no cast, range 24 45-degree cone
    _Spell_AlexandrianThunderIII = 43235, // Boss->self, 4.3s cast, single-target, visual
    _Spell_AlexandrianThunderIII1 = 43236, // Helper->players, 5.0s cast, ???, icon spread
    _Weaponskill_Roseblood2NdBloom = 43540, // Boss->self, 2.6+0.4s cast, single-target
    _Weaponskill_ThunderSlash = 43448, // Boss->self, 6.0+0.7s cast, single-target
    _Ability_ThunderSlash = 43216, // Helper->self, 6.7s cast, range 24 60-degree cone
    _Ability_AlexandrianThunderIV = 43450, // Helper->self, 6.7s cast, ???
    _Ability_AlexandrianThunderIV1 = 43451, // Helper->self, 6.7s cast, ???
    _Weaponskill_Roseblood3RdBloom = 43541, // Boss->self, 2.6+0.4s cast, single-target
    _Weaponskill_BudOfValor = 43186, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Emblazon = 43195, // Boss->self, 3.0s cast, single-target
    _Ability_Emblazon = 43040, // Helper->player, no cast, single-target
    _Spell_Explosion2 = 43201, // Helper->self, 13.0s cast, ???
    _Spell_AlexandrianBanishII = 43217, // 47CA->self, 5.0s cast, single-target
    _Spell_AlexandrianBanishII1 = 43218, // Helper->players, no cast, ???
    _Weaponskill_Roseblood4ThBloom = 43542, // Boss->self, 2.6+0.4s cast, single-target
    _Spell_AlexandrianThunderIII2 = 43238, // Helper->location, 7.5s cast, ???
    _Weaponskill_EncirclingThorns = 43203, // Boss->self, 4.0s cast, single-target
    _Spell_AlexandrianBanishIII = 43240, // Boss->self, 4.0s cast, single-target
    _Spell_AlexandrianBanishIII1 = 43241, // Helper->players, no cast, ???
    _Weaponskill_PowerBreak = 43184, // 47CA->self, 6.0s cast, range 24 width 64 rect
    _Weaponskill_Roseblood5ThBloom = 43543, // Boss->self, 2.6+0.4s cast, single-target
    _Weaponskill_ValorousAscension = 43206, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ValorousAscension1 = 43207, // Helper->self, 3.7s cast, range 50 circle
    _Weaponskill_ValorousAscension2 = 43208, // Helper->self, no cast, range 50 circle
    _Weaponskill_ValorousAscension3 = 43209, // Helper->self, no cast, range 50 circle
    _Weaponskill_ValorousAscension4 = 43210, // 47C7->self, 5.0s cast, range 40 width 8 rect
    _Weaponskill_Roseblood6ThBloom = 43544, // Boss->self, 2.6+0.4s cast, single-target
    _Ability_HolyHazard = 43234, // Helper->self, 7.7s cast, range 24 ?-degree cone
    _Weaponskill_HolyHazard = 43233, // Boss->self, 7.0+0.7s cast, single-target
    _Weaponskill_ThunderSlash1 = 43449, // Boss->self, 6.0+0.7s cast, single-target
    _Weaponskill_PowerBreak1 = 43185, // 47CA->self, 6.0s cast, range 24 width 64 rect
    _Weaponskill_HolyHazard1 = 43231, // Boss->self, 7.0+0.7s cast, single-target
    _Spell_UnmitigatedExplosion = 43069, // Helper->self, no cast, range 50 circle
    _Ability_HolyHazard1 = 43065, // Boss->self, no cast, single-target
    _Ability_EncirclingThorns = 43204, // Helper->self, no cast, ???
    _Spell_UnmitigatedExplosion1 = 43202, // Helper->self, no cast, range 50 circle
}

public enum IconID : uint
{
    ShockDonut = 580, // player->self
    ShockCircle = 581, // player->self
    StockBreak = 590, // player->self
    ThunderCW = 167, // Boss->self
    ThunderCCW = 168, // Boss->self
    _Gen_Icon_23 = 23, // player->self
    AlexandrianThunderIII = 596, // player->self
    _Gen_Icon_592 = 592, // player->self
    _Gen_Icon_93 = 93, // player->self
    _Gen_Icon_12 = 12, // player->self
    _Gen_Icon_597 = 597, // player->self
}

public enum SID : uint
{
    _Gen_LightResistanceDownII = 4164, // Helper->player, extra=0x0
    _Gen_ = 2970, // none->Boss, extra=0x2F7/0x2F6
    _Gen_SlashingResistanceDown = 3130, // Helper->player, extra=0x0
    _Gen_LightningResistanceDownII = 2998, // Helper->player, extra=0x0
    _Gen_VulnerabilityUp = 1789, // Helper/47CA/47C7->player, extra=0x1/0x2/0x3/0x4
    _Gen_MagicVulnerabilityUp = 3414, // Helper->player, extra=0x0
    _Gen_ThornyVine = 445, // none->player, extra=0x0
    _Gen_1 = 2056, // none->47CA/Boss, extra=0x382
    _Gen_Bleeding = 4137, // none->player, extra=0x0
    _Gen_Bleeding1 = 4138, // none->player, extra=0x0
}

public enum TetherID : uint
{
    _Gen_Tether_89 = 89, // player->Boss
    AddsTether = 17, // 47CA->player
    _Gen_Tether_18 = 18, // player->player
}
