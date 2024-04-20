using ImGuiNET;

namespace BossMod.ReplayVisualization;

// per-enemy details columns
public class ColumnEnemiesDetails : Timeline.ColumnGroup
{
    class PerOID
    {
        public uint OID;
        public List<(Replay.Participant, ColumnEnemyDetails?)> Columns = [];
    }

    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly Replay _replay;
    private readonly Replay.Encounter _encounter;
    private readonly List<PerOID> _data = [];

    public ColumnEnemiesDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc)
        : base(timeline)
    {
        _tree = tree;
        _phaseBranches = phaseBranches;
        _replay = replay;
        _encounter = enc;
        foreach (var (oid, participants) in enc.ParticipantsByOID)
        {
            var data = new PerOID() { OID = oid };
            foreach (var p in participants.Where(p => (p.HasAnyActions || p.Casts.Count > 0) && !(p.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)))
                data.Columns.Add((p, null));
            if (data.Columns.Count > 0)
                _data.Add(data);
        }
    }

    public void DrawConfig(UITree tree)
    {
        var moduleInfo = ModuleRegistry.FindByOID(_encounter.OID);
        foreach (var n in tree.Nodes(_data, d => new($"{d.OID:X} ({moduleInfo?.ObjectIDType?.GetEnumName(d.OID)})")))
        {
            for (int i = 0; i < n.Columns.Count; i++)
            {
                var (p, c) = n.Columns[i];
                if (c != null)
                    c.DrawConfig(tree);
                else if (ImGui.Button($"Show details for {ReplayUtils.ParticipantString(p, p.WorldExistence.FirstOrDefault().Start)}"))
                    n.Columns[i] = (p, Add(new ColumnEnemyDetails(Timeline, _tree, _phaseBranches, _replay, _encounter, p)));
            }
        }
    }
}
