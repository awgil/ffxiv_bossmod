namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class RazingVolleyParticleBeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RazingVolleyParticleBeam), new AOEShapeRect(45, 4))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var deadline = Module.CastFinishAt(Casters.Count > 0 ? Casters[0].CastInfo : null, 3);
        foreach (var c in Casters)
        {
            var activation = Module.CastFinishAt(c.CastInfo);
            if (activation > deadline)
                break;
            yield return new(Shape, c.Position, c.CastInfo?.Rotation ?? c.Rotation, activation);
        }
    }
}
