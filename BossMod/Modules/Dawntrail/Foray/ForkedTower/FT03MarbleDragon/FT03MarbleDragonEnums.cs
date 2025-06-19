namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

public enum OID : uint
{
    Boss = 0x3974,
    Helper = 0x233C,
    DeathWall = 0x46D5, // R0.500, x1
    Icewind = 0x3976, // R1.000, x0 (spawn during fight)
    IceGolem = 0x398A, // R2.850, x0 (spawn during fight)
    IceSprite = 0x39C2, // R1.040, x0 (spawn during fight)
    GelidGaol = 0x39D9, // R1.000, x0 (spawn during fight)
    IcePuddle = 0x1EBD52,
    CrossPuddle = 0x1EBD53,
    IceTower = 0x1EBD54,
}

public enum AID : uint
{
    AutoAttack = 30059, // Boss->player, no cast, single-target
    ImitationStarCast = 30705, // Boss->self, 5.0+1.6s cast, single-target
    ImitationStar = 40652, // Helper->self, no cast, ???
    DeathWall = 30786, // DeathWall->self, no cast, range ?-40 donut
    DraconiformMotionCast = 30657, // Boss->self, 4.0+0.8s cast, single-target
    DraconiformMotion1 = 30694, // Helper->self, 4.8s cast, range 60 90-degree cone
    DraconiformMotion2 = 30693, // Helper->self, 4.8s cast, range 60 90-degree cone
    ImitationRainInstant1 = 30343, // Helper->self, no cast, single-target
    ImitationRainInstant2 = 30615, // Helper->self, no cast, ???
    Jump = 30060, // Boss->location, no cast, single-target
    ImitationIcicle = 30063, // Boss->self, 3.0s cast, single-target
    ImitationIcicleAOE = 30180, // Helper->self, 7.0s cast, range 8 circle
    BallOfIceLarge = 42773, // Helper->self, 0.5s cast, range 8 circle
    BallOfIceSmall = 42774, // Helper->self, 0.5s cast, range 4 circle
    ImitationBlizzardCircle = 30210, // Helper->self, 1.0s cast, range 20 circle
    ImitationBlizzardCross = 30228, // Helper->self, 1.0s cast, range 60 width 16 cross
    DreadDelugeCast = 30696, // Boss->self, 3.0+2.0s cast, single-target
    DreadDeluge = 30704, // Helper->player, 5.0s cast, single-target
    FrigidTwisterCast = 30264, // Boss->self, 4.0s cast, single-target
    FrigidTwister = 30415, // Helper->location, no cast, range 5 circle
    WitheringEternity = 30419, // Boss->self, 5.0s cast, single-target
    AutoAttackGolem = 39462, // IceGolem->player, no cast, single-target
    Unk1 = 30613, // Boss->self, no cast, single-target
    FrigidDiveCast = 30614, // Boss->self, 7.2+0.8s cast, single-target
    FrigidDive = 37819, // Helper->self, 8.0s cast, range 60 width 20 rect
    ImitationBlizzardTower = 30229, // Helper->self, 4.0s cast, range 4 circle
    ImitationBlizzardTowerVisual = 30230, // Helper->self, no cast, single-target
    ImitationBlizzardTowerHit = 30417, // Helper->self, no cast, ???
    FrozenHeart = 37823, // IceGolem->self, 3.0s cast, single-target
    Unk2 = 30416, // Boss->self, no cast, single-target
    LifelessLegacyCast = 30616, // Boss->self, 35.0+1.6s cast, single-target
    LifelessLegacy = 37818, // Helper->self, no cast, ???
    WickedWater = 30695, // Boss->self, 4.0s cast, single-target
    LifelessLegacyEnrage = 30061, // Boss->self, 20.0+1.6s cast, single-target
}

public enum SID : uint
{
    Invincibility = 4410, // none->Boss, extra=0x0
    WickedWater = 4334, // none->player, extra=0x19
    Throttle = 938, // none->player, extra=0x0
    Stun = 2970, // Helper->player, extra=0x12
    GelidGaol = 4335, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
    LockOn = 23, // player->self
}
