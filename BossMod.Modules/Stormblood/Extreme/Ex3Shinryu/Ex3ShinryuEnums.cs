namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

public enum OID : uint
{
    Boss = 0x1F5A, // R22.000, x1
    Tail = 0x1F87, // R17.940, x0 (spawn during fight)
    Cocoon = 0x1F88, // R3.000, x0 (spawn during fight)
    Ginryu = 0x1F89, // R1.800, x0 (spawn during fight)
    Icicle = 0x1F8B, // R2.500, x0 (spawn during fight)
    EyeOfTheStorm = 0x1F8C, // R1.000, x0 (spawn during fight)
    WormsHeart = 0x1F8D, // R29.000, x0 (spawn during fight), Part type
    LeftWing = 0x1F8E, // R15.000, x1, Part type
    RightWing = 0x1F8F, // R15.000, x1, Part type
    Hakkinryu = 0x1F90, // R3.600, x0 (spawn during fight)
    MassiveCocoon = 0x1F91, // R6.000, x0 (spawn during fight)
    Reiryu = 0x1F92, // R2.000, x0 (spawn during fight)
    Helper = 0x1FA7, // R0.500, x56, Helper type
    LeftWingP3 = 0x1FA9, // R11.200, x0 (spawn during fight), Part type
    RightWingP3 = 0x1FAA, // R11.200, x0 (spawn during fight), Part type
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
    AutoAttack = 9720, // LeftWing/RightWing/LeftWingP3/RightWingP3->player, no cast, single-target

    EarthenFuryVisual = 9725, // Boss->self, 10.0s cast, single-target
    EarthenFury = 9694, // Helper->self, 10.0s cast, range 60 circle
    EarthenFuryTile = 9742, // Helper->self, 10.0s cast, range 20 width 20 rect
    TidalWaveVisual = 9721, // Boss->self, 10.0s cast, single-target
    TidalWave = 9690, // Helper->self, 10.0s cast, range 80 width 60 rect
    HellfireVisual = 9722, // Boss->self, 10.0s cast, single-target
    Hellfire = 9691, // Helper->self, 10.0s cast, range 60 circle
    JudgmentBoltVisual = 9723, // Boss->self, 10.0s cast, single-target
    JudgmentBolt = 9692, // Helper->self, 10.0s cast, range 60 circle
    DiamondDustVisual = 9724, // Boss->self, 10.0s cast, single-target
    DiamondDust = 9693, // Helper->self, 10.0s cast, range 60 circle
    AerialBlastVisual = 9726, // Boss->self, 10.0s cast, single-target
    AerialBlast = 9695, // Helper->self, 10.0s cast, range 60 circle
    SuperCyclone = 9740, // Helper->self, 0.5s cast, range 90 circle
    TailSlap = 9698, // Tail->self, 3.0s cast, range 40 width 20 rect
    TailSlapAlt = 9741, // Tail->self, 3.0s cast, range 40 width 20 rect
    TailSlapTile = 9699, // Tail->self, no cast, single-target
    LevinboltVisual = 9706, // RightWing->self, 8.0s cast, single-target
    Levinbolt = 9707, // Helper->player, no cast, range 5 circle spread on everyone
    HypernovaVisual = 9704, // RightWing->self, 8.0s cast, single-target
    Hypernova = 9705, // Helper->player, no cast, range 8 circle stack on a random player
    SummonIcicle = 9710, // LeftWing->self, 4.0s cast, single-target
    IcicleImpact = 9711, // Icicle->self, no cast, range 6 circle
    Spikesicle = 9712, // Icicle->self, 2.5s cast, range 62+R width 10 rect
    IceStormVisual = 9713, // LeftWing->self, 6.0s cast, single-target
    IceStorm = 9714, // Helper->self, no cast, range 60 circle
    AkhMorn = 9715, // Boss->players, 4.0s cast, range 4 circle
    AkhMornRepeat = 9716, // Boss->players, no cast, range 4 circle
    AkhRhaiFirst = 9717, // Helper->location, no cast, range 4 circle
    AkhRhai = 9718, // Helper->location, no cast, range 4 circle
    EarthBreath = 9708, // Boss->self, 6.0s cast, single-target
    EarthBreathAOE = 9709, // Helper->self, no cast, range 80 60-degree cone
    DragonfistVisual = 9744, // Boss->self, no cast, single-target
    Dragonfist = 9745, // Helper->self, 4.0s cast, range 16 circle
    GyreCharge = 9731, // Helper->self, 6.3s cast, range 100 width 60 rect
    Dragonflight = 7635, // Reiryu->self, 30.0s cast, single-target
    DragonSpirit = 9746, // ReiryuHelper->self, no cast
    BurningChains = 9730, // Helper->self, no cast
    Unknown1 = 9689, // Boss->self, no cast, single-target
    TailDiveNorth = 9696, // Boss->self, no cast, single-target
    TailDiveSouth = 9697, // Boss->self, no cast, single-target
    Unknown4 = 9719, // Boss->self, no cast, single-target
    Unknown5 = 9728, // Boss->self, no cast, single-target
    Unknown6 = 9729, // Boss->self, no cast, single-target
    Unknown7 = 9732, // Boss->self, no cast, single-target

