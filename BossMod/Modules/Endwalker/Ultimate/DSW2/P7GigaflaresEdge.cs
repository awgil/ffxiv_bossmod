namespace BossMod.Endwalker.Ultimate.DSW2;

class P7GigaflaresEdge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(20); // TODO: verify falloff

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.GigaflaresEdgeAOE1 or AID.GigaflaresEdgeAOE2 or AID.GigaflaresEdgeAOE3)
        {
            _aoes.Add(new(_shape, caster.Position, default, spell.NPCFinishAt));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.GigaflaresEdgeAOE1 or AID.GigaflaresEdgeAOE2 or AID.GigaflaresEdgeAOE3)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
