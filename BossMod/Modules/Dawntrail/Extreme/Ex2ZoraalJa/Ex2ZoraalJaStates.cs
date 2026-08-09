namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class Ex2ZoraalJaStates : StateMachineBuilder
{
    public Ex2ZoraalJaStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Actualize(id, 10.7f);
        MultidirectionalDivideHalf(id + 0x10000, 5.6f);
        MultidirectionalDivideRegicidalRage(id + 0x20000, 4.5f);
        DawnOfAnAge1(id + 0x30000, 8.4f);
        ProjectionOfTriumph1(id + 0x40000, 7.2f);
        ProjectionOfTurmoil1(id + 0x50000, 7.2f);
        DawnOfAnAge2(id + 0x60000, 8.4f);
        ProjectionOfTriumph2(id + 0x70000, 8.3f);
        ProjectionOfTurmoil2(id + 0x80000, 7.2f);
        DawnOfAnAge3(id + 0x90000, 8.4f);
        MultidirectionalDivideHalf(id + 0xA0000, 8.3f);
        Cast(id + 0xB0000, (uint)AID.Enrage, 5.3f, 10, "Enrage");
    }

    private void Actualize(uint id, float delay)
    {
        Cast(id, (uint)AID.Actualize, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MultidirectionalDivideHalf(uint id, float delay)
    {
        Cast(id, (uint)AID.MultidirectionalDivide, delay, 5, "Cross")
            .ActivateOnEnter<MultidirectionalDivide>()
            .DeactivateOnExit<MultidirectionalDivide>();
        CastStartMulti(id + 0x10, [(uint)AID.ForwardHalfR, (uint)AID.ForwardHalfL, (uint)AID.BackwardHalfR, (uint)AID.BackwardHalfL], 3.1f)
            .ActivateOnEnter<MultidirectionalDivideMain>()
            .ActivateOnEnter<MultidirectionalDivideExtra>();
        CastEnd(id + 0x11, 8, "Criss-cross") // criss-cross resolves around the cast end
            .ActivateOnEnter<ForwardBackwardHalf>()
            .DeactivateOnExit<MultidirectionalDivideMain>()
            .DeactivateOnExit<MultidirectionalDivideExtra>();
        ComponentCondition<ForwardBackwardHalf>(id + 0x20, 1.1f, comp => comp.NumCasts > 0, "Cleaves")
            .DeactivateOnExit<ForwardBackwardHalf>();
    }

    private void MultidirectionalDivideRegicidalRage(uint id, float delay)
    {
        Cast(id, (uint)AID.MultidirectionalDivide, delay, 5, "Cross")
            .ActivateOnEnter<MultidirectionalDivide>()
            .DeactivateOnExit<MultidirectionalDivide>();
        CastStart(id + 0x10, (uint)AID.RegicidalRage, 3.2f)
            .ActivateOnEnter<MultidirectionalDivideMain>()
            .ActivateOnEnter<MultidirectionalDivideExtra>()
            .ActivateOnEnter<RegicidalRage>(); // tethers appear ~0.1s before cast starts
        ComponentCondition<MultidirectionalDivideMain>(id + 0x11, 7.8f, comp => comp.NumCasts > 0, "Criss-cross")
            .DeactivateOnExit<MultidirectionalDivideMain>()
            .DeactivateOnExit<MultidirectionalDivideExtra>();
        CastEnd(id + 0x12, 0.2f);
        ComponentCondition<RegicidalRage>(id + 0x13, 0.1f, comp => comp.NumCasts > 0, "Tankbuster tethers")
            .DeactivateOnExit<RegicidalRage>();
    }

    private void HalfFull(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.HalfFullR, (uint)AID.HalfFullL], delay, 6)
            .ActivateOnEnter<HalfFull>();
        ComponentCondition<HalfFull>(id + 2, 0.3f, comp => comp.NumCasts > 0, "Side cleave")
            .DeactivateOnExit<HalfFull>();
    }

    private void DawnOfAnAge(uint id, float delay)
    {
        Cast(id, (uint)AID.DawnOfAnAge, delay, 7, "Raidwide + small arena")
            .ActivateOnEnter<DawnOfAnAgeArenaChange>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DutysEdgeResolve(uint id, float delay)
    {
        CastEnd(id, 4.9f);
        ComponentCondition<DutysEdge>(id + 1, 0.4f, comp => comp.NumCasts >= 1, "Line stack 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DutysEdge>(id + 2, 2.1f, comp => comp.NumCasts >= 2, "Line stack 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DutysEdge>(id + 3, 2.1f, comp => comp.NumCasts >= 3, "Line stack 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DutysEdge>(id + 4, 2.1f, comp => comp.NumCasts >= 4, "Line stack 4")
            .DeactivateOnExit<DutysEdge>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DawnOfAnAge1(uint id, float delay)
    {
        DawnOfAnAge(id, delay);

        Cast(id + 0x1000, (uint)AID.VollokSmall, 10.2f, 4)
            .DeactivateOnEnter<DawnOfAnAgeArenaChange>();
        Cast(id + 0x1010, (uint)AID.Sync, 5.4f, 5);
        ComponentCondition<ChasmOfVollokFangSmall>(id + 0x1020, 0.9f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<ChasmOfVollokFangSmall>();
        CastStartMulti(id + 0x1030, [(uint)AID.HalfFullR, (uint)AID.HalfFullL], 2.3f);
        ComponentCondition<ChasmOfVollokFangSmall>(id + 0x1031, 5.8f, comp => comp.NumCasts > 0, "Swords")
            .ActivateOnEnter<HalfFull>()
            .DeactivateOnExit<ChasmOfVollokFangSmall>();
        CastEnd(id + 0x1032, 0.2f);
        ComponentCondition<HalfFull>(id + 0x1033, 0.3f, comp => comp.NumCasts > 0, "Side cleave")
            .DeactivateOnExit<HalfFull>();

        Cast(id + 0x2000, (uint)AID.GreaterGateway, 4.9f, 4)
            .ActivateOnEnter<ForgedTrack>(); // envc happen ~0.9s after cast end
        Cast(id + 0x2010, (uint)AID.BladeWarp, 4.2f, 4)
            .ActivateOnEnter<ForgedTrackKnockback>();
        Cast(id + 0x2020, (uint)AID.ForgedTrack, 4.2f, 4);
        ComponentCondition<ForgedTrack>(id + 0x2030, 8.2f, comp => comp.NumCasts > 0, "Lanes") // wide aoe happens ~0.2s later, knockback ~0.6s later
            .ActivateOnEnter<ChasmOfVollokPlayer>() // icons appear ~1.2s before lanes resolve
            .ExecOnExit<ChasmOfVollokPlayer>(comp => comp.Active = true);

        CastStart(id + 0x3000, (uint)AID.Actualize, 4.9f, "Cells")
            .DeactivateOnExit<ForgedTrackKnockback>()
            .DeactivateOnExit<ForgedTrack>()
            .DeactivateOnExit<ChasmOfVollokPlayer>(); // this resolves right as cast starts
        CastEnd(id + 0x3001, 5, "Raidwide + normal arena")
            .OnExit(() => Module.Arena.Bounds = Trial.T02ZoraalJa.ZoraalJa.GetDefaultBounds())
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DawnOfAnAge2(uint id, float delay)
    {
        DawnOfAnAge(id, delay);
        ComponentCondition<DrumOfVollokPlatforms>(id + 0x10, 4.9f, comp => comp.Active)
            .DeactivateOnEnter<DawnOfAnAgeArenaChange>()
            .ActivateOnEnter<DrumOfVollokPlatforms>()
            .DeactivateOnExit<DrumOfVollokPlatforms>();

        Cast(id + 0x1000, (uint)AID.DrumOfVollok, 5.3f, 7.4f)
            .ActivateOnEnter<DrumOfVollok>()
            .ActivateOnEnter<DrumOfVollokKnockback>();
        ComponentCondition<DrumOfVollok>(id + 0x1010, 0.6f, comp => comp.NumFinishedStacks > 0, "Enumeration")
            .DeactivateOnExit<DrumOfVollok>()
            .DeactivateOnExit<DrumOfVollokKnockback>();

        Cast(id + 0x2000, (uint)AID.VollokLarge, 6.1f, 5)
            .ActivateOnEnter<ChasmOfVollokFangLarge>();
        Cast(id + 0x2010, (uint)AID.Sync, 4.2f, 5);
        CastStart(id + 0x2020, (uint)AID.AeroIII, 4.2f)
            .ActivateOnEnter<ChasmOfVollokPlayer>() // icons appear ~1.2s before cast start
            .ExecOnEnter<ChasmOfVollokPlayer>(comp => comp.Active = true);
        ComponentCondition<ChasmOfVollokFangLarge>(id + 0x2021, 4.7f, comp => comp.NumCasts > 0, "Swords")
            .DeactivateOnExit<ChasmOfVollokFangLarge>();
        ComponentCondition<ChasmOfVollokPlayer>(id + 0x2022, 0.1f, comp => comp.NumCasts > 0, "Cells")
            .DeactivateOnExit<ChasmOfVollokPlayer>();
        CastEnd(id + 0x2023, 0.2f);
        ComponentCondition<AeroIII>(id + 0x2030, 1.2f, comp => comp.Voidzones.Count > 0)
            .ActivateOnEnter<AeroIII>();

        CastMulti(id + 0x3000, [(uint)AID.ForwardHalfLongR, (uint)AID.ForwardHalfLongL, (uint)AID.BackwardHalfLongR, (uint)AID.BackwardHalfLongL], 5.2f, 9)
            .ActivateOnEnter<ForwardBackwardHalf>();
        ComponentCondition<ForwardBackwardHalf>(id + 0x3010, 1.1f, comp => comp.NumCasts > 0, "Cleaves")
            .DeactivateOnExit<ForwardBackwardHalf>();

        CastStart(id + 0x4000, (uint)AID.DutysEdge, 2.1f)
            .ActivateOnEnter<DutysEdge>();
        DutysEdgeResolve(id + 0x4010, 4.9f);

        Cast(id + 0x5000, (uint)AID.BurningChains, 2.1f, 5, "Chains")
            .ActivateOnEnter<BurningChains>();
        Cast(id + 0x6000, (uint)AID.Actualize, 15.2f, 5, "Raidwide + normal arena")
            .DeactivateOnExit<BurningChains>()
            .DeactivateOnExit<AeroIII>()
            .OnExit(() => Module.Arena.Bounds = Trial.T02ZoraalJa.ZoraalJa.GetDefaultBounds())
            .OnExit(() => Module.Arena.Center = new(100f, 100f))
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DawnOfAnAge3(uint id, float delay)
    {
        DawnOfAnAge(id, delay);

        Cast(id + 0x1000, (uint)AID.VollokSmall, 10.2f, 4);
        Cast(id + 0x1010, (uint)AID.Sync, 4.2f, 5);
        ComponentCondition<ChasmOfVollokFangSmall>(id + 0x1020, 0.9f, comp => comp.AOEs.Count > 0)
            .DeactivateOnEnter<DawnOfAnAgeArenaChange>()
            .ActivateOnEnter<ChasmOfVollokFangSmall>();
        ComponentCondition<ChasmOfVollokFangSmall>(id + 0x1030, 8, comp => comp.NumCasts > 0, "Swords")
            .ActivateOnEnter<ChasmOfVollokPlayer>() // icons appear ~2.7s before swords resolve
            .DeactivateOnExit<ChasmOfVollokFangSmall>();
        CastStart(id + 0x1040, (uint)AID.DutysEdge, 3.3f, "Cells") // player cells resolve together with cast start
            .ExecOnEnter<ChasmOfVollokPlayer>(comp => comp.Active = true)
            .ActivateOnEnter<DutysEdge>()
            .DeactivateOnExit<ChasmOfVollokPlayer>();
        DutysEdgeResolve(id + 0x1050, 4.9f);

        Cast(id + 0x2000, (uint)AID.GreaterGateway, 5.1f, 4)
            .ActivateOnEnter<ForgedTrack>(); // envc happen ~0.9s after cast end
        Cast(id + 0x2010, (uint)AID.BladeWarp, 4.2f, 4)
            .ActivateOnEnter<ForgedTrackKnockback>();
        Cast(id + 0x2020, (uint)AID.ForgedTrack, 4.2f, 4);
        ComponentCondition<ForgedTrack>(id + 0x2030, 8.2f, comp => comp.NumCasts > 0, "Lanes") // wide aoe happens ~0.2s later, knockback ~0.6s later
            .ActivateOnEnter<ChasmOfVollokPlayer>() // icons appear ~1.2s before lanes resolve
            .ExecOnExit<ChasmOfVollokPlayer>(comp => comp.Active = true);

        CastStart(id + 0x3000, (uint)AID.Actualize, 4.9f, "Cells")
            .DeactivateOnExit<ForgedTrackKnockback>()
            .DeactivateOnExit<ForgedTrack>()
            .DeactivateOnExit<ChasmOfVollokPlayer>(); // this resolves right as cast starts
        CastEnd(id + 0x3001, 5, "Raidwide + normal arena")
            .OnExit(() => Module.Arena.Bounds = Trial.T02ZoraalJa.ZoraalJa.GetDefaultBounds())
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ProjectionOfTriumph1(uint id, float delay)
    {
        Cast(id, (uint)AID.ProjectionOfTriumph, delay, 5)
            .ActivateOnEnter<ProjectionOfTriumph>();
        Cast(id + 0x10, (uint)AID.ProjectionOfTriumph, 5.2f, 5, "Swords 1"); // first set of circles/donuts happen right before cast ends
        ComponentCondition<ProjectionOfTriumph>(id + 0x20, 4.9f, comp => comp.NumCasts >= 16, "Swords 2");
        CastStartMulti(id + 0x30, [(uint)AID.ForwardHalfR, (uint)AID.ForwardHalfL, (uint)AID.BackwardHalfR, (uint)AID.BackwardHalfL], 4.5f);
        ComponentCondition<ProjectionOfTriumph>(id + 0x31, 0.6f, comp => comp.NumCasts >= 32, "Swords 3")
            .ActivateOnEnter<ForwardBackwardHalf>();
        ComponentCondition<ProjectionOfTriumph>(id + 0x40, 5.0f, comp => comp.NumCasts >= 48, "Swords 4");
        CastEnd(id + 0x41, 2.4f);
        ComponentCondition<ForwardBackwardHalf>(id + 0x42, 1.1f, comp => comp.NumCasts > 0, "Cleaves")
           .DeactivateOnExit<ForwardBackwardHalf>();
        ComponentCondition<ProjectionOfTriumph>(id + 0x50, 1.5f, comp => comp.NumCasts >= 56, "Swords 5");
        CastStart(id + 0x60, (uint)AID.Actualize, 3.6f);
        ComponentCondition<ProjectionOfTriumph>(id + 0x61, 1.5f, comp => comp.NumCasts >= 64, "Swords 6")
            .DeactivateOnExit<ProjectionOfTriumph>();
        CastEnd(id + 0x62, 3.5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ProjectionOfTriumph2(uint id, float delay)
    {
        Cast(id, (uint)AID.ProjectionOfTriumph, delay, 5)
            .ActivateOnEnter<ProjectionOfTriumph>();
        Cast(id + 0x10, (uint)AID.ProjectionOfTriumph, 5.2f, 5, "Swords 1"); // first set of circles/donuts happen right before cast ends
        CastStartMulti(id + 0x20, [(uint)AID.HalfCircuitCircle, (uint)AID.HalfCircuitDonut], 4.9f, "Swords 2"); // second set of circles/donuts happen together with cast start
        ComponentCondition<ProjectionOfTriumph>(id + 0x30, 5.1f, comp => comp.NumCasts >= 32, "Swords 3")
            .ActivateOnEnter<HalfCircuitRect>()
            .ActivateOnEnter<HalfCircuitDonut>()
            .ActivateOnEnter<HalfCircuitCircle>();
        CastEnd(id + 0x40, 1.9f, "In/out")
            .DeactivateOnExit<HalfCircuitDonut>()
            .DeactivateOnExit<HalfCircuitCircle>();
        ComponentCondition<HalfCircuitRect>(id + 0x41, 0.3f, comp => comp.NumCasts > 0, "Side cleave")
            .DeactivateOnExit<HalfCircuitRect>();
        ComponentCondition<ProjectionOfTriumph>(id + 0x50, 2.8f, comp => comp.NumCasts >= 48, "Swords 4");
        ComponentCondition<ProjectionOfTriumph>(id + 0x60, 5, comp => comp.NumCasts >= 56, "Swords 5");
        CastStart(id + 0x61, (uint)AID.RegicidalRage, 0.1f)
            .ActivateOnEnter<RegicidalRage>();
        ComponentCondition<ProjectionOfTriumph>(id + 0x70, 5.0f, comp => comp.NumCasts >= 64, "Swords 6")
            .DeactivateOnExit<ProjectionOfTriumph>();
        CastEnd(id + 0x71, 3);
        ComponentCondition<RegicidalRage>(id + 0x72, 0.1f, comp => comp.NumCasts > 0, "Tankbuster tethers")
            .DeactivateOnExit<RegicidalRage>();
    }

    private State BitterWhirlwind(uint id, float delay)
    {
        Cast(id, (uint)AID.BitterWhirlwind, delay, 5, "Tankbuster 1")
            .ActivateOnEnter<BitterWhirlwind>();
        ComponentCondition<BitterWhirlwind>(id + 0x10, 3.3f, comp => comp.NumCasts >= 2, "Tankbuster 2");
        return ComponentCondition<BitterWhirlwind>(id + 0x11, 3.1f, comp => comp.NumCasts >= 3, "Tankbuster 3")
            .DeactivateOnExit<BitterWhirlwind>();
    }

    private void ProjectionOfTurmoil1(uint id, float delay)
    {
        Cast(id, (uint)AID.ProjectionOfTurmoil, delay, 5, "Moving line with stacks")
            .ActivateOnEnter<ProjectionOfTurmoil>();
        BitterWhirlwind(id + 0x100, 45.3f)
            .DeactivateOnExit<ProjectionOfTurmoil>();
    }

    private void ProjectionOfTurmoil2(uint id, float delay)
    {
        Cast(id, (uint)AID.ProjectionOfTurmoil, delay, 5, "Moving line with stacks")
            .ActivateOnEnter<ProjectionOfTurmoil>();
        HalfFull(id + 0x100, 16.6f);
        HalfFull(id + 0x200, 2.9f);
        HalfFull(id + 0x300, 4.1f);
        BitterWhirlwind(id + 0x400, 3.9f)
            .DeactivateOnExit<ProjectionOfTurmoil>();
    }
}
