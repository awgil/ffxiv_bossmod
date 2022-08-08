using System.Collections.Generic;

namespace BossMod.MNK
{
    public enum AID : uint
    {
        None = 0,

        // single target GCDs
        Bootshine = 53, // L1, instant, range 3, single-target 0/0, targets=hostile
        DragonKick = 74, // L50, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        TrueStrike = 54, // L4, instant, range 3, single-target 0/0, targets=hostile
        TwinSnakes = 61, // L18, instant, range 3, single-target 0/0, targets=hostile
        SnapPunch = 56, // L6, instant, range 3, single-target 0/0, targets=hostile
        Demolish = 66, // L30, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        SixSidedStar = 16476, // L80, instant, range 3, single-target 0/0, targets=hostile, animLock=???

        // aoe GCDs
        ArmOfTheDestroyer = 62, // L26, instant, range 0, AOE circle 5/0, targets=self
        ShadowOfTheDestroyer = 25767, // L82, instant, range 0, AOE circle 5/0, targets=self, animLock=???
        FourPointFury = 16473, // L45, instant, range 0, AOE circle 5/0, targets=self, animLock=???
        Rockbreaker = 70, // L30, instant, range 0, AOE circle 5/0, targets=self, animLock=???

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
        HowlingFist = 25763, // L40, instant, 1.0s CD (group 0), range 10, AOE rect 10/2, targets=hostile, animLock=???
        Enlightenment = 16474, // L74, instant, 1.0s CD (group 0), range 10, AOE rect 10/4, targets=hostile, animLock=???

        // offsensive CDs
        PerfectBalance = 69, // L50, instant, 40.0s CD (group 10) (2 charges), range 0, single-target 0/0, targets=self, animLock=???
        RiddleOfFire = 7395, // L68, instant, 60.0s CD (group 11), range 0, single-target 0/0, targets=self, animLock=???
        Brotherhood = 7396, // L70, instant, 120.0s CD (group 19), range 0, AOE circle 15/0, targets=self, animLock=???
        RiddleOfWind = 25766, // L72, instant, 90.0s CD (group 16), range 0, single-target 0/0, targets=self, animLock=???

        // defensive CDs
        SecondWind = 7541, // L8, instant, 120.0s CD (group 40), range 0, single-target 0/0, targets=self
        Mantra = 65, // L42, instant, 90.0s CD (group 15), range 0, AOE circle 15/0, targets=self, animLock=???
        RiddleOfEarth = 7394, // L64, instant, 30.0s CD (group 14) (3 charges), range 0, single-target 0/0, targets=self, animLock=???
        Bloodbath = 7542, // L12, instant, 90.0s CD (group 42), range 0, single-target 0/0, targets=self
        Feint = 7549, // L22, instant, 90.0s CD (group 43), range 10, single-target 0/0, targets=hostile
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self, animLock=???

        // misc
        Meditation = 3546, // L15, instant, range 0, single-target 0/0, targets=self
        TrueNorth = 7546, // L50, instant, 45.0s CD (group 44) (2 charges), range 0, single-target 0/0, targets=self, animLock=???
        Thunderclap = 25762, // L35, instant, 30.0s CD (group 9) (2 charges), range 20, single-target 0/0, targets=party/hostile, animLock=???
        FormShift = 4262, // L52, instant, range 0, single-target 0/0, targets=self, animLock=???
        Anatman = 16475, // L78, instant, 60.0s CD (group 12), range 0, single-target 0/0, targets=self, animLock=???
        LegSweep = 7863, // L10, instant, 40.0s CD (group 41), range 3, single-target 0/0, targets=hostile
    }

    public enum CDGroup : int
    {
        SteelPeak = 0, // 1.0 max, shared by Steel Peak, Howling Fist, Forbidden Chakra, Enlightenment
        Thunderclap = 9, // 2*30.0 max
        PerfectBalance = 10, // 2*40.0 max
        RiddleOfFire = 11, // 60.0 max
        Anatman = 12, // 60.0 max
        RiddleOfEarth = 14, // 3*30.0 max
        Mantra = 15, // 90.0 max
        RiddleOfWind = 16, // 90.0 max
        Brotherhood = 19, // 120.0 max
        SecondWind = 40, // 120.0 max
        LegSweep = 41, // 40.0 max
        Bloodbath = 42, // 90.0 max
        Feint = 43, // 90.0 max
        TrueNorth = 44, // 2*45.0 max
        ArmsLength = 46, // 120.0 max
    }

