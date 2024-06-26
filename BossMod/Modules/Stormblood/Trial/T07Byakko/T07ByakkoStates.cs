namespace BossMod.Stormblood.Trial.T07Byakko;

class T07ByakkoStates : StateMachineBuilder
{
    public T07ByakkoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormPulse>()
            .ActivateOnEnter<HeavenlyStrike>()
            .ActivateOnEnter<HeavenlyStrikeSpread>()
            .ActivateOnEnter<SweepTheLeg1>()
            .ActivateOnEnter<SweepTheLeg3>()
            .ActivateOnEnter<TheRoarOfThunder>()
            .ActivateOnEnter<ImperialGuard>()
            //.ActivateOnEnter<HighestStakes>() stack marker not being removed properly
            .ActivateOnEnter<FireAndLightning1>()
            .ActivateOnEnter<FireAndLightning2>()
            .ActivateOnEnter<DistantClap>()
            //.ActivateOnEnter<HundredfoldHavoc>() just not appearing, unsure why
            .ActivateOnEnter<AratamaForce>();
    }
}
