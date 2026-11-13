namespace BossMod.ReplayAnalysis;

class GlobalStatusInfo : CommonEnumInfo
{
    private class StatusData
    {
        public readonly HashSet<uint> SourceOIDs = [];
        public readonly HashSet<uint> TargetOIDs = [];
        public readonly HashSet<ushort> Extras = [];
    }

    private readonly Dictionary<uint, StatusData> _statuses = [];

    public GlobalStatusInfo(List<Replay> replays)
    {
        foreach (var replay in replays)
        {
            foreach (var status in replay.Statuses)
            {
                var st = _statuses.GetOrAdd(status.ID);
                if (status.Source is { OID: > 0 })
                    st.SourceOIDs.Add(status.Source.OID);
                st.TargetOIDs.Add(status.Target.OID);
                st.Extras.Add(status.StartingExtra);
            }
        }
    }

    public void Draw(UITree tree)
    {
        static UITree.NodeProperties map(KeyValuePair<uint, StatusData> kv) => new($"{Utils.StatusString(kv.Key)}", false, Service.LuminaRow<Lumina.Excel.Sheets.Status>(kv.Key)?.CanDispel == true && kv.Value.TargetOIDs.Contains(0) ? 0xFF00FFFF : 0xffffffff);
        foreach (var (sid, data) in tree.Nodes(_statuses, map))
        {
            tree.LeafNode($"Source IDs: {OIDListString(data.SourceOIDs)}");
            tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
            tree.LeafNode($"Extras: {string.Join(", ", data.Extras.Select(ex => ex.ToString("X")))}");
        }
    }
}
