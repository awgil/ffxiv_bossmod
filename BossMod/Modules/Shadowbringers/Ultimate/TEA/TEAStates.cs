namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class TEAStates : StateMachineBuilder
    {
        private TEA _module;

        public TEAStates(TEA module) : base(module)
        {
            _module = module;
            DeathPhase(0, Phase1LivingLiquid)
                .ActivateOnEnter<P1HPDiffCheck>();
        }

        private void Phase1LivingLiquid(uint id)
        {
            P1FluidSwing(id, 10.2f);
            P1Cascade(id + 0x10000, 4);

            SimpleState(id + 0xF0000, 1000, "???");
        }

        private void P1FluidSwing(uint id, float delay)
        {
            ComponentCondition<P1FluidSwing>(id, delay, comp => comp.NumCasts > 0, "Cleave")
                .ActivateOnEnter<P1FluidSwing>()
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P1Cascade(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.Cascade, delay, 4, true, "Raidwide")
                .ActivateOnEnter<P1Cascade>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }
    }
}
