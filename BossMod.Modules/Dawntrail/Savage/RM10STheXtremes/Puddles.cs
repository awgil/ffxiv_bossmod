namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

abstract class FlamePuddle(BossModule module, AID[] actions, AOEShape shape, OID oid, bool originAtTarget = false, float spawnDelay = 1.2f, float activationDelay = 1.2f) : Components.GenericAOEs(module, null, "GTFO from fire!")
{
    public readonly AOEShape Shape = shape;
    public readonly float SpawnDelay = spawnDelay;
    public readonly float ActivationDelay = activationDelay;
    public readonly OID ObjectID = oid;
    public readonly List<ActionID> IDs = [.. actions.Select(ActionID.MakeSpell)];

    protected readonly List<AOEInstance> _predicted = [];
    protected readonly List<(Actor Actor, DateTime Spawn)> _actual = [];

    public int NumActors => _actual.Count;

    protected FlamePuddle(BossModule module, AID action, AOEShape shape, OID oid, bool originAtTarget = false, float spawnDelay = 1.2f, float activationDelay = 1.2f) : this(module, [action], shape, oid, originAtTarget, spawnDelay, activationDelay) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in _predicted)
            yield return p;

        foreach (var (p, spawn) in _actual)
            yield return new(Shape, p.Position, p.Rotation, Activation: spawn.AddSeconds(ActivationDelay));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (IDs.Contains(spell.Action))
            _predicted.Add(new(Shape, originAtTarget ? WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ : caster.Position, spell.Rotation, Activation: WorldState.FutureTime(SpawnDelay + ActivationDelay)));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == ObjectID)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            _actual.Add((actor, WorldState.CurrentTime));
        }
    }

    public override void Update()
    {
        _actual.RemoveAll(p => p.Actor.EventState == 7);
    }
}
