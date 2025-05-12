using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod;

public static class UIMisc
{
    public static bool Button(string label, float width, params (bool disabled, string reason)[] disabled)
    {
        var isDisabled = disabled.Any(d => d.disabled);
        using var disable = ImRaii.Disabled(isDisabled);
        var res = ImGui.Button(label, new(width, 0));
        if (isDisabled && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            using var tooltip = ImRaii.Tooltip();
            foreach (var d in disabled)
                if (d.disabled)
                    ImGui.TextUnformatted(d.reason);
        }
        return res;
    }
    public static bool Button(string label, bool disabled, string reason, float width = 0) => Button(label, width, (disabled, reason));

    // button that is disabled unless shift is held, useful for 'dangerous' operations like deletion
    public static bool DangerousButton(string label, float width = 0) => Button(label, !ImGui.IsKeyDown(ImGuiKey.ModShift), "Hold shift", width);

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

    public static void Image(ISharedImmediateTexture? icon, Vector2 size)
    {
        var wrap = icon?.GetWrapOrDefault();
        if (wrap != null)
            ImGui.Image(wrap.ImGuiHandle, size);
        else
            ImGui.Dummy(size);
    }

    public static void ImageWithText(ISharedImmediateTexture? icon, Vector2 size, bool state, string text)
    {
        var c = ImGui.GetCursorPos();
        ImGui.Selectable($"##sel{text}", false, ImGuiSelectableFlags.None, new(ImGui.GetContentRegionAvail().X, size.Y));
        ImGui.SetCursorPos(c);

        var wrap = icon?.GetWrapOrDefault();
        if (wrap != null)
        {
            Vector4 tintColor = state ? new(1f, 1f, 1f, 1f) : new(0.5f, 0.5f, 0.5f, 0.85f);
            ImGui.Image(wrap.ImGuiHandle, size, Vector2.Zero, Vector2.One, tintColor);
        }
        else
        {
            ImGui.Dummy(size);
        }

        ImGui.SameLine();
        var cursor = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new(cursor.X, cursor.Y + 0.5f * (size.Y - ImGui.GetFontSize())));
        ImGui.TextUnformatted(text);
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
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            using var tooltip = ImRaii.Tooltip();
            using var wrap = ImRaii.TextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(helpText());
        }
    }
    public static void HelpMarker(string helpText, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle) => HelpMarker(() => helpText, icon);
}
