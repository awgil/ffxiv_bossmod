namespace BossMod.Endwalker.Alliance.A11Byregot
{
    public enum OID : uint
    {
        Boss = 0x390B,
        Avatar = 0x390C, // x5
        Helper = 0x233C, // x14
    };

    public enum AID : uint
    {
        AutoAttackPhys = 6499,
        AutoAttackMagic = 29364,
        Teleport = 29033, // Boss->none, no cast, no effect

        OrdealOfThunder = 29046, // Boss->self, 5s cast, raidwide
        ByregotWard = 29045, // Boss->MT, 5s cast, tankbuster

        ByregotStrike = 29274, // Boss->location, 6s cast, range 8 aoe
        ByregotStrikeKnockback = 29029, // Helper->self, 7s cast, knockback 18

        BuilderBuild = 29433, // Boss->self, 3s cast
        ByregotStrikeWithCone = 29031, // Boss->location, 6s cast, range 8 aoe
        ByregotStrikeCone = 29032, // Helper->self, no cast, range 90 30-degree cone aoe

        BuilderForge = 29034, // Boss->self, no cast, ???
        PealOfTheHammer1 = 29039, // Boss->self, no cast, triggers side hammers (envcontrol 7-11)
        PealOfTheHammer2 = 29041, // Boss->self, no cast, triggers levinforge hammer (envcontrol 21)
        DestroySideTiles = 29044, // Helper->none, 9s cast, 5 half-width rect (cast at e.g. [+-20, 675])
        Levinforge = 29042, // Helper->self, 18.5s cast, 5 half-width rect aoe, caster starts at 'adjusted' location by hammer's hit (e.g. [0, 675])
        ByregotSpire = 29040, // Boss->self, 11s cast, 15 half-width rect aoe

        Reproduce = 29035, // Boss->self, 3s cast
        BuilderBuildAvatar = 29434, // Avatar->self, no cast, glow effect
        CloudToGround = 29036, // Avatar->self, 6s cast, vfx
        CloudToGroundFast = 29037, // Helper->self, 7s cast, range 7 aoe
        CloudToGroundSlow = 28749, // Helper->self, 7s cast, range 7 aoe
        CloudToGroundFastAOE = 29038, // Helper->self, no cast, range 7 aoe
        CloudToGroundSlowAOE = 28750, // Helper->self, no cast, range 7 aoe
    };

    public enum SID : uint
    {
        None = 0,
        Glow = 2056, // 0x017F extra, used by boss and glowing avatars
    }

    public enum TetherID : uint
    {
        None = 0,
    }

    public enum IconID : uint
    {
        None = 0,
    }
}
