namespace BossMod.Endwalker.Alliance.A13Azeyma;

public class A13AzeymaStates : StateMachineBuilder
{
    public A13AzeymaStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        WardensProminence(id, 6.2f);
        SolarWings(id + 0x10000, 14.0f);
        WardensWarmth(id + 0x20000, 7.7f);
        SolarWingsSunShine(id + 0x30000, 11.0f); // quite large variance here
        SolarFans(id + 0x40000, 5.3f);
        WardensWarmth(id + 0x50000, 10.8f); // quite large variance here
        FleetingSpark(id + 0x60000, 6.4f);
        SolarFold(id + 0x70000, 10.3f, true); // quite large variance here
        WildfireWard(id + 0x80000, 5.8f);
        NobleDawn(id + 0x90000, 7.5f);
        SublimeSunset(id + 0xA0000, 5.3f); // quite large variance here
        SolarFans(id + 0xB0000, 13.1f); // quite large variance here
        FleetingSpark(id + 0xC0000, 5.0f);
        SolarWingsSunShine(id + 0xD0000, 15.2f); // quite large variance here
        WardensWarmth(id + 0xE0000, 2.6f);
        SolarFold(id + 0xF0000, 9.5f, false); // quite large variance here
        NobleDawn(id + 0x100000, 7.8f);
        FleetingSpark(id + 0x110000, 2.6f);
        SublimeSunset(id + 0x120000, 4.7f);
        WardensWarmth(id + 0x130000, 3.6f);
        SolarFans(id + 0x140000, 11.1f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private State WardensProminence(uint id, float delay)
    {
        return Cast(id, AID.WardensProminence, delay, 6, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WardensWarmth(uint id, float delay)
    {
        Cast(id, AID.WardensWarmth, delay, 5, "Tankbusters")
            .ActivateOnEnter<WardensWarmth>()
            .DeactivateOnExit<WardensWarmth>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SolarWings(uint id, float delay)
    {
        Cast(id, AID.SolarWings, delay, 4, "Sides cleave")
            .ActivateOnEnter<SolarWingsL>()
            .ActivateOnEnter<SolarWingsR>()
            .DeactivateOnExit<SolarWingsL>()
            .DeactivateOnExit<SolarWingsR>();
        ComponentCondition<SolarFlair>(id + 0x10, 7.7f, comp => comp.NumCasts >= 6, "Circles")
            .ActivateOnEnter<SolarFlair>()
            .DeactivateOnExit<SolarFlair>();
    }

    private void SolarWingsSunShine(uint id, float delay)
    {
        Cast(id, AID.SolarWings, delay, 4, "Sides cleave")
            .ActivateOnEnter<SolarWingsL>()
            .ActivateOnEnter<SolarWingsR>()
            .DeactivateOnExit<SolarWingsL>()
            .DeactivateOnExit<SolarWingsR>();
        Cast(id + 0x10, AID.SunShine, 2.5f, 3);
        ComponentCondition<SolarFlair>(id + 0x20, 13.5f, comp => comp.NumCasts >= 6, "Circles")
            .ActivateOnEnter<SolarFlair>()
            .DeactivateOnExit<SolarFlair>();
    }

    private void SolarFans(uint id, float delay)
    {
        Cast(id, AID.SolarFans, delay, 4, "Fans start")
            .ActivateOnEnter<SolarFans>()
            .DeactivateOnExit<SolarFans>();
        // +0.5s: charge cast end
        CastStart(id + 0x10, AID.RadiantRhythmFirst, 3.2f)
            .ActivateOnEnter<RadiantRhythm>();
        CastEnd(id + 0x11, 5);
        ComponentCondition<RadiantRhythm>(id + 0x12, 0.1f, comp => comp.NumCasts >= 2); // first cast; after that there are 3 or 4 rhythm casts, 1.4s apart
        CastStart(id + 0x20, AID.RadiantFinish, 5.4f) // or 6.8, depending on number of rhythm casts
            .DeactivateOnExit<RadiantRhythm>();
        CastEnd(id + 0x21, 3, "Fans resolve")
            .ActivateOnEnter<RadiantFinish>()
            .DeactivateOnExit<RadiantFinish>();
    }

    private void FleetingSpark(uint id, float delay)
    {
        Cast(id, AID.FleetingSpark, delay, 5.5f, "Front/side cleave")
            .ActivateOnEnter<FleetingSpark>()
            .DeactivateOnExit<FleetingSpark>();
    }

    private void SolarFold(uint id, float delay, bool first)
    {
        Cast(id, AID.SolarFold, delay, 2.6f) // TODO: this is very weird, need more recent data...
            .ActivateOnEnter<SolarFold>();
        ComponentCondition<SolarFold>(id + 2, 1.4f, comp => comp.NumCasts > 0, "Cross")
            .DeactivateOnExit<SolarFold>();
        Cast(id + 0x10, AID.SunShine, 2.1f, 3);
        ComponentCondition<DancingFlame>(id + 0x20, 5.8f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<DancingFlame>();
        ComponentCondition<DancingFlame>(id + 0x30, 13.1f, comp => comp.NumCasts > 0, "Diagonals start");
        WardensProminence(id + 0x100, first ? 2.3f : 0.3f)
            .DeactivateOnExit<DancingFlame>();
    }

    private void WildfireWard(uint id, float delay)
    {
        Cast(id, AID.WildfireWard, delay, 5)
            .ActivateOnEnter<WildfireWard>();
        ComponentCondition<WildfireWard>(id + 0x10, 16.2f, comp => comp.NumCasts > 0, "Knockback 1");
        ComponentCondition<WildfireWard>(id + 0x11, 4, comp => comp.NumCasts > 1, "Knockback 2");
        ComponentCondition<WildfireWard>(id + 0x12, 4, comp => comp.NumCasts > 2, "Knockback 3")
            .DeactivateOnExit<WildfireWard>();
    }

    private void NobleDawn(uint id, float delay)
    {
        Cast(id, AID.NobleDawn, delay, 4);
        ComponentCondition<Sunbeam>(id + 0x10, 4.9f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Sunbeam>();
        ComponentCondition<Sunbeam>(id + 0x20, 6, comp => comp.NumCasts > 0, "Puddles 1");
        ComponentCondition<Sunbeam>(id + 0x21, 2, comp => comp.NumCasts > 7, "Puddles 2");
        ComponentCondition<Sunbeam>(id + 0x22, 2, comp => comp.NumCasts > 14, "Puddles 3")
            .DeactivateOnExit<Sunbeam>();
    }

    private void SublimeSunset(uint id, float delay)
    {
        Cast(id, AID.SublimeSunset, delay, 9)
            .ActivateOnEnter<SublimeSunset>();
        ComponentCondition<SublimeSunset>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Large AOE")
            .DeactivateOnExit<SublimeSunset>();
    }
}
