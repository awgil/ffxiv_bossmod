namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class P7SStates : StateMachineBuilder
    {
        public P7SStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase)
                .ActivateOnEnter<Border>();
        }

        private void SinglePhase(uint id)
        {
            SparkOfLife(id, 7.3f);
            DispersedCondensedAero(id + 0x10000, 6.3f);
            BladesOfAttisImmortalsObol(id + 0x20000, 3.7f);
            ForbiddenFruit1(id + 0x30000, 10.6f);
            DispersedCondensedAero(id + 0x40000, 3.9f);
            SparkOfLife(id + 0x50000, 4.7f);
            InviolateBonds(id + 0x60000, 5.2f);
            RootsOfAttis(id + 0x70000, 2.7f, "N bridge disappear");
            DispersedCondensedAero(id + 0x80000, 11.5f);
            ForbiddenFruit2(id + 0x90000, 3.6f);
            RootsOfAttis(id + 0xA0000, 4.3f, "All bridges disappear");
            ForbiddenFruit3(id + 0xB0000, 7.5f);
            BoughOfAttisFrontSide(id + 0xC0000, 10.3f);
            DispersedCondensedAero(id + 0xD0000, 2.2f);
            ForbiddenFruit4(id + 0xE0000, 2.6f);

            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void SparkOfLife(uint id, float delay)
        {
            Cast(id, AID.SparkOfLife, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void DispersedCondensedAero(uint id, float delay)
        {
            CastMulti(id, new AID[] { AID.DispersedAero, AID.CondensedAero }, delay, 7)
                .ActivateOnEnter<DispersedCondensedAero>();
            ComponentCondition<DispersedCondensedAero>(id + 2, 1.1f, comp => comp.Done, "Tankbuster")
                .DeactivateOnExit<DispersedCondensedAero>();
        }

        // leaves component active
        private void BladesOfAttisImmortalsObol(uint id, float delay)
        {
            Cast(id, AID.BladesOfAttis, delay, 3)
                .ActivateOnEnter<BladesOfAttis>();
            Cast(id + 0x10, AID.ImmortalsObol, 3.2f, 5.5f);
        }

        // expects blades of attis component to be active from previous state
        private void ForbiddenFruit1(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4)
                .DeactivateOnExit<BladesOfAttis>();
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2, 3);
            // TODO: somewhere at this point we should be able to detect add positions, investigate network packets... - at very least we can try to detect changed positions...
            Cast(id + 0x20, AID.HemitheosHoly, 3.3f, 3);
            CastStart(id + 0x30, AID.BoughOfAttisBack, 3.1f);
            ComponentCondition<HemitheosHoly>(id + 0x31, 6, comp => !comp.Active, "Stacks")
                .ActivateOnEnter<HemitheosHoly>()
                .ActivateOnEnter<BoughOfAttisBack>()
                .ActivateOnEnter<StaticMoon>()
                .ActivateOnEnter<StymphalianStrike>()
                .DeactivateOnExit<HemitheosHoly>();
            CastEnd(id + 0x32, 0.2f);
            ComponentCondition<BoughOfAttisBack>(id + 0x33, 0.8f, comp => comp.NumCasts > 0, "Fruit 1 resolve")
                .DeactivateOnExit<BoughOfAttisBack>()
                .DeactivateOnExit<StaticMoon>()
                .DeactivateOnExit<StymphalianStrike>();
        }

        private void InviolateBonds(uint id, float delay)
        {
            Cast(id, AID.InviolateBonds, delay, 4);
            ComponentCondition<WindsHoly>(id + 2, 1, comp => comp.Active)
                .ActivateOnEnter<WindsHoly>();
            Cast(id + 0x10, AID.BoughOfAttisFront, 3.1f, 5.8f)
                .ActivateOnEnter<BoughOfAttisFront>();
            ComponentCondition<WindsHoly>(id + 0x12, 0.1f, comp => comp.NumCasts >= 1, "Stack/spread back");
            ComponentCondition<BoughOfAttisFront>(id + 0x13, 1.1f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<BoughOfAttisFront>();
            Cast(id + 0x20, AID.BoughOfAttisSide, 2.4f, 4)
                .ActivateOnEnter<BoughOfAttisSide>();
            ComponentCondition<BoughOfAttisSide>(id + 0x22, 1, comp => comp.NumCasts > 0)
                .DeactivateOnExit<BoughOfAttisSide>();
            ComponentCondition<WindsHoly>(id + 0x23, 2.5f, comp => comp.NumCasts >= 2, "Stack/spread front")
                .DeactivateOnExit<WindsHoly>();
        }

        private void RootsOfAttis(uint id, float delay, string name)
        {
            Cast(id, AID.RootsOfAttis, delay, 3, name);
        }

        private void ForbiddenFruit2(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2, 3);
            Cast(id + 0x20, AID.Multicast, 2.1f, 3);
            ComponentCondition<HemitheosHolySpread>(id + 0x30, 4.8f, comp => comp.Active)
                .ActivateOnEnter<HemitheosAeroKnockback>()
                .ActivateOnEnter<HemitheosHolySpread>();
            ComponentCondition<HemitheosAeroKnockback>(id + 0x31, 1.2f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<HemitheosAeroKnockback>();
            ComponentCondition<HemitheosHolySpread>(id + 0x32, 3.8f, comp => !comp.Active, "Spread")
                .ActivateOnEnter<StymphalianStrike>()
                .DeactivateOnExit<HemitheosHolySpread>();
            ComponentCondition<StymphalianStrike>(id + 0x33, 1, comp => comp.NumCasts > 0, "Fruit 2 resolve")
                .DeactivateOnExit<StymphalianStrike>();
        }

        private void ForbiddenFruit3(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2, 3);
            Cast(id + 0x20, AID.HemitheosHoly, 3.8f, 3);
            ComponentCondition<HemitheosHoly>(id + 0x22, 0.1f, comp => comp.Active)
                .ActivateOnEnter<HemitheosHoly>();
            ComponentCondition<HemitheosHoly>(id + 0x30, 6, comp => !comp.Active, "Stacks")
                .ActivateOnEnter<StaticMoon>()
                .DeactivateOnExit<HemitheosHoly>();
            ComponentCondition<StaticMoon>(id + 0x31, 1.1f, comp => comp.NumCasts > 0, "Fruit 3 resolve")
                .DeactivateOnExit<StaticMoon>();
        }

        private void BoughOfAttisFrontSide(uint id, float delay)
        {
            Cast(id, AID.BoughOfAttisFront, delay, 5.8f)
                .ActivateOnEnter<BoughOfAttisFront>();
            ComponentCondition<BoughOfAttisFront>(id + 0x2, 1.2f, comp => comp.NumCasts > 0, "Front hit")
                .DeactivateOnExit<BoughOfAttisFront>();
            Cast(id + 0x10, AID.BoughOfAttisSide, 2.4f, 4)
                .ActivateOnEnter<BoughOfAttisSide>();
            ComponentCondition<BoughOfAttisSide>(id + 0x12, 1, comp => comp.NumCasts > 0, "Side hit")
                .DeactivateOnExit<BoughOfAttisSide>();
        }

        private void ForbiddenFruit4(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2, 3);
        }
    }
}
