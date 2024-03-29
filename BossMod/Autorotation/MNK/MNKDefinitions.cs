namespace BossMod.MNK;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

    // single target GCDs
    Bootshine = 53, // L1, instant, range 3, single-target 0/0, targets=hostile
    DragonKick = 74, // L50, instant, range 3, single-target 0/0, targets=hostile
    TrueStrike = 54, // L4, instant, range 3, single-target 0/0, targets=hostile
    TwinSnakes = 61, // L18, instant, range 3, single-target 0/0, targets=hostile
    SnapPunch = 56, // L6, instant, range 3, single-target 0/0, targets=hostile
    Demolish = 66, // L30, instant, range 3, single-target 0/0, targets=hostile
    SixSidedStar = 16476, // L80, instant, range 3, single-target 0/0, targets=hostile, animLock=???

    // aoe GCDs
    ArmOfTheDestroyer = 62, // L26, instant, range 0, AOE circle 5/0, targets=self
    ShadowOfTheDestroyer = 25767, // L82, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    FourPointFury = 16473, // L45, instant, range 0, AOE circle 5/0, targets=self
    Rockbreaker = 70, // L30, instant, range 0, AOE circle 5/0, targets=self

    // masterful blitz variants
    MasterfulBlitz = 25764, // L60, instant, range 0, single-target 0/0, targets=self, animLock=???
    ElixirField = 3545, // L60, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    FlintStrike = 25882, // L60, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    RisingPhoenix = 25768, // L86, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    CelestialRevolution = 25765, // L60, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    TornadoKick = 3543, // L60, instant, range 3, AOE circle 5/0, targets=hostile, animLock=???
    PhantomRush = 25769, // L90, instant, range 3, AOE circle 5/0, targets=hostile, animLock=???

    // oGCDs
    SteelPeak = 25761, // L15, instant, 1.0s CD (group 0), range 3, single-target 0/0, targets=hostile
    ForbiddenChakra = 3547, // L54, instant, 1.0s CD (group 0), range 3, single-target 0/0, targets=hostile, animLock=???
    HowlingFist = 25763, // L40, instant, 1.0s CD (group 0), range 10, AOE rect 10/2, targets=hostile
    Enlightenment = 16474, // L74, instant, 1.0s CD (group 0), range 10, AOE rect 10/4, targets=hostile, animLock=???

    // offsensive CDs
    PerfectBalance = 69, // L50, instant, 40.0s CD (group 10) (2 charges), range 0, single-target 0/0, targets=self
    RiddleOfFire = 7395, // L68, instant, 60.0s CD (group 11), range 0, single-target 0/0, targets=self, animLock=???
    Brotherhood = 7396, // L70, instant, 120.0s CD (group 19), range 0, AOE circle 15/0, targets=self, animLock=???
    RiddleOfWind = 25766, // L72, instant, 90.0s CD (group 16), range 0, single-target 0/0, targets=self, animLock=???

    // defensive CDs
    SecondWind = 7541, // L8, instant, 120.0s CD (group 40), range 0, single-target 0/0, targets=self
    Mantra = 65, // L42, instant, 90.0s CD (group 15), range 0, AOE circle 15/0, targets=self
    RiddleOfEarth = 7394, // L64, instant, 30.0s CD (group 14) (3 charges), range 0, single-target 0/0, targets=self, animLock=???
    Bloodbath = 7542, // L12, instant, 90.0s CD (group 42), range 0, single-target 0/0, targets=self
    Feint = 7549, // L22, instant, 90.0s CD (group 43), range 10, single-target 0/0, targets=hostile
    ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self

    // misc
    Meditation = 3546, // L15, instant, range 0, single-target 0/0, targets=self
    TrueNorth = 7546, // L50, instant, 45.0s CD (group 44) (2 charges), range 0, single-target 0/0, targets=self
    Thunderclap = 25762, // L35, instant, 30.0s CD (group 9) (2 charges), range 20, single-target 0/0, targets=party/hostile
    FormShift = 4262, // L52, instant, range 0, single-target 0/0, targets=self, animLock=???
    Anatman = 16475, // L78, instant, 60.0s CD (group 12), range 0, single-target 0/0, targets=self, animLock=???
    LegSweep = 7863, // L10, instant, 40.0s CD (group 41), range 3, single-target 0/0, targets=hostile
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

public enum CDGroup : int
{
    SteelPeak = 0, // 1.0 max, shared by Steel Peak, Howling Fist, the Forbidden Chakra, Enlightenment
    Thunderclap = 9, // 2*30.0 max
    PerfectBalance = 10, // 2*40.0 max
    RiddleOfFire = 11, // 60.0 max
    Anatman = 12, // 60.0 max
    RiddleOfEarth = 14, // 120.0 max
    Mantra = 15, // 90.0 max
    RiddleOfWind = 16, // 90.0 max
    Brotherhood = 19, // 120.0 max
    LegSweep = 41, // 40.0 max
    TrueNorth = 45, // 2*45.0 max
    Bloodbath = 46, // 90.0 max
    Feint = 47, // 90.0 max
    ArmsLength = 48, // 120.0 max
    SecondWind = 49, // 120.0 max
}

