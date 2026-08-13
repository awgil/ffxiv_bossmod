namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

public enum OID : uint
{
    PariOfPlenty = 0x4A70, // R5.016, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    FlyingCarpet = 0x4A74, // R4.356, x8
    FalseFlame = 0x4A71, // R5.016, x4
    FieryBauble = 0x4A72, // R6.000, x8
    IcyBauble = 0x4A73, // R5.000, x1
    PathPoints = 0x4A75, // R1.000, x6 - Used for pathing mainly in the fight
    BurnPuddle = 0x1EBF20, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 45545, // PariOfPlenty->player, no cast, single-target
    HeatBurst = 45517, // PariOfPlenty->self, 5.0s cast, range 60 circle
    Ability = 45421, // PariOfPlenty->location, no cast, single-target

    FireflightByPyrelightLeft = 45436, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByPyrelightRight = 45437, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByEmberlightLeft = 45434, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByEmberlightRight = 45435, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByEmberlightSpread = 45444, // Helper->players, no cast, range 3 circle
    FireflightByPyrelightStack = 45445, // Helper->players, no cast, range 3 circle
    SunCirclet = 45448, // PariOfPlenty->self, 2.0s cast, range ?-60 donut

    WheelOfFableflightRight = 45478, // 4A71->self, 11.0s cast, range 40 width 4 rect
    WheelOfFableflightLeft = 45479, // 4A71->self, 11.0s cast, range 40 width 4 rect
    WheelOfFireflight = 45469, // PariOfPlenty/4A71->self, no cast, range 40 ?-degree cone
    WheelOfFireflight1 = 45470, // PariOfPlenty/4A71->self, no cast, range 40 ?-degree cone
    WheelOfFireflight2 = 45471, // PariOfPlenty/4A71->self, no cast, range 40 ?-degree cone
    WheelOfFireflight3 = 45472, // PariOfPlenty/4A71->self, no cast, range 40 ?-degree cone
    KindledFlameStack = 45538, // Boss->self, 8.0s cast, single-target
    ScatteredKindlingSpread = 45536, // PariOfPlenty->self, 8.0s cast, single-target
    KindledFlame1 = 45539, // Helper->players, no cast, range 6 circle
    ScatteredKindling1 = 45537, // Helper->player, no cast, range 6 circle

    FireOfVictory = 45519, // PariOfPlenty->player, 5.0s cast, range 4 circle

    FireflightFourLongNightsRight = 45467, // Boss->self, 17.0s cast, range 40 width 4 rect
    FireflightFourLongNightsLeft = 45468, // Boss->self, 17.0s cast, range 40 width 4 rect
    FellFirework = 45476, // Helper->player, no cast, range 3 circle
    FireWell = 45477, // Helper->players, no cast, range 3 circle

    ParisCurse = 45551, // PariOfPlenty->self, 5.0s cast, range 60 circle
    CharmingBaubles = 45503, // PariOfPlenty->self, 3.0s cast, single-target
    CharmingBaubles1 = 45497, // PariOfPlenty->self, no cast, single-target
    BurningGleam = 45548, // 4A72->self, 7.0s cast, range 40 width 10 cross
    ChillingGleam = 45501, // 4A73->self, 2.0s cast, range 40 width 10 cross
    ThievesWeave = 46754, // PariOfPlenty->self, 3.0s cast, single-target
    CurseFableflightLeft = 45438, // _Gen_FalseFlame->self, 24.1s cast, range 40 width 4 rect
    CurseFableflightRight = 45439, // _Gen_FalseFlame->self, 24.1s cast, range 40 width 4 rect
    CarpetTeleport = 45504, // 4A74->self, 1.0s cast, single-target
    CarpetTeleport1 = 47153, // 4A74->location, no cast, single-target
    Unravel = 45506, // 4A74->self, 3.0s cast, single-target
    HighFirePowder = 45524, // Helper->location, no cast, range 15 circle
    FirePowder = 45523, // Helper->self, no cast, range 15 circle

