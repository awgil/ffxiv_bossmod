namespace BossMod.BLU;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Whistle = 18309, // L1, 1.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    TheRoseOfDestruction = 23275, // L1, 2.0s cast, 30.0s CD (group 16/57), range 25, single-target, targets=hostile, animLock=???
    DivineCataract = 23274, // L1, instant, GCD, range 0, AOE 10 circle, targets=self, animLock=???
    ChelonianGate = 23273, // L1, 2.0s cast, 30.0s CD (group 16/57), range 0, single-target, targets=self, animLock=???
    AngelsSnack = 23272, // L1, 2.0s cast, 120.0s CD (group 15/57), range 0, AOE 20 circle, targets=self, animLock=???
    FeculentFlood = 23271, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=hostile, animLock=???
    SaintlyBeam = 23270, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=hostile, animLock=???
    Stotram1 = 23269, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=self, animLock=???
    Stotram2 = 23416, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=self, animLock=???
    WhiteDeath = 23268, // L1, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    ColdFog = 23267, // L1, 2.0s cast, 90.0s CD (group 13/57), range 0, single-target, targets=self, animLock=???
    TatamiGaeshi = 23266, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 5 rect, targets=hostile, animLock=???
    Tingle = 23265, // L1, 2.0s cast, GCD, range 20, AOE 6 circle, targets=hostile, animLock=???
    TripleTrident = 23264, // L1, 2.0s cast, 90.0s CD (group 4/57), range 3, single-target, targets=hostile, animLock=???
    AethericMimicry1 = 18322, // L1, 1.0s cast, GCD, range 25, single-target, targets=party/alliance/friendly, animLock=???
    AethericMimicry2 = 19238, // L1, instant, GCD, range 0, single-target, targets=self, animLock=???
    AethericMimicry3 = 19239, // L1, instant, GCD, range 0, single-target, targets=self, animLock=???
    AethericMimicry4 = 19240, // L1, instant, GCD, range 0, single-target, targets=self, animLock=???
    BasicInstinct = 23276, // L1, 2.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    Quasar = 18324, // L1, instant, 60.0s CD (group 6), range 0, AOE 15 circle, targets=self, animLock=???
    Surpanakha = 18323, // L1, instant, 30.0s CD (group 5/70) (4? charges), range 0, AOE 16+R ?-degree cone, targets=self, animLock=???
    CondensedLibra = 18321, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Devour = 18320, // L1, 1.0s cast, 60.0s CD (group 11/57), range 3, single-target, targets=hostile, animLock=???
    Reflux = 18319, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Exuviation = 18318, // L1, 2.0s cast, GCD, range 0, AOE 6 circle, targets=self, animLock=???
    AngelWhisper = 18317, // L1, 10.0s cast, 300.0s CD (group 12/57), range 25, single-target, targets=party/alliance/friendly, animLock=???
    RevengeBlast = 18316, // L1, 2.0s cast, GCD, range 3, single-target, targets=hostile, animLock=???
    Cactguard = 18315, // L1, 1.0s cast, GCD, range 25, single-target, targets=party, animLock=???
    PerpetualRay = 18314, // L1, 3.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Launcher = 18313, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=self, animLock=???
    Level5Death = 18312, // L1, 2.0s cast, 180.0s CD (group 10/57), range 0, AOE 6 circle, targets=self, animLock=???
    BlackKnightsTour = 18311, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=hostile, animLock=???
    JKick = 18325, // L1, instant, 60.0s CD (group 6), range 25, AOE 6 circle, targets=hostile, animLock=???
    WhiteKnightsTour = 18310, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=hostile, animLock=???
    Ultravibration = 23277, // L1, 2.0s cast, 120.0s CD (group 10/57), range 0, AOE 6 circle, targets=self, animLock=???
    MustardBomb = 23279, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=hostile, animLock=???
    SeaShanty = 34580, // L1, instant, 120.0s CD (group 19), range 0, AOE 10 circle, targets=self, animLock=???
    MortalFlame = 34579, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    CandyCane = 34578, // L1, 1.0s cast, 90.0s CD (group 7/57), range 25, AOE 5 circle, targets=hostile, animLock=???
    LaserEye = 34577, // L1, 2.0s cast, GCD, range 25, AOE 8 circle, targets=hostile, animLock=???
    WingedReprobation = 34576, // L1, 1.0s cast, 90.0s CD (group 21/57), range 25, AOE 25+R width 5 rect, targets=hostile, animLock=???
    ForceField = 34575, // L1, 2.0s cast, 120.0s CD (group 22/57), range 0, single-target, targets=self, animLock=???
    ConvictionMarcato = 34574, // L1, 2.0s cast, GCD, range 25, AOE 25+R width 5 rect, targets=hostile, animLock=???
    DimensionalShift = 34573, // L1, 5.0s cast, GCD, range 0, AOE 10 circle, targets=self, animLock=???
    DivinationRune = 34572, // L1, 2.0s cast, GCD, range 0, AOE 15+R ?-degree cone, targets=self, animLock=???
    RubyDynamics = 34571, // L1, 2.0s cast, 30.0s CD (group 16/57), range 0, AOE 12+R ?-degree cone, targets=self, animLock=???
    DeepClean = 34570, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=hostile, animLock=???
    PeatPelt = 34569, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=hostile, animLock=???
    WildRage = 34568, // L1, 5.0s cast, GCD, range 0, AOE 10 circle, targets=self, animLock=???
    BreathOfMagic = 34567, // L1, 2.0s cast, GCD, range 0, AOE 10+R ?-degree cone, targets=self, animLock=???
    Blaze = 23278, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=hostile, animLock=???
    Rehydration = 34566, // L1, 5.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    RightRound = 34564, // L1, 2.0s cast, GCD, range 0, AOE 8 circle, targets=self, animLock=???
    GoblinPunch = 34563, // L1, instant, GCD, range 3, single-target, targets=hostile, animLock=???
    Nightbloom = 23290, // L1, instant, 120.0s CD (group 18), range 0, AOE 10 circle, targets=self, animLock=???
    PhantomFlurry1 = 23288, // L1, instant, 120.0s CD (group 17), range 0, AOE 8+R ?-degree cone, targets=self, animLock=???
    PhantomFlurry2 = 23289, // L1, instant, GCD, range 0, AOE 16+R ?-degree cone, targets=self, animLock=???
    BothEnds = 23287, // L1, instant, 120.0s CD (group 18), range 0, AOE 20 circle, targets=self, animLock=???
    PeripheralSynthesis = 23286, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=hostile, animLock=???
    MatraMagic = 23285, // L1, 2.0s cast, 120.0s CD (group 15/57), range 25, single-target, targets=hostile, animLock=???
    ChocoMeteor = 23284, // L1, 2.0s cast, GCD, range 25, AOE 8 circle, targets=hostile, animLock=???
    MaledictionOfWater = 23283, // L1, 2.0s cast, GCD, range 0, AOE 15+R width 6 rect, targets=self, animLock=???
    HydroPull = 23282, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=self, animLock=???
    AetherialSpark = 23281, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=hostile, animLock=???
    DragonForce = 23280, // L1, 2.0s cast, 120.0s CD (group 15/57), range 0, single-target, targets=self, animLock=???
    Schiltron = 34565, // L1, 2.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    Apokalypsis = 34581, // L1, instant, 120.0s CD (group 20), range 0, single-target, targets=self, animLock=???
    BeingMortal = 34582, // L1, instant, 120.0s CD (group 20), range 0, AOE 10 circle, targets=self, animLock=???
    FrogLegs = 18307, // L1, 1.0s cast, GCD, range 0, AOE 4 circle, targets=self, animLock=???
    ToadOil = 11410, // L1, 2.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    Transfusion = 11409, // L1, 2.0s cast, GCD, range 25, single-target, targets=party, animLock=???
    SelfDestruct = 11408, // L1, 2.0s cast, GCD, range 0, AOE 20 circle, targets=self, animLock=1.600, castAnimLock=1.600
    FinalSting = 11407, // L1, 2.0s cast, GCD, range 3, single-target, targets=hostile, animLock=???
    WhiteWind = 11406, // L1, 2.0s cast, GCD, range 0, AOE 15 circle, targets=self, animLock=???
    Missile = 11405, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Glower = 11404, // L1, 2.0s cast, GCD, range 15, AOE 15+R width 3 rect, targets=hostile, animLock=???
    Faze = 11403, // L1, 2.0s cast, GCD, range 0, AOE 4+R ?-degree cone, targets=self, animLock=???
    FlameThrower = 11402, // L1, 2.0s cast, GCD, range 0, AOE 8+R ?-degree cone, targets=self, animLock=???
    Loom = 11401, // L1, 1.0s cast, GCD, range 15, ???, targets=area, animLock=???
    SharpenedKnife = 11400, // L1, 1.0s cast, GCD, range 3, single-target, targets=hostile, animLock=???
    TheLook = 11399, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=self, animLock=???
    DrillCannons = 11398, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 3 rect, targets=hostile, animLock=???
    SonicBoom = 18308, // L1, 1.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    ThousandNeedles = 11397, // L1, 6.0s cast, GCD, range 0, AOE 4 circle, targets=self, animLock=???
    BloodDrain = 11395, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    MindBlast = 11394, // L1, 1.0s cast, GCD, range 0, AOE 6 circle, targets=self, animLock=???
    Bristle = 11393, // L1, 1.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    AcornBomb = 11392, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=hostile, animLock=???
    Plaincracker = 11391, // L1, 2.0s cast, GCD, range 0, AOE 6 circle, targets=self, animLock=???
    AquaBreath = 11390, // L1, 2.0s cast, GCD, range 0, AOE 8+R ?-degree cone, targets=self, animLock=???
    FlyingFrenzy = 11389, // L1, 1.0s cast, GCD, range 20, AOE 6 circle, targets=hostile, animLock=???
    BadBreath = 11388, // L1, 2.0s cast, GCD, range 0, AOE 8+R ?-degree cone, targets=self, animLock=???
    HighVoltage = 11387, // L1, 2.0s cast, GCD, range 0, AOE 12 circle, targets=self, animLock=???
    SongOfTorment = 11386, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    WaterCannon = 11385, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    FourTonzeWeight = 11384, // L1, 2.0s cast, GCD, range 25, AOE 4 circle, targets=area, animLock=???
    Snort = 11383, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=self, animLock=???
    BombToss = 11396, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=area, animLock=???
    StickyTongue = 11412, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    OffGuard = 11411, // L1, 1.0s cast, 60.0s CD (group 3), range 25, single-target, targets=hostile, animLock=???
    Level5Petrify = 11414, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=self, animLock=???
    Avail = 18306, // L1, 1.0s cast, 120.0s CD (group 8/57), range 10, single-target, targets=party, animLock=???
    MagicHammer = 18305, // L1, 1.0s cast, 90.0s CD (group 7/57), range 25, AOE 8 circle, targets=hostile, animLock=???
    Gobskin = 18304, // L1, 2.0s cast, GCD, range 0, AOE 20 circle, targets=self, animLock=???
    PomCure = 18303, // L1, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly, animLock=???
    EerieSoundwave = 18302, // L1, 2.0s cast, GCD, range 0, AOE 6 circle, targets=self, animLock=???
    Chirp = 18301, // L1, 2.0s cast, GCD, range 0, AOE 3 circle, targets=self, animLock=???
    AbyssalTransfixion = 18300, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Kaltstrahl = 18299, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=self, animLock=???
    Electrogenesis = 18298, // L1, 2.0s cast, GCD, range 25, AOE 6 circle, targets=hostile, animLock=???
    Northerlies = 18297, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=self, animLock=???
    ProteanWave = 18296, // L1, 2.0s cast, GCD, range 0, AOE 15+R ?-degree cone, targets=self, animLock=???
    AlpineDraft = 18295, // L1, 2.0s cast, GCD, range 20, AOE 20+R width 4 rect, targets=hostile, animLock=???
    VeilOfTheWhorl = 11431, // L1, instant, 90.0s CD (group 2), range 0, single-target, targets=self, animLock=???
    TailScrew = 11413, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    ShockStrike = 11429, // L1, instant, 60.0s CD (group 1), range 25, AOE 3 circle, targets=hostile, animLock=???
    GlassDance = 11430, // L1, instant, 90.0s CD (group 2), range 0, AOE 12+R ?-degree cone, targets=self, animLock=???
    MountainBuster = 11428, // L1, instant, 60.0s CD (group 1), range 0, AOE 6+R ?-degree cone, targets=self, animLock=???
    MoonFlute = 11415, // L1, 2.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    Doom = 11416, // L1, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    MightyGuard = 11417, // L1, 2.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    IceSpikes = 11418, // L1, 2.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    TheDragonsVoice = 11420, // L1, 2.0s cast, GCD, range 0, AOE ?-20 donut, targets=self, animLock=???
    PeculiarLight = 11421, // L1, 1.0s cast, 60.0s CD (group 3), range 0, AOE 6 circle, targets=self, animLock=???
    TheRamsVoice = 11419, // L1, 2.0s cast, GCD, range 0, AOE 6 circle, targets=self, animLock=???
    FlyingSardine = 11423, // L1, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    Diamondback = 11424, // L1, 2.0s cast, GCD, range 0, single-target, targets=self, animLock=???
    FireAngon = 11425, // L1, 1.0s cast, GCD, range 25, AOE 4 circle, targets=hostile, animLock=???
    FeatherRain = 11426, // L1, instant, 30.0s CD (group 0), range 30, AOE 5 circle, targets=area, animLock=???
    Eruption = 11427, // L1, instant, 30.0s CD (group 0), range 25, AOE 5 circle, targets=area, animLock=???
    InkJet = 11422, // L1, 2.0s cast, GCD, range 0, AOE 6+R ?-degree cone, targets=self, animLock=???

    // Shared
    Addle = ClassShared.AID.Addle, // L8, instant, 90.0s CD (group 46), range 25, single-target, targets=hostile
    Sleep = ClassShared.AID.Sleep, // L10, 2.5s cast, GCD, range 30, AOE 5 circle, targets=hostile
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 45), range 0, single-target, targets=self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 44), range 0, single-target, targets=self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=self
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

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Whistle); // animLock=???
        d.RegisterSpell(AID.TheRoseOfDestruction); // animLock=???
        d.RegisterSpell(AID.DivineCataract); // animLock=???
        d.RegisterSpell(AID.ChelonianGate); // animLock=???
        d.RegisterSpell(AID.AngelsSnack); // animLock=???
        d.RegisterSpell(AID.FeculentFlood); // animLock=???
        d.RegisterSpell(AID.SaintlyBeam); // animLock=???
        d.RegisterSpell(AID.Stotram1); // animLock=???
        d.RegisterSpell(AID.Stotram2); // animLock=???
        d.RegisterSpell(AID.WhiteDeath); // animLock=???
        d.RegisterSpell(AID.ColdFog); // animLock=???
        d.RegisterSpell(AID.TatamiGaeshi); // animLock=???
        d.RegisterSpell(AID.Tingle); // animLock=???
        d.RegisterSpell(AID.TripleTrident); // animLock=???
        d.RegisterSpell(AID.AethericMimicry1); // animLock=???
        d.RegisterSpell(AID.AethericMimicry2); // animLock=???
        d.RegisterSpell(AID.AethericMimicry3); // animLock=???
        d.RegisterSpell(AID.AethericMimicry4); // animLock=???
        d.RegisterSpell(AID.BasicInstinct); // animLock=???
        d.RegisterSpell(AID.Quasar); // animLock=???
        d.RegisterSpell(AID.Surpanakha, maxCharges: 4); // animLock=???
        d.RegisterSpell(AID.CondensedLibra); // animLock=???
        d.RegisterSpell(AID.Devour); // animLock=???
        d.RegisterSpell(AID.Reflux); // animLock=???
        d.RegisterSpell(AID.Exuviation); // animLock=???
        d.RegisterSpell(AID.AngelWhisper); // animLock=???
        d.RegisterSpell(AID.RevengeBlast); // animLock=???
        d.RegisterSpell(AID.Cactguard); // animLock=???
        d.RegisterSpell(AID.PerpetualRay); // animLock=???
        d.RegisterSpell(AID.Launcher); // animLock=???
        d.RegisterSpell(AID.Level5Death); // animLock=???
        d.RegisterSpell(AID.BlackKnightsTour); // animLock=???
        d.RegisterSpell(AID.JKick); // animLock=???
        d.RegisterSpell(AID.WhiteKnightsTour); // animLock=???
        d.RegisterSpell(AID.Ultravibration); // animLock=???
        d.RegisterSpell(AID.MustardBomb); // animLock=???
        d.RegisterSpell(AID.SeaShanty); // animLock=???
        d.RegisterSpell(AID.MortalFlame); // animLock=???
        d.RegisterSpell(AID.CandyCane); // animLock=???
        d.RegisterSpell(AID.LaserEye); // animLock=???
        d.RegisterSpell(AID.WingedReprobation); // animLock=???
        d.RegisterSpell(AID.ForceField); // animLock=???
        d.RegisterSpell(AID.ConvictionMarcato); // animLock=???
        d.RegisterSpell(AID.DimensionalShift); // animLock=???
        d.RegisterSpell(AID.DivinationRune); // animLock=???
        d.RegisterSpell(AID.RubyDynamics); // animLock=???
        d.RegisterSpell(AID.DeepClean); // animLock=???
        d.RegisterSpell(AID.PeatPelt); // animLock=???
        d.RegisterSpell(AID.WildRage); // animLock=???
        d.RegisterSpell(AID.BreathOfMagic); // animLock=???
        d.RegisterSpell(AID.Blaze); // animLock=???
        d.RegisterSpell(AID.Rehydration); // animLock=???
        d.RegisterSpell(AID.RightRound); // animLock=???
        d.RegisterSpell(AID.GoblinPunch); // animLock=???
        d.RegisterSpell(AID.Nightbloom); // animLock=???
        d.RegisterSpell(AID.PhantomFlurry1); // animLock=???
        d.RegisterSpell(AID.PhantomFlurry2); // animLock=???
        d.RegisterSpell(AID.BothEnds); // animLock=???
        d.RegisterSpell(AID.PeripheralSynthesis); // animLock=???
        d.RegisterSpell(AID.MatraMagic); // animLock=???
        d.RegisterSpell(AID.ChocoMeteor); // animLock=???
        d.RegisterSpell(AID.MaledictionOfWater); // animLock=???
        d.RegisterSpell(AID.HydroPull); // animLock=???
        d.RegisterSpell(AID.AetherialSpark); // animLock=???
        d.RegisterSpell(AID.DragonForce); // animLock=???
        d.RegisterSpell(AID.Schiltron); // animLock=???
        d.RegisterSpell(AID.Apokalypsis); // animLock=???
        d.RegisterSpell(AID.BeingMortal); // animLock=???
        d.RegisterSpell(AID.FrogLegs); // animLock=???
        d.RegisterSpell(AID.ToadOil); // animLock=???
        d.RegisterSpell(AID.Transfusion); // animLock=???
        d.RegisterSpell(AID.SelfDestruct, instantAnimLock: 1.60f, castAnimLock: 1.60f); // animLock=1.600, castAnimLock=1.600
        d.RegisterSpell(AID.FinalSting); // animLock=???
        d.RegisterSpell(AID.WhiteWind); // animLock=???
        d.RegisterSpell(AID.Missile); // animLock=???
        d.RegisterSpell(AID.Glower); // animLock=???
        d.RegisterSpell(AID.Faze); // animLock=???
        d.RegisterSpell(AID.FlameThrower); // animLock=???
        d.RegisterSpell(AID.Loom); // animLock=???
        d.RegisterSpell(AID.SharpenedKnife); // animLock=???
        d.RegisterSpell(AID.TheLook); // animLock=???
        d.RegisterSpell(AID.DrillCannons); // animLock=???
        d.RegisterSpell(AID.SonicBoom); // animLock=???
        d.RegisterSpell(AID.ThousandNeedles); // animLock=???
        d.RegisterSpell(AID.BloodDrain); // animLock=???
        d.RegisterSpell(AID.MindBlast); // animLock=???
        d.RegisterSpell(AID.Bristle); // animLock=???
        d.RegisterSpell(AID.AcornBomb); // animLock=???
        d.RegisterSpell(AID.Plaincracker); // animLock=???
        d.RegisterSpell(AID.AquaBreath); // animLock=???
        d.RegisterSpell(AID.FlyingFrenzy); // animLock=???
        d.RegisterSpell(AID.BadBreath); // animLock=???
        d.RegisterSpell(AID.HighVoltage); // animLock=???
        d.RegisterSpell(AID.SongOfTorment); // animLock=???
        d.RegisterSpell(AID.WaterCannon); // animLock=???
        d.RegisterSpell(AID.FourTonzeWeight); // animLock=???
        d.RegisterSpell(AID.Snort); // animLock=???
        d.RegisterSpell(AID.BombToss); // animLock=???
        d.RegisterSpell(AID.StickyTongue); // animLock=???
        d.RegisterSpell(AID.OffGuard); // animLock=???
        d.RegisterSpell(AID.Level5Petrify); // animLock=???
        d.RegisterSpell(AID.Avail); // animLock=???
        d.RegisterSpell(AID.MagicHammer); // animLock=???
        d.RegisterSpell(AID.Gobskin); // animLock=???
        d.RegisterSpell(AID.PomCure); // animLock=???
        d.RegisterSpell(AID.EerieSoundwave); // animLock=???
        d.RegisterSpell(AID.Chirp); // animLock=???
        d.RegisterSpell(AID.AbyssalTransfixion); // animLock=???
        d.RegisterSpell(AID.Kaltstrahl); // animLock=???
        d.RegisterSpell(AID.Electrogenesis); // animLock=???
        d.RegisterSpell(AID.Northerlies); // animLock=???
        d.RegisterSpell(AID.ProteanWave); // animLock=???
        d.RegisterSpell(AID.AlpineDraft); // animLock=???
        d.RegisterSpell(AID.VeilOfTheWhorl); // animLock=???
        d.RegisterSpell(AID.TailScrew); // animLock=???
        d.RegisterSpell(AID.ShockStrike); // animLock=???
        d.RegisterSpell(AID.GlassDance); // animLock=???
        d.RegisterSpell(AID.MountainBuster); // animLock=???
        d.RegisterSpell(AID.MoonFlute); // animLock=???
        d.RegisterSpell(AID.Doom); // animLock=???
        d.RegisterSpell(AID.MightyGuard); // animLock=???
        d.RegisterSpell(AID.IceSpikes); // animLock=???
        d.RegisterSpell(AID.TheDragonsVoice); // animLock=???
        d.RegisterSpell(AID.PeculiarLight); // animLock=???
        d.RegisterSpell(AID.TheRamsVoice); // animLock=???
        d.RegisterSpell(AID.FlyingSardine); // animLock=???
        d.RegisterSpell(AID.Diamondback); // animLock=???
        d.RegisterSpell(AID.FireAngon); // animLock=???
        d.RegisterSpell(AID.FeatherRain); // animLock=???
        d.RegisterSpell(AID.Eruption); // animLock=???
        d.RegisterSpell(AID.InkJet); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}
