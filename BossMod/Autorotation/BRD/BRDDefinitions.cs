using System.Collections.Generic;

namespace BossMod.BRD
{
    public enum AID : uint
    {
        None = 0,
        Sprint = 3,

        // single target GCDs
        HeavyShot = 97, // L1, instant, range 25, single-target 0/0, targets=hostile
        BurstShot = 16495, // L76, instant, range 25, single-target 0/0, targets=hostile
        StraightShot = 98, // L2, instant, range 25, single-target 0/0, targets=hostile
        RefulgentArrow = 7409, // L70, instant, range 25, single-target 0/0, targets=hostile
        VenomousBite = 100, // L6, instant, range 25, single-target 0/0, targets=hostile
        CausticBite = 7406, // L64, instant, range 25, single-target 0/0, targets=hostile
        Windbite = 113, // L30, instant, range 25, single-target 0/0, targets=hostile
        Stormbite = 7407, // L64, instant, range 25, single-target 0/0, targets=hostile
        IronJaws = 3560, // L56, instant, range 25, single-target 0/0, targets=hostile
        ApexArrow = 16496, // L80, instant, range 25, AOE rect 25/4, targets=hostile
        BlastArrow = 25784, // L86, instant, range 25, AOE rect 25/4, targets=hostile

        // aoe GCDs
        QuickNock = 106, // L18, instant, range 12, AOE cone 12/0 (90 degree), targets=hostile
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
        RadiantFinale = 25785, // L90, instant, 110.0s CD (group 15), range 0, AOE circle 20/0, targets=self

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

    public enum TraitID : uint
    {
        None = 0,
        HeavierShot = 17, // L2, straight shot proc on heavy shot
        IncreasedActionDamage1 = 18, // L20, damage increase
        IncreasedActionDamage2 = 20, // L40, damage increase
        BiteMastery1 = 168, // L64, dot upgrade
        EnhancedEmpyrealArrow = 169, // L68, empyreal arrow triggers repertoire
        StraightShotMastery = 282, // L70, straight shot -> refulgent arrow upgrade
        EnhancedQuickNock = 283, // L72, shadowbite proc on quick nock
        BiteMastery2 = 284, // L76, straight shot proc on dot
        HeavyShotMastery = 285, // L76, heavy shot -> burst shot upgrade
        EnhancedArmysPaeon = 286, // L78, army muse effect
        SoulVoice = 287, // L80, gauge unlock
        QuickNockMastery = 444, // L82, quck nock -> ladonsbite upgrade
        EnhancedBloodletter = 445, // L84, third charge
        EnhancedApexArrow = 446, // L86, blast arrow proc
        EnhancedTroubadour = 447, // L88, reduce cd
        MinstrelsCoda = 448, // L90, radiant finale mechanics
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

    public enum SID : uint
    {
        None = 0,
        StraightShotReady = 122, // procced or applied by Barrage to self
        VenomousBite = 124, // applied by Venomous Bite, dot
        Windbite = 129, // applied by Windbite, dot
        CausticBite = 1200, // applied by Caustic Bite, Iron Jaws to target, dot
        Stormbite = 1201, // applied by Stormbite, Iron Jaws to target, dot
        RagingStrikes = 125, // applied by Raging Strikes to self, damage buff
        Barrage = 128, // applied by Barrage to self
        Peloton = 1199,
        ShadowbiteReady = 3002, // applied by Ladonsbite to self
        NaturesMinne = 1202, // applied by Nature's Minne to self
        WardensPaean = 866, // applied by the Warden's Paean to self
        BattleVoice = 141, // applied by Battle Voice to self
        Troubadour = 1934, // applied by Troubadour to self
        ArmsLength = 1209, // applied by Arm's Length to self
        Bind = 13, // applied by Foot Graze to target
        BlastArrowReady = 2692, // applied by Apex Arrow to self
        RadiantFinale = 2964, // applied by Radiant Finale to self. damage up
        RadiantFinaleVisual = 2722, // applied by Radiant Finale to self, visual effect
        ArmysMuse = 1932, // applied when leaving army's paeon
    }

    public static class Definitions
    {
        public static uint[] UnlockQuests = { 65604, 65612, 66621, 66622, 66623, 66624, 66626, 67250, 67251, 67252, 67254, 68430 };

        public static bool Unlocked(AID aid, int level, int questProgress)
        {
            return aid switch
            {
                AID.StraightShot => level >= 2,
                AID.RagingStrikes => level >= 4,
                AID.VenomousBite => level >= 6,
                AID.LegGraze => level >= 6,
                AID.SecondWind => level >= 8,
                AID.FootGraze => level >= 10,
                AID.Bloodletter => level >= 12,
                AID.RepellingShot => level >= 15 && questProgress > 0,
                AID.QuickNock => level >= 18,
                AID.Peloton => level >= 20,
                AID.HeadGraze => level >= 24,
                AID.Windbite => level >= 30 && questProgress > 1,
                AID.MagesBallad => level >= 30 && questProgress > 2,
                AID.ArmsLength => level >= 32,
                AID.WardensPaean => level >= 35 && questProgress > 3,
                AID.Barrage => level >= 38,
                AID.ArmysPaeon => level >= 40 && questProgress > 4,
                AID.RainOfDeath => level >= 45 && questProgress > 5,
                AID.BattleVoice => level >= 50 && questProgress > 6,
                AID.PitchPerfect => level >= 52,
                AID.WanderersMinuet => level >= 52 && questProgress > 7,
                AID.EmpyrealArrow => level >= 54 && questProgress > 8,
                AID.IronJaws => level >= 56 && questProgress > 9,
                AID.Sidewinder => level >= 60 && questProgress > 10,
                AID.Troubadour => level >= 62,
                AID.Stormbite => level >= 64,
                AID.CausticBite => level >= 64,
                AID.NaturesMinne => level >= 66,
                AID.RefulgentArrow => level >= 70 && questProgress > 11,
                AID.Shadowbite => level >= 72,
                AID.BurstShot => level >= 76,
                AID.ApexArrow => level >= 80,
                AID.Ladonsbite => level >= 82,
                AID.BlastArrow => level >= 86,
                AID.RadiantFinale => level >= 90,
                _ => true,
            };
        }

        public static bool Unlocked(TraitID tid, int level, int questProgress)
        {
            return tid switch
            {
                TraitID.HeavierShot => level >= 2,
                TraitID.IncreasedActionDamage1 => level >= 20,
                TraitID.IncreasedActionDamage2 => level >= 40,
                TraitID.BiteMastery1 => level >= 64,
                TraitID.EnhancedEmpyrealArrow => level >= 68,
                TraitID.StraightShotMastery => level >= 70 && questProgress > 11,
                TraitID.EnhancedQuickNock => level >= 72,
                TraitID.BiteMastery2 => level >= 76,
                TraitID.HeavyShotMastery => level >= 76,
                TraitID.EnhancedArmysPaeon => level >= 78,
                TraitID.SoulVoice => level >= 80,
                TraitID.QuickNockMastery => level >= 82,
                TraitID.EnhancedBloodletter => level >= 84,
                TraitID.EnhancedApexArrow => level >= 86,
                TraitID.EnhancedTroubadour => level >= 88,
                TraitID.MinstrelsCoda => level >= 90,
                _ => true,
            };
        }

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
}
