namespace BossMod.Stormblood.Ultimate.UCOB;

class P2BahamutsFavorFireball : Components.UniformStackSpread
{
    public Actor? Target;
    private BitMask _forbidden;
    private DateTime _activation;

    public P2BahamutsFavorFireball() : base(4, 0, 1) { }

    public void Show()
    {
        if (Target != null)
            AddStack(Target, _activation, _forbidden);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Firescorched)
        {
            _forbidden.Set(module.Raid.FindSlot(actor.InstanceID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = _forbidden;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Firescorched)
        {
            _forbidden.Clear(module.Raid.FindSlot(actor.InstanceID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = _forbidden;
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Fireball)
        {
            Target = module.WorldState.Actors.Find(tether.Target);
            _activation = module.WorldState.CurrentTime.AddSeconds(5.1);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FireballP2)
        {
            Stacks.Clear();
            Target = null;
            _activation = default;
        }
    }
}

// note: if player dies immediately after chain lightning cast, he won't get a status or have aoe cast; if he dies after status application, aoe will be triggered immediately
class P2BahamutsFavorChainLightning : Components.UniformStackSpread
{
    private BitMask _pendingTargets;
    private DateTime _expectedStatuses;

    public P2BahamutsFavorChainLightning() : base(0, 5, alwaysShowSpreads: true) { }

    public bool ActiveOrSkipped(BossModule module) => Active || _pendingTargets.Any() && module.WorldState.CurrentTime >= _expectedStatuses && module.Raid.WithSlot(true).IncludedInMask(_pendingTargets).All(ip => ip.Item2.IsDead);

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Thunderstruck)
        {
            AddSpread(actor, status.ExpireAt);
            _pendingTargets.Reset();
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ChainLightning:
                _expectedStatuses = module.WorldState.CurrentTime.AddSeconds(1);
                foreach (var t in spell.Targets)
                    _pendingTargets.Set(module.Raid.FindSlot(t.ID));
                break;
            case AID.ChainLightningAOE:
                Spreads.Clear();
                break;
        }
    }
}

class P2BahamutsFavorDeathstorm : BossComponent
{
    public int NumDeathstorms { get; private set; }
    private List<(Actor player, DateTime expiration, bool cleansed)> _dooms = new();
    private List<(WPos predicted, Actor? voidzone)> _cleanses = new();

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var doomOrder = _dooms.FindIndex(d => d.player == actor);
        if (doomOrder >= 0 && !_dooms[doomOrder].cleansed)
            hints.Add($"Doom {doomOrder + 1}", (_dooms[doomOrder].expiration - module.WorldState.CurrentTime).TotalSeconds < 3);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var doomOrder = _dooms.FindIndex(d => d.player == pc);
        if (doomOrder >= 0 && !_dooms[doomOrder].cleansed && doomOrder < _cleanses.Count)
            arena.AddCircle(_cleanses[doomOrder].voidzone?.Position ?? _cleanses[doomOrder].predicted, 1, ArenaColor.Safe);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.VoidzoneSalvation)
        {
            var index = _cleanses.FindIndex(z => z.voidzone == null && z.predicted.AlmostEqual(actor.Position, 0.5f));
            if (index >= 0)
                _cleanses.Ref(index).voidzone = actor;
            else
                module.ReportError(this, $"Failed to find voidzone predicted pos for {actor}");
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
        {
            _dooms.Add((actor, status.ExpireAt, false));
            _dooms.SortBy(d => d.expiration);
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
        {
            var index = _dooms.FindIndex(d => d.player == actor);
            if (index >= 0)
                _dooms.Ref(index).cleansed = true;
            else
                module.ReportError(this, $"Failed to find doom on {actor}");
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WingsOfSalvation)
            _cleanses.Add((spell.LocXZ, null));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Deathstorm)
        {
            _dooms.Clear();
            _cleanses.Clear();
            ++NumDeathstorms;
        }
    }
}

class P2BahamutsFavorWingsOfSalvation : Components.LocationTargetedAOEs
{
    public P2BahamutsFavorWingsOfSalvation() : base(ActionID.MakeSpell(AID.WingsOfSalvation), 4) { }
}
