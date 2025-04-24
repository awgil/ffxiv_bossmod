#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

public enum OID : uint
{
    Boss = 0x4727, // R5.005, x1
    Helper = 0x233C, // R0.500, x25 (spawn during fight), Helper type
    _Gen_WolfOfWind = 0x472A, // R1.500, x9
    _Gen_GleamingFang = 0x472D, // R1.400, x2
    _Gen_GleamingFang1 = 0x472C, // R1.400, x8
    _Gen_MoonlitShadow = 0x4729, // R5.005, x4
    _Gen_HowlingBlade = 0x4728, // R4.235, x5
    _Gen_HowlingBlade1 = 0x472E, // R19.000, x1
    _Gen_WolfOfStone = 0x47B3, // R1.500, x0 (spawn during fight)
    _Gen_FontOfEarthAether = 0x4756, // R1.000-1.500, x0 (spawn during fight)
    _Gen_FontOfWindAether = 0x4755, // R1.000-1.500, x0 (spawn during fight)
    _Gen_WolfOfWind1 = 0x47B2, // R1.500, x0 (spawn during fight)
    _Gen_WolfOfWind2 = 0x472B, // R1.500, x0 (spawn during fight)
    _Gen_WolfOfStone1 = 0x4731, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 42222, // Boss->player, no cast, single-target
    ExtraplanarPursuitVisual = 41946, // Boss->self, 1.6+2.4s cast, single-target
    ExtraplanarPursuit = 42831, // Helper->self, 4.0s cast, range 40 circle

    WindfangCards = 41885, // Boss->self, 6.0s cast, range 15 width 6 cross
    WindfangIntercards = 41886, // Boss->self, 6.0s cast, range 15 width 6 cross
    WindfangDonut = 41887, // Helper->self, 6.0s cast, range 8-20 donut
    WindfangProtean = 41888, // Helper->self, no cast, range 40 25-degree cone

    StonefangCards = 41889, // Boss->self, 6.0s cast, range 15 width 6 cross
    StonefangIntercards = 41890, // Boss->self, 6.0s cast, range 15 width 6 cross
    StonefangCircle = 41904, // Helper->self, 6.0s cast, range 9 circle
    StonefangProtean = 41905, // Helper->self, no cast, range 40 25(?)-degree cone

    EminentReignVisual1 = 43281, // Boss->self, 5.1s cast, single-target
    EminentReignVisual2 = 43282, // Boss->self, 5.1s cast, single-target
    RevolutionaryReignVisual1 = 43283, // Boss->self, 5.1s cast, single-target
    RevolutionaryReignVisual2 = 43284, // Boss->self, 5.1s cast, single-target

    EminentReignDash = 43297, // Boss->location, no cast, single-target
    RevolutionaryReignDash = 43298, // Boss->location, no cast, single-target

    WolvesReignCloneVisual1 = 43305, // _Gen_HowlingBlade->self, 6.4s cast, single-target
    WolvesReignCloneVisual2 = 43306, // _Gen_HowlingBlade->self, 6.5s cast, single-target
    WolvesReignCloneVisual3 = 43307, // _Gen_HowlingBlade->self, 6.6s cast, single-target
    WolvesReignClone1 = 43308, // Helper->self, 6.7s cast, range 6 circle
    WolvesReignClone2 = 43309, // Helper->self, 6.8s cast, range 6 circle
    WolvesReignClone3 = 43310, // Helper->self, 6.9s cast, range 6 circle

    EminentReignJump = 43312, // Helper->self, 7.0s cast, range 6 circle
    RevolutionaryReignJump = 43313, // Helper->self, 7.0s cast, range 6 circle

    WolvesReignRect1 = 43369, // Helper->self, 1.5s cast, range 28 width 10 rect
    WolvesReignRect2 = 43370, // Helper->self, 1.5s cast, range 28 width 10 rect
    WolvesReignRectJump1 = 41880, // Boss->location, 1.0+0.5s cast, single-target
    WolvesReignRectJump2 = 42927, // Boss->location, 1.0+0.5s cast, single-target
    WolvesReignRectVisual1 = 41881, // Boss->self, 1.0+0.5s cast, single-target
    WolvesReignRectVisual2 = 41882, // Boss->self, 1.0+0.5s cast, single-target
    WolvesReignCone = 42929, // Helper->self, 1.5s cast, range 40 120-degree cone
    WolvesReignCircle = 42930, // Helper->self, 1.5s cast, range 14 circle

    SovereignScar = 41884, // Helper->self, no cast, range 40 30-degree cone
    ReignsEnd = 41883, // Helper->self, no cast, range 40 60-degree cone

    MillennialDecay = 41906, // Boss->self, 5.0s cast, range 40 circle

    AeroIII = 41912, // Helper->self, 5.0s cast, range 40 circle
    AeroIIIVisual = 41911, // Boss->self, 5.0s cast, single-target

