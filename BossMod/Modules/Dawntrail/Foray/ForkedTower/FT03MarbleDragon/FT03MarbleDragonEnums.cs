#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

public enum OID : uint
{
    Boss = 0x3974,
    Helper = 0x233C,
    _Gen_ = 0x46D5, // R0.500, x1
    _Gen_Icewind = 0x3976, // R1.000, x0 (spawn during fight)
    _Gen_IceGolem = 0x398A, // R2.850, x0 (spawn during fight)

    IcePuddle = 0x1EBD52,
    CrossPuddle = 0x1EBD53
}

public enum AID : uint
{
    _AutoAttack_ = 30059, // Boss->player, no cast, single-target
    _Weaponskill_ImitationStar = 30705, // Boss->self, 5.0+1.6s cast, single-target
    _Weaponskill_ImitationStar1 = 40652, // Helper->self, no cast, ???
    _Weaponskill_ = 30786, // 46D5->self, no cast, range ?-40 donut
    _Weaponskill_DraconiformMotion = 30657, // Boss->self, 4.0+0.8s cast, single-target
    _Weaponskill_DraconiformMotion1 = 30694, // Helper->self, 4.8s cast, range 60 90-degree cone
    _Weaponskill_DraconiformMotion2 = 30693, // Helper->self, 4.8s cast, range 60 90-degree cone
    _Ability_ImitationRain = 30343, // Helper->self, no cast, single-target
    _Ability_ImitationRain1 = 30615, // Helper->self, no cast, ???
    _Weaponskill_1 = 30060, // Boss->location, no cast, single-target
    _Weaponskill_ImitationIcicle = 30063, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ImitationIcicle1 = 30180, // Helper->self, 7.0s cast, range 8 circle
    _Ability_BallOfIce = 42773, // Helper->self, 0.5s cast, range 8 circle
    _Ability_ImitationBlizzard = 30210, // Helper->self, 1.0s cast, range 20 circle
    _Ability_ImitationBlizzard1 = 30228, // Helper->self, 1.0s cast, range 60 width 16 cross
    _Weaponskill_DreadDeluge = 30696, // Boss->self, 3.0+2.0s cast, single-target
    _Weaponskill_DreadDeluge1 = 30704, // Helper->player, 5.0s cast, single-target
    _Weaponskill_FrigidTwister = 30264, // Boss->self, 4.0s cast, single-target
    _Weaponskill_FrigidTwister1 = 30415, // Helper->location, no cast, range 5 circle
    _Ability_WitheringEternity = 30419, // Boss->self, 5.0s cast, single-target
    _AutoAttack_1 = 39462, // 398A->player, no cast, single-target
    _Ability_ = 30613, // Boss->self, no cast, single-target
    _Weaponskill_FrigidDive = 30614, // Boss->self, 7.2+0.8s cast, single-target
    _Weaponskill_FrigidDive1 = 37819, // Helper->self, 8.0s cast, range 60 width 20 rect
    _Ability_BallOfIce1 = 42774, // Helper->self, 0.5s cast, range 4 circle
    _Ability_ImitationBlizzard2 = 30229, // Helper->self, 4.0s cast, range 4 circle
    _Ability_ImitationBlizzard3 = 30230, // Helper->self, no cast, single-target
    _Ability_ImitationBlizzard4 = 30417, // Helper->self, no cast, ???
}
