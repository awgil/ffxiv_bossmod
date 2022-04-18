using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private Type? _oidType;
        private Dictionary<uint, ParticipantData> _data = new();

        public ParticipantInfo(List<Replay> replays, uint oid)
        {
            var moduleType = ModuleRegistry.TypeForOID(oid);
            _oidType = moduleType?.Module.GetType($"{moduleType.Namespace}.OID");
            foreach (var replay in replays)
            {
                foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
                {
                    foreach (var (commonOID, participants) in enc.Participants)
                    {
                        ActorType? commonType = null;
                        string commonName = "";
                        int spawnedPreFight = 0, spawnedMidFight = 0;
                        foreach (var p in participants.Where(p => !(p.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)))
                        {
                            if (commonType == null)
                                commonType = p.Type;
                            else if (commonType.Value != p.Type)
                                commonType = ActorType.None;

                            if (commonName.Length == 0)
                                commonName = p.Name.Replace(' ', '_');

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

        public void Draw(Tree tree)
        {
            foreach (var (oid, data) in tree.Nodes(_data, kv => ($"{kv.Key} ({_oidType?.GetEnumName(kv.Key)})", false)))
            {
                tree.LeafNode($"Type: {(data.Type != ActorType.None ? data.Type.ToString() : "mixed!")}");
                tree.LeafNode($"Name: {data.Name}");
                tree.LeafNode($"Spawned pre fight: {(data.SpawnedPreFight != null ? data.SpawnedPreFight.Value.ToString() : "mixed!")}");
                tree.LeafNode($"Spawned mid fight: {data.SpawnedMidFight}");
            }
        }
    }
}