    DarkMatter = 9703, // Boss->self, 3.0s cast, range 90 circle
    TouchdownVisual = 9747, // Boss->self, no cast, single-target
    Touchdown = 9748, // Helper->self, 5.0s cast, range 50 circle
    MeteorImpact = 9701, // Cocoon/MassiveCocoon->self, 4.0s cast, range 60 circle
    MeteorImpactMarker = 9743, // Helper->self, 5.0s cast, range 60 circle
    ExplosiveHatch = 9702, // Cocoon->self, no cast, range 90 circle
    CocoonHatch = 9733, // Cocoon/MassiveCocoon->self, no cast, single-target
    Fireball = 9739, // Ginryu/Hakkinryu->location, 2.5s cast, range 4 circle
    BlazingTrail = 9737, // Ginryu/Hakkinryu->self, 3.0s cast, range 15 width 11 rect
    SpikedTail = 9736, // Ginryu/Hakkinryu->player, 1.5s cast, single-target
    DeathSentence = 9738, // Hakkinryu->player, 4.0s cast, single-target
    Collapse = 9735, // Hakkinryu->self, no cast, range 8 cone
    DoubleDive = 9734, // Hakkinryu->self, 3.0s cast
    Protostar = 9700, // Boss->self, 6.0s cast
    ProtostarAOE = 9727, // Helper->self, no cast
    TailSpitVisual = 9749, // Boss->self, 1.0s cast, single-target
    TailSpit = 9750, // Helper->self, 3.3s cast
    TailSpitCenter = 7633, // Helper->self, 3.3s cast, range 5 circle
    Shatter = 9751, // Boss->self, no cast, single-target

    AetherialRay = 9752, // Helper->self, 2.5s cast, range 100 width 3 rect
    TeraSlash = 9803, // Boss->player, 4.0s cast, single-target
    AtomicRayVisual = 9804, // Boss->self, 2.5s cast, single-target
    AtomicRay = 9805, // Helper->players, no cast, range 6 circle stack
    Wormwail = 9800, // Boss->self, 5.0s cast, range 10-50 donut
    BenightingBreathVisual = 9801, // Boss->self, 3.3s cast, single-target
    BenightingBreath = 9802, // Helper->self, 4.0s cast, range 50 90-degree cone
    HypernovaVisualP3 = 10015, // RightWingP3->self, 10.0s cast, single-target
    LevinboltVisualP3 = 10016, // RightWingP3->self, 10.0s cast, single-target
    SummonIcicleP3 = 10017, // LeftWingP3->self, 4.0s cast, single-target
    IceStormVisualP3 = 10018, // LeftWingP3->self, 6.0s cast, single-target
    LevinboltP3 = 10021, // Helper->location, 3.0s cast, range 5 circle

    TidalWaveEnrage = 9806, // Boss->self, 70.0s cast, single-target
    TidalWaveEnrageAOE = 10005, // Helper->self, no cast, range 80 width 60 rect
    JudgmentBoltEnrage = 10008, // LeftWingP3->self, 35.0s cast, single-target
    HellfireEnrage = 10009, // RightWingP3->self, 35.0s cast, single-target
    HellfireEnrageAOE = 10010, // Helper->self, no cast, range 60 circle
    JudgmentBoltEnrageAOE = 10011, // Helper->self, no cast, range 60 circle
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
