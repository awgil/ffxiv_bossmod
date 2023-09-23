namespace BossMod.Endwalker.Savage.P12S1Athena
{
    class P12S1AthenaStates : StateMachineBuilder
    {
        public P12S1AthenaStates(BossModule module) : base(module)
        {
            SimplePhase(0, SinglePhase, "Single phase")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HP.Cur <= 1 && !Module.PrimaryActor.IsTargetable;
        }

        private void SinglePhase(uint id)
        {
            OnTheSoul(id, 6.2f);
            TrinityOfSoulsWhiteFlame(id + 0x10000, 7.2f);
            EngravementOfSouls1(id + 0x20000, 11.7f);
            OnTheSoul(id + 0x30000, 2.0f);
            Glaukopis(id + 0x40000, 3.2f);
            SuperchainTheory1(id + 0x50000, 11.2f);
            TrinityOfSouls(id + 0x60000, 1.0f);
            Dialogos(id + 0x70000, 3.3f);
            OnTheSoul(id + 0x80000, 1.6f);
            EngravementOfSouls3(id + 0x90000, 13.6f);
            Glaukopis(id + 0xA0000, 7.2f);
            Palladion(id + 0xB0000, 22.2f);
            SuperchainTheory2A(id + 0xC0000, 14.5f);
            Dialogos(id + 0xD0000, 1.0f);
            OnTheSoul(id + 0xE0000, 1.6f);
            SuperchainTheory2B(id + 0xF0000, 9.6f);
            OnTheSoul(id + 0x100000, 3.2f);
            Targetable(id + 0x110000, false, 8.0f, "Enrage");
        }

        private State OnTheSoul(uint id, float delay)
        {
            return Cast(id, AID.OnTheSoul, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Glaukopis(uint id, float delay)
        {
            Cast(id, AID.Glaukopis, delay, 5)
                .ActivateOnEnter<Glaukopis>();
            ComponentCondition<Glaukopis>(id + 0x10, 0.1f, comp => comp.NumCasts >= 1, "Tankbuster 1")
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<Glaukopis>(id + 0x11, 3.2f, comp => comp.NumCasts >= 2, "Tankbuster 2")
                .DeactivateOnExit<Glaukopis>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void Dialogos(uint id, float delay)
        {
            CastMulti(id, new[] { AID.Apodialogos, AID.Peridialogos }, delay, 5)
                .ActivateOnEnter<Dialogos>();
            ComponentCondition<Dialogos>(id + 0x10, 0.3f, comp => comp.NumCasts >= 1, "Tankbuster in/out")
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<Dialogos>(id + 0x11, 1, comp => comp.NumCasts >= 2, "Party stack out/in")
                .DeactivateOnExit<Dialogos>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void TrinityOfSoulsCast(uint id, float delay)
        {
            // note: icons appear ~1, 4, 7 seconds into cast
            CastMulti(id, new[] { AID.TrinityOfSoulsDirectTR, AID.TrinityOfSoulsDirectTL, AID.TrinityOfSoulsInvertBR, AID.TrinityOfSoulsInvertBL }, delay, 10, "Wings 1")
                .ActivateOnEnter<TrinityOfSouls>();
        }

        private void TrinityOfSoulsResolve(uint id, float delay)
        {
            ComponentCondition<TrinityOfSouls>(id, delay, comp => comp.NumCasts >= 2, "Wings 2");
            ComponentCondition<TrinityOfSouls>(id + 1, 2.6f, comp => comp.NumCasts >= 3, "Wings 3")
                .DeactivateOnExit<TrinityOfSouls>();
        }

        private void TrinityOfSouls(uint id, float delay)
        {
            TrinityOfSoulsCast(id, delay);
            TrinityOfSoulsResolve(id + 0x10, 2.6f);
        }

        private void TrinityOfSoulsWhiteFlame(uint id, float delay)
        {
            Cast(id, AID.Paradeigma, delay, 3)
                .ActivateOnEnter<WhiteFlame>()
                .OnEnter(() => Module.FindComponent<WhiteFlame>()?.Enable()); // enable asap
            TrinityOfSoulsCast(id + 0x10, 5.6f);
            ComponentCondition<WhiteFlame>(id + 0x20, 0.8f, comp => comp.NumCasts > 0, "Baits 1");
            TrinityOfSoulsResolve(id + 0x30, 1.8f);
            ComponentCondition<WhiteFlame>(id + 0x40, 0.7f, comp => comp.NumCasts > 4, "Baits 2")
                .DeactivateOnExit<WhiteFlame>();
        }

        private void EngravementOfSouls1(uint id, float delay)
        {
            Cast(id, AID.Paradeigma, delay, 3);
            Cast(id + 0x10, AID.EngravementOfSouls, 3.7f, 4);
            ComponentCondition<EngravementOfSoulsTethers>(id + 0x20, 9.9f, comp => comp.NumCasts > 0, "Tethers")
                .ActivateOnEnter<EngravementOfSoulsTethers>()
                .ActivateOnEnter<EngravementOfSouls1Spread>()
                .ActivateOnEnter<EngravementOfSoulsTowers>();
            ComponentCondition<EngravementOfSouls1Spread>(id + 0x30, 1, comp => !comp.Active, "Tower baits")
                .DeactivateOnExit<EngravementOfSouls1Spread>();
            ComponentCondition<EngravementOfSoulsTowers>(id + 0x40, 1, comp => comp.CastsStarted)
                .ActivateOnEnter<RayOfLight>(); // casts start just before towers casts; while we could predict it before based on PATE, it would just make things more messy
            ComponentCondition<EngravementOfSoulsTowers>(id + 0x41, 2, comp => comp.NumCasts > 0, "Towers")
                .DeactivateOnExit<EngravementOfSoulsTowers>()
                .DeactivateOnExit<EngravementOfSoulsTethers>();
            ComponentCondition<RayOfLight>(id + 0x50, 3, comp => comp.NumCasts > 0, "Lines")
                .DeactivateOnExit<RayOfLight>();
        }

        private void SuperchainTheory1(uint id, float delay)
        {
            Cast(id, AID.SuperchainTheory1, delay, 5)
                .ActivateOnEnter<SuperchainTheory1>();
            Cast(id + 0x10, AID.EngravementOfSouls, 3.2f, 4);
            ComponentCondition<SuperchainTheory1>(id + 0x20, 5.1f, comp => comp.NumCasts >= 2, "Chain 1");
            ComponentCondition<SuperchainTheory1>(id + 0x30, 7.0f, comp => comp.NumCasts >= 4, "Chain 2")
                .ActivateOnEnter<EngravementOfSouls2Lines>();
            ComponentCondition<EngravementOfSouls2Lines>(id + 0x31, 0.3f, comp => comp.NumCasts > 0, "Rays")
                .DeactivateOnExit<EngravementOfSouls2Lines>();
            ComponentCondition<SuperchainTheory1>(id + 0x40, 4.7f, comp => comp.NumCasts >= 5, "Chain 3 first");
            ComponentCondition<SuperchainTheory1>(id + 0x41, 2.0f, comp => comp.NumCasts >= 6, "Chain 3 second")
                .ActivateOnEnter<EngrameventOfSouls2Spread>() // TODO: reconsider activation time
                .DeactivateOnExit<SuperchainTheory1>();
            ComponentCondition<EngrameventOfSouls2Spread>(id + 0x50, 1.5f, comp => comp.NumCasts > 0, "Tower baits")
                .ActivateOnEnter<EngravementOfSoulsTowers>();
            ComponentCondition<EngravementOfSoulsTowers>(id + 0x51, 1.0f, comp => comp.CastsStarted);
            ComponentCondition<EngrameventOfSouls2Spread>(id + 0x52, 0.7f, comp => !comp.Active, "Spreads")
                .DeactivateOnExit<EngrameventOfSouls2Spread>();
            ComponentCondition<EngravementOfSoulsTowers>(id + 0x60, 1.3f, comp => comp.NumCasts > 0, "Towers")
                .DeactivateOnExit<EngravementOfSoulsTowers>();
        }

        private void EngravementOfSouls3(uint id, float delay)
        {
            Cast(id, AID.Paradeigma, delay, 3);
            Cast(id + 0x10, AID.EngravementOfSouls, 3.2f, 4)
                .ActivateOnEnter<EngravementOfSouls3Hints>()
                .ActivateOnEnter<EngravementOfSoulsTethers>(); // tethers appear ~2.3s after cast ends
            Cast(id + 0x20, AID.UnnaturalEnchainment, 3.2f, 7, "Fixed towers")
                .ActivateOnEnter<UnnaturalEnchainment>() // note: tethers appear ~0.1s before cast start
                .ActivateOnEnter<EngravementOfSouls3Shock>() // towers start ~3s into cast
                .ActivateOnEnter<WhiteFlame>() // PATE happens ~5.1s into cast - record the sources, but don't show hints yet
                .DeactivateOnExit<EngravementOfSouls3Shock>(); // towers resolve at roughly the same time as cast end (sometimes slightly earlier, sometimes slightly later)
            ComponentCondition<EngravementOfSoulsTethers>(id + 0x30, 1.3f, comp => comp.NumCasts > 0, "Destroy platforms + Tethers"); // destroy platforms (sample casts) happens roughly at the same time
            ComponentCondition<TheosCross>(id + 0x40, 3.7f, comp => comp.Casters.Count > 0, "Cross/plus bait") // TODO: don't show name?
                .ActivateOnEnter<EngravementOfSouls3Spread>()
                .ActivateOnEnter<EngravementOfSoulsTowers>()
                .ActivateOnEnter<TheosCross>()
                .ActivateOnEnter<TheosSaltire>();
            ComponentCondition<TheosCross>(id + 0x41, 3.0f, comp => comp.NumCasts > 0, "Cross/plus resolve") // TODO: don't show name?
                .OnEnter(() => Module.FindComponent<WhiteFlame>()?.Enable())
                .DeactivateOnExit<TheosCross>()
                .DeactivateOnExit<TheosSaltire>();
            ComponentCondition<EngravementOfSouls3Spread>(id + 0x42, 0.3f, comp => !comp.Active, "Tower baits")
                .DeactivateOnExit<EngravementOfSouls3Spread>()
                .DeactivateOnExit<EngravementOfSoulsTethers>(); // keep active so that spread component can use it
            ComponentCondition<WhiteFlame>(id + 0x43, 0.8f, comp => comp.NumCasts > 0, "Line baits")
                .DeactivateOnExit<WhiteFlame>();
            ComponentCondition<EngravementOfSoulsTowers>(id + 0x50, 0.2f, comp => comp.CastsStarted);
            ComponentCondition<EngravementOfSoulsTowers>(id + 0x51, 2.0f, comp => comp.NumCasts > 0, "Towers resolve")
                .DeactivateOnExit<EngravementOfSoulsTowers>()
                .DeactivateOnExit<EngravementOfSouls3Hints>();

            OnTheSoul(id + 0x1000, 1.0f)
                .DeactivateOnExit<UnnaturalEnchainment>(); // TODO: deactivate 1.6s later, when env controls happen
        }

        private void Palladion(uint id, float delay)
        {
            ComponentCondition<UltimaBlade>(id, delay, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<Palladion>() // PATEs for clones happen ~2s before visual
                .ActivateOnEnter<UltimaBlade>()
                .DeactivateOnExit<UltimaBlade>()
                .SetHint(StateMachine.StateHint.Raidwide);
            Targetable(id + 0x10, false, 12.8f, "Disappear")
                .ActivateOnEnter<PalladionArena>(); // TODO: what is the real activation point?..
            Cast(id + 0x100, AID.Palladion, 2.4f, 8, "Limit cut start")
                .ActivateOnEnter<PalladionShockwave>()
                .ActivateOnEnter<PalladionStack>()
                .ActivateOnEnter<PalladionVoidzone>()
                .ActivateOnEnter<PalladionClearCut>() // note: first clear cut/white flame happens ~0.9s before cast end
                .ActivateOnEnter<PalladionWhiteFlame>();
            // TODO: consider adding intermediate states?
            ComponentCondition<PalladionShockwave>(id + 0x200, 21.8f, comp => comp.NumCasts >= 8, "Limit cut resolve")
                .DeactivateOnExit<PalladionShockwave>()
                .DeactivateOnExit<PalladionStack>()
                .DeactivateOnExit<PalladionClearCut>() // last clear cut/white flame resolve right before last jump
                .DeactivateOnExit<PalladionWhiteFlame>()
                .DeactivateOnExit<Palladion>();
            ComponentCondition<PalladionDestroyPlatforms>(id + 0x300, 9.2f, comp => comp.NumCasts > 0, "Destroy platforms")
                .ActivateOnEnter<PalladionDestroyPlatforms>() // TODO: what is the real activation point?
                .DeactivateOnExit<PalladionVoidzone>() // voidzones are cleared ~4.2s before
                .DeactivateOnExit<PalladionArena>(); // TODO: what is the real deactivation point?
            Targetable(id + 0x310, true, 3.0f, "Reappear");
            Cast(id + 0x400, AID.TheosUltima, 1.8f, 7, "Raidwide")
                .DeactivateOnExit<PalladionDestroyPlatforms>() // TODO: what is the real deactivation point?
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void SuperchainTheory2A(uint id, float delay)
        {
            Cast(id, AID.SuperchainTheory2A, delay, 5)
                .ActivateOnEnter<SuperchainTheory2A>(); // tethers appear 1.5s after cast end
            TrinityOfSoulsCast(id + 0x10, 3.2f);
            //ComponentCondition<SuperchainTheory2A>(id + 0x20, 0.1f, comp => comp.NumCasts >= 3, "Chain 1 (pairs)");
            //ComponentCondition<SuperchainTheory2A>(id + 0x30, 2.4f, comp => comp.NumCasts >= 4, "Chain 2 (mid)");
            TrinityOfSoulsResolve(id + 0x40, 2.6f);
            ComponentCondition<SuperchainTheory2A>(id + 0x50, 3.4f, comp => comp.NumCasts >= 7, "Superchain 2A resolve")
                .DeactivateOnExit<SuperchainTheory2A>();
        }

        private void SuperchainTheory2B(uint id, float delay)
        {
            Cast(id, AID.SuperchainTheory2B, delay, 5)
                .ActivateOnEnter<SuperchainTheory2B>(); // first tethers appear 1.5s after cast end
            Cast(id + 0x10, AID.Paradeigma, 3.2f, 3);
            CastStart(id + 0x20, AID.Parthenos, 5.2f);
            ComponentCondition<SuperchainTheory2B>(id + 0x30, 1.9f, comp => comp.NumCasts >= 2, "Chain 1 (donut + circle)");
            CastEnd(id + 0x40, 3.1f, "Middle line")
                .ActivateOnEnter<Parthenos>() // note: activating after first chains are resolved
                .DeactivateOnExit<Parthenos>();
            ComponentCondition<SuperchainTheory2B>(id + 0x50, 3.9f, comp => comp.NumCasts >= 4, "Chain 2 (spread/pairs)")
                .ActivateOnEnter<RayOfLight>(); // casts start 1.3s after previous boss cast end
            ComponentCondition<RayOfLight>(id + 0x60, 2.4f, comp => comp.NumCasts > 0, "Lines")
                .DeactivateOnExit<RayOfLight>();
            CastStart(id + 0x70, AID.UnnaturalEnchainment, 2.4f);
            ComponentCondition<SuperchainTheory2B>(id + 0x80, 4.2f, comp => comp.NumCasts >= 7, "Chain 3 (circles + spread/pairs)")
                .DeactivateOnExit<SuperchainTheory2B>();
            CastEnd(id + 0x90, 2.8f)
                .ActivateOnEnter<UnnaturalEnchainment>(); // note: activating after third chains are resolved
            ComponentCondition<UnnaturalEnchainment>(id + 0xA0, 1.2f, comp => comp.NumCasts > 0, "Destroy platforms");

            OnTheSoul(id + 0x1000, 2.0f)
                .DeactivateOnExit<UnnaturalEnchainment>(); // TODO: what is the real deactivation point?
        }
    }
}
