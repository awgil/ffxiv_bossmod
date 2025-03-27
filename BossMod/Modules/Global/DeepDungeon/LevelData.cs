namespace BossMod.Global.DeepDungeon;

[Serializable]
public record class Floor<T>(
    uint DungeonId,
    uint Floorset,
    Tileset<T> RoomsA,
    Tileset<T> RoomsB
)
{
    public Floor<M> Map<M>(Func<T, M> Mapping) => new(DungeonId, Floorset, RoomsA.Map(Mapping), RoomsB.Map(Mapping));
}

[Serializable]
public record class Tileset<T>(List<RoomData<T>> Rooms)
{
    public Tileset<M> Map<M>(Func<T, M> Mapping) => new([.. Rooms.Select(m => m.Map(Mapping))]);

    public RoomData<T> this[int index] => Rooms[index];

    public override string ToString() => $"Tileset {{ Rooms = [{string.Join(", ", Rooms)}] }}";
}

[Serializable]
public record class RoomData<T>(
    T Center,
    T North,
    T South,
    T West,
    T East
)
{
    public RoomData<M> Map<M>(Func<T, M> F) => new(F(Center), F(North), F(South), F(West), F(East));
}

[Serializable]
public record struct Wall(WPos Position, float Depth);
