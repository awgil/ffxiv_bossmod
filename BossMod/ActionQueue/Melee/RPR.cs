namespace BossMod.RPR;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    TheEnd = 24858, // LB3, 4.5s cast, range 8, single-target, targets=Hostile, animLock=3.700s?
    Slice = 24373, // L1, instant, GCD, range 3, single-target, targets=Hostile
    WaxingSlice = 24374, // L5, instant, GCD, range 3, single-target, targets=Hostile
    ShadowofDeath = 24378, // L10, instant, GCD, range 3, single-target, targets=Hostile
    Harpe = 24386, // L15, 1.3s cast, GCD, range 25, single-target, targets=Hostile
    HellsEgress = 24402, // L20, instant, 20.0s CD (group 4), range 0, single-target, targets=Self, animLock=0.800s?
    HellsIngress = 24401, // L20, instant, 20.0s CD (group 4), range 0, single-target, targets=Self, animLock=0.800s?
    SpinningScythe = 24376, // L25, instant, GCD, range 0, AOE 5 circle, targets=Self
    InfernalSlice = 24375, // L30, instant, GCD, range 3, single-target, targets=Hostile
    WhorlofDeath = 24379, // L35, instant, GCD, range 0, AOE 5 circle, targets=Self
    ArcaneCrest = 24404, // L40, instant, 30.0s CD (group 6), range 0, single-target, targets=Self
    NightmareScythe = 24377, // L45, instant, GCD, range 0, AOE 5 circle, targets=Self
    BloodStalk = 24389, // L50, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile
    GrimSwathe = 24392, // L55, instant, 1.0s CD (group 0), range 8, AOE 8+R ?-degree cone, targets=Hostile
    SoulSlice = 24380, // L60, instant, 30.0s CD (group 8/57) (1-2 charges), range 3, single-target, targets=Hostile
    SoulScythe = 24381, // L65, instant, 30.0s CD (group 8/57) (1-2 charges), range 0, AOE 5 circle, targets=Self
    UnveiledGibbet = 24390, // L70, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile
    Guillotine = 24384, // L70, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile
    Gibbet = 24382, // L70, instant, GCD, range 3, single-target, targets=Hostile
    Gallows = 24383, // L70, instant, GCD, range 3, single-target, targets=Hostile
    UnveiledGallows = 24391, // L70, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile
    ArcaneCircle = 24405, // L72, instant, 120.0s CD (group 21), range 0, AOE 30 circle, targets=Self
    Regress = 24403, // L74, instant, 1.0s CD (group 1), range 30, ???, targets=Area, animLock=0.800s?
    Gluttony = 24393, // L76, instant, 60.0s CD (group 12), range 25, AOE 5 circle, targets=Hostile
    GrimReaping = 24397, // L80, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile
    CrossReaping = 24396, // L80, instant, GCD, range 3, single-target, targets=Hostile
    VoidReaping = 24395, // L80, instant, GCD, range 3, single-target, targets=Hostile
    Enshroud = 24394, // L80, instant, 15.0s CD (group 3), range 0, single-target, targets=Self
    SoulSow = 24387, // L82, 5.0s cast, GCD, range 0, single-target, targets=Self
    HarvestMoon = 24388, // L82, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    LemuresSlice = 24399, // L86, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile
    LemuresScythe = 24400, // L86, instant, 1.0s CD (group 0), range 8, AOE 8+R ?-degree cone, targets=Hostile
    PlentifulHarvest = 24385, // L88, instant, GCD, range 15, AOE 15+R width 4 rect, targets=Hostile
    Communio = 24398, // L90, 1.3s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    Sacrificium = 36969, // L92, instant, 1.0s CD (group 2), range 25, AOE 5 circle, targets=Hostile, animLock=???
    ExecutionersGibbet = 36970, // L96, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    ExecutionersGallows = 36971, // L96, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    ExecutionersGuillotine = 36972, // L96, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile, animLock=???
    Perfectio = 36973, // L100, instant, GCD, range 25, AOE 5 circle, targets=Hostile, animLock=???

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
    SoulGauge = 379, // L50
    DeathScytheMastery1 = 380, // L60
    EnhancedAvatar = 381, // L70
    Hellsgate = 382, // L74
    TemperedSoul = 383, // L78
    ShroudGauge = 384, // L80
    EnhancedArcaneCrest = 385, // L84
    DeathScytheMastery2 = 523, // L84
    EnhancedShroud = 386, // L86
    EnhancedArcaneCircle = 387, // L88
    EnhancedEnshroud = 594, // L92
    EnhancedSecondWind = 642, // L94
    MeleeMasteryIII = 657, // L94
    EnhancedGluttony = 596, // L96
    EnhancedFeint = 641, // L98
    EnhancedPlentifulHarvest = 597, // L100
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    DeathsDesign = 2586,
    SoulReaver = 2587,
    ImmortalSacrifice = 2592,
    ArcaneCircle = 2599,
    EnhancedGibbet = 2588,
    EnhancedGallows = 2589,
    EnhancedVoidReaping = 2590,
    EnhancedCrossReaping = 2591,
    EnhancedHarpe = 2845,
    Enshrouded = 2593,
    Soulsow = 2594,
    Threshold = 2595,
    CircleofSacrifice = 2600,
    BloodsownCircle = 2972,
    Bloodbath = 84,
    Stun = 2,
    IdealHost = 3905,
    Oblatio = 3857,
    Executioner = 3858,
    PerfectioOcculta = 3859,
    PerfectioParata = 3860,

    //Shared
    Feint = ClassShared.SID.Feint, // applied by Feint to target
    TrueNorth = ClassShared.SID.TrueNorth, // applied by True North to self
}

