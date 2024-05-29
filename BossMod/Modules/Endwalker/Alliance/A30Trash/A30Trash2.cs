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

class RingOfSkylight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RingOfSkylight), new AOEShapeDonut(8, 30));
class RingOfSkylightInterruptHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.RingOfSkylight));
class SkylightCross(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SkylightCross), new AOEShapeCross(60, 4));
class SkylightCrossInterruptHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.SkylightCross));
class Skylight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Skylight), new AOEShapeCircle(6));

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
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.AngelosMikros).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, veyn", PrimaryActorOID = (uint)OID.AngelosPack1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 5)]
public class A30Trash2Pack1(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Rectangle(new(800, 786), 21, 13.5f), new Rectangle(new(800, 767), 7.5f, 10), new Rectangle(new(800, 758), 10, 4)];
    private static readonly List<Shape> difference = [new Square(new(811.25f, 787), 1.5f), new Square(new(811.25f, 777.4f), 1.5f), new Square(new(788.75f, 787), 1.5f), new Square(new(788.75f, 777.4f), 1.5f),
    new Circle(new(793.4f, 762.75f), 1.25f), new Circle(new(806.6f, 762.75f), 1.25f)];
    private static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);

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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, veyn", PrimaryActorOID = (uint)OID.AngelosPack2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 6)]
public class A30Trash2Pack2(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 909.75f), new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.AngelosPack2), ArenaColor.Enemy);
    }
}
