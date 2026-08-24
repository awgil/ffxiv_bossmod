namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class ParhelicCircle(BossModule module) : Components.GenericAOEs(module)
{
    private const float _triRadius = 8;
    private const float _hexRadius = 17;
    private static readonly AOEShapeCircle _circle = new(6);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            // there are 10 orbs: 1 in center, 3 in vertices of a triangle with radius=8, 6 in vertices of a hexagon with radius=17
            // note: i'm not sure how exactly orientation is determined, it seems to be related to eventobj rotations...
            var hex = Module.Enemies(OID.RefulgenceHexagon).FirstOrDefault();
            var tri = Module.Enemies(OID.RefulgenceTriangle).FirstOrDefault();
            if (hex != null && tri != null)
            {
                var c = Module.Center;
                yield return new(_circle, c, Activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation + 60.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation + 180.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation - 60.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * hex.Rotation.ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 60.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 120.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 180.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation - 120.Degrees()).ToDirection(), Activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation - 60.Degrees()).ToDirection(), Activation: _activation);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.RefulgenceHexagon)
            _activation = WorldState.FutureTime(8.9f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Incandescence)
            ++NumCasts;
        if (NumCasts == 10)
        {
            NumCasts = 0;
            _activation = default;
        }
    }
}
