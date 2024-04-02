namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

class P12S2PallasAthenaStates : StateMachineBuilder
{
    public P12S2PallasAthenaStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Ultima(id, 6.2f);
        PalladianGrasp(id + 0x10000, 7.2f);
        Gaiaochos1(id + 0x20000, 6.1f);
        ClassicalConcepts1(id + 0x30000, 5.0f);
        Ultima(id + 0x40000, 1.6f);
        CrushHelm(id + 0x50000, 4.2f);
        CaloricTheory1(id + 0x60000, 6.1f);
        Ekpyrosis(id + 0x70000, 6.3f);
        Pangenesis(id + 0x80000, 7.2f);
        ClassicalConcepts2(id + 0x90000, 6.2f);
        Ultima(id + 0xA0000, 1.6f);
        CrushHelm(id + 0xB0000, 4.2f);
        CaloricTheory2(id + 0xC0000, 6.1f);
        Gaiaochos2(id + 0xD0000, 7.2f);
        Cast(id + 0xE0000, AID.Ignorabimus, 13.8f, 15, "Enrage");
    }

    private State Ultima(uint id, float delay)
    {
        return Cast(id, AID.UltimaNormal, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void PalladianGraspResolve(uint id, float delay)
    {
        ComponentCondition<PalladianGrasp>(id, delay, comp => comp.NumCasts >= 1, "Cleave 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        Cast(id + 0x10, AID.PalladianGrasp2, 2.1f, 1);
        ComponentCondition<PalladianGrasp>(id + 0x20, 0.2f, comp => comp.NumCasts >= 2, "Cleave 2")
            .DeactivateOnExit<PalladianGrasp>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void PalladianGrasp(uint id, float delay)
    {
        Cast(id, AID.PalladianGrasp1, delay, 5)
            .ActivateOnEnter<PalladianGrasp>();
        PalladianGraspResolve(id + 0x100, 0.2f);
    }

    private void CrushHelm(uint id, float delay)
    {
        Cast(id, AID.CrushHelm, delay, 5);
        ComponentCondition<CrushHelm>(id + 0x10, 4.0f, comp => comp.NumSmallHits >= 4, "Max vuln stacks")
            .ActivateOnEnter<CrushHelm>();
        ComponentCondition<CrushHelm>(id + 0x20, 2.1f, comp => comp.NumLargeHits > 0, "Tankbuster")
            .DeactivateOnExit<CrushHelm>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void EkpyrosisResolve(uint id, float delay)
    {
        CastEnd(id, delay);
        ComponentCondition<EkpyrosisExaflare>(id + 0x10, 6.8f, comp => comp.NumCasts > 0, "Exaflare")
            .ActivateOnEnter<EkpyrosisProximityH>()
            .ActivateOnEnter<EkpyrosisProximityV>()
            .ActivateOnEnter<EkpyrosisExaflare>();
        Condition(id + 0x20, 1.0f, () => Module.FindComponent<EkpyrosisProximityH>()?.NumCasts > 0 || Module.FindComponent<EkpyrosisProximityV>()?.NumCasts > 0, "Proximity")
            .DeactivateOnExit<EkpyrosisProximityH>()
            .DeactivateOnExit<EkpyrosisProximityV>();
        ComponentCondition<EkpyrosisSpread>(id + 0x30, 3.1f, comp => !comp.Active, "Spread")
            .ActivateOnEnter<EkpyrosisSpread>()
            .DeactivateOnExit<EkpyrosisSpread>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Ultima(id + 0x40, 3.3f)
            .DeactivateOnExit<EkpyrosisExaflare>(); // last happens exaflare ~0.8s into cast
    }

    private void Ekpyrosis(uint id, float delay)
    {
        CastStart(id, AID.Ekpyrosis, delay);
        EkpyrosisResolve(id + 0x100, 4);
    }

    private void Gaiaochos1(uint id, float delay)
    {
        Cast(id, AID.Gaiaochos, delay, 7, "Raidwide + small arena 1 start")
            .ActivateOnEnter<Gaiaochos>()
            .DeactivateOnExit<Gaiaochos>()
            .OnExit(() => Module.Arena.Bounds = P12S2PallasAthena.SmallBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10, AID.SummonDarkness, 8.2f, 3);
        ComponentCondition<MissingLink>(id + 0x20, 7.1f, comp => comp.TethersAssigned, "Tethers")
            .ActivateOnEnter<MissingLink>()
            .ActivateOnEnter<UltimaRay>(); // PATE happens 0.8s after cast end, casts start 4.9s
        ComponentCondition<UltimaRay>(id + 0x30, 4.8f, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<UltimaRay>();
        ComponentCondition<MissingLink>(id + 0x40, 1.3f, comp => comp.NumCasts > 0) // note: no point in having a name here, they should resolve automatically
            .DeactivateOnExit<MissingLink>();

        Cast(id + 0x100, AID.DemiParhelion, 6.0f, 3)
            .ActivateOnEnter<DemiParhelion>(); // note: casts start ~0.8s after boss cast ends
        CastMulti(id + 0x110, new[] { AID.GeocentrismV, AID.GeocentrismC, AID.GeocentrismH }, 3.2f, 7)
            .ActivateOnEnter<Geocentrism>()
            .ActivateOnEnter<DivineExcoriation>(); // icons appear 4.9s into cast
        ComponentCondition<DemiParhelion>(id + 0x120, 0.1f, comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<DemiParhelion>();
        ComponentCondition<Geocentrism>(id + 0x130, 0.5f, comp => comp.NumCasts > 0, "Lines/donut start");
        ComponentCondition<DivineExcoriation>(id + 0x140, 0.4f, comp => !comp.Active, "Spreads")
            .DeactivateOnExit<DivineExcoriation>();
        ComponentCondition<Geocentrism>(id + 0x150, 2.5f, comp => comp.NumCasts >= comp.NumConcurrentAOEs * 6, "Lines/donut resolve")
            .DeactivateOnExit<Geocentrism>();

        Cast(id + 0x200, AID.UltimaGaiaochos, 1.6f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<GaiaochosEnd>(id + 0x210, 2.2f, comp => comp.Finished, "Small arena 1 resolve")
            .ActivateOnEnter<GaiaochosEnd>()
            .DeactivateOnExit<GaiaochosEnd>()
            .OnExit(() => Module.Arena.Bounds = P12S2PallasAthena.DefaultBounds);
    }

    private void Gaiaochos2(uint id, float delay)
    {
        Cast(id, AID.Gaiaochos, delay, 7, "Raidwide + small arena 2 start")
            .ActivateOnEnter<Gaiaochos>()
            .DeactivateOnExit<Gaiaochos>()
            .OnExit(() => Module.Arena.Bounds = P12S2PallasAthena.SmallBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10, AID.SummonDarkness, 8.2f, 3);
        Cast(id + 0x20, AID.DemiParhelion, 3.1f, 3)
            .ActivateOnEnter<DemiParhelion>(); // note: casts start ~0.8s after boss cast ends
        CastStartMulti(id + 0x30, new[] { AID.GeocentrismV, AID.GeocentrismC, AID.GeocentrismH }, 3.2f);
        ComponentCondition<MissingLink>(id + 0x40, 1.8f, comp => comp.TethersAssigned, "Tethers")
            .ActivateOnEnter<MissingLink>()
            .ActivateOnEnter<Geocentrism>()
            .ActivateOnEnter<UltimaRay>(); // cast starts 0.7s into geocentrism cast
        CastEnd(id + 0x50, 5.2f);
        ComponentCondition<DemiParhelion>(id + 0x60, 0.1f, comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<DemiParhelion>();
        ComponentCondition<Geocentrism>(id + 0x70, 0.5f, comp => comp.NumCasts > 0, "Lines/donut start")
            .DeactivateOnExit<UltimaRay>(); // casts finish at the same time
        ComponentCondition<DivineExcoriation>(id + 0x80, 0.3f, comp => comp.Active)
            .ActivateOnEnter<DivineExcoriation>()
            .DeactivateOnExit<MissingLink>(); // finishes at the same time as icons appear
        ComponentCondition<Geocentrism>(id + 0x90, 2.6f, comp => comp.NumCasts >= comp.NumConcurrentAOEs * 6, "Lines/donut resolve")
            .DeactivateOnExit<Geocentrism>();
        ComponentCondition<DivineExcoriation>(id + 0xA0, 0.5f, comp => !comp.Active, "Spreads")
            .DeactivateOnExit<DivineExcoriation>();

        Cast(id + 0x100, AID.SummonDarkness, 5.1f, 3);
        CastStart(id + 0x110, AID.UltimaGaiaochos, 10.2f)
            .ActivateOnEnter<UltimaBlow>(); // tethers appear 2.8s after previous cast end
        ComponentCondition<UltimaBlow>(id + 0x120, 0.8f, comp => comp.NumCasts > 0, "Charges")
            .DeactivateOnExit<UltimaBlow>();
        CastEnd(id + 0x130, 4.2f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        CastStart(id + 0x200, AID.UltimaGaiaochos, 9.2f)
            .ActivateOnEnter<UltimaBlow>(); // tethers appear 1.7s after previous cast end
        ComponentCondition<UltimaBlow>(id + 0x210, 0.6f, comp => comp.NumCasts > 0, "Charges")
            .DeactivateOnExit<UltimaBlow>();
        CastEnd(id + 0x220, 4.4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State PalladianRayResolve(uint id, float delay)
    {
        CastEnd(id, delay);
        ComponentCondition<PalladianRayBait>(id + 0x10, 2.7f, comp => comp.NumCasts > 0, "Cones bait")
            .ActivateOnEnter<PalladianRayAOE>()
            .DeactivateOnExit<PalladianRayBait>();
        ComponentCondition<PalladianRayAOE>(id + 0x20, 1.6f, comp => comp.NumCasts > 0); // first non-baited hit
        return ComponentCondition<PalladianRayAOE>(id + 0x30, 2.3f, comp => comp.NumCasts >= 5 * comp.NumConcurrentAOEs, "Cones resolve")
            .DeactivateOnExit<PalladianRayAOE>();
    }

    private void ClassicalConcepts1(uint id, float delay)
    {
        Cast(id, AID.ClassicalConcepts, delay, 7, "Raidwide + playstation 1 start")
            .ActivateOnEnter<ClassicalConcepts1>() // icons appear ~0.9s after cast end, concepts spawn ~1.1s after cast end, tethers between players ~5s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<ClassicalConcepts>(id + 0x10, 5.0f, comp => comp.NumPlayerTethers > 0, "Player tethers");
        ComponentCondition<ClassicalConcepts>(id + 0x20, 7.8f, comp => comp.NumShapeTethers > 0, "Shape tethers");
        ComponentCondition<ClassicalConcepts>(id + 0x30, 3.0f, comp => comp.NumShapeTethers == 0, "Tethers resolve")
            .SetHint(StateMachine.StateHint.Raidwide); // note: actual damage effects are randomly staggered over ~1s after tethers disappear

        ComponentCondition<Implode>(id + 0x100, 2.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Implode>()
            .DeactivateOnExit<ClassicalConcepts>(); // note: it fully deactivates slightly later...
        CastStart(id + 0x110, AID.PalladianRay, 1.6f)
            .ActivateOnEnter<PalladianRayBait>(); // TODO: reconsider activation point?
        ComponentCondition<Implode>(id + 0x120, 1.4f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<Implode>();
        PalladianRayResolve(id + 0x200, 0.6f);
    }

    private void ClassicalConcepts2(uint id, float delay)
    {
        Cast(id, AID.ClassicalConcepts, delay, 7, "Raidwide + playstation 2 start")
            .ActivateOnEnter<ClassicalConcepts2>() // icons appear and concepts spawn ~0.9s after cast end, tethers between players ~5s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10, AID.PantaRhei, 7.2f, 10);
        // +0.8s: shapes teleport to mirrored positions
        ComponentCondition<ClassicalConcepts>(id + 0x20, 2.6f, comp => comp.NumShapeTethers > 0, "Shape tethers");
        CastStart(id + 0x30, AID.PalladianRay, 1.6f)
            .ActivateOnEnter<PalladianRayBait>();
        ComponentCondition<ClassicalConcepts>(id + 0x40, 1.4f, comp => comp.NumShapeTethers == 0, "Tethers resolve")
            .ActivateOnEnter<Implode>() // note: starts ~2.2s after palladian ray cast end
            .DeactivateOnExit<ClassicalConcepts>()
            .SetHint(StateMachine.StateHint.Raidwide); // note: actual damage effects are randomly staggered over ~1s after tethers disappear
        PalladianRayResolve(id + 0x100, 0.6f)
            .DeactivateOnExit<Implode>(); // note: ends ~1.3s before ray resolve
    }

    private void CaloricTheory1(uint id, float delay)
    {
        CastStart(id, AID.CaloricTheory, delay)
            .ActivateOnEnter<CaloricTheory1Part1>(); // icons appear right before cast start
        CastEnd(id + 1, 8, "Raidwide + caloric 1 start")
            .DeactivateOnExit<CaloricTheory1Part1>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<CaloricTheory1Part2>(id + 0x10, 0.8f, comp => comp.Active)
            .ActivateOnEnter<CaloricTheory1Part2>();
        ComponentCondition<CaloricTheory1Part2>(id + 0x11, 12, comp => !comp.Active, "Fire stacks")
            .DeactivateOnExit<CaloricTheory1Part2>();

        ComponentCondition<CaloricTheory1Part3>(id + 0x20, 0.6f, comp => comp.Stacks.Count > 0)
            .ActivateOnEnter<CaloricTheory1Part3>();
        ComponentCondition<CaloricTheory1Part3>(id + 0x30, 10.8f, comp => !comp.Active, "Caloric 1 resolve")
            .DeactivateOnExit<CaloricTheory1Part3>(); // note: spreads resolve ~0.2s earlier
        // TODO: movement debuffs disappear ~1.7s later
    }

    private void CaloricTheory2(uint id, float delay)
    {
        CastStart(id, AID.CaloricTheory, delay)
            .ActivateOnEnter<CaloricTheory2Part1>(); // icons appear right before cast start
        CastEnd(id + 1, 8, "Raidwide + caloric 2 start")
            .DeactivateOnExit<CaloricTheory2Part1>()
            .SetHint(StateMachine.StateHint.Raidwide);

        CastStart(id + 0x100, AID.Ekpyrosis, 26.2f)
            .ActivateOnEnter<CaloricTheory2Part2>()
            .ActivateOnEnter<EntropicExcess>();
        ComponentCondition<CaloricTheory2Part2>(id + 0x110, 1.7f, comp => comp.Done, "Caloric 2 resolve")
            .DeactivateOnExit<EntropicExcess>()
            .DeactivateOnExit<CaloricTheory2Part2>();
        // TODO: movement debuffs disappear ~0.8s later
        EkpyrosisResolve(id + 0x200, 2.3f);
    }

    private void Pangenesis(uint id, float delay)
    {
        Cast(id, AID.Pangenesis, delay, 7, "Raidwide + towers start")
            .ActivateOnEnter<Pangenesis>() // statuses appear 0.7s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10, AID.Pantheos, 4.1f, 4);
        ComponentCondition<Pangenesis>(id + 0x20, 5.8f, comp => comp.NumCasts >= 2, "Towers 1");
        ComponentCondition<Pangenesis>(id + 0x30, 5.0f, comp => comp.NumCasts >= 6, "Towers 2");
        ComponentCondition<Pangenesis>(id + 0x40, 5.0f, comp => comp.NumCasts >= 10, "Towers 3")
            .DeactivateOnExit<Pangenesis>();

        // next mechanic (palladian grasp) overlaps with resolve
        CastStart(id + 0x1000, AID.PalladianGrasp1, 10.4f)
            .ActivateOnEnter<FactorIn>(); // tethers appear ~5.3s after towers resolve
        ComponentCondition<FactorIn>(id + 0x1010, 2.0f, comp => comp.NumCasts > 0, "Slime baits")
            .ActivateOnEnter<PalladianGrasp>()
            .DeactivateOnExit<FactorIn>();
        CastEnd(id + 0x1020, 3.0f);
        PalladianGraspResolve(id + 0x1100, 0.2f);
    }
}
