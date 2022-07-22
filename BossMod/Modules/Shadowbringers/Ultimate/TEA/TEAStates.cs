namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class TEAStates : StateMachineBuilder
    {
        private TEA _module;

        public TEAStates(TEA module) : base(module)
        {
            _module = module;
            DeathPhase(0, Phase1LivingLiquid)
                .ActivateOnEnter<P1HandOfPain>();
        }

        private void Phase1LivingLiquid(uint id)
        {
            P1FluidSwing(id, 10.2f);
            P1Cascade(id + 0x10000, 4.1f);
            P1HandsProteansDollsCleaves(id + 0x20000, 14.2f);
            P1ProteansBoss(id + 0x30000, 10.2f);
            P1SplashDrainageCascade(id + 0x40000, 6.1f);

            SimpleState(id + 0xF0000, 1000, "???");
        }

        private void P1FluidSwing(uint id, float delay)
        {
            ComponentCondition<P1FluidSwing>(id, delay, comp => comp.NumCasts > 0, "Cleave")
                .ActivateOnEnter<P1FluidSwing>()
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        // keeps cascade component active
        private void P1Cascade(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.Cascade, delay, 4, true, "Raidwide")
                .ActivateOnEnter<P1Cascade>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P1HandsProteansDollsCleaves(uint id, float delay)
        {
            // TODO: hand of prayer/hand of parting check - detect...
            ComponentCondition<P1ProteanWaveTornadoVis>(id + 1, delay, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P1ProteanWaveTornadoVis>()
                .ActivateOnEnter<P1ProteanWaveTornado>();
            ComponentCondition<P1JagdDolls>(id + 2, 1, comp => comp.Active)
                .ActivateOnEnter<P1JagdDolls>();
            ComponentCondition<P1HandOfPartingPrayer>(id + 3, 1, comp => comp.Resolved, "Hand of parting/prayer")
                .ActivateOnEnter<P1HandOfPartingPrayer>()
                .DeactivateOnExit<P1HandOfPartingPrayer>();
            ComponentCondition<P1ProteanWaveTornadoVis>(id + 0x10, 1, comp => comp.Casters.Count == 0)
                .ActivateOnEnter<P1FluidStrike>()
                .ActivateOnEnter<P1FluidSwing>()
                .DeactivateOnExit<P1ProteanWaveTornadoVis>();
            ComponentCondition<P1FluidSwing>(id + 0x20, 1.1f, comp => comp.NumCasts > 0, "Cleaves")
                .DeactivateOnExit<P1FluidStrike>()
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P1ProteanWaveTornado>(id + 0x30, 1, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P1ProteanWaveTornado>();
            ComponentCondition<P1JagdDolls>(id + 0x100, 1, comp => comp.NumExhausts > 0, "Exhaust 1");
            // +0.1s: hand of pain start
            // +2.0s: pressurize
            // +2.6s: embolus spawn
            ComponentCondition<P1HandOfPain>(id + 0x200, 3.1f, comp => comp.NumCasts > 0, "HP check");
            ComponentCondition<P1JagdDolls>(id + 0x300, 7.5f, comp => comp.NumExhausts > 1, "Exhaust 2", 0.1f);
            ComponentCondition<P1FluidSwing>(id + 0x400, 6.4f, comp => comp.NumCasts > 0, "Cleaves")
                .ActivateOnEnter<P1FluidStrike>()
                .ActivateOnEnter<P1FluidSwing>()
                .DeactivateOnExit<P1FluidStrike>()
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P1ProteansBoss(uint id, float delay)
        {
            ActorCast(id + 1, _module.BossP1, AID.ProteanWaveLiquidVisBoss, delay, 3, true, "Protean baited")
                .ActivateOnEnter<P1ProteanWaveLiquidVisBoss>()
                .ActivateOnEnter<P1ProteanWaveLiquidVisHelper>()
                .DeactivateOnExit<P1ProteanWaveLiquidVisBoss>()
                .DeactivateOnExit<P1ProteanWaveLiquidVisHelper>();
            ComponentCondition<P1HandOfPain>(id + 0x10, 0.2f, comp => comp.NumCasts > 1, "HP check")
                .ActivateOnEnter<P1ProteanWaveLiquidInvis>()
                .ActivateOnEnter<P1Sluice>();
            ComponentCondition<P1ProteanWaveLiquidInvis>(id + 0x20, 1.9f, comp => comp.NumCasts > 0, "Protean 1");
            ComponentCondition<P1ProteanWaveLiquidInvis>(id + 0x30, 3.0f, comp => comp.NumCasts > 1, "Protean 2")
                .DeactivateOnExit<P1ProteanWaveLiquidInvis>()
                .DeactivateOnExit<P1Sluice>();
        }

        private void P1SplashDrainageCascade(uint id, float delay)
        {
            ComponentCondition<P1Splash>(id, delay, comp => comp.NumCasts > 0, "Splash start")
                .ActivateOnEnter<P1Splash>()
                .ActivateOnEnter<P1Drainage>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 1, 1.1f, comp => comp.NumCasts > 1).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 2, 1.1f, comp => comp.NumCasts > 2).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 3, 1.1f, comp => comp.NumCasts > 3).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 4, 1.1f, comp => comp.NumCasts > 4).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 5, 1.1f, comp => comp.NumCasts > 5) // drainage resolves almost at the same time
                .DeactivateOnExit<P1Splash>()
                .DeactivateOnExit<P1Drainage>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorCastStart(id + 0x100, _module.BossP1, AID.Cascade, 1.1f, true);
            ComponentCondition<P1HandOfPain>(id + 0x101, 1.3f, comp => comp.NumCasts > 2, "HP check");
            ActorCastEnd(id + 0x102, _module.BossP1, 2.7f, true, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }
    }
}
