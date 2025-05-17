namespace BossMod.SGE;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    TechneMakre = 24859, // LB3, 2.0s cast, range 0, AOE 50 circle, targets=Self, animLock=8.100s?
    Dosis = 24283, // L1, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    Diagnosis = 24284, // L2, 1.5s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    KardiaEnd = 28119, // L4, instant, range 30, single-target, targets=Self/Party
    Kardia = 24285, // L4, instant, 5.0s CD (group 1), range 30, single-target, targets=Self/Party
    Prognosis = 24286, // L10, 2.0s cast, GCD, range 0, AOE 15 circle, targets=Self
    Egeiro = 24287, // L12, 8.0s cast, GCD, range 30, single-target, targets=Party/Alliance/Friendly/Dead
    Physis = 24288, // L20, instant, 60.0s CD (group 10), range 0, AOE 15 circle, targets=Self
    Phlegma = 24289, // L26, instant, 40.0s CD (group 13/57) (2 charges), range 6, AOE 5 circle, targets=Hostile
    Eukrasia = 24290, // L30, instant, GCD, range 0, single-target, targets=Self
    EukrasianDiagnosis = 24291, // L30, instant, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    EukrasianPrognosis = 24292, // L30, instant, GCD, range 0, AOE 15 circle, targets=Self
    EukrasianDosis = 24293, // L30, instant, GCD, range 25, single-target, targets=Hostile
    Soteria = 24294, // L35, instant, 90.0s CD (group 14), range 0, single-target, targets=Self
    Icarus = 24295, // L40, instant, 45.0s CD (group 6), range 25, single-target, targets=Party/Hostile, animLock=0.700s?
    Druochole = 24296, // L45, instant, 1.0s CD (group 0), range 30, single-target, targets=Self/Party/Alliance/Friendly
    Dyskrasia = 24297, // L46, instant, GCD, range 0, AOE 5 circle, targets=Self
    Kerachole = 24298, // L50, instant, 30.0s CD (group 3), range 0, AOE 30 circle, targets=Self
    Ixochole = 24299, // L52, instant, 30.0s CD (group 4), range 0, AOE 15 circle, targets=Self
    Zoe = 24300, // L56, instant, 120.0s CD (group 19), range 0, single-target, targets=Self
    Pepsis = 24301, // L58, instant, 30.0s CD (group 2), range 0, AOE 15 circle, targets=Self
    PhysisII = 24302, // L60, instant, 60.0s CD (group 11), range 0, AOE 30 circle, targets=Self
    Taurochole = 24303, // L62, instant, 45.0s CD (group 7), range 30, single-target, targets=Self/Party
    Toxikon = 24304, // L66, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    Haima = 24305, // L70, instant, 120.0s CD (group 20), range 30, single-target, targets=Self/Party
    PhlegmaII = 24307, // L72, instant, 40.0s CD (group 13/57) (2 charges), range 6, AOE 5 circle, targets=Hostile
    EukrasianDosisII = 24308, // L72, instant, GCD, range 25, single-target, targets=Hostile
    DosisII = 24306, // L72, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    Rhizomata = 24309, // L74, instant, 90.0s CD (group 15), range 0, single-target, targets=Self
    Holos = 24310, // L76, instant, 120.0s CD (group 18), range 0, AOE 30 circle, targets=Self
    Panhaima = 24311, // L80, instant, 120.0s CD (group 21), range 0, AOE 30 circle, targets=Self
    EukrasianDyskrasia = 37032, // L82, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    PhlegmaIII = 24313, // L82, instant, 40.0s CD (group 13/57) (2 charges), range 6, AOE 5 circle, targets=Hostile
    DyskrasiaII = 24315, // L82, instant, GCD, range 0, AOE 5 circle, targets=Self
    EukrasianDosisIII = 24314, // L82, instant, GCD, range 25, single-target, targets=Hostile
    DosisIII = 24312, // L82, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    ToxikonII = 24316, // L82, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    Krasis = 24317, // L86, instant, 60.0s CD (group 12), range 30, single-target, targets=Self/Party
    Pneuma = 24318, // L90, 1.5s cast, 120.0s CD (group 22/57), range 25, AOE 25+R width 4 rect, targets=Hostile
    Psyche = 37033, // L92, instant, 60.0s CD (group 9), range 25, AOE 5 circle, targets=Hostile, animLock=???
    EukrasianPrognosisII = 37034, // L96, instant, GCD, range 0, AOE 15 circle, targets=Self, animLock=???
    Philosophia = 37035, // L100, instant, 180.0s CD (group 24), range 0, AOE 20 circle, targets=Self, animLock=???
    Eudaimonia = 37036, // L100, instant, range 30, single-target, targets=Self/Party, animLock=???

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
    MaimAndMend = 368, // L20
    MaimAndMendII = 369, // L40
    Addersgall = 370, // L45
    SomanouticOath = 371, // L54
    PhysisMastery = 510, // L60
    SomanouticOathII = 372, // L64
    Addersting = 373, // L66
    OffensiveMagicMastery = 374, // L72
    EnhancedKerachole = 375, // L78
    OffensiveMagicMasteryII = 376, // L82
    EnhancedHealingMagic = 377, // L85
    EnhancedZoe = 378, // L88
    EnhancedSwiftcast = 644, // L94
    EnhancedSoteria = 636, // L94
    MagickMastery = 669, // L94
    EukrasianPrognosisMastery = 637, // L96
    EnhancedPhysisII = 638, // L98
}

