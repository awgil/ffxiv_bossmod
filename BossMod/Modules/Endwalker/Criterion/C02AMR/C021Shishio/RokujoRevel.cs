using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio
{
    class RokujoRevel : Components.GenericAOEs
    {
        private int _numBreaths;
        private List<Actor> _clouds = new();
        private List<(Angle dir, DateTime activation)> _pendingLines = new();
        private List<(WPos origin, DateTime activation)> _pendingCircles = new();

        private static AOEShapeRect _shapeLine = new(30, 7, 30);
        private static AOEShapeCircle[] _shapesCircle = { new(8), new(12), new(23) };

        private AOEShapeCircle? ShapeCircle => _numBreaths is > 0 and <= 3 ? _shapesCircle[_numBreaths - 1] : null;

        public bool Active => _pendingLines.Count + _pendingCircles.Count > 0;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_pendingLines.Count > 1)
                yield return new(_shapeLine, module.Bounds.Center, _pendingLines[1].dir, _pendingLines[1].activation, risky: false);
            if (_pendingCircles.Count > 0 && ShapeCircle is var shapeCircle && shapeCircle != null)
            {
                var firstFutureActivation = _pendingCircles[0].activation.AddSeconds(1);
                var firstFutureIndex = _pendingCircles.FindIndex(p => p.activation >= firstFutureActivation);
                if (firstFutureIndex >= 0)
                {
                    var lastFutureActivation = _pendingCircles[firstFutureIndex].activation.AddSeconds(1.5f);
                    foreach (var p in _pendingCircles.Skip(firstFutureIndex).TakeWhile(p => p.activation <= lastFutureActivation))
                        yield return new(shapeCircle, p.origin, default, p.activation, risky: false);
                }
                else
                {
                    firstFutureIndex = _pendingCircles.Count;
                }
                foreach (var p in _pendingCircles.Take(firstFutureIndex))
                    yield return new(shapeCircle, p.origin, default, p.activation, ArenaColor.Danger);
            }
            if (_pendingLines.Count > 0)
                yield return new(_shapeLine, module.Bounds.Center, _pendingLines[0].dir, _pendingLines[0].activation, ArenaColor.Danger);
        }

        public override void Init(BossModule module)
        {
            _clouds.AddRange(module.Enemies(OID.NRaiun));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NRokujoRevelAOE:
                case AID.SRokujoRevelAOE:
                    _pendingLines.Add((spell.Rotation, spell.FinishAt));
                    AddHitClouds(_clouds.InShape(_shapeLine, caster.Position, spell.Rotation), spell.FinishAt, ShapeCircle?.Radius ?? 0);
                    _pendingCircles.SortBy(p => p.activation);
                    break;
                case AID.NLeapingLevin1:
                case AID.NLeapingLevin2:
                case AID.NLeapingLevin3:
                case AID.SLeapingLevin1:
                case AID.SLeapingLevin2:
                case AID.SLeapingLevin3:
                    var index = _pendingCircles.FindIndex(p => p.origin.AlmostEqual(caster.Position, 1));
                    if (index < 0)
                    {
                        module.ReportError(this, $"Failed to predict levin from {caster.InstanceID:X}");
                        _pendingCircles.Add((caster.Position, spell.FinishAt));
                    }
                    else if (Math.Abs((_pendingCircles[index].activation - spell.FinishAt).TotalSeconds) > 1)
                    {
                        module.ReportError(this, $"Mispredicted levin from {caster.InstanceID:X} by {(_pendingCircles[index].activation - spell.FinishAt).TotalSeconds}");
                    }
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NSmokeaterFirst:
                case AID.NSmokeaterRest:
                case AID.SSmokeaterFirst:
                case AID.SSmokeaterRest:
                    ++_numBreaths;
                    break;
                case AID.NSmokeaterAbsorb:
                case AID.SSmokeaterAbsorb:
                    _clouds.Remove(caster);
                    break;
                case AID.NRokujoRevelAOE:
                case AID.SRokujoRevelAOE:
                    if (_pendingLines.Count > 0)
                        _pendingLines.RemoveAt(0);
                    ++NumCasts;
                    break;
                case AID.NLeapingLevin1:
                case AID.NLeapingLevin2:
                case AID.NLeapingLevin3:
                case AID.SLeapingLevin1:
                case AID.SLeapingLevin2:
                case AID.SLeapingLevin3:
                    _pendingCircles.RemoveAll(p => p.origin.AlmostEqual(caster.Position, 1));
                    ++NumCasts;
                    break;
            }
        }

        private void AddHitClouds(IEnumerable<Actor> clouds, DateTime timeHit, float radius)
        {
            var explodeTime = timeHit.AddSeconds(1.1f);
            foreach (var c in clouds)
            {
                var existing = _pendingCircles.FindIndex(p => p.origin.AlmostEqual(c.Position, 1));
                if (existing >= 0 && _pendingCircles[existing].activation <= explodeTime)
                    continue; // this cloud is already going to be hit by some other earlier aoe

                if (existing < 0)
                    _pendingCircles.Add((c.Position, explodeTime));
                else
                    _pendingCircles[existing] = (c.Position, explodeTime);
                AddHitClouds(_clouds.InRadiusExcluding(c, radius), explodeTime, radius);
            }
        }
    }
}
