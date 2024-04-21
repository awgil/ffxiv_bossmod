namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

public enum OID : uint
{
    Boss = 0x34ED,
    Helper = 0x233C, // x31
    MysticRefulgence2 = 0x34CA, // x1 spawn during parhelic circle, stays in center and does nothing interesting
    Echo = 0x34EE, // big adds: x2 at start, after death they despawn, 2 more spawn and do nothing
    CrystalOfLight = 0x34F0, // x12, spawn in intermission in pairs (normal/glowing versions??)
    Parhelion = 0x34F2, // x15 ???
    MysticRefulgence = 0x3504, // x10 spawn during parhelic circle, despawn soon after
    LightwaveCrystal = 0x1EB24A, // eventobj
    LightwaveWaveTarget = 0x1EB24B, // eventobj
    RefulgenceHexagon = 0x1EB24C, // eventobj
    RefulgenceTriangle = 0x1EB24D, // eventobj
}

public enum AID : uint
{
    AutoAttackSword = 870, // Boss/Echo->player, no cast, single-target
    HerosRadiance = 26049, // Boss->self, 5,0s cast, range 40 circle
    ShiningSaber = 26824, // Boss->self, 4,9s cast, single-target, stack
    ShiningSaberAOE = 26006, // Boss->players, no cast, range 6 circle
    CrystallizeSwordStaffWater = 26010, // Boss->self, 4,0s cast, single-target, (1) water+blue->red/green, (6) ?
    WeaponChangeAOEStaff = 26008, // Boss->self, no cast, range 10 circle
    CrystallizeTriggerIce = 26016, // Boss->self, no cast, single-target
    CrystallineWater = 26018, // Helper->players, no cast, range 6 circle, healer light party stack
    MagosRadiance = 26050, // Boss->self, 5,0s cast, range 40 circle
    AutoAttackStaff = 27732, // Boss->player, no cast, single-target
    LateralAureole2 = 28435, // Boss->self, 5,0s cast, single-target
    LateralAureole2AOE = 28436, // Helper->self, 5,5s cast, range 40 150-degree cone
    CrystallizeStaffIce = 26012, // Boss->self, 4,0s cast, single-target, (2) ice+green->red, (4) ice+green->green
    WeaponChangeAOEChakram = 26004, // Boss->self, no cast, range 5-40 donut
    CrystallizeTriggerWater = 26017, // Boss->self, no cast, single-target
    CrystallineBlizzard = 26020, // Helper->players, no cast, range 5 circle
    AutoAttackChakram = 27733, // Boss->player, no cast, single-target
    MousaScorn = 26048, // Boss->players, 5,0s cast, range 4 circle
    LateralAureole1 = 26053, // Boss->self, 5,0s cast, single-target
    LateralAureole1AOE = 26256, // Helper->self, 5,5s cast, range 40 150-degree cone
    CrystallizeChakramEarth = 26014, // Boss->self, 4,0s cast, single-target, (3) earth+red->blue
    WeaponChangeVisualSword = 26051, // Boss->self, no cast, single-target
    WeaponChangeAOESword = 28338, // Helper->self, no cast, range 40 width 10 cross
    CrystallizeTriggerEarth = 26015, // Boss->self, no cast, single-target
    CrystallineStone = 26019, // Helper->players, no cast, range 6 circle
    CrystalPhase = 26044, // Boss->self, no cast, single-target, trigger for crystal phase
    PureCrystal = 26045, // Helper->self, no cast, range 40 circle
    IncreaseConviction = 26046, // CrystalOfLight->Boss, no cast, single-target, performed every 1s by glowing crystals to increase conviction
    CrystalOfLightDeath = 26732, // Helper->Echo, no cast, single-target, 1 cast per echo after each crystal dies
    ExodusVisual = 26043, // Boss->self, no cast, single-target, happens after all echoes die
    Exodus = 26155, // Helper->self, no cast, range 40 circle
    ExodusEnrage = 27911, // Helper->self, no cast, range 40 circle, enrage if crystal phase takes too long
    Halo = 26021, // Boss->self, 5,0s cast, range 40 circle
    LightwaveSword = 26259, // Boss->self, 4,0s cast, single-target
    RayOfLight = 26023, // Helper->self, no cast, range 15 width 16 rect, aoe inside lightwave
    LightOfTheCrystal = 26022, // Helper->self, 1,0s cast, range 40 circle, aoe when lightwave hits crystal
    TeleportToCenter = 26025, // Boss->location, no cast, single-target
    InfralateralArc = 26217, // Boss->self, 4,9s cast, single-target
    InfralateralArcAOE = 26026, // Boss->self, no cast, range 40 90-degree cone
    HerosGlory = 26024, // Boss->self, 5,0s cast, range 40 180-degree cone
    HerosSundering = 26047, // Boss->self/players, 5,0s cast, range 40 90-degree cone
    ParhelicCircle = 26028, // Boss->self, 6,0s cast, single-target
    MysticRefulgenceTeleport = 26030, // MysticRefulgence->location, no cast, single-target
    IncandescenceTrigger = 26029, // MysticRefulgence2->self, no cast, single-target
    Incandescence = 26031, // MysticRefulgence->self, no cast, range 6 circle
    Aureole1 = 27793, // Boss->self, 5,0s cast, single-target
    Aureole1AOE = 27794, // Helper->self, 5,5s cast, range 40 150-degree cone
    Aureole2 = 28433, // Boss->self, 5,0s cast, single-target
    Aureole2AOE = 28434, // Helper->self, 5,5s cast, range 40 150-degree cone
    CrystallizeChakramWater = 28373, // Boss->self, 4,0s cast, single-target, (5) water+red->red
    CrystallizeChakramIce = 26013, // Boss->self, 4,0s cast, single-target, (2) ice+red->green
    CrystallizeStaffEarth = 26011, // Boss->self, 4,0s cast, single-target, (3) earth+green->blue
    Parhelion = 26032, // Boss->self
    ParhelionNext = 26033, // Boss->self, no cast
    BeaconParhelion = 26034, // Parhelion->location
    BeaconSubparhelion = 26035, // Parhelion->self, no cast
    RadiantHalo = 26036, // Boss->self
    EchoesSword = 26037, // Boss->self
    EchoesStaff = 26038, // Boss->self
    EchoesChakram = 26039, // Boss->self
    DichroicSpectrum = 26040, // Helper->mt
    BrightSpectrum = 26041, // Helper->non-tank
    EchoesAOE = 26042, // Helper->target, no cast
    LightwaveStaff = 26260, // Boss->self
    LightwaveChakram = 26261, // Boss->self
    Subparhelion = 27734, // Boss->self
    HerosRadianceEnrage = 24571, // Boss->self
}

public enum SID : uint
{
    CrystallizeElement = 2056, // Boss->Boss, extra=0x151/0x153/0x152, invisible in ui, extra determines element type
    HydaelynsWeapon = 2273, // Boss->Boss, extra=0x1B4/0x1B5, (n/a for sword, 1B4 for staff, 1B5 for chakram)
    MagosMantle = 2877, // none->Boss, extra=0x0
    MousaMantle = 2878, // none->Boss, extra=0x0
    HerosMantle = 2876, // none->Boss/Echo, extra=0x0
}

public enum IconID : uint
{
    Echoes = 305,
}
