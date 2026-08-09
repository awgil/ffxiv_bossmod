namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

[SkipLocalsInit]
sealed class SwordDancerStates : StateMachineBuilder
{
    public SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwordStormCast>()
            .ActivateOnEnter<RushShort1>()
            .ActivateOnEnter<RushShort2>()
            .ActivateOnEnter<TurnInner>()
            .ActivateOnEnter<TurnOuter>()
            .ActivateOnEnter<MartialMystique>()
            .ActivateOnEnter<Cyclosword>()
            .ActivateOnEnter<SwordDance>()
            .ActivateOnEnter<Pierce>()
            .ActivateOnEnter<Steelsbreath>()
            .ActivateOnEnter<RushSurgesword>();
    }
}
