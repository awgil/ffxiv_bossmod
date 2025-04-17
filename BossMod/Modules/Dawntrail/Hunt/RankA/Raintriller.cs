namespace BossMod.Dawntrail.Hunt.RankA.Raintriller;

public enum OID : uint
{
    Boss = 0x457F, // R4.800, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 39804, // Boss->location, no cast, single-target
    DoReMisery1 = 39758, // Boss->self, 5.0s cast, single-target, visual (1 cast)
    DoReMisery2 = 39751, // Boss->self, 6.2s cast, single-target, visual (2 casts)
    DoReMisery3 = 39749, // Boss->self, 7.5s cast, single-target, visual (3 casts)
    Croakdown = 39750, // Boss->self, 1.0s cast, range 12 circle
    Ribbitygibbet = 39752, // Boss->self, 1.0s cast, range 10-40 donut
    ChirpOTheWisp = 39753, // Boss->self, 1.0s cast, range 40 270-degree cone
    DropOfVenom = 39754, // Boss->player, 5.0s cast, range 6 circle stack
}

public enum IconID : uint
{
    DropOfVenom = 62, // player
}

class DoReMisery(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEShape> _nextShapes = [];
    private Angle _rotation;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shapeOut = new(12);
    private static readonly AOEShapeDonut _shapeIn = new(10, 40);
    private static readonly AOEShapeCone _shapeCone = new(40, 135.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default && _nextShapes.Count > 0)
            yield return new(_nextShapes[0], Module.PrimaryActor.Position, _rotation, _activation);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_nextShapes.Count > 0)
        {
            hints.Add(string.Join(" -> ", _nextShapes.Select(s => s switch
            {
                AOEShapeCircle => "Out",
                AOEShapeDonut => "In",
                AOEShapeCone => "Back",
                _ => "???"
            })));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DoReMisery1 or AID.DoReMisery2 or AID.DoReMisery3)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell, 1.2f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.Croakdown => _shapeOut,
            AID.Ribbitygibbet => _shapeIn,
            AID.ChirpOTheWisp => _shapeCone,
            _ => null
        };
        if (shape == null)
            return;

        if (_nextShapes.Count == 0)
        {
            ReportError($"Unexpected resolve spell {spell.Action}");
            return;
        }

        if (_nextShapes[0] != shape)
            ReportError($"Unexpected shape: got {_nextShapes[0]}, expected {shape}");
        _nextShapes.RemoveAt(0);
        _activation = _nextShapes.Count > 0 ? WorldState.FutureTime(3.2f) : default;
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (actor == Module.PrimaryActor)
        {
            _nextShapes.AddRange(id switch
            {
                17719 => [_shapeOut, _shapeIn],
                17720 => [_shapeIn, _shapeOut, _shapeCone],
                17721 => [_shapeOut, _shapeIn, _shapeCone],
                17722 => [_shapeCone, _shapeIn, _shapeOut],
                17723 => [_shapeIn, _shapeCone, _shapeOut],
                17724 => [_shapeCone, _shapeOut, _shapeIn],
                17725 => [_shapeOut, _shapeCone, _shapeIn],
                17734 => [_shapeOut, _shapeCone, _shapeOut], // is it possible?..
                17793 => [_shapeIn, _shapeIn, _shapeOut], // is it possible?..
                17815 => [_shapeOut],
                17816 => [_shapeIn], // is it possible?..
                17817 => [_shapeCone], // is it possible?..
                _ => []
            });
        }
    }
}

class DropOfVenom(BossModule module) : Components.StackWithCastTargets(module, AID.DropOfVenom, 6, 4);

class RaintrillerStates : StateMachineBuilder
{
    public RaintrillerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DoReMisery>()
            .ActivateOnEnter<DropOfVenom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13442)]
public class Raintriller(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
