namespace BossMod.Endwalker.Alliance.A30Trash2;

public enum OID : uint
{
    AngelosPack1 = 0x4013, // R3.600, x1
    AngelosMikros = 0x4014, // R2.000, x8

    AngelosPack2 = 0x40E1, // R3.600, x3
}

public enum AID : uint
{
    AutoAttack = 870, // AngelosPack1/AngelosMikros/AngelosPack2->player, no cast, single-target
    RingOfSkylight = 35444, // AngelosPack1/AngelosPack2->self, 5.0s cast, range ?-30 donut
    SkylightCross = 35445, // AngelosPack1/AngelosPack2->self, 5.0s cast, range 60 width 8 cross
    Skylight = 35446, // AngelosMikros->self, 3.0s cast, range 6 circle
}

class RingOfSkylight(BossModule module) : Components.StandardAOEs(module, AID.RingOfSkylight, new AOEShapeDonut(8, 30)); // note: it's interruptible, but that's not worth the hint
class SkylightCross(BossModule module) : Components.StandardAOEs(module, AID.SkylightCross, new AOEShapeCross(60, 4)); // note: it's interruptible, but that's not worth the hint
class Skylight(BossModule module) : Components.StandardAOEs(module, AID.Skylight, new AOEShapeCircle(6));

public class A30Trash2Pack1States : StateMachineBuilder
{
    public A30Trash2Pack1States(A30Trash2Pack1 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RingOfSkylight>()
            .ActivateOnEnter<SkylightCross>()
            .ActivateOnEnter<Skylight>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.AngelosMikros).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.AngelosPack1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 5)]
public class A30Trash2Pack1(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 770), new ArenaBoundsRect(15, 25))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.AngelosMikros), ArenaColor.Enemy);
    }
}

public class A30Trash2Pack2States : StateMachineBuilder
{
    public A30Trash2Pack2States(A30Trash2Pack2 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RingOfSkylight>()
            .ActivateOnEnter<SkylightCross>()
            .Raw.Update = () => module.Enemies(OID.AngelosPack2).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.AngelosPack2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 6)]
public class A30Trash2Pack2(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 910), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.AngelosPack2), ArenaColor.Enemy);
    }
}
