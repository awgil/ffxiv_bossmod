namespace BossMod.VPR;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    WorldSwallower = 34866, // LB3, 4.5s cast, range 8, single-target, targets=Hostile, animLock=???
    SteelFangs = 34606, // L1, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    HuntersSting = 34608, // L5, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    DreadFangs = 34607, // L10, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    WrithingSnap = 34632, // L15, instant, GCD, range 20, single-target, targets=Hostile, animLock=???
    SwiftskinsSting = 34609, // L20, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    SteelMaw = 34614, // L25, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    FlankstingStrike = 34610, // L30, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    FlanksbaneFang = 34611, // L30, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    HindstingStrike = 34612, // L30, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    HindsbaneFang = 34613, // L30, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    DreadMaw = 34615, // L35, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    Slither = 34646, // L40, instant, 30.0s CD (group 13/70) (2? charges), range 20, single-target, targets=Party/Hostile, animLock=???
    HuntersBite = 34616, // L40, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    SwiftskinsBite = 34617, // L45, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    JaggedMaw = 34618, // L50, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    BloodiedMaw = 34619, // L50, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    DeathRattle = 34634, // L55, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile, animLock=???
    SerpentsTail = 35920, // L55, instant, 1.0s CD (group 0), range 0, single-target, targets=Self, animLock=???
    LastLash = 34635, // L60, instant, 1.0s CD (group 0), range 0, AOE 5 circle, targets=Self, animLock=???
    Dreadwinder = 34620, // L65, instant, 40.0s CD (group 14/57) (2? charges), range 3, single-target, targets=Hostile, animLock=???
    HuntersCoil = 34621, // L65, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    SwiftskinsCoil = 34622, // L65, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    PitOfDread = 34623, // L70, instant, 40.0s CD (group 14/57) (2? charges), range 0, AOE 5 circle, targets=Self, animLock=???
    SwiftskinsDen = 34625, // L70, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    HuntersDen = 34624, // L70, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    TwinfangBite = 34636, // L75, instant, 1.0s CD (group 1), range 3, single-target, targets=Hostile, animLock=???
    TwinbloodBite = 34637, // L75, instant, 1.0s CD (group 2), range 3, single-target, targets=Hostile, animLock=???
    Twinblood = 35922, // L75, instant, 1.0s CD (group 2), range 0, single-target, targets=Self, animLock=???
    Twinfang = 35921, // L75, instant, 1.0s CD (group 1), range 0, single-target, targets=Self, animLock=???
    TwinfangThresh = 34638, // L80, instant, 1.0s CD (group 1), range 0, AOE 5 circle, targets=Self, animLock=???
    TwinbloodThresh = 34639, // L80, instant, 1.0s CD (group 2), range 0, AOE 5 circle, targets=Self, animLock=???
    UncoiledFury = 34633, // L82, instant, GCD, range 20, AOE 5 circle, targets=Hostile, animLock=???
    SerpentsIre = 34647, // L86, instant, 120.0s CD (group 19), range 0, single-target, targets=Self, animLock=???
    ThirdGeneration = 34629, // L90, instant, GCD, range 3, AOE 5 circle, targets=Hostile, animLock=???
    FourthGeneration = 34630, // L90, instant, GCD, range 3, AOE 5 circle, targets=Hostile, animLock=???
    SecondGeneration = 34628, // L90, instant, GCD, range 3, AOE 5 circle, targets=Hostile, animLock=???
    FirstGeneration = 34627, // L90, instant, GCD, range 3, AOE 5 circle, targets=Hostile, animLock=???
    Reawaken = 34626, // L90, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    UncoiledTwinfang = 34644, // L92, instant, 1.0s CD (group 1), range 20, AOE 5 circle, targets=Hostile, animLock=???
    UncoiledTwinblood = 34645, // L92, instant, 1.0s CD (group 2), range 20, AOE 5 circle, targets=Hostile, animLock=???
    Ouroboros = 34631, // L96, instant, GCD, range 3, AOE 5 circle, targets=Hostile, animLock=???
    FourthLegacy = 34643, // L100, instant, 1.0s CD (group 0), range 3, AOE 5 circle, targets=Hostile, animLock=???
    SecondLegacy = 34641, // L100, instant, 1.0s CD (group 0), range 3, AOE 5 circle, targets=Hostile, animLock=???
    FirstLegacy = 34640, // L100, instant, 1.0s CD (group 0), range 3, AOE 5 circle, targets=Hostile, animLock=???
    ThirdLegacy = 34642, // L100, instant, 1.0s CD (group 0), range 3, AOE 5 circle, targets=Hostile, animLock=???

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast, range 8, single-target, targets=Hostile, animLock=3.860s?
    Bladedance = ClassShared.AID.Bladedance, // LB2, 3.0s cast, range 8, single-target, targets=Hostile, animLock=3.860s?
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=Self
    LegSweep = ClassShared.AID.LegSweep, // L10, instant, 40.0s CD (group 43), range 3, single-target, targets=Hostile
    Bloodbath = ClassShared.AID.Bloodbath, // L12, instant, 90.0s CD (group 46), range 0, single-target, targets=Self
    Feint = ClassShared.AID.Feint, // L22, instant, 90.0s CD (group 47), range 10, single-target, targets=Hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
    TrueNorth = ClassShared.AID.TrueNorth, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    VipersFangs = 524, // L55
    VipersMaw = 525, // L60
    MeleeMastery = 674, // L74
    VipersBite = 526, // L75
    VipersThresh = 527, // L80
    VipersRattle = 528, // L82
    EnhancedSlither = 529, // L84
    MeleeMasteryII = 675, // L84
    EnhancedVipersRattle = 530, // L88
    SerpentsLineage = 531, // L90
    UncoiledFangs = 532, // L92
    EnhancedSecondWind = 642, // L94
    EnhancedSerpentsLineage = 533, // L96
    EnhancedFeint = 641, // L98
    SerpentsLegacy = 534, // L100
}
public enum SID : uint
{
    None = 0,
    HuntersInstinct = 3668, // applied by Hunter's Sting, Hunter's Bite, Hunter's Coil, Hunter's Den to self
    NoxiousGnash = 3667, // applied by Dread Fangs to target
    Swiftscaled = 3669, // applied by Swiftskin's Sting, Swiftskin's Bite, Swiftskin's Coil, Swiftskin's Den to self
    HindstungVenom = 3647, // applied by Flanksting Strike to self
    HindsbaneVenom = 3648, // applied by Flanksbane Fang to self
    FlanksbaneVenom = 3646, // applied by Hindsting Strike to self
    FlankstungVenom = 3645, // applied by Hindsbane Fang to self
    GrimskinsVenom = 3650, // applied by Jagged Maw to self
    GrimhuntersVenom = 3649, // applied by Bloodied Maw to self
    HuntersVenom = 3657, // applied by Hunter's Coil, Twinblood Bite to self
    SwiftskinsVenom = 3658, // applied by Swiftskin's Coil, Twinfang Bite to self
    Reawakened = 3670, // applied by Reawaken to self
    ReawakenReady = 3671, // applied by Serpent's Ire to self
    FellhuntersVenom = 3659, // applied by Hunter's Den, Twinblood Thresh to self
    FellskinsVenom = 3660, // applied by Swiftskin's Den, Twinfang Thresh to self
    TwinbloodfangST = 3773, // applied by Hunter's Coil, Swiftskin's Coil to self
    TwinbloodfangAOE = 3774, // applied by Swiftskin's Den, Hunter's Den to self
    PoisedForTwinfang = 3665, // applied by Uncoiled Fury, Uncoiled Twinblood to self
    PoisedForTwinblood = 3666, // applied by Uncoiled Fury, Uncoiled Twinfang to self
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.WorldSwallower); // animLock=???
        d.RegisterSpell(AID.SteelFangs); // animLock=???
        d.RegisterSpell(AID.HuntersSting); // animLock=???
        d.RegisterSpell(AID.DreadFangs); // animLock=???
        d.RegisterSpell(AID.WrithingSnap); // animLock=???
        d.RegisterSpell(AID.SwiftskinsSting); // animLock=???
        d.RegisterSpell(AID.SteelMaw); // animLock=???
        d.RegisterSpell(AID.FlankstingStrike); // animLock=???
        d.RegisterSpell(AID.FlanksbaneFang); // animLock=???
        d.RegisterSpell(AID.HindstingStrike); // animLock=???
        d.RegisterSpell(AID.HindsbaneFang); // animLock=???
        d.RegisterSpell(AID.DreadMaw); // animLock=???
        d.RegisterSpell(AID.Slither); // animLock=???
        d.RegisterSpell(AID.HuntersBite); // animLock=???
        d.RegisterSpell(AID.SwiftskinsBite); // animLock=???
        d.RegisterSpell(AID.JaggedMaw); // animLock=???
        d.RegisterSpell(AID.BloodiedMaw); // animLock=???
        d.RegisterSpell(AID.DeathRattle); // animLock=???
        d.RegisterSpell(AID.SerpentsTail); // animLock=???
        d.RegisterSpell(AID.LastLash); // animLock=???
        d.RegisterSpell(AID.Dreadwinder); // animLock=???
        d.RegisterSpell(AID.HuntersCoil); // animLock=???
        d.RegisterSpell(AID.SwiftskinsCoil); // animLock=???
        d.RegisterSpell(AID.PitOfDread); // animLock=???
        d.RegisterSpell(AID.SwiftskinsDen); // animLock=???
        d.RegisterSpell(AID.HuntersDen); // animLock=???
        d.RegisterSpell(AID.TwinfangBite); // animLock=???
        d.RegisterSpell(AID.TwinbloodBite); // animLock=???
        d.RegisterSpell(AID.Twinblood); // animLock=???
        d.RegisterSpell(AID.Twinfang); // animLock=???
        d.RegisterSpell(AID.TwinfangThresh); // animLock=???
        d.RegisterSpell(AID.TwinbloodThresh); // animLock=???
        d.RegisterSpell(AID.UncoiledFury); // animLock=???
        d.RegisterSpell(AID.SerpentsIre); // animLock=???
        d.RegisterSpell(AID.FirstGeneration); // animLock=???
        d.RegisterSpell(AID.SecondGeneration); // animLock=???
        d.RegisterSpell(AID.ThirdGeneration); // animLock=???
        d.RegisterSpell(AID.FourthGeneration); // animLock=???
        d.RegisterSpell(AID.Reawaken); // animLock=???
        d.RegisterSpell(AID.UncoiledTwinfang); // animLock=???
        d.RegisterSpell(AID.UncoiledTwinblood); // animLock=???
        d.RegisterSpell(AID.Ouroboros); // animLock=???
        d.RegisterSpell(AID.FourthLegacy); // animLock=???
        d.RegisterSpell(AID.SecondLegacy); // animLock=???
        d.RegisterSpell(AID.FirstLegacy); // animLock=???
        d.RegisterSpell(AID.ThirdLegacy); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.Slither, TraitID.EnhancedSlither);
        // *** add any properties that can't be autogenerated here ***
    }
}
