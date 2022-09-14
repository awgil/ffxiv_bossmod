using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.AI
{
    // utility that determines active aoes automatically based on actor casts
    // this is used e.g. in outdoor or on trash, where we have no active bossmodules
    public class AutoAOEs : IDisposable
    {
        private WorldState _ws;
        private Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape, bool IsCharge)> _activeAOEs = new();

        public AutoAOEs(WorldState ws)
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

        public AIHints CalculateAIHints()
        {
            var hints = new AIHints();
            foreach (var aoe in _activeAOEs.Values)
            {
                var target = aoe.Target?.Position ?? aoe.Caster.CastInfo!.LocXZ;
                var rot = aoe.Caster.CastInfo!.Rotation;
                if (aoe.IsCharge)
                {
                    var shape = (AOEShapeRect)aoe.Shape;
                    shape.SetEndPoint(target, aoe.Caster.Position, rot);
                    hints.ForbiddenZones.Add((shape, aoe.Caster.Position, rot, aoe.Caster.CastInfo.FinishAt));
                }
                else
                {
                    hints.ForbiddenZones.Add((aoe.Shape, target, rot, aoe.Caster.CastInfo.FinishAt));
                }
            }
            return hints;
        }

        private void OnCastStarted(object? sender, Actor actor)
        {
            if (actor.Type != ActorType.Enemy || actor.IsAlly)
                return;
            var data = actor.CastInfo!.IsSpell() ? Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(actor.CastInfo.Action.ID) : null;
            if (data == null || data.CastType == 1)
                return;
            AOEShape? shape = data.CastType switch
            {
                2 => new AOEShapeCircle(data.EffectRange), // used for some point-blank aoes and enemy location-targeted - does not add caster hitbox
                3 => new AOEShapeCone(data.EffectRange + actor.HitboxRadius, DetermineConeAngle(data) * 0.5f),
                4 => new AOEShapeRect(data.EffectRange + actor.HitboxRadius, data.XAxisModifier * 0.5f),
                5 => new AOEShapeCircle(data.EffectRange + actor.HitboxRadius),
                //6 => ???
                //7 => new AOEShapeCircle(data.EffectRange), - used for player ground-targeted circles a-la asylum
                //8 => charge rect
                //10 => new AOEShapeDonut(actor.HitboxRadius, data.EffectRange), // TODO: find a way to determine inner radius (omen examples: 28762 - 4/40 - gl_sircle_4004bp1)
                //11 => cross == 12 + another 12 rotated 90 degrees
                12 => new AOEShapeRect(data.EffectRange, data.XAxisModifier * 0.5f),
                13 => new AOEShapeCone(data.EffectRange, DetermineConeAngle(data) * 0.5f),
                _ => null
            };
            if (shape == null)
            {
                Service.Log($"[AutoAOEs] Unknown cast type {data.CastType} for {actor.CastInfo.Action}");
                return;
            }
            var target = _ws.Actors.Find(actor.CastInfo.TargetID);
            _activeAOEs[actor.InstanceID] = (actor, target, shape, data.CastType == 8);
        }

        private void OnCastFinished(object? sender, Actor actor)
        {
            _activeAOEs.Remove(actor.InstanceID);
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
    }

    // utility that calculates forbidden zone (union of pending aoes) based on actor casts
    class AvoidAOE : IDisposable
    {
        public ClipperLib.PolyTree SafeZone { get; private set; } = new();
        public ClipperLib.PolyTree DesiredZone { get; private set; } = new();
        private BossModuleManager _bmm;
        private AutoAOEs _autoAOEs;
        private WPos? _desiredTargetPos;
        private Angle _desiredTargetRot;
        private float _desiredMaxRange;
        private CommonActions.Positional _desiredPositional;
        private WPos? _prevResult;

        public AvoidAOE(BossModuleManager bmm)
        {
            _bmm = bmm;
            _autoAOEs = new(bmm.WorldState);
        }

        public void Dispose()
        {
            _autoAOEs.Dispose();
        }

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
        public (WPos? DestPos, WDir? DestRot, DateTime RotDeadline) Update(Actor player)
        {
            var aiHints = _bmm.ActiveModule?.StateMachine.ActiveState != null ? _bmm.ActiveModule.CalculateAIHints(PartyState.PlayerSlot, player) : _autoAOEs.CalculateAIHints();

            // update forbidden zone
            var clipper = new Clip2D();
            SafeZone = clipper.Simplify(BuildInitialSafeZone(player.Position, aiHints));
            if (aiHints.RestrictedZones.Count > 0)
            {
                ClipperLib.PolyTree union = new();
                foreach (var zone in aiHints.RestrictedZones)
                    union = clipper.Union(union, zone.shape.Contour(zone.origin, zone.rot, -1, 0.5f));
                SafeZone = clipper.Intersect(union, SafeZone);
            }
            foreach (var zone in aiHints.ForbiddenZones)
            {
                SafeZone = clipper.Difference(SafeZone, zone.shape.Contour(zone.origin, zone.rot, 1, 0.5f));
            }

            var (destRot, rotDeadline) = SelectAllowedRotation(player, aiHints);
            return (SelectDestinationPos(player), destRot, rotDeadline);
        }

        private IEnumerable<IEnumerable<WPos>> BuildInitialSafeZone(WPos playerPos, AIHints hints)
        {
            if (_bmm.ActiveModule?.StateMachine.ActiveState != null)
            {
                // TODO: this is really basic, consider improving...
                var maxImminentKnockback = hints.ForcedMovements.Count > 0 ? MathF.Sqrt(hints.ForcedMovements.Max(x => x.move.LengthSq())) : 0;
                yield return _bmm.ActiveModule.Bounds.BuildClipPoly(-(maxImminentKnockback + 1));
            }
            else
            {
                yield return DefaultBounds(playerPos);
            }
        }

        public static IEnumerable<WPos> DefaultBounds(WPos center)
        {
            var s = 1000;
            yield return center + new WDir(s, -s);
            yield return center + new WDir(s, s);
            yield return center + new WDir(-s, s);
            yield return center + new WDir(-s, -s);
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

        private (WDir?, DateTime) SelectAllowedRotation(Actor player, AIHints hints)
        {
            if (hints.ForbiddenDirections.Count == 0)
                return (null, new());

            var earliest = hints.ForbiddenDirections.Min(d => d.activation);
            if (earliest < _bmm.WorldState.CurrentTime)
                earliest = _bmm.WorldState.CurrentTime;
            var latest = earliest.AddSeconds(1);

            DisjointSegmentList list = new();
            foreach (var d in hints.ForbiddenDirections.Where(d => d.activation <= latest))
            {
                var center = d.center.Normalized();
                var min = center - d.halfWidth;
                if (min.Rad < -MathF.PI)
                {
                    list.Add(min.Rad + 2 * MathF.PI, MathF.PI);
                }
                var max = center + d.halfWidth;
                if (max.Rad > MathF.PI)
                {
                    list.Add(-MathF.PI, max.Rad - 2 * MathF.PI);
                    max = MathF.PI.Radians();
                }
                list.Add(min.Rad, max.Rad);
            }

            if (!list.Contains(player.Rotation.Rad))
                return (null, new()); // all good

            if (_desiredTargetPos != null)
            {
                var toTarget = Angle.FromDirection(_desiredTargetPos.Value - player.Position);
                if (!list.Contains(toTarget.Rad))
                    return (toTarget.ToDirection(), earliest);
            }

            // select midpoint of largest allowed segment
            float bestWidth = list.Segments.First().Min + 2 * MathF.PI - list.Segments.Last().Max;
            float bestMidpoint = (list.Segments.First().Min + 2 * MathF.PI + list.Segments.Last().Max) / 2;
            for (int i = 1; i < list.Segments.Count; ++i)
            {
                float width = list.Segments[i].Min - list.Segments[i - 1].Max;
                if (width > bestWidth)
                {
                    bestWidth = width;
                    bestMidpoint = (list.Segments[i].Min + list.Segments[i - 1].Max) / 2;
                }
            }
            return (bestMidpoint.Radians().ToDirection(), earliest);
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
