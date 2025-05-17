namespace BossMod.DNC;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    CrimsonLotus = 17106, // LB3, 4.5s cast, range 30, AOE 30+R width 8 rect, targets=hostile, animLock=???, castAnimLock=3.700
    Cascade = 15989, // L1, instant, GCD, range 25, single-target, targets=hostile
    Fountain = 15990, // L2, instant, GCD, range 25, single-target, targets=hostile
    SingleStandardFinish = 16191, // L15, instant, GCD, range 0, AOE 15 circle, targets=self
    DoubleStandardFinish = 16192, // L15, instant, GCD, range 0, AOE 15 circle, targets=self
    StandardFinish = 16003, // L15, instant, GCD, range 0, AOE 15 circle, targets=self
    Pirouette = 16002, // L15, instant, GCD, range 0, single-target, targets=self
    Jete = 16001, // L15, instant, GCD, range 0, single-target, targets=self
    Entrechat = 16000, // L15, instant, GCD, range 0, single-target, targets=self
    Emboite = 15999, // L15, instant, GCD, range 0, single-target, targets=self
    Windmill = 15993, // L15, instant, GCD, range 0, AOE 5 circle, targets=self
    StandardStep = 15997, // L15, instant, 30.0s CD (group 8/57), range 0, single-target, targets=self
    ReverseCascade = 15991, // L20, instant, GCD, range 25, single-target, targets=hostile
    Bladeshower = 15994, // L25, instant, GCD, range 0, AOE 5 circle, targets=self
    FanDance = 16007, // L30, instant, 1.0s CD (group 1), range 25, single-target, targets=hostile
    RisingWindmill = 15995, // L35, instant, GCD, range 0, AOE 5 circle, targets=self
    Fountainfall = 15992, // L40, instant, GCD, range 25, single-target, targets=hostile
    Bloodshower = 15996, // L45, instant, GCD, range 0, AOE 5 circle, targets=self
    EnAvant = 16010, // L50, instant, 30.0s CD (group 9/70), range 0, single-target, targets=self
    FanDanceII = 16008, // L50, instant, 1.0s CD (group 2), range 0, AOE 5 circle, targets=self
    CuringWaltz = 16015, // L52, instant, 60.0s CD (group 11), range 0, AOE 3 circle, targets=self
    ShieldSamba = 16012, // L56, instant, 120.0s CD (group 22), range 0, AOE 30 circle, targets=self
    ClosedPosition = 16006, // L60, instant, 30.0s CD (group 0), range 30, single-target, targets=party
    Ending = 18073, // L60, instant, 1.0s CD (group 0), range 0, single-target, targets=self
    Devilment = 16011, // L62, instant, 120.0s CD (group 20), range 0, single-target, targets=self
    FanDanceIII = 16009, // L66, instant, 1.0s CD (group 3), range 25, AOE 5 circle, targets=hostile
    TechnicalStep = 15998, // L70, instant, 120.0s CD (group 19/57), range 0, single-target, targets=self
    TechnicalFinish = 16004, // L70, instant, GCD, range 0, AOE 15 circle, targets=self
    SingleTechnicalFinish = 16193, // L70, instant, GCD, range 0, AOE 15 circle, targets=self
    DoubleTechnicalFinish = 16194, // L70, instant, GCD, range 0, AOE 15 circle, targets=self
    TripleTechnicalFinish = 16195, // L70, instant, GCD, range 0, AOE 15 circle, targets=self
    QuadrupleTechnicalFinish = 16196, // L70, instant, GCD, range 0, AOE 15 circle, targets=self
    SingleTechnicalFinish2 = 33215, // L70, instant, range 0, AOE 30 circle, targets=self, animLock=???
    DoubleTechnicalFinish2 = 33216, // L70, instant, range 0, AOE 30 circle, targets=self, animLock=???
    TripleTechnicalFinish2 = 33217, // L70, instant, range 0, AOE 30 circle, targets=self, animLock=???
    QuadrupleTechnicalFinish2 = 33218, // L70, instant, range 0, AOE 30 circle, targets=self, animLock=???
    Flourish = 16013, // L72, instant, 60.0s CD (group 10), range 0, single-target, targets=self
    SaberDance = 16005, // L76, instant, GCD, range 25, AOE 5 circle, targets=hostile
    Improvisation = 16014, // L80, instant, 120.0s CD (group 18), range 0, ???, targets=self
    ImprovisedFinish = 25789, // L80, instant, 1.5s CD (group 5), range 0, AOE 8 circle, targets=self
    Tillana = 25790, // L82, instant, GCD, range 0, AOE 15 circle, targets=self
    FanDanceIV = 25791, // L86, instant, 1.0s CD (group 4), range 15, AOE 15+R ?-degree cone, targets=hostile
    StarfallDance = 25792, // L90, instant, GCD, range 25, AOE 25+R width 4 rect, targets=hostile
    LastDance = 36983,
    FinishingMove = 36984,
    DanceOfTheDawn = 36985,

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
    FourfoldFantasy = 252, // L30
    IncreasedActionDamage = 251, // L50
    IncreasedActionDamageII = 253, // L60
    EnhancedEnAvant = 254, // L68
    Esprit = 255, // L76
    EnhancedEnAvantII = 256, // L78
    EnhancedTechnicalFinish = 453, // L82
    EnhancedEsprit = 454, // L84
    EnhancedFlourish = 455, // L86
    EnhancedShieldSamba = 456, // L88
    EnhancedDevilment = 457, // L90
    EnhancedStandardFinish = 609, // L92
    EnhancedSecondWind = 642, // L94
    DynamicDancer = 662, // L94
    EnhancedFlourishII = 610, // L96
    EnhancedShieldSambaII = 611, // L98
    EnhancedTechnicalFinishII = 612, // L100
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    SilkenSymmetry = 2693, // applied by Cascade, Windmill to self
    Heavy = 14, // applied by Leg Graze to target
    Bind = 13, // applied by Foot Graze to target
    ArmsLength = 1209, // applied by Arm's Length to self
    TechnicalStep = 1819, // applied by Technical Step to self
    FlourishingFinish = 2698, // applied by Quadruple Technical Finish, Single Technical Finish, Double Technical Finish, Triple Technical Finish, Technical Finish to self
    TechnicalFinish = 1822, // applied by Quadruple Technical Finish, Single Technical Finish, Double Technical Finish, Triple Technical Finish to self
    TechnicalEsprit = 1848, // applied by Quadruple Technical Finish, Double Technical Finish to self
    StandardFinish = 1821, // applied by Tillana, Double Standard Finish, Single Standard Finish to self
    StandardEsprit = 1847, // applied by Tillana, Double Standard Finish, Single Standard Finish to self
    StandardStep = 1818, // applied by Standard Step to self
    Improvisation = 1827, // applied by Improvisation to self
    ImprovisedFinish = 2697, // applied by Improvised Finish to self
    ShieldSamba = 1826, // applied by Shield Samba to self
    Devilment = 1825, // applied by Devilment to self
    FlourishingStarfall = 2700, // applied by Devilment to self
    FlourishingSymmetry = 3017, // applied by Flourish to self
    FlourishingFlow = 3018, // applied by Flourish to self
    ThreefoldFanDance = 1820, // applied by Flourish, Fan Dance, Fan Dance II to self
    FourfoldFanDance = 2699, // applied by Flourish to self
    SilkenFlow = 2694, // applied by Fountain to self
    DancePartner = 1824, // applied by Closed Position to target
    ClosedPosition = 1823, // applied by Closed Position to self
    LastDanceReady = 3867,
    FinishingMoveReady = 3868,
    DanceOfTheDawnReady = 3869,

    //Shared
    Peloton = ClassShared.SID.Peloton, // applied by Peloton to self/party
}

