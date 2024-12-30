namespace BossMod.Heavensward.Extreme.Ex3Thordan;

public enum OID : uint
{
    Boss = 0x1453, // R3.800, x1
    Zephirin = 0x1454, // R2.200, spawn during fight - sacred cross
    Adelphel = 0x1455, // R2.200, spawn during fight - tank split 1
    Janlenoux = 0x1456, // R2.200, spawn during fight - tank split 2
    Vellguine = 0x1457, // R2.200, spawn during fight
    Paulecrain = 0x1458, // R2.200, spawn during fight
    Ignasse = 0x1459, // R2.200, spawn during fight
    Grinnaux = 0x145A, // R2.200, spawn during fight
    Hermenost = 0x145B, // R2.200, spawn during fight
    Guerrique = 0x145C, // R2.200, spawn during fight
    Charibert = 0x145D, // R2.200, spawn during fight
    Haumeric = 0x145E, // R2.200, spawn during fight
    Noudenet = 0x145F, // R2.200, spawn during fight
    Ascalon = 0x1460, // R3.800, x1 - sword
    MeteorCircle = 0x1461, // R1.000, spawn during fight - big middle meteor
    CometCircle = 0x1462, // R1.000, spawn during fight - small meteors in a circle
    Helper = 0x41463, // R0.500, x2, and more spawn during fight
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
    AutoAttackBoss = 5245, // Boss->player, no cast, single-target
    AscalonsMight = 5246, // Boss->self, no cast, range 8+R 90-degree cone, minor single tank buster
    AscalonsMercy = 5247, // Boss->self, 3.0s cast, range 31+R 20-degree cone, central fanned out aoe attack
    AscalonsMercyAOE = 5248, // Helper->self, 3.0s cast, range 34+R 20-degree cone
    LightningStorm = 5249, // Boss->self, 3.5s cast, single-target, visual (spread)
    LightningStormAOE = 5250, // Helper->players, 4.0s cast, range 5 circle spread
    Meteorain = 5151, // Boss->self, 2.7s cast, single-target, visual (puddles)
    MeteorainAOE = 5252, // Helper->location, 3.0s cast, range 6 circle
    AncientQuaga = 5253, // Boss->self, 3.0s cast, range 80+R circle, raidwide
    HeavenlyHeel = 5255, // Boss->player, 4.0s cast, single-target, tankbuster
    DragonsEye = 5256, // Boss->self, 3.0s cast, single-target, visual (moves dragon eye and/or buffs light of ascalon?)
    DragonsGaze = 5257, // Boss->self, 3.0s cast, range 80+R circle gaze (from boss)
    DragonsGlory = 5258, // Helper->self, 3.0s cast, range 80+R circle gaze (from eye)
    DragonsRage = 5259, // Boss->players, 5.0s cast, range 6 circle stack

    KnightAppear = 4120, // Charibert/Hermenost/Zephirin/Vellguine/Paulecrain/Ignasse/Janlenoux/Adelphel/Grinnaux/Haumeric/Noudenet/Guerrique->self, no cast, single-target, visual
    KnightDisappear = 4121, // Charibert/Hermenost/Zephirin/Vellguine/Paulecrain/Ignasse/Adelphel/Janlenoux/Haumeric/Grinnaux/Noudenet/Guerrique->self, no cast, single-target, visual
    KnightUltimateEnd = 4122, // Charibert/Hermenost/Haumeric/Zephirin/Grinnaux/Noudenet/Ignasse/Vellguine/Adelphel/Janlenoux/Guerrique/Paulecrain->self, no cast, single-target, visual
    IntermissionStart = 4123, // Boss->self, no cast, single-target, visual
    BossReappear = 4124, // Boss->self, no cast, single-target, visual

    Conviction = 5276, // Hermenost->self, 4.2s cast, single-target, visual (towers)
    ConvictionAOE = 5277, // Helper->location, 7.0s cast, range 3 circle tower
    EternalConviction = 5278, // Helper->self, no cast, range 80+R circle (tower punish, raidwide paralysis)
    Heavensflame = 5291, // Charibert->self, 2.5s cast, single-target, visual (puddles)
    HeavensflameAOE = 5292, // Helper->location, 3.0s cast, range 6 circle puddle
    HolyChain = 5293, // Helper->self, no cast, break chains with damage based on proximity

