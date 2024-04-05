namespace BossMod.Endwalker.Alliance.A10Lions;

class DoubleImmolation : Components.RaidwideCast
{
    public DoubleImmolation() : base(ActionID.MakeSpell(AID.DoubleImmolation)) { }
}

class A10LionsStates : StateMachineBuilder
{
    private bool IsDead(Actor? actor) => actor == null || actor.IsDestroyed || actor.IsDead;

    public A10LionsStates(A10Lions module) : base(module)
    {
        SimplePhase(0, id => SimpleState(id, 600, "???"), "Single phase")
            .ActivateOnEnter<DoubleImmolation>()
            .ActivateOnEnter<SlashAndBurn>()
            .ActivateOnEnter<RoaringBlaze>()
            .Raw.Update = () => IsDead(module.Lion()) && IsDead(module.Lioness());
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Lion, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11294, SortOrder = 4)]
public class A10Lions : BossModule
{
    private Actor? _lioness;

    public Actor? Lion() => PrimaryActor;
    public Actor? Lioness() => _lioness;

    public A10Lions(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-677.25f, -606.25f), 20)) { }

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _lioness ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Lioness).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_lioness, ArenaColor.Enemy);
    }
}
