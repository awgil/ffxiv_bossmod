namespace BossMod.Endwalker.Hunt.RankS.Ophioneus;

public enum OID : uint
{
    Boss = 0x35DC, // R5.875, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    RightMaw = 27350, // Boss->self, 5.0s cast, range 30 180-degree cone
    LeftMaw = 27351, // Boss->self, 5.0s cast, range 30 180-degree cone
    PyricCircle = 27352, // Boss->self, 5.0s cast, range 5-40 donut
    PyricBurst = 27353, // Boss->self, 5.0s cast, range 40 circle with ? falloff
    LeapingPyricCircle = 27341, // Boss->location, 6.0s cast, width 0 rect charge, visual
    LeapingPyricBurst = 27342, // Boss->location, 6.0s cast, width 0 rect charge, visual
    LeapingPyricCircleAOE = 27346, // Boss->self, 1.0s cast, range 5-40 donut
    LeapingPyricBurstAOE = 27347, // Boss->self, 1.0s cast, range 40 circle with ? falloff
    Scratch = 27348, // Boss->player, 5.0s cast, single-target
}

class RightMaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightMaw), new AOEShapeCone(30, 90.Degrees()));
class LeftMaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftMaw), new AOEShapeCone(30, 90.Degrees()));

class PyricCircleBurst(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCircle _shapeCircle = new(10); // TODO: verify falloff
    private static readonly AOEShapeDonut _shapeDonut = new(5, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PyricCircle:
            case AID.LeapingPyricCircleAOE:
                _aoe = new(_shapeDonut, caster.Position, spell.Rotation, spell.NPCFinishAt);
                break;
            case AID.PyricBurst:
            case AID.LeapingPyricBurstAOE:
                _aoe = new(_shapeCircle, caster.Position, spell.Rotation, spell.NPCFinishAt);
                break;
            case AID.LeapingPyricCircle:
                _aoe = new(_shapeDonut, spell.LocXZ, spell.Rotation, spell.NPCFinishAt.AddSeconds(5));
                break;
            case AID.LeapingPyricBurst:
                _aoe = new(_shapeCircle, spell.LocXZ, spell.Rotation, spell.NPCFinishAt.AddSeconds(5));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PyricCircle or AID.PyricBurst or AID.LeapingPyricCircleAOE or AID.LeapingPyricBurstAOE)
            _aoe = null;
    }
}

class Scratch(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Scratch));

class OphioneusStates : StateMachineBuilder
{
    public OphioneusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RightMaw>()
            .ActivateOnEnter<LeftMaw>()
            .ActivateOnEnter<PyricCircleBurst>()
            .ActivateOnEnter<Scratch>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 10621)]
public class Ophioneus(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
