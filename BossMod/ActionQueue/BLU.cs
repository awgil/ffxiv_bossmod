namespace BossMod.BLU;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Whistle = 18309, // L1, 1.0s cast, GCD, range 0, single-target, targets=Self
    TheRoseOfDestruction = 23275, // L1, 2.0s cast, 30.0s CD (group 3/57), range 25, single-target, targets=Hostile
    DivineCataract = 23274, // L1, instant, GCD, range 0, AOE 10 circle, targets=Self
    ChelonianGate = 23273, // L1, 2.0s cast, 30.0s CD (group 3/57), range 0, single-target, targets=Self
    AngelsSnack = 23272, // L1, 2.0s cast, 120.0s CD (group 15/57), range 0, AOE 20 circle, targets=Self
    FeculentFlood = 23271, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=Hostile
    SaintlyBeam = 23270, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=Hostile
    Stotram1 = 23269, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=Self
    WhiteDeath = 23268, // L1, instant, GCD, range 25, single-target, targets=Hostile
    ColdFog = 23267, // L1, 2.0s cast, 90.0s CD (group 11/57), range 0, single-target, targets=Self
    TatamiGaeshi = 23266, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 5 rect, targets=Hostile
    Tingle = 23265, // L1, 2.0s cast, GCD, range 20, AOE 6 circle, targets=Hostile
    TripleTrident = 23264, // L1, 2.0s cast, 90.0s CD (group 10/57), range 3, single-target, targets=Hostile
    AethericMimicryReleaseHealer = 19240, // L1, instant, GCD, range 0, single-target, targets=Self
    AethericMimicryReleaseDPS = 19239, // L1, instant, GCD, range 0, single-target, targets=Self
    BasicInstinct = 23276, // L1, 2.0s cast, GCD, range 0, single-target, targets=Self
    AethericMimicryReleaseTank = 19238, // L1, instant, GCD, range 0, single-target, targets=Self
    Quasar = 18324, // L1, instant, 60.0s CD (group 6), range 0, AOE 15 circle, targets=Self
    Surpanakha = 18323, // L1, instant, 30.0s CD (group 13/70) (4 charges), range 0, AOE 16+R ?-degree cone, targets=Self
    AethericMimicry = 18322, // L1, 1.0s cast, GCD, range 25, single-target, targets=Party/Alliance/Friendly
    CondensedLibra = 18321, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    Devour = 18320, // L1, 1.0s cast, 60.0s CD (group 7/57), range 3, single-target, targets=Hostile
    Reflux = 18319, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    Exuviation = 18318, // L1, 2.0s cast, GCD, range 0, AOE 6 circle, targets=Self
    AngelWhisper = 18317, // L1, 10.0s cast, 300.0s CD (group 24/57), range 25, single-target, targets=Party/Alliance/Friendly/Dead
    RevengeBlast = 18316, // L1, 2.0s cast, GCD, range 3, single-target, targets=Hostile
    Cactguard = 18315, // L1, 1.0s cast, GCD, range 25, single-target, targets=Party
    PerpetualRay = 18314, // L1, 3.0s cast, GCD, range 25, single-target, targets=Hostile
    Launcher = 18313, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=Self
    Level5Death = 18312, // L1, 2.0s cast, 180.0s CD (group 16/57), range 0, AOE 6 circle, targets=Self
    BlackKnightsTour = 18311, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=Hostile
    JKick = 18325, // L1, instant, 60.0s CD (group 6), range 25, AOE 6 circle, targets=Hostile, animLock=0.900
    WhiteKnightsTour = 18310, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=Hostile
    Ultravibration = 23277, // L1, 2.0s cast, 120.0s CD (group 16/57), range 0, AOE 6 circle, targets=Self
    MustardBomb = 23279, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=Hostile
    SeaShanty = 34580, // L1, instant, 120.0s CD (group 19), range 0, AOE 10 circle, targets=Self
    MortalFlame = 34579, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    CandyCane = 34578, // L1, 1.0s cast, 90.0s CD (group 9/57), range 25, AOE 5 circle, targets=Hostile
    LaserEye = 34577, // L1, 2.0s cast, GCD, range 25, AOE 8 circle, targets=Hostile
    WingedReprobation = 34576, // L1, 1.0s cast, 90.0s CD (group 12/57), range 25, AOE 25+R width 5 rect, targets=Hostile
    ForceField = 34575, // L1, 2.0s cast, 120.0s CD (group 21/57), range 0, single-target, targets=Self
    ConvictionMarcato = 34574, // L1, 2.0s cast, GCD, range 25, AOE 25+R width 5 rect, targets=Hostile
    DimensionalShift = 34573, // L1, 5.0s cast, GCD, range 0, AOE 10 circle, targets=Self
    DivinationRune = 34572, // L1, 2.0s cast, GCD, range 0, AOE 15+R ?-degree cone, targets=Self
    RubyDynamics = 34571, // L1, 2.0s cast, 30.0s CD (group 3/57), range 0, AOE 12+R ?-degree cone, targets=Self
    DeepClean = 34570, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=Hostile
    PeatPelt = 34569, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=Hostile
    WildRage = 34568, // L1, 5.0s cast, GCD, range 0, AOE 10 circle, targets=Self
    BreathOfMagic = 34567, // L1, 2.0s cast, GCD, range 0, AOE 10+R ?-degree cone, targets=Self
    Blaze = 23278, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=Hostile
    Rehydration = 34566, // L1, 5.0s cast, GCD, range 0, single-target, targets=Self
    RightRound = 34564, // L1, 2.0s cast, GCD, range 0, AOE 8 circle, targets=Self
    GoblinPunch = 34563, // L1, instant, GCD, range 3, single-target, targets=Hostile
    Stotram2 = 23416, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=Self
    Nightbloom = 23290, // L1, instant, 120.0s CD (group 18), range 0, AOE 10 circle, targets=Self
    PhantomFlurryEnd = 23289, // L1, instant, GCD, range 0, AOE 16+R ?-degree cone, targets=Self
    PhantomFlurry = 23288, // L1, instant, 120.0s CD (group 17), range 0, AOE 8+R ?-degree cone, targets=Self
    BothEnds = 23287, // L1, instant, 120.0s CD (group 18), range 0, AOE 20 circle, targets=Self
    PeripheralSynthesis = 23286, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=Hostile
    MatraMagic = 23285, // L1, 2.0s cast, 120.0s CD (group 15/57), range 25, single-target, targets=Hostile
    ChocoMeteor = 23284, // L1, 2.0s cast, GCD, range 25, AOE 8 circle, targets=Hostile
    MaledictionOfWater = 23283, // L1, 2.0s cast, GCD, range 0, AOE 15+R width 6 rect, targets=Self
    HydroPull = 23282, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=Self
    AetherialSpark = 23281, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=Hostile
    DragonForce = 23280, // L1, 2.0s cast, 120.0s CD (group 15/57), range 0, single-target, targets=Self
    Schiltron = 34565, // L1, 2.0s cast, GCD, range 0, single-target, targets=Self
    Apokalypsis = 34581, // L1, instant, 120.0s CD (group 20), range 0, single-target, targets=Self
    BeingMortal = 34582, // L1, instant, 120.0s CD (group 20), range 0, AOE 10 circle, targets=Self
    FrogLegs = 18307, // L1, 1.0s cast, GCD, range 0, AOE 4 circle, targets=Self
    ToadOil = 11410, // L1, 2.0s cast, GCD, range 0, single-target, targets=Self
    Transfusion = 11409, // L1, 2.0s cast, GCD, range 25, single-target, targets=Party
    SelfDestruct = 11408, // L1, 2.0s cast, GCD, range 0, AOE 20 circle, targets=Self, animLock=1.600s?
    FinalSting = 11407, // L1, 2.0s cast, GCD, range 3, single-target, targets=Hostile
    WhiteWind = 11406, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=Self
    Missile = 11405, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    Glower = 11404, // L1, 2.0s cast, GCD, range 15, AOE 15+R width 3 rect, targets=Hostile
    Faze = 11403, // L1, 2.0s cast, GCD, range 0, AOE 4+R ?-degree cone, targets=Self
    FlameThrower = 11402, // L1, 2.0s cast, GCD, range 0, AOE 8+R ?-degree cone, targets=Self
    Loom = 11401, // L1, 1.0s cast, GCD, range 15, ???, targets=Area
    SharpenedKnife = 11400, // L1, 1.0s cast, GCD, range 3, single-target, targets=Hostile
    TheLook = 11399, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=Self
    DrillCannons = 11398, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 3 rect, targets=Hostile
    SonicBoom = 18308, // L1, 1.0s cast, GCD, range 25, single-target, targets=Hostile
    ThousandNeedles = 11397, // L1, 6.0s cast, GCD, range 0, AOE 4 circle, targets=Self
    BloodDrain = 11395, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    MindBlast = 11394, // L1, 1.0s cast, GCD, range 0, AOE 6 circle, targets=Self
    Bristle = 11393, // L1, 1.0s cast, GCD, range 0, single-target, targets=Self
    AcornBomb = 11392, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=Hostile
    Plaincracker = 11391, // L1, 2.0s cast, GCD, range 0, AOE 6 circle, targets=Self
    AquaBreath = 11390, // L1, 2.0s cast, GCD, range 0, AOE 8+R ?-degree cone, targets=Self
    FlyingFrenzy = 11389, // L1, 1.0s cast, GCD, range 20, AOE 6 circle, targets=Hostile
    BadBreath = 11388, // L1, 2.0s cast, GCD, range 0, AOE 8+R ?-degree cone, targets=Self
    HighVoltage = 11387, // L1, 2.0s cast, GCD, range 0, AOE 12 circle, targets=Self
    SongOfTorment = 11386, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    WaterCannon = 11385, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    FourTonzeWeight = 11384, // L1, 2.0s cast, GCD, range 25, AOE 4 circle, targets=Area
    Snort = 11383, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=Self
    BombToss = 11396, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=Area
    StickyTongue = 11412, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    OffGuard = 11411, // L1, 1.0s cast, 60.0s CD (group 4), range 25, single-target, targets=Hostile
    Level5Petrify = 11414, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=Self
    Avail = 18306, // L1, 1.0s cast, 120.0s CD (group 14/57), range 10, single-target, targets=Party
    MagicHammer = 18305, // L1, 1.0s cast, 90.0s CD (group 9/57), range 25, AOE 8 circle, targets=Hostile
    Gobskin = 18304, // L1, 2.0s cast, GCD, range 0, AOE 20 circle, targets=Self
    PomCure = 18303, // L1, 1.5s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    EerieSoundwave = 18302, // L1, 2.0s cast, GCD, range 0, AOE 6 circle, targets=Self
    Chirp = 18301, // L1, 2.0s cast, GCD, range 0, AOE 3 circle, targets=Self
    AbyssalTransfixion = 18300, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    Kaltstrahl = 18299, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=Self
    Electrogenesis = 18298, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=Hostile
    Northerlies = 18297, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=Self
    ProteanWave = 18296, // L1, 2.0s cast, GCD, range 0, AOE 15+R ?-degree cone, targets=Self
    AlpineDraft = 18295, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=Hostile
    VeilOfTheWhorl = 11431, // L1, instant, 90.0s CD (group 8), range 0, single-target, targets=Self
    TailScrew = 11413, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    ShockStrike = 11429, // L1, instant, 60.0s CD (group 5), range 25, AOE 3 circle, targets=Hostile
    GlassDance = 11430, // L1, instant, 90.0s CD (group 8), range 0, AOE 12+R ?-degree cone, targets=Self
    MountainBuster = 11428, // L1, instant, 60.0s CD (group 5), range 0, AOE 6+R ?-degree cone, targets=Self
    MoonFlute = 11415, // L1, 2.0s cast, GCD, range 0, single-target, targets=Self
    Doom = 11416, // L1, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    MightyGuard = 11417, // L1, 2.0s cast, GCD, range 0, single-target, targets=Self
    IceSpikes = 11418, // L1, 2.0s cast, GCD, range 0, single-target, targets=Self
    TheDragonsVoice = 11420, // L1, 2.0s cast, GCD, range 0, AOE -20 donut, targets=Self
    PeculiarLight = 11421, // L1, 1.0s cast, 60.0s CD (group 4), range 0, AOE 6 circle, targets=Self
    TheRamsVoice = 11419, // L1, 2.0s cast, GCD, range 0, AOE 6 circle, targets=Self
    FlyingSardine = 11423, // L1, instant, GCD, range 25, single-target, targets=Hostile
    Diamondback = 11424, // L1, 2.0s cast, GCD, range 0, single-target, targets=Self
    FireAngon = 11425, // L1, 1.0s cast, GCD, range 25, AOE 4 circle, targets=Hostile
    FeatherRain = 11426, // L1, instant, 30.0s CD (group 1), range 30, AOE 5 circle, targets=Area
    Eruption = 11427, // L1, instant, 30.0s CD (group 1), range 25, AOE 5 circle, targets=Area
    InkJet = 11422, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=Self

    // Shared
    Addle = ClassShared.AID.Addle, // L8, instant, 90.0s CD (group 46), range 25, single-target, targets=Hostile
    Sleep = ClassShared.AID.Sleep, // L10, 2.5s cast, GCD, range 30, AOE 5 circle, targets=Hostile
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 44), range 0, single-target, targets=Self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 43), range 0, single-target, targets=Self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    Learning = 219, // L1
    MaimAndMend1 = 220, // L10
    MaimAndMend2 = 221, // L20
    MaimAndMend3 = 222, // L30
    MaimAndMend4 = 223, // L40
    MaimAndMend5 = 224, // L50
}

