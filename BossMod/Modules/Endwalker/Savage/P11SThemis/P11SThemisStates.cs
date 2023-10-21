namespace BossMod.Endwalker.Savage.P11SThemis
{
    class P11SThemisStates : StateMachineBuilder
    {
        public P11SThemisStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            Eunomia(id, 10.2f);
            Dike(id + 0x10000, 3.2f);
            JuryOverruling(id + 0x20000, 7.2f);
            UpheldOverruling(id + 0x30000, 6.2f);
            DivisiveOverruling(id + 0x40000, 7.3f);
            Styx(id + 0x50000, 4.3f, 5);
            ArcaneRevelationMirrors(id + 0x60000, 10.4f);
            ShadowedMessengers(id + 0x70000, 22.9f);
            Styx(id + 0x80000, 3.1f, 6);
            Lightstream(id + 0x90000, 8.2f);
            Eunomia(id + 0xA0000, 7.2f);
            UpheldOverruling(id + 0xB0000, 7.2f);
            DarkAndLight(id + 0xC0000, 14.3f);
            Styx(id + 0xD0000, 3.3f, 7);
            Dike(id + 0xE0000, 5.1f);
            DarkCurrent(id + 0xF0000, 15.4f);
            JuryOverruling(id + 0x100000, 2.6f);
            UpheldOverruling(id + 0x110000, 6.2f);
            DivisiveOverruling(id + 0x120000, 9.3f);
            Eunomia(id + 0x130000, 8.2f);
            LetterOfTheLaw(id + 0x140000, 22.4f);
            Styx(id + 0x150000, 3.6f, 8);
            Lightstream(id + 0x160000, 7.2f);
            Dike(id + 0x170000, 9.2f);
            JuryOverruling(id + 0x180000, 10.1f);
            Eunomia(id + 0x190000, 6.1f);
            Cast(id + 0x1A0000, AID.UltimateVerdict, 6.5f, 10, "Enrage");
        }

        private void Eunomia(uint id, float delay)
        {
            Cast(id, AID.Eunomia, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Dike(uint id, float delay)
        {
            Cast(id, AID.Dike, delay, 7, "Tankbuster 1")
                .ActivateOnEnter<Dike>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<Dike>(id + 2, 3.1f, comp => comp.NumCasts > 0, "Tankbuster 2")
                .DeactivateOnExit<Dike>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void Styx(uint id, float delay, int numCasts)
        {
            Cast(id, AID.Styx, delay, 5, "Stack hit 1")
                .ActivateOnEnter<Styx>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<Styx>(id + 0x10, 1.1f * numCasts - 1.0f, comp => comp.NumCasts >= numCasts, $"Stack hit {numCasts}")
                .DeactivateOnExit<Styx>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void JuryOverrulingResolve(uint id, float delay)
        {
            CastEnd(id, delay)
                .ActivateOnEnter<JuryOverrulingProtean>();
            ComponentCondition<JuryOverrulingProtean>(id + 0x10, 1.8f, comp => comp.NumCasts > 0, "Proteans")
                .ActivateOnEnter<InevitableLawSentence>()
                .DeactivateOnExit<JuryOverrulingProtean>();
            ComponentCondition<InevitableLawSentence>(id + 0x20, 4.2f, comp => !comp.Active, "Circles/donuts + Party/pair stacks")
                .ActivateOnEnter<IllusoryGlare>() // casts start 1.1s after proteans
                .ActivateOnEnter<IllusoryGloom>()
                .DeactivateOnExit<IllusoryGlare>() // casts end 0.1s before stacks
                .DeactivateOnExit<IllusoryGloom>()
                .DeactivateOnExit<InevitableLawSentence>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void JuryOverruling(uint id, float delay)
        {
            CastStartMulti(id, new[] { AID.JuryOverrulingLight, AID.JuryOverrulingDark }, delay);
            JuryOverrulingResolve(id + 0x100, 6);
        }

        private void UpheldOverruling(uint id, float delay)
        {
            CastMulti(id, new[] { AID.UpheldOverrulingLight, AID.UpheldOverrulingDark }, delay, 7.3f)
                .ActivateOnEnter<UpheldOverruling>();
            ComponentCondition<UpheldOverruling>(id + 0x10, 0.4f, comp => !comp.Active, "Stack/away from tank")
                .ActivateOnEnter<InevitableLawSentence>()
                .DeactivateOnExit<UpheldOverruling>();
            ComponentCondition<InevitableLawSentence>(id + 0x20, 4.2f, comp => !comp.Active, "In/out + Party/pair stacks")
                .ActivateOnEnter<LightburstBoss>() // cast starts 0.6s after jump
                .ActivateOnEnter<DarkPerimeterBoss>()
                .DeactivateOnExit<LightburstBoss>() // cast ends 0.1s before stacks
                .DeactivateOnExit<DarkPerimeterBoss>()
                .DeactivateOnExit<InevitableLawSentence>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void DivisiveOverrulingResolve(uint id, float delay)
        {
            ComponentCondition<DivisiveOverruling>(id, delay, comp => comp.NumCasts > 0, "Narrow line");
            ComponentCondition<DivisiveOverruling>(id + 1, 2.6f, comp => comp.NumCasts > 1, "Wide line/sides")
                .DeactivateOnExit<DivisiveOverruling>();
            ComponentCondition<InevitableLawSentence>(id + 0x10, 0.1f, comp => !comp.Active, "Party/pair stacks")
                .DeactivateOnExit<InevitableLawSentence>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void DivisiveOverruling(uint id, float delay)
        {
            CastMulti(id, new[] { AID.DivisiveOverrulingSoloLight, AID.DivisiveOverrulingSoloDark }, delay, 6.3f)
                .ActivateOnEnter<InevitableLawSentence>()
                .ActivateOnEnter<DivisiveOverruling>();
            DivisiveOverrulingResolve(id + 0x100, 1.9f);
        }

        private void ArcaneRevelationMirrors(uint id, float delay)
        {
            CastMulti(id, new[] { AID.ArcaneRevelationMirrorsLight, AID.ArcaneRevelationMirrorsDark }, delay, 5)
                .ActivateOnEnter<ArcaneRevelation>(); // PATE happens 1s after cast end, actual cast starts ~3s after PATE
            CastMulti(id + 0x10, new[] { AID.DismissalOverrulingLight, AID.DismissalOverrulingDark }, 2.1f, 5)
                .ActivateOnEnter<InevitableLawSentence>() // TODO: not sure whether this should be activated now (earliest point) or later...
                .ActivateOnEnter<DismissalOverruling>();
            ComponentCondition<DismissalOverruling>(id + 0x20, 0.5f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<DismissalOverruling>();
            ComponentCondition<ArcaneRevelation>(id + 0x30, 2, comp => comp.NumCasts > 0, "Mirrors")
                .DeactivateOnExit<ArcaneRevelation>();
            ComponentCondition<InevitableLawSentence>(id + 0x40, 3.0f, comp => !comp.Active, "In/out + Party/pair stacks")
                .ActivateOnEnter<InnerLight>() // note: these start casting together with dismissal overruling, but we want to start showing hints only after mirrors are done
                .ActivateOnEnter<OuterDark>()
                .DeactivateOnExit<InnerLight>() // casts end 0.1s before stacks
                .DeactivateOnExit<OuterDark>()
                .DeactivateOnExit<InevitableLawSentence>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void ShadowedMessengers(uint id, float delay)
        {
            Cast(id, AID.ShadowedMessengers, delay, 4);
            ComponentCondition<DivisiveOverruling>(id + 0x10, 5.2f, comp => comp.AOEs.Count > 0) // clones start their casts
                .ActivateOnEnter<DivisiveOverruling>();
            CastStartMulti(id + 0x20, new[] { AID.DivisiveOverrulingBossLight, AID.DivisiveOverrulingBossDark }, 4.0f);
            ComponentCondition<DivisiveOverruling>(id + 0x30, 5.2f, comp => comp.NumCasts > 0, "Clone lines 1");
            // +1.6s: upheld ruling casts start
            CastEnd(id + 0x40, 2.6f)
                .ActivateOnEnter<InevitableLawSentence>(); // note: activated after first lines (cross) are resolved
            ComponentCondition<DivisiveOverruling>(id + 0x50, 0.6f, comp => comp.NumCasts > 2, "Clone lines 2");
            ComponentCondition<DivisiveOverruling>(id + 0x60, 1.3f, comp => comp.NumCasts > 5, "Narrow line");
            ComponentCondition<DivisiveOverruling>(id + 0x70, 2.6f, comp => comp.NumCasts > 6, "Wide line/sides")
                .DeactivateOnExit<DivisiveOverruling>();
            ComponentCondition<InevitableLawSentence>(id + 0x80, 0.1f, comp => !comp.Active, "Party/pair stacks")
                .DeactivateOnExit<InevitableLawSentence>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<UpheldOverruling>(id + 0x100, 5.4f, comp => !comp.Active, "Stack away from tank") // note: casts end ~0.4s before aoes
                .ActivateOnEnter<UpheldOverruling>() // note: actual casts start way earlier, but we want to start showing these hints only after previous stacks resolve
                .DeactivateOnExit<UpheldOverruling>();
            // jury overruling slightly overlaps with upheld aoes
            CastStartMulti(id + 0x110, new[] { AID.JuryOverrulingLight, AID.JuryOverrulingDark }, 3.8f)
                .ActivateOnEnter<LightburstClone>() // cast starts 0.6s after jump
                .ActivateOnEnter<DarkPerimeterClone>();
            ComponentCondition<DarkPerimeterClone>(id + 0x120, 0.3f, comp => comp.NumCasts > 0, "Circle + donut")
                .DeactivateOnExit<LightburstClone>()
                .DeactivateOnExit<DarkPerimeterClone>();
            JuryOverrulingResolve(id + 0x200, 5.7f);
        }

        private void Lightstream(uint id, float delay)
        {
            Cast(id, AID.Lightstream, delay, 4);
            ComponentCondition<Lightstream>(id + 0x10, 12.2f, comp => comp.NumCasts > 0, "Rotating orbs start")
                .ActivateOnEnter<Lightstream>();
            CastStartMulti(id + 0x20, new[] { AID.DivisiveOverrulingSoloLight, AID.DivisiveOverrulingSoloDark }, 0.6f); // TODO: second time it is 0.1 instead
            ComponentCondition<Lightstream>(id + 0x30, 5.8f, comp => comp.NumCasts >= 21, maxOverdue: 2) // TODO: second time it is 6.3 instead
                .ActivateOnEnter<InevitableLawSentence>()
                .ActivateOnEnter<DivisiveOverruling>()
                .DeactivateOnExit<Lightstream>();
            CastEnd(id + 0x40, 0.5f); // TODO: second time it typically happens just before lightstream end instead
            DivisiveOverrulingResolve(id + 0x100, 1.9f);
        }

        private void DarkAndLight(uint id, float delay)
        {
            Cast(id, AID.DarkAndLight, delay, 4)
                .ActivateOnEnter<DarkAndLight>();
            CastMulti(id + 0x1000, new[] { AID.ArcaneRevelationSpheresLight, AID.ArcaneRevelationSpheresDark }, 8.2f, 5)
                .ActivateOnEnter<ArcaneRevelation>(); // PATE happens 1s after cast end, actual cast starts ~3s after PATE
            ComponentCondition<ArcaneRevelation>(id + 0x1010, 9.7f, comp => comp.NumCasts > 0, "Spheres")
                .ExecOnEnter<DarkAndLight>(comp => comp.ShowSafespots = false) // TODO: reconsider?
                .DeactivateOnExit<ArcaneRevelation>();
            JuryOverruling(id + 0x2000, 0.5f);
            DivisiveOverruling(id + 0x3000, 6.2f);
            Cast(id + 0x4000, AID.EmissarysWill, 3.2f, 4, "Tethers resolve")
                .DeactivateOnExit<DarkAndLight>();
        }

        private void DarkCurrent(uint id, float delay)
        {
            Cast(id, AID.DarkCurrent, delay, 4)
                .ActivateOnEnter<DarkCurrent>();
            CastStart(id + 0x10, AID.BlindingLight, 5.2f);
            ComponentCondition<DarkCurrent>(id + 0x11, 2.9f, comp => comp.NumCasts > 0, "Rotating aoe start")
                .ActivateOnEnter<BlindingLight>();
            CastEnd(id + 0x12, 2.1f, "Spreads")
                .DeactivateOnExit<BlindingLight>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<DarkCurrent>(id + 0x20, 5.5f, comp => comp.NumCasts >= 24, "Rotating aoe end")
                .DeactivateOnExit<DarkCurrent>();
        }

        private void LetterOfTheLaw(uint id, float delay)
        {
            Cast(id, AID.LetterOfTheLaw, delay, 4);
            CastMulti(id + 0x10, new[] { AID.TwofoldRevelationLight, AID.TwofoldRevelationDark }, 2.1f, 5)
                .ActivateOnEnter<ArcaneRevelation>() // PATE happens 1s after cast end, actual cast starts ~3s after PATE
                .ActivateOnEnter<UpheldOverruling>(); // ruling casts start ~1.1s after this cast end
            Cast(id + 0x20, AID.HeartOfJudgment, 6.2f, 3);
            ComponentCondition<ArcaneRevelation>(id + 0x30, 0.5f, comp => comp.NumCasts > 0, "Mirrors + spheres")
                .DeactivateOnExit<ArcaneRevelation>();
            ComponentCondition<UpheldOverruling>(id + 0x40, 2.5f, comp => !comp.Active, "Stack away from tank")
                .DeactivateOnExit<UpheldOverruling>();
            // +3.2s: divisive casts start
            ComponentCondition<DarkPerimeterClone>(id + 0x50, 4.1f, comp => comp.NumCasts > 0, "Circle + donut")
                .ActivateOnEnter<LightburstClone>() // cast starts 0.6s after jump
                .ActivateOnEnter<DarkPerimeterClone>()
                .DeactivateOnExit<LightburstClone>()
                .DeactivateOnExit<DarkPerimeterClone>();
            ComponentCondition<HeartOfJudgment>(id + 0x60, 4.9f, comp => comp.NumCasts > 0, "Towers")
                .ActivateOnEnter<HeartOfJudgment>() // note: activated now, since towers should be resolved after circle/donut
                .DeactivateOnExit<HeartOfJudgment>();
            ComponentCondition<DivisiveOverruling>(id + 0x70, 3.4f, comp => comp.NumCasts > 0, "Clone lines 1")
                .ActivateOnEnter<DivisiveOverruling>();
            CastStartMulti(id + 0x80, new[] { AID.DismissalOverrulingLight, AID.DismissalOverrulingDark }, 0.7f);
            ComponentCondition<DivisiveOverruling>(id + 0x90, 2.5f, comp => comp.NumCasts > 2, "Clone lines 2")
                .DeactivateOnExit<DivisiveOverruling>();
            CastEnd(id + 0xA0, 2.5f)
                .ActivateOnEnter<DismissalOverruling>() // TODO: is this the best place to activate?
                .ActivateOnEnter<InevitableLawSentence>(); // TODO: is this the best place to activate?
            ComponentCondition<DismissalOverruling>(id + 0xB0, 0.5f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<DismissalOverruling>();
            ComponentCondition<InevitableLawSentence>(id + 0xC0, 5.1f, comp => !comp.Active, "In/out + Party/pair stacks")
                .ActivateOnEnter<InnerLight>() // note: these start casting together with dismissal overruling, but we want to start showing hints only after knockbacks are done (?)
                .ActivateOnEnter<OuterDark>()
                .DeactivateOnExit<InnerLight>() // casts end 0.1s before stacks
                .DeactivateOnExit<OuterDark>()
                .DeactivateOnExit<InevitableLawSentence>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }
    }
}
