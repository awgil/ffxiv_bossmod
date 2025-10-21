#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

public enum OID : uint
{
    Boss = 0x48EE, // R28.500, x0-1
    Helper = 0x233C, // R0.500, x7-47, Helper type
    DevouredEater = 0x48EF, // R15.000, x0-1, Part type
    EminentGrief = 0x486C, // R1.000, x1-2
    VodorigaMinion = 0x48F0, // R1.200, x0 (spawn during fight)
    _Gen_ = 0x48F2, // R1.000, x0 (spawn during fight)
    ArcaneFont = 0x48F4, // R2.000, x0 (spawn during fight)
    ScourgingBlaze = 0x1EBE70,
}

public enum AID : uint
{
    _Spell_Attack = 44135, // Boss->self, no cast, single-target
    _Spell_ = 44820, // EminentGrief->player, 0.5s cast, single-target
    _Spell_Attack1 = 44814, // Helper->player, 0.8s cast, single-target
    _Spell_Attack2 = 44136, // DevouredEater->self, no cast, single-target
    _Spell_1 = 44802, // EminentGrief->player, 0.5s cast, single-target
    _Spell_Attack3 = 44137, // Helper->player, 0.8s cast, single-target
    CrystalAppear = 44115, // Helper->location, no cast, single-target

    ScourgingBlazeHorizontalFirst = 44797, // Boss->self, 3.0s cast, single-target
    ScourgingBlazeVerticalFirst = 44798, // Boss->self, 3.0s cast, single-target
    ScourgingBlazeHorizontalSecond = 44799, // Boss->self, no cast, single-target
    ScourgingBlazeVerticalSecond = 44800, // Boss->self, no cast, single-target

    BoundsOfSinBossCast = 44120, // DevouredEater->self, 3.3+0.7s cast, single-target
    BoundsOfSinBind = 44121, // Helper->self, 4.0s cast, range 40 circle
    BoundsOfSinIcicleDrop = 44122, // Helper->self, 3.0s cast, range 3 circle
    BoundsOfSinJailInside = 44123, // Helper->self, no cast, range 8 circle
    BoundsOfSinJailOutside = 44124, // Helper->self, no cast, range 8-30 donut

    BigBurstTower1 = 44141, // Helper->self, no cast, range 60 circle, tower failure?
    NeutralizeExplosion = 44142, // Helper->player, no cast, single-target, light/dark failure
    BigBurstTower2 = 44143, // Helper->self, no cast, range 60 circle, tower failure?
    AbyssalSun = 44139, // Helper->self, no cast, range 30 circle, raidwide after towers, applies bleed

    ScourgingBlazeFirst = 44118, // Helper->location, 7.0s cast, range 5 circle
    ScourgingBlazeRest = 44119, // Helper->location, no cast, range 5 circle

    ChainsOfCondemnationCastFast = 44099, // Boss->location, 4.3+0.7s cast, single-target
    ChainsOfCondemnationFast = 44100, // Helper->location, 5.0s cast, range 30 circle, move-only pyretic
    ChainsOfCondemnationCastSlow = 44106, // Boss->location, 7.3+0.7s cast, single-target
    ChainsOfCondemnationSlow = 44107, // Helper->location, 8.0s cast, range 30 circle, move-only pyretic

    BladeOfFirstLightInsideFast = 44102, // DevouredEater->self, 4.2+0.8s cast, single-target
    BladeOfFirstLightOutsideFast = 44103, // DevouredEater->self, 4.2+0.8s cast, single-target
    BladeOfFirstLightFast = 44104, // Helper->self, 5.0s cast, range 30 width 15 rect

    BladeOfFirstLightInsideSlow = 44108, // DevouredEater->self, 7.2+0.8s cast, single-target
    BladeOfFirstLightOutsideSlow = 44109, // DevouredEater->self, 7.2+0.8s cast, single-target
    BladeOfFirstLightSlow = 44110, // Helper->self, 8.0s cast, range 30 width 15 rect

    BallOfFireCastFast = 44097, // Boss->self, 5.0s cast, single-target
    BallOfFireCastSlow = 44105, // Boss->self, 8.0s cast, single-target

    BallOfFirePuddle = 44098, // Helper->location, 2.1s cast, range 6 circle

    _Spell_SearingChains = 44144, // Helper->self, no cast, range 50 width 6 cross
    _Ability_Spinelash = 44125, // Boss->self, 2.0s cast, single-target
    _Weaponskill_Spinelash = 44126, // Boss->self, 2.2+0.8s cast, single-target
    _Weaponskill_Spinelash1 = 45119, // Helper->self, 3.0s cast, range 60 width 8 rect
    _Ability_ = 44127, // Boss->self, no cast, single-target
    _Spell_Explosion1 = 44140, // Helper->player, no cast, single-target
    _AutoAttack_ = 45197, // VodorigaMinion->player, no cast, single-target
    _Weaponskill_BloodyClaw = 45116, // VodorigaMinion->player, no cast, single-target
    _Spell_ShacklesOfGreaterSanctity = 44801, // DevouredEater->self, 3.0s cast, single-target
    _Spell_ShacklesOfSanctity1 = 44147, // Helper->player, no cast, single-target
    _Spell_ShacklesOfSanctity = 44148, // Helper->player, no cast, single-target

