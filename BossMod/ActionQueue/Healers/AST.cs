namespace BossMod.AST;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    AstralStasis = 4248, // LB3, 2.0s cast, range 0, AOE 50 circle, targets=Self, animLock=8.100s?
    Malefic = 3596, // L1, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    Benefic = 3594, // L2, 1.5s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    Combust = 3599, // L4, instant, GCD, range 25, single-target, targets=Hostile
    Lightspeed = 3606, // L6, instant, 90.0s CD (group 18/71) (2 charges), range 0, single-target, targets=Self
    Helios = 3600, // L10, 1.5s cast, GCD, range 0, AOE 15 circle, targets=Self
    Ascend = 3603, // L12, 8.0s cast, GCD, range 30, single-target, targets=Party/Alliance/Friendly/Dead
    EssentialDignity = 3614, // L15, instant, 40.0s CD (group 10/70) (1-2 charges), range 30, single-target, targets=Self/Party/Alliance/Friendly
    BeneficII = 3610, // L26, 1.5s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    TheBole = 37027, // L30, instant, 1.0s CD (group 2), range 30, single-target, targets=Self/Party
    TheSpear = 37026, // L30, instant, 1.0s CD (group 1), range 30, single-target, targets=Self/Party
    TheArrow = 37024, // L30, instant, 1.0s CD (group 2), range 30, single-target, targets=Self/Party
    TheBalance = 37023, // L30, instant, 1.0s CD (group 1), range 30, single-target, targets=Self/Party
    PlayIII = 37021, // L30, instant, 1.0s CD (group 3), range 0, single-target, targets=Self
    AstralDraw = 37017, // L30, instant, 55.0s CD (group 16), range 0, single-target, targets=Self
    UmbralDraw = 37018, // L30, instant, 55.0s CD (group 16), range 0, single-target, targets=Self
    TheSpire = 37025, // L30, instant, 1.0s CD (group 3), range 30, single-target, targets=Self/Party
    TheEwer = 37028, // L30, instant, 1.0s CD (group 3), range 30, single-target, targets=Self/Party
    PlayII = 37020, // L30, instant, 1.0s CD (group 2), range 0, single-target, targets=Self
    PlayI = 37019, // L30, instant, 1.0s CD (group 1), range 0, single-target, targets=Self
    AspectedBenefic = 3595, // L34, instant, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    AspectedHelios = 3601, // L40, 1.5s cast, GCD, range 0, AOE 15 circle, targets=Self
    Gravity = 3615, // L45, 1.5s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    CombustII = 3608, // L46, instant, GCD, range 25, single-target, targets=Hostile
    Synastry = 3612, // L50, instant, 120.0s CD (group 19), range 30, single-target, targets=Party
    Divination = 16552, // L50, instant, 120.0s CD (group 20), range 0, AOE 30 circle, targets=Self
    MaleficII = 3598, // L54, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    CollectiveUnconscious = 3613, // L58, instant, 60.0s CD (group 11), range 0, AOE 30 circle, targets=Self
    CelestialOpposition = 16553, // L60, instant, 60.0s CD (group 13), range 0, AOE 15 circle, targets=Self
    EarthlyStar = 7439, // L62, instant, 60.0s CD (group 12), range 30, ???, targets=Area
    StellarDetonation = 8324, // L62, instant, 3.0s CD (group 8), range 0, AOE 20 circle, targets=Self
    MaleficIII = 7442, // L64, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    LordOfCrowns = 7444, // L70, instant, 1.0s CD (group 6), range 0, AOE 20 circle, targets=Self
    MinorArcana = 37022, // L70, instant, 1.0s CD (group 6), range 0, single-target, targets=Self
    LadyOfCrowns = 7445, // L70, instant, 1.0s CD (group 6), range 0, AOE 20 circle, targets=Self
    MaleficIV = 16555, // L72, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    CombustIII = 16554, // L72, instant, GCD, range 25, single-target, targets=Hostile
    CelestialIntersection = 16556, // L74, instant, 30.0s CD (group 9/72) (1-2 charges), range 30, single-target, targets=Self/Party
    HoroscopeEnd = 16558, // L76, instant, 1.0s CD (group 5), range 0, AOE 20 circle, targets=Self
    Horoscope = 16557, // L76, instant, 60.0s CD (group 14), range 0, AOE 20 circle, targets=Self
    NeutralSect = 16559, // L80, instant, 120.0s CD (group 21), range 0, single-target, targets=Self
    GravityII = 25872, // L82, 1.5s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    FallMalefic = 25871, // L82, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    Exaltation = 25873, // L86, instant, 60.0s CD (group 15), range 30, single-target, targets=Self/Party
    Macrocosmos = 25874, // L90, instant, 180.0s CD (group 22/57), range 0, AOE 20 circle, targets=Self
    MicrocosmosEnd = 25875, // L90, instant, 1.0s CD (group 0), range 0, AOE 20 circle, targets=Self
    Oracle = 37029, // L92, instant, 1.0s CD (group 4), range 25, AOE 5 circle, targets=Hostile
    HeliosConjunction = 37030, // L96, 1.5s cast, GCD, range 0, AOE 15 circle, targets=Self
    SunSign = 37031, // L100, instant, 1.0s CD (group 7), range 0, AOE 30 circle, targets=Self

    // Shared
    HealingWind = ClassShared.AID.HealingWind, // LB1, 2.0s cast, range 0, AOE 50 circle, targets=Self, animLock=2.100s?
    BreathOfTheEarth = ClassShared.AID.BreathOfTheEarth, // LB2, 2.0s cast, range 0, AOE 50 circle, targets=Self, animLock=5.130s?
    Repose = ClassShared.AID.Repose, // L8, 2.5s cast, GCD, range 30, single-target, targets=Hostile
    Esuna = ClassShared.AID.Esuna, // L10, 1.0s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 44), range 0, single-target, targets=Self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 43), range 0, single-target, targets=Self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
    Rescue = ClassShared.AID.Rescue, // L48, instant, 120.0s CD (group 49), range 30, single-target, targets=Party
}

