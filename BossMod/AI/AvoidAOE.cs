using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.AI
{
    // utility that calculates position based on ai hints and desired range/positional
    class AvoidAOE
    {
        public ClipperLib.PolyTree SafeZone { get; private set; } = new();
        public ClipperLib.PolyTree DesiredZone { get; private set; } = new();
        private WPos? _desiredTargetPos;
        private Angle _desiredTargetRot;
        private float _desiredMaxRange;
        private CommonActions.Positional _desiredPositional;
        private WPos? _prevResult;

        public void SetDesired(WPos targetPosition, Angle targetRotation, float maxRange, CommonActions.Positional positional = CommonActions.Positional.Any)
        {
            if (_desiredTargetPos == targetPosition && _desiredTargetRot == targetRotation && _desiredMaxRange == maxRange && _desiredPositional == positional)
                return;
            _desiredTargetPos = targetPosition;
            _desiredTargetRot = targetRotation;
            _desiredMaxRange = maxRange;
            _desiredPositional = positional;
            DesiredZone = new Clip2D().Union(DesiredContour(targetPosition, 0.5f));
        }

        public void ClearDesired()
        {
            if (_desiredTargetPos == null)
                return;
            _desiredTargetPos = null;
            _desiredTargetRot = new();
            _desiredMaxRange = 0;
            _desiredPositional = CommonActions.Positional.Any;
            DesiredZone = new();
        }

        // TODO: add position deadline
        public WPos? Update(Actor player, AIHints hints)
        {
            // update forbidden zone
            var clipper = new Clip2D();
            SafeZone = clipper.Simplify(BuildInitialSafeZone(hints));
            if (hints.RestrictedZones.Count > 0)
            {
                ClipperLib.PolyTree union = new();
                foreach (var zone in hints.RestrictedZones)
                    union = clipper.Union(union, zone.shape.Contour(zone.origin, zone.rot, -1, 0.5f));
                SafeZone = clipper.Intersect(union, SafeZone);
            }
            foreach (var zone in hints.ForbiddenZones)
            {
                SafeZone = clipper.Difference(SafeZone, zone.shape.Contour(zone.origin, zone.rot, 1, 0.5f));
            }

            return SelectDestinationPos(player);
        }

        private IEnumerable<IEnumerable<WPos>> BuildInitialSafeZone(AIHints hints)
        {
            // TODO: this is really basic, consider improving...
            var maxImminentKnockback = hints.ForcedMovements.Count > 0 ? MathF.Sqrt(hints.ForcedMovements.Max(x => x.move.LengthSq())) : 0;
            if (maxImminentKnockback >= hints.Bounds.HalfSize)
                maxImminentKnockback = 0;
            yield return hints.Bounds.BuildClipPoly(-(maxImminentKnockback + 1));
        }

        private WPos? SelectDestinationPos(Actor player)
        {
            if (SafeZone.ChildCount > 0)
            {
                if (DesiredZone.ChildCount > 0)
                {
                    var optimal = new Clip2D().Intersect(SafeZone, DesiredZone);
                    if (optimal.ChildCount > 0)
                    {
                        return SelectPointInZone(optimal, player.Position);
                    }
                }
                // desired zone is either empty or does not intersect safe zone - select any point in safe zone
                return SelectPointInZone(SafeZone, player.Position);
            }
            else
            {
                // safe zone is empty - nothing to do but try to stay in desired zone...
                return DesiredZone.ChildCount > 0 ? SelectPointInZone(DesiredZone, player.Position) : null;
            }
        }

        private WPos? SelectPointInZone(ClipperLib.PolyTree zone, WPos currentPosition)
        {
            var curNode = Clip2D.FindNodeContainingPoint(zone, currentPosition);
            if (!curNode.IsHole)
            {
                // we're in zone already, clear destination
                _prevResult = null;
            }
            else if (_prevResult == null || Clip2D.FindNodeContainingPoint(zone, _prevResult.Value).IsHole)
            {
                // we need to select new destination for movement
                _prevResult = FindClosestPointToNode(currentPosition, curNode);
            }
            // else: previously selected destination is still good
            return _prevResult;
        }

        private IEnumerable<IEnumerable<WPos>> DesiredContour(WPos center, float tolerance)
        {
            switch (_desiredPositional)
            {
                case CommonActions.Positional.Flank:
                    var left = _desiredTargetRot.ToDirection().OrthoL();
                    yield return CurveApprox.CircleSector(center + tolerance * 1.4142f * left, _desiredMaxRange - tolerance * 2.4142f, _desiredTargetRot + 45.Degrees(), _desiredTargetRot + 135.Degrees(), 1);
                    yield return CurveApprox.CircleSector(center - tolerance * 1.4142f * left, _desiredMaxRange - tolerance * 2.4142f, _desiredTargetRot - 45.Degrees(), _desiredTargetRot - 135.Degrees(), 1);
                    break;
                case CommonActions.Positional.Rear:
                    yield return CurveApprox.CircleSector(center - tolerance * 1.4142f * _desiredTargetRot.ToDirection(), _desiredMaxRange - tolerance * 2.4142f, _desiredTargetRot + 135.Degrees(), _desiredTargetRot + 225.Degrees(), 1);
                    break;
                default:
                    yield return CurveApprox.Circle(center, _desiredMaxRange - 0.5f, 1);
                    break;
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
    }
}
