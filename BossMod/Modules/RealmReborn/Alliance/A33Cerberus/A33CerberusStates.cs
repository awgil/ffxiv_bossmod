namespace BossMod.RealmReborn.Alliance.A33Cerberus;

class A33CerberusStates : StateMachineBuilder
{
    public A33CerberusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TailBlow>()
            .ActivateOnEnter<Slabber>()
            .ActivateOnEnter<Mini>()
            .ActivateOnEnter<SulphurousBreath1>()
            .ActivateOnEnter<SulphurousBreath2>()
            .ActivateOnEnter<LightningBolt2>()
            .ActivateOnEnter<HoundOutOfHell>()
            .ActivateOnEnter<Ululation>();
    }
}
