namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class Ex2GarudaStates : StateMachineBuilder
{
    private Ex2Garuda _module;

    public Ex2GarudaStates(Ex2Garuda module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "Plumes")
            .ActivateOnEnter<EyeOfTheStorm>()
            .ActivateOnEnter<FeatherRain>()
            .Raw.Update = () => _module.PrimaryActor.IsDestroyed || _module.Chirada.Count + _module.Suparna.Count > 0;
        DeathPhase(1, Phase2)
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<MistralShriek>()
            .ActivateOnEnter<FeatherRain>()
            .ActivateOnEnter<Gigastorm>()
            .ActivateOnEnter<SpinyShield>()
            .ActivateOnEnter<Ex2GarudaAI>();
    }

    private void Phase1(uint id)
    {
        // phase 1 is repeated casts of mistral song from various positions; at the beginning there is no eye of the storm, but it appears around first song cast
        // after every song, there are a bunch of plume adds that have to be killed
        // phase lasts until some hp threshold or until some time passes, at which point eye of the storm disappears and large adds spawn
        MistralSong<MistralSong1>(id, 34.7f);
        MistralSong<MistralSong2>(id + 0x10000, 63.6f);
        SimpleState(id + 0x20000, 37.6f, "Forced phase end");
    }

    private void MistralSong<Song>(uint id, float delay) where Song : MistralSong, new()
    {
        Targetable(id, false, delay, "Disappear")
            .ActivateOnEnter<DownburstBoss>()
            .ActivateOnEnter<Slipstream>()
            .ActivateOnEnter<Ex2GarudaAI>()
            .DeactivateOnExit<DownburstBoss>()
            .DeactivateOnExit<Slipstream>()
            .DeactivateOnExit<Ex2GarudaAI>();
        Targetable(id + 1, true, 6.8f, "Reappear")
            .ActivateOnEnter<Song>();
        Cast(id + 0x10, AID.MistralSong, 0.6f, 1, "Hide behind pillars")
            .DeactivateOnExit<Song>();
    }

    private void Phase2(uint id)
    {
        // phase 2 has several 'subphases'
        // 1. chirada + suparna adds, no eye of the storm or whirlwinds, monoliths - at the end boss casts aerial blast, which is a wipe if monoliths are destroyed
        // 2. eye of the storm + plumes - at the end boss casts mistral shriek, which is a wipe unless mitigated by spiny plume's shield
        // 3. chirada + suparna + spiny plume + whirlwinds - at the end boss casts aerial blast, which is a wipe unless mitigated by spiny plume's shield
        // 2 and 3 then repeat until boss death (or enrage?)
        Subphase2(id, 87.9f);
        Subphase3(id + 0x10000, 58.7f);
        Subphase4(id + 0x20000, 81.1f);
        Subphase3(id + 0x30000, 57.2f);
        Subphase4(id + 0x40000, 80.5f);
        Subphase3(id + 0x50000, 58.6f);
        Subphase4(id + 0x60000, 80.5f); // ?? timings?
        SimpleState(id + 0xFF0000, 1000, "???");
    }

    private void Subphase2(uint id, float delay)
    {
        Targetable(id, false, delay, "Disappear", 60)
            .ActivateOnEnter<DownburstBoss>()
            .ActivateOnEnter<DownburstSuparna>()
            .ActivateOnEnter<DownburstChirada>()
            .ActivateOnEnter<Slipstream>()
            .ActivateOnEnter<FrictionAdds>()
            .ActivateOnEnter<WickedWheel>()
            .DeactivateOnExit<DownburstBoss>()
            .DeactivateOnExit<DownburstSuparna>()
            .DeactivateOnExit<DownburstChirada>()
            .DeactivateOnExit<Slipstream>()
            .DeactivateOnExit<FrictionAdds>()
            .DeactivateOnExit<WickedWheel>();
        Targetable(id + 1, true, 6.8f, "Reappear");
        Cast(id + 0x10, AID.AerialBlast, 0.6f, 4, "Raidwide");
    }

    private void Subphase3(uint id, float delay)
    {
        Targetable(id, false, delay, "Disappear")
            .ActivateOnEnter<EyeOfTheStorm>()
            .ActivateOnEnter<DownburstBoss>()
            .ActivateOnEnter<Slipstream>()
            .DeactivateOnExit<DownburstBoss>()
            .DeactivateOnExit<Slipstream>();
        Targetable(id + 1, true, 7.5f, "Reappear");
        Cast(id + 0x10, AID.MistralShriek, 0.6f, 3, "Raidwide");
        Targetable(id + 0x20, false, 2.3f, "Disappear")
            .DeactivateOnExit<EyeOfTheStorm>();
        Targetable(id + 0x21, true, 6.8f, "Reappear");
    }

    private void Subphase4(uint id, float delay)
    {
        Targetable(id, false, delay, "Disappear")
            .ActivateOnEnter<GreatWhirlwind>()
            .ActivateOnEnter<DownburstBoss>()
            .ActivateOnEnter<DownburstSuparna>()
            .ActivateOnEnter<DownburstChirada>()
            .ActivateOnEnter<Slipstream>()
            .ActivateOnEnter<FrictionAdds>()
            .ActivateOnEnter<WickedWheel>()
            .DeactivateOnExit<DownburstBoss>()
            .DeactivateOnExit<DownburstSuparna>()
            .DeactivateOnExit<DownburstChirada>()
            .DeactivateOnExit<Slipstream>()
            .DeactivateOnExit<FrictionAdds>()
            .DeactivateOnExit<WickedWheel>();
        Targetable(id + 1, true, 6.8f, "Reappear")
            .DeactivateOnExit<GreatWhirlwind>();
        Cast(id + 0x10, AID.AerialBlast, 0.6f, 4, "Raidwide");
    }
}
