namespace BossMod.Endwalker.Savage.P6SHegemone;

class P6SStates : StateMachineBuilder
{
    public P6SStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        HemitheosDark(id, 8.2f);
        Synergy(id + 0x10000, 6.7f);
        PolyominoidUnholyDarknessExocleaver(id + 0x20000, 9.9f);
        PathogenicCells(id + 0x30000, 6);
        ExchangeOfAgoniesChorosIxou(id + 0x40000, 8.3f);
        Synergy(id + 0x50000, 7.1f);
        HemitheosDark(id + 0x60000, 7.2f);
        TransmissionChorosIxou(id + 0x70000, 6.7f);
        PolyominoidSigmaDarkDome(id + 0x80000, 6.2f);
        ExchangeOfAgoniesExocleaver(id + 0x90000, 6.6f);
        Synergy(id + 0xA0000, 6.1f);
        HemitheosDark(id + 0xB0000, 7.2f);
        PolyominoidUnholyDarkness(id + 0xC0000, 6.7f);
        DarkAshesChorosIxou(id + 0xD0000, 7.8f);
        CachexiaDualPredationPteraIxou(id + 0xE0000, 8.9f);
        Synergy(id + 0xF0000, 7.2f);
        HemitheosDark(id + 0x100000, 7.2f);
        PolyominoidDarkSphereDarkDome(id + 0x110000, 6.7f);
        ExchangeOfAgoniesChorosIxou(id + 0x120000, 7.6f);
        PolyominoidSigmaChorosIxou(id + 0x130000, 6.1f);
        Synergy(id + 0x140000, 6.1f);
        HemitheosDark(id + 0x150000, 7.2f);
        CachexiaTransmissionPolyominoidPteraIxou(id + 0x160000, 9.4f);
        AethericPolyominoidDarkDome(id + 0x170000, 8.2f);
        AethericPolyominoidChorosIxou(id + 0x180000, 7.5f);
        Synergy(id + 0x190000, 6.2f);
        HemitheosDark(id + 0x1A0000, 7.2f);
        Cast(id + 0x1B0000, AID.Enrage, 10, 10, "Enrage");
    }

    private void HemitheosDark(uint id, float delay)
    {
        Cast(id, AID.HemitheosDark, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Synergy(uint id, float delay)
    {
        // note that casts have different time, but resolve is the same
        CastStartMulti(id, new AID[] { AID.Synergy, AID.ChelicSynergy }, delay);
        ComponentCondition<Synergy>(id + 1, 7, comp => comp.Done, "Tankbuster")
            .ActivateOnEnter<Synergy>()
            .DeactivateOnExit<Synergy>()
            .SetHint(StateMachine.StateHint.Tankbuster | StateMachine.StateHint.BossCastEnd);
    }

    private void PathogenicCells(uint id, float delay)
    {
        CastStart(id, AID.PathogenicCells, delay)
            .ActivateOnEnter<PathogenicCells>();
        CastEnd(id + 1, 8, "Limit cut start");
        ComponentCondition<PathogenicCells>(id + 0x10, 14, comp => comp.NumCasts >= 8, "Limit cut resolve")
            .DeactivateOnExit<PathogenicCells>();
    }

    // leaves component active for second cone
    private void ChorosIxouStart(uint id, float delay, bool withParasiteStun = false)
    {
        CastMulti(id, new AID[] { AID.ChorosIxouFSFront, AID.ChorosIxouSFSides }, delay, 4.5f, withParasiteStun ? "Parasite stun" : "")
            .ActivateOnEnter<ChorosIxou>()
            .SetHint(StateMachine.StateHint.DowntimeStart, withParasiteStun);
        ComponentCondition<ChorosIxou>(id + 2, 0.5f, comp => comp.FirstDone, "Cones 1");
    }

    // happens ~3.1s after start
    private void ChorosIxouEnd(uint id, float delay)
    {
        ComponentCondition<ChorosIxou>(id, delay, comp => comp.SecondDone, "Cones 2")
            .DeactivateOnExit<ChorosIxou>();
    }

    // leaves component active; includes two 'exchange' casts
    private void ExchangeOfAgoniesStart(uint id, float delay)
    {
        Cast(id, AID.AetherialExchange, delay, 3);
        Cast(id + 2, AID.ExchangeOfAgonies, 2.7f, 4);
        ComponentCondition<Agonies>(id + 4, 0.9f, comp => comp.Active)
            .ActivateOnEnter<Agonies>();
    }

    // happens ~7s after start
    private void ExchangeOfAgoniesResolve(uint id, float delay)
    {
        ComponentCondition<Agonies>(id, delay, comp => !comp.Active, "Stack/spread")
            .DeactivateOnExit<Agonies>();
    }

    // leaves component active
    private State DarkDomeBait(uint id, float delay, string activateName = "")
    {
        Cast(id, AID.DarkDome, delay, 4, "Puddles bait");
        return ComponentCondition<DarkDome>(id + 2, 0.9f, comp => comp.Casters.Count > 0, activateName)
            .ActivateOnEnter<DarkDome>();
    }

    // happens ~4s after bait
    private void DarkDomeEnd(uint id, float delay)
    {
        ComponentCondition<DarkDome>(id, delay, comp => comp.Casters.Count == 0, "Puddles resolve")
            .DeactivateOnExit<DarkDome>();
    }

    private void ExchangeOfAgoniesChorosIxou(uint id, float delay)
    {
        ExchangeOfAgoniesStart(id, delay);
        ChorosIxouStart(id + 0x10, 1.9f);
        ExchangeOfAgoniesResolve(id + 0x20, 0.1f);
        ChorosIxouEnd(id + 0x30, 3);
    }

    private void TransmissionChorosIxou(uint id, float delay)
    {
        Cast(id, AID.Transmission, delay, 5)
            .ActivateOnEnter<Transmission>();
        ChorosIxouStart(id + 0x10, 8.4f, true); // out-of-control is applied right as cast ends
        ComponentCondition<Transmission>(id + 0x20, 1.5f, comp => !comp.StunsActive)
            .DeactivateOnExit<Transmission>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ChorosIxouEnd(id + 0x30, 1.6f);
    }

    private void DarkAshesChorosIxou(uint id, float delay)
    {
        Cast(id, AID.DarkAshes, delay, 4)
            .ActivateOnEnter<DarkAshes>();
        ChorosIxouStart(id + 0x10, 3.5f);
        ComponentCondition<DarkAshes>(id + 0x20, 0.4f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<DarkAshes>();
        ChorosIxouEnd(id + 0x30, 2.7f);
    }

    private void PolyominoidSigmaChorosIxou(uint id, float delay)
    {
        Cast(id, AID.AetherialExchange, delay, 3);
        Cast(id + 0x10, AID.PolyominoidSigma, 2.7f, 4)
            .ActivateOnEnter<Polyominoid>();
        ChorosIxouStart(id + 0x20, 7.4f);
        ComponentCondition<Polyominoid>(id + 0x30, 0.3f, comp => comp.NumCasts > 0, "Cells resolve")
            .DeactivateOnExit<Polyominoid>();
        ChorosIxouEnd(id + 0x40, 2.8f);
    }

    private void AethericPolyominoidChorosIxou(uint id, float delay)
    {
        Cast(id, AID.AethericPolyominoid, delay, 4)
            .ActivateOnEnter<Polyominoid>();
        ChorosIxouStart(id + 0x20, 4.8f);
        ComponentCondition<Polyominoid>(id + 0x30, 0.2f, comp => comp.NumCasts > 0, "Cells resolve")
            .DeactivateOnExit<Polyominoid>();
        ChorosIxouEnd(id + 0x40, 2.9f);
    }

    private void PolyominoidUnholyDarkness(uint id, float delay)
    {
        Cast(id, AID.AetherialExchange, delay, 3);
        Cast(id + 0x10, AID.PolyominoidSigma, 2.6f, 4)
            .ActivateOnEnter<Polyominoid>();
        Cast(id + 0x20, AID.UnholyDarkness, 2.7f, 4);
        ComponentCondition<UnholyDarkness>(id + 0x30, 7.1f, comp => comp.NumFinishedStacks > 0, "Cells resolve + Party stacks")
            .ActivateOnEnter<UnholyDarkness>() // activates ~1s after cast end
            .DeactivateOnExit<UnholyDarkness>()
            .DeactivateOnExit<Polyominoid>(); // resolves in the same frame
    }

    private void PolyominoidUnholyDarknessExocleaver(uint id, float delay)
    {
        Cast(id, AID.AethericPolyominoid, delay, 4)
            .ActivateOnEnter<Polyominoid>();
        Cast(id + 0x10, AID.UnholyDarkness, 2.7f, 4);
        Cast(id + 0x20, AID.Exocleaver, 2.2f, 4, "Cells resolve + Pizzas 1")
            .ActivateOnEnter<UnholyDarkness>() // activates ~0.1s after cast start
            .ActivateOnEnter<Exocleaver>()
            .DeactivateOnExit<Polyominoid>(); // resolves ~0.2s before cast end
        ComponentCondition<UnholyDarkness>(id + 0x30, 2.2f, comp => comp.NumFinishedStacks > 0, "Party stacks")
            .DeactivateOnExit<UnholyDarkness>();
        ComponentCondition<Exocleaver>(id + 0x31, 0.4f, comp => comp.NumCasts > 0, "Pizzas 2")
            .DeactivateOnExit<Exocleaver>();
    }

    private void ExchangeOfAgoniesExocleaver(uint id, float delay)
    {
        ExchangeOfAgoniesStart(id, delay);
        Cast(id + 0x10, AID.Exocleaver, 3, 4, "Pizzas 1")
            .ActivateOnEnter<Exocleaver>();
        ExchangeOfAgoniesResolve(id + 0x20, 0);
        ComponentCondition<Exocleaver>(id + 0x30, 2.6f, comp => comp.NumCasts > 0, "Pizzas 2")
            .DeactivateOnExit<Exocleaver>();
    }

    private void PolyominoidSigmaDarkDome(uint id, float delay)
    {
        Cast(id, AID.AetherialExchange, delay, 3);
        Cast(id + 0x10, AID.PolyominoidSigma, 2.7f, 4)
            .ActivateOnEnter<Polyominoid>();
        DarkDomeBait(id + 0x20, 5.7f);
        ComponentCondition<Polyominoid>(id + 0x30, 3.9f, comp => comp.NumCasts > 0, "Cells resolve")
            .DeactivateOnExit<Polyominoid>();
        DarkDomeEnd(id + 0x40, 0.1f);
    }

    private void AethericPolyominoidDarkDome(uint id, float delay)
    {
        Cast(id, AID.AethericPolyominoid, delay, 4)
            .ActivateOnEnter<Polyominoid>();
        DarkDomeBait(id + 0x10, 4.7f);
        ComponentCondition<Polyominoid>(id + 0x20, 3.6f, comp => comp.NumCasts > 0, "Cells resolve")
            .DeactivateOnExit<Polyominoid>();
        DarkDomeEnd(id + 0x30, 0.4f);
    }

    private void PolyominoidDarkSphereDarkDome(uint id, float delay)
    {
        Cast(id, AID.AethericPolyominoid, delay, 4)
            .ActivateOnEnter<Polyominoid>();
        Cast(id + 0x10, AID.DarkSphere, 2.7f, 4)
            .ActivateOnEnter<DarkSphere>(); // activates ~0.9s after cast end
        DarkDomeBait(id + 0x20, 2.1f, "Spread + Cells resolve")
            .DeactivateOnExit<DarkSphere>() // resolves ~0.1s before bait
            .DeactivateOnExit<Polyominoid>(); // resolves in the same frame
        DarkDomeEnd(id + 0x30, 4);
    }

    private void CachexiaDualPredationPteraIxou(uint id, float delay)
    {
        Cast(id, AID.Cachexia, delay, 3)
            .ActivateOnEnter<AetheronecrosisPredation>();
        ComponentCondition<AetheronecrosisPredation>(id + 2, 0.9f, comp => comp.Active, "Cachexia 1 start");
        ComponentCondition<AetheronecrosisPredation>(id + 0x10, 8, comp => comp.NumCastsAetheronecrosis > 0, "Explode 1");
        CastStart(id + 0x20, AID.DualPredationFirst, 2.3f);
        ComponentCondition<AetheronecrosisPredation>(id + 0x21, 1.7f, comp => comp.NumCastsAetheronecrosis > 2, "Explode 2");
        ComponentCondition<AetheronecrosisPredation>(id + 0x22, 4, comp => comp.NumCastsAetheronecrosis > 4, "Explode 3");
        CastEnd(id + 0x23, 0.3f);
        ComponentCondition<AetheronecrosisPredation>(id + 0x24, 0.9f, comp => comp.NumCastsDualPredation > 0, "Wing/snake 1");
        ComponentCondition<AetheronecrosisPredation>(id + 0x25, 2.8f, comp => comp.NumCastsAetheronecrosis > 6, "Explode 4");
        ComponentCondition<AetheronecrosisPredation>(id + 0x30, 1.2f, comp => comp.NumCastsDualPredation > 1, "Wing/snake 2");
        ComponentCondition<AetheronecrosisPredation>(id + 0x31, 4, comp => comp.NumCastsDualPredation > 2, "Wing/snake 3");
        ComponentCondition<AetheronecrosisPredation>(id + 0x32, 4, comp => comp.NumCastsDualPredation > 3, "Wing/snake 4")
            .DeactivateOnExit<AetheronecrosisPredation>();
        Cast(id + 0x40, AID.PteraIxou, 3.1f, 6)
            .ActivateOnEnter<PteraIxou>(); // old statuses are removed ~0.4s before cast start
        ComponentCondition<PteraIxou>(id + 0x42, 1, comp => comp.NumCasts > 0, "Sides")
            .DeactivateOnExit<PteraIxou>();
    }

    private void CachexiaTransmissionPolyominoidPteraIxou(uint id, float delay)
    {
        Cast(id, AID.Cachexia, delay, 3)
            .ActivateOnEnter<PteraIxou>(); // activate early, since side selection is first thing we do - and boss won't rotate
        Cast(id + 0x10, AID.Transmission, 2.7f, 5)
            .ActivateOnEnter<Transmission>();
        Cast(id + 0x20, AID.AetherialExchange, 4.6f, 3);
        Cast(id + 0x30, AID.PolyominoidSigma, 2.7f, 4)
            .ActivateOnEnter<Polyominoid>();
        CastStart(id + 0x40, AID.PteraIxou, 7.5f);
        ComponentCondition<Transmission>(id + 0x41, 5.2f, comp => comp.StunsActive, "Parasite stun")
            .ActivateOnEnter<PteraIxouSpreadStack>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        CastEnd(id + 0x42, 0.8f, "Cells resolve + Spread/stack")
            .DeactivateOnExit<PteraIxouSpreadStack>()
            .DeactivateOnExit<Polyominoid>();
        ComponentCondition<PteraIxou>(id + 0x43, 1, comp => comp.NumCasts > 0)
            .DeactivateOnExit<PteraIxou>();
        ComponentCondition<Transmission>(id + 0x44, 0.2f, comp => !comp.StunsActive, "Sides")
            .DeactivateOnExit<Transmission>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }
}
