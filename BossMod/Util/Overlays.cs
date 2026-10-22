using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace BossMod;

public static class Overlays
{
    public static void DrawPseudoTitlebar(string title, Type configType, uint iconId, string closeTooltip, Action onClose)
    {
        if (iconId != 0 && Service.Texture.TryGetFromGameIcon(iconId, out var tex))
        {
            UIMisc.Image(tex, new(24 * ImGuiHelpers.GlobalScale));
            ImGui.SameLine();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Text(title);

        var w = 24 * ImGuiHelpers.GlobalScale;

        ImGui.SameLine(0, 0);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - w - w - 10);

        if (UIMisc.IconButton(FontAwesomeIcon.Cog, new(w, 0)))
            Service.Mediator.Publish(new ShowConfigNode(configType));
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Open settings");
        ImGui.SameLine();
        if (UIMisc.IconButton(FontAwesomeIcon.SquareXmark, new(w, 0)))
            onClose.Invoke();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(closeTooltip);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
}
