namespace BossMod.Shadowbringers.Hunt.RankS.Tyger;

public enum OID : uint
{
    Boss = 0x288E, // R=5.92
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    TheLionsBreath = 16957, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheScorpionsSting = 16961, // Boss->self, no cast, range 18 90-degree cone, 2-4s after a voice attack, timing seems to vary, maybe depends if voice was interrupted and how fast?
    TheDragonsBreath = 16959, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheRamsBreath = 16958, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheRamsEmbrace = 16960, // Boss->location, 3.0s cast, range 9 circle
    TheDragonsVoice = 16963, // Boss->self, 4.0s cast, range 8-30 donut, interruptible raidwide donut
    TheRamsVoice = 16962, // Boss->self, 4.0s cast, range 9 circle
}

class TheLionsBreath(BossModule module) : Components.StandardAOEs(module, AID.TheLionsBreath, new AOEShapeCone(30, 60.Degrees()));

class TheScorpionsSting(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private static readonly AOEShapeCone cone = new(18, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(cone, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees(), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheRamsVoice or AID.TheDragonsVoice)
            _activation = Module.CastFinishAt(spell, 2.3f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheRamsVoice or AID.TheDragonsVoice)
            _activation = WorldState.FutureTime(2.3f); //timing varies, just used the lowest i could find, probably depends on interrupt status
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TheScorpionsSting)
            _activation = default;
    }
}

class TheDragonsBreath(BossModule module) : Components.StandardAOEs(module, AID.TheDragonsBreath, new AOEShapeCone(30, 60.Degrees()));
class TheRamsBreath(BossModule module) : Components.StandardAOEs(module, AID.TheRamsBreath, new AOEShapeCone(30, 60.Degrees()));
class TheRamsEmbrace(BossModule module) : Components.StandardAOEs(module, AID.TheRamsEmbrace, new AOEShapeCircle(9));
class TheRamsVoice(BossModule module) : Components.StandardAOEs(module, AID.TheRamsVoice, new AOEShapeCircle(9));
class TheRamsVoiceHint(BossModule module) : Components.CastInterruptHint(module, AID.TheRamsVoice);
class TheDragonsVoice(BossModule module) : Components.StandardAOEs(module, AID.TheDragonsVoice, new AOEShapeDonut(8, 30));
class TheDragonsVoiceHint(BossModule module) : Components.CastInterruptHint(module, AID.TheDragonsVoice, hintExtra: "Donut Raidwide");

class TygerStates : StateMachineBuilder
{
    public TygerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheDragonsBreath>()
            .ActivateOnEnter<TheScorpionsSting>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheDragonsVoiceHint>()
            .ActivateOnEnter<TheLionsBreath>()
            .ActivateOnEnter<TheRamsBreath>()
            .ActivateOnEnter<TheRamsEmbrace>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheRamsVoiceHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8905)]
public class Tyger(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
