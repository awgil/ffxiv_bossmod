namespace BossMod.Endwalker.Ultimate.DSW2;

class P6WrothFlames : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new(); // cauterize, then flame blasts
    private WPos _startingSpot;

    private static readonly AOEShapeRect _shapeCauterize = new(80, 11);
    private static readonly AOEShapeCross _shapeBlast = new(44, 3);

    public bool ShowStartingSpot => _startingSpot.X != 0 && _startingSpot.Z != 0 && NumCasts == 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(NumCasts > 0 ? 3 : 4);

    // assume it is activated when hraesvelgr is already in place; could rely on PATE 1E43 instead
    public override void Init(BossModule module)
    {
        var cauterizeCaster = module.Enemies(OID.HraesvelgrP6).FirstOrDefault();
        if (cauterizeCaster != null)
        {
            _aoes.Add(new(_shapeCauterize, cauterizeCaster.Position, cauterizeCaster.Rotation, module.WorldState.CurrentTime.AddSeconds(8.1f)));
            _startingSpot.X = cauterizeCaster.Position.X < 95 ? 120 : 80; // assume nidhogg is at 78, prefer uptime if possible
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (movementHints != null && ShowStartingSpot)
            movementHints.Add(actor.Position, _startingSpot, ArenaColor.Safe);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (ShowStartingSpot)
            arena.AddCircle(_startingSpot, 1, ArenaColor.Safe);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.ScarletPrice)
        {
            if (_aoes.Count == 4)
                _startingSpot.Z = actor.Position.Z < module.Bounds.Center.Z ? 120 : 80;

            var delay = _aoes.Count switch
            {
                < 4 => 8.7f,
                < 7 => 9.7f,
                _ => 6.9f
            };
            _aoes.Add(new(_shapeBlast, actor.Position, default, module.WorldState.CurrentTime.AddSeconds(delay)));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CauterizeH or AID.FlameBlast)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class P6AkhMorn : Components.StackWithCastTargets
{
    public P6AkhMorn() : base(ActionID.MakeSpell(AID.AkhMornFirst), 6, 8) { }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell) { } // do not clear stacks on first cast

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMornFirst or AID.AkhMornRest)
            if (++NumFinishedStacks >= 4)
                Stacks.Clear();
    }
}

class P6AkhMornVoidzone : Components.PersistentVoidzone
{
    public P6AkhMornVoidzone() : base(6, m => m.Enemies(OID.VoidzoneAhkMorn).Where(z => z.EventState != 7)) { }
}

class P6SpreadingEntangledFlames : Components.UniformStackSpread
{
    private P6HotWingTail? _wingTail;
    private bool _voidzonesNorth;

    public P6SpreadingEntangledFlames() : base(4, 5, 2, alwaysShowSpreads: true) { }

    public override void Init(BossModule module)
    {
        _wingTail = module.FindComponent<P6HotWingTail>();
        _voidzonesNorth = module.Enemies(OID.VoidzoneAhkMorn).Sum(z => z.Position.Z - module.Bounds.Center.Z) < 0;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (movementHints != null)
            foreach (var p in SafeSpots(module, actor))
                movementHints.Add(actor.Position, p, ArenaColor.Safe);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);
        foreach (var p in SafeSpots(module, pc))
            arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        // TODO: activation
        switch ((SID)status.ID)
        {
            case SID.SpreadingFlames:
                AddSpread(actor);
                break;
            case SID.EntangledFlames:
                AddStack(actor);
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SpreadingFlames:
                Spreads.Clear();
                break;
            case AID.EntangledFlames:
                Stacks.Clear();
                break;
        }
    }

    // note: this assumes standard positions (spreads = black debuffs = under black dragon nidhogg, etc)
    // TODO: consider assigning concrete spots to each player
    private IEnumerable<WPos> SafeSpots(BossModule module, Actor actor)
    {
        if (_wingTail == null)
            yield break;

        float z = module.Bounds.Center.Z + (_wingTail.NumAOEs != 1 ? 0 : _voidzonesNorth ? 10 : -10);
        if (IsSpreadTarget(actor))
        {
            yield return new WPos(module.Bounds.Center.X - 18, z);
            yield return new WPos(module.Bounds.Center.X - 12, z);
            yield return new WPos(module.Bounds.Center.X - 6, z);
            yield return new WPos(module.Bounds.Center.X, z);
        }
        else
        {
            yield return new WPos(module.Bounds.Center.X + 9, z);
            yield return new WPos(module.Bounds.Center.X + 18, z);
        }
    }
}
