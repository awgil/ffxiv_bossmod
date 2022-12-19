using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    class CastShadow : Components.GenericAOEs
    {
        public List<Actor> FirstAOECasters = new();
        public List<Actor> SecondAOECasters = new();

        private static AOEShape _shape = new AOEShapeCone(65, 15.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return (FirstAOECasters.Count > 0 ? FirstAOECasters : SecondAOECasters).Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            ListForAction(spell.Action)?.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            ListForAction(spell.Action)?.Remove(caster);
        }

        private List<Actor>? ListForAction(ActionID action) => (AID)action.ID switch
        {
            AID.NCastShadowAOE1 or AID.SCastShadowAOE1 => FirstAOECasters,
            AID.NCastShadowAOE2 or AID.SCastShadowAOE2 => SecondAOECasters,
            _ => null
        };
    }
}
