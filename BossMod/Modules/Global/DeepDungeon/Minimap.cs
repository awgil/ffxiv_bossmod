using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using ImGuiNET;

namespace BossMod.Global.DeepDungeon;

public record class Minimap(DeepDungeonState State, Angle PlayerRotation, int CurrentDestination)
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

        var playerCell = State.Party[0].Room - 1;

        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 0f));

        var roomsTex = Service.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_Rooms_hr1.tex").GetWrapOrEmpty();
        var mapTex = Service.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_hr1.tex").GetWrapOrEmpty();
        var passageTex = Service.Texture.GetFromGameIcon(new(State.PassageActive ? 60908u : 60907u)).GetWrapOrEmpty();
        var returnTex = Service.Texture.GetFromGameIcon(new(State.ReturnActive ? 60906u : 60905u)).GetWrapOrEmpty();

        for (var i = 0; i < 25; i++)
        {
            var highlight = CurrentDestination > 0 && CurrentDestination == i;

            var isValidDestination = State.MapData[i] > 0;

            using var _1 = ImRaii.PushId($"room{i}");

            var pos = ImGui.GetCursorPos();
            var tile = State.MapData[i] & 0xF;
            var row = tile / 4;
            var col = tile % 4;

            var xoff = 0.0104f + col * 0.25f;
            var yoff = 0.0104f + row * 0.25f;
            var xoffend = xoff + 0.2292f;
            var yoffend = yoff + 0.2292f;

            ImGui.SetCursorPos(pos);
            ImGui.Image(roomsTex.ImGuiHandle, highlight ? new(86) : new(88), new Vector2(xoff, yoff), new Vector2(xoffend, yoffend), tile > 0 ? new(1f) : new(0.6f), highlight ? new(0, 0.6f, 0, 1) : default);

            if (i == playerCell)
            {
                isValidDestination = false;
                ImGui.SetCursorPos(pos + new Vector2(12, 12));
                ImGui.Image(mapTex.ImGuiHandle, new Vector2(64, 64), new Vector2(0.2424f, 0.4571f), new Vector2(0.4848f, 0.6857f));
            }

            if (State.Map[i].HasFlag(InstanceContentDeepDungeon.RoomFlags.Passage))
            {
                ImGui.SetCursorPos(pos + new Vector2(20, 20));
                ImGui.Image(passageTex.ImGuiHandle, new Vector2(48, 48));
            }

            if (State.Map[i].HasFlag(InstanceContentDeepDungeon.RoomFlags.Return))
            {
                ImGui.SetCursorPos(pos + new Vector2(20, 20));
                ImGui.Image(returnTex.ImGuiHandle, new Vector2(48, 48));
            }

            if (i == playerCell)
            {
                ImGui.SetCursorPos(pos + new Vector2(44, 44));
                DrawPlayer(ImGui.GetCursorScreenPos(), PlayerRotation, mapTex.ImGuiHandle);
            }

            ImGui.SetCursorPos(pos);
            ImGui.Dummy(new(88, 88));
            if (isValidDestination)
            {
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    ImGui.SetTooltip(i == CurrentDestination ? "Click to clear destination" : "Click to set destination");
                }
                if (ImGui.IsItemClicked())
                    dest = i == CurrentDestination ? 0 : i;
            }
            if (i % 5 < 4)
                ImGui.SameLine();
        }

        return dest;
    }

    private static void DrawPlayer(Vector2 center, Angle rotation, nint texHandle)
    {
        var cos = -rotation.Cos();
        var sin = rotation.Sin();
        ImGui.GetWindowDrawList().AddImageQuad(
            texHandle,
            center + Rotate(new(-32, -37.5f), cos, sin),
            center + Rotate(new(32, -37.5f), cos, sin),
            center + Rotate(new(32, 26.5f), cos, sin),
            center + Rotate(new(-32, 26.5f), cos, sin),
            new Vector2(0.0000f, 0.4571f),
            new Vector2(0.2424f, 0.4571f),
            new Vector2(0.2424f, 0.6857f),
            new Vector2(0.0000f, 0.6857f)
        );
    }

    private static Vector2 Rotate(Vector2 v, float cosA, float sinA) => new(v.X * cosA - v.Y * sinA, v.X * sinA + v.Y * cosA);
}
