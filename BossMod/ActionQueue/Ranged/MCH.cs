using BossMod.Interfaces;

namespace BossMod.MCH;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    SatelliteBeam = 4245, // LB3, 4.5s cast, range 30, AOE 30+R width 8 rect, targets=Hostile, animLock=3.700s?
    SplitShot = 2866, // L1, instant, GCD, range 25, single-target, targets=Hostile
    SlugShot = 2868, // L2, instant, GCD, range 25, single-target, targets=Hostile
    HotShot = 2872, // L4, instant, 40.0s CD (group 7/57), range 25, single-target, targets=Hostile
    Reassemble = 2876, // L10, instant, 55.0s CD (group 17/72), range 0, single-target, targets=Self
    GaussRound = 2874, // L15, instant, 30.0s CD (group 14/70) (2-3 charges), range 25, single-target, targets=Hostile
    SpreadShot = 2870, // L18, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=Hostile
    CleanShot = 2873, // L26, instant, GCD, range 25, single-target, targets=Hostile
    Hypercharge = 17209, // L30, instant, 10.0s CD (group 1), range 0, single-target, targets=Self
    HeatBlast = 7410, // L35, instant, GCD, range 25, single-target, targets=Hostile
    RookAutoturret = 2864, // L40, instant, 6.0s CD (group 2), range 0, single-target, targets=Self
    RookOverdrive = 7415, // L40, instant, 15.0s CD (group 3), range 25, single-target, targets=Self
    Detonator = 16766, // L45, instant, 1.0s CD (group 0), range 25, single-target, targets=Self
    Wildfire = 2878, // L45, instant, 120.0s CD (group 19), range 25, single-target, targets=Hostile
    Ricochet = 2890, // L50, instant, 30.0s CD (group 15/71) (2-3 charges), range 25, AOE 5 circle, targets=Hostile
    AutoCrossbow = 16497, // L52, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=Hostile
    HeatedSplitShot = 7411, // L54, instant, GCD, range 25, single-target, targets=Hostile
    Tactician = 16889, // L56, instant, 120.0s CD (group 21), range 0, AOE 30 circle, targets=Self
    Drill = 16498, // L58, instant, 20.0s CD (group 4/57) (1-2 charges), range 25, single-target, targets=Hostile
    HeatedSlugShot = 7412, // L60, instant, GCD, range 25, single-target, targets=Hostile
    Dismantle = 2887, // L62, instant, 120.0s CD (group 18), range 25, single-target, targets=Hostile
    HeatedCleanShot = 7413, // L64, instant, GCD, range 25, single-target, targets=Hostile
    BarrelStabilizer = 7414, // L66, instant, 120.0s CD (group 20), range 0, single-target, targets=Self
    BlazingShot = 36978, // L68, instant, GCD, range 25, single-target, targets=Hostile
    Flamethrower = 7418, // L70, instant, 60.0s CD (group 12/57), range 0, single-target, targets=Self
    Bioblaster = 16499, // L72, instant, 20.0s CD (group 4/57) (1-2 charges), range 12, AOE 12+R ?-degree cone, targets=Hostile
    AirAnchor = 16500, // L76, instant, 40.0s CD (group 8/57), range 25, single-target, targets=Hostile
    AutomatonQueen = 16501, // L80, instant, 6.0s CD (group 2), range 0, single-target, targets=Self
    QueenOverdrive = 16502, // L80, instant, 15.0s CD (group 3), range 30, single-target, targets=Self
    Scattergun = 25786, // L82, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=Hostile
    ChainSaw = 25788, // L90, instant, 60.0s CD (group 11/57), range 25, AOE 25+R width 4 rect, targets=Hostile
    DoubleCheck = 36979, // L92, instant, 30.0s CD (group 14/70) (3 charges), range 25, AOE 5 circle, targets=Hostile
    Checkmate = 36980, // L92, instant, 30.0s CD (group 15/71) (3 charges), range 25, AOE 5 circle, targets=Hostile
    Excavator = 36981, // L96, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    FullMetalField = 36982, // L100, instant, GCD, range 25, AOE 5 circle, targets=Hostile

    // Shared
    BigShot = ClassShared.AID.BigShot, // LB1, 2.0s cast, range 30, AOE 30+R width 4 rect, targets=Hostile, animLock=3.100s?
    Desperado = ClassShared.AID.Desperado, // LB2, 3.0s cast, range 30, AOE 30+R width 5 rect, targets=Hostile, animLock=3.100s?
    LegGraze = ClassShared.AID.LegGraze, // L6, instant, 30.0s CD (group 42), range 25, single-target, targets=Hostile
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=Self
    FootGraze = ClassShared.AID.FootGraze, // L10, instant, 30.0s CD (group 41), range 25, single-target, targets=Hostile
    Peloton = ClassShared.AID.Peloton, // L20, instant, 5.0s CD (group 40), range 0, AOE 30 circle, targets=Self
    HeadGraze = ClassShared.AID.HeadGraze, // L24, instant, 30.0s CD (group 43), range 25, single-target, targets=Hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=Self

    #region PvP
    BlastChargePvP = 29402,
    ScattergunPvP = 29404,
    DrillPvP = 29405,
    BioblasterPvP = 29406,
    AirAnchorPvP = 29407,
    ChainSawPvP = 29408,
    WildfirePvP = 29409,
    BishopAutoturretPvP = 29412,
    AetherMortarPvP = 29413,
    AnalysisPvP = 29414,
    BlazingShotPvP = 41468,
    FullMetalFieldPvP = 41469,
    DetonatorPvP = 41470,

    //LB
    MarksmansSpitePvP = 29415,

    //Role
    DervishPvP = ClassShared.AID.DervishPvP,
    BraveryPvP = ClassShared.AID.BraveryPvP,
    EagleEyeShotPvP = ClassShared.AID.EagleEyeShotPvP,

    //Shared
    ElixirPvP = ClassShared.AID.ElixirPvP,
    RecuperatePvP = ClassShared.AID.RecuperatePvP,
    PurifyPvP = ClassShared.AID.PurifyPvP,
    GuardPvP = ClassShared.AID.GuardPvP,
    SprintPvP = ClassShared.AID.SprintPvP
    #endregion
}

