namespace BossMod.Stormblood.Ultimate.UCOB;

class P2BahamutsFavorFireball(BossModule module) : Components.UniformStackSpread(module, 4, 0, 1)
{
    public Actor? Target;
    private BitMask _forbidden;
    private DateTime _activation;

    public void Show()
    {
        if (Target != null)
            AddStack(Target, _activation, _forbidden);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Firescorched)
        {
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = _forbidden;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Firescorched)
        {
            _forbidden.Clear(Raid.FindSlot(actor.InstanceID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = _forbidden;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Fireball)
        {
            Target = WorldState.Actors.Find(tether.Target);
            _activation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
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
class P2BahamutsFavorChainLightning(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true)
{
    private BitMask _pendingTargets;
    private DateTime _expectedStatuses;

    public bool ActiveOrSkipped() => Active || _pendingTargets.Any() && WorldState.CurrentTime >= _expectedStatuses && Raid.WithSlot(true).IncludedInMask(_pendingTargets).All(ip => ip.Item2.IsDead);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Thunderstruck)
        {
            AddSpread(actor, status.ExpireAt);
            _pendingTargets.Reset();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ChainLightning:
                _expectedStatuses = WorldState.FutureTime(1);
                foreach (var t in spell.Targets)
                    _pendingTargets.Set(Raid.FindSlot(t.ID));
                break;
            case AID.ChainLightningAOE:
                Spreads.Clear();
                break;
        }
    }
}

class P2BahamutsFavorDeathstorm(BossModule module) : BossComponent(module)
{
    public int NumDeathstorms { get; private set; }
    private readonly List<(Actor player, DateTime expiration, bool cleansed)> _dooms = [];
    private readonly List<(WPos predicted, Actor? voidzone)> _cleanses = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var doomOrder = _dooms.FindIndex(d => d.player == actor);
        if (doomOrder >= 0 && !_dooms[doomOrder].cleansed)
            hints.Add($"Doom {doomOrder + 1}", (_dooms[doomOrder].expiration - WorldState.CurrentTime).TotalSeconds < 3);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var doomOrder = _dooms.FindIndex(d => d.player == pc);
        if (doomOrder >= 0 && !_dooms[doomOrder].cleansed && doomOrder < _cleanses.Count)
            Arena.AddCircle(_cleanses[doomOrder].voidzone?.Position ?? _cleanses[doomOrder].predicted, 1, ArenaColor.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.VoidzoneSalvation)
        {
            var index = _cleanses.FindIndex(z => z.voidzone == null && z.predicted.AlmostEqual(actor.Position, 0.5f));
            if (index >= 0)
                _cleanses.Ref(index).voidzone = actor;
            else
                ReportError($"Failed to find voidzone predicted pos for {actor}");
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
        {
            _dooms.Add((actor, status.ExpireAt, false));
            _dooms.SortBy(d => d.expiration);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
        {
            var index = _dooms.FindIndex(d => d.player == actor);
            if (index >= 0)
                _dooms.Ref(index).cleansed = true;
            else
                ReportError($"Failed to find doom on {actor}");
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WingsOfSalvation)
            _cleanses.Add((spell.LocXZ, null));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Deathstorm)
        {
            _dooms.Clear();
            _cleanses.Clear();
            ++NumDeathstorms;
        }
    }
}

class P2BahamutsFavorWingsOfSalvation(BossModule module) : Components.StandardAOEs(module, AID.WingsOfSalvation, 4);
