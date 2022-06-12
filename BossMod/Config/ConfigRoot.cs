using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BossMod
{
    public class ConfigRoot
    {
        private const int _version = 3;

        public event EventHandler? Modified;
        private Dictionary<Type, ConfigNode> _nodes = new();

        public IEnumerable<ConfigNode> Nodes => _nodes.Values;

        public ConfigRoot()
        {
            foreach (var t in Utils.GetDerivedTypes<ConfigNode>(Assembly.GetExecutingAssembly()))
            {
                var inst = Activator.CreateInstance(t) as ConfigNode;
                if (inst == null)
                {
                    Service.Log($"[Config] Failed to create an instance of {t}");
                    continue;
                }
                inst.Modified += (sender, args) => Modified?.Invoke(sender, args);
                _nodes[t] = inst;
            }
        }

        public T Get<T>() where T : ConfigNode
        {
            return (T)_nodes[typeof(T)];
        }

        public void LoadFromFile(FileInfo file)
        {
            try
            {
                var contents = File.ReadAllText(file.FullName);
                var json = JObject.Parse(contents);
                var version = (int?)json["Version"] ?? 0;
                var payload = json["Payload"] as JObject;
                if (payload != null)
                {
                    payload = ConvertConfig(payload, version);
                    var ser = BuildSerializer();
                    foreach (var (t, j) in payload)
                    {
                        var type = Type.GetType(t);
                        var node = type != null ? _nodes.GetValueOrDefault(type) : null;
                        var jObj = j as JObject;
                        if (node != null && jObj != null)
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
                var ser = BuildSerializer();
                JObject payload = new();
                foreach (var (t, n) in _nodes)
                {
                    var jNode = n.Serialize(ser);
                    if (jNode.Count > 0)
                    {
                        payload.Add(t.FullName!, jNode);
                    }
                }
                JObject jContents = new();
                jContents.Add("Version", _version);
                jContents.Add("Payload", payload);
                File.WriteAllText(file.FullName, jContents.ToString());
            }
            catch (Exception e)
            {
                Service.Log($"Failed to save config to {file.FullName}: {e}");
            }
        }

        public static JsonSerializer BuildSerializer()
        {
            var res = new JsonSerializer();
            res.Converters.Add(new StringEnumConverter());
            return res;
        }

        public List<string> ConsoleCommand(IReadOnlyList<string> args)
        {
            List<string> result = new();
            if (args.Count == 0)
            {
                result.Add("Usage: /vbm cfg <config-type> <field> <value>");
                result.Add("Both config-type and field can be shortened. Valid config-types:");
                foreach (var t in _nodes.Keys)
                    result.Add($"- {t.Name}");
            }
            else
            {
                List<ConfigNode> matchingNodes = new();
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
                    var fields = matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null);
                    List<FieldInfo> matchingFields = new();
                    foreach (var f in fields)
                        if (f.Name.Contains(args[1], StringComparison.CurrentCultureIgnoreCase))
                            matchingFields.Add(f);
                    if (matchingFields.Count == 0)
                    {
                        result.Add("Field not found. Valid fields:");
                        foreach (var f in fields)
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
                                matchingNodes[0].NotifyModified();
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
        {
            if (t == typeof(bool))
                return bool.Parse(str);
            else if (t == typeof(float))
                return float.Parse(str);
            else if (t.IsAssignableTo(typeof(Enum)))
                return Enum.Parse(t, str);
            else
                return null;
        }

        private static JObject ConvertConfig(JObject payload, int version)
        {
            // v1: moved BossModuleConfig children to special encounter config node; use type names as keys
            // v2: flat structure (config root contains all nodes)
            if (version < 2)
            {
                JObject newPayload = new();
                ConvertV1GatherChildren(newPayload, payload, version == 0);
                payload = newPayload;
            }
            // v3: modified namespaces for old modules
            if (version < 3)
            {
                JObject newPayload = new();
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
            return payload;
        }

        private static void ConvertV1GatherChildren(JObject result, JObject json, bool isV0)
        {
            var children = json["__children__"] as JObject;
            if (children == null)
                return;
            foreach ((var childTypeName, var jChild) in children)
            {
                var jChildObj = jChild as JObject;
                if (jChildObj == null)
                    continue;

                string realTypeName = isV0 ? (jChildObj["__type__"]?.ToString() ?? childTypeName) : childTypeName;
                ConvertV1GatherChildren(result, jChildObj, isV0);
                result.Add(realTypeName, jChild);
            }
            json.Remove("__children__");
        }
    }
}
