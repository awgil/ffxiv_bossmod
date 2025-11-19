using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Lumina.Data.Files;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace BossMod.Global.DeepDungeon;

abstract partial class AutoClear : ZoneModule
{
    private readonly Dictionary<string, Floor<Wall>> LoadedFloors;
    private readonly List<(Wall Wall, bool Rotated)> Walls = [];

    private void LoadWalls()
    {
        Service.Log($"loading walls for current floor...");
        Walls.Clear();
        var floorset = Palace.Floor / 10;
        var key = $"{(int)Palace.DungeonId}.{floorset + 1}";
        if (!LoadedFloors.TryGetValue(key, out var floor))
        {
            Service.Log($"unable to load floorset {key}");
            return;
        }
        Tileset<Wall> tileset;
        switch (Palace.Progress.Tileset)
        {
            case 0:
                tileset = floor.RoomsA;
                break;
            case 1:
                tileset = floor.RoomsB;
                break;
            case 2:
                Service.Log($"hall of fallacies - nothing to do");
                return;
            default:
                Service.Log($"unrecognized tileset number {Palace.Progress.Tileset}");
                return;
        }
        foreach (var (room, i) in Palace.Rooms.Select((m, i) => (m, i)))
        {
            if (room > 0)
            {
                var roomdata = tileset[i];
                if (roomdata.North != default && !room.HasFlag(RoomFlags.ConnectionN))
                    Walls.Add((roomdata.North, false));
                if (roomdata.South != default && !room.HasFlag(RoomFlags.ConnectionS))
                    Walls.Add((roomdata.South, false));
                if (roomdata.East != default && !room.HasFlag(RoomFlags.ConnectionE))
                    Walls.Add((roomdata.East, true));
                if (roomdata.West != default && !room.HasFlag(RoomFlags.ConnectionW))
                    Walls.Add((roomdata.West, true));
            }
        }
    }

    private record struct RoomBox(int Room, Vector3 Position, Vector3 Scale, Vector3 Rotation)
    {
        public readonly WPos Pos => new(Position.XZ());
        public readonly WDir Size => new(Scale.XZ());

        public readonly bool Contains(WPos p) => p.InRect(Pos, default(Angle), Size.Z, Size.Z, Size.X);
        public readonly bool Contains(Vector3 v) => Contains(new WPos(v.X, v.Z));
    }

    private readonly List<List<RoomBox>> _floorRects = [];

    private void LoadGeometry()
    {
        _floorRects.Clear();

        var tt = Service.LuminaRow<Lumina.Excel.Sheets.TerritoryType>(World.CurrentZone);
        if (tt == null)
        {
            Service.Log($"unable to load level data for zone {World.CurrentZone}");
            return;
        }

        var bg = tt.Value.Bg.ToString();
        var lgb = $"bg/{bg[..bg.LastIndexOf('/')]}/planmap.lgb";

        var levelData = Service.LuminaGameData?.GetFile<LgbFile>(lgb);
        if (levelData == null)
        {
            Service.Log($"unable to load level data for zone {World.CurrentZone}");
            return;
        }

        foreach (var layer in levelData.Layers)
        {
            var roomRanges = layer.InstanceObjects.Where(o => o.AssetType == Lumina.Data.Parsing.Layer.LayerEntryType.EventRange && ((EventRangeInstanceObject)o.Object).ParentData.Priority is > 100 and < 126).ToList();

            if (roomRanges.Count == 0)
                continue;

            var roomsPrio = roomRanges.GroupBy(r => ((EventRangeInstanceObject)r.Object).ParentData.Priority);

            _floorRects.Add([.. Enumerable.Repeat(default(RoomBox), 25)]);
            var fr = _floorRects[^1];

            foreach (var grouping in roomsPrio)
            {
                var prio = grouping.Key;
                // assume largest box is the room collider and any others are hallways
                var box = grouping.MaxBy(r => r.Transform.Scale.X * r.Transform.Scale.Z);

                var roomIndex = prio - 100;
                if (roomIndex is < 0 or > 24)
                    throw new ArgumentException($"got unrecognized priority {prio} for collision box");

                fr[roomIndex] = new(roomIndex, box.Transform.Translation.ToSystem(), box.Transform.Scale.ToSystem(), box.Transform.Rotation.ToSystem());
            }
        }

        // TODO: reverse more layout stuff so we can use a less stupid solution
        // tileset 0 is almost always SW and tileset 1 is NE but palace swaps them for certain floors
        var layerRow = Service.LuminaSheet<Lumina.Excel.Sheets.DeepDungeonLayer>()?.FirstOrNull(l => l.DeepDungeon.RowId == (uint)Palace.DungeonId && l.FloorSet == (byte)(Palace.Floor / 10));
        if (layerRow?.RoomA.RowId > layerRow?.RoomB.RowId)
            _floorRects.Reverse();
    }

