using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

public record class Minimap(DeepDungeonState State, Angle PlayerRotation, int CurrentDestination, int PlayerSlot)
{
    private readonly AutoDDConfig _config = Service.Config.Get<AutoDDConfig>();

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

    float Adjust(float input) => input * ImGuiHelpers.GlobalScale * _config.MinimapScale;

    public struct Size
    {
        public Vector2 Room;
        public Vector2 RoomHighlight;
        public Vector2 HomeIcon;
        public Vector2 PointOfInterest;
        public Vector2 PoIPadding;
        public Vector2 Treasure;
        public Vector2 TreasurePadding;
        public float TreasureAdjustX;
        public (float Width, float Front, float Back) Player;
    }

    public Size Sizes => new()
    {
        Room = new(Adjust(55)),
        RoomHighlight = new(Adjust(40)),
        HomeIcon = new(Adjust(40)),
        PointOfInterest = new(Adjust(20)),
        PoIPadding = new(Adjust(17.5f), Adjust(27.5f)),
        Treasure = new(Adjust(30)),
        TreasurePadding = new(Adjust(1.25f)),
        TreasureAdjustX = Adjust(11.25f),
        Player = (Adjust(20), Adjust(23.4375f), Adjust(16.5625f))
    };

    /// <summary>
    ///
    /// </summary>
    /// <returns>Integer index of the room the user clicked on.</returns>
    public int Draw()
    {
        var dest = -1;

        var chests = new RoomChest[25];
        foreach (var c in State.Chests)
            if (c.Room is > 0)
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

        var sizes = Sizes;

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
                xoff += 0.2292f / sizes.Room.X;
                yoff += 0.2292f / sizes.Room.X;
                xoffend -= 0.2292f / sizes.Room.X;
                yoffend -= 0.2292f / sizes.Room.X;
            }

            ImGui.SetCursorPos(pos);
            ImGui.Image(roomsTex.Handle, sizes.Room - new Vector2(highlight ? 2 : 0), new Vector2(xoff, yoff), new Vector2(xoffend, yoffend), tile > 0 ? new(1f) : new(0.6f), highlight ? new(0, 0.6f, 0, 1) : default);

            if (i == playerCell)
            {
                isValidDestination = false;
                ImGui.SetCursorPos(pos + (sizes.Room - sizes.RoomHighlight) * 0.5f);
                ImGui.Image(mapTex.Handle, sizes.RoomHighlight, new Vector2(0.2424f, 0.4571f), new Vector2(0.4848f, 0.6857f));
            }

            if (State.Rooms[i].HasFlag(RoomFlags.Home))
            {
                ImGui.SetCursorPos(pos + (sizes.Room - sizes.HomeIcon) * 0.5f);
                ImGui.Image(mapTex.Handle, sizes.HomeIcon, new Vector2(0.4848f, 0.4571f), new Vector2(0.7272f, 0.6657f));
            }

            if (State.Rooms[i].HasFlag(RoomFlags.Passage))
            {
                ImGui.SetCursorPos(pos + sizes.PoIPadding);
                ImGui.Image(passageTex.Handle, sizes.PointOfInterest);
            }

            if (State.Rooms[i].HasFlag(RoomFlags.Return))
            {
                ImGui.SetCursorPos(pos + sizes.PoIPadding);
                ImGui.Image(returnTex.Handle, sizes.PointOfInterest);
            }

            if (((ushort)State.Rooms[i] & 0x100) != 0)
            {
                ImGui.SetCursorPos(pos + sizes.PoIPadding);
                ImGui.Image(votiveTex.Handle, sizes.PointOfInterest);
            }

            if (chests[i].HasFlag(RoomChest.Bronze))
            {
                ImGui.SetCursorPos(pos + sizes.TreasurePadding);
                ImGui.Image(bronzeTex.Handle, sizes.Treasure);
            }

            if (chests[i].HasFlag(RoomChest.Silver))
            {
                ImGui.SetCursorPos(pos + sizes.TreasurePadding + new Vector2(sizes.TreasureAdjustX, 0));
                ImGui.Image(silverTex.Handle, sizes.Treasure);
            }

            if (chests[i].HasFlag(RoomChest.Gold))
            {
                ImGui.SetCursorPos(pos + sizes.TreasurePadding + new Vector2(sizes.TreasureAdjustX * 2, 0));
                ImGui.Image(goldTex.Handle, sizes.Treasure);
            }

            if (i == playerCell)
            {
                ImGui.SetCursorPos(pos + Sizes.Room * 0.5f);
                DrawPlayer(ImGui.GetCursorScreenPos(), PlayerRotation, mapTex.Handle, sizes);
            }

            ImGui.SetCursorPos(pos);
            ImGui.Dummy(Sizes.Room);
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

    private static void DrawPlayer(Vector2 center, Angle rotation, ImTextureID texHandle, in Size sizes)
    {
        var cos = -rotation.Cos();
        var sin = rotation.Sin();
        var (width, lenFront, lenBack) = sizes.Player;
        ImGui.GetWindowDrawList().AddImageQuad(
            texHandle,
            center + Rotate(new(-width, -lenFront), cos, sin),
            center + Rotate(new(width, -lenFront), cos, sin),
            center + Rotate(new(width, lenBack), cos, sin),
            center + Rotate(new(-width, lenBack), cos, sin),
            new Vector2(0.0000f, 0.4571f),
            new Vector2(0.2424f, 0.4571f),
            new Vector2(0.2424f, 0.6857f),
            new Vector2(0.0000f, 0.6857f)
        );
    }

    private static Vector2 Rotate(Vector2 v, float cosA, float sinA) => new(v.X * cosA - v.Y * sinA, v.X * sinA + v.Y * cosA);
}
