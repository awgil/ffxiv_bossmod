namespace BossMod.Stormblood.Extreme.Ex2Lakshmi;

class Ex2LakshmiStates : StateMachineBuilder
{
    public Ex2LakshmiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<InnerDemonsGaze>()
            .ActivateOnEnter<InnerDemonsAOE>()
            .ActivateOnEnter<ThePallOfLight>()
            .ActivateOnEnter<DivineDenial>();
    }
}
