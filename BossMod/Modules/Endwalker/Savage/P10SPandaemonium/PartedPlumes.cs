namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class PartedPlumes(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PartedPlumesAOE), new AOEShapeCone(50, 10.Degrees()), 16)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, i < 2 ? ArenaColor.Danger : ArenaColor.AOE));
}

class PartedPlumesVoidzone(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        yield return new(_shape, new WPos(100, 100));
    }
}
