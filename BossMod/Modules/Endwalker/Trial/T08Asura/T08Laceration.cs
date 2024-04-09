namespace BossMod.Endwalker.Trial.T08Asura;

class Laceration : Components.GenericAOEs
{
    private static readonly AOEShapeCircle circle = new(9);
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => aoes;

    public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
    {
        if (id == 0x11D6)
            aoes.Add(new(circle, actor.Position, activation: module.WorldState.CurrentTime.AddSeconds(7.1f - (0.5f * aoes.Count))));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Laceration)
            aoes.Clear();
    }
}
