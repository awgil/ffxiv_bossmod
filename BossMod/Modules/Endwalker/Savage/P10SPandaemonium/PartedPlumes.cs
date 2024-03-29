namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class PartedPlumes : Components.SelfTargetedAOEs
{
    public PartedPlumes() : base(ActionID.MakeSpell(AID.PartedPlumesAOE), new AOEShapeCone(50, 10.Degrees()), 16) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        return ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, i < 2 ? ArenaColor.Danger : ArenaColor.AOE));
    }
}

class PartedPlumesVoidzone : Components.GenericAOEs
{
    private static readonly AOEShapeCircle _shape = new(8);

    public PartedPlumesVoidzone() : base(default, "GTFO from voidzone!") { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        yield return new(_shape, new WPos(100, 100));
    }
}
