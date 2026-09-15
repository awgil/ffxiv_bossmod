namespace BossMod.Dawntrail.Savage.RM02SHoneyBLovely;

class BlindingLoveBait(BossModule module) : Components.StandardAOEs(module, AID.BlindingLoveBaitAOE, new AOEShapeRect(50, 4))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters.Skip(1))
            yield return new(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
        foreach (var c in Casters.Take(1))
            yield return new(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), ArenaColor.Danger);
    }
}
