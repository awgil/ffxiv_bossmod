namespace BossMod.WAR;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

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
    Defiance = 48, // L10, instant, 2.0s CD (group 2), range 0, single-target 0/0, targets=self
    ReleaseDefiance = 32066, // L10, instant, 1.0s CD (group 2), range 0, single-target 0/0, targets=self
    Provoke = 7533, // L15, instant, 30.0s CD (group 42), range 25, single-target 0/0, targets=hostile
    Shirk = 7537, // L48, instant, 120.0s CD (group 45), range 25, single-target 0/0, targets=party
    LowBlow = 7540, // L12, instant, 25.0s CD (group 41), range 3, single-target 0/0, targets=hostile
    Interject = 7538, // L18, instant, 30.0s CD (group 44), range 3, single-target 0/0, targets=hostile

    // special
    ShieldWall = 197, // LB1
    Stronghold = 198, // LB2
    LandWaker = 4240, // LB3
}

public enum TraitID : uint
{
    None = 0,
    TankMastery = 318, // L1
    TheBeastWithin = 249, // L35, gauge generation
    InnerBeastMastery = 265, // L54, IB->FC upgrade
    SteelCycloneMastery = 266, // L60,  steel cyclone -> decimate upgrade
    EnhancedInfuriate = 157, // L66, gauge spenders reduce cd by 5
    BerserkMastery = 218, // L70, berserk -> IR upgrade
    NascentChaos = 267, // L72, decimate -> chaotic cyclone after infuriate
    MasteringTheBeast = 268, // L74, mythril tempest gives gauge
    EnhancedShakeItOff = 417, // L76, adds heal
    EnhancedThrillOfBattle = 269, // L78, adds incoming heal buff
    RawIntuitionMastery = 418, // L82, raw intuition -> bloodwhetting
    EnhancedNascentFlash = 419, // L82, duration increase
    EnhancedEquilibrium = 420, // L84, adds hot
    MeleeMastery = 505, // L84, potency increase
    EnhancedOnslaught = 421, // L88, 3rd onslaught charge
}

public enum CDGroup : int
{
    Defiance = 2, // variable max, shared by Defiance, Release Defiance
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
    LowBlow = 41, // 25.0 max
    Provoke = 42, // 30.0 max
    Interject = 43, // 60.0 max
    Reprisal = 44, // 30.0 max
    Rampart = 46, // 90.0 max
    ArmsLength = 48, // 120.0 max
    Shirk = 49, // 120.0 max
    LimitBreak = 71, // special/fake (TODO: remove need for it?)
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

public static class Definitions
{
    public static readonly uint[] UnlockQuests = [65852, 65855, 66586, 66587, 66589, 66590, 66124, 66132, 66134, 66137, 68440];

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
        {
            AID.Maim => level >= 4,
            AID.Berserk => level >= 6,
            AID.Rampart => level >= 8,
            AID.Overpower => level >= 10,
            AID.Defiance => level >= 10,
            AID.ReleaseDefiance => level >= 10,
            AID.LowBlow => level >= 12,
            AID.Tomahawk => level >= 15 && questProgress > 0,
            AID.Provoke => level >= 15,
            AID.Interject => level >= 18,
            AID.Reprisal => level >= 22,
            AID.StormPath => level >= 26,
            AID.ThrillOfBattle => level >= 30 && questProgress > 1,
            AID.ArmsLength => level >= 32,
            AID.InnerBeast => level >= 35 && questProgress > 2,
            AID.Vengeance => level >= 38,
            AID.MythrilTempest => level >= 40 && questProgress > 3,
            AID.Holmgang => level >= 42,
            AID.SteelCyclone => level >= 45 && questProgress > 4,
            AID.Shirk => level >= 48,
            AID.Infuriate => level >= 50 && questProgress > 5,
            AID.StormEye => level >= 50,
            AID.FellCleave => level >= 54 && questProgress > 6,
            AID.RawIntuition => level >= 56 && questProgress > 7,
            AID.Equilibrium => level >= 58 && questProgress > 8,
            AID.Decimate => level >= 60 && questProgress > 9,
            AID.Onslaught => level >= 62,
            AID.Upheaval => level >= 64,
            AID.ShakeItOff => level >= 68,
            AID.InnerRelease => level >= 70 && questProgress > 10,
            AID.ChaoticCyclone => level >= 72,
            AID.NascentFlash => level >= 76,
            AID.InnerChaos => level >= 80,
            AID.Bloodwhetting => level >= 82,
            AID.Orogeny => level >= 86,
            AID.PrimalRend => level >= 90,
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
        {
            TraitID.TheBeastWithin => level >= 35 && questProgress > 2,
            TraitID.InnerBeastMastery => level >= 54 && questProgress > 6,
            TraitID.SteelCycloneMastery => level >= 60 && questProgress > 9,
            TraitID.EnhancedInfuriate => level >= 66,
            TraitID.BerserkMastery => level >= 70 && questProgress > 10,
            TraitID.NascentChaos => level >= 72,
            TraitID.MasteringTheBeast => level >= 74,
            TraitID.EnhancedShakeItOff => level >= 76,
            TraitID.EnhancedThrillOfBattle => level >= 78,
            TraitID.RawIntuitionMastery => level >= 82,
            TraitID.EnhancedNascentFlash => level >= 82,
            TraitID.EnhancedEquilibrium => level >= 84,
            TraitID.MeleeMastery => level >= 84,
            TraitID.EnhancedOnslaught => level >= 88,
            _ => true,
        };
    }

