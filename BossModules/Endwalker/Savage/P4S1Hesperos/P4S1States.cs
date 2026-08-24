namespace BossMod.Endwalker.Savage.P4S1Hesperos;

class P4S1States : StateMachineBuilder
{
    public P4S1States(BossModule module) : base(module)
    {
        SimplePhase(0, SinglePhase, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
    }

    private void SinglePhase(uint id)
    {
        Decollation(id, 9.3f);
        BloodrakeBelone(id + 0x010000, 4.2f);
        Decollation(id + 0x020000, 3.4f);
        ElegantEvisceration(id + 0x030000, 4.2f);

        Pinax(id + 0x100000, 11.3f, true);
        ElegantEvisceration(id + 0x110000, 4.1f);

        VengefulElementalBelone(id + 0x200000, 4.2f);

        BeloneCoils(id + 0x300000, 8.2f);
        Decollation(id + 0x310000, 3.4f);
        ElegantEvisceration(id + 0x320000, 4.2f);

        Pinax(id + 0x400000, 11.3f, false);
        Decollation(id + 0x410000, 0); // note: cast starts ~0.2s before pinax resolve, whatever...
        Decollation(id + 0x420000, 4.2f);
        Decollation(id + 0x430000, 4.2f);
        Targetable(id + 0x440000, false, 10, "Enrage"); // checkpoint is triggered by boss becoming untargetable...
    }

    private void Decollation(uint id, float delay)
    {
        Cast(id, AID.Decollation, delay, 5, "AOE")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ElegantEvisceration(uint id, float delay)
    {
        Cast(id, AID.ElegantEvisceration, delay, 5, "Tankbuster")
            .ActivateOnEnter<ElegantEvisceration>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<ElegantEvisceration>(id + 2, 3.2f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<ElegantEvisceration>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private State InversiveChlamys(uint id, float delay)
    {
        Cast(id, AID.InversiveChlamys, delay, 7);
        return ComponentCondition<InversiveChlamys>(id + 2, 0.8f, comp => !comp.TethersActive, "Chlamys"); // explosion happens when tethers disappear, shortly after cast end
    }

    private void BloodrakeBelone(uint id, float delay)
    {
        // note: just before (~0.1s) every bloodrake cast start, its targets are tethered to boss
        // targets of first bloodrake will be killed if they are targets of chlamys tethers later
        Cast(id, AID.Bloodrake, delay, 4, "Bloodrake 1")
            .ActivateOnEnter<InversiveChlamys>();

        // this cast is pure flavour and does nothing (replaces status 2799 'Aethersucker' with status 2800 'Casting Chlamys' on boss)
        Cast(id + 0x1000, AID.AethericChlamys, 3.2f, 4);

        // targets of second bloodrake will be killed if they are targets of 'Cursed Casting' (which targets players with 'Role Call')
        Cast(id + 0x2000, AID.Bloodrake, 4.2f, 4, "Bloodrake 2")
            .ActivateOnEnter<DirectorsBelone>();

        // this cast removes status 2799 'Aethersucker' from boss
        // right after it ends, instant cast 27111 applies 'Role Call' debuffs - corresponding component handles that
        CastStart(id + 0x3000, AID.DirectorsBelone, 4.2f)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x3001, 5);

        // Cursed Casting happens right before (0.5s) chlamys resolve
        InversiveChlamys(id + 0x4000, 9.2f)
            .DeactivateOnExit<InversiveChlamys>()
            .DeactivateOnExit<DirectorsBelone>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Pinax(uint id, float delay, bool keepScene)
    {
        Cast(id, AID.SettingTheScene, delay, 4, "Scene")
            .ActivateOnEnter<SettingTheScene>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        // ~1s after cast end, we get a bunch of env controls
        CastStart(id + 0x1000, AID.Pinax, 8.2f)
            .ActivateOnEnter<PinaxUptime>()
            .DeactivateOnExit<PinaxUptime>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        CastEnd(id + 0x1001, 5, "Pinax")
            .SetHint(StateMachine.StateHint.PositioningStart);
        // timeline:
        //  0.0s pinax cast end
        //  1.0s square 1 activation: env control (.10 = 00800040), helper starts casting 27095
        //  4.0s square 2 activation: env control (.15 = 00800040), helper starts casting 27092
        //  7.0s square 1 env control (.10 = 02000001)
        // 10.0s square 2 env control (.15 = 02000001)
        //       square 1 cast finish (+ instant 27091)
        // 13.0s square 2 cast finish (+ instant 27088)
        // 14.0s square 3 activation: env control (.20 = 00800040), helper starts casting 27094
        // 20.0s square 3 env control (.20 = 02000001)
        // 23.0s square 3 cast finish (+ instant 27090)
        // 25.0s square 4 activation: env control (.05 = 00800040), helper starts casting 27093
        // 31.0s square 4 env control (.05 = 02000001)
        // 34.0s square 4 cast finish (+ instant 27089)
        ComponentCondition<Pinax>(id + 0x2000, 10.1f, comp => comp.NumFinished == 1, "Corner1")
            .ActivateOnEnter<Pinax>();
        ComponentCondition<Pinax>(id + 0x3000, 3, comp => comp.NumFinished == 2, "Corner2")
            .SetHint(StateMachine.StateHint.PositioningEnd);
        CastStartMulti(id + 0x4000, [AID.NortherlyShiftCloak, AID.SoutherlyShiftCloak, AID.EasterlyShiftCloak, AID.WesterlyShiftCloak, AID.NortherlyShiftSword, AID.SoutherlyShiftSword, AID.EasterlyShiftSword, AID.WesterlyShiftSword], 3.9f)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Pinax>(id + 0x5000, 6.1f, comp => comp.NumFinished == 3, "Corner3")
            .ActivateOnEnter<Shift>(); // together with this, one of the helpers starts casting 27142 or 27137
        CastEnd(id + 0x6000, 1.9f, "Shift");
        ComponentCondition<Pinax>(id + 0x7000, 9.1f, comp => comp.NumFinished == 4, "Pinax resolve")
            .DeactivateOnExit<SettingTheScene>(!keepScene)
            .DeactivateOnExit<Pinax>()
            .DeactivateOnExit<Shift>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void VengefulElementalBelone(uint id, float delay)
    {
        // all other bloodrakes target all players
        // third bloodrake in addition 'targets' three of the four corner helpers - untethered one is safe during later mechanic
        Cast(id, AID.Bloodrake, delay, 4, "Bloodrake 3")
            .ActivateOnEnter<ElementalBelone>()
            .DeactivateOnExit<SettingTheScene>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x1000, AID.SettingTheScene, 7.3f, 4, "Scene")
            .ActivateOnEnter<SettingTheScene>()
            .ExecOnExit<ElementalBelone>(comp => comp.Visible = true);
        Cast(id + 0x2000, AID.VengefulBelone, 8.2f, 4, "Roles") // acting X applied after cast end
            .ActivateOnEnter<VengefulBelone>();
        Cast(id + 0x3000, AID.ElementalBelone, 4.2f, 4); // 'elemental resistance down' applied after cast end
        Cast(id + 0x4000, AID.Bloodrake, 4.2f, 4, "Bloodrake 4")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x5000, AID.BeloneBursts, 4.2f, 5, "Orbs") // orbs appear at cast start, tether and start moving at cast end
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x6000, AID.Periaktoi, 9.2f, 5, "Square explode")
            .DeactivateOnExit<SettingTheScene>()
            .DeactivateOnExit<ElementalBelone>()
            .DeactivateOnExit<VengefulBelone>() // TODO: reconsider deactivation time, debuffs fade ~12s later, but I think vengeful needs to be handled before explosion?
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void BeloneCoils(uint id, float delay)
    {
        Cast(id, AID.Bloodrake, delay, 4, "Bloodrake 5")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x1000, AID.BeloneCoils, 3.2f, 4, "Coils 1")
            .ActivateOnEnter<BeloneCoils>()
            .ActivateOnEnter<InversiveChlamys>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        InversiveChlamys(id + 0x2000, 3.2f)
            .SetHint(StateMachine.StateHint.PositioningEnd);
        Cast(id + 0x3000, AID.AethericChlamys, 2.4f, 4);
        Cast(id + 0x4000, AID.Bloodrake, 4.2f, 4, "Bloodrake 6")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x5000, AID.BeloneCoils, 4.2f, 4, "Coils 2")
            .ActivateOnEnter<DirectorsBelone>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x6000, AID.DirectorsBelone, 9.2f, 5);
        InversiveChlamys(id + 0x7000, 9.2f)
            .DeactivateOnExit<BeloneCoils>()
            .DeactivateOnExit<InversiveChlamys>()
            .DeactivateOnExit<DirectorsBelone>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }
}
