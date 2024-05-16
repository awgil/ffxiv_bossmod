namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

class DAL1SartauvoirStates : StateMachineBuilder
{
    public DAL1SartauvoirStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PyrokinesisAOE>()
            .ActivateOnEnter<TimeEruption>()
            .ActivateOnEnter<ReverseTimeEruption>()
            .ActivateOnEnter<ThermalGustAOE>()
            .ActivateOnEnter<GrandCrossflameAOE>()
            .ActivateOnEnter<Flamedive>()
            .ActivateOnEnter<BurningBlade>()
            .ActivateOnEnter<MannatheihwonFlame2>()
            .ActivateOnEnter<MannatheihwonFlame3>()
            .ActivateOnEnter<MannatheihwonFlame4>()
            .ActivateOnEnter<LeftBrand>()
            .ActivateOnEnter<RightBrand>()
            .ActivateOnEnter<Pyrocrisis>()
            .ActivateOnEnter<Pyrodoxy>();
    }
}