    public static readonly Dictionary<ActionID, ActionDefinition> SupportedActions = BuildSupportedActions();
    private static Dictionary<ActionID, ActionDefinition> BuildSupportedActions()
    {
        var res = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
        res.GCD(AID.HeavySwing, 3);
        res.GCD(AID.Maim, 3);
        res.GCD(AID.StormPath, 3);
        res.GCD(AID.StormEye, 3);
        res.GCD(AID.InnerBeast, 3);
        res.GCD(AID.FellCleave, 3);
        res.GCD(AID.InnerChaos, 3);
        res.GCD(AID.PrimalRend, 20, 1.15f);
        res.GCD(AID.Overpower, 0);
        res.GCD(AID.MythrilTempest, 0);
        res.GCD(AID.SteelCyclone, 0);
        res.GCD(AID.Decimate, 0);
        res.GCD(AID.ChaoticCyclone, 0);
        res.OGCDWithCharges(AID.Infuriate, 0, CDGroup.Infuriate, 60.0f, 2);
        res.OGCDWithCharges(AID.Onslaught, 20, CDGroup.Onslaught, 30.0f, 3);
        res.OGCD(AID.Upheaval, 3, CDGroup.Upheaval, 30.0f);
        res.OGCD(AID.Orogeny, 0, CDGroup.Upheaval, 30.0f);
        res.OGCD(AID.Berserk, 0, CDGroup.Berserk, 60.0f).EffectDuration = 15;
        res.OGCD(AID.InnerRelease, 0, CDGroup.InnerRelease, 60.0f).EffectDuration = 15;
        res.OGCD(AID.Rampart, 0, CDGroup.Rampart, 90.0f).EffectDuration = 20;
        res.OGCD(AID.Vengeance, 0, CDGroup.Vengeance, 120.0f).EffectDuration = 15;
        res.OGCD(AID.ThrillOfBattle, 0, CDGroup.ThrillOfBattle, 90.0f).EffectDuration = 10;
        res.OGCD(AID.Holmgang, 6, CDGroup.Holmgang, 240.0f).EffectDuration = 10;
        res.OGCD(AID.Equilibrium, 0, CDGroup.Equilibrium, 60.0f).EffectDuration = 0;
        res.OGCD(AID.Reprisal, 0, CDGroup.Reprisal, 60.0f).EffectDuration = 10;
        res.OGCD(AID.ShakeItOff, 0, CDGroup.ShakeItOff, 90.0f).EffectDuration = 30;
        res.OGCD(AID.RawIntuition, 0, CDGroup.Bloodwhetting, 25.0f).EffectDuration = 4;
        res.OGCD(AID.NascentFlash, 30, CDGroup.Bloodwhetting, 25.0f).EffectDuration = 4;
        res.OGCD(AID.Bloodwhetting, 0, CDGroup.Bloodwhetting, 25.0f).EffectDuration = 4;
        res.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
        res.GCD(AID.Tomahawk, 20);
        res.OGCD(AID.Defiance, 0, CDGroup.Defiance, 2.0f);
        res.OGCD(AID.ReleaseDefiance, 0, CDGroup.Defiance, 1.0f);
        res.OGCD(AID.Provoke, 25, CDGroup.Provoke, 30.0f);
        res.OGCD(AID.Shirk, 25, CDGroup.Shirk, 120.0f);
        res.OGCD(AID.LowBlow, 3, CDGroup.LowBlow, 25.0f);
        res.OGCD(AID.Interject, 3, CDGroup.Interject, 30.0f);
        res.OGCD(AID.ShieldWall, 0, CDGroup.LimitBreak, 0, 3.86f);
        res.OGCD(AID.Stronghold, 0, CDGroup.LimitBreak, 0, 3.86f);
        res.OGCD(AID.LandWaker, 0, CDGroup.LimitBreak, 0, 3.86f);
        return res;
    }
}
