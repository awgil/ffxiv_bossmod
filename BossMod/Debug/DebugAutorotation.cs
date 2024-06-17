using BossMod.Autorotation;

namespace BossMod;

class DebugAutorotation(RotationModuleManager autorot)
{
    private readonly UITree _tree = new();

    public void Draw()
    {
        var player = autorot.Bossmods.WorldState.Party[autorot.PlayerSlot];
        if (player == null)
            return;
        new AIHintsVisualizer(autorot.Hints, autorot.Bossmods.WorldState, player, player.TargetID, e =>
        {
            // TODO: ...
            return (e, 3, Positional.Any, false);
            //var t = e != null ? autorot.ClassActions.SelectBetterTarget(e) : new();
            //return (t.Target, t.PreferredRange, t.PreferredPosition, t.PreferTanking);
        }).Draw(_tree);
    }
}
