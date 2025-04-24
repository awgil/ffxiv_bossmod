namespace BossMod.Stormblood.Ultimate.UWU;

// crimson cyclone has multiple variations:
// p2 first cast is a single charge along cardinal
// p2 second cast is two charges along both cardinals
// p2 third cast is four staggered charges, with different patterns depending on whether awakening happened (TODO: we can predict that very early)
// p4 predation is a single awakened charge along intercardinal
class CrimsonCyclone(BossModule module, float predictionDelay) : Components.GenericAOEs(module, AID.CrimsonCyclone)
{
    private readonly float _predictionDelay = predictionDelay;
    private readonly List<(AOEShape shape, WPos pos, Angle rot, DateTime activation)> _predicted = []; // note: there could be 1/2/4 predicted normal charges and 0 or 2 'cross' charges
    private readonly List<Actor> _casters = [];

    private static readonly AOEShapeRect _shapeMain = new(49, 9, 5);
    private static readonly AOEShapeRect _shapeCross = new(44.5f, 5, 0.5f);

    public bool CastsPredicted => _predicted.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_predicted.Count <= 2) // don't draw 4 predicted charges, it is pointless
            foreach (var p in _predicted)
                yield return new(p.shape, p.pos, p.rot, p.activation);
        foreach (var c in _casters)
            yield return new(_shapeMain, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (NumCasts == 0 && (OID)actor.OID == OID.Ifrit && id == 0x1E43)
            _predicted.Add((_shapeMain, actor.Position, actor.Rotation, WorldState.FutureTime(_predictionDelay)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (NumCasts == 0)
                _predicted.Clear();
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _casters.Remove(caster);
            if (caster == ((UWU)Module).Ifrit() && caster.FindStatus(SID.Woken) != null)
            {
                _predicted.Add((_shapeCross, Module.Center - 19.5f * (spell.Rotation + 45.Degrees()).ToDirection(), spell.Rotation + 45.Degrees(), WorldState.FutureTime(2.2f)));
                _predicted.Add((_shapeCross, Module.Center - 19.5f * (spell.Rotation - 45.Degrees()).ToDirection(), spell.Rotation - 45.Degrees(), WorldState.FutureTime(2.2f)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.CrimsonCycloneCross)
        {
            _predicted.Clear();
        }
    }
}

class P2CrimsonCyclone(BossModule module) : CrimsonCyclone(module, 5.2f);
class P4CrimsonCyclone(BossModule module) : CrimsonCyclone(module, 8.1f);
