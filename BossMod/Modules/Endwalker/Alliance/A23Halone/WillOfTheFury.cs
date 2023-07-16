using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Alliance.A23Halone
{
    class WillOfTheFury : Components.GenericAOEs
    {
        private AOEInstance? _aoe;

        private static float _impactRadiusIncrement = 6;

        public bool Active => _aoe != null;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.WillOfTheFuryAOE1)
            {
                UpdateAOE(module.Bounds.Center, spell.FinishAt);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.WillOfTheFuryAOE1 or AID.WillOfTheFuryAOE2 or AID.WillOfTheFuryAOE3 or AID.WillOfTheFuryAOE4 or AID.WillOfTheFuryAOE5)
            {
                ++NumCasts;
                UpdateAOE(module.Bounds.Center, module.WorldState.CurrentTime.AddSeconds(2));
            }
        }

        private void UpdateAOE(WPos origin, DateTime activation)
        {
            var outerRadius = (5 - NumCasts) * _impactRadiusIncrement;
            AOEShape? shape = NumCasts switch
            {
                < 4 => new AOEShapeDonut(outerRadius - _impactRadiusIncrement, outerRadius),
                4 => new AOEShapeCircle(outerRadius),
                _ => null
            };
            _aoe = shape != null ? new(shape, origin, default, activation) : null;
        }
    }
}
