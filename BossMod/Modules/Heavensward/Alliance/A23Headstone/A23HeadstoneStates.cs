namespace BossMod.Heavensward.Alliance.A23Headstone;

class A23HeadstoneStates : StateMachineBuilder
{
    public A23HeadstoneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TremblingEpigraph>()
            .ActivateOnEnter<FlaringEpigraph>()
            .ActivateOnEnter<BigBurst>();
    }
}
