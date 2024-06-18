namespace BossMod.SGE;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    TechneMakre = 24859, // LB3, 2.0s cast, range 0, AOE 50 circle, targets=self, animLock=???, castAnimLock=8.100
    Dosis = 24283, // L1, 1.5s cast, GCD, range 25, single-target, targets=hostile
    Diagnosis = 24284, // L2, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    Kardia = 24285, // L4, instant, 5.0s CD (group 1), range 30, single-target, targets=self/party
    KardiaEnd = 28119, // L4, instant, range 30, single-target, targets=self/party, animLock=???
    Prognosis = 24286, // L10, 2.0s cast, GCD, range 0, AOE 15 circle, targets=self
    Egeiro = 24287, // L12, 8.0s cast, GCD, range 30, single-target, targets=party/alliance/friendly
    Physis = 24288, // L20, instant, 60.0s CD (group 10), range 0, AOE 15 circle, targets=self, animLock=???
    Phlegma = 24289, // L26, instant, 40.0s CD (group 18/57) (2? charges), range 6, AOE 5 circle, targets=hostile
    Eukrasia = 24290, // L30, instant, GCD, range 0, single-target, targets=self
    EukrasianDiagnosis = 24291, // L30, instant, GCD, range 30, single-target, targets=self/party/alliance/friendly
    EukrasianPrognosis = 24292, // L30, instant, GCD, range 0, AOE 15 circle, targets=self
    EukrasianDosis = 24293, // L30, instant, GCD, range 25, single-target, targets=hostile
    Soteria = 24294, // L35, instant, 90.0s CD (group 14), range 0, single-target, targets=self
    Icarus = 24295, // L40, instant, 45.0s CD (group 6), range 25, single-target, targets=party/hostile, animLock=0.700
    Druochole = 24296, // L45, instant, 1.0s CD (group 0), range 30, single-target, targets=self/party/alliance/friendly
    Dyskrasia = 24297, // L46, instant, GCD, range 0, AOE 5 circle, targets=self
    Kerachole = 24298, // L50, instant, 30.0s CD (group 3), range 0, AOE 30 circle, targets=self
    Ixochole = 24299, // L52, instant, 30.0s CD (group 4), range 0, AOE 15 circle, targets=self
    Zoe = 24300, // L56, instant, 120.0s CD (group 19), range 0, single-target, targets=self
    Pepsis = 24301, // L58, instant, 30.0s CD (group 2), range 0, AOE 15 circle, targets=self
    PhysisII = 24302, // L60, instant, 60.0s CD (group 23), range 0, AOE 30 circle, targets=self
    Taurochole = 24303, // L62, instant, 45.0s CD (group 7), range 30, single-target, targets=self/party
    Toxikon = 24304, // L66, instant, GCD, range 25, AOE 5 circle, targets=hostile
    Haima = 24305, // L70, instant, 120.0s CD (group 20), range 30, single-target, targets=self/party
    DosisII = 24306, // L72, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    PhlegmaII = 24307, // L72, instant, 40.0s CD (group 16/57) (2? charges), range 6, AOE 5 circle, targets=hostile, animLock=???
    EukrasianDosisII = 24308, // L72, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    Rhizomata = 24309, // L74, instant, 90.0s CD (group 15), range 0, single-target, targets=self
    Holos = 24310, // L76, instant, 120.0s CD (group 11), range 0, AOE 30 circle, targets=self
    Panhaima = 24311, // L80, instant, 120.0s CD (group 21), range 0, AOE 30 circle, targets=self
    DyskrasiaII = 24315, // L82, instant, GCD, range 0, AOE 5 circle, targets=self
    ToxikonII = 24316, // L82, instant, GCD, range 25, AOE 5 circle, targets=hostile
    DosisIII = 24312, // L82, 1.5s cast, GCD, range 25, single-target, targets=hostile
    EukrasianDosisIII = 24314, // L82, instant, GCD, range 25, single-target, targets=hostile
    PhlegmaIII = 24313, // L82, instant, 40.0s CD (group 17/57) (2? charges), range 6, AOE 5 circle, targets=hostile
    Krasis = 24317, // L86, instant, 60.0s CD (group 12), range 30, single-target, targets=self/party
    Pneuma = 24318, // L90, 1.5s cast, 120.0s CD (group 22/57), range 25, AOE 25+R width 4 rect, targets=hostile

    // Shared
    HealingWind = ClassShared.AID.HealingWind, // LB1, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=2.100
    BreathOfTheEarth = ClassShared.AID.BreathOfTheEarth, // LB2, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=5.130
    Repose = ClassShared.AID.Repose, // L8, 2.5s cast, GCD, range 30, single-target, targets=hostile
    Esuna = ClassShared.AID.Esuna, // L10, 1.0s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 45), range 0, single-target, targets=self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 44), range 0, single-target, targets=self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    Rescue = ClassShared.AID.Rescue, // L48, instant, 120.0s CD (group 49), range 30, single-target, targets=party
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
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.TechneMakre, castAnimLock: 8.10f); // castAnimLock=8.100
        d.RegisterSpell(AID.Dosis);
        d.RegisterSpell(AID.Diagnosis);
        d.RegisterSpell(AID.Kardia); // animLock=???
        d.RegisterSpell(AID.KardiaEnd);
        d.RegisterSpell(AID.Prognosis);
        d.RegisterSpell(AID.Egeiro);
        d.RegisterSpell(AID.Physis); // animLock=???
        d.RegisterSpell(AID.Phlegma, maxCharges: 2);
        d.RegisterSpell(AID.Eukrasia);
        d.RegisterSpell(AID.EukrasianDiagnosis);
        d.RegisterSpell(AID.EukrasianPrognosis);
        d.RegisterSpell(AID.EukrasianDosis);
        d.RegisterSpell(AID.Soteria);
        d.RegisterSpell(AID.Icarus, instantAnimLock: 0.70f);
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
        d.RegisterSpell(AID.PhlegmaII, maxCharges: 2); // animLock=???
        d.RegisterSpell(AID.EukrasianDosisII); // animLock=???
        d.RegisterSpell(AID.Rhizomata);
        d.RegisterSpell(AID.Holos);
        d.RegisterSpell(AID.Panhaima);
        d.RegisterSpell(AID.DyskrasiaII);
        d.RegisterSpell(AID.ToxikonII);
        d.RegisterSpell(AID.DosisIII);
        d.RegisterSpell(AID.EukrasianDosisIII);
        d.RegisterSpell(AID.PhlegmaIII, maxCharges: 2);
        d.RegisterSpell(AID.Krasis);
        d.RegisterSpell(AID.Pneuma);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}
