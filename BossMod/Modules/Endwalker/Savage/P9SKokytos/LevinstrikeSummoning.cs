namespace BossMod.Endwalker.Savage.P9SKokytos;

// TODO: positioning hints for unmarked players
// TODO: or is it a spread?.. one thing i like about bait-away better here is that it better distinguishes bait vs avoid
class LevinstrikeSummoningIcemeld : Components.GenericBaitAway
{
    private List<Actor> _pendingBaiters = new(); // we only want to show max 1 baiter at a time

    private static readonly AOEShapeCircle _shape = new(20);

    public LevinstrikeSummoningIcemeld() : base(centerAtTarget: true) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Icemeld)
        {
            if (CurrentBaits.Count == 0)
                CurrentBaits.Add(new(module.PrimaryActor, actor, _shape));
            else
                _pendingBaiters.Add(actor);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Icemeld1 or AID.Icemeld2 or AID.Icemeld3 or AID.Icemeld4)
        {
            ++NumCasts;
            CurrentBaits.Clear();
            if (_pendingBaiters.Count > 0)
            {
                CurrentBaits.Add(new(module.PrimaryActor, _pendingBaiters[0], _shape));
                _pendingBaiters.RemoveAt(0);
            }
        }
    }
}

// TODO: positioning hints for next baiter
// TODO: or is it a spread?.. one thing i like about bait-away better here is that it better distinguishes bait vs avoid
class LevinstrikeSummoningFiremeld : Components.GenericBaitAway
{
    private Actor?[] _baitOrder = { null, null, null, null };

    private static readonly AOEShapeCircle _shape = new(6);

    public LevinstrikeSummoningFiremeld() : base(ActionID.MakeSpell(AID.Firemeld), centerAtTarget: true) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        int order = (IconID)iconID switch
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
            InitBaits(module);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            InitBaits(module);
        }
    }

    private void InitBaits(BossModule module)
    {
        CurrentBaits.Clear();
        var target = NumCasts < _baitOrder.Length ? _baitOrder[NumCasts] : null;
        if (target != null)
            CurrentBaits.Add(new(module.PrimaryActor, target, _shape));
    }
}

// both explosions and towers
class LevinstrikeSummoningShock : Components.GenericAOEs
{
    public int NumTowers { get; private set; } // NumCasts counts explosions
    private WPos[] _explodeOrder = { default, default, default, default };
    private Actor?[] _soakerOrder = { null, null, null, null };
    private DateTime _firstExplosion;

    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (NumCasts < _explodeOrder.Length)
            yield return new(_shape, _explodeOrder[NumCasts], default, _firstExplosion.AddSeconds(NumCasts * 5.6f));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
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
            bool inTower = actor.Position.InCircle(_explodeOrder[NumTowers], 3);
            bool shouldSoak = _soakerOrder[NumTowers] == actor;
            if (shouldSoak != inTower)
                hints.Add(shouldSoak ? "Soak the tower!" : "GTFO from tower!");
        }

        base.AddHints(module, slot, actor, hints, movementHints);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (NumTowers < NumCasts)
            arena.AddCircle(_explodeOrder[NumTowers], 3, _soakerOrder[NumTowers] == pc ? ArenaColor.Safe : ArenaColor.Danger, 2);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var (order, isBall) = (IconID)iconID switch
        {
            IconID.Icon1 => (0, true),
            IconID.Icon3 => (1, true),
            IconID.Icon5 => (2, true),
            IconID.Icon7 => (3, true),
            IconID.Icon2 => (2, false),
            IconID.Icon4 => (3, false),
            IconID.Icon6 => (0, false),
            IconID.Icon8 => (1, false),
            _ => (-1, false)
        };
        if (order < 0)
            return;
        if (isBall)
        {
            var dir = (actor.Position - module.Bounds.Center).Normalized();
            _explodeOrder[order] = module.Bounds.Center - 16 * dir;
            _firstExplosion = module.WorldState.CurrentTime.AddSeconds(12.7f);
        }
        else
        {
            _soakerOrder[order] = actor;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShockExplosion:
                ++NumCasts;
                break;
            case AID.ShockTowerSoak:
            case AID.ShockTowerFail:
                ++NumTowers;
                break;
        }
    }
}
