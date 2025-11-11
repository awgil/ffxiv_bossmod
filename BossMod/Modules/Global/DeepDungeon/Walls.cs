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

    private record struct RoomBox(int Room, Vector3 Position, Vector3 Scale, Vector3 Rotation)
    {
        public readonly WPos Pos => new(Position.XZ());
        public readonly WDir Size => new(Scale.XZ());
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
            var roomRanges = layer.InstanceObjects.Where(o => o.AssetType == Lumina.Data.Parsing.Layer.LayerEntryType.EventRange).Take(21).ToList();

            // boss room layer has one object
            if (roomRanges.Count < 21)
                continue;

            var boxesOrdered = roomRanges.Select(r => new RoomBox(0, r.Transform.Translation.ToSystem(), r.Transform.Scale.ToSystem(), r.Transform.Rotation.ToSystem())).ToList();

            // level data boxes are ordered by column, room data is ordered by row
            _floorRects.Add([
                default,
                boxesOrdered[ 3] with { Room = 1 },
                boxesOrdered[ 8] with { Room = 2 },
                boxesOrdered[13] with { Room = 3 },
                default,

                boxesOrdered[ 0] with { Room = 5 },
                boxesOrdered[ 4] with { Room = 6 },
                boxesOrdered[ 9] with { Room = 7 },
                boxesOrdered[14] with { Room = 8 },
                boxesOrdered[18] with { Room = 9 },

                boxesOrdered[ 1] with { Room = 10 },
                boxesOrdered[ 5] with { Room = 11 },
                boxesOrdered[10] with { Room = 12 },
                boxesOrdered[15] with { Room = 13 },
                boxesOrdered[19] with { Room = 14 },

                boxesOrdered[ 2] with { Room = 15 },
                boxesOrdered[ 6] with { Room = 16 },
                boxesOrdered[11] with { Room = 17 },
                boxesOrdered[16] with { Room = 18 },
                boxesOrdered[20] with { Room = 19 },

                default,
                boxesOrdered[ 7] with { Room = 21 },
                boxesOrdered[12] with { Room = 22 },
                boxesOrdered[17] with { Room = 23 },
                default,
            ]);
        }
    }

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
}
