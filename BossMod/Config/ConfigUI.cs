using BossMod.Autorotation;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using System.IO;
using System.Reflection;

namespace BossMod;

internal static class SettingsSearchHelper
{
    public static bool NodeMatchesSearch(ConfigUI.UINode node, string searchFilter)
    {
        var trimmedFilter = searchFilter?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(trimmedFilter))
            return true;

        return NodeHasDirectMatch(node, trimmedFilter) || HasMatchingChildren(node, trimmedFilter);
    }

    public static bool PropertyMatchesSearch(FieldInfo field, PropertyDisplayAttribute props, object? value, string searchFilter)
    {
        if (string.IsNullOrWhiteSpace(searchFilter))
            return false;

        // Check property label and tooltip
        if (props.Label?.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) == true ||
            props.Tooltip?.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) == true)
            return true;

        // Check property values
        if (value != null)
        {
            if (value is string strValue && strValue.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                return true;

            if (value is Enum enumValue && EnumMatchesSearch(enumValue, searchFilter))
                return true;

            var combo = field.GetCustomAttribute<PropertyComboAttribute>();
            if (combo?.Values != null && combo.Values.Any(v => v?.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) == true))
                return true;
        }

        return false;
    }

    public static int CountMatchesInNode(ConfigUI.UINode node, string searchFilter)
    {
        var trimmedFilter = searchFilter?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(trimmedFilter))
            return 0;

        int count = 0;

        if (node.Name.Contains(trimmedFilter, StringComparison.OrdinalIgnoreCase))
            count++;

        foreach (var field in node.Node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props != null)
            {
                if (props.Label?.Contains(trimmedFilter, StringComparison.OrdinalIgnoreCase) == true)
                    count++;

                if (props.Tooltip?.Contains(trimmedFilter, StringComparison.OrdinalIgnoreCase) == true)
                    count++;

                var value = field.GetValue(node.Node);
                if (value != null)
                {
                    if (value is string strValue && strValue.Contains(trimmedFilter, StringComparison.OrdinalIgnoreCase))
                        count++;

                    if (value is Enum enumValue)
                    {
                        if (enumValue.ToString().Contains(trimmedFilter, StringComparison.OrdinalIgnoreCase))
                            count++;

                        var enumType = enumValue.GetType();
                        var enumField = enumType.GetField(enumValue.ToString());
                        if (enumField != null)
                        {
                            var displayAttr = enumField.GetCustomAttribute<PropertyDisplayAttribute>();
                            if (displayAttr?.Label?.Contains(trimmedFilter, StringComparison.OrdinalIgnoreCase) == true)
                                count++;
                        }
                    }

                    var combo = field.GetCustomAttribute<PropertyComboAttribute>();
                    if (combo?.Values != null)
                    {
                        count += combo.Values.Count(v => v?.Contains(trimmedFilter, StringComparison.OrdinalIgnoreCase) == true);
                    }
                }
            }
        }

        return count;
    }

    private static bool NodeHasDirectMatch(ConfigUI.UINode node, string searchFilter)
    {
        if (node.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
            return true;

        return node.Node.GetType().GetFields()
            .Where(field => field.GetCustomAttribute<PropertyDisplayAttribute>() != null)
            .Any(field => PropertyMatchesSearch(field, field.GetCustomAttribute<PropertyDisplayAttribute>()!, field.GetValue(node.Node), searchFilter));
    }

    private static bool HasMatchingChildren(ConfigUI.UINode node, string searchFilter)
    {
        return node.Children.Any(child => NodeMatchesSearch(child, searchFilter));
    }

    private static bool EnumMatchesSearch(Enum enumValue, string searchFilter)
    {
        if (enumValue.ToString().Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
            return true;

        var enumType = enumValue.GetType();
        var enumField = enumType.GetField(enumValue.ToString());
        if (enumField != null)
        {
            var displayAttr = enumField.GetCustomAttribute<PropertyDisplayAttribute>();
            if (displayAttr?.Label?.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) == true)
                return true;
        }

        foreach (var possibleValue in Enum.GetValues(enumType))
        {
            var possibleName = possibleValue.ToString();
            if (possibleName != null && possibleName.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                return true;

            if (possibleName != null)
            {
                var possibleField = enumType.GetField(possibleName);
                if (possibleField != null)
                {
                    var possibleDisplay = possibleField.GetCustomAttribute<PropertyDisplayAttribute>();
                    if (possibleDisplay?.Label?.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) == true)
                        return true;
                }
            }
        }

        return false;
    }
}

