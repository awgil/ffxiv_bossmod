namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class Ch01CloudOfDarknessStates : StateMachineBuilder
{
    public Ch01CloudOfDarknessStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        BladeOfDarkness(id, 6.2f);
        BladeOfDarkness(id + 0x10000, 4.4f);
        DelugeOfDarkness1(id + 0x20000, 4.5f);

        // note: flare starts casting ~1.9s before first criss-cross, aero/death ~3.2s after
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x30000, 4, comp => comp.Casters.Count > 0);
        Dictionary<bool, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [true] = (1, Fork1),
            [false] = (2, Fork2),
        };
        ConditionFork(id + 0x31000, 8, () => Module.PrimaryActor.CastInfo != null, () => Module.PrimaryActor.CastInfo!.IsSpell(AID.Flare), dispatch, "Criss-cross start ...");
    }

    private void Fork1(uint id)
    {
        Subphase1Variant1End(id, 0);
        FloodOfDarkness1(id + 0x100000, 8);
        Subphase2(id + 0x200000, 6.1f);

        DelugeOfDarkness1(id + 0x400000, 8.2f);
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x410000, 4, comp => comp.Casters.Count > 0);
        Subphase1Variant2End(id + 0x410000, 8);

        Cast(id + 0x500000, AID.Enrage, 11.2f, 12, "Enrage");
    }

    private void Fork2(uint id)
    {
        Subphase1Variant2End(id, 0);
        FloodOfDarkness1(id + 0x100000, 11.2f);
        Subphase2(id + 0x200000, 6.1f);

        DelugeOfDarkness1(id + 0x400000, 8.2f);
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x410000, 4, comp => comp.Casters.Count > 0);
        Subphase1Variant1End(id + 0x410000, 6.1f);

        Cast(id + 0x500000, AID.Enrage, 8, 12, "Enrage");
    }

    private void Subphase1Variant1End(uint id, float delay)
    {
        FlareUnholyDarknessBladeOfDarkness(id + 0x100, delay);
        EndeathEnaero(id + 0x10000, 1.4f);
        Break(id + 0x20000, 13.2f);
        BladeOfDarknessEndeathEnaeroResolve(id + 0x30000, 4);
        RazingVolleyParticleBeamStart(id + 0x40000, 4.2f);
        DeathAero(id + 0x50000, 12.3f);
    }

    private void Subphase1Variant2End(uint id, float delay)
    {
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x100, delay, comp => comp.NumCasts > 0, "Criss-cross start");
        DeathAero(id + 0x10000, 3.2f);
        EndeathEnaero(id + 0x20000, 6);
        RapidSequenceParticleBeam(id + 0x30000, 4.1f);
        RazingVolleyParticleBeamBladeOfDarknessEndeathEnaeroResolve(id + 0x40000, 12.3f);
        Flare(id + 0x50000, 9.3f);
    }

    private void Subphase2(uint id, float delay)
    {
        DelugeOfDarkness2(id, delay);
        DarkDominion(id + 0x10000, 9.3f); // note: 1s after cast ends, outer ring becomes dangerous
        ThirdArtOfDarknessParticleConcentration(id + 0x20000, 4); // note: 3s after towers resolve, outer ring becomes normal
        GhastlyGloom(id + 0x30000, 12.3f);
        CurseOfDarkness(id + 0x40000, 8.3f);
        EvilSeedChaosCondensedDiffusiveForceParticleBeam(id + 0x50000, 10);
        ActivePivotParticleBeam(id + 0x70000, 4.5f);
        LoomingChaos(id + 0x80000, 6.2f);

        CurseOfDarkness(id + 0x100000, 11.9f);
        ParticleConcentrationPhaser(id + 0x110000, 4.2f);
        DarkDominion(id + 0x120000, 1); // note: 1s after cast ends, outer ring becomes dangerous
        FeintParticleBeamThirdActOfDarkness(id + 0x130000, 3.1f); // note: 2.5s after act of darkness resolves, outer ring becomes normal
        GhastlyGloom(id + 0x140000, 11.4f);
        PhaserChaosCondensedDiffusiveForceParticleBeam(id + 0x150000, 3.3f);
        FloodOfDarknessAdds(id + 0x160000, 2.9f);
        FloodOfDarkness2(id + 0x170000, 8.6f);
    }

    private void BladeOfDarkness(uint id, float delay)
    {
        CastMulti(id, [AID.BladeOfDarknessL, AID.BladeOfDarknessR, AID.BladeOfDarknessC], delay, 7)
            .ActivateOnEnter<BladeOfDarkness>();
        ComponentCondition<BladeOfDarkness>(id + 2, 0.7f, comp => comp.NumCasts > 0, "In/out")
            .DeactivateOnExit<BladeOfDarkness>();
    }

    private void DelugeOfDarkness1(uint id, float delay)
    {
        Cast(id, AID.DelugeOfDarkness1, delay, 8, "Raidwide + arena transition")
            .ActivateOnEnter<DelugeOfDarkness1>()
            .DeactivateOnExit<DelugeOfDarkness1>()
            .OnExit(() => Module.Arena.Bounds = Ch01CloudOfDarkness.Phase1Bounds)
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x100, [AID.GrimEmbraceForward, AID.GrimEmbraceBackward], 9.2f, 5, "Debuffs 1")
            .ActivateOnEnter<GrimEmbraceBait>()
            .ActivateOnEnter<GrimEmbraceAOE>();
        CastMulti(id + 0x110, [AID.GrimEmbraceForward, AID.GrimEmbraceBackward], 3.1f, 5, "Debuffs 2")
            .ActivateOnEnter<RazingVolleyParticleBeam>() // has weird overlaps, easier to keep active for the entirety of the phase
            .ActivateOnEnter<EnaeroEndeath>() // we want to keep all these components active, so that they provide advance hints for delayed resolve
            .ActivateOnEnter<EndeathAOE>() // death has extra resolve steps, which make writing states weird
            .ActivateOnEnter<EnaeroAOE>();
    }

    private void RazingVolleyParticleBeamStart(uint id, float delay)
    {
        ComponentCondition<RazingVolleyParticleBeam>(id, delay, comp => comp.Casters.Count > 0);
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x10, 8, comp => comp.NumCasts > 0, "Criss-cross start");
    }

    private void DeathAero(uint id, float delay)
    {
        CastMulti(id, [AID.Death, AID.Aero], delay, 5.6f);
        ComponentCondition<EnaeroEndeath>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, "Knockback/attract");
    }

    private void EndeathEnaero(uint id, float delay)
    {
        CastMulti(id, [AID.Endeath, AID.Enaero], delay, 5, "Store knockback/attract");
    }

    private void BladeOfDarknessEndeathEnaeroResolve(uint id, float delay)
    {
        BladeOfDarkness(id, delay);
        ComponentCondition<EnaeroEndeath>(id + 0x100, 2.2f, comp => comp.NumCasts > 0, "Knockback/attract");
    }

    private void RazingVolleyParticleBeamBladeOfDarknessEndeathEnaeroResolve(uint id, float delay)
    {
        RazingVolleyParticleBeamStart(id, delay);
        BladeOfDarknessEndeathEnaeroResolve(id + 0x1000, 2.2f);
    }

    private void Break(uint id, float delay)
    {
        Cast(id, AID.BreakBoss, delay, 5)
            .ActivateOnEnter<Break>();
        ComponentCondition<Break>(id + 0x10, 1.1f, comp => comp.Eyes.Count == 0, "Gazes")
            .DeactivateOnExit<Break>();
    }

    private void RapidSequenceParticleBeam(uint id, float delay)
    {
        Cast(id, AID.RapidSequenceParticleBeam, delay, 7)
            .ActivateOnEnter<RapidSequenceParticleBeam>();
        ComponentCondition<RapidSequenceParticleBeam>(id + 0x10, 0.8f, comp => comp.NumCasts > 0, "Wild charges 1");
        ComponentCondition<RapidSequenceParticleBeam>(id + 0x11, 2, comp => comp.NumCasts > 3, "Wild charges 2");
        ComponentCondition<RapidSequenceParticleBeam>(id + 0x13, 2, comp => comp.NumCasts > 6, "Wild charges 3");
        ComponentCondition<RapidSequenceParticleBeam>(id + 0x14, 2, comp => comp.NumCasts > 9, "Wild charges 4")
            .DeactivateOnExit<RapidSequenceParticleBeam>();
    }

    private void Flare(uint id, float delay)
    {
        Cast(id, AID.Flare, delay, 4);
        ComponentCondition<Flare>(id + 0x10, 1, comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<Flare>();
        ComponentCondition<Flare>(id + 0x20, 8.1f, comp => comp.NumCasts > 0, "Flares")
            .DeactivateOnExit<Flare>();
    }

    private void FlareUnholyDarknessBladeOfDarkness(uint id, float delay)
    {
        CastStart(id, AID.Flare, delay);
        ComponentCondition<RazingVolleyParticleBeam>(id + 1, 1.9f, comp => comp.NumCasts > 0, "Criss-cross start");
        ComponentCondition<RazingVolleyParticleBeam>(id + 2, 2, comp => comp.NumCasts > 1);
        CastEnd(id + 3, 0.1f);
        ComponentCondition<Flare>(id + 0x10, 1, comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<Flare>();
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x11, 0.9f, comp => comp.NumCasts > 2);
        CastStart(id + 0x20, AID.UnholyDarkness, 1.2f);
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x21, 0.8f, comp => comp.NumCasts > 3);
        CastEnd(id + 0x22, 4.2f);
        ComponentCondition<UnholyDarkness>(id + 0x30, 0.7f, comp => comp.Stacks.Count > 0)
            .ActivateOnEnter<UnholyDarkness>();
        ComponentCondition<Flare>(id + 0x40, 0.3f, comp => comp.NumCasts > 0, "Flares")
            .DeactivateOnExit<Flare>();
        CastStartMulti(id + 0x50, [AID.BladeOfDarknessL, AID.BladeOfDarknessR, AID.BladeOfDarknessC], 7.1f);
        ComponentCondition<UnholyDarkness>(id + 0x51, 0.7f, comp => comp.NumFinishedStacks > 0, "Stacks")
            .ActivateOnEnter<BladeOfDarkness>()
            .DeactivateOnExit<UnholyDarkness>();
        CastEnd(id + 0x52, 6.3f);
        ComponentCondition<BladeOfDarkness>(id + 0x53, 0.7f, comp => comp.NumCasts > 0, "In/out")
            .DeactivateOnExit<BladeOfDarkness>();
    }

    private void FloodOfDarkness1(uint id, float delay)
    {
        Cast(id, AID.FloodOfDarkness1, delay, 7, "Raidwide + arena transition")
            .DeactivateOnExit<GrimEmbraceBait>()
            .DeactivateOnExit<GrimEmbraceAOE>()
            .DeactivateOnExit<RazingVolleyParticleBeam>()
            .DeactivateOnExit<EnaeroEndeath>()
            .DeactivateOnExit<EndeathAOE>()
            .DeactivateOnExit<EnaeroAOE>()
            .OnExit(() => Module.Arena.Bounds = Ch01CloudOfDarkness.InitialBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DelugeOfDarkness2(uint id, float delay)
    {
        Cast(id, AID.DelugeOfDarkness2, delay, 8, "Raidwide + arena transition")
            .ActivateOnEnter<DelugeOfDarkness2>()
            .DeactivateOnExit<DelugeOfDarkness2>()
            .OnExit(() => Module.Arena.Bounds = Ch01CloudOfDarkness.Phase2Bounds)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<StygianShadow>(id + 0x10, 4.2f, comp => comp.ActiveActors.Any(), "Platform adds")
            .ActivateOnEnter<StygianShadow>()
            .ActivateOnEnter<Phase2AIHints>()
            .ActivateOnEnter<Atomos>()
            .ActivateOnEnter<Phase2OuterRing>()
            .ActivateOnEnter<Phase2InnerCells>()
            .ActivateOnEnter<DarkEnergyParticleBeam>(); // overlaps with multiple mechanics
    }

    private void DarkDominion(uint id, float delay)
    {
        Cast(id, AID.DarkDominion, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ThirdArtOfDarknessParticleConcentration(uint id, float delay)
    {
        ComponentCondition<ThirdArtOfDarknessCleave>(id, delay, comp => comp.Mechanics.Count > 0)
            .ActivateOnEnter<ThirdArtOfDarknessCleave>()
            .ActivateOnEnter<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .ActivateOnEnter<ThirdArtOfDarknessMultiProngedParticleBeam>();
        Cast(id + 0x10, AID.ParticleConcentration, 2.1f, 6)
            .ActivateOnEnter<ParticleConcentration>(); // note: towers appear ~1s after cast end
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x20, 1.5f, comp => comp.NumCasts > 0, "Add cleave 1");
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x30, 3, comp => comp.NumCasts > 2, "Add cleave 2");
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x40, 3, comp => comp.NumCasts > 4, "Add cleave 3")
            .DeactivateOnExit<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .DeactivateOnExit<ThirdArtOfDarknessMultiProngedParticleBeam>()
            .DeactivateOnExit<ThirdArtOfDarknessCleave>();
        ComponentCondition<ParticleConcentration>(id + 0x50, 3.6f, comp => comp.Towers.Count == 0, "Towers")
            .ExecOnEnter<ParticleConcentration>(comp => comp.ShowOuterTowers())
            .DeactivateOnExit<ParticleConcentration>();
    }

    private State GhastlyGloom(uint id, float delay)
    {
        CastMulti(id, [AID.GhastlyGloomCross, AID.GhastlyGloomDonut], delay, 7.8f)
            .ActivateOnEnter<GhastlyGloomCross>()
            .ActivateOnEnter<GhastlyGloomDonut>();
        return Condition(id + 2, 0.7f, () => Module.FindComponent<GhastlyGloomCross>()?.NumCasts > 0 || Module.FindComponent<GhastlyGloomDonut>()?.NumCasts > 0, "Cross/donut")
            .DeactivateOnExit<GhastlyGloomCross>()
            .DeactivateOnExit<GhastlyGloomDonut>();
    }

    private void CurseOfDarkness(uint id, float delay)
    {
        ComponentCondition<CurseOfDarkness>(id, delay, comp => comp.NumCasts > 0, "Raidwide + bait debuffs")
            .ActivateOnEnter<CurseOfDarkness>()
            .DeactivateOnExit<CurseOfDarkness>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State FloodOfDarknessAdds(uint id, float delay)
    {
        ComponentCondition<FloodOfDarknessAdd>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FloodOfDarknessAdd>();
        return Timeout(id + 1, 6, "Interrupt adds")
            .DeactivateOnExit<FloodOfDarknessAdd>();
    }

    private void EvilSeedChaosCondensedDiffusiveForceParticleBeam(uint id, float delay)
    {
        ComponentCondition<EvilSeedBait>(id, delay, comp => comp.Baiters.Any())
            .ActivateOnEnter<EvilSeedBait>();
        ComponentCondition<EvilSeedAOE>(id + 0x10, 8.1f, comp => comp.Casters.Count > 0, "Seed plant")
            .ActivateOnEnter<EvilSeedAOE>()
            .ActivateOnEnter<EvilSeedVoidzone>()
            .DeactivateOnExit<EvilSeedBait>();

        GhastlyGloom(id + 0x1000, 2.8f)
            .DeactivateOnExit<EvilSeedAOE>();

        ComponentCondition<ThornyVine>(id + 0x2000, 14, comp => comp.Targets.Any())
            .ActivateOnEnter<ThornyVine>();
        ComponentCondition<ThornyVine>(id + 0x2010, 3, comp => comp.TethersAssigned, "Tethers");
        FloodOfDarknessAdds(id + 0x2020, 2.2f);

        CastMulti(id + 0x3000, [AID.ChaosCondensedParticleBeam, AID.DiffusiveForceParticleBeam], 8.1f, 8)
            .ActivateOnEnter<ChaosCondensedParticleBeam>()
            .ActivateOnEnter<DiffusiveForceParticleBeam>()
            .DeactivateOnExit<EvilSeedVoidzone>()
            .DeactivateOnExit<ThornyVine>();
        Condition(id + 0x3010, 0.8f, () => Module.FindComponent<ChaosCondensedParticleBeam>()?.NumCasts > 0 || Module.FindComponent<DiffusiveForceParticleBeam>()?.NumCasts > 0, "Spread/line stacks")
            .DeactivateOnExit<ChaosCondensedParticleBeam>()
            .DeactivateOnExit<DiffusiveForceParticleBeam>(); // TODO: show second wave ...
    }

    private void ActivePivotParticleBeam(uint id, float delay)
    {
        CastStartMulti(id, [AID.ActivePivotParticleBeamCW, AID.ActivePivotParticleBeamCCW], delay);
        ComponentCondition<Phaser>(id + 1, 0.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<ActivePivotParticleBeam>()
            .ActivateOnEnter<Phaser>();
        ComponentCondition<Phaser>(id + 2, 8, comp => comp.NumCasts > 0, "Adds front/sides");
        ComponentCondition<Phaser>(id + 3, 1.5f, comp => comp.NumCasts >= 6, "Adds sides/front")
            .DeactivateOnExit<Phaser>();
        CastEnd(id + 4, 3.7f);
        ComponentCondition<ActivePivotParticleBeam>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Rotation start");
        ComponentCondition<ActivePivotParticleBeam>(id + 0x20, 6.6f, comp => comp.NumCasts > 4, "Rotation end")
            .DeactivateOnExit<ActivePivotParticleBeam>();
        ComponentCondition<Excruciate>(id + 0x30, 0.7f, comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<Excruciate>();
        ComponentCondition<Excruciate>(id + 0x31, 5, comp => comp.NumCasts > 0, "Adds tankbusters")
            .DeactivateOnExit<Excruciate>();
    }

    private void LoomingChaos(uint id, float delay)
    {
        Cast(id, AID.LoomingChaosBoss, delay, 7);
        ComponentCondition<LoomingChaos>(id + 2, 0.7f, comp => comp.NumCasts > 0, "Raidwide + swap positions")
            .ActivateOnEnter<LoomingChaos>()
            .DeactivateOnExit<LoomingChaos>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ParticleConcentrationPhaser(uint id, float delay)
    {
        CastStart(id, AID.ParticleConcentration, delay);
        ComponentCondition<Phaser>(id + 1, 1, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Phaser>();
        CastEnd(id + 2, 5);
        ComponentCondition<Phaser>(id + 0x10, 3, comp => comp.NumCasts > 0, "Adds front/sides")
            .ActivateOnEnter<ParticleConcentration>(); // TODO: towers appear 1s after cast end
        ComponentCondition<Phaser>(id + 0x11, 1.5f, comp => comp.NumCasts >= 6, "Adds sides/front")
            .DeactivateOnExit<Phaser>();
        ComponentCondition<ParticleConcentration>(id + 0x20, 6.6f, comp => comp.Towers.Count == 0, "Towers")
            .ExecOnEnter<ParticleConcentration>(comp => comp.ShowOuterTowers())
            .DeactivateOnExit<ParticleConcentration>();
    }

    private void FeintParticleBeamThirdActOfDarkness(uint id, float delay)
    {
        CastStart(id, AID.FeintParticleBeam, delay);
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 1, 4.9f, comp => comp.Mechanics.Count > 0)
            .ActivateOnEnter<ThirdArtOfDarknessCleave>()
            .ActivateOnEnter<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .ActivateOnEnter<ThirdArtOfDarknessMultiProngedParticleBeam>();
        CastEnd(id + 2, 1.1f);
        ComponentCondition<FeintParticleBeam>(id + 0x10, 1.1f, comp => comp.Chasers.Count > 0)
            .ActivateOnEnter<FeintParticleBeam>();
        ComponentCondition<FeintParticleBeam>(id + 0x11, 4, comp => comp.NumCasts > 0, "Chasers start");
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x20, 3.5f, comp => comp.NumCasts > 0, "Add cleave 1");
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x30, 3, comp => comp.NumCasts > 2, "Add cleave 2")
            .DeactivateOnExit<FeintParticleBeam>(); // chasers resolve ~0.5s earlier
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x40, 3, comp => comp.NumCasts > 4, "Add cleave 3")
            .DeactivateOnExit<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .DeactivateOnExit<ThirdArtOfDarknessMultiProngedParticleBeam>()
            .DeactivateOnExit<ThirdArtOfDarknessCleave>();
    }

    private void PhaserChaosCondensedDiffusiveForceParticleBeam(uint id, float delay)
    {
        ComponentCondition<Phaser>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Phaser>();
        CastStartMulti(id + 0x10, [AID.ChaosCondensedParticleBeam, AID.DiffusiveForceParticleBeam], 7.5f);
        ComponentCondition<Phaser>(id + 0x11, 0.5f, comp => comp.NumCasts > 0, "Adds front/sides")
            .ActivateOnEnter<ChaosCondensedParticleBeam>()
            .ActivateOnEnter<DiffusiveForceParticleBeam>();
        ComponentCondition<Phaser>(id + 0x12, 1.5f, comp => comp.NumCasts >= 6, "Adds sides/front")
            .DeactivateOnExit<Phaser>();
        CastEnd(id + 0x13, 6);
        Condition(id + 0x20, 0.8f, () => Module.FindComponent<ChaosCondensedParticleBeam>()?.NumCasts > 0 || Module.FindComponent<DiffusiveForceParticleBeam>()?.NumCasts > 0, "Spread/line stacks")
            .DeactivateOnExit<ChaosCondensedParticleBeam>()
            .DeactivateOnExit<DiffusiveForceParticleBeam>(); // TODO: show second wave ...
    }

    private void FloodOfDarkness2(uint id, float delay)
    {
        CastStart(id, AID.FloodOfDarkness2, delay, "Adds disappear")
            .DeactivateOnExit<StygianShadow>()
            .DeactivateOnExit<Atomos>()
            .DeactivateOnExit<Phase2AIHints>()
            .DeactivateOnExit<Phase2OuterRing>()
            .DeactivateOnExit<Phase2InnerCells>()
            .DeactivateOnExit<DarkEnergyParticleBeam>();
        CastEnd(id + 1, 7, "Raidwide + arena transition")
            .OnExit(() => Module.Arena.Bounds = Ch01CloudOfDarkness.InitialBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}

