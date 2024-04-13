namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator;

public enum OID : uint
{
    NBoss = 0x39A5, // R6.500, x1
    NGladiatorMirage = 0x39A6, // R6.500, spawn during fight, specter of might
    NHatefulVisage = 0x39A7, // R2.250, spawn during fight
    NRegret = 0x39A8, // R1.000, spawn during fight

    SBoss = 0x3A10, // R6.500, x1
    SGladiatorMirage = 0x3A11, // R6.500, spawn during fight
    SHatefulVisage = 0x3A12, // R2.250, spawn during fight
    SRegret = 0x3A13, // R1.000, spawn during fight

    Helper = 0x233C, // R0.500, x18
}

public enum AID : uint
{
    AutoAttack = 870, // NBoss/SBoss->player, no cast, single-target
    Teleport = 30265, // NBoss/SBoss->location, no cast, single-target, teleport

    NFlashOfSteel = 30321, // NBoss->self, 5.0s cast, raidwide
    NMightySmite = 30322, // NBoss->player, 5.0s cast, single-target tankbuster
    SFlashOfSteel = 30643, // SBoss->self, 5.0s cast, raidwide
    SMightySmite = 30644, // SBoss->player, 5.0s cast, single-target tankbuster

    NSpecterOfMight = 30323, // NBoss->self, 4.0s cast, single-target, visual
    NRushOfMight1 = 30296, // NGladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
    NRushOfMight2 = 30297, // NGladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
    NRushOfMight3 = 30298, // NGladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
    NRushOfMightFront = 30299, // Helper->self, 10.5s cast, range 60 180-degree cone aoe
    NRushOfMightBack = 30300, // Helper->self, 12.5s cast, range 60 180-degree cone aoe
    SSpecterOfMight = 30645, // SBoss->self, 4.0s cast, single-target, visual
    SRushOfMight1 = 30618, // SGladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
    SRushOfMight2 = 30619, // SGladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
    SRushOfMight3 = 30620, // SGladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
    SRushOfMightFront = 30621, // Helper->self, 10.5s cast, range 60 180-degree cone aoe
    SRushOfMightBack = 30622, // Helper->self, 12.5s cast, range 60 180-degree cone aoe

    SculptorsPassionTargetSelection = 26708, // Helper->player, no cast, single-target, cast right before the beginning of visual cast
    NSculptorsPassion = 30316, // NBoss->self, 5.0s cast, single-target, visual
    NSculptorsPassionAOE = 31219, // NBoss->self, no cast, range 60 width 8 rect shared
    SSculptorsPassion = 30638, // SBoss->self, 5.0s cast, single-target, visual
    SSculptorsPassionAOE = 31220, // SBoss->self, no cast, range 60 width 8 rect shared

    RingOfMightVisual = 30655, // Helper->self, 10.0s cast, range 18 circle, ???
    NCurseOfTheFallen = 30324, // NBoss->self, 5.0s cast, single-target, visual
    NRingOfMight1Out = 30301, // NBoss->self, 10.0s cast, range 8 circle
    NRingOfMight2Out = 30302, // NBoss->self, 10.0s cast, range 13 circle
    NRingOfMight3Out = 30303, // NBoss->self, 10.0s cast, range 18 circle
    NRingOfMight1In = 30304, // Helper->self, 12.0s cast, range 8-30 donut
    NRingOfMight2In = 30305, // Helper->self, 12.0s cast, range 13-30 donut
    NRingOfMight3In = 30306, // Helper->self, 12.0s cast, range 18-30 donut
    NEchoOfTheFallen = 30325, // Helper->players, no cast, range 6 circle spread
    NThunderousEcho = 30326, // Helper->players, no cast, range 5 circle stack
    NLingeringEcho = 30327, // Helper->location, no cast, range 5 circle baited 5x repeated aoe
    NEchoOfTheFallenDeath = 30950, // Helper->location, no cast, range 8 circle aoe on debuffed player death
    SCurseOfTheFallen = 30646, // SBoss->self, 5.0s cast, single-target, visual
    SRingOfMight1Out = 30623, // SBoss->self, 10.0s cast, range 8 circle
    SRingOfMight2Out = 30624, // SBoss->self, 10.0s cast, range 13 circle
    SRingOfMight3Out = 30625, // SBoss->self, 10.0s cast, range 18 circle
    SRingOfMight1In = 30626, // Helper->self, 12.0s cast, range 8-30 donut
    SRingOfMight2In = 30627, // Helper->self, 12.0s cast, range 13-30 donut
    SRingOfMight3In = 30628, // Helper->self, 12.0s cast, range 18-30 donut
    SEchoOfTheFallen = 30647, // Helper->players, no cast, range 6 circle spread
    SThunderousEcho = 30648, // Helper->players, no cast, range 5 circle stack
    SLingeringEcho = 30649, // Helper->location, no cast, range 5 circle baited 5x repeated aoe
    //SEchoOfTheFallenDeath = ???, // Helper->location, no cast, range 8 circle aoe on debuffed player death

