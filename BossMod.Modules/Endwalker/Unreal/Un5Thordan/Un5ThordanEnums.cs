namespace BossMod.Endwalker.Unreal.Un5Thordan;

public enum OID : uint
{
    Boss = 0x4071, // R3.800, x1
    Zephirin = 0x4072, // R2.200, spawn during fight - sacred cross
    Adelphel = 0x4073, // R2.200, spawn during fight - tank split 1
    Janlenoux = 0x4074, // R2.200, spawn during fight - tank split 2
    Vellguine = 0x4075, // R2.200, spawn during fight
    Paulecrain = 0x4076, // R2.200, spawn during fight
    Ignasse = 0x4077, // R2.200, spawn during fight
    Grinnaux = 0x4078, // R2.200, spawn during fight
    Hermenost = 0x4079, // R2.200, spawn during fight
    Guerrique = 0x407A, // R2.200, spawn during fight
    Charibert = 0x407B, // R2.200, spawn during fight
    Haumeric = 0x407C, // R2.200, spawn during fight
    Noudenet = 0x407D, // R2.200, spawn during fight
    Ascalon = 0x407E, // R3.800, x1 - sword
    MeteorCircle = 0x407F, // R1.000, spawn during fight - big middle meteor
    CometCircle = 0x4080, // R1.000, spawn during fight - small meteors in a circle
    Helper = 0x4081, // R0.500, x2, and more spawn during fight
    HiemalStorm = 0x1E9729, // R0.500, EventObj type, spawn during fight

    DragonEyeN = 0x1E9E27, // R2.000, x1, EventObj type
    DragonEyeNE = 0x1E9E28, // R2.000, x1, EventObj type
    DragonEyeE = 0x1E9E29, // R2.000, x1, EventObj type
    DragonEyeSE = 0x1E9E2A, // R2.000, x1, EventObj type
    DragonEyeS = 0x1E9E2B, // R2.000, x1, EventObj type
    DragonEyeSW = 0x1E9E2C, // R2.000, x1, EventObj type
    DragonEyeW = 0x1E9E2D, // R2.000, x1, EventObj type
    DragonEyeNW = 0x1E9E2E, // R2.000, x1, EventObj type

    //_Gen_Actor1e9e2f = 0x1E9E2F, // R2.000, x1, EventObj type
    //_Gen_Actor1e9e30 = 0x1E9E30, // R2.000, x1, EventObj type
    //_Gen_Actor1e9e31 = 0x1E9E31, // R2.000, x1, EventObj type
    //_Gen_Actor1e9e32 = 0x1E9E32, // R2.000, x1, EventObj type
    //_Gen_Actor1e9e33 = 0x1E9E33, // R2.000, x1, EventObj type
    //_Gen_Actor1e9e34 = 0x1E9E34, // R2.000, x1, EventObj type
    //_Gen_Actor1e9e35 = 0x1E9E35, // R2.000, x1, EventObj type
    //_Gen_Actor1e9e36 = 0x1E9E36, // R2.000, x1, EventObj type
    //_Gen_Actor1e9e37 = 0x1E9E37, // R2.000, x1, EventObj type

    //_Gen_Actor1e9e3b = 0x1E9E3B, // R2.000, x1, EventObj type
    //_Gen_Actor1e9b99 = 0x1E9B99, // R2.000, x1, EventObj type
    //_Gen_Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttackKnight = 870, // Adelphel/Janlenoux->player, no cast, single-target
    AutoAttackBoss = 35260, // Boss->player, no cast, single-target
    AscalonsMight = 35261, // Boss->self, no cast, range 8+R 90-degree cone, minor single tank buster
    AscalonsMercy = 35262, // Boss->self, 3.0s cast, range 31+R 20-degree cone, central fanned out aoe attack
    AscalonsMercyAOE = 35263, // Helper->self, 3.0s cast, range 34+R 20-degree cone
    LightningStorm = 35264, // Boss->self, 3.5s cast, single-target, visual (spread)
    LightningStormAOE = 35265, // Helper->players, 4.0s cast, range 5 circle spread
    Meteorain = 35266, // Boss->self, 2.7s cast, single-target, visual (puddles)
    MeteorainAOE = 35267, // Helper->location, 3.0s cast, range 6 circle
    AncientQuaga = 35268, // Boss->self, 3.0s cast, range 80+R circle, raidwide
    HeavenlyHeel = 35270, // Boss->player, 4.0s cast, single-target, tankbuster
    DragonsEye = 35271, // Boss->self, 3.0s cast, single-target, visual (moves dragon eye and/or buffs light of ascalon?)
    DragonsGaze = 35272, // Boss->self, 3.0s cast, range 80+R circle gaze (from boss)
    DragonsGlory = 35273, // Helper->self, 3.0s cast, range 80+R circle gaze (from eye)
    DragonsRage = 35274, // Boss->players, 5.0s cast, range 6 circle stack

    KnightAppear = 35255, // Charibert/Hermenost/Zephirin/Vellguine/Paulecrain/Ignasse/Janlenoux/Adelphel/Grinnaux/Haumeric/Noudenet/Guerrique->self, no cast, single-target, visual
    KnightDisappear = 35256, // Charibert/Hermenost/Zephirin/Vellguine/Paulecrain/Ignasse/Adelphel/Janlenoux/Haumeric/Grinnaux/Noudenet/Guerrique->self, no cast, single-target, visual
    KnightUltimateEnd = 35257, // Charibert/Hermenost/Haumeric/Zephirin/Grinnaux/Noudenet/Ignasse/Vellguine/Adelphel/Janlenoux/Guerrique/Paulecrain->self, no cast, single-target, visual
    IntermissionStart = 35258, // Boss->self, no cast, single-target, visual
    BossReappear = 35259, // Boss->self, no cast, single-target, visual

