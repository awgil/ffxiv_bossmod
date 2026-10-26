namespace BossMod.Stormblood.Ultimate.UCOB;

class P2HugNael(BossModule module) : BossComponent(module)
{
    public enum Pos
    {
        DalamudDivePreTargetable,
        QuoteChainLightning1,
        Dooms1,
        Quote2,
        Dooms2,
        Dooms3,
        Divebomb,
        Standard
    }

    public Pos Mode = Pos.DalamudDivePreTargetable;

    WPos? DesiredPositionOverride;
    Angle? LastWings;

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"P2HugNael mode: {Mode}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Enemies(OID.NaelDeusDarnus).FirstOrDefault() is not { } nael)
            return;

        var quote = Module.FindComponent<Quote>();

        if (hints.FindEnemy(nael) is { } ne)
        {
            // we prefer to keep nael center to give casters/ranged the most options when trying to spread during mechanics
            ne.DesiredPosition = DesiredPositionOverride ?? Arena.Center;

            // at end of p2, party should target pointless nael to build gauge
            if (nael.HPMP.CurHP == 1)
                ne.ShouldBeTargeted = true;
        }

        switch (Mode)
        {
            // 0, -15 is her spawn point
            case Pos.DalamudDivePreTargetable:
                if (!nael.IsTargetable && !nael.Position.AlmostEqual(new(0, -15), 1))
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(nael.Position, nael.HitboxRadius));
                break;

            case Pos.QuoteChainLightning1:
                hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center + new WDir(0, 5), 1, 0.2f));
                break;

            case Pos.Quote2:
                var closeSpot = new WDir(0, 3);
                if (LastWings != null)
                    closeSpot = closeSpot.Rotate(LastWings.Value + 120.Degrees());

                hints.GoalZones.Add(AIHints.GoalSingleTarget(nael.Position + closeSpot, 1, 0.2f));

                // if current quote is chariot, everyone stack south
                hints.GoalZones.Add(AIHints.GoalSingleTarget(nael.Position + new WDir(0, 9), 1, 0.2f));
                break;

            // nothing special here since there are no concurrent mechanics happening
            case Pos.Dooms1:
                hints.GoalZones.Add(AIHints.GoalSingleTarget(nael.Position, nael.HitboxRadius + 0.5f, 0.1f));
                break;

            // party hugs Z axis along nael so that chain lightning and fireball spreaders can spread e/w
            case Pos.Standard:
                var rect = ShapeDistance.Rect(nael.Position, 0.Degrees(), nael.HitboxRadius + 3.5f, nael.HitboxRadius + 3.5f, 2);
                hints.GoalZones.Add(p => rect(p) <= 0 ? 0.1f : 0);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WingsOfSalvation && Module.Enemies(OID.NaelDeusDarnus).FirstOrDefault() is { } nael)
            LastWings = (spell.LocXZ - nael.Position).ToAngle();
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        base.OnStatusGain(actor, status);

        if ((SID)status.ID == SID.Thunderstruck && Module.Enemies(OID.NaelDeusDarnus).FirstOrDefault() is { } nael)
            DesiredPositionOverride = nael.Position;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.ChainLightningAOE)
            DesiredPositionOverride = null;
    }
}

class P2BahamutsFavorFireball(BossModule module) : Components.StrictUniformStackSpread(module, 4, 0, 1)
{
    public Actor? Target;
    private BitMask _fire;
    private BitMask _ice;
    private DateTime _activation;
    private Angle LastWingsCast;

    public bool FireOut;

    public void Show()
    {
        if (Target != null)
            AddStack(Target, _activation, Forbidden);
    }

