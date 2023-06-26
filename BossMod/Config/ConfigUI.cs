using ImGuiNET;
using System;
using System.Collections.Generic;
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
        private UITree _tree = new();
        private ConfigRoot _root;
        private WorldState _ws;

        public ConfigUI(ConfigRoot config, WorldState ws)
        {
            _root = config;
            _ws = ws;

            Dictionary<Type, UINode> nodes = new();
            foreach (var n in config.Nodes)
            {
                nodes[n.GetType()] = new(n);
            }

            foreach (var (t, n) in nodes)
            {
                var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
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
            nodes.SortBy(e => e.Order);
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
                    var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
                    if (props == null)
                        continue;

                    _ = field.GetValue(n.Node) switch
                    {
                        bool v => DrawProperty(props, n.Node, field, v),
                        Enum v => DrawProperty(props, n.Node, field, v),
                        float v => DrawProperty(props, n.Node, field, v),
                        GroupAssignment v => DrawProperty(props, n.Node, field, v),
                        _ => false
                    };
                }

                // draw custom stuff
                n.Node.DrawCustom(_tree, _ws);

                // draw subnodes
                DrawNodes(n.Children);
            }
        }

        private bool DrawProperty(PropertyDisplayAttribute props, ConfigNode node, FieldInfo member, bool v)
        {
            var combo = member.GetCustomAttribute<PropertyComboAttribute>();
            if (combo != null)
            {
                if (UICombo.Bool(props.Label, combo.Values, ref v))
                {
                    member.SetValue(node, v);
                    node.NotifyModified();
                }
            }
            else
            {
                if (ImGui.Checkbox(props.Label, ref v))
                {
                    member.SetValue(node, v);
                    node.NotifyModified();
                }
            }
            return true;
        }

        private bool DrawProperty(PropertyDisplayAttribute props, ConfigNode node, FieldInfo member, Enum v)
        {
            if (UICombo.Enum(props.Label, ref v))
            {
                member.SetValue(node, v);
                node.NotifyModified();
            }
            return true;
        }

        private bool DrawProperty(PropertyDisplayAttribute props, ConfigNode node, FieldInfo member, float v)
        {
            var slider = member.GetCustomAttribute<PropertySliderAttribute>();
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

        private bool DrawProperty(PropertyDisplayAttribute props, ConfigNode node, FieldInfo member, GroupAssignment v)
        {
            var group = member.GetCustomAttribute<GroupDetailsAttribute>();
            if (group == null)
                return false;

            foreach (var tn in _tree.Node(props.Label, false, v.Validate() ? 0xffffffff : 0xff00ffff, () => DrawPropertyContextMenu(node, member, v)))
            {
                var assignments = _root.Get<PartyRolesConfig>().SlotsPerAssignment(_ws.Party);
                if (ImGui.BeginTable("table", group.Names.Length + 2, ImGuiTableFlags.SizingFixedFit))
                {
                    foreach (var n in group.Names)
                        ImGui.TableSetupColumn(n);
                    ImGui.TableSetupColumn("----");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableHeadersRow();
                    for (int i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; ++i)
                    {
                        var r = (PartyRolesConfig.Assignment)i;
                        ImGui.TableNextRow();
                        for (int c = 0; c < group.Names.Length; ++c)
                        {
                            ImGui.TableNextColumn();
                            if (ImGui.RadioButton($"###{r}:{c}", v[r] == c))
                            {
                                v[r] = c;
                                node.NotifyModified();
                            }
                        }
                        ImGui.TableNextColumn();
                        if (ImGui.RadioButton($"###{r}:---", v[r] < 0 || v[r] >= group.Names.Length))
                        {
                            v[r] = -1;
                            node.NotifyModified();
                        }

                        string name = r.ToString();
                        if (assignments.Length > 0)
                            name += $" ({_ws.Party[assignments[i]]?.Name})";
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(name);
                    }
                    ImGui.EndTable();
                }
            }
            return true;
        }

        private void DrawPropertyContextMenu(ConfigNode node, FieldInfo member, GroupAssignment v)
        {
            foreach (var preset in member.GetCustomAttributes<GroupPresetAttribute>())
            {
                if (ImGui.MenuItem(preset.Name))
                {
                    for (int i = 0; i < preset.Preset.Length; ++i)
                        v.Assignments[i] = preset.Preset[i];
                    node.NotifyModified();
                }
            }
        }
    }
}
