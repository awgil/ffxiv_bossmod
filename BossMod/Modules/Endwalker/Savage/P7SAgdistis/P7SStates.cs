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
            BladesOfAttisImmortalsObol(id + 0x20000, 3.8f);
            ForbiddenFruit1(id + 0x30000, 12.5f);
            DispersedCondensedAero(id + 0x40000, 3.9f);
            SparkOfLife(id + 0x50000, 4.7f);
            InviolateBonds(id + 0x60000, 5.2f);
            RootsOfAttis(id + 0x70000, 2.7f, "N bridge disappear");
            DispersedCondensedAero(id + 0x80000, 11.5f);
            ForbiddenFruit2(id + 0x90000, 3.6f);
            RootsOfAttis(id + 0xA0000, 4.3f, "All bridges disappear");
            ForbiddenFruit3(id + 0xB0000, 7.4f);
            BoughOfAttisFrontSide(id + 0xC0000, 10.4f);
            DispersedCondensedAero(id + 0xD0000, 2.2f);
            ForbiddenFruit4(id + 0xE0000, 2.6f);
            BladesOfAttisMulticast(id + 0xF0000, 4.8f);
            DispersedCondensedAero(id + 0x100000, 7.4f, true);
            ForbiddenFruit5(id + 0x110000, 4.6f);
            SparkOfLife(id + 0x120000, 3.5f);
            ImmortalsObol(id + 0x130000, 7.4f);
            ForbiddenFruit6(id + 0x140000, 5.2f);
            ForbiddenFruit7(id + 0x150000, 4.8f);
            FaminesHarvest(id + 0x160000, 2.9f);
            DeathsHarvest(id + 0x170000, 7.8f);
            WarsHarvest(id + 0x180000, 7.8f);
            SparkOfLife(id + 0x190000, 8);
            BoughOfAttisFrontSideHemitheosHoly(id + 0x1A0000, 7.3f);
            SparkOfLife(id + 0x1B0000, 3.2f);
            SparkOfLife(id + 0x1C0000, 8.3f);
            Cast(id + 0x1D0000, AID.Enrage, 7.3f, 10, "Enrage");
        }

        private void SparkOfLife(uint id, float delay)
        {
            Cast(id, AID.SparkOfLife, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void DispersedCondensedAero(uint id, float delay, bool duringBladesOfAttis = false)
        {
            CastMulti(id, new AID[] { AID.DispersedAero, AID.CondensedAero }, delay, 7)
                .ActivateOnEnter<DispersedCondensedAero>()
                .DeactivateOnExit<BladesOfAttis>(duringBladesOfAttis)
                .SetHint(StateMachine.StateHint.PositioningEnd, duringBladesOfAttis);
            ComponentCondition<DispersedCondensedAero>(id + 2, 1.1f, comp => comp.Done, "Tankbuster")
                .DeactivateOnExit<DispersedCondensedAero>();
        }

        private void ImmortalsObol(uint id, float delay)
        {
            Cast(id, AID.ImmortalsObol, delay, 5.5f, "Destroy platform");
        }

        // leaves component active
        private void BladesOfAttisImmortalsObol(uint id, float delay)
        {
            Cast(id, AID.BladesOfAttis, delay, 3)
                .ActivateOnEnter<BladesOfAttis>();
            ImmortalsObol(id + 0x10, 3.2f);
        }

        // leaves component & pos flag active
        private void BladesOfAttisMulticast(uint id, float delay)
        {
            Cast(id, AID.BladesOfAttis, delay, 3)
                .ActivateOnEnter<BladesOfAttis>();
            Cast(id + 0x10, AID.Multicast, 3.2f, 3);
            ComponentCondition<HemitheosHoly>(id + 0x20, 2.7f, comp => comp.Active)
                .ActivateOnEnter<HemitheosAeroKnockback2>()
                .ActivateOnEnter<HemitheosHoly>();
            ComponentCondition<HemitheosAeroKnockback2>(id + 0x21, 3.3f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<HemitheosAeroKnockback2>()
                .SetHint(StateMachine.StateHint.PositioningStart);
            ComponentCondition<HemitheosHoly>(id + 0x22, 2.6f, comp => !comp.Active, "Stacks")
                .DeactivateOnExit<HemitheosHoly>();
        }

        // expects blades of attis component to be active from previous state
        private void ForbiddenFruit1(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4)
                .DeactivateOnExit<BladesOfAttis>();
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            // TODO: somewhere at this point we should be able to detect add positions, investigate network packets... - at very least we can try to detect changed positions...
            Cast(id + 0x20, AID.HemitheosHoly, 3.3f, 3);
            CastStart(id + 0x30, AID.BoughOfAttisBack, 3.2f);
            ComponentCondition<HemitheosHoly>(id + 0x31, 6, comp => !comp.Active, "Stacks")
                .ActivateOnEnter<HemitheosHoly>()
                .ActivateOnEnter<BoughOfAttisBack>()
                .ActivateOnEnter<StaticMoon>()
                .ActivateOnEnter<StymphalianStrike>()
                .DeactivateOnExit<HemitheosHoly>();
            CastEnd(id + 0x32, 0.2f);
            ComponentCondition<BoughOfAttisBack>(id + 0x33, 0.8f, comp => comp.NumCasts > 0, "Fruit 1 (2 birds + bull + stacks) resolve")
                .DeactivateOnExit<BoughOfAttisBack>()
                .DeactivateOnExit<StaticMoon>()
                .DeactivateOnExit<StymphalianStrike>();
        }

        private void InviolateBonds(uint id, float delay)
        {
            Cast(id, AID.InviolateBonds, delay, 4);
            ComponentCondition<WindsHoly>(id + 2, 1, comp => comp.Active)
                .ActivateOnEnter<WindsHoly>();
            CastStart(id + 0x10, AID.BoughOfAttisFront, 3.2f)
                .SetHint(StateMachine.StateHint.PositioningStart);
            CastEnd(id + 0x11, 5.8f)
                .ActivateOnEnter<BoughOfAttisFront>();
            ComponentCondition<WindsHoly>(id + 0x12, 0.1f, comp => comp.NumCasts >= 1, "Stack/spread back");
            ComponentCondition<BoughOfAttisFront>(id + 0x13, 1.1f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<BoughOfAttisFront>();
            CastMulti(id + 0x20, new AID[] { AID.BoughOfAttisSideW, AID.BoughOfAttisSideE }, 2.4f, 4)
                .ActivateOnEnter<BoughOfAttisSide>();
            ComponentCondition<BoughOfAttisSide>(id + 0x22, 1, comp => comp.NumCasts > 0)
                .DeactivateOnExit<BoughOfAttisSide>();
            ComponentCondition<WindsHoly>(id + 0x23, 2.5f, comp => comp.NumCasts >= 2, "Stack/spread front")
                .DeactivateOnExit<WindsHoly>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void RootsOfAttis(uint id, float delay, string name)
        {
            Cast(id, AID.RootsOfAttis, delay, 3, name);
        }

        private void ForbiddenFruit2(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            Cast(id + 0x20, AID.Multicast, 2.1f, 3);
            ComponentCondition<HemitheosHolySpread>(id + 0x30, 4.8f, comp => comp.Active)
                .ActivateOnEnter<HemitheosAeroKnockback1>()
                .ActivateOnEnter<HemitheosHolySpread>();
            ComponentCondition<HemitheosAeroKnockback1>(id + 0x31, 1.2f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<HemitheosAeroKnockback1>();
            ComponentCondition<HemitheosHolySpread>(id + 0x32, 3.8f, comp => !comp.Active, "Spread")
                .ActivateOnEnter<StymphalianStrike>()
                .DeactivateOnExit<HemitheosHolySpread>();
            ComponentCondition<StymphalianStrike>(id + 0x33, 1, comp => comp.NumCasts > 0, "Fruit 2 (3 birds + spread) resolve")
                .DeactivateOnExit<StymphalianStrike>();
        }

        private void ForbiddenFruit3(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            Cast(id + 0x20, AID.HemitheosHoly, 3.8f, 3);
            ComponentCondition<HemitheosHoly>(id + 0x22, 0.1f, comp => comp.Active)
                .ActivateOnEnter<HemitheosHoly>();
            ComponentCondition<HemitheosHoly>(id + 0x30, 6, comp => !comp.Active, "Stacks")
                .ActivateOnEnter<StaticMoon>()
                .DeactivateOnExit<HemitheosHoly>();
            ComponentCondition<StaticMoon>(id + 0x31, 1.1f, comp => comp.NumCasts > 0, "Fruit 3 (3 bulls + stack) resolve")
                .DeactivateOnExit<StaticMoon>();
        }

        private void BoughOfAttisFrontSide(uint id, float delay, bool withHoly = false)
        {
            Cast(id, AID.BoughOfAttisFront, delay, 5.8f)
                .ActivateOnEnter<BoughOfAttisFront>();
            ComponentCondition<BoughOfAttisFront>(id + 0x2, 1.2f, comp => comp.NumCasts > 0, withHoly ? "Fromt hit + stacks" : "Front hit")
                .DeactivateOnExit<BoughOfAttisFront>()
                .DeactivateOnExit<HemitheosHoly>(withHoly);
            CastMulti(id + 0x10, new AID[] { AID.BoughOfAttisSideW, AID.BoughOfAttisSideE }, 2.4f, 4)
                .ActivateOnEnter<BoughOfAttisSide>();
            ComponentCondition<BoughOfAttisSide>(id + 0x12, 1, comp => comp.NumCasts > 0, "Side hit")
                .DeactivateOnExit<BoughOfAttisSide>();
        }

        private void BoughOfAttisFrontSideHemitheosHoly(uint id, float delay)
        {
            Cast(id, AID.HemitheosHoly, delay, 3)
                .ActivateOnEnter<HemitheosHoly>();
            BoughOfAttisFrontSide(id + 0x100, 3.2f, true);
        }

        private void ForbiddenFruit4(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            ComponentCondition<ForbiddenFruit4>(id + 0x20, 6.5f, comp => comp.NumAssignedTethers > 0, "Tethers")
                .ActivateOnEnter<ForbiddenFruit4>();
            ComponentCondition<ForbiddenFruit4>(id + 0x21, 6.6f, comp => comp.MinotaursBaited, "Center bait");
            ComponentCondition<ForbiddenFruit4>(id + 0x22, 2.8f, comp => comp.NumCasts > 0, "Fruit 4 (bull + minotaurs) resolve")
                .DeactivateOnExit<ForbiddenFruit4>();
        }

        private void ForbiddenFruit5(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            ComponentCondition<ForbiddenFruit5>(id + 0x20, 6, comp => comp.NumAssignedTethers > 0, "Tethers")
                .ActivateOnEnter<ForbiddenFruit5>();
            ComponentCondition<ForbiddenFruit5>(id + 0x30, 9.6f, comp => comp.NumCasts > 0, "Fruit 5 (birds + towers) resolve")
                .DeactivateOnExit<ForbiddenFruit5>();
        }

        private void ForbiddenFruit6(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            Cast(id + 0x20, AID.InviolatePurgation, 2.2f, 4);
            ComponentCondition<WindsHoly>(id + 0x22, 1, comp => comp.Active)
                .ActivateOnEnter<WindsHoly>();
            CastStart(id + 0x30, AID.LightOfLife, 6.3f);
            ComponentCondition<WindsHoly>(id + 0x40, 3.8f, comp => comp.NumCasts >= 1, "Stack/spread 1")
                .ActivateOnEnter<StymphalianStrike>()
                .ActivateOnEnter<StaticMoon>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<StaticMoon>(id + 0x41, 0.4f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<StymphalianStrike>()
                .DeactivateOnExit<StaticMoon>();
            ComponentCondition<WindsHoly>(id + 0x50, 14.6f, comp => comp.NumCasts >= 2, "Stack/spread 2")
                .ActivateOnEnter<HemitheosTornado>()
                .ActivateOnEnter<HemitheosGlareMine>()
                .SetHint(StateMachine.StateHint.Raidwide);
            CastEnd(id + 0x60, 7.2f, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
            CastStart(id + 0x61, AID.LightOfLife, 5.2f);
            ComponentCondition<WindsHoly>(id + 0x70, 2.6f, comp => comp.NumCasts >= 3, "Stack/spread 3")
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<WindsHoly>(id + 0x80, 15, comp => comp.NumCasts >= 4, "Stack/spread 4")
                .DeactivateOnExit<WindsHoly>()
                .SetHint(StateMachine.StateHint.Raidwide);
            CastEnd(id + 0x90, 8.4f, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<HemitheosGlareMine>(id + 0x91, 6.7f, comp => comp.NumCasts >= 4, "Last dropped AOE")
                .DeactivateOnExit<HemitheosGlareMine>()
                .DeactivateOnExit<HemitheosTornado>();
        }

        private void ForbiddenFruit7(uint id, float delay)
        {
            Cast(id, AID.ForbiddenFruit, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            RootsOfAttis(id + 0x20, 2.2f, "");
            Cast(id + 0x30, AID.HemitheosGlare, 2.2f, 5);
            ComponentCondition<StymphalianStrike>(id + 0x40, 7.7f, comp => comp.NumCasts > 0, "Fruit 7 (2 birds + chasing aoes) resolve")
                .ActivateOnEnter<StymphalianStrike>()
                .DeactivateOnExit<StymphalianStrike>();
        }

        private void FaminesHarvest(uint id, float delay)
        {
            Cast(id, AID.FaminesHarvest, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            ComponentCondition<FaminesHarvest>(id + 0x20, 6.5f, comp => comp.NumAssignedTethers > 0, "Tethers")
                .ActivateOnEnter<FaminesHarvest>();
            ComponentCondition<FaminesHarvest>(id + 0x21, 6.6f, comp => comp.MinotaursBaited, "Bait");
            ComponentCondition<FaminesHarvest>(id + 0x22, 2.8f, comp => comp.NumCasts > 0, "Fruit 8 (6 minotaurs + 2 birds) resolve")
                .DeactivateOnExit<FaminesHarvest>();
        }

        private void DeathsHarvest(uint id, float delay)
        {
            Cast(id, AID.DeathsHarvest, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            ComponentCondition<ForbiddenFruit9>(id + 0x20, 6.5f, comp => comp.NumAssignedTethers > 0, "Tethers")
                .ActivateOnEnter<ForbiddenFruit9>();
            ComponentCondition<ForbiddenFruit9>(id + 0x21, 9.3f, comp => comp.NumCasts > 0, "Fruit 9 (3 bulls + 2 birds) resolve")
                .DeactivateOnExit<ForbiddenFruit9>();
        }

        private void WarsHarvest(uint id, float delay)
        {
            Cast(id, AID.WarsHarvest, delay, 4);
            Cast(id + 0x10, AID.ForbiddenFruitInvis, 2.1f, 3);
            ComponentCondition<ForbiddenFruit10>(id + 0x20, 6.5f, comp => comp.NumAssignedTethers > 0, "Tethers")
                .ActivateOnEnter<ForbiddenFruit10>();
            ComponentCondition<ForbiddenFruit10>(id + 0x21, 9.1f, comp => comp.NumCasts > 0, "Fruit 10 (bull + 2 birds + 2 minotaurs) resolve")
                .DeactivateOnExit<ForbiddenFruit10>();
        }
    }
}
