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
        }
    }
}
