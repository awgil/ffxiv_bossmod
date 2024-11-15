namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

public enum OID : uint
{
    BossP1 = 0x4677, // R22.000, x1
    BossP2 = 0x4678, // R22.000, x0 (spawn during fight)
    QueenEternalHandP1 = 0x4679, // R0.500-1.000, x2, Part type
    QueenEternalHandP2 = 0x467A, // R1.000, x0 (spawn during fight), Part type
    CoronationHelper = 0x467B, // R0.500, x4
    Twister = 0x467C, // R1.550, x0 (spawn during fight)
    VirtualBoulder = 0x467D, // R2.040, x0 (spawn during fight)
    GravityOrb = 0x467E, // R1.000, x0 (spawn during fight)
    IcePillar1 = 0x467F, // R1.000, x0 (spawn during fight)
    IcePillar2 = 0x4680, // R1.500, x0 (spawn during fight)
    GravityRayHelper = 0x4709, // R2.500, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x23 (spawn during fight), Helper type
}

public enum AID : uint
{
    AutoAttackP1 = 40968, // QueenEternalHandP1->player, no cast, single-target
    Aethertithe = 40972, // BossP1->self, 3.0s cast, single-target, visual (three cones with 4-man line stacks)
    AethertitheRaidwide = 40973, // Helper->self, no cast, range 100 circle, multi-hit light aoe
    AethertitheAOER = 40974, // BossP1->self, no cast, range 100 70-degree cone (right side cleave)
    AethertitheAOEC = 40975, // BossP1->self, no cast, range 100 70-degree cone (center cleave)
    AethertitheAOEL = 40976, // BossP1->self, no cast, range 100 70-degree cone (left side cleave)
    Retribute = 40977, // BossP1->self, no cast, single-target, visual (two line stacks on healers)
    RetributeAOE = 40978, // Helper->self, no cast, range 60 width 8 rect
    LegitimateForceFirstR = 40990, // BossP1->self, 8.0s cast, range 60 width 30 rect (right->left cleave)
    LegitimateForceFirstL = 40992, // BossP1->self, 8.0s cast, range 60 width 30 rect (left->right cleave)
    LegitimateForceSecondR = 40993, // BossP1->self, no cast, range 60 width 30 rect
    LegitimateForceSecondL = 40994, // BossP1->self, no cast, range 60 width 30 rect
    ProsecutionOfWar = 40970, // BossP1->player, 5.0s cast, single-target, tankbuster with swap
    ProsecutionOfWarAOE = 40971, // BossP1->player, no cast, single-target, tankbuster second hit
    DivideAndConquer = 40983, // BossP1->self, 7.5s cast, single-target, visual (proteans)
    DivideAndConquerBait = 40984, // Helper->self, no cast, range 60 width 5 rect
    DivideAndConquerAOE = 30505, // Helper->self, 3.0s cast, range 60 width 5 rect
    RoyalDomain = 41038, // BossP1->location, 5.0s cast, range 100 circle, raidwide

    VirtualShiftWind = 40985, // BossP1->self, 5.0s cast, range 100 circle, raidwide + platform change
    LawsOfWind = 40995, // BossP1->self, 4.0s cast, single-target, visual (party stacks + tornados)
    WindOfChange = 40996, // Helper->player, no cast, single-target, ??? (east/west knockback)
    Aeroquell = 40997, // Helper->players, 5.0s cast, range 5 circle 4-man stack
    BitingChains = 40998, // Helper->player, no cast, single-target, damage + stacking vuln every second if chain is not broken after 5s
    AeroIV = 40999, // Twister->self, no cast, range 5 circle
    WorldShatterP1 = 40988, // BossP1->self, 5.0s cast, range 100 circle, raidwide + back to normal platform

