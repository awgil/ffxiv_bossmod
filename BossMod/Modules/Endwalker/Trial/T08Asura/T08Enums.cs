namespace BossMod.Endwalker.Trial.T08Asura;

public enum OID : uint
{
    Boss = 0x409B, //R=5.0
    Helper = 0x233C,
    Helper2 = 0x40A9,
    PhantomAsura = 0x40F8, //R=5.0
    AsuraImage = 0x40A2, //R=45.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    LowerRealm = 36001, // Boss->self, 5,0s cast, range 40 circle, raidwide
    Teleport = 35966, // Boss->location, no cast, single-target, boss teleports mid
    AsuriChakra = 35994, // Boss->self, 6,0s cast, range 5 circle
    Chakra1 = 35995, // Helper->self, 6,0s cast, range 6-8 donut
    Chakra2 = 35996, // Helper->self, 6,0s cast, range 9-11 donut
    Chakra4 = 35997, // Helper->self, 6,0s cast, range 12-14 donut
    Chakra3 = 35998, // Helper->self, 6,0s cast, range 15-17 donut
    Chakra5 = 35999, // Helper->self, 6,0s cast, range 18-20 donut
    CuttingJewel = 36000, // Boss->players, 5,0s cast, range 4 circle, tankbuster
    Ephemerality = 35990, // Boss->self, 5,0s cast, range 20 circle, raidwide
    Laceration = 35991, // PhantomAsura->self, 1,0s cast, range 9 circle
    Divinity = 36008, // Boss->self, no cast, single-target
    Divinity2 = 36009, // AsuraImage->self, no cast, single-target
    DivineAwakening = 35967, // Boss->self, no cast, single-target
    DivineAwakening2 = 35968, // AsuraImage->self, no cast, single-target
    IconographyPedestalPurge = 35969, // Boss->self, 5,0s cast, range 10 circle
    IconicExecution = 36017, // Boss->self, 4,0s cast, single-target
    IconicExecution2 = 36018, // Boss->self, 3,0s cast, single-target
    PedestalPurge = 35970, // AsuraImage->self, 4,0s cast, range 27 circle (lumina data says range 60, which is obvious bs)
    IconographyWheelOfDeincarnation = 35971, // Boss->self, 5,0s cast, range 8-40 donut
    WheelOfDeincarnation = 35972, // AsuraImage->self, 4,0s cast, range 15-96 donut
    IconographyBladewise = 35973, // Boss->self, 5,0s cast, range 50 width 6 rect
    Bladewise = 35974, // AsuraImage->self, 4,0s cast, range 100 width 28 rect
    RemoveStatus = 36010, // AsuraImage->self, no cast, single-target, removes status 2552 (unknown effect) from itself
    KhadgaTelegraph1 = 36011, // Helper->self, 2,0s cast, range 20 180-degree cone
    KhadgaTelegraph2 = 36013, // Helper->self, 2,0s cast, range 20 180-degree cone
    KhadgaTelegraph3 = 36012, // Helper->self, 2,0s cast, range 20 180-degree cone
    SixBladedKhadga = 35976, // Boss->self, 13,0s cast, single-target
    Khadga1 = 35977, // Boss->self, no cast, range 20 180-degree cone
    Khadga2 = 35981, // Boss->self, no cast, range 20 180-degree cone
    Khadga3 = 35980, // Boss->self, no cast, range 20 180-degree cone
    Khadga4 = 35978, // Boss->self, no cast, range 20 180-degree cone
    Khadga5 = 35982, // Boss->self, no cast, range 20 180-degree cone
    Khadga6 = 35979, // Boss->self, no cast, range 20 180-degree cone
    ManyFaces1 = 35983, // Boss->self, 4,0s cast, single-target
    ManyFaces2 = 36014, // Boss->self, no cast, single-target
    TheFaceOfWrathA = 35984, // Boss->self, 8,0s cast, single-target
    TheFaceOfWrathB = 35986, // Boss->self, 8,0s cast, single-target
    TheFaceOfWrath = 36022, // Helper->self, no cast, single-target
    FaceMechanicWrath = 36015, // Helper->self, 8,0s cast, range 20 180-degree cone
    FaceMechanicDelight = 36016, // Helper->self, 8,0s cast, range 20 180-degree cone
    TheFaceOfDelightA = 35989, // Boss->self, 8,0s cast, single-target
    TheFaceOfDelightB = 35987, // Boss->self, 8,0s cast, single-target
    TheFaceOfDelight = 36023, // Helper->self, no cast, single-target
    TheFaceOfDelightSnapshot = 36007, // Helper->self, no cast, range 20 180-degree cone
    TheFaceOfWrathSnapshot = 36006, // Helper->self, no cast, range 20 180-degree cone
    MyriadAspects = 36019, // Boss->self, 3,0s cast, single-target
    MyriadAspects1 = 36020, // Helper->self, 4,0s cast, range 40 30-degree cone
    MyriadAspects2 = 36021, // Helper->self, 6,0s cast, range 40 30-degree cone
    Bladescatter = 35992, // Boss->self, 5,0s cast, single-target
    Scattering = 35993, // Helper->self, 3,0s cast, range 20 width 6 rect
    OrderedChaos = 36002, // Boss->self, no cast, single-target
    OrderedChaos2 = 36003, // Helper->player, 5,0s cast, range 5 circle
};

public enum IconID : uint
{
    Tankbuster = 342, // player
    Khadga1 = 454, // Helper, icon 1
    Khadga2 = 455, // Helper, icon 2
    Khadga3 = 456, // Helper, icon 3
    Khadga4 = 457, // Helper, icon 4
    Khadga5 = 458, // Helper, icon 5
    Khadga6 = 459, // Helper, icon 6
    Spreadmarker = 139, // player
};
