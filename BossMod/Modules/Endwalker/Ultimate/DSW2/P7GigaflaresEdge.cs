using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P7GigaflaresEdge : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCircle _shape = new(20); // TODO: verify falloff

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.GigaflaresEdgeAOE1 or AID.GigaflaresEdgeAOE2 or AID.GigaflaresEdgeAOE3)
            {
                _aoes.Add(new(_shape, caster.Position, default, spell.FinishAt));
                _aoes.SortBy(aoe => aoe.Activation);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.GigaflaresEdgeAOE1 or AID.GigaflaresEdgeAOE2 or AID.GigaflaresEdgeAOE3)
            {
                ++NumCasts;
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
            }
        }
    }
}