public sealed class ConfigUI : IDisposable
{
    internal class UINode(ConfigNode node)
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
    private readonly AboutTab _about;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;
    private string _searchFilter = "";

    public ConfigUI(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _about = new(replayDir);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB.Presets) : null;

        _tabs.Add("About", _about.Draw);
        _tabs.Add("Settings", DrawSettings);
        _tabs.Add("Supported Bosses", () => _mv.Draw(_tree, _ws));
        _tabs.Add("Autorotation Presets", () => _presets?.Draw());

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
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
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
    }

    private void DrawSettings()
    {
        ImGui.SetNextItemWidth(300);
        ImGui.InputTextWithHint("##SettingsSearch", "Search settings...", ref _searchFilter, 256);
        ImGui.SameLine();
        if (ImGui.Button("Clear"))
            _searchFilter = "";

        if (!string.IsNullOrWhiteSpace(_searchFilter))
        {
            ImGui.SameLine();
            var matchCount = CountMatchingNodes(_roots);
            var matchText = matchCount == 1 ? "1 match found" : $"{matchCount} matches found";
            ImGui.TextColored(matchCount > 0 ? new System.Numerics.Vector4(0, 1, 0, 1) : new System.Numerics.Vector4(1, 0, 0, 1),
                $"({matchText})");
        }

        ImGui.Separator();

        if (string.IsNullOrWhiteSpace(_searchFilter))
        {
            DrawNodes(_roots);
        }
        else
        {
            DrawFilteredNodes(_roots);
        }
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws)
    {
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            var disabled = false;
            if (props.Depends is { } prop)
            {
                var dependsEnabled = node.GetType().GetField(prop)?.GetValue(node) switch
                {
                    bool v => v,
                    _ => throw new InvalidDataException($"Internal error: cannot use dependsOn with a non-bool field")
                };
                disabled = !dependsEnabled;
            }

            var value = field.GetValue(node);
            using (ImRaii.Disabled(disabled))
            {
                if (DrawProperty(props.Label, props.Tooltip, node, field, value, root, tree, ws))
                {
                    node.Modified.Fire();
                }
            }

            if (props.Separator)
            {
                ImGui.Separator();
            }
        }

        node.DrawCustom(tree, ws);
    }

    private void DrawNodeWithHighlighting(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, string searchFilter)
    {
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            var disabled = false;
            if (props.Depends is { } prop)
            {
                var dependsEnabled = node.GetType().GetField(prop)?.GetValue(node) switch
                {
                    bool v => v,
                    _ => throw new InvalidDataException($"Internal error: cannot use dependsOn with a non-bool field")
                };
                disabled = !dependsEnabled;
            }

            var value = field.GetValue(node);
            var propertyMatches = SettingsSearchHelper.PropertyMatchesSearch(field, props, value, searchFilter);

            if (propertyMatches)
            {
                var cursorPos = ImGui.GetCursorPos();
                ImGui.SetCursorPos(new System.Numerics.Vector2(cursorPos.X - 18, cursorPos.Y + 2));
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 1, 0, 1));
                UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.ArrowRight, "");
                ImGui.PopStyleColor();
                ImGui.SetCursorPos(cursorPos);
            }

            using (ImRaii.Disabled(disabled))
            {
                if (DrawProperty(props.Label, props.Tooltip, node, field, value, root, tree, ws))
                {
                    node.Modified.Fire();
                }
            }

            if (props.Separator)
            {
                ImGui.Separator();
            }
        }

        node.DrawCustom(tree, ws);
    }

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name[..^"Config".Length] : t.Name;

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

    private void DrawFilteredNodes(List<UINode> nodes)
    {
        var trimmedFilter = _searchFilter?.Trim() ?? "";
        foreach (var n in nodes)
        {
            if (SettingsSearchHelper.NodeMatchesSearch(n, trimmedFilter))
            {
                var color = n.Name.Contains(trimmedFilter, StringComparison.OrdinalIgnoreCase) ? 0xFFFFFF00u : 0xFFFFFFFFu;
                foreach (var _ in _tree.Node(n.Name, true, color))
                {
                    DrawNodeWithHighlighting(n.Node, _root, _tree, _ws, trimmedFilter);
                    DrawFilteredNodes(n.Children);
                }
            }
            else
            {
                DrawFilteredNodes(n.Children);
            }
        }
    }

    private int CountMatchingNodes(List<UINode> nodes)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            count += SettingsSearchHelper.CountMatchesInNode(node, _searchFilter);
            count += CountMatchingNodes(node.Children);
        }
        return count;
    }

    private static void DrawHelp(string tooltip)
    {
        // draw tooltip marker with proper alignment
        ImGui.AlignTextToFramePadding();
        if (tooltip.Length > 0)
        {
            UIMisc.HelpMarker(tooltip);
        }
        else
        {
            using var invisible = ImRaii.PushColor(ImGuiCol.Text, 0x00000000);
            UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle, "(?)");
        }
        ImGui.SameLine();
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, object? value, ConfigRoot root, UITree tree, WorldState ws) => value switch
    {
        bool v => DrawProperty(label, tooltip, node, member, v),
        Enum v => DrawProperty(label, tooltip, node, member, v),
        float v => DrawProperty(label, tooltip, node, member, v),
        int v => DrawProperty(label, tooltip, node, member, v),
        string v => DrawProperty(label, tooltip, node, member, v),
        Color v => DrawProperty(label, tooltip, node, member, v),
        Color[] v => DrawProperty(label, tooltip, node, member, v),
        GroupAssignment v => DrawProperty(label, tooltip, node, member, v, root, tree, ws),
        _ => false
    };

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, bool v)
    {
        DrawHelp(tooltip);
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

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Enum v)
    {
        DrawHelp(tooltip);
        if (UICombo.Enum(label, ref v))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, float v)
    {
        DrawHelp(tooltip);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            ImGui.SetNextItemWidth(MathF.Min(ImGui.GetWindowWidth() * 0.30f, 175));
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

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, int v)
    {
        DrawHelp(tooltip);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            ImGui.SetNextItemWidth(MathF.Min(ImGui.GetWindowWidth() * 0.30f, 175));
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

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, string v)
    {
        DrawHelp(tooltip);
        if (ImGui.InputText(label, ref v, 256))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Color v)
    {
        DrawHelp(tooltip);
        var col = v.ToFloat4();
        if (ImGui.ColorEdit4(label, ref col, ImGuiColorEditFlags.PickerHueWheel))
        {
            member.SetValue(node, Color.FromFloat4(col));
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Color[] v)
    {
        var modified = false;
        for (int i = 0; i < v.Length; ++i)
        {
            DrawHelp(tooltip);
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

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, GroupAssignment v, ConfigRoot root, UITree tree, WorldState ws)
    {
        var group = member.GetCustomAttribute<GroupDetailsAttribute>();
        if (group == null)
            return false;

        DrawHelp(tooltip);
        var modified = false;
        foreach (var tn in tree.Node(label, false, v.Validate() ? 0xffffffff : 0xff00ffff, () => DrawPropertyContextMenu(node, member, v)))
        {
            using var indent = ImRaii.PushIndent();
            using var table = ImRaii.Table("table", group.Names.Length + 2, ImGuiTableFlags.SizingFixedFit);
            if (!table)
                continue;

            foreach (var n in group.Names)
                ImGui.TableSetupColumn(n);
            ImGui.TableSetupColumn("----");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
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
