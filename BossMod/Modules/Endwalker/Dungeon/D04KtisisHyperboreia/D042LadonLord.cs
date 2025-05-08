namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D042LadonLord;

public enum OID : uint
{
    Boss = 0x3425, // R3.995, x1
}

public enum AID : uint
{
    PyricBreathFront = 25734, // Boss->self, 7.0s cast, range 40 ?-degree cone
    PyricBreathLeft = 25735, // Boss->self, 7.0s cast, range 40 ?-degree cone
    PyricBreathRight = 25736, // Boss->self, 7.0s cast, range 40 ?-degree cone
    PyricBreathFront2 = 25737, // Boss->self, no cast, range 40 ?-degree cone
    PyricBreathLeft2 = 25738, // Boss->self, no cast, range 40 ?-degree cone
    PyricBreathRight2 = 25739, // Boss->self, no cast, range 40 ?-degree cone
    Intimidation = 25741, // Boss->self, 6.0s cast, range 40 circle
    PyricBlast = 25742, // Boss->none, 4.0s cast, range 6 circle
    Scratch = 25743, // Boss->none, 5.0s cast, single-target
    PyricSphere = 25745, // 233C->self, 10.0s cast, range 50 width 4 cross
}

public enum SID : uint
{
    Front = 2812, // none->Boss, extra=0x9F6
    Left = 2813, // none->Boss, extra=0x177F
    Right = 2814, // none->Boss, extra=0x21A8
}

class Scratch(BossModule module) : Components.SingleTargetCast(module, AID.Scratch);

class PyricBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<SID> _buffs = [];
    private static readonly AOEShapeCone shape = new(40, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < _aoes.Count; i++)
            yield return _aoes[i] with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE };
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Front or SID.Left or SID.Right)
            _buffs.Add((SID)status.ID);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PyricBreathFront or AID.PyricBreathRight or AID.PyricBreathLeft)
        {
            _aoes.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            if (_buffs.Count == 2)
                _aoes.Add(new(shape, caster.Position, spell.Rotation - BuffRelative(_buffs[0]) + BuffRelative(_buffs[1]), Module.CastFinishAt(spell, 2.1f)));
        }
    }

    private Angle BuffRelative(SID sid) => sid switch
    {
        SID.Front => default,
        SID.Left => 120.Degrees(),
        SID.Right => -120.Degrees(),
        _ => WrongAngle(sid)
    };

    private Angle WrongAngle(SID sid)
    {
        ReportError($"Wrong SID for angle {sid}");
        return default;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PyricBreathFront or AID.PyricBreathRight or AID.PyricBreathLeft)
        {
            _aoes.RemoveAt(0);
            _buffs.RemoveAt(0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PyricBreathFront2 or AID.PyricBreathLeft2 or AID.PyricBreathRight2)
        {
            _aoes.Clear();
            _buffs.Clear();
        }
    }
}

class PyricSphere(BossModule module) : Components.StandardAOEs(module, AID.PyricSphere, new AOEShapeCross(50, 2));
class PyricBlast(BossModule module) : Components.StackWithCastTargets(module, AID.PyricBlast, 6, 4);
class Intimidation(BossModule module) : Components.RaidwideCast(module, AID.Intimidation);

class D042LadonLordStates : StateMachineBuilder
{
    public D042LadonLordStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<PyricBreath>()
            .ActivateOnEnter<PyricSphere>()
            .ActivateOnEnter<PyricBlast>()
            .ActivateOnEnter<Intimidation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10398)]
public class D042LadonLord(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 48), new ArenaBoundsSquare(19.5f));
