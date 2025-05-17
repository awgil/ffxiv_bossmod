namespace BossMod.VPR;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    WorldSwallower = 34866, // LB3, 4.5s cast, range 8, single-target, targets=Hostile
    SteelFangs = 34606, // L1, instant, GCD, range 3, single-target, targets=Hostile
    HuntersSting = 34608, // L5, instant, GCD, range 3, single-target, targets=Hostile
    ReavingFangs = 34607, // L10, instant, GCD, range 3, single-target, targets=Hostile
    WrithingSnap = 34632, // L15, instant, GCD, range 20, single-target, targets=Hostile
    SwiftskinsSting = 34609, // L20, instant, GCD, range 3, single-target, targets=Hostile
    SteelMaw = 34614, // L25, instant, GCD, range 0, AOE 5 circle, targets=Self
    FlankstingStrike = 34610, // L30, instant, GCD, range 3, single-target, targets=Hostile
    FlanksbaneFang = 34611, // L30, instant, GCD, range 3, single-target, targets=Hostile
    HindstingStrike = 34612, // L30, instant, GCD, range 3, single-target, targets=Hostile
    HindsbaneFang = 34613, // L30, instant, GCD, range 3, single-target, targets=Hostile
    ReavingMaw = 34615, // L35, instant, GCD, range 0, AOE 5 circle, targets=Self
    Slither = 34646, // L40, instant, 30.0s CD (group 13/70) (2-3 charges), range 20, single-target, targets=Party/Hostile
    HuntersBite = 34616, // L40, instant, GCD, range 0, AOE 5 circle, targets=Self
    SwiftskinsBite = 34617, // L45, instant, GCD, range 0, AOE 5 circle, targets=Self
    JaggedMaw = 34618, // L50, instant, GCD, range 0, AOE 5 circle, targets=Self
    BloodiedMaw = 34619, // L50, instant, GCD, range 0, AOE 5 circle, targets=Self
    DeathRattle = 34634, // L55, instant, 1.0s CD (group 0), range 5, single-target, targets=Hostile
    SerpentsTail = 35920, // L55, instant, 1.0s CD (group 0), range 0, single-target, targets=Self
    LastLash = 34635, // L60, instant, 1.0s CD (group 0), range 0, AOE 5 circle, targets=Self
    Vicewinder = 34620, // L65, instant, 40.0s CD (group 14/57) (2 charges), range 3, single-target, targets=Hostile
    HuntersCoil = 34621, // L65, instant, GCD, range 3, single-target, targets=Hostile
    SwiftskinsCoil = 34622, // L65, instant, GCD, range 3, single-target, targets=Hostile
    Vicepit = 34623, // L70, instant, 40.0s CD (group 14/57) (2 charges), range 0, AOE 5 circle, targets=Self
    SwiftskinsDen = 34625, // L70, instant, GCD, range 0, AOE 5 circle, targets=Self
    HuntersDen = 34624, // L70, instant, GCD, range 0, AOE 5 circle, targets=Self
    TwinfangBite = 34636, // L75, instant, 1.0s CD (group 1), range 5, single-target, targets=Hostile
    TwinbloodBite = 34637, // L75, instant, 1.0s CD (group 2), range 5, single-target, targets=Hostile
    Twinblood = 35922, // L75, instant, 1.0s CD (group 2), range 0, single-target, targets=Self
    Twinfang = 35921, // L75, instant, 1.0s CD (group 1), range 0, single-target, targets=Self
    TwinfangThresh = 34638, // L80, instant, 1.0s CD (group 1), range 0, AOE 5 circle, targets=Self
    TwinbloodThresh = 34639, // L80, instant, 1.0s CD (group 2), range 0, AOE 5 circle, targets=Self
    UncoiledFury = 34633, // L82, instant, GCD, range 20, AOE 5 circle, targets=Hostile
    SerpentsIre = 34647, // L86, instant, 120.0s CD (group 19), range 0, single-target, targets=Self
    ThirdGeneration = 34629, // L90, instant, GCD, range 3, AOE 5 circle, targets=Hostile
    FourthGeneration = 34630, // L90, instant, GCD, range 3, AOE 5 circle, targets=Hostile
    SecondGeneration = 34628, // L90, instant, GCD, range 3, AOE 5 circle, targets=Hostile
    FirstGeneration = 34627, // L90, instant, GCD, range 3, AOE 5 circle, targets=Hostile
    Reawaken = 34626, // L90, instant, GCD, range 0, AOE 5 circle, targets=Self
    UncoiledTwinfang = 34644, // L92, instant, 1.0s CD (group 1), range 20, AOE 5 circle, targets=Hostile
    UncoiledTwinblood = 34645, // L92, instant, 1.0s CD (group 2), range 20, AOE 5 circle, targets=Hostile
    Ouroboros = 34631, // L96, instant, GCD, range 3, AOE 5 circle, targets=Hostile
    FourthLegacy = 34643, // L100, instant, 1.0s CD (group 0), range 5, AOE 5 circle, targets=Hostile
    SecondLegacy = 34641, // L100, instant, 1.0s CD (group 0), range 5, AOE 5 circle, targets=Hostile
    FirstLegacy = 34640, // L100, instant, 1.0s CD (group 0), range 5, AOE 5 circle, targets=Hostile
    ThirdLegacy = 34642, // L100, instant, 1.0s CD (group 0), range 5, AOE 5 circle, targets=Hostile

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast, range 8, single-target, targets=Hostile, castAnimLock=3.860
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
    Swiftscaled = 3669, // applied by Swiftskin's Sting, Swiftskin's Bite, Swiftskin's Coil, Swiftskin's Den to self
    HonedSteel = 3672,
    HonedReavers = 3772,
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

    //Shared
    Feint = ClassShared.SID.Feint, // applied by Feint to target
    TrueNorth = ClassShared.SID.TrueNorth, // applied by True North to self
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.WorldSwallower);
        d.RegisterSpell(AID.SteelFangs);
        d.RegisterSpell(AID.HuntersSting);
        d.RegisterSpell(AID.ReavingFangs);
        d.RegisterSpell(AID.WrithingSnap);
        d.RegisterSpell(AID.SwiftskinsSting);
        d.RegisterSpell(AID.SteelMaw);
        d.RegisterSpell(AID.FlankstingStrike);
        d.RegisterSpell(AID.FlanksbaneFang);
        d.RegisterSpell(AID.HindstingStrike);
        d.RegisterSpell(AID.HindsbaneFang);
        d.RegisterSpell(AID.ReavingMaw);
        d.RegisterSpell(AID.Slither);
        d.RegisterSpell(AID.HuntersBite);
        d.RegisterSpell(AID.SwiftskinsBite);
        d.RegisterSpell(AID.JaggedMaw);
        d.RegisterSpell(AID.BloodiedMaw);
        d.RegisterSpell(AID.DeathRattle);
        d.RegisterSpell(AID.SerpentsTail);
        d.RegisterSpell(AID.LastLash);
        d.RegisterSpell(AID.Vicewinder);
        d.RegisterSpell(AID.HuntersCoil);
        d.RegisterSpell(AID.SwiftskinsCoil);
        d.RegisterSpell(AID.Vicepit);
        d.RegisterSpell(AID.SwiftskinsDen);
        d.RegisterSpell(AID.HuntersDen);
        d.RegisterSpell(AID.TwinfangBite);
        d.RegisterSpell(AID.TwinbloodBite);
        d.RegisterSpell(AID.Twinblood);
        d.RegisterSpell(AID.Twinfang);
        d.RegisterSpell(AID.TwinfangThresh);
        d.RegisterSpell(AID.TwinbloodThresh);
        d.RegisterSpell(AID.UncoiledFury);
        d.RegisterSpell(AID.SerpentsIre);
        d.RegisterSpell(AID.ThirdGeneration);
        d.RegisterSpell(AID.FourthGeneration);
        d.RegisterSpell(AID.SecondGeneration);
        d.RegisterSpell(AID.FirstGeneration);
        d.RegisterSpell(AID.Reawaken);
        d.RegisterSpell(AID.UncoiledTwinfang);
        d.RegisterSpell(AID.UncoiledTwinblood);
        d.RegisterSpell(AID.Ouroboros);
        d.RegisterSpell(AID.FourthLegacy);
        d.RegisterSpell(AID.SecondLegacy);
        d.RegisterSpell(AID.FirstLegacy);
        d.RegisterSpell(AID.ThirdLegacy);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.Slither, TraitID.EnhancedSlither);

        d.Spell(AID.Slither)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
    }
}
