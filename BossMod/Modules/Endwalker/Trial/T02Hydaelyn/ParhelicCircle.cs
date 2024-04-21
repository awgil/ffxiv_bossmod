namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class ParhelicCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _circle = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        var activation = WorldState.FutureTime(8.9f);
        var c = Module.Bounds.Center;
        if ((OID)actor.OID == OID.RefulgenceHexagon)
        {
            _aoes.Add(new(_circle, c, default, activation));
            for (int i = 1; i < 7; ++i)
            _aoes.Add(new(_circle, Helpers.RotateAroundOrigin(i * 60, c, c + 17 * actor.Rotation.ToDirection()), default, activation));
        }
        if ((OID)actor.OID == OID.RefulgenceTriangle)
            for (int i = 1; i < 4; ++i)
                _aoes.Add(new(_circle, Helpers.RotateAroundOrigin(-60 + i * 120, c, c + 8 * actor.Rotation.ToDirection()), default, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Incandescence)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
