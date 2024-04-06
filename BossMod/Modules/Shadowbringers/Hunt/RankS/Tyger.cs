namespace BossMod.Shadowbringers.Hunt.RankS.Tyger;

public enum OID : uint
{
    Boss = 0x288E, // R=5.92
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    TheLionsBreath = 16957, // Boss->self, 4,0s cast, range 30 120-degree cone
    TheScorpionsSting = 16961, // Boss->self, no cast, range 18 90-degree cone, 2-4s after a voice attack, timing seems to vary, maybe depends if voice was interrupted and how fast?
    TheDragonsBreath = 16959, // Boss->self, 4,0s cast, range 30 120-degree cone
    TheRamsBreath = 16958, // Boss->self, 4,0s cast, range 30 120-degree cone
    TheRamsEmbrace = 16960, // Boss->location, 3,0s cast, range 9 circle
    TheDragonsVoice = 16963, // Boss->self, 4,0s cast, range 8-30 donut, interruptible raidwide donut
    TheRamsVoice = 16962, // Boss->self, 4,0s cast, range 9 circle
};

class TheLionsBreath : Components.SelfTargetedAOEs
{
    public TheLionsBreath() : base(ActionID.MakeSpell(AID.TheLionsBreath), new AOEShapeCone(30, 60.Degrees())) { }
}

class TheScorpionsSting : Components.GenericAOEs
{
    private DateTime _activation;
    private static readonly AOEShapeCone cone = new(18, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(cone, module.PrimaryActor.Position, module.PrimaryActor.Rotation + 180.Degrees(), _activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheRamsVoice or AID.TheDragonsVoice)
            _activation = spell.NPCFinishAt.AddSeconds(2.3f);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheRamsVoice or AID.TheDragonsVoice)
            _activation = module.WorldState.CurrentTime.AddSeconds(2.3f); //timing varies, just used the lowest i could find, probably depends on interrupt status
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TheScorpionsSting)
            _activation = default;
    }
}

class TheDragonsBreath : Components.SelfTargetedAOEs
{
    public TheDragonsBreath() : base(ActionID.MakeSpell(AID.TheDragonsBreath), new AOEShapeCone(30, 60.Degrees())) { }
}

class TheRamsBreath : Components.SelfTargetedAOEs
{
    public TheRamsBreath() : base(ActionID.MakeSpell(AID.TheRamsBreath), new AOEShapeCone(30, 60.Degrees())) { }
}

class TheRamsEmbrace : Components.SelfTargetedAOEs
{
    public TheRamsEmbrace() : base(ActionID.MakeSpell(AID.TheRamsEmbrace), new AOEShapeCircle(9)) { }
}

class TheRamsVoice : Components.SelfTargetedAOEs
{
    public TheRamsVoice() : base(ActionID.MakeSpell(AID.TheRamsVoice), new AOEShapeCircle(9)) { }
}

class TheRamsVoiceHint : Components.CastInterruptHint
{
    public TheRamsVoiceHint() : base(ActionID.MakeSpell(AID.TheRamsVoice)) { }
}

class TheDragonsVoice : Components.SelfTargetedAOEs
{
    public TheDragonsVoice() : base(ActionID.MakeSpell(AID.TheDragonsVoice), new AOEShapeDonut(8, 30)) { }
}

class TheDragonsVoiceHint : Components.CastInterruptHint
{
    public TheDragonsVoiceHint() : base(ActionID.MakeSpell(AID.TheDragonsVoice), hintExtra: "Donut Raidwide") { }
}

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
public class Tyger : SimpleBossModule
{
    public Tyger(WorldState ws, Actor primary) : base(ws, primary) { }
}
