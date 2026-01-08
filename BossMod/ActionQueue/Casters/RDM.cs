namespace BossMod.RDM;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    VermilionScourge = 7862, // LB3, 4.5s cast, range 25, AOE 15 circle, targets=Area, animLock=8.100s?
    Riposte = 7504, // L1, instant, GCD, range 3, single-target, targets=Hostile
    EnchantedRiposte = 7527, // L1, instant, GCD, range 3, single-target, targets=Hostile
    Jolt = 7503, // L2, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    Verthunder = 7505, // L4, 5.0s cast, GCD, range 25, single-target, targets=Hostile
    CorpsACorps = 7506, // L6, instant, 35.0s CD (group 10/70) (2 charges), range 25, single-target, targets=Hostile
    Veraero = 7507, // L10, 5.0s cast, GCD, range 25, single-target, targets=Hostile
    Scatter = 7509, // L15, 5.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    VerthunderII = 16524, // L18, 2.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    VeraeroII = 16525, // L22, 2.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    Verfire = 7510, // L26, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    Verstone = 7511, // L30, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    Zwerchhau = 7512, // L35, instant, GCD, range 3, single-target, targets=Hostile
    EnchantedZwerchhau = 7528, // L35, instant, GCD, range 3, single-target, targets=Hostile
    Engagement = 16527, // L40, instant, 35.0s CD (group 9/71) (2 charges), range 3, single-target, targets=Hostile
    Displacement = 7515, // L40, instant, 35.0s CD (group 9/71) (2 charges), range 5, single-target, targets=Hostile, animLock=0.800s?
    Fleche = 7517, // L45, instant, 25.0s CD (group 4), range 25, single-target, targets=Hostile
    EnchantedRedoublement = 7529, // L50, instant, GCD, range 3, single-target, targets=Hostile
    Acceleration = 7518, // L50, instant, 55.0s CD (group 19/72) (1-2 charges), range 0, single-target, targets=Self
    Redoublement = 7516, // L50, instant, GCD, range 3, single-target, targets=Hostile
    EnchantedMoulinetTrois = 37003, // L52, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile, animLock=???
    EnchantedMoulinetDeux = 37002, // L52, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile, animLock=???
    Moulinet = 7513, // L52, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile
    EnchantedMoulinet = 7530, // L52, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile
    Vercure = 7514, // L54, 2.0s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    ContreSixte = 7519, // L56, instant, 45.0s CD (group 7), range 25, AOE 6 circle, targets=Hostile
    Embolden = 7520, // L58, instant, 120.0s CD (group 20), range 0, AOE 30 circle, targets=Self
    Manafication = 7521, // L60, instant, 120.0s CD (group 21), range 0, single-target, targets=Self
    JoltII = 7524, // L62, 2.0s cast, GCD, range 25, single-target, targets=Hostile
    Verraise = 7523, // L64, 10.0s cast, GCD, range 30, single-target, targets=Party/Alliance/Friendly/Dead
    Impact = 16526, // L66, 5.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    Verflare = 7525, // L68, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    Verholy = 7526, // L70, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    Reprise = 16529, // L76, instant, GCD, range 3, single-target, targets=Hostile
    EnchantedReprise = 16528, // L76, instant, GCD, range 25, single-target, targets=Hostile
    Scorch = 16530, // L80, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    VerthunderIII = 25855, // L82, 5.0s cast, GCD, range 25, single-target, targets=Hostile
    VeraeroIII = 25856, // L82, 5.0s cast, GCD, range 25, single-target, targets=Hostile
    JoltIII = 37004, // L84, 2.0s cast, GCD, range 25, single-target, targets=Hostile, animLock=???
    MagickBarrier = 25857, // L86, instant, 120.0s CD (group 18), range 0, AOE 30 circle, targets=Self
    Resolution = 25858, // L90, instant, GCD, range 25, AOE 25+R width 4 rect, targets=Hostile
    ViceOfThorns = 37005, // L92, instant, 1.0s CD (group 0), range 25, AOE 5 circle, targets=Hostile, animLock=???
    GrandImpact = 37006, // L96, instant, GCD, range 25, AOE 5 circle, targets=Hostile, animLock=???
    Prefulgence = 37007, // L100, instant, 1.0s CD (group 1), range 25, AOE 5 circle, targets=Hostile, animLock=???

    // Shared
    Skyshard = ClassShared.AID.Skyshard, // LB1, 2.0s cast, range 25, AOE 8 circle, targets=Area, animLock=3.100s?
    Starstorm = ClassShared.AID.Starstorm, // LB2, 3.0s cast, range 25, AOE 10 circle, targets=Area, animLock=5.100s?
    Addle = ClassShared.AID.Addle, // L8, instant, 90.0s CD (group 46), range 25, single-target, targets=Hostile
    Sleep = ClassShared.AID.Sleep, // L10, 2.5s cast, GCD, range 30, AOE 5 circle, targets=Hostile
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 44), range 0, single-target, targets=Self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 43), range 0, single-target, targets=Self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    Dualcast = 216, // L1
    MaimAndMend = 200, // L20
    MaimAndMendII = 201, // L40
    EnhancedJolt = 195, // L62
    ScatterMastery = 303, // L66
    ManaStack = 482, // L68
    EnhancedDisplacement = 304, // L72
    RedMagicMastery = 306, // L74
    EnhancedManafication = 305, // L78
    RedMagicMasteryII = 483, // L82
    RedMagicMasteryIII = 484, // L84
    EnhancedAcceleration = 485, // L88
    EnhancedManaficationII = 486, // L90
    EnhancedEmbolden = 620, // L92
    EnhancedSwiftcast = 644, // L94
    EnchantedBladeMastery = 652, // L94
    EnhancedAccelerationII = 621, // L96
    EnhancedAddle = 643, // L98
    EnhancedManaficationIII = 622, // L100
}

