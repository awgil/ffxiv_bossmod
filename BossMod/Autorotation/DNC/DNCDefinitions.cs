namespace BossMod.DNC;

public enum AID : uint
{
    None = 0,

    // GCDs
    Cascade = 15989, // L1, instant, range 25, single-target 0/0, targets=hostile, animLock=0.600s
    Fountain = 15990, // L2, instant, range 25, single-target 0/0, targets=hostile, animLock=0.600s
    Pirouette = 16002, // L15, instant, range 0, single-target 0/0, targets=self, animLock=0.600s
    Jete = 16001, // L15, instant, range 0, single-target 0/0, targets=self, animLock=0.600s
    Entrechat = 16000, // L15, instant, range 0, single-target 0/0, targets=self, animLock=0.600s
    Emboite = 15999, // L15, instant, range 0, single-target 0/0, targets=self, animLock=0.600s
    SingleStandardFinish = 16191, // L15, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    Windmill = 15993, // L15, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
    StandardFinish = 16003, // L15, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    DoubleStandardFinish = 16192, // L15, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    ReverseCascade = 15991, // L20, instant, range 25, single-target 0/0, targets=hostile, animLock=0.600s
    Bladeshower = 15994, // L25, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
    RisingWindmill = 15995, // L35, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
    Fountainfall = 15992, // L40, instant, range 25, single-target 0/0, targets=hostile, animLock=0.600s
    Bloodshower = 15996, // L45, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
    TripleTechnicalFinish = 16195, // L70, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    DoubleTechnicalFinish = 16194, // L70, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    QuadrupleTechnicalFinish = 16196, // L70, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    TechnicalFinish = 16004, // L70, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    SingleTechnicalFinish = 16193, // L70, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    SaberDance = 16005, // L76, instant, range 25, AOE circle 5/0, targets=hostile, animLock=0.600s
    Tillana = 25790, // L82, instant, range 0, AOE circle 15/0, targets=self, animLock=0.600s
    StarfallDance = 25792, // L90, instant, range 25, AOE rect 25/4, targets=hostile, animLock=0.600s

    // oGCDs
    LegGraze = 7554, // L6, instant, 30.0s CD (group 42), range 25, single-target 0/0, targets=hostile, animLock=0.600s
    SecondWind = 7541, // L8, instant, 120.0s CD (group 49), range 0, single-target 0/0, targets=self, animLock=0.600s
    FootGraze = 7553, // L10, instant, 30.0s CD (group 41), range 25, single-target 0/0, targets=hostile, animLock=0.600s
    StandardStep = 15997, // L15, instant, 30.0s CD (group 8), range 0, single-target 0/0, targets=self, animLock=0.600s
    Peloton = 7557, // L20, instant, 5.0s CD (group 40), range 0, AOE circle 30/0, targets=self, animLock=0.600s
    HeadGraze = 7551, // L24, instant, 30.0s CD (group 43), range 25, single-target 0/0, targets=hostile, animLock=0.600s
    FanDance = 16007, // L30, instant, 1.0s CD (group 1), range 25, single-target 0/0, targets=hostile, animLock=0.600s
    ArmsLength = 7548, // L32, instant, 120.0s CD (group 48), range 0, single-target 0/0, targets=self, animLock=0.600s
    FanDanceII = 16008, // L50, instant, 1.0s CD (group 2), range 0, AOE circle 5/0, targets=self, animLock=0.600s
    EnAvant = 16010, // L50, instant, 30.0s CD (group 9) (3 charges), range 0, single-target 0/0, targets=self, animLock=0.600s
    CuringWaltz = 16015, // L52, instant, 60.0s CD (group 11), range 0, AOE circle 3/0, targets=self, animLock=0.600s
    ShieldSamba = 16012, // L56, instant, 120.0s CD (group 22), range 0, AOE circle 30/0, targets=self, animLock=0.600s
    Ending = 18073, // L60, instant, 1.0s CD (group 0), range 0, single-target 0/0, targets=self, animLock=0.600s
    ClosedPosition = 16006, // L60, instant, 30.0s CD (group 0), range 30, single-target 0/0, targets=party, animLock=0.600s
    Devilment = 16011, // L62, instant, 120.0s CD (group 20), range 0, single-target 0/0, targets=self, animLock=0.600s
    FanDanceIII = 16009, // L66, instant, 1.0s CD (group 3), range 25, AOE circle 5/0, targets=hostile, animLock=0.600s
    TechnicalStep = 15998, // L70, instant, 120.0s CD (group 19), range 0, single-target 0/0, targets=self, animLock=0.600s
    Flourish = 16013, // L72, instant, 60.0s CD (group 10), range 0, single-target 0/0, targets=self, animLock=0.600s
    ImprovisedFinish = 25789, // L80, instant, 1.5s CD (group 5), range 0, AOE circle 8/0, targets=self, animLock=0.600s
    Improvisation = 16014, // L80, instant, 120.0s CD (group 18), range 0, Ground circle 8/0, targets=self, animLock=0.600s
    FanDanceIV = 25791, // L86, instant, 1.0s CD (group 4), range 15, AOE cone 15/0, targets=hostile, animLock=0.600s
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
}

