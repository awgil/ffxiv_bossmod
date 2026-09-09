namespace BossMod.Endwalker.Trial.T08Asura;

class Laceration(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(9);

    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Module.Enemies(OID.PhantomAsura))
            if (_activation != default)
                yield return new(circle, c.Position, default, _activation);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D6)
            _activation = WorldState.FutureTime(5); //actual time is 5-7s delay, but the AOEs end up getting casted at the same time, so we take the earliest time
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Laceration)
            _activation = default;
    }
}
