namespace BossMod.Stormblood.Ultimate.UWU
{
    class UWUStates : StateMachineBuilder
    {
        private UWU _module;

        public UWUStates(UWU module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1Garuda, "P1: Garuda")
                .ActivateOnEnter<P1Plumes>()
                .ActivateOnEnter<P1Gigastorm>()
                .ActivateOnEnter<P1GreatWhirlwind>() // TODO: not sure about this...
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HP.Cur <= 1 && !Module.PrimaryActor.IsTargetable;
            SimplePhase(1, Phase2Ifrit, "P2: Ifrit")
                .ActivateOnEnter<P2Nails>()
                .ActivateOnEnter<P2InfernalFetters>()
                .ActivateOnEnter<P2SearingWind>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (_module.Ifrit()?.HP.Cur <= 1 && !(_module.Ifrit()?.IsTargetable ?? true));
            SimplePhase(2, Phase3Titan, "P3Titan")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed; // TODO: condition
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

        // note: they aren't necessarily awakened if previous mechanics were fucked up, but who cares...
        private void P1AwakenedWickedWheelDownburst(uint id, float delay)
        {
            ActorCast(id, _module.Garuda, AID.WickedWheel, delay, 3, true, "Out")
                .ActivateOnEnter<P1WickedWheel>()
                .DeactivateOnExit<P1WickedWheel>();
            ComponentCondition<P1WickedTornado>(id + 2, 2.1f, comp => comp.NumCasts > 0, "In")
                .ActivateOnEnter<P1WickedTornado>()
                .DeactivateOnExit<P1WickedTornado>();
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
            SimpleState(id + 0xFF0000, 10000, "???");
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
            P3RockBusterMountainBuster(id + 0x10000, 8.2f);
            P3WeightOfTheLandGeocrush(id + 0x20000, 2.1f);
            P3UpheavalGaolsLandslideTumult(id + 0x30000, 2.2f);
            P3WeightOfTheLand(id + 0x40000, 5.2f);

            SimpleState(id + 0xFF0000, 10000, "???");
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

        private void P3RockBusterMountainBuster(uint id, float delay)
        {
            ComponentCondition<P3RockBuster>(id, delay, comp => comp.NumCasts > 0, "Cleave 1")
                .ActivateOnEnter<P3RockBuster>()
                .DeactivateOnExit<P3RockBuster>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P3MountainBuster>(id + 1, 3.1f, comp => comp.NumCasts > 0, "Cleave 2")
                .ActivateOnEnter<P3MountainBuster>()
                .DeactivateOnExit<P3MountainBuster>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P3WeightOfTheLandGeocrush(uint id, float delay)
        {
            ActorCast(id, _module.Titan, AID.WeightOfTheLand, delay, 2.5f, true)
                .ActivateOnEnter<P3WeightOfTheLand>();
            ComponentCondition<P3WeightOfTheLand>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, "Puddles 1");

            ActorTargetable(id + 0x20, _module.Titan, false, 2.7f, "Disappear");
            ComponentCondition<P3WeightOfTheLand>(id + 0x30, 0.3f, comp => comp.Casters.Count == 0)
                .DeactivateOnExit<P3WeightOfTheLand>();

            ActorCastStart(id + 0x40, _module.Titan, AID.Geocrush2, 2.2f, true)
                .ActivateOnEnter<P3Geocrush2>();
            ActorCastEnd(id + 0x41, _module.Titan, 3, true, "Proximity")
                .DeactivateOnExit<P3Geocrush2>();
            ActorTargetable(id + 0x50, _module.Titan, true, 2.4f, "Reappear");
        }

        private void P3UpheavalGaolsLandslideTumult(uint id, float delay)
        {
            ActorCast(id, _module.Titan, AID.Upheaval, delay, 4, true, "Knockback")
                .ActivateOnEnter<P3Upheaval>()
                .ActivateOnEnter<P3Burst>() // bombs appear ~0.2s after cast start
                .DeactivateOnExit<P3Upheaval>();
            ComponentCondition<P3Gaols>(id + 0x10, 2.1f, comp => comp.Active)
                .ActivateOnEnter<P3Gaols>();
            ComponentCondition<P3Burst>(id + 0x11, 0.4f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P3Burst>();

            ActorCast(id + 0x20, _module.Titan, AID.LandslideBoss, 1.8f, 2.2f, true, "Landslide 1")
                .ActivateOnEnter<P3LandslideBoss>()
                .ActivateOnEnter<P3LandslideHelper>()
                .ActivateOnEnter<P3Burst>() // extra bomb appears ~0.1s before landslide start
                .DeactivateOnExit<P3LandslideBoss>()
                .DeactivateOnExit<P3LandslideHelper>();
            // +0.6s: fetters for gaols
            ActorCast(id + 0x30, _module.Titan, AID.LandslideBoss, 2.2f, 2.2f, true, "Landslide 2")
                .ActivateOnEnter<P3LandslideBoss>()
                .ActivateOnEnter<P3LandslideHelper>()
                .DeactivateOnExit<P3LandslideBoss>()
                .DeactivateOnExit<P3LandslideHelper>()
                .DeactivateOnExit<P3Burst>(); // bomb explodes ~0.5s before landslide end

            ComponentCondition<P3Tumult>(id + 0x1000, 2.1f, comp => comp.NumCasts > 0, "Raidwide 1")
                .ActivateOnEnter<P3Tumult>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P3Tumult>(id + 0x1007, 7.8f, comp => comp.NumCasts >= 8, "Raidwide 8")
                .DeactivateOnExit<P3Tumult>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P3WeightOfTheLand(uint id, float delay)
        {
            ActorCast(id, _module.Titan, AID.WeightOfTheLand, delay, 2.5f, true)
                .ActivateOnEnter<P3WeightOfTheLand>();
            ComponentCondition<P3WeightOfTheLand>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, "Puddles 1");

            // TODO: ...
        }
    }
}
