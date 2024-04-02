namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class ParhelicCircle : Components.GenericAOEs
{
    private const float _triRadius = 8;
    private const float _hexRadius = 17;
    private static readonly AOEShapeCircle _circle = new(6);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
        {
            // there are 10 orbs: 1 in center, 3 in vertices of a triangle with radius=8, 6 in vertices of a hexagon with radius=17
            // note: i'm not sure how exactly orientation is determined, it seems to be related to eventobj rotations...
            var hex = module.Enemies(OID.RefulgenceHexagon).FirstOrDefault();
            var tri = module.Enemies(OID.RefulgenceTriangle).FirstOrDefault();
            if (hex != null && tri != null)
            {
                var c = module.Bounds.Center;
                yield return new(_circle, c, activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation + 60.Degrees()).ToDirection(), activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation + 180.Degrees()).ToDirection(), activation: _activation);
                yield return new(_circle, c + _triRadius * (tri.Rotation - 60.Degrees()).ToDirection(), activation: _activation);
                yield return new(_circle, c + _hexRadius * hex.Rotation.ToDirection(), activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 60.Degrees()).ToDirection(), activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 120.Degrees()).ToDirection(), activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation + 180.Degrees()).ToDirection(), activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation - 120.Degrees()).ToDirection(), activation: _activation);
                yield return new(_circle, c + _hexRadius * (hex.Rotation - 60.Degrees()).ToDirection(), activation: _activation);
            }
        }
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.RefulgenceHexagon)
            _activation = module.WorldState.CurrentTime.AddSeconds(8.9f);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
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
