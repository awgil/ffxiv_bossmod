namespace BossMod.Dawntrail.Savage.RM04WickedThunder;

public enum OID : uint
{
    BossP1 = 0x43AA, // R4.900, x1
    BossP2 = 0x43AE, // R22.500, x1
    SerpentsTongue = 0x43AF, // R5.000, x8, tower
    Helper = 0x233C, // R0.500, x25, Helper type
    WickedReplica = 0x43AB, // R3.675-4.900, x8
    GunBattery = 0x43AC, // R1.000, x0 (spawn during fight) - bewitching flight / electrifying witch hunt
    Electromine = 0x43AD, // R1.000, x0 (spawn during fight) - electrope edge
    //_Gen_WickedThunder = 0x4568, // R8.750, x1

    //_Gen_Actor1ea1a1 = 0x1EA1A1, // R0.500-2.000, x1, EventObj type
    //_Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttackP1 = 39147, // BossP1->player, no cast, single-target
    Teleport = 37577, // BossP1->location, no cast, single-target
    WrathOfZeus = 38383, // BossP1->self, 5.0s cast, range 60 circle, raidwide
    WickedJolt = 38384, // BossP1->self/player, 5.0s cast, range 60 width 5 rect, tankbuster hit 1
    WickedJoltSecond = 38385, // BossP1->self, no cast, range 60 width 5 rect, tankbuster hit 2
    WickedBolt = 37570, // BossP1->self, 4.0+1.0s cast, single-target, visual (multihit stack)
    WickedBoltAOE = 38382, // Helper->players, no cast, range 5 circle stack

    WingsStart = 37545, // BossP1->self, no cast, single-target, visual (create wings)
    BewitchingFlightR = 36335, // BossP1->self, 6.0+1.0s cast, single-target, visual (criss-cross)
    BewitchingFlightL = 38513, // BossP1->self, 6.0+1.0s cast, single-target, visual (criss-cross)
    BewitchingFlightAOE = 38377, // Helper->self, 7.0s cast, range 40 width 5 rect, vertical rects
    Electray = 38379, // GunBattery->self, 2.0s cast, range 40 width 5 rect, horizontal rects

    ElectrifyingWitchHunt = 38373, // BossP1->self, 5.0s cast, single-target, visual (center/sides + spread)
    ElectrifyingWitchHuntAOE = 38374, // WickedReplica->players, no cast, range 6 circle spread
    ElectrifyingWitchHuntBurst = 38378, // Helper->self, 6.0s cast, range 40 width 16 rect (center/sides lines)
    ForkedLightning = 38375, // Helper->self, no cast, range 5 circle spread (for initial targets)
    WitchHunt = 38366, // BossP1->self, 5.0s cast, single-target, visual (near/far bait)
    ElectrifyingWitchHuntBait = 38367, // WickedReplica->players, no cast, range 6 circle spread

    WideningWitchHunt = 38368, // BossP1->self, 14.0+1.0s cast, single-target, visual (out-in-out-in)
    NarrowingWitchHunt = 38369, // BossP1->self, 14.0+1.0s cast, single-target, visual (in-out-in-out)
    LightningVortex = 38370, // BossP1->self, no cast, single-target, visual (out)
    Thundering = 38371, // BossP1->self, no cast, single-target, visual (in)
    LightningVortexAOE = 19729, // Helper->self, 1.0s cast, range 10 circle, out
    ThunderingAOE = 19730, // Helper->self, 1.0s cast, range 10-60 donut, in
    WitchHuntAOE = 38372, // WickedReplica->players, no cast, range 6 circle, baited spread
    WingsEnd = 37546, // BossP1->self, no cast, single-target, visual (remove wings)

