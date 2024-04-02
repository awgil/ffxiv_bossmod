namespace BossMod.Endwalker.Unreal.Un2Sephirot;

public enum OID : uint
{
    BossP1 = 0x39DA, // x1 (R=4.5)
    BossP3 = 0x39DB, // x1
    Helper = 0x39E0, // x25
    Binah = 0x39DD, // spawn during fight
    Cochma = 0x39DC, // spawn during fight
    StormOfWords = 0x39DE, // spawn during fight
    CoronalWind = 0x39DF, // spawn during fight when storm of words is killed
};

public enum AID : uint
{
    AutoAttack = 872, // BossP1/Cochma/Binah->player, no cast, single-target
    TripleTrial = 30355, // BossP1->self, no cast, range 14+R 90?-degree cone cleave
    Tiferet = 30357, // BossP1->self, no cast, raidwide

    EinSof = 30363, // BossP1->self, no cast, single-target, visual
    EinSofAOE = 30364, // Helper->self, no cast, range 1 circle, pulsing aoe from orbs (11 per orb total)
    FiendishRage = 30362, // BossP1->players, no cast, range 6 circle, shared damage
    Chesed = 30356, // BossP1->player, 4.0s cast, single-target, tankbuster
    Ein = 30358, // BossP1->self, 4.0s cast, range 45+R width 45 rect aoe
    Ratzon = 30359, // BossP1->self, no cast, single-target, visual
    RatzonAOEGreen = 30360, // Helper->players, no cast, range 5 circle
    RatzonAOEPurple = 30361, // Helper->players, no cast, range 10 circle

    EmptyHand = 30389, // Cochma->player, no cast, single-target mini-tankbuster
    SolidStone = 30390, // Binah->player, no cast, single-target mini-tankbuster
    GenesisCochma = 30391, // Cochma->self, no cast, raidwide on death
    GenesisBinah = 30392, // Binah->self, no cast, raidwide on death

    EinSofOhr = 30365, // BossP3->self, no cast, single-target, visual
    EinSofOhrAOE = 30366, // Helper->self, no cast, range 80+R circle, raidwide on transition
    Yesod = 30379, // Helper->self, 3.0s cast, range 4 circle aoe, twister
    ForceField = 30388, // BossP3->self, no cast, single-target, visual for force against might/magic application
    GevurahChesed = 30373, // BossP3->self, 5.0s cast, single-target, visual (phys left, magic right)
    ChesedGevurah = 30374, // BossP3->self, 5.0s cast, single-target, visual (magic left, phys right)
    LifeForce = 30375, // Helper->self, no cast, phys damage half arena cleave
    Spirit = 30376, // Helper->self, no cast, magic damage half arena cleave
    FiendishWail = 30370, // BossP3->self, no cast, single-target, visual for towers appearing
    FiendishWailAOE = 30371, // Helper->self, 4.0s cast, range 5 circle towers
    FiendishWailFail = 30372, // Helper->self, no cast, raidwide if towers aren't soaked
    DaatTethers = 30369, // BossP3->players, no cast, range 5 circle on tethered players
    EarthShaker = 30377, // BossP3->self, no cast, single-target, visual
    EarthShakerAOE = 30378, // Helper->self, no cast, range 60+R 30-degree cone
    DaatMT = 30367, // BossP3->players, 5.0s cast, range 5 circle on mt
    DaatRandom = 30368, // BossP3->players, no cast, range 5 circle on random
    PillarOfMercy = 30380, // BossP3->self, no cast, single-target, visual
    PillarOfMercyKnockback = 30381, // Helper->self, no cast, knockback 17
    PillarOfMercyAOE = 30382, // Helper->self, 4.5s cast, range 5 oneshot if inside pillar
    PillarOfMercyUnknown = 30393, // Helper->self, 5.0s cast, range 5 circle ???
    Malkuth = 30383, // BossP3->self, 4.0s cast, range 80+R circle, knockback 25

    Revelation = 30384, // StormOfWords->self, 20.0s cast, raidwide knockback 20 (if not killed in time)
    PillarOfSeverity = 30386, // BossP3->self, no cast, single-target, visual
    ImpactOfHod = 30394, // Helper->self, no cast, raidwide knockback 5
    Ascension = 30385, // CoronalWind->self, 4.0s cast, range 6 circle
    PillarOfSeverityAOE = 30387, // Helper->self, no cast, raidwide enrage (unless hit by ascension)
};

public enum SID : uint
{
    ForceAgainstMight = 1005, // BossP3->player, extra=0x0
    ForceAgainstMagic = 1006, // BossP3->player, extra=0x0
};

public enum IconID : uint
{
    RatzonGreen = 70, // player
    RatzonPurple = 71, // player
    FiendishRage = 72, // player
    Earthshaker = 40, // player
    Ascension = 62, // CoronalWind
};
