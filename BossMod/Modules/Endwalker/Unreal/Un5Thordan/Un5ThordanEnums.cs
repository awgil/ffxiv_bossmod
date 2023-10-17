namespace BossMod.Endwalker.Unreal.Un5Thordan;

public enum OID : uint
{
    Thordan = 0x4071,
    Zephirin = 0x4072, // sacred cross
    Adelphel = 0x4073, // tank split 1
    Janlenoux = 0x4074, // tank split 2
    Vellguine = 0x4075,
    Paulecrain = 0x4076,
    Ignasse = 0x4077,
    Grinnaux = 0x4078,
    Guerrique = 0x407A,
    Charibert = 0x407B,
    Haumeric = 0x407C,
    Noudenet = 0x407D,
    Ascalon = 0x407E, // sword
    MeteorCircle = 0x407F, // big middle meteor
    CometCircle = 0x4080, // small meteors in a circle
    Helper = 0x4081,
    HiemalStorm = 0x1E9729
};

public enum AID : uint
{
    AscalonsMight = 35261, // minor single tank buster
    AscalonsMercy = 35262, // fanned out aoe attack
    AscalonsMercyHelper = 35263, 
    LightningStorm = 35264, // spread
    LightningStormAOE = 35265,
    Meteorain = 35266,
    MeteorainAOE = 35267,
    AncientQuaga = 35268, // raidwide
    HeavenlyHeel = 35270, // single tank buster
    DragonsEye = 35271, // moves dragon eye
    DragonsGaze = 35272, // gaze attack
    KnightsOfTheRound = 35265, // phase advance?
    UltimateEnd = 35277, // raid wide after sword pushes
    LightOfAscalon = 35278, // sword pushes
    SacredCross = 35279, // dps check
    SacredCrossInvul = 35280, // dps check during invul
    SpearOfTheFury = 35281, // press F for Haurchefant
    DivineRight = 35282, // attack/defense buff during tank split
    HeavenlySlash = 35283, // minor attack during tank split
    HoliestOfHoly = 35284, // raidwide during tank split
    HolyBladedance = 35285, // tank split tank buster
    HolyShieldBash = 35286, // healer stun
    DimensionalCollapse = 35289, // puddle before push
    FaithUnmoving = 35290, // push
    Conviction = 35291, // tower
    ConvictionAOE = 35292, 
    EternalConviction = 35293, // tower punish, raidwide paralysis
    HeavyImpactA = 35294, // slow wave aoes during meteors?
    HeavyImpactB = 35295,
    HeavyImpactC = 35296,
    HeavyImpactD = 35297,
    HeavyImpactE = 35298,
    SpiralThrust = 35301, // line attack
    SpiralPierce = 35302, // tethered line attack
    SkywardLeap = 35304, // dive bomb
    HeavenswardLeap = 35305, // big dive bombs?
    Heavensflame = 35306, // puddle cast
    HeavensflameAOE = 35307, // puddles
    HolyChain = 35308, // places burning chains tether
    HiemalStorm = 35309, // spread before push
    HiemalStormAOE = 35310, // ?
    HolyMeteor = 35311, // ?
    PureOfSoulDamage = 35312, // raidwide
    PureOfSoul = 35313,
    AbsoluteConviction = 35299, // raidwide
    AbsoluteConvictionDamage = 35300,
    TargetedComet = 35316, // targeted comet
};

public enum IconID : uint
{
    TargetedComet = 11,
    SkywardLeap = 14,
    HolyShieldBash = 16,
    LightningStorm = 24,
    HiemalStorm = 29,
    HeavenlyHeel = 198,
};

public enum TetherID : uint
{
    ThordanInvul = 1,
    SpiralPierce = 5,
    BurningChains = 9
};

public enum StatusID : uint
{
    DamageUp = 290,
    SwordOfTheHeavens = 944,
    ShieldOfTheHeavens = 945
};