    BreathOfDecay = 41908, // _Gen_WolfOfWind->self, 8.0s cast, range 40 width 8 rect
    Gust = 41907, // Helper->players, no cast, range 5 circle
    ProwlingGale = 41910, // Helper->location, 7.0s cast, range 2 circle
    WindsOfDecay = 41909, // _Gen_WolfOfWind->self, no cast, range 40 30(?)-degree cone

    GreatWhirlwind = 41957, // Helper->self, no cast, range 40 circle, tower explosion

    TrackingTremorsVisual = 41913, // Boss->self, 5.0s cast, single-target
    TrackingTremors = 41915, // Helper->players, no cast, range 6 circle
    GreatDivide = 41944, // Boss->self/players, 5.0s cast, range 60 width 6 rect

    TerrestrialTitansVisual = 41924, // Boss->self, 4.0s cast, single-target
    TerrestrialTitans = 41925, // Helper->self, 4.0s cast, range 5 circle

    TitanicPursuitVisual = 41927, // Boss->self, 1.6+2.4s cast, range 40 circle
    TitanicPursuit = 42833, // Helper->self, 4.0s cast, range 40 circle

    BareFangs = 42188, // Boss->self, no cast, single-target
    Towerfall = 41926, // Helper->self, 8.0s cast, range 30 width 10 rect
    FangedCrossing = 41943, // _Gen_GleamingFang->self, 4.0s cast, range 21 width 7 cross

    Unk1 = 42886, // Boss->self, no cast, single-target
    Unk2 = 41871, // Boss->location, no cast, single-target

    TacticalPackVisual = 41928, // Boss->self, 3.0s cast, single-target

    _Ability_ = 41929, // Boss->self, no cast, single-target
    _Spell_HowlingHavoc = 41947, // _Gen_WolfOfWind2->self, 5.0s cast, single-target
    _Spell_HowlingHavoc1 = 41949, // Helper->self, 5.0s cast, range 40 circle
    _Spell_HowlingHavoc2 = 41948, // _Gen_WolfOfStone1->self, 5.0s cast, single-target
    _Ability_PackPredation = 41932, // _Gen_WolfOfWind2/_Gen_WolfOfStone1->self, 5.0s cast, single-target
    _Ability_AlphaStone = 41954, // Helper->self, no cast, range 40 ?-degree cone
    _Ability_StalkingStone = 41956, // Helper->self, no cast, range 40 width 6 rect
    _Ability_AlphaWind = 41933, // Helper->self, no cast, range 40 ?-degree cone
    _Ability_StalkingWind = 41935, // Helper->self, no cast, range 40 width 6 rect
    _AutoAttack_ = 42225, // _Gen_WolfOfWind2->player, no cast, single-target
    _AutoAttack_1 = 42226, // _Gen_WolfOfStone1->player, no cast, single-target
    _Ability_WindSurge = 41965, // _Gen_FontOfWindAether->self, no cast, ???
    _Ability_GaleSurge = 41967, // _Gen_FontOfWindAether->self, no cast, range 40 circle
    _Ability_StoneSurge = 41968, // _Gen_FontOfEarthAether->self, no cast, range 40 circle
    _Ability_SandSurge = 41966, // _Gen_FontOfEarthAether->self, no cast, ???
}

public enum SID : uint
{
    _Gen_ = 2056, // Boss->_Gen_WolfOfWind/_Gen_GleamingFang/_Gen_WolfOfStone/_Gen_WolfOfWind2/_Gen_WolfOfStone1/_Gen_WolfOfWind1/Boss, extra=0x367/0x36D/0x368/0x385
    _Gen_MagicVulnerabilityUp = 2941, // Helper/_Gen_WolfOfWind/_Gen_FontOfWindAether/_Gen_FontOfEarthAether->player, extra=0x0
    _Gen_PhysicalVulnerabilityUp = 2940, // Helper->player, extra=0x0
    _Gen_SustainedDamage = 4149, // Helper->player, extra=0x1/0x2
    _Gen_DamageDown = 2911, // Helper/_Gen_WolfOfWind->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_1 = 4465, // none->_Gen_HowlingBlade, extra=0x32
    _Gen_DirectionalDisregard = 3808, // none->Boss, extra=0x0
    _Gen_2 = 2234, // none->_Gen_FontOfEarthAether/_Gen_FontOfWindAether, extra=0x3E
    _Gen_Windpack = 4389, // none->_Gen_FontOfWindAether/player, extra=0x0
    _Gen_Stonepack = 4390, // none->_Gen_FontOfEarthAether/player, extra=0x0
    _Gen_EarthborneEnd = 4391, // none->player, extra=0x0
    _Gen_WindborneEnd = 4392, // none->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0

}

public enum IconID : uint
{
    Gust = 376, // player->self
    TrackingTremors = 316, // player->self
    GreatDivide = 598, // player->self
    LockOn = 23, // player->self
}

public enum TetherID : uint
{
    WindsOfDecayShort = 57, // _Gen_WolfOfWind->player
    WindsOfDecayLong = 1, // _Gen_WolfOfWind->player
    WolfOfStone = 335, // player->_Gen_WolfOfStone1
    WolfOfWind = 336, // player->_Gen_WolfOfWind2
}
