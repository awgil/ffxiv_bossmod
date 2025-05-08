namespace BossMod.Dawntrail.Hunt.RankA.SallyTheSweeper;

public enum OID : uint
{
    Boss = 0x4395, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ExecutionModelCircle = 38454, // Boss->self, 2.0s cast, range 10 circle
    ExecutionModelDonut = 38455, // Boss->self, 2.0s cast, range 10-40 donut
    ExecutionModelCross = 38456, // Boss->self, 2.0s cast, range 40 width 10 cross
    CodeExecutionFirstCircle = 38457, // Boss->self, 5.0s cast, range 10 circle
    CodeExecutionFirstDonut = 38458, // Boss->self, 5.0s cast, range 10-40 donut
    ReverseCodeFirstCircle = 38459, // Boss->self, 5.0s cast, range 10 circle
    ReverseCodeFirstCross = 38460, // Boss->self, 5.0s cast, range 40 width 10 cross
    CodeExecutionRestCircle = 38461, // Boss->self, no cast, range 10 circle
    CodeExecutionRestDonut = 38462, // Boss->self, no cast, range 10-40 donut
    CodeExecutionRestCross = 38463, // Boss->self, no cast, range 40 width 10 cross
    TargetedAdvance = 38466, // Boss->location, 7.0s cast, range 18 circle
    ReverseCodeVisual = 40056, // Boss->self, 5.0s cast, single-target, visual (???)
}

class ExecutionModel(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEShape> _shapes = [];
    private bool _reverse;
    private DateTime _activation;
    private Angle _rotation;

    private static readonly AOEShapeCircle _shapeCircle = new(10);
    private static readonly AOEShapeDonut _shapeDonut = new(10, 40);
    private static readonly AOEShapeCross _shapeCross = new(40, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default && _shapes.Count > 0)
            yield return new(_shapes[_reverse ? ^1 : 0], Module.PrimaryActor.Position, _rotation, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExecutionModelCircle:
                _shapes.Add(_shapeCircle);
                break;
            case AID.ExecutionModelDonut:
                _shapes.Add(_shapeDonut);
                break;
            case AID.ExecutionModelCross:
                _shapes.Add(_shapeCross);
                _rotation = spell.Rotation;
                break;
            case AID.CodeExecutionFirstCircle:
            case AID.CodeExecutionFirstDonut:
                _reverse = false;
                _activation = Module.CastFinishAt(spell);
                break;
            case AID.ReverseCodeFirstCircle:
            case AID.ReverseCodeFirstCross:
                _reverse = true;
                _activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CodeExecutionFirstCircle or AID.CodeExecutionFirstDonut or AID.ReverseCodeFirstCircle or AID.ReverseCodeFirstCross or AID.CodeExecutionRestCircle or AID.CodeExecutionRestDonut or AID.CodeExecutionRestCross && _shapes.Count > 0)
        {
            _shapes.RemoveAt(_reverse ? _shapes.Count - 1 : 0);
            _activation = _shapes.Count > 0 ? WorldState.FutureTime(2.5f) : default;
        }
    }
}

class TargetedAdvance(BossModule module) : Components.StandardAOEs(module, AID.TargetedAdvance, 18, warningText: "GTFO from jump!");

class SallyTheSweeperStates : StateMachineBuilder
{
    public SallyTheSweeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ExecutionModel>()
            .ActivateOnEnter<TargetedAdvance>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13435)]
public class SallyTheSweeper(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
