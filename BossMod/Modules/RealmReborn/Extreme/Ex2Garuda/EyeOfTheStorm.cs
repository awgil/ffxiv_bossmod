using System;
using System.Collections.Generic;

namespace BossMod.RealmReborn.Extreme.Ex2Garuda
{
    class EyeOfTheStorm : Components.GenericAOEs
    {
        private Actor? _caster;
        private DateTime _nextCastAt;
        private static AOEShapeDonut _shape = new(12, 25); // TODO: verify inner radius

        public EyeOfTheStorm() : base(ActionID.MakeSpell(AID.EyeOfTheStorm)) { }

        public bool Active(BossModule module) => _caster?.CastInfo != null || _nextCastAt > module.WorldState.CurrentTime;

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_caster != null)
                yield return (_shape, _caster.Position, new(), _nextCastAt);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _caster = caster;
                _nextCastAt = caster.CastInfo!.FinishAt;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _nextCastAt = module.WorldState.CurrentTime.AddSeconds(4.2f);
            }
        }

    }
}