public enum TraitID : uint
{
    None = 0,
    IncreasedActionDamage = 117, // L20
    IncreasedActionDamageII = 119, // L40
    SplitShotMastery = 288, // L54
    SlugShotMastery = 289, // L60
    CleanShotMastery = 290, // L64
    HeatBlastMastery = 603, // L68
    ChargedActionMastery = 292, // L74
    HotShotMastery = 291, // L76
    EnhancedWildfire = 293, // L78
    Promotion = 294, // L80
    SpreadShotMastery = 449, // L82
    MarksmansMastery = 517, // L84
    EnhancedReassemble = 450, // L84
    QueensGambit = 451, // L86
    EnhancedTactician = 452, // L88
    DoubleBarrelMastery = 604, // L92
    EnhancedSecondWind = 642, // L94
    EnhancedMultiweapon = 605, // L94
    MarksmansMasteryII = 658, // L94
    EnhancedMultiweaponII = 606, // L96
    EnhancedTacticianII = 607, // L98
    EnhancedBarrelStabilizer = 608, // L100
}

public enum SID : uint
{
    None = 0,
    Reassembled = 851, // applied by Reassemble to self
    Overheated = 2688, // applied by Hypercharge to self
    WildfirePlayer = 1946, // applied by Wildfire to self
    WildfireTarget = 861, // applied by Wildfire to target
    Dismantled = 860, // applied by Dismantle to target
    Hypercharged = 3864, // applied by Barrel Stabilizer to self
    Flamethrower = 1205, // applied by Flamethrower to self
    ExcavatorReady = 3865, // applied by Chain Saw to self
    FullMetalMachinist = 3866, // applied by Hypercharge to self
    Tactician = 1951, // applied by Tactician to self
    Bioblaster = 1866, // applied by Bioblaster to target

    //Shared
    Peloton = ClassShared.SID.Peloton, // applied by Peloton to self/party

    #region PvP
    HeatPvP = 3148,
    OverheatedPvP = 3149,
    DrillPrimed = 3150,
    BioblasterPrimed = 3151,
    AirAnchorPrimed = 3152,
    ChainSawPrimed = 3153,
    AnalysisPvP = 3158,
    WildfirePlayerPvP = 2018,
    WildfireTargetPvP = 1323,

    //Role
    DervishEquippedPvP = ClassShared.SID.DervishEquippedPvP,
    BraveryEquippedPvP = ClassShared.SID.BraveryEquippedPvP,
    EagleEyeShotEquippedPvP = ClassShared.SID.EagleEyeShotEquippedPvP,

