namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class Tiles : BossComponent
{
    public BitMask Mask;

    public bool ShouldDraw;

    public bool this[int index] => Mask[index];

    public Tiles(BossModule module) : base(module)
    {
#if DEBUG
        WorldState.Network.ServerIPCReceived.Subscribe(OnIPCReceived);
    }

    private unsafe void OnIPCReceived(NetworkState.OpServerIPC packet)
    {
        switch (packet.Packet.ID)
        {
            case Network.ServerIPC.PacketID.EnvControl4:
                fixed (byte* payload = packet.Packet.Payload)
                    for (var i = 0; i < *payload; i++)
                        ApplyMapEffect(payload[i + 18], *(ushort*)(payload + 2 * i + 10), *(ushort*)(payload + 2 * i + 2));
                break;
            case Network.ServerIPC.PacketID.EnvControl8:
                fixed (byte* payload = packet.Packet.Payload)
                    for (var i = 0; i < *payload; i++)
                        ApplyMapEffect(payload[i + 34], *(ushort*)(payload + 2 * i + 18), *(ushort*)(payload + 2 * i + 2));
                break;
            case Network.ServerIPC.PacketID.EnvControl12:
                fixed (byte* payload = packet.Packet.Payload)
                    for (var i = 0; i < *payload; i++)
                        ApplyMapEffect(payload[i + 50], *(ushort*)(payload + 2 * i + 26), *(ushort*)(payload + 2 * i + 2));
                break;
        }
    }

    private void ApplyMapEffect(byte index, ushort s1, ushort s2)
    {
        OnEventEnvControl(index, s1 | ((uint)s2 << 16));
#endif
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00800040)
            Mask.Set(index - 4);

        if (state == 0x00400100)
            Mask.Set(index - 4);

        if (state == 0x00040020)
            Mask.Clear(index - 4);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (ShouldDraw)
            foreach (var tile in Mask.SetBits())
                ZoneTile(Arena, tile, ArenaColor.AOE);
    }

    public static void ZoneTile(MiniArena arena, int tile, uint color) => arena.ZoneCone(new(100, 100), tile < 8 ? 0 : 8, tile < 8 ? 8 : 16, 157.5f.Degrees() - 45.Degrees() * (tile % 8), 22.5f.Degrees(), color);

    public static void DrawTile(MiniArena arena, int tile, uint color) => arena.AddDonutCone(new(100, 100), tile < 8 ? 0 : 8, tile < 8 ? 8 : 16, 157.5f.Degrees() - 45.Degrees() * (tile % 8), 22.5f.Degrees(), color);

    public static int GetTile(WPos position)
    {
        var off = new WPos(100, 100) - position;
        var dist = off.Length();
        var angle = Angle.FromDirection(off);
        var ix = (8 - (int)MathF.Ceiling(angle.Deg / 45)) % 8;
        return dist <= 8 ? ix : ix + 8;
    }
    public static int GetTile(Actor actor) => GetTile(actor.Position);

    public bool InActiveTile(Actor actor) => Mask[GetTile(actor)];

    public Func<WPos, bool> TileShape() => p => Mask[GetTile(p)];

    public IEnumerable<int> ActiveTiles => Mask.SetBits();

    public BitMask GetConnectedTiles(int tile)
    {
        var mask = new BitMask();
        var queue = new Queue<int>();
        mask[tile] = true;
        queue.Enqueue(tile);
        while (queue.TryDequeue(out var v))
        {
            foreach (var w in Edges(v))
            {
                if (Mask[w] && !mask[w])
                {
                    mask[w] = true;
                    queue.Enqueue(w);
                }
            }
        }

        return mask;
    }

    private static IEnumerable<int> Edges(int tile)
    {
        switch (tile)
        {
            case 0:
                yield return 7;
                yield return 1;
                yield return 8;
                break;
            case 7:
                yield return 6;
                yield return 0;
                yield return 15;
                break;
            case 8:
                yield return 15;
                yield return 9;
                yield return 0;
                break;
            case 15:
                yield return 14;
                yield return 8;
                yield return 7;
                break;
            default:
                yield return tile - 1;
                yield return tile + 1;
                yield return tile > 8 ? tile - 8 : tile + 8;
                break;
        }
    }
}

