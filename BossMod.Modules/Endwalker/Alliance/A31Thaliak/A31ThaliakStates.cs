namespace BossMod.Endwalker.Alliance.A31Thaliak;

class A31ThaliakStates : StateMachineBuilder
{
    public A31ThaliakStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Katarraktes(id, 5.2f);
        Rheognosis(id + 0x10000, 7.8f);
        Thlipsis(id + 0x20000, 3.9f);
        LeftRightBank(id + 0x30000, 8.5f);
        LeftRightBank(id + 0x40000, 2.1f);
        Hydroptosis(id + 0x50000, 2.1f);
        Rhyton(id + 0x60000, 6.2f);
        Tetraktys1(id + 0x70000, 8.7f);
        RheognosisPetrine(id + 0x80000, 8.4f);
        Hieroglyphica(id + 0x90000, 6.5f);
        HieroglyphicaLeftRightBank(id + 0xA0000, 4.4f);
        Tetraktys2(id + 0xB0000, 8.4f);
        Rhyton(id + 0xC0000, 6.1f);
        RheognosisPetrine(id + 0xD0000, 7.6f);
        Thlipsis(id + 0xE0000, 2.1f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Katarraktes(uint id, float delay)
    {
        Cast(id, AID.Katarraktes, delay, 5);
        ComponentCondition<Katarraktes>(id + 2, 0.7f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Katarraktes>()
            .DeactivateOnExit<Katarraktes>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Rheognosis(uint id, float delay)
    {
        Cast(id, AID.Rheognosis, delay, 5)
            .ActivateOnEnter<RheognosisKnockback>();
        ComponentCondition<RheognosisKnockback>(id + 0x10, 20.3f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<RheognosisKnockback>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RheognosisPetrine(uint id, float delay)
    {
        Cast(id, AID.RheognosisPetrine, delay, 5)
            .ActivateOnEnter<RheognosisKnockback>();
        ComponentCondition<RheognosisKnockback>(id + 0x10, 20.3f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<RheognosisCrash>()
            .DeactivateOnExit<RheognosisKnockback>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RheognosisCrash>(id + 0x20, 1.8f, comp => comp.NumCasts >= 5, "Half-arena cleave")
            .DeactivateOnExit<RheognosisCrash>();
    }

    private void Thlipsis(uint id, float delay)
    {
        Cast(id, AID.Thlipsis, delay, 4)
            .ActivateOnEnter<Thlipsis>();
        ComponentCondition<Thlipsis>(id + 2, 2, comp => comp.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<Thlipsis>();
    }

    private void LeftRightBank(uint id, float delay)
    {
        CastMulti(id, [AID.LeftBank, AID.RightBank], delay, 5, "Half-arena cleave")
            .ActivateOnEnter<LeftBank>()
            .ActivateOnEnter<RightBank>()
            .DeactivateOnExit<LeftBank>()
            .DeactivateOnExit<RightBank>();
    }

    private void Hydroptosis(uint id, float delay)
    {
        Cast(id, AID.Hydroptosis, delay, 4)
            .ActivateOnEnter<Hydroptosis>();
        ComponentCondition<Hydroptosis>(id + 2, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Hydroptosis>();
    }

    private void Rhyton(uint id, float delay)
    {
        CastStart(id, AID.Rhyton, delay)
            .ActivateOnEnter<Rhyton>();
        CastEnd(id + 1, 5);
        ComponentCondition<Rhyton>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<Rhyton>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Tetraktys1(uint id, float delay)
    {
        CastStart(id, AID.Tetraktys, delay)
            .ActivateOnEnter<TetraktysBorder>(); // telegraph appears ~0.1s before cast start
        CastEnd(id + 1, 6);
        ComponentCondition<TetraktysBorder>(id + 2, 0.4f, comp => comp.Active, "Triangles start")
            .OnExit(() => Module.Arena.Bounds = A31Thaliak.TriBounds);
        ComponentCondition<Tetraktys>(id + 0x10, 3.6f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<Tetraktys>();
        ComponentCondition<Tetraktys>(id + 0x11, 3.9f, comp => comp.NumCasts >= 3, "Small tri 1");
        ComponentCondition<Tetraktys>(id + 0x20, 2.5f, comp => comp.NumCasts >= 6, "Small tri 2");
        ComponentCondition<Tetraktys>(id + 0x30, 2.5f, comp => comp.NumCasts >= 9, "Small tri 3");
        ComponentCondition<Tetraktys>(id + 0x40, 2.5f, comp => comp.NumCasts >= 10, "Large tri 1");
        ComponentCondition<Tetraktys>(id + 0x50, 2.5f, comp => comp.NumCasts >= 11, "Large tri 2");
        ComponentCondition<Tetraktys>(id + 0x60, 2.5f, comp => comp.NumCasts >= 12, "Large tri 3")
            .DeactivateOnExit<Tetraktys>();

        Cast(id + 0x100, AID.TetraktuosKosmos, 1.7f, 4);
        ComponentCondition<TetraktuosKosmos>(id + 0x110, 0.8f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<TetraktuosKosmos>();
        CastStart(id + 0x120, AID.TetraktuosKosmos, 6.3f);
        ComponentCondition<TetraktuosKosmos>(id + 0x121, 1.7f, comp => comp.NumCasts >= 1, "Splitting tri 1");
        CastEnd(id + 0x122, 2.3f);
        ComponentCondition<TetraktuosKosmos>(id + 0x130, 0.8f, comp => comp.AOEs.Count > 0);
        ComponentCondition<TetraktuosKosmos>(id + 0x140, 8, comp => comp.NumCasts >= 3, "Splitting tri 2+3")
            .DeactivateOnExit<TetraktuosKosmos>();

        ComponentCondition<TetraktysBorder>(id + 0x200, 4.2f, comp => !comp.Active, "Triangles resolve")
            .DeactivateOnExit<TetraktysBorder>()
            .OnExit(() => Module.Arena.Bounds = A31Thaliak.NormalBounds);
    }

    private void Tetraktys2(uint id, float delay)
    {
        CastStart(id, AID.Tetraktys, delay)
            .ActivateOnEnter<TetraktysBorder>(); // telegraph appears ~0.1s before cast start
        CastEnd(id + 1, 6);
        ComponentCondition<TetraktysBorder>(id + 2, 0.4f, comp => comp.Active, "Triangles start")
            .OnExit(() => Module.Arena.Bounds = A31Thaliak.TriBounds);
        ComponentCondition<Tetraktys>(id + 0x10, 3.6f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<Tetraktys>();
        ComponentCondition<Tetraktys>(id + 0x11, 3.9f, comp => comp.NumCasts >= 3, "Small tri 1");
        ComponentCondition<Tetraktys>(id + 0x20, 2.5f, comp => comp.NumCasts >= 6, "Small tri 2");
        ComponentCondition<Tetraktys>(id + 0x30, 2.5f, comp => comp.NumCasts >= 9, "Small tri 3");
        ComponentCondition<Tetraktys>(id + 0x40, 2.5f, comp => comp.NumCasts >= 10, "Large tri 1");
        ComponentCondition<Tetraktys>(id + 0x50, 2.5f, comp => comp.NumCasts >= 11, "Large tri 2");
        CastStart(id + 0x60, AID.TetraktuosKosmos, 2.2f);
        ComponentCondition<Tetraktys>(id + 0x61, 0.3f, comp => comp.NumCasts >= 12, "Large tri 3")
            .DeactivateOnExit<Tetraktys>();

        CastEnd(id + 0x100, 3.7f);
        ComponentCondition<TetraktuosKosmos>(id + 0x110, 0.8f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<TetraktuosKosmos>();
        CastStart(id + 0x120, AID.TetraktuosKosmos, 6.3f);
        ComponentCondition<TetraktuosKosmos>(id + 0x121, 1.7f, comp => comp.NumCasts >= 2, "Splitting tri 1+2");
        CastEnd(id + 0x122, 2.3f);
        ComponentCondition<TetraktuosKosmos>(id + 0x130, 0.8f, comp => comp.AOEs.Count > 0);
        ComponentCondition<TetraktuosKosmos>(id + 0x140, 8, comp => comp.NumCasts >= 4, "Splitting tri 3+4")
            .DeactivateOnExit<TetraktuosKosmos>();

        ComponentCondition<TetraktysBorder>(id + 0x200, 3.2f, comp => !comp.Active, "Triangles resolve")
            .DeactivateOnExit<TetraktysBorder>()
            .OnExit(() => Module.Arena.Bounds = A31Thaliak.NormalBounds);
    }

    private void Hieroglyphica(uint id, float delay)
    {
        Cast(id, AID.Hieroglyphika, delay, 5);
        ComponentCondition<Hieroglyphika>(id + 0x10, 0.9f, comp => comp.SafeSideDir != default)
            .ActivateOnEnter<Hieroglyphika>();
        ComponentCondition<Hieroglyphika>(id + 0x11, 2, comp => comp.AOEs.Count > 0);
        ComponentCondition<Hieroglyphika>(id + 0x20, 13, comp => comp.BindsAssigned, "Binds");
        ComponentCondition<Hieroglyphika>(id + 0x30, 4.1f, comp => comp.NumCasts > 0, "Squares")
            .DeactivateOnExit<Hieroglyphika>();
    }

    private void HieroglyphicaLeftRightBank(uint id, float delay)
    {
        Cast(id, AID.Hieroglyphika, delay, 5);
        ComponentCondition<Hieroglyphika>(id + 0x10, 0.9f, comp => comp.SafeSideDir != default)
            .ActivateOnEnter<Hieroglyphika>();
        CastStartMulti(id + 0x11, [AID.HieroglyphikaLeftBank, AID.HieroglyphikaRightBank], 1.2f);
        ComponentCondition<Hieroglyphika>(id + 0x12, 0.8f, comp => comp.AOEs.Count > 0);
        ComponentCondition<Hieroglyphika>(id + 0x20, 13, comp => comp.BindsAssigned, "Binds");
        ComponentCondition<Hieroglyphika>(id + 0x30, 4.1f, comp => comp.NumCasts > 0, "Squares")
            .DeactivateOnExit<Hieroglyphika>();
        CastEnd(id + 0x40, 4.1f, "Half-arena cleave")
            .ActivateOnEnter<HieroglyphikaLeftBank>() // note: we activate this only now - there's enough time to dodge from any safespot, and this simplifies staying on maxmelee
            .ActivateOnEnter<HieroglyphikaRightBank>()
            .DeactivateOnExit<HieroglyphikaLeftBank>()
            .DeactivateOnExit<HieroglyphikaRightBank>();
    }
}
