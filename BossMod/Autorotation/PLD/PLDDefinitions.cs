using System.Collections.Generic;

namespace BossMod.PLD
{
    public enum AID : uint
    {
        None = 0,

        // single target GCDs
        FastBlade = 9, // L1, instant, range 3, single-target 0/0, targets=hostile
        RiotBlade = 15, // L4, instant, range 3, single-target 0/0, targets=hostile
        RageOfHalone = 21, // L26, instant, range 3, single-target 0/0, targets=hostile
        GoringBlade = 3538, // L54, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        RoyalAuthority = 3539, // L60, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        HolySpirit = 7384, // L64, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Atonement = 16460, // L76, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        Confiteor = 16459, // L80, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
        BladeOfFaith = 25748, // L90, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
        BladeOfTruth = 25749, // L90, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
        BladeOfValor = 25750, // L90, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???

        // aoe GCDs
        TotalEclipse = 7381, // L6, instant, range 0, AOE circle 5/0, targets=self
        Prominence = 16457, // L40, instant, range 0, AOE circle 5/0, targets=self
        HolyCircle = 16458, // L72, 1.5s cast, range 0, AOE circle 5/0, targets=self, animLock=???

        // oGCDs
        SpiritsWithin = 29, // L30, instant, 30.0s CD (group 5), range 3, single-target 0/0, targets=hostile
        Expiacion = 25747, // L86, instant, 30.0s CD (group 5), range 3, AOE circle 5/0, targets=hostile, animLock=???
        CircleOfScorn = 23, // L50, instant, 30.0s CD (group 4), range 0, AOE circle 5/0, targets=self, animLock=???
        Intervene = 16461, // L74, instant, 30.0s CD (group 9) (2 charges), range 20, single-target 0/0, targets=hostile, animLock=???

        // offensive CDs
        FightOrFlight = 20, // L2, instant, 60.0s CD (group 10), range 0, single-target 0/0, targets=self
        Requiescat = 7383, // L68, instant, 60.0s CD (group 11), range 3, single-target 0/0, targets=hostile, animLock=???

        // defensive CDs
        Rampart = 7531, // L8, instant, 90.0s CD (group 40), range 0, single-target 0/0, targets=self
        Sheltron = 3542, // L35, instant, 5.0s CD (group 0), range 0, single-target 0/0, targets=self
        Sentinel = 17, // L38, instant, 120.0s CD (group 19), range 0, single-target 0/0, targets=self
        Cover = 27, // L45, instant, 120.0s CD (group 20), range 10, single-target 0/0, targets=party, animLock=???
        HolySheltron = 25746, // L82, instant, 5.0s CD (group 2), range 0, single-target 0/0, targets=self, animLock=???
        HallowedGround = 30, // L50, instant, 420.0s CD (group 24), range 0, single-target 0/0, targets=self, animLock=???
        Reprisal = 7535, // L22, instant, 60.0s CD (group 43), range 0, AOE circle 5/0, targets=self
        PassageOfArms = 7385, // L70, instant, 120.0s CD (group 21), range 0, Ground circle 8/0, targets=self, animLock=???
        DivineVeil = 3540, // L56, instant, 90.0s CD (group 14), range 0, single-target 0/0, targets=self, animLock=???
        Intervention = 7382, // L62, instant, 10.0s CD (group 1), range 30, single-target 0/0, targets=party, animLock=???
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self

        // misc
        Clemency = 3541, // L58, 1.5s cast, range 30, single-target 0/0, targets=self/party/friendly, animLock=???
        ShieldBash = 16, // L10, instant, range 3, single-target 0/0, targets=hostile
        ShieldLob = 24, // L15, instant, range 20, single-target 0/0, targets=hostile
        IronWill = 28, // L10, instant, 3.0s CD (group 3), range 0, single-target 0/0, targets=self
        Provoke = 7533, // L15, instant, 30.0s CD (group 42), range 25, single-target 0/0, targets=hostile
        Shirk = 7537, // L48, instant, 120.0s CD (group 45), range 25, single-target 0/0, targets=party
        LowBlow = 7540, // L12, instant, 25.0s CD (group 41), range 3, single-target 0/0, targets=hostile
        Interject = 7538, // L18, instant, 30.0s CD (group 44), range 3, single-target 0/0, targets=hostile
    }

