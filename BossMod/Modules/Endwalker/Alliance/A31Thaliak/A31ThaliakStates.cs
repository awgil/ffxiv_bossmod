namespace BossMod.Endwalker.Alliance.A31Thaliak;

class A31ThaliakStates : StateMachineBuilder
{
    public A31ThaliakStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Katarraktes>()
            .ActivateOnEnter<Hieroglyphika>()
            .ActivateOnEnter<Tetraktys>()
            .ActivateOnEnter<Thlipsis>()
            .ActivateOnEnter<Hydroptosis>()
            .ActivateOnEnter<Rhyton>()
            .ActivateOnEnter<LeftBank>()
            .ActivateOnEnter<LeftBank2>()
            .ActivateOnEnter<RightBank>()
            .ActivateOnEnter<RightBank2>()
            .ActivateOnEnter<RheognosisKnockback>()
            .ActivateOnEnter<RheognosisCrash>()
            .ActivateOnEnter<Rheognosis>()
            .ActivateOnEnter<TetraTriangles>();
    }
}
