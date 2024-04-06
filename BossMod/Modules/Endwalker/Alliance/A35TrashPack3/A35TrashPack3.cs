namespace BossMod.Endwalker.Alliance.A35TrashPack3;

class RingOfSkylight : Components.SelfTargetedAOEs
{
    public RingOfSkylight() : base(ActionID.MakeSpell(AID.RingOfSkylight), new AOEShapeDonut(8, 30)) { }
}

class SkylightCross : Components.SelfTargetedAOEs
{
    public SkylightCross() : base(ActionID.MakeSpell(AID.SkylightCross), new AOEShapeCross(60, 4)) { }
}

class RingOfSkylightHint : Components.CastInterruptHint
{
    public RingOfSkylightHint() : base(ActionID.MakeSpell(AID.RingOfSkylight)) { }
}

class SkylightCrossHint : Components.CastInterruptHint
{
    public SkylightCrossHint() : base(ActionID.MakeSpell(AID.SkylightCross)) { }
}

public class A35TrashPack2States : StateMachineBuilder
{
    public A35TrashPack2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RingOfSkylight>()
            .ActivateOnEnter<RingOfSkylightHint>()
            .ActivateOnEnter<SkylightCross>()
            .ActivateOnEnter<SkylightCrossHint>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481)]
public class A35TrashPack2 : BossModule
{
    public A35TrashPack2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(800, 910), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var e in Enemies(OID.Boss))
            Arena.Actor(e, ArenaColor.Enemy);
    }
}
