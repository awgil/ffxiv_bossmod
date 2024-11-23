namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class A12FafnirStates : StateMachineBuilder
{
    public A12FafnirStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<SpikeFlail>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<DragonBreath>() // this lingers for next mechanic typically
            .ActivateOnEnter<Darter>();
    }

    private void SinglePhase(uint id)
    {
        DarkMatterBlast(id, 6.2f);
        OffensivePosture(id + 0x10000, 5.1f); // TODO: always cone?
        OffensivePosture(id + 0x20000, 6.2f); // TODO: always out?
        OffensivePosture(id + 0x30000, 10.4f); // TODO: always in?
        OffensivePosture(id + 0x40000, 11.0f); // TODO: always out?
        OffensivePosture(id + 0x50000, 4.2f); // TODO: always cone?
        BalefulBreath(id + 0x60000, 3.2f);
        SharpSpike(id + 0x70000, 3.7f);
        HurricaneWing(id + 0x80000, 14.2f);
        OffensivePosture(id + 0x90000, 2.6f); // TODO: always cone?
        BalefulBreath(id + 0xA0000, 3.2f);
        AbsoluteWingedTerror(id + 0xB0000, 14.3f);
        AbsoluteWingedTerror(id + 0xC0000, 10.8f);
        DarkMatterBlast(id + 0xD0000, 4.8f);
        HorridRoarOffensivePosture(id + 0xE0000, 6.2f);
        SharpSpike(id + 0xF0000, 3.2f);
        // then: offensive posture cone > ???
        //HurricaneWing(id + 0x100000, 12.2f); // TODO: hurricane wing 2 is different, includes absolute/winged terror, does not have adds
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

    private void DarkMatterBlast(uint id, float delay)
    {
        Cast(id, AID.DarkMatterBlast, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State OffensivePosture(uint id, float delay)
    {
        return CastMulti(id, [AID.OffensivePostureSpikeFlail, AID.OffensivePostureDragonBreath, AID.OffensivePostureTouchdown], delay, 8, "Cone/in/out");
    }

    private void BalefulBreath(uint id, float delay)
    {
        CastStart(id, AID.BalefulBreath, delay)
            .ActivateOnEnter<BalefulBreath>();
        CastEnd(id + 1, 8);
        ComponentCondition<BalefulBreath>(id + 0x10, 0.2f, comp => comp.NumCasts > 0, "Line stack 1");
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

    private void HurricaneWing(uint id, float delay)
    {
        Cast(id, AID.HurricaneWingRaidwide, delay, 3);
        ComponentCondition<HurricaneWingRaidwide>(id + 0x10, 2.8f, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<HurricaneWingRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HurricaneWingRaidwide>(id + 0x20, 8.5f, comp => comp.NumCasts >= 9, "Raidwide 9")
            .DeactivateOnExit<HurricaneWingRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<HurricaneWingAOE>(id + 0x100, 2, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<HurricaneWingAOE>();
        ComponentCondition<GreatWhirlwind>(id + 0x110, 2.5f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<GreatWhirlwind>();
        ComponentCondition<HurricaneWingAOE>(id + 0x120, 2.5f, comp => comp.NumCasts > 0, "Concentric 1");
        CastStart(id + 0x121, AID.HorridRoar, 0.9f); // TODO: happens way earlier on second??
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

        ComponentCondition<HurricaneWingAOE>(id + 0x300, 4, comp => comp.NumCasts >= 1, "Concentric 3");
        Cast(id + 0x310, AID.HorridRoar, 0.2f, 3);
        ComponentCondition<HurricaneWingAOE>(id + 0x320, 2.8f, comp => comp.NumCasts >= 4)
            .DeactivateOnExit<HurricaneWingAOE>();

        ComponentCondition<GreatWhirlwind>(id + 0x400, 5.9f, comp => comp.AOEs.Count == 0, "Whirlwind end")
            .DeactivateOnExit<GreatWhirlwind>()
            .DeactivateOnExit<HorridRoarPuddle>()
            .DeactivateOnExit<HorridRoarSpread>();
    }

    private void AbsoluteWingedTerror(uint id, float delay)
    {
        CastMulti(id, [AID.AbsoluteTerror, AID.WingedTerror], delay, 6)
            .ActivateOnEnter<AbsoluteTerror>()
            .ActivateOnEnter<WingedTerror>();
        Condition(id + 2, 1.4f, () => Module.FindComponent<AbsoluteTerror>()?.NumCasts > 0 || Module.FindComponent<WingedTerror>()?.NumCasts > 0, "Center/sides")
            .DeactivateOnExit<AbsoluteTerror>()
            .DeactivateOnExit<WingedTerror>();
    }

    private void HorridRoarOffensivePosture(uint id, float delay)
    {
        Cast(id, AID.HorridRoar, delay, 3);
        ComponentCondition<HorridRoarPuddle>(id + 0x10, 1.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<HorridRoarPuddle>();
        ComponentCondition<HorridRoarPuddle>(id + 0x11, 4, comp => comp.NumCasts > 0, "Puddles");
        OffensivePosture(id + 0x100, 3); // TODO: always in?
        OffensivePosture(id + 0x200, 9.5f) // TODO: always cone or out?
            .DeactivateOnExit<HorridRoarPuddle>();
    }
}
