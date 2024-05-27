namespace BossMod.Stormblood.Alliance.A22Belias;

class A22BeliasStates : StateMachineBuilder
{
    public A22BeliasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TimeEruption>()
            .ActivateOnEnter<TimeBomb2>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<FireIV>();
    }
}
