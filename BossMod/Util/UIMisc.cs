using ImGuiNET;

namespace BossMod
{
    public static class UIMisc
    {
        // button that is disabled unless shift is held, useful for 'dangerous' operations like deletion
        public static bool DangerousButton(string label)
        {
            bool disabled = !ImGui.IsKeyDown(ImGuiKey.ModShift);
            ImGui.BeginDisabled(disabled);
            bool res = ImGui.Button(label);
            ImGui.EndDisabled();
            if (disabled && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                ImGui.SetTooltip("Hold shift");
            return res;
        }
    }
}
