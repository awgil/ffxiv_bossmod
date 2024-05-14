namespace BossMod.Heavensward.Alliance.A32FerdiadHollow;

public enum OID : uint
{
    Boss = 0x1928, // R3.400, x?
    FerdiadHollow = 0x192E, // R0.500, x?
    FerdiadsFool = 0x192D, // R0.750, x?
    AbyssalScythe = 0x194E, // R1.000, x?
    Aether = 0x192B, // R2.500, x?
    WailingAtomos = 0x1929, // R3.000, x?
    CursingAtomos = 0x192A, // R3.000, x?
    AetherialChakram = 0x192C, // R2.500, x?
    Unknown = 0x19AE, // R5.000, x?
}

public enum AID : uint
{
    AutoAttack = 7319, // Boss->player, no cast, single-target

    Blackbolt = 7341, // Boss->players, 5.0s cast, range 6 circle

    Blackfire1 = 7338, // Boss->self, 8.2s cast, single-target
    Blackfire2 = 7339, // FerdiadHollow->location, 9.0s cast, range 7 circle

    BlackWind = 7340, // Boss->players, no cast, range 5 circle
    DeathScythe = 7345, // AbyssalScythe->self, no cast, range 4 circle
    Debilitator = 7334, // Boss->self, 3.0s cast, single-target

    Explosion = 7329, // Aether->self, 1.0s cast, range 20 circle
    Explosion2 = 7330, // AetherialChakram->self, 1.0s cast, range 75 circle

    Fire = 7212, // FerdiadsFool->player, no cast, single-target

    Flameflow1 = 7335, // Boss->location, 3.0s cast, range 30 circle
    Flameflow2 = 7336, // FerdiadHollow->self, no cast, range 60 circle
    Flameflow3 = 7337, // FerdiadHollow->self, no cast, range 60 circle

    Icefall = 7344, // FerdiadsFool->location, 3.0s cast, range 5 circle

    JestersJig1 = 7332, // Boss->self, 5.0s cast, range 30 circle
    JestersJig2 = 7333, // FerdiadHollow->self, no cast, range 30 circle

    JestersReap = 7745, // Boss->self, 3.0s cast, range 10+R 120-degree cone
    JestersReward = 7331, // Boss->self, 8.0s cast, range 28+R 180-degree cone

    JongleursX = 7320, // Boss->player, 5.0s cast, single-target
    JugglingSphere = 7327, // Aether->WailingAtomos/CursingAtomos, 7.0s cast, width 6 rect charge
    JugglingSphere2 = 7328, // AetherialChakram->WailingAtomos/CursingAtomos, 7.0s cast, width 6 rect charge

    LuckyPierrot1 = 7342, // FerdiadsFool->location, no cast, width 6 rect charge
    LuckyPierrot2 = 7343, // FerdiadsFool->WailingAtomos, 5.0s cast, width 6 rect charge

    PetrifyingEye = 7779, // FerdiadsFool->self, 5.0s cast, range 40 circle

    Sleight = 7321, // Boss->location, no cast, ???

    AtmosAOE1 = 7241, // WailingAtomos/CursingAtomos->self, 6.8s cast, single-target // circle?
    AtmosAOE2 = 7324, // WailingAtomos/CursingAtomos->self, 8.5s cast, single-target // circle?
    AtmosDonut = 7325, // WailingAtomos/CursingAtomos->self, 8.5s cast, single-target // donut?

    Unknown4 = 7326, // WailingAtomos->location, 3.0s cast, width 6 rect charge
    Unknown5 = 7323, // WailingAtomos->self, 4.8s cast, single-target
    Unknown6 = 7766, // WailingAtomos->location, 6.5s cast, width 6 rect charge

    Wormhole = 7322, // Boss->self, 6.5s cast, single-target
}

public enum SID : uint
{
    FleshWound = 264, // AbyssalScythe->player, extra=0x0
    VulnerabilityUp = 202, // Aether/AetherialChakram/FerdiadsFool/FerdiadHollow->player, extra=0x1/0x2/0x3
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    FireResistanceDownII = 1137, // Boss->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    Petrification = 610, // FerdiadsFool->player, extra=0x0
    AreaOfInfluenceUp = 618, // none->FerdiadHollow/Boss, extra=0x1/0x2/0x3/0x4/0x5/0x6
    WaterResistanceDownII = 1157, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Icon_23 = 23, // player
    Icon_62 = 62, // player
}

public enum TetherID : uint
{
    Tether_1 = 1, // Aether/AetherialChakram->WailingAtomos/CursingAtomos
}