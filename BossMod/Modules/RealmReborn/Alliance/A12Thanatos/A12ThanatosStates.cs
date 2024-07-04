namespace BossMod.RealmReborn.Alliance.A12Thanatos;
class A12ThanatosStates : StateMachineBuilder
{
    public A12ThanatosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlackCloud>()
            .ActivateOnEnter<Cloudscourge>()
            .ActivateOnEnter<VoidFireII>()
            .ActivateOnEnter<AstralLight>();
    }
}
