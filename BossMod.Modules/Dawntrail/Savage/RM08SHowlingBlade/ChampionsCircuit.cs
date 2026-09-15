namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

abstract class CCRotatingAOE(BossModule module, AOEShape shape, AID castFirst, AID castRest, bool smallDonut = false) : Components.GenericAOEs(module)
{
    public AOEShape Shape { get; init; } = shape;
    public bool SmallDonut { get; init; } = smallDonut;

    private readonly List<Actor> _casters = [];
    private readonly List<AOEInstance> _aoes = [];

    public enum Direction
    {
        None,
        CW,
        CCW
    }

    public Direction CurDirection = Direction.None;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ChampionsCircuitCCW:
                CurDirection = Direction.CCW;
                Start();
                break;
            case AID.ChampionsCircuitCW:
                CurDirection = Direction.CW;
                Start();
                break;
            default:
                if ((AID)spell.Action.ID == castFirst)
                {
                    _casters.Add(caster);
                    Start();
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == castFirst || (AID)spell.Action.ID == castRest)
        {
            NumCasts++;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }

    private void Start()
    {
        if (CurDirection == Direction.None || _casters.Count == 0)
            return;

        var src = SmallDonut ? _casters[0].Position : Module.PrimaryActor.Position;
        var angleNext = SmallDonut ? Angle.FromDirection(src - Arena.Center) : _casters[0].Rotation;
        var nextActivation = Module.CastFinishAt(_casters[0].CastInfo);

        var inc = CurDirection == Direction.CCW ? 72.Degrees() : -72.Degrees();

        for (var i = 0; i < 5; i++)
        {
            _aoes.Add(new AOEInstance(Shape, src, Rotation: SmallDonut ? default : angleNext, nextActivation));
            angleNext += inc;
            nextActivation += TimeSpan.FromSeconds(4.3f);
            if (SmallDonut)
                src = Module.PrimaryActor.Position + angleNext.ToDirection() * 17.5f;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);
}

class CCDonutSmall(BossModule module) : CCRotatingAOE(module, new AOEShapeDonut(4, 13), AID.ChampionsCircuitDonutSmallFirst, AID.ChampionsCircuitDonutSmallRest, true);
class CCDonutLarge1(BossModule module) : CCRotatingAOE(module, new AOEShapeDonutSector(15.85f, 28, 30.Degrees()), AID.ChampionsCircuitDonutLarge1First, AID.ChampionsCircuitDonutLarge1Rest);
class CCDonutLarge2(BossModule module) : CCRotatingAOE(module, new AOEShapeDonutSector(15.85f, 28, 30.Degrees()), AID.ChampionsCircuitDonutLarge2First, AID.ChampionsCircuitDonutLarge2Rest);
class CCCone(BossModule module) : CCRotatingAOE(module, new AOEShapeCone(22, 30.Degrees()), AID.ChampionsCircuitConeFirst, AID.ChampionsCircuitConeRest);
class CCRect(BossModule module) : CCRotatingAOE(module, new AOEShapeRect(30, 6), AID.ChampionsCircuitRectFirst, AID.ChampionsCircuitRectRest);
class GleamingBarrage(BossModule module) : Components.StandardAOEs(module, AID.GleamingBarrage, new AOEShapeRect(31, 4));
