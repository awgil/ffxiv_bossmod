namespace BossMod.Endwalker.Savage.P9SKokytos;

public enum OID : uint
{
    Boss = 0x3ED7, // R9.500, x1
    Helper = 0x233C, // R0.500, x20
    BallOfLevin = 0x3ED9, // R2.000, spawn during fight
    KokytossEcho = 0x3ED8, // R9.500, spawn during fight
    Comet = 0x3EDA, // R2.400, spawn during fight
    Charybdis = 0x1EB881, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttackNormal = 33099, // Boss->player, no cast, single-target
    AutoAttackMage = 33104, // Boss->player, no cast, single-target
    AutoAttackMartialist = 33117, // Boss->player, no cast, single-target
    AutoAttackBeast = 33161, // Boss->player, no cast, single-target
    Teleport = 33092, // Boss->location, no cast, single-target

    GluttonysAugur = 33100, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    GluttonysAugurAOE = 33101, // Helper->self, no cast, range 60 circle, raidwide
    RaveningMage = 33048, // Boss->self, 4.0s cast, single-target, visual (raidwide + soul switch)
    RaveningMartialist = 33049, // Boss->self, 4.0s cast, single-target, visual (raidwide + soul switch)
    RaveningChimeric = 33147, // Boss->self, 4.0s cast, single-target, visual (raidwide + soul switch)
    RaveningBeast = 33050, // Boss->self, 3.0s cast, single-target, visual (raidwide + soul switch)
    SoulSurge = 33051, // Helper->self, no cast, range 60 circle, raidwide
    DisgorgeMage = 33060, // Boss->self, no cast, single-target, visual (clear soul)
    DisgorgeMartialist = 33061, // Boss->self, no cast, single-target, visual (clear soul)
    DisgorgeChimeric = 33334, // Boss->self, no cast, single-target, visual (clear soul)
    DisgorgeBeast = 33062, // Boss->self, no cast, single-target, visual (clear soul)

    DualityOfDeath = 33105, // Boss->self, 5.0s cast, single-target, visual (tankbuster)
    DualityOfDeathFire = 33106, // Helper->player, no cast, range 6 circle tankbuster
    DualityOfDeathAero = 33107, // Helper->players, no cast, range 6 circle tankbuster
    DualspellIceFire = 33108, // Boss->self, 5.0s cast, single-target, visual (2-man stacks + in/out)
    DualspellIceLightning = 33109, // Boss->self, 5.0s cast, single-target (spreads + in/out)
    DualspellVisualFire = 33058, // Helper->self, no cast, single-target (out)
    DualspellVisualIce = 33059, // Helper->self, no cast, single-target (in)
    DualspellVisualLightning = 33116, // Helper->self, no cast, single-target (out)
    DualspellPilePyreFireSmall = 33110, // Helper->players, no cast, range 6 circle, 2-man stacks
    DualspellPilePyreFireLarge = 33112, // Helper->players, no cast, range 12 circle, 2-man stacks
    DualspellBlizzardOut = 33111, // Helper->self, no cast, range 14-40 donut
    DualspellBlizzardIn = 33113, // Helper->self, no cast, range 8-40 donut
    DualspellThunderLarge = 33114, // Helper->self, no cast, range 40 width 16 rect
    DualspellThunderSmall = 33115, // Helper->self, no cast, range 40 width 8 rect

    Uplift = 33118, // Helper->self, 11.0s cast, range 4 width 16 rect (create wall)
    AscendantFist = 33135, // Boss->player, 5.0s cast, single-target, tankbuster
    ArchaicRockbreakerCenter = 33119, // Boss->location, 5.0s cast, range 6 circle (in center, before knockback)
    ArchaicRockbreakerShockwave = 33120, // Helper->self, no cast, range 60 circle knockback 21
    ArchaicRockbreakerLine = 33121, // Helper->location, 8.0s cast, range 8 circle (at the end of the line)
    ArchaicRockbreakerPairs = 33122, // Helper->players, no cast, range 6 circle, 2-man stack
    FrontCombinationOut = 33127, // Boss->self, 6.0s cast, single-target, visual (out + front cleave)
    FrontCombinationIn = 33128, // Boss->self, 6.0s cast, single-target, visual (in + front cleave)
    RearCombinationOut = 33129, // Boss->self, 6.0s cast, single-target, visual (out + back cleave)
    RearCombinationIn = 33130, // Boss->self, 6.0s cast, single-target, visual (in + back cleave)
    InsideRoundhouseHint = 33291, // Helper->self, 6.7s cast, range 12 circle, visual
    InsideRoundhouseVisual = 33125, // Boss->self, no cast, single-target, visual
    InsideRoundhouseAOE = 33336, // Helper->self, 0.7s cast, range 12 circle
    OutsideRoundhouseHint = 33292, // Helper->self, 6.7s cast, range 8-20 donut, visual
    OutsideRoundhouseVisual = 33126, // Boss->self, no cast, single-target, visual
    OutsideRoundhouseAOE = 33337, // Helper->self, 0.7s cast, range 8-20 donut
    SwingingKickFrontVisual = 33131, // Boss->self, no cast, single-target, visual
    SwingingKickRearVisual = 33132, // Boss->self, no cast, single-target, visual
    SwingingKickFrontAOE = 33314, // Helper->self, 0.7s cast, range 40 180-degree cone
    SwingingKickRearAOE = 33315, // Helper->self, 0.7s cast, range 40 180-degree cone
    ArchaicDemolish = 33133, // Boss->self, 4.0s cast, single-target, visual
    ArchaicDemolishAOE = 33134, // Helper->players, no cast, range 6 circle, 4-man stack

