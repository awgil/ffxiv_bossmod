using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

public class ColumnPlayersDetails : Timeline.ColumnGroup
{
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly Replay _replay;
    private readonly Replay.Encounter _encounter;
    private readonly PlanDatabase _planDB;
    private readonly ColumnPlayerDetails?[] _columns;

    public bool AnyPlanModified => _columns.Any(c => c?.PlanModified ?? false);

    public ColumnPlayersDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, BitMask showPlayers, PlanDatabase planDB)
        : base(timeline)
    {
        _tree = tree;
        _phaseBranches = phaseBranches;
        _replay = replay;
        _encounter = enc;
        _planDB = planDB;
        _columns = new ColumnPlayerDetails[enc.PartyMembers.Count];
        foreach (var i in showPlayers.SetBits())
        {
            var (p, c, _) = enc.PartyMembers[i];
            _columns[i] = Add(new ColumnPlayerDetails(Timeline, _tree, _phaseBranches, _replay, _encounter, p, c, planDB));
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
                    _columns[i] = Add(new ColumnPlayerDetails(Timeline, _tree, _phaseBranches, _replay, _encounter, p, c, _planDB));
            }
        }
    }

    public void SaveAll()
    {
        foreach (var c in _columns)
            c?.SaveChanges();
    }
}
