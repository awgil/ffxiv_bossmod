namespace BossMod;

class DebugAutorotation(Autorotation autorot)
{
    private readonly UITree _tree = new();

    public void Draw()
    {
        if (autorot.ClassActions == null)
            return;

        _tree.LeafNode($"Primary target: {autorot.PrimaryTarget}");
        _tree.LeafNode($"Secondary target: {autorot.SecondaryTarget}");
        new AIHintsVisualizer(autorot.Hints, autorot.WorldState, autorot.ClassActions.Player, autorot.PrimaryTarget?.InstanceID ?? 0, e =>
        {
            var t = e != null ? autorot.ClassActions.SelectBetterTarget(e) : new();
            return (t.Target, t.PreferredRange, t.PreferredPosition, t.PreferTanking);
        }).Draw(_tree);
    }
}
