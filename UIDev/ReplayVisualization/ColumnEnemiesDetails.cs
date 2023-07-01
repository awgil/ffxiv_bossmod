using BossMod;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    // per-enemy details columns
    public class ColumnEnemiesDetails : Timeline.ColumnGroup
    {
        class PerOID
        {
            public uint OID;
            public List<(Replay.Participant, ColumnEnemyDetails?)> Columns = new();
        }

        private StateMachineTree _tree;
        private List<int> _phaseBranches;
        private Replay _replay;
        private Replay.Encounter _encounter;
        private List<PerOID> _data = new();

        public ColumnEnemiesDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc)
            : base(timeline)
        {
            _tree = tree;
            _phaseBranches = phaseBranches;
            _replay = replay;
            _encounter = enc;
            foreach (var (oid, participants) in enc.Participants)
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
                    else if (ImGui.Button($"Show details for {ReplayUtils.ParticipantString(p)}"))
                        n.Columns[i] = (p, Add(new ColumnEnemyDetails(Timeline, _tree, _phaseBranches, _replay, _encounter, p)));
                }
            }
        }
    }
}
