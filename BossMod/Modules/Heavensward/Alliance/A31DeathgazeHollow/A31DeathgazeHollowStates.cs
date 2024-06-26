namespace BossMod.Heavensward.Alliance.A31DeathgazeHollow;

class A31DeathgazeHollowStates : StateMachineBuilder
{
    public A31DeathgazeHollowStates(A31DeathgazeHollow module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkII>()
            //.ActivateOnEnter<BoltOfDarkness3>()
            .ActivateOnEnter<VoidDeath>()
            .ActivateOnEnter<VoidAeroII>()
            .ActivateOnEnter<VoidBlizzardIIIAOE>()
            .ActivateOnEnter<VoidAeroIVKB1>()
            .ActivateOnEnter<VoidAeroIVKB2>()
            .ActivateOnEnter<Unknown3>()
            .ActivateOnEnter<VoidDeathKB2>()
            .ActivateOnEnter<VoidDeathKB>();
    }
}
