using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BossMod;

public class ConfigRoot
{
    private const int _version = 10;

    public Event Modified = new();
    private readonly Dictionary<Type, ConfigNode> _nodes = [];

    public IEnumerable<ConfigNode> Nodes => _nodes.Values;

    public void Initialize()
    {
        foreach (var t in Utils.GetDerivedTypes<ConfigNode>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            if (Activator.CreateInstance(t) is not ConfigNode inst)
            {
                Service.Log($"[Config] Failed to create an instance of {t}");
                continue;
            }
            inst.Modified.Subscribe(Modified.Fire);
            _nodes[t] = inst;
        }
    }

    public T Get<T>() where T : ConfigNode => (T)_nodes[typeof(T)];
    public T Get<T>(Type derived) where T : ConfigNode => (T)_nodes[derived];

    public ConfigListener<T> GetAndSubscribe<T>(Action<T> modified) where T : ConfigNode => new(Get<T>(), modified);

    public void LoadFromFile(FileInfo file)
    {
        try
        {
            using var json = ReadConvertFile(file);
            var ser = Serialization.BuildSerializationOptions();
            foreach (var jconfig in json.RootElement.GetProperty("Payload").EnumerateObject())
            {
                var type = Type.GetType(jconfig.Name);
                var node = type != null ? _nodes.GetValueOrDefault(type) : null;
                node?.Deserialize(jconfig.Value, ser);
            }
        }
        catch (Exception e)
        {
            Service.Log($"Failed to load config from {file.FullName}: {e}");
        }
    }

    public void SaveToFile(FileInfo file)
    {
        try
        {
            WriteFile(file, jwriter =>
            {
                jwriter.WriteStartObject();
                var ser = Serialization.BuildSerializationOptions();
                foreach (var (t, n) in _nodes)
                {
                    jwriter.WritePropertyName(t.FullName!);
                    n.Serialize(jwriter, ser);
                }
                jwriter.WriteEndObject();
            });
        }
        catch (Exception e)
        {
            Service.Log($"Failed to save config to {file.FullName}: {e}");
        }
    }

