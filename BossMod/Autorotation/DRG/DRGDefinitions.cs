using System.Collections.Generic;

namespace BossMod.DRG
{
    public enum AID : uint
    {
        None = 0,

        // single target GCDs
        TrueThrust = 75, // L1, instant, range 3, single-target 0/0, targets=hostile
        VorpalThrust = 78, // L4, instant, range 3, single-target 0/0, targets=hostile
        Disembowel = 87, // L18, instant, range 3, single-target 0/0, targets=hostile
        FullThrust = 84, // L26, instant, range 3, single-target 0/0, targets=hostile
        HeavensThrust = 25771, // L86, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        ChaosThrust = 88, // L50, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        ChaoticSpring = 25772, // L86, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        FangAndClaw = 3554, // L56, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        WheelingThrust = 3556, // L58, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        RaidenThrust = 16479, // L76, instant, range 3, single-target 0/0, targets=hostile, animLock=???

        // aoe GCDs
        DoomSpike = 86, // L40, instant, range 10, AOE rect 10/4, targets=hostile
        SonicThrust = 7397, // L62, instant, range 10, AOE rect 10/4, targets=hostile, animLock=???
        CoerthanTorment = 16477, // L72, instant, range 10, AOE rect 10/4, targets=hostile, animLock=???
        DraconianFury = 25770, // L82, instant, range 10, AOE rect 10/4, targets=hostile, animLock=???

        // oGCDs
        Jump = 92, // L30, instant, 30.0s CD (group 4), range 20, single-target 0/0, targets=hostile, animLock=0.800s
        HighJump = 16478, // L74, instant, 30.0s CD (group 8), range 20, single-target 0/0, targets=hostile, animLock=???
        SpineshatterDive = 95, // L45, instant, 60.0s CD (group 19), range 20, single-target 0/0, targets=hostile, animLock=???
        DragonfireDive = 96, // L50, instant, 120.0s CD (group 20), range 20, AOE circle 5/0, targets=hostile, animLock=???
        Geirskogul = 3555, // L60, instant, 30.0s CD (group 7), range 15, AOE rect 15/4, targets=hostile, animLock=???
        Nastrond = 7400, // L70, instant, 10.0s CD (group 2), range 15, AOE rect 15/4, targets=hostile, animLock=???
        MirageDive = 7399, // L68, instant, 1.0s CD (group 0), range 20, single-target 0/0, targets=hostile, animLock=???
        Stardiver = 16480, // L80, instant, 30.0s CD (group 6), range 20, AOE circle 5/0, targets=hostile, animLock=???
        WyrmwindThrust = 25773, // L90, instant, 10.0s CD (group 1), range 15, AOE rect 15/4, targets=hostile, animLock=???

        // offsensive CDs
        LifeSurge = 83, // L6, instant, 45.0s CD (group 14), range 0, single-target 0/0, targets=self
        LanceCharge = 85, // L30, instant, 60.0s CD (group 9), range 0, single-target 0/0, targets=self
        BattleLitany = 3557, // L52, instant, 120.0s CD (group 23), range 0, AOE circle 15/0, targets=self, animLock=???
        DragonSight = 7398, // L66, instant, 120.0s CD (group 21), range 30, single-target 0/0, targets=self/party, animLock=???

        // defensive CDs
        SecondWind = 7541, // L8, instant, 120.0s CD (group 40), range 0, single-target 0/0, targets=self
        Bloodbath = 7542, // L12, instant, 90.0s CD (group 42), range 0, single-target 0/0, targets=self
        Feint = 7549, // L22, instant, 90.0s CD (group 43), range 10, single-target 0/0, targets=hostile
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self

        // misc
        PiercingTalon = 90, // L15, instant, range 20, single-target 0/0, targets=hostile
        ElusiveJump = 94, // L35, instant, 30.0s CD (group 5), range 0, single-target 0/0, targets=self, animLock=0.800s
        TrueNorth = 7546, // L50, instant, 45.0s CD (group 44) (2 charges), range 0, single-target 0/0, targets=self, animLock=???
        LegSweep = 7863, // L10, instant, 40.0s CD (group 41), range 3, single-target 0/0, targets=hostile
    }

    public enum CDGroup : int
    {
        MirageDive = 0, // 1.0 max
        WyrmwindThrust = 1, // 10.0 max
        Nastrond = 2, // 10.0 max
        Jump = 4, // 30.0 max
        ElusiveJump = 5, // 30.0 max
        Stardiver = 6, // 30.0 max
        Geirskogul = 7, // 30.0 max
        HighJump = 8, // 30.0 max
        LanceCharge = 9, // 60.0 max
        LifeSurge = 14, // 2*45.0 max
        SpineshatterDive = 19, // 2*60.0 max
        DragonfireDive = 20, // 120.0 max
        DragonSight = 21, // 120.0 max
        BattleLitany = 23, // 120.0 max
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
        VorpalThrust = 4,
        LifeSurge = 6,
        SecondWind = 8,
        LegSweep = 10,
        Bloodbath = 12,
        PiercingTalon = 15, // unlocked by quest 65591 'A Dangerous Proposition'
        Disembowel = 18,
        Feint = 22,
        FullThrust = 26,
        LanceCharge = 30, // unlocked by quest 65975 'Proof of Might'
        Jump = 30, // unlocked by quest 66603 'Eye of the Dragon'
        ArmsLength = 32,
        ElusiveJump = 35, // unlocked by quest 66604 'Lance of Fury'
        DoomSpike = 40, // unlocked by quest 66605 'Unfading Skies'
        SpineshatterDive = 45, // unlocked by quest 66607 'Fatal Seduction'
        ChaosThrust = 50,
        TrueNorth = 50,
        DragonfireDive = 50, // unlocked by quest 66608 'Into the Dragon's Maw'
        BattleLitany = 52, // unlocked by quest 67226 'Days of Azure'
        FangAndClaw = 56, // unlocked by quest 67229 'Dragoon's Errand'
        WheelingThrust = 58, // unlocked by quest 67230 'Sanguine Dragoon'
        Geirskogul = 60, // unlocked by quest 67231 'Dragoon's Fate'
        SonicThrust = 62,
        DragonSight = 66,
        MirageDive = 68,
        Nastrond = 70, // unlocked by quest 68450 'Dragon Sound'
        CoerthanTorment = 72,
        HighJump = 74,
        RaidenThrust = 76,
        Stardiver = 80,
        DraconianFury = 82,
        ChaoticSpring = 86,
        HeavensThrust = 86,
        WyrmwindThrust = 90,

