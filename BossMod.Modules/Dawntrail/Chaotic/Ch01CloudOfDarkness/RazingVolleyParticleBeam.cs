namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class RazingVolleyParticleBeam(BossModule module) : Components.StandardAOEs(module, AID.RazingVolleyParticleBeam, new AOEShapeRect(45, 4))
{
    private DateTime _nextBundle;

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

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            NumCasts = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && WorldState.CurrentTime > _nextBundle)
        {
            ++NumCasts;
            _nextBundle = WorldState.FutureTime(1);
        }
    }
}
