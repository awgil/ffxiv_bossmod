namespace BossMod.Endwalker.Alliance.A10Lions;

public enum OID : uint
{
    Lion = 0x38DA, // R5.600, x1
    Lioness = 0x38DB, // R5.600, x1
    Lions = 0x38DC, // R0.500, x1
    Helper = 0x233C, // R0.500, x2
};

public enum AID : uint
{
    AutoAttack = 870, // Lion/Lioness->player, no cast, single-target
    Teleport = 29133, // Lioness/Lion->location, no cast, single-target
    RoaringBlazeFirst = 29134, // Lion/Lioness->self, 6.0s cast, range 50 180-degree cone
    RoaringBlazeSecond = 29135, // Lioness/Lion->self, 9.0s cast, range 50 180-degree cone
    SlashAndBurnOutFirst = 29136, // Lion->self, 7.0s cast, range 14 circle
    SlashAndBurnOutSecondVisual = 29137, // Lion->self, no cast, single-target
    SlashAndBurnInFirst = 29138, // Lioness->self, 7.0s cast, range 6-30 donut
    SlashAndBurnInSecondVisual = 29139, // Lioness->self, no cast, single-target
    SlashAndBurnSecondVisual = 29140, // Lioness/Lion->self, 7.0s cast, single-target
    SlashAndBurnOutSecond = 29141, // Helper->self, 10.2s cast, range 14 circle
    SlashAndBurnInSecond = 29142, // Helper->self, 10.2s cast, range 6-30 donut
    DoubleImmolation = 29143, // Lioness/Lion->self, 5.0s cast, single-target, visual (raidwide)
    DoubleImmolationAOE = 29144, // Lions->self, no cast, range 25 circle, raidwide
    RoaringBlazeSolo = 29375, // Lioness/Lion->self, 4.0s cast, range 50 180-degree cone
    TrialByFire = 29376, // Lion->self, 4.0s cast, range 14 circle
    SpinningSlash = 29377, // Lioness->self, 4.0s cast, range 6-30 donut
};

public enum IconID : uint
{
    Order1 = 332, // Lion/Lioness
    Order2 = 333, // Lion/Lioness
};
