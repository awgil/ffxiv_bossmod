namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class ProjectionOfTriumph(BossModule module) : Components.GenericAOEs(module)
{
    private record struct Line(WDir Direction, AOEShape Shape);

    private readonly List<Line> _lines = [];
    private DateTime _nextActivation;

    private static readonly AOEShapeCircle _shapeCircle = new(4);
    private static readonly AOEShapeDonut _shapeDonut = new(2, 8); // TODO: verify inner radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var nextOrder = NextOrder();
        for (int i = 0; i < _lines.Count; ++i)
        {
            var order = i >= 2 ? nextOrder - 2 : nextOrder;
            if (order is >= 0 and < 4)
            {
                var line = _lines[i];
                var lineCenter = Module.Center + (-15 + 10 * order) * line.Direction;
                var ortho = line.Direction.OrthoL();
                for (int j = -15; j <= 15; j += 10)
                {
                    yield return new(line.Shape, lineCenter + j * ortho, default, _nextActivation);
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = (OID)actor.OID switch
        {
            OID.ProjectionOfTriumphCircle => _shapeCircle,
            OID.ProjectionOfTriumphDonut => _shapeDonut,
            _ => null
        };
        if (shape != null)
        {
            _lines.Add(new(actor.Rotation.ToDirection(), shape));
            _nextActivation = WorldState.FutureTime(9);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SiegeOfVollok or AID.WallsOfVollok)
        {
            ++NumCasts;
            _nextActivation = WorldState.FutureTime(5);
        }
    }

    public int NextOrder() => NumCasts switch
    {
        < 8 => 0,
        < 16 => 1,
        < 32 => 2,
        < 48 => 3,
        < 56 => 4,
        < 64 => 5,
        _ => 6
    };
}
