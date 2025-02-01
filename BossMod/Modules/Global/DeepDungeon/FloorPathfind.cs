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
