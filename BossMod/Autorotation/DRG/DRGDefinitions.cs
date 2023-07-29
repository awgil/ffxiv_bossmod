using System.Collections.Generic;

namespace BossMod.DRG
{
    public enum AID : uint
    {
        None = 0,
        Sprint = 3,

        // single target GCDs
        TrueThrust = 75, // L1, instant, range 3, single-target 0/0, targets=hostile
        VorpalThrust = 78, // L4, instant, range 3, single-target 0/0, targets=hostile
        Disembowel = 87, // L18, instant, range 3, single-target 0/0, targets=hostile
        FullThrust = 84, // L26, instant, range 3, single-target 0/0, targets=hostile
        HeavensThrust = 25771, // L86, instant, range 3, single-target 0/0, targets=hostile
        ChaosThrust = 88, // L50, instant, range 3, single-target 0/0, targets=hostile
        ChaoticSpring = 25772, // L86, instant, range 3, single-target 0/0, targets=hostile
        FangAndClaw = 3554, // L56, instant, range 3, single-target 0/0, targets=hostile
        WheelingThrust = 3556, // L58, instant, range 3, single-target 0/0, targets=hostile
        RaidenThrust = 16479, // L76, instant, range 3, single-target 0/0, targets=hostile

        // aoe GCDs
        DoomSpike = 86, // L40, instant, range 10, AOE rect 10/4, targets=hostile
        SonicThrust = 7397, // L62, instant, range 10, AOE rect 10/4, targets=hostile
        CoerthanTorment = 16477, // L72, instant, range 10, AOE rect 10/4, targets=hostile
        DraconianFury = 25770, // L82, instant, range 10, AOE rect 10/4, targets=hostile

        // oGCDs
        Jump = 92, // L30, instant, 30.0s CD (group 4), range 20, single-target 0/0, targets=hostile, animLock=0.800s
        HighJump = 16478, // L74, instant, 30.0s CD (group 8), range 20, single-target 0/0, targets=hostile, animLock=0.800s
        SpineshatterDive = 95, // L45, instant, 60.0s CD (group 19), range 20, single-target 0/0, targets=hostile, animLock=0.800s
        DragonfireDive = 96, // L50, instant, 120.0s CD (group 20), range 20, AOE circle 5/0, targets=hostile, animLock=0.800s
        Geirskogul = 3555, // L60, instant, 30.0s CD (group 7), range 15, AOE rect 15/4, targets=hostile
        Nastrond = 7400, // L70, instant, 10.0s CD (group 2), range 15, AOE rect 15/4, targets=hostile
        MirageDive = 7399, // L68, instant, 1.0s CD (group 0), range 20, single-target 0/0, targets=hostile
        Stardiver = 16480, // L80, instant, 30.0s CD (group 6), range 20, AOE circle 5/0, targets=hostile, animLock=1.500s
        WyrmwindThrust = 25773, // L90, instant, 10.0s CD (group 1), range 15, AOE rect 15/4, targets=hostile, animLock=???

        // offsensive CDs
        LifeSurge = 83, // L6, instant, 45.0s CD (group 14), range 0, single-target 0/0, targets=self
        LanceCharge = 85, // L30, instant, 60.0s CD (group 9), range 0, single-target 0/0, targets=self
        BattleLitany = 3557, // L52, instant, 120.0s CD (group 23), range 0, AOE circle 15/0, targets=self
        DragonSight = 7398, // L66, instant, 120.0s CD (group 21), range 30, single-target 0/0, targets=self/party

        // defensive CDs
        SecondWind = 7541, // L8, instant, 120.0s CD (group 40), range 0, single-target 0/0, targets=self
        Bloodbath = 7542, // L12, instant, 90.0s CD (group 42), range 0, single-target 0/0, targets=self
        Feint = 7549, // L22, instant, 90.0s CD (group 43), range 10, single-target 0/0, targets=hostile
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self

        // misc
        PiercingTalon = 90, // L15, instant, range 20, single-target 0/0, targets=hostile
        ElusiveJump = 94, // L35, instant, 30.0s CD (group 5), range 0, single-target 0/0, targets=self, animLock=0.800s
        TrueNorth = 7546, // L50, instant, 45.0s CD (group 44) (2 charges), range 0, single-target 0/0, targets=self
        LegSweep = 7863, // L10, instant, 40.0s CD (group 41), range 3, single-target 0/0, targets=hostile
    }

    public enum TraitID : uint
    {
        None = 0,
        BloodOfTheDragon = 434, // L54, jump potency increase
        LanceMastery1 = 162, // L64
        LifeOfTheDragon = 163, // L70
        JumpMastery = 275, // L74, jump -> high jump upgrade
        LanceMastery2 = 247, // L76
        LifeOfTheDragonMastery = 276, // L78, duration increase
        EnhancedCoerthanTorment = 435, // L82
        EnhancedSpineshatterDive = 436, // L84, second charge
        LanceMastery3 = 437, // L86, full thrust -> heavens thrust, chaos thrust -> chaos spring upgrade
        EnhancedLifeSurge = 438, // L88, second charge
        LanceMastery4 = 508, // L90, potency increase
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

