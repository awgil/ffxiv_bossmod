namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class TwofoldTether(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor, Actor)> Tethers = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GenericPassable && WorldState.Actors.Find(tether.Target) is { } target)
            Tethers.Add((source, target));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GenericPassable)
            Tethers.RemoveAll(t => t.Item1 == source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (a, b) in Tethers)
            Arena.AddLine(a.Position, b.Position, ArenaColor.Danger);
    }
}

class TwofoldStack(BossModule module) : Components.GenericStackSpread(module)
{
    private DateTime _activation;
    public int NumFinishedStacks;
    private bool _tetherAppeared;

    private Actor? _target;

    public override void Update()
    {
        Stacks.Clear();
        if (_target is { } t)
        {
            Stacks.Add(new(t, 6, 2, 2, _activation, forbiddenPlayers: Raid.WithSlot().Where(r => r.Item2.FindStatus(SID.MagicVulnerabilityUp)?.ExpireAt > _activation).Mask()));
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GenericPassable && WorldState.Actors.Find(tether.Target) is { } target)
        {
            if (!_tetherAppeared)
            {
                _tetherAppeared = true;
                _activation = WorldState.FutureTime(7.1f);
            }
            _target = target;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TwofoldTempestStack)
        {
            NumFinishedStacks++;
            _activation = WorldState.FutureTime(7.1f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        foreach (var s in ActiveStacks)
            if (P2Platforms.SamePlatform(s.Target, pc))
                Arena.AddCircle(s.Target.Position, 9, ArenaColor.Danger); // visual helper for max voidzone size
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Stacks.Any(s => s.Target == actor && s.ForbiddenPlayers[slot]))
            hints.Add("Pass tether!");
        else
            base.AddHints(slot, actor, hints);
    }
}

class TwofoldLineBait(BossModule module) : Components.CastCounter(module, AID.TwofoldTempestLine)
{
    private DateTime _nextActivation;

    public int NextBait = -1;
    public bool ShowHints;

    private readonly DateTime[] _vulns = new DateTime[PartyState.MaxPartySize];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GenericPassable)
        {
            ShowHints = true;
            if (_nextActivation == default)
                _nextActivation = WorldState.FutureTime(7.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            ShowHints = false;
            _nextActivation = WorldState.FutureTime(7.1f);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _vulns[slot] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _vulns[slot] = default;
    }

    public override void Update()
    {
        if (_nextActivation > WorldState.CurrentTime)
        {
            var closest = Raid.WithoutSlot().Closest(Module.PrimaryActor.Position);
            NextBait = closest == null ? -1 : P2Platforms.GetPlatform(closest);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!ShowHints)
            return;

        if (NextBait >= 0)
        {
            var dir = -72.Degrees() * NextBait;
            Arena.AddRect(Module.PrimaryActor.Position, dir.ToDirection(), 35.5f, 0, 8, ArenaColor.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ShowHints)
            return;

        if (P2Platforms.GetPlatform(actor) == NextBait && _vulns[slot] >= _nextActivation)
            hints.Add("Avoid baiting!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_vulns[slot] >= _nextActivation)
        {
            var nextClosest = Raid.WithoutSlot().OnDifferentPlatform(actor).Closest(Module.PrimaryActor.Position);
            if (nextClosest != null)
            {
                var dist = (nextClosest.Position - Module.PrimaryActor.Position).Length();
                hints.AddForbiddenZone(ShapeContains.Circle(Module.PrimaryActor.Position, dist + 0.5f), _nextActivation);
            }
        }
        else
        {
            hints.AddPredictedDamage(Raid.WithSlot().OnPlatform(NextBait).Mask(), _nextActivation);
        }
    }
}

class TwofoldVoidzone : Components.PersistentVoidzoneAtCastTarget
{
    private readonly List<Actor> _sources = [];

    public TwofoldVoidzone(BossModule module) : base(module, 9, AID.TwofoldTempestStack, m => [], 3.4f)
    {
        Sources = _ => _sources;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TwofoldVoidzone)
            _sources.Add(actor);
    }

    // this is NOT the same as EventState
    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 4)
            _sources.Remove(actor);
    }
}
