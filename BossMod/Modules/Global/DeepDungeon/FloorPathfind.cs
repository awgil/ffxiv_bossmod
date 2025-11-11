using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

public enum Direction
{
    North,
    South,
    East,
    West
}

// neat feature of deep dungeons - there is only one path from any room to any other room (no loops) and the grid is so small that brute forcing is basically free!
internal class FloorPathfind(ReadOnlySpan<RoomFlags> Map)
{
    public readonly RoomFlags[] Map = Map.ToArray();

    private readonly bool[] Explored = new bool[25];

    private readonly Queue<List<int>> Queue = new();

    public List<int> Pathfind(int startRoom, int destRoom)
    {
        if (startRoom == destRoom)
            return [];

        Explored[startRoom] = true;
        Queue.Enqueue([startRoom]);
        while (Queue.TryDequeue(out var v))
        {
            if (v[^1] == destRoom)
            {
                v.RemoveAt(0);
                return v;
            }
            foreach (var w in Edges(v[^1]))
            {
                if (!Explored[w])
                {
                    Explored[w] = true;
                    Queue.Enqueue([.. v, w]);
                }
            }
        }

        return [];
    }

    private IEnumerable<int> Edges(int roomIndex)
    {
        var md = Map[roomIndex];
        if (md.HasFlag(RoomFlags.ConnectionN))
            yield return roomIndex - 5;
        if (md.HasFlag(RoomFlags.ConnectionS))
            yield return roomIndex + 5;
        if (md.HasFlag(RoomFlags.ConnectionW))
            yield return roomIndex - 1;
        if (md.HasFlag(RoomFlags.ConnectionE))
            yield return roomIndex + 1;
    }
}

abstract partial class AutoClear : ZoneModule
{
    private void HandleFloorPathfind(Actor player, AIHints hints)
    {
        var slot = Array.FindIndex(Palace.Party, p => p.EntityId == player.InstanceID);
        if (slot < 0)
            return;
        var reportedRoom = Palace.Party[slot].Room;
        if (DesiredRoom == reportedRoom || DesiredRoom == 0)
        {
            DesiredRoom = 0;
            return;
        }

        var path = new FloorPathfind(Palace.Rooms).Pathfind(PlayerRoom, DesiredRoom);

        var next = path.Count > 0 ? path[0] : DesiredRoom;
        var destBox = _floorRects[Palace.Progress.Tileset][next];

        var destToSrc = ToCardinal(PlayerRoomBox.Pos - destBox.Pos).Sign();
        var (destLength, destWidth) = destToSrc.X != 0 ? destBox.Size.OrthoR().Abs() : destBox.Size.Abs();

        destLength *= 0.8f;

        hints.GoalZones.Add(p =>
        {
            var pdir = p - destBox.Pos;
            var pdot = pdir.Dot(destToSrc);
            var pdotNormal = MathF.Abs(pdir.Dot(destToSrc.OrthoL()));
            var distToRect = pdotNormal - destWidth;

            // behind target room, we aren't really likely to ever hit this case
            if (pdot < -destLength)
                return 0;

            // inside target room
            if (distToRect < 0 && MathF.Abs(pdot) <= destLength)
                return 10;

            // create a goal cone extending from room edge, with gradually decreasing priority - makes corners more consistent
            var distToRoomEdge = pdot - destLength;
            if (distToRect < distToRoomEdge * 0.2f)
            {
                var weight = 120 - Math.Clamp(distToRoomEdge, 0, 120);
                return MathF.Floor(weight / 12f);
            }

            // outside cone
            return 0;
        });
    }
}
