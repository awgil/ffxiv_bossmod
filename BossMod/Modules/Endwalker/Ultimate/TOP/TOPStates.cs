namespace BossMod.Endwalker.Ultimate.TOP
{
    class TOPStates : StateMachineBuilder
    {
        public TOPStates(BossModule module) : base(module)
        {
            DeathPhase(0, Phase1);
        }

        private void Phase1(uint id)
        {
            P1ProgramLoop(id, 10.1f);
            P1Pantokrator(id + 0x10000, 8.2f);
            SimpleState(id + 0xFF0000, 10, "???");
        }

        private void P1ProgramLoop(uint id, float delay)
        {
            Cast(id, AID.ProgramLoop, delay, 4)
                .ActivateOnEnter<P1ProgramLoop>();
            Cast(id + 0x10, AID.Blaster, 6.1f, 7.9f);
            // note: tethers explode ~0.1s after each tower set
            ComponentCondition<P1ProgramLoop>(id + 0x20, 0.1f, comp => comp.NumTowersDone >= 2, "Towers 1/tethers 3");
            ComponentCondition<P1ProgramLoop>(id + 0x30, 9.0f, comp => comp.NumTowersDone >= 4, "Towers 2/tethers 4");
            ComponentCondition<P1ProgramLoop>(id + 0x40, 9.0f, comp => comp.NumTowersDone >= 6, "Towers 3/tethers 1");
            ComponentCondition<P1ProgramLoop>(id + 0x50, 9.0f, comp => comp.NumTowersDone >= 8, "Towers 4/tethers 2")
                .DeactivateOnExit<P1ProgramLoop>();
        }

        private void P1Pantokrator(uint id, float delay)
        {
            Cast(id, AID.Pantokrator, delay, 5);
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

            ComponentCondition<P1DiffuseWaveCannonKyrios>(id + 0x100, 6.7f, comp => comp.NumCasts > 0, "Tankbusters + spreads")
                .ActivateOnEnter<P1DiffuseWaveCannonKyrios>()
                .ActivateOnEnter<P1WaveCannonKyrios>()
                .DeactivateOnExit<P1DiffuseWaveCannonKyrios>()
                .DeactivateOnExit<P1WaveCannonKyrios>();
        }
    }
}
