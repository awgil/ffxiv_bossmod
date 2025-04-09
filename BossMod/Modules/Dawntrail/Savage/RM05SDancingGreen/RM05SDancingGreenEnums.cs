namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

public enum OID : uint
{
    Boss = 0x47B9, // R4.998, x1
    Helper = 0x233C, // R0.500, x25, Helper type
    Frogtourage = 0x47BA, // R3.142, x8
    Spotlight = 0x47BB, // R1.000, x8
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    Bleeding = 2088, // Helper->player, extra=0x0
    NeedlePoised = 4459, // Boss->Boss, extra=0x0
    UnknownFrog = 2056, // Frogtourage->Spotlight/Frogtourage, extra=0x37D/0x386/0xE1
    Unknown = 4515, // none->player, extra=0x0
    Spotlightless = 4472, // none->player, extra=0x0
    InTheSpotlight = 4471, // none->player, extra=0x0
    DirectionalDisregard = 3808, // none->Boss, extra=0x0
    PerfectGroove = 4464, // none->player, extra=0x0
    SustainedDamage = 2935, // Helper->player, extra=0x0
    BurnBabyBurn = 4461, // none->player, extra=0x0
    WavelengthA = 4462, // none->player, extra=0x0
    WavelengthB = 4463, // none->player, extra=0x0
    FrogtourageFan = 3998, // Helper->player, extra=0x0
}

