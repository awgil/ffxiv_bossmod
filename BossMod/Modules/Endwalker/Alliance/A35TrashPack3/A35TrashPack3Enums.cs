namespace BossMod.Endwalker.Alliance.A35TrashPack3;

public enum OID : uint
{
    Boss = 0x40E1, // R=3.6
};

public enum AID : uint
{
    AutoAttack = 870, // 40E1->player, no cast, single-target
    SkylightCross = 35445, // 40E1->self, 5,0s cast, range 60 width 8 cross
    RingOfSkylight = 35444, // 40E1->self, 5,0s cast, range ?-30 donut
};
