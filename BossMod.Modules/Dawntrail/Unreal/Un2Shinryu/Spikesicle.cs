namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class Spikesicle(BossModule module) : Components.GenericAOEs(module, AID.Spikesicle)
{
    private static readonly AOEShapeRect _shape = new(64.5f, 5);
    private readonly List<(Actor Icicle, DateTime Activation)> _icicles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _icicles.Select(i => new AOEInstance(_shape, i.Icicle.Position, i.Icicle.Rotation, i.Activation));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_icicles.Count > 0 && Module.FindComponent<WaterPuddles>() != null && Module.Enemies(OID.WaterPuddle).Any(p => !p.IsDead && actor.Position.InCircle(p.Position, 5)) && ActiveAOEs(slot, actor).Any(a => a.Check(actor.Position)))
            hints.Add("Icicle knockback will push you out of the water!");
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Icicle)
            _icicles.Add((actor, WorldState.FutureTime(8.8f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var i = _icicles.FindIndex(i => i.Icicle == caster);
            if (i >= 0)
                _icicles[i] = (caster, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _icicles.RemoveAll(i => i.Icicle == caster);
            ++NumCasts;
        }
    }

    public override void Update() => _icicles.RemoveAll(i => i.Icicle.IsDestroyed);
}

class SpikesicleKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Spikesicle, 10, maxCasts: 3, shape: new AOEShapeRect(64.5f, 5), kind: Kind.DirForward)
{
    public override PointTest DestinationUnsafe(int slot, Actor actor) => Module.FindComponent<ArenaChanges>() is { } arena ? arena.Broken : PointTest.Never;
}
