namespace BossMod.Endwalker.Variant.V02MR.V021Yozakura;

class V021YozakuraStates : StateMachineBuilder
{
    public V021YozakuraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //Right No Dogu
            .ActivateOnEnter<RootArrangement>()
            //Right Dogu
            .ActivateOnEnter<Witherwind>()
            //Left Windy
            .ActivateOnEnter<WindblossomWhirl2>()
            .ActivateOnEnter<WindblossomWhirl3>()
            .ActivateOnEnter<LevinblossomStrike2>()
            .ActivateOnEnter<DriftingPetals>()
            //Left Rainy
            .ActivateOnEnter<MudrainAOE>()
            .ActivateOnEnter<IcebloomRest>()
            .ActivateOnEnter<ShadowflightAOE>()
            .ActivateOnEnter<MudPieAOE>()
            //Middle Rope Pulled
            .ActivateOnEnter<FireblossomFlare2>()
            .ActivateOnEnter<ArtOfTheFluff1>()
            .ActivateOnEnter<ArtOfTheFluff2>()
            //Middle Rope Unpulled
            .ActivateOnEnter<LevinblossomLance>()
            .ActivateOnEnter<TatamiGaeshiAOE>()
            //Standard
            .ActivateOnEnter<KugeRantsui>()
            .ActivateOnEnter<OkaRanman>()
            .ActivateOnEnter<SealOfTheFireblossom1>()
            .ActivateOnEnter<SealOfTheWindblossom1>()
            .ActivateOnEnter<SealOfTheRainblossom1>()
            .ActivateOnEnter<SealOfTheLevinblossom1>()
            .ActivateOnEnter<SeasonOfFire>()
            .ActivateOnEnter<SeasonOfWater>()
            .ActivateOnEnter<SeasonOfLightning>()
            .ActivateOnEnter<SeasonOfEarth>()
            .ActivateOnEnter<ArtOfTheFireblossom>()
            .ActivateOnEnter<ArtOfTheWindblossom>();
    }
}