public enum CDGroup : int
{
    Ending = 0, // variable max, shared by Ending, Closed Position
    FanDance = 1, // 1.0 max
    FanDanceII = 2, // 1.0 max
    FanDanceIII = 3, // 1.0 max
    FanDanceIV = 4, // 1.0 max
    ImprovisedFinish = 5, // 1.5 max
    StandardStep = 8, // 30.0 max
    EnAvant = 9, // 3*30.0 max
    Flourish = 10, // 60.0 max
    CuringWaltz = 11, // 60.0 max
    Improvisation = 18, // 120.0 max
    TechnicalStep = 19, // 120.0 max
    Devilment = 20, // 120.0 max
    ShieldSamba = 22, // 120.0 max
    Peloton = 40, // 5.0 max
    FootGraze = 41, // 30.0 max
    LegGraze = 42, // 30.0 max
    HeadGraze = 43, // 30.0 max
    ArmsLength = 48, // 120.0 max
    SecondWind = 49, // 120.0 max
}

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
    Peloton = 1199, // applied by Peloton to self
    DancePartner = 1824, // applied by Closed Position to target
    ClosedPosition = 1823, // applied by Closed Position to self
}

public static class Definitions
{
    public static uint[] UnlockQuests = { 68790 };

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
        {
            AID.Fountain => level >= 2,
            AID.LegGraze => level >= 6,
            AID.SecondWind => level >= 8,
            AID.FootGraze => level >= 10,
            AID.Pirouette => level >= 15,
            AID.Jete => level >= 15,
            AID.Entrechat => level >= 15,
            AID.Emboite => level >= 15,
            AID.SingleStandardFinish => level >= 15,
            AID.StandardStep => level >= 15,
            AID.Windmill => level >= 15,
            AID.StandardFinish => level >= 15,
            AID.DoubleStandardFinish => level >= 15,
            AID.ReverseCascade => level >= 20,
            AID.Peloton => level >= 20,
            AID.HeadGraze => level >= 24,
            AID.Bladeshower => level >= 25,
            AID.FanDance => level >= 30,
            AID.ArmsLength => level >= 32,
            AID.RisingWindmill => level >= 35,
            AID.Fountainfall => level >= 40,
            AID.Bloodshower => level >= 45,
            AID.FanDanceII => level >= 50,
            AID.EnAvant => level >= 50,
            AID.CuringWaltz => level >= 52,
            AID.ShieldSamba => level >= 56,
            AID.Ending => level >= 60,
            AID.ClosedPosition => level >= 60,
            AID.Devilment => level >= 62,
            AID.FanDanceIII => level >= 66,
            AID.DoubleTechnicalFinish => level >= 70 && questProgress > 0,
            AID.SingleTechnicalFinish => level >= 70 && questProgress > 0,
            AID.TechnicalStep => level >= 70 && questProgress > 0,
            AID.TripleTechnicalFinish => level >= 70 && questProgress > 0,
            AID.QuadrupleTechnicalFinish => level >= 70 && questProgress > 0,
            AID.TechnicalFinish => level >= 70 && questProgress > 0,
            AID.Flourish => level >= 72,
            AID.SaberDance => level >= 76,
            AID.ImprovisedFinish => level >= 80,
            AID.Improvisation => level >= 80,
            AID.Tillana => level >= 82,
            AID.FanDanceIV => level >= 86,
            AID.StarfallDance => level >= 90,
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
        {
            TraitID.FourfoldFantasy => level >= 30,
            TraitID.IncreasedActionDamage => level >= 50,
            TraitID.IncreasedActionDamageII => level >= 60,
            TraitID.EnhancedEnAvant => level >= 68,
            TraitID.Esprit => level >= 76,
            TraitID.EnhancedEnAvantII => level >= 78,
            TraitID.EnhancedTechnicalFinish => level >= 82,
            TraitID.EnhancedEsprit => level >= 84,
            TraitID.EnhancedFlourish => level >= 86,
            TraitID.EnhancedShieldSamba => level >= 88,
            TraitID.EnhancedDevilment => level >= 90,
            _ => true,
        };
    }

