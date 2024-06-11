namespace BossMod.MNK;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    FinalHeaven = 202, // LB3, 4.5s cast, range 8, single-target, targets=hostile, animLock=???, castAnimLock=3.700
    Bootshine = 53, // L1, instant, GCD, range 3, single-target, targets=hostile
    TrueStrike = 54, // L4, instant, GCD, range 3, single-target, targets=hostile
    SnapPunch = 56, // L6, instant, GCD, range 3, single-target, targets=hostile
    SteelPeak = 25761, // L15, instant, 1.0s CD (group 0), range 3, single-target, targets=hostile
    Meditation = 3546, // L15, instant, GCD, range 0, single-target, targets=self
    TwinSnakes = 61, // L18, instant, GCD, range 3, single-target, targets=hostile
    ArmOfTheDestroyer = 62, // L26, instant, GCD, range 0, AOE 5 circle, targets=self
    Demolish = 66, // L30, instant, GCD, range 3, single-target, targets=hostile
    Rockbreaker = 70, // L30, instant, GCD, range 0, AOE 5 circle, targets=self
    Thunderclap = 25762, // L35, instant, 30.0s CD (group 9/72) (2? charges), range 20, single-target, targets=party/hostile
    HowlingFist = 25763, // L40, instant, 1.0s CD (group 0), range 10, AOE 10+R width 2 rect, targets=hostile
    Mantra = 65, // L42, instant, 90.0s CD (group 15), range 0, AOE 30 circle, targets=self
    FourPointFury = 16473, // L45, instant, GCD, range 0, AOE 5 circle, targets=self
    DragonKick = 74, // L50, instant, GCD, range 3, single-target, targets=hostile
    PerfectBalance = 69, // L50, instant, 40.0s CD (group 10/70) (2? charges), range 0, single-target, targets=self
    FormShift = 4262, // L52, instant, GCD, range 0, single-target, targets=self
    ForbiddenChakra = 3547, // L54, instant, 1.0s CD (group 0), range 3, single-target, targets=hostile
    ElixirField = 3545, // L60, instant, GCD, range 0, AOE 5 circle, targets=self
    MasterfulBlitz = 25764, // L60, instant, GCD, range 0, single-target, targets=self, animLock=???
    CelestialRevolution = 25765, // L60, instant, GCD, range 3, single-target, targets=hostile
    FlintStrike = 25882, // L60, instant, GCD, range 0, AOE 5 circle, targets=self, animLock=???
    TornadoKick = 3543, // L60, instant, GCD, range 3, AOE 5 circle, targets=hostile, animLock=???
    RiddleOfEarth = 7394, // L64, instant, 120.0s CD (group 14), range 0, single-target, targets=self
    RiddleOfFire = 7395, // L68, instant, 60.0s CD (group 11), range 0, single-target, targets=self
    Brotherhood = 7396, // L70, instant, 120.0s CD (group 19), range 0, AOE 30 circle, targets=self
    RiddleOfWind = 25766, // L72, instant, 90.0s CD (group 16), range 0, single-target, targets=self
    Enlightenment = 16474, // L74, instant, 1.0s CD (group 0), range 10, AOE 10+R width 4 rect, targets=hostile
    Anatman = 16475, // L78, instant, 60.0s CD (group 12/57), range 0, single-target, targets=self
    SixSidedStar = 16476, // L80, instant, GCD, range 3, single-target, targets=hostile
    ShadowOfTheDestroyer = 25767, // L82, instant, GCD, range 0, AOE 5 circle, targets=self
    RisingPhoenix = 25768, // L86, instant, GCD, range 0, AOE 5 circle, targets=self
    PhantomRush = 25769, // L90, instant, GCD, range 3, AOE 5 circle, targets=hostile

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
    Bladedance = ClassShared.AID.Bladedance, // LB2, 3.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=self
    LegSweep = ClassShared.AID.LegSweep, // L10, instant, 40.0s CD (group 41), range 3, single-target, targets=hostile
    Bloodbath = ClassShared.AID.Bloodbath, // L12, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    Feint = ClassShared.AID.Feint, // L22, instant, 90.0s CD (group 47), range 10, single-target, targets=hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    TrueNorth = ClassShared.AID.TrueNorth, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=self
}

public enum TraitID : uint
{
    None = 0,
    GreasedLightning = 364, // L1, haste
    EnhancedGreasedLightning1 = 365, // L20, haste
    DeepMeditation1 = 160, // L38, chakra proc on crit
    EnhancedGreasedLightning2 = 366, // L40, haste
    SteelPeakMastery = 428, // L54, steel peek -> forbidden chakra upgrade
    EnhancedPerfectBalance = 433, // L60
    DeepMeditation2 = 245, // L74, chakra on crit
    HowlingFistMastery = 429, // L74, howling fist -> enlightenment upgrade
    EnhancedGreasedLightning3 = 367, // L76, haste and buff effect increase
    ArmOfTheDestroyerMastery = 430, // L82, arm of the destroyer -> shadow of the destroyer
    EnhancedThunderclap = 431, // L84, third charge
    MeleeMastery = 518, // L84, potency increase
    FlintStrikeMastery = 512, // L86, flint strike -> rising phoenix upgrade
    EnhancedBrotherhood = 432, // L88, gives chakra for each gcd under brotherhood buff
    TornadoKickMastery = 513, // L90, tornado kick -> phantom rush upgrade
}

