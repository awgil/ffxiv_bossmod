namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class FloorCounter(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID._Weaponskill_FunkyFloor1));

class FunkyFloor(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID._Weaponskill_FunkyFloor1))
{
    private static readonly BitMask EvenTiles = new(0xAA55AA55AA55AA55ul);

    public BitMask Tiles;
    private DateTime NextActivation;

    public bool Active => Tiles.Any();

    public enum Pattern { None, Even, Odd }
    public Pattern CurPattern;

    private int NumActivations;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID._Weaponskill_FunkyFloor1)
        {
            NextActivation = WorldState.FutureTime(4.1f);
            Tiles = ~Tiles;
            NumActivations++;

            if (NumActivations >= 10)
            {
                Tiles.Reset();
                CurPattern = Pattern.None;
                NextActivation = default;
            }
        }
    }

    private void Activate(bool even)
    {
        NextActivation = WorldState.FutureTime(3.1f);
        Tiles = even ? EvenTiles : ~EvenTiles;
        CurPattern = even ? Pattern.Even : Pattern.Odd;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (!Active && index == 3)
        {
            if (state == 0x00020001)
                Activate(true);
            else if (state == 0x00200010)
                Activate(false);
        }
    }

    private int GetTile(WPos p)
    {
        var bit = (p - new WPos(80, 80)) / 5;
        return (int)bit.X + ((int)bit.Z << 3);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;

        hints.AddForbiddenZone(p => Tiles[GetTile(p)] ? -1 : 1, NextActivation);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        switch (CurPattern)
        {
            case Pattern.Even:
                hints.Add($"Outside: 1/3, inside: 2/4");
                break;
            case Pattern.Odd:
                hints.Add($"Outside: 2/4, inside: 1/3");
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        foreach (var bit in Tiles.SetBits())
        {
            var center = LocateTile(bit);
            Arena.ZoneRect(center - new WDir(2.5f, 0), center + new WDir(2.5f, 0), 2.5f, ArenaColor.AOE);
        }
    }

    public WPos LocateTile(int bit) => new(82.5f + (bit & 7) * 5, 82.5f + (bit >> 3) * 5);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Tiles[GetTile(actor.Position)])
            hints.Add("GTFO from tile!");
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
        if (status.ID == (uint)SID._Gen_BurnBabyBurn)
        {
            Timers[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt;
            Orders[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt > WorldState.FutureTime(30) ? Order.Long : Order.Short;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_BurnBabyBurn)
        {
            Timers[Raid.FindSlot(actor.InstanceID)] = default;
            Orders[Raid.FindSlot(actor.InstanceID)] = Order.None;
        }
    }

    private IEnumerable<int> GetSafeTiles(int slot)
    {
        var order = Orders[slot];
        if (order == Order.None || _danceFloor == null || _danceFloor.CurPattern == FunkyFloor.Pattern.None)
            yield break;

        if (_danceFloor.CurPattern == FunkyFloor.Pattern.Even)
        {
            yield return 9;
            yield return 54;
        }
        else
        {
            yield return 14;
            yield return 49;
        }

        switch ((order, _danceFloor.CurPattern))
        {
            case (Order.Short, FunkyFloor.Pattern.Even):
                yield return 29;
                yield return 34;
                break;
            case (Order.Short, FunkyFloor.Pattern.Odd):
                yield return 26;
                yield return 37;
                break;
            case (Order.Long, FunkyFloor.Pattern.Even):
                yield return 20;
                yield return 43;
                break;
            case (Order.Long, FunkyFloor.Pattern.Odd):
                yield return 19;
                yield return 44;
                break;
        }
    }

    private bool Imminent(int slot) => Orders[slot] != Order.None && Timers[slot] < WorldState.FutureTime(7);

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var t in GetSafeTiles(slot))
            movementHints.Add((actor.Position, _danceFloor!.LocateTile(t), Imminent(slot) ? ArenaColor.Safe : ArenaColor.Danger));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Imminent(slot))
        {
            var tiles = GetSafeTiles(slot).Select(t => ShapeDistance.Circle(_danceFloor!.LocateTile(t), 2.5f)).ToList();
            var all = ShapeDistance.Union(tiles);
            hints.AddForbiddenZone(p => -all(p), Timers[slot]);
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
        if ((AID)spell.Action.ID == AID._Weaponskill_InsideOut1)
        {
            _aoes.Add(new(Circle, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(Donut, caster.Position, default, Module.CastFinishAt(spell, 2.5f)));
        }
        if ((AID)spell.Action.ID == AID._Weaponskill_OutsideIn1)
        {
            _aoes.Add(new(Donut, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(Circle, caster.Position, default, Module.CastFinishAt(spell, 2.5f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_InsideOut or AID._Weaponskill_InsideOut2 or AID._Weaponskill_OutsideIn or AID._Weaponskill_OutsideIn2)
        {
            NumCasts++;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