public enum SID : uint
{
    None = 0,
    BasicInstinct = 2498, // applied by Basic Instinct to self
    SurpanakhasFury = 2130, // applied by Surpanakha to self
    HPBoost = 2120, // applied by Devour to self
    BreathOfMagic = 3712, // applied by Breath of Magic to target
    Bleeding = 1714, // applied by Nightbloom to target
    PhantomFlurry = 2502, // applied by Phantom Flurry to self
    Boost = 1716, // applied by Bristle to self
    DeepFreeze = 1731, // applied by the Ram's Voice to target
    Windburn = 1723, // applied by Feather Rain to target
    LucidDreaming = 1204, // applied by Lucid Dreaming to self
    Swiftcast = 167, // applied by Swiftcast to self
    SpickAndSpan = 3637,
    MightyGuard = 1719, // applied by Mighty Guard to self
    AethericMimicryTank = 2124,
    AethericMimicryDPS = 2125,
    AethericMimicryHealer = 2126,
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Whistle);
        d.RegisterSpell(AID.TheRoseOfDestruction);
        d.RegisterSpell(AID.DivineCataract);
        d.RegisterSpell(AID.ChelonianGate);
        d.RegisterSpell(AID.AngelsSnack);
        d.RegisterSpell(AID.FeculentFlood);
        d.RegisterSpell(AID.SaintlyBeam);
        d.RegisterSpell(AID.Stotram1);
        d.RegisterSpell(AID.WhiteDeath);
        d.RegisterSpell(AID.ColdFog);
        d.RegisterSpell(AID.TatamiGaeshi);
        d.RegisterSpell(AID.Tingle);
        d.RegisterSpell(AID.TripleTrident);
        d.RegisterSpell(AID.AethericMimicryReleaseHealer);
        d.RegisterSpell(AID.AethericMimicryReleaseDPS);
        d.RegisterSpell(AID.BasicInstinct);
        d.RegisterSpell(AID.AethericMimicryReleaseTank);
        d.RegisterSpell(AID.Quasar);
        d.RegisterSpell(AID.Surpanakha);
        d.RegisterSpell(AID.AethericMimicry);
        d.RegisterSpell(AID.CondensedLibra);
        d.RegisterSpell(AID.Devour);
        d.RegisterSpell(AID.Reflux);
        d.RegisterSpell(AID.Exuviation);
        d.RegisterSpell(AID.AngelWhisper);
        d.RegisterSpell(AID.RevengeBlast);
        d.RegisterSpell(AID.Cactguard);
        d.RegisterSpell(AID.PerpetualRay);
        d.RegisterSpell(AID.Launcher);
        d.RegisterSpell(AID.Level5Death);
        d.RegisterSpell(AID.BlackKnightsTour);
        d.RegisterSpell(AID.JKick, instantAnimLock: 0.90f); // animLock=0.900
        d.RegisterSpell(AID.WhiteKnightsTour);
        d.RegisterSpell(AID.Ultravibration);
        d.RegisterSpell(AID.MustardBomb);
        d.RegisterSpell(AID.SeaShanty);
        d.RegisterSpell(AID.MortalFlame);
        d.RegisterSpell(AID.CandyCane);
        d.RegisterSpell(AID.LaserEye);
        d.RegisterSpell(AID.WingedReprobation);
        d.RegisterSpell(AID.ForceField);
        d.RegisterSpell(AID.ConvictionMarcato);
        d.RegisterSpell(AID.DimensionalShift);
        d.RegisterSpell(AID.DivinationRune);
        d.RegisterSpell(AID.RubyDynamics);
        d.RegisterSpell(AID.DeepClean);
        d.RegisterSpell(AID.PeatPelt);
        d.RegisterSpell(AID.WildRage);
        d.RegisterSpell(AID.BreathOfMagic);
        d.RegisterSpell(AID.Blaze);
        d.RegisterSpell(AID.Rehydration);
        d.RegisterSpell(AID.RightRound);
        d.RegisterSpell(AID.GoblinPunch);
        d.RegisterSpell(AID.Stotram2);
        d.RegisterSpell(AID.Nightbloom);
        d.RegisterSpell(AID.PhantomFlurryEnd);
        d.RegisterSpell(AID.PhantomFlurry);
        d.RegisterSpell(AID.BothEnds);
        d.RegisterSpell(AID.PeripheralSynthesis);
        d.RegisterSpell(AID.MatraMagic);
        d.RegisterSpell(AID.ChocoMeteor);
        d.RegisterSpell(AID.MaledictionOfWater);
        d.RegisterSpell(AID.HydroPull);
        d.RegisterSpell(AID.AetherialSpark);
        d.RegisterSpell(AID.DragonForce);
        d.RegisterSpell(AID.Schiltron);
        d.RegisterSpell(AID.Apokalypsis);
        d.RegisterSpell(AID.BeingMortal);
        d.RegisterSpell(AID.FrogLegs);
        d.RegisterSpell(AID.ToadOil);
        d.RegisterSpell(AID.Transfusion);
        d.RegisterSpell(AID.SelfDestruct, instantAnimLock: 1.60f, castAnimLock: 1.60f); // animLock=1.600s?
        d.RegisterSpell(AID.FinalSting);
        d.RegisterSpell(AID.WhiteWind);
        d.RegisterSpell(AID.Missile);
        d.RegisterSpell(AID.Glower);
        d.RegisterSpell(AID.Faze);
        d.RegisterSpell(AID.FlameThrower);
        d.RegisterSpell(AID.Loom);
        d.RegisterSpell(AID.SharpenedKnife);
        d.RegisterSpell(AID.TheLook);
        d.RegisterSpell(AID.DrillCannons);
        d.RegisterSpell(AID.SonicBoom);
        d.RegisterSpell(AID.ThousandNeedles);
        d.RegisterSpell(AID.BloodDrain);
        d.RegisterSpell(AID.MindBlast);
        d.RegisterSpell(AID.Bristle);
        d.RegisterSpell(AID.AcornBomb);
        d.RegisterSpell(AID.Plaincracker);
        d.RegisterSpell(AID.AquaBreath);
        d.RegisterSpell(AID.FlyingFrenzy);
        d.RegisterSpell(AID.BadBreath);
        d.RegisterSpell(AID.HighVoltage);
        d.RegisterSpell(AID.SongOfTorment);
        d.RegisterSpell(AID.WaterCannon);
        d.RegisterSpell(AID.FourTonzeWeight);
        d.RegisterSpell(AID.Snort);
        d.RegisterSpell(AID.BombToss);
        d.RegisterSpell(AID.StickyTongue);
        d.RegisterSpell(AID.OffGuard);
        d.RegisterSpell(AID.Level5Petrify);
        d.RegisterSpell(AID.Avail);
        d.RegisterSpell(AID.MagicHammer);
        d.RegisterSpell(AID.Gobskin);
        d.RegisterSpell(AID.PomCure);
        d.RegisterSpell(AID.EerieSoundwave);
        d.RegisterSpell(AID.Chirp);
        d.RegisterSpell(AID.AbyssalTransfixion);
        d.RegisterSpell(AID.Kaltstrahl);
        d.RegisterSpell(AID.Electrogenesis);
        d.RegisterSpell(AID.Northerlies);
        d.RegisterSpell(AID.ProteanWave);
        d.RegisterSpell(AID.AlpineDraft);
        d.RegisterSpell(AID.VeilOfTheWhorl);
        d.RegisterSpell(AID.TailScrew);
        d.RegisterSpell(AID.ShockStrike);
        d.RegisterSpell(AID.GlassDance);
        d.RegisterSpell(AID.MountainBuster);
        d.RegisterSpell(AID.MoonFlute);
        d.RegisterSpell(AID.Doom);
        d.RegisterSpell(AID.MightyGuard);
        d.RegisterSpell(AID.IceSpikes);
        d.RegisterSpell(AID.TheDragonsVoice);
        d.RegisterSpell(AID.PeculiarLight);
        d.RegisterSpell(AID.TheRamsVoice);
        d.RegisterSpell(AID.FlyingSardine);
        d.RegisterSpell(AID.Diamondback);
        d.RegisterSpell(AID.FireAngon);
        d.RegisterSpell(AID.FeatherRain);
        d.RegisterSpell(AID.Eruption);
        d.RegisterSpell(AID.InkJet);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.Spell(AID.FlyingFrenzy)!.ForbidExecute = d.Spell(AID.JKick)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;

        d.Spell(AID.Loom)!.ForbidExecute = ActionDefinitions.DashToPositionCheck;
    }
}