    ElectropeEdge = 38341, // BossP1->self, 3.0s cast, single-target, visual (safe corner + spread/pairs)
    ElectropeEdgeWitchgleam = 38342, // BossP1->self, 3.0s cast, single-target, visual (rect at electromine)
    ElectropeEdgeWitchgleamRest = 39618, // BossP1->self, no cast, single-target, visual
    ElectropeEdgeWitchgleamAOE = 38343, // Helper->self, no cast, range 60 width 5 rect
    SymphonyFantastique = 38344, // BossP1->self, 3.0s cast, single-target, visual (activate mines hit twice)
    ElectropeEdgeSpark1 = 38345, // Electromine->self, 7.0s cast, range 10 width 10 rect (if hit once)
    ElectropeEdgeSpark2 = 38346, // Electromine->self, 7.0s cast, range 30 width 30 rect (if hit twice)
    ElectropeEdgeSidewiseSparkR = 38380, // BossP1->self, 7.0s cast, range 60 180-degree cone
    ElectropeEdgeSidewiseSparkL = 38381, // BossP1->self, 7.0s cast, range 60 180-degree cone
    FourStar = 38352, // Helper->players, no cast, range 6 circle, 2-man stack
    EightStar = 38353, // Helper->players, no cast, range 6 circle, spread
    LightningCageWitchgleam = 38789, // BossP1->self, 3.0s cast, single-target, visual (rects at people)
    LightningCageWitchgleamAOE = 38790, // Helper->self, no cast, range 60 width 5 rect
    LightningCage = 38350, // BossP1->self, 3.0s cast, single-target, visual (anchor)
    LightningCageAOE = 38351, // Helper->self, 7.0s cast, range 8 width 8 rect
    LightningCageSpark2 = 38347, // Helper->self, no cast, range 24 width 24 rect
    LightningCageSpark3 = 38348, // Helper->self, no cast, range 40 width 40 rect
    LightningCageSparkUnmitigatedExplosion = 38349, // Helper->location, no cast, range 80 circle, wipe if spark target is dead when it expires

    IonCluster = 38356, // BossP1->self, 3.0s cast, single-target, visual (cannon start)
    IonClusterVisualStart = 37543, // BossP1->self, no cast, single-target, visual (show cannon)
    IonClusterVisualReset = 37544, // BossP1->self, no cast, single-target, visual (??? something on wipe)
    IonClusterVisualR = 38354, // BossP1->self, no cast, single-target, visual (aim right)
    IonClusterVisualL = 38355, // BossP1->self, no cast, single-target, visual (aim left)
    StampedingThunderStart = 35334, // BossP1->self, no cast, single-target, visual
    StampedingThunderAOE = 36151, // Helper->self, no cast, range 40 width 30 rect
    StampedingThunderFinish = 36399, // Helper->self, 7.3s cast, range 40 width 30 rect
    ElectronStream1 = 38358, // BossP1->self, 6.0s cast, single-target, visual (positron N, negatron S)
    ElectronStream2 = 38359, // BossP1->self, 6.0s cast, single-target, visual (positron S, negatron N)
    PositronStream = 38360, // Helper->self, 6.0s cast, range 40 width 10 rect
    NegatronStream = 38361, // Helper->self, 6.0s cast, range 40 width 10 rect
    AxeCurrent = 38362, // Helper->self, no cast, range 50 25-degree cone, bait
    SpinningCurrent = 38363, // Helper->players, no cast, range 2 circle
    RoundhouseCurrent = 38364, // Helper->players, no cast, range 10-25 donut
    ColliderCurrent = 38365, // Helper->location, no cast, range 80 circle, raidwide if not cleansed

    ElectropeTransplant = 39123, // BossP1->self, 4.0s cast, single-target, visual (transition)
    FulminousField = 37118, // Helper->self, 3.0s cast, range 30 30-degree cone
    FulminousFieldRest = 39117, // Helper->self, no cast, range 30 30-degree cone
    ConductionPoint = 39118, // Helper->players, no cast, range 6 circle, vuln on 4 targets of same role
    ForkedFissures = 39119, // Helper->self, no cast, range 40 width 10 rect, wild charge
    Soulshock = 20033, // Helper->self, no cast, range 60 circle, raidwide during transition
    Impact = 20034, // Helper->self, no cast, range 60 circle, raidwide during transition
    Cannonbolt = 39120, // Helper->self, no cast, range 60 circle, raidwide during transition
    CrossTailSwitch = 38386, // BossP2->self, 5.0+1.0s cast, single-target, visual (9-hit raidwide)
    CrossTailSwitchAOE = 38387, // Helper->location, no cast, range 60 circle
    CrossTailSwitchLast = 38388, // Helper->location, no cast, range 60 circle

    AutoAttackP2 = 36446, // BossP2->player, no cast, single-target
    AzureThunder = 38447, // BossP2->self, 5.0s cast, range 60 circle, raidwide
    WickedThunder = 38043, // BossP2->self, 5.0s cast, range 60 circle, raidwide