public sealed class Definitions : IDisposable
{
    public static readonly uint[] UnlockQuests = [66094, 66103, 66597, 66598, 66599, 66600, 66602, 67563, 67564, 67567, 67966];

    public static bool Unlocked(AID id, int level, int questProgress) => id switch
    {
        AID.TrueStrike => level >= 4,
        AID.SnapPunch => level >= 6,
        AID.SecondWind => level >= 8,
        AID.LegSweep => level >= 10,
        AID.Bloodbath => level >= 12,
        AID.SteelPeak => level >= 15 && questProgress > 0,
        AID.Meditation => level >= 15 && questProgress > 0,
        AID.TwinSnakes => level >= 18,
        AID.Feint => level >= 22,
        AID.ArmOfTheDestroyer => level >= 26,
        AID.Demolish => level >= 30 && questProgress > 1,
        AID.Rockbreaker => level >= 30 && questProgress > 2,
        AID.ArmsLength => level >= 32,
        AID.Thunderclap => level >= 35 && questProgress > 3,
        AID.HowlingFist => level >= 40 && questProgress > 4,
        AID.Mantra => level >= 42,
        AID.FourPointFury => level >= 45 && questProgress > 5,
        AID.TrueNorth => level >= 50,
        AID.DragonKick => level >= 50,
        AID.PerfectBalance => level >= 50 && questProgress > 6,
        AID.FormShift => level >= 52 && questProgress > 7,
        AID.ForbiddenChakra => level >= 54 && questProgress > 8,
        AID.ElixirField => level >= 60 && questProgress > 9,
        AID.MasterfulBlitz => level >= 60 && questProgress > 9,
        AID.CelestialRevolution => level >= 60 && questProgress > 9,
        AID.FlintStrike => level >= 60 && questProgress > 9,
        AID.TornadoKick => level >= 60 && questProgress > 9,
        AID.RiddleOfEarth => level >= 64,
        AID.RiddleOfFire => level >= 68,
        AID.Brotherhood => level >= 70 && questProgress > 10,
        AID.RiddleOfWind => level >= 72,
        AID.Enlightenment => level >= 74,
        AID.Anatman => level >= 78,
        AID.SixSidedStar => level >= 80,
        AID.ShadowOfTheDestroyer => level >= 82,
        AID.RisingPhoenix => level >= 86,
        AID.PhantomRush => level >= 90,
        _ => true
    };

    public static bool Unlocked(TraitID id, int level, int questProgress) => id switch
    {
        TraitID.EnhancedGreasedLightning1 => level >= 20,
        TraitID.DeepMeditation1 => level >= 38,
        TraitID.EnhancedGreasedLightning2 => level >= 40,
        TraitID.SteelPeakMastery => level >= 54 && questProgress > 8,
        TraitID.EnhancedPerfectBalance => level >= 60 && questProgress > 9,
        TraitID.DeepMeditation2 => level >= 74,
        TraitID.HowlingFistMastery => level >= 74,
        TraitID.EnhancedGreasedLightning3 => level >= 76,
        TraitID.ArmOfTheDestroyerMastery => level >= 82,
        TraitID.EnhancedThunderclap => level >= 84,
        TraitID.MeleeMastery => level >= 84,
        TraitID.FlintStrikeMastery => level >= 86,
        TraitID.EnhancedBrotherhood => level >= 88,
        TraitID.TornadoKickMastery => level >= 90,
        _ => true
    };

    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.FinalHeaven, castAnimLock: 3.70f);
        d.RegisterSpell(AID.Bootshine);
        d.RegisterSpell(AID.TrueStrike);
        d.RegisterSpell(AID.SnapPunch);
        d.RegisterSpell(AID.SteelPeak);
        d.RegisterSpell(AID.Meditation);
        d.RegisterSpell(AID.TwinSnakes);
        d.RegisterSpell(AID.ArmOfTheDestroyer);
        d.RegisterSpell(AID.Demolish);
        d.RegisterSpell(AID.Rockbreaker);
        d.RegisterSpell(AID.Thunderclap, maxCharges: 3);
        d.RegisterSpell(AID.HowlingFist);
        d.RegisterSpell(AID.Mantra);
        d.RegisterSpell(AID.FourPointFury);
        d.RegisterSpell(AID.DragonKick);
        d.RegisterSpell(AID.PerfectBalance, maxCharges: 2);
        d.RegisterSpell(AID.FormShift);
        d.RegisterSpell(AID.ForbiddenChakra);
        d.RegisterSpell(AID.ElixirField);
        d.RegisterSpell(AID.MasterfulBlitz); // animLock=???
        d.RegisterSpell(AID.CelestialRevolution);
        d.RegisterSpell(AID.FlintStrike); // animLock=???
        d.RegisterSpell(AID.TornadoKick); // animLock=???
        d.RegisterSpell(AID.RiddleOfEarth);
        d.RegisterSpell(AID.RiddleOfFire);
        d.RegisterSpell(AID.Brotherhood);
        d.RegisterSpell(AID.RiddleOfWind);
        d.RegisterSpell(AID.Enlightenment);
        d.RegisterSpell(AID.Anatman);
        d.RegisterSpell(AID.SixSidedStar);
        d.RegisterSpell(AID.ShadowOfTheDestroyer);
        d.RegisterSpell(AID.RisingPhoenix);
        d.RegisterSpell(AID.PhantomRush);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.Spell(AID.Mantra)!.EffectDuration = 15;
        d.Spell(AID.RiddleOfEarth)!.EffectDuration = 10;
    }
}
