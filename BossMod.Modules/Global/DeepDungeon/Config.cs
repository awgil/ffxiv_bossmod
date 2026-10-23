using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;

namespace BossMod.Global.DeepDungeon;

[ConfigDisplay(Name = "Auto-DeepDungeon (Experimental)", Parent = typeof(ModuleConfig))]
public class AutoDDConfig : ConfigNode
{
    public enum ClearBehavior
    {
        [PropertyDisplay("Do not auto target")]
        None,
        [PropertyDisplay("Stop when passage opens")]
        Passage,
        [PropertyDisplay("Target everything if not at level cap, otherwise stop when passage opens")]
        Leveling,
        [PropertyDisplay("Target everything")]
        All,
    }

    [PropertyDisplay("Enable module", tooltip: "This is experimental. Use at your own risk. You will sometimes need to help the AI to navigate.")]
    public bool Enable = true;
    [PropertyDisplay("Enable minimap")]
    public bool EnableMinimap = true;
    [PropertyDisplay("Minimap scale")]
    [PropertySlider(0.2f, 3)]
    public float MinimapScale = 1.0f;
    [PropertyDisplay("Try to avoid traps", tooltip: "Avoid known trap locations sourced from PalacePal data. (Traps revealed by a Pomander of Sight will always be avoided regardless of this setting.)")]
    public bool TrapHints = true;
    [PropertyDisplay("Draw potential trap locations in game world")]
    public bool DrawTraps = false;
    [PropertyDisplay("Automatically navigate to Cairn of Passage")]
    public bool AutoPassage = true;

    [PropertyDisplay("Automatic mob targeting behavior")]
    public ClearBehavior AutoClear = ClearBehavior.Leveling;

    [PropertyDisplay("Disable DoTs on non-boss floors (only affects VBM autorotation)")]
    public bool ForbidDOTs = false;

    [PropertyDisplay("Max number of mobs to pull before pausing navigation (0 = do not navigate while in combat)")]
    [PropertySlider(0, 15)]
    public int MaxPull = 0;
    [PropertyDisplay("Try to use terrain to LOS attacks", tooltip: "This feature is currently very inconsistent and will likely get you killed. Enabling it is not recommended.")]
    public bool AutoLOS = false;

    [PropertyDisplay("Automatically navigate to coffers")]
    public bool AutoMoveTreasure = true;
    [PropertyDisplay("Prioritize opening coffers over Cairn of Passage")]
    public bool OpenChestsFirst = false;
    [PropertyDisplay("Open gold coffers")]
    public bool GoldCoffer = true;
    [PropertyDisplay("Open silver coffers")]
    public bool SilverCoffer = true;
    [PropertyDisplay("Open bronze coffers")]
    public bool BronzeCoffer = true;

    [PropertyDisplay("Reveal all rooms before proceeding to next floor")]
    public bool FullClear = false;

    public BitMask AutoPoms;
    public BitMask AutoMagicite;
    public BitMask AutoDemiclone;

    public override void DrawCustom(string? page, UITree tree, WorldState ws, RotationDatabase? db)
    {
        UIMisc.SectionHeader("Auto-poms");

        using var _ = ImRaii.PushIndent(ImGui.GetFrameHeight());

        ImGui.Spacing();

        ImGui.TextWrapped("Selected pomanders will be used when a gold chest contains one that you can't carry. Disabled while in parties.");

        ImGui.Spacing();

        for (var i = 1; i < (int)PomanderID.Count; i++)
            using (ImRaii.PushId($"pom{i}"))
            {
                var row = Service.LuminaRow<DeepDungeonItem>((uint)i)!.Value;
                var wrap = Service.Texture.GetFromGameIcon(row.Icon).GetWrapOrEmpty();
                var scale = new Vector2(32 * ImGuiHelpers.GlobalScale);
                ImGui.Image(wrap.Handle, scale, new Vector2(0), tintCol: new Vector4(1) with { W = AutoPoms[i] ? 1 : 0.4f });
                if (ImGui.IsItemClicked())
                {
                    AutoPoms.Toggle(i);
                    Modified.Fire();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(row.Name.ToString());
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
                if (i % 8 > 0)
                    ImGui.SameLine();
            }

        ImGui.Text(""); // undo last sameline

        ImGui.Spacing();
    }
}
