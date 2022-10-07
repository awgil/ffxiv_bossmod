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
            if (_autorot.ClassActions == null)
                return;

            _tree.LeafNode($"Primary target: {_autorot.PrimaryTarget}");
            _tree.LeafNode($"Secondary target: {_autorot.SecondaryTarget}");
            foreach (var n in _tree.Node("Potential targets", _autorot.Hints.PotentialTargets.Count == 0))
            {
                _tree.LeafNodes(_autorot.Hints.PotentialTargets, e => $"[{e.Priority}] {e.Actor} (TTL={e.TimeToKill:f2}, str={e.AttackStrength:f2}), dist={(e.Actor.Position - _autorot.ClassActions.Player.Position).Length():f2}, tank={e.TankAffinity}/{e.DesiredPosition}/{e.DesiredRotation}");
            }
            foreach (var n in _tree.Node("Forbidden zones", _autorot.Hints.ForbiddenZones.Count == 0))
            {
                foreach (var (shape, _) in _tree.Nodes(_autorot.Hints.ForbiddenZones, z => new($"Activated at {Math.Max(0, (z.activation - _autorot.WorldState.CurrentTime).TotalSeconds):f3}", true)))
                {
                    var map = _autorot.Hints.Bounds.BuildMap();
                    map.BlockPixelsInside(shape, 0, NavigationDecision.DefaultForbiddenZoneCushion);
                    new MapVisualizer(map, 0, _autorot.ClassActions.Player.Position).Draw();
                }
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
                var targetEnemy = _autorot.Hints.PotentialTargets.FirstOrDefault(e => e.Actor == _autorot.PrimaryTarget);
                var targeting = targetEnemy != null ? _autorot.ClassActions.SelectBetterTarget(targetEnemy) : new();
                var navi = BuildPathfind(_autorot.ClassActions, targeting);
                if (navi.Map == null)
                {
                    navi.Map = _autorot.Hints.Bounds.BuildMap();
                    var imm = NavigationDecision.ImminentExplosionTime(_autorot.WorldState.CurrentTime);
                    foreach (var (shape, activation) in _autorot.Hints.ForbiddenZones)
                        NavigationDecision.AddBlockerZone(navi.Map, imm, activation, shape, NavigationDecision.DefaultForbiddenZoneCushion);
                    if (targetEnemy != null)
                        navi.MapGoal = NavigationDecision.AddTargetGoal(navi.Map, targetEnemy.Actor.Position, targetEnemy.Actor.HitboxRadius + _autorot.ClassActions.Player.HitboxRadius + targeting.PreferredRange, targetEnemy.Actor.Rotation, targeting.PreferredPosition, 0);
                }
                new MapVisualizer(navi.Map, navi.MapGoal, _autorot.ClassActions.Player.Position).Draw();
            }
        }

        private NavigationDecision BuildPathfind(CommonActions actions, CommonActions.Targeting targeting)
        {
            if (targeting.Target == null)
                return NavigationDecision.Build(_autorot.WorldState, _autorot.Hints, actions.Player, null, 0, new(), Positional.Any);

            var adjRange = targeting.PreferredRange + actions.Player.HitboxRadius + targeting.Target.Actor.HitboxRadius;
            if (targeting.PreferTanking)
            {
                // see whether we need to move target
                // TODO: think more about keeping uptime while tanking, this is tricky...
                var desiredToTarget = targeting.Target.Actor.Position - targeting.Target.DesiredPosition;
                if (desiredToTarget.LengthSq() > 4 && actions.GetState().GCD > 0.5f)
                {
                    var dest = targeting.Target.DesiredPosition - adjRange * desiredToTarget.Normalized();
                    return NavigationDecision.Build(_autorot.WorldState, _autorot.Hints, actions.Player, dest, 0.5f, new(), Positional.Any);
                }
            }

            var adjRotation = targeting.PreferTanking ? targeting.Target.DesiredRotation : targeting.Target.Actor.Rotation;
            return NavigationDecision.Build(_autorot.WorldState, _autorot.Hints, actions.Player, targeting.Target.Actor.Position, adjRange, adjRotation, targeting.PreferredPosition);
        }
    }
}