    VirtualShiftEarth = 40986, // BossP1->self, 5.0s cast, range 100 circle, raidwide + platform change
    LawsOfEarth = 41000, // BossP1->self, 4.0s cast, single-target, visual (8 towers)
    LawsOfEarthBurst = 41001, // Helper->self, no cast, range 2 circle tower
    LawsOfEarthBigBurst = 41002, // Helper->self, no cast, range 30 circle failed tower
    GravitationalEmpire = 41003, // BossP1->self, 7.0s cast, single-target, visual (tether cones + defamations + 4 towers)
    GravityPillar = 41004, // Helper->player, 8.0s cast, range 10 circle, defamation
    GravityRay = 41005, // Helper->self, no cast, range 50 ?-degree cone
    MeteorImpact = 41006, // BossP1->self, no cast, single-target, visual (meteors start)
    MeteorImpactPlatform = 41007, // VirtualBoulder->self, no cast, range 4 circle
    MeteorImpactFall = 41008, // VirtualBoulder->player, no cast, single-target
    MeteorImpactBigBurst = 41009, // VirtualBoulder->self, no cast, range 60 circle, wipe if too many meteors hit platform
    WeightyBlow = 41011, // BossP1->self, 5.0s cast, single-target, visual (los behind boulders)
    WeightyBlowAOE = 41012, // GravityOrb->VirtualBoulder, no cast, width 3 rect charge
    WeightyBlowDestroy = 41010, // VirtualBoulder->self, no cast, single-target, visual (destroy boulder)

    Coronation = 40979, // BossP1->self, 3.0s cast, single-target, visual (double tethers)
    RuthlessRegalia = 40980, // CoronationHelper->self, no cast, range 100 width 12 rect
    AtomicRay = 40981, // BossP1->self, 3.0s cast, single-target, visual (spread)
    AtomicRayAOE = 40982, // Helper->players, 6.0s cast, range 16 circle

    AbsoluteAuthorityPuddles = 41025, // BossP1->self, 10.0s cast, single-target, visual (puddles + spread/stack + enumeration)
    AbsoluteAuthorityRaidwide = 41026, // BossP1->self, no cast, single-target, visual (raidwide start)
    AbsoluteAuthorityPuddlesAOE = 41027, // Helper->self, 4.0s cast, range 8 circle puddle
    AbsoluteAuthorityRaidwideAOE = 41028, // Helper->self, no cast, range 100 circle raidwide
    AbsoluteAuthorityKnockback = 41029, // Helper->self, 15.0s cast, range 40 width 40 rect knockback 30
    AbsoluteAuthorityExpansion = 41030, // Helper->players, no cast, range 100 circle proximity with ? falloff
    AbsoluteAuthorityBoot = 41031, // Helper->players, no cast, range 6 circle 4-man stack
    AbsoluteAuthorityHeel = 41032, // Helper->player, no cast, single-target, low damage if stacked
    AbsoluteAuthorityHeelFail = 41033, // Helper->player, no cast, single-target, heavy damage if not stacked

    VirtualShiftIce = 40987, // BossP1->self, 5.0s cast, range 100 circle, raidwide + platform change
    LawsOfIce = 41013, // BossP1->self, 4.0+1.0s cast, single-target, visual (freeze)
    LawsOfIceAOE = 41014, // Helper->self, 5.0s cast, range 100 circle, apply freezing up
    RushFirst = 41015, // IcePillar2->self, 12.0s cast, single-target, visual (tether & charge)
    RushSecond = 41016, // IcePillar1->self, 9.0s cast, single-target, visual (tether & charge)
    RushFirstAOE = 41017, // IcePillar2->self, no cast, range 80 width 4 rect, baited aoe if stretched
    RushSecondAOE = 41018, // IcePillar1->self, no cast, range 80 width 4 rect, baited aoe if stretched
    RushFirstFail = 41019, // IcePillar2->self, no cast, range 80 width 4 rect, knockback 20 if not stretched
    RushSecondFail = 41020, // IcePillar1->self, no cast, range 80 width 4 rect, knockback 20 if not stretched
    Shatter = 41021, // IcePillar1->self, no cast, range 8 circle, explosion if pillar is not tethered to anyone - if someone is dead
    DrearRising = 41022, // BossP1->self, no cast, single-target, visual (tether baits + line stack)
    IceDart = 41023, // Helper->players, no cast, range 16 circle spread
    RaisedTribute = 41024, // Helper->self, no cast, range 80 width 8 rect, line stack

