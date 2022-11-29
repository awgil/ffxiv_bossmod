using System.Linq;

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
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed; // TODO: next phase condition
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

            // TODO: awakened wicked wheel > awakened downburst > slipstream > enrage
            SimpleState(id + 0xFF0000, 10000, "???");
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

        private void Phase2Ifrit(uint id)
        {
            P2CrimsonCycloneRadiantPlumeHellfire(id, 4.2f);
            P2VulcanBurst(id + 0x10000, 8.2f);
            P2Incinerate(id + 0x20000, 2.8f);
            P2Nails(id + 0x30000, 7.2f);
            P2InfernoHowlEruptionCrimsonCyclone(id + 0x40000, 6.3f);

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
            ActorCastStart(id + 0x110, _module.Ifrit, AID.Eruption, 3.1f, true, "Eruption baits")
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
            ComponentCondition<P2CrimsonCyclone>(id + 0x50, 1.1f, comp => comp.NumCasts > 0, "Charges")
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
            // +2.2s: PATE 1E43 on 4 ifrits
            // +3.6s: searing wind 3 (2nd target)
            // +4.4s: first charge start
            // +6.4s: second charge start
            // +7.4s: first charge end
            // TODO: ...
        }
    }
}
