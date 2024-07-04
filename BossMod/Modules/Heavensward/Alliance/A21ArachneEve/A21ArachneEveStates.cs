namespace BossMod.Heavensward.Alliance.A21ArachneEve;

class A21ArachneEveStates : StateMachineBuilder
{
    public A21ArachneEveStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkSpike>()
            .ActivateOnEnter<SilkenSpray>()
            .ActivateOnEnter<ShadowBurst>()
            .ActivateOnEnter<SpiderThread>()
            .ActivateOnEnter<FrondAffeared>()
            .ActivateOnEnter<TheWidowsEmbrace>()
            .ActivateOnEnter<TheWidowsKiss>()
            .ActivateOnEnter<Tremblor1>()
            .ActivateOnEnter<Tremblor2>()
            .ActivateOnEnter<Tremblor3>();
    }
}
