namespace BossMod.PLD;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

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
    CircleOfScorn = 23, // L50, instant, 30.0s CD (group 4), range 0, AOE circle 5/0, targets=self
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
    HallowedGround = 30, // L50, instant, 420.0s CD (group 24), range 0, single-target 0/0, targets=self
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

public enum TraitID : uint
{
    None = 0,
    TankMastery = 317, // L1
    OathMastery = 209, // L35, gauge unlock
    Chivalry = 246, // L58, riot blade & spirits within restore mp
    RageOfHaloneMastery = 260, // L60, rage of halone -> royal authority upgrade
    DivineMagicMastery1 = 207, // L64, reduce mp cost and prevent interruptions
    EnhancedProminence = 261, // L66, prominence restores mp
    EnhancedSheltron = 262, // L74, duration increase
    SwordOath = 264, // L76
    SheltronMastery = 412, // L82, sheltron -> holy sheltron upgrade
    EnhancedIntervention = 413, // L82
    DivineMagicMastery2 = 414, // L84, adds heal
    MeleeMastery = 504, // L84, potency increase
    SpiritsWithinMastery = 415, // L86, spirits within -> expiacion upgrade
    EnhancedDivineVeil = 416, // L88, adds heal
}

public enum CDGroup : int
{
    Sheltron = 0, // 5.0 max
    Intervention = 1, // 10.0 max
    HolySheltron = 2, // 5.0 max
    IronWill = 3, // variable max, shared by Iron Will, Release Iron Will
    CircleOfScorn = 4, // 30.0 max
    SpiritsWithin = 5, // 30.0 max, shared by Spirits Within, Expiacion
    Intervene = 9, // 2*30.0 max
    FightOrFlight = 10, // 60.0 max
    Requiescat = 11, // 60.0 max
    GoringBlade = 12, // 60.0 max
    DivineVeil = 14, // 90.0 max
    Bulwark = 15, // 90.0 max
    Sentinel = 19, // 120.0 max
    Cover = 20, // 120.0 max
    PassageOfArms = 21, // 120.0 max
    HallowedGround = 24, // 420.0 max
    LowBlow = 41, // 25.0 max
    Provoke = 42, // 30.0 max
    Interject = 43, // 30.0 max
    Reprisal = 44, // 60.0 max
    Rampart = 46, // 90.0 max
    ArmsLength = 48, // 120.0 max
    Shirk = 49, // 120.0 max
}

public enum SID : uint
{
    None = 0,
    FightOrFlight = 76, // applied by Fight or Flight to self, +25% physical damage dealt buff
    CircleOfScorn = 248, // applied by Circle of Scorn to target, dot
    Rampart = 1191, // applied by Rampart to self, -20% damage taken
    Reprisal = 1193, // applied by Reprisal to target
    HallowedGround = 82, // applied by Hallowed Ground to self, immune
    IronWill = 79, // applied by Iron Will to self, tank stance
    Stun = 2, // applied by Low Blow, Shield Bash to target
}

public static class Definitions
{
    public static uint[] UnlockQuests = { 65798, 66591, 66592, 66593, 66595, 66596, 67570, 67571, 67572, 67573, 68111 };

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
        {
            AID.FightOrFlight => level >= 2,
            AID.RiotBlade => level >= 4,
            AID.TotalEclipse => level >= 6,
            AID.Rampart => level >= 8,
            AID.ShieldBash => level >= 10,
            AID.IronWill => level >= 10,
            AID.LowBlow => level >= 12,
            AID.Provoke => level >= 15,
            AID.ShieldLob => level >= 15 && questProgress > 0,
            AID.Interject => level >= 18,
            AID.Reprisal => level >= 22,
            AID.RageOfHalone => level >= 26,
            AID.SpiritsWithin => level >= 30 && questProgress > 1,
            AID.ArmsLength => level >= 32,
            AID.Sheltron => level >= 35 && questProgress > 2,
            AID.Sentinel => level >= 38,
            AID.Prominence => level >= 40 && questProgress > 3,
            AID.Cover => level >= 45 && questProgress > 4,
            AID.Shirk => level >= 48,
            AID.HallowedGround => level >= 50 && questProgress > 5,
            AID.CircleOfScorn => level >= 50,
            AID.GoringBlade => level >= 54 && questProgress > 6,
            AID.DivineVeil => level >= 56 && questProgress > 7,
            AID.Clemency => level >= 58 && questProgress > 8,
            AID.RoyalAuthority => level >= 60 && questProgress > 9,
            AID.Intervention => level >= 62,
            AID.HolySpirit => level >= 64,
            AID.Requiescat => level >= 68,
            AID.PassageOfArms => level >= 70 && questProgress > 10,
            AID.HolyCircle => level >= 72,
            AID.Intervene => level >= 74,
            AID.Atonement => level >= 76,
            AID.Confiteor => level >= 80,
            AID.HolySheltron => level >= 82,
            AID.Expiacion => level >= 86,
            AID.BladeOfTruth => level >= 90,
            AID.BladeOfFaith => level >= 90,
            AID.BladeOfValor => level >= 90,
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
        {
            TraitID.OathMastery => level >= 35 && questProgress > 2,
            TraitID.Chivalry => level >= 58,
            TraitID.RageOfHaloneMastery => level >= 60 && questProgress > 9,
            TraitID.DivineMagicMastery1 => level >= 64,
            TraitID.EnhancedProminence => level >= 66,
            TraitID.EnhancedSheltron => level >= 74,
            TraitID.SwordOath => level >= 76,
            TraitID.SheltronMastery => level >= 82,
            TraitID.EnhancedIntervention => level >= 82,
            TraitID.DivineMagicMastery2 => level >= 84,
            TraitID.MeleeMastery => level >= 84,
            TraitID.SpiritsWithinMastery => level >= 86,
            TraitID.EnhancedDivineVeil => level >= 88,
            _ => true,
        };
    }

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
        SupportedActions.OGCD(AID.FightOrFlight, 0, CDGroup.FightOrFlight, 60.0f).EffectDuration = 25;
        SupportedActions.OGCD(AID.Requiescat, 3, CDGroup.Requiescat, 60.0f);
        SupportedActions.OGCD(AID.Rampart, 0, CDGroup.Rampart, 90.0f).EffectDuration = 20;
        SupportedActions.OGCD(AID.Sheltron, 0, CDGroup.Sheltron, 5.0f).EffectDuration = 4;
        SupportedActions.OGCD(AID.Sentinel, 0, CDGroup.Sentinel, 120.0f).EffectDuration = 15;
        SupportedActions.OGCD(AID.Cover, 10, CDGroup.Cover, 120.0f);
        SupportedActions.OGCD(AID.HolySheltron, 0, CDGroup.HolySheltron, 5.0f);
        SupportedActions.OGCD(AID.HallowedGround, 0, CDGroup.HallowedGround, 420.0f).EffectDuration = 10;
        SupportedActions.OGCD(AID.Reprisal, 0, CDGroup.Reprisal, 60.0f).EffectDuration = 10;
        SupportedActions.OGCD(AID.PassageOfArms, 0, CDGroup.PassageOfArms, 120.0f);
        SupportedActions.OGCD(AID.DivineVeil, 0, CDGroup.DivineVeil, 90.0f);
        SupportedActions.OGCD(AID.Intervention, 30, CDGroup.Intervention, 10.0f);
        SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
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