    public List<string> ConsoleCommand(IReadOnlyList<string> args, bool save = true)
    {
        List<string> result = [];
        if (args.Count == 0)
        {
            result.Add("Usage: /vbm cfg <config-type> <field> <value>");
            result.Add("Both config-type and field can be shortened. Valid config-types:");
            foreach (var t in _nodes.Keys)
                result.Add($"- {t.Name}");
        }
        else
        {
            List<ConfigNode> matchingNodes = [];
            foreach (var (t, n) in _nodes)
                if (t.Name.Contains(args[0], StringComparison.CurrentCultureIgnoreCase))
                    matchingNodes.Add(n);
            if (matchingNodes.Count == 0)
            {
                result.Add("Config type not found. Valid types:");
                foreach (var t in _nodes.Keys)
                    result.Add($"- {t.Name}");
            }
            else if (matchingNodes.Count > 1)
            {
                result.Add("Ambiguous config type, pass longer pattern. Matches:");
                foreach (var n in matchingNodes)
                    result.Add($"- {n.GetType().Name}");
            }
            else if (args.Count == 1)
            {
                result.Add("Usage: /vbm cfg <config-type> <field> <value>");
                result.Add($"Valid fields for {matchingNodes[0].GetType().Name}:");
                foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                    result.Add($"- {f.Name}");
            }
            else
            {
                List<FieldInfo> matchingFields = [];
                foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                    if (f.Name.Contains(args[1], StringComparison.CurrentCultureIgnoreCase))
                        matchingFields.Add(f);
                if (matchingFields.Count == 0)
                {
                    result.Add($"Field not found {args[1]}, Valid fields:");
                    foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                        result.Add($"- {f.Name}");
                }
                else if (matchingFields.Count > 1)
                {
                    result.Add("Ambiguous field name, pass longer pattern. Matches:");
                    foreach (var f in matchingFields)
                        result.Add($"- {f.Name}");
                }
                /*else if (args.Count == 2)
                {
                    result.Add("Usage: /vbm cfg <config-type> <field> <value>");
                    result.Add($"Type of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} is {matchingFields[0].FieldType.Name}");
                }*/
                else
                {
                    try
                    {
                        if (args.Count == 2)
                            result.Add(matchingFields[0].GetValue(matchingNodes[0])?.ToString() ?? $"Failed to get value of '{args[2]}'");
                        else
                        {
                            var val = FromConsoleString(args[2], matchingFields[0].FieldType);
                            if (val == null)
                            {
                                result.Add($"Failed to convert '{args[2]}' to {matchingFields[0].FieldType}");
                            }
                            else
                            {
                                matchingFields[0].SetValue(matchingNodes[0], val);
                                if (save)
                                    matchingNodes[0].Modified.Fire();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (args.Count == 2)
                            result.Add($"Failed to get value of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} : {e}");
                        else
                            result.Add($"Failed to set {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} to {args[2]}: {e}");
                    }
                }
            }
        }
        return result;
    }

    private object? FromConsoleString(string str, Type t)
        => t == typeof(bool) ? bool.Parse(str)
        : t == typeof(float) ? float.Parse(str)
        : t == typeof(int) ? int.Parse(str)
        : t.IsAssignableTo(typeof(Enum)) ? Enum.Parse(t, str)
        : null;

    private JsonDocument ReadConvertFile(FileInfo file)
    {
        var json = Serialization.ReadJson(file.FullName);
        var version = json.RootElement.TryGetProperty("Version", out var jver) ? jver.GetInt32() : 0;
        if (version > _version)
            throw new ArgumentException($"Config file version {version} is newer than supported {_version}");
        if (version == _version)
            return json;

        var converted = ConvertConfig(JsonObject.Create(json.RootElement.GetProperty("Payload"))!, version, file.Directory!);

        var original = new FileInfo(file.FullName);
        var backup = new FileInfo(file.FullName + $".v{version}");
        if (!backup.Exists)
            file.MoveTo(backup.FullName);
        WriteFile(original, jwriter => converted.WriteTo(jwriter));
        json.Dispose();

        return Serialization.ReadJson(original.FullName);
    }

    private void WriteFile(FileInfo file, Action<Utf8JsonWriter> writePayload)
    {
        using var fstream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var jwriter = Serialization.WriteJson(fstream);
        jwriter.WriteStartObject();
        jwriter.WriteNumber("Version", _version);
        jwriter.WritePropertyName("Payload");
        writePayload(jwriter);
        jwriter.WriteEndObject();
    }

    private static JsonObject ConvertConfig(JsonObject payload, int version, DirectoryInfo dir)
    {
        // v1: moved BossModuleConfig children to special encounter config node; use type names as keys
        // v2: flat structure (config root contains all nodes)
        if (version < 2)
        {
            JsonObject newPayload = [];
            ConvertV1GatherChildren(newPayload, payload, version == 0);
            payload = newPayload;
        }
        // v3: modified namespaces for old modules
        if (version < 3)
        {
            if (TryRemoveNode(payload, "BossMod.Endwalker.P1S.P1SConfig", out var p1s))
                payload.Add("BossMod.Endwalker.Savage.P1SErichthonios.P1SConfig", p1s);
            if (TryRemoveNode(payload, "BossMod.Endwalker.P4S2.P4S2Config", out var p4s2))
                payload.Add("BossMod.Endwalker.Savage.P4S2Hesperos.P4S2Config", p4s2);
        }
        // v4: cooldown plans moved to encounter configs
        if (version < 4)
        {
            if (payload["BossMod.CooldownPlanManager"]?["Plans"] is JsonObject plans)
            {
                foreach (var (k, planData) in plans)
                {
                    var oid = uint.Parse(k);
                    var info = ModuleRegistry.FindByOID(oid);
                    var config = info?.PlanLevel > 0 ? info.ConfigType : null;
                    if (config?.FullName == null)
                        continue;

                    if (payload[config.FullName] is not JsonObject node)
                        payload[config.FullName] = node = [];
                    node["CooldownPlans"] = planData;
                }
            }
            payload.Remove("BossMod.CooldownPlanManager");
        }
        // v5: bloodwhetting -> raw intuition in cd planner, to support low-level content
        if (version < 5)
        {
            foreach (var (k, config) in payload)
            {
                if (config?["CooldownPlans"]?["WAR"]?["Available"] is JsonArray plans)
                {
                    foreach (var plan in plans)
                    {
                        if (plan!["PlanAbilities"] is JsonObject planAbilities)
                        {
                            if (TryRemoveNode(planAbilities, ActionID.MakeSpell(WAR.AID.Bloodwhetting).Raw.ToString(), out var bw))
                                planAbilities.Add(ActionID.MakeSpell(WAR.AID.RawIntuition).Raw.ToString(), bw);
                        }
                    }
                }
            }
        }
        // v6: new cooldown planner
        if (version < 6)
        {
            foreach (var (k, config) in payload)
            {
                if (config?["CooldownPlans"] is not JsonObject plans)
                    continue;
                bool isTEA = k == typeof(Shadowbringers.Ultimate.TEA.TEAConfig).FullName;
                foreach (var (cls, planList) in plans)
                {
                    if (planList?["Available"] is not JsonArray avail)
                        continue;
                    var c = Enum.Parse<Class>(cls);
                    foreach (var plan in avail)
                    {
                        if (plan?["PlanAbilities"] is not JsonObject abilities)
                            continue;

                        var actions = new JsonArray();
                        foreach (var (aidRaw, aidData) in abilities)
                        {
                            if (aidData is not JsonArray aidList)
                                continue;

                            var aid = new ActionID(uint.Parse(aidRaw));
                            // hack revert, out of config modules existing before v6 only TEA could use raw intuition instead of BW
                            if (!isTEA && aid.ID == (uint)WAR.AID.RawIntuition)
                                aid = ActionID.MakeSpell(WAR.AID.Bloodwhetting);

                            foreach (var abilUse in aidList)
                            {
                                abilUse!["ID"] = Utils.StringToIdentifier(aid.Name());
                                abilUse["StateID"] = $"0x{abilUse["StateID"]?.GetValue<uint>():X8}";
                                actions.Add(abilUse);
                            }
                        }
                        var jplan = (JsonObject)plan!;
                        jplan.Remove("PlanAbilities");
                        jplan["Actions"] = actions;
                    }
                }
            }
        }
        // v7: action manager refactor
        if (version < 7)
        {
            var amConfig = payload["BossMod.ActionManagerConfig"] = new JsonObject();
            var autorotConfig = payload["BossMod.AutorotationConfig"];
            amConfig["RemoveAnimationLockDelay"] = autorotConfig?["RemoveAnimationLockDelay"] ?? false;
            amConfig["PreventMovingWhileCasting"] = autorotConfig?["PreventMovingWhileCasting"] ?? false;
            amConfig["RestoreRotation"] = autorotConfig?["RestoreRotation"] ?? false;
            amConfig["GTMode"] = autorotConfig?["GTMode"] ?? "Manual";
        }
        // v8: remove accidentally serializable Modified field
        // v9: and again the same thing...
        if (version < 9)
        {
            foreach (var (_, config) in payload)
            {
                if (config is JsonObject jconfig)
                {
                    jconfig.Remove("Modified");
                }
            }
        }
        // v10: autorotation v2: moved configs around and importantly moved cdplans outside
        if (version < 10)
        {
            if (TryRemoveNode(payload, "BossMod.ActionManagerConfig", out var tweaks))
                payload.Add("BossMod.ActionTweaksConfig", tweaks);
            if (TryRemoveNode(payload, "BossMod.AutorotationConfig", out var autorot))
                payload.Add("BossMod.Autorotation.AutorotationConfig", autorot);
            ConvertV9Plans(payload, dir);
        }
        return payload;
    }

    private static void ConvertV1GatherChildren(JsonObject result, JsonObject json, bool isV0)
    {
        if (json["__children__"] is not JsonObject children)
            return;
        foreach ((var childTypeName, var jChild) in children)
        {
            if (jChild is not JsonObject jChildObj)
                continue;

            string realTypeName = isV0 ? (jChildObj["__type__"]?.ToString() ?? childTypeName) : childTypeName;
            ConvertV1GatherChildren(result, jChildObj, isV0);
            result.Add(realTypeName, jChild);
        }
        json.Remove("__children__");
    }

    private static void ConvertV9Plans(JsonObject payload, DirectoryInfo dir)
    {
        var dbRoot = new DirectoryInfo(dir.FullName + "/BossMod/autorot/plans");
        if (!dbRoot.Exists)
            dbRoot.Create();
        using var manifestStream = new FileStream(dbRoot + ".manifest.json", FileMode.Create, FileAccess.Write, FileShare.Read);
        using var manifest = Serialization.WriteJson(manifestStream);
        manifest.WriteStartObject();
        manifest.WriteNumber("version", 0);
        manifest.WriteStartObject("payload");
        foreach (var (ct, cfg) in payload.AsObject())
        {
            if (!TryRemoveNode(cfg!.AsObject(), "CooldownPlans", out var cdplans))
                continue;
            var t = ct[..^6];
            var type = Type.GetType(t);
            manifest.WriteStartObject(t);
            foreach (var (cls, plans) in cdplans!.AsObject())
            {
                manifest.WriteStartObject(cls);
                if (plans!.AsObject().TryGetPropertyValue("SelectedIndex", out var jsel))
                    manifest.WriteNumber("SelectedIndex", jsel!.GetValue<int>());
                manifest.WriteStartArray("Plans");
                foreach (var plan in plans["Available"]!.AsArray())
                {
                    var guid = Guid.NewGuid().ToString();
                    manifest.WriteStringValue(guid);

                    var oplan = plan!.AsObject();
                    var utilityTracks = ConvertV9ActionsToUtilityTracks(oplan);
                    var rotationTracks = ConvertV9StrategiesToRotationTracks(oplan);
                    if (rotationTracks.Remove("Special", out var lb))
                        utilityTracks["LB"] = lb;

                    using var planStream = new FileStream($"{dbRoot}/{guid}.json", FileMode.Create, FileAccess.Write, FileShare.Read);
                    using var jplan = Serialization.WriteJson(planStream);
                    jplan.WriteStartObject();
                    jplan.WriteNumber("version", 0);
                    jplan.WriteStartObject("payload");
                    jplan.WriteString("Name", plan!["Name"]!.GetValue<string>());
                    jplan.WriteString("Encounter", t);
                    jplan.WriteString("Class", cls);
                    jplan.WriteNumber("Level", type != null ? ModuleRegistry.FindByType(type)?.PlanLevel ?? 0 : 0);
                    jplan.WriteStartArray("PhaseDurations");
                    foreach (var d in plan["Timings"]!["PhaseDurations"]!.AsArray())
                        jplan.WriteNumberValue(d!.GetValue<float>());
                    jplan.WriteEndArray();
                    jplan.WriteStartObject("Modules");
                    ConvertV9WriteTrack(jplan, $"BossMod.Autorotation.Class{cls}Utility", utilityTracks);
                    ConvertV9WriteTrack(jplan, $"BossMod.Autorotation.Legacy.Legacy{cls}", rotationTracks);
                    jplan.WriteEndObject();
                    if (oplan.TryGetPropertyValue("Targets", out var jtargets))
                    {
                        jplan.WriteStartArray("Targeting");
                        foreach (var target in jtargets!.AsArray())
                        {
                            var jt = target!.AsObject();
                            if (TryRemoveNode(jt, "OID", out var oid))
                            {
                                jt["Target"] = "EnemyByOID";
                                jt["TargetParam"] = int.Parse(oid!.GetValue<string>()[2..], System.Globalization.NumberStyles.HexNumber);
                            }
                            jt.WriteTo(jplan);
                        }
                        jplan.WriteEndArray();
                    }
                    jplan.WriteEndObject();
                    jplan.WriteEndObject();
                }
                manifest.WriteEndArray();
                manifest.WriteEndObject();
            }
            manifest.WriteEndObject();
        }
        manifest.WriteEndObject();
        manifest.WriteEndObject();
    }

    private static Dictionary<string, List<JsonObject>> ConvertV9ActionsToUtilityTracks(JsonObject plan)
    {
        Dictionary<string, List<JsonObject>> tracks = [];
        if (!plan.TryGetPropertyValue("Actions", out var actions))
            return tracks;
        foreach (var action in actions!.AsArray())
        {
            var aobj = action!.AsObject();
            aobj["Option"] = "Use";
            if (TryRemoveNode(aobj, "LowPriority", out var jprio))
                aobj.Add("PriorityOverride", jprio!.GetValue<bool>() ? ActionQueue.Priority.Low : ActionQueue.Priority.High);
            if (TryRemoveNode(aobj, "Target", out var jtarget))
            {
                switch (jtarget!["Type"]!.GetValue<string>()!)
                {
                    case "Self":
                        aobj["Target"] = "Self";
                        break;
                    case "EnemyByOID":
                        aobj["Target"] = "EnemyByOID";
                        aobj["TargetParam"] = jtarget["OID"]!.GetValue<int>();
                        break;
                    case "LowestHPPartyMember":
                        aobj["Target"] = "PartyWithLowestHP";
                        aobj["TargetParam"] = jtarget["AllowSelf"]!.GetValue<bool>() ? 1 : 0;
                        break;
                }
            }
            if (TryRemoveNode(aobj, "ID", out var jid))
            {
                var id = jid!.GetValue<string>();
                switch (id)
                {
                    case "Bloodwhetting":
                        id = "BW";
                        aobj["Option"] = "BW";
                        break;
                    case "RawIntuition":
                        id = "BW";
                        aobj["Option"] = "RI";
                        break;
                    case "NascentFlash":
                        id = "BW";
                        aobj["Option"] = "NF";
                        break;
                }
                tracks.GetOrAdd(id).Add(aobj);
            }
        }
        return tracks;
    }

    private static Dictionary<string, List<JsonObject>> ConvertV9StrategiesToRotationTracks(JsonObject plan)
    {
        Dictionary<string, List<JsonObject>> tracks = [];
        if (!plan.TryGetPropertyValue("Strategies", out var strategies))
            return tracks;
        foreach (var (track, values) in strategies!.AsObject())
        {
            var t = tracks[track] = [.. values!.AsArray().Select(n => n!.AsObject())];
            foreach (var v in t)
                if (TryRemoveNode(v, "Value", out var jv))
                    v["Option"] = jv;
        }
        return tracks;
    }

    private static void ConvertV9WriteTrack(Utf8JsonWriter writer, string module, Dictionary<string, List<JsonObject>> tracks)
    {
        writer.WriteStartObject(module);
        foreach (var (tn, td) in tracks)
        {
            writer.WriteStartArray(tn);
            foreach (var d in td)
            {
                d.WriteTo(writer);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }

    private static bool TryRemoveNode(JsonObject parent, string key, out JsonNode? node) => parent.TryGetPropertyValue(key, out node) && parent.Remove(key);
}
