using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json.Linq;
using System;

namespace BossMod
{
    class ConfigRoot : ConfigNode
    {
        private class Serialized : IPluginConfiguration
        {
            public int Version { get; set; } = 1;
            public JObject? Payload;
        }

        private DalamudPluginInterface? _dalamud;

        public static ConfigRoot ReadConfig(DalamudPluginInterface dalamud)
        {
            ConfigRoot? obj = null;
            var serialized = dalamud.GetPluginConfig() as Serialized;
            if (serialized?.Payload != null)
                obj = DeserializeNode(serialized.Payload, typeof(ConfigRoot), serialized.Version) as ConfigRoot;
            if (obj == null)
                obj = new();
            obj._dalamud = dalamud;
            return obj;
        }

        public ConfigRoot()
        {
            Modified += (_, _) =>
            {
                Serialized serialized = new();
                serialized.Payload = SerializeNode(this);
                _dalamud?.SavePluginConfig(serialized);
            };
        }

        private JObject SerializeNode(ConfigNode n)
        {
            JObject children = new();
            foreach (var child in n.Children)
                children[child.GetType().FullName!] = SerializeNode(child);

            JObject j = JObject.FromObject(n);
            j["__children__"] = children;
            return j;
        }

        private static ConfigNode? DeserializeNode(JObject j, Type type, int version)
        {
            var deserialized = j.ToObject(type) as ConfigNode;
            if (version == 0 && type == typeof(BossModuleConfig))
                return deserialized; // children of this node are moved...

            if (deserialized != null)
            {
                var children = j["__children__"] as JObject;
                if (children != null)
                {
                    foreach ((var childTypeName, var jChild) in children)
                    {
                        var jChildObj = jChild as JObject;
                        if (jChildObj == null)
                            continue;

                        var realTypeName = version == 0 ? jChildObj["__type__"]?.ToString() : childTypeName;
                        var childType = realTypeName != null ? Type.GetType(realTypeName) : null;
                        if (childType == null)
                            continue;

                        var child = DeserializeNode(jChildObj, childType, version);
                        if (child != null)
                            deserialized.AddChild(child);
                    }
                }
            }
            return deserialized;
        }
    }
}
