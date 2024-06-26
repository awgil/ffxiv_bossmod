namespace BossMod.RealmReborn.Alliance.A34CloudofDarkness;

class A34CloudofDarknessStates : StateMachineBuilder
{
    public A34CloudofDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FeintParticleBeam>()
            .ActivateOnEnter<ZeroFormParticleBeam>()
            .ActivateOnEnter<ParticleBeam2>();
    }
}
