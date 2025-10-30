namespace BossMod.SMN;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Teraflare = 4246, // LB3, 4.5s cast (0 charges), range 25, AOE 15 circle, targets=Area, animLock=8.100s?
    Ruin1 = 163, // L1, 1.5s cast, GCD (0 charges), range 25, single-target, targets=Hostile
    SummonCarbuncle = 25798, // L2, 1.5s cast, GCD (0 charges), range 0, single-target, targets=Self
    RadiantAegis = 25799, // L2, instant, 60.0s CD (group 20/70) (1-2 charges), range 0, single-target, targets=Self
    Physick = 16230, // L4, 1.5s cast, GCD (0 charges), range 30, single-target, targets=Self/Party/Alliance/Friendly
    Aethercharge = 25800, // L6, instant, 60.0s CD (group 9/57) (0 charges), range 0, single-target, targets=Self
    SummonRuby = 25802, // L6, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    RubyRuin1 = 25808, // L6, 2.8s cast, GCD (0 charges), range 25, single-target, targets=Hostile
    Gemshine = 25883, // L6, 2.5s cast, GCD (0 charges), range 25, single-target, targets=Hostile
    EnergyDrain = 16508, // L10, instant, 60.0s CD (group 11) (0 charges), range 25, single-target, targets=Hostile
    Fester = 181, // L10, instant, 1.0s CD (group 0) (0 charges), range 25, single-target, targets=Hostile
    SummonTopaz = 25803, // L15, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    TopazRuin1 = 25809, // L15, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    EmeraldRuin1 = 25810, // L22, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    SummonEmerald = 25804, // L22, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    Outburst = 16511, // L26, 1.5s cast, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    RubyOutburst = 25814, // L26, 2.8s cast, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    EmeraldOutburst = 25816, // L26, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    PreciousBrilliance = 25884, // L26, 2.5s cast, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    TopazOutburst = 25815, // L26, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    TopazRuin2 = 25812, // L30, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    EmeraldRuin2 = 25813, // L30, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    Ruin2 = 172, // L30, 1.5s cast, GCD (0 charges), range 25, single-target, targets=Hostile
    SummonIfrit1 = 25805, // L30, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    RubyRuin2 = 25811, // L30, 2.8s cast, GCD (0 charges), range 25, single-target, targets=Hostile
    SummonTitan1 = 25806, // L35, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    Painflare = 3578, // L40, instant, 1.0s CD (group 1) (0 charges), range 25, AOE 5 circle, targets=Hostile
    SummonGaruda1 = 25807, // L45, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    EnergySiphon = 16510, // L52, instant, 60.0s CD (group 11) (0 charges), range 25, AOE 5 circle, targets=Hostile
    RubyRuin3 = 25817, // L54, 2.8s cast, GCD (0 charges), range 25, single-target, targets=Hostile
    Ruin3 = 3579, // L54, 1.5s cast, GCD (0 charges), range 25, single-target, targets=Hostile
    TopazRuin3 = 25818, // L54, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    EmeraldRuin3 = 25819, // L54, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    DreadwyrmTrance = 3581, // L58, instant, 60.0s CD (group 9/57) (0 charges), range 0, single-target, targets=Self
    AstralImpulse = 25820, // L58, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    AstralFlare = 25821, // L58, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    Deathflare = 3582, // L60, instant, 20.0s CD (group 4) (0 charges), range 25, AOE 5 circle, targets=Hostile
    AstralFlow = 25822, // L60, instant, GCD (0 charges), range 0, single-target, targets=Self
    Ruin4 = 7426, // L62, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    SearingLight = 25801, // L66, instant, 120.0s CD (group 19) (0 charges), range 0, AOE 30 circle, targets=Self
    SummonBahamut = 7427, // L70, instant, 60.0s CD (group 9/57) (0 charges), range 25, single-target, targets=Hostile
    EnkindleBahamut = 7429, // L70, instant, 20.0s CD (group 5) (0 charges), range 25, single-target, targets=Hostile
    TopazRite = 25824, // L72, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    EmeraldRite = 25825, // L72, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    RubyRite = 25823, // L72, 2.8s cast, GCD (0 charges), range 25, single-target, targets=Hostile
    TriDisaster = 25826, // L74, 1.5s cast, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    RubyDisaster = 25827, // L74, 2.8s cast, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    TopazDisaster = 25828, // L74, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    EmeraldDisaster = 25829, // L74, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    Rekindle = 25830, // L80, instant, 20.0s CD (group 4) (0 charges), range 30, single-target, targets=Self/Party
    SummonPhoenix = 25831, // L80, instant, 60.0s CD (group 9/57) (0 charges), range 25, single-target, targets=Hostile
    EnkindlePhoenix = 16516, // L80, instant, 20.0s CD (group 5) (0 charges), range 25, single-target, targets=Hostile
    BrandOfPurgatory = 16515, // L80, instant, GCD (0 charges), range 25, AOE 8 circle, targets=Hostile
    FountainOfFire = 16514, // L80, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    RubyCatastrophe = 25832, // L82, 2.8s cast, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    TopazCatastrophe = 25833, // L82, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    EmeraldCatastrophe = 25834, // L82, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    Slipstream = 25837, // L86, 3.0s cast, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile
    MountainBuster = 25836, // L86, instant, 1.0s CD (group 2) (0 charges), range 25, AOE 5 circle, targets=Hostile
    CrimsonCyclone = 25835, // L86, instant, GCD (0 charges), range 25, AOE 5 circle, targets=Hostile, animLock=0.750
    CrimsonStrike = 25885, // L86, instant, GCD (0 charges), range 3, AOE 5 circle, targets=Hostile
    SummonIfrit2 = 25838, // L90, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    SummonTitan2 = 25839, // L90, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    SummonGaruda2 = 25840, // L90, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    Necrotize = 36990, // L92, instant, 1.0s CD (group 0) (0 charges), range 25, single-target, targets=Hostile
    SearingFlash = 36991, // L96, instant, 1.0s CD (group 3) (0 charges), range 25, AOE 5 circle, targets=Hostile, animLock=???
    UmbralImpulse = 36994, // L100, instant, GCD (0 charges), range 25, single-target, targets=Hostile, animLock=???
    UmbralFlare = 36995, // L100, instant, GCD (0 charges), range 25, AOE 8 circle, targets=Hostile, animLock=???
    Sunflare = 36996, // L100, instant, 20.0s CD (group 4) (0 charges), range 25, AOE 5 circle, targets=Hostile, animLock=???
    LuxSolaris = 36997, // L100, instant, 60.0s CD (group 12) (0 charges), range 0, AOE 15 circle, targets=Self, animLock=???
    EnkindleSolarBahamut = 36998, // L100, instant, 20.0s CD (group 5) (0 charges), range 25, single-target, targets=Hostile, animLock=???
    SummonSolarBahamut = 36992, // L100, instant, 60.0s CD (group 9/57) (0 charges), range 25, single-target, targets=Hostile, animLock=???

    // Shared
    Skyshard = ClassShared.AID.Skyshard, // LB1, 2.0s cast (0 charges), range 25, AOE 8 circle, targets=Area, animLock=3.100s?
    Starstorm = ClassShared.AID.Starstorm, // LB2, 3.0s cast (0 charges), range 25, AOE 10 circle, targets=Area, animLock=5.100s?
    Addle = ClassShared.AID.Addle, // L8, instant, 90.0s CD (group 46) (0 charges), range 25, single-target, targets=Hostile
    Sleep = ClassShared.AID.Sleep, // L10, 2.5s cast, GCD (0 charges), range 30, AOE 5 circle, targets=Hostile
    Resurrection = ClassShared.AID.Resurrection, // L12, 8.0s cast, GCD (0 charges), range 30, single-target, targets=Party/Alliance/Friendly/Dead
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 44) (0 charges), range 0, single-target, targets=Self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 43) (0 charges), range 0, single-target, targets=Self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48) (0 charges), range 0, single-target, targets=Self

    #region PvP
    Ruin3PvP = 29664,
    MountainBusterPvP = 29671,
    SlipstreamPvP = 29669,
    CrimsonCyclonePvP = 29667,
    NecrotizePvP = 41483,
    RadiantAegisPvP = 29670,
    Ruin4PvP = 41482,
    AstralImpulsePvP = 29665,
    FountainOfFirePvP = 29666,
    CrimsonStrikePvP = 29668,
    DeathflarePvP = 41484,
    BrandOfPurgatoryPvP = 41485,
    WyrmwavePvP = 29676,
    ScarletFlamePvP = 29681,
    MegaflarePvP = 29675,
    EverlastingFlightPvP = 29680,

    //LB
    SummonBahamutPvP = 29673,
    SummonPhoenixPvP = 29678,

    //Role
    CometPvP = ClassShared.AID.CometPvP,
    PhantomDartPvP = ClassShared.AID.PhantomDartPvP,
    RustPvP = ClassShared.AID.RustPvP,

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
    EnhancedAethercharge1 = 466, // L15
    MaimAndMend1 = 66, // L20
    EnhancedAethercharge2 = 467, // L22
    RuinMastery1 = 217, // L30
    RubySummoningMastery = 468, // L30
    TopazSummoningMastery = 469, // L35
    MaimAndMend2 = 69, // L40
    EmeraldSummoningMastery = 470, // L45
    Enkindle1 = 471, // L50
    RuinMastery2 = 473, // L54
    AetherchargeMastery = 474, // L58
    EnhancedEnergySiphon = 475, // L62
    EnhancedDreadwyrmTrance = 178, // L70
    RuinMastery3 = 476, // L72
    OutburstMastery1 = 477, // L74
    EnhancedSummonBahamut = 502, // L80
    OutburstMastery2 = 478, // L82
    RuinMastery4 = 479, // L84
    ElementalMastery = 503, // L86
    EnhancedRadiantAegis = 480, // L88
    Enkindle2 = 481, // L90
    EnhancedFester = 617, // L92
    EnhancedSwiftcast = 644, // L94
    ArcaneMastery = 664, // L94
    EnhancedSearingLight = 618, // L96
    EnhancedAddle = 643, // L98
    EnhancedSummonBahamutII = 619, // L100
}

