using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Lumina.Data.Files;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

abstract partial class AutoClear : ZoneModule
{
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
                RoomCenters.Add(roomdata.Center.Position);
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

    record struct FloorRect(string Tileset, WPos Position, WDir Scale, Vector3 Rotation);

    private readonly List<FloorRect> _floorRects = [];
    private readonly List<(int, int)> _connections = [];

    private void LoadGeometry()
    {
        _floorRects.Clear();
        _connections.Clear();

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
            if (layer.Name.Contains("LVD_Boss_", StringComparison.InvariantCultureIgnoreCase))
                continue;

            foreach (var obj in layer.InstanceObjects)
            {
                if (obj.AssetType == Lumina.Data.Parsing.Layer.LayerEntryType.EventRange)
                {
                    var trans = obj.Transform;
                    var pos = new WPos(trans.Translation.X, trans.Translation.Z);
                    var scale = new WDir(trans.Scale.X, trans.Scale.Z);
                    _floorRects.Add(new(layer.Name, pos, scale, new(trans.Rotation.X, trans.Rotation.Y, trans.Rotation.Z)));
                }
            }
        }

        for (var i = 0; i < _floorRects.Count; i++)
        {
            var rect = _floorRects[i];
            var x1 = rect.Position.X - rect.Scale.X;
            var x2 = rect.Position.X + rect.Scale.X;
            var z1 = rect.Position.Z - rect.Scale.Z;
            var z2 = rect.Position.Z + rect.Scale.Z;

            for (var j = i + 1; j < _floorRects.Count; j++)
            {
                var rect2 = _floorRects[j];
                var xj1 = rect2.Position.X - rect2.Scale.X;
                var xj2 = rect2.Position.X + rect2.Scale.X;
                var zj1 = rect2.Position.Z - rect2.Scale.Z;
                var zj2 = rect2.Position.Z + rect2.Scale.Z;

                if (x1 < xj2 && x2 > xj1 && z1 < zj2 && z2 > zj1)
                    _connections.Add((i, j));
            }
        }
    }

    private void DrawBoxes(Actor? player)
    {
        if (player == null)
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

        var hovered = _floorRects.FindIndex(r => player.Position.InRect(r.Position, 0.Degrees(), r.Scale.Z, r.Scale.Z, r.Scale.X));

        var connected = new HashSet<int>();
        if (hovered >= 0)
        {
            foreach (var c in _connections)
            {
                if (c.Item2 == hovered)
                    connected.Add(c.Item1);
                if (c.Item1 == hovered)
                    connected.Add(c.Item2);
            }
        }

        for (var i = 0; i < _floorRects.Count; i++)
        {
            var r = _floorRects[i];

            if (hovered == i)
                ImGui.GetWindowDrawList().AddRectFilled(worldToWindow(r.Position + r.Scale), worldToWindow(r.Position - r.Scale), ArenaColor.Object);
            else if (connected.Contains(i))
                ImGui.GetWindowDrawList().AddRectFilled(worldToWindow(r.Position + r.Scale), worldToWindow(r.Position - r.Scale), 0x80FFFFFF);
            else
                ImGui.GetWindowDrawList().AddRect(worldToWindow(r.Position + r.Scale), worldToWindow(r.Position - r.Scale), ArenaColor.Border);

            var label = $"{r.Tileset} {i}";
            var ts = ImGui.CalcTextSize(label);
            ImGui.GetWindowDrawList().AddText(worldToWindow(r.Position) - ts * 0.5f, 0xFFFFFFFF, label);
        }

        if (player is { } p)
            ImGui.GetWindowDrawList().AddCircleFilled(worldToWindow(p.Position), 5 * ImGuiHelpers.GlobalScale, ArenaColor.Safe);

        ImGui.GetWindowDrawList().AddCircle(worldToWindow(default), 10, 0xFFFF0000);

        ImGui.Text($"{windowToWorld(mousePos)}");
        ImGui.Text($"connected: {_connections.Count}");
    }
}
