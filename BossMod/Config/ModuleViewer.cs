using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using System.Data;
using System.Globalization;

namespace BossMod;

public class ModuleViewer
{
    private const int SpaceBetweenFilterWidgets = 3;

    private BitMask _availableExpansions;
    private BitMask _filterExpansions;
    private readonly Dictionary<SeString, bool> ContentFilter;

    private Vector2 _iconSize = new(30, 30);

    public ModuleViewer()
    {
        foreach (var m in ModuleRegistry.RegisteredModules.Values)
            _availableExpansions.Set((int)m.Expansion);
        _availableExpansions.Clear((int)BossModuleInfo.Expansion.Count);

        ContentFilter = ModuleRegistry.AvailableContent.ToDictionary(c => c, c => true);
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
        foreach (var e in _availableExpansions.SetBits())
        {
            var expansion = (BossModuleInfo.Expansion)e;
            UIMisc.ImageToggleButton(GetExpansionIcon(expansion), _iconSize, !_filterExpansions[e], GetExpansionName(expansion));
            if (ImGui.IsItemClicked())
            {
                _filterExpansions.Toggle(e);
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _filterExpansions ^= _availableExpansions;
                _filterExpansions.Toggle(e);
            }
        }
    }

    private void DrawContentTypeFilters()
    {
        foreach (var cont in ModuleRegistry.AvailableContentIcons)
        {
            UIMisc.ImageToggleButton(GetIcon(cont.Value), _iconSize, ContentFilter[cont.Key], cont.Key);
            if (ImGui.IsItemClicked())
                ContentFilter[cont.Key] = !ContentFilter[cont.Key];
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                ContentFilter.Keys.Except([cont.Key]).ToList().ForEach(k => ContentFilter[k] = !ContentFilter[k]);
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
            if (!ContentFilter[mod.ContentType ?? new()] || _filterExpansions[(int)mod.Expansion])
                continue;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            UIMisc.Image(GetExpansionIcon(mod.Expansion), new(36));
            ImGui.SameLine();
            UIMisc.Image(GetIcon(mod.ContentIcon), new(36));
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

    private string GetExpansionName(BossModuleInfo.Expansion expansion) => Service.LuminaRow<ExVersion>((uint)expansion)?.Name ?? expansion.ToString();
    private IDalamudTextureWrap? GetExpansionIcon(BossModuleInfo.Expansion expansion) => GetIcon(expansion switch
    {
        BossModuleInfo.Expansion.RealmReborn => 61875,
        BossModuleInfo.Expansion.Heavensward => 61876,
        BossModuleInfo.Expansion.Stormblood => 61877,
        BossModuleInfo.Expansion.Shadowbringers => 61878,
        BossModuleInfo.Expansion.Endwalker => 61879,
        _ => 0,
    });

    private static IDalamudTextureWrap? GetIcon(uint iconId) => iconId != 0 ? Service.Texture.GetIcon(iconId, Dalamud.Plugin.Services.ITextureProvider.IconFlags.HiRes) : null;
}
