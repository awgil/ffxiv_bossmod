namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

public enum OID : uint
{
    Boss = 0x48F8, // R6.000, x1
    Helper = 0x233C, // R0.500, x30, Helper type
    _Gen_SublimeEstoc = 0x48FA, // R1.000, x14
    _Gen_ = 0x49C7, // R1.000, x8
    _Gen_Kamlanaut = 0x48F9, // R6.000, x3

    ProvingGround = 0x1EBEEF,
}

public enum AID : uint
{
    _AutoAttack_ = 44876, // Boss->player, no cast, single-target
    _Weaponskill_EnspiritedSwordplay = 44221, // Boss->self, 5.0s cast, range 60 circle
    _Ability_ = 44225, // Boss->location, no cast, single-target
    _Spell_ProvingGround = 45065, // Boss->self, 3.0s cast, range 5 circle
    _Weaponskill_ElementalBlade = 44177, // Boss->self, 8.0s cast, single-target
    _Weaponskill_SublimeElements = 45066, // Boss->self, 8.0+1.0s cast, single-target

    _Spell_SublimeFire1 = 44179, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeEarth = 44180, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeWater = 44181, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeIce = 44182, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeLightning = 44183, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeWind1 = 44184, // Helper->self, 9.0s cast, range 40 ?-degree cone

    _Spell_SublimeFire = 44185, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeEarth1 = 44186, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeWater1 = 44187, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeIce1 = 44188, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeLightning1 = 44189, // Helper->self, 9.0s cast, range 40 ?-degree cone
    _Spell_SublimeWind = 44190, // Helper->self, 9.0s cast, range 40 ?-degree cone

    _Spell_FireBlade = 44191, // Helper->self, 9.0s cast, range 80 width 5 rect
    _Spell_EarthBlade = 44192, // Helper->self, 9.0s cast, range 80 width 5 rect
    _Spell_WaterBlade = 44193, // Helper->self, 9.0s cast, range 80 width 5 rect
    _Spell_IceBlade = 44194, // Helper->self, 9.0s cast, range 80 width 5 rect
    _Spell_LightningBlade1 = 44195, // Helper->self, 9.0s cast, range 80 width 5 rect
    _Spell_WindBlade1 = 44196, // Helper->self, 9.0s cast, range 80 width 5 rect

    _Spell_FireBlade1 = 44197, // Helper->self, 9.0s cast, range 80 width 20 rect
    _Spell_EarthBlade1 = 44198, // Helper->self, 9.0s cast, range 80 width 20 rect
    _Spell_WaterBlade1 = 44199, // Helper->self, 9.0s cast, range 80 width 20 rect
    _Spell_IceBlade1 = 44200, // Helper->self, 9.0s cast, range 80 width 20 rect
    _Spell_LightningBlade = 44201, // Helper->self, 9.0s cast, range 80 width 20 rect
    _Spell_WindBlade = 44202, // Helper->self, 9.0s cast, range 80 width 20 rect

    _Weaponskill_PrincelyBlow = 44219, // Boss->self, 7.2+0.8s cast, single-target
    _Weaponskill_PrincelyBlow1 = 44220, // Helper->self, no cast, range 60 width 10 rect
    _Weaponskill_LightBlade = 44203, // Boss->self, 3.0s cast, range 120 width 13 rect
    _Weaponskill_SublimeEstoc = 44204, // _Gen_SublimeEstoc->self, 3.0s cast, range 40 width 5 rect
    _Weaponskill_GreatWheel = 44207, // Boss->self, 3.0s cast, range 10 circle
    _Weaponskill_GreatWheel1 = 44209, // Helper->self, 5.8s cast, range 80 180-degree cone
    _Weaponskill_EsotericScrivening = 44210, // Boss->self, 6.0s cast, single-target
    _Weaponskill_ = 44409, // Boss->self, no cast, single-target
    _Weaponskill_1 = 44410, // Boss->self, no cast, single-target
    _Spell_Shockwave = 44211, // Helper->self, 5.2s cast, range 60 circle
    _Spell_ = 41727, // Helper->Boss, no cast, single-target
    _Weaponskill_TranscendentUnion = 44212, // Boss->self, 5.0s cast, single-target
    _Spell_ElementalEdge = 44289, // Helper->self, no cast, range 60 circle
    _Spell_TranscendentUnion = 44213, // Helper->self, no cast, range 60 circle
    _Weaponskill_EsotericPalisade = 44214, // Boss->self, 3.0s cast, single-target
    _Weaponskill_CrystallineResonance = 44215, // Boss->self, 3.0s cast, single-target
    _Spell_ElementalResonance = 44216, // Helper->self, 7.0s cast, range 18 circle
    _Spell_EmpyrealBanishIII = 44223, // Helper->players, 5.0s cast, range 5 circle
    _Spell_IllumedFacet = 44217, // Boss->self, 3.0s cast, single-target
    _Weaponskill_IllumedEstoc = 44218, // _Gen_Kamlanaut->self, 8.0s cast, range 120 width 13 rect
    _Weaponskill_ShieldBash = 44222, // Boss->self, 7.0s cast, range 60 circle
    _Spell_EmpyrealBanishIV = 44907, // Boss->self, 4.0+1.0s cast, single-target
    _Spell_EmpyrealBanishIV1 = 44224, // Helper->players, 5.0s cast, range 5 circle
    _Weaponskill_ElementalBlade1 = 44178, // Boss->self, 11.0s cast, single-target
    _Weaponskill_GreatWheel2 = 44205, // Boss->self, 3.0s cast, range 10 circle
    _Weaponskill_GreatWheel3 = 44206, // Boss->self, 3.0s cast, range 10 circle
    _Weaponskill_GreatWheel4 = 44208, // Boss->self, 3.0s cast, range 10 circle
}

public enum IconID : uint
{
    _Gen_Icon_z6r2b3_8sec_lockon_c0a1 = 613, // Boss->player
    _Gen_Icon_loc05sp_05a_se_p = 376, // player->self
    _Gen_Icon_com_share1f = 93, // player->self
}
