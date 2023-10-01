namespace BossMod.Endwalker.Alliance.A11Byregot
{
    class A11ByregotStates : StateMachineBuilder
    {
        public A11ByregotStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            OrdealOfThunder(id, 6.2f);
            ByregotStrikeSimple(id + 0x10000, 5.8f);
            ByregotWard(id + 0x20000, 8.2f);
            ByregotStrikeCones(id + 0x30000, 5.1f);

            Hammers1(id + 0x100000, 6.2f);
            ByregotStrikeAny(id + 0x110000, 14.8f);
            OrdealOfThunder(id + 0x120000, 2.1f);
            Reproduce(id + 0x130000, 6.1f);
            ByregotWard(id + 0x140000, 2.6f);

            Hammers2(id + 0x200000, 2.1f);
            ByregotStrikeAny(id + 0x210000, 16.1f);
            OrdealOfThunder(id + 0x220000, 2.1f);
            ByregotWard(id + 0x230000, 7.2f);
            Reproduce(id + 0x240000, 5.1f);

            SimpleState(id + 0xFF0000, 10, "???");
        }

        private void OrdealOfThunder(uint id, float delay)
        {
            Cast(id, AID.OrdealOfThunder, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void ByregotWard(uint id, float delay)
        {
            Cast(id, AID.ByregotWard, delay, 5)
                .ActivateOnEnter<ByregotWard>();
            ComponentCondition<ByregotWard>(id + 2, 0.1f, comp => comp.NumCasts > 0, "Tankbuster")
                .DeactivateOnExit<ByregotWard>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void ByregotStrikeSimple(uint id, float delay)
        {
            Cast(id, AID.ByregotStrikeJump, delay, 6)
                .ActivateOnEnter<ByregotStrikeJump>()
                .ActivateOnEnter<ByregotStrikeKnockback>()
                .DeactivateOnExit<ByregotStrikeJump>();
            ComponentCondition<ByregotStrikeKnockback>(id + 2, 1, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<ByregotStrikeKnockback>();
        }

        private void ByregotStrikeCones(uint id, float delay)
        {
            Cast(id, AID.BuilderBuild, delay, 3);
            Cast(id + 0x10, AID.ByregotStrikeJumpCone, 2.6f, 6)
                .ActivateOnEnter<ByregotStrikeJumpCone>()
                .ActivateOnEnter<ByregotStrikeKnockback>()
                .ActivateOnEnter<ByregotStrikeCone>()
                .DeactivateOnExit<ByregotStrikeJumpCone>();
            ComponentCondition<ByregotStrikeCone>(id + 0x20, 0.9f, comp => comp.NumCasts > 0, "Cones")
                .DeactivateOnExit<ByregotStrikeCone>();
            ComponentCondition<ByregotStrikeKnockback>(id + 0x21, 0.1f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<ByregotStrikeKnockback>();
        }

        private void ByregotStrikeAny(uint id, float delay)
        {
            // with 'cone' variant, we'll first get "builder's build" visual, and that will delay actual cast by ~5.6s - ignore that for simplicity
            SimpleState(id, delay, "")
                .SetHint(StateMachine.StateHint.BossCastStart)
                .Raw.Update = _ => Module.PrimaryActor.CastInfo != null && (AID)Module.PrimaryActor.CastInfo.Action.ID is AID.ByregotStrikeJump or AID.ByregotStrikeJumpCone ? 0 : -1;
            CastEnd(id + 1, 6)
                .ActivateOnEnter<ByregotStrikeJump>()
                .ActivateOnEnter<ByregotStrikeJumpCone>()
                .ActivateOnEnter<ByregotStrikeKnockback>()
                .ActivateOnEnter<ByregotStrikeCone>()
                .DeactivateOnExit<ByregotStrikeJump>()
                .DeactivateOnExit<ByregotStrikeJumpCone>();
            ComponentCondition<ByregotStrikeKnockback>(id + 2, 1, comp => comp.NumCasts > 0, "Knockback + maybe cones")
                .DeactivateOnExit<ByregotStrikeKnockback>()
                .DeactivateOnExit<ByregotStrikeCone>();
        }

        private void Reproduce(uint id, float delay)
        {
            Cast(id, AID.Reproduce, delay, 3);
            ComponentCondition<Reproduce>(id + 0x10, 5.0f, comp => comp.Active)
                .ActivateOnEnter<Reproduce>();
            ComponentCondition<Reproduce>(id + 0x20, 7.0f, comp => comp.NumCasts > 0, "Exaflares start");
            ComponentCondition<Reproduce>(id + 0x30, 6.6f, comp => !comp.Active, "Exaflares resolve")
                .DeactivateOnExit<Reproduce>();
        }

        private void HammersStart(uint id, float delay)
        {
            ComponentCondition<HammersCells>(id, delay, comp => comp.Active)
                .ActivateOnEnter<HammersCells>();
            ComponentCondition<HammersCells>(id + 1, 9.0f, comp => comp.NumCasts > 0, "Destroy side tiles");
        }

        private void HammersMove(uint id, float delay)
        {
            ComponentCondition<HammersCells>(id, delay, comp => comp.MovementPending);
            ComponentCondition<HammersCells>(id + 0x10, 16.0f, comp => !comp.MovementPending, "Move tiles");
        }

        private void HammersLevinforge(uint id, float delay)
        {
            ComponentCondition<HammersLevinforge>(id, delay, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<HammersLevinforge>();
            ComponentCondition<HammersCells>(id + 0x10, 2.2f, comp => comp.MovementPending);
            ComponentCondition<HammersCells>(id + 0x20, 16.0f, comp => !comp.MovementPending, "Move tiles");
            ComponentCondition<HammersLevinforge>(id + 0x21, 0.3f, comp => comp.NumCasts > 0, "Narrow line")
                .DeactivateOnExit<HammersLevinforge>();
        }

        private void HammersSpire(uint id, float delay)
        {
            ComponentCondition<HammersCells>(id, delay, comp => comp.MovementPending);
            ComponentCondition<HammersSpire>(id + 0x10, 7.1f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<HammersSpire>();
            ComponentCondition<HammersCells>(id + 0x20, 8.9f, comp => !comp.MovementPending, "Move tiles");
            ComponentCondition<HammersSpire>(id + 0x21, 2.1f, comp => comp.NumCasts > 0, "Wide line")
                .DeactivateOnExit<HammersSpire>();
        }

        private void HammersResolve(uint id, float delay)
        {
            ComponentCondition<HammersCells>(id, delay, comp => !comp.Active, "Hammers resolve")
                .DeactivateOnExit<HammersCells>();
        }

        private void Hammers1(uint id, float delay)
        {
            HammersStart(id, delay);
            HammersMove(id + 0x100, 14.6f); // large variance
            HammersLevinforge(id + 0x200, 8.8f);
            HammersSpire(id + 0x300, 2.4f);
            HammersResolve(id + 0x400, 10.8f);
        }

        private void Hammers2(uint id, float delay)
        {
            HammersStart(id, delay);
            HammersLevinforge(id + 0x100, 14.6f); // large variance
            HammersLevinforge(id + 0x200, 8.6f);
            HammersSpire(id + 0x300, 2.4f);
            HammersResolve(id + 0x400, 10.8f);
        }
    }
}
