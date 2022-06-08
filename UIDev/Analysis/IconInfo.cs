using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIDev.Analysis
{
    class IconInfo
    {
        private class IconData
        {
            public HashSet<uint> TargetOIDs = new();
        }

        private Type? _oidType;
        private Type? _iidType;
        private Dictionary<uint, IconData> _data = new();

        public IconInfo(List<Replay> replays, uint oid)
        {
            var moduleType = ModuleRegistry.TypeForOID(oid);
            _oidType = moduleType?.Module.GetType($"{moduleType.Namespace}.OID");
            _iidType = moduleType?.Module.GetType($"{moduleType.Namespace}.IconID");
            foreach (var replay in replays)
            {
                foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
                {
                    foreach (var icon in replay.EncounterIcons(enc))
                    {
                        var data = _data.GetOrAdd(icon.ID);
                        if (icon.Target != null)
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
            foreach (var (tid, data) in tree.Nodes(_data, map))
            {
                tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
            }
        }

        public void DrawContextMenu()
        {
            if (ImGui.MenuItem("Generate enum for boss module"))
            {
                var sb = new StringBuilder("public enum IconID : uint\n{");
                foreach (var (iid, data) in _data)
                {
                    var name = _iidType?.GetEnumName(iid) ?? $"_Gen_Icon_{iid}";
                    sb.Append($"\n    {name} = {iid}, // {OIDListString(data.TargetOIDs)}");
                }
                sb.Append("\n};\n");
                ImGui.SetClipboardText(sb.ToString());
            }
        }

        private string OIDListString(IEnumerable<uint> oids)
        {
            var s = string.Join('/', oids.Select(oid => oid == 0 ? "player" : _oidType?.GetEnumName(oid) ?? $"{oid:X}"));
            return s.Length > 0 ? s : "none";
        }
    }
}
