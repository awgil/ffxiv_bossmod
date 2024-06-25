namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

public enum OID : uint
{
    Boss = 0x31C4, // R=0.5
    MagitekTurret = 0x31C5, // R=0.6
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 23903, // Boss->location, no cast, single-target
    CallousCrossfire = 23901, // Boss->self, 4.0s cast, single-target
    Aethershot = 23902, // MagitekTurret->player, no cast, range 4 circle
    MagitekMinefield = 23887, // Boss->self, 3.0s cast, single-target
    ActivateBlueMine = 23888, // Helper->self, no cast, single-target
    ActivateRedMine = 23889, // Helper->self, no cast, single-target
    DetonateBlueMine = 23890, // Helper->self, no cast, range 8 circle
    Explosion = 23873,
    GigaTempest = 23875, // Boss->self, 5.0s cast, range 60 circle
    GigaTempestLargeStart = 23876, // Helper->self, 5.0s cast, range 35 width 13 rect
    GigaTempestSmallStart = 23877, // Helper->self, 5.0s cast, range 10 width 13 rect
    GigaTempestSmallMove = 23879, // Helper->self, no cast, range 10 width 13 rect
    GigaTempestLargeMove = 23878, // Helper->self, no cast, range 35 width 13 rect
    DarkShot = 23884, // Boss->self, 4.0s cast, single-target
    GunberdDark = 23886, // Boss->player, 4.0s cast, single-target
    MagitekImpetus = 23899, // Boss->self, 3.0s cast, single-target
    WindslicerShot = 23883, // Boss->self, 4.0s cast, single-target
    GunberdWindslicer = 23885, // Boss->player, 4.0s cast, single-target
    Ruination = 23880, // Boss->self, 4.0s cast, range 40 width 8 cross
    RuinationExaStart = 23881, // Helper->self, 7.0s cast, range 4 circle
    RuinationExaMove = 23882, // Helper->self, no cast, range 4 circle
    SpiralScourge = 23900, // Boss->player, 6.0s cast, single-target
    TeraTempest = 23904,
    DetonateRedMine = 23873,
    IndiscriminateDetonation = 23892,
    ProactiveMunition = 23896,
    ProactiveMunitionTrackingStart = 23897,
    ProactiveMunitionTrackingMove = 23898,
    ReactiveMunition = 23894,
    SenseWeakness = 23893,
}

public enum SID : uint
{
    NastySurprise = 2549, // Boss->Boss, extra=0x0, special ammo is loaded
    ForwardMarch = 1293, // none->player, extra=0x0
    AboutFace = 1294, // none->player, extra=0x0
    LeftFace = 1295, // none->player, extra=0x0
    RightFace = 1296, // none->player, extra=0x0
    ForcedMarch = 1257, // none->player, extra=0x4/0x1/0x2/0x8
    AccelerationBomb = 1072,
}
