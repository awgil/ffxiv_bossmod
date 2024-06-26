namespace BossMod.Stormblood.Trial.T02Lakshmi;

class T02LakshmiStates : StateMachineBuilder
{
    public T02LakshmiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DivineDenial>()
            .ActivateOnEnter<Stotram1>()
            .ActivateOnEnter<Stotram2>()
            .ActivateOnEnter<ThePallOfLightStack>()
            .ActivateOnEnter<ThePullOfLightTB1>()
            .ActivateOnEnter<ThePullOfLightTB2>();
    }
}
