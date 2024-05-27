namespace BossMod.Stormblood.Alliance.A31Mustadio;

class A31MustadioStates : StateMachineBuilder
{
    public A31MustadioStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EnergyBurst>()
            .ActivateOnEnter<ArmShot>()
            .ActivateOnEnter<LegShot>()
            .ActivateOnEnter<LeftHandgonne>()
            .ActivateOnEnter<RightHandgonne>()
            .ActivateOnEnter<SatelliteBeam>()
            .ActivateOnEnter<Compress>()
            .ActivateOnEnter<BallisticSpread>();
    }
}
