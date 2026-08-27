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
