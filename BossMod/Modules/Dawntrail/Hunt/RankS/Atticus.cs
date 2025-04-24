namespace BossMod.Dawntrail.Hunt.RankS.Atticus;

public enum OID : uint
{
    Boss = 0x416D, // R4.700, x1
}

public enum AID : uint
{
    AutoAttack = 39015, // Boss->player, no cast, single-target
    BreathYellFast = 39883, // Boss->self, no cast, single-target, visual (start yell sequence or continue fast yell sequence)
    BreathYellSlow = 39884, // Boss->self, no cast, single-target, visual (continue normal yell sequence)
    BreathSequenceFFirst = 39003, // Boss->self, 5.0s cast, range 60 120-degree cone (front)
    BreathSequenceRFirst = 39004, // Boss->self, 5.0s cast, range 60 120-degree cone (back-right)
    BreathSequenceLFirst = 39005, // Boss->self, 5.0s cast, range 60 120-degree cone (back-left)
    BreathSequenceFRest = 39009, // Boss->self, no cast, range 60 120-degree cone (front)
    BreathSequenceRRest = 39010, // Boss->self, no cast, range 60 120-degree cone (back-right)
    BreathSequenceLRest = 39011, // Boss->self, no cast, range 60 120-degree cone (back-left)
    Brutality = 39012, // Boss->self, 3.0s cast, single-target, visual (apply status that speeds up yells sequence)
    PyricBlast = 39013, // Boss->players, 5.0s cast, range 6 circle stack
    Intimidation = 39014, // Boss->self, 4.0s cast, range 40 circle, raidwide
}

public enum IconID : uint
{
    PyricBlast = 161, // player
}

class BreathSequence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Angle> _sequence = [];
    private Angle _baseRotation;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(60, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        if (_sequence.Count > 1)
            yield return new(_shape, Module.PrimaryActor.Position, _baseRotation + _sequence[1], _activation.AddSeconds(2.4f));
        if (_sequence.Count > 0)
            yield return new(_shape, Module.PrimaryActor.Position, _baseRotation + _sequence[0], _activation, ArenaColor.Danger);
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        // there are 30 yells total; first half belongs to first triplet, second half belongs to second triplet
        // each triplet is split into 3 groups based on first aoe; first yell corresponds to first head, second/third correspond to CW order, fourth/fifth correspond to CCW order
        // first triplet:  16874 - F, +1/+2 = RL, +3/+4 = LR
        //                 16879 - R, +1/+2 = LF, +3/+4 = FL
        //                 16884 - L, +1/+2 = FR, +3/+4 = RF
        // second triplet: 16889 - F, +1/+2 = RL, +3/+4 = LR
        //                 16894 - R, +1/+2 = LF, +3/+4 = FL
        //                 16899 - L, +1/+2 = FR, +3/+4 = RF
        if (actor != Module.PrimaryActor || id < 16874 || id > 16903)
            return;
        var index = id - 16874;
        var firstTriplet = index < 15;
        index %= 15;
        var group = index / 5;
        index %= 5;

        var tripletStart = firstTriplet ? 0 : 3;
        var expectedSeqCount = index switch
        {
            1 or 3 => 1,
            2 or 4 => 2,
            _ => 0
        } + tripletStart;
        var groupStart = -120.Degrees() * group;
        if (expectedSeqCount != _sequence.Count)
        {
            ReportError($"Unexpected triplet: yell {id} (first-triplet={firstTriplet}, group={group}, index={index}), cur seq has size {_sequence.Count}, expected {expectedSeqCount}");
        }
        else if (index != 0 && !_sequence[tripletStart].AlmostEqual(groupStart, 0.1f))
        {
            ReportError($"Unexpected triplet: yell {id} (first-triplet={firstTriplet}, group={group}, index={index}), triplet start at {_sequence[tripletStart]}, expected for this group to be {groupStart}");
        }

        var offset = index switch
        {
            1 or 4 => -120.Degrees(),
            2 or 3 => 120.Degrees(),
            _ => default
        };
        _sequence.Add(groupStart + offset);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Angle? offset = (AID)spell.Action.ID switch
        {
            AID.BreathSequenceFFirst => 0.Degrees(),
            AID.BreathSequenceRFirst => -120.Degrees(),
            AID.BreathSequenceLFirst => 120.Degrees(),
            _ => null
        };
        if (offset == null)
            return;

        if (_sequence.Count is not 3 and not 6)
        {
            ReportError($"Unexpected sequence: size={_sequence.Count}");
            return;
        }

        if (!_sequence[0].AlmostEqual(offset.Value, 0.1f))
        {
            ReportError($"Unexpected sequence start: expected {_sequence[0]}, got {offset.Value} ({spell.Action})");
            return;
        }

        _baseRotation = spell.Rotation - offset.Value;
        _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        Angle? offset = (AID)spell.Action.ID switch
        {
            AID.BreathSequenceFFirst or AID.BreathSequenceFRest => 0.Degrees(),
            AID.BreathSequenceRFirst or AID.BreathSequenceRRest => -120.Degrees(),
            AID.BreathSequenceLFirst or AID.BreathSequenceLRest => 120.Degrees(),
            _ => null
        };
        if (offset == null || _sequence.Count == 0)
            return;

        if (!_sequence[0].AlmostEqual(offset.Value, 0.1f))
            ReportError($"Unexpected cast: got {offset.Value} ({spell.Action}), expected {_sequence[0]}");
        _sequence.RemoveAt(0);
        _activation = _sequence.Count > 0 ? WorldState.FutureTime(2.4f) : default;
    }
}

class PyricBlast(BossModule module) : Components.StackWithCastTargets(module, AID.PyricBlast, 6, 4);
class Intimidation(BossModule module) : Components.RaidwideCast(module, AID.Intimidation);

class AtticusStates : StateMachineBuilder
{
    public AtticusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BreathSequence>()
            .ActivateOnEnter<PyricBlast>()
            .ActivateOnEnter<Intimidation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13156)]
public class Atticus(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
