namespace BossMod.Stormblood.Ultimate.UCOB;

class P2HugNael(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(Module.Enemies(OID.NaelDeusDarnus).FirstOrDefault()) is { } nael && nael.Actor.IsTargetable)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(nael.Actor.Position, 10, 0.5f));
            // we prefer to keep nael center to give casters/ranged the most options when trying to spread during mechanics
            nael.DesiredPosition = Arena.Center;

            if (nael.Actor.HPMP.CurHP == 1)
                nael.ShouldBeTargeted = true;
        }
    }
}

class P2BahamutsFavorFireball(BossModule module) : Components.UniformStackSpread(module, 4, 0, 1)
{
    public Actor? Target;
    private BitMask _fire;
    private BitMask _ice;
    private DateTime _activation;

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

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FireballP2)
        {
            Stacks.Clear();
            Target = null;
            _activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsStackTarget(actor) && ((UCOB)Module).Nael() is { } nael)
        {
            if (FireOut)
                hints.GoalZonesEnabled = false;
            else
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(nael.Position, 5), Stacks[0].Activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

// note: if player dies immediately after chain lightning cast, he won't get a status or have aoe cast; if he dies after status application, aoe will be triggered immediately
class P2BahamutsFavorChainLightning(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true)
{
    private BitMask _pendingTargets;
    private DateTime _expectedStatuses;

    public bool FirstSet;

    public bool ActiveOrSkipped() => Active || _pendingTargets.Any() && WorldState.CurrentTime >= _expectedStatuses && Raid.WithSlot(true).IncludedInMask(_pendingTargets).All(ip => ip.Item2.IsDead);

    public override void OnStatusGain(Actor actor, in ActorStatus status)
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!EnableHints)
            return;

        if (IsSpreadTarget(actor))
        {
            hints.GoalZonesEnabled = false;

            if (FirstSet)
            {
                var ordered = Service.Config.Get<PartyRolesConfig>().AssignmentsPerSlot(Raid);
                var myOrder = Spreads.OrderBy(s => ordered[Raid.FindSlot(s.Target.InstanceID)]).Index().First(s => s.Item.Target == actor).Index;
                var myDir = myOrder == 0 ? -45.Degrees() : 45.Degrees();
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(((UCOB)Module).Nael()!.Position + myDir.ToDirection() * 5, 1), Spreads[0].Activation);
                return;
            }

            // avoid doom cleanse puddles (unless we are doomed, in which case ignore them)
            // note no activation time specified here, we want to clear the path to the puddle ASAP for other players since they won't want to walk through lightning aoe
            if (!(actor.FindStatus(SID.Doom)?.ExpireAt < Spreads[0].Activation.AddSeconds(1)))
                foreach (var p in Module.Enemies(OID.VoidzoneSalvation).Where(e => e.EventState != 7))
                    hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, 1 + SpreadRadius + ExtraAISpreadThreshold));
        }

        foreach (var sp in ActiveSpreadTargets.Exclude(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(sp.Position, Spreads[0].Radius + ExtraAISpreadThreshold), Spreads[0].Activation);
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
                // despite our best efforts, it's possible that a wings puddle can spawn on top of the cleanse puddle
                var isCovered = Module.FindComponent<P2BahamutsFavorWingsOfSalvation>()?.ActiveAOEs(slot, actor).Any(a => pos.Value.InCircle(a.Origin, 4)) == true;
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos.Value, 1), isCovered ? default : d.Expiration.AddSeconds(-0.5f));
            }
            else
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(pos.Value, 1));
                // encourage non-dooms to bait next puddle away
                hints.AddForbiddenZone(ShapeDistance.Circle(pos.Value, 5), WorldState.FutureTime(2));
            }
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
