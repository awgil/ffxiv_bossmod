namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

public enum OID : uint
{
    Boss = 0x4AEC, // R5.000, x1
    Helper = 0x233C, // R0.500, x25-36, Helper type
    TheTyrant = 0x4AEA, // R3.000, x3-4
    Comet = 0x4AED, // R2.160, x8
    Axe = 0x4AF0, // R1.000, x2
    Scythe = 0x4AF1, // R1.000, x2
    Sword = 0x4AF2, // R1.000, x2
    Maelstrom = 0x4AEF, // R1.000, x0 (spawn during fight)

    AtomicImpact = 0x1EBF24, // fire puddle
}

public enum AID : uint
{
    AutoAttack = 46085, // Boss->player, no cast, single-target
    CrownOfArcadia = 46086, // Boss->self, 5.0s cast, range 60 circle
    RawSteelTrophyAxe = 46114, // Boss->self, 2.0+4.0s cast, single-target
    RawSteelTrophyScythe = 46115, // Boss->self, 2.0+4.0s cast, single-target
    RawSteelAxeBoss = 46090, // Boss->self, no cast, single-target
    RawSteelAxeBuster = 46091, // Boss->players, no cast, range 6 circle, tankbuster
    RawSteelAxeImpact = 46092, // Helper->players, no cast, range 6 circle, spread
    RawSteelScytheBoss1 = 46093, // Boss->self, no cast, single-target
    RawSteelScytheBoss2 = 46094, // Boss->self, no cast, single-target
    RawSteelScytheBuster = 46095, // Helper->self, no cast, range 60 90-degree cone, tankbuster
    RawSteelScytheHeavyHitter = 46096, // Helper->self, no cast, range 60 45-degree cone, party stack

    TrophyWeapons = 46102, // Boss->self, 3.0s cast, single-target
    AssaultEvolvedLongCast = 46103, // Boss->self, 6.0s cast, single-target
    AssaultEvolvedAxeVisual = 46403, // Boss->location, no cast, single-target
    AssaultEvolvedScytheVisual = 46404, // Boss->location, no cast, single-target
    AssaultEvolvedSwordVisual = 46405, // Boss->location, no cast, single-target
    AssaultEvolvedAxeAOE = 46104, // Helper->self, 2.0s cast, range 8 circle
    AssaultEvolvedScytheAOE = 46105, // Helper->self, 2.0s cast, range 5-60 donut
    AssaultEvolvedSwordAOE = 46106, // Helper->self, 2.0s cast, range 40 width 10 cross
    HeavyWeight = 46107, // Helper->players, no cast, range 6 circle, axe
    SweepingVictory = 46108, // Helper->self, no cast, range 60 30-degree cone, scythe
    SharpTaste = 46109, // Helper->self, no cast, range 60 width 6 rect, sword

    VoidStardust = 46098, // Boss->self, 4.0+1.0s cast, single-target
    Cometite = 46099, // Helper->location, 3.0s cast, range 6 circle
    CometSpread = 46100, // Helper->players, 5.0s cast, range 6 circle
    CrushingCometStack = 46101, // Helper->players, 5.0s cast, range 6 circle

    DanceOfDominationTrophy = 47035, // Boss->self, 2.0+4.0s cast, single-target
    DanceOfDominationBoss = 46110, // Boss->self, no cast, single-target
    DanceOfDominationFirst = 46111, // Helper->self, 0.5s cast, range 60 circle
    DanceOfDominationRepeat = 46113, // Helper->self, no cast, range 60 circle
    DanceOfDominationLast = 47082, // Helper->self, no cast, range 60 circle
    EyeOfTheHurricane = 46116, // Helper->players, 5.0s cast, range 6 circle
    HurricaneExplosion = 46112, // Helper->self, 5.0s cast, range 60 width 10 rect
    HurricaneExplosionVisual = 47036, // Helper->self, 5.5s cast, range 60 width 10 rect, visual only? doesn't deal damage
    Charybdistopia = 46117, // Boss->self, 5.0s cast, range 60 circle
    UltimateTrophyWeapons = 47085, // Boss->self, 3.0s cast, single-target
    AssaultApex = 47086, // Boss->self, 5.0s cast, single-target
    Charybdis = 46118, // Helper->player, no cast, single-target, tornado
    PowerfulGust = 46119, // Helper->self, no cast, range 60 90-degree cone
    ImmortalReign = 46120, // Boss->self, 3.0+1.0s cast, single-target
    OneAndOnlyBoss = 46121, // Boss->self, 6.0+2.0s cast, single-target
    OneAndOnlyRaidwide = 46122, // Helper->self, 9.0s cast, range 60 circle

    GreatWallOfFireCast = 46123, // Boss->self, 5.0s cast, single-target
    GreatWallOfFireRect = 46124, // Boss->self, no cast, range 60 width 6 rect
    GreatWallOfFireUnk = 46125, // Helper->self, no cast, single-target
    WallExplosion = 46126, // Helper->self, 3.0s cast, range 60 width 6 rect
    OrbitalOmenBoss = 46130, // Boss->self, 5.0s cast, single-target
    OrbitalOmenRect = 46131, // Helper->self, 6.0s cast, range 60 width 10 rect
    FireAndFuryBoss = 46127, // Boss->self, 4.0+1.0s cast, single-target
    FireAndFuryFront = 46128, // Helper->self, 5.0s cast, range 60 90-degree cone
    FireAndFuryBack = 46129, // Helper->self, 5.0s cast, range 60 90-degree cone

