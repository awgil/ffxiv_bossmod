namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    class C013ShadowcasterStates : StateMachineBuilder
    {
        public C013ShadowcasterStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            ShowOfStrength(id, 10.2f);
            InfernBrand1(id + 0x10000, 8.2f);
            FiresteelFracture(id + 0x20000, 5.1f);
            InfernBrand2(id + 0x30000, 8.2f);
            ShowOfStrength(id + 0x40000, 4.1f);
            InfernBrand3(id + 0x50000, 8.2f);
            FiresteelFracture(id + 0x60000, 3.4f);
            InfernBrand4(id + 0x70000, 8.2f);
            ShowOfStrength(id + 0x80000, 5.2f);
            InfernBrand5(id + 0x90000, 8.2f);

            SimpleState(id + 0xFF000000, 10000, "???");
        }

        private void ShowOfStrength(uint id, float delay)
        {
            Cast(id, AID.ShowOfStrength, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void FiresteelFracture(uint id, float delay)
        {
            Cast(id, AID.FiresteelFracture, delay, 5)
                .ActivateOnEnter<FiresteelFracture>();
            ComponentCondition<FiresteelFracture>(id + 2, 0.2f, comp => comp.NumCasts > 0, "Tankbuster")
                .DeactivateOnExit<FiresteelFracture>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void InfernBrand1(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4);
            Cast(id + 0x10, AID.CrypticPortal, 3.2f, 4)
                .ActivateOnEnter<PortalsBurn>(); // eobjanims ~0.8s after cast end
            Cast(id + 0x20, AID.FiresteelStrike, 6.2f, 4.9f)
                .ActivateOnEnter<FiresteelStrike>(); // TODO: consider activating earlier?..
            ComponentCondition<FiresteelStrike>(id + 0x22, 0.3f, comp => comp.NumJumps > 0, "Jump 1");
            ComponentCondition<PortalsBurn>(id + 0x30, 1, comp => comp.NumCasts > 0, "Portal aoe")
                .DeactivateOnExit<PortalsBurn>();
            ComponentCondition<FiresteelStrike>(id + 0x31, 0.5f, comp => comp.NumJumps > 1, "Jump 2");

            Cast(id + 0x40, AID.BlessedBeacon, 3.1f, 4.9f);
            ComponentCondition<FiresteelStrike>(id + 0x42, 0.5f, comp => comp.NumCleaves > 0, "Cleave 1");
            ComponentCondition<FiresteelStrike>(id + 0x43, 2.2f, comp => comp.NumCleaves > 1, "Cleave 2")
                .DeactivateOnExit<FiresteelStrike>();
        }

        private void InfernBrand2(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4);
            // +0.8s: PATE 1E44 on 8 Lasers
            // +3.0s: statuses (brands on players, counters 1C2-1C9 on lasers)
            // right at next cast start - Arcane Font spawn
            Cast(id + 0x10, AID.CrypticFlames, 3.2f, 8.3f)
                .ActivateOnEnter<BlazingBenifice>();
            // +2.7s: brand -> flame debuffs
            // on laser break - PATE 1E3A on laser
            // +3.7s: second set of Arcane Fonts spawn

            CastStart(id + 0x20, AID.CastShadow, 9.3f);
            ComponentCondition<BlazingBenifice>(id + 0x21, 4.1f, comp => comp.NumCasts > 0, "Mirrors 1")
                .ActivateOnEnter<CastShadow1>(); // TODO: proper activation time
            CastEnd(id + 0x22, 0.6f);
            ComponentCondition<CastShadow1>(id + 0x23, 0.7f, comp => comp.NumCasts > 0, "Pizzas 1")
                .DeactivateOnExit<CastShadow1>();
            ComponentCondition<CastShadow2>(id + 0x24, 2, comp => comp.NumCasts > 0, "Pizzas 2")
                .ActivateOnEnter<CastShadow2>()
                .DeactivateOnExit<CastShadow2>();

            CastStart(id + 0x30, AID.FiresteelFracture, 7.7f);
            ComponentCondition<BlazingBenifice>(id + 0x31, 1, comp => comp.NumCasts >= 5, "Mirrors 2")
                .ActivateOnEnter<FiresteelFracture>()
                .DeactivateOnExit<BlazingBenifice>();
            CastEnd(id + 0x32, 4);
            ComponentCondition<FiresteelFracture>(id + 0x33, 0.2f, comp => comp.NumCasts > 0, "Tankbuster")
                .DeactivateOnExit<FiresteelFracture>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        // TODO: component!
        private void InfernBrand3(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4);
            // +0.8s: PATE 1E44 on 4 lasers, 1E43 on 4 beacons
            // +1.2s: 4 portals spawn
            // +1.9s: lasers gain counter 1C1
            Cast(id + 0x10, AID.InfernWave, 4.2f, 4);
            // +0.8s: beacons gain counter 1CC, 4 activate beacon cast events
            Cast(id + 0x20, AID.Banishment, 4.5f, 4);
            // +0.0s: statuses on players
            // +0.7s: eobjanim on portals
            Cast(id + 0x30, AID.InfernWard, 4.2f, 4);
            // +0.9s: 4 activate beacon cast events
            // +1.3s: beacons gain counter 1F3
            // +1.4s: 4 29847 cast events
            // +3.1s: teleports
            ComponentCondition<InfernWave>(id + 0x40, 7.8f, comp => comp.NumCasts > 0, "Wave 1")
                .ActivateOnEnter<InfernWave>();
            // TODO: timings..
            ComponentCondition<InfernWave>(id + 0x41, 8, comp => comp.NumCasts > 4, "Wave 2")
                .DeactivateOnExit<InfernWave>();
        }

        private void InfernBrand4(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4);
            Cast(id + 0x10, AID.CrypticPortal, 3.2f, 4)
                .ActivateOnEnter<PortalsMirror>(); // eobjanims ~0.8s after cast end
            Cast(id + 0x20, AID.FiresteelStrike, 6.2f, 4.9f)
                .ActivateOnEnter<FiresteelStrike>(); // TODO: consider activating earlier?..
            ComponentCondition<FiresteelStrike>(id + 0x22, 0.3f, comp => comp.NumJumps > 0, "Jump 1");
            ComponentCondition<PortalsMirror>(id + 0x30, 1, comp => comp.NumCasts > 0, "Mirror aoe")
                .DeactivateOnExit<PortalsMirror>();
            ComponentCondition<FiresteelStrike>(id + 0x31, 0.5f, comp => comp.NumJumps > 1, "Jump 2");

            Cast(id + 0x40, AID.BlessedBeacon, 3.1f, 4.9f);
            ComponentCondition<FiresteelStrike>(id + 0x42, 0.5f, comp => comp.NumCleaves > 0, "Cleave 1");
            ComponentCondition<FiresteelStrike>(id + 0x43, 2.1f, comp => comp.NumCleaves > 1, "Cleave 2")
                .DeactivateOnExit<FiresteelStrike>();
        }

        // TODO: component! timings!
        private void InfernBrand5(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4);
            Cast(id + 0x10, AID.InfernWave, 4.2f, 4);
            Cast(id + 0x20, AID.CrypticFlames, 4.2f, 8.3f);
            Cast(id + 0x100, AID.PureFire, 11.9f, 3);

            Cast(id + 0x200, AID.CastShadow, 8.6f, 4.8f)
                .ActivateOnEnter<CastShadow1>(); // TODO: proper activation time
            ComponentCondition<CastShadow1>(id + 0x202, 0.7f, comp => comp.NumCasts > 0, "Pizzas 1")
                .DeactivateOnExit<CastShadow1>();
            ComponentCondition<CastShadow2>(id + 0x203, 2, comp => comp.NumCasts > 0, "Pizzas 2")
                .ActivateOnEnter<CastShadow2>()
                .DeactivateOnExit<CastShadow2>();

            FiresteelFracture(id + 0x300, 4.7f);
        }
    }
}
