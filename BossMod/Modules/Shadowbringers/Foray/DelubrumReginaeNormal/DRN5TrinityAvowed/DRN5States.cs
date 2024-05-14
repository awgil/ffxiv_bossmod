namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN5TrinityAvowed;
class DRN5TrinityAvowedStates : StateMachineBuilder
{
    public DRN5TrinityAvowedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //.ActivateOnEnter<TemperatureAOE>()
            .ActivateOnEnter<WrathOfBozja>()
            .ActivateOnEnter<ElementalImpact1>()
            .ActivateOnEnter<ElementalImpact2>()
            //.ActivateOnEnter<ShimmeringShot>()
            //.ActivateOnEnter<ShimmeringShot1>()
            //.ActivateOnEnter<ShimmeringShot2>()
            .ActivateOnEnter<AllegiantArsenal>()
            .ActivateOnEnter<BladeOfEntropy>()
            //.ActivateOnEnter<FlamesOfBozja>()
            //.ActivateOnEnter<FlamesOfBozja1>()
            //.ActivateOnEnter<FlamesOfBozja2>()
            //.ActivateOnEnter<FreedomOfBozja>()
            //.ActivateOnEnter<FreedomOfBozja1>()
            //.ActivateOnEnter<FreedomOfBozja2>()
            .ActivateOnEnter<GleamingArrow>();
    }
}