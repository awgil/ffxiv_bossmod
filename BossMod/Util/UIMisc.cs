using ImGuiNET;
using System.Numerics;

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

        public static void TextUnderlined(Vector4 colour, string text)
        {
            var size = ImGui.CalcTextSize(text);
            var cur = ImGui.GetCursorScreenPos();
            cur.Y += size.Y;
            ImGui.GetWindowDrawList().PathLineTo(cur);
            cur.X += size.X;
            ImGui.GetWindowDrawList().PathLineTo(cur);
            ImGui.GetWindowDrawList().PathStroke(ImGui.ColorConvertFloat4ToU32(colour));
            ImGui.TextColored(colour, text);
        }
    }
}
