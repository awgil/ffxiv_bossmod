// TODO: revise and refactor voidzone components; 'persistent' part of name is redundant
namespace BossMod.Components;

// voidzone (circle aoe that stays active for some time) centered at each existing object with specified OID, assumed to be persistent voidzone center
// TODO: typically sources are either eventobj's with eventstate != 7 or normal actors that are non dead; other conditions are much rarer
public class PersistentVoidzone : GenericAOEs
{
    public AOEShapeCircle Shape { get; private init; }
    public Func<BossModule, IEnumerable<Actor>> Sources { get; private init; }

    public PersistentVoidzone(float radius, Func<BossModule, IEnumerable<Actor>> sources) : base(new(), "GTFO from voidzone!")
    {
        Shape = new(radius);
        Sources = sources;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var s in Sources(module))
            yield return new(Shape, s.Position);
    }
}

// voidzone that appears with some delay at cast target
// note that if voidzone is predicted by cast start rather than cast event, we have to account for possibility of cast finishing without event (e.g. if actor dies before cast finish)
// TODO: this has problems when target moves - castevent and spawn position could be quite different
// TODO: this has problems if voidzone never actually spawns after castevent, eg because of phase changes
public class PersistentVoidzoneAtCastTarget : GenericAOEs
{
    public AOEShapeCircle Shape { get; private init; }
    public Func<BossModule, IEnumerable<Actor>> Sources { get; private init; }
    public float CastEventToSpawn { get; private init; }
    private List<(WPos pos, DateTime time)> _predictedByEvent = new();
    private List<(Actor caster, DateTime time)> _predictedByCast = new();

    public bool HaveCasters => _predictedByCast.Count > 0;

    public PersistentVoidzoneAtCastTarget(float radius, ActionID aid, Func<BossModule, IEnumerable<Actor>> sources, float castEventToSpawn) : base(aid, "GTFO from voidzone!")
    {
        Shape = new(radius);
        Sources = sources;
        CastEventToSpawn = castEventToSpawn;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var p in _predictedByEvent)
            yield return new(Shape, p.pos, activation: p.time);
        foreach (var p in _predictedByCast)
            yield return new(Shape, module.WorldState.Actors.Find(p.caster.CastInfo!.TargetID)?.Position ?? p.caster.CastInfo.LocXZ, activation: p.time);
        foreach (var z in Sources(module))
            yield return new(Shape, z.Position);
    }

    public override void Update(BossModule module)
    {
        if (_predictedByEvent.Count > 0)
            foreach (var s in Sources(module))
                _predictedByEvent.RemoveAll(p => p.pos.InCircle(s.Position, 3));
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predictedByCast.Add((caster, spell.NPCFinishAt.AddSeconds(CastEventToSpawn)));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predictedByCast.RemoveAll(p => p.caster == caster);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if (spell.Action == WatchedAction)
            _predictedByEvent.Add((module.WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ, module.WorldState.CurrentTime.AddSeconds(CastEventToSpawn)));
    }
}

// these are normal voidzones that could be 'inverted' (e.g. when you need to enter a voidzone at specific time to avoid some mechanic)
// TODO: i'm not sure whether these should be considered actual voidzones (if so, should i merge them with base component? what about cast prediction?) or some completely other type of mechanic (maybe drawing differently)
// TODO: might want to have per-player invertability
public class PersistentInvertibleVoidzone : CastCounter
{
    public AOEShapeCircle Shape { get; private init; }
    public Func<BossModule, IEnumerable<Actor>> Sources { get; private init; }
    public DateTime InvertResolveAt;

    public bool Inverted => InvertResolveAt != default;

    public PersistentInvertibleVoidzone(float radius, Func<BossModule, IEnumerable<Actor>> sources, ActionID aid = default) : base(aid)
    {
        Shape = new(radius);
        Sources = sources;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var inVoidzone = Sources(module).Any(s => Shape.Check(actor.Position, s));
        if (Inverted)
            hints.Add(inVoidzone ? "Stay in voidzone" : "Go to voidzone!", !inVoidzone);
        else if (inVoidzone)
            hints.Add("GTFO from voidzone!");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shapes = Sources(module).Select(s => Shape.Distance(s.Position, s.Rotation)).ToList();
        if (shapes.Count == 0)
            return;

        Func<WPos, float> distance = p =>
        {
            float dist = shapes.Select(s => s(p)).Min();
            return Inverted ? -dist : dist;
        };
        hints.AddForbiddenZone(distance, InvertResolveAt);
    }

    // TODO: reconsider - draw foreground circles instead?
    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var color = Inverted ? ArenaColor.SafeFromAOE : ArenaColor.AOE;
        foreach (var s in Sources(module))
            Shape.Draw(arena, s.Position, s.Rotation, color);
    }
}

// invertible voidzone that is inverted when specific spell is being cast; resolved when cast ends
public class PersistentInvertibleVoidzoneByCast : PersistentInvertibleVoidzone
{
    public PersistentInvertibleVoidzoneByCast(float radius, Func<BossModule, IEnumerable<Actor>> sources, ActionID aid) : base(radius, sources, aid) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            InvertResolveAt = spell.NPCFinishAt;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            InvertResolveAt = default;
    }
}
