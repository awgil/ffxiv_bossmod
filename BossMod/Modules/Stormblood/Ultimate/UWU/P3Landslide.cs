using System;
using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    // in p3, landslide is baited on a random (?) target (rotation phi for main cast); helpers cast their casts at phi +- 45 and phi +- 135
    // if boss is awakened, these 5 landslides are followed by another 5 landslides at phi +- 22.5, phi +- 90 and phi + 180; there is no point predicting them, since corresponding casts start almost immediately (<0.1s)
    // in p4, landslides are cast at predetermined angles (ultimate predation, ???)
    class Landslide : Components.GenericAOEs
    {
        public bool Awakened { get; private set; }
        public DateTime PredictedActivation;
        protected Actor? PredictedSource;
        private List<Actor> _casters = new();

        public static AOEShapeRect ShapeBoss = new(44.55f, 3, 4.55f);
        public static AOEShapeRect ShapeHelper = new(40.5f, 3, 0.5f); // difference is only in hitbox radius

        public bool CastsActive => _casters.Count > 0;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (PredictedSource != null)
            {
                yield return new(ShapeBoss, PredictedSource.Position, PredictedSource.Rotation, PredictedActivation);
                yield return new(ShapeHelper, PredictedSource.Position, PredictedSource.Rotation + 45.Degrees(), PredictedActivation);
                yield return new(ShapeHelper, PredictedSource.Position, PredictedSource.Rotation - 45.Degrees(), PredictedActivation);
                yield return new(ShapeHelper, PredictedSource.Position, PredictedSource.Rotation + 135.Degrees(), PredictedActivation);
                yield return new(ShapeHelper, PredictedSource.Position, PredictedSource.Rotation - 135.Degrees(), PredictedActivation);
            }

            foreach (var c in _casters)
                yield return new((OID)c.OID == OID.Titan ? ShapeBoss : ShapeHelper, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.LandslideBoss or AID.LandslideBossAwakened or AID.LandslideHelper or AID.LandslideHelperAwakened or AID.LandslideUltima or AID.LandslideUltimaHelper)
            {
                PredictedSource = null;
                _casters.Add(caster);
                if ((AID)spell.Action.ID == AID.LandslideBossAwakened)
                    Awakened = true;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.LandslideBoss or AID.LandslideBossAwakened or AID.LandslideHelper or AID.LandslideHelperAwakened or AID.LandslideUltima or AID.LandslideUltimaHelper)
            {
                _casters.Remove(caster);
                ++NumCasts;
                if ((AID)spell.Action.ID == AID.LandslideBoss)
                    PredictedActivation = module.WorldState.CurrentTime.AddSeconds(2); // used if boss wasn't awakened when it should've been
            }
        }
    }

    class P3Landslide : Landslide { }

    class P4Landslide : Landslide
    {
        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.Titan && id == 0x1E43)
            {
                PredictedSource = actor;
                PredictedActivation = module.WorldState.CurrentTime.AddSeconds(8.1f);
            }
        }
    }
}
