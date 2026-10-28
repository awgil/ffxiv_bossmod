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
    enum PathfindReason
    {
        Nothing,
        ClosedPassage,
        Everything,
        Hunting,
        OpenPassage,
        Hoard,
        Candelabra,
        Manual
    }

    static string ReasonReadable(PathfindReason r) => r switch
    {
        PathfindReason.ClosedPassage => "revealing passage",
        PathfindReason.Everything => "revealing the map",
        PathfindReason.Hunting => "hunting mobs for progress",
        PathfindReason.OpenPassage => "heading for passage",
        PathfindReason.Hoard => "heading for hoard",
        PathfindReason.Candelabra => "heading for candelabra",
        PathfindReason.Manual => "navigating manually",
        _ => ""
    };

    private RoomBox PlayerRoomBox;
    private int PlayerRoom;

    private (int Index, PathfindReason Reason) DesiredRoom;

    private void PickARoom(Actor player, AIHints hints)
    {
        if (DesiredRoom.Reason == PathfindReason.Hunting && (Palace.PassageActive || player.InCombat))
            DesiredRoom = default;

        if (DesiredRoom.Reason == PathfindReason.ClosedPassage && Palace.Rooms.Any(r => r.HasFlag(RoomFlags.Passage) && r.HasFlag(RoomFlags.Revealed)))
            DesiredRoom = default;

        if (DesiredRoom.Index > 0)
            return;

        if (Palace.PassageActive && !player.InCombat)
            DesiredRoom = (Array.FindIndex(Palace.Rooms, d => d.HasFlag(RoomFlags.Passage)), PathfindReason.OpenPassage);

        if (_config.FullClear)
        {
            var unexplored = Array.FindIndex(Palace.Rooms, r => r != RoomFlags.None && !r.HasFlag(RoomFlags.Revealed));
            if (unexplored > 0)
                DesiredRoom = (unexplored, PathfindReason.Everything);
        }

        if (!Palace.PassageActive && !player.InCombat && hints.PotentialTargets.MinBy(t => (t.Actor.Position - player.Position).LengthSq()) is { } closestMob)
            DesiredRoom = (FindClosestRoom(closestMob.Actor.Position).Index, PathfindReason.Hunting);

        var unrevealedPassage = Array.FindIndex(Palace.Rooms, d => d.HasFlag(RoomFlags.Passage) && !d.HasFlag(RoomFlags.Revealed));
        if (unrevealedPassage > 0)
            DesiredRoom = (unrevealedPassage, PathfindReason.ClosedPassage);

        // TODO: figure out if content director has a flag for the candelabra disappearing
        var unrevealedDevo = Array.FindIndex(Palace.Rooms, d => d.HasFlag((RoomFlags)0x100) && !d.HasFlag(RoomFlags.Revealed));
        if (unrevealedDevo > 0)
            DesiredRoom = (unrevealedDevo, PathfindReason.Candelabra);

        if (Palace.Progress.HoardCurrentFloor && World.Actors.FirstOrDefault(a => a.OID == (uint)OID.BandedCofferIndicator) is { } hoard)
            DesiredRoom = (FindClosestRoom(hoard.Position).Index, PathfindReason.Hoard);
    }

    private void HandleFloorPathfind(Actor player, AIHints hints)
    {
        var slot = Array.FindIndex(Palace.Party, p => p.EntityId == player.InstanceID);
        if (slot < 0)
            return;

        var destRoom = _floorRects[Palace.Progress.Tileset][DesiredRoom.Index];
        if (destRoom.Contains(player.Position) || DesiredRoom.Index == 0)
        {
            DesiredRoom = default;
            return;
        }

        var goalWeight = player.InCombat ? 1 : 0.01f;

        // player is in a hallway/bridge near destination room, move closer
        if (PlayerRoom == DesiredRoom.Index)
        {
            hints.GoalZones.Add(p => p.InRect(destRoom.Pos, default(Angle), destRoom.Scale.Z, destRoom.Scale.Z, destRoom.Scale.X) ? goalWeight : 0);
            return;
        }

        var path = new FloorPathfind(Palace.Rooms).Pathfind(PlayerRoom, DesiredRoom.Index);
        if (path.Count == 0)
        {
            Service.Log($"no path from {PlayerRoom} to {DesiredRoom.Index}, doing nothing");
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

        hints.GoalZones.Add(p =>
        {
            // align the near side of the goal rect to the grid since we get annoying jittering otherwise
            if (p.InRect(playerPos, srcToDestCardinal, 10, 100, 100))
                return 0;

            if (p.InRect(srcBox.Pos, srcToDest.ToAngle(), srcToDestLen, srcToDestLen, rectWidth))
                return goalWeight;
            return 0;
        });
    }
}
