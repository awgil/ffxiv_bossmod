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
    AutoAttackSword = 870,
    Enrage = 24571, // Boss->self
    WeaponChangeAOEChakram = 26004, // Boss->self, no cast, chakram aoe
    WeaponChangeAOEStaff = 26008, // Boss->self, no cast, staff aoe
    ShiningSaberAOE = 26006, // Boss->target, no cast, shared damage
    CrystallizeSwordStaffWater = 26010, // Boss->self: (1) water+blue->red/green, (6) ?
    CrystallizeStaffEarth = 26011, // Boss->self: (3) earth+green->blue
    CrystallizeStaffIce = 26012, // Boss->self: (2) ice+green->red, (4) ice+green->green
    CrystallizeChakramIce = 26013, // Boss->self: (2) ice+red->green
    CrystallizeChakramEarth = 26014, // Boss->self: (3) earth+red->blue
    CrystallizeTriggerEarth = 26015, // Boss->self, no cast, removes buff and triggers aoe
    CrystallizeTriggerIce = 26016, // Boss->self, no cast, removes buff and triggers aoe
    CrystallizeTriggerWater = 26017, // Boss->self, no cast, removes buff and triggers aoe
    CrystallineWater = 26018, // Helper->healer, no cast
    CrystallineStone = 26019, // Helper->target, no cast
    CrystallineBlizzard = 26020, // Helper->target, no cast
    Halo = 26021, // Boss->self
    LightOfTheCrystal = 26022, // Helper->self, no cast, aoe when lightwave hits crystal
    RayOfLight = 26023, // Helper->self, no cast, aoe inside lightwave
    HerosGlory = 26024, // Boss->self
    TeleportToCenter = 26025, // Boss->n/a
    InfralateralArcAOE = 26026, // Boss->self, no cast
    ParhelicCircle = 26028, // Boss->self
    Incandescence = 26031, // MysticRefulgence->self, no cast, aoe
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
    ExodusVisual = 26043, // Boss->self, no cast, ??? (happens after all echoes die)
    PureCrystal = 26045, // Helper->self, no cast, raidwide
    IncreaseConviction = 26046, // CrystalOfLight->Boss, no cast, performed every 1s by glowing crystals to increase conviction
    HerosSundering = 26047, // Boss->mt
    MousaScorn = 26048, // Boss->mt
    HerosRadiance = 26049, // Boss->self
    MagosRadiance = 26050, // Boss->self
    WeaponChangeVisualSword = 26051, // Boss->self, no cast, sword visual
    LateralAureole1 = 26053, // Boss->self
    Exodus = 26155, // Helper->self, no cast, raidwide
    InfralateralArc = 26217, // Boss->self
    LateralAureole1AOE = 26256, // Helper->self
    LightwaveSword = 26259, // Boss->self
    LightwaveStaff = 26260, // Boss->self
    LightwaveChakram = 26261, // Boss->self
    CrystalOfLightDeath = 26732, // Helper->Echo, no cast, 1 cast per echo after each crystal dies
    ShiningSaber = 26824, // Boss->self
    AutoAttackStaff = 27732,
    AutoAttackChakram = 27733,
    Subparhelion = 27734, // Boss->self
    Aureole1 = 27793, // Boss->self
    Aureole1AOE = 27794, // Helper->self
    WeaponChangeAOESword = 28338, // Helper->self, no cast, sword aoe
    CrystallizeChakramWater = 28373, // Boss->self: (5) water+red->red
    Aureole2 = 28433, // Boss->self
    Aureole2AOE = 28434, // Helper->self
    LateralAureole2 = 28435, // Boss->self
    LateralAureole2AOE = 28436, // Helper->self
}

public enum SID : uint
{
    CrystallizeElement = 2056, // invisible in ui, extra determines element type
    HerosMantle = 2876, // sword stance
    MagosMantle = 2877, // staff stance
    MousaMantle = 2878, // chakram stance
}

public enum IconID : uint
{
    Echoes = 305,
}