    NHatefulVisage = 30318, // NBoss->self, 3.0s cast, single-target, visual
    NAccursedVisage = 30349, // NBoss->self, 3.0s cast, single-target, visual
    NWrathOfRuin = 30307, // NBoss->self, 3.0s cast, single-target, visual
    NRackAndRuin = 30308, // NRegret->location, 4.0s cast, range 40 width 5 rect
    NGoldenFlame = 30319, // NHatefulVisage->self, 10.0s cast, range 60 width 10 rect
    NSilverFlame = 30320, // NHatefulVisage->self, 10.0s cast, range 60 width 10 rect
    NNothingBesideRemains = 30347, // NBoss->self, 5.0s cast, single-target, visual
    NNothingBesideRemainsAOE = 30348, // Helper->players, 5.0s cast, range 8 circle spread
    SHatefulVisage = 30640, // SBoss->self, 3.0s cast, single-target, visual
    SAccursedVisage = 30654, // SBoss->self, 3.0s cast, single-target, visual
    SWrathOfRuin = 30629, // SBoss->self, 3.0s cast, single-target, visual
    SRackAndRuin = 30630, // SRegret->location, 4.0s cast, range 40 width 5 rect
    SGoldenFlame = 30641, // SHatefulVisage->self, 10.0s cast, range 60 width 10 rect
    SSilverFlame = 30642, // SHatefulVisage->self, 10.0s cast, range 60 width 10 rect
    SNothingBesideRemains = 30652, // SBoss->self, 5.0s cast, single-target, visual
    SNothingBesideRemainsAOE = 30653, // Helper->players, 5.0s cast, range 8 circle spread

    NCurseOfTheMonument = 30310, // NBoss->self, 4.0s cast, single-target, visual
    NChainsOfResentment = 30311, // Helper->self, no cast, raidwide if tether is not broken in time
    NSunderedRemains = 30312, // Helper->self, 6.0s cast, range 10 circle aoe
    NScreamOfTheFallen = 30328, // Helper->players, no cast, range 15 circle spread
    NScreamOfTheFallenDeath = 30951, // Helper->location, no cast, range 15 circle on debuffed player death
    NColossalWreck = 30313, // NBoss->self, 6.0s cast, single-target, visual
    NExplosion = 30314, // Helper->self, 6.5s cast, range 3 circle tower
    NMassiveExplosion = 30315, // Helper->self, no cast, raidwide if tower is unsoaked
    SCurseOfTheMonument = 30632, // SBoss->self, 4.0s cast, single-target, visual
    SChainsOfResentment = 30633, // Helper->self, no cast, raidwide if tether is not broken in time
    SSunderedRemains = 30634, // Helper->self, 6.0s cast, range 10 circle aoe
    SScreamOfTheFallen = 30650, // Helper->players, no cast, range 15 circle spread
    //SScreamOfTheFallenDeath = ???, // Helper->location, no cast, range 15 circle on debuffed player death
    SColossalWreck = 30635, // SBoss->self, 6.0s cast, single-target, visual
    SExplosion = 30636, // Helper->self, 6.5s cast, range 3 circle tower
    SMassiveExplosion = 30637, // Helper->self, no cast, raidwide if tower is unsoaked

    Enrage = 30329, // NBoss->self, 10.0s cast, enrage
    Enrage2 = 31282, // NBoss->self, no cast, range 60 circle ???
}

public enum SID : uint
{
    EchoOfTheFallen = 3290, // none->player, extra=0x0, spread
    ThunderousEcho = 3293, // none->player, extra=0x0, stack
    LingeringEchoes = 3292, // none->player, extra=0x0, voidzone

    GildedFate = 3295, // none->player, extra=0x1/0x2 (stand under SilverFlame)
    SilveredFate = 3296, // none->player, extra=0x1/0x2 (stand under GoldenFlame)

    FirstInLine = 3004, // none->player, extra=0x0
    SecondInLine = 3005, // none->player, extra=0x0
    ScreamOfTheFallen = 3291, // none->player, extra=0x0
    ChainsOfResentment = 3294, // none->player, extra=0x0
}
