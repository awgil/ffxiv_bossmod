namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class TEAStates : StateMachineBuilder
    {
        private TEA _module;

        public TEAStates(TEA module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1LivingLiquid, "P1: Living Liquid")
                .ActivateOnEnter<P1HandOfPain>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsDead;
            SimplePhase(1, Phase2BruteJusticeCruiseChaser, "P2: BJ+CC")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.BruteJustice()?.IsDestroyed ?? true) && (_module.CruiseChaser()?.IsDestroyed ?? true);
        }

        private void Phase1LivingLiquid(uint id)
        {
            P1FluidSwing(id, 10.2f);
            P1Cascade(id + 0x10000, 4.1f);
            P1HandsProteansDollsCleaves(id + 0x20000, 11.1f);
            P1ProteansBoss(id + 0x30000, 10.2f);
            P1SplashDrainageCascade(id + 0x40000, 6.1f);
            P1Throttle(id + 0x50000, 4.6f);
            P1ProteansBoth(id + 0x60000, 7.7f);
            P1PainSplashCleaves(id + 0x70000, 5.1f);
            ActorCast(id + 0x80000, _module.BossP1, AID.Enrage, 3.1f, 4, true, "Enrage");
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

        private State P1HandOfPain(uint id, float delay, int seq)
        {
            return ComponentCondition<P1HandOfPain>(id, delay, comp => comp.NumCasts >= seq, $"HP check {seq}");
        }

        private void P1HandsProteansDollsCleaves(uint id, float delay)
        {
            Condition(id, delay, () => (_module.LiquidHand()?.ModelState.ModelState ?? 0) != 0, "Hand of parting/prayer bait");
            ComponentCondition<P1ProteanWaveTornadoVis>(id + 1, 3.1f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P1HandOfPartingPrayer>()
                .ActivateOnEnter<P1ProteanWaveTornadoVis>()
                .ActivateOnEnter<P1ProteanWaveTornado>();
            ComponentCondition<P1JagdDolls>(id + 2, 1, comp => comp.Active)
                .ActivateOnEnter<P1JagdDolls>();
            ComponentCondition<P1HandOfPartingPrayer>(id + 3, 1, comp => comp.Resolved, "Resolve")
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
            P1HandOfPain(id + 0x200, 3.1f, 1);
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
            P1HandOfPain(id + 0x10, 0.2f, 2)
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
            P1HandOfPain(id + 0x101, 1.3f, 3);
            ActorCastEnd(id + 0x102, _module.BossP1, 2.7f, true, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P1Throttle(uint id, float delay)
        {
            ComponentCondition<P1Throttle>(id, delay, comp => comp.Applied, "Debuffs")
                .ActivateOnEnter<P1Throttle>()
                .DeactivateOnExit<P1Throttle>();
        }

        private void P1ProteansBoth(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.ProteanWaveLiquidVisBoss, delay, 3, true, "Protean baited")
                .ActivateOnEnter<P1ProteanWaveLiquidVisBoss>()
                .ActivateOnEnter<P1ProteanWaveLiquidVisHelper>()
                .DeactivateOnExit<P1ProteanWaveLiquidVisBoss>()
                .DeactivateOnExit<P1ProteanWaveLiquidVisHelper>();
            P1HandOfPain(id + 0x10, 2, 4)
                .ActivateOnEnter<P1ProteanWaveLiquidInvis>()
                .ActivateOnEnter<P1Sluice>()
                .ActivateOnEnter<P1ProteanWaveTornadoVis>()
                .ActivateOnEnter<P1ProteanWaveTornado>();
            ComponentCondition<P1ProteanWaveLiquidInvis>(id + 0x11, 0.1f, comp => comp.NumCasts > 0, "Protean 1");
            ComponentCondition<P1ProteanWaveTornadoVis>(id + 0x12, 0.9f, comp => comp.Casters.Count > 0);
            ComponentCondition<P1ProteanWaveLiquidInvis>(id + 0x20, 2.1f, comp => comp.NumCasts > 1, "Protean 2")
                .DeactivateOnExit<P1ProteanWaveLiquidInvis>()
                .DeactivateOnExit<P1Sluice>();
            ComponentCondition<P1ProteanWaveTornadoVis>(id + 0x21, 0.8f, comp => comp.Casters.Count == 0)
                .DeactivateOnExit<P1ProteanWaveTornadoVis>();
            ComponentCondition<P1ProteanWaveTornado>(id + 0x30, 2.1f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P1ProteanWaveTornado>();
            // +3.9s: pressurize
            // +4.5s: embolus spawn
            Condition(id + 0x40, 3.9f, () => (_module.LiquidHand()?.ModelState.ModelState ?? 0) != 0, "Hand of parting/prayer bait");
            ComponentCondition<P1HandOfPartingPrayer>(id + 0x41, 5.1f, comp => comp.Resolved, "Resolve")
                .ActivateOnEnter<P1HandOfPartingPrayer>()
                .DeactivateOnExit<P1HandOfPartingPrayer>();
        }

        private void P1PainSplashCleaves(uint id, float delay)
        {
            P1HandOfPain(id + 1, delay, 5)
                .DeactivateOnExit<P1HandOfPain>();
            ComponentCondition<P1Splash>(id + 0x10, 0.3f, comp => comp.NumCasts > 0, "Splash start")
                .ActivateOnEnter<P1Splash>()
                .ActivateOnEnter<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 0x11, 1.1f, comp => comp.NumCasts > 1).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 0x12, 1.1f, comp => comp.NumCasts > 2).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 0x13, 1.1f, comp => comp.NumCasts > 3).SetHint(StateMachine.StateHint.Raidwide)
                .DeactivateOnExit<P1Splash>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1FluidSwing>(id + 0x20, 1.2f, comp => comp.NumCasts > 0, "Cleave")
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void Phase2BruteJusticeCruiseChaser(uint id)
        {
            P2Intermission(id);
            P2WhirlwindDebuffs(id + 0x10000, 5.2f);
            P2ChakramOpticalSightPhoton(id + 0x20000, 6);
            P2SpinCrusher(id + 0x30000, 7.2f);
            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void P2Intermission(uint id)
        {
            Timeout(id, 0)
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<P2JKick>(id + 0x100, 31.2f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P2Intermission>()
                .ActivateOnEnter<P2JKick>()
                .DeactivateOnExit<P2Intermission>()
                .DeactivateOnExit<P2JKick>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorTargetable(id + 0x101, _module.BruteJustice, true, 3, "Intermission end")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void P2WhirlwindDebuffs(uint id, float delay)
        {
            ActorCastStart(id, _module.CruiseChaser, AID.Whirlwind, delay);
            ActorCastStart(id + 1, _module.BruteJustice, AID.JudgmentNisi, 3);
            ActorCastEnd(id + 2, _module.CruiseChaser, 1, false, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorCastEnd(id + 3, _module.BruteJustice, 3, false, "Nisi");
            ActorCast(id + 0x10, _module.BruteJustice, AID.LinkUp, 3.2f, 3, false, "Debuffs");
        }

        private void P2ChakramOpticalSightPhoton(uint id, float delay)
        {
            ActorCastStart(id, _module.CruiseChaser, AID.OpticalSight, delay)
                .ActivateOnEnter<P2EyeOfTheChakram>();
            ActorCastEnd(id + 1, _module.CruiseChaser, 2);
            ComponentCondition<P2EyeOfTheChakram>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Chakrams")
                .ActivateOnEnter<P2HawkBlasterOpticalSight>()
                .DeactivateOnExit<P2EyeOfTheChakram>();
            ActorCastStart(id + 0x10, _module.CruiseChaser, AID.Photon, 3.2f);
            ComponentCondition<P2HawkBlasterOpticalSight>(id + 0x11, 1.1f, comp => comp.NumCasts > 0, "Puddles")
                .DeactivateOnExit<P2HawkBlasterOpticalSight>();
            ActorCastEnd(id + 0x12, _module.CruiseChaser, 1.9f, false, "Photon")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P2SpinCrusher(uint id, float delay)
        {
            ActorCast(id, _module.CruiseChaser, AID.SpinCrusher, delay, 3, false, "Baited cleave")
                .ActivateOnEnter<P2SpinCrusher>()
                .DeactivateOnExit<P2SpinCrusher>();
        }
    }
}
