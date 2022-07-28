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
        PrimalRend = 25753, // L90, instant, range 20, AOE circle 5/0, targets=hostile

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

    public enum SID : uint
    {
        None = 0,
        SurgingTempest = 2677, // applied by StormEye, damage buff
        NascentChaos = 1897, // applied by Infuriate, converts next FC to IC
        Berserk = 86, // applied by Berserk, next 3 GCDs are crit dhit
        InnerRelease = 1177, // applied by InnerRelease, next 3 GCDs should be free FCs
        PrimalRend = 2624, // applied by InnerRelease, allows casting PR
        InnerStrength = 2663, // applied by InnerRelease, immunes
        VengeanceRetaliation = 89, // applied by Vengeance, retaliation for physical attacks
        VengeanceDefense = 912, // applied by Vengeance, -30% damage taken
        Rampart = 1191, // applied by Rampart, -20% damage taken
        ThrillOfBattle = 87,
        Holmgang = 409,
        EquilibriumRegen = 2681, // applied by Equilibrium, hp regen
        // TODO: reprisal (debuff on enemy)
        ShakeItOff = 1457, // applied by ShakeItOff, damage shield
        // TODO: raw intuition
        NascentFlashSelf = 1857, // applied by NascentFlash to self, heal on hit
        NascentFlashTarget = 1858, // applied by NascentFlash to target, -10% damage taken + heal on hit
        BloodwhettingDefenseLong = 2678, // applied by Bloodwhetting, -10% damage taken + heal on hit for 8 sec
        BloodwhettingDefenseShort = 2679, // applied by Bloodwhetting/NascentFlash, -10% damage taken for 4 sec
        BloodwhettingShield = 2680, // applied by Bloodwhetting/NascentFlash, damage shield
        ArmsLength = 1209,
        Defiance = 91, // applied by Defiance, tank stance
    }
}
