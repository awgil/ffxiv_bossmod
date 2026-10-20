using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;

namespace BossMod;

public class Overlays(MediatorService mediator, IUiBuilder uiBuilder, ITextureProvider texture)
{
    public void DrawPseudoTitlebar(string title, Type configType, uint iconId, string closeTooltip, Action onClose)
    {
        if (iconId != 0 && texture.TryGetFromGameIcon(iconId, out var tex))
        {
            UIMisc.Image(tex, new(24 * ImGuiHelpers.GlobalScale));
            ImGui.SameLine();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Text(title);

        var w = 24 * ImGuiHelpers.GlobalScale;

        ImGui.SameLine(0, 0);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - w - w - 10);

        if (uiBuilder.IconButton(FontAwesomeIcon.Cog, new(w, 0)))
            mediator.Publish(new ShowConfigNode(configType));
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Open settings");
        ImGui.SameLine();
        if (uiBuilder.IconButton(FontAwesomeIcon.SquareXmark, new(w, 0)))
            onClose.Invoke();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(closeTooltip);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
}
