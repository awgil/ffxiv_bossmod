namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

public enum OID : uint
{
    Boss = 0x3250, // R0.500-5.100, x1
    BossP2 = 0x3252, // R4.500, x0 (spawn during fight)

    Helper = 0x233C, // R0.500, x10, 523 type
    IgnisEst = 0x3253, // R1.000, x2 (spawn during fight)
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Huma = 0x3251, // R1.120, x0 (spawn during fight)
    Actor1eb214 = 0x1EB214, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb213 = 0x1EB213, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Boss2AutoAttack = 25022, // Boss->player, no cast, single-target

    Burn = 24191, // BossP2->self, no cast, range 60 circle
    Immolate = 24192, // BossP2->self, no cast, range 60 circle
    BurningBlade = 24208, // Boss->player, 5.0s cast, single-target
    DoubleCast = 24205, // Boss->self, 4.0s cast, single-target
    Flamedive = 24179, // Huma->self, 4.0s cast, range 55 width 5 rect

    LeftBrand = 24204, // Boss->self, 5.0s cast, range 40 180-degree cone
    RightBrand = 24203, // Boss->self, 5.0s cast, range 40 180-degree cone

    MannatheihwonFlame1 = 24199, // Boss->self, 4.0s cast, single-target
    MannatheihwonFlame2 = 24200, // Helper->self, 5.0s cast, range 60 circle
    MannatheihwonFlame3 = 24201, // IgnisEst->self, 5.0s cast, range 50 width 8 rect
    MannatheihwonFlame4 = 24202, // Helper->self, 5.0s cast, range 10 circle

    Phenex1 = 24197, // Boss->self, 4.0s cast, single-target
    Phenex2 = 24178, // Boss->self, 4.0s cast, single-target

    Pyroclysm = 24184, // Helper->location, no cast, range 40 circle
    Pyrocrisis = 24207, // Helper->player, 8.0s cast, range 6 circle
    Pyrodoxy = 24206, // Helper->player, 8.0s cast, range 6 circle

    Pyrokinesis = 24188, // Boss->self, 5.0s cast, single-target
    PyrokinesisAOE = 24189, // Helper->self, 5.0s cast, range 60 circle
    PyrokinesisVisual = 24194, // Boss->self, 4.0s cast, single-target

    Hyperpyroplexy1 = 24198, // Boss->self, 4.0s cast, single-target
    Hyperpyroplexy2 = 24182, // Boss->self, 4.0s cast, single-target
    Pyroplexy = 24183, // Helper->location, no cast, range 4 circle

    ReverseTimeEruption = 24173, // Boss->self, 7.0s cast, single-target
    ReverseTimeEruptionAOESecond = 24176, // Helper->self, 7.0s cast, range 20 width 20 rect
    ReverseTimeEruptionAOEFirst = 24177, // Helper->self, 5.0s cast, range 20 width 20 rect
    ReverseTimeEruptionVisual = 24196, // Boss->self, 4.0s cast, single-target

    TimeEruptionVisual = 24172, // Boss->self, 7.0s cast, single-target
    TimeEruptionAOEFirst = 24174, // Helper->self, 5.0s cast, range 20 width 20 rect
    TimeEruptionAOESecond = 24175, // Helper->self, 7.0s cast, range 20 width 20 rect

    GrandCrossflame = 24186, // Boss->self, 5.0s cast, single-target
    GrandCrossflameAOE = 24187, // Helper->self, 5.0s cast, range 40 width 18 cross
    GrandSword = 24579, // 32E2->self, no cast, range 27 ?-degree cone

    ThermalGust = 24180, // Boss->self, 4.0s cast, single-target
    ThermalGustAOE = 24181, // Helper->self, 4.0s cast, range 44 width 10 rect

    UnknownWeaponskill1 = 24185, // Boss->location, no cast, single-target
    UnknownWeaponskill2 = 24190, // Boss->self, no cast, single-target
    UnknownWeaponskill3 = 24193, // Boss->self, no cast, single-target

}

public enum SID : uint
{
    Bleeding = 642, // none->player, extra=0x0
    FeyIllumination = 317, // player->player, extra=0x0
    LeftUnseen = 1708, // none->player, extra=0xEA
    MagicVulnerabilityUp1 = 1138, // Helper->player, extra=0x0
    MagicVulnerabilityUp2 = 1138, // Helper->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Transfiguration = 2548, // none->Boss, extra=0x1A5
    Unknown1 = 2056, // none->Huma, extra=0x132
    Unknown2 = 2056, // none->Huma, extra=0x132
    VulnerabilityUp1 = 1789, // Helper/player->player, extra=0x4/0x3/0x1/0x2
    VulnerabilityUp2 = 1789, // Huma/player/Helper/Boss->player, extra=0x1/0x2/0x3
    Weakness = 43, // none->player, extra=0x0
    WhisperingDawn = 315, // player->player, extra=0x0
    RightUnseen = 1707, // none->player, extra=0xE9
}

public enum IconID : uint
{
    Icon_100 = 100, // player
    Icon_101 = 101, // player
    Icon_288 = 288, // player
}
