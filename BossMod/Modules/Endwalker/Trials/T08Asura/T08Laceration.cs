using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Trials.T08Asura
{
    class Laceration : Components.GenericAOEs
    {
        private static readonly AOEShapeCircle circle = new(9);

        private DateTime _activation;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in module.Enemies(OID.PhantomAsura))
                if (_activation != default)
                    yield return new(circle, c.Position, activation: _activation);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id == 0x11D6)
                _activation = module.WorldState.CurrentTime.AddSeconds(5); //actual time is 5-7s delay, but the AOEs end up getting casted at the same time, so we take the earliest time
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Laceration)
                _activation = default;
        }
    }
}
