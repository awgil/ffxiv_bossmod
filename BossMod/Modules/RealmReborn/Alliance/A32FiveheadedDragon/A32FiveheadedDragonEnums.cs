namespace BossMod.RealmReborn.Alliance.A32FiveheadedDragon;

public enum OID : uint
{
    Boss = 0xDFC, // R8.000, x?
    FiveHeadedDragonHelper = 0x1B2, // R0.500, x?, 523 type
    HeadOfFire = 0xE13, // R8.000, x?, Part type
    HeadOfPoison = 0xE16, // R8.000, x?, Part type
    HeadOfThunder = 0xE15, // R8.000, x?, Part type
    HeadOfIce = 0xE14, // R8.000, x?, Part type
    Prominence = 0xDFD, // R1.000, x?
    PoisonSlime = 0xDFF, // R1.000, x?
    ToxicSlime = 0xE17, // R3.000, x?
    DragonfireFly = 0xDFE, // R0.800, x?
}

public enum AID : uint
{
    AutoAttack1 = 3355, // Boss->players, no cast, range 6+R ?-degree cone
    WhiteBreath = 3295, // Boss->self, 3.5s cast, range 22+R 120-degree cone
    BreathOfFire = 3285, // FiveHeadedDragonHelper->location, 3.0s cast, range 6 circle
    Fireball = 3290, // DragonfireFly->player, no cast, single-target
    BreathOfThunder = 3287, // FiveHeadedDragonHelper->players, no cast, range 6 circle
    Discordance = 3282, // Boss->self, 5.0s cast, range 60 circle
    BreathOfLight = 3284, // FiveHeadedDragonHelper->location, 3.0s cast, range 6 circle
    BreathOfPoison = 3288, // FiveHeadedDragonHelper->location, 3.0s cast, range 6 circle
    AutoAttack2 = 872, // PoisonSlime/ToxicSlime->player, no cast, single-target
    Seep = 3291, // PoisonSlime->self, no cast, range 10 circle
    Digest = 724, // ToxicSlime->player, no cast, single-target
    BreathOfIce = 3286, // FiveHeadedDragonHelper->location, 3.0s cast, range 6 circle
    TheLastSong = 2376, // ToxicSlime->self, no cast, range 60 circle
    Fermata = 3283, // Boss->location, no cast, range 20 circle
    Radiance = 3289, // Prominence->self, 10.0s cast, range 60 circle
    FluidSpread = 725, // ToxicSlime->player, no cast, single-target
    HeatWave = 3294, // Boss->self, 1.5s cast, range 60 circle
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    Infirmity = 172, // Boss->player, extra=0x0
    Prey1 = 420, // none->player, extra=0x0
    Prey2 = 664, // none->player, extra=0x0
    SlipperyPrey1 = 475, // none->player, extra=0x0
    SlipperyPrey2 = 665, // none->player, extra=0x0
    Suppuration = 375, // DragonfireFly->player, extra=0x1/0x2
    Haste = 480, // none->FiveHeadedDragonHelper/Boss, extra=0x1/0x2/0x3/0x4
    VulnerabilityUp = 202, // FiveHeadedDragonHelper/Boss->player, extra=0x1
    Pollen = 19, // none->player, extra=0x0
    PoisonResistanceUp = 640, // PoisonSlime->player, extra=0x0
    Silence = 7, // ToxicSlime->player, extra=0x0
    DeepFreeze = 487, // none->player, extra=0x1
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Heavy = 14, // FiveHeadedDragonHelper->player, extra=0x32
    Poison = 18, // ToxicSlime/FiveHeadedDragonHelper->player, extra=0x0
    MeatAndMead = 360, // none->player, extra=0xA
    Pyretic = 639, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Icon318 = 318, // player
    Icon46 = 46, // player
}

public enum TetherID : uint
{
    Tether5 = 5, // DragonfireFly->player
}
