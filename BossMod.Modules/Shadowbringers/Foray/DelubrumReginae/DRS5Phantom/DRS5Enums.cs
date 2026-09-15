namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

public enum OID : uint
{
    Boss = 0x30AE, // R2.400, x1
    BloodyWraith = 0x30B0, // R2.000, spawn during fight
    MistyWraith = 0x30B1, // R2.000, spawn during fight
    Helper = 0x233C, // R0.500, x18
    MiasmaLowRect = 0x1EB0DD, // R0.500, EventObj type, spawn during fight
    MiasmaLowCircle = 0x1EB0DE, // R0.500, EventObj type, spawn during fight
    MiasmaLowDonut = 0x1EB0DF, // R0.500, EventObj type, spawn during fight
    MiasmaHighRect = 0x1EB0E0, // R0.500, EventObj type, spawn during fight
    MiasmaHighCircle = 0x1EB0E1, // R0.500, EventObj type, spawn during fight
    MiasmaHighDonut = 0x1EB0E2, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    MaledictionOfAgony = 22461, // Boss->self, 4.0s cast, single-target, visual (raidwide)
    MaledictionOfAgonyAOE = 23350, // Helper->self, no cast, range 40 circle raidwide
    WeaveMiasma = 22450, // Boss->self, 3.0s cast, single-target, visual (create miasma markers)
    ManipulateMiasma = 22451, // Boss->self, 9.0s cast, single-target, visual (low -> high)
    InvertMiasma = 23022, // Boss->self, 9.0s cast, single-target, visual (high -> low)
    CreepingMiasmaFirst = 22452, // Helper->self, 10.0s cast, range 50 width 12 rect
    CreepingMiasmaRest = 22453, // Helper->self, 1.0s cast, range 50 width 12 rect
    LingeringMiasmaFirst = 22454, // Helper->location, 10.0s cast, range 8 circle
    LingeringMiasmaRest = 22455, // Helper->location, 1.0s cast, range 8 circle
    SwirlingMiasmaFirst = 22456, // Helper->location, 10.0s cast, range 5-19 donut
    SwirlingMiasmaRest = 22457, // Helper->location, 1.0s cast, range 5-19 donut
    Transference = 22445, // Boss->location, no cast, single-target, teleport
    Summon = 22464, // Boss->self, 3.0s cast, single-target, visual (go untargetable and spawn adds)
    MaledictionOfRuin = 22465, // Boss->self, 43.0s cast, single-target
}