public enum SID : uint
{
    None = 0,
    VerfireReady = 1234,
    VerstoneReady = 1235,
    Acceleration = 1238,
    EmboldenSelf = 1239,
    Dualcast = 1249,
    Embolden = 1297,
    Manafication = 1971,
    MagickedSwordplay = 3875,
    ThornedFlourish = 3876,
    GrandImpactReady = 3877,
    PrefulgenceReady = 3878,

    //Shared
    Addle = ClassShared.SID.Addle, // applied by Addle to target
    Surecast = ClassShared.SID.Surecast, // applied by Surecast to self
    LucidDreaming = ClassShared.SID.LucidDreaming, // applied by Lucid Dreaming to self
    Swiftcast = ClassShared.SID.Swiftcast, // applied by Swiftcast to self
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.VermilionScourge, castAnimLock: 8.10f); // animLock=8.100s?
        d.RegisterSpell(AID.Riposte);
        d.RegisterSpell(AID.EnchantedRiposte);
        d.RegisterSpell(AID.Jolt);
        d.RegisterSpell(AID.Verthunder);
        d.RegisterSpell(AID.CorpsACorps);
        d.RegisterSpell(AID.Veraero);
        d.RegisterSpell(AID.Scatter);
        d.RegisterSpell(AID.VerthunderII);
        d.RegisterSpell(AID.VeraeroII);
        d.RegisterSpell(AID.Verfire);
        d.RegisterSpell(AID.Verstone);
        d.RegisterSpell(AID.Zwerchhau);
        d.RegisterSpell(AID.EnchantedZwerchhau);
        d.RegisterSpell(AID.Engagement);
        d.RegisterSpell(AID.Displacement, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.Fleche);
        d.RegisterSpell(AID.EnchantedRedoublement);
        d.RegisterSpell(AID.Acceleration);
        d.RegisterSpell(AID.Redoublement);
        d.RegisterSpell(AID.EnchantedMoulinetTrois); // animLock=???
        d.RegisterSpell(AID.EnchantedMoulinetDeux); // animLock=???
        d.RegisterSpell(AID.Moulinet);
        d.RegisterSpell(AID.EnchantedMoulinet);
        d.RegisterSpell(AID.Vercure);
        d.RegisterSpell(AID.ContreSixte);
        d.RegisterSpell(AID.Embolden);
        d.RegisterSpell(AID.Manafication);
        d.RegisterSpell(AID.JoltII);
        d.RegisterSpell(AID.Verraise);
        d.RegisterSpell(AID.Impact);
        d.RegisterSpell(AID.Verflare);
        d.RegisterSpell(AID.Verholy);
        d.RegisterSpell(AID.Reprise);
        d.RegisterSpell(AID.EnchantedReprise);
        d.RegisterSpell(AID.Scorch);
        d.RegisterSpell(AID.VerthunderIII);
        d.RegisterSpell(AID.VeraeroIII);
        d.RegisterSpell(AID.JoltIII); // animLock=???
        d.RegisterSpell(AID.MagickBarrier);
        d.RegisterSpell(AID.Resolution);
        d.RegisterSpell(AID.ViceOfThorns); // animLock=???
        d.RegisterSpell(AID.GrandImpact); // animLock=???
        d.RegisterSpell(AID.Prefulgence); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.Acceleration, TraitID.EnhancedAcceleration);
        // *** add any properties that can't be autogenerated here ***

        d.Spell(AID.CorpsACorps)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
        d.Spell(AID.Displacement)!.ForbidExecute = ActionDefinitions.BackdashCheck(15);
    }
}