    public static Dictionary<ActionID, ActionDefinition> SupportedActions;

    static Definitions()
    {
        SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionDex);
        SupportedActions.GCD(AID.Cascade, 25);
        SupportedActions.GCD(AID.Fountain, 25);
        SupportedActions.OGCD(AID.LegGraze, 25, CDGroup.LegGraze, 30.0f);
        SupportedActions.OGCD(AID.SecondWind, 0, CDGroup.SecondWind, 120.0f);
        SupportedActions.OGCD(AID.FootGraze, 25, CDGroup.FootGraze, 30.0f);
        SupportedActions.GCD(AID.Pirouette, 0);
        SupportedActions.GCD(AID.Jete, 0);
        SupportedActions.GCD(AID.Entrechat, 0);
        SupportedActions.GCD(AID.Emboite, 0);
        SupportedActions.GCD(AID.SingleStandardFinish, 0);
        SupportedActions.OGCD(AID.StandardStep, 0, CDGroup.StandardStep, 30.0f).EffectDuration = 15;
        SupportedActions.GCD(AID.Windmill, 0);
        SupportedActions.GCD(AID.StandardFinish, 0);
        SupportedActions.GCD(AID.DoubleStandardFinish, 0);
        SupportedActions.GCD(AID.ReverseCascade, 25);
        SupportedActions.OGCD(AID.Peloton, 0, CDGroup.Peloton, 5.0f).EffectDuration = 30;
        SupportedActions.OGCD(AID.HeadGraze, 25, CDGroup.HeadGraze, 30.0f);
        SupportedActions.GCD(AID.Bladeshower, 0);
        SupportedActions.OGCD(AID.FanDance, 25, CDGroup.FanDance, 1.0f);
        SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
        SupportedActions.GCD(AID.RisingWindmill, 0);
        SupportedActions.GCD(AID.Fountainfall, 25);
        SupportedActions.GCD(AID.Bloodshower, 0);
        SupportedActions.OGCD(AID.FanDanceII, 0, CDGroup.FanDanceII, 1.0f);
        SupportedActions.OGCDWithCharges(AID.EnAvant, 0, CDGroup.EnAvant, 30.0f, 3);
        SupportedActions.OGCD(AID.CuringWaltz, 0, CDGroup.CuringWaltz, 60.0f);
        SupportedActions.OGCD(AID.ShieldSamba, 0, CDGroup.ShieldSamba, 120.0f).EffectDuration = 15;
        SupportedActions.OGCD(AID.Ending, 0, CDGroup.Ending, 1.0f);
        SupportedActions.OGCD(AID.ClosedPosition, 30, CDGroup.Ending, 30.0f);
        SupportedActions.OGCD(AID.Devilment, 0, CDGroup.Devilment, 120.0f);
        SupportedActions.OGCD(AID.FanDanceIII, 25, CDGroup.FanDanceIII, 1.0f);
        SupportedActions.OGCD(AID.TechnicalStep, 0, CDGroup.TechnicalStep, 120.0f);
        SupportedActions.GCD(AID.TripleTechnicalFinish, 0);
        SupportedActions.GCD(AID.DoubleTechnicalFinish, 0);
        SupportedActions.GCD(AID.QuadrupleTechnicalFinish, 0);
        SupportedActions.GCD(AID.TechnicalFinish, 0);
        SupportedActions.GCD(AID.SingleTechnicalFinish, 0);
        SupportedActions.OGCD(AID.Flourish, 0, CDGroup.Flourish, 60.0f).EffectDuration = 30;
        SupportedActions.GCD(AID.SaberDance, 25);
        SupportedActions.OGCD(AID.ImprovisedFinish, 0, CDGroup.ImprovisedFinish, 1.5f).EffectDuration = 60;
        SupportedActions.OGCD(AID.Improvisation, 0, CDGroup.Improvisation, 120.0f);
        SupportedActions.GCD(AID.Tillana, 0);
        SupportedActions.OGCD(AID.FanDanceIV, 15, CDGroup.FanDanceIV, 1.0f);
        SupportedActions.GCD(AID.StarfallDance, 25);
    }
}
