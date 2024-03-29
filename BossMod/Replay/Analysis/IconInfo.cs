using ImGuiNET;
using Lumina.Excel.GeneratedSheets2;
using System.Text;

namespace BossMod.ReplayAnalysis;

class IconInfo : CommonEnumInfo
{
    private class IconData
    {
        public HashSet<uint> TargetOIDs = new();
    }

    private Type? _iidType;
    private Dictionary<uint, IconData> _data = new();

    public IconInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = ModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        _iidType = moduleInfo?.IconIDType;
        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var icon in replay.EncounterIcons(enc))
                {
                    var data = _data.GetOrAdd(icon.ID);
                    data.TargetOIDs.Add(icon.Target.OID);
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        Func<KeyValuePair<uint, IconData>, UITree.NodeProperties> map = kv =>
        {
            var name = _iidType?.GetEnumName(kv.Key);
            return new($"{kv.Key} ({name})", false, name == null ? 0xff00ffff : 0xffffffff);
        };
        foreach (var (iid, data) in tree.Nodes(_data, map))
        {
            tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
            tree.LeafNode($"VFX: {Service.LuminaRow<Lockon>(iid)?.Unknown0}");
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate enum for boss module"))
        {
            var sb = new StringBuilder("public enum IconID : uint\n{\n");
            foreach (var (iid, data) in _data)
                sb.Append($"    {EnumMemberString(iid, data)}\n");
            sb.Append("};\n");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Generate missing enum values for boss module"))
        {
            var sb = new StringBuilder();
            foreach (var (iid, data) in _data.Where(kv => _iidType?.GetEnumName(kv.Key) == null))
                sb.AppendLine(EnumMemberString(iid, data));
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private string EnumMemberString(uint iid, IconData data)
    {
        var name = _iidType?.GetEnumName(iid) ?? $"_Gen_Icon_{iid}";
        return $"{name} = {iid}, // {OIDListString(data.TargetOIDs)}";
    }
}
