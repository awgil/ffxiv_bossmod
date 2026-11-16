namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

public enum OID : uint
{
    Boss = 0x4DDE, // R22.000, x1
    Tail = 0x4DDF, // R17.940, x0 (spawn during fight)
    Cocoon = 0x4DE0, // R3.000, x0 (spawn during fight)
    Ginryu = 0x4DE1, // R1.800, x0 (spawn during fight)
    Icicle = 0x4DE2, // R2.500, x0 (spawn during fight)
    EyeOfTheStorm = 0x4DE3, // R1.000, x0 (spawn during fight)
    WormsHeart = 0x4DE4, // R29.000, x0 (spawn during fight), Part type
    LeftWing = 0x4DE5, // R15.000, x1, Part type
    RightWing = 0x4DE6, // R15.000, x1, Part type
    Hakkinryu = 0x4DE7, // R3.600, x0 (spawn during fight)
    MassiveCocoon = 0x4DE8, // R6.000, x0 (spawn during fight)
    Reiryu = 0x4DE9, // R2.000, x0 (spawn during fight)
    Helper = 0x4DEA, // R0.500, x12, Helper type
    HelperLower = 0x4DEB, // R0.500, x0 (spawn during fight), Helper type
    LeftWingP3 = 0x4DEC, // R11.200, x0 (spawn during fight), Part type
    RightWingP3 = 0x4DED, // R11.200, x0 (spawn during fight), Part type
    CocoonMeteor = 0x1FA7, // R0.500, x0 (spawn during fight)
    ReiryuHelper = 0x1FA8, // R0.500, x0 (spawn during fight)
    Platform = 0x1EA1A1, // R2.000, x10, EventObj type
    WaterSpout = 0x1E8536, // R2.000, x1, EventObj type
    WaterPuddle = 0x1E950D, // R0.500, x0 (spawn during fight), EventObj type
    DoomPuddle = 0x1EA7E9, // R0.500, x0 (spawn during fight), EventObj type
    JumpPadWest = 0x1EA7F1, // EventObj type
    JumpPadEast = 0x1EA7D9, // EventObj type
}

public enum AID : uint
{
    AutoAttack = 50260, // LeftWing/RightWing/LeftWingP3/RightWingP3->player, no cast, single-target

    EarthenFuryVisual = 50265, // Boss->self, 10.0s cast, single-target
    EarthenFury = 50234, // Helper->self, 10.0s cast, range 60 circle
    EarthenFuryTile = 50282, // Helper->self, 10.0s cast, range 20 width 20 rect
    TidalWaveVisual = 50261, // Boss->self, 10.0s cast, single-target
    TidalWave = 50230, // Helper->self, 10.0s cast, range 80 width 60 rect
    HellfireVisual = 50262, // Boss->self, 10.0s cast, single-target
    Hellfire = 50231, // Helper->self, 10.0s cast, range 60 circle
    JudgmentBoltVisual = 50263, // Boss->self, 10.0s cast, single-target
    JudgmentBolt = 50232, // Helper->self, 10.0s cast, range 60 circle
    DiamondDustVisual = 50264, // Boss->self, 10.0s cast, single-target
    DiamondDust = 50233, // Helper->self, 10.0s cast, range 60 circle
    AerialBlastVisual = 50266, // Boss->self, 10.0s cast, single-target
    AerialBlast = 50235, // Helper->self, 10.0s cast, range 60 circle
    SuperCyclone = 50280, // Helper->self, 0.5s cast, range 90 circle
    TailSlap = 50238, // Tail->self, 3.0s cast, range 40 width 20 rect
    TailSlapAlt = 50281, // Tail->self, 3.0s cast, range 40 width 20 rect
    TailSlapTile = 50239, // Tail->self, no cast, single-target
    LevinboltVisual = 50246, // RightWing->self, 8.0s cast, single-target
    Levinbolt = 50247, // Helper->player, no cast, range 5 circle spread on everyone
    HypernovaVisual = 50244, // RightWing->self, 8.0s cast, single-target
    Hypernova = 50245, // Helper->player, no cast, range 8 circle stack on a random player
    SummonIcicle = 50250, // LeftWing->self, 4.0s cast, single-target
    IcicleImpact = 50251, // Icicle->self, no cast, range 6 circle
    Spikesicle = 50252, // Icicle->self, 2.5s cast, range 62+R width 10 rect
    IceStormVisual = 50253, // LeftWing->self, 6.0s cast, single-target
    IceStorm = 50254, // Helper/HelperLower->self, no cast, range 60 circle
    AkhMorn = 50255, // Boss->players, 4.0s cast, range 4 circle
    AkhMornRepeat = 50256, // Boss->players, no cast, range 4 circle
    AkhRhaiFirst = 50257, // Helper->location, no cast, range 4 circle
    AkhRhai = 50258, // Helper->location, no cast, range 4 circle
    EarthBreath = 50248, // Boss->self, 6.0s cast, single-target
    EarthBreathAOE = 50249, // Helper->self, no cast, range 80 60-degree cone
    DragonfistVisual = 50284, // Boss->self, no cast, single-target
    Dragonfist = 50285, // Helper->self, 4.0s cast, range 16 circle
    GyreCharge = 50271, // Helper->self, 6.3s cast, range 100 width 60 rect
    Dragonflight = 50228, // Reiryu->self, 30.0s cast, single-target
    DragonSpirit = 50286, // ReiryuHelper->self, no cast
    BurningChains = 50270, // Helper/HelperLower->self, no cast
    Unknown1 = 50229, // Boss->self, no cast, single-target
    TailDiveNorth = 50236, // Boss->self, no cast, single-target
    TailDiveSouth = 50237, // Boss->self, no cast, single-target
    Unknown4 = 50259, // Boss->self, no cast, single-target
    Unknown5 = 50268, // Boss->self, no cast, single-target
    Unknown6 = 50269, // Boss->self, no cast, single-target
    Unknown7 = 50272, // Boss->self, no cast, single-target

