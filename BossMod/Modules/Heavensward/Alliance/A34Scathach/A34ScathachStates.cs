namespace BossMod.Heavensward.Alliance.A34Scathach;

class A34ScathachStates : StateMachineBuilder
{
    public A34ScathachStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //.ActivateOnEnter<ThirtyCries>()
            .ActivateOnEnter<ThirtyThorns4>()
            .ActivateOnEnter<ThirtySouls>()
            .ActivateOnEnter<ThirtyArrows2>()
            .ActivateOnEnter<ThirtyArrows1>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<Shadespin2>()
            .ActivateOnEnter<Shadesmite1>()
            .ActivateOnEnter<Shadesmite2>()
            .ActivateOnEnter<Shadesmite3>()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<MarrowDrain>()
            .ActivateOnEnter<MarrowDrain2>()
            .ActivateOnEnter<Pitfall>()
            .ActivateOnEnter<FullSwing>()
            .ActivateOnEnter<BigHug>();
    }
}
