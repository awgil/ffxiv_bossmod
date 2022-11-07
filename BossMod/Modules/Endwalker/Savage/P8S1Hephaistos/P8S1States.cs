using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class P8S1States : StateMachineBuilder
    {
        public P8S1States(BossModule module) : base(module)
        {
            SimplePhase(0, SinglePhase, "Single phase")
                .Raw.Update = () => module.PrimaryActor.IsDestroyed;
        }

        private void SinglePhase(uint id)
        {
            GenesisOfFlame(id, 6.2f);
            VolcanicTorchesSunforge(id + 0x10000, 3.2f);
            Flameviper(id + 0x20000, 2);

            Dictionary<AID, (uint seqID, Action<uint> buildState)> fork = new();
            fork[AID.ReforgedReflectionCentaur] = (1, ForkCentaur);
            fork[AID.ReforgedReflectionSnake] = (2, ForkSnake);
            CastStartFork(id + 0x30000, fork, 9.4f, "Centaur -or- Snake");
        }

        private void ForkCentaur(uint id)
        {
            Centaur1(id);
            Intermission(id + 0x10000, 14.4f);
            Snake1(id + 0x20000, 9.5f);
            FourfoldFires(id + 0x30000, 15.4f);
            Flameviper(id + 0x40000, 2);
            Centaur2(id + 0x50000, 9.4f);
            Flameviper(id + 0x60000, 7.6f);
            Snake2(id + 0x70000, 9.4f);
            GenesisOfFlame(id + 0x80000, 8.1f);
            Enrage(id + 0x90000, 8.2f);
        }

        private void ForkSnake(uint id)
        {
            Snake1(id);
            Intermission(id + 0x10000, 15.4f);
            Centaur1(id + 0x20000, 9.5f);
            FourfoldFires(id + 0x30000, 14.4f);
            Flameviper(id + 0x40000, 2);
            Snake2(id + 0x50000, 9.4f);
            Flameviper(id + 0x60000, 8);
            Centaur2(id + 0x70000, 9.4f);
            GenesisOfFlame(id + 0x80000, 7.6f);
            Enrage(id + 0x90000, 8.2f);
        }

        private void GenesisOfFlame(uint id, float delay)
        {
            Cast(id, AID.GenesisOfFlame, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void VolcanicTorchesSunforge(uint id, float delay)
        {
            CastMulti(id, new AID[] { AID.ConceptualOctaflare, AID.ConceptualTetraflare }, delay, 3)
                .ActivateOnEnter<TetraOctaFlareConceptual>();
            Cast(id + 0x10, AID.VolcanicTorches, 3.2f, 3)
                .ActivateOnEnter<VolcanicTorches>(); // casts start ~4s later
            CastStartMulti(id + 0x20, new AID[] { AID.SunforgeCenter, AID.SunforgeSides }, 8.5f);
            ComponentCondition<VolcanicTorches>(id + 0x30, 5.5f, comp => comp.NumCasts > 0, "Torches")
                .ActivateOnEnter<SunforgeCenterHint>()
                .ActivateOnEnter<SunforgeSidesHint>()
                .DeactivateOnExit<VolcanicTorches>()
                .DeactivateOnExit<SunforgeCenterHint>()
                .DeactivateOnExit<SunforgeSidesHint>();
            CastEnd(id + 0x40, 1.4f)
                .ActivateOnEnter<SunforgeCenter>()
                .ActivateOnEnter<SunforgeSides>()
                .Raw.Enter.Add(() => Module.FindComponent<TetraOctaFlareConceptual>()?.Show(Module));
            // note: sunforge aoe happens ~0.1s before flares
            ComponentCondition<TetraOctaFlareConceptual>(id + 0x50, 1.1f, comp => !comp.Active, "Sunforge + stack/spread")
                .DeactivateOnExit<TetraOctaFlareConceptual>()
                .DeactivateOnExit<SunforgeCenter>()
                .DeactivateOnExit<SunforgeSides>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Flameviper(uint id, float delay)
        {
            Cast(id, AID.Flameviper, delay, 5, "Tankbuster hit 1")
                .ActivateOnEnter<Flameviper>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<Flameviper>(id + 2, 3.4f, comp => comp.NumCasts > 0, "Tankbuster hit 2")
                .DeactivateOnExit<Flameviper>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void Intermission(uint id, float delay)
        {
            Cast(id, AID.IllusoryCreation, delay, 3);
            Cast(id + 0x10, AID.CreationOnCommand, 3.2f, 3)
                .ActivateOnEnter<SunforgeCenterIntermission>() // note that sunforges start ~0.8s after cast ends
                .ActivateOnEnter<SunforgeSidesIntermission>();
            Cast(id + 0x20, AID.ManifoldFlames, 3.2f, 5)
                .ActivateOnEnter<ManifoldFlames>();
            // note: sunforges resolves ~0.1s before flares
            ComponentCondition<NestOfFlamevipersBaited>(id + 0x30, 0.8f, comp => comp.Active, "Spread in safe cells")
                .ActivateOnEnter<NestOfFlamevipersBaited>()
                .DeactivateOnExit<ManifoldFlames>()
                .DeactivateOnExit<SunforgeCenterIntermission>()
                .DeactivateOnExit<SunforgeSidesIntermission>();
            // +3.9s: start of second set of sunforges; but we activate components only after baits are done to reduce clutter
            ComponentCondition<NestOfFlamevipersBaited>(id + 0x40, 4.2f, comp => !comp.Active, "Baits")
                .DeactivateOnExit<NestOfFlamevipersBaited>();

            CastStartMulti(id + 0x50, new AID[] { AID.NestOfFlamevipers, AID.Tetraflare }, 2.3f)
                .ActivateOnEnter<SunforgeCenterIntermission>() // note that sunforges start ~0.3s before baits
                .ActivateOnEnter<SunforgeSidesIntermission>()
                .ActivateOnEnter<TetraOctaFlareImmediate>()
                .ActivateOnEnter<NestOfFlamevipersEveryone>();
            CastEnd(id + 0x51, 5);
            // +0.4s: sunforges resolve
            Condition(id + 0x60, 0.8f, () => !Module.FindComponent<NestOfFlamevipersEveryone>()!.Active && !Module.FindComponent<TetraOctaFlareImmediate>()!.Active, "Spread/stack in pairs")
                .DeactivateOnExit<SunforgeCenterIntermission>()
                .DeactivateOnExit<SunforgeSidesIntermission>()
                .DeactivateOnExit<TetraOctaFlareImmediate>()
                .DeactivateOnExit<NestOfFlamevipersEveryone>();

            ComponentCondition<VolcanicTorches>(id + 0x70, 2.3f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<VolcanicTorches>();
            CastStart(id + 0x71, AID.GenesisOfFlame, 9.2f);
            ComponentCondition<VolcanicTorches>(id + 0x72, 0.8f, comp => comp.Casters.Count == 0, "Torches")
                .DeactivateOnExit<VolcanicTorches>();
            CastEnd(id + 0x73, 4.2f, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void FourfoldFires(uint id, float delay)
        {
            CastMulti(id, new AID[] { AID.ConceptualOctaflare, AID.ConceptualTetraflare }, delay, 3)
                .ActivateOnEnter<TetraOctaFlareConceptual>();
            Cast(id + 0x10, AID.FourfoldFires, 3.2f, 3);
            ComponentCondition<AbyssalFires>(id + 0x12, 5.8f, comp => comp.NumCasts > 0, "Proximity aoes")
                .ActivateOnEnter<AbyssalFires>()
                .DeactivateOnExit<AbyssalFires>();

            Cast(id + 0x20, AID.CthonicVent, 0.4f, 3);
            // +0.8f: vents cast start
            ComponentCondition<CthonicVent>(id + 0x30, 5.8f, comp => comp.NumTotalCasts > 0, "Vent 1")
                .ActivateOnEnter<CthonicVent>();
            CastStartMulti(id + 0x40, new AID[] { AID.Tetraflare, AID.Octaflare }, 4.4f);
            ComponentCondition<CthonicVent>(id + 0x50, 4.7f, comp => comp.NumTotalCasts > 2, "Vent 2")
                .ActivateOnEnter<TetraOctaFlareImmediate>();
            CastEnd(id + 0x60, 0.3f);
            ComponentCondition<TetraOctaFlareImmediate>(id + 0x61, 0.8f, comp => !comp.Active, "Spread/stack in pairs")
                .DeactivateOnExit<TetraOctaFlareImmediate>();
            CastStartMulti(id + 0x70, new AID[] { AID.SunforgeCenter, AID.SunforgeSides }, 5.7f);
            ComponentCondition<CthonicVent>(id + 0x80, 2.2f, comp => comp.NumTotalCasts > 4, "Vent 3")
                .ActivateOnEnter<SunforgeCenterHint>()
                .ActivateOnEnter<SunforgeSidesHint>()
                .DeactivateOnExit<CthonicVent>()
                .DeactivateOnExit<SunforgeCenterHint>()
                .DeactivateOnExit<SunforgeSidesHint>();
            CastEnd(id + 0x90, 4.7f)
                .ActivateOnEnter<SunforgeCenter>()
                .ActivateOnEnter<SunforgeSides>()
                .Raw.Enter.Add(() => Module.FindComponent<TetraOctaFlareConceptual>()?.Show(Module));
            // note: sunforge aoe happens ~0.1s before flares
            ComponentCondition<TetraOctaFlareConceptual>(id + 0xA0, 1.1f, comp => !comp.Active, "Sunforge + stack/spread")
                .DeactivateOnExit<TetraOctaFlareConceptual>()
                .DeactivateOnExit<SunforgeCenter>()
                .DeactivateOnExit<SunforgeSides>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void CentaurStart(uint id, float delay)
        {
            if (delay >= 0)
                CastStart(id, AID.ReforgedReflectionCentaur, delay);
            CastEnd(id + 1, 3)
                .ActivateOnEnter<Footprint>();
            ComponentCondition<Footprint>(id + 2, 6.5f, comp => comp.NumCasts > 0, "Centaur knockback")
                .DeactivateOnExit<Footprint>();
        }

        // if delay is negative, cast-start state is not created
        private void Centaur1(uint id, float delay = -1)
        {
            CentaurStart(id, delay);

            Cast(id + 0x100, AID.RearingRampage, 0.9f, 5, "First raidwide")
                .ActivateOnEnter<UpliftStompDead>()
                .SetHint(StateMachine.StateHint.Raidwide);
            // +1s: first set of uplifts
            ComponentCondition<RearingRampageSecond>(id + 0x110, 2.2f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<RearingRampageSecond>()
                .SetHint(StateMachine.StateHint.Raidwide);
            // +1s: second set of uplifts
            ComponentCondition<RearingRampageSecond>(id + 0x120, 2.1f, comp => comp.NumCasts > 1)
                .DeactivateOnExit<RearingRampageSecond>()
                .SetHint(StateMachine.StateHint.Raidwide);
            // +1s: third set of uplifts
            ComponentCondition<RearingRampageLast>(id + 0x130, 2.1f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<RearingRampageLast>()
                .DeactivateOnExit<RearingRampageLast>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<UpliftStompDead>(id + 0x140, 1, comp => comp.NumUplifts >= 8, "Last uplift");

            Cast(id + 0x150, AID.StompDead, 1.2f, 5);
            ComponentCondition<UpliftStompDead>(id + 0x152, 0.3f, comp => comp.NumStomps > 0, "First stomp");
            ComponentCondition<UpliftStompDead>(id + 0x153, 2.3f, comp => comp.NumStomps > 1);
            ComponentCondition<UpliftStompDead>(id + 0x154, 2.3f, comp => comp.NumStomps > 2);
            ComponentCondition<UpliftStompDead>(id + 0x155, 2.3f, comp => comp.NumStomps > 3, "Last stomp")
                .DeactivateOnExit<UpliftStompDead>();
        }

        private void Centaur2(uint id, float delay)
        {
            CentaurStart(id, delay);

            CastMulti(id + 0x100, new AID[] { AID.QuadrupedalImpact, AID.QuadrupedalCrush }, 1.1f, 5)
                .ActivateOnEnter<QuadrupedalImpact>()
                .ActivateOnEnter<QuadrupedalCrush>();
            Condition(id + 0x102, 0.9f, () => Module.FindComponent<QuadrupedalImpact>()!.NumCasts + Module.FindComponent<QuadrupedalCrush>()!.NumCasts > 0, "Knockback or aoe")
                .DeactivateOnExit<QuadrupedalImpact>()
                .DeactivateOnExit<QuadrupedalCrush>();

            CastMulti(id + 0x110, new AID[] { AID.ConceptualTetraflareCentaur, AID.ConceptualDiflare }, 2.7f, 3)
                .ActivateOnEnter<CentaurTetraflare>()
                .ActivateOnEnter<CentaurDiflare>();

            Cast(id + 0x120, AID.BlazingFootfalls, 3.2f, 12, "Boss disappear")
                .ActivateOnEnter<BlazingFootfalls>()
                .SetHint(StateMachine.StateHint.DowntimeStart); // boss becomes untargetable right at cast end
            ComponentCondition<BlazingFootfalls>(id + 0x130, 0.7f, comp => comp.NumMechanicsDone > 0);
            // +1.1s: di/tetra flare resolve
            ComponentCondition<BlazingFootfalls>(id + 0x140, 4.8f, comp => comp.NumMechanicsDone > 1, "Knockback/aoe 1"); // right after this torchest start
            ComponentCondition<BlazingFootfalls>(id + 0x150, 3.7f, comp => comp.NumMechanicsDone > 2, "Horizonal")
                .ActivateOnEnter<VolcanicTorches>();
            ComponentCondition<BlazingFootfalls>(id + 0x160, 2.8f, comp => comp.NumMechanicsDone > 3, "Knockback/aoe 2")
                .DeactivateOnExit<BlazingFootfalls>();
            ComponentCondition<VolcanicTorches>(id + 0x170, 3.5f, comp => comp.NumCasts > 0, "Torches")
                .DeactivateOnExit<VolcanicTorches>();
            Targetable(id + 0x180, true, 0.8f, "Boss reappear");
        }

        // returns state on entering which mechanic-specific component should be activated
        private State SnakeStart(uint id, float delay)
        {
            if (delay >= 0)
                CastStart(id, AID.ReforgedReflectionSnake, delay);
            CastEnd(id + 1, 3)
                .ActivateOnEnter<SnakingKick>();
            ComponentCondition<SnakingKick>(id + 2, 7.3f, comp => comp.NumCasts > 0, "Snake aoe")
                .DeactivateOnExit<SnakingKick>();

            Cast(id + 0x10, AID.Gorgomanteia, 3.2f, 3);
            // +0.7s: debuffs are applied
            var s = CastStart(id + 0x20, AID.IntoTheShadows, 3.2f);
            CastEnd(id + 0x21, 3);
            return s;
        }

        // if delay is negative, cast-start state is not created
        private void Snake1(uint id, float delay = -1)
        {
            SnakeStart(id, delay)
                .ActivateOnEnter<Snake1>();
            ComponentCondition<Snake1>(id + 0x110, 15.2f, comp => comp.NumCasts > 0, "First snakes appear");
            ComponentCondition<Snake1>(id + 0x120, 2.5f, comp => comp.NumEyeCasts > 0, "First order petrify");
            ComponentCondition<Snake1>(id + 0x121, 3.0f, comp => comp.NumBloodCasts > 0, "First order explode");
            ComponentCondition<Snake1>(id + 0x130, 2.5f, comp => comp.NumCasts > 2, "Second snakes appear");
            ComponentCondition<Snake1>(id + 0x140, 2.5f, comp => comp.NumEyeCasts > 2, "Second order petrify");
            ComponentCondition<Snake1>(id + 0x141, 3.0f, comp => comp.NumBloodCasts > 2, "Second order explode")
                .DeactivateOnExit<Snake1>();

            Cast(id + 0x150, AID.Ektothermos, 4.6f, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Snake2(uint id, float delay)
        {
            SnakeStart(id, delay)
                .ActivateOnEnter<Snake2>();
            // +6.0s: petrifaction casts start
            // +8.0s: gorgospit animations
            // +10.0s: gorgospit casts start
            // +14.0s: petrifaction casts end
            ComponentCondition<Snake2>(id + 0x110, 15.2f, comp => comp.NumCasts > 0, "Snakes appear")
                .ActivateOnEnter<Gorgospit>();
            // +1.0s: gorgospit casts end
            ComponentCondition<Snake2>(id + 0x120, 2.5f, comp => comp.NumEyeCasts > 0, "First order petrify/explode");
            Cast(id + 0x130, AID.IllusoryCreationSnakes, 1.6f, 3);
            // +0.8s: gorgospit animations
            ComponentCondition<Snake2>(id + 0x140, 1.4f, comp => comp.NumEyeCasts > 4, "Second order petrify/explode");
            // +1.5s: gorgospit cast start
            // +7.5s: gorgospit cast end
            ComponentCondition<Snake2>(id + 0x150, 9, comp => comp.NumCrownCasts > 0, "Blockable petrify")
                .DeactivateOnExit<Gorgospit>();
            ComponentCondition<Snake2>(id + 0x151, 1, comp => comp.NumBreathCasts > 0, "Party stacks")
                .DeactivateOnExit<Snake2>();
        }

        private void Enrage(uint id, float delay)
        {
            Targetable(id, false, delay, "Enrage");
            Cast(id + 0x10, AID.Enrage, 0.1f, 5, "Finish");
        }
    }
}
