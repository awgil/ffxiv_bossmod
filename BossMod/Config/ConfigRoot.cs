using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BossMod
{
    public class ConfigRoot
    {
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
                        var fields = j as JObject;
                        if (node == null || fields == null)
                            continue;

                        foreach (var (f, data) in fields)
                        {
                            var field = type!.GetField(f);
                            if (field == null)
                                continue;

                            var value = data?.ToObject(field.FieldType, ser);
                            if (value == null)
                                continue;

                            field.SetValue(node, value);
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
                    payload.Add(t.FullName!, JObject.FromObject(n, ser));
                JObject j = new();
                j.Add("Version", 2);
                j.Add("Payload", payload);
                File.WriteAllText(file.FullName, j.ToString());
            }
            catch (Exception e)
            {
                Service.Log($"Failed to save config to {file.FullName}: {e}");
            }
        }

        private static JsonSerializer BuildSerializer()
        {
            var res = new JsonSerializer();
            res.Converters.Add(new StringEnumConverter());
            return res;
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
