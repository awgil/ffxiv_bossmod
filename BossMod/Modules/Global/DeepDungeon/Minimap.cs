using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

public record class Minimap(DeepDungeonState State, Angle PlayerRotation, int CurrentDestination, int PlayerSlot)
{
    enum IconID : uint
    {
        ReturnClosed = 60905,
        ReturnOpen = 60906,
        PassageClosed = 60907,
        PassageOpen = 60908,
        ChestBronze = 60911,
        ChestSilver = 60912,
        ChestGold = 60913,
        Votive = 63988
    }

    [Flags]
    enum RoomChest
    {
        None = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 4
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Integer index of the room the user clicked on.</returns>
    public int Draw()
    {
        var dest = -1;

        var chests = new RoomChest[25];
        foreach (var c in State.Chests)
            if (c.Room > 0)
                chests[c.Room] |= (RoomChest)(1 << (c.Type - 1));

        var playerCell = State.Party[PlayerSlot].Room;

        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 0f));

        var roomsTex = Service.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_Rooms_hr1.tex").GetWrapOrEmpty();
        var mapTex = Service.Texture.GetFromGame("ui/uld/DeepDungeonNaviMap_hr1.tex").GetWrapOrEmpty();
        var passageTex = Service.Texture.GetFromGameIcon(new((uint)(State.PassageActive ? IconID.PassageOpen : IconID.PassageClosed))).GetWrapOrEmpty();
        var returnTex = Service.Texture.GetFromGameIcon(new((uint)(State.ReturnActive ? IconID.ReturnOpen : IconID.ReturnClosed))).GetWrapOrEmpty();
        var votiveTex = Service.Texture.GetFromGameIcon(new((uint)IconID.Votive)).GetWrapOrEmpty();
        var bronzeTex = Service.Texture.GetFromGameIcon(new((uint)IconID.ChestBronze)).GetWrapOrEmpty();
        var silverTex = Service.Texture.GetFromGameIcon(new((uint)IconID.ChestSilver)).GetWrapOrEmpty();
        var goldTex = Service.Texture.GetFromGameIcon(new((uint)IconID.ChestGold)).GetWrapOrEmpty();

        for (var i = 0; i < 25; i++)
        {
            var highlight = CurrentDestination > 0 && CurrentDestination == i;

            var isValidDestination = State.Rooms[i] > 0;

            using var _1 = ImRaii.PushId($"room{i}");

            var pos = ImGui.GetCursorPos();
            var tile = (byte)State.Rooms[i] & 0xF;
            var row = tile / 4;
            var col = tile % 4;

            var xoff = 0.0104f + col * 0.25f;
            var yoff = 0.0104f + row * 0.25f;
            var xoffend = xoff + 0.2292f;
            var yoffend = yoff + 0.2292f;

            // trim off 1px from each edge to account for extra space from highlight square
            // TODO there is probably a sensible primitive for this somewhere
            if (highlight)
            {
                xoff += 0.2292f / 88f;
                yoff += 0.2292f / 88f;
                xoffend -= 0.2292f / 88f;
                yoffend -= 0.2292f / 88f;
            }

            ImGui.SetCursorPos(pos);
            ImGui.Image(roomsTex.Handle, highlight ? new(86) : new(88), new Vector2(xoff, yoff), new Vector2(xoffend, yoffend), tile > 0 ? new(1f) : new(0.6f), highlight ? new(0, 0.6f, 0, 1) : default);

            if (i == playerCell)
            {
                isValidDestination = false;
                ImGui.SetCursorPos(pos + new Vector2(12, 12));
                ImGui.Image(mapTex.Handle, new Vector2(64, 64), new Vector2(0.2424f, 0.4571f), new Vector2(0.4848f, 0.6857f));
            }

            if (State.Rooms[i].HasFlag(RoomFlags.Passage))
            {
                ImGui.SetCursorPos(pos + new Vector2(28, 44));
                ImGui.Image(passageTex.Handle, new Vector2(32, 32));
            }

            if (State.Rooms[i].HasFlag(RoomFlags.Return))
            {
                ImGui.SetCursorPos(pos + new Vector2(28, 44));
                ImGui.Image(returnTex.Handle, new Vector2(32, 32));
            }

            if (((ushort)State.Rooms[i] & 0x100) != 0)
            {
                ImGui.SetCursorPos(pos + new Vector2(28, 44));
                ImGui.Image(votiveTex.Handle, new Vector2(32, 32));
            }

            if (chests[i].HasFlag(RoomChest.Bronze))
            {
                ImGui.SetCursorPos(pos + new Vector2(2, 2));
                ImGui.Image(bronzeTex.Handle, new Vector2(48, 48));
            }

            if (chests[i].HasFlag(RoomChest.Silver))
            {
                ImGui.SetCursorPos(pos + new Vector2(20, 2));
                ImGui.Image(silverTex.Handle, new Vector2(48, 48));
            }

            if (chests[i].HasFlag(RoomChest.Gold))
            {
                ImGui.SetCursorPos(pos + new Vector2(38, 2));
                ImGui.Image(goldTex.Handle, new Vector2(48, 48));
            }

            if (i == playerCell)
            {
                ImGui.SetCursorPos(pos + new Vector2(44, 44));
                DrawPlayer(ImGui.GetCursorScreenPos(), PlayerRotation, mapTex.Handle);
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

    private static void DrawPlayer(Vector2 center, Angle rotation, ImTextureID texHandle)
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