    SacredCross = 5264, // Zephirin->self, 20.0s cast, range 80+R circle, raidwide with damage scaled by remaining hp
    SpiralThrust = 5286, // Vellguine/Paulecrain/Ignasse->self, 6.0s cast, range 52+R width 12 rect

    DivineRight = 5267, // Janlenoux/Adelphel->self, 3.0s cast, single-target, assign/swap attack/defense buffs
    HeavenlySlash = 5268, // Adelphel/Janlenoux->self, no cast, range 8+R ?-degree cone, minor attack during tank split
    HoliestOfHoly = 5269, // Adelphel/Janlenoux->self, 3.0s cast, range 80+R circle, raidwide during tank split
    HolyBladedance = 5270, // Janlenoux/Adelphel->players, 4.0s cast, single-target, tank split tank buster
    SkywardLeap = 5289, // Vellguine/Paulecrain/Ignasse->self, no cast, range 80+R circle raidwide with ? falloff

    DimensionalCollapse = 5273, // Grinnaux->self, 5.5s cast, single-target, visual (growing voidzones)
    DimensionalCollapseAOE = 5274, // Helper->location, 6.0s cast, range 3+6 circle voidzone (area-of-influence-up stacks to 6)
    FaithUnmoving = 35290, // Grinnaux->self, no cast, range 80+R circle knockback 16
    SpiralPierce = 5287, // Paulecrain/Vellguine/Ignasse->players, 6.0s cast, width 12 rect charge on tethered target
    HiemalStorm = 5294, // Haumeric->self, 2.5s cast, single-target, visual (spread that leaves voidzone)
    HiemalStormAOE = 5295, // Helper->players, no cast, range 6 circle spread
    HolyMeteor = 5296, // Noudenet->self, 3.0s cast, single-target, visual (prey icons)
    CometImpact = 5299, // CometCircle->self, no cast, range 80+R circle (raidwide if small comet is not killed in time)
    TargetedComet = 5301, // Helper->location, no cast, range 4 circle (spread for prey targets)
    HeavyImpactVisual = 5279, // Guerrique->self, no cast, single-target, visual (expanding aoes start)
    HeavyImpactAOE1 = 5280, // Helper->self, 3.0s cast, range 6+R 270?-degree cone
    HeavyImpactAOE2 = 5281, // Helper->self, 3.0s cast, range 12+R 270?-degree cone
    HeavyImpactAOE3 = 5282, // Helper->self, 3.0s cast, range 18+R 270?-degree cone
    HeavyImpactAOE4 = 5283, // Helper->self, 3.0s cast, range 27+R 270?-degree cone

    UltimateEnd = 5261, // Boss->self, no cast, single-target, visual (heavy raidwide)
    UltimateEndAOE = 5262, // Helper->self, no cast, range 80+R circle raidwide
    LightOfAscalon = 5263, // Helper->self, no cast, range 80+R circle knockback 3

    KnightsOfTheRound = 5260, // Boss->self, 3.0s cast, single-target, visual (trio start)
    HolyShieldBash = 5271, // Adelphel/Janlenoux->player, 4.0s cast, single-target (stun healer)
    SpearOfTheFury = 5266, // Zephirin->self, 6.0s cast, wild charge on stunned target
    SacredCrossFail = 5272, // Zephirin->self, no cast, range 80+R circle, wipe if anyone dies to spear of the fury

    HeavenswardLeap = 5290, // Vellguine/Paulecrain/Ignasse->self, no cast, range 80+R circle, raidwide
    SacredCrossInvuln = 5265, // Zephirin->self, 25.0s cast, range 80+R circle, raidwide with damage scaled by remaining hp (when boss is invulnerable)
    PureOfSoul = 5297, // Charibert->location, 3.0s cast, range 80 circle, raidwide
    PureOfSoulVisual = 5298, // Noudenet/Haumeric->self, 3.0s cast, range 80 circle, visual?
    AbsoluteConvictionVisual = 5284, // Hermenost/Guerrique/Grinnaux->self, no cast, single-target, visual
    AbsoluteConviction = 5285, // Helper->self, no cast, range 80+R circle, raidwide

    Enrage = 5254, // Boss->self, 10.0s cast, range 80+R circle, enrage
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
