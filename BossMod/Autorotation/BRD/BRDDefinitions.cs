using System.Collections.Generic;

namespace BossMod.BRD
{
    public enum AID : uint
    {
        None = 0,

        // single target GCDs
        HeavyShot = 97, // L1, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        BurstShot = 16495, // L76, instant, range 25, single-target 0/0, targets=hostile
        StraightShot = 98, // L2, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        RefulgentArrow = 7409, // L70, instant, range 25, single-target 0/0, targets=hostile
        VenomousBite = 100, // L6, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        CausticBite = 7406, // L64, instant, range 25, single-target 0/0, targets=hostile
        Windbite = 113, // L30, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        Stormbite = 7407, // L64, instant, range 25, single-target 0/0, targets=hostile
        IronJaws = 3560, // L56, instant, range 25, single-target 0/0, targets=hostile
        ApexArrow = 16496, // L80, instant, range 25, AOE rect 25/4, targets=hostile
        BlastArrow = 25784, // L86, instant, range 25, AOE rect 25/4, targets=hostile, animLock=???

        // aoe GCDs
        QuickNock = 106, // L18, instant, range 12, AOE cone 12/0 (90 degree), targets=hostile, animLock=???
        Ladonsbite = 25783, // L82, instant, range 12, AOE cone 12/0 (90 degree), targets=hostile
        Shadowbite = 16494, // L72, instant, range 25, AOE circle 5/0, targets=hostile

        // oGCDs
        Bloodletter = 110, // L12, instant, 15.0s CD (group 4) (3 charges), range 25, single-target 0/0, targets=hostile
        RainOfDeath = 117, // L45, instant, 15.0s CD (group 4) (3 charges), range 25, AOE circle 8/0, targets=hostile
        PitchPerfect = 7404, // L52, instant, 1.0s CD (group 0), range 25, single-target 0/0, targets=hostile
        EmpyrealArrow = 3558, // L54, instant, 15.0s CD (group 2), range 25, single-target 0/0, targets=hostile
        Sidewinder = 3562, // L60, instant, 60.0s CD (group 12), range 25, single-target 0/0, targets=hostile

        // offsensive CDs
        RagingStrikes = 101, // L4, instant, 120.0s CD (group 13), range 0, single-target 0/0, targets=self
        Barrage = 107, // L38, instant, 120.0s CD (group 19), range 0, single-target 0/0, targets=self
        MagesBallad = 114, // L30, instant, 120.0s CD (group 20), range 25, single-target 0/0, targets=hostile
        ArmysPaeon = 116, // L40, instant, 120.0s CD (group 16), range 25, single-target 0/0, targets=hostile
        WanderersMinuet = 3559, // L52, instant, 120.0s CD (group 17), range 25, single-target 0/0, targets=hostile
        BattleVoice = 118, // L50, instant, 120.0s CD (group 22), range 0, AOE circle 20/0, targets=self
        RadiantFinale = 25785, // L90, instant, 110.0s CD (group 15), range 0, AOE circle 20/0, targets=self, animLock=???

        // defensive CDs
        SecondWind = 7541, // L8, instant, 120.0s CD (group 40), range 0, single-target 0/0, targets=self
        Troubadour = 7405, // L62, instant, 120.0s CD (group 23), range 0, AOE circle 20/0, targets=self
        NaturesMinne = 7408, // L66, instant, 90.0s CD (group 14), range 30, single-target 0/0, targets=self/party
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self

        // misc
        Peloton = 7557, // L20, instant, 5.0s CD (group 43), range 0, AOE circle 20/0, targets=self
        LegGraze = 7554, // L6, instant, 30.0s CD (group 42), range 25, single-target 0/0, targets=hostile
        FootGraze = 7553, // L10, instant, 30.0s CD (group 41), range 25, single-target 0/0, targets=hostile
        HeadGraze = 7551, // L24, instant, 30.0s CD (group 44), range 25, single-target 0/0, targets=hostile
        RepellingShot = 112, // L15, instant, 30.0s CD (group 5), range 15, single-target 0/0, targets=hostile, animLock=0.800s
        WardensPaean = 3561, // L35, instant, 45.0s CD (group 8), range 30, single-target 0/0, targets=self/party
    }

