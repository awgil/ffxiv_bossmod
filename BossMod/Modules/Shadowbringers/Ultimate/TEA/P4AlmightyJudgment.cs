using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    // note: sets are 2s apart, 8-9 casts per set
    class P4AlmightyJudgment : Components.GenericAOEs
    {
        private List<(WPos pos, DateTime activation)> _casters = new();

        private static AOEShapeCircle _shape = new(6);

        public bool Active => _casters.Count > 0;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_casters.Count > 0)
            {
                var deadlineImminent = _casters[0].activation.AddSeconds(1);
                var deadlineFuture = _casters[0].activation.AddSeconds(3);
                foreach (var c in Enumerable.Reverse(_casters).SkipWhile(c => c.activation > deadlineFuture))
                {
                    yield return new(_shape, c.pos, default, c.activation, c.activation < deadlineImminent ? ArenaColor.Danger : ArenaColor.AOE);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.AlmightyJudgmentVisual)
                _casters.Add((caster.Position, module.WorldState.CurrentTime.AddSeconds(8)));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.AlmightyJudgmentAOE)
                _casters.RemoveAll(c => c.pos.AlmostEqual(caster.Position, 1));
        }
    }
}
