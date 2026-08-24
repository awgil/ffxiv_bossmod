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

        var destRoom = _floorRects[Palace.Progress.Tileset][DesiredRoom];
        if (destRoom.Contains(player.Position) || DesiredRoom == 0)
        {
            DesiredRoom = 0;
            return;
        }

        // player is in a hallway/bridge near destination room, move closer
        if (PlayerRoom == DesiredRoom)
        {
            hints.GoalZones.Add(p => p.InRect(destRoom.Pos, default(Angle), destRoom.Scale.Z, destRoom.Scale.Z, destRoom.Scale.X) ? 10 : 0);
            return;
        }

        var path = new FloorPathfind(Palace.Rooms).Pathfind(PlayerRoom, DesiredRoom);
        if (path.Count == 0)
        {
            Service.Log($"no path from {PlayerRoom} to {DesiredRoom}, doing nothing");
            return;
        }

        var destBox = _floorRects[Palace.Progress.Tileset][path[0]];
        var srcBox = PlayerRoomBox;

        var srcToDest = destBox.Pos - srcBox.Pos;
        var srcToDestLen = srcToDest.Length();
        var playerPos = player.Position;
        var srcToPlayer = playerPos - srcBox.Pos;
        var playerProgress = srcToDest.Dot(srcToPlayer) / srcToDestLen;
        var srcToDestCardinal = ToCardinal(srcToDest).Sign();

        var rectWidth = 10f;

        if (Service.IsDev)
        {
            var a = srcBox.Position with { Y = player.PosRot.Y } + (srcToDest.Normalized().OrthoL() * rectWidth).ToVec3();
            var b = srcBox.Position with { Y = player.PosRot.Y } + (srcToDest.Normalized().OrthoR() * rectWidth).ToVec3();
            var c = destBox.Position with { Y = player.PosRot.Y } + (srcToDest.Normalized().OrthoR() * rectWidth).ToVec3();
            var d = destBox.Position with { Y = player.PosRot.Y } + (srcToDest.Normalized().OrthoL() * rectWidth).ToVec3();
            Camera.Instance?.DrawWorldLine(a, b, 0xFFFF00FF);
            Camera.Instance?.DrawWorldLine(b, c, 0xFFFF00FF);
            Camera.Instance?.DrawWorldLine(c, d, 0xFFFF00FF);
            Camera.Instance?.DrawWorldLine(d, a, 0xFFFF00FF);
        }

        hints.GoalZones.Add(p =>
        {
            // align the near side of the goal rect to the grid since we get annoying jittering otherwise
            if (p.InRect(playerPos, srcToDestCardinal, 10, 100, 100))
                return 0;

            if (p.InRect(srcBox.Pos, srcToDest.ToAngle(), srcToDestLen, srcToDestLen, rectWidth))
                return 1;
            return 0;
        });
    }
}
