#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

public enum OID : uint
{
    Boss = 0x4A37,
    Helper = 0x233C,
    _Gen_Doomtrain = 0x4A3B, // R1.000, x1, Helper type
    _Gen_LevinSignal = 0x4A38, // R1.000, x0 (spawn during fight)
    _Gen_KinematicTurret = 0x4A39, // R1.200, x0 (spawn during fight)
    _Gen_Aether = 0x4A3A, // R1.500, x0 (spawn during fight)
    _Gen_GhostTrain = 0x4B81, // R2.720, x0 (spawn during fight)
    _Gen_Doomtrain1 = 0x4B7F, // R19.040, x0 (spawn during fight)
    _Gen_ = 0x4A36, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Ability_ = 45716, // Helper->player, no cast, single-target
    DeadMansOverdraughtSpread = 45663, // Boss->self, 4.0+1.0s cast, single-target
    DeadMansOverdraughtStack = 45664, // Boss->self, 4.0+1.0s cast, single-target
    DeadMansExpress = 45670, // Boss->self, 6.0s cast, range 70 width 70 rect
    DeadMansWindpipeBoss = 45677, // Boss->self, 6.0+1.0s cast, single-target
    DeadMansWindpipe = 45696, // Helper->self, 7.0s cast, range 30 width 20 rect
    DeadMansBlastpipeBoss = 45678, // Boss->self, no cast, single-target
    DeadMansBlastpipe = 45679, // Helper->self, 3.0s cast, range 10 width 20 rect
    PlasmaBeamGround = 45671, // _Gen_LevinSignal->self, 1.0s cast, range 30 width 5 rect
    PlasmaBeamOverhead = 45672, // _Gen_LevinSignal->self, 1.0s cast, range 30 width 5 rect
    _Ability_HyperexplosivePlasma = 45676, // Helper->players, no cast, range 5 circle
    _Ability_Plasma = 45675, // Helper->players, no cast, range 5 circle
    _Ability_UnlimitedExpress = 45623, // Boss->self, 5.0s cast, single-target
    _Ability_UnlimitedExpress1 = 45680, // Helper->self, 5.9s cast, range 70 width 70 rect
    _Ability_1 = 45641, // Boss->location, no cast, single-target
    _Ability_TurretCrossing = 45628, // Boss->self, 3.0s cast, single-target
    _Ability_Electray = 45681, // _Gen_KinematicTurret->self, 7.0s cast, range 25 width 5 rect
    _Ability_Electray1 = 45686, // _Gen_KinematicTurret->self, 7.0s cast, range 5 width 5 rect
    _Ability_Electray2 = 45683, // _Gen_KinematicTurret->self, 7.0s cast, range 20 width 5 rect
    _Ability_LightningBurst = 45660, // Boss->self, 5.0s cast, single-target
    _Ability_LightningBurst1 = 45715, // Helper->player, 0.5s cast, range 5 circle
    _Ability_PlasmaBeam2 = 45674, // _Gen_LevinSignal->self, 1.0s cast, range 10 width 5 rect
    _Ability_PlasmaBeam3 = 45673, // _Gen_LevinSignal->self, 1.0s cast, range 20 width 5 rect
    _Ability_RunawayTrain = 45638, // Boss->self, 5.0s cast, single-target
    _Ability_Overdraught = 45639, // _Gen_Aether->self, no cast, single-target
    _Ability_Aetherosote = 45698, // Helper->players, no cast, range 6 circle
    _Ability_AetherialRay = 45693, // Helper->self, no cast, range 50 ?-degree cone
    _Ability_Aetherochar = 45697, // Helper->player, no cast, range 6 circle
}

public enum IconID : uint
{
    LightningBurst = 343, // player->self
    Horn = 639, // _Gen_Doomtrain1->self
    AetherialRay = 412, // player->self
    DoubleToot = 637, // _Gen_Doomtrain1->self
    TripleToot = 638, // _Gen_Doomtrain1->self
}

public enum SID : uint
{
    _Gen_DeadMansOverdraught = 4720, // none->Boss, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_VulnerabilityUp = 1789, // Helper/_Gen_KinematicTurret/_Gen_LevinSignal/_Gen_Doomtrain->player, extra=0x1/0x2/0x3
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_ = 2056, // none->_Gen_Doomtrain1, extra=0x400
    _Gen_Distance = 4541, // none->_Gen_GhostTrain, extra=0x578 (170 degree rotation)/0x960 (106 degree rotation)
    _Gen_Unk1 = 2552, // none->_Gen_GhostTrain, extra=0x42B
    _Gen_Stop = 4176, // none->_Gen_GhostTrain, extra=0x0
    _Gen_Invincibility = 1570, // none->player, extra=0x0
    _Gen_SystemLock = 2578, // none->player, extra=0x0
    _Gen_Unk2 = 4721, // none->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_Unk3 = 3913, // none->Boss, extra=0x3D8/0x3D7
    _Gen_DesignatedConductor = 4719, // none->player, extra=0x0
    _Gen_PhysicalVulnerabilityUp = 2940, // Helper->player, extra=0x0

}

