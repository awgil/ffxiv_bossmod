using static BossMod.Shadowbringers.Ultimate.TEA.TEAConfig;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class TOPStates : StateMachineBuilder
    {
        private TOP _module;

        private bool IsEffectivelyDead(Actor? actor) => actor != null && !actor.IsTargetable && actor.HP.Cur <= 1;

        public TOPStates(TOP module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1, "P1: Beetle")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
            SimplePhase(1, Phase2, "P2: M/F")
                .Raw.Update = () => (_module.OpticalUnit()?.IsDestroyed ?? true) || IsEffectivelyDead(_module.BossP2M()) && IsEffectivelyDead(_module.BossP2F());
            SimplePhase(2, Phase3, "P3")
                .Raw.Update = () => (_module.OpticalUnit()?.IsDestroyed ?? true); // TODO: reconsider condition...
        }

        private void Phase1(uint id)
        {
            P1ProgramLoop(id, 10.1f);
            P1Pantokrator(id + 0x10000, 8.2f);
            P1WaveCannons(id + 0x20000, 6.6f);
            ActorCast(id + 0x30000, _module.BossP1, AID.AtomicRay, 5.8f, 5, true, "Enrage");
        }

        private void Phase2(uint id)
        {
            P2FirewallSolarRay(id, 3.4f);
            P2PartySynergy(id + 0x10000, 13.4f);
            P2LimitlessSynergy(id + 0x20000, 9.2f);
        }

        private void Phase3(uint id)
        {
            P3Intermission(id, 9.4f);
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void P1ProgramLoop(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.ProgramLoop, delay, 4, true)
                .ActivateOnEnter<P1ProgramLoop>();
            ActorCast(id + 0x10, _module.BossP1, AID.Blaster, 6.1f, 7.9f, true);
            // note: tethers explode ~0.1s after each tower set
            ComponentCondition<P1ProgramLoop>(id + 0x20, 0.1f, comp => comp.NumTowersDone >= 2, "Towers 1/tethers 3");
            ComponentCondition<P1ProgramLoop>(id + 0x30, 9.0f, comp => comp.NumTowersDone >= 4, "Towers 2/tethers 4");
            ComponentCondition<P1ProgramLoop>(id + 0x40, 9.0f, comp => comp.NumTowersDone >= 6, "Towers 3/tethers 1");
            ComponentCondition<P1ProgramLoop>(id + 0x50, 9.0f, comp => comp.NumTowersDone >= 8, "Towers 4/tethers 2")
                .DeactivateOnExit<P1ProgramLoop>();
        }

        private void P1Pantokrator(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.Pantokrator, delay, 5, true);
            ComponentCondition<P1Pantokrator>(id + 0x10, 12.1f, comp => comp.NumSpreadsDone >= 2, "Spread 1/stack 3")
                .ActivateOnEnter<P1Pantokrator>()
                .ActivateOnEnter<P1BallisticImpact>()
                .ActivateOnEnter<P1FlameThrowerFirst>()
                .ActivateOnEnter<P1FlameThrowerRest>()
                .DeactivateOnExit<P1FlameThrowerFirst>();
            ComponentCondition<P1Pantokrator>(id + 0x20, 6.0f, comp => comp.NumSpreadsDone >= 4, "Spread 2/stack 4");
            ComponentCondition<P1Pantokrator>(id + 0x30, 6.0f, comp => comp.NumSpreadsDone >= 6, "Spread 3/stack 1");
            ComponentCondition<P1Pantokrator>(id + 0x40, 6.0f, comp => comp.NumSpreadsDone >= 8, "Spread 4/stack 2")
                .DeactivateOnExit<P1Pantokrator>();
            ComponentCondition<P1FlameThrowerRest>(id + 0x50, 2.1f, comp => comp.Casters.Count == 0, "Last flamethrower")
                .DeactivateOnExit<P1BallisticImpact>()
                .DeactivateOnExit<P1FlameThrowerRest>();
        }

        private void P1WaveCannons(uint id, float delay)
        {
            ComponentCondition<P1DiffuseWaveCannonKyrios>(id, delay, comp => comp.NumCasts > 0, "Tankbusters (1) + spreads (1)")
                .ActivateOnEnter<P1DiffuseWaveCannonKyrios>()
                .ActivateOnEnter<P1WaveCannonKyrios>();
            // +2.1s: 2nd tankbuster (diffuse) hit
            // +3.9s: 2nd set of icons
            // +4.1s: 3rd tankbuster hit
            // +6.2s: 4th tankbuster hit
            ComponentCondition<P1DiffuseWaveCannonKyrios>(id + 0x10, 8.3f, comp => comp.NumCasts >= 10, "Tankbusters resolve (5)")
                .DeactivateOnExit<P1DiffuseWaveCannonKyrios>();
            ComponentCondition<P1WaveCannonKyrios>(id + 0x20, 0.7f, comp => comp.CurrentBaits.Count == 0, "Spreads (2)")
                .DeactivateOnExit<P1WaveCannonKyrios>();
        }

        private void P2FirewallSolarRay(uint id, float delay)
        {
            ActorTargetable(id, _module.BossP2M, true, delay, "M/F appear");
            ActorCast(id + 0x10, _module.BossP2M, AID.FirewallM, 1.2f, 3);
            ActorCast(id + 0x20, _module.BossP2M, AID.SolarRayM, 3.2f, 5, false, "Tankbusters")
                .ActivateOnEnter<SolarRayM>()
                .ActivateOnEnter<SolarRayF>()
                .DeactivateOnExit<SolarRayM>()
                .DeactivateOnExit<SolarRayF>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P2PartySynergy(uint id, float delay)
        {
            ActorCast(id, _module.BossP2M, AID.PartySynergyM, delay, 3);
            ActorTargetable(id + 0x10, _module.BossP2M, false, 3.1f, "M/F disappear")
                .ActivateOnEnter<P2PartySynergy>()
                .SetHint(StateMachine.StateHint.DowntimeStart);

            ComponentCondition<P2PartySynergyDoubleAOEs>(id + 0x20, 2.2f, comp => comp.AOEs.Count > 0)
                .ActivateOnEnter<P2PartySynergyDoubleAOEs>()
                .ActivateOnEnter<P2PartySynergyOpticalLaser>();
            ComponentCondition<P2PartySynergyDoubleAOEs>(id + 0x21, 5.1f, comp => comp.NumCasts > 0, "Double AOEs")
                .DeactivateOnExit<P2PartySynergyDoubleAOEs>();

            ComponentCondition<P2PartySynergyOptimizedFire>(id + 0x30, 6.4f, comp => !comp.Active, "Spreads")
                .ActivateOnEnter<P2PartySynergyOptimizedFire>()
                .ExecOnEnter<P2PartySynergyOpticalLaser>(comp => comp.Show(_module))
                .ActivateOnEnter<P2PartySynergyEfficientBladework>() // PATEs happen 0.8s after double aoes
                .DeactivateOnExit<P2PartySynergyOptimizedFire>();
            ComponentCondition<P2PartySynergyOpticalLaser>(id + 0x31, 0.4f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P2PartySynergyOpticalLaser>();

            ComponentCondition<P2PartySynergyDischarger>(id + 0x40, 6.9f, comp => comp.NumCasts > 0, "Knockback")
                .ActivateOnEnter<P2PartySynergyDischarger>()
                .ActivateOnEnter<P2PartySynergySpotlight>() // TODO: reconsider (icons appear ~0.6s after laser end, but do we even care about this?)
                .DeactivateOnExit<P2PartySynergyDischarger>();
            ComponentCondition<P2PartySynergyEfficientBladework>(id + 0x41, 4.4f, comp => comp.NumCasts > 0, "Stacks")
                .DeactivateOnExit<P2PartySynergySpotlight>() // TODO: reconsider (happens right before aoes, but do we even care?)
                .DeactivateOnExit<P2PartySynergyEfficientBladework>()
                .DeactivateOnExit<P2PartySynergy>();

            ActorTargetable(id + 0x50, _module.BossP2M, true, 3.0f, "M/F reappear")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void P2LimitlessSynergy(uint id, float delay)
        {
            ActorCast(id, _module.BossP2F, AID.SyntheticShield, delay, 1, true);
            ActorCast(id + 0x10, _module.BossP2F, AID.LimitlessSynergyM, 5.3f, 5, true, "Remove debuffs");
            ActorCastStart(id + 0x20, _module.BossP2M, AID.LaserShower, 5.0f, false, "F invincible");
            ComponentCondition<P2OptimizedBladedance>(id + 0x30, 8.5f, comp => comp.NumCasts > 0, "Baited rect + Tankbusters")
                .ActivateOnEnter<P2OptimizedBladedance>()
                .ActivateOnEnter<P2OptimizedSagittariusArrow>()
                .ActivateOnEnter<P2BeyondDefense>()
                .DeactivateOnExit<P2OptimizedBladedance>()
                .DeactivateOnExit<P2OptimizedSagittariusArrow>() // resolves right before tethers
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P2BeyondDefense>(id + 0x40, 6.0f, comp => comp.CurMechanic == P2BeyondDefense.Mechanic.Spread);
            ComponentCondition<P2BeyondDefense>(id + 0x50, 5.1f, comp => comp.CurMechanic == P2BeyondDefense.Mechanic.Stack, "Jump bait");
            // +2.8s: flares resolve, we typically don't care?..
            ComponentCondition<P2BeyondDefense>(id + 0x60, 3.2f, comp => comp.CurMechanic == P2BeyondDefense.Mechanic.None, "Flares + Stack")
                .DeactivateOnExit<P2BeyondDefense>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P2CosmoMemory>(id + 0x70, 10.2f, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<P2CosmoMemory>()
                .DeactivateOnExit<P2CosmoMemory>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorCastEnd(id + 0x80, _module.BossP2M, 27, false, "Enrage");
        }

        private void P3Intermission(uint id, float delay)
        {
            ComponentCondition<P3SniperCannon>(id, delay, comp => comp.Active)
                .ActivateOnEnter<P3SniperCannon>();
            ComponentCondition<P3WaveRepeater>(id + 1, 0.1f, comp => comp.Sequences.Count > 0)
                .ActivateOnEnter<P3WaveRepeater>();
            ComponentCondition<P3ColossalBlow>(id + 0x10, 3.1f, comp => comp.AOEs.Count > 0)
                .ActivateOnEnter<P3ColossalBlow>();
            ComponentCondition<P3WaveRepeater>(id + 0x11, 1.9f, comp => comp.NumCasts > 0, "Ring 1");
            ComponentCondition<P3ColossalBlow>(id + 0x12, 1.1f, comp => comp.AOEs.Count > 3);
            ComponentCondition<P3WaveRepeater>(id + 0x13, 1.0f, comp => comp.NumCasts > 1, "Ring 2");
            ComponentCondition<P3WaveRepeater>(id + 0x20, 1.1f, comp => comp.Sequences.Count > 1);
            ComponentCondition<P3WaveRepeater>(id + 0x21, 1.0f, comp => comp.NumCasts > 2, "Ring 3");
            ComponentCondition<P3WaveRepeater>(id + 0x22, 2.1f, comp => comp.NumCasts > 3, "Ring 4");
            ComponentCondition<P3WaveRepeater>(id + 0x30, 1.9f, comp => comp.NumCasts > 4, "Ring 5");
            ComponentCondition<P3WaveRepeater>(id + 0x31, 2.1f, comp => comp.NumCasts > 5, "Ring 6");
            ComponentCondition<P3ColossalBlow>(id + 0x40, 1.8f, comp => comp.NumCasts > 0, "Arms 1");
            ComponentCondition<P3WaveRepeater>(id + 0x41, 0.3f, comp => comp.NumCasts > 6, "Ring 7");
            ComponentCondition<P3SniperCannon>(id + 0x50, 1.8f, comp => !comp.Active, "Stack/spread")
                .DeactivateOnExit<P3SniperCannon>();
            ComponentCondition<P3WaveRepeater>(id + 0x51, 0.3f, comp => comp.NumCasts > 7)
                .DeactivateOnExit<P3WaveRepeater>();
            ComponentCondition<P3ColossalBlow>(id + 0x52, 0.2f, comp => comp.NumCasts > 3, "Arms 2")
                .DeactivateOnExit<P3ColossalBlow>();

            ActorTargetable(id + 0x100, _module.BossP3, true, 3.5f, "Boss reappears");
        }
    }
}
