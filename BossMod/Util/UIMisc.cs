using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public static class UIMisc
{
    // ABGR
    public const uint ColorGood = 0xff40c040;
    public const uint ColorInfo = 0xffe0a040;
    public const uint ColorWarning = 0xff00c8ff;
    public const uint ColorDanger = 0xff4040e0;
    public const uint ColorActive = 0xff358a35;

    public static uint ScaleColor(uint color, float factor)
    {
        uint channel(int shift) => (uint)Math.Clamp(((color >> shift) & 0xff) * factor, 0, 255) << shift;
        return (color & 0xff000000) | channel(16) | channel(8) | channel(0);
    }

    public static void SectionHeader(string text)
    {
        ImGui.Spacing();
        using (ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.CheckMark)))
            ImGui.TextUnformatted(text);
        ImGui.Separator();
    }

    public static void Badge(string text, uint color, string tooltip = "")
    {
        var scale = ImGuiHelpers.GlobalScale;
        var pad = new Vector2(5, 1) * scale;
        var size = ImGui.CalcTextSize(text) + pad * 2;
        var pos = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();
        dl.AddRectFilled(pos, pos + size, (color & 0x00ffffff) | 0x40000000, 4 * scale);
        dl.AddRect(pos, pos + size, color, 4 * scale);
        dl.AddText(pos + pad, color, text);
        ImGui.Dummy(size);
        if (tooltip.Length > 0 && ImGui.IsItemHovered())
        {
            using var tt = ImRaii.Tooltip();
            ImGui.TextUnformatted(tooltip);
        }
    }

    public static void Banner(FontAwesomeIcon icon, string text, uint color)
    {
        var pad = 4 * ImGuiHelpers.GlobalScale;
        var start = ImGui.GetCursorScreenPos();
        var right = start.X + ImGui.GetContentRegionAvail().X;
        var dl = ImGui.GetWindowDrawList();
        dl.ChannelsSplit(2);
        dl.ChannelsSetCurrent(1);
        ImGui.SetCursorScreenPos(start + new Vector2(pad, pad));
        ImGui.BeginGroup();
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            IconText(icon);
        ImGui.SameLine();
        using (ImRaii.TextWrapPos(right - ImGui.GetWindowPos().X - pad))
            ImGui.TextUnformatted(text);
        ImGui.EndGroup();
        var max = new Vector2(right, ImGui.GetItemRectMax().Y + pad);
        dl.ChannelsSetCurrent(0);
        dl.AddRectFilled(start, max, (color & 0x00ffffff) | 0x30000000, pad);
        dl.AddRect(start, max, (color & 0x00ffffff) | 0x90000000, pad);
        dl.ChannelsMerge();
        ImGui.SetCursorScreenPos(new(start.X, max.Y + ImGui.GetStyle().ItemSpacing.Y));
        ImGui.Dummy(default);
    }

    public static bool ToggleButton(string label, bool active, uint activeColor = ColorActive)
    {
        using var c1 = ImRaii.PushColor(ImGuiCol.Button, activeColor, active);
        using var c2 = ImRaii.PushColor(ImGuiCol.ButtonHovered, ScaleColor(activeColor, 1.3f), active);
        using var c3 = ImRaii.PushColor(ImGuiCol.ButtonActive, ScaleColor(activeColor, 1.5f), active);
        return ImGui.Button(label);
    }

    public static float ButtonWidth(string label) => ImGui.CalcTextSize(label, true).X + ImGui.GetStyle().FramePadding.X * 2;

    public static void SameLineOrWrap(float nextItemWidth)
    {
        ImGui.SameLine();
        if (ImGui.GetContentRegionAvail().X < nextItemWidth)
            ImGui.NewLine();
    }

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
            ImGui.Image(wrap.Handle, size);
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
            Vector4 tintColor = state ? new(1) : new(0.5f, 0.5f, 0.5f, 0.85f);
            ImGui.Image(wrap.Handle, size, Vector2.Zero, Vector2.One, tintColor);
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

    public static bool IconButton(FontAwesomeIcon icon, string id, Vector2 size = default) => IconButtonRaw($"{icon.ToIconString()}##{id}", size);
    public static bool IconButton(FontAwesomeIcon icon, Vector2 size = default) => IconButtonRaw(icon.ToIconString(), size);

    static bool IconButtonRaw(string text, Vector2 size = default)
    {
        using (ImRaii.PushFont(Service.IconFont))
            return ImGui.Button(text, size);
    }

    public static bool IconButtonWithText(FontAwesomeIcon icon, string text)
    {
        bool button;

        Vector2 iconSize;
        using (ImRaii.PushFont(Service.IconFont))
            iconSize = ImGui.CalcTextSize(icon.ToIconString());

        var textStr = text;
        if (textStr.Contains('#', StringComparison.Ordinal))
            textStr = textStr[..textStr.IndexOf('#', StringComparison.Ordinal)];

        var framePadding = ImGui.GetStyle().FramePadding;
        var iconPadding = 3 * ImGuiHelpers.GlobalScale;

        var cursor = ImGui.GetCursorScreenPos();

        using (ImRaii.PushId(text))
        {
            var textSize = ImGui.CalcTextSize(textStr);
            var width = iconSize.X + textSize.X + framePadding.X * 2 + iconPadding;
            var height = ImGui.GetFrameHeight();

            button = ImGui.Button(string.Empty, new Vector2(width, height));
        }

        var iconPos = cursor + framePadding;
        var textPos = new Vector2(iconPos.X + iconSize.X + iconPadding, cursor.Y + framePadding.Y);

        var dl = ImGui.GetWindowDrawList();

        using (ImRaii.PushFont(Service.IconFont))
            dl.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());

        dl.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), textStr);

        return button;
    }

    public static void IconText(FontAwesomeIcon icon)
    {
        using var scope = ImRaii.PushFont(Service.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
    }

    public static void HelpMarker(Func<string> helpText, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle)
    {
        IconText(icon);
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            using var tooltip = ImRaii.Tooltip();
            using var wrap = ImRaii.TextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(helpText());
        }
    }
    public static void HelpMarker(string helpText, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle) => HelpMarker(() => helpText, icon);

    /// <summary>
    /// Draw rotated text at the current cursor position. The origin of rotation is the point on the left edge of the text's bounding box and at its vertical midpoint.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="radians"></param>
    public static unsafe void TextRotated(ImU8String text, float radians)
    {
        var dl = ImGui.GetWindowDrawList();
        var ts = ImGui.CalcTextSize(text, false);
        var startVert = dl.VtxCurrentIdx;
        var screenPos = ImGui.GetCursorScreenPos();
        dl.AddText(screenPos - new Vector2(0, ts.Y * 0.5f), ImGui.GetColorU32(ImGuiCol.Text), text);

        var endVert = dl.VtxCurrentIdx;

        for (var ix = (int)startVert; ix < endVert; ix++)
        {
            ref var vert = ref dl.Handle->VtxBuffer.Ref(ix);
            vert.Pos = Rotate(vert.Pos - screenPos, radians) + screenPos;
        }
    }

    static Vector2 Rotate(Vector2 vec, float rad)
    {
        return new WDir(vec).Rotate(new Angle(rad)).ToVec2();
    }
}