    public enum CDGroup : int
    {
        PitchPerfect = 0, // 1.0 max
        EmpyrealArrow = 2, // 15.0 max
        Bloodletter = 4, // 3*15.0 max, shared by Bloodletter, Rain of Death
        RepellingShot = 5, // 30.0 max
        WardensPaean = 8, // 45.0 max
        Sidewinder = 12, // 60.0 max
        RagingStrikes = 13, // 120.0 max
        NaturesMinne = 14, // 90.0 max
        RadiantFinale = 15, // 110.0 max
        ArmysPaeon = 16, // 120.0 max
        WanderersMinuet = 17, // 120.0 max
        Barrage = 19, // 120.0 max
        MagesBallad = 20, // 120.0 max
        BattleVoice = 22, // 120.0 max
        Troubadour = 23, // 120.0 max
        SecondWind = 40, // 120.0 max
        FootGraze = 41, // 30.0 max
        LegGraze = 42, // 30.0 max
        Peloton = 43, // 5.0 max
        HeadGraze = 44, // 30.0 max
        ArmsLength = 46, // 120.0 max
    }

    public enum MinLevel : int
    {
        // actions
        StraightShot = 2,
        RagingStrikes = 4,
        VenomousBite = 6,
        LegGraze = 6,
        SecondWind = 8,
        FootGraze = 10,
        Bloodletter = 12,
        RepellingShot = 15, // unlocked by quest 65604 'Violators Will Be Shot'
        QuickNock = 18,
        Peloton = 20,
        HeadGraze = 24,
        Windbite = 30, // unlocked by quest 65612 'The One That Got Away'
        MagesBallad = 30, // unlocked by quest 66621 'A Song of Bards and Bowmen'
        ArmsLength = 32,
        WardensPaean = 35, // unlocked by quest 66622 'The Archer's Anthem'
        Barrage = 38,
        ArmysPaeon = 40, // unlocked by quest 66623 'Bard's-eye View'
        RainOfDeath = 45, // unlocked by quest 66624 'Doing It the Bard Way'
        BattleVoice = 50, // unlocked by quest 66626 'Requiem for the Fallen'
        WanderersMinuet = 52, // unlocked by quest 67250 'The Stiff and the Spent'
        EmpyrealArrow = 54, // unlocked by quest 67251 'Requiem on Ice'
        IronJaws = 56, // unlocked by quest 67252 'When Gnaths Cry'
        Sidewinder = 60, // unlocked by quest 67254 'The Ballad of Oblivion'
        Troubadour = 62,
        Stormbite = 64,
        CausticBite = 64,
        NaturesMinne = 66,
        RefulgentArrow = 70, // unlocked by quest 68430 'Sweet Dreams Are Made of Peace'
        Shadowbite = 72,
        BurstShot = 76,
        ApexArrow = 80,
        Ladonsbite = 82,
        BlastArrow = 86,
        RadiantFinale = 90,

        // traits
        IncreasedActionDamage1 = 20, // passive, damage increase
        IncreasedActionDamage2 = 40, // passive, damage increase
        EnhancedEmpyrealArrow = 68, // passive, empyreal arrow triggers repertoire
        BiteMastery2 = 76, // passive, dots can trigger straight shot
        EnhancedArmysPaeon = 78, // passive
        EnhancedBloodletter = 84, // passive, third bloodletter charge
        EnhancedTroubadour = 88, // passive, cd reduction
    }

