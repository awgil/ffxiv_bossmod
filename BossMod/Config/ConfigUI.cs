using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.IO;
using System.Reflection;

namespace BossMod;

public sealed class ConfigUI : IDisposable
{
    private class UINode(Type type, ConfigNode node)
    {
        public Type Type = type;
        public ConfigNode Node = node;
        public string Name = "";
        public int Order;
        public UINode? Parent;
        public List<UINode> Children = [];
        public string[] Tags = [];
        public string Group = "";
        public List<string> Pages = [];
        public bool HasGeneral;

        public List<string> Path = [];

        public string? DisplayName;
        public BossModuleInfo.Category? Category;
    }

    private static readonly Dictionary<Type, PropertyRenderer> _customRenderers = [];

    private readonly List<UINode> _roots = [];
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();
    private readonly AboutTab _about;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;
    private readonly RotationDatabase? _rotationDB;

    private readonly List<List<string>> _filterNodes = [];

    public ConfigUI(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _about = new(replayDir);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB) : null;
        _rotationDB = rotationDB;

        _tabs.Add("About", _about.Draw);
        _tabs.Add("Settings", DrawSettings);
        _tabs.Add("Supported Bosses", () => _mv.Draw(_tree, _ws));
        _tabs.Add("Autorotation Presets", () => _presets?.Draw());

        Dictionary<Type, UINode> nodes = [];
        foreach (var n in config.Nodes)
        {
            nodes[n] = new(n, config.Get<ConfigNode>(n));
        }

