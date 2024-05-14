namespace BossMod.Stormblood.Trial.T03Shinryu;

public enum OID : uint
{
    Boss = 0x1983, // R22.000, x1

    Actor1e950d = 0x1E950D, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x18D6, // R0.500, x26, mixed types
    Cocoon = 0x1B13, // R3.000, x0 (spawn during fight)
    EyeOfTheStorm = 0x1B17, // R1.000, x0 (spawn during fight)
    Ginryu = 0x1B14, // R1.800, x0 (spawn during fight)
    Hakkinryu = 0x1C83, // R3.600, x0 (spawn during fight)
    Icicle = 0x1B16, // R2.500, x0 (spawn during fight)
    LeftWing = 0x1B19, // R10.000-15.000, x1, Part type
    MassiveCocoon = 0x1C86, // R6.000, x0 (spawn during fight)
    RightWing = 0x1B1A, // R10.000-15.000, x1, Part type
    Tail = 0x1B12, // R17.940, x0 (spawn during fight), 523 type
    Unknown = 0x1B15, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 8105, // LeftWing/RightWing->player, no cast, single-target
    MobAutoAttack = 870, // Ginryu/Hakkinryu->player, no cast, single-target

    AerialBlast1 = 8080, // Helper->self, 10.0s cast, range 60 circle
    AerialBlast2 = 8111, // Boss->self, 10.0s cast, single-target

    BlazingTrail = 8730, // Ginryu->self, 3.0s cast, range 15+R width 11 rect
    BurningChains = 8144, // Helper->self, no cast, ???
    Collapse = 8728, // Hakkinryu->self, no cast, range 8+R ?-degree cone
    DarkMatter = 8088, // Boss->self, 3.0s cast, range 60 circle
    DeathSentence = 8731, // Hakkinryu->player, 4.0s cast, single-target

    Dragonfist1 = 9455, // Boss->self, no cast, single-target
    Dragonfist2 = 9456, // Helper->self, 4.0s cast, range 16 circle

    GyreCharge1 = 8104, // Boss->self, no cast, range 100+R width 60 rect
    GyreCharge2 = 8180, // Helper->self, 6.3s cast, range 100+R width 60 rect

    IceStorm1 = 8098, // LeftWing->self, 6.0s cast, single-target
    IceStorm2 = 8099, // Helper->self, no cast, range 60 circle

    IcicleImpact = 8096, // Icicle->self, no cast, range 6 circle

    JudgmentBolt1 = 8077, // Helper->self, 10.0s cast, range 60 circle
    JudgmentBolt2 = 8108, // Boss->self, 10.0s cast, single-target

    Levinbolt1 = 8091, // RightWing->self, 6.0s cast, single-target
    Levinbolt2 = 8092, // Helper->player, no cast, range 5 circle

    Hellfire1 = 8076, // Helper->self, 10.0s cast, range 60 circle
    Hellfire2 = 8107, // Boss->self, 10.0s cast, single-target

    DiamondDust1 = 8078, // Helper->self, 10.0s cast, range 60 circle
    DiamondDust2 = 8109, // Boss->self, 10.0s cast, single-target

    MeteorImpact1 = 8086, // Cocoon/MassiveCocoon->self, 4.0s cast, range 60 circle
    MeteorImpact2 = 9291, // Helper->self, 5.0s cast, range 60 circle

    Protostar1 = 8085, // Boss->self, 6.0s cast, range 80 circle
    Protostar2 = 8123, // Helper->self, no cast, range 50 circle

    Spikesicle = 8097, // Icicle->self, 2.5s cast, range 62+R width 10 rect
    SummonIcicle = 8095, // LeftWing->self, 4.0s cast, single-target
    SuperCyclone = 8984, // Helper->self, 0.5s cast, range 90 circle
    TailSlap = 8083, // Tail->self, 3.0s cast, range 40 width 20 rect

    TidalWave1 = 8075, // Helper->self, 10.0s cast, range 80+R width 60 rect
    TidalWave2 = 8106, // Boss->self, 10.0s cast, single-target

    UnknownAbility1 = 8074, // Boss->self, no cast, single-target
    UnknownAbility2 = 8081, // Boss->self, no cast, single-target
    UnknownAbility3 = 8082, // Boss->self, no cast, single-target
    UnknownAbility4 = 8084, // Tail->self, no cast, single-target
    UnknownAbility5 = 8142, // Boss->self, no cast, single-target
    UnknownAbility6 = 8143, // Boss->self, no cast, single-target
    UnknownAbility7 = 8488, // Boss->self, no cast, single-target
    UnknownAbility8 = 8514, // Cocoon/MassiveCocoon->self, no cast, single-target
}

public enum SID : uint
{
    FireResistanceUp = 520, // none->player, extra=0x0
    LightningResistanceDownII = 1260, // none->player, extra=0x0
    Paralysis = 17, // Helper->player, extra=0x0
    Fetters = 667, // Boss->player, extra=0xEC4
    Affixed = 1267, // Boss->player, extra=0xE/0xC/0x14/0x18/0x12/0xD/0x13/0xF
    BurningChains = 769, // none->player, extra=0x0
    VulnerabilityUp = 202, // Helper/Icicle->player, extra=0x1
    DownForTheCount = 783, // Helper->player, extra=0xEC7
}

public enum IconID : uint
{
    Icon_24 = 24, // player
    Icon_97 = 97, // player
}

public enum TetherID : uint
{
    Tether_9 = 9, // player->player
}
