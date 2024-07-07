using BossMod.Autorotation;
using System.Diagnostics;
using ImGuiNET;
using System.Reflection;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public sealed class ConfigUI : IDisposable
{
    private class UINode(ConfigNode node)
    {
        public ConfigNode Node = node;
        public string Name = "";
        public int Order;
        public UINode? Parent;
        public List<UINode> Children = [];
    }

    private readonly List<UINode> _roots = [];
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
        _tabs.Add("Configs", () => DrawNodes(_roots));
        _tabs.Add("Modules", () => _mv.Draw(_tree, _ws));
        _tabs.Add("Presets", () => _presets?.Draw());

        Dictionary<Type, UINode> nodes = [];
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

    public void Dispose()
    {
        _mv.Dispose();
    }

    public void ShowTab(string name) => _tabs.Select(name);

    public void Draw()
    {
        _tabs.Draw();
        using var tabs = ImRaii.TabBar("Tabs");
        if (tabs)
        {
            using (var tab = ImRaii.TabItem("Slash commands"))
                if (tab)
                    DrawAvailableCommands();
            using (var tab = ImRaii.TabItem("Read me!"))
                if (tab)
                    DrawReadMe();
        }
    }

    private static readonly Dictionary<string, string> _availableAICommands = new()
    {
        { "on", "Enables the AI." },
        { "off", "Disables the AI." },
        { "toggle", "Toggles the AI on/off." },
        { "targetmaster", "Toggles the focus on target leader." },
        { "follow slotX", "Follows the specified slot, eg. Slot1." },
        { "follow name", "Follows the specified party member by name." },
        { "ui", "Toggles the AI menu." },
        { "forbidactions", "Toggles the forbidding of actions. (only for autorotation)" },
        { "forbidmovement", "Toggles the forbidding of movement." },
        { "followcombat", "Toggles following during combat." },
        { "followmodule", "Toggles following during active boss module." },
        { "followoutofcombat", "Toggles following during out of combat." },
        { "followtarget", "Toggles following targets during combat." },
        { "followtarget on/off", "Sets following target during combat to on or off." },
        { "positional X", "Switch to positional when following targets. (any, rear, flank, front)" }
    };

    private static readonly Dictionary<string, string> _autorotationCommands = new()
    {
        { "ar", "Lists all autorotation commands." },
        { "ar clear", "Clear current preset; autorotation will do nothing unless plan is active" },
        { "ar set Preset", "Force disable autorotation; no actions will be executed automatically even if plan is active." },
        { "ar toggle", "Force disable autorotation if not already; otherwise clear overrides." },
        { "ar toggle Preset", "Start executing specified preset unless it's already active; clear otherwise" },
    };

    private static readonly Dictionary<string, string> _availableOtherCommands = new()
    {
        { "d", "Opens the debug menu." },
        { "r", "Opens the replay menu." },
        { "gc", "Triggers the garbage collection." },
        { "cfg", "Lists all configs." }
    };

    private void DrawAvailableCommands()
    {
        ImGui.Text("Available Commands:");
        ImGui.Separator();
        ImGui.Text("AI:");
        ImGui.Separator();
        foreach (var command in _availableAICommands)
        {
            ImGui.Text($"/bmrai {command.Key}: {command.Value}");
        }
        ImGui.Separator();
        ImGui.Text("Autorotation commands:");
        ImGui.Separator();
        foreach (var command in _autorotationCommands)
        {
            ImGui.Text($"/bmr {command.Key}: {command.Value}");
        }
        ImGui.Separator();
        ImGui.Text("Other commands:");
        ImGui.Separator();
        foreach (var command in _availableOtherCommands)
        {
            ImGui.Text($"/bmr {command.Key}: {command.Value}");
        }
        ImGui.Separator();
        ImGui.Text("/vbm can be used instead of /bmr and /vbmai can be used instead of /bmrai");
    }

    private void DrawReadMe()
    {
        ImGui.Text("Important information");
        ImGui.Separator();
        ImGui.Text("This is a FORK of veyn's BossMod (VBM).");
        ImGui.Spacing();
        ImGui.Text("Please do not ask him for any support for problems you encounter while using this fork.");
        ImGui.Spacing();
        ImGui.Text("Instead visit the Combat Reborn Discord and ask for support there:");
        RenderTextWithLink("https://discord.gg/p54TZMPnC9", new Uri("https://discord.gg/p54TZMPnC9"));
        ImGui.NewLine();
        ImGui.Text("Please also make sure to not load VBM and this fork at the same time.");
        ImGui.Spacing();
        ImGui.Text("The consequences of doing that are unexplored and unsupported.");
        ImGui.Separator();
        ImGui.Text("The AI is designed for legacy movement, make sure to turn on legacy movement\nwhile using AI.");
        ImGui.Spacing();
        ImGui.Text("It is advised to pause AutoDuty during boss modules since it can conflict with BMR AI.");
    }

    static void RenderTextWithLink(string displayText, Uri url)
    {
        ImGui.PushID(url.ToString());
        ImGui.Text(displayText);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
        }
        var textSize = ImGui.CalcTextSize(displayText);
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        drawList.AddLine(cursorPos, new Vector2(cursorPos.X + textSize.X, cursorPos.Y), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 1, 1)));
        ImGui.PopID();
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws)
    {
        // draw standard properties
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            var value = field.GetValue(node);
            if (DrawProperty(props.Label, node, field, value, root, tree, ws))
            {
                node.Modified.Fire();
            }
        }

        // draw custom stuff
        node.DrawCustom(tree, ws);
    }

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name.Remove(t.Name.Length - "Config".Length) : t.Name;

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
}
