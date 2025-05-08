namespace BossMod.Dawntrail.Hunt.RankA.RraxYitya;

public enum OID : uint
{
    Boss = 0x4232, // R5.000, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    RightWingbladeFirst = 37164, // Boss->self, 3.0s cast, range 25 180-degree cone (first in sequence)
    LeftWingbladeFirst = 37165, // Boss->self, 3.0s cast, range 25 180-degree cone (first in sequence)
    RightWingbladeRest = 37166, // Boss->self, 3.0s cast, range 25 180-degree cone (remaining in sequence)
    LeftWingbladeRest = 37167, // Boss->self, 3.0s cast, range 25 180-degree cone (remaining in sequence)
    TriplicateReflex = 37170, // Boss->self, 5.0s cast, single-target, visual (repeat last sequence)
    RightWingbladeAOE = 37171, // Boss->self, no cast, range 25 180-degree cone
    LeftWingbladeAOE = 37172, // Boss->self, no cast, range 25 180-degree cone
    LaughingLeap = 37372, // Boss->self, 4.0s cast, range 15 width 5 rect
}

class Wingblade(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _nextAOE;
    private readonly List<Angle> _lastSeq = [];

    private static readonly AOEShapeCone _shape = new(25, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_nextAOE);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_lastSeq.Count > 0)
            hints.Add($"Sequence: {string.Join(" -> ", _lastSeq.Select(a => a.Rad < 0 ? "Right" : "Left"))}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RightWingbladeFirst:
                _lastSeq.Clear();
                _lastSeq.Add(-90.Degrees());
                _nextAOE = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.LeftWingbladeFirst:
                _lastSeq.Clear();
                _lastSeq.Add(90.Degrees());
                _nextAOE = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.RightWingbladeRest:
                _lastSeq.Add(-90.Degrees());
                _nextAOE = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.LeftWingbladeRest:
                _lastSeq.Add(90.Degrees());
                _nextAOE = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.TriplicateReflex:
                if (_lastSeq.Count != 3)
                    ReportError($"Spell {spell.Action} started when sequence length is {_lastSeq.Count}");
                if (_lastSeq.Count > 0)
                    _nextAOE = new(_shape, caster.Position, spell.Rotation + _lastSeq[0], Module.CastFinishAt(spell, 0.4f));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RightWingbladeFirst or AID.LeftWingbladeFirst or AID.RightWingbladeRest or AID.LeftWingbladeRest)
            _nextAOE = null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RightWingbladeAOE or AID.LeftWingbladeAOE)
        {
            if (_lastSeq.Count > 0)
                _lastSeq.RemoveAt(0);
            _nextAOE = _lastSeq.Count > 0 ? new(_shape, caster.Position, caster.Rotation + _lastSeq[0], WorldState.FutureTime(2)) : null;
        }
    }
}

class LaughingLeap(BossModule module) : Components.StandardAOEs(module, AID.LaughingLeap, new AOEShapeRect(15, 2.5f));

class RraxYityaStates : StateMachineBuilder
{
    public RraxYityaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wingblade>()
            .ActivateOnEnter<LaughingLeap>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 12753)]
public class RraxYitya(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
