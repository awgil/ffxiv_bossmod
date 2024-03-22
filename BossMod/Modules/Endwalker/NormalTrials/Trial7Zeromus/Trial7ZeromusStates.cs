namespace BossMod.Endwalker.NormalTrials.Trial7Zeromus
{
    class Trial7ZeromusStates : StateMachineBuilder
    {
        public Trial7ZeromusStates(BossModule module) : base(module)
        {
           SimplePhase(0, id => { SimpleState(id, 10000, "Enrage"); }, "Single phase")
                .ActivateOnEnter<VisceralWhirl>()
                .ActivateOnEnter<VoidBio>()
                .ActivateOnEnter<BondsOfDarkness>()
                .ActivateOnEnter<DarkDivides>()
                .ActivateOnEnter<MeteorImpactCharge>()
                .ActivateOnEnter<MeteorImpactProximity>()
                .ActivateOnEnter<MeteorImpactExplosion>()
                .ActivateOnEnter<SableThread>()
                .ActivateOnEnter<NostalgiaDimensionalSurge>()
                .ActivateOnEnter<NostalgiaDimensionalSurgeSmall>()
                .ActivateOnEnter<NostalgiaDimensionalSurgeLine>()
                .ActivateOnEnter<Nostalgia>()
                .ActivateOnEnter<ChasmicNails>()
                .ActivateOnEnter<FlowOfTheAbyssAkhRhai>()
                .ActivateOnEnter<FlowOfTheAbyssSpreadStack>()
                .ActivateOnEnter<FlowOfTheAbyssDimensionalSurge>()
                .ActivateOnEnter<Nox>()
                .ActivateOnEnter<FlareScald>()
                .ActivateOnEnter<FlareTowers>()
                .ActivateOnEnter<DarkMatter>()
                .ActivateOnEnter<ForkedLightningDarkBeckons>()
                .ActivateOnEnter<FracturedEventide>()
                .ActivateOnEnter<BlackHole>()
                .ActivateOnEnter<AbyssalEchoes>()
                .ActivateOnEnter<BigBangPuddle>()
                .ActivateOnEnter<BigBangSpread>()
                .ActivateOnEnter<BigCrunchPuddle>()
                .ActivateOnEnter<BigCrunchSpread>()
                .ActivateOnEnter<UnknownBlackHole>()
                .ActivateOnEnter<BigCrunchSpread>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed;
        }
    }
}
