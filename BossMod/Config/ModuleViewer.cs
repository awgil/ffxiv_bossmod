using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BossMod;

public sealed class ModuleViewer : IDisposable
{
    private record struct ModuleInfo(BossModuleRegistry.Info Info, string Name, int SortOrder);
    private record struct ModuleGroupInfo(string Name, uint Id, uint SortOrder, uint Icon = default);
    private record struct ModuleGroup(ModuleGroupInfo Info, List<ModuleInfo> Modules);

    private readonly PlanDatabase? _planDB;
    private readonly WorldState _ws; // TODO: reconsider...

    private BitMask _filterExpansions;
    private BitMask _filterCategories;

    private readonly (string name, uint icon)[] _expansions = new (string, uint)[(int)BossModuleInfo.Expansion.Count];
    private readonly (string name, uint icon)[] _categories = new (string, uint)[(int)BossModuleInfo.Category.Count];
    private readonly uint _iconFATE;
    private readonly uint _iconHunt;
    private readonly List<ModuleGroup>[,] _groups;
    private readonly Vector2 _iconSize = new(30f, 30f);

    private string _searchText = "";

    public ModuleViewer(PlanDatabase? planDB, WorldState ws)
    {
        _planDB = planDB;
        _ws = ws;

        const uint defaultIcon = 61762u;
        var expansionNames = Enum.GetNames<BossModuleInfo.Expansion>();
        for (var i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            _expansions[i] = (expansionNames[i], defaultIcon);
        }

        var categoryNames = Enum.GetNames<BossModuleInfo.Category>();
        for (var i = 0; i < (int)BossModuleInfo.Category.Count; ++i)
        {
            _categories[i] = (categoryNames[i], defaultIcon);
        }

        var exVersion = Service.LuminaSheet<ExVersion>()!;
        Customize(BossModuleInfo.Expansion.RealmReborn, 61875u, exVersion.GetRow(0u).Name);
        Customize(BossModuleInfo.Expansion.Heavensward, 61876u, exVersion.GetRow(1u).Name);
        Customize(BossModuleInfo.Expansion.Stormblood, 61877u, exVersion.GetRow(2u).Name);
        Customize(BossModuleInfo.Expansion.Shadowbringers, 61878u, exVersion.GetRow(3u).Name);
        Customize(BossModuleInfo.Expansion.Endwalker, 61879u, exVersion.GetRow(4u).Name);
        Customize(BossModuleInfo.Expansion.Dawntrail, 61880u, exVersion.GetRow(5u).Name);

        var contentType = Service.LuminaSheet<ContentType>()!;
        Customize(BossModuleInfo.Category.Dungeon, contentType.GetRow(2u));
        Customize(BossModuleInfo.Category.Trial, contentType.GetRow(4u));
        Customize(BossModuleInfo.Category.Raid, contentType.GetRow(5u));
        Customize(BossModuleInfo.Category.Chaotic, contentType.GetRow(37u));
        Customize(BossModuleInfo.Category.PVP, contentType.GetRow(6u));
        Customize(BossModuleInfo.Category.Quest, contentType.GetRow(7u));
        Customize(BossModuleInfo.Category.FATE, contentType.GetRow(8u));
        Customize(BossModuleInfo.Category.TreasureHunt, contentType.GetRow(9u));
        Customize(BossModuleInfo.Category.GoldSaucer, contentType.GetRow(19u));
        Customize(BossModuleInfo.Category.DeepDungeon, contentType.GetRow(21u));
        Customize(BossModuleInfo.Category.Quantum, contentType.GetRow(21u), "Quantum");
        Customize(BossModuleInfo.Category.Ultimate, contentType.GetRow(28u));
        Customize(BossModuleInfo.Category.VariantCriterion, contentType.GetRow(30u));
        Customize(BossModuleInfo.Category.HallOfTheNovice, contentType.GetRow(20u), "Hall of the Novice");

        var playStyle = Service.LuminaSheet<CharaCardPlayStyle>()!;
        Customize(BossModuleInfo.Category.Foray, playStyle.GetRow(6u));
        Customize(BossModuleInfo.Category.MaskedCarnivale, playStyle.GetRow(8u));
        Customize(BossModuleInfo.Category.Hunt, playStyle.GetRow(10u));

        _categories[(int)BossModuleInfo.Category.Extreme].icon = _categories[(int)BossModuleInfo.Category.Trial].icon;
        _categories[(int)BossModuleInfo.Category.Unreal].icon = _categories[(int)BossModuleInfo.Category.Trial].icon;
        _categories[(int)BossModuleInfo.Category.Savage].icon = _categories[(int)BossModuleInfo.Category.Raid].icon;
        _categories[(int)BossModuleInfo.Category.Alliance].icon = _categories[(int)BossModuleInfo.Category.Raid].icon;
        //_categories[(int)BossModuleInfo.Category.Event].icon = GetIcon(61757);

        _iconFATE = contentType.GetRow(8u).Icon;
        _iconHunt = (uint)playStyle.GetRow(10u).Icon;

        _groups = new List<ModuleGroup>[(int)BossModuleInfo.Expansion.Count, (int)BossModuleInfo.Category.Count];
        for (var i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            for (var j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
            {
                _groups[i, j] = [];
            }
        }

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

        for (var i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            for (var j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
            {
                var groups = _groups[i, j];
                groups.Sort(static (a, b) => a.Info.SortOrder.CompareTo(b.Info.SortOrder));

                var count = groups.Count;
                for (var g = 0; g < count - 1; ++g)
                {
                    var g1 = groups[g];
                    var g2 = groups[g + 1];
                    if (g1.Info.SortOrder == g2.Info.SortOrder)
                    {
                        Service.Log($"[ModuleViewer] Same sort order between groups {g1.Info} and {g2.Info}");
                    }
                }

                for (var g = 0; g < count; ++g)
                {
                    var group = groups[g];
                    group.Modules.Sort(static (a, b) => a.SortOrder.CompareTo(b.SortOrder));

                    var countM = group.Modules.Count - 1;
                    for (var m = 0; m < countM; ++m)
                    {
                        var m1 = group.Modules[m];
                        var m2 = group.Modules[m + 1];
                        if (m1.SortOrder == m2.SortOrder)
                        {
                            Service.Log($"[ModuleViewer] Same sort order between modules {m1.Info.ModuleType.FullName} and {m2.Info.ModuleType.FullName}");
                        }
                    }
                }
            }
        }
    }

    public void Dispose()
    {
    }

    public void Draw(UITree tree, WorldState ws)
    {
        var availWidth = ImGui.GetContentRegionAvail().X;
        var filterWidth = 300f; // Fixed width for filter panel
        var moduleWidth = availWidth - filterWidth - ImGui.GetStyle().ItemSpacing.X;

        using (var child = ImRaii.Child("FiltersPanel", new Vector2(filterWidth, 0), true))
        {
            if (child)
                DrawFilters();
        }

        ImGui.SameLine();
        using (var child = ImRaii.Child("ModulesPanel", new Vector2(moduleWidth, 0), true))
        {
            if (child)
                DrawModules(tree, ws);
        }
    }

    private void DrawFilters()
    {
        using var table = ImRaii.Table("Filters", 1, ImGuiTableFlags.BordersInner | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.SizingFixedSame);
        if (!table)
        {
            return;
        }

        ImGui.TableNextColumn();
        ImGui.TableNextColumn(); //spacing with only one seemed to be a bit small on certain window sizes
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Search:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        DrawSearchBar();
        ImGui.TableNextColumn();

        ImGui.TableNextColumn();
        ImGui.TableHeader("Expansion");
        ImGui.TableNextRow(ImGuiTableRowFlags.None);
        ImGui.TableNextColumn();
        DrawExpansionFilters();

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.TableHeader("Content");
        ImGui.TableNextRow(ImGuiTableRowFlags.None);
        ImGui.TableNextColumn();
        DrawContentTypeFilters();
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
            ref var expansion = ref _expansions[(int)e];
            UIMisc.ImageToggleButton(Service.Texture?.GetFromGameIcon(expansion.icon), _iconSize, !_filterExpansions[(int)e], expansion.name);
            if (ImGui.IsItemClicked())
            {
                _filterExpansions.Toggle((int)e);
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _filterExpansions = ~_filterExpansions;
                _filterExpansions.Toggle((int)e);
            }
        }
    }

    private void DrawContentTypeFilters()
    {
        for (var c = BossModuleInfo.Category.Uncategorized; c < BossModuleInfo.Category.Count; ++c)
        {
            ref var category = ref _categories[(int)c];
            UIMisc.ImageToggleButton(Service.Texture?.GetFromGameIcon(category.icon), _iconSize, !_filterCategories[(int)c], category.name);
            if (ImGui.IsItemClicked())
            {
                _filterCategories.Toggle((int)c);
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _filterCategories = ~_filterCategories;
                _filterCategories.Toggle((int)c);
            }
        }
    }

    private void DrawModules(UITree tree, WorldState ws)
    {
        using var table = ImRaii.Table("ModulesTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersV | ImGuiTableFlags.RowBg | ImGuiTableFlags.NoHostExtendX);
        if (!table)
        {
            return;
        }

        for (var i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            if (_filterExpansions[i])
            {
                continue;
            }

            for (var j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
            {
                if (_filterCategories[j])
                {
                    continue;
                }

                foreach (var group in _groups[i, j])
                {
                    if (!_searchText.IsNullOrEmpty() && !group.Info.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    UIMisc.Image(Service.Texture?.GetFromGameIcon(_expansions[i].icon), new(36));
                    ImGui.SameLine();
                    UIMisc.Image(Service.Texture?.GetFromGameIcon(group.Info.Icon != 0 ? group.Info.Icon : _categories[j].icon), new(36));
                    ImGui.TableNextColumn();

                    foreach (var ng in tree.Node($"{group.Info.Name}###{i}/{j}/{group.Info.Id}"))
                    {
                        foreach (var mod in group.Modules)
                        {
                            if (mod.Info.Maturity == BossModuleInfo.Maturity.Dummy)
                            {
                                continue;
                            }

                            using (ImRaii.Disabled(mod.Info.ConfigType == null))
                            {
                                if (UIMisc.IconButton(FontAwesomeIcon.Cog, $"{mod.Info.ModuleType.FullName}_cfg"))
                                {
                                    _ = new BossModuleConfigWindow(mod.Info, ws);
                                }
                            }

                            ImGui.SameLine();
                            using (ImRaii.Disabled(mod.Info.PlanLevel == 0))
                            {
                                if (UIMisc.IconButton(FontAwesomeIcon.ClipboardList, $"{mod.Info.ModuleType.FullName}_plans"))
                                {
                                    ImGui.OpenPopup($"{mod.Info.ModuleType.FullName}_popup");
                                }
                            }

                            ImGui.SameLine();
                            UIMisc.HelpMarker(() => ModuleHelpText(mod));
                            ImGui.SameLine();
                            var textColor = mod.Info.Maturity switch
                            {
                                BossModuleInfo.Maturity.WIP => Colors.TextColor3,
                                BossModuleInfo.Maturity.Verified => Colors.TextColor4,
                                BossModuleInfo.Maturity.AISupport => Colors.TextColor2,
                                _ => Colors.TextColor1
                            };
                            using (ImRaii.PushColor(ImGuiCol.Text, textColor))
                            {
                                ImGui.TextUnformatted($"{mod.Name} [{mod.Info.ModuleType.Name}]");
                            }

                            using (var popup = ImRaii.Popup($"{mod.Info.ModuleType.FullName}_popup"))
                            {
                                if (popup)
                                {
                                    ModulePlansPopup(mod.Info);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void Customize(BossModuleInfo.Expansion expansion, uint iconId, ReadOnlySeString name) => _expansions[(int)expansion] = (name.ToString(), iconId);
    private void Customize(BossModuleInfo.Category category, uint iconId, ReadOnlySeString name) => _categories[(int)category] = (name.ToString(), iconId);
    private void Customize(BossModuleInfo.Category category, ContentType ct, string? name = null) => Customize(category, ct.Icon, name ?? ct.Name);
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
                var fixedName = RegexHelper.RemoveTags(cfcRow.Name.ToString());
                return (new(FixCase(fixedName), groupId, cfcSort != 0 ? cfcSort : groupId),
                        new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.MaskedCarnivale:
                groupId |= module.GroupID;
                var mcRow = Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value;
                var mcSort = uint.Parse(mcRow.ShortCode.ToString().AsSpan(3), CultureInfo.InvariantCulture); // 'aozNNN'
                var mcName = $"Stage {mcSort}: {FixCase(mcRow.Name)}";
                return (new(mcName, groupId, mcSort), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.RemovedUnreal:
                return (new("Removed Content", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.BaldesionArsenal:
                return (new("Baldesion Arsenal", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.CastrumLacusLitore:
                return (new("Castrum Lacus Litore", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.TheDalriada:
                return (new("The Dalriada", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.TheForkedTowerBlood:
                return (new("The Forked Tower: Blood", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
            case BossModuleInfo.GroupType.ForayFATE:
                var fateRowBozjaSkirmish = Service.LuminaRow<Fate>(module.NameID)!.Value;
                var skirmishName = $"{FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.Name)} FATE";
                return (new(skirmishName, groupId, groupId), new(module, $"{fateRowBozjaSkirmish.Name}", module.SortOrder));
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
            case BossModuleInfo.GroupType.CriticalEngagement:
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
                return (new("Gold Saucer", groupId, groupId), new(module, $"{Service.LuminaRow<GoldSaucerTextData>(module.GroupID)?.Text}: {BNpcName(module.NameID)}", module.SortOrder));
            default:
                return (new("Uncategorized", groupId, groupId), new(module, BNpcName(module.NameID), module.SortOrder));
        }
    }

    private string ModuleHelpText(ModuleInfo info)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.CurrentCulture, $"Cooldown planning: {(info.Info.PlanLevel > 0 ? $"L{info.Info.PlanLevel}" : "not supported")}");
        if (info.Info.Contributors.Length > 0)
        {
            sb.AppendLine(CultureInfo.CurrentCulture, $"Contributors: {info.Info.Contributors}");
        }

        return sb.ToString();
    }

    private void ModulePlansPopup(BossModuleRegistry.Info info)
    {
        if (_planDB == null)
        {
            return;
        }

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

public static partial class RegexHelper
{
    [GeneratedRegex("<italic\\(\\d\\)>|<-->")]
    private static partial Regex TagsRegex();

    public static string RemoveTags(string input) => TagsRegex().Replace(input, string.Empty);
}
