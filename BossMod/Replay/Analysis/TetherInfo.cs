using ImGuiNET;
using Lumina.Excel.GeneratedSheets2;
using System.Text;

namespace BossMod.ReplayAnalysis;

class TetherInfo : CommonEnumInfo
{
    private class TetherData
    {
        public HashSet<uint> SourceOIDs = new();
        public HashSet<uint> TargetOIDs = new();
    }

    private Type? _tidType;
    private Dictionary<uint, TetherData> _data = new();

    public TetherInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = ModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        _tidType = moduleInfo?.TetherIDType;
        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var tether in replay.EncounterTethers(enc))
                {
                    var data = _data.GetOrAdd(tether.ID);
                    data.SourceOIDs.Add(tether.Source.OID);
                    data.TargetOIDs.Add(tether.Target.OID);
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        Func<KeyValuePair<uint, TetherData>, UITree.NodeProperties> map = kv =>
        {
            var name = _tidType?.GetEnumName(kv.Key);
            return new($"{kv.Key} ({name})", false, name == null ? 0xff00ffff : 0xffffffff);
        };
        foreach (var (tid, data) in tree.Nodes(_data, map))
        {
            tree.LeafNode($"Source IDs: {OIDListString(data.SourceOIDs)}");
            tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
            tree.LeafNode($"VFX: {Service.LuminaRow<Channeling>(tid)?.File}");
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate enum for boss module"))
        {
            var sb = new StringBuilder("public enum TetherID : uint\n{\n");
            foreach (var (tid, data) in _data)
                sb.Append($"    {EnumMemberString(tid, data)}\n");
            sb.Append("};\n");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Generate missing enum values for boss module"))
        {
            var sb = new StringBuilder();
            foreach (var (tid, data) in _data.Where(kv => _tidType?.GetEnumName(kv.Key) == null))
                sb.AppendLine(EnumMemberString(tid, data));
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private string EnumMemberString(uint tid, TetherData data)
    {
        var name = _tidType?.GetEnumName(tid) ?? $"_Gen_Tether_{tid}";
        return $"{name} = {tid}, // {OIDListString(data.SourceOIDs)}->{OIDListString(data.TargetOIDs)}";
    }
}
