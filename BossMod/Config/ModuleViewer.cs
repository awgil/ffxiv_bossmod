using BossMod.Autorotation;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using System.Data;
using System.Globalization;
using System.Text;

namespace BossMod;

public sealed class ModuleViewer : IDisposable
{
    private record struct ModuleInfo(BossModuleRegistry.Info Info, string Name, int SortOrder);
    private record struct ModuleGroupInfo(string Name, uint Id, uint SortOrder, uint Icon = 0);
    private record struct ModuleGroup(ModuleGroupInfo Info, List<ModuleInfo> Modules);

    private readonly PlanDatabase? _planDB;
    private readonly WorldState _ws; // TODO: reconsider...

    private readonly BossModuleConfig _bmConfig = Service.Config.Get<BossModuleConfig>();

    private BitMask _filterExpansions;
    private BitMask _filterCategories;

    private readonly (string name, uint icon)[] _expansions;
    private readonly (string name, uint icon)[] _categories;
    private readonly uint _iconFATE;
    private readonly uint _iconHunt;
    private readonly List<ModuleGroup>[,] _groups;
    private readonly Vector2 _iconSize = new Vector2(32, 32) * ImGuiHelpers.GlobalScale;

    private string _searchText = "";

    public ModuleViewer(PlanDatabase? planDB, WorldState ws)
    {
        _planDB = planDB;
        _ws = ws;

        uint defaultIcon = 61762;
        _expansions = [.. Enum.GetNames<BossModuleInfo.Expansion>().Take((int)BossModuleInfo.Expansion.Count).Select(n => (n, defaultIcon))];
        _categories = [.. Enum.GetNames<BossModuleInfo.Category>().Take((int)BossModuleInfo.Category.Count).Select(n => (n, defaultIcon))];

        var exVersion = Service.LuminaSheet<ExVersion>()!;
        Customize(BossModuleInfo.Expansion.RealmReborn, 61875, exVersion.GetRow(0).Name);
        Customize(BossModuleInfo.Expansion.Heavensward, 61876, exVersion.GetRow(1).Name);
        Customize(BossModuleInfo.Expansion.Stormblood, 61877, exVersion.GetRow(2).Name);
        Customize(BossModuleInfo.Expansion.Shadowbringers, 61878, exVersion.GetRow(3).Name);
        Customize(BossModuleInfo.Expansion.Endwalker, 61879, exVersion.GetRow(4).Name);
        Customize(BossModuleInfo.Expansion.Dawntrail, 61880, exVersion.GetRow(5).Name);

        var contentType = Service.LuminaSheet<ContentType>()!;
        Customize(BossModuleInfo.Category.Dungeon, contentType.GetRow(2));
        Customize(BossModuleInfo.Category.Trial, contentType.GetRow(4));
        Customize(BossModuleInfo.Category.Raid, contentType.GetRow(5));
        Customize(BossModuleInfo.Category.Chaotic, contentType.GetRow(37));
        Customize(BossModuleInfo.Category.PVP, contentType.GetRow(6));
        Customize(BossModuleInfo.Category.Quest, contentType.GetRow(7));
        Customize(BossModuleInfo.Category.FATE, contentType.GetRow(8));
        Customize(BossModuleInfo.Category.TreasureHunt, contentType.GetRow(9));
        Customize(BossModuleInfo.Category.GoldSaucer, contentType.GetRow(19));
        Customize(BossModuleInfo.Category.DeepDungeon, contentType.GetRow(21));
        Customize(BossModuleInfo.Category.Ultimate, contentType.GetRow(28));
        Customize(BossModuleInfo.Category.Variant, contentType.GetRow(30));
        Customize(BossModuleInfo.Category.Criterion, contentType.GetRow(30));

        var playStyle = Service.LuminaSheet<CharaCardPlayStyle>()!;
        Customize(BossModuleInfo.Category.Foray, playStyle.GetRow(6));
        Customize(BossModuleInfo.Category.MaskedCarnivale, playStyle.GetRow(8));
        Customize(BossModuleInfo.Category.Hunt, playStyle.GetRow(10));

        _categories[(int)BossModuleInfo.Category.Extreme].icon = _categories[(int)BossModuleInfo.Category.Trial].icon;
        _categories[(int)BossModuleInfo.Category.Unreal].icon = _categories[(int)BossModuleInfo.Category.Trial].icon;
        _categories[(int)BossModuleInfo.Category.Savage].icon = _categories[(int)BossModuleInfo.Category.Raid].icon;
        _categories[(int)BossModuleInfo.Category.Alliance].icon = _categories[(int)BossModuleInfo.Category.Raid].icon;
        //_categories[(int)BossModuleInfo.Category.Event].icon = GetIcon(61757);

        _iconFATE = contentType.GetRow(8).Icon;
        _iconHunt = (uint)playStyle.GetRow(10).Icon;

        _groups = new List<ModuleGroup>[(int)BossModuleInfo.Expansion.Count, (int)BossModuleInfo.Category.Count];
        for (int i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
            for (int j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
                _groups[i, j] = [];

        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            var groups = _groups[(int)info.Expansion, (int)info.Category];
            var (groupInfo, moduleInfo) = Classify(info);
            var groupIndex = groups.FindIndex(g => g.Info.Id == groupInfo.Id);
            if (groupIndex < 0)
            {
                groupIndex = groups.Count;
                groups.Add(new(groupInfo, []));
            }
            else if (groups[groupIndex].Info != groupInfo)
            {
                Service.Log($"[ModuleViewer] Group properties mismatch between {groupInfo} and {groups[groupIndex].Info}");
            }
            groups[groupIndex].Modules.Add(moduleInfo);
        }

        foreach (var groups in _groups)
        {
            groups.SortBy(g => g.Info.SortOrder);
            foreach (var (g1, g2) in groups.Pairwise())
                if (g1.Info.SortOrder == g2.Info.SortOrder)
                    Service.Log($"[ModuleViewer] Same sort order between groups {g1.Info} and {g2.Info}");

            foreach (var g in groups)
            {
                g.Modules.SortBy(m => m.SortOrder);
                foreach (var (m1, m2) in g.Modules.Pairwise())
                    if (m1.SortOrder == m2.SortOrder)
                        Service.Log($"[ModuleViewer] Same sort order between modules {m1.Info.ModuleType.FullName} and {m2.Info.ModuleType.FullName}");
            }
        }
    }

    public void Dispose()
    {
    }

    public void Draw(UITree tree, WorldState ws)
    {
        var c1 = ImGui.GetCursorPos();
        using (ImRaii.Child("##filters", new(200 * ImGuiHelpers.GlobalScale, -1)))
            DrawFilters();
        ImGui.SetCursorPos(new(c1.X + 205 * ImGuiHelpers.GlobalScale, c1.Y));
        using (ImRaii.Child("##modules"))
            DrawModules(tree, ws);
    }

    private void DrawFilters()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Search:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        DrawSearchBar();
        ImGui.Separator();

        using (ImRaii.Child("##belowsearch"))
        {
            DrawFiltersTable("Expansion", DrawExpansionFilters);
            DrawFiltersTable("Category", DrawContentTypeFilters);
        }
    }

