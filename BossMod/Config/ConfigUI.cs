using BossMod.Autorotation;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Reflection;
using System.Numerics;

namespace BossMod;

public sealed class ConfigUI : IDisposable
{
    private class UINode
    {
        public ConfigNode Node;
        public string Name;
        public int Order;
        public UINode? Parent;
        public List<UINode> Children = new();

        public UINode(ConfigNode node)
        {
            Node = node;
            Name = string.Empty;
        }
    }

    private readonly List<UINode> _roots = new();
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;

    public ConfigUI(ConfigRoot config, WorldState ws, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB.Presets) : null;

        _tabs.Add("About", ConfigAboutTab.Draw);
        _tabs.Add("Settings", DrawSettings);
        _tabs.Add("Supported Duties", () => _mv.Draw(_tree, _ws));
        _tabs.Add("Autorotation Presets", () => _presets?.Draw());

        var nodes = new Dictionary<Type, UINode>();
        foreach (var n in config.Nodes)
        {
            nodes[n.GetType()] = new(n);
        }

        foreach (var (t, n) in nodes)
        {
            var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;

            if (n.Parent != null)
            {
                n.Parent.Children.Add(n);
            }
            else
            {
                _roots.Add(n);
            }
        }

        SortByOrder(_roots);
    }

    public void Dispose()
    {
        _mv.Dispose();
    }

    public void ShowTab(string name) => _tabs.Select(name);

    public void Draw()
    {
        var style = ImGui.GetStyle();
        var originalStyle = new ImGuiStyleSnapshot(style);

        ApplyCustomStyling();

        _tabs.Draw();

        originalStyle.Restore(style);
    }

    private void DrawSettings()
    {
        if (ImGui.BeginChild("SettingsWindow", new Vector2(0, 0), true))
        {
            foreach (var node in _tree.Nodes(_roots, n => new(n.Name)))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                DrawNode(node.Node, _root, _tree, _ws);
                DrawNodes(node.Children);
            }
            ImGui.EndChild();
        }
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws)
    {
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            var cursor = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(cursor + ImGui.GetStyle().FramePadding.Y);
            if (!string.IsNullOrEmpty(props.Tooltip))
            {
                UIMisc.HelpMarker(props.Tooltip);
            }
            else
            {
                using var invisible = ImRaii.PushColor(ImGuiCol.Text, 0x00000000);
                UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle, "(?)");
            }
            ImGui.SameLine();
            ImGui.SetCursorPosY(cursor);

            var value = field.GetValue(node);
            if (DrawProperty(props.Label, node, field, value, root, tree, ws))
            {
                node.Modified.Fire();
            }
        }

        node.DrawCustom(tree, ws);
    }

    private static string GenerateNodeName(Type t)
    {
        return t.Name.EndsWith("Config", StringComparison.Ordinal)
            ? t.Name[..^"Config".Length]
            : t.Name;
    }

    private void SortByOrder(List<UINode> nodes)
    {
        nodes.Sort((a, b) => a.Order.CompareTo(b.Order));
        foreach (var n in nodes)
        {
            SortByOrder(n.Children);
        }
    }

    private void DrawNodes(List<UINode> nodes)
    {
        foreach (var n in _tree.Nodes(nodes, n => new(n.Name)))
        {
            DrawNode(n.Node, _root, _tree, _ws);
            DrawNodes(n.Children);
        }
    }

    private static bool DrawProperty(string label, ConfigNode node, FieldInfo member, object? value, ConfigRoot root, UITree tree, WorldState ws) => value switch
    {
        bool v => DrawProperty(label, node, member, v),
        Enum v => DrawProperty(label, node, member, v),
        float v => DrawProperty(label, node, member, v),
        int v => DrawProperty(label, node, member, v),
        Color v => DrawProperty(label, node, member, v),
        Color[] v => DrawProperty(label, node, member, v),
        GroupAssignment v => DrawProperty(label, node, member, v, root, tree, ws),
        _ => false
    };

    private static bool DrawProperty(string label, ConfigNode node, FieldInfo member, bool v)
    {
        var combo = member.GetCustomAttribute<PropertyComboAttribute>();
        if (combo != null)
        {
            if (UICombo.Bool(label, combo.Values, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.Checkbox(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, ConfigNode node, FieldInfo member, Enum v)
    {
        if (UICombo.Enum(label, ref v))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, ConfigNode node, FieldInfo member, float v)
    {
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            if (ImGui.DragFloat(label, ref v, slider.Speed, slider.Min, slider.Max, "%.3f", flags))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputFloat(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, ConfigNode node, FieldInfo member, int v)
    {
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            if (ImGui.DragInt(label, ref v, slider.Speed, (int)slider.Min, (int)slider.Max, "%d", flags))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputInt(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, ConfigNode node, FieldInfo member, Color v)
    {
        var col = v.ToFloat4();
        if (ImGui.ColorEdit4(label, ref col, ImGuiColorEditFlags.PickerHueWheel))
        {
            member.SetValue(node, Color.FromFloat4(col));
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, ConfigNode node, FieldInfo member, Color[] v)
    {
        var modified = false;
        for (int i = 0; i < v.Length; ++i)
        {
            var col = v[i].ToFloat4();
            if (ImGui.ColorEdit4($"{label} {i}", ref col, ImGuiColorEditFlags.PickerHueWheel))
            {
                v[i] = Color.FromFloat4(col);
                member.SetValue(node, v);
                modified = true;
            }
        }
        return modified;
    }

    private static bool DrawProperty(string label, ConfigNode node, FieldInfo member, GroupAssignment v, ConfigRoot root, UITree tree, WorldState ws)
    {
        var group = member.GetCustomAttribute<GroupDetailsAttribute>();
        if (group == null)
            return false;

        var modified = false;
        foreach (var tn in tree.Node(label, false, v.Validate() ? 0xffffffff : 0xff00ffff, () => DrawPropertyContextMenu(node, member, v)))
        {
            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
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
                            modified = true;
                        }
                    }
                    ImGui.TableNextColumn();
                    if (ImGui.RadioButton($"###{r}:---", v[r] < 0 || v[r] >= group.Names.Length))
                    {
                        v[r] = -1;
                        modified = true;
                    }

                    string name = r.ToString();
                    if (assignments.Length > 0)
                        name += $" ({ws.Party[assignments[i]]?.Name})";
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(name);
                }
                ImGui.EndTable();
            }
        }
        return modified;
    }

    private static void DrawPropertyContextMenu(ConfigNode node, FieldInfo member, GroupAssignment v)
    {
        foreach (var preset in member.GetCustomAttributes<GroupPresetAttribute>())
        {
            if (ImGui.MenuItem(preset.Name))
            {
                for (int i = 0; i < preset.Preset.Length; ++i)
                    v.Assignments[i] = preset.Preset[i];
                node.Modified.Fire();
            }
        }
    }

    private void ApplyCustomStyling()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        style.FramePadding = new Vector2(8, 4);
        style.ItemSpacing = new Vector2(10, 6);
        style.WindowPadding = new Vector2(10, 10);
        style.FrameRounding = 4.0f;
        style.GrabRounding = 4.0f;

        style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.15f, 0.15f, 0.15f, 1.0f);
        style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.0f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.09f, 0.30f, 0.50f, 1.0f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.12f, 0.40f, 0.60f, 1.0f);
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.0f, 0.48f, 0.8f, 0.6f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.0f, 0.60f, 0.9f, 0.8f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.0f, 0.72f, 1.0f, 1.0f);
        style.Colors[(int)ImGuiCol.Header] = new Vector4(0.0f, 0.48f, 0.8f, 0.7f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.0f, 0.60f, 0.9f, 0.8f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.0f, 0.72f, 1.0f, 0.9f);
    }
}

public class ImGuiStyleSnapshot
{
    public Vector2 FramePadding { get; }
    public Vector2 ItemSpacing { get; }
    public Vector2 WindowPadding { get; }
    public float FrameRounding { get; }
    public float GrabRounding { get; }
    public Vector4[] Colors { get; }

    public ImGuiStyleSnapshot(ImGuiStylePtr style)
    {
        FramePadding = style.FramePadding;
        ItemSpacing = style.ItemSpacing;
        WindowPadding = style.WindowPadding;
        FrameRounding = style.FrameRounding;
        GrabRounding = style.GrabRounding;

        Colors = new Vector4[style.Colors.Count];
        for (int i = 0; i < style.Colors.Count; i++)
        {
            Colors[i] = style.Colors[i];
        }
    }

    public void Restore(ImGuiStylePtr style)
    {
        style.FramePadding = FramePadding;
        style.ItemSpacing = ItemSpacing;
        style.WindowPadding = WindowPadding;
        style.FrameRounding = FrameRounding;
        style.GrabRounding = GrabRounding;

        for (int i = 0; i < Colors.Length; i++)
        {
            style.Colors[i] = Colors[i];
        }
    }
}
