namespace BossMod.MCH;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    SatelliteBeam = 4245, // LB3, 4.5s cast, range 30, AOE 30+R width 8 rect, targets=hostile, animLock=???, castAnimLock=3.700
    SplitShot = 2866, // L1, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    SlugShot = 2868, // L2, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    HotShot = 2872, // L4, instant, 40.0s CD (group 7/57), range 25, single-target, targets=hostile
    Reassemble = 2876, // L10, instant, 55.0s CD (group 21/72), range 0, single-target, targets=self
    GaussRound = 2874, // L15, instant, 30.0s CD (group 9/70) (2? charges), range 25, single-target, targets=hostile
    SpreadShot = 2870, // L18, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=hostile, animLock=???
    CleanShot = 2873, // L26, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    Hypercharge = 17209, // L30, instant, 10.0s CD (group 4), range 0, single-target, targets=self
    HeatBlast = 7410, // L35, instant, GCD, range 25, single-target, targets=hostile
    RookAutoturret = 2864, // L40, instant, 6.0s CD (group 3), range 0, single-target, targets=self
    RookOverdrive = 7415, // L40, instant, 15.0s CD (group 3), range 25, single-target, targets=self, animLock=???
    Detonator = 16766, // L45, instant, 1.0s CD (group 0), range 25, single-target, targets=self
    Wildfire = 2878, // L45, instant, 120.0s CD (group 19), range 25, single-target, targets=hostile
    Ricochet = 2890, // L50, instant, 30.0s CD (group 10/71) (2? charges), range 25, AOE 5 circle, targets=hostile
    AutoCrossbow = 16497, // L52, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=hostile
    HeatedSplitShot = 7411, // L54, instant, GCD, range 25, single-target, targets=hostile
    Tactician = 16889, // L56, instant, 120.0s CD (group 23), range 0, AOE 30 circle, targets=self
    Drill = 16498, // L58, instant, 20.0s CD (group 6/57), range 25, single-target, targets=hostile
    HeatedSlugShot = 7412, // L60, instant, GCD, range 25, single-target, targets=hostile
    Dismantle = 2887, // L62, instant, 120.0s CD (group 18), range 25, single-target, targets=hostile
    HeatedCleanShot = 7413, // L64, instant, GCD, range 25, single-target, targets=hostile
    BarrelStabilizer = 7414, // L66, instant, 120.0s CD (group 20), range 0, single-target, targets=self
    Flamethrower = 7418, // L70, instant, 60.0s CD (group 12/57), range 0, single-target, targets=self
    Bioblaster = 16499, // L72, instant, 20.0s CD (group 6/57), range 12, AOE 12+R ?-degree cone, targets=hostile
    AirAnchor = 16500, // L76, instant, 40.0s CD (group 8/57), range 25, single-target, targets=hostile
    QueenOverdrive = 16502, // L80, instant, 15.0s CD (group 1), range 30, single-target, targets=self
    AutomatonQueen = 16501, // L80, instant, 6.0s CD (group 1), range 0, single-target, targets=self
    Scattergun = 25786, // L82, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=hostile
    ChainSaw = 25788, // L90, instant, 60.0s CD (group 11/57), range 25, AOE 25+R width 4 rect, targets=hostile

    // Shared
    BigShot = ClassShared.AID.BigShot, // LB1, 2.0s cast, range 30, AOE 30+R width 4 rect, targets=hostile, castAnimLock=3.100
    Desperado = ClassShared.AID.Desperado, // LB2, 3.0s cast, range 30, AOE 30+R width 5 rect, targets=hostile, castAnimLock=3.100
    LegGraze = ClassShared.AID.LegGraze, // L6, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=self
    FootGraze = ClassShared.AID.FootGraze, // L10, instant, 30.0s CD (group 41), range 25, single-target, targets=hostile
    Peloton = ClassShared.AID.Peloton, // L20, instant, 5.0s CD (group 40), range 0, AOE 30 circle, targets=self
    HeadGraze = ClassShared.AID.HeadGraze, // L24, instant, 30.0s CD (group 43), range 25, single-target, targets=hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
}

public enum TraitID : uint
{
    None = 0,
    IncreasedActionDamage = 117, // L20
    IncreasedActionDamageII = 119, // L40
    SplitShotMastery = 288, // L54
    SlugShotMastery = 289, // L60
    CleanShotMastery = 290, // L64
    ChargedActionMastery = 292, // L74
    HotShotMastery = 291, // L76
    EnhancedWildfire = 293, // L78
    Promotion = 294, // L80
    SpreadShotMastery = 449, // L82
    EnhancedReassemble = 450, // L84
    MarksmansMastery = 517, // L84
    QueensGambit = 451, // L86
    EnhancedTactician = 452, // L88
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.SatelliteBeam, true, castAnimLock: 3.70f); // animLock=???, castAnimLock=3.700
        d.RegisterSpell(AID.SplitShot, true); // animLock=???
        d.RegisterSpell(AID.SlugShot, true); // animLock=???
        d.RegisterSpell(AID.HotShot, true);
        d.RegisterSpell(AID.Reassemble, true);
        d.RegisterSpell(AID.GaussRound, true, maxCharges: 2);
        d.RegisterSpell(AID.SpreadShot, true); // animLock=???
        d.RegisterSpell(AID.CleanShot, true); // animLock=???
        d.RegisterSpell(AID.Hypercharge, true);
        d.RegisterSpell(AID.HeatBlast, true);
        d.RegisterSpell(AID.RookAutoturret, true);
        d.RegisterSpell(AID.RookOverdrive, true); // animLock=???
        d.RegisterSpell(AID.Detonator, true);
        d.RegisterSpell(AID.Wildfire, true);
        d.RegisterSpell(AID.Ricochet, true, maxCharges: 2);
        d.RegisterSpell(AID.AutoCrossbow, true);
        d.RegisterSpell(AID.HeatedSplitShot, true);
        d.RegisterSpell(AID.Tactician, true);
        d.RegisterSpell(AID.Drill, true);
        d.RegisterSpell(AID.HeatedSlugShot, true);
        d.RegisterSpell(AID.Dismantle, true);
        d.RegisterSpell(AID.HeatedCleanShot, true);
        d.RegisterSpell(AID.BarrelStabilizer, true);
        d.RegisterSpell(AID.Flamethrower, true);
        d.RegisterSpell(AID.Bioblaster, true);
        d.RegisterSpell(AID.AirAnchor, true);
        d.RegisterSpell(AID.QueenOverdrive, true);
        d.RegisterSpell(AID.AutomatonQueen, true);
        d.RegisterSpell(AID.Scattergun, true);
        d.RegisterSpell(AID.ChainSaw, true);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}
