namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN1TrinitySeeker;

public enum OID : uint
{
    Boss = 0x3131, // R4.000, x1
    Helper = 0x233C, // R0.500, x16, 523 type
    AetherialOrb = 0x3132, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack1 = 6497, // Boss->player, no cast, single-target
    AutoAttack2 = 6499, // Boss->player, no cast, single-target
    Teleport = 23194, // Boss/SeekerAvatar->location, no cast, single-target, teleport

    VerdantTempest = 23222, // Boss->self, 5.0s cast, range 50 circle

    VerdantPathKatana = 23191, // Boss->self, 3.0s cast, single-target, visual (switch to katana, cross aoe)
    ActOfMercy = 23218, // Boss->self, 3.0s cast, range 50 width 8 cross
    FirstMercy = 23389, // Boss->self, 3.0s cast, single-target
    SecondMercy = 23390, // Boss->self, 3.0s cast, single-target
    ThirdMercy = 23391, // Boss->self, 3.0s cast, single-target
    FourthMercy = 23392, // Boss->self, 3.0s cast, single-target
    MercyFourfold = 23195, // Boss->self, 2.0s cast, single-target
    MercyFourfoldAOE = 23348, // Helper->self, 1.7s cast, range 50 180-degree cone
    MercyFourfold1 = 23196, // Boss->self, no cast, single-target
    MercyFourfold2 = 23197, // Boss->self, no cast, single-target
    MercyFourfold3 = 23198, // Boss->self, no cast, single-target
    MercyFourfold4 = 23199, // Boss->self, no cast, single-target
    SeasonsOfMercy = 23210, // SeekerAvatar/Boss->self, 5.0s cast, single-target, visual (crisscross + gaze + big aoe)
    MercifulBreeze = 23211, // Helper->self, 2.5s cast, range 50 width 5 rect aoe (crisscross)
    MercifulMoon = 23212, // AetherialOrb->self, no cast, range 50 circle gaze
    MercifulBlooms = 23213, // Helper->self, 9.0s cast, range 4 circle aoe (real radius is 20 due to influence up)
    MercifulArc = 23223, // Boss->self, no cast, range 12 ?-degree cone cleave

    VerdantPathSword = 23192, // Boss->self, 3.0s cast, single-target, visual (switch to sword, side aoes)
    BalefulSwathe = 23219, // Boss->self, no cast, single-target, visual (side aoes)
    BalefulSwatheAOE = 23220, // Helper->self, no cast, range 50 ?-degree cone (doesn't really look like cone...)
    PhantomEdge = 23200, // Boss->self, 4.0s cast, single-target, visual (applies status changing some effects)
    ScorchingShackle = 23214, // Helper->self, no cast, ??? (happens if chains aren't broken in time)
    BalefulBlade1 = 23336, // Boss->self, 8.0s cast, range 30 circle, visual (knockback, 'blockable' variant)
    BalefulBlade2 = 23337, // Boss->self, 8.0s cast, range 30 circle, visual (knockback, 'unblockable' variant)
    BalefulBladeAOE1 = 23201, // Helper->self, no cast, ???, LOSable knockback 30
    BalefulBladeAOE2 = 23202, // Helper->self, no cast, ???, knockback 30

    VerdantPathFist = 23193, // Boss->self, 3.0s cast, single-target, visual (switch to fists, line stack)
    IronImpact = 23259, // Boss->self, no cast, range 50 width 8 rect line stack
    IronRose = 23221, // SeekerAvatar->self, 3.5s cast, range 50 width 8 rect aoe
    IronSplitter = 23203, // Boss/SeekerAvatar->self, 5.0s cast, single-target, visual (tiles/sands)
    IronSplitterTile1 = 23204, // Helper->self, no cast, range 4 circle
    IronSplitterTile2 = 23205, // Helper->self, no cast, range 8-12 donut
    IronSplitterTile3 = 23206, // Helper->self, no cast, range 16-20 donut
    IronSplitterSand1 = 23207, // Helper->self, no cast, range 4-8 donut
    IronSplitterSand2 = 23208, // Helper->self, no cast, range 12-16 donut
    IronSplitterSand3 = 23209, // Helper->self, no cast, range 20-25 donut
    DeadIron = 23215, // SeekerAvatar->self, 4.0s cast, single-target, visual (earthshakers)
    DeadIronAOE = 23216, // Helper->self, no cast, range 50 30-degree cone earthshaker
    DeadIronSecond = 23364, // SeekerAvatar->self, no cast, single-target, visual (second earthshakers, without cast)

    Ability_Unknown = 14588, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    Mercy = 2056, // none->Boss, extra=0xFA/0xF8/0xF9/0xF7
    BalefulAir = 2490, // Boss->Boss, extra=0x195
    PhantomEdge = 2488, // Boss->Boss, extra=0x0
    IronAir = 2491, // Boss->Boss, extra=0x196
    BurningChains = 769, // none->player, extra=0x0
    MercifulAir = 2489, // Boss->Boss, extra=0x194
}

public enum IconID : uint
{
    MercifulArc = 243, // player
    BurningChains = 238, // player
    DeadIron = 237, // player
}

public enum TetherID : uint
{
    BurningChains = 128, // player->player
    DeadIron = 138, // player->Boss
}
