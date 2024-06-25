namespace BossMod.Heavensward.Alliance.A11Cetus;

class A11CetusStates : StateMachineBuilder
{
    public A11CetusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricSwipe>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<Immersion>()
            .ActivateOnEnter<ElectricWhorl>()
            .ActivateOnEnter<ExpulsionAOE>()
            .ActivateOnEnter<ExpulsionKnockback>()
            .ActivateOnEnter<BiteAndRun>();
    }
}