    _Spell_TerrorEye = 45117, // VodorigaMinion->location, 3.0s cast, range 6 circle

    ShackleHealerExplosion = 44149, // Helper->players, no cast, range 21 circle, triggered on healing spell
    ShackleDPSExplosion = 44150, // Helper->players, no cast, range 8 circle, triggered on ability use

    HellishEarthCast = 44151, // Boss->location, 5.0+1.0s cast, single-target
    HellishEarthPull = 44152, // Helper->self, 6.0s cast, distance 10 attract on non-tethered players
    HellishEarthPullTether = 44153, // Helper->self, 6.0s cast, distance 60 attract on tethered (furthest) player
    _Spell_HellishTorment = 44155, // Helper->player, no cast, single-target
    _Spell_HellishTorment1 = 44154, // Helper->self, no cast, range 50 circle
    _Weaponskill_ManifoldLashings = 44157, // Boss->self, 5.0+1.3s cast, single-target
    _Weaponskill_ManifoldLashings1 = 44159, // Helper->self, 6.3s cast, range 2 circle
    _Spell_Eruption = 44156, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_ManifoldLashings2 = 44160, // Helper->self, no cast, range 2 circle
    _Weaponskill_ManifoldLashings3 = 44161, // Helper->self, 2.3s cast, range 42 width 9 rect
    _Spell_BurningChains = 44145, // Helper->self, no cast, ???
    _Weaponskill_ManifoldLashings4 = 44158, // Boss->self, 5.0+1.3s cast, single-target
    _Weaponskill_BigBurst = 44162, // Helper->self, no cast, range 80 circle
    _Spell_UnholyDarkness = 44163, // DevouredEater->self, 6.0+0.7s cast, single-target
    _Spell_UnholyDarkness1 = 44164, // Helper->self, 6.7s cast, range 30 circle

    _Spell_CrimeAndPunishment = 44165, // DevouredEater->self, 6.0+0.7s cast, single-target
    _Spell_CrimeAndPunishment1 = 44166, // Helper->player, no cast, single-target
    _Spell_Explosion = 44167, // Helper->players, no cast, range 4 circle, rot pass
    _Spell_BigBurst = 44168, // Helper->player, no cast, range 60 circle
}

public enum SID : uint
{
    Bind = 4510, // Helper->player, extra=0x0
    DarkVengeance = 4559, // none->player, extra=0x0
    LightVengeance = 4560, // none->player, extra=0x0
    ChainsOfCondemnation = 4562, // Helper->player, extra=0x0
    SearingChains = 4563, // none->player, extra=0x0
    FireResistanceDownII = 2937, // Helper->player, extra=0x0
    Burns = 2082, // Helper->player, extra=0x0
    _Gen_ = 3913, // Boss->Boss, extra=0x3C6
    Suppuration = 4512, // Helper->player, extra=0x4/0x2/0x1
    ShackledAbilities = 4565, // none->player, extra=0x0
    ShackledHealing = 4564, // none->player, extra=0x0
    HellishEarth = 4566, // none->player, extra=0x0
    HPBoost = 586, // none->ArcaneFont, extra=0x8
    Petrification = 1, // none->player, extra=0x0
    Poison = 3462, // Helper->player, extra=0x1/0x2/0x3

    Bleeding1 = 2922, // none->player, extra=0x0
    Bleeding2 = 2951, // Helper->player, extra=0x0
    DamageDown = 3304, // Helper->player, extra=0x1/0x2/0x3/0x4
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Bleeding3 = 2088, // Helper->player, extra=0x0
    SinBearer = 4567, // Helper->player, extra=1-16
    Doom = 4594, // none->player, extra=0x0
    WithoutSin = 4569, // none->player, extra=0x0
    SumOfAllSins = 4568, // none->player, extra=0xEC7
    Unk1 = 4685, // none->player, extra=0x0
}

public enum IconID : uint
{
    RingLight = 77, // player->self
    RingDark = 78, // player->self
    _Gen_Icon_m0376trg_fire3_a0p = 97, // player->self
    _Gen_Icon_lockon5_t0h = 23, // player->self
    _Gen_Icon_share_laser_3sec_0t = 527, // Boss->player
}

public enum TetherID : uint
{
    _Gen_Tether_chn_hfchain1f = 9, // player->player
    _Gen_Tether_chn_fire001f = 5, // Boss->player
}
