using System;
using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P3Geocrush1 : Components.SelfTargetedAOEs
    {
        public P3Geocrush1() : base(ActionID.MakeSpell(AID.Geocrush1), new AOEShapeCircle(18)) { }
    }

    class P3Geocrush2 : Components.GenericAOEs
    {
        private Actor? _caster;
        private Angle? _prevPredictedAngle;
        private WPos? _predictedPos;
        private DateTime _activation;

        private static WDir[] _possibleOffsets = { new(14, 0), new(0, 14), new(-14, 0), new(0, -14) };
        private static AOEShapeCircle _shape = new(25); // TODO: verify falloff

        public P3Geocrush2() : base(ActionID.MakeSpell(AID.Geocrush2)) { }

        public override void Init(BossModule module)
        {
            _caster = ((UWU)module).Titan();
            _activation = module.WorldState.CurrentTime.AddSeconds(5.2f);
        }

        public override void Update(BossModule module)
        {
            if (_caster == null)
                return;

            // TODO: find a way to get non-interpolated actor rotation...
            if (_prevPredictedAngle == _caster.Rotation)
            {
                var dir = _caster.Rotation.ToDirection();
                _predictedPos = module.Bounds.Center + _possibleOffsets.MinBy(o =>
                {
                    var off = module.Bounds.Center + o - _caster.Position;
                    return (off - off.Dot(dir) * dir).LengthSq();
                });
            }
            else
            {
                _prevPredictedAngle = _caster.Rotation; // retry on next frame
            }
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_caster == null)
                yield break;
            if (_caster.CastInfo != null)
                yield return (_shape, _caster.Position, 0.Degrees(), _caster.CastInfo.FinishAt);
            else if (_predictedPos != null)
                yield return (_shape, _predictedPos.Value, 0.Degrees(), _activation);
        }
    }
}
