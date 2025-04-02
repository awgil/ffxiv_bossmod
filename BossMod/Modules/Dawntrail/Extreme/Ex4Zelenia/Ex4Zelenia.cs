namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class ThornedCatharsis : Components.RaidwideCast
{
    public ThornedCatharsis(BossModule module) : base(module, ActionID.MakeSpell(AID._Weaponskill_ThornedCatharsis))
    {
        KeepOnPhaseChange = true;
    }
}

class TileTracker : BossComponent
{
    public BitMask TileMask;
    public Func<WPos, float> ActiveTiles { get; private set; } = _ => float.MaxValue;

    public TileTracker(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;

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
            TileMask.Set(index - 4);

        if (state == 0x00400100)
            TileMask.Set(index - 4);

        if (state == 0x00040020)
            TileMask.Clear(index - 4);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        // hints.Add($"Tiles: {string.Join(", ", TileMask.SetBits())}");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add($"Tile: {GetTile(actor)}", false);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var tile in TileMask.SetBits())
        {
            var order = tile % 8;

            Arena.ZoneCone(Module.Center, tile < 8 ? 0 : 8, tile < 8 ? 8 : 16, 157.5f.Degrees() - 45.Degrees() * order, 22.5f.Degrees(), ArenaColor.AOE);
        }
    }

    public int GetTile(WPos position)
    {
        var off = Arena.Center - position;
        var dist = off.Length();
        var angle = Angle.FromDirection(off);
        var ix = (8 - (int)MathF.Ceiling(angle.Deg / 45)) % 8;
        return dist <= 8 ? ix : ix + 8;
    }
    public int GetTile(Actor actor) => GetTile(actor.Position);

    public bool InActiveTile(Actor actor) => TileMask[GetTile(actor)];

    public override void Update()
    {
        List<Func<WPos, float>> zones = [];
        foreach (var tile in TileMask.SetBits())
        {
            var order = tile % 8;

            zones.Add(ShapeDistance.DonutSector(Module.Center, tile < 8 ? 0 : 8, tile < 8 ? 8 : 16, 157.5f.Degrees() - 45.Degrees() * order, 22.5f.Degrees()));
        }

        ActiveTiles = zones.Count > 0 ? ShapeDistance.Union(zones) : _ => float.MaxValue;
    }
}

#if DEBUG
[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1031, NameID = 13861)]
public class Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(16));
#endif
