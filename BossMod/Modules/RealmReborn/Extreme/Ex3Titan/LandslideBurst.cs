using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex3Titan
{
    // burst (bomb explosion) needs to be shown in particular moment (different for different patterns) so that ai can avoid them nicely
    class LandslideBurst : Components.GenericAOEs
    {
        public int MaxBombs = 9;
        private List<Actor> _landslides = new();
        private List<Actor> _bursts = new(); // TODO: reconsider: we can start showing bombs even before cast starts...
        public int NumActiveBursts => _bursts.Count;

        private static AOEShapeRect _shapeLandslide = new(40.25f, 3);
        private static AOEShapeCircle _shapeBurst = new(6.3f);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var l in _landslides)
                yield return new(_shapeLandslide, l.Position, l.CastInfo!.Rotation, l.CastInfo.FinishAt);
            foreach (var b in _bursts.Take(MaxBombs))
                yield return new(_shapeBurst, b.Position, b.CastInfo!.Rotation, b.CastInfo.FinishAt);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.LandslideBoss:
                case AID.LandslideHelper:
                case AID.LandslideGaoler:
                    _landslides.Add(caster);
                    break;
                case AID.Burst:
                    _bursts.Add(caster);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.LandslideBoss:
                case AID.LandslideHelper:
                case AID.LandslideGaoler:
                    _landslides.Remove(caster);
                    break;
                case AID.Burst:
                    _bursts.Remove(caster);
                    break;
            }
        }
    }
}
