namespace BossMod.Dawntrail.Trial.T03QueenEternal;

public enum OID : uint
{

    Boss = 0x41C0, // R22.000, x1
    Helper = 0x233C, // R0.500, x35, 523 type
    WukLamat = 0x41C5, // R0.500, x0 (spawn during fight)
    Unknown = 0x41C6, // R1.500, x0 (spawn during fight)
    QueenEternal1 = 0x41C1, // R0.500, x2
    QueenEternal2 = 0x41C2, // R0.500, x1
    QueenEternal3 = 0x41C4, // R1.000, x0 (spawn during fight)
    QueenEternal4 = 0x4477, // R1.000, x1, Part type
}

public enum AID : uint
{
    AutoAttack = 36656, // QueenEternal4->player, no cast, single-target

    LegitimateForce1 = 36638, // Boss->self, 8.0s cast, range 60 width 30 rect
    LegitimateForce2 = 36639, // Boss->self, 8.0s cast, range 60 width 30 rect
    LegitimateForce3 = 36640, // Boss->self, 8.0s cast, range 60 width 30 rect
    LegitimateForce4 = 36641, // Boss->self, 8.0s cast, range 60 width 30 rect
    LegitimateForce5 = 36642, // Boss->self, no cast, range 60 width 30 rect // Immediate follow up after a cast of LF1-4
    LegitimateForce6 = 36643, // Boss->self, no cast, range 60 width 30 rect

    Aethertithe1 = 36604, // Boss->self, 3.0s cast, single-target
    Aethertithe2 = 36605, // Helper->self, no cast, range 100 circle
    Aethertithe3 = 36657, // Boss->self, no cast, range 100 ?-degree cone // need to be tied to animation/env, only 3 different versions
    Aethertithe4 = 36658, // Boss->self, no cast, range 100 ?-degree cone
    Aethertithe5 = 36659, // Boss->self, no cast, range 100 ?-degree cone

    Coronation = 36629, // Boss->self, 3.0s cast, single-target

    WaltzOfTheRegalia1 = 36631, // QueenEternal1->self, no cast, single-target
    WaltzOfTheRegalia2 = 36632, // Helper->self, 1.0s cast, range 14 width 4 rect

    ProsecutionOfWar = 36602, // Boss->player, 5.0s cast, single-target // Tankbuster

    LockAndKey = 36630, // Boss->self, no cast, single-target

    VirtualShift1 = 36606, // Boss->self, 5.0s cast, range 100 circle
    VirtualShift2 = 36607, // Boss->self, 5.0s cast, range 100 circle
    VirtualShift3 = 36608, // Boss->self, 5.0s cast, range 100 circle

    RuthlessRegalia = 36634, // QueenEternal2->self, no cast, range 100 width 12 rect

    Downburst1 = 36609, // Boss->self, 6.0s cast, single-target
    Downburst2 = 36610, // Helper->self, 6.0s cast, range 100 circle

    BrutalCrown = 36633, // QueenEternal1->self, 8.0s cast, range ?-60 donut

    PowerfulGust1 = 36611, // Boss->self, 6.0s cast, single-target
    PowerfulGust2 = 36612, // Helper->self, 6.0s cast, range 60 width 60 rect

    RoyalDomain = 36603, // Boss->location, 5.0s cast, range 100 circle
    Castellation = 36613, // Boss->self, 3.0s cast, single-target

    Besiegement1 = 36614, // Helper->self, no cast, range 60 width 4 rect
    Besiegement2 = 36615, // Helper->self, no cast, range 60 width 8 rect
    Besiegement3 = 36616, // Helper->self, no cast, range 60 width 10 rect
    Besiegement4 = 36617, // Helper->self, no cast, range 60 width 12 rect
    Besiegement5 = 36618, // Helper->self, no cast, range 60 width 18 rect

    AbsoluteAuthority1 = 36619, // Boss->self, no cast, single-target
    AbsoluteAuthority2 = 36620, // Helper->self, no cast, range 100 circle
    AbsoluteAuthority3 = 36621, // Helper->self, no cast, range 100 circle
    AbsoluteAuthority4 = 36622, // Helper->self, 3.8s cast, range 8 circle
    AbsoluteAuthority5 = 36624, // Helper->players, no cast, range 100 circle
    AbsoluteAuthority6 = 36627, // Helper->self, no cast, range 100 circle
    AbsoluteAuthority7 = 36628, // Helper->self, no cast, range 100 circle
    AbsoluteAuthority8 = 39518, // Helper->players, no cast, range 100 circle
    AbsoluteAuthority9 = 39519, // Helper->player, no cast, single-target
    AbsoluteAuthority10 = 39520, // Helper->player, no cast, single-target
    AbsoluteAuthority11 = 39531, // Boss->self, 19.0s cast, range 100 circle

    DivideAndConquer1 = 36636, // Boss->self, 5.0s cast, single-target // baited line AOEs from the boss
    DivideAndConquer2 = 36637, // Helper->self, no cast, range 100 width 5 rect

    MorningStars = 39134, // Helper->player, no cast, single-target
    FangsOfTheBraax = 36653, // WukLamat->Boss, no cast, single-target

    DynasticDiadem1 = 40058, // Boss->self, 5.0s cast, single-target
    DynasticDiadem2 = 40059, // Helper->self, 5.0s cast, range ?-70 donut

    TrialsOfTural = 36652, // WukLamat->Boss, no cast, single-target
    TailOfTheBraax = 36654, // WukLamat->Boss, no cast, single-target
    RoyalBanishment = 36644, // Boss->self, 3.0s cast, single-target
    JawOfTheBraax = 36655, // WukLamat->Boss, no cast, single-target

    RoyalBanishment1 = 36645, // Helper->self, no cast, range 100 circle
    RoyalBanishment2 = 36647, // Helper->self, 3.5s cast, range 100 30.000-degree cone

    DawnsResolve = 36651, // WukLamat->Boss, 9.5s cast, range 60 circle
}

public enum SID : uint
{
    AuthoritysGaze = 3815, // none->player, extra=0x0
    AuthoritysHold = 4130, // none->player, extra=0x0
    DawnsResolve = 4129, // WukLamat->player, extra=0x0
    DownForTheCount = 1963, // Helper->player, extra=0xEC7
    GravitationalAnomaly = 3814, // none->player, extra=0x15E
    Hover = 2390, // none->WukLamat, extra=0x64
    Petrification = 1511, // Helper->player, extra=0x0
    Preoccupied = 1619, // none->player, extra=0x0
    UnknownStatus1 = 2056, // none->Boss/QueenEternal2/WukLamat, extra=0x320/0x2E7/0x310
    UnknownStatus2 = 3572, // Boss->Unknown, extra=0xC
    UnknownStatus3 = 3941, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Boss/QueenEternal1/Helper->player, extra=0x1/0x2
}

public enum IconID : uint
{
    Icon1 = 1, // player
    DoritoStack = 55, // player
    Icon75 = 75, // player // shared in A24 Ozma and A35UltimaP2, likely spread or gaze
    Icon179 = 179, // player
    Tankbuster = 218, // player
    Icon327 = 327, // player // shared with EX3 EchoesFuture
    Icon521 = 521, // Boss
}

public enum TetherID : uint
{
    Tether270 = 270, // player->QueenEternal2
    Tether271 = 271, // player->QueenEternal2
}