    //private static bool IsRoomRange(Eve)

    private static WDir ToCardinal(WDir x)
    {
        var abs = x.Abs();
        if (abs.X < abs.Z)
            x.X = 0;
        else
            x.Z = 0;
        return x;
    }

    private float DistanceBox2D(WPos p, Vector2 rectMin, Vector2 rectMax)
    {
        var dx = MathF.Max(MathF.Max(rectMin.X - p.X, 0), p.X - rectMax.X);
        var dy = MathF.Max(MathF.Max(rectMin.Y - p.Z, 0), p.Z - rectMax.Y);
        return dx * dx + dy * dy;
    }

    private float DistanceBox2D(WPos p, RoomBox rect) => DistanceBox2D(p, (rect.Position - rect.Scale).XZ(), (rect.Position + rect.Scale).XZ());

    private (RoomBox Room, int Index) FindClosestRoom(WPos p) => _floorRects[Palace.Progress.Tileset].Select((box, i) => (box, i)).MinBy(r => DistanceBox2D(p, r.box));

    private bool IsRoomAdjacent(int i, int j)
    {
        if (i is >= 25 or < 0)
            return false;
        var md = Palace.Rooms[i];
        if (md.HasFlag(RoomFlags.ConnectionN) && i - 5 == j)
            return true;
        if (md.HasFlag(RoomFlags.ConnectionS) && i + 5 == j)
            return true;
        if (md.HasFlag(RoomFlags.ConnectionW) && i - 1 == j)
            return true;
        if (md.HasFlag(RoomFlags.ConnectionE) && i + 1 == j)
            return true;
        return false;
    }

    private bool IsInThisRoomOrAdjacent(WPos p)
    {
        var (_, r) = FindClosestRoom(p);
        return PlayerRoom == r || IsRoomAdjacent(r, PlayerRoom);
    }

    private bool IsInThisRoomOrAdjacent(Actor a) => IsInThisRoomOrAdjacent(a.Position);

    private bool _drawGeometry;

    private void DrawBoxes(Actor? player)
    {
        ImGui.Checkbox("Draw geometry", ref _drawGeometry);
        if (player == null || !_drawGeometry)
            return;

        var windowPos = ImGui.GetWindowPos();
        var cursorPos = ImGui.GetCursorPos();

        var dims = ImGui.GetContentRegionAvail();

        ImGui.Dummy(dims);

        var worldCenter = dims * -0.5f + player.Position.ToVec2();

        Vector2 worldToWindow(WPos p) => windowPos + cursorPos + p.ToVec2() - worldCenter;
        WPos windowToWorld(Vector2 v) => new(v + worldCenter - cursorPos - windowPos);

        var mousePos = ImGui.GetMousePos();
        var mouseWorld = windowToWorld(mousePos);

        for (var t = 0; t < _floorRects.Count; t++)
        {
            var hovered = _floorRects[t].FindIndex(r => player.Position.InRect(r.Pos, default(Angle), r.Scale.Z, r.Scale.Z, r.Scale.X));

            var labelTs = false;
            for (var i = 0; i < _floorRects[t].Count; i++)
            {
                var r = _floorRects[t][i];

                if (r.Scale == default)
                    continue;

                if (hovered == i)
                    ImGui.GetWindowDrawList().AddRectFilled(worldToWindow(r.Pos + r.Size), worldToWindow(r.Pos - r.Size), ArenaColor.Object);
                else
                    ImGui.GetWindowDrawList().AddRect(worldToWindow(r.Pos + r.Size), worldToWindow(r.Pos - r.Size), ArenaColor.Border);

                var label = i.ToString();

                var ts2 = ImGui.CalcTextSize(label);
                ImGui.GetWindowDrawList().AddText(worldToWindow(r.Pos) - ts2 * 0.5f, 0xFFFFFFFF, label);

                if (!labelTs)
                {
                    var xmin = _floorRects[t].Where(r => r.Scale != default).Min(r => r.Position.X);
                    var zmin = _floorRects[t].Where(r => r.Scale != default).Min(r => r.Position.Z);

                    ImGui.GetWindowDrawList().AddText(worldToWindow(new WPos(xmin, zmin) - new WDir(40, 40)), 0xFFFFFFFF, $"Tileset {t}");
                    labelTs = true;
                }
            }

            if (player is { } p)
                ImGui.GetWindowDrawList().AddCircleFilled(worldToWindow(p.Position), 5 * ImGuiHelpers.GlobalScale, ArenaColor.Safe);
        }

        ImGui.GetWindowDrawList().AddCircle(worldToWindow(default), 10, 0xFFFF0000);

        ImGui.Text($"{windowToWorld(mousePos)}");
    }
}
