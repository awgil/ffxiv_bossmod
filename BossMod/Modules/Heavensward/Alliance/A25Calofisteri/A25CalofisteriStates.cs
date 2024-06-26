namespace BossMod.Heavensward.Alliance.A25Calofisteri;

class A25CalofisteriStates : StateMachineBuilder
{
    public A25CalofisteriStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AuraBurst>()
            .ActivateOnEnter<DepthCharge>()
            .ActivateOnEnter<Extension2>()
            .ActivateOnEnter<FeintParticleBeam1>()
            .ActivateOnEnter<Penetration>()
            .ActivateOnEnter<Graft>()
            .ActivateOnEnter<Haircut1>()
            .ActivateOnEnter<Haircut2>()
            .ActivateOnEnter<SplitEnd1>()
            .ActivateOnEnter<SplitEnd2>();
    }
}
