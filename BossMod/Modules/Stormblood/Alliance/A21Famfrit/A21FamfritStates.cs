namespace BossMod.Stormblood.Alliance.A21Famfrit;

class A21FamfritStates : StateMachineBuilder
{
    public A21FamfritStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Tsunami9>()
            .ActivateOnEnter<Materialize>()
            .ActivateOnEnter<DarkRain2>()
            .ActivateOnEnter<DarkeningDeluge>()
            .ActivateOnEnter<WaterIV>()
            .ActivateOnEnter<TidePod>();
    }
}
