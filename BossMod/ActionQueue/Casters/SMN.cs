namespace BossMod.SMN;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Teraflare = 4246, // LB3, 4.5s cast, range 25, AOE 15 circle, targets=area, animLock=???, castAnimLock=8.100
    Ruin1 = 163, // L1, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    RadiantAegis = 25799, // L2, instant, 60.0s CD (group 20/70), range 0, single-target, targets=self
    SummonCarbuncle = 25798, // L2, 1.5s cast, GCD, range 0, single-target, targets=self
    Physick = 16230, // L4, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    RubyRuin1 = 25808, // L6, 2.8s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    SummonRuby = 25802, // L6, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    Aethercharge = 25800, // L6, instant, 60.0s CD (group 6/57), range 0, single-target, targets=self, animLock=???
    Gemshine = 25883, // L6, 2.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Fester = 181, // L10, instant, 1.0s CD (group 0), range 25, single-target, targets=hostile
    EnergyDrain = 16508, // L10, instant, 60.0s CD (group 7), range 25, single-target, targets=hostile
    TopazRuin1 = 25809, // L15, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    SummonTopaz = 25803, // L15, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    SummonEmerald = 25804, // L22, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    EmeraldRuin1 = 25810, // L22, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    RubyOutburst = 25814, // L26, 2.8s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    TopazOutburst = 25815, // L26, instant, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    Outburst = 16511, // L26, 1.5s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    EmeraldOutburst = 25816, // L26, instant, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    PreciousBrilliance = 25884, // L26, 2.5s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    TopazRuin2 = 25812, // L30, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    Ruin2 = 172, // L30, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    EmeraldRuin2 = 25813, // L30, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    RubyRuin2 = 25811, // L30, 2.8s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    SummonIfrit1 = 25805, // L30, instant, GCD, range 25, single-target, targets=hostile
    SummonTitan1 = 25806, // L35, instant, GCD, range 25, single-target, targets=hostile
    Painflare = 3578, // L40, instant, 1.0s CD (group 1), range 25, AOE 5 circle, targets=hostile
    SummonGaruda1 = 25807, // L45, instant, GCD, range 25, single-target, targets=hostile
    EnergySiphon = 16510, // L52, instant, 60.0s CD (group 7), range 25, AOE 5 circle, targets=hostile
    Ruin3 = 3579, // L54, 1.5s cast, GCD, range 25, single-target, targets=hostile
    RubyRuin3 = 25817, // L54, 2.8s cast, GCD, range 25, single-target, targets=hostile
    TopazRuin3 = 25818, // L54, instant, GCD, range 25, single-target, targets=hostile
    EmeraldRuin3 = 25819, // L54, instant, GCD, range 25, single-target, targets=hostile
    AstralImpulse = 25820, // L58, instant, GCD, range 25, single-target, targets=hostile
    DreadwyrmTrance = 3581, // L58, instant, 60.0s CD (group 6/57), range 0, single-target, targets=self, animLock=???
    AstralFlare = 25821, // L58, instant, GCD, range 25, AOE 5 circle, targets=hostile
    AstralFlow = 25822, // L60, instant, GCD, range 0, single-target, targets=self, animLock=???
    Deathflare = 3582, // L60, instant, 20.0s CD (group 5), range 25, AOE 5 circle, targets=hostile
    Ruin4 = 7426, // L62, instant, GCD, range 25, AOE 5 circle, targets=hostile
    SearingLight = 25801, // L66, instant, 120.0s CD (group 19), range 0, AOE 30 circle, targets=self
    EnkindleBahamut = 7429, // L70, instant, 20.0s CD (group 3), range 25, single-target, targets=hostile
    SummonBahamut = 7427, // L70, instant, 60.0s CD (group 6/57), range 25, single-target, targets=hostile
    RubyRite = 25823, // L72, 2.8s cast, GCD, range 25, single-target, targets=hostile
    TopazRite = 25824, // L72, instant, GCD, range 25, single-target, targets=hostile
    EmeraldRite = 25825, // L72, instant, GCD, range 25, single-target, targets=hostile
    EmeraldDisaster = 25829, // L74, instant, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    RubyDisaster = 25827, // L74, 2.8s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    TriDisaster = 25826, // L74, 1.5s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    TopazDisaster = 25828, // L74, instant, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    SummonPhoenix = 25831, // L80, instant, 60.0s CD (group 6/57), range 25, single-target, targets=hostile
    EnkindlePhoenix = 16516, // L80, instant, 20.0s CD (group 3), range 25, single-target, targets=hostile
    BrandOfPurgatory = 16515, // L80, instant, GCD, range 25, AOE 8 circle, targets=hostile
    Rekindle = 25830, // L80, instant, 20.0s CD (group 4), range 30, single-target, targets=self/party
    FountainOfFire = 16514, // L80, instant, GCD, range 25, single-target, targets=hostile
    EmeraldCatastrophe = 25834, // L82, instant, GCD, range 25, AOE 5 circle, targets=hostile
    TopazCatastrophe = 25833, // L82, instant, GCD, range 25, AOE 5 circle, targets=hostile
    RubyCatastrophe = 25832, // L82, 2.8s cast, GCD, range 25, AOE 5 circle, targets=hostile
    MountainBuster = 25836, // L86, instant, 1.0s CD (group 2), range 25, AOE 5 circle, targets=hostile
    CrimsonCyclone = 25835, // L86, instant, GCD, range 25, AOE 5 circle, targets=hostile, animLock=0.750
    CrimsonStrike = 25885, // L86, instant, GCD, range 3, AOE 5 circle, targets=hostile
    Slipstream = 25837, // L86, 3.0s cast, GCD, range 25, AOE 5 circle, targets=hostile
    SummonIfrit2 = 25838, // L90, instant, GCD, range 25, single-target, targets=hostile
    SummonGaruda2 = 25840, // L90, instant, GCD, range 25, single-target, targets=hostile
    SummonTitan2 = 25839, // L90, instant, GCD, range 25, single-target, targets=hostile

    // pet abilities (TODO: regenerate)
    PetRadiantAegis = 25841, // L2, instant, range 30, single-target 0/0, targets=party, animLock=???
    PetWyrmwave = 7428, // L70, instant, range 50, single-target 0/0, targets=hostile, animLock=???
    PetAkhMorn = 7449, // L70, instant, range 50, AOE circle 5/0, targets=hostile, animLock=???
    PetEverlastingFlight = 16517, // L80, instant, range 0, AOE circle 15/0, targets=self, animLock=???
    PetRevelation = 16518, // L80, instant, range 50, AOE circle 5/0, targets=hostile, animLock=???
    PetGlitteringRuby = 25843, // L6, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    PetGlitteringTopaz = 25844, // L15, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    PetGlitteringEmerald = 25845, // L22, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    PetBurningStrike = 25846, // L30, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    PetRockBuster = 25847, // L35, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    PetAerialSlash = 25848, // L45, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    PetInferno1 = 25849, // L50, instant, range 5, AOE cone 5/0, targets=hostile, animLock=???
    PetEarthenFury1 = 25850, // L50, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    PetAerialBlast1 = 25851, // L50, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
    PetScarletFlame = 16519, // L80, instant, range 50, single-target 0/0, targets=hostile, animLock=???
    PetInferno2 = 25852, // L90, instant, range 35, AOE circle 5/0, targets=hostile, animLock=???
    PetEarthenFury2 = 25853, // L90, instant, range 35, AOE circle 5/0, targets=hostile, animLock=???
    PetAerialBlast2 = 25854, // L90, instant, range 35, AOE circle 5/0, targets=hostile, animLock=???

    // Shared
    Skyshard = ClassShared.AID.Skyshard, // LB1, 2.0s cast, range 25, AOE 8 circle, targets=area, castAnimLock=3.100
    Starstorm = ClassShared.AID.Starstorm, // LB2, 3.0s cast, range 25, AOE 10 circle, targets=area, castAnimLock=5.100
    Addle = ClassShared.AID.Addle, // L8, instant, 90.0s CD (group 46), range 25, single-target, targets=hostile
    Sleep = ClassShared.AID.Sleep, // L10, 2.5s cast, GCD, range 30, AOE 5 circle, targets=hostile
    Resurrection = ClassShared.AID.Resurrection, // L12, 8.0s cast, GCD, range 30, single-target, targets=party/alliance/friendly
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 45), range 0, single-target, targets=self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 44), range 0, single-target, targets=self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=self
}

