namespace BossMod.Stormblood.Alliance.A31Mustadio;

public enum OID : uint
{
    Boss = 0x25B7, // R5.880, x?
    Helper = 0x233C, // R0.500, x?, 523 type
    Actor1eaa60 = 0x1EAA60, // R0.500, x0 (spawn during fight), EventObj type
    EarlyTurret = 0x25B8, // R2.660, x4
    Fran = 0x273E, // R0.595, x1
    IronConstruct = 0x25B9, // R4.550, x8
    Montblanc = 0x239D, // R0.300, x1
    Mustadio1 = 0x25BC, // R1.000, x0 (spawn during fight)
    Mustadio2 = 0x262A, // R0.500, x1
}

public enum AID : uint
{
    AutoAttack = 14138, // Boss->player, no cast, single-target
    Analysis = 14133, // Boss->self, 4.0s cast, single-target
    ArmShot = 14137, // Boss->player, 4.0s cast, single-target
    BallisticImpact1 = 14147, // Helper->player, 5.0s cast, range 6 circle
    BallisticImpact2 = 14149, // Boss->self, no cast, range 60 circle
    BallisticMissile = 14140, // Boss->self, 3.0s cast, single-target
    Compress = 14144, // IronConstruct->self, 2.0s cast, range 100 width 15 rect
    EnergyBurst = 14139, // Boss->self, 4.0s cast, range 90 circle
    LastTestament = 14135, // Boss->self, 6.0s cast, range 100 width 60 rect
    LeftHandgonne = 14143, // Boss->self, 4.0s cast, range 30 210-degree cone
    LegShot1 = 14136, // Boss->self, 4.0s cast, single-target
    LegShot2 = 14146, // Helper->location, no cast, range 6 circle
    Maintenance = 14132, // Boss->self, 8.0s cast, single-target
    RightHandgonne = 14142, // Boss->self, 4.0s cast, range 30 210-degree cone
    SatelliteBeam = 14145, // EarlyTurret->self, 2.0s cast, range 30 width 30 rect
    Searchlight1 = 14141, // Boss->self, no cast, single-target
    Searchlight2 = 14148, // Helper->self, no cast, range 5 circle
    Unknown1 = 14134, // Boss->self, no cast, single-target
    Unknown2 = 14150, // Boss->self, no cast, single-target
    Unknown3 = 14188, // Helper->self, no cast, range 60 circle
}

public enum SID : uint
{
    BackUnseen = 1709, // none->player, extra=0x56
    DownForTheCount = 783, // Boss->player, extra=0xEC7
    LeftUnseen = 1708, // none->player, extra=0x58
    Paralysis = 17, // Helper->player, extra=0x0
    RightUnseen = 1707, // none->player, extra=0x57
    Stun = 149, // Boss->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 202, // Boss/IronConstruct->player, extra=0x1/0x2
    Weakness = 43, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_127 = 127, // player
    Icon_139 = 139, // player
    Icon_152 = 152, // player
    Icon_164 = 164, // player
    Icon_186 = 186, // player
    Icon_198 = 198, // player
}

public enum TetherID : uint
{
    Tether_86 = 86, // IronConstruct->Boss
    Tether_88 = 88, // EarlyTurret->Boss
}
