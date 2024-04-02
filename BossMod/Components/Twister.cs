namespace BossMod.Components;

// generic 'twister' component: a set of aoes that appear under players, but can't be accurately predicted until it's too late
// normally you'd predict them at the end (or slightly before the end) of some cast, or on component creation
public class GenericTwister : GenericAOEs
{
    private AOEShapeCircle _shape;
    private uint _twisterOID;
    protected DateTime PredictedActivation;
    protected List<WPos> PredictedPositions = new();
    protected IReadOnlyList<Actor> Twisters = ActorEnumeration.EmptyList;

    public IEnumerable<Actor> ActiveTwisters => Twisters.Where(v => v.EventState != 7);
    public bool Active => ActiveTwisters.Count() > 0;

    public GenericTwister(float radius, uint oid, ActionID aid = default) : base(aid, "GTFO from twister!")
    {
        _shape = new(radius);
        _twisterOID = oid;
    }

    public void AddPredicted(BossModule module, float activationDelay)
    {
        PredictedPositions.Clear();
        PredictedPositions.AddRange(module.Raid.WithoutSlot().Select(a => a.Position));
        PredictedActivation = module.WorldState.CurrentTime.AddSeconds(activationDelay);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var p in PredictedPositions)
            yield return new(_shape, p, default, PredictedActivation);
        foreach (var p in ActiveTwisters)
            yield return new(_shape, p.Position);
    }

    public override void Init(BossModule module)
    {
        Twisters = module.Enemies(_twisterOID);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if (actor.OID == _twisterOID)
            PredictedPositions.Clear();
    }
}

// twister that activates immediately on init
public class ImmediateTwister : GenericTwister
{
    private float _activationDelay;

    public ImmediateTwister(float radius, uint oid, float activationDelay) : base(radius, oid)
    {
        _activationDelay = activationDelay;
    }

    public override void Init(BossModule module)
    {
        base.Init(module);
        AddPredicted(module, _activationDelay);
    }
}

// twister that activates on cast end, or slightly before
public class CastTwister : GenericTwister
{
    private float _activationDelay; // from cast-end to twister spawn
    private float _predictBeforeCastEnd;
    private DateTime _predictStart = DateTime.MaxValue;

    public CastTwister(float radius, uint oid, ActionID aid, float activationDelay, float predictBeforeCastEnd = 0) : base(radius, oid, aid)
    {
        _activationDelay = activationDelay;
        _predictBeforeCastEnd = predictBeforeCastEnd;
    }

    public override void Update(BossModule module)
    {
        if (PredictedPositions.Count == 0 && Twisters.Count == 0 && module.WorldState.CurrentTime >= _predictStart)
        {
            AddPredicted(module, _predictBeforeCastEnd + _activationDelay);
            _predictStart = DateTime.MaxValue;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _predictStart == DateTime.MaxValue)
        {
            _predictStart = spell.NPCFinishAt.AddSeconds(-_predictBeforeCastEnd);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _predictStart < DateTime.MaxValue)
        {
            // cast finished earlier than expected, just activate things now
            AddPredicted(module, _activationDelay);
            _predictStart = DateTime.MaxValue;
        }
    }
}
