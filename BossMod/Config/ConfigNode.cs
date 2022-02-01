using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BossMod
{
    // base class for configuration nodes
    // configuration is split into hierarchical structure to allow storing options next to classes they're relevant to
    class ConfigNode
    {
        protected ConfigNode? Parent;

        [JsonProperty]
        private Dictionary<string, object> Children = new();

        [JsonProperty]
        private string? _t;

        protected void FixAfterDeserialize()
        {
            Dictionary<string, object> fix = new();
            foreach ((string name, var child) in Children)
            {
                var serialized = child as Newtonsoft.Json.Linq.JObject;
                if (serialized == null)
                    continue;

                var type = Type.GetType(serialized["_t"]?.ToString() ?? "");
                if (type == null)
                    continue;

                var deserialized = serialized.ToObject(type) as ConfigNode;
                if (deserialized == null)
                    continue;

                deserialized.Parent = this;
                deserialized.FixAfterDeserialize();
                fix[name] = deserialized;
            }
            Children = fix;
        }

        // get child node of specified type with specified name; if one doesn't exist or is of incorrect type, new child is created (old one with same name is then removed)
        public T Get<T>(string name) where T : ConfigNode, new()
        {
            var child = Children.GetValueOrDefault(name) as T;
            if (child == null)
            {
                var inserted = Children[name] = new T();
                child = inserted as T;
                child!.Parent = this;
                child!._t = typeof(T).FullName;
            }
            return child!;
        }

        // get generic child node with specified name
        public ConfigNode Get(string name)
        {
            return Get<ConfigNode>(name);
        }

        // get child node of specified type with name matching type name
        public T Get<T>() where T : ConfigNode, new()
        {
            return Get<T>(typeof(T).Name);
        }

        // draw this node and all children
        public void Draw()
        {
            foreach ((var name, var child) in Children)
            {
                var castChild = (ConfigNode)child;
                if (ImGui.TreeNode(castChild.NameOverride() ?? name))
                {
                    castChild.Draw();
                    ImGui.TreePop();
                }
            }
            DrawContents();
        }

        // save state; should be called by derived classes whenever they make any modifications
        // default implementation just forwards request to parent; root should handle actual saving
        protected virtual void Save()
        {
            Parent!.Save();
        }

        // draw actual node contents; at very least leaves should override this to draw something useful
        protected virtual void DrawContents() { }

        // return human-readable name override for the node
        protected virtual string? NameOverride() { return null; }

        // draw common property types
        protected void DrawProperty(ref bool v, string label)
        {
            if (ImGui.Checkbox(label, ref v))
                Save();
        }
    }
}
