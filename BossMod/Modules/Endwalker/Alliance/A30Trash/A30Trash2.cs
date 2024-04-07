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

class RingOfSkylight() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.RingOfSkylight), new AOEShapeDonut(8, 30));
class RingOfSkylightInterruptHint() : Components.CastInterruptHint(ActionID.MakeSpell(AID.RingOfSkylight));
class SkylightCross() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SkylightCross), new AOEShapeCross(60, 4));
class SkylightCrossInterruptHint() : Components.CastInterruptHint(ActionID.MakeSpell(AID.SkylightCross));
class Skylight() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Skylight), new AOEShapeCircle(6));

public class A30Trash2Pack1States : StateMachineBuilder
{
    public A30Trash2Pack1States(A30Trash2Pack1 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RingOfSkylight>()
            .ActivateOnEnter<RingOfSkylightInterruptHint>()
            .ActivateOnEnter<SkylightCross>()
            .ActivateOnEnter<SkylightCrossInterruptHint>()
            .ActivateOnEnter<Skylight>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.AngelosMikros.All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.AngelosPack1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 5)]
public class A30Trash2Pack1 : BossModule
{
    public IReadOnlyList<Actor> AngelosMikros;

    public A30Trash2Pack1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(800, 770), 15, 25))
    {
        AngelosMikros = Enemies(OID.AngelosMikros);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(AngelosMikros, ArenaColor.Enemy);
    }
}

public class A30Trash2Pack2States : StateMachineBuilder
{
    public A30Trash2Pack2States(A30Trash2Pack2 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RingOfSkylight>()
            .ActivateOnEnter<RingOfSkylightInterruptHint>()
            .ActivateOnEnter<SkylightCross>()
            .ActivateOnEnter<SkylightCrossInterruptHint>()
            .Raw.Update = () => module.Angelos.All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.AngelosPack2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 6)]
public class A30Trash2Pack2 : BossModule
{
    public IReadOnlyList<Actor> Angelos;

    public A30Trash2Pack2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(800, 910), 20))
    {
        Angelos = Enemies(OID.AngelosPack2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Angelos, ArenaColor.Enemy);
    }
}