public enum SID : uint
{
    None = 0,
    OpoOpoForm = 107, // applied by Snap Punch to self
    RaptorForm = 108, // applied by Bootshine, Arm of the Destroyer to self
    CoeurlForm = 109, // applied by True Strike, Twin Snakes to self
    RiddleOfFire = 1181, // applied by Riddle of Fire to self
    RiddleOfWind = 2687, // applied by Riddle of Wind to self
    LeadenFist = 1861, // applied by Dragon Kick to self
    DisciplinedFist = 3001, // applied by Twin Snakes to self, damage buff
    PerfectBalance = 110, // applied by Perfect Balance to self, ignore form requirements
    Demolish = 246, // applied by Demolish to target, dot
    Bloodbath = 84, // applied by Bloodbath to self, lifesteal
    Feint = 1195, // applied by Feint to target, -10% phys and -5% magic damage dealt
    Mantra = 102, // applied by Mantra to targets, +10% healing taken
    TrueNorth = 1250, // applied by True North to self, ignore positionals
    Stun = 2, // applied by Leg Sweep to target
    FormlessFist = 2513, // applied by Form Shift to self
    SixSidedStar = 2514, // applied by Six-Sided Star to self
}

public static class Definitions
{
    public static uint[] UnlockQuests = { 66094, 66103, 66597, 66598, 66599, 66600, 66602, 67563, 67564, 67567, 67966 };

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
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
            AID.PerfectBalance => level >= 50 && questProgress > 6,
            AID.DragonKick => level >= 50,
            AID.FormShift => level >= 52 && questProgress > 7,
            AID.ForbiddenChakra => level >= 54 && questProgress > 8,
            AID.TornadoKick => level >= 60 && questProgress > 9,
            AID.CelestialRevolution => level >= 60 && questProgress > 9,
            AID.MasterfulBlitz => level >= 60 && questProgress > 9,
            AID.FlintStrike => level >= 60 && questProgress > 9,
            AID.ElixirField => level >= 60 && questProgress > 9,
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
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
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
            _ => true,
        };
    }

    public static Dictionary<ActionID, ActionDefinition> SupportedActions;
    static Definitions()
    {
        SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
        SupportedActions.GCD(AID.Bootshine, 3);
        SupportedActions.GCD(AID.DragonKick, 3);
        SupportedActions.GCD(AID.TrueStrike, 3);
        SupportedActions.GCD(AID.TwinSnakes, 3);
        SupportedActions.GCD(AID.SnapPunch, 3);
        SupportedActions.GCD(AID.Demolish, 3);
        SupportedActions.GCD(AID.SixSidedStar, 3);
        SupportedActions.GCD(AID.ArmOfTheDestroyer, 0);
        SupportedActions.GCD(AID.ShadowOfTheDestroyer, 0);
        SupportedActions.GCD(AID.FourPointFury, 0);
        SupportedActions.GCD(AID.Rockbreaker, 0);
        SupportedActions.GCD(AID.MasterfulBlitz, 0);
        SupportedActions.GCD(AID.ElixirField, 0);
        SupportedActions.GCD(AID.FlintStrike, 0);
        SupportedActions.GCD(AID.RisingPhoenix, 0);
        SupportedActions.GCD(AID.CelestialRevolution, 3);
        SupportedActions.GCD(AID.TornadoKick, 3);
        SupportedActions.GCD(AID.PhantomRush, 3);
        SupportedActions.OGCD(AID.SteelPeak, 3, CDGroup.SteelPeak, 1.0f);
        SupportedActions.OGCD(AID.ForbiddenChakra, 3, CDGroup.SteelPeak, 1.0f);
        SupportedActions.OGCD(AID.HowlingFist, 10, CDGroup.SteelPeak, 1.0f);
        SupportedActions.OGCD(AID.Enlightenment, 10, CDGroup.SteelPeak, 1.0f);
        SupportedActions.OGCDWithCharges(AID.PerfectBalance, 0, CDGroup.PerfectBalance, 40.0f, 2);
        SupportedActions.OGCD(AID.RiddleOfFire, 0, CDGroup.RiddleOfFire, 60.0f);
        SupportedActions.OGCD(AID.Brotherhood, 0, CDGroup.Brotherhood, 120.0f);
        SupportedActions.OGCD(AID.RiddleOfWind, 0, CDGroup.RiddleOfWind, 90.0f);
        SupportedActions.OGCD(AID.SecondWind, 0, CDGroup.SecondWind, 120.0f);
        SupportedActions.OGCD(AID.Mantra, 0, CDGroup.Mantra, 90.0f).EffectDuration = 15;
        SupportedActions.OGCD(AID.RiddleOfEarth, 0, CDGroup.RiddleOfEarth, 120.0f).EffectDuration = 10;
        SupportedActions.OGCD(AID.Bloodbath, 0, CDGroup.Bloodbath, 90.0f);
        SupportedActions.OGCD(AID.Feint, 10, CDGroup.Feint, 90.0f).EffectDuration = 10;
        SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
        SupportedActions.GCD(AID.Meditation, 0);
        SupportedActions.OGCDWithCharges(AID.TrueNorth, 0, CDGroup.TrueNorth, 45.0f, 2);
        SupportedActions.OGCDWithCharges(AID.Thunderclap, 20, CDGroup.Thunderclap, 30.0f, 3);
        SupportedActions.GCD(AID.FormShift, 0);
        SupportedActions.OGCD(AID.Anatman, 0, CDGroup.Anatman, 60.0f);
        SupportedActions.OGCD(AID.LegSweep, 3, CDGroup.LegSweep, 40.0f);
    }
}
