using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using static System.Net.Mime.MediaTypeNames;

namespace BossMod;

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

    public static void Image(IDalamudTextureWrap? icon, Vector2 size)
    {
        if (icon != null)
            ImGui.Image(icon.ImGuiHandle, size);
        else
            ImGui.Dummy(size);
    }

    public static bool ImageToggleButton(IDalamudTextureWrap? icon, Vector2 size, bool state, string text)
    {
        var cursor = ImGui.GetCursorPos();
        var padding = ImGui.GetStyle().FramePadding;
        ImGui.SetCursorPos(new(cursor.X + size.X + 2 * padding.X, cursor.Y + 0.5f * (size.Y - ImGui.GetFontSize())));
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(cursor);

        if (icon != null)
        {
            Vector4 tintColor = state ? new(1f, 1f, 1f, 1f) : new(0.5f, 0.5f, 0.5f, 0.85f);
            return ImGui.ImageButton(icon.ImGuiHandle, size, Vector2.Zero, Vector2.One, 1, Vector4.Zero, tintColor);
        }
        else
        {
            return ImGui.Button("", size);
        }
    }

    // works around issues with fonts in uidev
    public static unsafe bool IconButton(FontAwesomeIcon icon, string fallback, string text)
    {
        if (Service.PluginInterface == null)
            return ImGui.Button(fallback + text);
        using var scope = ImRaii.PushFont(UiBuilder.IconFont);
        return ImGui.Button(icon.ToIconString() + text);
    }

    public static unsafe void IconText(FontAwesomeIcon icon, string fallback)
    {
        if (Service.PluginInterface == null)
        {
            ImGui.TextUnformatted(fallback);
        }
        else
        {
            using var scope = ImRaii.PushFont(UiBuilder.IconFont);
            ImGui.TextUnformatted(icon.ToIconString());
        }
    }

    public static void HelpMarker(Func<string> helpText, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle)
    {
        IconText(icon, "(?)");
        if (ImGui.IsItemHovered())
        {
            using var tooltip = ImRaii.Tooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(helpText());
            ImGui.PopTextWrapPos();
        }
    }
    public static void HelpMarker(string helpText, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle) => HelpMarker(() => helpText, icon);
}
