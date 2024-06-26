namespace BossMod.Stormblood.Alliance.A24Yiazmat;

class A24YiazmatStates : StateMachineBuilder
{
    public A24YiazmatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RakeTB>()
            .ActivateOnEnter<RakeSpread>()
            .ActivateOnEnter<RakeAOE>()
            .ActivateOnEnter<RakeLoc1>()
            .ActivateOnEnter<RakeLoc2>()
            .ActivateOnEnter<StoneBreath>()
            .ActivateOnEnter<DustStorm2>()
            .ActivateOnEnter<WhiteBreath>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<Karma>()
            .ActivateOnEnter<UnholyDarkness>()
            .ActivateOnEnter<SolarStorm1>();
    }
}
