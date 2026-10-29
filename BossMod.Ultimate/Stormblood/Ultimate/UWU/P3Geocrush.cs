
namespace BossMod.Stormblood.Ultimate.UWU;

class P3Geocrush1(BossModule module) : Components.StandardAOEs(module, AID.Geocrush1, new AOEShapeCircle(18));

// TODO: add prediction after PATE xxx - need non-interpolated actor rotation for that...
class P3Geocrush2(BossModule module) : Components.GenericAOEs(module, AID.Geocrush2)
{
    private Actor? _caster;
    private AOEShapeDonut? _shapeReduced;

    //private static WDir[] _possibleOffsets = { new(14, 0), new(0, 14), new(-14, 0), new(0, -14) };
    private static readonly AOEShapeCircle _shapeCrush = new(24);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster != null)
            yield return new(_shapeCrush, _caster.Position, _caster.CastInfo!.Rotation, Module.CastFinishAt(_caster.CastInfo));
        if (_shapeReduced != null)
            yield return new(_shapeReduced, Module.Center);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _caster = caster;
            _shapeReduced = new(NumCasts == 0 ? 16 : 12, Module.Bounds.Radius); // TODO: verify second radius
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _caster = null;
        }
    }

    //public override void Update()
    //{
    //    if (_caster == null || _caster.CastInfo != null)
    //        return;

    //    // TODO: find a way to get non-interpolated actor rotation...
    //    if (_prevPredictedAngle == _caster.Rotation && ++_numFramesStill > 2)
    //    {
    //        var dir = _caster.Rotation.ToDirection();
    //        _predictedPos = Module.Center + _possibleOffsets.MinBy(o =>
    //        {
    //            var off = Module.Center + o - _caster.Position;
    //            var proj = off.Dot(dir);
    //            return proj > 0 ? (off - proj * dir).LengthSq() : float.MaxValue;
    //        });
    //    }
    //    else
    //    {
    //        _prevPredictedAngle = _caster.Rotation; // retry on next frame
    //        _numFramesStill = 0;
    //    }
    //}
}

class P3GeocrushBury(BossModule module) : Components.GenericAOEs(module, AID.Bury)
{
    readonly List<AOEInstance> _predicted = [];

    public WPos? LastBoulder;

    // assuming proximity is true north
    static readonly WDir[] BombOffsets = [
        new(-5, +5),
        new(+5, +5),
        new(-5, +12),
        new(+5, +12),
        new(-11, +11),
        new(+11, +11)
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
            if (_predicted.Count == 1)
            {
                ref var last = ref _predicted.Ref(0);
                LastBoulder = last.Origin;
                last.Activation = WorldState.FutureTime(8);
                last.Risky = false;
            }
        }

        if ((AID)spell.Action.ID == AID.Burst && _predicted.Count == 1)
            _predicted.Ref(0).Risky = true;

        if ((AID)spell.Action.ID == AID.Geocrush2)
        {
            var toN = (caster.Position - Arena.Center).ToAngle() - 180.Degrees();

            foreach (var off in BombOffsets)
                _predicted.Add(new(new AOEShapeCircle(4.3f), Arena.Center + off.Rotate(toN), default, WorldState.FutureTime(5.5f)));
        }
    }
}
