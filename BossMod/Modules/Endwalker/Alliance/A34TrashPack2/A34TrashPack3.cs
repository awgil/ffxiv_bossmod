namespace BossMod.Endwalker.Alliance.A34TrashPack2;

class RingOfSkylight : Components.SelfTargetedAOEs
{
    public RingOfSkylight() : base(ActionID.MakeSpell(AID.RingOfSkylight), new AOEShapeDonut(8, 30)) { }
}

class SkylightCross : Components.SelfTargetedAOEs
{
    public SkylightCross() : base(ActionID.MakeSpell(AID.SkylightCross), new AOEShapeCross(60, 4)) { }
}

class Skylight : Components.SelfTargetedAOEs
{
    public Skylight() : base(ActionID.MakeSpell(AID.SkylightCross), new AOEShapeCircle(6)) { }
}

class RingOfSkylightHint : Components.CastInterruptHint
{
    public RingOfSkylightHint() : base(ActionID.MakeSpell(AID.RingOfSkylight)) { }
}

class SkylightCrossHint : Components.CastInterruptHint
{
    public SkylightCrossHint() : base(ActionID.MakeSpell(AID.SkylightCross)) { }
}

public class A34TrashPack2States : StateMachineBuilder
{
    public A34TrashPack2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RingOfSkylight>()
            .ActivateOnEnter<RingOfSkylightHint>()
            .ActivateOnEnter<SkylightCross>()
            .ActivateOnEnter<SkylightCrossHint>()
            .ActivateOnEnter<Skylight>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.AngelosMikros).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481)]
public class A34TrashPack2 : BossModule
{
    public A34TrashPack2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(800, 770), 15, 25)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var e in Enemies(OID.Boss))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.AngelosMikros))
            Arena.Actor(e, ArenaColor.Enemy);
    }
}