    AuthorityEternal = 41034, // BossP1->self, 10.0s cast, single-target, visual (phase change)
    AuthorityEternalAOE = 41035, // BossP1->self, no cast, range 100 circle, raidwide
    IntermissionEnd = 41037, // BossP2->self, no cast, single-target, visual (intermission end)

    AutoAttackP2 = 40969, // QueenEternalHandP2->player, no cast, single-target
    RadicalShift = 41039, // BossP2->self, 11.0s cast, range 100 circle, raidwide + platform change
    RadicalShiftAOE = 41040, // Helper->player, 10.0s cast, range 5 circle spread
    WorldShatterP2 = 30354, // BossP2->self, 5.0s cast, range 100 circle, raidwide + back to normal platform
    DimensionalDistortion = 41042, // BossP2->self, 4.0+1.0s cast, single-target, visual (exaflares)
    DimensionalDistortionFirst = 41043, // Helper->self, 5.0s cast, range 6 circle
    DimensionalDistortionRest = 41044, // Helper->self, no cast, range 6 circle
    TyrannysGrasp = 41045, // BossP2->location, 5.0s cast, visual (half-arena cleave + tankbuster towers)
    TyrannysGraspAOE = 41046, // Helper->self, 5.0s cast, range 20 width 40 rect
    TyrannysGraspTower1 = 41047, // Helper->self, 6.2s cast, range 4 circle tankbuster tower
    TyrannysGraspTower2 = 30418, // Helper->self, 8.9s cast, range 4 circle tankbuster tower
    DyingMemory = 41049, // BossP2->self, no cast, range 40 circle raidwide
    DyingMemoryLast = 20042, // BossP2->self, no cast, range 40 circle raidwide
    RoyalBanishment = 41050, // BossP2->location, 5.0s cast, visual
    RoyalBanishmentAOE = 41051, // Helper->self, no cast, range 60 width 10 rect, line stack
    RoyalBanishmentLast = 41052, // Helper->self, no cast, range 60 width 10 rect, line stack
}

public enum SID : uint
{
    EastWindOfChange = 4189, // none->player, extra=0x0
    WestWindOfChange = 4190, // none->player, extra=0x0
    MissingLink = 3587, // none->player, extra=0x0
    GravitationalAnomaly = 3770, // none->player, extra=0x15E
    AuthoritysExpansion = 4186, // none->player, extra=0x0, flare
    AuthoritysBoot = 4187, // none->player, extra=0x0, stack
    AuthoritysHeel = 4188, // none->player, extra=0x0, enumeration
    FreezingUp = 2540, // Helper->player, extra=0x0
    DeepFreeze = 3519, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Aeroquell = 93, // player->self
    MissingLink = 326, // player->self
    ProsecutionOfWar = 218, // player->self
    DivideAndConquer = 521, // BossP1->player
    GravityRay = 219, // player->self
    GravityPillar = 372, // player->self
    MeteorImpact = 559, // player->self
    Coronation = 1, // player->self
    AtomicRay = 555, // player->self
    AuthoritysExpansion = 87, // player->self
    AuthoritysBoot = 161, // player->self
    AuthoritysHeel = 55, // player->self
    LawsOfIce = 554, // player->self
    RaisedTribute = 524, // Helper->player
    RadicalShift = 556, // player->self
    RoyalBanishmentFirst = 572, // Helper->player
    RoyalBanishmentLast = 527, // Helper->player
}

public enum TetherID : uint
{
    MissingLink = 163, // player->player
    GravityRay = 17, // player->GravityRayHelper
    CoronationR = 270, // player->CoronationHelper
    CoronationL = 271, // player->CoronationHelper
    RushShort = 57, // IcePillar2/IcePillar1->player
    RushLong = 1, // IcePillar2/IcePillar1->player
    IceDart = 89, // player->BossP1
}
