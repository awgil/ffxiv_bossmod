using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

public class ColumnPlayersDetails : Timeline.ColumnGroup
{
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly Replay.Encounter _encounter;
    private readonly ColumnPlayerDetails.Factory playerFac;
    private readonly ColumnPlayerDetails?[] _columns;

    public bool AnyPlanModified => _columns.Any(c => c?.PlanModified ?? false);

    public delegate ColumnPlayersDetails Factory(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay.Encounter enc, BitMask showPlayers);

    public ColumnPlayersDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay.Encounter enc, BitMask showPlayers, ColumnPlayerDetails.Factory playerFac)
        : base(timeline)
    {
        _tree = tree;
        _phaseBranches = phaseBranches;
        _encounter = enc;
        this.playerFac = playerFac;
        _columns = new ColumnPlayerDetails[enc.PartyMembers.Count];
        foreach (var i in showPlayers.SetBits())
        {
            var (p, c, _) = enc.PartyMembers[i];
            _columns[i] = Add(playerFac.Invoke(Timeline, _tree, _phaseBranches, _encounter, p, c));
        }
    }

    public void DrawConfig(UITree tree)
    {
        for (int i = 0; i < _columns.Length; ++i)
        {
            var (p, c, l) = _encounter.PartyMembers[i];
            foreach (var n in tree.Node($"{c} {ReplayUtils.ParticipantString(p, p.WorldExistence.FirstOrDefault().Start)}"))
            {
                var col = _columns[i];
                if (col != null)
                    col.DrawConfig(tree);
                else if (ImGui.Button("Show details..."))
                    _columns[i] = Add(playerFac.Invoke(Timeline, _tree, _phaseBranches, _encounter, p, c));
            }
        }
    }

    public void SaveAll()
    {
        foreach (var c in _columns)
            c?.SaveChanges();
    }
}
