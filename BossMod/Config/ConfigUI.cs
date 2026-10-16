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
    private readonly EventSubscriptions _subscriptions;

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

        _subscriptions = new(BossModuleRegistry.Modified.ExecuteAndSubscribe(RebuildTree));
    }

    private void RebuildTree()
    {
        _roots.Clear();
        _selectedRoot = null;
        _scrollToNode = null;
        _reveal = [];
        _expandCategory = null;

        Dictionary<Type, UINode> nodes = [];
        foreach (var n in _root.Nodes)
        {
            nodes[n] = new(n, _root.Get<ConfigNode>(n));
        }

        foreach (var (t, n) in nodes)
        {
            var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;
            n.Tags = props?.Tags ?? [];
            n.Group = props?.Group ?? "";
            (n.Pages, n.HasGeneral) = GetPages(n.Node);

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
        }

        SortByOrder(_roots);
        ResolvePaths(_roots, []);
        FilterNodes();
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
        _subscriptions.Dispose();
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
        if (FindNode(_roots, configType) is not { } node)
            return;

        List<UINode> chain = [];
        for (var n = node; n != null; n = n.Parent)
            chain.Insert(0, n);

        Select(chain[0], chain.Count > 1 ? SectionKey(chain[1]) : null);
        _expandCategory = chain[0];
        _scrollSidebar = true;
        if (chain.Count > 2)
        {
            _scrollToNode = node;
            _reveal = [.. chain.Skip(2)];
        }
    }

    private static UINode? FindNode(List<UINode> nodes, Type type)
        => nodes.Select(n => n.Type == type ? n : FindNode(n.Children, type)).FirstOrDefault(n => n != null);

    public void Draw()
    {
        _tabs.Draw();
    }

    private static readonly string[] _groupOrder = ["General", "Radar & Modules", "Autorotation", "AI & Automation"];

    private string _searchText = "";
    private UINode? _selectedRoot;
    private string? _selectedSection; // null shows the whole category
    private UINode? _scrollToNode;
    private HashSet<UINode> _reveal = [];
    private bool _resetScroll;
    private UINode? _expandCategory;
    private bool _scrollSidebar;

    private void DrawSettings()
    {
        ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputTextEx("###settings-search", "Search for a setting...", ref _searchText))
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
                foreach (var group in _groupOrder.Concat(_roots.Select(RootGroup)).Distinct())
                    if (_roots.Where(r => RootGroup(r) == group).ToList() is { Count: > 0 } nodes)
                    {
                        UIMisc.SectionHeader(group);
                        DrawCategoryTree(nodes);
                    }
            }
        ImGui.SameLine();
        using var content = ImRaii.Child("##settings", new(-1, -1));
        if (!content)
            return;
        if (_resetScroll && _scrollToNode == null)
            ImGui.SetScrollY(0);
        _resetScroll = false;

        if (_filterNodes.Count > 0)
        {
            DrawNodes(_roots);
            return;
        }

        if (_selectedRoot == null && (_roots.FirstOrDefault(r => RootGroup(r) == _groupOrder[0]) ?? _roots.FirstOrDefault()) is { } first)
            Select(first, null);
        if (_selectedRoot is not { } root)
            return;

        if (!IsHeader(root))
        {
            UIMisc.SectionHeader(root.Name);
            DrawNode(root.Node, _root, _tree, _ws, _rotationDB);
            return;
        }

        foreach (var (key, label, draw) in Sections(root))
        {
            if (_selectedSection != null && _selectedSection != key)
                continue;
            UIMisc.SectionHeader(_selectedSection != null ? $"{root.Name} / {label}" : label);
            draw();
        }
    }

    private static string RootGroup(UINode node) => node.Group.Length > 0 ? node.Group : "Other";
    private static bool IsHeader(UINode node) => node.Pages.Count > 0 || node.Parent == null && node.Children.Count > 0;
    private static string PageKey(UINode node, string page) => $"{string.Join('/', node.Path)}#{page}";
    private static string SectionKey(UINode child) => child.Parent != null && child.Group.Length > 0 ? $"{string.Join('/', child.Parent.Path)}#group:{child.Group}" : PageKey(child, "");

    private List<(string key, string label, Action draw)> Sections(UINode root)
    {
        List<(string key, string label, Action draw)> res = [];
        if (root.HasGeneral)
            res.Add((PageKey(root, ""), "General", () => DrawNode(root.Node, _root, _tree, _ws, _rotationDB, page: "")));
        foreach (var p in root.Pages)
            res.Add((PageKey(root, p), p, () => DrawNode(root.Node, _root, _tree, _ws, _rotationDB, page: p)));

        List<(string key, string label, List<UINode> nodes)> children = [];
        foreach (var c in root.Children)
        {
            ResolveModule(c);
            var key = SectionKey(c);
            var index = children.FindIndex(s => s.key == key);
            if (index >= 0)
                children[index].nodes.Add(c);
            else
                children.Add((key, c.Group.Length > 0 ? c.Group : c.DisplayName ?? c.Name, [c]));
        }
        foreach (var (key, label, nodes) in children)
            res.Add((key, label, nodes.Count == 1 ? () => DrawChildNode(nodes[0]) : () => DrawChildGroup(nodes)));
        return res;
    }

    private void DrawChildNode(UINode node)
    {
        DrawNode(node.Node, _root, _tree, _ws, _rotationDB);
        DrawNodesByCategory(node.Children);
    }

    private void DrawChildGroup(List<UINode> nodes)
    {
        foreach (var n in nodes)
        {
            ImGui.Spacing();
            ImGui.TextUnformatted(n.DisplayName);
            using var indent = ImRaii.PushIndent();
            DrawChildNode(n);
        }
    }

    private void Select(UINode root, string? section)
    {
        _selectedRoot = root;
        _selectedSection = section;
        _scrollToNode = null;
        _reveal = [];
        _resetScroll = true;
        ClearSearch();
    }

    private void DrawCategoryTree(List<UINode> nodes)
    {
        var searching = _filterNodes.Count > 0;
        foreach (var n in nodes)
        {
            using var dim = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled), searching && !MatchesFilter(n.Path));
            if (!IsHeader(n))
            {
                DrawCategoryEntry(n.Name, n, null);
                continue;
            }

            var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding;
            if (!searching && n == _selectedRoot && _selectedSection == null)
                flags |= ImGuiTreeNodeFlags.Selected;
            if (_expandCategory == n)
            {
                ImGui.SetNextItemOpen(true);
                _expandCategory = null;
            }
            var open = ImGui.TreeNodeEx($"{n.Name}###{string.Join('/', n.Path)}", flags);
            if (_scrollSidebar && n == _selectedRoot && _selectedSection == null)
            {
                ImGui.SetScrollHereY(0.5f);
                _scrollSidebar = false;
            }
            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                Select(n, null);
            if (!open)
                continue;
            foreach (var (key, label, _) in Sections(n))
                DrawCategoryEntry(label, n, key);
            ImGui.TreePop();
        }
    }

    private void DrawCategoryEntry(string label, UINode root, string? key)
    {
        var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        if (_filterNodes.Count == 0 && root == _selectedRoot && key == _selectedSection)
            flags |= ImGuiTreeNodeFlags.Selected;
        ImGui.TreeNodeEx($"{label}###{key ?? PageKey(root, "")}", flags);
        if (_scrollSidebar && root == _selectedRoot && key == _selectedSection)
        {
            ImGui.SetScrollHereY(0.5f);
            _scrollSidebar = false;
        }
        if (ImGui.IsItemClicked())
            Select(root, key);
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
            ImGui.Spacing();
            ImGui.TextDisabled(g.Key switch
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
        foreach (var n in nodes.Where(n => MatchesFilter(n.Path)))
        {
            if (_reveal.Remove(n))
                ImGui.SetNextItemOpen(true);
            if (_scrollToNode == n)
            {
                ImGui.SetScrollHereY(0);
                _scrollToNode = null;
            }
            using var id = ImRaii.PushId(n.Name);
            if (!ImGui.TreeNodeEx($"{label(n)}###node"))
                continue;
            DrawNode(n.Node, _root, _tree, _ws, _rotationDB, props => MatchesFilter([.. n.Path, props.Label]));
            DrawNodes(n.Children);
            ImGui.TreePop();
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

    private static float DrawLabelAbove(PropertyDisplayAttribute attrs, bool radio, bool nested)
    {
        DrawHelp(attrs.Tooltip, nested);
        var x = ImGui.GetCursorPosX();
        ImGui.TextUnformatted($"{attrs.Label}{(radio ? ":" : "")}");
        ImGui.SetCursorPosX(x);
        ImGui.SetNextItemWidth(ControlWidth());
        return x;
    }

    private static float ControlWidth() => MathF.Min(ImGui.GetContentRegionAvail().X, 300 * ImGuiHelpers.GlobalScale);

    private const int MaxRadioOptions = 6;

    private static bool DrawRadioList(float x, string id, string[] options, bool inline, ref int v)
    {
        var res = false;
        using var _ = ImRaii.PushId(id);
        x += ImGui.GetFrameHeight();
        for (var i = 0; i < options.Length; ++i)
        {
            if (!inline || i == 0)
                ImGui.SetCursorPosX(x);
            if (ImGui.RadioButton($"{options[i]}###{i}", v == i) && v != i)
            {
                v = i;
                res = true;
            }
            if (inline && i < options.Length - 1)
                ImGui.SameLine();
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
            var x = DrawLabelAbove(attrs, true, nested);
            var ix = v ? 1 : 0;
            var modified = combo.Values.Length <= MaxRadioOptions
                ? DrawRadioList(x, attrs.Label, combo.Values, false, ref ix) && SetValue(node, member, ix != 0)
                : UICombo.Bool($"##{attrs.Label}", combo.Values, ref v) && SetValue(node, member, v);
            ImGui.Spacing();
            return modified;
        }

        DrawHelp(attrs.Tooltip, nested);
        return ImGui.Checkbox(attrs.Label, ref v) && SetValue(node, member, v);
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, Enum v)
    {
        var x = DrawLabelAbove(attrs, true, nested);
        var modified = false;
        var values = Enum.GetValues(v.GetType()).Cast<Enum>().ToArray();
        var radio = member.GetCustomAttribute<PropertyRadioAttribute>();
        if (values.Length <= MaxRadioOptions || radio != null)
        {
            var inline = radio?.Inline ?? false;
            var ix = Array.IndexOf(values, v);
            modified = DrawRadioList(x, attrs.Label, [.. values.Select(UICombo.EnumString)], inline, ref ix) && SetValue(node, member, values[ix]);
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
        DrawLabelAbove(attrs, false, nested);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        var flags = slider?.Logarithmic == true ? ImGuiSliderFlags.Logarithmic : ImGuiSliderFlags.None;
        var modified = slider != null
            ? ImGui.SliderFloat($"##{attrs.Label}", ref v, slider.Min, slider.Max, "%.3f", flags)
            : ImGui.InputFloat($"##{attrs.Label}", ref v);
        modified = modified && SetValue(node, member, v);
        ImGui.Spacing();
        return modified;
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, int v)
    {
        DrawLabelAbove(attrs, false, nested);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        var flags = slider?.Logarithmic == true ? ImGuiSliderFlags.Logarithmic : ImGuiSliderFlags.None;
        var modified = slider != null
            ? ImGui.SliderInt($"##{attrs.Label}", ref v, (int)slider.Min, (int)slider.Max, "%d", flags)
            : ImGui.InputInt($"##{attrs.Label}", ref v);
        modified = modified && SetValue(node, member, v);
        ImGui.Spacing();
        return modified;
    }

    private static bool DrawProperty(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, string v)
    {
        DrawLabelAbove(attrs, false, nested);
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
            using var table = ImRaii.Table("table", numRoles + 1, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PadOuterX);
            if (!table)
                continue;

            for (var i = 0; i < numRoles; ++i)
                ImGui.TableSetupColumn(((PartyRolesConfig.Assignment)i).ToString());
            ImGui.TableSetupColumn("");

            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
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
                ImGui.TableNextColumn();
                if (c < 0)
                    ImGui.TextDisabled("Unassigned");
                else
                    ImGui.TextWrapped(group.Names[c]);
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
