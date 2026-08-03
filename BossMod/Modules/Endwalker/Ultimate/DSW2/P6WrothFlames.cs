namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P6WrothFlames : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = []; // cauterize, then flame blasts
    private WPos _startingSpot;
    private readonly DSW2 bossmodule;

    private static readonly AOEShapeRect _shapeCauterize = new(80f, 11f);
    private static readonly AOEShapeCross _shapeBlast = new(44f, 3f);

    public bool ShowStartingSpot => _startingSpot.X != 0 && _startingSpot.Z != 0 && NumCasts == 0;

    // assume it is activated when hraesvelgr is already in place; could rely on PATE 1E43 instead
    public P6WrothFlames(BossModule module) : base(module)
    {
        bossmodule = (DSW2)module;
        if (bossmodule._HraesvelgrP6 is Actor hraesvelgr)
        {
            _aoes.Add(new(_shapeCauterize, hraesvelgr.Position.Quantized(), hraesvelgr.Rotation, WorldState.FutureTime(8.1d)));
            _startingSpot = new(hraesvelgr.PosRot.X < 95f ? 120f : 80f, _startingSpot.Z); // assume nidhogg is at 78, prefer uptime if possible
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int count = (NumCasts > 0) ? 3 : 4;
        return _aoes.Count > count ? _aoes.AsSpan()[..count] : CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (ShowStartingSpot)
            movementHints.Add(actor.Position, _startingSpot, Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (ShowStartingSpot)
            Arena.ZoneCircleOutline(_startingSpot, 1f, Colors.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ScarletPrice)
        {
            if (_aoes.Count == 4)
            {
                _startingSpot = new(_startingSpot.X, actor.PosRot.Z < Arena.Center.Z ? 120f : 80f);
            }

            var delay = _aoes.Count switch
            {
                < 4 => 8.7d,
                < 7 => 9.7d,
                _ => 6.9d
            };
            _aoes.Add(new(_shapeBlast, actor.Position.Quantized(), default, WorldState.FutureTime(delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CauterizeH or (uint)AID.FlameBlast)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

sealed class P6AkhMorn(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AkhMornFirst, 6f, 8, 8)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { } // do not clear stacks on first cast

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMornFirst or (uint)AID.AkhMornRest)
        {
            if (++NumFinishedStacks >= 4)
            {
                Stacks.Clear();
            }
        }
    }
}

sealed class P6AkhMornVoidzone(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.VoidzoneAhkMorn);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class P6SpreadingEntangledFlames(BossModule module) : Components.UniformStackSpread(module, 4f, 5f, 2)
{
    private readonly P6HotWingTail? _wingTail = module.FindComponent<P6HotWingTail>();
    private readonly bool _voidzonesNorth = module.Enemies((uint)OID.VoidzoneAhkMorn).Sum(z => z.PosRot.Z - module.Center.Z) < 0;

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots(actor))
            movementHints.Add(actor.Position, p, Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pc))
            Arena.ZoneCircleOutline(p, 1f, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        // TODO: activation
        switch (status.ID)
        {
            case (uint)SID.SpreadingFlames:
                AddSpread(actor);
                break;
            case (uint)SID.EntangledFlames:
                AddStack(actor);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SpreadingFlames:
                Spreads.Clear();
                break;
            case (uint)AID.EntangledFlames:
                Stacks.Clear();
                break;
        }
    }

    // note: this assumes standard positions (spreads = black debuffs = under black dragon nidhogg, etc)
    // TODO: consider assigning concrete spots to each player
    private WPos[] SafeSpots(Actor actor)
    {
        if (_wingTail == null)
            return [];

        var x = Arena.Center.X;
        var z = Arena.Center.Z + (_wingTail.NumAOEs != 1 ? default : _voidzonesNorth ? 10f : -10f);
        if (IsSpreadTarget(actor))
        {
            return
            [
                new(x - 18f, z),
                new(x - 12f, z),
                new(x - 6f, z),
                new(x, z)
            ];
        }
        else
        {
            return
            [
                new(x + 9f, z),
                new(x + 18f, z)
            ];
        }
    }
}
