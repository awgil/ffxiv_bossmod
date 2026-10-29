using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

// includes a bunch of random general config options, the name of the class is vestigial
[ConfigDisplay(Name = "Interface", Order = -10, Group = "General")]
public sealed class DtrConfig : ConfigNode
{
    [PropertyDisplay("Hide windows when game UI is hidden")]
    public bool HideWithGameUi = true;

    [PropertyDisplay("Occlude in-world guide elements", tooltip: "If enabled, shapes that BossMod draws in the game world, such as movement hints and deep dungeon trap indicators, will appear 'behind' other objects the same way that native AOE indicators do.", since: "7.5.7.15")]
    public bool Occlude = false;

    [PropertyDisplay("Occluded item transparency", depends: nameof(Occlude))]
    [PropertySlider(0, 1)]
    public float OcclusionAlpha = 0.1f;

    public enum DtrStatus
    {
        [PropertyDisplay("Disabled")]
        None,
        [PropertyDisplay("Text only")]
        TextOnly,
        [PropertyDisplay("With icon")]
        Icon
    }

    [PageStart("Server info bar")]
    [PropertyDisplay("Autorotation status indicator")]
    [PropertyRadio(inline: true)]
    public DtrStatus ShowAutorot = Service.Config.Get<AutorotationConfig>().ShowDTR;

    [PropertyDisplay("Show legacy AI status")]
    public bool ShowAI = Service.Config.Get<AIConfig>().ShowDTR;

    [PropertyDisplay("Show performance stats")]
    public bool ShowStats = Service.Config.Get<AutorotationConfig>().ShowStatsDTR;

    public List<string> ShowPresets = ["VBM Default", "VBM AI"];

    public override void DrawCustom(string? page, UITree tree, WorldState ws, RotationDatabase? db)
    {
        if (db == null || page != "Server info bar")
            return;

        var modified = false;

        ConfigUI.DrawHelp("A separate info bar entry will be created for each preset you select here. The entry can be clicked to toggle the preset on and off.");

        ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);

        List<string> add = [];
        List<string> remove = [];

        using (var combo = ImRaii.Combo("Presets to display", "Select..."))
        {
            if (combo)
            {
                foreach (var p in db.Presets.AllPresets.Where(p => !ShowPresets.Contains(p.Name)))
                {
                    if (ImGui.Selectable(p.Name))
                        add.Add(p.Name);
                }
            }
        }

        ConfigUI.DrawHelp("");

        foreach (var p in ShowPresets)
        {
            ImGui.Button(p);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                remove.Add(p);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Right-click to remove");
            ImGui.SameLine();
        }

        using (ImRaii.Disabled(ShowPresets.Count == 0))
        {
            if (UIMisc.IconButtonWithText(Dalamud.Interface.FontAwesomeIcon.SquareXmark, "Remove all"))
                remove.AddRange(ShowPresets);
        }

        if (add.Count > 0 || remove.Count > 0)
        {
            ShowPresets.RemoveAll(remove.Contains);
            ShowPresets.AddRange(add);
            modified = true;
        }

        if (modified)
            Modified.Fire();
    }
}
