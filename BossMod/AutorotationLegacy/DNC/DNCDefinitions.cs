namespace BossMod.DNC;

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
    Peloton = 1199, // applied by Peloton to self
    DancePartner = 1824, // applied by Closed Position to target
    ClosedPosition = 1823, // applied by Closed Position to self
}
