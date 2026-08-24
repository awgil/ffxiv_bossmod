namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

class Ex3EndsingerStates : StateMachineBuilder
{
    public Ex3EndsingerStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ElegeiaUnforgotten(id, 6.2f);
        ElegeiaUnforgotten(id + 0x10000, 9.9f, false);
        KatasterismoiGripElenchos(id + 0x20000, 10.9f);
        Telos(id + 0x30000, 4.1f);
        Hubris(id + 0x40000, 5.1f);

        ElegeiaUnforgotten(id + 0x100000, 6.2f);
        Eironeia(id + 0x110000, 8.9f);
        ElegeiaFatalism(id + 0x120000, 4.1f);

        TwinsongAporrhoia(id + 0x200000, 6.2f);
        Hubris(id + 0x210000, 4.2f);
        DespairUnforgotten(id + 0x220000, 6.2f);
        Telomania(id + 0x230000, 2);
        EndsongAporrhoia(id + 0x240000, 11);
        Telos(id + 0x250000, 4.2f);
        Hubris(id + 0x260000, 5.2f);

        ElegeiaUnforgotten(id + 0x300000, 8.3f);
        Eironeia(id + 0x310000, 8.9f);
        ElegeiaDoubleFatalism(id + 0x320000, 2.1f, false);

        Telomania(id + 0x400000, 4.1f);
        TwinsongAporrhoia(id + 0x410000, 10.9f);
        Hubris(id + 0x420000, 4.2f);
        EndsongAporrhoia(id + 0x430000, 6.2f);
        Telos(id + 0x440000, 4.2f);
        ElegeiaDoubleFatalism(id + 0x450000, 10.2f, true);