public enum TraitID : uint
{
    None = 0,
    EnhancedAethercharge1 = 466, // L15, ruby/topaz arcanum
    MaimAndMend1 = 66, // L20, damage increase
    EnhancedAethercharge2 = 467, // L22, emerald arcanum
    RuinMastery1 = 217, // L30, ruin1 -> ruin2 upgrade
    RubySummoningMastery = 468, // L30, summon ruby -> ifrit upgrade
    TopazSummoningMastery = 469, // L35, summon topaz -> titan upgrade
    MaimAndMend2 = 69, // L40, damage increase
    EmeraldSummoningMastery = 470, // L45, summon emerald -> garuda upgrade
    Enkindle1 = 471, // L50, summon attacks upgrade
    RuinMastery2 = 473, // L54, ruin2 -> ruin3 upgrade
    AetherchargeMastery = 474, // L58, aethercharge -> dreadwyrm trance upgrade
    EnhancedEnergySiphon = 475, // L62, further ruin status
    EnhancedDreadwyrmTrance = 178, // L70, dreadwyrm trance -> summon bahamut upgrade
    RuinMastery3 = 476, // L72, gemshine upgrade
    OutburstMastery1 = 477, // L74, outburst -> tri-disaster upgrade
    EnhancedSummonBahamut = 502, // L80, summon bahamut -> phoenix upgrade
    OutburstMastery2 = 478, // L82, precious brilliance upgrade
    RuinMastery4 = 479, // L84, potency increase
    ElementalMastery = 503, // L86
    EnhancedRadiantAegis = 480, // L88, second charge
    Enkindle2 = 481, // L90, summon upgrade
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    Addle = 1203, // applied by Addle to target, -5% phys and -10% magic damage dealt
    LucidDreaming = 1204, // applied by Lucid Dreaming to self, MP restore
    Swiftcast = 167, // applied by Swiftcast to self, next cast is instant
    Sleep = 3, // applied by Sleep to target
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Teraflare, castAnimLock: 8.10f); // animLock=???, castAnimLock=8.100
        d.RegisterSpell(AID.Ruin1); // animLock=???
        d.RegisterSpell(AID.RadiantAegis);
        d.RegisterSpell(AID.SummonCarbuncle);
        d.RegisterSpell(AID.Physick);
        d.RegisterSpell(AID.RubyRuin1); // animLock=???
        d.RegisterSpell(AID.SummonRuby); // animLock=???
        d.RegisterSpell(AID.Aethercharge); // animLock=???
        d.RegisterSpell(AID.Gemshine); // animLock=???
        d.RegisterSpell(AID.Fester);
        d.RegisterSpell(AID.EnergyDrain);
        d.RegisterSpell(AID.TopazRuin1); // animLock=???
        d.RegisterSpell(AID.SummonTopaz); // animLock=???
        d.RegisterSpell(AID.SummonEmerald); // animLock=???
        d.RegisterSpell(AID.EmeraldRuin1); // animLock=???
        d.RegisterSpell(AID.RubyOutburst); // animLock=???
        d.RegisterSpell(AID.TopazOutburst); // animLock=???
        d.RegisterSpell(AID.Outburst); // animLock=???
        d.RegisterSpell(AID.EmeraldOutburst); // animLock=???
        d.RegisterSpell(AID.PreciousBrilliance); // animLock=???
        d.RegisterSpell(AID.TopazRuin2); // animLock=???
        d.RegisterSpell(AID.Ruin2); // animLock=???
        d.RegisterSpell(AID.EmeraldRuin2); // animLock=???
        d.RegisterSpell(AID.RubyRuin2); // animLock=???
        d.RegisterSpell(AID.SummonIfrit1);
        d.RegisterSpell(AID.SummonTitan1);
        d.RegisterSpell(AID.Painflare);
        d.RegisterSpell(AID.SummonGaruda1);
        d.RegisterSpell(AID.EnergySiphon);
        d.RegisterSpell(AID.Ruin3);
        d.RegisterSpell(AID.RubyRuin3);
        d.RegisterSpell(AID.TopazRuin3);
        d.RegisterSpell(AID.EmeraldRuin3);
        d.RegisterSpell(AID.AstralImpulse);
        d.RegisterSpell(AID.DreadwyrmTrance); // animLock=???
        d.RegisterSpell(AID.AstralFlare);
        d.RegisterSpell(AID.AstralFlow); // animLock=???
        d.RegisterSpell(AID.Deathflare);
        d.RegisterSpell(AID.Ruin4);
        d.RegisterSpell(AID.SearingLight);
        d.RegisterSpell(AID.EnkindleBahamut);
        d.RegisterSpell(AID.SummonBahamut);
        d.RegisterSpell(AID.RubyRite);
        d.RegisterSpell(AID.TopazRite);
        d.RegisterSpell(AID.EmeraldRite);
        d.RegisterSpell(AID.EmeraldDisaster); // animLock=???
        d.RegisterSpell(AID.RubyDisaster); // animLock=???
        d.RegisterSpell(AID.TriDisaster); // animLock=???
        d.RegisterSpell(AID.TopazDisaster); // animLock=???
        d.RegisterSpell(AID.SummonPhoenix);
        d.RegisterSpell(AID.EnkindlePhoenix);
        d.RegisterSpell(AID.BrandOfPurgatory);
        d.RegisterSpell(AID.Rekindle);
        d.RegisterSpell(AID.FountainOfFire);
        d.RegisterSpell(AID.EmeraldCatastrophe);
        d.RegisterSpell(AID.TopazCatastrophe);
        d.RegisterSpell(AID.RubyCatastrophe);
        d.RegisterSpell(AID.MountainBuster);
        d.RegisterSpell(AID.CrimsonCyclone, instantAnimLock: 0.75f); // animLock=0.750
        d.RegisterSpell(AID.CrimsonStrike);
        d.RegisterSpell(AID.Slipstream);
        d.RegisterSpell(AID.SummonIfrit2);
        d.RegisterSpell(AID.SummonGaruda2);
        d.RegisterSpell(AID.SummonTitan2);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}
