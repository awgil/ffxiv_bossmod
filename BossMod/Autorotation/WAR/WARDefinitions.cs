namespace BossMod.WAR
{
    public enum AID : uint
    {
        None = 0,

        // single target GCDs
        HeavySwing = 31, // L1, instant, range 3, single-target 0/0, targets=hostile
        Maim = 37, // L4, instant, range 3, single-target 0/0, targets=hostile
        StormPath = 42, // L26, instant, range 3, single-target 0/0, targets=hostile
        StormEye = 45, // L50, instant, range 3, single-target 0/0, targets=hostile
        InnerBeast = 49, // L35, instant, range 3, single-target 0/0, targets=hostile
        FellCleave = 3549, // L54, instant, range 3, single-target 0/0, targets=hostile
        InnerChaos = 16465, // L80, instant, range 3, single-target 0/0, targets=hostile
        PrimalRend = 25753, // L90, instant, range 20, AOE circle 5/0, targets=hostile, animLock=1.15s

        // aoe GCDs
        Overpower = 41, // L10, instant, range 0, AOE circle 5/0, targets=self
        MythrilTempest = 16462, // L40, instant, range 0, AOE circle 5/0, targets=self
        SteelCyclone = 51, // L45, instant, range 0, AOE circle 5/0, targets=self
        Decimate = 3550, // L60, instant, range 0, AOE circle 5/0, targets=self
        ChaoticCyclone = 16463, // L72, instant, range 0, AOE circle 5/0, targets=self

        // oGCDs
        Infuriate = 52, // L50, instant, 60.0s CD (group 19) (2 charges), range 0, single-target 0/0, targets=self
        Onslaught = 7386, // L62, instant, 30.0s CD (group 9) (3 charges), range 20, single-target 0/0, targets=hostile
        Upheaval = 7387, // L64, instant, 30.0s CD (group 5), range 3, single-target 0/0, targets=hostile
        Orogeny = 25752, // L86, instant, 30.0s CD (group 5), range 0, AOE circle 5/0, targets=self

        // offsensive CDs
        Berserk = 38, // L6, instant, 60.0s CD (group 10), range 0, single-target 0/0, targets=self
        InnerRelease = 7389, // L70, instant, 60.0s CD (group 11), range 0, single-target 0/0, targets=self

        // defensive CDs
        Rampart = 7531, // L8, instant, 90.0s CD (group 40), range 0, single-target 0/0, targets=self
        Vengeance = 44, // L38, instant, 120.0s CD (group 21), range 0, single-target 0/0, targets=self
        ThrillOfBattle = 40, // L30, instant, 90.0s CD (group 16), range 0, single-target 0/0, targets=self
        Holmgang = 43, // L42, instant, 240.0s CD (group 23), range 6, single-target 0/0, targets=self/hostile
        Equilibrium = 3552, // L58, instant, 60.0s CD (group 14), range 0, single-target 0/0, targets=self
        Reprisal = 7535, // L22, instant, 60.0s CD (group 43), range 0, AOE circle 5/0, targets=self
        ShakeItOff = 7388, // L68, instant, 90.0s CD (group 17), range 0, AOE circle 15/0, targets=self
        RawIntuition = 3551, // L56, instant, 25.0s CD (group 3), range 0, single-target 0/0, targets=self
        NascentFlash = 16464, // L76, instant, 25.0s CD (group 3), range 30, single-target 0/0, targets=party
        Bloodwhetting = 25751, // L82, instant, 25.0s CD (group 3), range 0, single-target 0/0, targets=self
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self

        // misc
        Tomahawk = 46, // L15, instant, range 20, single-target 0/0, targets=hostile
        Defiance = 48, // L10, instant, 3.0s CD (group 2), range 0, single-target 0/0, targets=self
        Provoke = 7533, // L15, instant, 30.0s CD (group 42), range 25, single-target 0/0, targets=hostile
        Shirk = 7537, // L48, instant, 120.0s CD (group 45), range 25, single-target 0/0, targets=party
        LowBlow = 7540, // L12, instant, 25.0s CD (group 41), range 3, single-target 0/0, targets=hostile
        Interject = 7538, // L18, instant, 30.0s CD (group 44), range 3, single-target 0/0, targets=hostile
    }

    public enum CDGroup : int
    {
        Defiance = 2, // 3.0 max
        Bloodwhetting = 3, // 25.0 max, shared by Raw Intuition, Nascent Flash, Bloodwhetting
        Upheaval = 5, // 30.0 max, shared by Upheaval, Orogeny
        Onslaught = 9, // 3*30.0 max
        Berserk = 10, // 60.0 max
        InnerRelease = 11, // 60.0 max
        Equilibrium = 14, // 60.0 max
        ThrillOfBattle = 16, // 90.0 max
        ShakeItOff = 17, // 90.0 max
        Infuriate = 19, // 2*60.0 max
        Vengeance = 21, // 120.0 max
        Holmgang = 23, // 240.0 max
        Rampart = 40, // 90.0 max
        LowBlow = 41, // 25.0 max
        Provoke = 42, // 30.0 max
        Reprisal = 43, // 60.0 max
        Interject = 44, // 30.0 max
        Shirk = 45, // 120.0 max
        ArmsLength = 46, // 120.0 max
    }

    public enum MinLevel : int
    {
        Maim = 4,
        Berserk = 6,
        Rampart = 8,
        Overpower = 10,
        Defiance = 10,
        LowBlow = 12,
        Tomahawk = 15, // unlocked by quest 65852 'Brutal Strength'
        Provoke = 15,
        Interject = 18,
        Reprisal = 22,
        StormPath = 26,
        ThrillOfBattle = 30, // unlocked by quest 65855 'Bringing Down the Mountain'
        ArmsLength = 32,
        InnerBeast = 35, // unlocked by quest 66586 'Embracing the Beast'
        Vengeance = 38,
        MythrilTempest = 40, // unlocked by quest 66587 'Curious Gorge Goes to Wineport'
        Holmgang = 42,
        SteelCyclone = 45, // unlocked by quest 66589 'Proof Is the Pudding'
        Shirk = 48,
        Infuriate = 50, // unlocked by quest 66590 'How to Quit You'
        StormEye = 50,
        FellCleave = 54, // unlocked by quest 66124 'The Bear Necessity'
        RawIntuition = 56, // unlocked by quest 66132 'Pirates of Shallow Water'
        Equilibrium = 58, // unlocked by quest 66134 'Slap an' Chop'
        Decimate = 60, // unlocked by quest 66137 'And My Axe'
        Onslaught = 62,
        Upheaval = 64,
        EnhancedInfuriate = 66, // passive, gauge spenders reduce infCD by 5
        ShakeItOff = 68,
        InnerRelease = 70, // unlocked by quest 68440 'The Heart of the Problem'
        ChaoticCyclone = 72,
        MasteringTheBeast = 74, // passive, mythril tempest gives gauge
        NascentFlash = 76,
        InnerChaos = 80,
        Bloodwhetting = 82,
        Orogeny = 86,
        EnhancedOnslaught = 88, // passive, gives third charge to onslaught
        PrimalRend = 90,
    }

    public static class QuestLock
    {
        public static (int Level, uint QuestID)[] QuestsPerLevel = {
            (15, 65852),
            (30, 65855),
            (35, 66586),
            (40, 66587),
            (45, 66589),
            (50, 66590),
            (54, 66124),
            (56, 66132),
            (58, 66134),
            (60, 66137),
            (70, 68440),
        };
    }

    public enum SID : uint
    {
        None = 0,
        SurgingTempest = 2677, // applied by Storm's Eye, Mythril Tempest to self, damage buff
        NascentChaos = 1897, // applied by Infuriate to self, converts next FC to IC
        Berserk = 86, // applied by Berserk to self, next 3 GCDs are crit dhit
        InnerRelease = 1177, // applied by Inner Release to self, next 3 GCDs should be free FCs
        PrimalRend = 2624, // applied by Inner Release to self, allows casting PR
        InnerStrength = 2663, // applied by Inner Release to self, immunes
        VengeanceRetaliation = 89, // applied by Vengeance to self, retaliation for physical attacks
        VengeanceDefense = 912, // applied by Vengeance to self, -30% damage taken
        Rampart = 1191, // applied by Rampart to self, -20% damage taken
        ThrillOfBattle = 87, // applied by Thrill of Battle to self
        Holmgang = 409, // applied by Holmgang to self
        EquilibriumRegen = 2681, // applied by Equilibrium to self, hp regen
        Reprisal = 1193, // applied by Reprisal to target
        ShakeItOff = 1457, // applied by ShakeItOff to self/target, damage shield
        RawIntuition = 735, // applied by Raw Intuition to self
        NascentFlashSelf = 1857, // applied by Nascent Flash to self, heal on hit
        NascentFlashTarget = 1858, // applied by Nascent Flash to target, -10% damage taken + heal on hit
        BloodwhettingDefenseLong = 2678, // applied by Bloodwhetting to self, -10% damage taken + heal on hit for 8 sec
        BloodwhettingDefenseShort = 2679, // applied by Bloodwhetting, Nascent Flash to self/target, -10% damage taken for 4 sec
        BloodwhettingShield = 2680, // applied by Bloodwhetting, Nascent Flash to self/target, damage shield
        ArmsLength = 1209, // applied by Arm's Length to self
        Defiance = 91, // applied by Defiance to self, tank stance
        Stun = 2, // applied by Low Blow to target
    }
}