        Enrage(id + 0x500000, 9.3f);
    }

    private State Telos(uint id, float delay)
    {
        return Cast(id, AID.Telos, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Hubris(uint id, float delay)
    {
        CastStart(id, AID.Hubris, delay);
        CastEnd(id + 1, 5)
            .ActivateOnEnter<Hubris>();
        ComponentCondition<Hubris>(id + 2, 1, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<Hubris>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private State Elenchos(uint id, float delay)
    {
        return CastMulti(id, [AID.ElenchosCenter, AID.ElenchosSides], delay, 6, "Elenchos")
            .ActivateOnEnter<Elenchos>()
            .DeactivateOnExit<Elenchos>();
    }

    private void Telomania(uint id, float delay)
    {
        Cast(id, AID.Telomania, delay, 5);
        ComponentCondition<Telomania>(id + 0x10, 10.2f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Telomania>()
            .DeactivateOnExit<Telomania>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    // note: resolve typically overlaps with next mechanic, so we always leave Planets component active
    // if this is a second cast, we assume that component already active and we don't need to activate it again
    private void ElegeiaUnforgotten(uint id, float delay, bool first = true)
    {
        Cast(id, AID.ElegeiaUnforgotten, delay, 5);
        ComponentCondition<Elegeia>(id + 0x10, 3.3f, comp => comp.NumCasts > 0, "Planets")
            .ActivateOnEnter<Elegeia>()
            .ActivateOnEnter<Planets>(first)
            .DeactivateOnExit<Elegeia>()
            .SetHint(StateMachine.StateHint.Raidwide);
        // resolve happens ~11.6s after this
    }

    private void ElegeiaFatalism(uint id, float delay)
    {
        Cast(id, AID.Fatalism, delay, 3, "Double Planets")
            .ActivateOnEnter<Planets>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        // resolve happens ~19s after this
        Elenchos(id + 0x100, 17.2f)
            .DeactivateOnExit<Planets>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ElegeiaDoubleFatalism(uint id, float delay, bool withTelos)
    {
        Cast(id, AID.Fatalism, delay, 3, "Quad Planets")
            .ActivateOnEnter<Planets>();
        Cast(id + 0x1000, AID.Fatalism, 7.2f, 3)
            .SetHint(StateMachine.StateHint.PositioningStart);

        if (withTelos)
        {
            Telos(id + 0x2000, 19.3f)
                .DeactivateOnExit<Planets>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }
        else
        {
            // TODO: components
            Cast(id + 0x2000, AID.Katasterismoi, 11.2f, 3, "Towers");
            Elenchos(id + 0x3000, 6.2f)
                .DeactivateOnExit<Planets>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }
    }

    // note: assumes that Planets component is active when first state is entered and resolve happens mid-cast
    private void KatasterismoiGripElenchos(uint id, float delay)
    {
        // TODO: components...
        Cast(id, AID.Katasterismoi, delay, 3, "Towers")
            .DeactivateOnExit<Planets>();
        Cast(id + 0x1000, AID.GripOfDespair, 3.2f, 5, "Tethers");
        Elenchos(id + 0x2000, 4.1f);
    }

    // note: assumes that Planets component is active when first state is entered and resolve happens mid-cast
    private void Eironeia(uint id, float delay)
    {
        CastStart(id, AID.Eironeia, delay)
            .DeactivateOnExit<Planets>();
        CastEnd(id + 1, 5)
            .ActivateOnEnter<Eironeia>();
        ComponentCondition<Eironeia>(id + 2, 1, comp => comp.NumCasts > 0, "Party stacks")
            .DeactivateOnExit<Eironeia>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State AporrhoiaUnforgotten(uint id, float delay)
    {
        CastStart(id, AID.AporrhoiaUnforgotten, delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        return CastEnd(id + 1, 5)
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void TwinsongAporrhoia(uint id, float delay)
    {
        Cast(id, AID.TwinsongAporrhoia, delay, 3, "Twinsong");
        AporrhoiaUnforgotten(id + 0x100, 6.2f)
            .ActivateOnEnter<TwinsongAporrhoia>();
        AporrhoiaUnforgotten(id + 0x200, 4.2f);
        AporrhoiaUnforgotten(id + 0x300, 4.2f);
        Cast(id + 0x400, AID.TheologicalFatalism, 5.2f, 3, "Rewind")
            .SetHint(StateMachine.StateHint.PositioningStart);
        Elenchos(id + 0x500, 7.2f)
            .ActivateOnEnter<Eironeia>()
            .DeactivateOnExit<TwinsongAporrhoia>()
            .DeactivateOnExit<Eironeia>() // note: no separate state, since it's almost immediate
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private void DespairUnforgotten(uint id, float delay)
    {
        Cast(id, AID.DespairUnforgotten, delay, 3, "Despair 1")
            .ActivateOnEnter<DespairUnforgotten>();
        Cast(id + 0x100, AID.DespairUnforgotten, 10.2f, 3, "Despair 2");
        Elenchos(id + 0x200, 6.2f);
        Cast(id + 0x300, AID.DespairUnforgotten, 5.2f, 3, "Despair 3");
        Cast(id + 0x400, AID.TheologicalFatalism, 11.2f, 3, "Rewind");
        ComponentCondition<DespairUnforgotten>(id + 0x500, 10.1f, comp => comp.Done, "Despair resolve")
            .DeactivateOnExit<DespairUnforgotten>();
    }

    private void EndsongAporrhoia(uint id, float delay)
    {
        Cast(id, AID.EndsongAporrhoia, delay, 3, "Endsong");
        Cast(id + 0x100, AID.Endsong, 5.2f, 3)
            .ActivateOnEnter<Endsong>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Elenchos(id + 0x200, 26.2f)
            .DeactivateOnExit<Endsong>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.Enrage, 9.3f, 5);
        ComponentCondition<UltimateFate>(id + 2, 27.4f, comp => comp.NumCasts > 0, "Enrage")
            .ActivateOnEnter<UltimateFate>()
            .DeactivateOnExit<UltimateFate>();
    }
}
