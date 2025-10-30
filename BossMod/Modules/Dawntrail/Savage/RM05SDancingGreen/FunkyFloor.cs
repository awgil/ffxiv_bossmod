namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

record class AOEShapeTiles(BitMask Tiles) : AOEShape
{
    public BitMask Tiles = Tiles;

    public override bool Check(WPos position, WPos origin, Angle rotation) => Tiles[FunkyFloor.GetPosTile(position)];
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0)
    {
        foreach (var t in Tiles.SetBits())
        {
            var center = FunkyFloor.GetTilePos(t);
            arena.ZoneRect(center - new WDir(2.5f, 0), center + new WDir(2.5f, 0), 2.5f, color);
        }
    }
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0) { }
}

class FloorCounter(BossModule module) : Components.CastCounter(module, AID.FunkyFloorActivate);

class FunkyFloor(BossModule module) : Components.GenericAOEs(module, AID.FunkyFloorActivate)
{
    private static readonly BitMask EvenTiles = new(0xAA55AA55AA55AA55ul);
    private static readonly BitMask InnerTiles = new(0x00003C3C3C3C0000ul);

    public int MaxCasts = 10;

    public BitMask Tiles;
    private DateTime NextActivation;

    public bool Active => Tiles.Any();

    public BitMask InnerSafeShort;
    public BitMask InnerSafeLong;
    public BitMask OuterSafe;

    private int NumActivations;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.FunkyFloorActivate)
        {
            NextActivation = WorldState.FutureTime(4.1f);
            Tiles = ~Tiles;
            NumActivations++;

            if (NumActivations >= MaxCasts)
            {
                Tiles.Reset();
                NextActivation = default;
            }
        }
    }

    private void Activate(bool even)
    {
        NextActivation = WorldState.FutureTime(3.1f);
        Tiles = even ? EvenTiles : ~EvenTiles;
        foreach (var sp in Module.Enemies(OID.Spotlight))
        {
            var t = GetPosTile(sp.Position);
            if (InnerTiles[t])
            {
                if (Tiles[t])
                    InnerSafeLong.Set(t);
                else
                {
                    var col = t & 7;
                    var row = t >> 3;
                    var invcol = col is 3 or 4 ? col : col > 4 ? col - 3 : col + 3;
                    var invrow = row is 3 or 4 ? row : row > 4 ? row - 3 : row + 3;
                    InnerSafeShort.Set(invcol + (invrow << 3));
                }
            }
            else if (Tiles[t])
                OuterSafe.Set(t);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (!Active && index == 3)
        {
            if (state == 0x00020001)
                Activate(true);
            else if (state == 0x00200010)
                Activate(false);
        }
    }

    public static WPos GetTilePos(int bit) => new(82.5f + (bit & 7) * 5, 82.5f + (bit >> 3) * 5);
    public static int GetPosTile(WPos p)
    {
        var bit = (p - new WPos(80, 80)) / 5;
        return (int)bit.X + ((int)bit.Z << 3);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield break;

        yield return new(new AOEShapeTiles(Tiles), default, default, NextActivation);
    }
}

class BurnBabyBurn(BossModule module) : BossComponent(module)
{
    public enum Order { None, Short, Long }
    public readonly Order[] Orders = Utils.MakeArray(8, Order.None);

    public readonly DateTime[] Timers = Utils.MakeArray(8, default(DateTime));

    public int NumShort => Orders.Count(o => o == Order.Short);
    public int NumLong => Orders.Count(o => o == Order.Long);

    private FunkyFloor? _danceFloor;

    public override void Update()
    {
        _danceFloor ??= Module.FindComponent<FunkyFloor>();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        switch (Orders[slot])
        {
            case Order.Short:
                hints.Add($"Short timer", false);
                break;
            case Order.Long:
                hints.Add("Long timer", false);
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.BurnBabyBurn)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            Timers[slot] = status.ExpireAt;
            Orders[slot] = status.ExpireAt > WorldState.FutureTime(30) ? Order.Long : Order.Short;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.BurnBabyBurn)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            Timers[slot] = default;
            Orders[slot] = Order.None;
        }
    }

    private IEnumerable<int> GetSafeTiles(int slot)
    {
        var order = Orders[slot];
        if (order == Order.None || _danceFloor == null)
            yield break;

        foreach (var t in _danceFloor.OuterSafe.SetBits())
            yield return t;

        foreach (var t in (order == Order.Short ? _danceFloor.InnerSafeShort : _danceFloor.InnerSafeLong).SetBits())
            yield return t;
    }

    private readonly RM05SDancingGreenConfig _cfg = Service.Config.Get<RM05SDancingGreenConfig>();

    private bool Imminent(int slot) => Orders[slot] != Order.None && Timers[slot] < WorldState.FutureTime(_cfg.SpotlightHintSeconds);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Imminent(pcSlot))
            foreach (var t in GetSafeTiles(pcSlot))
                Arena.AddCircle(FunkyFloor.GetTilePos(t), 1, ArenaColor.Safe);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (Imminent(slot))
            foreach (var t in GetSafeTiles(slot))
                movementHints.Add((actor.Position, FunkyFloor.GetTilePos(t), Imminent(slot) ? ArenaColor.Safe : ArenaColor.Danger));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Imminent(slot))
        {
            var tiles = GetSafeTiles(slot).Select(t => ShapeContains.Circle(FunkyFloor.GetTilePos(t), 2.5f)).ToList();
            var all = ShapeContains.Union(tiles);
            hints.AddForbiddenZone(p => !all(p), Timers[slot]);
        }
    }
}

class InsideOutside(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeDonut Donut = new(4.998f, 60);
    public static readonly AOEShapeCircle Circle = new(7);

    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InsideOutVisual)
        {
            _aoes.Add(new(Circle, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(Donut, caster.Position, default, Module.CastFinishAt(spell, 2.5f)));
        }
        if ((AID)spell.Action.ID == AID.OutsideInVisual)
        {
            _aoes.Add(new(Donut, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(Circle, caster.Position, default, Module.CastFinishAt(spell, 2.5f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.InsideOutCircle or AID.InsideOutDonut or AID.OutsideInDonut or AID.OutsideInCircle)
        {
            NumCasts++;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class BurnBabyBurn2(BossModule module) : BossComponent(module)
{
    private readonly int[] orders = new int[PartyState.MaxPartySize];
    private readonly List<Actor> Spotlights = [];
    private int FrogCasts;

    private BackupDance? backupDance;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BurnBabyBurn && Raid.TryFindSlot(actor.InstanceID, out var slot))
            orders[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 14 ? 1 : 2;
    }

    public override void Update()
    {
        backupDance ??= Module.FindComponent<BackupDance>();
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DC)
            Spotlights.Add(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (FrogCasts > 4)
            return;

        if (orders[slot] == 1 && FrogCasts > 0 || orders[slot] == 2 && FrogCasts == 0)
            hints.Add("Bait next!", backupDance != null && !backupDance.CurrentBaits.Any(b => b.Target == actor));
        else if (orders[slot] == 1 && FrogCasts == 0 || orders[slot] == 2 && FrogCasts > 0)
            hints.Add("Go to spotlight!", !Spotlights.Any(s => actor.Position.InCircle(s.Position, 2.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BackUpDance)
            FrogCasts++;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (FrogCasts > 4)
            return;

        foreach (var sp in Spotlights)
            Arena.ZoneCircle(sp.Position, 2.5f, ArenaColor.SafeFromAOE);
    }
}