    DarkMatter = 50243, // Boss->self, 3.0s cast, range 90 circle
    TouchdownVisual = 50287, // Boss->self, no cast, single-target
    Touchdown = 50288, // HelperLower->self, 5.0s cast, range 50 circle
    MeteorImpact = 50241, // Cocoon/MassiveCocoon->self, 4.0s cast, range 60 circle
    MeteorImpactMarker = 50283, // CocoonMeteor->self, 5.0s cast, range 60 circle
    ExplosiveHatch = 50242, // Cocoon->self, no cast, range 90 circle
    CocoonHatch = 50273, // Cocoon/MassiveCocoon->self, no cast, single-target
    Fireball = 50279, // Ginryu/Hakkinryu->location, 2.5s cast, range 4 circle
    BlazingTrail = 50277, // Ginryu/Hakkinryu->self, 3.0s cast, range 15 width 11 rect
    SpikedTail = 50276, // Ginryu/Hakkinryu->player, 1.5s cast, single-target
    DeathSentence = 50278, // Hakkinryu->player, 4.0s cast, single-target
    Collapse = 50275, // Hakkinryu->self, no cast, range 8 cone
    DoubleDive = 50274, // Hakkinryu->self, 3.0s cast
    Protostar = 50240, // Boss->self, 6.0s cast
    ProtostarAOE = 50267, // HelperLower->self, no cast
    TailSpitVisual = 50289, // Boss->self, 1.0s cast, single-target
    TailSpit = 50290, // HelperLower->self, 3.3s cast
    TailSpitCenter = 50227, // HelperLower->self, 3.3s cast, range 5 circle
    Shatter = 50291, // Boss->self, no cast, single-target

    AetherialRay = 50292, // HelperLower->self, 2.5s cast, range 100 width 3 rect
    TeraSlash = 50296, // Boss->player, 4.0s cast, single-target
    AtomicRayVisual = 50297, // Boss->self, 2.5s cast, single-target
    AtomicRay = 50298, // HelperLower->players, no cast, range 6 circle stack
    Wormwail = 50293, // Boss->self, 5.0s cast, range 10-50 donut
    BenightingBreathVisual = 50294, // Boss->self, 3.3s cast, single-target
    BenightingBreath = 50295, // HelperLower->self, 4.0s cast, range 50 90-degree cone
    HypernovaVisualP3 = 50305, // RightWingP3->self, 10.0s cast, single-target
    LevinboltVisualP3 = 50306, // RightWingP3->self, 10.0s cast, single-target
    SummonIcicleP3 = 50307, // LeftWingP3->self, 4.0s cast, single-target
    IceStormVisualP3 = 50308, // LeftWingP3->self, 6.0s cast, single-target
    LevinboltP3 = 50309, // HelperLower->location, 3.0s cast, range 5 circle

    TidalWaveEnrage = 50299, // Boss->self, 70.0s cast, single-target
    TidalWaveEnrageAOE = 50300, // HelperLower->self, no cast, range 80 width 60 rect
    JudgmentBoltEnrage = 50301, // LeftWingP3->self, 35.0s cast, single-target
    HellfireEnrage = 50302, // RightWingP3->self, 35.0s cast, single-target
    HellfireEnrageAOE = 50303, // HelperLower->self, no cast, range 60 circle
    JudgmentBoltEnrageAOE = 50304, // HelperLower->self, no cast, range 60 circle
}

public enum SID : uint
{
    Paralysis = 17, // Helper->player, extra=0x0
    Doom = 210, // none->player, extra=0x0
    BurningChains = 769, // none->player, extra=0x0
    Fetters = 822, // Boss->player, extra=0x0
    ThinIce = 911, // Helper->player, extra=0x0
    Affixed = 1267, // Boss->player, extra=0x0
    WormsCurse = 1419, // Reiryu->player, extra=0x0
}

public enum IconID : uint
{
    EarthBreath = 40, // player
    CocoonMarker = 57, // player
    AtomicRay = 93, // player
    BurningChains = 97, // player
    TailMarker = 126, // player
}

public enum TetherID : uint
{
    Dragonflight = 1, // Reiryu->player
    BurningChains = 9, // player->player
    AkhMorn = 22, // player->player
}