        // traits
        BloodOfTheDragon = 54, // unlocked by quest 67228 'Sworn Upon a Lance', passive, potency increase
        LanceMastery1 = 64, // passive
        LanceMastery2 = 76, // passive
        LifeOfTheDragonMastery = 78,
        EnhancedCoerthanTorment = 82,
        EnhancedSpineshatterDive = 84,
        EnhancedLifeSurge = 88, // passive, adds lifesurge charge
        LanceMastery4 = 90, // passive, potency increase
    }

    public static class Definitions
    {
        public static QuestLockEntry[] QuestsPerLevel = {
            new(15, 65591),
            new(30, 65975),
            new(30, 66603),
            new(35, 66604),
            new(40, 66605),
            new(45, 66607),
            new(50, 66608),
            new(52, 67226),
            new(54, 67228),
            new(56, 67229),
            new(58, 67230),
            new(60, 67231),
            new(70, 68450),
        };

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;
        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
            SupportedActions.GCD(AID.TrueThrust, 3);
            SupportedActions.GCD(AID.VorpalThrust, 3);
            SupportedActions.GCD(AID.Disembowel, 3);
            SupportedActions.GCD(AID.FullThrust, 3);
            SupportedActions.GCD(AID.ChaosThrust, 3);
            SupportedActions.GCD(AID.FangAndClaw, 3);
            SupportedActions.GCD(AID.WheelingThrust, 3);
            SupportedActions.GCD(AID.RaidenThrust, 3);
            SupportedActions.GCD(AID.HeavensThrust, 3);
            SupportedActions.GCD(AID.ChaoticSpring, 3);
            SupportedActions.GCD(AID.DoomSpike, 10);
            SupportedActions.GCD(AID.SonicThrust, 10);
            SupportedActions.GCD(AID.CoerthanTorment, 10);
            SupportedActions.GCD(AID.DraconianFury, 10);
            SupportedActions.OGCD(AID.Jump, 20, CDGroup.Jump, 30.0f, 0.800f);
            SupportedActions.OGCD(AID.HighJump, 20, CDGroup.HighJump, 30.0f, 0.800f);
            SupportedActions.OGCDWithCharges(AID.SpineshatterDive, 20, CDGroup.SpineshatterDive, 60.0f, 2);
            SupportedActions.OGCD(AID.DragonfireDive, 20, CDGroup.DragonfireDive, 120.0f);
            SupportedActions.OGCD(AID.Geirskogul, 15, CDGroup.Geirskogul, 30.0f);
            SupportedActions.OGCD(AID.Nastrond, 15, CDGroup.Nastrond, 10.0f);
            SupportedActions.OGCD(AID.MirageDive, 20, CDGroup.MirageDive, 1.0f);
            SupportedActions.OGCD(AID.Stardiver, 20, CDGroup.Stardiver, 30.0f);
            SupportedActions.OGCD(AID.WyrmwindThrust, 15, CDGroup.WyrmwindThrust, 10.0f);
            SupportedActions.OGCDWithCharges(AID.LifeSurge, 0, CDGroup.LifeSurge, 45.0f, 2);
            SupportedActions.OGCD(AID.LanceCharge, 0, CDGroup.LanceCharge, 60.0f);
            SupportedActions.OGCD(AID.BattleLitany, 0, CDGroup.BattleLitany, 120.0f);
            SupportedActions.OGCD(AID.DragonSight, 30, CDGroup.DragonSight, 120.0f);
            SupportedActions.OGCD(AID.SecondWind, 0, CDGroup.SecondWind, 120.0f);
            SupportedActions.OGCD(AID.Bloodbath, 0, CDGroup.Bloodbath, 90.0f);
            SupportedActions.OGCD(AID.Feint, 10, CDGroup.Feint, 90.0f);
            SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f);
            SupportedActions.GCD(AID.PiercingTalon, 20);
            SupportedActions.OGCD(AID.ElusiveJump, 0, CDGroup.ElusiveJump, 30.0f);
            SupportedActions.OGCDWithCharges(AID.TrueNorth, 0, CDGroup.TrueNorth, 45.0f, 2);
            SupportedActions.OGCD(AID.LegSweep, 3, CDGroup.LegSweep, 40.0f);
        }
    }

    public enum SID : uint
    {
        None = 0,
        LifeSurge = 116, // applied by Life Surge to self, forced crit for next gcd
        PowerSurge = 2720, // applied by Disembowel to self, damage buff
        Bloodbath = 84, // applied by Bloodbath to self, lifesteal
        Feint = 1195, // applied by Feint to target, -10% phys and -5% magic damage dealt
        Stun = 2, // applied by Leg Sweep to target
    }
}
