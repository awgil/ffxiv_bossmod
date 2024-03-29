namespace BossMod.Endwalker.Alliance.A23Halone;

public enum OID : uint
{
    Boss = 0x3DAD, // R7.030, x1
    Helper = 0x233C, // R0.500, x48
    GlacialSpearSmall = 0x3DAE, // R3.500, x3 spawn during fight
    GlacialSpearLarge = 0x3DAF, // R4.000, x1 spawn during fight
};

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    Teleport = 32124, // Boss->location, no cast, single-target
    RainOfSpears = 32121, // Boss->self, 4.3s cast, single-target, visual (triple raidwide)
    RainOfSpearsFirst = 32122, // Helper->self, 5.0s cast, range 60 circle, raidwide
    RainOfSpearsRest = 32123, // Helper->self, no cast, range 60 circle, raidwide
    Tetrapagos = 32069, // Boss->self, 13.0s cast, single-target
    TetrapagosHailstormPrepare = 32070, // Helper->self, 2.5s cast, range 10 circle
    TetrapagosSwirlPrepare = 32071, // Helper->self, 2.5s cast, range 10-30 donut
    TetrapagosRightrimePrepare = 32072, // Helper->self, 2.5s cast, range 30 180-degree cone
    TetrapagosLeftrimePrepare = 32073, // Helper->self, 2.5s cast, range 30 180-degree cone
    TetrapagosHailstormVisual = 32074, // Boss->self, no cast, single-target
    TetrapagosSwirlVisual = 32075, // Boss->self, no cast, single-target
    TetrapagosRightrimeVisual = 32076, // Boss->self, no cast, single-target
    TetrapagosLeftrimeVisual = 32077, // Boss->self, no cast, single-target
    TetrapagosHailstormAOE = 32078, // Helper->self, 1.6s cast, range 10 circle
    TetrapagosSwirlAOE = 32079, // Helper->self, 1.8s cast, range 10-30 donut
    TetrapagosRightrimeAOE = 32080, // Helper->self, 1.7s cast, range 30 180-degree cone
    TetrapagosLeftrimeAOE = 32081, // Helper->self, 1.7s cast, range 30 180-degree cone
    DoomSpear = 32941, // Boss->location, 7.0s cast, single-target, visual (3 towers)
    DoomSpearJump = 32942, // Boss->location, no cast, single-target, visual (jump to subsequent towers)
    DoomSpearAOE1 = 32943, // Helper->self, 8.0s cast, range 6 circle tower
    DoomSpearAOE2 = 32944, // Helper->self, 10.0s cast, range 6 circle tower
    DoomSpearAOE3 = 32945, // Helper->self, 12.0s cast, range 6 circle tower
    SpearsThree = 32119, // Boss->self, 4.3s cast, single-target, visual (tankbuster)
    SpearsThreeAOE = 32120, // Helper->player, 5.0s cast, range 5 circle tankbuster
    ThousandfoldThrustFirst1 = 32082, // Boss->self, 5.0s cast, single-target, visual (first cast, same direction as caster)
    ThousandfoldThrustFirst2 = 32083, // Boss->self, 5.0s cast, single-target, visual (first cast, opposite direction from caster)
    ThousandfoldThrustFirst3 = 32084, // Boss->self, 5.0s cast, single-target, visual (first cast, ...)
    ThousandfoldThrustFirst4 = 32085, // Boss->self, 5.0s cast, single-target, visual (first cast, ...)
    ThousandfoldThrustRest1 = 32086, // Boss->self, no cast, single-target, visual (remaining casts)
    ThousandfoldThrustRest2 = 32799, // Boss->self, no cast, single-target, visual (remaining casts)
    ThousandfoldThrustRest3 = 32800, // Boss->self, no cast, single-target, visual (remaining casts)
    ThousandfoldThrustRest4 = 32801, // Boss->self, no cast, single-target, visual (remaining casts)
    ThousandfoldThrustAOEFirst = 32087, // Helper->self, 6.3s cast, range 30 180-degree cone
    ThousandfoldThrustAOERest = 32088, // Helper->self, no cast, range 30 180-degree cone
    Lochos = 32090, // Boss->self, 5.0s cast, single-target, visual (two half-arena cleaves)
    LochosFirst = 32091, // Helper->self, 1.0s cast, range 60 width 30 rect
    LochosRest = 32092, // Helper->self, no cast, range 60 width 30 rect
    WillOfTheFury = 32093, // Boss->self, 3.0s cast, single-target
    WillOfTheFuryAOE1 = 32094, // Helper->self, 5.0s cast, range 24-30 donut
    WillOfTheFuryAOE2 = 32095, // Helper->self, 7.0s cast, range 18-24 donut
    WillOfTheFuryAOE3 = 32096, // Helper->self, 9.0s cast, range 12-18 donut
    WillOfTheFuryAOE4 = 32097, // Helper->self, 11.0s cast, range 6-12 donut
    WillOfTheFuryAOE5 = 32098, // Helper->self, 13.0s cast, range 6 circle
    WrathOfHalone = 32099, // Boss->location, 8.0s cast, single-target, visual (proximity)
    WrathOfHaloneAOE = 32100, // Helper->self, 8.9s cast, range 60 circle with ? falloff
    Disappear = 32104, // Boss->self, no cast, single-target, visual
    IceDart = 32102, // Helper->players, 6.0s cast, range 6 circle spread
    Niphas = 32105, // GlacialSpearSmall->self, 8.0s cast, range 9 circle
    Cheimon1 = 32106, // GlacialSpearLarge->self, 8.0s cast, single-target, visual (rotating aoe, ? dir)
    Cheimon2 = 32107, // GlacialSpearLarge->self, 8.0s cast, single-target, visual (rotating aoe, ? dir)
    CheimonAOE = 32108, // Helper->self, 8.0s cast, range 30 width 10 rect
    FurysAegis = 32110, // Boss->self, no cast, single-target, visual (end add phase)
    Shockwave = 32111, // Helper->self, 1.0s cast, range 60 circle
    FurysAegisAOE1 = 32112, // Helper->self, 8.9s cast, range 60 circle
    FurysAegisAOE2 = 32113, // Helper->self, 9.4s cast, range 60 circle
    FurysAegisAOE3 = 32114, // Helper->self, 9.8s cast, range 60 circle
    FurysAegisAOE4 = 32115, // Helper->self, 10.4s cast, range 60 circle
    FurysAegisAOE5 = 32116, // Helper->self, 11.0s cast, range 60 circle
    FurysAegisAOE6 = 32117, // Helper->self, 17.3s cast, range 60 width 60 rect
    TetrapagosThrust = 32089, // Boss->self, 13.0s cast, single-target, visual (tetrapagos + thousandfold thrust)
    Chalaza = 32101, // Boss->self, 3.0s cast, single-target, visual (stack + spread)
    IceRondel = 32103, // Helper->players, 6.0s cast, range 6 circle stack
};

public enum IconID : uint
{
    SpearsThree = 343, // player
    ThousandfoldThrust1 = 386, // Boss
    ThousandfoldThrust2 = 387, // Boss
    ThousandfoldThrust3 = 388, // Boss
    ThousandfoldThrust4 = 389, // Boss
    IceDart = 311, // player
    IceRondel = 318, // player
    Cheimon1 = 156, // GlacialSpearLarge
    Cheimon2 = 157, // GlacialSpearLarge
};
