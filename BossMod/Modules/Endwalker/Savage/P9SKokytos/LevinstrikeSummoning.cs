namespace BossMod.Endwalker.Savage.P9SKokytos;

// TODO: positioning hints for unmarked players
// TODO: or is it a spread?.. one thing i like about bait-away better here is that it better distinguishes bait vs avoid
class LevinstrikeSummoningIcemeld(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly List<Actor> _pendingBaiters = []; // we only want to show max 1 baiter at a time

    private static readonly AOEShapeCircle _shape = new(20);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Icemeld)
        {
            if (CurrentBaits.Count == 0)
                CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
            else
                _pendingBaiters.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Icemeld1 or AID.Icemeld2 or AID.Icemeld3 or AID.Icemeld4)
        {
            ++NumCasts;
            CurrentBaits.Clear();
            if (_pendingBaiters.Count > 0)
            {
                CurrentBaits.Add(new(Module.PrimaryActor, _pendingBaiters[0], _shape));
                _pendingBaiters.RemoveAt(0);
            }
        }
    }
}

// TODO: positioning hints for next baiter
// TODO: or is it a spread?.. one thing i like about bait-away better here is that it better distinguishes bait vs avoid
class LevinstrikeSummoningFiremeld(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Firemeld, centerAtTarget: true)
{
    private readonly Actor?[] _baitOrder = [null, null, null, null];

    private static readonly AOEShapeCircle _shape = new(6);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = (IconID)iconID switch
        {
            IconID.Icon2 => 0,
            IconID.Icon4 => 1,
            IconID.Icon6 => 2,
            IconID.Icon8 => 3,
            _ => -1
        };
        if (order < 0)
            return;
        _baitOrder[order] = actor;
        if (order == 0)
            InitBaits();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            InitBaits();
        }
    }

    private void InitBaits()
    {
        CurrentBaits.Clear();
        var target = NumCasts < _baitOrder.Length ? _baitOrder[NumCasts] : null;
        if (target != null)
            CurrentBaits.Add(new(Module.PrimaryActor, target, _shape));
    }
}

// both explosions and towers
class LevinstrikeSummoningShock(BossModule module) : Components.GenericAOEs(module)
{
    public int NumTowers; // NumCasts counts explosions
    private readonly WPos[] _explodeOrder = [default, default, default, default];
    private readonly Actor?[] _soakerOrder = [null, null, null, null];
    private DateTime _firstExplosion;

    private static readonly AOEShapeCircle _shape = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts < _explodeOrder.Length)
            return new AOEInstance[1] { new(_shape, _explodeOrder[NumCasts], default, _firstExplosion.AddSeconds(NumCasts * 5.6d)) };
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var hint = Array.IndexOf(_soakerOrder, actor) switch
        {
            0 => "Tower -> skip -> bait -> skip",
            1 => "Skip -> tower -> skip -> bait",
            2 => "Bait -> skip -> tower -> skip",
            3 => "Skip -> bait -> skip -> tower",
            _ => ""
        };
        if (hint.Length > 0)
            hints.Add(hint, false);

        if (NumTowers < NumCasts)
        {
            var inTower = actor.Position.InCircle(_explodeOrder[NumTowers], 3f);
            var shouldSoak = _soakerOrder[NumTowers] == actor;
            if (shouldSoak != inTower)
                hints.Add(shouldSoak ? "Soak the tower!" : "GTFO from tower!");
        }

        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumTowers < NumCasts)
            Arena.ZoneCircleOutline(_explodeOrder[NumTowers], 3, _soakerOrder[NumTowers] == pc ? Colors.Safe : 0, 2f);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var (order, isBall) = iconID switch
        {
            (uint)IconID.Icon1 => (0, true),
            (uint)IconID.Icon3 => (1, true),
            (uint)IconID.Icon5 => (2, true),
            (uint)IconID.Icon7 => (3, true),
            (uint)IconID.Icon2 => (2, false),
            (uint)IconID.Icon4 => (3, false),
            (uint)IconID.Icon6 => (0, false),
            (uint)IconID.Icon8 => (1, false),
            _ => (-1, false)
        };
        if (order < 0)
            return;
        if (isBall)
        {
            var dir = (actor.Position - Arena.Center).Normalized();
            _explodeOrder[order] = (Arena.Center - 16f * dir).Quantized();
            _firstExplosion = WorldState.FutureTime(12.7d);
        }
        else
        {
            _soakerOrder[order] = actor;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ShockExplosion:
                ++NumCasts;
                break;
            case (uint)AID.ShockTowerSoak:
            case (uint)AID.ShockTowerFail:
                ++NumTowers;
                break;
        }
    }
}
