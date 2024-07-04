namespace BossMod.Endwalker.Variant.V01SS.V013Gladiator;

class V013GladiatorStates : StateMachineBuilder
{
    public V013GladiatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SunderedRemains>()
            .ActivateOnEnter<BitingWindBad>()
            .ActivateOnEnter<RingOfMight1>()
            .ActivateOnEnter<RingOfMight2>()
            .ActivateOnEnter<RingOfMight3>()
            .ActivateOnEnter<RushOfMight>()
            .ActivateOnEnter<ShatteringSteelMeteor>()
            .ActivateOnEnter<RackAndRuin>()
            .ActivateOnEnter<FlashOfSteel1>()
            .ActivateOnEnter<FlashOfSteel2>()
            .ActivateOnEnter<SculptorsPassion>()
            .ActivateOnEnter<GoldenFlame>()
            .ActivateOnEnter<SilverFlame1>()
            .ActivateOnEnter<SilverFlame2>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<MightySmite>();
    }
}
