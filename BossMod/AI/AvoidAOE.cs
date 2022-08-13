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
        private Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape)> _activeAOEs = new();

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

        public SafeZone CalculateSafeZone(WPos playerPos)
        {
            var zone = new SafeZone(playerPos);
            foreach (var aoe in _activeAOEs.Values)
                zone.ForbidZone(aoe.Shape, aoe.Target?.Position ?? aoe.Caster.CastInfo!.LocXZ, aoe.Caster.Rotation, aoe.Caster.CastInfo!.FinishAt);
            return zone;
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
                //7 => new AOEShapeCircle(data.EffectRange), - used for player ground-targeted circles a-la asylum
                //10 => new AOEShapeDonut(actor.HitboxRadius, data.EffectRange), // TODO: find a way to determine inner radius (omen examples: 28762 - 4/40 - gl_sircle_4004bp1)
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
            _activeAOEs[actor.InstanceID] = (actor, target, shape);
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
            // update forbidden zone
            var z = _bmm.ActiveModule?.StateMachine.ActiveState != null ? _bmm.ActiveModule.CalculateSafeZone(PartyState.PlayerSlot, player) : _autoAOEs.CalculateSafeZone(player.Position);
            SafeZone = z.Result;
            return (SelectDestinationPos(player), SelectAllowedRotation(player, z), z.ForbiddenRotationsActivation);
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

        private WDir? SelectAllowedRotation(Actor player, SafeZone z)
        {
            if (!z.ForbiddenRotations.Contains(player.Rotation.Rad))
                return null; // all good

            if (_desiredTargetPos != null)
            {
                var toTarget = Angle.FromDirection(_desiredTargetPos.Value - player.Position);
                if (!z.ForbiddenRotations.Contains(toTarget.Rad))
                    return toTarget.ToDirection();
            }

            // select midpoint of largest allowed segment
            float bestWidth = z.ForbiddenRotations.Segments.First().Min + 2 * MathF.PI - z.ForbiddenRotations.Segments.Last().Max;
            float bestMidpoint = (z.ForbiddenRotations.Segments.First().Min + 2 * MathF.PI + z.ForbiddenRotations.Segments.Last().Max) / 2;
            for (int i = 1; i < z.ForbiddenRotations.Segments.Count; ++i)
            {
                float width = z.ForbiddenRotations.Segments[i].Min - z.ForbiddenRotations.Segments[i - 1].Max;
                if (width > bestWidth)
                {
                    bestWidth = width;
                    bestMidpoint = (z.ForbiddenRotations.Segments[i].Min + z.ForbiddenRotations.Segments[i - 1].Max) / 2;
                }
            }
            return bestMidpoint.Radians().ToDirection();
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
