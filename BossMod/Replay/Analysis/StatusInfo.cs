using ImGuiNET;
using System.Text;

namespace BossMod.ReplayAnalysis;

class StatusInfo : CommonEnumInfo
{
    private class StatusData
    {
        public HashSet<uint> SourceOIDs = [];
        public HashSet<uint> TargetOIDs = [];
        public HashSet<ushort> Extras = [];
    }

    private readonly Type? _sidType;
    private readonly Dictionary<uint, StatusData> _data = [];

    public StatusInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = ModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        _sidType = moduleInfo?.StatusIDType;
        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var status in replay.EncounterStatuses(enc).Where(s => !(s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo) && !(s.Target.Type is ActorType.Pet or ActorType.Chocobo)))
                {
                    var data = _data.GetOrAdd(status.ID);
                    if (status.Source != null)
                        data.SourceOIDs.Add(status.Source.OID);
                    data.TargetOIDs.Add(status.Target.OID);
                    data.Extras.Add(status.StartingExtra);
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        UITree.NodeProperties map(KeyValuePair<uint, StatusData> kv)
        {
            var name = _sidType?.GetEnumName(kv.Key);
            return new($"{Utils.StatusString(kv.Key)} ({name})", false, name == null ? 0xff00ffff : 0xffffffff);
        }
        foreach (var (sid, data) in tree.Nodes(_data, map))
        {
            tree.LeafNode($"Source IDs: {OIDListString(data.SourceOIDs)}");
            tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
            tree.LeafNode($"Extras: {string.Join(", ", data.Extras.Select(extra => $"{extra:X}"))}");
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate enum for boss module"))
        {
            var sb = new StringBuilder("public enum SID : uint\n{\n");
            foreach (var (sid, data) in _data)
                sb.Append($"    {EnumMemberString(sid, data)}\n");
            sb.Append("\n}\n");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Generate missing enum values for boss module"))
        {
            var sb = new StringBuilder();
            foreach (var (sid, data) in _data.Where(kv => _sidType?.GetEnumName(kv.Key) == null))
                sb.AppendLine(EnumMemberString(sid, data));
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private string EnumMemberString(uint sid, StatusData data)
    {
        string name = _sidType?.GetEnumName(sid) ?? $"_Gen_{Utils.StringToIdentifier(Service.LuminaRow<Lumina.Excel.GeneratedSheets.Status>(sid)?.Name.ToString() ?? $"Status{sid}")}";
        return $"{name} = {sid}, // {OIDListString(data.SourceOIDs)}->{OIDListString(data.TargetOIDs)}, extra={JoinStrings(data.Extras.Select(extra => $"0x{extra:X}"))}";
    }
}
