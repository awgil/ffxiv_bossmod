namespace BossMod.RealmReborn.Alliance.A23Amon;

class A23AmonStates : StateMachineBuilder
{
    public A23AmonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlizzagaForte>()
            .ActivateOnEnter<Darkness>()
            .ActivateOnEnter<CurtainCall>()
            .ActivateOnEnter<ThundagaForte1>()
            .ActivateOnEnter<ThundagaForte2>();
    }
}