public sealed class Definitions : IDisposable
{
    private readonly DNCConfig _config = Service.Config.Get<DNCConfig>();
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.CrimsonLotus, true, castAnimLock: 3.70f); // animLock=???, castAnimLock=3.700
        d.RegisterSpell(AID.Cascade, true);
        d.RegisterSpell(AID.Fountain, true);
        d.RegisterSpell(AID.SingleStandardFinish, true);
        d.RegisterSpell(AID.DoubleStandardFinish, true);
        d.RegisterSpell(AID.StandardFinish, true);
        d.RegisterSpell(AID.Pirouette, true);
        d.RegisterSpell(AID.Jete, true);
        d.RegisterSpell(AID.Entrechat, true);
        d.RegisterSpell(AID.Emboite, true);
        d.RegisterSpell(AID.Windmill, true);
        d.RegisterSpell(AID.StandardStep, true);
        d.RegisterSpell(AID.ReverseCascade, true);
        d.RegisterSpell(AID.Bladeshower, true);
        d.RegisterSpell(AID.FanDance, true);
        d.RegisterSpell(AID.RisingWindmill, true);
        d.RegisterSpell(AID.Fountainfall, true);
        d.RegisterSpell(AID.Bloodshower, true);
        d.RegisterSpell(AID.EnAvant, true);
        d.RegisterSpell(AID.FanDanceII, true);
        d.RegisterSpell(AID.CuringWaltz, true);
        d.RegisterSpell(AID.ShieldSamba, true);
        d.RegisterSpell(AID.ClosedPosition, true);
        d.RegisterSpell(AID.Ending, true);
        d.RegisterSpell(AID.Devilment, true);
        d.RegisterSpell(AID.FanDanceIII, true);
        d.RegisterSpell(AID.TechnicalStep, true);
        d.RegisterSpell(AID.TechnicalFinish, true);
        d.RegisterSpell(AID.SingleTechnicalFinish, true);
        d.RegisterSpell(AID.DoubleTechnicalFinish, true);
        d.RegisterSpell(AID.TripleTechnicalFinish, true);
        d.RegisterSpell(AID.QuadrupleTechnicalFinish, true);
        d.RegisterSpell(AID.SingleTechnicalFinish2, true); // animLock=???
        d.RegisterSpell(AID.DoubleTechnicalFinish2, true); // animLock=???
        d.RegisterSpell(AID.TripleTechnicalFinish2, true); // animLock=???
        d.RegisterSpell(AID.QuadrupleTechnicalFinish2, true); // animLock=???
        d.RegisterSpell(AID.Flourish, true);
        d.RegisterSpell(AID.SaberDance, true);
        d.RegisterSpell(AID.Improvisation, true);
        d.RegisterSpell(AID.ImprovisedFinish, true);
        d.RegisterSpell(AID.Tillana, true);
        d.RegisterSpell(AID.FanDanceIV, true);
        d.RegisterSpell(AID.StarfallDance, true);
        d.RegisterSpell(AID.LastDance, true);
        d.RegisterSpell(AID.FinishingMove, true);
        d.RegisterSpell(AID.DanceOfTheDawn, true);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // hardcoded mechanics
        d.RegisterChargeIncreaseTrait(AID.EnAvant, TraitID.EnhancedEnAvantII);
        d.RegisterChargeIncreaseTrait(AID.EnAvant, TraitID.EnhancedEnAvant);

        d.Spell(AID.EnAvant)!.TransformAngle = (ws, _, _, _) => _config.AlignDashToCamera
            ? ws.Client.CameraAzimuth + 180.Degrees()
            : null;

        d.Spell(AID.EnAvant)!.ForbidExecute = ActionDefinitions.DashFixedDistanceCheck(10);
    }
}