    SabertailFirst = 38389, // Helper->self, 8.0s cast, range 6 circle, exaflare start
    SabertailRest = 38390, // Helper->self, no cast, range 6 circle, exaflare
    WickedBlaze = 38391, // BossP2->self, 5.0s cast, single-target, visual (3-hit party stack)
    WickedBlazeAOE = 38392, // Helper->players, no cast, range 10 circle, 4-man stack
    WickedSpecialCenter = 38416, // BossP2->self, 5.0+1.0s cast, single-target, visual (cleave center)
    WickedSpecialCenterAOE = 38417, // Helper->self, 6.0s cast, range 40 width 20 rect
    WickedSpecialSides = 38418, // BossP2->self, 5.0+1.0s cast, single-target, visual (cleave sides)
    WickedSpecialSidesAOE = 38419, // Helper->self, 6.0s cast, range 40 width 15 rect

    MustardBomb = 38430, // BossP2->self, 8.0+1.0s cast, single-target
    MustardBombFirst = 38431, // Helper->player, no cast, range 6 circle, tether target hit
    KindlingCauldron = 38432, // Helper->player, no cast, range 6 circle, spread applying vuln
    MustardBombSecond = 38433, // Helper->player, no cast, range 6 circle, status target hit

    AetherialConversionHitLR = 38402, // BossP2->self, 7.0s cast, single-target, visual (aoe left->right)
    AetherialConversionKnockbackLR = 38403, // BossP2->self, 7.0s cast, single-target, visual (knockback left->right)
    AetherialConversionHitRL = 38404, // BossP2->self, 7.0s cast, single-target, visual (aoe right->left)
    AetherialConversionKnockbackRL = 38405, // BossP2->self, 7.0s cast, single-target, visual (knockback right->left)
    TailThrust1HitL = 38406, // BossP2->self, 5.0+1.0s cast, single-target, visual (aoe 1 left)
    TailThrust1KnockbackL = 38407, // BossP2->self, 5.0+1.0s cast, single-target, visual (knockback 1 left)
    TailThrust1HitR = 38408, // BossP2->self, 5.0+1.0s cast, single-target, visual (aoe 1 right)
    TailThrust1KnockbackR = 38409, // BossP2->self, 5.0+1.0s cast, single-target, visual (knockback 1 right)
    TailThrust2HitR = 38412, // BossP2->self, no cast, single-target, visual (aoe 2 right)
    TailThrust2KnockbackR = 38413, // BossP2->self, no cast, single-target, visual (knockback 2 right)
    TailThrust2HitL = 38410, // BossP2->self, no cast, single-target, visual (aoe 2 left)
    TailThrust2KnockbackL = 38411, // BossP2->self, no cast, single-target, visual (knockback 2 left)
    TailThrust = 38414, // Helper->location, 1.0s cast, range 18 circle
    SwitchOfTides = 38415, // Helper->location, 1.0s cast, range 60 circle, knockback 25

    TwilightSabbath = 38435, // BossP2->self, 3.0s cast, single-target, visual (mechanic start)
    WickedFire = 38448, // BossP2->self, 4.0s cast, single-target, visual (baited puddles)
    WickedFireAOE = 38449, // Helper->location, 4.0s cast, range 10 circle puddle
    TwilightSabbathSidewiseSparkR = 38441, // WickedReplica->self, 1.0s cast, range 60 180-degree cone
    TwilightSabbathSidewiseSparkL = 38442, // WickedReplica->self, 1.0s cast, range 60 180-degree cone

    MidnightSabbath = 39609, // BossP2->self, 3.0s cast, single-target, visual (mechanic start)
    ConcentratedBurst = 38443, // BossP2->self, 7.0s cast, single-target, visual (pairs -> spread)
    ScatteredBurst = 38444, // BossP2->self, 7.0s cast, single-target, visual (spread -> pairs)
    WickedSpark = 38445, // Helper->players, no cast, range 5 circle, spread
    WickedFlare = 38446, // Helper->players, no cast, range 5 circle, 2-man stack
    MidnightSabbathThundering = 38439, // WickedReplica->self, 1.0s cast, range 5-15 donut
    MidnightSabbathWickedCannon = 38436, // WickedReplica->self, 1.0s cast, range 40 width 10 rect