    BitMask Forbidden => FireOut ? ~_ice : _fire;

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Firescorched)
        {
            _fire.Set(Raid.FindSlot(actor.InstanceID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = Forbidden;
        }

        if ((SID)status.ID == SID.Icebitten)
        {
            _ice.Set(Raid.FindSlot(actor.InstanceID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = Forbidden;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Firescorched)
        {
            _fire.Clear(Raid.FindSlot(actor.InstanceID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = Forbidden;
        }

        if ((SID)status.ID == SID.Icebitten)
        {
            _ice.Clear(Raid.FindSlot(actor.InstanceID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = Forbidden;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Fireball)
        {
            Target = WorldState.Actors.Find(tether.Target);
            _activation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.WingsOfSalvation && ((UCOB)Module).Nael() is { } nael)
            LastWingsCast = Angle.FromDirection(spell.LocXZ - nael.Position);
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

    public override (WPos, float)? GetStackDestination(int order, Actor target)
    {
        var nael = ((UCOB)Module).Nael()!;

        var dist = nael.TargetID == target.InstanceID ? nael.HitboxRadius + 0.5f : 5.5f;

        var angle = FireOut ? LastWingsCast + 90.Degrees() : LastWingsCast + 180.Degrees();

        return (nael.Position + angle.ToDirection() * dist, 0);
    }
}

// note: if player dies immediately after chain lightning cast, he won't get a status or have aoe cast; if he dies after status application, aoe will be triggered immediately
class P2BahamutsFavorChainLightning : Components.StrictUniformStackSpread
{
    private BitMask _pendingTargets;
    private DateTime _expectedStatuses;

    public bool FirstSet;
    Angle RelativeSouth;

    public P2BahamutsFavorChainLightning(BossModule module) : base(module, 0, 5, alwaysShowSpreads: true)
    {
        ExtraAISpreadThreshold = 0;
    }

    public bool ActiveOrSkipped() => Active || _pendingTargets.Any() && WorldState.CurrentTime >= _expectedStatuses && Raid.WithSlot(true).IncludedInMask(_pendingTargets).All(ip => ip.Item2.IsDead);

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Thunderstruck)
        {
            AddSpread(actor, status.ExpireAt);
            Spreads.SortBy(s => s.Target.InstanceID);
            _pendingTargets.Reset();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.WingsOfSalvation && ((UCOB)Module).Nael() is { } nael)
            RelativeSouth = (spell.LocXZ - nael.Position).ToAngle();
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

    public override (WPos, float)? GetSpreadDestination(int order, Actor target)
    {
        var nael = ((UCOB)Module).Nael()!;
        var sign = order == 0 ? -1 : 1;

        if (FirstSet)
            return (nael.Position + (135 * sign).Degrees().ToDirection() * 5, 1);

        // during later sets, a wings puddle is active during this mechanic, so we have to ensure tank is dodging it
        if (nael.TargetID == target.InstanceID)
        {
            var dir = (RelativeSouth + 180.Degrees()).ToDirection();
            var posIdeal = nael.Position + dir * (nael.HitboxRadius + 0.5f);

            // worst case scenario, puddle has been baited so closely onto nael that max pull range is still too close, just pull her farther
            if (Module.FindComponent<P2BahamutsFavorWingsOfSalvation>()?.ActiveAOEs(0, target).Any(a => posIdeal.InCircle(a.Origin, 4.5f)) == true)
                posIdeal = nael.Position + dir * 5;

            return (posIdeal, 0);
        }

        return (nael.Position + ((90 * sign).Degrees() + RelativeSouth).ToDirection() * 5.5f, 0);
    }
}

class P2BahamutsFavorDeathstorm(BossModule module) : BossComponent(module)
{
    public int NumDeathstorms { get; private set; }

    class Doom
    {
        public required Actor Player;
        public required DateTime Expiration;
        public int Order;
        public bool Cleansed;
        public WPos? ZonePredicted;
        public Actor? Voidzone;
    }

    private readonly List<Doom> _dooms = [];
    private readonly Doom?[] _doomArray = new Doom?[PartyState.MaxPartySize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_doomArray[slot] is { } d && !d.Cleansed)
            hints.Add($"Doom {d.Order + 1}", false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_doomArray[pcSlot] is { Cleansed: false } d)
        {
            var pos = d.Voidzone?.Position ?? d.ZonePredicted;
            if (pos != null)
                Arena.AddCircle(pos.Value, 1, ArenaColor.Safe);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var d in _dooms.Where(d => !d.Cleansed && d.Player != pc))
        {
            var pos = d.Voidzone?.Position ?? d.ZonePredicted;
            if (pos == null)
                continue;

            Arena.ZoneCircle(pos.Value, 1, ArenaColor.AOE);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var d in _dooms.Where(d => !d.Cleansed))
        {
            var pos = d.Voidzone?.Position ?? d.ZonePredicted;
            if (pos == null)
                continue;

            if (d.Player == actor)
            {
                if (actor.FindStatus(SID.Thunderstruck)?.ExpireAt.AddSeconds(0.5f) < d.Expiration)
                    return;

                var deadline = d.Expiration.AddSeconds(-0.5f);
                if (Module.FindComponent<P2BahamutsFavorWingsOfSalvation>()?.Check(slot, actor, pos.Value) == true)
                    deadline = default;

                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos.Value, 1), deadline);
            }
            else
                hints.AddForbiddenZone(ShapeDistance.Circle(pos.Value, 1));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.VoidzoneSalvation)
        {
            var d = _dooms.FirstOrDefault(d => d.Voidzone == null);
            if (d != null)
                d.Voidzone = actor;
            else
                ReportError($"Failed to find voidzone predicted pos for {actor}");
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
        {
            var d = new Doom()
            {
                Player = actor,
                Expiration = status.ExpireAt
            };

            _dooms.Add(d);
            _dooms.SortBy(d => d.Expiration);
            for (var i = 0; i < _dooms.Count; i++)
                _dooms[i].Order = i;

            if (Raid.TryFindSlot(actor, out var slot))
                _doomArray[slot] = d;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
        {
            var d = _dooms.FirstOrDefault(d => d.Player == actor);
            if (d != null)
                d.Cleansed = true;
            else
                ReportError($"Failed to find doom on {actor}");
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WingsOfSalvation)
        {
            var d = _dooms.FirstOrDefault(d => d.ZonePredicted == null);
            if (d != null)
                d.ZonePredicted = spell.LocXZ;
            else
                ReportError($"No spare dooms for puddle at {spell.LocXZ}");
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Deathstorm)
        {
            _dooms.Clear();
            Array.Fill(_doomArray, null);
            ++NumDeathstorms;
        }
    }
}

// TODO: we need everyone to spread away from non-expired puddles while baits are active
class P2BahamutsFavorWingsOfSalvation(BossModule module) : Components.StandardAOEs(module, AID.WingsOfSalvation, 4);
