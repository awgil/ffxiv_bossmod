namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class Voidzone(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime NextActivation;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 1 && state == 0x00020001)
            NextActivation = WorldState.CurrentTime;
        if (index == 1 && state == 0x00080004)
            NextActivation = default;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NextActivation != default)
            yield return new AOEInstance(new AOEShapeCircle(2), Arena.Center, Activation: NextActivation);
    }
}

class Tiles(BossModule module) : BossComponent(module)
{
    public BitMask Mask;

    public bool ShouldDraw;

    public bool this[int index] => Mask[index];

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
            ZoneTiles(Arena, Mask, ArenaColor.AOE);
    }

    public static void ZoneTile(MiniArena arena, int tile, uint color) => arena.ZoneCone(new(100, 100), tile < 8 ? 0 : 8, tile < 8 ? 8 : 16, 157.5f.Degrees() - 45.Degrees() * (tile % 8), 22.5f.Degrees(), color);

    public static void ZoneTiles(MiniArena arena, BitMask tiles, uint color)
    {
        foreach (var t in tiles.SetBits())
            ZoneTile(arena, t, color);
    }

    public static void DrawTile(MiniArena arena, int tile, uint color) => arena.AddDonutCone(new(100, 100), tile < 8 ? 0 : 8, tile < 8 ? 8 : 16, 157.5f.Degrees() - 45.Degrees() * (tile % 8), 22.5f.Degrees(), color);

    public static void TextTile(MiniArena arena, int tile, uint color)
        => arena.TextWorld(arena.Center + GetTileCenter(tile), tile.ToString(), color);

    public static int GetTile(WPos position)
    {
        var off = new WPos(100, 100) - position;
        var dist = off.Length();
        var angle = Angle.FromDirection(off);
        var ix = (8 - (int)MathF.Ceiling(angle.Deg / 45)) % 8;
        return dist <= 8 ? ix : ix + 8;
    }
    public static int GetTile(Actor actor) => GetTile(actor.Position);

    public static Angle GetTileOrientation(int tile) => 157.5f.Degrees() - 45.Degrees() * (tile % 8);
    public static WDir GetTileCenter(int tile) => GetTileOrientation(tile).ToDirection() * (tile < 8 ? 4 : 12);

    public bool InActiveTile(Actor actor) => Mask[GetTile(actor)];

    public Func<WPos, bool> TileShape() => p => Mask[GetTile(p)];

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

    public static IEnumerable<int> Edges(int tile)
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

abstract class Emblazon(BossModule module) : Components.CastCounter(module, AID.Emblazon)
{
    public BitMask Baiters;
    public DateTime Activation { get; private set; }
    protected readonly Tiles _tiles = module.FindComponent<Tiles>()!;

    protected bool DrawTiles = true;

    protected IEnumerable<Actor> OtherBaits(Actor actor) => Raid.WithSlot().IncludedInMask(Baiters).Exclude(actor).Select(p => p.Item2);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Rose)
        {
            Baiters.Set(Raid.FindSlot(actor.InstanceID));
            Activation = WorldState.FutureTime(6.7f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            DrawTiles = false;
            Baiters.Clear(Raid.FindSlot(spell.MainTargetID));
            if (!Baiters.Any())
                Activation = default;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (slot, player) in Raid.WithSlot().IncludedInMask(Baiters))
        {
            var tile = Tiles.GetTile(player);
            if (slot == pcSlot)
                Tiles.DrawTile(Arena, tile, ArenaColor.Danger);
            else
                Tiles.ZoneTile(Arena, tile, ArenaColor.AOE);
        }

        if (DrawTiles)
            Tiles.ZoneTiles(Arena, _tiles.Mask, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Baiters[slot])
        {
            var tile = Tiles.GetTile(actor);

            if (_tiles[tile])
                hints.Add("GTFO from tile!");

            if (OtherBaits(actor).Any(p => Tiles.GetTile(p) == tile))
                hints.Add("GTFO from spreads!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Baiters[slot])
        {
            hints.AddForbiddenZone(_tiles.TileShape(), Activation);

            foreach (var b in OtherBaits(actor))
            {
                var t = Tiles.GetTile(b);
                hints.AddForbiddenZone(p => Tiles.GetTile(p) == t, Activation);
            }
        }

        if (Baiters.Any())
            hints.AddPredictedDamage(Baiters, Activation, AIHints.PredictedDamageType.Raidwide);
    }
}
