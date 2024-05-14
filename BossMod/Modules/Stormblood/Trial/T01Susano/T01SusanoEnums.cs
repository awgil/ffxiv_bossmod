namespace BossMod.Stormblood.Trial.T01Susano;

public enum OID : uint
{
    Boss = 0x1AF7, // R3.500, x1
    Helper = 0x1BA1, // R0.500, x20, mixed types
    Susano = 0x1AF8, // R0.000, x1
    DarkCloud = 0x1F53, // R3.000, x0 (spawn during fight)
    Actor1ba2 = 0x1BA2, // R1.000, x0 (spawn during fight)
    AmeNoMurakumo = 0x1C84, // R8.000, x0 (spawn during fight), Part type
    DarkLevin = 0x1B9B, // R1.000, x0 (spawn during fight)
    AmaNoIwato = 0x1C20, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    AmeNoMurakumo1 = 8226, // Susano->self, no cast, range 40+R circle
    AmeNoMurakumo2 = 8588, // Helper->self, 4.0s cast, range 40+R width 6 rect
    AmeNoMurakumo3 = 9506, // AmeNoMurakumo->self, 24.0s cast, single-target

    Assail = 8220, // Boss->player, no cast, single-target

    Brightstorm = 8224, // Boss->players, no cast, range 6 circle

    RasenKaikyo1 = 8221, // Boss->self, 3.0s cast, single-target
    RasenKaikyo2 = 8222, // Helper->self, 3.0s cast, range 6 circle

    Seasplitter1 = 8232, // Helper->self, 3.0s cast, range 21+R width 40 rect
    Seasplitter2 = 8233, // Helper->self, no cast, range 7+R width 40 rect
    Seasplitter3 = 8234, // Helper->self, no cast, range 7+R width 40 rect
    Seasplitter4 = 8235, // Helper->self, no cast, range 7+R width 40 rect
    Seasplitter5 = 9661, // Helper->self, 2.9s cast, single-target

    SheerForce = 8225, // Helper->self, no cast, range 40+R circle
    Stormsplitter = 8227, // Boss->self/player, 5.0s cast, range 20+R width 4 rect
    TheAlteredGate = 8333, // Helper->AmaNoIwato, no cast, ???
    TheHiddenGate = 8228, // Boss->self, no cast, single-target
    ThePartingClouds = 9631, // DarkCloud->self, 3.5s cast, range 50+R width 10 rect
    TheSealedGate = 8229, // AmaNoIwato->self, 15.0s cast, single-target

    Ukehi1 = 8230, // Boss->self, 4.0s cast, range 40+R circle
    Ukehi2 = 8231, // Boss->self, no cast, range 40+R circle

    UnknownWeaponskill = 8646, // Susano->self, no cast, single-target
    YasakaniNoMagatama = 9633, // Boss->self, no cast, single-target
    YataNoKagami = 8223, // Boss->player, no cast, single-target
}

public enum SID : uint
{
    LightningResistanceDown = 898, // Helper->player, extra=0x1/0x2
    Clashing = 1271, // Boss->player, extra=0x17E3
    FleshWound = 624, // Boss->player, extra=0x0
    Fetters = 292, // Boss->player, extra=0x0
    Paralysis = 216, // DarkCloud->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_23 = 23, // player
    Icon_62 = 62, // player
    Icon_230 = 230, // player
    Icon_112 = 112, // AmaNoIwato
}

public enum TetherID : uint
{
    Tether_66 = 66, // AmaNoIwato->Boss
}