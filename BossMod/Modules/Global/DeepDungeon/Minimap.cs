using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Global.DeepDungeon;

public record class Minimap(byte[] MapData, int CurrentDestination)
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Integer index of the room the user clicked on.</returns>
    public int Draw()
    {
        var dest = -1;

        // UIDev - TODO fix this
        if (Service.Texture == null)
            return 0;

        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 0f));

        if (Service.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_Rooms_hr1.tex")?.TryGetWrap(out var tex, out var exc) ?? false)
        {
            for (var i = 0; i < 25; i++)
            {
                var highlight = CurrentDestination > 0 && CurrentDestination == i;
                using var _1 = ImRaii.PushId($"room{i}");

                var pos = ImGui.GetCursorPos();
                var tile = MapData[i] & 0xF;
                var row = tile / 4;
                var col = tile % 4;

                var xoff = 0.0104f + col * 0.25f;
                var yoff = 0.0104f + row * 0.25f;
                var xoffend = xoff + 0.2292f;
                var yoffend = yoff + 0.2292f;

                ImGui.SetCursorPos(pos);
                ImGui.Image(tex.ImGuiHandle, highlight ? new(86) : new(88), new Vector2(xoff, yoff), new Vector2(xoffend, yoffend), tile > 0 ? new(1f) : new(0.6f), highlight ? new(0, 0.6f, 0, 1) : default);
                ImGui.SetCursorPos(pos + new Vector2(27, 28));
                if (tile > 0 && ImGuiComponents.IconButton(Dalamud.Interface.FontAwesomeIcon.Crosshairs))
                {
                    Service.Log($"dest: {i}");
                    dest = i;
                }
                ImGui.SetCursorPos(pos);
                ImGui.Dummy(new(88, 88));
                if (i % 5 < 4)
                    ImGui.SameLine();
            }
        }
        else
        {
            ImGui.Text("unable to load texture");
        }

        return dest;
    }
}
