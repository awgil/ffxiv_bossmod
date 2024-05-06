using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace BossMod;

public class ConfigRoot
{
    private const int _version = 9;

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
            var contents = File.ReadAllText(file.FullName);
            var json = JObject.Parse(contents);
            var version = (int?)json["Version"] ?? 0;
            if (json["Payload"] is JObject payload)
            {
                payload = ConvertConfig(payload, version);
                var ser = Serialization.BuildSerializer();
                foreach (var (t, j) in payload)
                {
                    var type = Type.GetType(t);
                    var node = type != null ? _nodes.GetValueOrDefault(type) : null;
                    if (node != null && j is JObject jObj)
                    {
                        node.Deserialize(jObj, ser);
                    }
                }
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
            var ser = Serialization.BuildSerializer();
            JObject payload = [];
            foreach (var (t, n) in _nodes)
            {
                var jNode = n.Serialize(ser);
                if (jNode.Count > 0)
                {
                    payload.Add(t.FullName!, jNode);
                }
            }
            JObject jContents = new()
            {
                { "Version", _version },
                { "Payload", payload }
            };
            File.WriteAllText(file.FullName, jContents.ToString());
        }
        catch (Exception e)
        {
            Service.Log($"Failed to save config to {file.FullName}: {e}");
        }
    }

    public List<string> ConsoleCommand(IReadOnlyList<string> args)
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
                    result.Add("Field not found. Valid fields:");
                    foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                        result.Add($"- {f.Name}");
                }
                else if (matchingFields.Count > 1)
                {
                    result.Add("Ambiguous field name, pass longer pattern. Matches:");
                    foreach (var f in matchingFields)
                        result.Add($"- {f.Name}");
                }
                else if (args.Count == 2)
                {
                    result.Add("Usage: /vbm cfg <config-type> <field> <value>");
                    result.Add($"Type of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} is {matchingFields[0].FieldType.Name}");
                }
                else
                {
                    try
                    {
                        var val = FromConsoleString(args[2], matchingFields[0].FieldType);
                        if (val == null)
                        {
                            result.Add($"Failed to convert '{args[2]}' to {matchingFields[0].FieldType}");
                        }
                        else
                        {
                            matchingFields[0].SetValue(matchingNodes[0], val);
                            matchingNodes[0].Modified.Fire();
                        }
                    }
                    catch (Exception e)
                    {
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
        : t.IsAssignableTo(typeof(Enum)) ? Enum.Parse(t, str)
        : null;

    private static JObject ConvertConfig(JObject payload, int version)
    {
        // v1: moved BossModuleConfig children to special encounter config node; use type names as keys
        // v2: flat structure (config root contains all nodes)
        if (version < 2)
        {
            JObject newPayload = [];
            ConvertV1GatherChildren(newPayload, payload, version == 0);
            payload = newPayload;
        }
        // v3: modified namespaces for old modules
        if (version < 3)
        {
            JObject newPayload = [];
            foreach (var (k, v) in payload)
            {
                var newKey = k switch
                {
                    "BossMod.Endwalker.P1S.P1SConfig" => "BossMod.Endwalker.Savage.P1SErichthonios.P1SConfig",
                    "BossMod.Endwalker.P4S2.P4S2Config" => "BossMod.Endwalker.Savage.P4S2Hesperos.P4S2Config",
                    _ => k
                };
                newPayload[newKey] = v;
            }
            payload = newPayload;
        }
        // v4: cooldown plans moved to encounter configs
        if (version < 4)
        {
            if (payload["BossMod.CooldownPlanManager"]?["Plans"] is JObject plans)
            {
                foreach (var (k, planData) in plans)
                {
                    var oid = uint.Parse(k);
                    var info = ModuleRegistry.FindByOID(oid);
                    var config = (info?.CooldownPlanningSupported ?? false) ? info.ConfigType : null;
                    if (config?.FullName == null)
                        continue;

                    if (payload[config.FullName] is not JObject node)
                        payload[config.FullName] = node = [];
                    node["CooldownPlans"] = planData;
                }
            }
            payload.Remove("BossMod.CooldownPlanManager");
        }
        // v5: bloodwhetting -> raw intuition in cd planner, to support low-level content
        if (version < 5)
        {
            Dictionary<string, string> renames = new()
            {
                [ActionID.MakeSpell(WAR.AID.Bloodwhetting).Raw.ToString()] = ActionID.MakeSpell(WAR.AID.RawIntuition).Raw.ToString()
            };
            foreach (var (k, config) in payload)
            {
                if (config?["CooldownPlans"]?["WAR"]?["Available"] is JArray plans)
                {
                    foreach (var plan in plans)
                    {
                        if (plan["PlanAbilities"] is JObject planAbilities)
                        {
                            RenameKeys(planAbilities, renames);
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
                if (config?["CooldownPlans"] is not JObject plans)
                    continue;
                bool isTEA = k == typeof(Shadowbringers.Ultimate.TEA.TEAConfig).FullName;
                foreach (var (cls, planList) in plans)
                {
                    if (planList?["Available"] is not JArray avail)
                        continue;
                    var c = Enum.Parse<Class>(cls);
                    var aidType = PlanDefinitions.Classes[c].AIDType;
                    foreach (var plan in avail)
                    {
                        if (plan?["PlanAbilities"] is not JObject abilities)
                            continue;

                        var actions = new JArray();
                        foreach (var (aidRaw, aidData) in abilities)
                        {
                            if (aidData is not JArray aidList)
                                continue;

                            var aid = new ActionID(uint.Parse(aidRaw));
                            // hack revert, out of config modules existing before v6 only TEA could use raw intuition instead of BW
                            if (!isTEA && aid.ID == (uint)WAR.AID.RawIntuition)
                                aid = ActionID.MakeSpell(WAR.AID.Bloodwhetting);

                            foreach (var abilUse in aidList)
                            {
                                abilUse["ID"] = aidType.GetEnumName(aid.ID);
                                abilUse["StateID"] = $"0x{abilUse["StateID"]?.Value<uint>():X8}";
                                actions.Add(abilUse);
                            }
                        }
                        var jplan = (JObject)plan!;
                        jplan.Remove("PlanAbilities");
                        jplan["Actions"] = actions;
                    }
                }
            }
        }
        // v7: action manager refactor
        if (version < 7)
        {
            var amConfig = payload["BossMod.ActionManagerConfig"] = new JObject();
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
                if (config is JObject jconfig)
                {
                    jconfig.Remove("Modified");
                }
            }
        }
        return payload;
    }

    private static void ConvertV1GatherChildren(JObject result, JObject json, bool isV0)
    {
        if (json["__children__"] is not JObject children)
            return;
        foreach ((var childTypeName, var jChild) in children)
        {
            if (jChild is not JObject jChildObj)
                continue;

            string realTypeName = isV0 ? (jChildObj["__type__"]?.ToString() ?? childTypeName) : childTypeName;
            ConvertV1GatherChildren(result, jChildObj, isV0);
            result.Add(realTypeName, jChild);
        }
        json.Remove("__children__");
    }

    private static void RenameKeys(JObject map, Dictionary<string, string> rename)
    {
        JObject upd = [];
        foreach (var (k, v) in map)
            upd[rename.GetValueOrDefault(k, k)] = v;
        map.Replace(upd);
    }
}
