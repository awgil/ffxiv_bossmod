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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.AngelosPack1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 5)]
public class A30Trash2Pack1(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsPolygon(arenacoords))
{
    private static readonly List<WPos> arenacoords = [new(794.8f, 762.7f), new(794.8f, 763.6f), new(792.9f, 764), new(793.2f, 770.5f), new(793.5f, 770.8f), new(793.5f, 772.3f),
    new(793.2f, 772.6f), new(783.4f, 772.6f), new(783.4f, 777), new(788.4f, 775.8f), new(790, 775.9f), new(790.3f, 776.2f), new(790.3f, 778.8f), new(789.6f, 779.1f),
    new(783.1f, 778.9f), new(783.3f, 786.2f), new(788.4f, 785.4f), new(790.4f, 785.9f), new(790.4f, 788.6f), new(783, 788.7f), new(783.2f, 795.8f), new(818.1f, 796.3f),
    new(818.2f, 787.6f), new(812.5f, 788.6f), new(809.8f, 788.7f), new(809.9f, 785.4f), new(817.4f, 785.8f), new(817.5f, 778.4f), new(812.6f, 779.1f), new(810.3f, 779.1f),
    new(809.4f, 779f), new(809.6f, 776.2f), new(819, 776), new(819, 772.6f), new(806.6f, 772.5f), new(806.5f, 772.2f), new(806.7f, 764), new(805.5f, 763.5f), new(805.3f, 762.2f)];
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
            .ActivateOnEnter<RingOfSkylightInterruptHint>()
            .ActivateOnEnter<SkylightCross>()
            .ActivateOnEnter<SkylightCrossInterruptHint>()
            .Raw.Update = () => module.Enemies(OID.AngelosPack2).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.AngelosPack2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 6)]
public class A30Trash2Pack2(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsSquare(new(800, 910), 20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.AngelosPack2), ArenaColor.Enemy);
    }
}
