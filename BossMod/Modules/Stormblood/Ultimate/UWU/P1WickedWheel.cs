using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    // wicked wheel is used in phase 1 (depending on 'woken' status, it can be used with followup wicked tornado - this can happen with low dps late in the phase) - it is triggered by cast start
    // it is also used during phase 4 as part of some mechanics (ultimate predation, ???) - in such case we typically want to show it earlier (based on PATE)
    class WickedWheel : Components.GenericAOEs
    {
        public DateTime AwakenedResolve { get; private set; }
        public List<(Actor source, AOEShape shape, DateTime activation)> Sources = new();

        public static AOEShapeCircle ShapeWheel = new(8.7f);
        public static AOEShapeDonut ShapeTornado = new(7, 20);
        public static AOEShapeCircle ShapeSister = new(8.36f);
        public static AOEShapeCircle ShapeCombined = new(20); // wheel+tornado, used when players are expected to outrange both - e.g. during ultimate predation

        public bool Active => Sources.Count > 0;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return Sources.Select(s => new AOEInstance(s.shape, s.source.Position, s.source.Rotation, s.activation));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.WickedWheel:
                    // if wheel was predicted, keep the shape, but update the activation time
                    var predictedIndex = Sources.FindIndex(s => s.source == caster);
                    if (predictedIndex >= 0)
                        Sources[predictedIndex] = (caster, Sources[predictedIndex].shape, spell.NPCFinishAt);
                    else
                        Sources.Add((caster, ShapeWheel, spell.NPCFinishAt));
                    break;
                case AID.WickedWheelSister:
                    Sources.Add((caster, ShapeSister, spell.NPCFinishAt));
                    break;
            };
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.WickedWheel:
                    Sources.RemoveAll(s => s.source == caster);
                    AwakenedResolve = module.WorldState.CurrentTime.AddSeconds(2.1f); // for tornado
                    if (caster.FindStatus(SID.Woken) != null)
                        Sources.Add((caster, ShapeTornado, AwakenedResolve));
                    ++NumCasts;
                    break;
                case AID.WickedTornado:
                    Sources.RemoveAll(s => s.shape == ShapeTornado);
                    ++NumCasts;
                    break;
                case AID.WickedWheelSister:
                    Sources.RemoveAll(s => s.source == caster);
                    ++NumCasts;
                    break;
            }
        }
    }

    class P1WickedWheel : WickedWheel { }

    class P4WickedWheel : WickedWheel
    {
        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.Garuda && id == 0x1E43)
                Sources.Add((actor, ShapeCombined, module.WorldState.CurrentTime.AddSeconds(8.1f)));
        }
    }
}
