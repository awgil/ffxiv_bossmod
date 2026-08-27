// TODO: revise and refactor voidzone components; 'persistent' part of name is redundant
namespace BossMod.Components;

// voidzone (circle aoe that stays active for some time) centered at each existing object with specified OID, assumed to be persistent voidzone center
// for moving 'voidzones', the hints can mark the area in front of each source as dangerous
// TODO: typically sources are either eventobj's with eventstate != 7 or normal actors that are non dead; other conditions are much rarer
public class Voidzone(BossModule module, float radius, uint oid, Func<Actor, bool>? isDeactivated = null, float moveHintLength = 0) : GenericAOEs(module, default, "GTFO from voidzone!")
{
    public Voidzone(BossModule module, float radius, Enum oid, Func<Actor, bool>? isDeactivated = null, float moveHintLength = 0) : this(module, radius, (uint)(object)oid, isDeactivated, moveHintLength) { }

    public AOEShapeCircle Shape { get; init; } = new(radius);
    public IReadOnlyList<Actor> Sources => _sources;
    private readonly List<Actor> _sources = [];
    public readonly uint ID = oid;
    protected Func<Actor, bool>? IsDeactivated = isDeactivated;
    public float MoveHintLength = moveHintLength;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Sources.Select(s => new AOEInstance(Shape, s.Position));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources)
        {
            hints.AddForbiddenZone(Shape.Distance(s.Position, s.Rotation));
            if (MoveHintLength > 0)
                hints.AddForbiddenZone(ShapeDistance.Capsule(s.Position, s.Rotation, MoveHintLength, Shape.Radius), WorldState.FutureTime(2));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == ID)
            AddSource(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        RemoveSource(actor);
    }

    public override void Update()
    {
        if (IsDeactivated != null)
            _sources.RemoveAll(IsDeactivated.Invoke);
    }

    protected void AddSource(Actor a) => _sources.Add(a);
    protected void RemoveSource(Actor a) => _sources.Remove(a);
}

// voidzone that appears with some delay at cast target
// note that if voidzone is predicted by cast start rather than cast event, we have to account for possibility of cast finishing without event (e.g. if actor dies before cast finish)
// TODO: this has problems when target moves - castevent and spawn position could be quite different
// TODO: this has problems if voidzone never actually spawns after castevent, eg because of phase changes
public class VoidzoneAtCastTarget(BossModule module, float radius, Enum aid, uint oid, float castEventToSpawn, Func<Actor, bool>? isDeactivated = null, float castEventTimeout = float.MaxValue) : GenericAOEs(module, aid, "GTFO from voidzone!")
{
    public VoidzoneAtCastTarget(BossModule module, float radius, Enum aid, Enum oid, float castEventToSpawn, Func<Actor, bool>? isDeactivated = null, float castEventTimeout = float.MaxValue) : this(module, radius, aid, (uint)(object)oid, castEventToSpawn, isDeactivated, castEventTimeout) { }

    public float Radius = radius;
    public AOEShapeCircle Shape { get; init; } = new(radius);
    public IReadOnlyList<Actor> Sources => _sources;
    private readonly List<Actor> _sources = [];
    public readonly uint ID = oid;
    protected Func<Actor, bool>? IsDeactivated = isDeactivated;
    public float CastEventToSpawn { get; init; } = castEventToSpawn;
    protected readonly List<(WPos pos, DateTime time)> _predictedByEvent = [];
    private readonly List<(Actor caster, DateTime time)> _predictedByCast = [];

    public bool HaveCasters => _predictedByCast.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in _predictedByEvent)
            yield return new(Shape, p.pos, Activation: p.time);
        foreach (var p in _predictedByCast)
            yield return new(Shape, WorldState.Actors.Find(p.caster.CastInfo!.TargetID)?.Position ?? p.caster.CastInfo.LocXZ, Activation: p.time);
        foreach (var z in Sources)
            yield return new(Shape, z.Position);
    }

    public override void Update()
    {
        if (castEventTimeout < float.MaxValue)
            _predictedByEvent.RemoveAll(e => e.time.AddSeconds(castEventTimeout) < WorldState.CurrentTime);

        _sources.RemoveAll(a => a.EventState == 7 || IsDeactivated?.Invoke(a) == true);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predictedByCast.Add((caster, Module.CastFinishAt(spell, CastEventToSpawn)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predictedByCast.RemoveAll(p => p.caster == caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            _predictedByEvent.Add((WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ, WorldState.FutureTime(CastEventToSpawn)));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == ID)
            AddSource(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        RemoveSource(actor);
    }

    protected void AddSource(Actor a)
    {
        _sources.Add(a);

        if (_predictedByEvent.Count > 0)
            _predictedByEvent.RemoveAt(0);
    }
    protected void RemoveSource(Actor a) => _sources.Remove(a);
}

// these are normal voidzones that could be 'inverted' (e.g. when you need to enter a voidzone at specific time to avoid some mechanic)
// TODO: i'm not sure whether these should be considered actual voidzones (if so, should i merge them with base component? what about cast prediction?) or some completely other type of mechanic (maybe drawing differently)
// TODO: might want to have per-player invertability
public class InvertibleVoidzone(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, Enum? aid = default) : CastCounter(module, aid)
{
    public AOEShapeCircle Shape { get; init; } = new(radius);
    public Func<BossModule, IEnumerable<Actor>> Sources { get; init; } = sources;
    public DateTime InvertResolveAt;

    public bool Inverted => InvertResolveAt != default;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var inVoidzone = Sources(Module).Any(s => Shape.Check(actor.Position, s));
        if (Inverted)
            hints.Add(inVoidzone ? "Stay in voidzone" : "Go to voidzone!", !inVoidzone);
        else if (inVoidzone)
            hints.Add("GTFO from voidzone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shapes = Sources(Module).Select(s => Shape.Distance(s.Position, s.Rotation)).ToList();
        if (shapes.Count == 0)
            return;

        float distance(WPos p)
        {
            var dist = shapes.Min(s => s(p));
            return Inverted ? -dist : dist;
        }
        hints.AddForbiddenZone(distance, InvertResolveAt);
    }

    // TODO: reconsider - draw foreground circles instead?
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var color = Inverted ? ArenaColor.SafeFromAOE : ArenaColor.AOE;
        foreach (var s in Sources(Module))
            Shape.Draw(Arena, s.Position, s.Rotation, color);
    }
}

// invertible voidzone that is inverted when specific spell is being cast; resolved when cast ends
public class InvertibleVoidzoneByCast(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, Enum aid) : InvertibleVoidzone(module, radius, sources, aid)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            InvertResolveAt = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            InvertResolveAt = default;
    }
}
