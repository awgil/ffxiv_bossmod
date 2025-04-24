namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class EyeOfTheStorm(BossModule module) : Components.GenericAOEs(module, AID.EyeOfTheStorm)
{
    private Actor? _caster;
    private DateTime _nextCastAt;
    private static readonly AOEShapeDonut _shape = new(12, 25); // TODO: verify inner radius

    public bool Active() => _caster?.CastInfo != null || _nextCastAt > WorldState.CurrentTime;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster != null)
            yield return new(_shape, _caster.Position, new(), _nextCastAt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _caster = caster;
            _nextCastAt = Module.CastFinishAt(caster.CastInfo!);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _nextCastAt = WorldState.FutureTime(4.2f);
        }
    }
}
