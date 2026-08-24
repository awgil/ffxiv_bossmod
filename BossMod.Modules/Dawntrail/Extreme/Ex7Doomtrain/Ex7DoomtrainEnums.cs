namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

public enum OID : uint
{
    Boss = 0x4A37, // R19.040, x1
    Helper = 0x233C,
    HeadlightTrain = 0x4A3B, // R1.000, x1, Helper type, only has one job
    LevinSignal = 0x4A38, // R1.000, x0 (spawn during fight)
    KinematicTurret = 0x4A39, // R1.200, x0 (spawn during fight)
    AetherIntermission = 0x4A3A, // R1.500, x0 (spawn during fight)
    GhostTrain = 0x4B81, // R2.720, x0 (spawn during fight)
    IntermissionTrain = 0x4B7F, // R19.040, x0 (spawn during fight)
    ArcaneRevelation = 0x4A36, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45716, // Helper->player, no cast, single-target
    DeadMansOverdraughtSpread = 45663, // Boss->self, 4.0+1.0s cast, single-target
    DeadMansOverdraughtStack = 45664, // Boss->self, 4.0+1.0s cast, single-target
    DeadMansExpress = 45670, // Boss->self, 6.0s cast, range 70 width 70 rect
    DeadMansWindpipeBoss = 45677, // Boss->self, 6.0+1.0s cast, single-target
    DeadMansWindpipe = 45696, // Helper->self, 7.0s cast, range 30 width 20 rect
    DeadMansBlastpipeBoss = 45678, // Boss->self, no cast, single-target
    DeadMansBlastpipe = 45679, // Helper->self, 3.0s cast, range 10 width 20 rect
    Plasma = 45675, // Helper->players, no cast, range 5 circle, spread
    HyperexplosivePlasma = 45676, // Helper->players, no cast, range 5 circle, n-player stack

    PlasmaBeamGround = 45671, // LevinSignal->self, 1.0s cast, range 30 width 5 rect
    PlasmaBeamOverhead = 45672, // LevinSignal->self, 1.0s cast, range 30 width 5 rect
    PlasmaBeamLong = 45673, // LevinSignal->self, 1.0s cast, range 20 width 5 rect
    PlasmaBeamMedium = 45674, // LevinSignal->self, 1.0s cast, range 10 width 5 rect
    UnlimitedExpressBoss = 45623, // Boss->self, 5.0s cast, single-target
    UnlimitedExpress = 45680, // Helper->self, 5.9s cast, range 70 width 70 rect
    TurretCrossing = 45628, // Boss->self, 3.0s cast, single-target
    Electray1 = 45681, // KinematicTurret->self, 7.0s cast, range 25 width 5 rect
    Electray2 = 45682, // KinematicTurret->self, 7.0s cast, range 25 width 5 rect
    ElectrayMedium = 45683, // KinematicTurret->self, 7.0s cast, range 20 width 5 rect
    ElectrayShort = 45686, // KinematicTurret->self, 7.0s cast, range 5 width 5 rect
    LightningBurstCast = 45660, // Boss->self, 5.0s cast, single-target
    LightningBurst = 45715, // Helper->player, 0.5s cast, range 5 circle

    RunawayTrainStart = 45638, // Boss->self, 5.0s cast, single-target
    Overdraught = 45639, // AetherIntermission->self, no cast, single-target
    Aetherochar = 45697, // Helper->player, no cast, range 6 circle, spread on non-tanks
    Aetherosote = 45698, // Helper->players, no cast, range 6 circle, stack on healers
    AetherialRay = 45693, // Helper->self, no cast, range 50 35-degree cone
    RunawayTrainRaidwide = 45645, // Helper->self, no cast, range 20 circle
    RunawayTrainEnd = 45644, // IntermissionTrain->self, no cast, single-target

