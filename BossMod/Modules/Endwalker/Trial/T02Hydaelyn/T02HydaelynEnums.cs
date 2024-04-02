namespace BossMod.Endwalker.Trial.T02Hydaelyn;

public enum OID : uint
{
    Boss = 0x34EA, //R=5.04
    Helper = 0x233C,
    Parhelion = 0x34F1, //R=3.0
    EchoOfHydaelyn = 0x34EB, //R=5.04
    CrystalOfLight = 0x34EC, //R=2.5
    MysticRefulgence = 0x3503, //R=1.0
    MysticRefulgence2 = 0x34C9, //R=2.0
    RefulgenceTriangle = 0x1EB24D,
    RefulgenceHexagon = 0x1EB24C,
    LightwaveWaveTarget = 0x1EB24B,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttackStaff = 27732, // Boss->player, no cast, single-target
    AutoAttackChakram = 27733, // Boss->player, no cast, single-target
    HerossRadiance = 26071, // Boss->self, 5,0s cast, range 40 circle
    Teleport = 26025, // Boss->location, no cast, single-target, boss teleports mid
    Teleport2 = 28282, // Boss->location, no cast, single-target
    Teleport3 = 26030, // 3503->location, no cast, single-target, Mystic Refulgence teleports
    DawnMantle = 27660, // Boss->self, 4,9s cast, single-target
    Anthelion = 26056, // Boss->self, no cast, range 5-40 donut (not sure if 5.04 or 5, impossible to tell with naked eye)
    MousasScorn = 26070, // Boss->players, 5,0s cast, range 4 circle, shared tankbuster
    HighestHoly = 26055, // Boss->self, no cast, range 10 circle
    MagossRadiance = 26072, // Boss->self, 5,0s cast, range 40 circle
    Equinox = 26051, // Boss->self, no cast, single-target
    Equinox2 = 26255, // Helper->self, no cast, range 40 width 10 cross
    HerossSundering = 26069, // Boss->self/player, 5,0s cast, range 40 90-degree cone, tankbuster
    StartsCrystalPhase = 26067, // Boss->self, no cast, single-target
    HydaelynsRay = 26060, // EchoOfHydaelyn->self, 6,0s cast, range 45 width 40 rect, damage fall off rectangle, approx 15 optimal width
    CrystallizeTriggerEarth = 26015, // EchoOfHydaelyn/Boss->self, no cast, single-target
    Crystallize = 27729, // EchoOfHydaelyn->self, no cast, single-target
    CrystallineStoneIII = 27855, // EchoOfHydaelyn->self, 5,0s cast, single-target
    CrystallineStoneIII2 = 27737, // Helper->players, 5,0s cast, range 6 circle
    CrystallizeTriggerIce = 26016, // Boss->self, no cast, single-target
    Crystallize2 = 27853, // EchoOfHydaelyn->self, no cast, single-target
    CrystallineBlizzardIII = 27854, // EchoOfHydaelyn->self, 5,0s cast, single-target
    CrystallineBlizzardIII2 = 27738, // Helper->players, 5,0s cast, range 5 circle
    Exodus = 26043, // Boss->self, no cast, single-target
    Exodus2 = 26066, // Helper->self, no cast, range 40 circle
    CrystallizeStaffIce = 26012, // Boss->self, 4,0s cast, single-target
    ParhelicCircle = 26028, // Boss->self, 6,0s cast, single-target
    IncandescenceTrigger = 26029, // 34C9->self, no cast, single-target
    Incandescence = 26061, // 3503->self, 1,5s cast, range 6 circle
    CrystallizeChakramEarth = 26014, // Boss->self, 4,0s cast, single-target
    CrystallizeTriggerWater = 26017, // Boss->self, no cast, single-target
    Parhelion = 26032, // Boss->self, 5,0s cast, single-target
    Parhelion2 = 26033, // Boss->self, no cast, single-target
    Beacon = 26062, // Parhelion->location, 5,2s cast, width 6 rect charge
    Beacon2 = 26063, // Parhelion->self, 5,0s cast, range 45 width 6 rect
    Subparhelion = 27734, // Boss->self, 5,0s cast, single-target
    RadiantHalo = 26064, // Boss->self, 5,0s cast, range 40 circle
    EchoesA = 26037, // Boss->self, 5,0s cast, single-target
    EchoesB = 26038, // Boss->self, 5,0s cast, single-target
    EchoesC = 26039, // Boss->self, 5,0s cast, single-target
    Echoes = 26065, // Helper->players, no cast, range 6 circle
    RayOfLight = 26058, // Helper->self, no cast, range 15 width 16 rect
    Lightwave = 26260, // Boss->self, 4,0s cast, single-target
    Lightwave2 = 26261, // Boss->self, 4,0s cast, single-target
    Lightwave3 = 26259, // Boss->self, 4,0s cast, single-target
};

public enum IconID : uint
{
    Echoes = 305, // player, stack 5 times
};

public enum SID : uint
{
    HerossMantle = 2876, // none->EchoOfHydaelyn/Boss, extra=0x0, sword stance
    HydaelynsWeapon = 2273, // Boss->Boss, extra=0x1B5/0x1B4 (n/a for sword, 1B4 for staff, 1B5 for chakram)
    MousasMantle = 2878, // none->Boss, extra=0x0, chakram stance
    MagossMantle = 2877, // none->Boss, extra=0x0, staff stance
    CrystallizeElement = 2056, // EchoOfHydaelyn/Boss->EchoOfHydaelyn/Boss, extra=0x152/0x153
};
