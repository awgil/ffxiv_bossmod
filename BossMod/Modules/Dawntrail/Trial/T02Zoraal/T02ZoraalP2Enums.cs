namespace BossMod.Dawntrail.Trial.T02ZoraalP2;

public enum OID : uint
{
    Boss = 0x42B4, // R10.050, x?
    Helper = 0x233C, // R0.500, x?, mixed types
    Fang1 = 0x42AA, // R1.000, x?
    Fang2 = 0x42B6, // R1.000, x?
    ShadowOfTural1 = 0x43A8, // R0.500, x?
    ShadowOfTural2 = 0x42AC, // R1.000, x?
    ShadowOfTural3 = 0x42AD, // R1.000, x?
    UnknownActor1 = 0x42B9, // R10.050, x0 (spawn during fight)
    UnknownActor2 = 0x19A, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    //Phase2
    AutoAttack2 = 37798, // Boss->player, no cast, single-target
    Teleport = 37717, // Boss->location, no cast, single-target

    SmitingCircuit1 = 37731, // Boss->self, 7.0s cast, single-target
    SmitingCircuit2 = 37732, // UnknownActor1->self, no cast, single-target
    SmitingCircuit3 = 37733, // UnknownActor1->self, no cast, single-target
    SmitingCircuit4 = 37734, // Helper->self, 7.0s cast, range ?-30 donut
    SmitingCircuit5 = 37735, // Helper->self, 7.0s cast, range 10 circle

    DawnOfAnAge = 37716, // Boss->self, 7.0s cast, range 100 circle // Raidwide and arena change
    Vollok = 37719, // Boss->self, 4.0s cast, single-target // Summons swords

    BitterReaping1 = 37753, // Boss->self, 4.4+0.6s cast, single-target
    BitterReaping2 = 37754, // Helper->player, 5.0s cast, single-target // double telegraphed tankbusters

    Sync = 37721, // Boss->self, 5.0s cast, single-target // square AoE telegraphs on one outside platform
    Gateway = 37723, // Boss->self, 4.0s cast, single-target // Spawns multiple blue tethers linking rows/columns of two outside platforms with different rows/columns of the central platform
    BladeWarp = 37726, // Boss->self, 4.0s cast, single-target // Summons two blades on an outside platform that will face the middle arena.

    ChasmOfVollok1 = 37720, // Fang2->self, 7.0s cast, range 5 width 5 rect
    ChasmOfVollok2 = 37722, // Helper->self, 1.0s cast, range 5 width 5 rect

    ForgedTrack1 = 37727, // Boss->self, 4.0s cast, single-target
    ForgedTrack2 = 37729, // Fang1->self, 11.9s cast, range 20 width 5 rect
    ForgedTrack3 = 37730, // Fang1->self, no cast, range 20 width 5 rect

    UnknownAbility2 = 37728, // Helper->UnknownActor2, no cast, single-target

    Actualize = 37718, // Boss->self, 5.0s cast, range 100 circle // Raidwide and arena change

    HalfFul1l = 37736, // Boss->self, 6.0s cast, single-target
    HalfFull2 = 37737, // Boss->self, 6.0s cast, single-target
    HalfFull3 = 37738, // Helper->self, 6.3s cast, range 60 width 60 rect

    HalfCircuit1 = 37739, // Boss->self, 7.0s cast, single-target
    HalfCircuit2 = 37740, // Boss->self, 7.0s cast, single-target
    HalfCircuit3 = 37741, // Helper->self, 7.3s cast, range 60 width 60 rect
    HalfCircuit4 = 37742, // Helper->self, 7.0s cast, range ?-30 donut
    HalfCircuit5 = 37743, // Helper->self, 7.0s cast, range 10 circle

    FireIII = 37752, // Helper->player, no cast, range 5 circle

    UnknownSpell = 35567, // Helper->player, no cast, single-target

    DutysEdge1 = 37748, // Boss->self, 4.9s cast, single-target
    DutysEdge2 = 37749, // Boss->self, no cast, single-target
    DutysEdge3 = 37750, // Helper->self, no cast, range 100 width 8 rect
}

public enum SID : uint
{
    //Phase2
    UnknownStatus2 = 2397, // none->UnknownActor1, extra=0x2CD
}

public enum IconID : uint
{
    //Phase2
    Icon376 = 376, // player
}

public enum TetherID : uint
{
    //Phase2
    Tether86 = 86, // Fang1->Boss
}
