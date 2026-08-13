namespace BossMod.Modules.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

public enum OID : uint
{
    PariOfPlenty = 0x4A6D,
    Helper = 0x233C,
    FlyingCarpet = 0x4A74, // R4.356, x8
    FieryBauble = 0x4A6F, // R6.000, x8
    FalseFlame = 0x4A6E, // R4.356, x4
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    TheMerchantsTaleAbridged = 0x1EBFAB, // R0.500, x1, EventObj type
    Actor1ebf49 = 0x1EBF49, // R0.500, x1, EventObj type
    AetherialFlow = 0x1EBFA5, // R2.000, x1, EventObj type
    CarpetTetherTarget = 0x4A75, // R1.000, x6
    SparkPuddle = 0x1EBF20, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 45544, // PariOfPlenty->player, no cast, single-target
    HeatBurst = 45516, // PariOfPlenty->self, 5.0s cast, range 60 circle
    Ability = 45421, // PariOfPlenty->location, no cast, single-target
    RightFireflight = 45426, // PariOfPlenty->self, 10.0s cast, range 40 width 4 rect
    CarpetRide = 45430, // PariOfPlenty/4A6E->location, no cast, single-target
    CarpetRide1 = 45432, // Helper->location, 2.0s cast, ???
    SunCirclet = 45447, // PariOfPlenty->self, 2.0s cast, range ?-60 donut
    Doubling = 45203, // PariOfPlenty->self, 3.0s cast, single-target
    CharmdChains = 45199, // PariOfPlenty->self, 4.0s cast, single-target
    LeftFableflight = 45429, // 4A6E->self, 10.5s cast, range 40 width 4 rect
    CarpetRide2 = 45431, // 4A6E/PariOfPlenty->location, no cast, single-target
    CarpetRide3 = 45433, // Helper->location, 2.0s cast, ??? -- Seems to be the actual 'cast' for the AOEs
    BurningGleam = 45499, // 4A6F->self, 7.0s cast, range 40 width 10 cross
    RightFableflight = 45428, // 4A6E->self, 10.5s cast, range 40 width 4 rect
    FireOfVictory = 45518, // PariOfPlenty->player, 5.0s cast, range 4 circle
    RightFireflightFourLongNights = 45461, // PariOfPlenty->self, 17.0s cast, range 40 width 4 rect
    WheelOfFireflight = 45463, // PariOfPlenty->self, no cast, range 40 ?-degree cone
    FellSpark = 45475, // Helper->player, no cast, single-target
    LeftFireflight = 45427, // PariOfPlenty->self, 10.0s cast, range 40 width 4 rect
    LeftFireflightFourLongNights = 45462, // PariOfPlenty->self, 17.0s cast, range 40 width 4 rect
    WheelOfFireflight1 = 45466, // PariOfPlenty->self, no cast, range 40 ?-degree cone
    WheelOfFireflight2 = 45465, // PariOfPlenty->self, no cast, range 40 ?-degree cone
    RightFireflightFactAndFiction = 47025, // PariOfPlenty->self, 10.0s cast, range 40 width 4 rect
    CarpetRide4 = 45425, // 4A74->location, no cast, single-target
    LeftFireflightFactAndFiction = 47026, // PariOfPlenty->self, 10.0s cast, range 40 width 4 rect
    ParisCurse = 45520, // PariOfPlenty->self, 5.0s cast, range 60 circle
    CharmingBaubles = 45496, // PariOfPlenty->self, 3.0s cast, single-target
    BurningGleam1 = 47397, // 4A6F->self, 8.0s cast, range 40 width 10 cross
    FirePowder = 45521, // Helper->self, no cast, range 15 circle
    HighFirePowder = 45522, // Helper->location, no cast, range 15 circle
    CharmdFableflight = 47024, // PariOfPlenty->self, 4.0s cast, single-target
    LeftFableflight1 = 46947, // 4A6E->self, 9.6s cast, range 40 width 4 rect
    CarpetRide5 = 46949, // 4A6E->location, no cast, single-target
    CarpetRide6 = 46951, // Helper->location, 2.0s cast, ???
    BurningGleam2 = 45043, // 4A6F->self, 14.0s cast, range 40 width 10 cross
    CarpetRide7 = 47021, // Helper->location, 3.7s cast, ???
    SpurningFlames = 45481, // PariOfPlenty->self, 7.0s cast, range 40 circle
    ImpassionedSparks = 45483, // PariOfPlenty->self, 5.0s cast, single-target
    ImpassionedSparks1 = 45484, // PariOfPlenty->self, no cast, single-target
    ImpassionedSparks2 = 45485, // Helper->self, 2.0s cast, single-target
    ImpassionedSparks3 = 45487, // Helper->self, 6.0s cast, range 8 circle
    BurningPillar = 45526, // Helper->self, 4.0s cast, range 10 circle
    FireWell = 45528, // Helper->players, no cast, range 6 circle
    ScouringScorn = 45490, // PariOfPlenty->self, 6.0s cast, range 40 circle
    BurningChains = 45530, // Helper->player, no cast, single-target
    WheelOfFireflight3 = 45464, // PariOfPlenty->self, no cast, range 40 ?-degree cone
    RightFableflight1 = 46946, // 4A6E->self, 9.6s cast, range 40 width 4 rect
    CarpetRide8 = 46948, // 4A6E->location, no cast, single-target
    CarpetRide9 = 46950, // Helper->location, 2.0s cast, ???
    CarpetRide10 = 47020, // Helper->location, 3.7s cast, ???
}

public enum SID : uint
{
    Unknown = 2056, // PariOfPlenty/4A6E->PariOfPlenty/4A6F/4A6E, extra=0x3D9/0x448/0x3DA
    VulnerabilityUp = 1789, // Helper/PariOfPlenty/4A6F->player, extra=0x1/0x2/0x3/0x4
    BurningChains = 769, // none->player, extra=0x0
    DarkResistanceDown = 3619, // Helper->player, extra=0x0
    CurseOfCompanionship = 4616, // none->player, extra=0x0
    CurseOfSolitude = 4615, // none->player, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    Fury = 4627, // PariOfPlenty->PariOfPlenty, extra=0x16B

}

public enum TetherID : uint
{
    Fireflight = 355, // 4A75->4A75
    CharmedChain = 9, // player->player
    FellSpark = 84, // PariOfPlenty->player
}

public enum IconID : uint
{
    _Gen_Icon_m0376trg_fire3_a0p = 97, // player->self
    _Gen_Icon_tank_lockonae_4m_5s_01t = 342, // player->self
    TurningRight = 624, // PariOfPlenty->self
    TurningLeft = 625, // PariOfPlenty->self
    TurningRRight = 644, // PariOfPlenty->self
    TurningRLeft = 645, // PariOfPlenty->self
    Stack = 318, // player->self
}
