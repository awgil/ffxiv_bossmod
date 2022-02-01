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
            public int Version { get; set; } = 0;
            public JObject? Payload;
        }

        private DalamudPluginInterface? _dalamud;

        public static ConfigRoot ReadConfig(DalamudPluginInterface dalamud)
        {
            var serialized = dalamud.GetPluginConfig() as Serialized;
            var obj = DeserializeNode(serialized?.Payload) as ConfigRoot ?? new ConfigRoot();
            obj._dalamud = dalamud;
            return obj;
        }

        protected override void Save()
        {
            Serialized serialized = new();
            serialized.Payload = SerializeNode(this);
            _dalamud?.SavePluginConfig(serialized);
        }

        private JObject SerializeNode(ConfigNode n)
        {
            JObject children = new();
            foreach ((var name, var child) in n.Children())
                children[name] = SerializeNode(child);

            JObject j = JObject.FromObject(n);
            j["__type__"] = n.GetType().FullName;
            j["__children__"] = children;
            return j;
        }

        private static ConfigNode? DeserializeNode(JObject? j)
        {
            if (j == null)
                return null;
            var typeName = j["__type__"]?.ToString();
            if (typeName == null)
                return null;
            var type = Type.GetType(typeName);
            if (type == null)
                return null;

            var deserialized = j.ToObject(type) as ConfigNode;
            if (deserialized != null)
            {
                var children = j["__children__"] as JObject;
                if (children != null)
                {
                    foreach ((var name, var jChild) in children)
                    {
                        var child = DeserializeNode(jChild as JObject);
                        if (child != null)
                        {
                            deserialized.AddDeserializedChild(name, child);
                        }
                    }
                }
            }
            return deserialized;
        }
    }
}
