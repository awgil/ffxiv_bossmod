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

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Firescorched)
        {
            _forbidden[Raid.FindSlot(actor.InstanceID)] = true;
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = _forbidden;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Firescorched)
        {
            _forbidden[Raid.FindSlot(actor.InstanceID)] = false;
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = _forbidden;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Fireball)
        {
            Target = WorldState.Actors.Find(tether.Target);
            _activation = WorldState.FutureTime(5.1d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FireballP2)
        {
            Stacks.Clear();
            Target = null;
            _activation = default;
        }
    }
}

// note: if player dies immediately after chain lightning cast, he won't get a status or have aoe cast; if he dies after status application, aoe will be triggered immediately
class P2BahamutsFavorChainLightning(BossModule module) : Components.UniformStackSpread(module, default, 5f)
{
    private BitMask _pendingTargets;
    private DateTime _expectedStatuses;

    public bool ActiveOrSkipped() => Active || _pendingTargets.Any() && WorldState.CurrentTime >= _expectedStatuses && Raid.WithSlot(true, true, true).IncludedInMask(_pendingTargets).All(ip => ip.Item2.IsDead);

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Thunderstruck)
        {
            AddSpread(actor, status.ExpireAt);
            _pendingTargets = default;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChainLightning:
                _expectedStatuses = WorldState.FutureTime(1d);
                foreach (var t in spell.Targets)
                    _pendingTargets.Set(Raid.FindSlot(t.ID));
                break;
            case (uint)AID.ChainLightningAOE:
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
            Arena.ZoneCircleOutline(_cleanses[doomOrder].voidzone?.Position ?? _cleanses[doomOrder].predicted, 1, Colors.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VoidzoneSalvation)
        {
            var index = _cleanses.FindIndex(z => z.voidzone == null && z.predicted.AlmostEqual(actor.Position, 0.5f));
            if (index >= 0)
                _cleanses.Ref(index).voidzone = actor;
            else
                ReportError($"Failed to find voidzone predicted pos for {actor}");
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            _dooms.Add((actor, status.ExpireAt, false));
            _dooms.Sort(static (a, b) => a.expiration.CompareTo(b.expiration));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
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
        if (spell.Action.ID == (uint)AID.WingsOfSalvation)
            _cleanses.Add((spell.LocXZ, null));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Deathstorm)
        {
            _dooms.Clear();
            _cleanses.Clear();
            ++NumDeathstorms;
        }
    }
}

class P2BahamutsFavorWingsOfSalvation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingsOfSalvation, 4f);
