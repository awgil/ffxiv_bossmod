namespace BossMod.RealmReborn.Alliance.A21Scylla;
class A21ScyllaStates : StateMachineBuilder
{
    public A21ScyllaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Topple>()
            .ActivateOnEnter<SearingChain>()
            .ActivateOnEnter<InfiniteAnguish>()
            .ActivateOnEnter<AncientFlare>();
    }
}
