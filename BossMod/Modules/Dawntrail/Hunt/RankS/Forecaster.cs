namespace BossMod.Dawntrail.Hunt.RankS.Forecaster;

public enum OID : uint
{
    Boss = 0x4397, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WildfireConditions = 38532, // Boss->self, 4.0s cast, range 5-40 donut
    Hyperelectricity = 38533, // Boss->self, 4.0s cast, range 10 circle
    GaleForceWinds = 38534, // Boss->self, 4.0s cast, range 40 width 40 rect
    BlizzardConditions = 38535, // Boss->self, 4.0s cast, range 40 width 5 cross
    ForecastWHB = 38520, // Boss->self, 7.0s cast, single-target, visual (wildfires + hyperelectricity + blizzard)
    ForecastHGW = 38521, // Boss->self, 7.0s cast, single-target, visual (hyperelectricity + gale-force winds + wildfires)
    ForecastBHG = 38522, // Boss->self, 7.0s cast, single-target, visual (blizzard + hyperelectricity + gale-force winds)
    ForecastGWH = 38523, // Boss->self, 7.0s cast, single-target, visual (gale-force winds + wildfires + hyperelectricity)
    ForecastEnd = 38537, // Boss->self, no cast, single-target, visual (clear statuses)
    WeatherChannelWFirst = 38524, // Boss->self, 5.0s cast, range 5-40 donut
    WeatherChannelWRest = 38525, // Boss->self, no cast, range 5-40 donut
    WeatherChannelHFirst = 38526, // Boss->self, 5.0s cast, range 10 circle
    WeatherChannelHRest = 38527, // Boss->self, no cast, range 10 circle
    WeatherChannelGFirst = 38528, // Boss->self, 5.0s cast, range 40 width 40 rect
    WeatherChannelGRest = 38529, // Boss->self, no cast, range 40 width 40 rect
    WeatherChannelBFirst = 38530, // Boss->self, 5.0s cast, range 40 width 5 cross
    WeatherChannelBRest = 38531, // Boss->self, no cast, range 40 width 5 cross
    ClimateChangeW = 39125, // Boss->self, 3.0s cast, single-target, visual (revises blizzard to wildfires)
    ClimateChangeH = 39126, // Boss->self, 3.0s cast, single-target, visual (revises wildfires to hyperelectricity)
    ClimateChangeG = 39127, // Boss->self, 3.0s cast, single-target, visual (revises hyperelectricity to gale-force winds)
    ClimateChangeB = 39128, // Boss->self, 3.0s cast, single-target, visual (revises gale-force winds to blizzard)
    ClimateChangeEnd = 39133, // Boss->self, no cast, single-target, visual (clear revise-to statuses)
    FloodConditions = 38536, // Boss->location, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    ChanceOfWildfires = 4011, // Boss->Boss, extra=0x0
    ChanceOfHyperelectricity = 4012, // Boss->Boss, extra=0x0
    ChanceOfGaleForceWinds = 4013, // Boss->Boss, extra=0x0
    ChanceOfBlizzards = 4014, // Boss->Boss, extra=0x0
    RevisedToWildfires = 4073, // Boss->Boss, extra=0x0
    RevisedToBlizzards = 4074, // Boss->Boss, extra=0x0
    RevisedToHyperelectricity = 4075, // Boss->Boss, extra=0x0
    RevisedToGaleForceWinds = 4076, // Boss->Boss, extra=0x0
}

