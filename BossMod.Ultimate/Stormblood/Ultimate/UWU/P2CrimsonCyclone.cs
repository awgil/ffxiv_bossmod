
namespace BossMod.Stormblood.Ultimate.UWU;

// crimson cyclone has multiple variations:
// p2 first cast is a single charge along cardinal
// p2 second cast is two charges along both cardinals
// p2 third cast is four staggered charges, with different patterns depending on whether awakening happened (TODO: we can predict that very early)
// p4 predation is a single awakened charge along intercardinal
class CrimsonCyclone(BossModule module, float predictionDelay) : Components.GenericAOEs(module, AID.CrimsonCyclone)
{
    public bool Risky = true;
    private readonly float _predictionDelay = predictionDelay;
    private readonly List<(AOEShape shape, WPos pos, Angle rot, DateTime activation)> _predicted = []; // note: there could be 1/2/4 predicted normal charges and 0 or 2 'cross' charges
    private readonly List<Actor> _casters = [];

    private static readonly AOEShapeRect _shapeMain = new(49, 9, 5);
    private static readonly AOEShapeRect _shapeCross = new(44.5f, 5, 0.5f);

    public bool CastsPredicted => _predicted.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in _predicted.Take(2))
            yield return new(p.shape, p.pos, p.rot, p.activation, Risky: Risky);
        foreach (var c in _casters)
            yield return new(_shapeMain, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Risky: Risky);
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

abstract class CC2(BossModule module, float predictionDelay) : Components.GenericAOEs(module, AID.CrimsonCyclone)
{
    public bool Risky = true;
    public readonly float PredictionDelay = predictionDelay;
    protected readonly List<(AOEShape shape, WPos pos, Angle rot, DateTime activation)> Predicted = [];

    public static readonly AOEShapeRect ShapeMain = new(49, 9);
    public static readonly AOEShapeRect ShapeCross = new(25, 5, 25);

    public bool CastsPredicted => Predicted.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Predicted.TakeSpan(p => p.activation, TimeSpan.FromSeconds(3)).Select(p => new AOEInstance(p.shape, p.pos, p.rot, p.activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var ix = Predicted.FindIndex(p => p.pos.AlmostEqual(spell.LocXZ, 1) && p.rot.AlmostEqual(spell.Rotation, 0.1f));
            if (ix >= 0)
                Predicted[ix] = (ShapeMain, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var ix = Predicted.FindIndex(p => p.shape == ShapeMain);
            if (ix >= 0)
                Predicted.RemoveAt(ix);
        }

        if ((AID)spell.Action.ID == AID.CrimsonCycloneCross)
            Predicted.RemoveAll(p => p.shape == ShapeCross);
    }

    protected void Predict(WPos source, Angle rotation, DateTime? activation = null, AOEShape? shape = null)
    {
        Predicted.Add((shape ?? ShapeMain, source, rotation, activation ?? WorldState.FutureTime(PredictionDelay)));
    }

    protected void Predict(Actor source, DateTime? activation = null, AOEShape? shape = null) => Predict(source.Position, source.Rotation, activation, shape);
}

class P2CrimsonCyclone1(BossModule module) : CC2(module, 5.2f)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Ifrit && id == 0x1E43)
            Predict(actor);
    }
}

class P2CrimsonCyclone2(BossModule module) : CC2(module, 5.2f)
{
    readonly List<Actor> Sources = [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Ifrit && id == 0x1E43)
        {
            Sources.Add(actor);
            if (Sources.Count == 4 && Module.FindComponent<P2Nails>() is { KilledOrder: var ko })
            {
                Sources.SortBy(s => ko.FindIndex(k => k.InRect(s.Position, s.Rotation, 500, 0, 3)));
                var next = WorldState.FutureTime(PredictionDelay);
                foreach (var src in Sources)
                {
                    Predict(src, next);
                    if (src.FindStatus(SID.Woken) != null)
                    {
                        var rotation = src.Rotation + 45.Degrees();
                        Predict(Arena.Center, rotation, next.AddSeconds(2.2f), ShapeCross);
                        Predict(Arena.Center, rotation + 90.Degrees(), next.AddSeconds(2.2f), ShapeCross);
                    }

                    next = next.AddSeconds(1.4f);
                }
            }
        }
    }
}
