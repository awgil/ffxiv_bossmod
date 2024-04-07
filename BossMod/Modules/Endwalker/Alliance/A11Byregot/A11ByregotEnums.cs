namespace BossMod.Endwalker.Alliance.A11Byregot;

public enum OID : uint
{
    Boss = 0x390B, // R6.000, x1
    Avatar = 0x390C, // R6.000, x5
    Helper = 0x233C, // R0.500, x14
}

public enum AID : uint
{
    AutoAttackPhys = 6499, // Boss->player, no cast, single-target
    AutoAttackMagic = 29364, // Boss->player, no cast, single-target
    Teleport = 29033, // Boss->location, no cast, single-target

    OrdealOfThunder = 29046, // Boss->self, 5.0s cast, range 80 circle, raidwide
    ByregotWard = 29045, // Boss->players, 5.0s cast, range 10 90-degree cone, tankbuster

    ByregotStrikeJump = 29274, // Boss->location, 6.0s cast, range 8 circle
    ByregotStrikeKnockback = 29029, // Helper->self, 7.0s cast, range 45 circle knockback 18
    BuilderBuild = 29433, // Boss->self, 3.0s cast, single-target, visual
    ByregotStrikeJumpCone = 29031, // Boss->location, 6.0s cast, range 8 circle
    ByregotStrikeCone = 29032, // Helper->self, no cast, range 90 45-degree cone

    BuilderForge = 29034, // Boss->self, no cast, single-target, visual
    DestroySideTiles = 29044, // Helper->self, 9.0s cast, range 50 width 10 rect (cast at e.g. [+-20, 675])
    PealOfTheHammer1 = 29039, // Boss->self, no cast, single-target, triggers side hammers (envcontrol 7-11)
    PealOfTheHammer2 = 29041, // Boss->self, no cast, single-target, triggers levinforge hammer (envcontrol 21)
    Levinforge = 29042, // Helper->self, 18.5s cast, range 50 width 10 rect aoe, caster starts at 'adjusted' location by hammer's hit (e.g. [0, 675])
    ByregotSpire = 29040, // Boss->self, 11.0s cast, range 50 width 30 rect aoe

    Reproduce = 29035, // Boss->self, 3.0s cast, single-target, visual
    BuilderBuildAvatar = 29434, // Avatar->self, no cast, single-target, glow effect
    CloudToGround = 29036, // Avatar->self, 6.0s cast, single-target, visual
    CloudToGroundFast = 29037, // Helper->self, 7.0s cast, range 7 circle
    CloudToGroundSlow = 28749, // Helper->self, 7.0s cast, range 7 circle
    CloudToGroundFastAOE = 29038, // Helper->self, no cast, range 7 circle
    CloudToGroundSlowAOE = 28750, // Helper->self, no cast, range 7 circle
}

public enum SID : uint
{
    None = 0,
    Glow = 2056, // 0x017F extra, used by boss and glowing avatars
}