    MeteorainBoss = 46132, // Boss->self, 5.0s cast, single-target
    FearsomeFireballBoss = 46137, // Boss->self, 5.0s cast, single-target
    FearsomeFireballCharge = 46138, // Boss->self, no cast, range 60 width 6 rect
    CosmicKiss = 46133, // Comet->self, no cast, range 4 circle
    ForegoneFatality = 46134, // TheTyrant->player/Comet, no cast, single-target
    UnmitigatedExplosionComet = 46135, // Comet->self, no cast, range 60 circle, comet explodes if dropped too close to other comet
    ExplosionComet = 46136, // Comet->self, 3.0s cast, range 8 circle, comet explodes when hit
    TripleTyrannhilationInstant = 46139, // Boss->self, no cast, single-target
    TripleTyrannhilationCast = 46140, // Boss->self, 7.0+1.0s cast, single-target
    Shockwave = 46141, // Helper->self, no cast, range 60 circle
    CometBreak = 46142, // Comet->self, no cast, single-target

    FlatlinerBoss = 46143, // Boss->self, 4.0+2.0s cast, single-target
    Flatliner = 47760, // Helper->self, 6.0s cast, range 60 circle
    MajesticMeteorBoss = 46144, // Boss->self, 5.0s cast, single-target
    ExplosionTower = 46148, // Helper->self, 10.0s cast, range 4 circle
    UnmitigatedExplosionTower = 46149, // Helper->self, no cast, range 60 circle
    MajesticMeteorAOE = 46145, // Helper->location, 3.0s cast, range 6 circle
    FireBreathBoss = 46150, // Boss->self, 8.0+1.0s cast, single-target
    FireBreathRect = 46151, // Helper->self, no cast, range 60 width 6 rect
    MajesticMeteowrathRect = 46147, // Helper->self, no cast, range 60 width 10 rect
    MajesticMeteorainRect = 46146, // Helper->self, no cast, range 60 width 10 rect
    MassiveMeteorBoss = 46152, // Boss->self, 5.0s cast, single-target
    MassiveMeteorStack = 46153, // Helper->players, no cast, range 6 circle

    ArcadionAvalancheBoss1 = 46154, // Boss->self, 6.0+9.5s cast, single-target
    ArcadionAvalancheBoss2 = 46156, // Boss->self, 6.0+9.5s cast, single-target
    ArcadionAvalancheBoss3 = 46158, // Boss->self, 6.0+9.5s cast, single-target
    ArcadionAvalancheBoss4 = 46160, // Boss->self, 6.0+9.5s cast, single-target
    ArcadionAvalanchePlatform1 = 46155, // Helper->self, 15.5s cast, range 40 width 40 rect
    ArcadionAvalanchePlatform2 = 46157, // Helper->self, 15.5s cast, range 40 width 40 rect
    ArcadionAvalanchePlatform3 = 46159, // Helper->self, 15.5s cast, range 40 width 40 rect
    ArcadionAvalanchePlatform4 = 46161, // Helper->self, 15.5s cast, range 40 width 40 rect

    EclipticStampede = 46162, // Boss->self, 5.0s cast, single-target
    MammothMeteor = 46163, // Helper->location, 6.0s cast, range 60 circle
    AtomicImpact = 46164, // Helper->players, no cast, range 5 circle
    MajesticMeteorEcliptic = 46165, // Helper->location, 3.0s cast, range 6 circle
    CosmicKissEcliptic = 46166, // Helper->location, 5.0s cast, range 4 circle
    WeightyImpact = 46167, // Helper->location, 5.0s cast, range 4 circle
    UnmitigatedExplosionEcliptic = 46168, // Helper->self, no cast, range 60 circle
    MajesticMeteowrathEcliptic = 46169, // Helper->self, no cast, range 60 width 10 rect

    TwoWayFireballBoss = 47037, // Boss->self, 6.0+1.0s cast, single-target
    TwoWayFireball = 47038, // Helper->self, no cast, range 60 width 6 rect
    FourWayFireballBoss = 46170, // Boss->self, 6.0+1.0s cast, single-target
    FourWayFireball = 46171, // Helper->self, no cast, range 60 width 6 rect

    HeartbreakKickBoss = 46173, // Boss->self, 5.0+1.0s cast, single-target
    HeartbreakKickTower = 46174, // Helper->self, no cast, range 4 circle
    ToughBreak = 46177, // Helper->self, no cast, range 60 circle, heartbreak kick tower miss
    HeartbreakerCast = 46178, // Boss->self, 8.0s cast, single-target
    HeartbreakerEnrage = 46179, // Boss->self, no cast, range 60 circle

    Jump = 46084, // Boss->location, no cast, single-target
    UnkHK1 = 46172, // Boss->self, no cast, single-target
    UnkHK2 = 46175, // Boss->self, no cast, single-target
    Unk1 = 46176, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    FireResistanceDownII = 2937, // none->player, extra=0x0
    PhysicalVulnerabilityUp = 2940, // Boss/Helper/Comet->player, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper/Boss/TheTyrant->player/Comet, extra=0x0
    Unk1 = 3913, // none->_Gen_1/_Gen_/Boss/_Gen_2, extra=0x3EB/0x3EC/0x0/0x432
    Unk2 = 4435, // none->TheTyrant/Boss, extra=0xA
}

public enum IconID : uint
{
    Stack = 161, // player->self
    Spread = 139, // player->self
    FireBreath = 244, // player->self
    WildCharge = 525, // Boss->player
    MultiStack = 305, // player->self
    AtomicImpactPrey = 30, // player->self
}

public enum TetherID : uint
{
    ForegoneFatality = 356, // TheTyrant->Comet/player
    Unstretched = 57, // TheTyrant->player
    Stretched = 249, // TheTyrant->player
}
