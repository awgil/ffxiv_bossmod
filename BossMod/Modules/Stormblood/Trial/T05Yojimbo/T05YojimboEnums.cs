namespace BossMod.Stormblood.Trial.T05Yojimbo;

public enum OID : uint
{
    Boss = 0x25FD, // R1.800-2.000, x1
    Helper = 0x233C, // R0.500, x18, mixed types
    Embodiment = 0x25FF, // R2.000, x2
    Gilgamesh = 0x2600, // R0.000, x1
    Inoshikacho = 0x25FE, // R0.600, x0 (spawn during fight)
    DragonsHead = 0x2601, // R3.000, x0 (spawn during fight)
    ElectrogeneticForce = 0x2603, // R1.000, x0 (spawn during fight)
    IronChain = 0x2602, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Embodiment->player, no cast, single-target

    AmeNoMurakumo = 14397, // Gilgamesh->self, 4.0s cast, range 100 circle

    BitterEnd1 = 12766, // Boss->self, no cast, range 8+R ?-degree cone
    BitterEnd2 = 14382, // Boss/Embodiment->self, no cast, range 8+R ?-degree cone

    Bunshin = 14394, // Boss->self, no cast, single-target
    DragonNight = 14392, // Helper->self, no cast, range 40+R circle
    DragonsLair = 14390, // Boss->self, no cast, single-target

    ElectrogeneticForce1 = 14399, // Boss->self, no cast, single-target
    ElectrogeneticForce2 = 14400, // ElectrogeneticForce->self, 2.0s cast, range 8 circle

    Enchain = 14401, // Boss->self, 5.0s cast, ???
    EpicStormsplitter = 14405, // Boss->self, 5.0s cast, range 40+R width 4 rect
    Fragility = 14378, // Inoshikacho->self, 3.0s cast, range 8 circle

    Gekko1 = 14384, // Boss/Embodiment->self, no cast, single-target
    Gekko2 = 14387, // Helper->players, no cast, range 5 circle

    GigaJump1 = 14396, // Embodiment->players, no cast, range 50 circle
    GigaJump2 = 14398, // Boss->players, no cast, range 50 circle

    HellsGate = 14402, // Boss->location, 20.0s cast, single-target
    Inoshikacho = 14377, // Boss->self, 3.0s cast, single-target

    Kasha1 = 14385, // Boss/Embodiment->self, no cast, single-target
    Kasha2 = 14388, // Helper->location, 4.0s cast, range ?-10 donut

    Masamune = 14403, // Boss->location, 2.5s cast, width 8 rect charge
    MettaGiri = 14376, // Boss->self, 3.0s cast, range 80 circle
    TinySong = 14389, // Boss->self, 5.0s cast, ???
    Unknown = 14410, // Helper->self, no cast, ???
    UnknownAbility = 14395, // Embodiment->self, no cast, single-target
    UnknownWeaponskill = 14379, // Boss->location, no cast, single-target

    Unveiling1 = 14380, // Boss->self, 3.0s cast, single-target
    Unveiling2 = 14381, // Boss->self, no cast, single-target

    Wakizashi = 14375, // Boss->player, no cast, single-target

    Yukikaze1 = 14383, // Boss->self, no cast, single-target
    Yukikaze2 = 14386, // Helper->self, 3.0s cast, range 44+R width 4 rect

    ZanmaZanmai = 14404, // Boss->self, 3.0s cast, range 80 circle

    Seasplitter1 = 14409, // Helper->self, 5.0s cast, range 21+R width 40 rect
    Seasplitter2 = 14406, // Helper->self, no cast, range 7+R width 40 rect
    Seasplitter3 = 14407, // Helper->self, no cast, range 7+R width 40 rect
    Seasplitter4 = 14408, // Helper->self, no cast, range 7+R width 40 rect
    MightyBlow = 14393, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    Unveiled = 1620, // none->Boss, extra=0xC4
    Fetters = 668, // Boss->player, extra=0xEC4
}

public enum IconID : uint
{
    Icon144 = 144, // player
    DoritoStack = 55, // player
    Icon87 = 87, // player
    Icon5 = 5, // player
}

public enum TetherID : uint
{
    Tether57 = 57, // player->Boss
    Tether1 = 1, // player->Boss
    Tether81 = 81, // player->Boss
}
