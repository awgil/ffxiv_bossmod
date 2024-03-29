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
    private const int IconSize = 30;
    private readonly Dictionary<uint, bool> ExpansionFilter;
    private readonly Dictionary<SeString, bool> ContentFilter;
    private static readonly Lumina.Excel.ExcelSheet<ExVersion> _exSheet = Service.LuminaGameData!.GetExcelSheet<ExVersion>()!;
    protected static ImGuiTableFlags TABLE_FLAG = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.NoHostExtendX;

    private Dictionary<uint, int> exversionToIconID = new()
    {
        {0, 61875},
        {1, 61876},
        {2, 61877},
        {3, 61878},
        {4, 61879},
    };

    public ModuleViewer()
    {
        ExpansionFilter = ModuleRegistry.AvailableExpansions.ToDictionary(e => e, e => true);
        ContentFilter = ModuleRegistry.AvailableContent.ToDictionary(c => c, c => true);
    }

    public static string GetExpansionName(uint id) => _exSheet.GetRow(id)!.Name;

    public static IDalamudTextureWrap? GetIcon(int iconId) => Service.Texture.GetIcon((uint)iconId, Dalamud.Plugin.Services.ITextureProvider.IconFlags.HiRes);
    public static IDalamudTextureWrap? GetIcon(uint iconId) => GetIcon((int)iconId);

    public void Draw(UITree _tree)
    {
        using (var group = ImRaii.Group())
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
        ImGui.SameLine();
        using (var group = ImRaii.Group())
            DrawModules(_tree);
    }

    public void DrawExpansionFilters()
    {
        foreach (var expac in ModuleRegistry.AvailableExpansions)
        {
            var icon = GetIcon(exversionToIconID[expac]);
            if (icon != null)
            {
                UIMisc.ImageToggleButton(icon, new Vector2(IconSize, IconSize), ExpansionFilter[expac]);
                if (ImGui.IsItemClicked())
                    ExpansionFilter[expac] = !ExpansionFilter[expac];
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    ExpansionFilter.Keys.Except([expac]).ToList().ForEach(k => ExpansionFilter[k] = !ExpansionFilter[k]);
                ImGui.SameLine();
            }
            UIMisc.TextV($"{_exSheet.GetRow(expac)!.Name}");
        }
    }

    public void DrawContentTypeFilters()
    {
        foreach (var cont in ModuleRegistry.AvailableContentIcons)
        {
            var icon = GetIcon(cont.Value);
            if (icon != null)
            {
                UIMisc.ImageToggleButton(icon, new Vector2(IconSize, IconSize), ContentFilter[cont.Key]);
                if (ImGui.IsItemClicked())
                    ContentFilter[cont.Key] = !ContentFilter[cont.Key];
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    ContentFilter.Keys.Except([cont.Key]).ToList().ForEach(k => ContentFilter[k] = !ContentFilter[k]);
                ImGui.SameLine();
            }
            UIMisc.TextV($"{cont.Key}");
        }
    }

    public void DrawModules(UITree _tree)
    {
        using var table = ImRaii.Table("ModulesTable", 2, TABLE_FLAG);
        if (!table)
            return;

        DrawRows(ModuleRegistry.CataloguedModules.DistinctBy(x => x.DisplayName), _tree);
        DrawRows(ModuleRegistry.UncataloguedModules, _tree);
    }

    private void DrawRows(IEnumerable<ModuleRegistry.Info> enumerable, UITree _tree)
    {
        foreach (var mod in enumerable)
        {
            if (!ContentFilter[mod.ContentType ?? new()] || !ExpansionFilter[mod.ExVersion])
                continue;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Image(Service.Texture.GetIcon(mod.ContentIcon)!.ImGuiHandle, new Vector2(36));
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
}
