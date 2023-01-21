using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex5Rubicante
{
    class Ex5RubicanteStates : StateMachineBuilder
    {
        public Ex5RubicanteStates(BossModule module) : base(module)
        {
            SimplePhase(0, Phase1, "Start + adds")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsDead || (Module.PrimaryActor.CastInfo?.IsSpell(AID.BlazingRapture) ?? false);
            DeathPhase(1, Phase2);
        }

        private void Phase1(uint id)
        {
            InfernoRaidwide(id, 6.1f);
            OrdealOfPurgation(id + 0x10000, 25.5f);
            OrdealOfPurgation(id + 0x20000, 6.6f);
            ShatteringHeatBoss(id + 0x30000, 9.2f);
            ArchInferno(id + 0x40000, 9.3f);
            InfernoRaidwide(id + 0x50000, 4.3f);
            OrdealOfPurgation(id + 0x60000, 25.6f);
            OrdealOfPurgation(id + 0x70000, 6.7f);
            Flamesent(id + 0x80000, 5.1f);
        }

        private void Phase2(uint id)
        {
            BlazingRapture(id, 0);
            FlamespireBrand(id + 0x10000, 26.9f);
            InfernoSpread(id + 0x20000, 6.5f);
            ScaldingSignalRingSweepingImmolation(id + 0x30000, 5.2f);
            Dualfire(id + 0x40000, 8.8f);
            OrdealOfPurgation(id + 0x50000, 31.7f);
            OrdealOfPurgation(id + 0x60000, 6.7f);
            InfernoRaidwide(id + 0x70000, 4.2f);
            FlamespireClaw(id + 0x80000, 11.4f);
            InfernoSpread(id + 0x90000, 5.2f);
            ScaldingSignalRingSweepingImmolation(id + 0xA0000, 5.2f);
            Dualfire(id + 0xB0000, 8.8f);
            OrdealOfPurgation(id + 0xC0000, 32.0f);
            OrdealOfPurgation(id + 0xD0000, 6.7f);
            Cast(id + 0xE0000, AID.Enrage, 11.6f, 10, "Enrage");
        }

        private void InfernoRaidwide(uint id, float delay)
        {
            Cast(id, AID.InfernoRaidwide, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void ShatteringHeatBoss(uint id, float delay)
        {
            Cast(id, AID.ShatteringHeatBoss, delay, 5, "Tankbuster")
                .ActivateOnEnter<ShatteringHeatBoss>()
                .DeactivateOnExit<ShatteringHeatBoss>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void OrdealOfPurgation(uint id, float delay)
        {
            CastStart(id, AID.OrdealOfPurgation, delay)
                .ActivateOnEnter<OrdealOfPurgation>();
            CastEnd(id + 1, 12, "Shackle");
            ComponentCondition<OrdealOfPurgation>(id + 0x10, 8.1f, comp => comp.NumCasts > 0, "Ordeal resolve")
                .DeactivateOnExit<OrdealOfPurgation>();
        }

        private State ArchInfernoSpreadStack(uint id, float delay)
        {
            Condition(id, delay, () => Module.FindComponent<SpikeOfFlame>()!.Active || Module.FindComponent<FourfoldFlame>()!.Active || Module.FindComponent<TwinfoldFlame>()!.Active)
                .ActivateOnEnter<SpikeOfFlame>()
                .ActivateOnEnter<FourfoldFlame>()
                .ActivateOnEnter<TwinfoldFlame>();
            return Condition(id + 1, 5, () => !Module.FindComponent<SpikeOfFlame>()!.Active && !Module.FindComponent<FourfoldFlame>()!.Active && !Module.FindComponent<TwinfoldFlame>()!.Active, "Spread/stack")
                .DeactivateOnExit<SpikeOfFlame>()
                .DeactivateOnExit<FourfoldFlame>()
                .DeactivateOnExit<TwinfoldFlame>();
        }

        private void ArchInferno(uint id, float delay)
        {
            Cast(id, AID.ArchInferno, delay, 6, "Rotating aoes start")
                .ActivateOnEnter<ArchInferno>()
                .ActivateOnEnter<InfernoDevilFirst>()
                .ActivateOnEnter<InfernoDevilRest>()
                .DeactivateOnExit<InfernoDevilFirst>();
            ArchInfernoSpreadStack(id + 0x10, 4);
            Cast(id + 0x20, AID.Conflagration, 1.4f, 3, "Line AOE")
                .ActivateOnEnter<Conflagration>()
                .DeactivateOnExit<Conflagration>();
            ArchInfernoSpreadStack(id + 0x30, 4.1f)
                .DeactivateOnExit<ArchInferno>(); // last tick happens ~1.3s before mechanic end
            Cast(id + 0x40, AID.RadialFlagration, 0.2f, 7)
                .ActivateOnEnter<RadialFlagration>()
                .DeactivateOnExit<InfernoDevilRest>();
            ComponentCondition<RadialFlagration>(id + 0x50, 0.9f, comp => comp.NumCasts > 0, "Protean")
                .DeactivateOnExit<RadialFlagration>();
        }

        private void Flamesent(uint id, float delay)
        {
            Targetable(id, false, delay, "Boss disappear");
            ComponentCondition<GreaterFlamesent>(id + 0x10, 4.9f, comp => comp.ActiveActors.Any(), "Adds appear")
                .ActivateOnEnter<GreaterFlamesent>()
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            // TODO: consider adding more states here, mechanics are well timed it seems...
            // note: there is no exit transition, we either wipe (?) or transition to next phase
            SimpleState(id + 0x1000, 60, "Adds enrage")
                .ActivateOnEnter<FlamesentNS>()
                .ActivateOnEnter<FlamesentSS>()
                .ActivateOnEnter<FlamesentNC>()
                .ActivateOnEnter<GhastlyTorch>()
                .ActivateOnEnter<ShatteringHeatAdd>()
                .ActivateOnEnter<GhastlyWind>()
                .ActivateOnEnter<GhastlyFlame>();
        }

        private void BlazingRapture(uint id, float delay)
        {
            Cast(id, AID.BlazingRapture, delay, 4, "DOT");
            ComponentCondition<BlazingRapture>(id + 0x10, 9.7f, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<BlazingRapture>()
                .DeactivateOnExit<BlazingRapture>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void FlamespireBrand(uint id, float delay)
        {
            Cast(id, AID.FlamespireBrand, delay, 4)
                .ActivateOnEnter<Welts>() // statuses appear ~1s after cast end
                .ActivateOnEnter<Flamerake>(); // env controls happen right before next cast start
            Cast(id + 0x10, AID.Flamerake, 2.1f, 6);
            ComponentCondition<Flamerake>(id + 0x20, 2.5f, comp => comp.NumCasts >= 1, "Cross 1");
            ComponentCondition<Flamerake>(id + 0x21, 2.0f, comp => comp.NumCasts >= 2, "Cross 2");
            ComponentCondition<Welts>(id + 0x30, 0.5f, comp => comp.NextMechanic == Welts.Mechanic.Spreads, "Stack/flares");
            ComponentCondition<Flamerake>(id + 0x40, 2.0f, comp => comp.NumCasts >= 3, "Cross 3")
                .DeactivateOnExit<Flamerake>();
            ComponentCondition<Welts>(id + 0x50, 1.0f, comp => comp.NextMechanic == Welts.Mechanic.Done, "Spread")
                .DeactivateOnExit<Welts>();
        }

        private void InfernoSpread(uint id, float delay)
        {
            Cast(id, AID.InfernoSpread, delay, 4)
                .ActivateOnEnter<InfernoSpread>();
            ComponentCondition<InfernoSpread>(id + 0x10, 1, comp => !comp.Active, "Spread")
                .DeactivateOnExit<InfernoSpread>();
        }

        private void ScaldingSignalRingSweepingImmolation(uint id, float delay)
        {
            CastMulti(id, new[] { AID.ScaldingSignal, AID.ScaldingRing }, delay, 5, "Circle/donut")
                .ActivateOnEnter<ScaldingSignal>()
                .ActivateOnEnter<ScaldingRing>()
                .ActivateOnEnter<ScaldingFleetFirst>()
                .DeactivateOnExit<ScaldingSignal>()
                .DeactivateOnExit<ScaldingRing>();
            ComponentCondition<ScaldingFleetFirst>(id + 0x10, 1.1f, comp => comp.NumCasts > 0, "Baited lines")
                .DeactivateOnExit<ScaldingFleetFirst>();

            CastMulti(id + 0x20, new[] { AID.SweepingImmolationSpread, AID.SweepingImmolationStack }, 2.5f, 7)
                .ActivateOnEnter<SweepingImmolationSpread>()
                .ActivateOnEnter<SweepingImmolationStack>()
                .ActivateOnEnter<PartialTotalImmolation>()
                .ActivateOnEnter<ScaldingFleetSecond>() // casts start ~0.3s or ~1s into cast
                .DeactivateOnExit<SweepingImmolationSpread>()
                .DeactivateOnExit<SweepingImmolationStack>();
            ComponentCondition<PartialTotalImmolation>(id + 0x30, 0.5f, comp => comp.NumFinishedSpreads + comp.NumFinishedStacks > 0, "Spread/stack")
                .DeactivateOnExit<PartialTotalImmolation>()
                .DeactivateOnExit<ScaldingFleetSecond>(); // can resolve either slightly before sweeping or together with stack/spread
        }

        private void Dualfire(uint id, float delay)
        {
            CastStart(id, AID.Dualfire, delay)
                .ActivateOnEnter<Dualfire>(); // icons appear slightly before cast start
            CastEnd(id + 1, 5);
            ComponentCondition<Dualfire>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Tankbusters")
                .DeactivateOnExit<Dualfire>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void FlamespireClaw(uint id, float delay)
        {
            CastStart(id, AID.FlamespireClaw, delay)
                .ActivateOnEnter<FlamespireClaw>(); // icons appear slightly before cast start
            CastEnd(id + 1, 6, "Limit cut start");
            ComponentCondition<FlamespireClaw>(id + 0x10, 19.5f, comp => comp.NumCasts >= 8, "Limit cut resolve")
                .DeactivateOnExit<FlamespireClaw>();
        }
    }
}
