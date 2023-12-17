using BossMod.Pathfinding;
using ImGuiNET;
using System;
using System.Linq;

namespace BossMod
{
    public class AIHintsVisualizer
    {
        private WorldState _ws;
        private Actor _player;
        private ulong _targetID;
        private Func<AIHints.Enemy?, (AIHints.Enemy? enemy, float range, Positional pos, bool tank)> _targetSelect;
        private AIHints _hints;
        private MapVisualizer?[] _zoneVisualizers;
        private MapVisualizer? _pathfindVisualizer;
        private NavigationDecision _navi;

        public AIHintsVisualizer(AIHints hints, WorldState ws, Actor player, ulong targetID, Func<AIHints.Enemy?, (AIHints.Enemy? enemy, float range, Positional pos, bool tank)> targetSelect)
        {
            _ws = ws;
            _player = player;
            _targetID = targetID;
            _targetSelect = targetSelect;
            _hints = hints;
            _zoneVisualizers = new MapVisualizer?[hints.ForbiddenZones.Count];
        }

        public void Draw(UITree tree)
        {
            foreach (var n in tree.Node("Potential targets", _hints.PotentialTargets.Count == 0))
            {
                tree.LeafNodes(_hints.PotentialTargets, e => $"[{e.Priority}] {e.Actor} (str={e.AttackStrength:f2}), dist={(e.Actor.Position - _player.Position).Length():f2}, tank={e.ShouldBeTanked}/{e.PreferProvoking}/{e.DesiredPosition}/{e.DesiredRotation}");
            }
            tree.LeafNode($"Forced target: {_hints.ForcedTarget}");
            foreach (var n in tree.Node("Forbidden zones", _hints.ForbiddenZones.Count == 0))
            {
                for (int i = 0; i < _hints.ForbiddenZones.Count; i++)
                {
                    foreach (var nn in tree.Node($"[{i}] activated at {Math.Max(0, (_hints.ForbiddenZones[i].activation - _ws.CurrentTime).TotalSeconds):f3}"))
                    {
                        _zoneVisualizers[i] ??= BuildZoneVisualizer(_hints.ForbiddenZones[i].shapeDistance);
                        _zoneVisualizers[i]!.Draw();
                    }
                }
            }
            foreach (var n in tree.Node("Forbidden directions", _hints.ForbiddenDirections.Count == 0))
            {
                tree.LeafNodes(_hints.ForbiddenDirections, d => $"{d.center} +- {d.halfWidth}, at {Math.Max(0, (d.activation - _ws.CurrentTime).TotalSeconds):f3}");
            }
            foreach (var n in tree.Node("Predicted damage", _hints.PredictedDamage.Count == 0))
            {
                tree.LeafNodes(_hints.PredictedDamage, d => $"[{string.Join(", ", _ws.Party.WithSlot().IncludedInMask(d.players).Select(ia => ia.Item2.Name))}], at {Math.Max(0, (d.activation - _ws.CurrentTime).TotalSeconds):f3}");
            }
            foreach (var n in tree.Node("Planned actions", _hints.PlannedActions.Count == 0))
            {
                tree.LeafNodes(_hints.PlannedActions, e => $"{e.action} @ {e.target} in {e.windowEnd:f3}s ({(e.lowPriority ? "low" : "high")} priority)");
            }
            foreach (var n in tree.Node("Pathfinding"))
            {
                _pathfindVisualizer ??= BuildPathfindingVisualizer();
                _pathfindVisualizer!.Draw();
                ImGui.TextUnformatted($"Decision: {_navi.DecisionType}, leeway={_navi.LeewaySeconds:f3}, ttg={_navi.TimeToGoal:f3}, dist={(_navi.Destination != null ? $"{(_navi.Destination.Value - _player.Position).Length():f3}" : "---")}");
            }
        }

        private MapVisualizer BuildZoneVisualizer(Func<WPos, float> shape)
        {
            var map = _hints.Bounds.BuildMap();
            map.BlockPixelsInside(shape, 0, NavigationDecision.DefaultForbiddenZoneCushion);
            return new MapVisualizer(map, 0, _player.Position);
        }

        private MapVisualizer BuildPathfindingVisualizer()
        {
            var targetEnemy = _targetID != 0 ? _hints.PotentialTargets.FirstOrDefault(e => e.Actor.InstanceID == _targetID) : null;
            var targeting = _targetSelect(targetEnemy);
            _navi = BuildPathfind(targeting.enemy, targeting.range, targeting.pos, targeting.tank);
            if (_navi.Map == null)
            {
                _navi.Map = _hints.Bounds.BuildMap();
                var imm = NavigationDecision.ImminentExplosionTime(_ws.CurrentTime);
                foreach (var (shape, activation) in _hints.ForbiddenZones)
                    NavigationDecision.AddBlockerZone(_navi.Map, imm, activation, shape, NavigationDecision.DefaultForbiddenZoneCushion);
                if (targetEnemy != null)
                    _navi.MapGoal = NavigationDecision.AddTargetGoal(_navi.Map, targetEnemy.Actor.Position, targetEnemy.Actor.HitboxRadius + _player.HitboxRadius + targeting.range, targetEnemy.Actor.Rotation, targeting.pos, 0);
            }
            return new MapVisualizer(_navi.Map, _navi.MapGoal, _player.Position);
        }

        private NavigationDecision BuildPathfind(AIHints.Enemy? target, float range, Positional positional, bool preferTanking)
        {
            if (target == null)
                return NavigationDecision.Build(_ws, _hints, _player, null, 0, new(), Positional.Any);

            var adjRange = range + _player.HitboxRadius + target.Actor.HitboxRadius;
            if (preferTanking)
            {
                // see whether we need to move target
                // TODO: think more about keeping uptime while tanking, this is tricky...
                var desiredToTarget = target.Actor.Position - target.DesiredPosition;
                if (desiredToTarget.LengthSq() > 4 /* && gcd check*/)
                {
                    var dest = target.DesiredPosition - adjRange * desiredToTarget.Normalized();
                    return NavigationDecision.Build(_ws, _hints, _player, dest, 0.5f, new(), Positional.Any);
                }
            }

            var adjRotation = preferTanking ? target.DesiredRotation : target.Actor.Rotation;
            return NavigationDecision.Build(_ws, _hints, _player, target.Actor.Position, adjRange, adjRotation, positional);
        }
    }
}
