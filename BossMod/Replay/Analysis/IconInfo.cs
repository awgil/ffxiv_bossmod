using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using System.Text;

namespace BossMod.ReplayAnalysis;

class IconInfo : CommonEnumInfo
{
    private class IconData
    {
        public HashSet<uint> SourceOIDs = [];
        public HashSet<uint> TargetOIDs = [];
        public bool SeenTargetNonSelf;
    }

    private readonly Type? _iidType;
    private readonly Dictionary<uint, IconData> _data = [];

    public IconInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = BossModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        _iidType = moduleInfo?.IconIDType;
        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var icon in replay.EncounterIcons(enc))
                {
                    var data = _data.GetOrAdd(icon.ID);
                    data.SourceOIDs.Add(icon.Source.Type != ActorType.DutySupport ? icon.Source.OID : 0);
                    if (icon.Target != null)
                    {
                        data.TargetOIDs.Add(icon.Target.Type != ActorType.DutySupport ? icon.Target.OID : 0);
                        data.SeenTargetNonSelf |= icon.Target != icon.Source;
                    }
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        UITree.NodeProperties map(KeyValuePair<uint, IconData> kv)
        {
            var name = _iidType?.GetEnumName(kv.Key);
            return new($"{kv.Key} ({name})", false, name == null ? 0xff00ffff : 0xffffffff);
        }
        foreach (var (iid, data) in tree.Nodes(_data, map))
        {
            tree.LeafNode($"Source IDs: {OIDListString(data.SourceOIDs)}");
            tree.LeafNode($"Target IDs: {(data.TargetOIDs.Count == 0 ? "???" : data.SeenTargetNonSelf ? OIDListString(data.TargetOIDs) : "self")}");
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
            sb.Append("}\n");
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
        string generateIconName() => Service.LuminaRow<Lockon>(iid)?.Unknown0.ToString() ?? iid.ToString();

        var name = _iidType?.GetEnumName(iid) ?? $"_Gen_Icon_{generateIconName()}";
        return $"{name} = {iid}, // {OIDListString(data.SourceOIDs)}->{(data.TargetOIDs.Count == 0 ? "???" : data.SeenTargetNonSelf ? OIDListString(data.TargetOIDs) : "self")}";
    }
}
