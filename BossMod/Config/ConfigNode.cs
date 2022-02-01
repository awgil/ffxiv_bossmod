using ImGuiNET;
using System;
using System.Collections.Generic;

namespace BossMod
{
    // base class for configuration nodes
    // configuration is split into hierarchical structure to allow storing options next to classes they're relevant to
    class ConfigNode
    {
        protected ConfigNode? Parent;
        private Dictionary<string, ConfigNode> _children = new();
        public IReadOnlyDictionary<string, ConfigNode> Children() => _children;

        // get child node of specified type with specified name; if one doesn't exist or is of incorrect type, new child is created (old one with same name is then removed)
        public T Get<T>(string name) where T : ConfigNode, new()
        {
            var child = _children.GetValueOrDefault(name) as T;
            if (child == null)
            {
                var inserted = _children[name] = new T();
                child = inserted as T;
                child!.Parent = this;
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
            foreach ((var name, var child) in _children)
            {
                if (ImGui.TreeNode(child.NameOverride() ?? name))
                {
                    child.Draw();
                    ImGui.TreePop();
                }
            }
            DrawContents();
        }

        public void AddDeserializedChild(string name, ConfigNode node)
        {
            node.Parent = this;
            _children[name] = node;
        }

        // save state; should be called by derived classes whenever they make any modifications
        // default implementation just forwards request to parent; root should handle actual saving
        protected virtual void Save()
        {
            Parent?.Save();
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
