namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class A12FafnirStates : StateMachineBuilder
{
    public A12FafnirStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1: Until 85%")
            .ActivateOnEnter<ShudderingEarth>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.FindComponent<ShudderingEarth>()?.NumCasts > 0;
        SimplePhase(1, Phase2, "P2: Until 15%")
            .ActivateOnEnter<Darter>()
            .ActivateOnEnter<Venom>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.DarkMatterBlast) ?? false);
        DeathPhase(2, Phase3)
            .ActivateOnEnter<Darter>()
            .ActivateOnEnter<Venom>();
    }

    private void Phase1(uint id)
    {
        DarkMatterBlast(id, 6.2f);
        OffensivePostureSpikeFlail(id + 0x10000, 5.2f);
        OffensivePostureTouchdown(id + 0x20000, 5.2f);
        SimpleState(id + 0x30000, 5.2f, "Next phase (85% hp)");
    }

    private void Phase2(uint id)
    {
        OffensivePostureDragonBreathTouchdown(id, 4.5f);
        OffensivePostureSpikeFlail(id + 0x10000, 3.0f);
        BalefulBreath(id + 0x20000, 2.2f);
        SharpSpike(id + 0x30000, 3.7f);
        HurricaneWing1(id + 0x40000, 14.2f);
        OffensivePostureSpikeFlail(id + 0x50000, 2.6f);
        BalefulBreath(id + 0x60000, 2.2f);
        AbsoluteWingedTerror(id + 0x70000, 14.4f);
        AbsoluteWingedTerror(id + 0x80000, 10.7f);
        SimpleState(id + 0x90000, 4.8f, "Next phase (15% hp)");
    }

    private void Phase3(uint id)
    {
        Phase3Repeat(id, 0, true);
        Phase3Repeat(id + 0x100000, 10.3f, false);

        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<BalefulBreath>()
            .ActivateOnEnter<SharpSpike>()
            .ActivateOnEnter<HurricaneWingAOE>()
            .ActivateOnEnter<GreatWhirlwind>()
            .ActivateOnEnter<HorridRoarPuddle>()
            .ActivateOnEnter<HorridRoarSpread>()
            .ActivateOnEnter<AbsoluteTerror>()
            .ActivateOnEnter<WingedTerror>();
    }

    private void Phase3Repeat(uint id, float delay, bool first)
    {
        DarkMatterBlast(id, delay);
        HorridRoarOffensivePosture(id + 0x10000, first ? 6.2f : 9.2f);
        SharpSpike(id + 0x20000, 2.0f);
        HurricaneWing2(id + 0x30000, 12.2f);
        OffensivePostureSpikeFlail(id + 0x40000, 6.8f);
        BalefulBreath(id + 0x50000, 2.2f);
    }

    private void DarkMatterBlast(uint id, float delay)
    {
        Cast(id, AID.DarkMatterBlast, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void OffensivePostureSpikeFlail(uint id, float delay)
    {
        Cast(id, AID.OffensivePostureSpikeFlail, delay, 8)
            .ActivateOnEnter<SpikeFlail>();
        ComponentCondition<SpikeFlail>(id + 2, 1, comp => comp.NumCasts > 0, "Back cleave")
            .DeactivateOnExit<SpikeFlail>();
    }

    private State OffensivePostureTouchdown(uint id, float delay)
    {
        Cast(id, AID.OffensivePostureTouchdown, delay, 8)
            .ActivateOnEnter<Touchdown>();
        return ComponentCondition<Touchdown>(id + 2, 1.2f, comp => comp.NumCasts > 0, "Out")
            .DeactivateOnExit<Touchdown>();
    }

    private void OffensivePostureDragonBreathTouchdown(uint id, float delay)
    {
        Cast(id, AID.OffensivePostureDragonBreath, delay, 8)
            .ActivateOnEnter<DragonBreath>();
        ComponentCondition<DragonBreath>(id + 2, 1.2f, comp => comp.NumCasts > 0, "In");
        OffensivePostureTouchdown(id + 0x1000, 9.5f)
            .DeactivateOnExit<DragonBreath>();
    }

    private void BalefulBreath(uint id, float delay)
    {
        CastStart(id, AID.BalefulBreath, delay)
            .ActivateOnEnter<BalefulBreath>();
        CastEnd(id + 1, 8);
        ComponentCondition<BalefulBreath>(id + 0x10, 0.3f, comp => comp.NumCasts > 0, "Line stack 1");
        ComponentCondition<BalefulBreath>(id + 0x20, 5.2f, comp => comp.NumCasts >= 4, "Line stack 4")
            .DeactivateOnExit<BalefulBreath>();
    }

    private void SharpSpike(uint id, float delay)
    {
        CastStart(id, AID.SharpSpike, delay)
            .ActivateOnEnter<SharpSpike>();
        CastEnd(id + 1, 5);
        ComponentCondition<SharpSpike>(id + 2, 1.2f, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<SharpSpike>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HurricaneWingRaidwide(uint id, float delay)
    {
        Cast(id, AID.HurricaneWingRaidwide, delay, 3);
        ComponentCondition<HurricaneWingRaidwide>(id + 0x10, 2.7f, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<HurricaneWingRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HurricaneWingRaidwide>(id + 0x20, 8.5f, comp => comp.NumCasts >= 9, "Raidwide 9")
            .DeactivateOnExit<HurricaneWingRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HurricaneWing1(uint id, float delay)
    {
        HurricaneWingRaidwide(id, delay);

        ComponentCondition<HurricaneWingAOE>(id + 0x100, 2, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<HurricaneWingAOE>();
        ComponentCondition<GreatWhirlwind>(id + 0x110, 2.5f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<GreatWhirlwind>();
        ComponentCondition<HurricaneWingAOE>(id + 0x120, 2.5f, comp => comp.NumCasts > 0, "Concentric 1");
        CastStart(id + 0x121, AID.HorridRoar, 0.9f);
        ComponentCondition<GreatWhirlwind>(id + 0x122, 1.1f, comp => comp.NumCasts > 0, "Whirlwinds");
        CastEnd(id + 0x123, 1.9f);
        ComponentCondition<HurricaneWingAOE>(id + 0x130, 2.1f, comp => comp.NumCasts >= 4)
            .ActivateOnEnter<HorridRoarPuddle>()
            .ActivateOnEnter<HorridRoarSpread>();

        ComponentCondition<HurricaneWingAOE>(id + 0x200, 1.1f, comp => comp.AOEs.Count > 0);
        Cast(id + 0x210, AID.HorridRoar, 1, 3, "Concentric 2");
        CastStart(id + 0x220, AID.HorridRoar, 4.2f);
        ComponentCondition<HurricaneWingAOE>(id + 0x221, 1.9f, comp => comp.NumCasts >= 4);
        CastEnd(id + 0x222, 1.1f);

        ComponentCondition<HurricaneWingAOE>(id + 0x300, 4, comp => comp.NumCasts > 0, "Concentric 3");
        Cast(id + 0x310, AID.HorridRoar, 0.2f, 3);
        ComponentCondition<HurricaneWingAOE>(id + 0x320, 2.8f, comp => comp.NumCasts >= 4)
            .DeactivateOnExit<HurricaneWingAOE>();

        ComponentCondition<GreatWhirlwind>(id + 0x400, 5.9f, comp => comp.AOEs.Count == 0, "Whirlwind end")
            .DeactivateOnExit<GreatWhirlwind>()
            .DeactivateOnExit<HorridRoarPuddle>()
            .DeactivateOnExit<HorridRoarSpread>();
    }

    private State AbsoluteWingedTerror(uint id, float delay)
    {
        CastMulti(id, [AID.AbsoluteTerror, AID.WingedTerror], delay, 6)
            .ActivateOnEnter<AbsoluteTerror>()
            .ActivateOnEnter<WingedTerror>();
        return Condition(id + 2, 1.4f, () => Module.FindComponent<AbsoluteTerror>()?.NumCasts > 0 || Module.FindComponent<WingedTerror>()?.NumCasts > 0, "Center/sides")
            .DeactivateOnExit<AbsoluteTerror>()
            .DeactivateOnExit<WingedTerror>();
    }

    private void HorridRoarOffensivePosture(uint id, float delay)
    {
        Cast(id, AID.HorridRoar, delay, 3);
        ComponentCondition<HorridRoarPuddle>(id + 0x10, 1.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<HorridRoarPuddle>();
        ComponentCondition<HorridRoarPuddle>(id + 0x11, 4, comp => comp.NumCasts > 0, "Puddles");

        Cast(id + 0x1000, AID.OffensivePostureDragonBreath, 2.9f, 8)
            .ActivateOnEnter<DragonBreath>();
        ComponentCondition<DragonBreath>(id + 0x1002, 1.2f, comp => comp.NumCasts > 0, "In");

        CastMulti(id + 0x2000, [AID.OffensivePostureSpikeFlail, AID.OffensivePostureTouchdown], 7.2f, 8)
            .ActivateOnEnter<SpikeFlail>()
            .ActivateOnEnter<Touchdown>()
            .DeactivateOnExit<HorridRoarPuddle>();
        Condition(id + 0x2002, 1.1f, () => Module.FindComponent<SpikeFlail>()?.NumCasts > 0 || Module.FindComponent<Touchdown>()?.NumCasts > 0, "Out/Back cleave")
            .DeactivateOnExit<SpikeFlail>()
            .DeactivateOnExit<Touchdown>()
            .DeactivateOnExit<DragonBreath>();
    }

    private void HurricaneWing2(uint id, float delay)
    {
        HurricaneWingRaidwide(id, delay);

        ComponentCondition<HurricaneWingAOE>(id + 0x100, 2, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<HurricaneWingAOE>();
        ComponentCondition<GreatWhirlwind>(id + 0x110, 2.5f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<GreatWhirlwind>();
        CastStart(id + 0x120, AID.HorridRoar, 1.4f);
        ComponentCondition<HurricaneWingAOE>(id + 0x121, 1.1f, comp => comp.NumCasts > 0, "Concentric 1");
        CastEnd(id + 0x122, 1.9f, "Whirlwinds");
        ComponentCondition<HurricaneWingAOE>(id + 0x130, 4.1f, comp => comp.NumCasts >= 4)
            .ActivateOnEnter<HorridRoarPuddle>()
            .ActivateOnEnter<HorridRoarSpread>();

        CastStart(id + 0x200, AID.HorridRoar, 0.1f);
        ComponentCondition<HurricaneWingAOE>(id + 0x201, 1, comp => comp.AOEs.Count > 0);
        CastEnd(id + 0x202, 2);
        ComponentCondition<HurricaneWingAOE>(id + 0x203, 2, comp => comp.NumCasts > 0, "Concentric 2");
        Cast(id + 0x210, AID.HorridRoar, 2.1f, 3);
        ComponentCondition<HurricaneWingAOE>(id + 0x220, 0.9f, comp => comp.NumCasts >= 4);

        ComponentCondition<HurricaneWingAOE>(id + 0x300, 1.1f, comp => comp.AOEs.Count > 0);
        ComponentCondition<HurricaneWingAOE>(id + 0x301, 4, comp => comp.NumCasts > 0, "Concentric 3");
        ComponentCondition<HurricaneWingAOE>(id + 0x310, 6, comp => comp.NumCasts >= 4)
            .DeactivateOnExit<HurricaneWingAOE>();

        AbsoluteWingedTerror(id + 0x400, 3.2f)
            .DeactivateOnExit<GreatWhirlwind>()
            .DeactivateOnExit<HorridRoarPuddle>()
            .DeactivateOnExit<HorridRoarSpread>();
    }
}