    Conviction = 35291, // Hermenost->self, 4.2s cast, single-target, visual (towers)
    ConvictionAOE = 35292, // Helper->location, 7.0s cast, range 3 circle tower
    EternalConviction = 35293, // Helper->self, no cast, range 80+R circle (tower punish, raidwide paralysis)
    Heavensflame = 35306, // Charibert->self, 2.5s cast, single-target, visual (puddles)
    HeavensflameAOE = 35307, // Helper->location, 3.0s cast, range 6 circle puddle
    HolyChain = 35308, // Helper->self, no cast, break chains with damage based on proximity

    SacredCross = 35279, // Zephirin->self, 20.0s cast, range 80+R circle, raidwide with damage scaled by remaining hp
    SpiralThrust = 35301, // Vellguine/Paulecrain/Ignasse->self, 6.0s cast, range 52+R width 12 rect

    DivineRight = 35282, // Janlenoux/Adelphel->self, 3.0s cast, single-target, assign/swap attack/defense buffs
    HeavenlySlash = 35283, // Adelphel/Janlenoux->self, no cast, range 8+R ?-degree cone, minor attack during tank split
    HoliestOfHoly = 35284, // Adelphel/Janlenoux->self, 3.0s cast, range 80+R circle, raidwide during tank split
    HolyBladedance = 35285, // Janlenoux/Adelphel->players, 4.0s cast, single-target, tank split tank buster
    SkywardLeap = 35304, // Vellguine/Paulecrain/Ignasse->self, no cast, range 80+R circle raidwide with ? falloff

    DimensionalCollapse = 35288, // Grinnaux->self, 5.5s cast, single-target, visual (growing voidzones)
    DimensionalCollapseAOE = 35289, // Helper->location, 6.0s cast, range 3+6 circle voidzone (area-of-influence-up stacks to 6)
    FaithUnmoving = 35290, // Grinnaux->self, no cast, range 80+R circle knockback 16
    SpiralPierce = 35302, // Paulecrain/Vellguine/Ignasse->players, 6.0s cast, width 12 rect charge on tethered target
    HiemalStorm = 35309, // Haumeric->self, 2.5s cast, single-target, visual (spread that leaves voidzone)
    HiemalStormAOE = 35310, // Helper->players, no cast, range 6 circle spread
    HolyMeteor = 35311, // Noudenet->self, 3.0s cast, single-target, visual (prey icons)
    CometImpact = 35314, // CometCircle->self, no cast, range 80+R circle (raidwide if small comet is not killed in time)
    TargetedComet = 35316, // Helper->location, no cast, range 4 circle (spread for prey targets)
    HeavyImpactVisual = 35294, // Guerrique->self, no cast, single-target, visual (expanding aoes start)
    HeavyImpactAOE1 = 35295, // Helper->self, 3.0s cast, range 6+R 270?-degree cone
    HeavyImpactAOE2 = 35296, // Helper->self, 3.0s cast, range 12+R 270?-degree cone
    HeavyImpactAOE3 = 35297, // Helper->self, 3.0s cast, range 18+R 270?-degree cone
    HeavyImpactAOE4 = 35298, // Helper->self, 3.0s cast, range 27+R 270?-degree cone

    UltimateEnd = 35276, // Boss->self, no cast, single-target, visual (heavy raidwide)
    UltimateEndAOE = 35277, // Helper->self, no cast, range 80+R circle raidwide
    LightOfAscalon = 35278, // Helper->self, no cast, range 80+R circle knockback 3

    KnightsOfTheRound = 35275, // Boss->self, 3.0s cast, single-target, visual (trio start)
    HolyShieldBash = 35286, // Adelphel/Janlenoux->player, 4.0s cast, single-target (stun healer)
    SpearOfTheFury = 35281, // Zephirin->self, 6.0s cast, wild charge on stunned target
    SacredCrossFail = 35287, // Zephirin->self, no cast, range 80+R circle, wipe if anyone dies to spear of the fury

    HeavenswardLeap = 35305, // Vellguine/Paulecrain/Ignasse->self, no cast, range 80+R circle, raidwide
    SacredCrossInvuln = 35280, // Zephirin->self, 25.0s cast, range 80+R circle, raidwide with damage scaled by remaining hp (when boss is invulnerable)
    PureOfSoul = 35312, // Charibert->location, 3.0s cast, range 80 circle, raidwide
    PureOfSoulVisual = 35313, // Noudenet/Haumeric->self, 3.0s cast, range 80 circle, visual?
    AbsoluteConvictionVisual = 35299, // Hermenost/Guerrique/Grinnaux->self, no cast, single-target, visual
    AbsoluteConviction = 35300, // Helper->self, no cast, range 80+R circle, raidwide

    Enrage = 35269, // Boss->self, 10.0s cast, range 80+R circle, enrage
}

public enum SID : uint
{
    DamageUp = 290,
    SwordOfTheHeavens = 944,
    ShieldOfTheHeavens = 945
}

public enum IconID : uint
{
    LightningStorm = 24, // player
    DragonsRage = 62, // player
    HeavenlyHeel = 198, // player
    SkywardLeap = 14, // player
    HiemalStorm = 29, // player
    TargetedComet = 11, // player
    HolyShieldBash = 16, // player
}

public enum TetherID : uint
{
    ThordanInvul = 1,
    SpiralPierce = 5,
    BurningChains = 9
}