    public static class Definitions
    {
        public static QuestLockEntry[] QuestsPerLevel = {
            new(15, 65604),
            new(30, 65612),
            new(30, 66621),
            new(35, 66622),
            new(40, 66623),
            new(45, 66624),
            new(50, 66626),
            new(52, 67250),
            new(54, 67251),
            new(56, 67252),
            new(60, 67254),
            new(70, 68430),
        };

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;
        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionDex);
            SupportedActions.GCD(AID.HeavyShot, 25);
            SupportedActions.GCD(AID.BurstShot, 25);
            SupportedActions.GCD(AID.StraightShot, 25);
            SupportedActions.GCD(AID.RefulgentArrow, 25);
            SupportedActions.GCD(AID.VenomousBite, 25);
            SupportedActions.GCD(AID.CausticBite, 25);
            SupportedActions.GCD(AID.Windbite, 25);
            SupportedActions.GCD(AID.Stormbite, 25);
            SupportedActions.GCD(AID.IronJaws, 25);
            SupportedActions.GCD(AID.ApexArrow, 25);
            SupportedActions.GCD(AID.BlastArrow, 25);
            SupportedActions.GCD(AID.QuickNock, 12);
            SupportedActions.GCD(AID.Ladonsbite, 12);
            SupportedActions.GCD(AID.Shadowbite, 25);
            SupportedActions.OGCDWithCharges(AID.Bloodletter, 25, CDGroup.Bloodletter, 15.0f, 3);
            SupportedActions.OGCDWithCharges(AID.RainOfDeath, 25, CDGroup.Bloodletter, 15.0f, 3);
            SupportedActions.OGCD(AID.PitchPerfect, 25, CDGroup.PitchPerfect, 1.0f);
            SupportedActions.OGCD(AID.EmpyrealArrow, 25, CDGroup.EmpyrealArrow, 15.0f);
            SupportedActions.OGCD(AID.Sidewinder, 25, CDGroup.Sidewinder, 60.0f);
            SupportedActions.OGCD(AID.RagingStrikes, 0, CDGroup.RagingStrikes, 120.0f);
            SupportedActions.OGCD(AID.Barrage, 0, CDGroup.Barrage, 120.0f);
            SupportedActions.OGCD(AID.MagesBallad, 25, CDGroup.MagesBallad, 120.0f);
            SupportedActions.OGCD(AID.ArmysPaeon, 25, CDGroup.ArmysPaeon, 120.0f);
            SupportedActions.OGCD(AID.WanderersMinuet, 25, CDGroup.WanderersMinuet, 120.0f);
            SupportedActions.OGCD(AID.BattleVoice, 0, CDGroup.BattleVoice, 120.0f);
            SupportedActions.OGCD(AID.RadiantFinale, 0, CDGroup.RadiantFinale, 110.0f);
            SupportedActions.OGCD(AID.SecondWind, 0, CDGroup.SecondWind, 120.0f);
            SupportedActions.OGCD(AID.Troubadour, 0, CDGroup.Troubadour, 120.0f);
            SupportedActions.OGCD(AID.NaturesMinne, 30, CDGroup.NaturesMinne, 90.0f);
            SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f);
            SupportedActions.OGCD(AID.Peloton, 0, CDGroup.Peloton, 5.0f);
            SupportedActions.OGCD(AID.LegGraze, 25, CDGroup.LegGraze, 30.0f);
            SupportedActions.OGCD(AID.FootGraze, 25, CDGroup.FootGraze, 30.0f);
            SupportedActions.OGCD(AID.HeadGraze, 25, CDGroup.HeadGraze, 30.0f);
            SupportedActions.OGCD(AID.RepellingShot, 15, CDGroup.RepellingShot, 30.0f, 0.800f);
            SupportedActions.OGCD(AID.WardensPaean, 30, CDGroup.WardensPaean, 45.0f);
        }
    }

    public enum SID : uint
    {
        None = 0,
        StraightShotReady = 122, // procced or applied by Barrage to self
        VenomousBite = 124, // applied by Venomous Bite, dot
        Windbite = 129, // applied by Windbite, dot
        RagingStrikes = 125, // applied by Raging Strikes to self, damage buff
        Barrage = 128, // applied by Barrage to self
        Peloton = 1199,
        CausticBite = 1200, // applied by Caustic Bite, Iron Jaws to target, dot
        Stormbite = 1201, // applied by Stormbite, Iron Jaws to target, dot
        ShadowbiteReady = 3002, // applied by Ladonsbite to self
        NaturesMinne = 1202, // applied by Nature's Minne to self
        WardensPaean = 866, // applied by the Warden's Paean to self
        BattleVoice = 141, // applied by Battle Voice to self
        Troubadour = 1934, // applied by Troubadour to self
        ArmsLength = 1209, // applied by Arm's Length to self
        Bind = 13, // applied by Foot Graze to target
    }
}
