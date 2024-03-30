using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using System.Data;
using System.Globalization;

namespace BossMod;

public class ModuleViewer : IDisposable
{
    private BitMask _filterExpansions;
    private BitMask _filterCategories;

    private (string name, IDalamudTextureWrap? icon)[] _expansions;
    private (string name, IDalamudTextureWrap? icon)[] _categories;

    private Vector2 _iconSize = new(30, 30);

    public ModuleViewer()
    {
        var defaultIcon = GetIcon(61762);
        _expansions = Enum.GetNames<BossModuleInfo.Expansion>().Take((int)BossModuleInfo.Expansion.Count).Select(n => (n, defaultIcon)).ToArray();
        _categories = Enum.GetNames<BossModuleInfo.Category>().Take((int)BossModuleInfo.Category.Count).Select(n => (n, defaultIcon)).ToArray();

        var exVersion = Service.LuminaGameData?.GetExcelSheet<ExVersion>();
        Customize(BossModuleInfo.Expansion.RealmReborn, 61875, exVersion?.GetRow(0)?.Name);
        Customize(BossModuleInfo.Expansion.Heavensward, 61876, exVersion?.GetRow(1)?.Name);
        Customize(BossModuleInfo.Expansion.Stormblood, 61877, exVersion?.GetRow(2)?.Name);
        Customize(BossModuleInfo.Expansion.Shadowbringers, 61878, exVersion?.GetRow(3)?.Name);
        Customize(BossModuleInfo.Expansion.Endwalker, 61879, exVersion?.GetRow(4)?.Name);

        var contentType = Service.LuminaGameData?.GetExcelSheet<ContentType>();
        Customize(BossModuleInfo.Category.Dungeon, contentType?.GetRow(2));
        Customize(BossModuleInfo.Category.Trial, contentType?.GetRow(4));
        Customize(BossModuleInfo.Category.Raid, contentType?.GetRow(5));
        Customize(BossModuleInfo.Category.PVP, contentType?.GetRow(6));
        Customize(BossModuleInfo.Category.Quest, contentType?.GetRow(7));
        Customize(BossModuleInfo.Category.FATE, contentType?.GetRow(8));
        Customize(BossModuleInfo.Category.TreasureHunt, contentType?.GetRow(9));
        Customize(BossModuleInfo.Category.GoldSaucer, contentType?.GetRow(19));
        Customize(BossModuleInfo.Category.DeepDungeon, contentType?.GetRow(21));
        Customize(BossModuleInfo.Category.Ultimate, contentType?.GetRow(28));
        Customize(BossModuleInfo.Category.Criterion, contentType?.GetRow(30));

        var playStyle = Service.LuminaGameData?.GetExcelSheet<CharaCardPlayStyle>();
        Customize(BossModuleInfo.Category.Foray, playStyle?.GetRow(6));
        Customize(BossModuleInfo.Category.MaskedCarnivale, playStyle?.GetRow(8));
        Customize(BossModuleInfo.Category.Hunt, playStyle?.GetRow(10));

        _categories[(int)BossModuleInfo.Category.Extreme].icon = _categories[(int)BossModuleInfo.Category.Trial].icon;
        _categories[(int)BossModuleInfo.Category.Unreal].icon = _categories[(int)BossModuleInfo.Category.Trial].icon;
        _categories[(int)BossModuleInfo.Category.Savage].icon = _categories[(int)BossModuleInfo.Category.Raid].icon;
        _categories[(int)BossModuleInfo.Category.Alliance].icon = _categories[(int)BossModuleInfo.Category.Raid].icon;
        _categories[(int)BossModuleInfo.Category.Event].icon = GetIcon(61757);
    }

    public void Dispose()
    {
        foreach (var e in _expansions)
            e.icon?.Dispose();
        foreach (var c in _categories)
            c.icon?.Dispose();
    }

    public void Draw(UITree _tree)
    {
        using (var group = ImRaii.Group())
            DrawFilters();
        ImGui.SameLine();
        using (var group = ImRaii.Group())
            DrawModules(_tree);
    }

    private void DrawFilters()
    {
        using var table = ImRaii.Table("Filters", 1, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.SizingFixedSame | ImGuiTableFlags.ScrollY);
        if (!table)
            return;

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

    private void DrawExpansionFilters()
    {
        for (var e = BossModuleInfo.Expansion.RealmReborn; e < BossModuleInfo.Expansion.Count; ++e)
        {
            ref var expansion = ref _expansions[(int)e];
            UIMisc.ImageToggleButton(expansion.icon, _iconSize, !_filterExpansions[(int)e], expansion.name);
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
            UIMisc.ImageToggleButton(category.icon, _iconSize, !_filterCategories[(int)c], category.name);
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

    private void DrawModules(UITree _tree)
    {
        using var table = ImRaii.Table("ModulesTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.NoHostExtendX);
        if (!table)
            return;

        DrawRows(ModuleRegistry.CataloguedModules.DistinctBy(x => x.DisplayName), _tree);
        DrawRows(ModuleRegistry.UncataloguedModules, _tree);
    }

    private void DrawRows(IEnumerable<ModuleRegistry.Info> enumerable, UITree _tree)
    {
        foreach (var mod in enumerable)
        {
            if (_filterExpansions[(int)mod.Expansion] || _filterCategories[(int)mod.Category])
                continue;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            UIMisc.Image(_expansions[(int)mod.Expansion].icon, new(36));
            ImGui.SameLine();
            UIMisc.Image(_categories[(int)mod.Category].icon, new(36));
            ImGui.TableNextColumn();
            foreach (var _ in _tree.Node($"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(mod.DisplayName!)}##{mod.PrimaryActorOID}"))
                DrawBosses(ModuleRegistry.RegisteredModules.Values, mod.DisplayName ?? new());
        }
    }

    private void DrawBosses(IEnumerable<ModuleRegistry.Info> modules, SeString name)
    {
        foreach (var mod in modules.Where(x => x.DisplayName == name))
            if (!mod.BossName!.RawString.IsNullOrEmpty())
                ImGui.TextUnformatted($"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(mod.BossName)}");
    }

    private void Customize((string name, IDalamudTextureWrap? icon)[] array, int element, uint iconId, SeString? name)
    {
        var icon = iconId != 0 ? GetIcon(iconId) : null;
        if (icon != null)
            array[element].icon = icon;
        if (name != null)
            array[element].name = name;
    }
    private void Customize(BossModuleInfo.Expansion expansion, uint iconId, SeString? name) => Customize(_expansions, (int)expansion, iconId, name);
    private void Customize(BossModuleInfo.Category category, uint iconId, SeString? name) => Customize(_categories, (int)category, iconId, name);
    private void Customize(BossModuleInfo.Category category, ContentType? ct) => Customize(category, ct?.Icon ?? 0, ct?.Name);
    private void Customize(BossModuleInfo.Category category, CharaCardPlayStyle? ps) => Customize(category, (uint)(ps?.Icon ?? 0), ps?.Name);

    private static IDalamudTextureWrap? GetIcon(uint iconId) => Service.Texture?.GetIcon(iconId, Dalamud.Plugin.Services.ITextureProvider.IconFlags.HiRes);
}
