namespace BossMod.RealmReborn.Alliance.A11BoneDragon;
public enum OID : uint
{
    Boss = 0x92B, // R5.000, x?
    BoneDragonHelper = 0x92C, // R0.500, x?
    Platinal = 0x92D, // R1.000, x?
    RottingEye = 0x982, // R1.800, x?
}

public enum AID : uint
{
    AutoAttack = 1461, // Boss/Platinal->player, no cast, single-target
    Apocalypse = 749, // BoneDragonHelper->location, 3.0s cast, range 6 circle
    DarkThorn = 745, // Boss->location, no cast, range 6 circle
    DarkWave = 736, // Boss->self, no cast, range 6+R circle
    EvilEye = 750, // Boss->self, 3.0s cast, range 100+R 120-degree cone
    HellSlash = 341, // Platinal->player, no cast, single-target
    MiasmaBreath = 735, // Boss->self, no cast, range 8+R ?-degree cone
    SoulDouse = 746, // Platinal->self, no cast, ???
    Stone = 970, // RottingEye->player, 1.0s cast, single-target
    ChaosBreath = 751, // Boss->self, no cast, range 12+R ?-degree cone
    Level5Petrify = 1828, // RottingEye->self, 2.0s cast, range 6+R 90-degree cone
}

public enum SID : uint
{
    Disease = 181, // Boss/BoneDragonHelper->player, extra=0x28
    Leaden = 67, // none->Platinal, extra=0x50
}
public enum IconID : uint
{
    Icon_5 = 5, // player
}

public enum TetherID : uint
{
    Tether_1 = 1, // Platinal->Platinal
}
