namespace BossMod.Endwalker.Savage.P3SPhoinix;

public enum OID : uint
{
    Boss = 0x353F,
    Sparkfledged = 0x3540, // spawned mid fight, "eyes" with cone aoe
    SunbirdSmall = 0x3541, // spawned mid fight
    SunbirdLarge = 0x3543, // spawned mid fight
    Sunshadow = 0x3544, // spawned mid fight, mini birds that charge during fountains of fire
    DarkenedFire = 0x3545, // spawned mid fight
    FountainOfFire = 0x3546, // spawned mid fight, towers that healers soak
    DarkblazeTwister = 0x3547, // spawned mid fight, tornadoes
    SparkfledgedUnknown = 0x3800, // spawned mid fight, have weird kind... - look like "eyes" during death toll?..
    Helper = 0x233C, // x45
}

public enum AID : uint
{
    FledglingFlight = 26282, // Boss->Boss
    DarkenedFire = 26297, // Boss->Boss
    DarkenedFireFail = 26298, // DarkenedFire->DarkenedFire, no cast - wipe if they spawn too close to each other
    DarkenedBlaze = 26299, // DarkenedFire->DarkenedFire, 22sec cast
    BrightenedFire = 26300, // Boss->Boss
    BrightenedFireAOE = 26301, // Boss->target, no cast
    ExperimentalFireplumeSingle = 26302, // Boss->Boss
    ExperimentalFireplumeSingleAOE = 26303, // Helper->Helper
    ExperimentalFireplumeMulti = 26304, // Boss->Boss
    ExperimentalFireplumeMultiAOE = 26305, // Helper->Helper
    ExperimentalAshplumeStack = 26306, // Boss->Boss
    ExperimentalAshplumeStackAOE = 26307, // Helper->targets, no cast
    ExperimentalAshplumeSpread = 26308, // Boss->Boss
    ExperimentalAshplumeSpreadAOE = 26309, // Helper->targets, no cast, 7sec after cast end
    ExperimentalGloryplumeSingle = 26310, // Boss->Boss, single+whatever variant
    ExperimentalGloryplumeSingleAOE = 26311, // Helper->Helper, 'normal single plume' aoe
    ExperimentalGloryplumeSpread = 26312, // Boss->Boss, no cast, cast 3sec after gloryplume cast end for 'spread' variant and determines visual cue
    ExperimentalGloryplumeSpreadAOE = 26313, // Helper->target, no cast, actual damage, ~10sec after cue
    ExperimentalGloryplumeMulti = 26314, // Boss->Boss, multi+whatever variant
    ExperimentalGloryplumeMultiAOE = 26315, // Helper->Helper, 'normal multi plume' aoes
    ExperimentalGloryplumeStack = 26316, // Boss->Boss, no cast, no cast, cast 3sec after gloryplume cast end for 'stack' variant and determines visual cue
    ExperimentalGloryplumeStackAOE = 26317, // Helper->target, no cast, actual damage, ~10sec after cue
    DevouringBrand = 26318, // Boss->Boss
    DevouringBrandMiniAOE = 26319, // Helper->Helper (ones standing on cardinals)
    DevouringBrandLargeAOE = 26321, // Helper->Helper (ones standing on cardinals)
    GreatWhirlwindSmall = 26323, // SunbirdSmall->SunbirdSmall (enrage)
    GreatWhirlwindLarge = 26325, // SunbirdLarge->SunbirdLarge (enrage)
    FlamesOfUndeath = 26326, // Boss->Boss, no cast - aoe when small or big birds all die (?)
    JointPyre = 26329, // Sparkfledged->Sparkfledged, no cast - aoe when big birds die too close to each other (?)
    FireglideSweep = 26336, // SunbirdLarge->SunbirdLarge (charges)
    FireglideSweepAOE = 26337, // SunbirdLarge->targets, no cast (charge aoe)
    DeadRebirth = 26340, // Boss->Boss
    AshenEye = 26342, // Sparkfledged->Sparkfledged, eye cone
    FountainOfFire = 26343, // Boss->Boss
    SunsPinion = 26346, // Boss->Boss
    Fireglide = 26348, // Sunshadow->target, no cast (charge aoe)
    DeathToll = 26349, // Boss->Boss
    LifesAgonies = 26350, // Boss->Boss
    FirestormsOfAsphodelos = 26352, // Boss->Boss
    FlamesOfAsphodelos = 26353, // Boss->Boss
    FlamesOfAsphodelosAOE1 = 26354, // Helper->Helper, first cone, 7sec cast
    FlamesOfAsphodelosAOE2 = 26355, // Helper->Helper, first cone, 8sec cast
    FlamesOfAsphodelosAOE3 = 26356, // Helper->Helper, first cone, 9sec cast
    StormsOfAsphodelos = 26357, // Boss->Boss
    WindsOfAsphodelos = 26358, // Helper->targets, no cast, some damage during storms
    BeaconsOfAsphodelos = 26359, // Helper->targets, no cast, some damage during storms
    DarkblazeTwister = 26360, // Boss->Boss
    DarkTwister = 26361, // Twister->Twister, knockback
    BurningTwister = 26362, // Twister->Twister, aoe
    TrailOfCondemnationCenter = 26363, // Boss->Boss (central aoe variant - spread)
    TrailOfCondemnationSides = 26364, // Boss->Boss (side aoe variant - stack in pairs)
    TrailOfCondemnationAOE = 26365, // Helper->Helper (actual aoe that hits those who fail the mechanic)
    FlareOfCondemnation = 26366, // Helper->target, no cast, hit and apply fire resist debuff (spread variant)
    SparksOfCondemnation = 26367, // Helper->target, no cast, hit and apply fire resist debuff (pairs variant)
    HeatOfCondemnation = 26368, // Boss->Boss
    HeatOfCondemnationAOE = 26369, // Helper->target, no cast, hit and apply fire resist debuff
    RightCinderwing = 26370, // Boss->Boss
    LeftCinderwing = 26371, // Boss->Boss
    SearingBreeze = 26372, // Boss->Boss
    SearingBreezeAOE = 36373, // Helper->Helper
    ScorchedExaltation = 26374, // Boss->Boss
    FinalExaltation = 27691, // Boss->Boss
    DevouringBrandAOE = 28035, // Helper->Helper (one in the center), 20sec cast
}

public enum SID : uint
{
    DeathsToll = 2762,
}

public enum TetherID : uint
{
    LargeBirdFar = 1,
    LargeBirdClose = 57,
    HeatOfCondemnation = 89,
    BurningTwister = 167,
    DarkTwister = 168,
}