public sealed class Definitions : IDisposable
{
    private readonly RPRConfig _config = Service.Config.Get<RPRConfig>();

    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.TheEnd, castAnimLock: 3.70f); // animLock=3.700s?
        d.RegisterSpell(AID.Slice);
        d.RegisterSpell(AID.WaxingSlice);
        d.RegisterSpell(AID.ShadowofDeath);
        d.RegisterSpell(AID.Harpe);
        d.RegisterSpell(AID.HellsEgress, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.HellsIngress, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.SpinningScythe);
        d.RegisterSpell(AID.InfernalSlice);
        d.RegisterSpell(AID.WhorlofDeath);
        d.RegisterSpell(AID.ArcaneCrest);
        d.RegisterSpell(AID.NightmareScythe);
        d.RegisterSpell(AID.BloodStalk);
        d.RegisterSpell(AID.GrimSwathe);
        d.RegisterSpell(AID.SoulSlice);
        d.RegisterSpell(AID.SoulScythe);
        d.RegisterSpell(AID.UnveiledGibbet);
        d.RegisterSpell(AID.Guillotine);
        d.RegisterSpell(AID.Gibbet);
        d.RegisterSpell(AID.Gallows);
        d.RegisterSpell(AID.UnveiledGallows);
        d.RegisterSpell(AID.ArcaneCircle);
        d.RegisterSpell(AID.Regress, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.Gluttony);
        d.RegisterSpell(AID.GrimReaping);
        d.RegisterSpell(AID.CrossReaping);
        d.RegisterSpell(AID.VoidReaping);
        d.RegisterSpell(AID.Enshroud);
        d.RegisterSpell(AID.SoulSow);
        d.RegisterSpell(AID.HarvestMoon);
        d.RegisterSpell(AID.LemuresSlice);
        d.RegisterSpell(AID.LemuresScythe);
        d.RegisterSpell(AID.PlentifulHarvest);
        d.RegisterSpell(AID.Communio);
        d.RegisterSpell(AID.Sacrificium); // animLock=???
        d.RegisterSpell(AID.ExecutionersGibbet); // animLock=???
        d.RegisterSpell(AID.ExecutionersGallows); // animLock=???
        d.RegisterSpell(AID.ExecutionersGuillotine); // animLock=???
        d.RegisterSpell(AID.Perfectio); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.SoulSlice, TraitID.TemperedSoul);
        d.RegisterChargeIncreaseTrait(AID.SoulScythe, TraitID.TemperedSoul);

        d.Spell(AID.Harpe)!.ForbidExecute = (ws, player, _, _) => _config.ForbidEarlyHarpe && !player.InCombat && ws.Client.CountdownRemaining > 1.7f;

        d.Spell(AID.HellsEgress)!.TransformAngle =
            d.Spell(AID.HellsIngress)!.TransformAngle = (ws, _, _, _) => _config.AlignDashToCamera
                ? ws.Client.CameraAzimuth + 180.Degrees()
                : null;

        d.Spell(AID.HellsIngress)!.ForbidExecute = ActionDefinitions.DashFixedDistanceCheck(15);
        d.Spell(AID.HellsEgress)!.ForbidExecute = ActionDefinitions.DashFixedDistanceCheck(15, backwards: true);
        d.Spell(AID.Regress)!.ForbidExecute = ActionDefinitions.DashToPositionCheck;

        // upgrades (TODO: don't think we actually care...)
        //d.Spell(AID.BloodStalk)!.TransformAction = d.Spell(AID.UnveiledGallows)!.TransformAction = d.Spell(AID.UnveiledGibbet)!.TransformAction = () => ActionID.MakeSpell(_state.Beststalk);
        //d.Spell(AID.Gibbet)!.TransformAction = d.Spell(AID.VoidReaping)!.TransformAction = () => ActionID.MakeSpell(_state.BestGibbet);
        //d.Spell(AID.Gallows)!.TransformAction = d.Spell(AID.CrossReaping)!.TransformAction = () => ActionID.MakeSpell(_state.BestGallow);
        //d.Spell(AID.SoulSow)!.TransformAction = d.Spell(AID.HarvestMoon)!.TransformAction = () => ActionID.MakeSpell(_state.BestSow);
    }
}