public enum AID : uint
{
    AutoAttack = 41767, // Boss->player, no cast, single-target
    DeepCutVisual = 42785, // Boss->self, 5.0s cast, single-target
    DeepCut = 42786, // Helper->self, no cast, range 60 ?-degree cone
    FlipToBSide = 42881, // Boss->self, 4.0s cast, single-target
    PlayBSideVisual = 37833, // Boss->self, no cast, single-target
    PlayBSide = 42884, // Helper->self, no cast, range 50 width 8 rect
    FlipToASide = 42880, // Boss->self, 4.0s cast, single-target
    PlayASideVisual = 37832, // Boss->self, no cast, single-target
    PlayASide = 42883, // Helper->self, no cast, range 60 ?-degree cone
    CelebrateGoodTimes = 42787, // Boss->self, 5.0s cast, range 60 circle
    DiscoInfernal = 42838, // Boss->self, 4.0s cast, range 60 circle
    FunkyFloor = 42834, // Boss->self, 2.5+0.5s cast, single-target
    FunkyFloorActivate = 42835, // Helper->self, no cast, ???
    InsideOutCircle = 37826, // Helper->self, no cast, range 7 circle
    InsideOutVisual = 42876, // Boss->self, 5.0s cast, single-target
    InsideOutVisual2 = 42877, // Boss->self, no cast, single-target
    InsideOutDonut = 37827, // Helper->self, no cast, range ?-40 donut
    Shame = 42840, // Helper->player, 1.0s cast, single-target
    EnsembleAssemble = 39474, // Boss->self, 3.0s cast, single-target
    ArcadyNightFever = 42848, // Boss->self, 4.8s cast, single-target
    GetDownAOEFirst = 39908, // Helper->self, 5.2s cast, range 7 circle
    GetDownProtean = 42852, // Helper->self, no cast, range 40 ?-degree cone
    FroggyRight = 42764, // Frogtourage->self, 1.7s cast, single-target
    GetDownAOEDonut = 42851, // Helper->self, no cast, range ?-40 donut
    GetDownRepeat = 42853, // Helper->self, 2.5s cast, range 40 ?-degree cone
    FroggyLeft = 42765, // Frogtourage->self, 1.7s cast, single-target
    GetDownAOECircle = 42850, // Helper->self, no cast, range 7 circle
    LetsDance = 42858, // Boss->self, 5.8s cast, single-target
    LetsDanceAOE = 39901, // Helper->self, no cast, range 25 width 50 rect
    LetsDanceVisual = 42861, // Boss->self, no cast, single-target
    LetsDanceVisual2 = 42862, // Boss->self, no cast, single-target
    MinorFreakOut = 42856, // Helper->location, no cast, range 2 circle
    MinorFreakOut1 = 39475, // Helper->location, no cast, range 2 circle
    MinorFreakOut2 = 39476, // Helper->location, no cast, range 2 circle
    MinorFreakOut3 = 39478, // Helper->location, no cast, range 2 circle
    LetsPose1 = 42863, // Boss->self, 5.0s cast, range 60 circle
    FreakOut1 = 42854, // Helper->player, no cast, single-target
    FreakOut2 = 42855, // Helper->player, no cast, single-target
    RideTheWavesVisual = 42836, // Boss->self, 3.5+0.5s cast, single-target
    RideTheWaves = 42837, // Helper->self, no cast, ???
    QuarterBeats = 42844, // Helper->players, 5.0s cast, range 4 circle
    QuarterBeatsBoss = 42843, // Boss->self, 5.0s cast, single-target
    EighthBeats = 42846, // Helper->players, 5.0s cast, range 5 circle
    EighthBeatsBoss = 42845, // Boss->self, 5.0s cast, single-target
    Frogtourage = 42847, // Boss->self, 3.0s cast, single-target
    Moonburn1 = 42868, // Helper->self, 10.5s cast, range 40 width 15 rect
    Moonburn2 = 42867, // Helper->self, 10.5s cast, range 40 width 15 rect
    BackUpDanceVisual = 42871, // Frogtourage->self, 8.9s cast, single-target
    BackUpDance = 42872, // Helper->self, no cast, range 60 ?-degree cone
    OutsideInDonut = 37828, // Helper->self, no cast, range ?-40 donut
    OutsideInVisual = 42878, // Boss->self, 5.0s cast, single-target
    OutsideInCircle = 37829, // Helper->self, no cast, range 7 circle
    OutsideInVisual2 = 42879, // Boss->self, no cast, single-target
    ArcadyNightEncore = 41840, // Boss->self, 4.8s cast, single-target
    FroggyUp = 42763, // Frogtourage->self, 1.7s cast, single-target
    FroggyDown = 42762, // Frogtourage->self, 1.7s cast, single-target
    LetsDanceRemix = 41872, // Boss->self, 5.8s cast, single-target
    LetsDanceRemixAOE = 41877, // Helper->self, no cast, range 25 width 50 rect
    LetsDanceRemixVisual = 41874, // Boss->self, no cast, single-target
    LetsDanceRemixVisual2 = 41875, // Boss->self, no cast, single-target
    LetsDanceRemixVisual3 = 41873, // Boss->self, no cast, single-target
    LetsDanceRemixVisual4 = 41876, // Boss->self, no cast, single-target
    LetsPose2 = 42864, // Boss->self, 5.0s cast, range 60 circle
    DoTheHustleFrogs1 = 42869, // Frogtourage->self, 5.0s cast, range 50 ?-degree cone
    DoTheHustleFrogs2 = 42870, // Frogtourage->self, 5.0s cast, range 50 ?-degree cone
    DoTheHustleBoss1 = 42789, // Boss->self, 5.0s cast, range 50 ?-degree cone
    DoTheHustleBoss2 = 42788, // Boss->self, 5.0s cast, range 50 ?-degree cone
    FrogtourageFinale = 42209, // Boss->self, 3.0s cast, single-target
    HiNRGFever = 42873, // Boss->self, 12.0s cast, range 60 circle
    BossReposition = 42693, // Boss->location, no cast, single-target

    W2SnapBoss1 = 42792, // Boss->self, 5.0s cast, range 20 width 40 rect
    W2SnapBoss2 = 42793, // Boss->self, 5.0s cast, range 20 width 40 rect
    W2SnapBoss3 = 42794, // Boss->self, 5.0s cast, range 20 width 40 rect
    W2SnapBoss4 = 42795, // Boss->self, 5.0s cast, range 20 width 40 rect
    W2SnapBoss5 = 42796, // Boss->self, 5.0s cast, range 20 width 40 rect
    W2SnapBoss6 = 42797, // Boss->self, 5.0s cast, range 20 width 40 rect
    W2SnapAOE1 = 42798, // Helper->self, 1.5s cast, range 25 width 50 rect
    W2SnapAOELast = 42799, // Helper->self, 3.5s cast, range 25 width 50 rect

