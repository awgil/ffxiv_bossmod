namespace BossMod.Dawntrail.Ultimate.UMAD;

class P2UltimateEmbrace(BossModule module) : Components.CastSharedTankbuster(module, AID._Ability_UltimateEmbrace, 5);

class P2ForsakenRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Ability_Forsaken);

class P2AllThingsEnding(BossModule module) : Components.StandardAOEs(module, AID._Ability_AllThingsEnding, new AOEShapeCone(100, 90.Degrees()));

// mapeffect: XX.00020001
// index is 1 (N) clockwise through 8 (NW)
// tower triggers 10.2s later
class P2PathOfLight(BossModule module) : Components.GenericTowers(module, AID._Ability_ThePathOfLight)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 1 and <= 8 && state == 0x00020001)
        {
            var angle = (180 - (index - 1) * 45).Degrees();
            Towers.Add(new(Arena.Center + angle.ToDirection() * 8, 4, minSoakers: 2, maxSoakers: 2, default, WorldState.FutureTime(10.2f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }

    public bool InAnyTower(Actor a) => Towers.Any(t => a.Position.InCircle(t.Position, t.Radius));
}

class P2Shapes(BossModule module) : Components.CastCounterMulti(module, [AID._Ability_Spelldriver, AID._Ability_Spellscatter, AID._Ability_Spellwave])
{
    public enum Shape
    {
        None,
        Stack,
        Spread,
        Cone
    }

    public readonly Shape[] Shapes = new Shape[8];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shapes[slot] != default)
            hints.Add($"Current shape: {Shapes[slot]}", false);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var shape = (SID)status.ID switch
        {
            SID.ForsakenStack => Shape.Stack,
            SID.ForsakenSpread => Shape.Spread,
            SID.ForsakenCone => Shape.Cone,
            _ => default
        };

        if (shape != default && Raid.TryFindSlot(actor, out var slot))
            Shapes[slot] = shape;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.ForsakenStack or SID.ForsakenSpread or SID.ForsakenCone && Raid.TryFindSlot(actor, out var slot))
            Shapes[slot] = Shape.None;
    }

    public void Reset()
    {
        NumCasts = 0;
    }
}

class P2StackSpread(BossModule module) : Components.UniformStackSpread(module, 5, 5, 3)
{
    readonly P2Shapes _shapes = module.FindComponent<P2Shapes>()!;
    readonly P2PathOfLight _towers = module.FindComponent<P2PathOfLight>()!;

    bool _castPending;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_ThePathOfLight:
                _castPending = true;
                break;
            case AID._Ability_Spellscatter:
            case AID._Ability_Spellwave:
            case AID._Ability_Spelldriver:
                _castPending = false;
                break;
        }
    }

    public override void Update()
    {
        if (_castPending)
            return;

        Stacks.Clear();
        Spreads.Clear();

        foreach (var (i, player) in Raid.WithSlot().WhereActor(_towers.InAnyTower))
        {
            if (_shapes.Shapes[i] == P2Shapes.Shape.Stack)
                AddStack(player, _towers.Towers[0].Activation);
            else if (_shapes.Shapes[i] == P2Shapes.Shape.Spread)
                AddSpread(player, _towers.Towers[0].Activation);
        }
    }
}

class P2Spellwave(BossModule module) : Components.GenericBaitAway(module, AID._Ability_Spellwave)
{
    readonly P2Shapes _shapes = module.FindComponent<P2Shapes>()!;
    readonly P2PathOfLight _towers = module.FindComponent<P2PathOfLight>()!;

    bool _castPending;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_ThePathOfLight:
                _castPending = true;
                break;
            case AID._Ability_Spellscatter:
            case AID._Ability_Spellwave:
            case AID._Ability_Spelldriver:
                _castPending = false;
                break;
        }
    }

    public override void Update()
    {
        if (_castPending)
            return;

        CurrentBaits.Clear();

        foreach (var (i, player) in Raid.WithSlot().WhereSlot(s => _shapes.Shapes[s] == P2Shapes.Shape.Cone).WhereActor(_towers.InAnyTower))
        {
            var closest = Raid.WithoutSlot().Exclude(player).Closest(player.Position);
            if (closest != null)
                CurrentBaits.Add(new(player, closest, new AOEShapeCone(40, 45.Degrees()), _towers.Towers[0].Activation));
        }
    }
}

class P2PastFutureEnd(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    WPos _source;
    DateTime _activation;
    int _numJumps;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_PastsEnd or AID._Ability_FuturesEnd)
        {
            _source = spell.LocXZ;
            _activation = Module.CastFinishAt(spell, 0.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_PastsEnd1 or AID._Ability_PastsEnd2)
        {
            _numJumps++;
            if (_numJumps >= 3)
            {
                _source = default;
                _numJumps = 0;
            }
        }
    }

    public override void Update()
    {
        Spreads.Clear();

        if (_source == default)
            return;

        AddSpreads(Raid.WithoutSlot().SortedByRange(_source).Take(4), _activation);
    }
}
