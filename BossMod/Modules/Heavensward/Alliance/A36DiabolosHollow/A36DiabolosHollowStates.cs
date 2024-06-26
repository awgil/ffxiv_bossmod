namespace BossMod.Heavensward.Alliance.A36DiabolosHollow;

class A36DiabolosHollowStates : StateMachineBuilder
{
    public A36DiabolosHollowStates(BossModule module) : base(module)
    {
        TrivialPhase()
        .ActivateOnEnter<Shadethrust>()
        .ActivateOnEnter<HollowCamisado>()
        .ActivateOnEnter<HollowNightmare>()
        .ActivateOnEnter<HollowOmen1>()
        .ActivateOnEnter<HollowOmen2>()
        .ActivateOnEnter<Blindside>()
        .ActivateOnEnter<Nox>()
        .ActivateOnEnter<HollowNight>()
        .ActivateOnEnter<HollowNightGaze>()
        .ActivateOnEnter<ParticleBeam2>()
        .ActivateOnEnter<ParticleBeam4>();
    }
}
