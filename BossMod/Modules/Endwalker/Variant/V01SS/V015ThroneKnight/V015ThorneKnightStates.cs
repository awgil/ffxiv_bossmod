namespace BossMod.Endwalker.Variant.V01SS.V015ThorneKnight;

class V015ThorneKnightStates : StateMachineBuilder
{
    public V015ThorneKnightStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlisteringBlow>()
            .ActivateOnEnter<BlazingBeacon1>()
            .ActivateOnEnter<BlazingBeacon2>()
            .ActivateOnEnter<BlazingBeacon3>()
            .ActivateOnEnter<SignalFlareAOE>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<SacredFlay1>()
            .ActivateOnEnter<SacredFlay2>()
            .ActivateOnEnter<SacredFlay3>()
            .ActivateOnEnter<ForeHonor>()
            .ActivateOnEnter<Cogwheel>();
    }
}
