namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class Phaser(BossModule module) : Components.StandardAOEs(module, AID.Phaser, new AOEShapeCone(23, 30.Degrees())) // TODO: verify angle
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var deadline = Module.CastFinishAt(Casters.Count > 0 ? Casters[0].CastInfo : null, 1);
        foreach (var c in Casters)
        {
            var activation = Module.CastFinishAt(c.CastInfo);
            if (activation < deadline)
                yield return new(Shape, c.Position, c.CastInfo?.Rotation ?? c.Rotation, activation);
        }
    }
}