    public enum SID : uint
    {
        None = 0,
        LanceCharge = 1864, // applied by Lance Charge to self, damage buff
        LifeSurge = 116, // applied by Life Surge to self, forced crit for next gcd
        BattleLitany = 786, // applied by Battle Litany to self
        PowerSurge = 2720, // applied by Disembowel & Sonic Thrust to self, damage buff
        ChaosThrust = 118, // applied by Chaos Thrust to target, dot
        ChaoticSpring = 2719, // applied by Chaotic Spring to target, dot
        FangAndClawBared = 802, // applied by Full Thrust to self
        WheelInMotion = 803, // applied by Chaos Thrust to self
        DraconianFire = 1863, // applied by Fang and Claw, Wheeling Thrust to self
        RightEye = 1910, // applied by Dragon Sight to self
        DiveReady = 1243, // applied by Jump to self
        TrueNorth = 1250, // applied by True North to self, ignore positionals
        Bloodbath = 84, // applied by Bloodbath to self, lifesteal
        Feint = 1195, // applied by Feint to target, -10% phys and -5% magic damage dealt
        Stun = 2, // applied by Leg Sweep to target
    }

    public static class Definitions
    {
        public static uint[] UnlockQuests = { 65591, 65975, 66603, 66604, 66605, 66607, 66608, 67226, 67228, 67229, 67230, 67231, 68450 };

        public static bool Unlocked(AID aid, int level, int questProgress)
        {
            return aid switch
            {
                AID.VorpalThrust => level >= 4,
                AID.LifeSurge => level >= 6,
                AID.SecondWind => level >= 8,
                AID.LegSweep => level >= 10,
                AID.Bloodbath => level >= 12,
                AID.PiercingTalon => level >= 15 && questProgress > 0,
                AID.Disembowel => level >= 18,
                AID.Feint => level >= 22,
                AID.FullThrust => level >= 26,
                AID.Jump => level >= 30 && questProgress > 2,
                AID.LanceCharge => level >= 30 && questProgress > 1,
                AID.ArmsLength => level >= 32,
                AID.ElusiveJump => level >= 35 && questProgress > 3,
                AID.DoomSpike => level >= 40 && questProgress > 4,
                AID.SpineshatterDive => level >= 45 && questProgress > 5,
                AID.DragonfireDive => level >= 50 && questProgress > 6,
                AID.ChaosThrust => level >= 50,
                AID.TrueNorth => level >= 50,
                AID.BattleLitany => level >= 52 && questProgress > 7,
                AID.FangAndClaw => level >= 56 && questProgress > 9,
                AID.WheelingThrust => level >= 58 && questProgress > 10,
                AID.Geirskogul => level >= 60 && questProgress > 11,
                AID.SonicThrust => level >= 62,
                AID.DragonSight => level >= 66,
                AID.MirageDive => level >= 68,
                AID.Nastrond => level >= 70 && questProgress > 12,
                AID.CoerthanTorment => level >= 72,
                AID.HighJump => level >= 74,
                AID.RaidenThrust => level >= 76,
                AID.Stardiver => level >= 80,
                AID.DraconianFury => level >= 82,
                AID.ChaoticSpring => level >= 86,
                AID.HeavensThrust => level >= 86,
                AID.WyrmwindThrust => level >= 90,
                _ => true,
            };
        }

        public static bool Unlocked(TraitID tid, int level, int questProgress)
        {
            return tid switch
            {
                TraitID.BloodOfTheDragon => level >= 54 && questProgress > 8,
                TraitID.LanceMastery1 => level >= 64,
                TraitID.LifeOfTheDragon => level >= 70 && questProgress > 12,
                TraitID.JumpMastery => level >= 74,
                TraitID.LanceMastery2 => level >= 76,
                TraitID.LifeOfTheDragonMastery => level >= 78,
                TraitID.EnhancedCoerthanTorment => level >= 82,
                TraitID.EnhancedSpineshatterDive => level >= 84,
                TraitID.LanceMastery3 => level >= 86,
                TraitID.EnhancedLifeSurge => level >= 88,
                TraitID.LanceMastery4 => level >= 90,
                _ => true,
            };
        }

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
            SupportedActions.OGCD(AID.Feint, 10, CDGroup.Feint, 90.0f).EffectDuration = 10;
            SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
            SupportedActions.GCD(AID.PiercingTalon, 20);
            SupportedActions.OGCD(AID.ElusiveJump, 0, CDGroup.ElusiveJump, 30.0f);
            SupportedActions.OGCDWithCharges(AID.TrueNorth, 0, CDGroup.TrueNorth, 45.0f, 2);
            SupportedActions.OGCD(AID.LegSweep, 3, CDGroup.LegSweep, 40.0f);
        }
    }
}
