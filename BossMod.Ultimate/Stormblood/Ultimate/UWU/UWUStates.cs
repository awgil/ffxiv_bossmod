namespace BossMod.Stormblood.Ultimate.UWU;

class UWUStates : StateMachineBuilder
{
    private readonly UWU _module;

    public UWUStates(UWU module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1Garuda, "P1: Garuda")
            .ActivateOnEnter<P1Plumes>()
            .ActivateOnEnter<P1Gigastorm>()
            .ActivateOnEnter<P1GreatWhirlwind>() // TODO: not sure about this...
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HPMP.CurHP <= 1 && !Module.PrimaryActor.IsTargetable;
        SimplePhase(1, Phase2Ifrit, "P2: Ifrit")
            .ActivateOnEnter<P2Nails>()
            .ActivateOnEnter<P2InfernalFetters>()
            .ActivateOnEnter<P2SearingWind>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (_module.Ifrit()?.HPMP.CurHP <= 1 && !(_module.Ifrit()?.IsTargetable ?? true));
        SimplePhase(2, Phase3Titan, "P3: Titan")
            .ActivateOnEnter<P3Geocrush2>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (_module.Titan()?.HPMP.CurHP <= 1 && !(_module.Titan()?.IsTargetable ?? true));
        SimplePhase(3, Phase4LahabreaUltima, "P4: Lahabrea + Ultima")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (_module.Ultima()?.CastInfo?.IsSpell(AID.UltimateSuppression) ?? false);
        SimplePhase(4, Phase5Ultima, "P4: Ultima - suppression to enrage")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (_module.Ultima()?.IsDead ?? false);
    }

    private void Phase1Garuda(uint id)
    {
        P1SlipstreamMistralSong(id, 5.2f);
        P1Adds1(id + 0x10000, 8.5f);
        P1Frictions(id + 0x20000, 7.4f);
        P1GarudaFeatherRainRaidwide(id + 0x30000, 12.1f, AID.AerialBlast);
        P1SistersFeatherRain(id + 0x40000, 13.7f);
        P1MistralSongEyeOfTheStormWickedWheelFeatherRain(id + 0x50000, 4.4f);
        P1Adds2(id + 0x60000, 5.7f);
        // awakening happens here...
        P1SistersFeatherRain(id + 0x70000, 2.0f);
        P1Slipstream(id + 0x80000, 10.0f);
        P1AwakenedWickedWheelDownburst(id + 0x90000, 5.5f);
        P1Slipstream(id + 0xA0000, 5.5f);
        P1Enrage(id + 0xB0000, 9.4f);
    }

    private State P1Slipstream(uint id, float delay)
    {
        return ActorCast(id, _module.Garuda, AID.Slipstream, delay, 2.5f, true, "Slipstream")
            .ActivateOnEnter<P1Slipstream>()
            .DeactivateOnExit<P1Slipstream>();
    }

    private void P1SlipstreamMistralSong(uint id, float delay)
    {
        ActorCastStart(id, _module.Garuda, AID.Slipstream, delay, true)
            .ActivateOnEnter<P1MistralSongBoss>(); // icon appears slightly before cast start
        ActorCastEnd(id + 1, _module.Garuda, 2.5f, true, "Slipstream")
            .ActivateOnEnter<P1Slipstream>()
            .DeactivateOnExit<P1Slipstream>();
        ComponentCondition<P1MistralSongBoss>(id + 0x10, 2.7f, comp => comp.NumCasts > 0, "Mistral song")
            .DeactivateOnExit<P1MistralSongBoss>();
        // note: mistral song leaves a puddle which explodes 3 times, with 3.1s delay between casts and 3s cast time; it overlaps with logically-next mechanic
        //ComponentCondition<P1GreatWhirlwind>(id + 0x20, 3.1f, comp => comp.Casters.Count > 0)
        //    .ActivateOnEnter<P1GreatWhirlwind>();
        //ComponentCondition<P1GreatWhirlwind>(id + 0x21, 3, comp => comp.Casters.Count == 0, "Puddle")
        //    .DeactivateOnExit<P1GreatWhirlwind>();
    }

    private void P1Adds1(uint id, float delay)
    {
        ComponentCondition<P1Plumes>(id, delay, comp => comp.Active, "Adds");
        P1Slipstream(id + 0x10, 1.8f);
        ComponentCondition<P1Downburst>(id + 0x20, 3.5f, comp => comp.NumCasts > 0, "Cleave + Cyclone 1") // note: cyclone happens ~0.1s after cleave
            .ActivateOnEnter<P1Downburst>()
            .DeactivateOnExit<P1Downburst>();
        // great whirlwind disappears somewhere here...

        P1GarudaFeatherRainRaidwide(id + 0x1000, 7.3f, AID.MistralShriek, "Cyclone 2 + Feathers"); // note: cyclone happens ~0.6s before feathers
    }

    private void P1Frictions(uint id, float delay)
    {
        ActorCast(id, _module.Garuda, AID.Friction, delay, 2, true, "Friction 1");
        ActorCast(id + 0x10, _module.Garuda, AID.Friction, 4.2f, 2, true, "Friction 2");
    }

    private void P1GarudaFeatherRainRaidwide(uint id, float delay, AID raidwide, string name = "Feathers")
    {
        ActorTargetable(id, _module.Garuda, false, delay, "Disappear");
        ComponentCondition<P1FeatherRain>(id + 1, 1.6f, comp => comp.CastsActive)
            .ActivateOnEnter<P1FeatherRain>();
        ComponentCondition<P1FeatherRain>(id + 2, 1, comp => !comp.CastsActive, name)
            .DeactivateOnExit<P1FeatherRain>();
        ActorTargetable(id + 3, _module.Garuda, true, 1.7f, "Reappear");

        ActorCast(id + 0x10, _module.Garuda, raidwide, 0.1f, 3, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1SistersFeatherRain(uint id, float delay)
    {
        ComponentCondition<P1FeatherRain>(id, delay, comp => comp.CastsPredicted, "Feathers bait")
            .ActivateOnEnter<P1FeatherRain>();
        ComponentCondition<P1FeatherRain>(id + 1, 1.5f, comp => comp.CastsActive);
        ComponentCondition<P1FeatherRain>(id + 2, 1, comp => !comp.CastsActive, "Feathers")
            .DeactivateOnExit<P1FeatherRain>();
    }

    private void P1MistralSongEyeOfTheStormWickedWheelFeatherRain(uint id, float delay)
    {
        ActorCastStart(id, _module.Garuda, AID.WickedWheel, delay, true)
            .ActivateOnEnter<P1MistralSongAdds>() // icons on targets ~2.7s before cast start
            .ActivateOnEnter<P1EyeOfTheStorm>(); // cast starts ~0.2s before
        ComponentCondition<P1MistralSongAdds>(id + 1, 2.5f, comp => comp.NumCasts > 0, "Mistral song")
            .ActivateOnEnter<P1WickedWheel>()
            .DeactivateOnExit<P1MistralSongAdds>();
        ActorCastEnd(id + 2, _module.Garuda, 0.5f, true)
            .DeactivateOnExit<P1EyeOfTheStorm>() // finishes ~0.2s before
            .DeactivateOnExit<P1WickedWheel>();

        P1SistersFeatherRain(id + 0x10, 1.6f);
        ComponentCondition<P1GreatWhirlwind>(id + 0x20, 1.5f, comp => comp.Casters.Count == 0, "Whirlwind");
    }

    private void P1Adds2(uint id, float delay)
    {
        ComponentCondition<P1Plumes>(id, delay, comp => comp.Active, "Adds");
        P1Slipstream(id + 0x10, 7.8f)
            .ActivateOnEnter<P1EyeOfTheStorm>() // starts ~0.9s after cast start
            .ActivateOnEnter<P1Mesohigh>(); // tethers appear ~0.9s after cast start
        ComponentCondition<P1Downburst>(id + 0x20, 3.5f, comp => comp.NumCasts > 0, "Cleave + Mesohigh")
            .ActivateOnEnter<P1Downburst>()
            .DeactivateOnExit<P1Downburst>()
            .DeactivateOnExit<P1EyeOfTheStorm>() // ends ~2.2s before cleave
            .DeactivateOnExit<P1Mesohigh>(); // resolves at the same time as cleave
    }

    private void P1AwakenedWickedWheelDownburst(uint id, float delay)
    {
        ActorCast(id, _module.Garuda, AID.WickedWheel, delay, 3, true, "Out")
            .ActivateOnEnter<P1WickedWheel>();
        ComponentCondition<P1WickedWheel>(id + 2, 2.1f, comp => comp.NumCasts >= 2 || comp.Sources.Count == 0 && _module.WorldState.CurrentTime >= comp.AwakenedResolve, "In") // complicated condition handles fucked up awakening
            .DeactivateOnExit<P1WickedWheel>();
        ComponentCondition<P1Downburst>(id + 0x10, 2.8f, comp => comp.NumCasts > 0, "Cleave")
            .ActivateOnEnter<P1Downburst>()
            .DeactivateOnExit<P1Downburst>();
    }

    private void P1Enrage(uint id, float delay)
    {
        // note: similar to P1GarudaFeatherRainRaidwide, except that garuda doesn't reappear
        ActorTargetable(id, _module.Garuda, false, delay, "Disappear");
        ComponentCondition<P1FeatherRain>(id + 1, 1.6f, comp => comp.CastsActive)
            .ActivateOnEnter<P1FeatherRain>();
        ComponentCondition<P1FeatherRain>(id + 2, 1, comp => !comp.CastsActive)
            .DeactivateOnExit<P1FeatherRain>();
        ActorCast(id + 0x10, _module.Garuda, AID.AerialBlast, 1.8f, 3, true, "Enrage");
    }

    private void Phase2Ifrit(uint id)
    {
        P2CrimsonCycloneRadiantPlumeHellfire(id, 4.2f);
        P2VulcanBurst(id + 0x10000, 8.2f);
        P2Incinerate(id + 0x20000, 2.8f);
        P2Nails(id + 0x30000, 7.2f);
        P2InfernoHowlEruptionCrimsonCyclone(id + 0x40000, 6.3f);
        P2Incinerate(id + 0x50000, 4.2f);

        // TODO: eruptions > flaming crush > enrage
        SimpleState(id + 0xFF0000, 100, "???");
    }

    private void P2CrimsonCycloneRadiantPlumeHellfire(uint id, float delay)
    {
        ComponentCondition<P2CrimsonCyclone>(id, delay, comp => comp.CastsPredicted)
            .ActivateOnEnter<P2CrimsonCyclone>()
            .ActivateOnEnter<P2RadiantPlume>(); // casts starts at the same time as charge (2.1s)
        ComponentCondition<P2CrimsonCyclone>(id + 0x10, 5.1f, comp => comp.NumCasts > 0, "Charge")
            .DeactivateOnExit<P2CrimsonCyclone>();
        ComponentCondition<P2RadiantPlume>(id + 0x20, 1, comp => comp.NumCasts > 0, "Plumes")
            .DeactivateOnExit<P2RadiantPlume>();

        Condition(id + 0x100, 3.2f, () => _module.Ifrit() != null, "Ifrit appears");
        ActorCast(id + 0x200, _module.Ifrit, AID.Hellfire, 0.1f, 3, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2VulcanBurst(uint id, float delay)
    {
        ComponentCondition<P2VulcanBurst>(id, delay, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<P2VulcanBurst>()
            .DeactivateOnExit<P2VulcanBurst>();
    }

    private void P2Incinerate(uint id, float delay)
    {
        ComponentCondition<P2Incinerate>(id, delay, comp => comp.NumCasts > 0, "Incinerate 1")
            .ActivateOnEnter<P2Incinerate>();
        ComponentCondition<P2Incinerate>(id + 1, 3.1f, comp => comp.NumCasts > 1, "Incinerate 2");
        ComponentCondition<P2Incinerate>(id + 2, 4.1f, comp => comp.NumCasts > 2, "Incinerate 3")
            .DeactivateOnExit<P2Incinerate>();
    }

    private void P2Nails(uint id, float delay)
    {
        ComponentCondition<P2Nails>(id, delay, comp => comp.Active, "Nails spawn");
        // +5.0s: fetters
        ActorCast(id + 0x100, _module.Ifrit, AID.InfernoHowl, 5.2f, 2, true, "Searing wind start");
        ActorCastStart(id + 0x110, _module.Ifrit, AID.Eruption, 3.2f, true, "Eruption baits")
            .ActivateOnEnter<P2Eruption>(); // activate early to show bait hints
        ActorCastEnd(id + 0x111, _module.Ifrit, 2.5f, true);
        ComponentCondition<P2Eruption>(id + 0x120, 6.5f, comp => comp.NumCasts >= 8)
            .DeactivateOnExit<P2Eruption>();
        ComponentCondition<P2SearingWind>(id + 0x130, 5.8f, comp => !comp.Active, "Searing wind end");

        ActorTargetable(id + 0x200, _module.Ifrit, false, 5.1f, "Disappear");
        ActorTargetable(id + 0x201, _module.Ifrit, true, 4.3f, "Reappear");
        ActorCast(id + 0x210, _module.Ifrit, AID.Hellfire, 0.1f, 3, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2InfernoHowlEruptionCrimsonCyclone(uint id, float delay)
    {
        ActorCast(id, _module.Ifrit, AID.InfernoHowl, delay, 2, true, "Searing wind 1 start");
        ActorCastStart(id + 0x10, _module.Ifrit, AID.Eruption, 3.2f, true, "Eruption baits")
            .ActivateOnEnter<P2Eruption>(); // activate early to show bait hints
        ActorCastEnd(id + 0x21, _module.Ifrit, 2.5f, true);
        // +0.3s: searing wind 1
        ComponentCondition<P2CrimsonCyclone>(id + 0x30, 2.5f, comp => comp.CastsPredicted)
            .ActivateOnEnter<P2CrimsonCyclone>();
        // +3.8s: searing wind 2
        ComponentCondition<P2Eruption>(id + 0x40, 4, comp => comp.NumCasts >= 8)
            .DeactivateOnExit<P2Eruption>();
        ComponentCondition<P2CrimsonCyclone>(id + 0x50, 1.0f, comp => comp.NumCasts > 0, "Charges")
            .DeactivateOnExit<P2CrimsonCyclone>();

        ActorCast(id + 0x1000, _module.Ifrit, AID.InfernoHowl, 2.9f, 2, true, "Searing wind 2 start");
        // +0.0s: searing wind 3 (on first target only)
        ComponentCondition<P2FlamingCrush>(id + 0x1010, 5.1f, comp => comp.Active)
            .ActivateOnEnter<P2FlamingCrush>();
        // +0.8s: searing wind 4 (1st target) / 1 (2nd target)
        ComponentCondition<P2FlamingCrush>(id + 0x1020, 5.1f, comp => !comp.Active, "Stack")
            .DeactivateOnExit<P2FlamingCrush>();
        // +1.7s: searing wind 5 (1st target) / 2 (2nd target)

        ActorTargetable(id + 0x2000, _module.Ifrit, false, 4.1f, "Disappear");
        ComponentCondition<P2CrimsonCyclone>(id + 0x2001, 2.3f, comp => comp.CastsPredicted)
            .ActivateOnEnter<P2CrimsonCyclone>();
        // +2.2s: PATE 1E43 on 4 ifrits
        // +3.6s: searing wind 3 (2nd target)
        // +4.4s: first charge start (others are staggered by 1.4s); 3s cast duration, ~2.2s after awakened charge we get 2 'cross' charges
        // +9.6s: searing wind 4 (2nd target)
        // +15.6s: searing wind 5 (2nd target)
        ActorTargetable(id + 0x2100, _module.Ifrit, true, 13.5f, "Awakened charges + Reappear")
            .DeactivateOnExit<P2CrimsonCyclone>();
    }

    private void Phase3Titan(uint id)
    {
        P3GeocrushEarthenFury(id, 2.2f);
        P3RockBusterMountainBuster(id + 0x10000, 8.2f, false);
        P3WeightOfTheLandGeocrush(id + 0x20000, 2.1f);
        P3UpheavalGaolsLandslideTumult(id + 0x30000, 2.2f);
        P3WeightOfTheLandLandslideAwakened(id + 0x40000, 5.1f);
        P3Geocrush3(id + 0x50000, 4.4f);
        P3LandslideAwakened(id + 0x60000, 12.3f);
        P3Tumult(id + 0x70000, 3.2f, 6);
        P3RockBusterMountainBuster(id + 0x80000, 2.2f, true);
        P3TripleWeightOfTheLandLandslideAwakenedBombs(id + 0x90000, 4.2f);
        P3RockBusterMountainBuster(id + 0xA0000, 4.2f, true);
        P3TripleWeightOfTheLandTumult(id + 0xB0000, 2.1f);
        P3Enrage(id + 0xC0000, 8);
    }

    private void P3RockBusterMountainBuster(uint id, float delay, bool longDelay)
    {
        ComponentCondition<P3RockBuster>(id, delay, comp => comp.NumCasts > 0, "Cleave 1")
            .ActivateOnEnter<P3RockBuster>()
            .DeactivateOnExit<P3RockBuster>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P3MountainBuster>(id + 1, longDelay ? 4.1f : 3.1f, comp => comp.NumCasts > 0, "Cleave 2")
            .ActivateOnEnter<P3MountainBuster>()
            .DeactivateOnExit<P3MountainBuster>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    // note: keeps component active for second and maybe third sets
    private State P3WeightOfTheLandFirst(uint id, float delay, string name)
    {
        ActorCast(id, _module.Titan, AID.WeightOfTheLand, delay, 2.5f, true)
            .ActivateOnEnter<P3WeightOfTheLand>();
        return ComponentCondition<P3WeightOfTheLand>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, name);
    }

    private State P3LandslideNormal(uint id, float delay, string name = "Landslide")
    {
        return ActorCast(id, _module.Titan, AID.LandslideBoss, delay, 2.2f, true, name)
            .ActivateOnEnter<P3Landslide>()
            .DeactivateOnExit<P3Landslide>();
    }

    private State P3LandslideAwakened(uint id, float delay)
    {
        ActorCastMulti(id, _module.Titan, [AID.LandslideBoss, AID.LandslideBossAwakened], delay, 2.2f, true, "Landslide (awakened)")
            .ActivateOnEnter<P3Landslide>();
        return ComponentCondition<P3Landslide>(id + 0x10, 2, comp => comp.Awakened ? comp.NumCasts >= 10 : (comp.NumCasts >= 5 && Module.WorldState.CurrentTime >= comp.PredictedActivation), "Landslide second hit")
            .DeactivateOnExit<P3Landslide>();
    }

    private State P3GeocrushSide(uint id, float delay)
    {
        ActorTargetable(id, _module.Titan, false, delay, "Disappear");
        ActorCast(id + 1, _module.Titan, AID.Geocrush2, 2.2f, 3, true, "Proximity");
        return ActorTargetable(id + 3, _module.Titan, true, 2.4f, "Reappear");
    }

    private State P3Tumult(uint id, float delay, uint numHits)
    {
        ComponentCondition<P3Tumult>(id, delay, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<P3Tumult>()
            .SetHint(StateMachine.StateHint.Raidwide);
        return ComponentCondition<P3Tumult>(id + numHits - 1, 0.1f + 1.1f * (numHits - 1), comp => comp.NumCasts >= numHits, $"Raidwide {numHits}")
            .DeactivateOnExit<P3Tumult>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P3GeocrushEarthenFury(uint id, float delay)
    {
        ActorCast(id, _module.Titan, AID.Geocrush1, delay, 3, true, "Proximity")
            .ActivateOnEnter<P3Geocrush1>()
            .DeactivateOnExit<P3Geocrush1>();
        ActorTargetable(id + 0x10, _module.Titan, true, 2.4f, "Titan appears");
        ActorCast(id + 0x20, _module.Titan, AID.EarthenFury, 0.1f, 3, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P3WeightOfTheLandGeocrush(uint id, float delay)
    {
        P3WeightOfTheLandFirst(id, delay, "Puddles x2");
        P3GeocrushSide(id + 0x1000, 3)
            .DeactivateOnExit<P3WeightOfTheLand>(); // note: weight of the land typically ends ~0.3s after cast start, but sometimes slightly earlier
    }

    private void P3UpheavalGaolsLandslideTumult(uint id, float delay)
    {
        ActorCast(id, _module.Titan, AID.Upheaval, delay, 4, true, "Knockback")
            .ActivateOnEnter<P3Upheaval>()
            .ActivateOnEnter<P3Burst>() // bombs appear ~0.2s after cast start
            .DeactivateOnExit<P3Upheaval>();
        ComponentCondition<P3Gaols>(id + 0x10, 2.1f, comp => comp.CurState == P3Gaols.State.TargetSelection)
            .ActivateOnEnter<P3Gaols>();
        ComponentCondition<P3Burst>(id + 0x11, 0.4f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<P3Burst>();

        P3LandslideNormal(id + 0x20, 1.8f, "Landslide 1")
            .ActivateOnEnter<P3Burst>(); // extra bomb appears ~0.1s before landslide start
        // +0.6s: fetters for gaols
        P3LandslideNormal(id + 0x30, 2.3f, "Landslide 2")
            .DeactivateOnExit<P3Burst>(); // bomb explodes ~0.5s before landslide end

        P3Tumult(id + 0x1000, 2.1f, 8)
            .DeactivateOnExit<P3Gaols>(); // if everything is done correctly, last gaol explodes ~0.7s before raidwide
    }

    private void P3WeightOfTheLandLandslideAwakened(uint id, float delay)
    {
        // TODO: gaol voidzones disappear during cast
        P3WeightOfTheLandFirst(id, delay, "Puddles x2");

        // titan normally awakens here...
        P3LandslideAwakened(id + 0x1000, 2.8f)
            .DeactivateOnExit<P3WeightOfTheLand>(); // note: weight of the land typically ends ~0.3s after cast start
    }

    private void P3TripleWeightOfTheLandLandslideAwakenedBombs(uint id, float delay)
    {
        ComponentCondition<P3WeightOfTheLand>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P3WeightOfTheLand>();
        ComponentCondition<P3WeightOfTheLand>(id + 1, 3, comp => comp.NumCasts > 0, "Puddles x3")
            .ActivateOnEnter<P3Burst>(); // bombs appear immediately after first puddles cast start
        // +0.0s: second set of weights start
        // +3.0s: second set of weights end, third set start

        P3LandslideAwakened(id + 0x1000, 3.3f)
            .DeactivateOnExit<P3WeightOfTheLand>(); // third set ends ~1.5s before landslide

        ComponentCondition<P3Burst>(id + 0x2000, 2.1f, comp => comp.NumCasts >= 4, "Central bombs")
            .DeactivateOnExit<P3Burst>();
    }

    private void P3Geocrush3(uint id, float delay)
    {
        P3GeocrushSide(id, delay);
        // +0.1s: rock throw
        // +5.0s: fetters
        // +7.1s: gaol targetable
        // +14.1s: impact end
    }

    private void P3TripleWeightOfTheLandTumult(uint id, float delay)
    {
        P3WeightOfTheLandFirst(id, delay, "Puddles x3");
        P3Tumult(id + 0x100, 7.6f, 8)
            .DeactivateOnExit<P3WeightOfTheLand>();
    }

    private void P3Enrage(uint id, float delay)
    {
        ActorTargetable(id, _module.Titan, false, delay, "Disappear");
        ActorCast(id + 0x10, _module.Titan, AID.EarthenFury, 4.4f, 3, true, "Enrage");
    }

    private void Phase4LahabreaUltima(uint id)
    {
        P4Lahabrea(id, 9.1f);
        P4BeforePredation(id + 0x10000, 39.7f);
        P4Predation(id + 0x20000, 3.2f);
        P4BeforeAnnihilation(id + 0x30000, 2.0f);
        P4Annihilation(id + 0x40000, 2.3f);
        P4BeforeSuppression(id + 0x50000, 0.7f);
        SimpleState(id + 0x60000, 3.1f, "Suppression start");
    }

    private void P4Lahabrea(uint id, float delay)
    {
        ComponentCondition<P4Freefire>(id, delay, comp => comp.NumCasts > 0, "Proximity")
            .ActivateOnEnter<P4Freefire>()
            .DeactivateOnExit<P4Freefire>();
        ComponentCondition<P4MagitekBits>(id + 0x1000, 2.2f, comp => comp.Active, "Caster LB")
            .ActivateOnEnter<P4MagitekBits>();
        ComponentCondition<P4Blight>(id + 0x2000, 11.5f, comp => comp.NumCasts > 0, "Heal LB", 10) // note: timing variance is extreme (up to 16s), 11.5 is minimal seen
            .ActivateOnEnter<P4Blight>()
            .DeactivateOnExit<P4Blight>();
        ActorTargetable(id + 0x3000, _module.Lahabrea, true, 9.1f, "Melee LB")
            .DeactivateOnExit<P4MagitekBits>(); // long since gone
        ActorCast(id + 0x4000, _module.Ultima, AID.Ultima, 18, 5, true, "Tank LB");
    }

    private void P4BeforePredation(uint id, float delay)
    {
        ActorTargetable(id, _module.Ultima, true, 39.7f, "Ultima appears");
        ActorCast(id + 0x10, _module.Ultima, AID.TankPurge, 0.1f, 4, true, "Raidwide")
            .ActivateOnEnter<P4ViscousAetheroplasmApply>() // show MT hint early
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P4ViscousAetheroplasmApply>(id + 0x20, 2.2f, comp => comp.NumCasts > 0, "Aetheroplasm apply")
            .ActivateOnEnter<P4ViscousAetheroplasmResolve>() // activate early to let component determine aetheroplasm target
            .DeactivateOnExit<P4ViscousAetheroplasmApply>();
        P4HomingLasers(id + 0x30, 3.2f);
        ComponentCondition<P4ViscousAetheroplasmResolve>(id + 0x40, 4.9f, comp => !comp.Active, "Aetheroplasm resolve")
            .DeactivateOnExit<P4ViscousAetheroplasmResolve>();
    }

    private void P4Predation(uint id, float delay)
    {
        ActorCast(id, _module.Ultima, AID.UltimatePredation, delay, 3, true);
        ActorTargetable(id + 0x10, _module.Ultima, false, 4.4f, "Disappear (predation)");

        ComponentCondition<P4UltimatePredation>(id + 0x20, 2.2f, comp => comp.CurState == P4UltimatePredation.State.Predicted)
            .ActivateOnEnter<P4UltimatePredation>()
            .ActivateOnEnter<P4WickedWheel>()
            .ActivateOnEnter<P4CrimsonCyclone>()
            .ActivateOnEnter<P4Landslide>()
            .ActivateOnEnter<P4CeruleumVent>();
        ComponentCondition<P4UltimatePredation>(id + 0x30, 8.1f, comp => comp.CurState == P4UltimatePredation.State.Second, "Predation 1"); // wicked wheel + crimson cyclone + landslides
        // awakened landslided and ceruleum vent happen ~0.1s earlier, awakened cyclone and tornado happen at the same time
        ComponentCondition<P4UltimatePredation>(id + 0x40, 2.1f, comp => comp.CurState == P4UltimatePredation.State.Done, "Predation 2")
            .DeactivateOnExit<P4UltimatePredation>()
            .DeactivateOnExit<P4WickedWheel>()
            .DeactivateOnExit<P4CrimsonCyclone>()
            .DeactivateOnExit<P4Landslide>()
            .DeactivateOnExit<P4CeruleumVent>();

        // PATE happens ~1.5s earlier
        ComponentCondition<P1FeatherRain>(id + 0x51, 4.3f, comp => comp.CastsActive)
            .ActivateOnEnter<P1FeatherRain>();
        ComponentCondition<P1FeatherRain>(id + 0x52, 1, comp => !comp.CastsActive, "Feathers")
            .DeactivateOnExit<P1FeatherRain>();
    }

    private void P4BeforeAnnihilation(uint id, float delay)
    {
        ActorTargetable(id, _module.Ultima, true, delay, "Reappear");
        ActorCast(id + 0x10, _module.Ultima, AID.PrepareIfrit, 3.2f, 2, true);

        ActorCastStart(id + 0x1000, _module.Ifrit, AID.Eruption, 4.3f, false, "Eruption baits")
            .ActivateOnEnter<P2Eruption>(); // activate early to show bait hints
        ActorCastEnd(id + 0x1001, _module.Ifrit, 2.5f, false);
        ActorCast(id + 0x1010, _module.Ultima, AID.PrepareTitan, 2.4f, 2, true);
        ComponentCondition<P2Eruption>(id + 0x1020, 2.1f, comp => comp.NumCasts >= 8)
            .ActivateOnEnter<P2InfernalFetters>() // ~1s after previous cast end
            .DeactivateOnExit<P2Eruption>();

        ActorCast(id + 0x2000, _module.Ultima, AID.RadiantPlumeUltima, 1.1f, 3.2f, true)
            .ActivateOnEnter<P2RadiantPlume>();
        ComponentCondition<P2RadiantPlume>(id + 0x2010, 0.8f, comp => comp.NumCasts > 0, "Outer plumes")
            .ActivateOnEnter<P3Burst>() // first bomb appears ~0.1s after cast end
            .DeactivateOnExit<P2RadiantPlume>();

        ActorCastStart(id + 0x3000, _module.Titan, AID.LandslideBossAwakened, 1.4f, false);
        ActorCastStart(id + 0x3001, _module.Ultima, AID.LandslideUltima, 0.6f, true)
            .ActivateOnEnter<Landslide>();
        ActorCastEnd(id + 0x3002, _module.Titan, 1.6f, false, "Landslides first");
        ActorCastEnd(id + 0x3003, _module.Ultima, 0.6f, true);
        ComponentCondition<Landslide>(id + 0x3004, 1.4f, comp => !comp.CastsActive, "Landslides last")
            .ActivateOnEnter<P4ViscousAetheroplasmApply>() // activate early to show hint for MT
            .DeactivateOnExit<Landslide>();

        // note: there are tumults during these casts; first cast happens ~1.3s after next cast start, 7 total
        ActorCast(id + 0x4000, _module.Ultima, AID.PrepareGaruda, 1.8f, 2, true);
        ComponentCondition<P4ViscousAetheroplasmApply>(id + 0x4010, 2.1f, comp => comp.NumCasts > 0, "Aetheroplasm apply")
            .ActivateOnEnter<P4ViscousAetheroplasmResolve>()
            .DeactivateOnExit<P4ViscousAetheroplasmApply>();

        ComponentCondition<WickedWheel>(id + 0x5000, 2.1f, comp => comp.Sources.Count > 0)
            .ActivateOnEnter<WickedWheel>();
        ActorCastStart(id + 0x5001, _module.Garuda, AID.MistralShriek, 2, false);
        ComponentCondition<WickedWheel>(id + 0x5002, 1, comp => comp.NumCasts > 0, "Wheels")
            .DeactivateOnExit<P3Burst>() // last explosion ~0.8s before wheels cast end
            .DeactivateOnExit<WickedWheel>();
        ActorCastEnd(id + 0x5003, _module.Garuda, 2, false, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x6000, _module.Ultima, AID.HomingLasers, 2, true)
            .ActivateOnEnter<P1FeatherRain>(); // sisters: PATE ~1.1s before cast start
        ComponentCondition<P1FeatherRain>(id + 0x6001, 1.3f, comp => comp.NumCasts > 0, "Feathers 1")
            .ActivateOnEnter<P4HomingLasers>()
            .DeactivateOnExit<P1FeatherRain>();
        ComponentCondition<P4ViscousAetheroplasmResolve>(id + 0x6002, 0.7f, comp => !comp.Active, "Aetheroplasm resolve")
            .ActivateOnEnter<P1FeatherRain>() // garuda: PATE ~0.3s after resolve, but activate earlier just in case...
            .DeactivateOnExit<P4ViscousAetheroplasmResolve>();
        ActorCastEnd(id + 0x6003, _module.Ultima, 0.9f, true, "Tankbuster")
            .DeactivateOnExit<P4HomingLasers>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P1FeatherRain>(id + 0x6004, 1.9f, comp => comp.NumCasts > 0, "Feathers 2")
            .DeactivateOnExit<P1FeatherRain>()
            .DeactivateOnExit<P2InfernalFetters>(); // TODO: it should probably be deactivated earlier...
    }

    private void P4Annihilation(uint id, float delay)
    {
        ActorCast(id, _module.Ultima, AID.UltimateAnnihilation, delay, 3, true);
        ActorTargetable(id + 0x10, _module.Ultima, false, 4.5f, "Disappear (annihilation)");

        ActorTargetable(id + 0x20, _module.Ultima, true, 4.3f, "Reappear")
            .ActivateOnEnter<P3WeightOfTheLand>() // first set starts at the same time as boss becomes targetable
            .ActivateOnEnter<P4FlamingCrush>();
        ComponentCondition<P4FlamingCrush>(id + 0x21, 0, comp => comp.Active); // icon appears at the same time as or slightly after boss becomes targetable

        ComponentCondition<P4FlamingCrush>(id + 0x30, 5.1f, comp => !comp.Active, "Stack")
            .ActivateOnEnter<P1EyeOfTheStorm>() // cast starts ~1.8s before crash resolve
            .DeactivateOnExit<P4FlamingCrush>();

        ComponentCondition<P1Mesohigh>(id + 0x40, 3.2f, comp => comp.NumCasts > 0, "Tether 1 resolve")
            .ActivateOnEnter<P1Mesohigh>() // tether appears ~1.9s before crash resolve, but should be handled after this mechanic
            .ActivateOnEnter<P4UltimateAnnihilation>() // first orb spawns ~1.9s before tether resolve, then new orb spawns every 6 sec
            .ActivateOnEnter<P2SearingWind>() // inferno howl cast starts ~1.1s before tether resolve
            .DeactivateOnExit<P1EyeOfTheStorm>() // cast ends ~2s before tether resolve
            .DeactivateOnExit<P1Mesohigh>();

        ComponentCondition<P1FeatherRain>(id + 0x50, 2.1f, comp => comp.CastsPredicted, "Feathers bait")
            .ActivateOnEnter<P1FeatherRain>()
            .DeactivateOnExit<P3WeightOfTheLand>(); // last set ends ~1.3s before feathers PATE

        ActorCastStart(id + 0x60, _module.Ifrit, AID.CrimsonCyclone, 4.1f, false);
        ActorCastStart(id + 0x61, _module.Titan, AID.LandslideBossAwakened, 1.5f, false)
            .ActivateOnEnter<P2CrimsonCyclone>() // TODO: proper activation time for prediction? PATE happens extremely early, but we don't want drawing hints too early
            .DeactivateOnExit<P1FeatherRain>();// usually resolves before crimson cyclone cast start, but sometimes slightly after?..
        ActorCastEnd(id + 0x62, _module.Ifrit, 1.5f, false, "Diag charge")
            .ActivateOnEnter<Landslide>(); // TODO: prediction PATE happens ~1.6s before crimson cyclone cast start...
        ActorCastEnd(id + 0x63, _module.Titan, 0.7f, false, "Landslide 1");
        ComponentCondition<P2CrimsonCyclone>(id + 0x64, 1.4f, comp => !comp.CastsPredicted)
            .DeactivateOnExit<P2CrimsonCyclone>();
        ComponentCondition<Landslide>(id + 0x65, 0.7f, comp => !comp.CastsActive)
            .DeactivateOnExit<Landslide>();

        ActorCastStart(id + 0x70, _module.Ultima, AID.TankPurge, 4.3f, true)
            .ActivateOnEnter<P1Mesohigh>() // tether appears ~3.9s before cast start
            .ActivateOnEnter<P1EyeOfTheStorm>() // eots starts ~4.2s before cast start
            .DeactivateOnExit<P1EyeOfTheStorm>(); // second eots ends ~1.2s before cast start
        ComponentCondition<P1Mesohigh>(id + 0x71, 1.1f, comp => comp.NumCasts > 0, "Tether 2 resolve")
            .DeactivateOnExit<P1Mesohigh>();
        ComponentCondition<P1FeatherRain>(id + 0x82, 2.1f, comp => comp.CastsPredicted, "Feathers bait")
            .ActivateOnEnter<P1FeatherRain>();
        ActorCastEnd(id + 0x73, _module.Ultima, 0.7f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1FeatherRain>(id + 0x74, 1.8f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<P1FeatherRain>();

        ActorTargetable(id + 0x80, _module.Ultima, false, 2.3f, "Disappear");
        ActorTargetable(id + 0x81, _module.Ultima, true, 4.3f, "Reappear");

        ComponentCondition<P2SearingWind>(id + 0x90, 2.3f, comp => !comp.Active)
            .ActivateOnEnter<P1EyeOfTheStorm>() // cast starts ~1.9s before searing wind resolve
            .DeactivateOnExit<P2SearingWind>();
        ComponentCondition<P1EyeOfTheStorm>(id + 0x91, 1.1f, comp => comp.NumCasts > 0, "Annihilation resolve")
            .DeactivateOnExit<P4UltimateAnnihilation>() // orbs should really be soaked by this point...
            .DeactivateOnExit<P1EyeOfTheStorm>();
    }

    private void P4BeforeSuppression(uint id, float delay)
    {
        P4HomingLasers(id, delay);

        ActorCastStart(id + 0x100, _module.Ultima, AID.RadiantPlumeUltima, 7.3f, true)
            .ActivateOnEnter<P1EyeOfTheStorm>(); // eots starts ~2.1s before plumes cast start
        ActorCastEnd(id + 0x101, _module.Ultima, 3.2f, true)
            .ActivateOnEnter<P2RadiantPlume>()
            .ActivateOnEnter<P4DiffractiveLaser>() // show hint early...
            .DeactivateOnExit<P1EyeOfTheStorm>(); // eots ends ~0.9s after plumes cast start
        ComponentCondition<P2RadiantPlume>(id + 0x102, 0.8f, comp => comp.NumCasts > 0, "Inner plumes")
            .DeactivateOnExit<P2RadiantPlume>();

        ComponentCondition<P4DiffractiveLaser>(id + 0x200, 2.5f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<P4DiffractiveLaser>();

        ComponentCondition<P1EyeOfTheStorm>(id + 0x300, 3.0f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P1EyeOfTheStorm>()
            .ActivateOnEnter<P4VulcanBurst>();
        ComponentCondition<P4VulcanBurst>(id + 0x301, 1.0f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<P4VulcanBurst>();
        ComponentCondition<P1EyeOfTheStorm>(id + 0x302, 2.0f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<P1EyeOfTheStorm>();

        P4HomingLasers(id + 0x400, 2.8f);

        ComponentCondition<P1EyeOfTheStorm>(id + 0x500, 3.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P1EyeOfTheStorm>()
            .ActivateOnEnter<P4VulcanBurst>()
            .ActivateOnEnter<P4DiffractiveLaser>(); // show hint early...
        ComponentCondition<P4VulcanBurst>(id + 0x501, 1.0f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<P4VulcanBurst>();
        ComponentCondition<P1EyeOfTheStorm>(id + 0x502, 2.0f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<P1EyeOfTheStorm>();

        ComponentCondition<P4DiffractiveLaser>(id + 0x600, 1.5f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<P4DiffractiveLaser>();
    }

    private void P4HomingLasers(uint id, float delay)
    {
        ActorCast(id, _module.Ultima, AID.HomingLasers, delay, 3, true, "Tankbuster")
            .ActivateOnEnter<P4HomingLasers>()
            .DeactivateOnExit<P4HomingLasers>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Phase5Ultima(uint id)
    {
        P5Suppression(id, 0);
        P5Ultima(id + 0x10000, 6.3f);
        P5AethericBoom(id + 0x20000, 6.2f);

        Dictionary<AID, (uint seqID, Action<uint> buildState)> fork = new()
        {
            [AID.PrepareGaruda] = ((id >> 24) + 1, Phase5GarudaIfritTitan),
            [AID.PrepareIfrit] = ((id >> 24) + 2, Phase5IfritGarudaTitan),
            [AID.PrepareTitan] = ((id >> 24) + 3, Phase5TitanIfritGaruda)
        };
        ActorCastStartFork(id + 0x30000, _module.Ultima, fork, 23.3f, true, "Primal roulette")
            .ActivateOnEnter<P5ViscousAetheroplasmTriple>();
    }

    private void Phase5GarudaIfritTitan(uint id)
    {
        P5PrimalRouletteGaruda(id, 0);
        P5PrimalRouletteIfrit(id + 0x10000, 4);
        P5PrimalRouletteTitan(id + 0x20000, 2.8f);
        P5Enrage(id + 0x30000, 3.9f);
    }

    private void Phase5IfritGarudaTitan(uint id)
    {
        // TODO: timings
        P5PrimalRouletteIfrit(id, 0);
        P5PrimalRouletteGaruda(id + 0x10000, 2.8f);
        P5PrimalRouletteTitan(id + 0x20000, 4);
        P5Enrage(id + 0x30000, 3.9f);
    }

    private void Phase5TitanIfritGaruda(uint id)
    {
        // TODO: timings
        P5PrimalRouletteTitan(id, 0);
        P5PrimalRouletteIfrit(id + 0x10000, 4);
        P5PrimalRouletteGaruda(id + 0x20000, 2.8f);
        P5Enrage(id + 0x30000, 4);
    }

    // TODO: razor plumes featherlance...
    private void P5Suppression(uint id, float delay)
    {
        ActorCast(id, _module.Ultima, AID.UltimateSuppression, delay, 3, true);
        ActorTargetable(id + 0x10, _module.Ultima, false, 4.4f, "Disappear (suppression)");

        ActorCast(id + 0x20, _module.Ifrit, AID.Eruption, 4.4f, 2.5f, false)
            .ActivateOnEnter<P2Eruption>()
            .ActivateOnEnter<P1MistralSongAdds>(); // icons appear ~0.9s after cast start
        // +0.8s: 4 razor plumes spawn at intercards, r=17

        ActorCastStart(id + 0x30, _module.Ultima, AID.LightPillar, 3.5f, true); // cast starts at the same time as 4th eruptions start
        ComponentCondition<P1MistralSongAdds>(id + 0x31, 0.1f, comp => comp.NumCasts > 0, "Mistral song")
            .DeactivateOnExit<P1MistralSongAdds>();
        ActorCastStart(id + 0x32, _module.Garuda, AID.MistralSongCone, 0.3f, false);
        // +0.6s: 3rd set of eruptions end, fetters applied

        ActorCastEnd(id + 0x40, _module.Ultima, 1.6f, true)
            .ActivateOnEnter<P5MistralSongCone>();
        ComponentCondition<P1FeatherRain>(id + 0x41, 0.2f, comp => comp.CastsPredicted, "Feathers bait 1")
            .ActivateOnEnter<P1FeatherRain>();
        ActorCastEnd(id + 0x42, _module.Garuda, 0.2f, false)
            .DeactivateOnExit<P5MistralSongCone>();

        ComponentCondition<P2Eruption>(id + 0x50, 0.6f, comp => comp.Casters.Count == 0)
            .DeactivateOnExit<P2Eruption>();

        // second set of feathers are baited at the same time as first laser cast start
        ActorCastStartMulti(id + 0x60, _module.Ultima, [AID.AetherochemicalLaserCenter, AID.AetherochemicalLaserRight, AID.AetherochemicalLaserLeft], 1.2f, true, "Feathers bait 2")
            .ActivateOnEnter<P5LightPillar>() // first light pillar cast starts at the same time as first laser, activate earlier to show bait hints
            .ActivateOnEnter<P1GreatWhirlwind>(); // cast starts ~0.2s after last eruption ends
        ComponentCondition<P1FeatherRain>(id + 0x61, 0.5f, comp => comp.NumCasts > 0); // second set should still be predicted
        ComponentCondition<P1GreatWhirlwind>(id + 0x62, 1.5f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<P5AetherochemicalLaserCenter>()
            .ActivateOnEnter<P5AetherochemicalLaserRight>()
            .ActivateOnEnter<P5AetherochemicalLaserLeft>()
            .DeactivateOnExit<P1GreatWhirlwind>();
        ComponentCondition<P1FeatherRain>(id + 0x63, 0.5f, comp => !comp.CastsPredicted && !comp.CastsActive)
            .DeactivateOnExit<P1FeatherRain>();
        ActorCastEnd(id + 0x64, _module.Ultima, 0.5f, true, "Laser 1");

        // TODO: gaol deadline is ~1.8s into second laser cast
        ActorCastMulti(id + 0x70, _module.Ultima, [AID.AetherochemicalLaserCenter, AID.AetherochemicalLaserRight, AID.AetherochemicalLaserLeft], 1.2f, 3, true, "Laser 2")
            .DeactivateOnExit<P5LightPillar>(); // last light pillar ends ~1.1s before cast end

        ActorCastStartMulti(id + 0x80, _module.Ultima, [AID.AetherochemicalLaserCenter, AID.AetherochemicalLaserRight, AID.AetherochemicalLaserLeft], 1.2f, true, "Landslide bait")
            .ActivateOnEnter<Landslide>() // landslide cast starts together with third laser cast, but activate it earlier just in case
            .ActivateOnEnter<P1Mesohigh>(); // mesohigh tether appears ~0.1s before cast start
        ActorCastEnd(id + 0x81, _module.Ultima, 3, true, "Laser 3")
            .ActivateOnEnter<P5FlamingCrush>() // icon appears ~0.7s after cast start
            .DeactivateOnExit<P5AetherochemicalLaserCenter>()
            .DeactivateOnExit<P5AetherochemicalLaserRight>()
            .DeactivateOnExit<P5AetherochemicalLaserLeft>();
        ComponentCondition<Landslide>(id + 0x82, 1.3f, comp => !comp.CastsActive)
            .DeactivateOnExit<Landslide>();
        ComponentCondition<P1Mesohigh>(id + 0x83, 0.7f, comp => comp.NumCasts > 0, "Mesohigh")
            .DeactivateOnExit<P1Mesohigh>();
        ComponentCondition<P5FlamingCrush>(id + 0x84, 0.8f, comp => !comp.Active)
            .DeactivateOnExit<P5FlamingCrush>();

        ActorCastStart(id + 0x90, _module.Ultima, AID.TankPurge, 1.4f, true);
        ComponentCondition<P1FeatherRain>(id + 0x91, 0.9f, comp => comp.CastsPredicted, "Feathers bait 3")
            .ActivateOnEnter<P1FeatherRain>(); // PATE ~0.9s after cast start
        ComponentCondition<P1FeatherRain>(id + 0x93, 2.5f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<P1FeatherRain>();
        ActorCastEnd(id + 0x94, _module.Ultima, 0.6f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P5Ultima(uint id, float delay)
    {
        ActorTargetable(id, _module.Ultima, true, delay, "Reappear");
        ActorCast(id + 0x10, _module.Ultima, AID.Ultima, 0.1f, 5, true, "Tank LB3")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P5AethericBoom(uint id, float delay)
    {
        ActorCast(id, _module.Ultima, AID.AethericBoom, delay, 4, true, "Knockback + orbs")
            .ActivateOnEnter<P5AethericBoom>()
            .DeactivateOnExit<P5AethericBoom>();
        // TODO: consider adding more hints and states here...
    }

    private void P5PrimalRouletteGaruda(uint id, float delay)
    {
        ActorCast(id, _module.Ultima, AID.PrepareGaruda, delay, 2, true);
        ActorCast(id + 0x10, _module.Garuda, AID.WickedWheel, 4.3f, 3, false, "Out")
            .ActivateOnEnter<WickedWheel>();
        // +1.5s: aetheroplasm resolve
        ComponentCondition<WickedWheel>(id + 0x20, 2.1f, comp => !comp.Active, "In")
            .DeactivateOnExit<WickedWheel>();
        ActorCast(id + 0x30, _module.Garuda, AID.AerialBlast, 1.8f, 3, false, "Raidwide")
            .ActivateOnEnter<P1FeatherRain>() // leave it active, it will overlap next primal
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P5PrimalRouletteIfrit(uint id, float delay)
    {
        ActorCast(id, _module.Ultima, AID.PrepareIfrit, delay, 2, true);
        // both cyclone and eruptions happen at the same time
        ActorCast(id + 0x10, _module.Ifrit, AID.Eruption, 4.3f, 2.5f, false)
            .ActivateOnEnter<P2Eruption>()
            .ActivateOnEnter<P2CrimsonCyclone>();
        ComponentCondition<P2Eruption>(id + 0x20, 0.5f, comp => comp.NumCasts > 0, "Eruptions + Charges")
            .DeactivateOnExit<P2Eruption>()
            .DeactivateOnExit<P2CrimsonCyclone>();
        // +1.5s: aetheroplasm resolve
        ActorCast(id + 0x30, _module.Ifrit, AID.Hellfire, 5, 3, false, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P5PrimalRouletteTitan(uint id, float delay)
    {
        ActorCast(id, _module.Ultima, AID.PrepareTitan, delay, 2, true);
        ComponentCondition<P3WeightOfTheLand>(id + 0x10, 2.2f, comp => comp.Casters.Count > 0, "Puddles x3")
            .ActivateOnEnter<P3WeightOfTheLand>();
        ActorCast(id + 0x30, _module.Titan, AID.EarthenFury, 9.1f, 3, false, "Raidwide")
            .DeactivateOnExit<P3WeightOfTheLand>() // last puddle finishes just before cast start
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P5Enrage(uint id, float delay)
    {
        ActorTargetable(id, _module.Ultima, false, delay, "Disappear");
        ActorTargetable(id + 1, _module.Ultima, true, 4.3f, "Reappear");
        SimpleState(id + 2, 10, "Enrage"); // TODO: better timing/condition
    }
}
