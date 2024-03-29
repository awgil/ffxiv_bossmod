namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

public enum OID : uint
{
    Boss = 0x39EA, // x1
    Helper = 0x233C, // x40
    Jaws = 0x39EB, // spawn during fight
    JumpMarker = 0x39EC, // spawn during fight
    LivelyBait = 0x39ED, // spawn during fight
    TopazStoneAny = 0x1EB78C, // EventObj type, spawn during fight ('bent circle' spawns near wall and looks towards wall)
    TopazStonePoison = 0x1EB78D, // EventObj type, spawn during fight
};

public enum AID : uint
{
    AutoAttack = 31246, // Boss->player, no cast, single-target
    Teleport = 30453, // Boss->location, no cast, single-target

    SonicHowl = 30496, // Boss->self, 5.0s cast, raidwide
    RubyGlow = 30451, // Boss->self, 5.0s cast, single-target, visual
    RubyGlowAOE = 30452, // Helper->self, 5.0s cast, raidwide

    TopazStones = 30461, // Boss->self, 4.0s cast, single-target, visual
    TopazStonesVisualHelper = 30462, // Helper->self, 4.0s cast, single-target, visual
    TopazRayStones = 31230, // Helper->self, 12.0s cast, range 4 circle (semicircle?..)
    RubyReflectionQuarter = 30464, // Helper->self, no cast, single-target, visual
    RubyReflectionQuarterAOE = 30465, // Helper->self, no cast, range 15 width 15 rect (1/4 arena)
    RubyReflectionHalf = 30456, // Helper->self, no cast, single-target, visual
    RubyReflectionHalfAOE = 30457, // Helper->self, no cast, ??? (1/2 arena by diagonal)

    VenomousMass = 30493, // Boss->self, 5.0s cast, single-target, visual
    VenomousMassAOE = 30494, // Helper->players, no cast, range 6 circle, tankbuster
    ToxicCrunch = 30794, // Boss->self, 5.0s cast, single-target, visual
    ToxicCrunchAOE = 30495, // Boss->player, no cast, single-target, tankbuster

    Venom = 30476, // Helper->players, no cast, range 5 circle shared aoe (on closest player to unsoaked tower)
    Scatterbait = 30477, // LivelyBait->self, 10.0s cast, raidwide (if not killed in time)

    DoubleRush = 30491, // Boss->location, 6.0s cast, width 100 rect charge, knockback 15 & physical vuln up
    DoubleRushReturn = 30492, // Boss->location, no cast, width 100 rect charge, knockback 15

    TopazCluster = 30466, // Boss->self, 4.0s cast, single-target, visual
    TopazClusterHit1 = 30467, // Helper->self, 4.0s cast, single-target, visual
    TopazClusterHit2 = 30468, // Helper->self, 6.5s cast, single-target, visual
    TopazClusterHit3 = 30469, // Helper->self, 9.0s cast, single-target, visual
    TopazClusterHit4 = 30470, // Helper->self, 11.5s cast, single-target, visual
    TopazRayCluster = 31231, // Helper->self, 9.0s cast, range 4 circle

    VenomSquall = 30486, // Boss->self, 5.0s cast, single-target, visual
    VenomSurge = 30487, // Boss->self, 5.0s cast, single-target, visual
    VenomRain = 30488, // Helper->player, no cast, range 5 circle baited (spread)
    VenomDrops = 30489, // Helper->location, 3.0s cast, range 5 circle baited (stack mid)
    VenomPool = 30490, // Helper->players, no cast, range 5 circle shared (party stacks on healers)

    ClawToTail = 30478, // Boss->self, 6.0s cast, single-target, visual
    RagingClawFirst = 30479, // Helper->self, 6.0s cast, range 45 180-degree cone aoe, first hit
    RagingClawFirstRest = 30480, // Helper->self, no cast, range 45 180-degree cone
    RagingTailSecond = 30481, // Boss->self, no cast, range 45 180-degree cone
    TailToClaw = 30482, // Boss->self, 5.5s cast, single-target, visual
    RagingTailFirst = 31244, // Helper->self, 6.0s cast, range 45 180-degree cone
    RagingClawSecondVisual = 30483, // Boss->self, no cast, single-target, visual
    RagingClawSecond = 30484, // Helper->self, no cast, range 45 180-degree cone
    RagingClawSecondRest = 30485, // Helper->self, no cast, range 45 180-degree cone

    BeginJawsTeleports = 30472, // Boss->self, no cast, single-target
    JawsTeleport = 30473, // Jaws->location, no cast, single-target
    StarvingStampedeTeleport = 30474, // Boss->location, no cast, single-target
    StarvingStampede = 31235, // Helper->self, 1.8s cast, range 12 circle aoe
    DevourPlayer = 30501, // Boss->player, no cast, single-target (players that failed mechanic)
    SpitPlayer = 30503, // Boss->self, 2.0s cast, single-target
    DevourPlayerKill = 30504, // Helper->player, no cast, single-target
    ImpactPlayer = 31263, // Helper->self, 4.5s cast, range 6 circle (related to failing a mechanic?)
    DevourBait = 30793, // Boss->LivelyBait, no cast, single-target

    VenomPoolRecolor = 31202, // Boss->self, 5.0s cast, single-target, visual
    VenomPoolRecolorAOE = 31203, // Helper->players, no cast, range 5 circle shared
    SearingRay = 30455, // Boss->self, 5.0s cast, reflected aoe
    RagingClaw = 30458, // Boss->self, 5.0s cast, single-target
    RagingClawAOEFirst = 30459, // Helper->self, 5.0s cast, range 45 180-degree cone
    RagingClawAOERest = 30460, // Helper->self, no cast, range 45 180-degree cone

    SonicShatter = 30497, // Boss->self, 5.0s cast, raidwide
    SonicShatterRest = 30498, // Boss->self, no cast, raidwide
    AcidicSlaver = 30499, // Boss->self, 5.0s cast, enrage
};