    ShockwaveVisual = 45646, // Boss->self, no cast, single-target
    Shockwave = 45699, // Helper->self, no cast, range 50 circle
    DerailmentSiegeCast1 = 45648, // Boss->self, 6.0+1.0s cast, single-target
    DerailmentSiegeCast2 = 45700, // Boss->self, 6.0+1.0s cast, single-target
    DerailmentSiegeCast3 = 45701, // Boss->self, 6.0+1.0s cast, single-target
    DerailmentSiegeCast4 = 45702, // Boss->self, 6.0+1.0s cast, single-target

    DerailmentSiegeHit = 45706, // Helper->self, no cast, range 5 circle
    DerailmentSiegeHit2 = 45707, // Helper->self, 0.5s cast, range 5 circle
    DerailmentSiegeFinal1 = 45649, // Helper->self, 10.0s cast, range 5 circle
    DerailmentSiegeFinal2 = 45703, // Helper->self, 11.0s cast, range 5 circle
    DerailmentSiegeFinal3 = 45704, // Helper->self, 12.0s cast, range 5 circle
    DerailmentSiegeFinal4 = 45705, // Helper->self, 13.0s cast, range 5 circle

    DerailBoss1 = 45709, // Boss->self, 5.1s cast, single-target
    DerailBoss2 = 46489, // Boss->self, 5.1s cast, single-target
    DerailBoss3 = 46490, // Boss->self, 5.1s cast, single-target
    DerailBossEnrage = 45710, // Boss->self, 15.1s cast, single-target

    Derail = 45711, // Helper->self, 5.0s cast, range 30 width 20 rect
    DerailEnrage = 45712, // Helper->self, 15.0s cast, range 30 width 20 rect

    Zoom = 45655, // Boss->location, no cast, single-target

    ThirdRailSnap = 45665, // Boss->self, no cast, single-target
    ThirdRailPuddle = 45666, // Helper->location, 3.0s cast, range 4 circle

    ThunderousBreathFirst = 45687, // Boss->self, 6.0s cast, single-target
    ThunderousBreathSecond = 45688, // Boss->self, no cast, single-target
    ThunderousBreath = 45689, // Helper->self, no cast, range 70 width 70 rect
    HeadlightFirst = 45690, // Boss->self, 6.0s cast, single-target
    HeadlightSecond = 45691, // Boss->self, no cast, single-target
    Headlight = 45692, // HeadlightTrain->self, no cast, range 70 width 70 rect

    ArcaneRevelation = 47527, // Boss->self, 2.0+1.0s cast, single-target

    HailOfThunderShort = 45656, // Boss->self, no cast, single-target
    HailOfThunderMedium = 45657, // Boss->self, no cast, single-target
    HailOfThunderLong = 45658, // Boss->self, no cast, single-target

    HailOfThunder = 45713, // Helper->location, 2.7s cast, range 16 circle
    HyperconductivePlasma = 45714, // Helper->players, no cast, range 13 circle

    Psychokinesis = 45668, // Boss->self, 7.0s cast, single-target
    Plummet = 45669, // Helper->player, 5.0s cast, range 8 circle
}

public enum IconID : uint
{
    LightningBurst = 343, // player->self
    Horn = 639, // IntermissionTrain->self
    AetherialRay = 412, // player->self
    DoubleToot = 637, // IntermissionTrain->self
    TripleToot = 638, // IntermissionTrain->self
    Plummet = 499, // player->self
}

public enum SID : uint
{
    DeadMansOverdraught = 4720, // none->Boss, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    Distance = 4541, // none->GhostTrain, extra=0x578 (170 degree rotation)/0x960 (106 degree rotation)
    Unk1 = 2056, // none->IntermissionTrain, extra=0x400
    Unk2 = 2552, // none->GhostTrain, extra=0x42B
    Stop = 4176, // none->GhostTrain, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0
    SystemLock = 2578, // none->player, extra=0x0
    UpHigh = 4721, // none->player, extra=0x0
    HeadlightThunderBreath = 3913, // none->Boss, extra=0x3D8 (ground first)/0x3D7 (air first)
    DesignatedConductor = 4719, // none->player, extra=0x0
    PhysicalVulnerabilityUp = 2940, // Helper->player, extra=0x0
}
