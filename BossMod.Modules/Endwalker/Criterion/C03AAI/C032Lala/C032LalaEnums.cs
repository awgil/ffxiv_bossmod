namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

public enum OID : uint
{
    NBoss = 0x403E, // R4.500, x1
    NArcaneFont = 0x403F, // R1.000, spawn during fight
    NArcaneGlobe = 0x4040, // R1.500, spawn during fight
    NAloaloGolem = 0x4041, // R1.900, spawn during fight

    SBoss = 0x40DC, // R4.500, x1
    SArcaneFont = 0x40DD, // R1.000, spawn during fight
    SArcaneGlobe = 0x40DE, // R1.500, spawn during fight
    SAloaloGolem = 0x40DF, // R1.900, spawn during fight

    Helper = 0x233C, // R0.500, x33, 523 type
    ArrowBright = 0x1EB941, // R0.500, EventObj type, spawn during fight
    ArrowDim = 0x1EB942, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 872, // *Boss->player, no cast, single-target
    Teleport = 34932, // NBoss->location, no cast, single-target
    NStrategicStrike = 34989, // NBoss->players, 5.0s cast, single-target, tankbuster
    NInfernoTheorem = 34990, // NBoss->self, 5.0s cast, range 80 circle, raidwide
    SStrategicStrike = 35844, // SBoss->players, 5.0s cast, single-target, tankbuster
    SInfernoTheorem = 35845, // SBoss->self, 5.0s cast, range 80 circle, raidwide

    NAngularAdditionThree = 34953, // NBoss->self, 3.0s cast, single-target, visual (applies 'times three')
    NAngularAdditionFive = 36142, // NBoss->self, 3.0s cast, single-target, visual (applies 'times five')
    SAngularAdditionThree = 35808, // SBoss->self, 3.0s cast, single-target, visual (applies 'times three')
    SAngularAdditionFive = 36143, // SBoss->self, 3.0s cast, single-target, visual (applies 'times five')
    NArcaneBlightFront = 34955, // NBoss->self, 6.0s cast, single-target, visual (rotate then cleave front side)
    NArcaneBlightBack = 34956, // NBoss->self, 6.0s cast, single-target, visual (rotate then cleave back side)
    NArcaneBlightLeft = 34957, // NBoss->self, 6.0s cast, single-target, visual (rotate then cleave left side)
    NArcaneBlightRight = 34958, // NBoss->self, 6.0s cast, single-target, visual (rotate then cleave right side)
    NArcaneBlightAOE = 34959, // Helper->self, 6.0s cast, range 60 270-degree cone aoe
    SArcaneBlightFront = 35810, // SBoss->self, 6.0s cast, single-target, visual (rotate then cleave front side)
    SArcaneBlightBack = 35811, // SBoss->self, 6.0s cast, single-target, visual (rotate then cleave back side)
    SArcaneBlightLeft = 35812, // SBoss->self, 6.0s cast, single-target, visual (rotate then cleave left side)
    SArcaneBlightRight = 35813, // SBoss->self, 6.0s cast, single-target, visual (rotate then cleave right side)
    SArcaneBlightAOE = 35814, // Helper->self, 6.0s cast, range 60 270-degree cone aoe

    NArcaneArray1 = 34960, // NBoss->self, 3.0s cast, single-target, visual (create arrows & globes)
    NBrightPulseFirst = 34961, // Helper->self, 5.0s cast, range 8 width 8 rect
    NBrightPulseRest = 34962, // Helper->self, no cast, range 8 width 8 rect
    NRadiance1 = 34964, // NArcaneGlobe->self, no cast, range 60 circle inverted gaze
    NAnalysis = 34965, // NBoss->self, 3.0s cast, single-target, visual (apply x unseen debuffs)
    SArcaneArray1 = 35815, // SBoss->self, 3.0s cast, single-target, visual (create arrows & globes)
    SBrightPulseFirst = 35816, // Helper->self, 5.0s cast, range 8 width 8 rect
    SBrightPulseRest = 35817, // Helper->self, no cast, range 8 width 8 rect
    SRadiance1 = 35819, // SArcaneGlobe->self, no cast, range 60 circle inverted gaze
    SAnalysis = 35820, // SBoss->self, 3.0s cast, single-target, visual (apply x unseen debuffs)
    NTargetedLight = 36062, // NBoss->self, 5.0s cast, single-target, visual (inverted gaze)
    NTargetedLightAOE = 36063, // Helper->player, 5.5s cast, single-target, inverted gaze
    STargetedLight = 36064, // SBoss->self, 5.0s cast, single-target, visual (inverted gaze)
    STargetedLightAOE = 36065, // Helper->player, 5.5s cast, single-target, inverted gaze

