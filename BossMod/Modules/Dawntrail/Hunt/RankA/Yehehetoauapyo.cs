namespace BossMod.Dawntrail.Hunt.RankA.Yehehetoauapyo;

public enum OID : uint
{
    Boss = 0x43DB, // R6.250, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WhirlingOmenL = 38626, // Boss->self, 5.0s cast, single-target, visual (apply left windup 1)
    WhirlingOmenR = 38627, // Boss->self, 5.0s cast, single-target, visual (apply right windup 1)
    WhirlingOmenLR = 38628, // Boss->self, 5.0s cast, single-target, visual (apply left windup 1 and right windup 2)
    WhirlingOmenLL = 38629, // Boss->self, 5.0s cast, single-target, visual (?)
    WhirlingOmenRL = 38630, // Boss->self, 5.0s cast, single-target, visual (apply right windup 1 and left windup 2)
    WhirlingOmenRR = 38631, // Boss->self, 5.0s cast, single-target, visual (?)
    PteraspitToTurntail = 38632, // Boss->self, 5.0s cast, single-target, visual (front, turn, back)
    DactailToTurnspit = 38633, // Boss->self, 5.0s cast, single-target, visual (back, turn, front)
    TurnspitToDactail = 38634, // Boss->self, 5.0s cast, single-target, visual (turn, front, back)
    TurntailToPteraspit = 38635, // Boss->self, 5.0s cast, single-target, visual (turn, back, front)
    PteraspitStay = 38636, // Boss->self, 0.8s cast, range 40 150-degree cone (front)
    DactailStay = 38637, // Boss->self, 0.8s cast, range 40 150-degree cone (back)
    PteraspitTurnL = 38638, // Boss->self, 0.8s cast, range 40 150-degree cone (turn left -> front)
    DactailTurnR = 38639, // Boss->self, 0.8s cast, range 40 150-degree cone (turn right -> back)
    PteraspitTurnR = 38640, // Boss->self, 0.8s cast, range 40 150-degree cone (turn right -> front)
    DactailTurnL = 38641, // Boss->self, 0.8s cast, range 40 150-degree cone (turn left -> back)
}

public enum SID : uint
{
    LeftWindup1 = 4029, // Boss->Boss, extra=0x0 (first/only)
    RightWindup1 = 4030, // Boss->Boss, extra=0x0 (first/only)
    LeftWindup2 = 4031, // Boss->Boss, extra=0x0 (second)
    RightWindup2 = 4032, // Boss->Boss, extra=0x0 (second)
}

class WhirlingOmen(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _imminentTurn;
    private Angle _futureTurn;
    private readonly List<Angle> _aoeRotations = [];
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(40, 75.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoeRotations.Count > 1)
            yield return new(_shape, Module.PrimaryActor.Position, _aoeRotations[1], _activation.AddSeconds(2.5f));
        if (_aoeRotations.Count > 0)
            yield return new(_shape, Module.PrimaryActor.Position, _aoeRotations[0], _activation, ArenaColor.Danger);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.LeftWindup1:
                _imminentTurn = 90.Degrees();
                break;
            case SID.RightWindup1:
                _imminentTurn = -90.Degrees();
                break;
            case SID.LeftWindup2:
                _futureTurn = 90.Degrees();
                break;
            case SID.RightWindup2:
                _futureTurn = -90.Degrees();
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (first, second) = (AID)spell.Action.ID switch
        {
            AID.PteraspitToTurntail => (default, _imminentTurn + 180.Degrees()),
            AID.DactailToTurnspit => (180.Degrees(), _imminentTurn),
            AID.TurnspitToDactail => (_imminentTurn, _imminentTurn + 180.Degrees()),
            AID.TurntailToPteraspit => (_imminentTurn + 180.Degrees(), _imminentTurn),
            _ => (default, default)
        };
        if (first == second)
            return;

        if (_imminentTurn == default)
            ReportError($"No turn data when sequence starts");
        _aoeRotations.Add(spell.Rotation + first);
        _aoeRotations.Add(spell.Rotation + second);
        _imminentTurn = _futureTurn;
        _futureTurn = default;
        _activation = Module.CastFinishAt(spell, 1.4f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PteraspitStay or AID.DactailStay or AID.PteraspitTurnL or AID.DactailTurnR or AID.PteraspitTurnR or AID.DactailTurnL)
        {
            if (_aoeRotations.Count == 0)
            {
                ReportError($"Unexpected resolve: {spell.Action} @ {spell.Rotation}");
                return;
            }
            if (!_aoeRotations[0].AlmostEqual(spell.Rotation, 0.1f))
                ReportError($"Unexpected resolve: {spell.Action} @ {spell.Rotation}, expected {_aoeRotations[0]}");
            _aoeRotations.RemoveAt(0);
            _activation = WorldState.FutureTime(2.5f);
        }
    }
}

class YehehetoauapyoStates : StateMachineBuilder
{
    public YehehetoauapyoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhirlingOmen>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13400)]
public class Yehehetoauapyo(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
