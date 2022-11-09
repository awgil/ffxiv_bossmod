using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P8S2
{
    class AshingBlaze : Components.GenericAOEs
    {
        private WPos? _origin;
        private static AOEShapeRect _shape = new(46, 10);

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_origin != null)
                yield return (_shape, _origin.Value, 0.Degrees(), module.PrimaryActor.CastInfo?.FinishAt ?? new DateTime());
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AshingBlazeL:
                    _origin = caster.Position - new WDir(_shape.HalfWidth, 0);
                    break;
                case AID.AshingBlazeR:
                    _origin = caster.Position + new WDir(_shape.HalfWidth, 0);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.AshingBlazeL or AID.AshingBlazeR)
                _origin = null;
        }
    }
}