    private void DrawFiltersTable(string label, System.Action drawTable)
    {
        ImGui.TextUnformatted(label);
        ImGui.Separator();
        using var _ = ImRaii.Table(label, 2, ImGuiTableFlags.SizingStretchProp);
        ImGui.TableSetupColumn("icon", ImGuiTableColumnFlags.WidthFixed, _iconSize.X);
        drawTable.Invoke();
    }

    private void DrawSearchBar()
    {
        ImGui.InputTextWithHint("##search", "e.g. \"Ultimate\"", ref _searchText, 100, ImGuiInputTextFlags.CallbackCompletion);

        if (ImGui.IsItemHovered() && !ImGui.IsItemFocused())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Type here to search for any specific instance by its respective title.");
            ImGui.EndTooltip();
        }
    }

    private void DrawExpansionFilters()
    {
        for (var e = BossModuleInfo.Expansion.RealmReborn; e < BossModuleInfo.Expansion.Count; ++e)
        {
            using var _ = ImRaii.PushId($"expac_{e}");
            var bit = (int)e;
            ref var expansion = ref _expansions[bit];

            DrawFilter(
                expansion.name,
                expansion.icon,
                _filterExpansions[bit],
                click: () => _filterExpansions.Toggle(bit),
                shiftRmb: () => ToggleFocus(ref _filterExpansions, bit),
                context: () =>
                {
                    if (ImGui.MenuItem($"Hide expansion", "LMB", selected: _filterExpansions[bit]))
                        _filterExpansions.Toggle(bit);

                    var isFocused = _filterExpansions == ~BitMask.Build(bit);
                    if (ImGui.MenuItem($"Hide other expansions", "Shift + RMB", selected: isFocused))
                        ToggleFocus(ref _filterExpansions, bit);
                }
            );
        }
    }

    private void DrawContentTypeFilters()
    {
        var modified = false;

        for (var c = BossModuleInfo.Category.Uncategorized; c < BossModuleInfo.Category.Count; ++c)
        {
            using var _ = ImRaii.PushId($"cat_{c}");
            var isDisabledCategory = _bmConfig.DisabledCategories.Contains(c);
            var bit = (int)c;
            ref var category = ref _categories[bit];

            DrawFilter(
                category.name,
                category.icon,
                _filterCategories[bit],
                disabled: isDisabledCategory,
                click: () => _filterCategories.Toggle(bit),
                shiftRmb: () => ToggleFocus(ref _filterCategories, bit),
                context: () =>
                {
                    if (ImGui.MenuItem($"Hide category", "LMB", selected: _filterCategories[bit]))
                        _filterCategories.Toggle(bit);

                    var isFocused = _filterCategories == ~BitMask.Build(bit);
                    if (ImGui.MenuItem($"Hide other categories", "Shift + RMB", selected: isFocused))
                        ToggleFocus(ref _filterCategories, bit);

                    ImGui.Separator();

                    if (ImGui.MenuItem("Disable all modules in this category", "", isDisabledCategory))
                    {
                        modified = true;
                        if (isDisabledCategory)
                            _bmConfig.DisabledCategories.Remove(c);
                        else
                            _bmConfig.DisabledCategories.Add(c);
                    }
                }
            );
        }

        if (modified)
            _bmConfig.Modified.Fire();
    }

    private void ToggleFocus(ref BitMask filter, int bit)
    {
        var focused = ~BitMask.Build(bit);
        if (filter == focused)
            filter.Reset();
        else
            filter = focused;
    }

    private void DrawFilter(string label, uint iconId, bool filtered, bool disabled = false, System.Action? click = null, System.Action? context = null, System.Action? shiftRmb = null)
    {
        var shift = ImGui.GetIO().KeyShift;
        Vector4 tintCol = filtered ? new(0.5f, 0.5f, 0.5f, 0.85f) : new(1);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        if (Service.Texture.GetFromGameIcon(iconId).TryGetWrap(out var tex, out var ex))
        {
            ImGui.Image(tex.ImGuiHandle, _iconSize, Vector2.Zero, Vector2.One, tintCol);
            if (ex != null)
                Service.Logger.Warning(ex, $"unable to load icon {iconId}");
        }

        ImGui.TableNextColumn();
        var c = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new Vector2(c.X, c.Y + (_iconSize.Y - ImGui.GetFontSize()) / 2));
        var s = ImGui.GetCursorScreenPos();
        var textCol = ImGui.GetColorU32(filtered || disabled ? ImGuiCol.TextDisabled : ImGuiCol.Text);
        using (ImRaii.PushColor(ImGuiCol.Text, textCol))
            ImGui.TextUnformatted(label);

        if (disabled)
        {
            s.Y += ImGui.GetFontSize() * 0.5f;
            var width = ImGui.CalcTextSize(label);
            ImGui.GetWindowDrawList().AddLine(s, new(s.X + width.X, s.Y), textCol);
        }

        ImGui.SetCursorPos(c);
        var hdr = ImGui.GetColorU32(ImGuiCol.Header);
        using (ImRaii.PushColor(ImGuiCol.HeaderActive, hdr))
        {
            using (ImRaii.PushColor(ImGuiCol.HeaderHovered, hdr))
            {
                ImGui.Selectable("", false, ImGuiSelectableFlags.SpanAllColumns, new(0, _iconSize.Y));
            }
        }

        if (ImGui.IsItemHovered() && disabled)
            ImGui.SetTooltip("All modules in this category are disabled. Individual module settings are ignored.");
        if (ImGui.IsItemClicked())
            click?.Invoke();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && shift)
            shiftRmb?.Invoke();
        if (!shift)
        {
            using var ctx = ImRaii.ContextPopupItem("");
            if (ctx)
                context?.Invoke();
        }
    }

    private void DrawModules(UITree tree, WorldState ws)
    {
        using var table = ImRaii.Table("ModulesTable", 2, ImGuiTableFlags.RowBg);
        ImGui.TableSetupColumn("icons", ImGuiTableColumnFlags.WidthFixed, _iconSize.X * 2 + ImGui.GetStyle().FramePadding.X * 2);
        if (!table)
            return;

        var modified = false;

        for (int i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            if (_filterExpansions[i])
                continue;
            for (int j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
            {
                if (_filterCategories[j])
                    continue;

                foreach (var group in _groups[i, j])
                {
                    if (!_searchText.IsNullOrEmpty() && !group.Info.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                        continue;

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    UIMisc.Image(Service.Texture?.GetFromGameIcon(_expansions[i].icon), _iconSize);
                    ImGui.SameLine();
                    UIMisc.Image(Service.Texture?.GetFromGameIcon(group.Info.Icon != 0 ? group.Info.Icon : _categories[j].icon), _iconSize);
                    ImGui.TableNextColumn();

                    foreach (var ng in tree.Node($"{group.Info.Name}###{i}/{j}/{group.Info.Id}"))
                    {
                        foreach (var mod in group.Modules)
                        {
                            var identifier = mod.Info.ModuleType.ToString();
                            var isEnabled = !_bmConfig.DisabledModules.Contains(identifier);
                            if (ImGui.Checkbox($"###enabled{mod.Info.ModuleType}", ref isEnabled))
                            {
                                modified = true;
                                if (isEnabled)
                                    _bmConfig.DisabledModules.Remove(identifier);
                                else
                                    _bmConfig.DisabledModules.Add(identifier);
                            }
                            ImGui.SameLine();
                            using (ImRaii.Disabled(mod.Info.ConfigType == null))
                                if (UIMisc.IconButton(FontAwesomeIcon.Cog, "cfg", $"###{mod.Info.ModuleType.FullName}_cfg"))
                                    _ = new BossModuleConfigWindow(mod.Info, ws);
                            ImGui.SameLine();
                            using (ImRaii.Disabled(mod.Info.PlanLevel == 0))
                                if (UIMisc.IconButton(FontAwesomeIcon.ClipboardList, "plan", $"###{mod.Info.ModuleType.FullName}_plans"))
                                    ImGui.OpenPopup($"{mod.Info.ModuleType.FullName}_popup");
                            ImGui.SameLine();
                            UIMisc.HelpMarker(() => ModuleHelpText(mod));
                            ImGui.SameLine();
                            var textColor = mod.Info.Maturity switch
                            {
                                BossModuleInfo.Maturity.WIP => 0xff0000ff,
                                BossModuleInfo.Maturity.Verified => 0xff00ff00,
                                _ => 0xffffffff
                            };
                            using (ImRaii.PushColor(ImGuiCol.Text, textColor))
                                ImGui.TextUnformatted($"{mod.Name} [{mod.Info.ModuleType.Name}]");

                            using (var popup = ImRaii.Popup($"{mod.Info.ModuleType.FullName}_popup"))
                                if (popup)
                                    ModulePlansPopup(mod.Info);
                        }
                    }
                }
            }
        }

        if (modified)
            _bmConfig.Modified.Fire();
    }

    private void Customize(BossModuleInfo.Expansion expansion, uint iconId, ReadOnlySeString name) => _expansions[(int)expansion] = (name.ToString(), iconId);
    private void Customize(BossModuleInfo.Category category, uint iconId, ReadOnlySeString name) => _categories[(int)category] = (name.ToString(), iconId);
    private void Customize(BossModuleInfo.Category category, ContentType ct) => Customize(category, ct.Icon, ct.Name);
    private void Customize(BossModuleInfo.Category category, CharaCardPlayStyle ps) => Customize(category, (uint)ps.Icon, ps.Name);

    //private static IDalamudTextureWrap? GetIcon(uint iconId) => iconId != 0 ? Service.Texture?.GetIcon(iconId, Dalamud.Plugin.Services.ITextureProvider.IconFlags.HiRes) : null;
    public static string FixCase(ReadOnlySeString str) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToString());
    public static string BNpcName(uint id) => FixCase(Service.LuminaRow<BNpcName>(id)!.Value.Singular);

    private (ModuleGroupInfo, ModuleInfo) Classify(BossModuleRegistry.Info module)
    {
        var groupId = (uint)module.GroupType << 24;
        switch (module.GroupType)
        {
            case BossModuleInfo.GroupType.CFC:
                groupId |= module.GroupID;
                var cfcRow = Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value;
                var cfcSort = cfcRow.SortKey;
                return (new(FixCase(cfcRow.Name), groupId, cfcSort != 0 ? cfcSort : groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.MaskedCarnivale:
                groupId |= module.GroupID;
                var mcRow = Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value;
                var mcSort = uint.Parse(mcRow.ShortCode.ToString().AsSpan(3), CultureInfo.InvariantCulture); // 'aozNNN'
                var mcName = $"Stage {mcSort}: {FixCase(mcRow.Name)}";
                return (new(mcName, groupId, mcSort), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.RemovedUnreal:
                return (new("Removed Content", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.Quest:
                var questRow = Service.LuminaRow<Quest>(module.GroupID)!.Value;
                groupId |= questRow.JournalGenre.RowId;
                var questCategoryName = questRow.JournalGenre.ValueNullable?.Name.ToString() ?? "";
                return (new(questCategoryName, groupId, groupId), new(module, $"{questRow.Name}: {BNpcName(module.NameID)}", module.SortOrder));
            case BossModuleInfo.GroupType.Fate:
                var fateRow = Service.LuminaRow<Fate>(module.GroupID)!.Value;
                return (new($"{module.Expansion.ShortName()} FATE", groupId, groupId, _iconFATE), new(module, $"{fateRow.Name}: {BNpcName(module.NameID)}", module.SortOrder));
            case BossModuleInfo.GroupType.Hunt:
                groupId |= module.GroupID;
                return (new($"{module.Expansion.ShortName()} Hunt {(BossModuleInfo.HuntRank)module.GroupID}", groupId, groupId, _iconHunt), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.BozjaCE:
                groupId |= module.GroupID;
                var ceName = $"{FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.Name)} CE";
                return (new(ceName, groupId, groupId), new(module, Service.LuminaRow<DynamicEvent>(module.NameID)!.Value.Name.ToString(), module.SortOrder));
            case BossModuleInfo.GroupType.BozjaDuel:
                groupId |= module.GroupID;
                var duelName = $"{FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.Name)} Duel";
                return (new(duelName, groupId, groupId), new(module, Service.LuminaRow<DynamicEvent>(module.NameID)!.Value.Name.ToString(), module.SortOrder));
            case BossModuleInfo.GroupType.EurekaNM:
                groupId |= module.GroupID;
                var nmName = FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.Name);
                return (new(nmName, groupId, groupId), new(module, Service.LuminaRow<Fate>(module.NameID)!.Value.Name.ToString(), module.SortOrder));
            case BossModuleInfo.GroupType.GoldSaucer:
                return (new("Gold saucer", groupId, groupId), new(module, $"{Service.LuminaRow<GoldSaucerTextData>(module.GroupID)?.Text}: {BNpcName(module.NameID)}", module.SortOrder));
            default:
                return (new("Ungrouped", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
        }
    }

    private string ModuleHelpText(ModuleInfo info)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.CurrentCulture, $"Cooldown planning: {(info.Info.PlanLevel > 0 ? $"L{info.Info.PlanLevel}" : "not supported")}");
        if (info.Info.Contributors.Length > 0)
            sb.AppendLine(CultureInfo.CurrentCulture, $"Contributors: {info.Info.Contributors}");
        return sb.ToString();
    }

    private void ModulePlansPopup(BossModuleRegistry.Info info)
    {
        if (_planDB == null)
            return;

        var mplans = _planDB.Plans.GetOrAdd(info.ModuleType);
        foreach (var (cls, plans) in mplans)
        {
            foreach (var plan in plans.Plans)
            {
                if (ImGui.Selectable($"Edit {cls} '{plan.Name}' ({plan.Guid})"))
                {
                    UIPlanDatabaseEditor.StartPlanEditor(_planDB, plan);
                }
            }
        }

        var player = _ws.Party.Player();
        if (player != null)
        {
            if (ImGui.Selectable($"New plan for {player.Class}..."))
            {
                var plans = mplans.GetOrAdd(player.Class);
                var plan = new Plan($"New {plans.Plans.Count + 1}", info.ModuleType) { Guid = Guid.NewGuid().ToString(), Class = player.Class, Level = info.PlanLevel };
                _planDB.ModifyPlan(null, plan);
                UIPlanDatabaseEditor.StartPlanEditor(_planDB, plan);
            }
        }
    }
}
