namespace BossMod.Components;

// generic 'twister' component: a set of aoes that appear under players, but can't be accurately predicted until it's too late
// normally you'd predict them at the end (or slightly before the end) of some cast, or on component creation
public class GenericTwister(BossModule module, float radius, uint oid, Enum? aid = default) : GenericAOEs(module, aid, "GTFO from twister!")
{
    private readonly AOEShapeCircle _shape = new(radius);
    private readonly uint _twisterOID = oid;
    public readonly float Radius = radius;
    protected IReadOnlyList<Actor> Twisters = module.Enemies(oid);
    protected DateTime PredictedActivation;
    protected List<WPos> PredictedPositions = [];

    public IEnumerable<Actor> ActiveTwisters => Twisters.Where(v => v.EventState != 7);
    public bool Active => ActiveTwisters.Any();

    public void AddPredicted(float activationDelay) => AddPredicted(WorldState.FutureTime(activationDelay));

    public void AddPredicted(DateTime activationTime)
    {
        PredictedPositions.Clear();
        PredictedPositions.AddRange(Raid.WithoutSlot().Select(a => a.Position));
        PredictedActivation = activationTime;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in PredictedPositions)
            yield return new(_shape, p, default, PredictedActivation);
        foreach (var p in ActiveTwisters)
            yield return new(_shape, p.Position);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == _twisterOID)
            PredictedPositions.Clear();
    }
}

// twister that activates on cast end, or slightly before
public class CastTwister(BossModule module, float radius, uint oid, Enum aid, float spawnDelay, float predictBeforeSpawn = 0) : GenericTwister(module, radius, oid, aid)
{
    public readonly float SpawnDelay = spawnDelay; // from cast event to twister spawn
    public readonly float PredictionTime = predictBeforeSpawn;
    private DateTime _predictAt = DateTime.MaxValue;
    private DateTime _spawnAt;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var p in PredictedPositions)
            hints.AddForbiddenZone(ShapeDistance.Circle(p, Radius * 3), PredictedActivation);
        foreach (var p in ActiveTwisters)
            hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, Radius));
    }

    public override void Update()
    {
        if (PredictedPositions.Count == 0 && Twisters.Count == 0 && WorldState.CurrentTime >= _predictAt)
        {
            AddPredicted(_spawnAt);
            _predictAt = DateTime.MaxValue;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _spawnAt = Module.CastFinishAt(spell, SpawnDelay);
            _predictAt = _spawnAt.AddSeconds(-PredictionTime);
        }
    }
}