    //Shared
    GuardPvP = ClassShared.SID.GuardPvP,
    SprintPvP = ClassShared.SID.SprintPvP,
    SilencePvP = ClassShared.SID.SilencePvP,
    BindPvP = ClassShared.SID.BindPvP,
    StunPvP = ClassShared.SID.StunPvP,
    HalfAsleepPvP = ClassShared.SID.HalfAsleepPvP,
    SleepPvP = ClassShared.SID.SleepPvP,
    DeepFreezePvP = ClassShared.SID.DeepFreezePvP,
    HeavyPvP = ClassShared.SID.HeavyPvP,
    UnguardedPvP = ClassShared.SID.UnguardedPvP,
    #endregion
}

public sealed class Definitions : IDefinitions
{
    public void Initialize(ActionDefinitions defs)
    {
        defs.RegisterSpell(AID.SatelliteBeam, true, castAnimLock: 3.70f); // animLock=3.700s?
        defs.RegisterSpell(AID.SplitShot, true);
        defs.RegisterSpell(AID.SlugShot, true);
        defs.RegisterSpell(AID.HotShot, true);
        defs.RegisterSpell(AID.Reassemble, true);
        defs.RegisterSpell(AID.GaussRound, true);
        defs.RegisterSpell(AID.SpreadShot, true);
        defs.RegisterSpell(AID.CleanShot, true);
        defs.RegisterSpell(AID.Hypercharge, true);
        defs.RegisterSpell(AID.HeatBlast, true);
        defs.RegisterSpell(AID.RookAutoturret, true);
        defs.RegisterSpell(AID.RookOverdrive, true);
        defs.RegisterSpell(AID.Detonator, true);
        defs.RegisterSpell(AID.Wildfire, true);
        defs.RegisterSpell(AID.Ricochet, true);
        defs.RegisterSpell(AID.AutoCrossbow, true);
        defs.RegisterSpell(AID.HeatedSplitShot, true);
        defs.RegisterSpell(AID.Tactician, true);
        defs.RegisterSpell(AID.Drill, true);
        defs.RegisterSpell(AID.HeatedSlugShot, true);
        defs.RegisterSpell(AID.Dismantle, true);
        defs.RegisterSpell(AID.HeatedCleanShot, true);
        defs.RegisterSpell(AID.BarrelStabilizer, true);
        defs.RegisterSpell(AID.BlazingShot, true);
        defs.RegisterSpell(AID.Flamethrower, true);
        defs.RegisterSpell(AID.Bioblaster, true);
        defs.RegisterSpell(AID.AirAnchor, true);
        defs.RegisterSpell(AID.AutomatonQueen, true);
        defs.RegisterSpell(AID.QueenOverdrive, true);
        defs.RegisterSpell(AID.Scattergun, true);
        defs.RegisterSpell(AID.ChainSaw, true);
        defs.RegisterSpell(AID.DoubleCheck, true); // animLock=???
        defs.RegisterSpell(AID.Checkmate, true); // animLock=???
        defs.RegisterSpell(AID.Excavator, true); // animLock=???
        defs.RegisterSpell(AID.FullMetalField, true); // animLock=???

        // PvP
        defs.RegisterSpell(AID.BlastChargePvP, true);
        defs.RegisterSpell(AID.ScattergunPvP, true);
        defs.RegisterSpell(AID.DrillPvP, true);
        defs.RegisterSpell(AID.BioblasterPvP, true);
        defs.RegisterSpell(AID.AirAnchorPvP, true);
        defs.RegisterSpell(AID.ChainSawPvP, true);
        defs.RegisterSpell(AID.WildfirePvP, true);
        defs.RegisterSpell(AID.BishopAutoturretPvP, true);
        defs.RegisterSpell(AID.AetherMortarPvP, true);
        defs.RegisterSpell(AID.AnalysisPvP, true);
        defs.RegisterSpell(AID.MarksmansSpitePvP, true);
        defs.RegisterSpell(AID.BlazingShotPvP, true);
        defs.RegisterSpell(AID.FullMetalFieldPvP, true);
        defs.RegisterSpell(AID.DetonatorPvP, true);

        Customize(defs);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.Drill, TraitID.EnhancedMultiweapon);
        d.RegisterChargeIncreaseTrait(AID.Bioblaster, TraitID.EnhancedMultiweapon);

        d.RegisterChargeIncreaseTrait(AID.Reassemble, TraitID.EnhancedReassemble);

        // checkmate/double check have 3 max charges in sheets so don't need to add here
        d.RegisterChargeIncreaseTrait(AID.GaussRound, TraitID.ChargedActionMastery);
        d.RegisterChargeIncreaseTrait(AID.Ricochet, TraitID.ChargedActionMastery);
    }
}
