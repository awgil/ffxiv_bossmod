namespace BossMod.Endwalker.Quest.LifeEphemeralPathEternal;

public enum OID : uint
{
    Boss = 0x35C5,
    BossP2 = 0x35C6,
    Helper = 0x233C,
    MahaudFlamehand = 0x35C4, // R0.500, x1
    Lalah = 0x35C2,
    Loifa = 0x35C3,
    Mahaud = 0x361C,
    Ancel = 0x361D,
    EnhancedNoulith = 0x3859, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    ChiBlast = 26838, // Boss->self, 5.0s cast, single-target
    ChiBlast1 = 26839, // Helper->self, 5.0s cast, range 100 circle
    ChiBomb = 26835, // Boss->self, 5.0s cast, single-target
    Explosion = 26837, // 35C7->self, 5.0s cast, range 6 circle
    ArmOfTheScholar = 26836, // Boss->self, 5.0s cast, range 5 circle
    RawRockbreaker = 26832, // Boss->self, 5.0s cast, single-target
    RawRockbreaker1 = 26833, // Helper->self, 4.0s cast, range 10 circle
    RawRockbreaker2 = 26834, // Helper->self, 4.0s cast, range 10-20 donut
    DemifireII = 26842, // MahaudFlamehand->Lalah, 8.0s cast, single-target
    Demiburst = 26843, // MahaudFlamehand->self, 7.0s cast, single-target
    ElectrogeneticForce = 26844, // Helper->self, 8.0s cast, range 6 circle
    ElectrogeneticBlast = 26845, // Helper->self, 1.0s cast, range 80 circle
    DemifireIII = 26841, // MahaudFlamehand->Lalah, 3.0s cast, single-target
    FourElements = 26846, // MahaudFlamehand->self, 8.0s cast, single-target
    ClassicalFire = 26847, // Helper->Lalah, 8.0s cast, range 6 circle
    ClassicalThunder = 26848, // Helper->player/Loifa/Lalah, 5.0s cast, range 6 circle
    ClassicalBlizzard = 26849, // Helper->location, 5.0s cast, range 6 circle
    ClassicalStone = 26850, // Helper->self, 9.0s cast, range 50 circle

    Nouliths = 26851, // BossP2->self, 5.0s cast, single-target
    AetherstreamTank = 26852, // 35C8->Lalah, no cast, range 50 width 4 rect
    AetherstreamPlayer = 26853, // 35C8->players/Loifa, no cast, range 50 width 4 rect
    Tracheostomy = 26854, // BossP2->self, 5.0s cast, range 10-20 donut
    RightScalpel = 26855, // BossP2->self, 5.0s cast, range 15 210-degree cone
    LeftScalpel = 26856, // BossP2->self, 5.0s cast, range 15 210-degree cone
    Laparotomy = 26857, // BossP2->self, 5.0s cast, range 15 120-degree cone
    Amputation = 26858, // BossP2->self, 7.0s cast, range 20 120-degree cone
    Hypothermia = 26861, // BossP2->self, 5.0s cast, range 50 circle
    Cryonics = 26860, // Helper->player, 8.0s cast, range 6 circle
    Cryonics1 = 26859, // BossP2->self, 8.0s cast, single-target
    Craniotomy = 28386, // BossP2->self, 8.0s cast, range 40 circle
    RightLeftScalpel = 26862, // BossP2->self, 7.0s cast, range 15 210-degree cone
    RightLeftScalpel1 = 26863, // BossP2->self, 3.0s cast, range 15 210-degree cone
    LeftRightScalpel = 26864, // BossP2->self, 7.0s cast, range 15 210-degree cone
    LeftRightScalpel1 = 26865, // BossP2->self, 3.0s cast, range 15 210-degree cone
    Frigotherapy = 26866, // BossP2->self, 5.0s cast, single-target
    Frigotherapy1 = 26867, // Helper->players/Mahaud/Loifa, 7.0s cast, range 5 circle
}

public enum IconID : uint
{
    Tankbuster = 230, // Lalah
    Noulith = 244, // player/Loifa
}

public enum TetherID : uint
{
    Noulith = 17, // StrengthenedNoulith->Lalah/player/Loifa
    Craniotomy = 174, // EnhancedNoulith->Lalah/Loifa/player/Mahaud/Ancel
}

public enum SID : uint
{
    Craniotomy = 2968, // none->player/Lalah/Mahaud/Ancel/Loifa, extra=0x0
    DownForTheCount = 1953, // none->player/Lalah/Mahaud/Ancel/Loifa, extra=0xEC7

}
