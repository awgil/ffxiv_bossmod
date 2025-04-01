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

    public TileTracker(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;

#if DEBUG
        WorldState.Network.ServerIPCReceived.Subscribe(OnIPCReceived);
#endif
    }

    public override void OnEventMapEffect(uint index, ushort state)
    {
        if (index is >= 4 and <= 19)
        {
            var ix = (int)index - 4;
            switch (state)
            {
                case 0x100:
                    TileMask.Set(ix);
                    break;
                case 0x20:
                    TileMask.Clear(ix);
                    break;
            }
        }
    }

#if DEBUG
    private unsafe void OnIPCReceived(NetworkState.OpServerIPC packet)
    {
        var opcode = (int)packet.Packet.ID;
        if (opcode == 405)
            fixed (byte* payload = packet.Packet.Payload)
                for (var i = 0; i < *payload; i++)
                    OnEventMapEffect(payload[i + 50], *(ushort*)(payload + 2 * i + 26));

        if (opcode == 404)
            fixed (byte* payload = packet.Packet.Payload)
                for (var i = 0; i < *payload; i++)
                    OnEventMapEffect(payload[i + 34], *(ushort*)(payload + 2 * i + 18));

        if (opcode == 403)
            fixed (byte* payload = packet.Packet.Payload)
                for (var i = 0; i < *payload; i++)
                    OnEventMapEffect(payload[i + 18], *(ushort*)(payload + 2 * i + 10));
    }
#endif

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

    public Func<WPos, float> ActiveTiles { get; private set; }

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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1031, NameID = 13861)]
public class Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(16));

