namespace BossMod.Heavensward.Alliance.A35Diabolos;

class A35DiabolosStates : StateMachineBuilder
{
    public A35DiabolosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Nightmare>()
            .ActivateOnEnter<NightTerror>()
            .ActivateOnEnter<RuinousOmen1>()
            .ActivateOnEnter<RuinousOmen2>()
            .ActivateOnEnter<UltimateTerror>();
    }
}
