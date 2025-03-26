namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

class Ex1ZodiarkStates : StateMachineBuilder
{
    public Ex1ZodiarkStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID.Kokytos, 6.1f, 4, "Kokytos");
        Paradeigma1(id + 0x010000, 7.2f);
        Ania(id + 0x020000, 2.7f);
        Exoterikos1(id + 0x030000, 4.1f);
        Paradeigma2(id + 0x040000, 9.2f);
        Phobos(id + 0x050000, 7.2f);
        Paradeigma3(id + 0x060000, 7.2f);
        Ania(id + 0x070000, 3);
        Paradeigma4(id + 0x080000, 3.2f);

        Intermission(id + 0x100000, 9.5f);
        AstralEclipse(id + 0x110000, 6.1f, true);

        Paradeigma5(id + 0x200000, 10.1f);
        Ania(id + 0x210000, 9);
        Exoterikos4(id + 0x220000, 6.2f);
        Paradeigma6(id + 0x230000, 10.2f);
        TrimorphosExoterikos(id + 0x240000, 0.6f, true);
        AstralEclipse(id + 0x250000, 8.5f, false);

        Ania(id + 0x300000, 7.2f);
        Paradeigma7(id + 0x310000, 6.2f);
        Exoterikos6(id + 0x320000, 2.5f);
        Paradeigma8(id + 0x330000, 5.1f);
        Phobos(id + 0x340000, 4.9f);
        TrimorphosExoterikos(id + 0x350000, 10.2f, false);
        Styx(id + 0x360000, 3.2f, 9);
        Paradeigma9(id + 0x370000, 0.4f);
        Cast(id + 0x380000, AID.Enrage, 3.5f, 8, "Enrage");
    }

    private void Ania(uint id, float delay)
    {
        Cast(id, AID.Ania, delay, 4)
            .ActivateOnEnter<Ania>();
        ComponentCondition<Ania>(id + 2, 1, comp => comp.Done, "Tankbuster")
            .DeactivateOnExit<Ania>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Phobos(uint id, float delay)
    {
        Cast(id, AID.Phobos, delay, 4, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State Algedon(uint id, float delay, bool setPosFlags)
    {
        CastStartMulti(id, [AID.AlgedonTL, AID.AlgedonTR], delay)
            .SetHint(StateMachine.StateHint.PositioningStart, setPosFlags);
        CastEnd(id + 1, 7)
            .ActivateOnEnter<Algedon>();
        return ComponentCondition<Algedon>(id + 2, 1, comp => comp.Done, "Diagonal")
            .DeactivateOnExit<Algedon>()
            .SetHint(StateMachine.StateHint.PositioningEnd, setPosFlags);
    }

    private State Adikia(uint id, float delay)
    {
        Cast(id, AID.Adikia, delay, 6)
            .ActivateOnEnter<Adikia>();
        return ComponentCondition<Adikia>(id + 0x10, 1.7f, comp => comp.Done, "SideSmash")
            .DeactivateOnExit<Adikia>();
    }

    private State Styx(uint id, float delay, int numHits)
    {
        CastStart(id, AID.Styx, delay)
            .ActivateOnEnter<Styx>();
        CastEnd(id + 1, 5, "Stack");
        return ComponentCondition<Styx>(id + 0x10, 1.1f * numHits - 0.1f, comp => comp.NumCasts >= numHits, "Stack resolve", 2)
            .DeactivateOnExit<Styx>();
    }

    // note that exoterikos component is optionally activated, but unconditionally deactivated
    private State TripleEsotericRay(uint id, float delay, bool startExo, bool setPosFlags)
    {
        Cast(id, AID.TripleEsotericRay, delay, 7, "TripleRay")
            .ActivateOnEnter<Exoterikos>(startExo)
            .SetHint(StateMachine.StateHint.PositioningStart, setPosFlags);
        return ComponentCondition<Exoterikos>(id + 0x10, 3.1f, comp => comp.Done, "TripleRay resolve")
            .DeactivateOnExit<Exoterikos>()
            .SetHint(StateMachine.StateHint.PositioningEnd, setPosFlags);
    }

    // this is used by various paradeigma states; the state activates component
    private void ParadeigmaStart(uint id, float delay, string name)
    {
        Cast(id, AID.Paradeigma, delay, 3, name)
            .ActivateOnEnter<Paradeigma>();
    }

    // this is used by various paradeigma states; automatically deactivates paradeigma component
    private State AstralFlow(uint id, float delay)
    {
        CastStartMulti(id, [AID.AstralFlowCW, AID.AstralFlowCCW], delay);
        CastEnd(id + 1, 10, "Rotate")
            .SetHint(StateMachine.StateHint.PositioningStart);
        return Condition(id + 0x10, 6.2f, () => Module.WorldState.Party.WithoutSlot().All(a => (a.FindStatus(SID.TenebrousGrasp) == null)), "Rotate resolve", 5, 1)
            .DeactivateOnExit<Paradeigma>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    // this is used by various exoterikos states; the state activates component
    private void ExoterikosStart(uint id, float delay, string name)
    {
        Cast(id, AID.ExoterikosGeneric, delay, 5, name)
            .ActivateOnEnter<Exoterikos>();
    }

    private void Paradeigma1(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para1 (4 birds)");
        Styx(id + 0x1000, 11.2f, 6)
            .OnEnter(Module.DeactivateComponent<Paradeigma>); // TODO: paradeigma should hide itself when done, then we can deactivate it on state exit...
    }

    private void Exoterikos1(uint id, float delay)
    {
        ExoterikosStart(id, delay, "Exo1 (side tri)");
        Cast(id + 0x1000, AID.ExoterikosFront, 2.1f, 7, "Exo2 (front)")
            .DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma2(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para2 (birds/behemoths)");
        Algedon(id + 0x1000, 5.2f, true)
            .DeactivateOnExit<Paradeigma>();
    }

    private void Paradeigma3(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para3 (snakes)");
        ExoterikosStart(id + 0x1000, 2.1f, "Exo3 (side)");
        AstralFlow(id + 0x2000, 2.2f)
            .DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma4(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para4 (snakes side)");
        Adikia(id + 0x1000, 4.1f)
            .DeactivateOnExit<Paradeigma>();
    }

    private void Paradeigma5(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para5 (birds/behemoths)");
        AstralFlow(id + 0x1000, 5.2f);
    }

    private void Exoterikos4(uint id, float delay)
    {
        ExoterikosStart(id, delay, "Exo4 (side sq)");
        Algedon(id + 0x1000, 2.1f, true)
            .DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma6(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para6 (4 birds + snakes)");
        AstralFlow(id + 0x1000, 5.2f);
        Styx(id + 0x2000, 0, 7); // note: cast starts slightly before flow resolve
    }

    private void TrimorphosExoterikos(uint id, float delay, bool first)
    {
        Cast(id, AID.TrimorphosExoterikos, delay, 13, "TriExo")
            .ActivateOnEnter<Exoterikos>();

        var followup = first ? Adikia(id + 0x1000, 6.2f) : Algedon(id + 0x1000, 5.2f, true);
        followup.DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma7(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para7 (snakes)");
        ExoterikosStart(id + 0x1000, 2.1f, "Exo5 (side)");
        AstralFlow(id + 0x2000, 2.2f)
            .DeactivateOnExit<Exoterikos>();
        Cast(id + 0x3000, AID.Phlegeton, 0, 2.9f, "Puddles") // note: 3s cast starts ~0.1s before flow resolve...
            .ActivateOnEnter<Phlegethon>();
        Styx(id + 0x4000, 2.2f, 8)
            .DeactivateOnExit<Phlegethon>(); // resolve happens mid cast
    }

    private void Exoterikos6(uint id, float delay)
    {
        ExoterikosStart(id, delay, "Exo6 (side)");
        TripleEsotericRay(id + 0x1000, 2.1f, false, true);
    }

    private void Paradeigma8(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para8 (birds/behemoths)");
        ExoterikosStart(id + 0x1000, 2.1f, "Exo7 (back sq)");
        AstralFlow(id + 0x2000, 2.1f)
            .DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma9(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para9 (4 birds + snakes)");
        ExoterikosStart(id + 0x1000, 2.1f, "Exo8 (side/back?)");
        AstralFlow(id + 0x2000, 2.1f)
            .DeactivateOnExit<Exoterikos>();
        Styx(id + 0x3000, 0, 9); // note: cast starts right as flow resolve happens
    }

    private void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Intermission start")
            .ClearHint(StateMachine.StateHint.DowntimeStart); // adds appear almost immediately, so there is no downtime
        CastStartMulti(id + 0x1000, [AID.AddsEndFail, AID.AddsEndSuccess], 40, "Add enrage")
            .ActivateOnEnter<Exoterikos>()
            .DeactivateOnExit<Exoterikos>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        CastEnd(id + 0x2000, 1.1f);
        ComponentCondition<Apomnemoneumata>(id + 0x3000, 11.5f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Apomnemoneumata>()
            .DeactivateOnExit<Apomnemoneumata>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x4000, true, 10.6f, "Intermission end");
    }

    private void AstralEclipse(uint id, float delay, bool first)
    {
        Cast(id, AID.AstralEclipse, delay, 5, "Eclipse")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Targetable(id + 0x1000, true, 12.1f, "Boss reappear", 1)
            .ActivateOnEnter<AstralEclipse>()
            .SetHint(StateMachine.StateHint.PositioningStart);

        //  5.1s first explosion
        //  8.2s triple ray cast start
        //  9.2s second explosion
        // 13.2s third explosion
        // 15.2s triple ray cast end
        // 15.3s ray 1
        // 18.3s ray 2
        // -or-
        //  5.1s first explosion
        //  9.2s second explosion
        // 10.7s algedon cast start
        // 13.2s third explosion
        // 17.7s algedon cast end
        // 18.7s algedon aoe
        var followup = first ? TripleEsotericRay(id + 0x2000, 8.2f, true, false) : Algedon(id + 0x2000, 10.6f, false);
        followup.DeactivateOnExit<AstralEclipse>();
        followup.SetHint(StateMachine.StateHint.PositioningEnd);
    }
}
