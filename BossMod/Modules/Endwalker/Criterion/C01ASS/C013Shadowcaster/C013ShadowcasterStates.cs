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
            ShowOfStrength(id + 0x80000, 5.1f);
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

        // TODO: hints for mirrors
        private void InfernBrand2(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4);
            Cast(id + 0x10, AID.CrypticFlames, 3.2f, 8.3f)
                .ActivateOnEnter<CrypticFlames>(); // note: statuses appear right before cast start
            ComponentCondition<CrypticFlames>(id + 0x12, 2.7f, comp => comp.ReadyToBreak, "Lasers break start");

            CastStart(id + 0x20, AID.CastShadow, 6.7f)
                .ActivateOnEnter<BlazingBenifice>(); // TODO: proper activation time (first set of arcane fonts spawn around cryptic flames cast start, second spawn ~12s later)
            ComponentCondition<BlazingBenifice>(id + 0x21, 4.1f, comp => comp.NumCasts > 0, "Mirrors 1")
                .ActivateOnEnter<CastShadow>(); // all cast-shadow casts start at the same time
            CastEnd(id + 0x22, 0.6f);
            ComponentCondition<CastShadow>(id + 0x23, 0.7f, comp => comp.FirstAOECasters.Count == 0, "Pizzas 1");
            ComponentCondition<CastShadow>(id + 0x24, 2, comp => comp.SecondAOECasters.Count == 0, "Pizzas 2")
                .DeactivateOnExit<CastShadow>();

            CastStart(id + 0x30, AID.FiresteelFracture, 7.7f);
            ComponentCondition<BlazingBenifice>(id + 0x31, 1, comp => comp.NumCasts >= 5, "Mirrors 2")
                .ActivateOnEnter<FiresteelFracture>()
                .DeactivateOnExit<CrypticFlames>()
                .DeactivateOnExit<BlazingBenifice>();
            CastEnd(id + 0x32, 4);
            ComponentCondition<FiresteelFracture>(id + 0x33, 0.2f, comp => comp.NumCasts > 0, "Tankbuster")
                .DeactivateOnExit<FiresteelFracture>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void InfernBrand3(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4)
                .ActivateOnEnter<CrypticFlames>(); // lasers gain counter 1C1 ~1.9s after cast end
            Cast(id + 0x10, AID.InfernWave, 4.2f, 4);
            Cast(id + 0x20, AID.Banishment, 4.5f, 4)
                .ActivateOnEnter<InfernWave1>() // two of the beacons activate ~2.6s after cast start
                .ActivateOnEnter<PortalsWave>(); // portal statuses appear right at cast end, eobjanims on portals happen ~0.8s after cast end
            Cast(id + 0x30, AID.InfernWard, 4.2f, 4);
            ComponentCondition<PortalsWave>(id + 0x40, 2.9f, comp => comp.Done, "Portals")
                .OnExit(() => Module.FindComponent<InfernWave1>()!.ShowHints = true)
                .DeactivateOnExit<PortalsWave>();
            ComponentCondition<InfernWave1>(id + 0x50, 4.8f, comp => comp.NumCasts > 0, "Wave 1");
            // +0.9s: stun
            // +5.9s: stun end
            ComponentCondition<InfernWave1>(id + 0x60, 8, comp => comp.NumCasts > 4, "Wave 2")
                .DeactivateOnExit<InfernWave1>();
        }

        private void InfernBrand4(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4);
            Cast(id + 0x10, AID.CrypticPortal, 3.2f, 4)
                .ActivateOnEnter<PortalsMirror>() // eobjanims ~0.8s after cast end
                .ActivateOnEnter<FiresteelStrike>();
            Cast(id + 0x20, AID.FiresteelStrike, 6.2f, 4.9f);
            ComponentCondition<FiresteelStrike>(id + 0x22, 0.3f, comp => comp.NumJumps > 0, "Jump 1");
            ComponentCondition<PortalsMirror>(id + 0x30, 1, comp => comp.NumCasts > 0, "Mirror aoe")
                .DeactivateOnExit<PortalsMirror>();
            ComponentCondition<FiresteelStrike>(id + 0x31, 0.5f, comp => comp.NumJumps > 1, "Jump 2");

            Cast(id + 0x40, AID.BlessedBeacon, 3.1f, 4.9f);
            ComponentCondition<FiresteelStrike>(id + 0x42, 0.5f, comp => comp.NumCleaves > 0, "Cleave 1");
            ComponentCondition<FiresteelStrike>(id + 0x43, 2.1f, comp => comp.NumCleaves > 1, "Cleave 2")
                .DeactivateOnExit<FiresteelStrike>();
        }

        private void InfernBrand5(uint id, float delay)
        {
            Cast(id, AID.InfernBrand, delay, 4);
            Cast(id + 0x10, AID.InfernWave, 4.2f, 4)
                .ActivateOnEnter<InfernWave2>(); // first beacon activates ~2.3s after cast end
            Cast(id + 0x20, AID.CrypticFlames, 4.2f, 8.3f)
                .ActivateOnEnter<CrypticFlames>(); // note: statuses appear right before cast start
            ComponentCondition<CrypticFlames>(id + 0x22, 2.7f, comp => comp.ReadyToBreak, "Lasers break start");

            ComponentCondition<InfernWave2>(id + 0x30, 1.3f, comp => comp.NumCasts > 0, "Wave 1");
            CastStart(id + 0x40, AID.PureFire, 8);
            ComponentCondition<InfernWave2>(id + 0x41, 2, comp => comp.NumCasts > 2, "Wave 2");
            CastEnd(id + 0x42, 1);
            ComponentCondition<PureFire>(id + 0x50, 0.8f, comp => comp.Casters.Count > 0, "Puddle bait")
                .ActivateOnEnter<PureFire>();
            ComponentCondition<PureFire>(id + 0x51, 3, comp => comp.Casters.Count == 0)
                .DeactivateOnExit<PureFire>();

            CastStart(id + 0x60, AID.CastShadow, 4.9f);
            ComponentCondition<InfernWave2>(id + 0x61, 0.4f, comp => comp.NumCasts > 4, "Wave 3")
                .ActivateOnEnter<CastShadow>();
            CastEnd(id + 0x62, 4.4f);
            ComponentCondition<CastShadow>(id + 0x63, 0.7f, comp => comp.FirstAOECasters.Count == 0, "Pizzas 1");
            ComponentCondition<CastShadow>(id + 0x64, 2, comp => comp.SecondAOECasters.Count == 0, "Pizzas 2")
                .DeactivateOnExit<CastShadow>();
            ComponentCondition<InfernWave2>(id + 0x65, 3, comp => comp.NumCasts > 6, "Wave 4")
                .DeactivateOnExit<InfernWave2>()
                .DeactivateOnExit<CrypticFlames>();

            FiresteelFracture(id + 0x300, 1.6f);
        }
    }
}