public enum SID : uint
{
    None = 0,
    Sleep = 3, // applied by Sleep to target
    FurtherRuin = 2701, // applied by Energy Drain, Energy Siphon to self
    SearingLight = 2703, // applied by Searing Light to self/target
    TitansFavor = 2853, // applied by Topaz Rite, Topaz Catastrophe to self
    Rekindle = 2704, // applied by Rekindle to self
    RadiantAegis = 2702,
    Slipstream = 2706, // applied by Slipstream to self
    IfritsFavor = 2724, // applied by Summon Ifrit II to self
    GarudasFavor = 2725, // applied by Summon Garuda II to self
    RubysGlimmer = 3873, // applied by Searing Light to self
    RefulgentLux = 3874, // applied by Summon Solar Bahamut to self
    CrimsonStrikeReady = 4403, // applied by Crimson Cyclone to self
    DreadwyrmTrance = 3228, // applied to self by summoning Bahamut
    FirebirdTrance = 3229, // applied to self by summoning Phoenix

    //Shared
    Addle = ClassShared.SID.Addle, // applied by Addle to target
    Surecast = ClassShared.SID.Surecast, // applied by Surecast to self
    LucidDreaming = ClassShared.SID.LucidDreaming, // applied by Lucid Dreaming to self
    Swiftcast = ClassShared.SID.Swiftcast, // applied by Swiftcast to self