    FlameSlash = 38420, // BossP2->self, 6.0+1.0s cast, single-target, visual (split arena + towers)
    FlameSlashAOE = 38421, // Helper->self, 7.0s cast, range 40 width 20 rect
    RainingSwords = 38422, // BossP2->self, 2.0+1.0s cast, single-target, visual (towers)
    RainingSwordsAOE = 38423, // SerpentsTongue->location, 3.0s cast, range 3 circle tower
    RainingSwordsUnmitigatedExplosion = 38424, // SerpentsTongue->self, no cast, range 60 circle, tower fail
    ChainLightning = 38425, // BossP2->self, 16.0+1.0s cast, single-target, visual (lightnings between towers)
    ChainLightningAOEFirst = 38426, // Helper->location, no cast, range 7 circle
    ChainLightningAOERest = 38427, // Helper->location, no cast, range 7 circle

    SunriseSabbathIonCluster = 38434, // BossP2->self, 3.0s cast, single-target, visual (debuffs)
    SunriseSabbath = 39610, // BossP2->self, 3.0s cast, single-target, visual (mechanic start)
    SunriseSabbathPositronStream1 = 38437, // WickedReplica->self, no cast, range 40 width 12 rect
    SunriseSabbathNegatronStream1 = 38438, // WickedReplica->self, no cast, range 40 width 12 rect
    SoaringSoulpress1 = 38440, // WickedReplica->location, 3.0s cast, range 3 circle tower
    SunriseSabbathPositronStream2 = 39257, // WickedReplica->self, no cast, range 40 width 12 rect
    SunriseSabbathNegatronStream2 = 39258, // WickedReplica->self, no cast, range 40 width 12 rect
    SoaringSoulpress2 = 39259, // WickedReplica->location, 3.0s cast, range 3 circle tower
    SoaringSoulpressUnmitigatedExplosion = 39297, // Helper->self, 1.5s cast, range 60 circle, tower fail
    IonicDischarge = 38357, // Helper->player, no cast, single-target, kill if debuff expires without soaking correct cannon

    SwordQuiverN = 38393, // BossP2->self, 5.0+1.3s cast, single-target, visual (swords across N)
    SwordQuiverC = 38394, // BossP2->self, 5.0+1.3s cast, single-target, visual (swords across center)
    SwordQuiverS = 38395, // BossP2->self, 5.0+1.3s cast, single-target, visual (swords across S)
    SwordQuiverRaidwide1 = 38396, // Helper->location, 1.3s cast, range 60 circle, raidwide
    SwordQuiverRaidwide2 = 38397, // Helper->location, 2.3s cast, range 60 circle, raidwide
    SwordQuiverRaidwide3 = 38398, // Helper->location, 3.3s cast, range 60 circle, raidwide
    SwordQuiverRaidwide4 = 38399, // Helper->location, 4.5s cast, range 60 circle, raidwide
    SwordQuiverBurst = 38400, // Helper->self, 1.0s cast, range 60 width 12 rect
    SwordQuiverLaceration = 38401, // Helper->self, no cast, range 40 ?-degree cone

    Enrage = 38450, // BossP2->self, 10.0s cast, range 60 circle, enrage
}

public enum SID : uint
{
    ForkedLightning = 587, // WickedReplica->player, extra=0x0
    Marker = 2970, // none->BossP1/WickedReplica, extra=0x2F6/0x2F7/0x2F0/0x2F1/0x2F4/0x2F5
    ElectricalCondenser = 3999, // none->player, extra=0x0
    Positron = 4000, // none->player, extra=0x3/0x2/0x1
    Negatron = 4001, // none->player, extra=0x3/0x2/0x4/0x1
    RemoteCurrent = 4002, // none->player, extra=0x0 (bait at farthest)
    ProximateCurrent = 4003, // none->player, extra=0x0 (bait at closest)
    SpinningConductor = 4004, // none->player, extra=0x0 (circle)
    RoundhouseConductor = 4005, // none->player, extra=0x0 (donut)
    ColliderConductor = 4006, // none->player, extra=0x0 (need to be hit by bait)
    MustardBomb = 4007, // Helper->player, extra=0x0
    MustardBombproof = 4008, // none->player, extra=0x0
}

public enum IconID : uint
{
    WickedJolt = 471, // player
    WickedBolt = 316, // player
}

public enum TetherID : uint
{
    MustardBomb = 283, // player->BossP2
    ChainLightning1 = 279, // SerpentsTongue->BossP2
    ChainLightning2 = 280, // SerpentsTongue->SerpentsTongue
}
