namespace BossMod.Endwalker.Trial.T08Asura;

class Laceration(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(9);
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D6)
            aoes.Add(new(circle, actor.Position, default, Module.WorldState.CurrentTime.AddSeconds(7.1f - (0.5f * aoes.Count))));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Laceration)
            aoes.Clear();
    }
}
