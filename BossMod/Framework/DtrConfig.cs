using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

[ConfigDisplay(Name = "Server info bar", Order = 6, Group = "General")]
public sealed class DtrConfig : ConfigNode
{
    public enum DtrStatus
    {
        [PropertyDisplay("Disabled")]
        None,
        [PropertyDisplay("Text only")]
        TextOnly,
        [PropertyDisplay("With icon")]
        Icon
    }

    [PropertyDisplay("Autorotation status indicator")]
    [PropertyRadio(inline: true)]
    public DtrStatus ShowAutorot = Service.Config.Get<AutorotationConfig>().ShowDTR;

    [PropertyDisplay("Show legacy AI status")]
    public bool ShowAI = Service.Config.Get<AIConfig>().ShowDTR;

    [PropertyDisplay("Show performance stats")]
    public bool ShowStats = Service.Config.Get<AutorotationConfig>().ShowStatsDTR;

    public List<string> ShowPresets = ["VBM Default", "VBM AI"];

    public override void DrawCustom(UITree tree, WorldState ws, RotationDatabase? db)
    {
        if (db == null)
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