    W3SnapBoss1 = 42800, // Boss->self, 5.0s cast, range 20 width 40 rect
    W3SnapBoss2 = 42801, // Boss->self, 5.0s cast, range 20 width 40 rect
    W3SnapBoss3 = 42802, // Boss->self, 5.0s cast, range 20 width 40 rect
    W3SnapBoss4 = 42803, // Boss->self, 5.0s cast, range 20 width 40 rect
    W3SnapBoss5 = 42804, // Boss->self, 5.0s cast, range 20 width 40 rect
    W3SnapBoss6 = 42805, // Boss->self, 5.0s cast, range 20 width 40 rect
    W3SnapAOE1 = 42806, // Helper->self, 1.2s cast, range 25 width 50 rect
    W3SnapAOE2 = 42807, // Helper->self, 1.9s cast, range 25 width 50 rect
    W3SnapAOELast = 42808, // Helper->self, 3.5s cast, range 25 width 50 rect

    W4SnapBoss1 = 42809, // Boss->self, 5.0s cast, range 20 width 40 rect
    W4SnapBoss2 = 42810, // Boss->self, 5.0s cast, range 20 width 40 rect
    W4SnapBoss3 = 42811, // Boss->self, 5.0s cast, range 20 width 40 rect
    W4SnapBoss4 = 42812, // Boss->self, 5.0s cast, range 20 width 40 rect
    W4SnapBoss5 = 42813, // Boss->self, 5.0s cast, range 20 width 40 rect
    W4SnapBoss6 = 42814, // Boss->self, 5.0s cast, range 20 width 40 rect
    W4SnapAOE1 = 42815, // Helper->self, 1.0s cast, range 25 width 50 rect
    W4SnapAOE2 = 42816, // Helper->self, 1.5s cast, range 25 width 50 rect
    W4SnapAOE3 = 42817, // Helper->self, 2.0s cast, range 25 width 50 rect
    W4SnapAOELast = 42818, // Helper->self, 3.5s cast, range 25 width 50 rect

    W2SnapBoss7 = 42203, // Boss->self, 5.0s cast, range 20 width 40 rect
    W2SnapBoss8 = 42204, // Boss->self, 5.0s cast, range 20 width 40 rect
    W3SnapBoss7 = 42205, // Boss->self, 5.0s cast, range 20 width 40 rect
    W3SnapBoss8 = 42206, // Boss->self, 5.0s cast, range 20 width 40 rect
    W4SnapBoss7 = 42207, // Boss->self, 5.0s cast, range 20 width 40 rect
    W4SnapBoss8 = 42208, // Boss->self, 5.0s cast, range 20 width 40 rect

    Unk1 = 42849, // Boss->self, no cast, single-target
    Unk2 = 37825, // Helper->Frogtourage, 1.2s cast, single-target
    Unk3 = 37830, // Boss->self, no cast, single-target
    Unk4 = 38464, // Boss->self, no cast, single-target
    Unk5 = 38465, // Boss->self, no cast, single-target
    Unk6 = 39091, // Boss->self, no cast, single-target
    Fire = 39093, // Boss->self, no cast, single-target
    Unk7 = 39906, // Frogtourage->self, no cast, single-target
    Unk8 = 39907, // Frogtourage->self, no cast, single-target
    Unk9 = 37844, // Frogtourage->self, 5.0s cast, single-target
    Unk10 = 37843, // Frogtourage->self, 5.0s cast, single-target
    Unk11 = 42781, // Frogtourage->self, no cast, single-target
    Unk12 = 42782, // Frogtourage->self, 1.0s cast, single-target
    Unk13 = 41839, // Frogtourage->self, no cast, single-target
    Unk14 = 41836, // Frogtourage->self, no cast, single-target
    Unk15 = 41838, // Frogtourage->self, no cast, single-target
    Unk16 = 41837, // Frogtourage->self, no cast, single-target
    Unk17 = 39904, // Frogtourage->self, 5.0s cast, single-target
    Unk18 = 39905, // Frogtourage->self, 5.0s cast, single-target
    Unk19 = 42874, // Frogtourage->self, no cast, single-target
    Unk20 = 42875, // Frogtourage->self, no cast, single-target
}

