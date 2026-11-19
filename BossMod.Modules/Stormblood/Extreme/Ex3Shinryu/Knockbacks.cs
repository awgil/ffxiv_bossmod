namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWave, 35, kind: Kind.DirForward)
{
    public override PointTest DestinationUnsafe(int slot, Actor actor) => Module.FindComponent<ArenaChanges>() is { } arena ? arena.Broken : PointTest.Never;
}

class AerialBlast(BossModule module) : Components.Knockback(module, AID.AerialBlast)
{
    public override PointTest DestinationUnsafe(int slot, Actor actor) => Module.FindComponent<ArenaChanges>() is { } arena ? arena.Broken : PointTest.Never;

    private WPos _origin;
    private DateTime _activation;
    private DateTime _positionDeadline;
    private int _smallPushes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AerialBlast)
        {
            _origin = caster.Position;
            _activation = Module.CastFinishAt(spell, 1.6f);
            _positionDeadline = Module.CastFinishAt(spell, -6.3f);
            _smallPushes = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.SuperCyclone && _activation != default)
            _positionDeadline = ++_smallPushes < 3 ? WorldState.FutureTime(2.1f) : _activation;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default && WorldState.CurrentTime <= _activation)
            yield return new(_origin, 15, _activation);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default || WorldState.CurrentTime > _activation.AddSeconds(0.5f) || IsImmune(slot, _activation))
            return;
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_origin, 10), _positionDeadline);
        if (Module.FindComponent<ArenaChanges>() is not { } arena)
            return;
        var origin = _origin;
        hints.AddForbiddenZone(Sdf.Discrete(p => arena.Broken(AwayFromSource(p, origin, 15))), _positionDeadline);
    }
}