    public enum CDGroup : int
    {
        Sheltron = 0, // 5.0 max
        Intervention = 1, // 10.0 max
        HolySheltron = 2, // 5.0 max
        IronWill = 3, // 3.0 max
        CircleOfScorn = 4, // 30.0 max
        SpiritsWithin = 5, // 30.0 max, shared by Spirits Within, Expiacion
        Intervene = 9, // 2*30.0 max
        FightOrFlight = 10, // 60.0 max
        Requiescat = 11, // 60.0 max
        DivineVeil = 14, // 90.0 max
        Sentinel = 19, // 120.0 max
        Cover = 20, // 120.0 max
        PassageOfArms = 21, // 120.0 max
        HallowedGround = 24, // 420.0 max
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
        // actions
        FightOrFlight = 2,
        RiotBlade = 4,
        TotalEclipse = 6,
        Rampart = 8,
        ShieldBash = 10,
        IronWill = 10,
        LowBlow = 12,
        Provoke = 15,
        ShieldLob = 15, // unlocked by quest 65798 'That Old Familiar Feeling'
        Interject = 18,
        Reprisal = 22,
        RageOfHalone = 26,
        SpiritsWithin = 30, // unlocked by quest 66591 'Paladin's Pledge'
        ArmsLength = 32,
        Sheltron = 35, // unlocked by quest 66592 'Honor Lost'
        Sentinel = 38,
        Prominence = 40, // unlocked by quest 66593 'Power Struggles'
        Cover = 45, // unlocked by quest 66595 'Parley in the Sagolii'
        Shirk = 48,
        HallowedGround = 50, // unlocked by quest 66596 'Keeping the Oath'
        CircleOfScorn = 50,
        GoringBlade = 54, // unlocked by quest 67570 'Big Sollerets to Fill'
        DivineVeil = 56, // unlocked by quest 67571 'Hey Soul Crystal'
        Clemency = 58, // unlocked by quest 67572 'All According to Plan'
        RoyalAuthority = 60, // unlocked by quest 67573 'This Little Sword of Mine'
        Intervention = 62,
        HolySpirit = 64,
        Requiescat = 68,
        PassageOfArms = 70, // unlocked by quest 68111 'Raising the Sword'
        HolyCircle = 72,
        Intervene = 74,
        Atonement = 76,
        Confiteor = 80,
        HolySheltron = 82,
        Expiacion = 86,
        BladeOfTruth = 90,
        BladeOfFaith = 90,
        BladeOfValor = 90,

        // traits
        OathMastery = 35, // unlocked by quest 66592 'Honor Lost'
        Chivalry = 58,
        RageOfHaloneMastery = 60, // unlocked by quest 67573 'This Little Sword of Mine'
        DivineMagicMastery = 64,
        EnhancedProminence = 66,
        EnhancedSheltron = 74,
        SwordOath = 76,
        SheltronMastery = 82,
        EnhancedIntervention = 82,
        DivineMagicMasteryII = 84,
        MeleeMastery = 84,
        SpiritsWithinMastery = 86,
        EnhancedDivineVeil = 88,
    }

