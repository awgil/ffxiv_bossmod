namespace BossMod.RDM;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    VermilionScourge = 7862, // LB3, 4.5s cast, range 25, AOE 15 circle, targets=area, animLock=???, castAnimLock=8.100
    EnchantedRiposte = 7527, // L1, instant, GCD, range 3, single-target, targets=hostile
    Riposte = 7504, // L1, instant, GCD, range 3, single-target, targets=hostile
    Jolt = 7503, // L2, 2.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Verthunder = 7505, // L4, 5.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    CorpsACorps = 7506, // L6, instant, 35.0s CD (group 10/70) (2? charges), range 25, single-target, targets=hostile
    Veraero = 7507, // L10, 5.0s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Scatter = 7509, // L15, 5.0s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    VerthunderII = 16524, // L18, 2.0s cast, GCD, range 25, AOE 5 circle, targets=hostile
    VeraeroII = 16525, // L22, 2.0s cast, GCD, range 25, AOE 5 circle, targets=hostile
    Verfire = 7510, // L26, 2.0s cast, GCD, range 25, single-target, targets=hostile
    Verstone = 7511, // L30, 2.0s cast, GCD, range 25, single-target, targets=hostile
    Zwerchhau = 7512, // L35, instant, GCD, range 3, single-target, targets=hostile
    EnchantedZwerchhau = 7528, // L35, instant, GCD, range 3, single-target, targets=hostile
    Engagement = 16527, // L40, instant, 35.0s CD (group 12/71) (2? charges), range 3, single-target, targets=hostile
    Displacement = 7515, // L40, instant, 35.0s CD (group 12/71) (2? charges), range 5, single-target, targets=hostile, animLock=0.800
    Fleche = 7517, // L45, instant, 25.0s CD (group 4), range 25, single-target, targets=hostile
    Acceleration = 7518, // L50, instant, 55.0s CD (group 19/72), range 0, single-target, targets=self
    Redoublement = 7516, // L50, instant, GCD, range 3, single-target, targets=hostile
    EnchantedRedoublement = 7529, // L50, instant, GCD, range 3, single-target, targets=hostile
    Moulinet = 7513, // L52, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=hostile, animLock=???
    EnchantedMoulinet = 7530, // L52, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=hostile
    Vercure = 7514, // L54, 2.0s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    ContreSixte = 7519, // L56, instant, 45.0s CD (group 7), range 25, AOE 6 circle, targets=hostile
    Embolden = 7520, // L58, instant, 120.0s CD (group 20), range 0, AOE 30 circle, targets=self
    Manafication = 7521, // L60, instant, 120.0s CD (group 21), range 0, single-target, targets=self
    JoltII = 7524, // L62, 2.0s cast, GCD, range 25, single-target, targets=hostile
    Verraise = 7523, // L64, 10.0s cast, GCD, range 30, single-target, targets=party/alliance/friendly
    Impact = 16526, // L66, 5.0s cast, GCD, range 25, AOE 5 circle, targets=hostile
    Verflare = 7525, // L68, instant, GCD, range 25, AOE 5 circle, targets=hostile
    Verholy = 7526, // L70, instant, GCD, range 25, AOE 5 circle, targets=hostile
    EnchantedReprise = 16528, // L76, instant, GCD, range 25, single-target, targets=hostile
    Reprise = 16529, // L76, instant, GCD, range 3, single-target, targets=hostile, animLock=???
    Scorch = 16530, // L80, instant, GCD, range 25, AOE 5 circle, targets=hostile
    VerthunderIII = 25855, // L82, 5.0s cast, GCD, range 25, single-target, targets=hostile
    VeraeroIII = 25856, // L82, 5.0s cast, GCD, range 25, single-target, targets=hostile
    MagickBarrier = 25857, // L86, instant, 120.0s CD (group 23), range 0, AOE 30 circle, targets=self
    Resolution = 25858, // L90, instant, GCD, range 25, AOE 25+R width 4 rect, targets=hostile

    // Shared
    Skyshard = ClassShared.AID.Skyshard, // LB1, 2.0s cast, range 25, AOE 8 circle, targets=area, castAnimLock=3.100
    Starstorm = ClassShared.AID.Starstorm, // LB2, 3.0s cast, range 25, AOE 10 circle, targets=area, castAnimLock=5.100
    Addle = ClassShared.AID.Addle, // L8, instant, 90.0s CD (group 46), range 25, single-target, targets=hostile
    Sleep = ClassShared.AID.Sleep, // L10, 2.5s cast, GCD, range 30, AOE 5 circle, targets=hostile
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 45), range 0, single-target, targets=self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 44), range 0, single-target, targets=self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=self
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
}

public enum SID : uint
{
    Embolden = 1297,
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.VermilionScourge, castAnimLock: 8.10f); // animLock=???, castAnimLock=8.100
        d.RegisterSpell(AID.EnchantedRiposte);
        d.RegisterSpell(AID.Riposte);
        d.RegisterSpell(AID.Jolt); // animLock=???
        d.RegisterSpell(AID.Verthunder); // animLock=???
        d.RegisterSpell(AID.CorpsACorps);
        d.RegisterSpell(AID.Veraero); // animLock=???
        d.RegisterSpell(AID.Scatter); // animLock=???
        d.RegisterSpell(AID.VerthunderII);
        d.RegisterSpell(AID.VeraeroII);
        d.RegisterSpell(AID.Verfire);
        d.RegisterSpell(AID.Verstone);
        d.RegisterSpell(AID.Zwerchhau);
        d.RegisterSpell(AID.EnchantedZwerchhau);
        d.RegisterSpell(AID.Engagement);
        d.RegisterSpell(AID.Displacement, instantAnimLock: 0.80f); // animLock=0.800
        d.RegisterSpell(AID.Fleche);
        d.RegisterSpell(AID.Acceleration);
        d.RegisterSpell(AID.Redoublement);
        d.RegisterSpell(AID.EnchantedRedoublement);
        d.RegisterSpell(AID.Moulinet); // animLock=???
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
        d.RegisterSpell(AID.EnchantedReprise);
        d.RegisterSpell(AID.Reprise); // animLock=???
        d.RegisterSpell(AID.Scorch);
        d.RegisterSpell(AID.VerthunderIII);
        d.RegisterSpell(AID.VeraeroIII);
        d.RegisterSpell(AID.MagickBarrier);
        d.RegisterSpell(AID.Resolution);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.Acceleration, TraitID.EnhancedAcceleration);
        // *** add any properties that can't be autogenerated here ***
    }
}
