namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

class V03DaryaTheSeaMaidStates : StateMachineBuilder {
    public V03DaryaTheSeaMaidStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PiercingPlunge>()
            .ActivateOnEnter<FamiliarCall>()
            .ActivateOnEnter<SunkenTreasure>()
            .ActivateOnEnter<Hydrobullet>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<AquaSpear>()
            .ActivateOnEnter<SeaShackles>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<SwimmingInTheAir>()
            .ActivateOnEnter<SwimmingInTheAirSpread>()
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<SurgingCurrent>();
    }
}
