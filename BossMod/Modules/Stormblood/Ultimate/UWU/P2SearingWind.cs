using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P2SearingWind : Components.UniformStackSpread
    {
        public P2SearingWind() : base(0, 14, alwaysShowSpreads: true, includeDeadTargets: true) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.InfernoHowl && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            {
                AddSpread(target, module.WorldState.CurrentTime.AddSeconds(8));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SearingWind)
            {
                var index = Enumerable.Range(0, Spreads.Count).MinBy(i => (spell.TargetXZ - Spreads[i].Target.Position).LengthSq());
                if (index < Spreads.Count)
                {
                    ref var spread = ref Spreads.Ref(index);
                    var status = spread.Target.FindStatus(SID.SearingWind);
                    if (status == null || (status.Value.ExpireAt - module.WorldState.CurrentTime).TotalSeconds < 6)
                    {
                        Spreads.RemoveAt(index);
                    }
                    else
                    {
                        spread.Activation = module.WorldState.CurrentTime.AddSeconds(6);
                    }
                }
            }
        }
    }
}
