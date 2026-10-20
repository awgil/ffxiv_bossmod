using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

public class ColumnPlayersDetails : Timeline.ColumnGroup
{
    private readonly Replay.Encounter _encounter;
    private readonly ColumnPlayerDetails.Factory playerDetailsFactory;
    private readonly ColumnPlayerDetails?[] _columns;

    public bool AnyPlanModified => _columns.Any(c => c?.PlanModified ?? false);

    public delegate ColumnPlayersDetails Factory(BitMask showPlayers);

    public ColumnPlayersDetails(Timeline timeline, Replay.Encounter enc, BitMask showPlayers, ColumnPlayerDetails.Factory playerDetailsFactory)
        : base(timeline)
    {
        _encounter = enc;
        this.playerDetailsFactory = playerDetailsFactory;
        _columns = new ColumnPlayerDetails[enc.PartyMembers.Count];
        foreach (var i in showPlayers.SetBits())
        {
            var (p, c, _) = enc.PartyMembers[i];
            _columns[i] = Add(playerDetailsFactory(p, c));
        }
    }

    public void DrawConfig(UITree tree)
    {
        for (var i = 0; i < _columns.Length; ++i)
        {
            var (p, c, l) = _encounter.PartyMembers[i];
            foreach (var n in tree.Node($"{c} {ReplayUtils.ParticipantString(p, p.WorldExistence.FirstOrDefault().Start)}"))
            {
                var col = _columns[i];
                if (col != null)
                    col.DrawConfig(tree);
                else if (ImGui.Button("Show details..."))
                    _columns[i] = Add(playerDetailsFactory.Invoke(p, c));
            }
        }
    }

    public void SaveAll()
    {
        foreach (var c in _columns)
            c?.SaveChanges();
    }
}
