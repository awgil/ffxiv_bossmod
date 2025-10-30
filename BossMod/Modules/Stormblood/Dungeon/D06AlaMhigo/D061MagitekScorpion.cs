namespace BossMod.Stormblood.Dungeon.D06AlaMhigo.D061MagitekScorpion;

public enum AID : uint
{
    Attack = 9303, // Boss->player, no cast, single-target
    ElectromagneticField = 8269, // Boss->self, 3.0s cast, range 40 circle
    TargetSearch = 8262, // Boss->self, 3.0s cast, single-target
    LockOn = 8263, // 18D6->self, no cast, range 5 circle
    TailLaser1 = 8264, // Boss->self, 2.0s cast, single-target
    TailLaser2 = 8265, // 18D6->self, 3.0s cast, range 20+R width 10 rect
    TailLaser3 = 8266, // 18D6->self, 3.0s cast, range 20+R width 10 rect
    TailLaser4 = 8267, // 18D6->self, no cast, range 20+R width 10 rect
    TailLaser5 = 8268, // 18D6->self, no cast, range 20+R width 10 rect
}

public enum OID : uint
{
    Boss = 0x1BA4,
    Helper = 0x233C,
    N1 = 0x1DCD, // R0.500, x2
    TargetSearchCrossHair = 0x1BA5, // R1.000, x8
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1ea4ff = 0x1EA4FF, // R2.000, x1 (spawn during fight), EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
    MagitekScorpion = 0x18D6, // R0.500, x10
    LockOnPuddle = 0x1EA66D, // R0.500, x0 (spawn during fight), EventObj type
    N3 = 0x1DC9, // R0.500, x0 (spawn during fight)
    N4 = 0x1DD1, // R0.500, x0 (spawn during fight)
    Actor1e8536 = 0x1E8536, // R2.000, x0 (spawn during fight), EventObj type
}

class LockOnPuddle(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.LockOnPuddle));
class TailLaser(BossModule module) : Components.GenericAOEs(module, AID.TailLaser1)
{
    private readonly List<AOEInstance> _aoes = [];
    private int eventCastCount;
    private DateTime _timeout;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoes.Add(new AOEInstance(new AOEShapeRect(20, 5, 20), caster.Position, caster.Rotation));
            _timeout = WorldState.FutureTime(10);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WorldState.CurrentTime >= _timeout || spell.Action.ID is (uint)AID.TailLaser4 or (uint)AID.TailLaser5 && eventCastCount++ > 12)
        {
            _aoes.Clear();
            eventCastCount = 0;
        }
    }
}

class TargetSearch(BossModule module) : Components.GenericAOEs(module, AID.TargetSearch)
{
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _time;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.TargetSearchCrossHair && animState1 == 1)
            _aoes.Add(new AOEInstance(new AOEShapeCircle(5), actor.Position));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LockOn)
            _time = WorldState.FutureTime(2);
    }

    public override void Update()
    {
        if (WorldState.CurrentTime > _time)
        {
            _aoes.Clear();
            _time = default;
        }
    }
}

class D061MagitekScorpionStates : StateMachineBuilder
{
    public D061MagitekScorpionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TailLaser>()
            .ActivateOnEnter<TargetSearch>()
            .ActivateOnEnter<LockOnPuddle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 247, NameID = 6037)]
public class D061MagitekScorpion(WorldState ws, Actor primary) : BossModule(ws, primary, new(-191, 72), new ArenaBoundsCircle(20));
