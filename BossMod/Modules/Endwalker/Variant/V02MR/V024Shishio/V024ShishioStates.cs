namespace BossMod.Endwalker.Variant.V02MR.V024Shishio;

class V024ShishioStates : StateMachineBuilder
{
    public V024ShishioStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //Route 8
            .ActivateOnEnter<ThunderVortex>()
            .ActivateOnEnter<UnsagelySpin>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<Vasoconstrictor>()
            //Route 9
            .ActivateOnEnter<Yoki2>()
            .ActivateOnEnter<YokiUzu>()
            //Route 10
            .ActivateOnEnter<HauntingThrall>()
            //Route 11
            .ActivateOnEnter<ReishoAOE>()
            //Standard
            .ActivateOnEnter<ThunderOnefold2>()
            .ActivateOnEnter<NoblePursuit>()
            .ActivateOnEnter<Enkyo>()
            .ActivateOnEnter<ThriceOnRokujo3>()
            .ActivateOnEnter<TwiceOnRokujo3>()
            .ActivateOnEnter<ThunderTwofold2>()
            .ActivateOnEnter<ThunderThreefold2>()
            .ActivateOnEnter<OnceOnRokujoAOE>()
            .ActivateOnEnter<LeapingLevin1>()
            .ActivateOnEnter<LeapingLevin2>()
            .ActivateOnEnter<LeapingLevin3>()
            .ActivateOnEnter<SplittingCry>()
            .ActivateOnEnter<CloudToCloud>();
        //.ActivateOnEnter<RokujoRevel>()
    }
}