public enum SID : uint
{
    None = 0,
    Sprint = 50, // applied by Sprint to self
    Eukrasia = 2606,
    Kardion = 2605, // applied by Kardia to self
    Kardia = 2604, // applied by Kardia to self
    Haima = 2612, // applied by Haima to self
    Haimatinon = 2642, // applied by Haima to self
    Panhaima = 2613, // applied by Panhaima to self
    Panhaimatinon = 2643, // applied by Panhaima to self
    Holosakos = 3365, // applied by Holos to self
    Holos = 3003, // applied by Holos to self
    PhysisII = 2620, // applied by Physis II to self
    Autophysis = 2621, // applied by Physis II to self
    Kerachole = 2618, // applied by Kerachole to self
    Kerakeia = 2938, // applied by Kerachole to self
    Taurochole = 2619, // applied by Taurochole to self
    Krasis = 2622, // applied by Krasis to self
    Soteria = 2610, // applied by Soteria to self
    Zoe = 2611, // applied by Zoe to self
    EukrasianPrognosis = 2609, // applied by Eukrasian Prognosis to self
    EukrasianDiagnosis = 2607, // applied by Eukrasian Diagnosis to self
    DifferentialDiagnosis = 2608, // applied by Eukrasian Diagnosis to self
    EukrasianDosis = 2614, // applied by Eukrasian Dosis to target
    EukrasianDosisII = 2615, // applied by Eukrasian Dosis II to target
    EukrasianDosisIII = 2616, // applied by Eukrasian Dosis III to target
    EukrasianDyskrasia = 3897, // applied by Eukrasian Dyskrasia to target

    //Shared
    Surecast = ClassShared.SID.Surecast, // applied by Surecast to self
    LucidDreaming = ClassShared.SID.LucidDreaming, // applied by Lucid Dreaming to self
    Swiftcast = ClassShared.SID.Swiftcast, // applied by Swiftcast to self
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.TechneMakre, castAnimLock: 8.10f); // animLock=8.100s?
        d.RegisterSpell(AID.Dosis);
        d.RegisterSpell(AID.Diagnosis);
        d.RegisterSpell(AID.KardiaEnd);
        d.RegisterSpell(AID.Kardia);
        d.RegisterSpell(AID.Prognosis);
        d.RegisterSpell(AID.Egeiro);
        d.RegisterSpell(AID.Physis); // animLock=???
        d.RegisterSpell(AID.Phlegma);
        d.RegisterSpell(AID.Eukrasia);
        d.RegisterSpell(AID.EukrasianDiagnosis);
        d.RegisterSpell(AID.EukrasianPrognosis);
        d.RegisterSpell(AID.EukrasianDosis);
        d.RegisterSpell(AID.Soteria);
        d.RegisterSpell(AID.Icarus, instantAnimLock: 0.70f); // animLock=0.700s?
        d.RegisterSpell(AID.Druochole);
        d.RegisterSpell(AID.Dyskrasia);
        d.RegisterSpell(AID.Kerachole);
        d.RegisterSpell(AID.Ixochole);
        d.RegisterSpell(AID.Zoe);
        d.RegisterSpell(AID.Pepsis);
        d.RegisterSpell(AID.PhysisII);
        d.RegisterSpell(AID.Taurochole);
        d.RegisterSpell(AID.Toxikon);
        d.RegisterSpell(AID.Haima);
        d.RegisterSpell(AID.DosisII); // animLock=???
        d.RegisterSpell(AID.PhlegmaII); // animLock=???
        d.RegisterSpell(AID.EukrasianDosisII); // animLock=???
        d.RegisterSpell(AID.Rhizomata);
        d.RegisterSpell(AID.Holos);
        d.RegisterSpell(AID.Panhaima);
        d.RegisterSpell(AID.EukrasianDyskrasia); // animLock=???
        d.RegisterSpell(AID.PhlegmaIII);
        d.RegisterSpell(AID.DyskrasiaII);
        d.RegisterSpell(AID.EukrasianDosisIII);
        d.RegisterSpell(AID.DosisIII);
        d.RegisterSpell(AID.ToxikonII);
        d.RegisterSpell(AID.Krasis);
        d.RegisterSpell(AID.Pneuma);
        d.RegisterSpell(AID.Psyche); // animLock=???
        d.RegisterSpell(AID.EukrasianPrognosisII); // animLock=???
        d.RegisterSpell(AID.Philosophia); // animLock=???
        d.RegisterSpell(AID.Eudaimonia); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
        d.Spell(AID.Icarus)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
    }
}
