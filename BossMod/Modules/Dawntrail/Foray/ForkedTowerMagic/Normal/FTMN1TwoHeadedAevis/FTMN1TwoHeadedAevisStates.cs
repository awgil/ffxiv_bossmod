namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

[SkipLocalsInit]
sealed class TwoHeadedAevisStates : StateMachineBuilder
{
    public TwoHeadedAevisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<PoisonBreath>()
            .ActivateOnEnter<StormsBreath>()
            .ActivateOnEnter<ThunderfrostTempest>()
            .ActivateOnEnter<TwoTerrors>()
            .ActivateOnEnter<IceCluster>()
            .ActivateOnEnter<LightningCluster>()
            .ActivateOnEnter<HypothermalCombustionShock>()
            .ActivateOnEnter<HissingReprise>()
            .ActivateOnEnter<BlazeLoop>()
            .ActivateOnEnter<ArcaneBeacon>()
            .ActivateOnEnter<Archaeofury1>()
            .ActivateOnEnter<Archaeofury2>();
    }
}
