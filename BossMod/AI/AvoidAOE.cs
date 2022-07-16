using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.AI
{
    // utility that calculates forbidden zone (union of pending aoes) based on actor casts
    class AvoidAOE : IDisposable
    {
        private class ActorState
        {
            public Actor Actor;
            public AOEShape Shape;
            public WPos Origin;
            // this bs is needed only because caster can rotate for some time after cast start...
            public Angle? CasterRot;
            public int NumFramesUnmoving;

            public bool IncludedInResult => CasterRot == null || NumFramesUnmoving >= 2;

            public ActorState(Actor a, AOEShape shape, WPos origin)
            {
                Actor = a;
                Shape = shape;
                Origin = origin;
                CasterRot = shape is AOEShapeCone || shape is AOEShapeRect ? a.Rotation : null;
            }
        }

        public ClipperLib.PolyTree ForbiddenZone { get; private set; } = new();
        public ClipperLib.PolyTree DesiredZone { get; private set; } = new();
        private WorldState _ws;
        private Clip2D _clipper = new();
        private Dictionary<ulong, ActorState> _states = new();
        private bool _forbiddenDirty;
        private bool _desiredDirty;
        private WPos? _desiredTargetPos;
        private Angle _desiredTargetRot;
        private float _desiredMaxRange;
        private CommonActions.Positional _desiredPositional;
        private WPos _prevPos;
        private WPos? _cachedResult;

        public AvoidAOE(WorldState ws)
        {
            _ws = ws;
            _ws.Actors.CastStarted += OnCastStarted;
            _ws.Actors.CastFinished += OnCastFinished;
        }

        public void Dispose()
        {
            _ws.Actors.CastStarted -= OnCastStarted;
            _ws.Actors.CastFinished -= OnCastFinished;
        }

        public void SetDesired(WPos? targetPosition, Angle targetRotation, float maxRange, CommonActions.Positional positional = CommonActions.Positional.Any)
        {
            if (_desiredTargetPos == targetPosition && _desiredTargetRot == targetRotation && _desiredMaxRange == maxRange && _desiredPositional == positional)
                return;
            _desiredTargetPos = targetPosition;
            _desiredTargetRot = targetRotation;
            _desiredMaxRange = maxRange;
            _desiredPositional = positional;
            _desiredDirty = true;
        }

        public WPos? Update(WPos currentPosition)
        {
            // check for rotated casters (this is cancer)
            foreach (var s in _states.Values)
            {
                if (s.CasterRot == null)
                    continue;

                var curRot = s.Actor.Rotation;
                if (curRot == s.CasterRot.Value)
                {
                    var wasIncluded = s.IncludedInResult;
                    ++s.NumFramesUnmoving;
                    _forbiddenDirty |= !wasIncluded && s.IncludedInResult;
                }
                else
                {
                    _forbiddenDirty |= s.IncludedInResult;
                    s.CasterRot = curRot;
                    s.NumFramesUnmoving = 0;
                }
            }

            // recalculate forbidden zone, if needed
            if (_forbiddenDirty)
            {
                _forbiddenDirty = false;
                _desiredDirty = true;
                ForbiddenZone = _clipper.Union(ActiveAOEs());
            }

            // recalculate desired zone, if needed
            bool cachedDirty = _desiredDirty || _prevPos != currentPosition;
            if (_desiredDirty)
            {
                _desiredDirty = false;
                DesiredZone = _desiredTargetPos != null && _desiredMaxRange > 0 ? _clipper.Difference(DesiredContour(_desiredTargetPos.Value), ForbiddenZone) : new();
            }

            if (cachedDirty)
            {
                _prevPos = currentPosition;
                // check whether current position is good, and if not, return closest safespot
                if (DesiredZone.ChildCount > 0)
                {
                    var desiredNode = Clip2D.FindNodeContainingPoint(DesiredZone, currentPosition);
                    if (!desiredNode.IsHole)
                    {
                        // we're inside desired zone now
                        _cachedResult = null;
                    }
                    else if (_cachedResult == null || Clip2D.FindNodeContainingPoint(DesiredZone, _cachedResult.Value).IsHole)
                    {
                        _cachedResult = FindClosestPointToNode(currentPosition, desiredNode);
                    }
                    // else: cached result is available and is in desired zone, continue moving there
                }
                else
                {
                    var forbiddenNode = Clip2D.FindNodeContainingPoint(ForbiddenZone, currentPosition);
                    if (forbiddenNode.IsHole)
                    {
                        // we're outside forbidden zone now
                        _cachedResult = null;
                    }
                    else if (_cachedResult == null || !Clip2D.FindNodeContainingPoint(ForbiddenZone, _cachedResult.Value).IsHole)
                    {
                        _cachedResult = FindClosestPointToNode(currentPosition, forbiddenNode);
                    }
                    // else: cached result is available and is outside forbidden zone, continue moving there
                }
            }
            return _cachedResult;
        }

        private IEnumerable<IEnumerable<WPos>> ActiveAOEs()
        {
            foreach (var s in _states.Values.Where(s => s.IncludedInResult))
            {
                yield return s.Shape.Contour(s.Origin, s.CasterRot ?? 0.Degrees());
            }
        }

        private IEnumerable<WPos> DesiredContour(WPos center)
        {
            switch (_desiredPositional)
            {
                case CommonActions.Positional.Flank:
                    return CurveApprox.CircleSector(center, _desiredMaxRange, _desiredTargetRot + 45.Degrees(), _desiredTargetRot + 135.Degrees(), 1).Concat(CurveApprox.CircleSector(center, _desiredMaxRange, _desiredTargetRot - 45.Degrees(), _desiredTargetRot - 135.Degrees(), 1));
                case CommonActions.Positional.Rear:
                    return CurveApprox.CircleSector(center, _desiredMaxRange, _desiredTargetRot + 135.Degrees(), _desiredTargetRot + 225.Degrees(), 1);
                default:
                    return CurveApprox.Circle(center, _desiredMaxRange, 1);
            }
        }

        private WPos FindClosestPointToNode(WPos origin, ClipperLib.PolyNode node)
        {
            var (closest, closestDist) = FindClosestPointToContour(origin, node);
            foreach (var hole in node.Childs.Where(o => !o.IsOpen && o.Contour.Count > 0))
            {
                var (holeClosest, holeClosestDist) = FindClosestPointToContour(origin, hole);
                if (holeClosestDist < closestDist)
                {
                    closest = holeClosest;
                    closestDist = holeClosestDist;
                }
            }
            return closest;
        }

        private (WPos, float) FindClosestPointToContour(WPos origin, ClipperLib.PolyNode node)
        {
            var best = origin;
            var bestDistSq = float.MaxValue;
            foreach (var (a, b) in Clip2D.Contour(node))
            {
                var ab = b - a;
                var len = ab.Length();
                var v = ab / len;
                var proj = (origin - a).Dot(v);
                proj = Math.Clamp(proj, 0, len);
                var intersect = a + v * proj;
                var distSq = (origin - intersect).LengthSq();
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = intersect;
                }
            }
            return (best, bestDistSq);
        }

        private Angle DetermineConeAngle(Lumina.Excel.GeneratedSheets.Action data)
        {
            var omen = data.Omen.Value;
            if (omen == null)
            {
                Service.Log($"[AvoidAOE] No omen data for {data.RowId} '{data.Name}'...");
                return 180.Degrees();
            }
            var path = omen.Path.ToString();
            var pos = path.IndexOf("fan");
            if (pos < 0 || pos + 6 > path.Length)
            {
                Service.Log($"[AvoidAOE] Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
                return 180.Degrees();
            }
            return int.Parse(path.Substring(pos + 3, 3)).Degrees();
        }

        private void OnCastStarted(object? sender, Actor actor)
        {
            var data = actor.CastInfo!.IsSpell() ? Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(actor.CastInfo.Action.ID) : null;
            if (data == null || data.CastType == 1)
                return;
            AOEShape? shape = data.CastType switch
            {
                2 or 5 or 7 => new AOEShapeCircle(data.EffectRange),
                3 or 13 => new AOEShapeCone(data.EffectRange, DetermineConeAngle(data) * 0.5f),
                4 or 12 => new AOEShapeRect(data.EffectRange + 2, (data.XAxisModifier + 1) * 0.5f, 1),
                10 => new AOEShapeDonut(actor.HitboxRadius, data.EffectRange), // TODO: find a way to determine inner radius
                _ => null
            };
            if (shape == null)
            {
                Service.Log($"[AvoidAOE] Unknown cast type {data.CastType} for {actor.CastInfo.Action}");
                return;
            }
            var target = _ws.Actors.Find(actor.CastInfo.TargetID)?.Position ?? actor.CastInfo.LocXZ;
            var s = _states[actor.InstanceID] = new(actor, shape, target);
            _forbiddenDirty |= s.IncludedInResult;
        }

        private void OnCastFinished(object? sender, Actor actor)
        {
            var s = _states.GetValueOrDefault(actor.InstanceID);
            if (s == null)
                return;
            _forbiddenDirty |= s.IncludedInResult;
            _states.Remove(actor.InstanceID);
        }
    }
}
