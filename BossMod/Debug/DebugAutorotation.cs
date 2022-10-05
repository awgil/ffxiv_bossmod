using BossMod.Pathfinding;
using System;
using System.Linq;

namespace BossMod
{
    class DebugAutorotation
    {
        private Autorotation _autorot;
        private UITree _tree = new();

        public DebugAutorotation(Autorotation autorot)
        {
            _autorot = autorot;
        }

        public void Draw()
        {
            _tree.LeafNode($"Primary target: {_autorot.PrimaryTarget}");
            _tree.LeafNode($"Secondary target: {_autorot.SecondaryTarget}");
            foreach (var n in _tree.Node("Potential targets", _autorot.Hints.PotentialTargets.Count == 0))
            {
                _tree.LeafNodes(_autorot.Hints.PotentialTargets, e => $"[{e.Priority}] {e.Actor} (TTL={e.TimeToKill:f2}, str={e.AttackStrength:f2}), dist={(e.Actor.Position - _autorot.WorldState.Party.Player()?.Position ?? new()).Length():f2}");
            }
            foreach (var n in _tree.Node("Forbidden zones", _autorot.Hints.ForbiddenZones.Count == 0))
            {
                _tree.LeafNodes(_autorot.Hints.ForbiddenZones, z => $"{z.shape} @ {z.origin} rot={z.rot}, at {Math.Max(0, (z.activation - _autorot.WorldState.CurrentTime).TotalSeconds):f3}");
            }
            foreach (var n in _tree.Node("Forbidden directions", _autorot.Hints.ForbiddenDirections.Count == 0))
            {
                _tree.LeafNodes(_autorot.Hints.ForbiddenDirections, d => $"{d.center} +- {d.halfWidth}, at {Math.Max(0, (d.activation - _autorot.WorldState.CurrentTime).TotalSeconds):f3}");
            }
            foreach (var n in _tree.Node("Predicted damage", _autorot.Hints.PredictedDamage.Count == 0))
            {
                _tree.LeafNodes(_autorot.Hints.PredictedDamage, d => $"[{string.Join(", ", _autorot.WorldState.Party.WithSlot().IncludedInMask(d.players).Select(ia => ia.Item2.Name))}], at {Math.Max(0, (d.activation - _autorot.WorldState.CurrentTime).TotalSeconds):f3}");
            }
            foreach (var n in _tree.Node("Pathfinding"))
            {
                var player = _autorot.WorldState.Party.Player();
                if (player != null)
                {
                    var map = _autorot.Hints.Bounds.BuildMap();
                    var imm = NavigationDecision.ImminentExplosionTime(_autorot.WorldState.CurrentTime);
                    foreach (var z in _autorot.Hints.ForbiddenZones)
                        NavigationDecision.AddBlockerZone(map, imm, z.activation, z.shape.Distance(z.origin, z.rot));
                    int goal = 0;
                    if (_autorot.PrimaryTarget != null && _autorot.ClassActions != null)
                    {
                        var tgt = _autorot.ClassActions.SelectBetterTarget(_autorot.PrimaryTarget);
                        if (tgt.Target != null)
                            goal = NavigationDecision.AddTargetGoal(map, tgt.Target.Position, tgt.Target.HitboxRadius + player.HitboxRadius + tgt.PreferredRange, tgt.Target.Rotation, tgt.PreferredPosition, 0);
                    }
                    new MapVisualizer(map, goal, player.Position).Draw();
                }
            }
        }
    }
}