    public static class Definitions
    {
        public static QuestLockEntry[] QuestsPerLevel = {
            new(15, 65798),
            new(30, 66591),
            new(35, 66592),
            new(40, 66593),
            new(45, 66595),
            new(50, 66596),
            new(54, 67570),
            new(56, 67571),
            new(58, 67572),
            new(60, 67573),
            new(70, 68111),
        };

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;
        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
            SupportedActions.GCD(AID.FastBlade, 3);
            SupportedActions.GCD(AID.RiotBlade, 3);
            SupportedActions.GCD(AID.RageOfHalone, 3);
            SupportedActions.GCD(AID.GoringBlade, 3);
            SupportedActions.GCD(AID.RoyalAuthority, 3);
            SupportedActions.GCD(AID.HolySpirit, 25);
            SupportedActions.GCD(AID.Atonement, 3);
            SupportedActions.GCD(AID.Confiteor, 25);
            SupportedActions.GCD(AID.BladeOfFaith, 25);
            SupportedActions.GCD(AID.BladeOfTruth, 25);
            SupportedActions.GCD(AID.BladeOfValor, 25);
            SupportedActions.GCD(AID.TotalEclipse, 0);
            SupportedActions.GCD(AID.Prominence, 0);
            SupportedActions.GCD(AID.HolyCircle, 0);
            SupportedActions.OGCD(AID.SpiritsWithin, 3, CDGroup.SpiritsWithin, 30.0f);
            SupportedActions.OGCD(AID.Expiacion, 3, CDGroup.SpiritsWithin, 30.0f);
            SupportedActions.OGCD(AID.CircleOfScorn, 0, CDGroup.CircleOfScorn, 30.0f);
            SupportedActions.OGCDWithCharges(AID.Intervene, 20, CDGroup.Intervene, 30.0f, 2);
            SupportedActions.OGCD(AID.FightOrFlight, 0, CDGroup.FightOrFlight, 60.0f);
            SupportedActions.OGCD(AID.Requiescat, 3, CDGroup.Requiescat, 60.0f);
            SupportedActions.OGCD(AID.Rampart, 0, CDGroup.Rampart, 90.0f);
            SupportedActions.OGCD(AID.Sheltron, 0, CDGroup.Sheltron, 5.0f);
            SupportedActions.OGCD(AID.Sentinel, 0, CDGroup.Sentinel, 120.0f);
            SupportedActions.OGCD(AID.Cover, 10, CDGroup.Cover, 120.0f);
            SupportedActions.OGCD(AID.HolySheltron, 0, CDGroup.HolySheltron, 5.0f);
            SupportedActions.OGCD(AID.HallowedGround, 0, CDGroup.HallowedGround, 420.0f);
            SupportedActions.OGCD(AID.Reprisal, 0, CDGroup.Reprisal, 60.0f);
            SupportedActions.OGCD(AID.PassageOfArms, 0, CDGroup.PassageOfArms, 120.0f);
            SupportedActions.OGCD(AID.DivineVeil, 0, CDGroup.DivineVeil, 90.0f);
            SupportedActions.OGCD(AID.Intervention, 30, CDGroup.Intervention, 10.0f);
            SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f);
            SupportedActions.GCD(AID.Clemency, 30);
            SupportedActions.GCD(AID.ShieldBash, 3);
            SupportedActions.GCD(AID.ShieldLob, 20);
            SupportedActions.OGCD(AID.IronWill, 0, CDGroup.IronWill, 3.0f);
            SupportedActions.OGCD(AID.Provoke, 25, CDGroup.Provoke, 30.0f);
            SupportedActions.OGCD(AID.Shirk, 25, CDGroup.Shirk, 120.0f);
            SupportedActions.OGCD(AID.LowBlow, 3, CDGroup.LowBlow, 25.0f);
            SupportedActions.OGCD(AID.Interject, 3, CDGroup.Interject, 30.0f);
        }
    }

    public enum SID : uint
    {
        None = 0,
        FightOrFlight = 76, // applied by Fight or Flight to self, +25% physical damage dealt buff
        Rampart = 1191, // applied by Rampart to self, -20% damage taken
        Reprisal = 1193, // applied by Reprisal to target
        IronWill = 79, // applied by Iron Will to self, tank stance
        Stun = 2, // applied by Low Blow, Shield Bash to target
    }
}