        foreach (var (t, n) in nodes)
        {
            var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;
            n.Tags = props?.Tags ?? [];
            n.Group = props?.Group ?? "Other";
            (n.Pages, n.HasGeneral) = GetPages(n.Node);

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

    public void Dispose()
    {
        _mv.Dispose();
    }

    public void ShowTab(string name) => _tabs.Select(name);

    public void ShowPreset(string name)
    {
        ShowTab("Autorotation Presets");
        _presets?.SelectPresetByName(name);
    }

    public void OpenConfigNode(Type configType)
    {
        ShowTab("Settings");
        if (FindNode(_roots, configType) is { } node)
            Select(node, "");
    }

    private static UINode? FindNode(List<UINode> nodes, Type type)
        => nodes.Select(n => n.Type == type ? n : FindNode(n.Children, type)).FirstOrDefault(n => n != null);

    public void Draw()
    {
        _tabs.Draw();
    }

    private static readonly string[] _groupOrder = ["General", "Radar & Modules", "Autorotation", "AI & Automation"];

    private string _searchText = "";
    private UINode? _selectedNode;
    private string _selectedPage = "";

    private void DrawSettings()
    {
        ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputTextEx("", "Search for a setting...", ref _searchText))
            FilterNodes();

        ImGui.SameLine();
        using (ImRaii.Disabled(_searchText.Length == 0))
            if (ImGui.Button("Clear"))
                ClearSearch();

        using (var categories = ImRaii.Child("##categories", new(230 * ImGuiHelpers.GlobalScale, -1), true))
            if (categories)
            {
                using var padding = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(6, 3) * ImGuiHelpers.GlobalScale);
                using var spacing = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.GetStyle().ItemSpacing.X, 1 * ImGuiHelpers.GlobalScale));
                foreach (var group in _groupOrder.Concat(_roots.Select(r => r.Group)).Distinct())
                    if (_roots.Where(r => r.Group == group).ToList() is { Count: > 0 } nodes)
                    {
                        UIMisc.SectionHeader(group);
                        DrawCategoryTree(nodes);
                    }
            }
        ImGui.SameLine();
        using var content = ImRaii.Child("##settings", new(-1, -1));
        if (!content)
            return;

        if (_filterNodes.Count > 0)
        {
            DrawNodes(_roots);
            return;
        }

        if (_selectedNode == null && (_roots.FirstOrDefault(r => r.Group == _groupOrder[0]) ?? _roots.FirstOrDefault()) is { } first)
            Select(first, IsHeader(first) && !first.HasGeneral ? first.Pages.FirstOrDefault() ?? "" : "");
        if (_selectedNode is not { } node)
            return;

        var title = _selectedPage.Length > 0 ? $"{node.Name} / {_selectedPage}" : IsHeader(node) ? $"{node.Name} / General" : node.Name;
        UIMisc.SectionHeader(title);
        DrawNode(node.Node, _root, _tree, _ws, _rotationDB, page: _selectedPage);
        if (_selectedPage.Length == 0 && node.Parent != null)
            DrawNodesByCategory(node.Children);
    }

    private static bool IsHeader(UINode node) => node.Pages.Count > 0 || node.Parent == null && node.Children.Count > 0;

    private void Select(UINode node, string page)
    {
        _selectedNode = node;
        _selectedPage = page;
        ClearSearch();
    }

    private void DrawCategoryTree(List<UINode> nodes)
    {
        var searching = _filterNodes.Count > 0;
        foreach (var n in nodes)
        {
            using var dim = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled), searching && !MatchesFilter(n.Path));
            var id = string.Join('/', n.Path);
            if (!IsHeader(n))
            {
                DrawCategoryEntry(n.Name, $"{id}#", n, "");
                continue;
            }

            if (!ImGui.TreeNodeEx($"{n.Name}###{id}", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding))
                continue;
            if (n.HasGeneral)
                DrawCategoryEntry("General", $"{id}#", n, "");
            foreach (var p in n.Pages)
                DrawCategoryEntry(p, $"{id}#{p}", n, p);
            if (n.Parent == null)
                DrawCategoryTree(n.Children);
            ImGui.TreePop();
        }
    }

    private void DrawCategoryEntry(string label, string id, UINode node, string page)
    {
        var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        if (_filterNodes.Count == 0 && node == _selectedNode && page == _selectedPage)
            flags |= ImGuiTreeNodeFlags.Selected;
        ImGui.TreeNodeEx($"{label}###{id}", flags);
        if (ImGui.IsItemClicked())
            Select(node, page);
    }

    private void ClearSearch()
    {
        _searchText = "";
        FilterNodes();
    }

    private void FilterNodes()
    {
        _filterNodes.Clear();

        if (_searchText.Length == 0)
            return;

        foreach (var r in _roots)
            foreach (var path in WalkNodes(r))
                _filterNodes.Add(path);
    }

    private IEnumerable<List<string>> WalkNodes(UINode node)
    {
        // if node matches, show all children
        if (Utils.TextMatch(node.Name, _searchText) || node.Tags.Any(t => Utils.TextMatch(t, _searchText)))
        {
            yield return [node.Name, "*"];
            yield break;
        }

        // check for matching fields
        foreach (var field in node.Node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            if (Utils.TextMatch(props.Label, _searchText) || props.Tags.Any(t => Utils.TextMatch(t, _searchText)))
                yield return [node.Name, props.Label];
        }

        // check for matching subnodes
        foreach (var p in node.Children)
            foreach (var path in WalkNodes(p))
                yield return [node.Name, .. path];
    }

    // page: "" is the part before the first page start, null draws all pages
    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, RotationDatabase? db, Func<PropertyDisplayAttribute, bool>? filter = null, string? page = null)
    {
        // draw standard properties
        var currentPage = "";
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            var pageStart = field.GetCustomAttribute<PageStartAttribute>();
            if (pageStart != null)
                currentPage = pageStart.Name;
            if (page != null && page != currentPage)
                continue;

            if (filter?.Invoke(props) == false)
                continue;

            if (pageStart != null && page == null)
                UIMisc.SectionHeader(pageStart.Name);

            var newSection = field.GetCustomAttribute<SectionStartAttribute>();

            var disabled = false;
            var nested = false;

            if (props.Depends is { } prop)
            {
                if (newSection != null)
                    throw new InvalidOperationException($"A field with a dependency cannot start a new config section");

                nested = true;
                var dependsEnabled = node.GetType().GetField(prop)?.GetValue(node) switch
                {
                    bool v => v,
                    _ => throw new InvalidDataException($"Internal error: cannot use dependsOn with a non-bool field")
                };
                disabled = !dependsEnabled;
            }

            if (newSection != null)
            {
                if (newSection.Label.Length > 0)
                    UIMisc.SectionHeader(newSection.Label);
                else if (newSection.Separator)
                    ImGui.Separator();
            }

            var value = field.GetValue(node);
            using (ImRaii.Disabled(disabled))
            {
                if (DrawProperty(props, nested, node, field, value, root, tree, ws))
                {
                    node.Modified.Fire();
                }
                DrawResetMenu(node, field, value);
            }
        }

        // draw custom stuff
        if (page is null or "")
            node.DrawCustom(tree, ws, db);
    }

    private static (List<string> pages, bool hasDefault) GetPages(ConfigNode node)
    {
        List<string> pages = [];
        var hasDefault = false;
        foreach (var field in node.GetType().GetFields())
        {
            if (field.GetCustomAttribute<PropertyDisplayAttribute>() == null)
                continue;
            if (field.GetCustomAttribute<PageStartAttribute>() is { } p)
                pages.Add(p.Name);
            else if (pages.Count == 0)
                hasDefault = true;
        }
        return (pages, hasDefault);
    }

    private static readonly Dictionary<Type, ConfigNode?> _defaultNodes = [];

    private static void DrawResetMenu(ConfigNode node, FieldInfo field, object? value)
    {
        if (!field.FieldType.IsValueType && field.FieldType != typeof(string))
            return;

        using var ctx = ImRaii.ContextPopupItem($"###reset_{field.Name}");
        if (!ctx)
            return;

        var type = node.GetType();
        if (!_defaultNodes.TryGetValue(type, out var defaults))
        {
            try
            {
                defaults = Activator.CreateInstance(type) as ConfigNode;
            }
            catch (Exception ex)
            {
                Service.Log($"[ConfigUI] Cannot create default instance of {type}: {ex.Message}");
            }
            _defaultNodes[type] = defaults;
        }

        var defaultValue = defaults != null ? field.GetValue(defaults) : null;
        if (ImGui.MenuItem("Reset to default", "", false, defaults != null && !Equals(defaultValue, value)))
        {
            field.SetValue(node, defaultValue);
            node.Modified.Fire();
        }
    }

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name[..^"Config".Length] : t.Name;

    private void SortByOrder(List<UINode> nodes)
    {
        nodes.SortBy(e => e.Order);
        foreach (var n in nodes)
            SortByOrder(n.Children);
    }

    private void DrawNodesByCategory(List<UINode> nodes)
    {
        nodes.ForEach(ResolveModule);
        var groups = nodes.GroupBy(n => n.Category).OrderBy(g => g.Key ?? BossModuleInfo.Category.Count).ToList();
        if (groups.Count == 1)
        {
            DrawNodes(nodes);
            return;
        }
        foreach (var g in groups)
        {
            UIMisc.SectionHeader(g.Key switch
            {
                null => "Other",
                BossModuleInfo.Category.Raid => "Normal Raids",
                BossModuleInfo.Category.Trial => "Trials",
                BossModuleInfo.Category.Dungeon => "Dungeons",
                { } c => c.ToString()
            });
            DrawNodes([.. g.OrderBy(n => n.DisplayName)]);
        }
    }

    private static void ResolveModule(UINode node)
    {
        if (node.DisplayName != null)
            return;
        node.DisplayName = node.Name;
        var type = node.Node.GetType();
        if (type.GetCustomAttribute<ConfigDisplayAttribute>()?.Name != null || BossModuleRegistry.RegisteredModules.Values.FirstOrDefault(i => i.ConfigType == type) is not { } info)
            return;
        node.Category = info.Category;
        try
        {
            if (info.GroupType == BossModuleInfo.GroupType.CFC && Service.LuminaRow<Lumina.Excel.Sheets.ContentFinderCondition>(info.GroupID) is { } cfc && !cfc.Name.IsEmpty)
                node.DisplayName = ModuleViewer.FixCase(cfc.Name);
            else if (info.NameID != 0)
                node.DisplayName = ModuleViewer.BNpcName(info.NameID);
        }
        catch (Exception ex)
        {
            Service.Log($"[ConfigUI] Failed to resolve name for {type}: {ex.Message}");
        }
    }

    private void DrawNodes(List<UINode> nodes)
    {
        nodes.ForEach(ResolveModule);
        var duplicates = nodes.GroupBy(n => n.DisplayName).Where(g => g.Skip(1).Any()).Select(g => g.Key).ToHashSet();
        string label(UINode n)
        {
            if (!duplicates.Contains(n.DisplayName) || n.DisplayName == n.Name)
                return n.DisplayName ?? n.Name;
            var phase = System.Text.RegularExpressions.Regex.Match(n.Name, @"^P\d+S(\d)");
            return phase.Success ? $"{n.DisplayName} - Phase {phase.Groups[1].Value}" : $"{n.DisplayName} [{n.Name}]";
        }
        foreach (var n in _tree.Nodes(nodes.Where(n => MatchesFilter(n.Path)), n => new($"{label(n)}###{n.Name}")))
        {
            DrawNode(n.Node, _root, _tree, _ws, _rotationDB, props => MatchesFilter([.. n.Path, props.Label]));
            DrawNodes(n.Children);
        }
    }

    private bool MatchesFilter(List<string> path)
    {
        if (_filterNodes.Count == 0)
            return true;

        bool matchesOneFilter(List<string> filter)
        {
            var i = 0;
            foreach (var f in filter)
            {
                if (f == "*" || i >= path.Count)
                    return true;

                if (f != path[i])
                    return false;

                i++;
            }

            return true;
        }

        return _filterNodes.Any(matchesOneFilter);
    }

    public static void DrawHelp(string tooltip, bool nested = false)
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
            UIMisc.IconText(FontAwesomeIcon.InfoCircle);
        }
        ImGui.SameLine();
        if (nested)
            DrawNesting();
    }

    private static void DrawNesting()
    {
        var sHeight = ImGui.GetFrameHeight();
        var sBox = new Vector2(sHeight, sHeight);

        var bar = "└";
        var pos = ImGui.GetCursorScreenPos();
        var size = ImGui.CalcTextSize(bar);

        ImGui.Dummy(new(sHeight - ImGui.GetStyle().ItemInnerSpacing.X, 0));
        ImGui.SameLine();
        ImGui.GetWindowDrawList().AddText(pos + (sBox - size) * 0.5f, ImGui.GetColorU32(ImGuiCol.Text), bar);
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, object? value, ConfigRoot root, UITree tree, WorldState ws) => value switch
    {
        bool v => DrawProperty(attrs, nested, node, member, v),
        Enum v => DrawProperty(attrs, nested, node, member, v),
        float v => DrawProperty(attrs, nested, node, member, v),
        int v => DrawProperty(attrs, nested, node, member, v),
        string v => DrawProperty(attrs, nested, node, member, v),
        Color v => DrawProperty(attrs, nested, node, member, v),
        Color[] v => DrawProperty(attrs, nested, node, member, v),
        GroupAssignment v => DrawProperty(attrs, nested, node, member, v, root, tree, ws),
        object v => DrawCustom(attrs, nested, node, member, v, root, tree, ws),
        null => false
    };

    private static float DrawLabelAbove(PropertyDisplayAttribute attrs, bool nested)
    {
        DrawHelp(attrs.Tooltip, nested);
        var x = ImGui.GetCursorPosX();
        ImGui.TextUnformatted(attrs.Label);
        ImGui.SetCursorPosX(x);
        ImGui.SetNextItemWidth(ControlWidth());
        return x;
    }

    private static float ControlWidth() => MathF.Min(ImGui.GetContentRegionAvail().X, 300 * ImGuiHelpers.GlobalScale);

    private const int MaxRadioOptions = 6;

    private static bool DrawRadioList(float x, string id, string[] options, ref int v)
    {
        var res = false;
        using var _ = ImRaii.PushId(id);
        x += ImGui.GetFrameHeight();
        for (var i = 0; i < options.Length; ++i)
        {
            ImGui.SetCursorPosX(x);
            if (ImGui.RadioButton($"{options[i]}###{i}", v == i) && v != i)
            {
                v = i;
                res = true;
            }
        }
        return res;
    }

    private static bool SetValue(ConfigNode node, FieldInfo member, object value)
    {
        member.SetValue(node, value);
        return true;
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, bool v)
    {
        var combo = member.GetCustomAttribute<PropertyComboAttribute>();
        if (combo != null)
        {
            var x = DrawLabelAbove(attrs, nested);
            var ix = v ? 1 : 0;
            var modified = combo.Values.Length <= MaxRadioOptions
                ? DrawRadioList(x, attrs.Label, combo.Values, ref ix) && SetValue(node, member, ix != 0)
                : UICombo.Bool($"##{attrs.Label}", combo.Values, ref v) && SetValue(node, member, v);
            ImGui.Spacing();
            return modified;
        }

        DrawHelp(attrs.Tooltip, nested);
        return ImGui.Checkbox(attrs.Label, ref v) && SetValue(node, member, v);
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, Enum v)
    {
        var x = DrawLabelAbove(attrs, nested);
        var modified = false;
        var values = Enum.GetValues(v.GetType()).Cast<Enum>().ToArray();
        if (values.Length <= MaxRadioOptions || member.GetCustomAttribute<PropertyRadioAttribute>() != null)
        {
            var ix = Array.IndexOf(values, v);
            modified = DrawRadioList(x, attrs.Label, [.. values.Select(UICombo.EnumString)], ref ix) && SetValue(node, member, values[ix]);
        }
        else
        {
            modified = UICombo.Enum($"##{attrs.Label}", ref v) && SetValue(node, member, v);
        }
        ImGui.Spacing();
        return modified;
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, float v)
    {
        DrawLabelAbove(attrs, nested);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        var flags = slider?.Logarithmic == true ? ImGuiSliderFlags.Logarithmic : ImGuiSliderFlags.None;
        var modified = slider != null
            ? ImGui.DragFloat($"##{attrs.Label}", ref v, slider.Speed, slider.Min, slider.Max, "%.3f", flags)
            : ImGui.InputFloat($"##{attrs.Label}", ref v);
        modified = modified && SetValue(node, member, v);
        ImGui.Spacing();
        return modified;
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, int v)
    {
        DrawLabelAbove(attrs, nested);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        var flags = slider?.Logarithmic == true ? ImGuiSliderFlags.Logarithmic : ImGuiSliderFlags.None;
        var modified = slider != null
            ? ImGui.DragInt($"##{attrs.Label}", ref v, slider.Speed, (int)slider.Min, (int)slider.Max, "%d", flags)
            : ImGui.InputInt($"##{attrs.Label}", ref v);
        modified = modified && SetValue(node, member, v);
        ImGui.Spacing();
        return modified;
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, string v)
    {
        DrawLabelAbove(attrs, nested);
        var modified = ImGui.InputText($"##{attrs.Label}", ref v, 256) && SetValue(node, member, v);
        ImGui.Spacing();
        return modified;
    }

    private const ImGuiColorEditFlags ColorFlags = ImGuiColorEditFlags.PickerHueWheel | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaPreviewHalf;

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, Color v)
    {
        DrawHelp(attrs.Tooltip, nested);
        var col = v.ToFloat4();
        return ImGui.ColorEdit4(attrs.Label, ref col, ColorFlags) && SetValue(node, member, Color.FromFloat4(col));
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, Color[] v)
    {
        var modified = false;
        for (var i = 0; i < v.Length; ++i)
        {
            DrawHelp(attrs.Tooltip, nested);
            var col = v[i].ToFloat4();
            if (ImGui.ColorEdit4($"{attrs.Label} {i}", ref col, ColorFlags))
            {
                v[i] = Color.FromFloat4(col);
                member.SetValue(node, v);
                modified = true;
            }
        }
        return modified;
    }

    public static void DrawGroupPresetIndicator(string text, string help, Action contextMenu)
    {
        using var _ = ImRaii.PushId(text);
        ImGui.AlignTextToFramePadding();
        var paddingX = ImGui.GetStyle().FramePadding.X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - paddingX);
        if (UIMisc.IconButton(help.Length == 0 ? FontAwesomeIcon.ListUl : FontAwesomeIcon.InfoCircle))
        {
            Service.Log($"Opening popup {text}popup");
            ImGui.OpenPopup($"{text}popup");
        }

        if (ImGui.BeginPopup($"{text}popup"))
        {
            contextMenu();
            ImGui.EndPopup();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            if (help.Length > 0)
            {
                ImGui.TextUnformatted(help);
                ImGui.Separator();
            }
            ImGui.TextUnformatted("Click to select a preset");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - paddingX);
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, GroupAssignment v, ConfigRoot root, UITree tree, WorldState ws)
    {
        var group = member.GetCustomAttribute<GroupDetailsAttribute>();
        if (group == null)
            return false;

        var haveTooltip = attrs.Tooltip.Length > 0;
        var havePresetButton = member.GetCustomAttributes<GroupPresetAttribute>().Any();

        var spaced = haveTooltip || havePresetButton;

        ImGui.AlignTextToFramePadding();
        if (havePresetButton)
        {
            DrawGroupPresetIndicator(attrs.Label, attrs.Tooltip, () => DrawPropertyContextMenu(node, member, v));
        }
        else if (haveTooltip)
        {
            UIMisc.HelpMarker(attrs.Tooltip);
            ImGui.SameLine();
        }
        else
        {
            using (ImRaii.PushColor(ImGuiCol.Text, 0))
            {
                UIMisc.IconText(FontAwesomeIcon.InfoCircle);
                ImGui.SameLine();
            }
        }

        if (nested)
            DrawNesting();

        var modified = false;
        foreach (var tn in tree.Node(attrs.Label, false, v.Validate() ? 0xffffffff : 0xff00ffff))
        {
            using var indent = ImRaii.PushIndent();
            const int numRoles = (int)PartyRolesConfig.Assignment.Unassigned;
            using var table = ImRaii.Table("table", numRoles + 1, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH);
            if (!table)
                continue;

            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            for (var i = 0; i < numRoles; ++i)
                ImGui.TableSetupColumn(((PartyRolesConfig.Assignment)i).ToString(), ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X);

            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
            ImGui.TableNextColumn();
            for (var i = 0; i < numRoles; ++i)
            {
                ImGui.TableNextColumn();
                ImGui.TableHeader(((PartyRolesConfig.Assignment)i).ToString());
                if (assignments.Length > 0 && ImGui.IsItemHovered())
                    ImGui.SetTooltip(ws.Party[assignments[i]]?.Name ?? "");
            }

            for (var c = -1; c < group.Names.Length; ++c)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (c < 0)
                    ImGui.TextDisabled("Not assigned");
                else
                    ImGui.TextWrapped(group.Names[c]);
                for (var i = 0; i < numRoles; ++i)
                {
                    var r = (PartyRolesConfig.Assignment)i;
                    var selected = c < 0 ? v[r] < 0 || v[r] >= group.Names.Length : v[r] == c;
                    ImGui.TableNextColumn();
                    if (ImGui.RadioButton($"###{r}:{c}", selected))
                    {
                        v[r] = c;
                        modified = true;
                    }
                }
            }
        }
        return modified;
    }

    private static bool DrawCustom(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, object value, ConfigRoot root, UITree tree, WorldState ws)
    {
        if (attrs.Renderer != null)
        {
            if (!_customRenderers.TryGetValue(attrs.Renderer, out var r))
            {
                if (Activator.CreateInstance(attrs.Renderer) is PropertyRenderer p)
                {
                    _customRenderers.Add(attrs.Renderer, p);
                    r = p;
                }
                else
                    throw new InvalidOperationException($"Invalid renderer type for {member}, must inherit from PropertyRenderer");
            }

            return r.Draw(attrs, nested, node, member, value, root, tree, ws);
        }

        return false;
    }

    private static void DrawPropertyContextMenu(ConfigNode node, FieldInfo member, GroupAssignment v)
    {
        foreach (var preset in member.GetCustomAttributes<GroupPresetAttribute>())
        {
            if (ImGui.MenuItem(preset.Name))
            {
                for (var i = 0; i < preset.Preset.Length; ++i)
                    v.Assignments[i] = preset.Preset[i];
                node.Modified.Fire();
            }
        }
    }
}