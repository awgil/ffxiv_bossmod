namespace BossMod;

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
        new AIHintsVisualizer(_autorot.Hints, _autorot.WorldState, _autorot.ClassActions.Player, _autorot.PrimaryTarget?.InstanceID ?? 0, e =>
        {
            var t = e != null ? _autorot.ClassActions.SelectBetterTarget(e) : new();
            return (t.Target, t.PreferredRange, t.PreferredPosition, t.PreferTanking);
        }).Draw(_tree);
    }
}