    public enum MinLevel : int
    {
        // actions
        TrueStrike = 4,
        SnapPunch = 6,
        SecondWind = 8,
        LegSweep = 10,
        Bloodbath = 12,
        SteelPeak = 15, // unlocked by quest 66094 'The Spirit Is Willing', includes meditation
        TwinSnakes = 18,
        Feint = 22,
        ArmOfTheDestroyer = 26,
        Demolish = 30, // unlocked by quest 66103 'Return of the Holyfist'
        Rockbreaker = 30, // unlocked by quest 66597 'Brother from Another Mother'
        ArmsLength = 32,
        Thunderclap = 35, // unlocked by quest 66598 'Insulted Intelligence'
        HowlingFist = 40, // unlocked by quest 66599 'A Slave to the Aether'
        Mantra = 42,
        FourPointFury = 45, // unlocked by quest 66600 'The Pursuit of Power'
        TrueNorth = 50,
        DragonKick = 50,
        PerfectBalance = 50, // unlocked by quest 66602 'Five Easy Pieces'
        FormShift = 52, // unlocked by quest 67563 'Let's Talk about Sects'
        ForbiddenChakra = 54, // unlocked by quest 67564 'Against the Shadow'
        MasterfulBlitz = 60, // unlocked by quest 67567 'Appetite for Destruction', includes 4 basic chakra abilities
        RiddleOfEarth = 64,
        RiddleOfFire = 68,
        Brotherhood = 70, // unlocked by quest 67966 'The Power to Protect'
        RiddleOfWind = 72,
        Enlightenment = 74,
        Anatman = 78,
        SixSidedStar = 80,
        ShadowOfTheDestroyer = 82,
        RisingPhoenix = 86,
        PhantomRush = 90,

        // traits
        EnhancedGreasedLightning1 = 20, // passive, haste
        DeepMeditation1 = 38, // passive, 80% to open chakra on crit
        EnhancedGreasedLightning2 = 40, // passive, haste
        DeepMeditation2 = 74, // passive, open chakra on crit
        EnhancedGreasedLightning3 = 76, // passive, haste
        EnhancedThunderclap = 84, // passive, 3rd thunderclap charge
        MeleeMastery = 84, // passive, potency increase
        EnhancedBrotherhood = 88, // passive, gives chakra for each GCD under Brotherhood buff
    }

    public static class Definitions
    {
        public static QuestLockEntry[] QuestsPerLevel = {
            new(15, 66094),
            new(30, 66103),
            new(30, 66597),
            new(35, 66598),
            new(40, 66599),
            new(45, 66600),
            new(50, 66602),
            new(52, 67563),
            new(54, 67564),
            new(60, 67567),
            new(70, 67966),
        };

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
            SupportedActions.OGCD(AID.Mantra, 0, CDGroup.Mantra, 90.0f);
            SupportedActions.OGCDWithCharges(AID.RiddleOfEarth, 0, CDGroup.RiddleOfEarth, 30.0f, 3);
            SupportedActions.OGCD(AID.Bloodbath, 0, CDGroup.Bloodbath, 90.0f);
            SupportedActions.OGCD(AID.Feint, 10, CDGroup.Feint, 90.0f);
            SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f);
            SupportedActions.GCD(AID.Meditation, 0);
            SupportedActions.OGCDWithCharges(AID.TrueNorth, 0, CDGroup.TrueNorth, 45.0f, 2);
            SupportedActions.OGCDWithCharges(AID.Thunderclap, 20, CDGroup.Thunderclap, 30.0f, 2);
            SupportedActions.GCD(AID.FormShift, 0);
            SupportedActions.OGCD(AID.Anatman, 0, CDGroup.Anatman, 60.0f);
            SupportedActions.OGCD(AID.LegSweep, 3, CDGroup.LegSweep, 40.0f);
        }
    }

    public enum SID : uint
    {
        None = 0,
        OpoOpoForm = 107, // applied by Snap Punch to self
        RaptorForm = 108, // applied by Bootshine, Arm of the Destroyer to self
        CoeurlForm = 109, // applied by True Strike, Twin Snakes to self
        DisciplinedFist = 3001, // applied by Twin Snakes to self, damage buff
        Bloodbath = 84, // applied by Bloodbath to self, lifesteal
        Feint = 1195, // applied by Feint to target, -10% phys and -5% magic damage dealt
        Stun = 2, // applied by Leg Sweep to target
    }
}