    SpurningFlames = 45482, // PariOfPlenty->self, 7.0s cast, range 40 circle
    ImpassionedSparks = 45483, // PariOfPlenty->self, 5.0s cast, single-target
    ImpassionedSparks1 = 45484, // PariOfPlenty->self, no cast, single-target
    ImpassionedSparks2 = 45485, // Helper->self, 2.0s cast, single-target
    ImpassionedSparks3 = 45488, // Helper->self, 6.0s cast, range 8 circle
    BurningPillar = 45527, // Helper->self, 4.0s cast, range 10 circle
    HotFoot = 45542, // Helper->player, no cast, range 10 circle
    ScouringScorn = 45491, // PariOfPlenty->self, 6.0s cast, range 40 circle

    Doubling = 47041, // PariOfPlenty->self, 3.0s cast, single-target
    TowerExplosion = 45532, // Helper->self, 8.0s cast, range 4 circle
    FableflightLeft = 45450, // 4A71->self, 8.0s cast, range 40 width 4 rect
    FableflightLeft1 = 45452, // 4A71->self, 15.0s cast, range 40 width 4 rect
    FableflightRight = 45449, // 4A71->self, 8.0s cast, range 40 width 4 rect
    FableflightRight1 = 45451, // 4A71->self, 15.0s cast, range 40 width 4 rect

    KindledFlame3 = 45540, // PariOfPlenty->self, 5.0s cast, single-target
    KindledFlame2 = 45541, // Helper->players, no cast, range 6 circle

    CharmedFlightFourNightsRight = 47031, // PariOfPlenty->self, 17.5s cast, range 40 width 4 rect
    CharmedFlightFourNightsLeft = 47032, // PariOfPlenty->self, 17.5s cast, range 40 width 4 rect

    CarpetRide = 45441, // PariOfPlenty/4A71->location, no cast, single-target
    CarpetRide1 = 45442, // Helper->location, 2.0s cast, ???
    CarpetRide2 = 45443, // Helper->location, 2.0s cast, ???
    CarpetRide3 = 45440, // 4A71->location, no cast, single-target
    CarpetRide4 = 46574, // Helper->location, 2.1s cast, ???
    CarpetRide5 = 46573, // Helper->location, 2.1s cast, ???
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    WitchHunt = 2970, // none->Boss, extra=0x3F5/0x3F4
    DarkResistanceDown = 3619, // Helper->player, extra=0x0

    CurseOfSolitude = 4615, // none->player, extra=0x0
    CurseOfImmolation = 4617, // none->player, extra=0x0
    CurseOfCompanionship = 4616, // none->player, extra=0x0
    FireResistanceDownII = 2937, // Helper->player, extra=0x0

    Fury = 4627, // PariOfPlenty->PariOfPlenty, extra=0x16B
    ChainsOfPassionA = 4844, // none->player, extra=0x0
    ChainsOfPassionB = 4845, // none->player, extra=0x0
    BurningChains = 769, // none->player, extra=0x0
    Prey = 2939, // none->player, extra=0x0

    _Gen_ = 2056, // PariOfPlenty/_Gen_FalseFlame->PariOfPlenty/_Gen_FalseFlame/_Gen_IcyBauble/_Gen_FieryBauble, extra=0x3DA/0x3D9/0x448
}

public enum IconID : uint
{
    FalseFlameRight = 628, // _Gen_FalseFlame->self
    FalseFlameLeft = 629, // _Gen_FalseFlame->self
    FalseFlameRRight = 646, // _Gen_FalseFlame->self
    FalseFlameRLeft = 647, // _Gen_FalseFlame->self

    TankBuster = 342, // player->self

    TurnRight = 624, // Boss->self
    TurnLeft = 625, // Boss->self
    TurnRLeft = 645, // Boss->self
    TurnRRight = 644, // Boss->self

    ChainsAppearing = 97, // player->self
    HotFootSpread = 138, // player->self

    BlueRug = 631, // player->self

    StackIcon = 161, // player->self
}

public enum TetherID : uint
{
    CarpetRideTether = 355, // _Gen_->_Gen_
    FireChain = 9, // player->player
    TowerTether = 17, // _Gen_FalseFlame->player
}
