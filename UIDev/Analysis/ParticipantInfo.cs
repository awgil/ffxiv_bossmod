using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIDev.Analysis
{
    class ParticipantInfo
    {
        class ParticipantData
        {
            public ActorType Type; // none if value is different in different encounters
            public string Name;
            public int? SpawnedPreFight; // null if value is different in different encounters
            public bool SpawnedMidFight;

            public ParticipantData(ActorType type, string name, int spawnedPreFight, bool spawnedMidFight)
            {
                Type = type;
                Name = name;
                SpawnedPreFight = spawnedPreFight;
                SpawnedMidFight = spawnedMidFight;
            }
        }

        private uint _encOID;
        private Type? _oidType;
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
                    foreach (var (commonOID, participants) in enc.Participants)
                    {
                        ActorType? commonType = null;
                        string commonName = "";
                        int spawnedPreFight = 0, spawnedMidFight = 0;
                        foreach (var p in participants.Where(p => !(p.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Area or ActorType.Treasure) && (enc.Time.End - p.Existence.Start).TotalSeconds > 1))
                        {
                            if (commonType == null)
                                commonType = p.Type;
                            else if (commonType.Value != p.Type)
                                commonType = ActorType.None;

                            if (commonName.Length == 0)
                                commonName = p.Name;

                            if (p.Existence.Start <= enc.Time.Start)
                                ++spawnedPreFight;
                            else
                                ++spawnedMidFight;
                        }

                        if (commonType != null)
                        {
                            var data = _data.GetValueOrDefault(commonOID);
                            if (data == null)
                            {
                                data = _data[commonOID] = new(commonType.Value, commonName, spawnedPreFight, spawnedMidFight > 0);
                            }
                            else
                            {
                                if (data.Type != commonType.Value)
                                    data.Type = ActorType.None;
                                if (data.SpawnedPreFight != spawnedPreFight)
                                    data.SpawnedPreFight = null;
                                data.SpawnedMidFight |= spawnedMidFight > 0;
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
            }
        }

        public void DrawContextMenu()
        {
            if (ImGui.MenuItem("Generate enum for boss module"))
            {
                var sb = new StringBuilder($"public enum OID : uint\n{{");
                foreach (var (oid, data) in _data)
                {
                    sb.Append($"\n    {_oidType?.GetEnumName(oid) ?? $"_Gen_{Utils.StringToIdentifier(data.Name.Length > 0 ? data.Name : $"Actor{oid:X}")}"} = 0x{oid:X}, // x{data.SpawnedPreFight}");
                    if (data.Type != ActorType.Enemy)
                    {
                        sb.Append(data.Type == ActorType.None ? ", mixed types" : $", {data.Type} type");
                    }
                    if (data.SpawnedMidFight)
                    {
                        sb.Append(", and more spawn during fight");
                    }
                }
                sb.Append("\n};\n");
                ImGui.SetClipboardText(sb.ToString());
            }
        }
    }
}
