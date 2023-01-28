namespace BossMod.Endwalker.Ultimate.TOP
{
    class TOPStates : StateMachineBuilder
    {
        private TOP _module;

        public TOPStates(TOP module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1, "P1")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
            SimplePhase(1, Phase2, "P2")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.OpticalUnit()?.IsDestroyed ?? true); // TODO: reconsider condition...
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
                .ActivateOnEnter<P2PartySynergyDoubleAOEs>();
            ComponentCondition<P2PartySynergyDoubleAOEs>(id + 0x21, 5.1f, comp => comp.NumCasts > 0, "Double AOEs")
                .DeactivateOnExit<P2PartySynergyDoubleAOEs>();

            ComponentCondition<P2PartySynergyOptimizedFire>(id + 0x30, 6.4f, comp => !comp.Active, "Spreads")
                .ActivateOnEnter<P2PartySynergyOptimizedFire>()
                .ActivateOnEnter<P2PartySynergyOpticalLaser>()
                .DeactivateOnExit<P2PartySynergyOptimizedFire>();
            ComponentCondition<P2PartySynergyOpticalLaser>(id + 0x31, 0.4f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P2PartySynergyOpticalLaser>();

            // TODO: kb + mid/remote stacks, reappear
        }
    }
}
