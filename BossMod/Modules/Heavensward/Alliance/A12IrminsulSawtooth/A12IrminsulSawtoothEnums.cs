namespace BossMod.Heavensward.Alliance.A12IrminsulSawtooth;

public enum OID : uint
{
    Irminsul = 0x13F1, // R9.000, x?
    Sawtooth = 0x13EF, // R6.000, x?
    SawtoothHelper = 0x1B2, // R0.500, x?, mixed types
    SawtoothHelper2 = 0x13F0, // R7.500, x?, 523 type
    ArkKed = 0x13F2, // R1.500, x?
}

public enum AID : uint
{
    Attack1 = 5210, // Irminsul->player, no cast, single-target
    Attack2 = 5207, // Sawtooth->players, no cast, range 6+R ?-degree cone
    Attack3 = 878, // ArkKed->player, no cast, single-target

    Ambush = 5172, // SawtoothHelper2->self, 3.5s cast, range 9 circle
    MeanThrash = 5209, // Sawtooth->self, 2.5s cast, range 6+R 120-degree cone
    MucusBomb = 5205, // SawtoothHelper->players, no cast, ???
    MucusSpray1 = 5206, // Sawtooth->self, 3.0s cast, single-target
    MucusSpray2 = 5472, // SawtoothHelper->self, no cast, range 20 circle
    PulseOfTheVoid = 5211, // Irminsul->self, no cast, range 18 circle
    Rootstorm = 5212, // Irminsul->self, 3.0s cast, range 100 circle
    ShockwaveStomp = 5213, // Sawtooth->self, 15.0s cast, ???
    Thunderstrike = 5244, // ArkKed->self, no cast, range 10+R width 3 rect
    Unknown1 = 5332, // Sawtooth->self, no cast, single-target
    Unknown2 = 5173, // Sawtooth->Irminsul, no cast, single-target
    Unknown3 = 5339, // Sawtooth->self, no cast, single-target
    Unknown4 = 5368, // SawtoothHelper->self, no cast, single-target
    VoidWall = 5203, // SawtoothHelper->Sawtooth, no cast, single-target
    VoidWard = 5204, // SawtoothHelper->Irminsul, no cast, single-target
    WhiteBreath = 5208, // Sawtooth->self, 3.5s cast, range 22+R 120-degree cone
}

public enum SID : uint
{
    Invincibility1 = 1570, // none->player, extra=0x0
    RangedResistance = 941, // none->SawtoothHelper/Sawtooth/SawtoothHelper2, extra=0x0
    MagicResistance = 942, // none->Irminsul, extra=0x0
    Seized = 961, // SawtoothHelper->player, extra=0x0
    TheHeatOfBattle = 365, // none->player, extra=0xA
    ReducedRates = 364, // none->player, extra=0x1E
    Invincibility2 = 325, // Irminsul->Irminsul, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Devoured = 421, // SawtoothHelper2->player, extra=0x0
    DamageUp = 443, // Irminsul->SawtoothHelper/Sawtooth/SawtoothHelper2, extra=0x1
    Poison = 18, // Irminsul->player, extra=0x0
    UnwillingHost = 937, // none->player, extra=0xA
}

public enum IconID : uint
{
    Icon_60 = 60, // player
}

public enum TetherID : uint
{
    Tether_44 = 44, // player->player
}