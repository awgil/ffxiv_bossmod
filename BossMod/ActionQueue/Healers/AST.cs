namespace BossMod.AST;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    AstralStasis = 4248, // LB3, 2.0s cast, range 0, AOE 50 circle, targets=self, animLock=???, castAnimLock=8.100
    Malefic = 3596, // L1, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Benefic = 3594, // L2, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly, animLock=???
    Combust = 3599, // L4, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    Lightspeed = 3606, // L6, instant, 120.0s CD (group 18), range 0, single-target, targets=self
    Helios = 3600, // L10, 1.5s cast, GCD, range 0, AOE 15 circle, targets=self
    Ascend = 3603, // L12, 8.0s cast, GCD, range 30, single-target, targets=party/alliance/friendly
    EssentialDignity = 3614, // L15, instant, 40.0s CD (group 10/70), range 30, single-target, targets=self/party/alliance/friendly
    BeneficII = 3610, // L26, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    Undraw = 9629, // L30, instant, 1.0s CD (group 2), range 0, single-target, targets=self, animLock=???
    Play = 17055, // L30, instant, 1.0s CD (group 1), range 0, single-target, targets=self, animLock=???
    TheSpire = 4406, // L30, instant, 1.0s CD (group 1), range 30, single-target, targets=self/party
    TheEwer = 4405, // L30, instant, 1.0s CD (group 1), range 30, single-target, targets=self/party
    TheBole = 4404, // L30, instant, 1.0s CD (group 1), range 30, single-target, targets=self/party
    Draw = 3590, // L30, instant, 30.0s CD (group 11/71) (2? charges), range 0, single-target, targets=self
    TheArrow = 4402, // L30, instant, 1.0s CD (group 1), range 30, single-target, targets=self/party
    TheBalance = 4401, // L30, instant, 1.0s CD (group 1), range 30, single-target, targets=self/party
    TheSpear = 4403, // L30, instant, 1.0s CD (group 1), range 30, single-target, targets=self/party
    AspectedBenefic = 3595, // L34, instant, GCD, range 30, single-target, targets=self/party/alliance/friendly
    Redraw = 3593, // L40, instant, 1.0s CD (group 5), range 0, single-target, targets=self
    AspectedHelios = 3601, // L42, 1.5s cast, GCD, range 0, AOE 15 circle, targets=self
    Gravity = 3615, // L45, 1.5s cast, GCD, range 25, AOE 5 circle, targets=hostile
    CombustII = 3608, // L46, instant, GCD, range 25, single-target, targets=hostile
    Divination = 16552, // L50, instant, 120.0s CD (group 23), range 0, AOE 30 circle, targets=self
    Synastry = 3612, // L50, instant, 120.0s CD (group 19), range 30, single-target, targets=party
    Astrodyne = 25870, // L50, instant, 1.0s CD (group 7), range 0, single-target, targets=self
    MaleficII = 3598, // L54, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    CollectiveUnconscious = 3613, // L58, instant, 60.0s CD (group 17), range 0, AOE 30 circle, targets=self
    CelestialOpposition = 16553, // L60, instant, 60.0s CD (group 16), range 0, AOE 15 circle, targets=self
    StellarDetonation = 8324, // L62, instant, 3.0s CD (group 6), range 0, AOE 20 circle, targets=self
    EarthlyStar = 7439, // L62, instant, 60.0s CD (group 13), range 30, ???, targets=area
    MaleficIII = 7442, // L64, 1.5s cast, GCD, range 25, single-target, targets=hostile
    LordOfCrowns = 7444, // L70, instant, 1.0s CD (group 4), range 0, AOE 20 circle, targets=self
    LadyOfCrowns = 7445, // L70, instant, 1.0s CD (group 4), range 0, AOE 20 circle, targets=self
    MinorArcana = 7443, // L70, instant, 60.0s CD (group 12), range 0, single-target, targets=self
    MaleficIV = 16555, // L72, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    CombustIII = 16554, // L72, instant, GCD, range 25, single-target, targets=hostile
    CelestialIntersection = 16556, // L74, instant, 30.0s CD (group 9/72), range 30, single-target, targets=self/party
    Horoscope = 16557, // L76, instant, 60.0s CD (group 14), range 0, AOE 20 circle, targets=self
    HoroscopeEnd = 16558, // L76, instant, 1.0s CD (group 3), range 0, AOE 20 circle, targets=self
    NeutralSect = 16559, // L80, instant, 120.0s CD (group 21), range 0, single-target, targets=self
    FallMalefic = 25871, // L82, 1.5s cast, GCD, range 25, single-target, targets=hostile
    GravityII = 25872, // L82, 1.5s cast, GCD, range 25, AOE 5 circle, targets=hostile
    Exaltation = 25873, // L86, instant, 60.0s CD (group 15), range 30, single-target, targets=self/party
    Macrocosmos = 25874, // L90, instant, 180.0s CD (group 20/57), range 0, AOE 20 circle, targets=self
    MicrocosmosEnd = 25875, // L90, instant, 1.0s CD (group 0), range 0, AOE 20 circle, targets=self

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
    MaimAndMend = 122, // L20
    EnhancedBenefic = 124, // L36
    MaimAndMendII = 125, // L40
    EnhancedDraw = 495, // L40
    CombustMastery = 186, // L46
    EnhancedDrawII = 496, // L50
    MaleficMastery = 187, // L54
    MaleficMasteryII = 188, // L64
    HyperLightspeed = 189, // L68
    CombustMasteryII = 314, // L72
    MaleficMasteryIII = 315, // L72
    EnhancedEssentialDignity = 316, // L78
    MaleficMasteryIV = 497, // L82
    GravityMastery = 498, // L82
    EnhancedHealingMagic = 499, // L85
    EnhancedCelestialIntersection = 500, // L88
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.AstralStasis, castAnimLock: 8.10f); // animLock=???, castAnimLock=8.100
        d.RegisterSpell(AID.Malefic); // animLock=???
        d.RegisterSpell(AID.Benefic); // animLock=???
        d.RegisterSpell(AID.Combust); // animLock=???
        d.RegisterSpell(AID.Lightspeed);
        d.RegisterSpell(AID.Helios);
        d.RegisterSpell(AID.Ascend);
        d.RegisterSpell(AID.EssentialDignity);
        d.RegisterSpell(AID.BeneficII);
        d.RegisterSpell(AID.Undraw); // animLock=???
        d.RegisterSpell(AID.Play); // animLock=???
        d.RegisterSpell(AID.TheSpire);
        d.RegisterSpell(AID.TheEwer);
        d.RegisterSpell(AID.TheBole);
        d.RegisterSpell(AID.Draw, maxCharges: 2);
        d.RegisterSpell(AID.TheArrow);
        d.RegisterSpell(AID.TheBalance);
        d.RegisterSpell(AID.TheSpear);
        d.RegisterSpell(AID.AspectedBenefic);
        d.RegisterSpell(AID.Redraw);
        d.RegisterSpell(AID.AspectedHelios);
        d.RegisterSpell(AID.Gravity);
        d.RegisterSpell(AID.CombustII);
        d.RegisterSpell(AID.Divination);
        d.RegisterSpell(AID.Synastry);
        d.RegisterSpell(AID.Astrodyne);
        d.RegisterSpell(AID.MaleficII); // animLock=???
        d.RegisterSpell(AID.CollectiveUnconscious);
        d.RegisterSpell(AID.CelestialOpposition);
        d.RegisterSpell(AID.StellarDetonation);
        d.RegisterSpell(AID.EarthlyStar);
        d.RegisterSpell(AID.MaleficIII);
        d.RegisterSpell(AID.LordOfCrowns);
        d.RegisterSpell(AID.LadyOfCrowns);
        d.RegisterSpell(AID.MinorArcana);
        d.RegisterSpell(AID.MaleficIV); // animLock=???
        d.RegisterSpell(AID.CombustIII);
        d.RegisterSpell(AID.CelestialIntersection);
        d.RegisterSpell(AID.Horoscope);
        d.RegisterSpell(AID.HoroscopeEnd);
        d.RegisterSpell(AID.NeutralSect);
        d.RegisterSpell(AID.FallMalefic);
        d.RegisterSpell(AID.GravityII);
        d.RegisterSpell(AID.Exaltation);
        d.RegisterSpell(AID.Macrocosmos);
        d.RegisterSpell(AID.MicrocosmosEnd);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}