class Forecast(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEShape> _shapes = [];
    private Angle _rotation;
    private DateTime _activation;

    private static readonly AOEShapeDonut _shapeWildfire = new(5, 40);
    private static readonly AOEShapeCircle _shapeHyperelectricity = new(10);
    private static readonly AOEShapeRect _shapeGaleForceWinds = new(40, 20);
    private static readonly AOEShapeCross _shapeBlizzard = new(40, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default && _shapes.Count > 0)
            yield return new(_shapes[0], Module.PrimaryActor.Position, _rotation, _activation);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_shapes.Count > 0)
        {
            hints.Add($"Sequence: {string.Join(" -> ", _shapes.Select(s => s switch
            {
                AOEShapeDonut => "In",
                AOEShapeCircle => "Out",
                AOEShapeRect => "Behind",
                AOEShapeCross => "Cross",
                _ => "???"
            }))}");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WildfireConditions:
                StartSimple(_shapeWildfire, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.Hyperelectricity:
                StartSimple(_shapeHyperelectricity, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.GaleForceWinds:
                StartSimple(_shapeGaleForceWinds, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.BlizzardConditions:
                StartSimple(_shapeBlizzard, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.ForecastWHB:
                StartForecast(_shapeWildfire, _shapeHyperelectricity, _shapeBlizzard);
                break;
            case AID.ForecastHGW:
                StartForecast(_shapeHyperelectricity, _shapeGaleForceWinds, _shapeWildfire);
                break;
            case AID.ForecastBHG:
                StartForecast(_shapeBlizzard, _shapeHyperelectricity, _shapeGaleForceWinds);
                break;
            case AID.ForecastGWH:
                StartForecast(_shapeGaleForceWinds, _shapeWildfire, _shapeHyperelectricity);
                break;
            case AID.ClimateChangeW:
                ClimateChange(_shapeBlizzard, _shapeWildfire);
                break;
            case AID.ClimateChangeH:
                ClimateChange(_shapeWildfire, _shapeHyperelectricity);
                break;
            case AID.ClimateChangeG:
                ClimateChange(_shapeHyperelectricity, _shapeGaleForceWinds);
                break;
            case AID.ClimateChangeB:
                ClimateChange(_shapeGaleForceWinds, _shapeBlizzard);
                break;
            case AID.WeatherChannelWFirst:
            case AID.WeatherChannelHFirst:
            case AID.WeatherChannelGFirst:
            case AID.WeatherChannelBFirst:
                _rotation = spell.Rotation;
                _activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.WildfireConditions or AID.WeatherChannelWFirst or AID.WeatherChannelWRest => _shapeWildfire,
            AID.Hyperelectricity or AID.WeatherChannelHFirst or AID.WeatherChannelHRest => _shapeHyperelectricity,
            AID.GaleForceWinds or AID.WeatherChannelGFirst or AID.WeatherChannelGRest => _shapeGaleForceWinds,
            AID.BlizzardConditions or AID.WeatherChannelBFirst or AID.WeatherChannelBRest => _shapeBlizzard,
            _ => null
        };
        if (shape == null || _shapes.Count == 0)
            return;

        if (_shapes[0] != shape)
            ReportError($"Unexpected resolve: expected {_shapes[0]}, got {shape} ({spell.Action})");
        _shapes.RemoveAt(0);
        _activation = _shapes.Count > 0 ? WorldState.FutureTime(3.1f) : default;
    }

    private void StartSimple(AOEShape shape, Angle rotation, DateTime activation)
    {
        if (_shapes.Count > 0)
            ReportError($"Unexpected simple start, {_shapes.Count} pending shapes");
        _shapes.Clear();
        _shapes.Add(shape);
        _rotation = rotation;
        _activation = activation;
    }

    private void StartForecast(AOEShape first, AOEShape second, AOEShape third)
    {
        if (_shapes.Count > 0)
            ReportError($"Unexpected forecast start, {_shapes.Count} pending shapes");
        _shapes.Clear();
        _shapes.Add(first);
        _shapes.Add(second);
        _shapes.Add(third);
    }

    private void ClimateChange(AOEShape from, AOEShape to)
    {
        var index = _shapes.IndexOf(from);
        if (index >= 0)
            _shapes[index] = to;
        else
            ReportError($"Failed to find {from}, to replace with {to}");
    }
}

class FloodConditions(BossModule module) : Components.StandardAOEs(module, AID.FloodConditions, 6);

class ForecasterStates : StateMachineBuilder
{
    public ForecasterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Forecast>()
            .ActivateOnEnter<FloodConditions>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13437)]
public class Forecaster(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
