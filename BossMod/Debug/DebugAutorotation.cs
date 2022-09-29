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
            foreach (var n in _tree.Node("Potential targets"))
            {
                _tree.LeafNodes(_autorot.Hints.PotentialTargets, e => $"[{e.Priority}] {e.Actor} (TTL={e.TimeToKill:f2}), dist={(e.Actor.Position - _autorot.WorldState.Party.Player()?.Position ?? new()).Length():f2}");
            }
        }
    }
}
