namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1EnrageCondition(BossModule module) : BossComponent(module)
{
    public enum State
    {
        None,
        Enrage,
        P2
    }

    public State CurrentState { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.P1LightOfJudgmentEnrage)
            CurrentState = State.Enrage;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BossP1 && id == 0x1E39)
            CurrentState = State.P2;
    }
}

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1094, NameID = 7131, PlanLevel = 100)]
public class UMAD(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public override bool ShouldPrioritizeAllEnemies => true;

    Actor? _bossP2;

    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        _bossP2 ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.BossP2).FirstOrDefault() : null;
    }
}
