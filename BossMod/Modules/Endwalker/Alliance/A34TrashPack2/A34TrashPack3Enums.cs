namespace BossMod.Endwalker.Alliance.A34TrashPack2;

public enum OID : uint
{
    Boss = 0x4013, // R=3.6
    AngelosMikros = 0x4014, // R=2.0
};

public enum AID : uint
{
    AutoAttack = 870, // 4014/4013->player, no cast, single-target
    Skylight = 35446, // 4014->self, 3,0s cast, range 6 circle
    SkylightCross = 35445, // 4013->self, 5,0s cast, range 60 width 8 cross
    RingOfSkylight = 35444, // 4013->self, 5,0s cast, range ?-30 donut
};