    LevinstrikeSummoning = 33148, // Boss->self, 4.0s cast, single-target, visual (summon orbs)
    ScrambledSuccession = 33149, // Boss->self, 10.0s cast, single-target, visual (icons)
    KickBall = 33150, // Boss->self, no cast, single-target, visual
    ShockExplosion = 33151, // BallOfLevin->location, no cast, range 6 circle (initial ball explosion)
    Firemeld = 33152, // Boss->location, no cast, range 6 circle (aoe around 2/4/6/8)
    ShockTowerSoak = 33153, // Helper->self, 2.0s cast, range 3 circle (tower)
    ShockTowerFail = 33154, // Helper->self, no cast, range 60 circle (unsoaked tower)
    Icemeld1 = 33155, // KokytossEcho->self, no cast, range 20 circle (defamation)
    Icemeld2 = 33168, // KokytossEcho->self, no cast, range 20 circle (defamation)
    Icemeld3 = 33169, // KokytossEcho->self, no cast, range 20 circle (defamation)
    Icemeld4 = 33170, // KokytossEcho->self, no cast, range 20 circle (defamation)
    TwoMindsIceFire = 33156, // Boss->location, 7.0s cast, single-target, visual (2-man stacks + in/out)
    TwoMindsIceLightning = 33157, // Boss->location, 7.0s cast, single-target (spreads + in/out)

    Charybdis = 33136, // Boss->self, 3.0s cast, single-target, visual (proteans + meteors)
    CharybdisAOE = 33137, // Helper->location, 4.0s cast, range 6 circle
    Comet = 33138, // Boss->self, 5.0s cast, single-target, visual (proximity)
    CometImpact = 33139, // Comet->self, 5.0s cast, range 60 circle with ? falloff
    CometBurstLong = 33140, // Comet->self, 6.0s cast, range 10 circle
    CometBurstInstant = 33091, // Comet->self, no cast, single-target
    BeastlyBile = 33143, // Boss->self, 5.0s cast, single-target, visual (baited 4-man stack)
    BeastlyBileAOE = 33144, // Helper->players, no cast, range 6 circle 4-man stack
    Thunderbolt = 33145, // Boss->self, 3.0s cast, single-target, visual (proteans)
    ThunderboltAOE = 33146, // Helper->self, no cast, range 40 45-degree cone protean
    EclipticMeteor = 33141, // Boss->self, 7.0s cast, single-target, visual (explode one comet)
    EclipticMeteorVisual = 33142, // Helper->self, no cast, single-target (when cast starts)
    EclipticMeteorAOE = 33160, // Helper->self, 1.0s cast, range 60 circle
    BeastlyFury = 33158, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    BeastlyFuryAOE = 33159, // Helper->self, no cast, range 60 circle, raidwide

    ChimericSuccession = 33211, // Boss->self, 5.0s cast, single-target, visual (limit cut)
    FrontFirestrikes = 34702, // Boss->self, 8.0s cast, single-target, visual (front kick)
    RearFirestrikes = 34703, // Boss->self, 8.0s cast, single-target, visual (rear kick)
    PyremeldFront = 34707, // Boss->location, no cast, range 6 circle, baited 4-man stack
    PyremeldRear = 34708, // Boss->location, no cast, range 6 circle, baited 4-man stack
    SwingingKickFront = 34709, // Boss->self, 3.0s cast, range 40 180-degree cone
    SwingingKickRear = 34710, // Boss->self, 3.0s cast, range 40 180-degree cone

    Disintegration = 33162, // Boss->self, 10.0s cast, single-target, visual (enrage)
    DisintegrationAOE = 33163, // Helper->self, no cast, range 60 circle, enrage
}

public enum IconID : uint
{
    DualityOfDeath = 468, // player
    AscendantFist = 218, // player
    Icon1 = 79, // BallOfLevin/player
    Icon2 = 80, // player
    Icon3 = 81, // BallOfLevin/player
    Icon4 = 82, // player
    Icon5 = 83, // BallOfLevin
    Icon6 = 84, // player
    Icon7 = 85, // BallOfLevin
    Icon8 = 86, // player
    Icemeld = 330, // player
    EclipticMeteor = 435, // Comet
}
