namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

[SkipLocalsInit]
sealed class TwoHeadedAevisStates : StateMachineBuilder {
    public TwoHeadedAevisStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<PoisonBreath>()
            .ActivateOnEnter<StormsBreath>()
            .ActivateOnEnter<ThunderfrostTempest>()
            .ActivateOnEnter<TwoTerrors>()
            .ActivateOnEnter<HissingReprise>()
            .ActivateOnEnter<TwoHeadedAevisCluster>()
            .ActivateOnEnter<Blaze>()
            .ActivateOnEnter<ArcaneBeacon>();
    }
}
