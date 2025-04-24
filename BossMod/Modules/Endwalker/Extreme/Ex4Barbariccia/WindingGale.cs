namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

// TODO: not sure how 'spiral arms' are really implemented
class WindingGale(BossModule module) : Components.GenericAOEs(module, AID.WindingGale)
{
    private readonly List<Actor> _casters = [];

    private static readonly AOEShapeDonutSector _shape = new(9, 11, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(_shape, c.Position + _shape.OuterRadius * c.Rotation.ToDirection(), c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }
}
