using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.IO;
using System.Reflection;

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
        public string[] Tags = [];

        public List<string> Path = [];
    }

    private readonly List<UINode> _roots = [];
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();
    private readonly AboutTab _about;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;

    private readonly List<List<string>> _filterNodes = [];

    public ConfigUI(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _about = new(replayDir);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB) : null;

        _tabs.Add("Settings", DrawSettings);
        _tabs.Add("Supported fights", () => _mv.Draw(_tree, _ws));
        _tabs.Add("Autorotation presets", () => _presets?.Draw());
        _tabs.Add("Slash commands", DrawAvailableCommands);
        _tabs.Add("About", _about.Draw);

        Dictionary<Type, UINode> nodes = [];
        var nodes2 = _root.Nodes;
        for (var i = 0; i < nodes2.Count; ++i)
        {
            var n = nodes2[i];
            nodes[n.GetType()] = new(n);
        }

        foreach (var (t, n) in nodes)
        {
            var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;
            n.Tags = props?.Tags ?? [];

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
        }

        SortByOrder(_roots);
        ResolvePaths(_roots, []);
    }

    private void ResolvePaths(List<UINode> nodes, IEnumerable<string> parent)
    {
        foreach (var n in nodes)
        {
            n.Path = [.. parent, n.Name];
            ResolvePaths(n.Children, n.Path);
        }
    }

    public void Dispose() => _mv.Dispose();

    public void ShowTab(string name) => _tabs.Select(name);

    public void Draw() => _tabs.Draw();

    private string _searchText = "";

    private void DrawSettings()
    {
        ImGui.SetNextItemWidth(300);
        if (ImGui.InputTextEx("", "Search for a setting...", ref _searchText))
        {
            FilterNodes();
        }

        ImGui.SameLine();
        using (ImRaii.Disabled(_searchText.Length == 0))
        {
            if (ImGui.Button("Clear"))
            {
                _searchText = "";
                FilterNodes();
            }
        }

        DrawNodes(_roots);
    }

    private static readonly (string, string)[] _availableAICommands =
    [
        ( "on", "Enables the AI." ),
        ( "off", "Disables the AI." ),
        ( "toggle", "Toggles the AI on/off." ),
        ( "targetmaster", "Toggles the focus on target leader." ),
        ( "follow slotX", "Follows the specified slot, eg. Slot1." ),
        ( "follow name", "Follows the specified party member by name." ),
        ( "ui", "Toggles the AI menu." ),
        ( "forbidactions", "Toggles the forbidding of actions. (only for autorotation)" ),
        ( "forbidactions on/off", "Sets forbid actions to on or off. (only for autorotation)" ),
        ( "forbidmovement", "Toggles the forbidding of movement." ),
        ( "forbidmovement on/off", "Sets forbid movement to on or off." ),
        ( "idlewhilemounted", "Toggles the idling while mounted." ),
        ( "idlewhilemounted on/off", "Sets idle while mounted to on or off." ),
        ( "followcombat", "Toggles following during combat." ),
        ( "followcombat on/off", "Sets following following during combat to on or off." ),
        ( "followmodule", "Toggles following during active boss module." ),
        ( "followmodule on/off", "Sets following following during active boss module to on or off." ),
        ( "followoutofcombat", "Toggles following during out of combat." ),
        ( "followoutofcombat on/off", "Sets following target out of combat to on or off." ),
        ( "followtarget", "Toggles following targets during combat." ),
        ( "followtarget on/off", "Sets following target during combat to on or off." ),
        ( "positional X", "Switch to positional when following targets. (any, rear, flank, front)" ),
        ( "maxdistancetarget X", "Sets max distance to target. (default = 2.6)" ),
        ( "maxdistanceslot X", "Sets max distance to slot. (default = 1)" ),
        ( "mindistance X", "Sets min distance to hitbox. (default = 0)" ),
        ( "prefdistance X", "Sets preferred distance to forbidden zones. (default = 0)" ),
        ( "movedelay X", "Sets AI movement decision delay. (default = 0)" ),
        ( "obstaclemaps", "Toggles loading obstacle maps." ),
        ( "obstaclemaps on/off", "Sets the loading of obstacle maps to on or off." ),
        ( "setpresetname X", "Sets an autorotation preset for the AI, eg. setpresetname vbm default." )
    ];

    private static readonly (string, string)[] _autorotationCommands =
    [
        ( "ar clear", "Clear current preset; autorotation will do nothing unless plan is active" ),
        ( "ar disable", "Force disable autorotation; no actions will be executed automatically even if plan is active." ),
        ( "ar set Preset", "Start executing specified preset." ),
        ( "ar toggle", "Force disable autorotation if not already; otherwise clear overrides." ),
        ( "ar toggle Preset", "Start executing specified preset unless it's already active; clear otherwise" ),
        ( "ar ui", "Toggle autorotation ui." ),
    ];

    private static readonly (string, string)[] _availableOtherCommands =
    [
        ( "restorerotation", "Toggle restore character orientation after action use setting." ),
        ( "resetcolors", "Resets all colors to their default values." ),
        ( "d", "Opens the debug menu." ),
        ( "r", "Opens the replay menu." ),
        ( "r on/off", "Starts/stops recording a replay." ),
        ( "gc", "Triggers the garbage collection." ),
        ( "radar", "toggles radar display" ),
        ( "radar on/off", "Sets radar display to on or off." ),
        ( "cfg", "Lists all configs." )
    ];

    private static void DrawAvailableCommands()
    {
        ImGui.Text("Available Commands:");
        ImGui.Separator();
        ImGui.Text("AI:");
        ImGui.Separator();
        for (var i = 0; i < 30; ++i)
        {
            ref readonly var text = ref _availableAICommands[i];
            ImGui.Text($"/bmrai {text.Item1}: {text.Item2}");
        }
        ImGui.Separator();
        ImGui.Text("Autorotation commands:");
        ImGui.Separator();
        for (var i = 0; i < 6; ++i)
        {
            ref readonly var text = ref _autorotationCommands[i];
            ImGui.Text($"/bmr {text.Item1}: {text.Item2}");
        }
        ImGui.Separator();
        ImGui.Text("Other commands:");
        ImGui.Separator();
        for (var i = 0; i < 9; ++i)
        {
            ref readonly var text = ref _availableOtherCommands[i];
            ImGui.Text($"/bmr {text.Item1}: {text.Item2}");
        }
    }

    private void FilterNodes()
    {
        _filterNodes.Clear();

        if (_searchText.Length == 0)
        {
            return;
        }

        foreach (var r in _roots)
        {
            foreach (var path in WalkNodes(r))
            {
                _filterNodes.Add(path);
            }
        }
    }

    private static readonly Dictionary<Type, List<(FieldInfo Field, PropertyDisplayAttribute Attr)>> _fieldCache = [];

    private List<List<string>> WalkNodes(UINode node)
    {
        var results = new List<List<string>>();
        WalkNodesInternal(node, [], results);
        return results;
    }

    private void WalkNodesInternal(UINode node, List<string> path, List<List<string>> results)
    {
        if (Utils.TextMatch(node.Name, _searchText) || TagsMatch(node.Tags))
        {
            var matchPath = new List<string>(path) { node.Name, "*" };
            results.Add(matchPath);
            return;
        }

        foreach (var (_, props) in GetFieldAttributes(node.Node.GetType()))
        {
            if (Utils.TextMatch(props.Label, _searchText) || TagsMatch(props.Tags))
            {
                var matchPath = new List<string>(path) { node.Name, props.Label };
                results.Add(matchPath);
            }
        }

        path.Add(node.Name);
        foreach (var child in node.Children)
        {
            WalkNodesInternal(child, path, results);
        }
        path.RemoveAt(path.Count - 1);
    }

    private static List<(FieldInfo, PropertyDisplayAttribute)> GetFieldAttributes(Type type)
    {
        if (_fieldCache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        var list = new List<(FieldInfo, PropertyDisplayAttribute)>();
        foreach (var field in type.GetFields())
        {
            var attr = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (attr != null)
            {
                list.Add((field, attr));
            }
        }

        _fieldCache[type] = list;
        return list;
    }

    private bool TagsMatch(string[] tags)
    {
        foreach (var tag in tags)
        {
            if (Utils.TextMatch(tag, _searchText))
            {
                return true;
            }
        }
        return false;
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, Func<PropertyDisplayAttribute, bool>? filter = null)
    {
        // draw standard properties
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
            {
                continue;
            }

            if (filter?.Invoke(props) == false)
            {
                continue;
            }

            var value = field.GetValue(node);
            if (DrawProperty(props.Label, props.Tooltip, node, field, value, root, tree, ws))
            {
                node.Modified.Fire();
            }

            if (props.Separator)
            {
                ImGui.Separator();
            }
        }

        // draw custom stuff
        node.DrawCustom(tree, ws);
    }

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name[..^"Config".Length] : t.Name;

    private static void SortByOrder(List<UINode> nodes)
    {
        nodes.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        foreach (var n in nodes)
        {
            SortByOrder(n.Children);
        }
    }

    private void DrawNodes(List<UINode> nodes)
    {
        var filteredNodes = new List<UINode>();
        foreach (var n in nodes)
        {
            if (MatchesFilter(n.Path))
            {
                filteredNodes.Add(n);
            }
        }

        foreach (var n in _tree.Nodes(filteredNodes, n => new(n.Name)))
        {
            DrawNode(n.Node, _root, _tree, _ws, props => MatchesFilter([.. n.Path, props.Label]));
            DrawNodes(n.Children);
        }
    }

    private bool MatchesFilter(List<string> path)
    {
        if (_filterNodes.Count == 0)
        {
            return true;
        }

        bool matchesOneFilter(List<string> filter)
        {
            var i = 0;
            foreach (var f in filter)
            {
                if (f == "*" || i >= path.Count)
                {
                    return true;
                }

                if (f != path[i])
                {
                    return false;
                }

                ++i;
            }

            return true;
        }

        foreach (var filter in _filterNodes)
        {
            if (matchesOneFilter(filter))
            {
                return true;
            }
        }

        return false;
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
            UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle);
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
            {
                flags |= ImGuiSliderFlags.Logarithmic;
            }

            ImGui.SetNextItemWidth(Math.Min(ImGui.GetWindowWidth() * 0.30f, 175));
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
            {
                flags |= ImGuiSliderFlags.Logarithmic;
            }

            ImGui.SetNextItemWidth(Math.Min(ImGui.GetWindowWidth() * 0.30f, 175));
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
        for (var i = 0; i < v.Length; ++i)
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

    public static void DrawGroupPresetIndicator(string text, Action contextMenu)
    {
        ImGui.AlignTextToFramePadding();
        if (UIMisc.IconButton(Dalamud.Interface.FontAwesomeIcon.ListUl, $"###{text}open"))
            ImGui.OpenPopup($"{text}popup");

        if (ImGui.BeginPopup($"{text}popup"))
        {
            contextMenu();
            ImGui.EndPopup();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Select a preset");
        }

        ImGui.SameLine();
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, GroupAssignment v, ConfigRoot root, UITree tree, WorldState ws)
    {
        var group = member.GetCustomAttribute<GroupDetailsAttribute>();
        if (group == null)
        {
            return false;
        }

        var spaced = false;

        ImGui.AlignTextToFramePadding();
        if (tooltip.Length > 0)
        {
            spaced = true;
            UIMisc.HelpMarker(tooltip);
            ImGui.SameLine();
        }

        var hasPreset = false;
        foreach (var _ in member.GetCustomAttributes<GroupPresetAttribute>())
        {
            hasPreset = true;
            break;
        }
        if (hasPreset)
        {
            spaced = true;
            DrawGroupPresetIndicator(label, () => DrawPropertyContextMenu(node, member, v));
        }

        if (!spaced)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, 0))
            {
                UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle);
            }
        }

        var modified = false;
        foreach (var tn in tree.Node(label, false, v.Validate() ? Colors.TextColor1 : Colors.TextColor2))
        {
            using var indent = ImRaii.PushIndent();
            using var table = ImRaii.Table("table", group.Names.Length + 2, ImGuiTableFlags.SizingFixedFit);
            if (!table)
            {
                continue;
            }

            foreach (var n in group.Names)
            {
                ImGui.TableSetupColumn(n);
            }

            ImGui.TableSetupColumn("----");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
            for (var i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; ++i)
            {
                var r = (PartyRolesConfig.Assignment)i;
                ImGui.TableNextRow();
                for (var c = 0; c < group.Names.Length; ++c)
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

                var name = r.ToString();
                if (assignments.Length > 0)
                {
                    name += $" ({ws.Party[assignments[i]]?.Name})";
                }

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
                for (var i = 0; i < preset.Preset.Length; ++i)
                {
                    v.Assignments[i] = preset.Preset[i];
                }

                node.Modified.Fire();
            }
        }
    }
}