    #region PvP
    FurtherRuinPvP = 4399,
    CrimsonStrikeReadyPvP = 4400,

    //Role
    CometEquippedPvP = ClassShared.SID.CometEquippedPvP,
    PhantomDartEquippedPvP = ClassShared.SID.PhantomDartEquippedPvP,
    RustEquippedPvP = ClassShared.SID.RustEquippedPvP,

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

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Teraflare, castAnimLock: 8.10f); // animLock=8.100s?
        d.RegisterSpell(AID.Ruin1);
        d.RegisterSpell(AID.SummonCarbuncle);
        d.RegisterSpell(AID.RadiantAegis);
        d.RegisterSpell(AID.Physick);
        d.RegisterSpell(AID.Aethercharge);
        d.RegisterSpell(AID.SummonRuby);
        d.RegisterSpell(AID.RubyRuin1);
        d.RegisterSpell(AID.Gemshine);
        d.RegisterSpell(AID.EnergyDrain);
        d.RegisterSpell(AID.Fester);
        d.RegisterSpell(AID.SummonTopaz);
        d.RegisterSpell(AID.TopazRuin1);
        d.RegisterSpell(AID.EmeraldRuin1);
        d.RegisterSpell(AID.SummonEmerald);
        d.RegisterSpell(AID.Outburst);
        d.RegisterSpell(AID.RubyOutburst);
        d.RegisterSpell(AID.EmeraldOutburst);
        d.RegisterSpell(AID.PreciousBrilliance);
        d.RegisterSpell(AID.TopazOutburst);
        d.RegisterSpell(AID.TopazRuin2);
        d.RegisterSpell(AID.EmeraldRuin2);
        d.RegisterSpell(AID.Ruin2);
        d.RegisterSpell(AID.SummonIfrit1);
        d.RegisterSpell(AID.RubyRuin2);
        d.RegisterSpell(AID.SummonTitan1);
        d.RegisterSpell(AID.Painflare);
        d.RegisterSpell(AID.SummonGaruda1);
        d.RegisterSpell(AID.EnergySiphon);
        d.RegisterSpell(AID.RubyRuin3);
        d.RegisterSpell(AID.Ruin3);
        d.RegisterSpell(AID.TopazRuin3);
        d.RegisterSpell(AID.EmeraldRuin3);
        d.RegisterSpell(AID.DreadwyrmTrance);
        d.RegisterSpell(AID.AstralImpulse);
        d.RegisterSpell(AID.AstralFlare);
        d.RegisterSpell(AID.Deathflare);
        d.RegisterSpell(AID.AstralFlow);
        d.RegisterSpell(AID.Ruin4);
        d.RegisterSpell(AID.SearingLight);
        d.RegisterSpell(AID.SummonBahamut);
        d.RegisterSpell(AID.EnkindleBahamut);
        d.RegisterSpell(AID.TopazRite);
        d.RegisterSpell(AID.EmeraldRite);
        d.RegisterSpell(AID.RubyRite);
        d.RegisterSpell(AID.TriDisaster);
        d.RegisterSpell(AID.RubyDisaster);
        d.RegisterSpell(AID.TopazDisaster);
        d.RegisterSpell(AID.EmeraldDisaster);
        d.RegisterSpell(AID.Rekindle);
        d.RegisterSpell(AID.SummonPhoenix);
        d.RegisterSpell(AID.EnkindlePhoenix);
        d.RegisterSpell(AID.BrandOfPurgatory);
        d.RegisterSpell(AID.FountainOfFire);
        d.RegisterSpell(AID.RubyCatastrophe);
        d.RegisterSpell(AID.TopazCatastrophe);
        d.RegisterSpell(AID.EmeraldCatastrophe);
        d.RegisterSpell(AID.Slipstream);
        d.RegisterSpell(AID.MountainBuster);
        d.RegisterSpell(AID.CrimsonCyclone, instantAnimLock: 0.75f); // animLock=0.750
        d.RegisterSpell(AID.CrimsonStrike);
        d.RegisterSpell(AID.SummonIfrit2);
        d.RegisterSpell(AID.SummonTitan2);
        d.RegisterSpell(AID.SummonGaruda2);
        d.RegisterSpell(AID.Necrotize);
        d.RegisterSpell(AID.SearingFlash); // animLock=???
        d.RegisterSpell(AID.UmbralImpulse); // animLock=???
        d.RegisterSpell(AID.UmbralFlare); // animLock=???
        d.RegisterSpell(AID.Sunflare); // animLock=???
        d.RegisterSpell(AID.LuxSolaris); // animLock=???
        d.RegisterSpell(AID.EnkindleSolarBahamut); // animLock=???
        d.RegisterSpell(AID.SummonSolarBahamut); // animLock=???

        // PvP
        d.RegisterSpell(AID.Ruin3PvP);
        d.RegisterSpell(AID.MountainBusterPvP);
        d.RegisterSpell(AID.SlipstreamPvP);
        d.RegisterSpell(AID.CrimsonCyclonePvP);
        d.RegisterSpell(AID.NecrotizePvP);
        d.RegisterSpell(AID.RadiantAegisPvP);
        d.RegisterSpell(AID.Ruin4PvP);
        d.RegisterSpell(AID.AstralImpulsePvP);
        d.RegisterSpell(AID.FountainOfFirePvP);
        d.RegisterSpell(AID.CrimsonStrikePvP);
        d.RegisterSpell(AID.DeathflarePvP);
        d.RegisterSpell(AID.BrandOfPurgatoryPvP);
        d.RegisterSpell(AID.WyrmwavePvP);
        d.RegisterSpell(AID.ScarletFlamePvP);
        d.RegisterSpell(AID.MegaflarePvP);
        d.RegisterSpell(AID.EverlastingFlightPvP);
        d.RegisterSpell(AID.SummonBahamutPvP);
        d.RegisterSpell(AID.SummonPhoenixPvP);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.RadiantAegis, TraitID.EnhancedRadiantAegis);
        // *** add any properties that can't be autogenerated here ***

        d.Spell(AID.CrimsonCyclone)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
        d.Spell(AID.CrimsonCyclonePvP)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
    }
}
