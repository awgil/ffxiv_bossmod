namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko;

class C023MokoStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C023MokoStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        KenkiRelease(id, 10.2f);
        TripleKasumiGiri(id + 0x10000, 7.2f);
        TripleKasumiGiri(id + 0x20000, 2.2f);
        LateralSlice(id + 0x30000, 4.3f);
        ScarletAuspice(id + 0x40000, 7.4f);
        KenkiRelease(id + 0x50000, 3.1f);
        ShadowTwin(id + 0x60000, 8.5f);
        KenkiRelease(id + 0x70000, 0.1f);
        AzureAuspice(id + 0x80000, 9.4f);
        KenkiRelease(id + 0x90000, 2.2f);
        SoldiersOfDeath(id + 0xA0000, 9.6f);
        TripleKasumiGiri(id + 0xB0000, 1.5f);
        KenkiRelease(id + 0xC0000, 2.1f);
        LateralSlice(id + 0xD0000, 5.3f);
        KenkiRelease(id + 0xE0000, 3.1f);
        Cast(id + 0xF0000, AID.Enrage, 5.3f, 10, "Enrage");
    }

    private void KenkiRelease(uint id, float delay)
    {
        Cast(id, _savage ? AID.SKenkiRelease : AID.NKenkiRelease, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void LateralSlice(uint id, float delay)
    {
        Cast(id, _savage ? AID.SLateralSlice : AID.NLateralSlice, delay, 5)
            .ActivateOnEnter<NLateralSlice>(!_savage)
            .ActivateOnEnter<SLateralSlice>(_savage);
        ComponentCondition<LateralSlice>(id + 2, 0.2f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<LateralSlice>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private State TripleKasumiGiri(uint id, float delay)
    {
        AID[] firstCasts = _savage
            ? [AID.STripleKasumiGiriOutFrontFirst, AID.STripleKasumiGiriOutRightFirst, AID.STripleKasumiGiriOutBackFirst, AID.STripleKasumiGiriOutLeftFirst, AID.STripleKasumiGiriInFrontFirst, AID.STripleKasumiGiriInRightFirst, AID.STripleKasumiGiriInBackFirst, AID.STripleKasumiGiriInLeftFirst]
            : [AID.NTripleKasumiGiriOutFrontFirst, AID.NTripleKasumiGiriOutRightFirst, AID.NTripleKasumiGiriOutBackFirst, AID.NTripleKasumiGiriOutLeftFirst, AID.NTripleKasumiGiriInFrontFirst, AID.NTripleKasumiGiriInRightFirst, AID.NTripleKasumiGiriInBackFirst, AID.NTripleKasumiGiriInLeftFirst];
        AID[] restCasts = _savage
            ? [AID.STripleKasumiGiriOutFrontRest, AID.STripleKasumiGiriOutRightRest, AID.STripleKasumiGiriOutBackRest, AID.STripleKasumiGiriOutLeftRest, AID.STripleKasumiGiriInFrontRest, AID.STripleKasumiGiriInRightRest, AID.STripleKasumiGiriInBackRest, AID.STripleKasumiGiriInLeftRest]
            : [AID.NTripleKasumiGiriOutFrontRest, AID.NTripleKasumiGiriOutRightRest, AID.NTripleKasumiGiriOutBackRest, AID.NTripleKasumiGiriOutLeftRest, AID.NTripleKasumiGiriInFrontRest, AID.NTripleKasumiGiriInRightRest, AID.NTripleKasumiGiriInBackRest, AID.NTripleKasumiGiriInLeftRest];
        CastMulti(id, firstCasts, delay, 12, "Cleave 1")
            .ActivateOnEnter<TripleKasumiGiri>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastMulti(id + 0x10, restCasts, 2.1f, 1, "Cleave 2");
        return CastMulti(id + 0x20, restCasts, 2.1f, 1, "Cleave 3")
            .DeactivateOnExit<TripleKasumiGiri>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ScarletAuspice(uint id, float delay)
    {
        Cast(id, _savage ? AID.SScarletAuspice : AID.NScarletAuspice, delay, 5, "Out")
            .ActivateOnEnter<NScarletAuspice>(!_savage)
            .ActivateOnEnter<SScarletAuspice>(_savage)
            .DeactivateOnExit<ScarletAuspice>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x10, AID.BoundlessScarlet, 3.2f, 2.4f)
            .ActivateOnEnter<NBoundlessScarletFirst>(!_savage)
            .ActivateOnEnter<SBoundlessScarletFirst>(_savage);
        ComponentCondition<BoundlessScarletFirst>(id + 0x12, 0.6f, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<BoundlessScarletFirst>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        Cast(id + 0x20, AID.InvocationOfVengeance, 3.5f, 3)
            .ActivateOnEnter<InvocationOfVengeance>(); // +0.8s: stack/spread debuffs
        Cast(id + 0x30, AID.FleetingIaiGiri, 3.2f, 9, "Jump")
            .ActivateOnEnter<FleetingIaiGiriBait>()
            .ActivateOnEnter<IaiGiriResolve>()
            .ActivateOnEnter<NBoundlessScarletRest>(!_savage) // first pair starts 0.6 into cast, pairs 7s apart
            .ActivateOnEnter<SBoundlessScarletRest>(_savage)
            .DeactivateOnExit<FleetingIaiGiriBait>();
        CastMulti(id + 0x40, new[] { AID.NFleetingIaiGiriFront, AID.NFleetingIaiGiriRight, AID.NFleetingIaiGiriLeft, AID.SFleetingIaiGiriFront, AID.SFleetingIaiGiriRight, AID.SFleetingIaiGiriLeft }, 1.6f, 1, "Cleave")
            .DeactivateOnExit<IaiGiriResolve>();
        ComponentCondition<InvocationOfVengeance>(id + 0x50, 1.2f, comp => comp.NumMechanics > 0, "Stack/spread"); // first pair of explosions happen right before this
        ComponentCondition<BoundlessScarletRest>(id + 0x60, 6.8f, comp => comp.Casters.Count == 0, "Lines resolve")
            .DeactivateOnExit<BoundlessScarletRest>();
        ComponentCondition<InvocationOfVengeance>(id + 0x70, 1.1f, comp => comp.NumMechanics > 1, "Spread/stack")
            .DeactivateOnExit<InvocationOfVengeance>();
    }

    private void AzureAuspice(uint id, float delay)
    {
        Cast(id, _savage ? AID.SAzureAuspice : AID.NAzureAuspice, delay, 5, "In")
            .ActivateOnEnter<NAzureAuspice>(!_savage)
            .ActivateOnEnter<SAzureAuspice>(_savage)
            .DeactivateOnExit<AzureAuspice>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x10, AID.BoundlessAzure, 3.2f, 2.4f)
            .ActivateOnEnter<NBoundlessAzure>(!_savage)
            .ActivateOnEnter<SBoundlessAzure>(_savage);
        ComponentCondition<BoundlessAzure>(id + 0x12, 0.6f, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<BoundlessAzure>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        ComponentCondition<Upwell>(id + 0x20, 11.0f, comp => comp.NumCasts > 0, "Expanding start")
            .ActivateOnEnter<Upwell>();

        TripleKasumiGiri(id + 0x1000, 7.8f)
            .DeactivateOnExit<Upwell>();
    }

    private void ShadowTwin(uint id, float delay)
    {
        Cast(id, AID.ShadowTwin, delay, 3);
        // +0.8s: PATE 1E43 on 2 shadows
        Cast(id + 0x10, _savage ? AID.SMoonlessNight : AID.NMoonlessNight, 3.1f, 3, "Raidwide")
            .ActivateOnEnter<DoubleIaiGiriBait>() // first statuses appear 0.1s after cast start
            .ActivateOnEnter<IaiGiriResolve>()
            .ActivateOnEnter<Clearout>() // PATEs 1.0s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x20, false, 3.1f, "Boss disappears"); // around here we get second statuses
        CastMulti(id + 0x30, new[] { AID.FarEdge, AID.NearEdge }, 0.1f, 6)
            .ActivateOnEnter<AccursedEdge>();
        ComponentCondition<AccursedEdge>(id + 0x32, 0.1f, comp => comp.NumCasts > 0, "Jump/bait 1") // also first clearout
            .DeactivateOnExit<AccursedEdge>();
        ComponentCondition<IaiGiriResolve>(id + 0x40, 2.8f, comp => comp.NumCasts >= 2, "Cleave back")
            .DeactivateOnExit<DoubleIaiGiriBait>(); // note: this could be deactivated early, but we want to make sure resolve components picks up everything
        ComponentCondition<IaiGiriResolve>(id + 0x50, 3.1f, comp => comp.NumCasts >= 4, "Cleave 2")
            .DeactivateOnExit<Clearout>() // TODO: last aoe ~1.2s before?..
            .DeactivateOnExit<IaiGiriResolve>();

        Cast(id + 0x100, _savage ? AID.SMoonlessNight : AID.NMoonlessNight, 2.5f, 3, "Raidwide")
            .ActivateOnEnter<DoubleIaiGiriBait>() // first statuses appear 0.3s after cast start
            .ActivateOnEnter<IaiGiriResolve>()
            .ActivateOnEnter<Clearout>() // PATEs 1.0s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x110, new[] { AID.FarEdge, AID.NearEdge }, 3.2f, 6)
            .ActivateOnEnter<AccursedEdge>();
        ComponentCondition<AccursedEdge>(id + 0x112, 0.1f, comp => comp.NumCasts > 0, "Jump/bait 2") // also first clearout
           .DeactivateOnExit<AccursedEdge>();
        ComponentCondition<IaiGiriResolve>(id + 0x120, 2.8f, comp => comp.NumCasts >= 2, "Cleave back")
            .DeactivateOnExit<DoubleIaiGiriBait>(); // note: this could be deactivated early, but we want to make sure resolve components picks up everything
        ComponentCondition<IaiGiriResolve>(id + 0x130, 3.1f, comp => comp.NumCasts >= 4, "Cleave 2")
            .DeactivateOnExit<Clearout>() // TODO: last aoe ~1.2s before?..
            .DeactivateOnExit<IaiGiriResolve>();

        Targetable(id + 0x200, true, 2.2f, "Boss reappears");
    }

    private void SoldiersOfDeath(uint id, float delay)
    {
        Cast(id, AID.SoldiersOfDeath, delay, 3);
        Cast(id + 0x10, AID.ShadowTwin, 3.2f, 3);
        ComponentCondition<IronRainStorm>(id + 0x20, 0.9f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<DoubleIaiGiriBait>() // casts start 2.4s after iron rain/storm, but we activate earlier, so that rain/storm component can provide hints
            .ActivateOnEnter<IronRainStorm>();
        ComponentCondition<IronRainStorm>(id + 0x30, 15, comp => comp.NumCasts > 0, "Jumps + AOEs")
            .ActivateOnEnter<IaiGiriResolve>()
            .DeactivateOnExit<DoubleIaiGiriBait>(); // baits happen ~0.6s before aoe
        ComponentCondition<IaiGiriResolve>(id + 0x40, 2.3f, comp => comp.NumCasts > 0, "Cleave back");
        ComponentCondition<IaiGiriResolve>(id + 0x50, 3.1f, comp => comp.NumCasts > 4, "Cleave sides")
            .DeactivateOnExit<IaiGiriResolve>();
        ComponentCondition<IronRainStorm>(id + 0x60, 0.7f, comp => comp.NumCasts > 5, "AOE resolve")
            .DeactivateOnExit<IronRainStorm>();
    }
}

class C023NMokoStates(BossModule module) : C023MokoStates(module, false);
class C023SMokoStates(BossModule module) : C023MokoStates(module, true);
