using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Globalization;
using System.Text;

namespace BossMod.ReplayAnalysis;

class ParticipantInfo : CommonEnumInfo
{
    class ParticipantData
    {
        public List<ActorType> Types = [];
        public List<(uint zoneId, uint cfcId)> Zones = [];
        public List<(string name, uint id)> Names = [];
        public List<int> SpawnedPreFight = [];
        public bool SpawnedMidFight;
        public float MinRadius = float.MaxValue;
        public float MaxRadius = float.MinValue;
    }

    private readonly Dictionary<uint, ParticipantData> _data = [];

    public ParticipantInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = ModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                var minExistence = enc.Time.End.AddSeconds(-1); // we don't want to add actors that spawned right before wipe, they could belong to reset
                foreach (var (commonOID, participants) in enc.ParticipantsByOID)
                {
                    var data = _data.GetOrAdd(commonOID);
                    int spawnedPreFight = 0;
                    foreach (var p in participants.Where(p => !IsIgnored(p) && p.EffectiveExistence.Start <= minExistence))
                    {
                        data.Types.Add(p.Type);
                        data.Zones.Add((p.ZoneID, p.CFCID));
                        data.Names.AddRange(p.NameHistory.Values);

                        if (p.ExistsInWorldAt(enc.Time.Start))
                            ++spawnedPreFight;
                        else
                            data.SpawnedMidFight = true;

                        data.MinRadius = Math.Min(data.MinRadius, p.MinRadius);
                        data.MaxRadius = Math.Max(data.MaxRadius, p.MaxRadius);
                    }
                    data.SpawnedPreFight.Add(spawnedPreFight);
                }
            }
        }
        FinishBuild();
    }

    public ParticipantInfo(List<Replay> replays)
    {
        foreach (var replay in replays)
        {
            foreach (var p in replay.Participants.Where(p => !IsIgnored(p)))
            {
                var data = _data.GetOrAdd(p.OID);
                data.Types.Add(p.Type);
                data.Zones.Add((p.ZoneID, p.CFCID));
                data.Names.AddRange(p.NameHistory.Values);
                data.MinRadius = Math.Min(data.MinRadius, p.MinRadius);
                data.MaxRadius = Math.Max(data.MaxRadius, p.MaxRadius);
            }
        }
        FinishBuild();
    }

    public void Draw(UITree tree)
    {
        UITree.NodeProperties map(KeyValuePair<uint, ParticipantData> kv)
        {
            var name = _oidType?.GetEnumName(kv.Key);
            var typeName = kv.Value.Types.Count switch
            {
                0 => "???",
                1 => kv.Value.Types[0].ToString(),
                _ => "mixed!"
            };
            return new($"{kv.Key:X} ({_oidType?.GetEnumName(kv.Key)}) '{kv.Value.Names.FirstOrDefault().name}' ({typeName})", false, name == null ? 0xff00ffff : 0xffffffff);
        }
        foreach (var (oid, data) in tree.Nodes(_data, map, kv => DrawSubContextMenu(kv.Key, kv.Value)))
        {
            foreach (var n in tree.Node($"Types ({data.Types.Count})", data.Types.Count == 0))
                tree.LeafNodes(data.Types, t => t.ToString());
            foreach (var n in tree.Node($"Zones ({data.Zones.Count})", data.Zones.Count == 0))
                tree.LeafNodes(data.Zones, z => $"{z.zoneId} '{Service.LuminaRow<TerritoryType>(z.zoneId)?.PlaceName.Value?.Name}' (cfc={z.cfcId})");
            foreach (var n in tree.Node($"Names ({data.Names.Count})", data.Names.Count == 0))
                tree.LeafNodes(data.Names, n => $"[{n.id}] {n.name}");
            tree.LeafNode($"Spawned pre fight: {string.Join(", ", data.SpawnedPreFight)}");
            tree.LeafNode($"Spawned mid fight: {data.SpawnedMidFight}");
            tree.LeafNode($"Radius: {RadiusString(data)}");
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate enum for boss module"))
        {
            ImGui.SetClipboardText(AddOIDEnum(new()).ToString());
        }

        if (ImGui.MenuItem("Generate missing enum values for boss module"))
        {
            var sb = new StringBuilder();
            foreach (var (oid, data) in _data.Where(kv => _oidType?.GetEnumName(kv.Key) == null))
                sb.AppendLine(EnumMemberString(oid, data));
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private void FinishBuild()
    {
        List<uint> toDel = [];
        foreach (var (curOID, data) in _data)
        {
            if (data.Types.Count == 0)
            {
                toDel.Add(curOID);
            }
            else
            {
                data.Types.SortAndRemoveDuplicates();
                data.Zones.SortAndRemoveDuplicates();
                data.Names.SortAndRemoveDuplicates();
                data.SpawnedPreFight.SortAndRemoveDuplicates();
            }
        }
        foreach (var curOID in toDel)
            _data.Remove(curOID);
    }

    private void DrawSubContextMenu(uint oid, ParticipantData data)
    {
        if (ImGui.MenuItem("Generate module stub (trivial states)"))
        {
            ImGui.SetClipboardText(AddBossModuleStub(new(), oid, data, false).ToString());
        }
        if (ImGui.MenuItem("Generate module stub (with state machine)"))
        {
            ImGui.SetClipboardText(AddBossModuleStub(new(), oid, data, true).ToString());
        }
    }

    private static bool IsIgnored(Replay.Participant p) => p.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Area or ActorType.Treasure;
    private string RadiusString(ParticipantData d) => d.MinRadius != d.MaxRadius ? string.Create(CultureInfo.InvariantCulture, $"{d.MinRadius:f3}-{d.MaxRadius:f3}") : string.Create(CultureInfo.InvariantCulture, $"{d.MinRadius:f3}");
    private string GuessName(uint oid, ParticipantData d) => Utils.StringToIdentifier(d.Names.Count > 0 ? d.Names[0].name : $"Actor{oid:X}");

    private string EnumMemberString(uint oid, ParticipantData data, string? forcedName = null)
    {
        var enumName = forcedName ?? _oidType?.GetEnumName(oid) ?? ("_Gen_" + GuessName(oid, data));
        var spawnStr = data.SpawnedPreFight.Count switch
        {
            0 => "?",
            1 => data.SpawnedPreFight[0].ToString(),
            _ => $"{data.SpawnedPreFight[0]}-{data.SpawnedPreFight[^1]}",
        };
        if (data.SpawnedMidFight)
            spawnStr += " (spawn during fight)";
        var typeStr = data.Types.Count switch
        {
            0 => ", ??? type",
            1 => data.Types[0] == ActorType.Enemy ? "" : $", {data.Types[0]} type",
            _ => ", mixed types"
        };
        return $"{enumName} = 0x{oid:X}, // R{RadiusString(data)}, x{spawnStr}{typeStr}";
    }

    private StringBuilder AddOIDEnum(StringBuilder sb, uint forcedBossOID = 0)
    {
        sb.AppendLine("public enum OID : uint");
        sb.AppendLine("{");
        foreach (var (oid, data) in _data)
            sb.AppendLine($"    {EnumMemberString(oid, data, oid == forcedBossOID ? "Boss" : null)}");
        sb.AppendLine("}");
        return sb;
    }

    private StringBuilder AddBossModuleStub(StringBuilder sb, uint oid, ParticipantData data, bool withStates)
    {
        var name = GuessName(oid, data);
        AddOIDEnum(sb, oid);
        sb.AppendLine();
        sb.AppendLine($"class {name}States : StateMachineBuilder");
        sb.AppendLine("{");
        sb.AppendLine($"    public {name}States(BossModule module) : base(module)");
        sb.AppendLine("    {");
        if (withStates)
            sb.AppendLine($"        DeathPhase(0, SinglePhase);");
        else
            sb.AppendLine($"        TrivialPhase();");
        sb.AppendLine("    }");
        if (withStates)
        {
            sb.AppendLine();
            sb.AppendLine("    private void SinglePhase(uint id)");
            sb.AppendLine("    {");
            sb.AppendLine("        SimpleState(id + 0xFF0000, 10000, \"???\"");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    //private void XXX(uint id, float delay)");
        }
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = {data.Zones.FirstOrDefault().cfcId}, NameID = {data.Names.FirstOrDefault().id})]");
        sb.AppendLine($"public class {name} : BossModule");
        sb.AppendLine("{");
        sb.AppendLine($"    public {name}(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(20)) {{ }}");
        sb.AppendLine("}");
        return sb;
    }
}
