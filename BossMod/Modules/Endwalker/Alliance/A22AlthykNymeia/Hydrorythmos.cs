using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Alliance.A22AlthykNymeia
{
    class Hydrorythmos : Components.GenericAOEs
    {
        private Angle _dir;
        private DateTime _activation;

        private static AOEShapeRect _shapeFirst = new(25, 5, 25);
        private static AOEShapeRect _shapeRest = new(25, 2.5f, 25);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (NumCasts > 0)
            {
                var offset = ((NumCasts + 1) >> 1) * 5 * _dir.ToDirection().OrthoL();
                yield return new(_shapeRest, module.Bounds.Center + offset, _dir, _activation);
                yield return new(_shapeRest, module.Bounds.Center - offset, _dir, _activation);
            }
            else if (_activation != default)
            {
                yield return new(_shapeFirst, module.Bounds.Center, _dir, _activation);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.HydrorythmosFirst)
            {
                _dir = spell.Rotation;
                _activation = spell.FinishAt;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HydrorythmosFirst or AID.HydrorythmosRest)
            {
                ++NumCasts;
                _activation = module.WorldState.CurrentTime.AddSeconds(2.1f);
            }
        }
    }
}
