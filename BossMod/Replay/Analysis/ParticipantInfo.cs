using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BossMod.ReplayAnalysis
{
    class ParticipantInfo : CommonEnumInfo
    {
        class ParticipantData
        {
            public ActorType Type; // none if value is different in different encounters
            public string Name;
            public int? SpawnedPreFight; // null if value is different in different encounters
            public bool SpawnedMidFight;
            public float MinRadius;
            public float MaxRadius;

            public ParticipantData(ActorType type, string name, int spawnedPreFight, bool spawnedMidFight, float minRadius, float maxRadius)
            {
                Type = type;
                Name = name;
                SpawnedPreFight = spawnedPreFight;
                SpawnedMidFight = spawnedMidFight;
                MinRadius = minRadius;
                MaxRadius = maxRadius;
            }
        }

        private uint _encOID;
        private Dictionary<uint, ParticipantData> _data = new();

        public ParticipantInfo(List<Replay> replays, uint oid)
        {
            _encOID = oid;
            var moduleInfo = ModuleRegistry.FindByOID(oid);
            _oidType = moduleInfo?.ObjectIDType;
            foreach (var replay in replays)
            {
                foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
                {
                    var minExistence = enc.Time.End.AddSeconds(-1); // we don't want to add actors that spawned right before wipe, they could belong to reset
                    foreach (var (commonOID, participants) in enc.ParticipantsByOID)
                    {
                        ActorType? commonType = null;
                        string commonName = "";
                        int spawnedPreFight = 0, spawnedMidFight = 0;
                        float minRadius = float.MaxValue;
                        float maxRadius = float.MinValue;
                        foreach (var p in participants.Where(p => !(p.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Area or ActorType.Treasure) && p.EffectiveExistence.Start <= minExistence))
                        {
                            if (commonType == null)
                                commonType = p.Type;
                            else if (commonType.Value != p.Type)
                                commonType = ActorType.None;

                            if (commonName.Length == 0 && p.NameHistory.Count > 0)
                                commonName = p.NameHistory.Values.First();

                            if (p.ExistsInWorldAt(enc.Time.Start))
                                ++spawnedPreFight;
                            else
                                ++spawnedMidFight;

                            minRadius = Math.Min(minRadius, p.MinRadius);
                            maxRadius = Math.Max(maxRadius, p.MaxRadius);
                        }

                        if (commonType != null)
                        {
                            var data = _data.GetValueOrDefault(commonOID);
                            if (data == null)
                            {
                                data = _data[commonOID] = new(commonType.Value, commonName, spawnedPreFight, spawnedMidFight > 0, minRadius, maxRadius);
                            }
                            else
                            {
                                if (data.Type != commonType.Value)
                                    data.Type = ActorType.None;
                                if (data.SpawnedPreFight != spawnedPreFight)
                                    data.SpawnedPreFight = null;
                                data.SpawnedMidFight |= spawnedMidFight > 0;
                                data.MinRadius = Math.Min(minRadius, data.MinRadius);
                                data.MaxRadius = Math.Max(maxRadius, data.MaxRadius);
                            }
                        }
                    }
                }
            }
        }

        public void Draw(UITree tree)
        {
            Func<KeyValuePair<uint, ParticipantData>, UITree.NodeProperties> map = kv =>
            {
                var name = _oidType?.GetEnumName(kv.Key);
                return new($"{kv.Key:X} ({_oidType?.GetEnumName(kv.Key)}) '{kv.Value.Name}' ({kv.Value.Type})", false, name == null ? 0xff00ffff : 0xffffffff);
            };
            foreach (var (oid, data) in tree.Nodes(_data, map))
            {
                tree.LeafNode($"Type: {(data.Type != ActorType.None ? data.Type.ToString() : "mixed!")}");
                tree.LeafNode($"Name: {data.Name}");
                tree.LeafNode($"Spawned pre fight: {(data.SpawnedPreFight != null ? data.SpawnedPreFight.Value.ToString() : "mixed!")}");
                tree.LeafNode($"Spawned mid fight: {data.SpawnedMidFight}");
                tree.LeafNode($"Radius: {RadiusString(data)}");
            }
        }

        public void DrawContextMenu()
        {
            if (ImGui.MenuItem("Generate enum for boss module"))
            {
                var sb = new StringBuilder("public enum OID : uint\n{\n");
                foreach (var (oid, data) in _data)
                    sb.Append($"    {EnumMemberString(oid, data)}\n");
                sb.Append("};\n");
                ImGui.SetClipboardText(sb.ToString());
            }

            if (ImGui.MenuItem("Generate missing enum values for boss module"))
            {
                var sb = new StringBuilder();
                foreach (var (oid, data) in _data.Where(kv => _oidType?.GetEnumName(kv.Key) == null))
                    sb.AppendLine(EnumMemberString(oid, data));
                ImGui.SetClipboardText(sb.ToString());
            }
        }

        private string RadiusString(ParticipantData d) => d.MinRadius != d.MaxRadius ? $"{d.MinRadius:f3}-{d.MaxRadius:f3}" : $"{d.MinRadius:f3}";

        private string EnumMemberString(uint oid, ParticipantData data)
        {
            var res = $"{_oidType?.GetEnumName(oid) ?? $"_Gen_{Utils.StringToIdentifier(data.Name.Length > 0 ? data.Name : $"Actor{oid:X}")}"} = 0x{oid:X}, // R{RadiusString(data)}";
            if (data.SpawnedPreFight > 0)
                res += $", x{data.SpawnedPreFight}";
            if (data.Type != ActorType.Enemy)
                res += data.Type == ActorType.None ? ", mixed types" : $", {data.Type} type";
            if (data.SpawnedMidFight)
                res += data.SpawnedPreFight > 0 ? ", and more spawn during fight" : ", spawn during fight";
            return res;
        }
    }
}
