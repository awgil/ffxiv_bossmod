namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

class V03DaryaTheSeaMaidStates : StateMachineBuilder {
    public V03DaryaTheSeaMaidStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PiercingPlunge>()
            .ActivateOnEnter<FamiliarCall>()
            .ActivateOnEnter<SunkenTreasure>()
            .ActivateOnEnter<AlluringOrderRaidwide>()
            .ActivateOnEnter<AlluringOrderForcedMovement>()
            .ActivateOnEnter<Hydrobullet>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<AquaSpear>()
            .ActivateOnEnter<SeaShackles>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<SwimmingInTheAir>()
            .ActivateOnEnter<SwimingInTheAirStackSpread>()
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<SurgingCurrent>()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<RecedingTwinTides>();
    }
}