    NPlanarTactics = 34968, // NBoss->self, 5.0s cast, single-target, visual (debuffs)
    NArcaneMine = 34969, // NBoss->self, 13.1s cast, single-target, visual (mines)
    NArcaneMineAOE = 34970, // Helper->self, 13.1s cast, range 8 width 8 rect
    NArcaneCombustion = 34971, // Helper->self, no cast, square explosion
    NMassiveExplosion1 = 34972, // Helper->self, no cast, range 60 circle, ??? (some sort of mine fail)
    NMassiveExplosion2 = 34973, // Helper->self, no cast, range 60 circle, ??? (some sort of mine fail)
    NSymmetricSurgeAOE = 34974, // Helper->players, no cast, range 6 circle, pair stack
    SPlanarTactics = 35823, // SBoss->self, 5.0s cast, single-target, visual (debuffs)
    SArcaneMine = 35824, // SBoss->self, 13.1s cast, single-target, visual (mines)
    SArcaneMineAOE = 35825, // Helper->self, 13.1s cast, range 8 width 8 rect
    SArcaneCombustion = 35826, // Helper->self, no cast, square explosion
    SMassiveExplosion1 = 35827, // Helper->self, no cast, range 60 circle, ??? (some sort of mine fail)
    SMassiveExplosion2 = 35828, // Helper->self, no cast, range 60 circle, ??? (some sort of mine fail)
    SSymmetricSurgeAOE = 35829, // Helper->players, no cast, range 6 circle, pair stack

    NArcaneArray2 = 34975, // NBoss->self, 3.0s cast, single-target, visual (create arrows and fonts)
    NSpatialTactics = 34976, // NBoss->self, 5.0s cast, single-target, visual (debuffs)
    NInfernoDivide = 34963, // NArcaneFont->self, no cast, range 50 width 8 cross
    NRadiance2 = 36127, // NArcaneGlobe->self, no cast, range 60 circle
    SArcaneArray2 = 35830, // SBoss->self, 3.0s cast, single-target, visual (create arrows and fonts)
    SSpatialTactics = 35831, // SBoss->self, 5.0s cast, single-target, visual (debuffs)
    SInfernoDivide = 35818, // SArcaneFont->self, no cast, range 50 width 8 cross
    SRadiance2 = 36128, // SArcaneGlobe->self, no cast, range 60 circle

    NSymmetricSurge = 34977, // NBoss->self, 5.0s cast, single-target, visual (stack debuffs)
    NArcanePlot = 34978, // NBoss->self, 3.0s cast, single-target, visual (create arrows)
    NConstructiveFigure = 34979, // NBoss->self, 3.0s cast, single-target, visual (golems)
    NAero = 34980, // NAloaloGolem->self, 14.0s cast, range 50 width 8 rect
    NArcanePoint = 34981, // NBoss->self, 5.0s cast, single-target, visual (make squares under players unsafe)
    NPowerfulLight = 34982, // Helper->self, no cast, ??? (square spreads)
    NExplosiveTheorem = 34983, // NBoss->self, 5.0s cast, single-target, visual (spreads with lingering puddles)
    NExplosiveTheoremAOE = 34984, // Helper->player, 5.0s cast, range 8 circle spread
    NTelluricTheorem = 34985, // Helper->location, 4.5s cast, range 8 circle puddle
    SSymmetricSurge = 35832, // SBoss->self, 5.0s cast, single-target, visual (stack debuffs)
    SArcanePlot = 35833, // SBoss->self, 3.0s cast, single-target, visual (create arrows)
    SConstructiveFigure = 35834, // SBoss->self, 3.0s cast, single-target, visual (golems)
    SAero = 35835, // SAloaloGolem->self, 14.0s cast, range 50 width 8 rect
    SArcanePoint = 35836, // SBoss->self, 5.0s cast, single-target, visual (make squares under players unsafe)
    SPowerfulLight = 35837, // Helper->self, no cast, ??? (square spreads)
    SExplosiveTheorem = 35838, // SBoss->self, 5.0s cast, single-target, visual (spreads with lingering puddles)
    SExplosiveTheoremAOE = 35839, // Helper->player, 5.0s cast, range 8 circle spread
    STelluricTheorem = 35840, // Helper->location, 4.5s cast, range 8 circle puddle
}

public enum SID : uint
{
    TimesThreeBoss = 3938, // *Boss->*Boss, extra=0x0
    TimesFiveBoss = 3939, // *Boss->*Boss, extra=0x0
    TimesThreePlayer = 3721, // *Boss->player, extra=0x0
    TimesFivePlayer = 3790, // *Boss->player, extra=0x0
    FrontUnseen = 3726, // *Boss->player, extra=0x297
    BackUnseen = 3727, // *Boss->player, extra=0x298
    RightUnseen = 3728, // *Boss->player, extra=0x299
    LeftUnseen = 3729, // *Boss->player, extra=0x29A
    SurgeVector = 3723, // *Boss->player, extra=0x0
    SubtractiveSuppressorAlpha = 3724, // *Boss->player, extra=0x1/0x2/0x3
    SubtractiveSuppressorBeta = 3725, // *Boss->player, extra=0x1/0x2/0x3/0x4
    ForwardMarch = 3715, // *Boss->player, extra=0x0
    ForcedMarch = 3719, // *Boss->player, extra=0x8/0x4
}

public enum IconID : uint
{
    BossRotateCW = 484, // *Boss
    BossRotateCCW = 485, // *Boss
    PlayerRotateCW = 493, // player
    PlayerRotateCCW = 494, // player
    RadianceSuccess = 503, // player
    RadianceFail = 504, // player
    StrategicStrike = 243, // player
    SymmetricSurge = 62, // player
    ArcanePoint = 23, // player
    ExplosiveTheorem = 229, // player
}

public enum TetherID : uint
{
    TargetedLight = 17, // player->*Boss
}
