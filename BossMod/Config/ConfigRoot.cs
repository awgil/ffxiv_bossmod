using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace BossMod
{
    public class ConfigRoot : ConfigNode
    {
        public void LoadFromFile(FileInfo file)
        {
            Clear();
            try
            {
                var contents = File.ReadAllText(file.FullName);
                var json = JObject.Parse(contents);
                var version = (int?)json["Version"] ?? 0;
                var payload = (JObject?)json["Payload"];
                if (payload != null)
                {
                    DeserializeChildren(this, payload, version);
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
                JObject j = new();
                j.Add("Version", 1);
                j.Add("Payload", SerializeNode(this));
                File.WriteAllText(file.FullName, j.ToString());
            }
            catch (Exception e)
            {
                Service.Log($"Failed to save config to {file.FullName}: {e}");
            }
        }

        private JObject SerializeNode(ConfigNode n)
        {
            JObject children = new();
            foreach (var child in n.Children())
                children[child.GetType().FullName!] = SerializeNode(child);

            JObject j = JObject.FromObject(n);
            j["__children__"] = children;
            return j;
        }

        private static void DeserializeChildren(ConfigNode node, JObject json, int version)
        {
            if (version == 0 && node is BossModuleConfig)
                return; // children of this node are moved...

            var children = json["__children__"] as JObject;
            if (children == null)
                return;

            foreach ((var childTypeName, var jChild) in children)
            {
                var jChildObj = jChild as JObject;
                if (jChildObj == null)
                    continue;

                var realTypeName = version == 0 ? jChildObj["__type__"]?.ToString() : childTypeName;
                var childType = realTypeName != null ? Type.GetType(realTypeName) : null;
                if (childType == null)
                    continue;

                var child = jChildObj.ToObject(childType) as ConfigNode;
                if (child == null)
                    continue;

                node.AddChild(child);
                DeserializeChildren(child, jChildObj, version);
            }
        }
    }
}