public enum TraitID : uint
{
    None = 0,
    MaimAndMend = 122, // L20
    EnhancedBenefic = 124, // L36
    MaimAndMendII = 125, // L40
    CombustMastery = 186, // L46
    MaleficMastery = 187, // L54
    MaleficMasteryII = 188, // L64
    HyperLightspeed = 189, // L68
    MinorArcanaMastery = 631, // L70
    CombustMasteryII = 314, // L72
    MaleficMasteryIII = 315, // L72
    EnhancedEssentialDignity = 316, // L78
    MaleficMasteryIV = 497, // L82
    GravityMastery = 498, // L82
    EnhancedHealingMagic = 499, // L85
    EnhancedCelestialIntersection = 500, // L88
    EnhancedDivination = 632, // L92
    EnhancedSwiftcast = 644, // L94
    MagickMastery = 673, // L94
    AspectedHeliosMastery = 633, // L96
    EnhancedEssentialDignityII = 634, // L98
    EnhancedNeutralSect = 635, // L100
}

public enum SID : uint
{
    None = 0,
    LucidDreaming = 1204, // applied by Lucid Dreaming to self
    Swiftcast = 167, // applied by Swiftcast to self
    Surecast = 160, // applied by Surecast to self
    Combust = 838,
    AspectedHelios = 836,
    AspectedBenefic = 835,
    HeliosConjunction = 3894,
    Lightspeed = 841,
    CombustII = 843,
    CombustIII = 1881,
    Divination = 1878,
    TheBalance = 3887,
    TheSpear = 3889,
    Divining = 3893,
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.AstralStasis, castAnimLock: 8.10f); // animLock=8.100s?
        d.RegisterSpell(AID.Malefic);
        d.RegisterSpell(AID.Benefic);
        d.RegisterSpell(AID.Combust);
        d.RegisterSpell(AID.Lightspeed);
        d.RegisterSpell(AID.Helios);
        d.RegisterSpell(AID.Ascend);
        d.RegisterSpell(AID.EssentialDignity);
        d.RegisterSpell(AID.BeneficII);
        d.RegisterSpell(AID.TheBole);
        d.RegisterSpell(AID.TheSpear);
        d.RegisterSpell(AID.TheArrow);
        d.RegisterSpell(AID.TheBalance);
        d.RegisterSpell(AID.PlayIII);
        d.RegisterSpell(AID.AstralDraw);
        d.RegisterSpell(AID.UmbralDraw);
        d.RegisterSpell(AID.TheSpire);
        d.RegisterSpell(AID.TheEwer);
        d.RegisterSpell(AID.PlayII);
        d.RegisterSpell(AID.PlayI);
        d.RegisterSpell(AID.AspectedBenefic);
        d.RegisterSpell(AID.AspectedHelios);
        d.RegisterSpell(AID.Gravity);
        d.RegisterSpell(AID.CombustII);
        d.RegisterSpell(AID.Synastry);
        d.RegisterSpell(AID.Divination);
        d.RegisterSpell(AID.MaleficII);
        d.RegisterSpell(AID.CollectiveUnconscious);
        d.RegisterSpell(AID.CelestialOpposition);
        d.RegisterSpell(AID.EarthlyStar);
        d.RegisterSpell(AID.StellarDetonation);
        d.RegisterSpell(AID.MaleficIII);
        d.RegisterSpell(AID.LordOfCrowns);
        d.RegisterSpell(AID.MinorArcana);
        d.RegisterSpell(AID.LadyOfCrowns);
        d.RegisterSpell(AID.MaleficIV);
        d.RegisterSpell(AID.CombustIII);
        d.RegisterSpell(AID.CelestialIntersection);
        d.RegisterSpell(AID.HoroscopeEnd);
        d.RegisterSpell(AID.Horoscope);
        d.RegisterSpell(AID.NeutralSect);
        d.RegisterSpell(AID.GravityII);
        d.RegisterSpell(AID.FallMalefic);
        d.RegisterSpell(AID.Exaltation);
        d.RegisterSpell(AID.Macrocosmos);
        d.RegisterSpell(AID.MicrocosmosEnd);
        d.RegisterSpell(AID.Oracle);
        d.RegisterSpell(AID.HeliosConjunction);
        d.RegisterSpell(AID.SunSign);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.EssentialDignity, TraitID.EnhancedEssentialDignity);
        d.RegisterChargeIncreaseTrait(AID.EssentialDignity, TraitID.EnhancedEssentialDignityII);
        d.RegisterChargeIncreaseTrait(AID.CelestialIntersection, TraitID.EnhancedCelestialIntersection);
    }
}

