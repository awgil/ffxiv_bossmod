namespace BossMod.Heavensward.Alliance.A32FerdiadHollow;

class A32FerdiadHollowStates : StateMachineBuilder
{
    public A32FerdiadHollowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Blackbolt>()
            .ActivateOnEnter<Blackfire2>()
            .ActivateOnEnter<JestersJig1>()
            .ActivateOnEnter<JestersReap>()
            .ActivateOnEnter<JestersReward>()
            .ActivateOnEnter<JongleursX>()
            .ActivateOnEnter<JugglingSphere>()
            .ActivateOnEnter<JugglingSphere2>()
            .ActivateOnEnter<AtmosAOE1>()
            .ActivateOnEnter<AtmosAOE2>()
            .ActivateOnEnter<AtmosDonut>()
            .ActivateOnEnter<PetrifyingEye>()
            .ActivateOnEnter<Flameflow1>()
            .ActivateOnEnter<Flameflow2>()
            .ActivateOnEnter<Flameflow3>()
            .ActivateOnEnter<Unknown4>()
            .ActivateOnEnter<Unknown6>();
    }
}
