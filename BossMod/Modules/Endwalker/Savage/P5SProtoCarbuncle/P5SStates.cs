namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    class P5SStates : StateMachineBuilder
    {
        public P5SStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            SonicHowl(id, 9.1f);
            RubyGlowTopazStones(id + 0x10000, 2.2f);
            VenomousMassToxicCrunch(id + 0x20000, 8.1f);
            VenomTowers(id + 0x30000, 8.1f);
            VenomousMassToxicCrunch(id + 0x40000, 4.1f);
            RubyGlowTopazStonesDoubleRush(id + 0x50000, 9);
            SonicHowl(id + 0x60000, 4.8f);
            RubyGlowTopazCluster(id + 0x70000, 8.3f);
            VenomousMassToxicCrunch(id + 0x80000, 3.7f);
            VenomSquallSurge(id + 0x90000, 6.1f);
            ClawTail(id + 0xA0000, 2.4f);
            StarvingStampede(id + 0xB0000, 19.6f);

            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void SonicHowl(uint id, float delay)
        {
            Cast(id, AID.SonicHowl, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void RubyGlow(uint id, float delay)
        {
            Cast(id, AID.RubyGlow, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void RubyGlowTopazStones(uint id, float delay)
        {
            RubyGlow(id, delay);
            Cast(id + 0x1000, AID.TopazStones, 3.2f, 4)
                .ActivateOnEnter<TopazStones>();
            ComponentCondition<TopazStones>(id + 0x1100, 14, comp => comp.NumCasts > 0, "Cells")
                .DeactivateOnExit<TopazStones>();
        }

        private void VenomousMassToxicCrunch(uint id, float delay)
        {
            Cast(id, AID.VenomousMass, delay, 5)
                .ActivateOnEnter<VenomousMass>();
            ComponentCondition<VenomousMass>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Tankbuster 1")
                .DeactivateOnExit<VenomousMass>();

            Cast(id + 0x1000, AID.ToxicCrunch, 1.4f, 5)
                .ActivateOnEnter<ToxicCrunch>();
            ComponentCondition<ToxicCrunch>(id + 0x1002, 0.3f, comp => comp.NumCasts > 0, "Tankbuster2")
                .DeactivateOnExit<ToxicCrunch>();
        }

        private void VenomTowers(uint id, float delay)
        {
            ComponentCondition<VenomTowers>(id, delay, comp => comp.Active)
                .ActivateOnEnter<VenomTowers>();
            ComponentCondition<VenomTowers>(id + 0x10, 13, comp => !comp.Active, "Towers")
                .DeactivateOnExit<VenomTowers>();
        }

        private void RubyGlowTopazStonesDoubleRush(uint id, float delay)
        {
            RubyGlow(id, delay);
            Cast(id + 0x1000, AID.TopazStones, 3.2f, 4);
            Cast(id + 0x1010, AID.DoubleRush, 4.6f, 6, "Charge 1")
                .ActivateOnEnter<DoubleRush>()
                .DeactivateOnExit<DoubleRush>();
            ComponentCondition<DoubleRushReturn>(id + 0x1012, 2, comp => comp.NumCasts > 0, "Charge 2")
                .ActivateOnEnter<DoubleRushReturn>()
                .DeactivateOnExit<DoubleRushReturn>();
            // TODO: improve below...
            ComponentCondition<RubyReflection2>(id + 0x1020, 1.4f, comp => comp.NumCasts > 0, "Cells")
                .ActivateOnEnter<RubyReflection2>()
                .DeactivateOnExit<RubyReflection2>();
        }

        private void RubyGlowTopazCluster(uint id, float delay)
        {
            RubyGlow(id, delay);
            Cast(id + 0x1000, AID.TopazCluster, 2.1f, 4)
                .ActivateOnEnter<TopazCluster>();
            ComponentCondition<TopazCluster>(id + 0x1010, 11, comp => comp.NumCasts >= 2, "Cells 1");
            ComponentCondition<TopazCluster>(id + 0x1011, 2.5f, comp => comp.NumCasts >= 4, "Cells 2");
            ComponentCondition<TopazCluster>(id + 0x1012, 2.5f, comp => comp.NumCasts >= 7, "Cells 3");
            ComponentCondition<TopazCluster>(id + 0x1013, 2.5f, comp => comp.NumCasts >= 10, "Cells 4")
                .DeactivateOnExit<TopazCluster>();
        }

        private void VenomSquallSurge(uint id, float delay)
        {
            CastMulti(id, new AID[] { AID.VenomSquall, AID.VenomSurge }, delay, 5)
                .ActivateOnEnter<VenomSquallSurge>();
            ComponentCondition<VenomSquallSurge>(id + 2, 3.8f, comp => comp.Progress > 0, "Spread/stack");
            ComponentCondition<VenomSquallSurge>(id + 3, 3, comp => comp.Progress > 1, "Mid bait");
            ComponentCondition<VenomDrops>(id + 4, 3, comp => comp.NumCasts > 0)
                .ActivateOnEnter<VenomDrops>()
                .DeactivateOnExit<VenomDrops>();
            ComponentCondition<VenomSquallSurge>(id + 5, 3, comp => comp.Progress > 2, "Stack/spread")
                .DeactivateOnExit<VenomSquallSurge>();
        }

        private void ClawTail(uint id, float delay)
        {
            // note: tail to claw is ~0.5s shorter (and next state is longer), but other timings are unaffected
            CastMulti(id, new AID[] { AID.ClawToTail, AID.TailToClaw }, delay, 6)
                .ActivateOnEnter<ClawTail>();
            ComponentCondition<ClawTail>(id + 0x100, 4.1f, comp => comp.Progress >= 8, "Claw/tail")
                .DeactivateOnExit<ClawTail>();
        }

        private void StarvingStampede(uint id, float delay)
        {
            Targetable(id, false, delay, "Jumps disappear")
                .ActivateOnEnter<StarvingStampede>();
            ComponentCondition<VenomTowers>(id + 1, 1, comp => comp.Active)
                .ActivateOnEnter<VenomTowers>();
            Targetable(id + 2, true, 9.7f, "Jumps reappear")
                .DeactivateOnExit<StarvingStampede>();
            ComponentCondition<VenomTowers>(id + 3, 3.3f, comp => !comp.Active, "Towers")
                .DeactivateOnExit<VenomTowers>();
        }
    }
}
