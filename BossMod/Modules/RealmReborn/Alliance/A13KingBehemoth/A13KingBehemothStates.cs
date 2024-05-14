namespace BossMod.RealmReborn.Alliance.A13KingBehemoth;
class A13KingBehemothStates : StateMachineBuilder
{
    public A13KingBehemothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EclipticMeteor>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<CharybdisAOE>();
    }
}
