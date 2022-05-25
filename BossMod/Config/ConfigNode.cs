using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // base class for configuration nodes
    // configuration is split into hierarchical structure to allow storing options next to classes they're relevant to
    public class ConfigNode
    {
        public event EventHandler? Modified;
        protected ConfigNode? Parent;
        protected string? DisplayName;
        protected int DisplayOrder;
        protected bool Visible = true;

        private List<ConfigNode> _children = new();
        public IReadOnlyList<ConfigNode> Children() => _children;

        // get child node of specified type with name matching type name
        // if one doesn't exist or is of incorrect type, new child is created (old one with same name is then removed)
        public T Get<T>() where T : ConfigNode, new()
        {
            var child = _children.OfType<T>().FirstOrDefault();
            if (child != null)
                return child;

            child = new();
            AddChild(child);
            return child;
        }

        protected void Clear()
        {
            _children.Clear();
        }

        // draw this node and all children
        public void Draw()
        {
            DrawContents();
            foreach (var child in _children)
            {
                if (child.Visible && ImGui.TreeNode(child.DisplayName ?? child.GetType().Name))
                {
                    child.Draw();
                    ImGui.TreePop();
                }
            }
        }

        public void AddChild(ConfigNode child)
        {
            child.Parent = this;
            if (child.DisplayName == null)
            {
                child.DisplayName = child.GetType().Name;
                if (child.DisplayName.EndsWith("Config"))
                    child.DisplayName = child.DisplayName.Remove(child.DisplayName.Length - "Config".Length);
            }
            _children.Add(child);
            _children.Sort((l, r) => l.DisplayOrder.CompareTo(r.DisplayOrder));
        }

        // notify that configuration node was modified; should be called by derived classes whenever they make any modifications
        // implementation dispatches modification event and forwards request to parent
        // root should subscribe to modification event to save updated configuration
        protected void NotifyModified()
        {
            Modified?.Invoke(this, EventArgs.Empty);
            Parent?.NotifyModified();
        }

        // draw actual node contents; at very least leaves should override this to draw something useful
        protected virtual void DrawContents() { }

        // draw common property types
        protected void DrawProperty(ref bool v, string label)
        {
            if (ImGui.Checkbox(label, ref v))
                NotifyModified();
        }

        protected void DrawProperty<E>(ref E v, string label) where E : struct, Enum
        {
            ImGui.SetNextItemWidth(100);
            if (ImGui.BeginCombo(label, v.ToString()))
            {
                foreach (var opt in Enum.GetValues<E>())
                {
                    if (ImGui.Selectable(opt.ToString(), opt.Equals(v)))
                    {
                        v = opt;
                        NotifyModified();
                    }
                }
                ImGui.EndCombo();
            }
        }
    }
}
