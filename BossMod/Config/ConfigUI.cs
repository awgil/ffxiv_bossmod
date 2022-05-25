using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BossMod
{
    public class ConfigUI
    {
        private class UINode
        {
            public ConfigNode Node;
            public string Name = "";
            public int Order;
            public UINode? Parent;
            public List<UINode> Children = new();

            public UINode(ConfigNode node)
            {
                Node = node;
            }
        }

        private List<UINode> _roots = new();
        private Tree _tree = new();

        public ConfigUI(ConfigRoot config)
        {
            Dictionary<Type, UINode> nodes = new();
            foreach (var n in config.Nodes)
            {
                nodes[n.GetType()] = new(n);
            }

            foreach (var (t, n) in nodes)
            {
                var props = Attribute.GetCustomAttribute(t, typeof(ConfigDisplayAttribute)) as ConfigDisplayAttribute;
                n.Name = props?.Name ?? GenerateNodeName(t);
                n.Order = props?.Order ?? 0;
                if (props?.Parent != null)
                    n.Parent = nodes.GetValueOrDefault(props.Parent);

                if (n.Parent != null)
                    n.Parent.Children.Add(n);
                else
                    _roots.Add(n);
            }

            SortByOrder(_roots);
        }

        public void Draw()
        {
            DrawNodes(_roots);
        }

        private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config") ? t.Name.Remove(t.Name.Length - "Config".Length) : t.Name;

        private void SortByOrder(List<UINode> nodes)
        {
            nodes.Sort((l, r) => l.Order.CompareTo(r.Order));
            foreach (var n in nodes)
                SortByOrder(n.Children);
        }

        private void DrawNodes(List<UINode> nodes)
        {
            foreach (var n in _tree.Nodes(nodes, n => new(n.Name)))
            {
                // draw standard properties
                foreach (var field in n.Node.GetType().GetFields())
                {
                    var props = Attribute.GetCustomAttribute(field, typeof(PropertyDisplayAttribute)) as PropertyDisplayAttribute;
                    if (props == null)
                        continue;

                    _ = field.GetValue(n.Node) switch
                    {
                        bool v => DrawProperty(props, n.Node, field, v),
                        Enum v => DrawProperty(props, n.Node, field, v),
                        float v => DrawProperty(props, n.Node, field, v),
                        _ => false
                    };
                }

                // draw custom stuff
                n.Node.DrawCustom(_tree);

                // draw subnodes
                DrawNodes(n.Children);
            }
        }

        private bool DrawProperty(PropertyDisplayAttribute props, ConfigNode node, FieldInfo member, bool v)
        {
            if (ImGui.Checkbox(props.Label, ref v))
            {
                member.SetValue(node, v);
                node.NotifyModified();
            }
            return true;
        }

        private bool DrawProperty(PropertyDisplayAttribute props, ConfigNode node, FieldInfo member, Enum v)
        {
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo(props.Label, v.ToString()))
            {
                foreach (var opt in Enum.GetValues(v.GetType()))
                {
                    if (ImGui.Selectable(opt.ToString(), opt.Equals(v)))
                    {
                        member.SetValue(node, opt);
                        node.NotifyModified();
                    }
                }
                ImGui.EndCombo();
            }
            return true;
        }

        private bool DrawProperty(PropertyDisplayAttribute props, ConfigNode node, FieldInfo member, float v)
        {
            var slider = Attribute.GetCustomAttribute(member, typeof(PropertySliderAttribute)) as PropertySliderAttribute;
            if (slider != null)
            {
                var flags = ImGuiSliderFlags.None;
                if (slider.Logarithmic)
                    flags |= ImGuiSliderFlags.Logarithmic;
                if (ImGui.DragFloat(props.Label, ref v, slider.Speed, slider.Min, slider.Max, "%.1f", flags))
                {
                    member.SetValue(node, v);
                    node.NotifyModified();
                }
            }
            else
            {
                if (ImGui.InputFloat(props.Label, ref v))
                {
                    member.SetValue(node, v);
                    node.NotifyModified();
                }
            }
            return true;
        }
    }
}
