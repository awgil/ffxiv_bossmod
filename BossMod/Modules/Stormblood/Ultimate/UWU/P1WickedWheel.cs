﻿namespace BossMod.Stormblood.Ultimate.UWU;

// wicked wheel is used in phase 1 (depending on 'woken' status, it can be used with followup wicked tornado - this can happen with low dps late in the phase) - it is triggered by cast start
// it is also used during phase 4 as part of some mechanics (ultimate predation, ???) - in such case we typically want to show it earlier (based on PATE)
class WickedWheel(BossModule module) : Components.GenericAOEs(module)
{
    public DateTime AwakenedResolve { get; private set; }
    public List<(Actor source, AOEShape shape, DateTime activation)> Sources = [];

    public static readonly AOEShapeCircle ShapeWheel = new(8.7f);
    public static readonly AOEShapeDonut ShapeTornado = new(7, 20);
    public static readonly AOEShapeCircle ShapeSister = new(8.36f);
    public static readonly AOEShapeCircle ShapeCombined = new(20); // wheel+tornado, used when players are expected to outrange both - e.g. during ultimate predation

    public bool Active => Sources.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Sources.Select(s => new AOEInstance(s.shape, s.source.Position, s.source.Rotation, s.activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WickedWheel:
                // if wheel was predicted, keep the shape, but update the activation time
                var predictedIndex = Sources.FindIndex(s => s.source == caster);
                if (predictedIndex >= 0)
                    Sources[predictedIndex] = (caster, Sources[predictedIndex].shape, Module.CastFinishAt(spell));
                else
                    Sources.Add((caster, ShapeWheel, Module.CastFinishAt(spell)));
                break;
            case AID.WickedWheelSister:
                Sources.Add((caster, ShapeSister, Module.CastFinishAt(spell)));
                break;
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WickedWheel:
                Sources.RemoveAll(s => s.source == caster);
                AwakenedResolve = WorldState.FutureTime(2.1f); // for tornado
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

class P1WickedWheel(BossModule module) : WickedWheel(module) { }

class P4WickedWheel(BossModule module) : WickedWheel(module)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Garuda && id == 0x1E43)
            Sources.Add((actor, ShapeCombined, WorldState.FutureTime(8.1f)));
    }
}
