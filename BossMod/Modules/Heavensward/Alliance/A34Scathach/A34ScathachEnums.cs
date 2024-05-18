namespace BossMod.Heavensward.Alliance.A34Scathach;

public enum OID : uint
{
    Boss = 0x194F, // R5.000, x?
    Connla = 0x19A, // R0.500, x?, mixed types
    Connla2 = 0x1952, // R2.700, x?
    ShadowLimb = 0x1950, // R2.000, x?
    ShadowcourtJester = 0x1953, // R0.600, x?
    ChimeraPoppet = 0x1954, // R2.250, x?
    ShadowcourtHound = 0x1955, // R4.440, x?
}

public enum AID : uint
{
    AutoAttack = 7760, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // ChimeraPoppet/ShadowcourtHound->player, no cast, single-target
    AutoAttack3 = 871, // Connla2->player, no cast, single-target

    BigHug = 7631, // ChimeraPoppet->self, 2.5s cast, range 3+R width 3 rect
    BlindingShadow1 = 7476, // Boss->self, 5.0s cast, range 60 circle
    BlindingShadow2 = 7598, // Connla->self, no cast, range 30 circle

    Blizzard = 7777, // ShadowcourtJester->player, 1.0s cast, single-target
    Manos = 7379, // Boss->self, no cast, single-target
    MarrowDrain = 3342, // ShadowcourtHound->self, 3.0s cast, range 6+R 120-degree cone

    NoxAOEFirst = 7458, // Connla->location, 5.0s cast, range 10 circle
    NoxAOERest = 7457, // Connla->location, no cast, range 10 circle

    ParticleBeam1 = 7463, // Connla->location, no cast, range 60 circle
    ParticleBeam2 = 7464, // Connla->location, no cast, range 5 circle
    Pitfall = 7377, // Connla->self, 5.0s cast, range 60 circle
    FullSwing = 7378, // Connla2->self, 7.0s cast, range 60 circle

    Shadesmite1 = 7453, // Connla->self, 1.5s cast, range 15 circle
    Shadesmite2 = 7636, // Connla->self, 1.5s cast, range 3 circle
    Shadesmite3 = 7637, // Connla->self, 1.5s cast, range 3 circle

    Shadespin = 7455, // Boss->self, 4.0s cast, single-target
    Shadespin1 = 7454, // Boss->self, 4.0s cast, single-target
    Shadespin2 = 7456, // Connla->self, 1.2s cast, range 30+R 90-degree cone

    Shadethrust = 7459, // Boss->location, 3.0s cast, range 40+R width 5 rect
    ShadowRelease = 7380, // Boss->self, no cast, single-target
    Shred = 7460, // ShadowLimb->player, no cast, single-target
    Snuggle = 7630, // ChimeraPoppet->player, no cast, single-target
    Soar = 7451, // Boss->location, no cast, ???
    TheDragonsVoice = 3344, // ShadowcourtHound->self, 4.5s cast, range 30 circle

    ThirtyArrows1 = 7471, // Boss->location, 3.0s cast, range 8 circle
    ThirtyArrows2 = 7472, // Connla->self, 2.5s cast, range 35+R width 8 rect

    ThirtyCries = 7475, // Boss->players, no cast, range 12 circle
    ThirtySickles = 7473, // Boss->self, no cast, range 15 circle
    ThirtySouls = 7474, // Boss->self, 4.0s cast, range 60 circle

    ThirtyThorns1 = 7467, // Boss->self, no cast, range 7+R width 3 rect
    ThirtyThorns2 = 7468, // Connla->self, no cast, range 12+R width 3 rect
    ThirtyThorns3 = 7469, // Connla->self, no cast, range 12+R width 3 rect
    ThirtyThorns4 = 7470, // Connla->self, 1.5s cast, range 8 circle

    Unknown = 7452, // Boss->self, no cast, range 15 circle
    Unknown2 = 7376, // Connla2->self, no cast, single-target

    VoidBlizzard = 7778, // ShadowcourtJester->player, 2.5s cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 202, // Connla/Boss->player, extra=0x1/0x2
    ShadowLimb = 1148, // Boss->Boss, extra=0x0
    ShadowLinks = 1147, // none->player, extra=0x3C
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Riled = 1146, // none->Connla/Connla2/Boss, extra=0x0
    Petrification = 610, // none->ShadowLimb, extra=0x0
    EvasionDown = 32, // none->ShadowLimb, extra=0x0
}

public enum IconID : uint
{
    Nox = 197, // player
    Icon_62 = 62, // player
}

public enum TetherID : uint
{
    Tether2 = 2, // Connla2/ShadowLimb->Boss/player
    Tether1 = 1, // ShadowLimb->player
}
