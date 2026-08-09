namespace BossMod.Endwalker.Alliance.A13Azeyma;

public sealed class A13AzeymaStates : StateMachineBuilder
{
    public A13AzeymaStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<WardensWarmth>()
            .ActivateOnEnter<SolarWingsL>()
            .ActivateOnEnter<SolarWingsR>()
            .ActivateOnEnter<SolarFlair>()
            .ActivateOnEnter<RadiantFlourish>()
            .ActivateOnEnter<SolarFans>()
            .ActivateOnEnter<RadiantRhythm>()
            .ActivateOnEnter<FleetingSpark>()
            .ActivateOnEnter<SolarFold>()
            .ActivateOnEnter<DancingFlame>()
            .ActivateOnEnter<WildfireWard>()
            .ActivateOnEnter<Sunbeam>()
            .ActivateOnEnter<SublimeSunset>();
    }

    private void SinglePhase(uint id)
    {
        WardensProminence(id, 6.2f);
        SolarWings(id + 0x10000u, 14.0f);
        WardensWarmth(id + 0x20000u, 7.7f);
        SolarWingsSunShine(id + 0x30000u, 11.0f); // quite large variance here
        SolarFans(id + 0x40000u, 5.3f);
        WardensWarmth(id + 0x50000u, 10.8f); // quite large variance here
        FleetingSpark(id + 0x60000u, 6.4f);
        SolarFold(id + 0x70000u, 10.3f, true); // quite large variance here
        WildfireWard(id + 0x80000u, 5.8f);
        NobleDawn(id + 0x90000u, 7.5f);
        SublimeSunset(id + 0xA0000u, 5.3f); // quite large variance here
        SolarFans(id + 0xB0000u, 13.1f); // quite large variance here
        FleetingSpark(id + 0xC0000u, 5.0f);
        SolarWingsSunShine(id + 0xD0000u, 15.2f); // quite large variance here
        WardensWarmth(id + 0xE0000u, 2.6f);
        SolarFold(id + 0xF0000u, 9.5f, false); // quite large variance here
        NobleDawn(id + 0x100000u, 7.8f);
        FleetingSpark(id + 0x110000u, 2.6f);
        SublimeSunset(id + 0x120000u, 4.7f);
        WardensWarmth(id + 0x130000u, 3.6f);
        SolarFans(id + 0x140000u, 11.1f);

        SimpleState(id + 0xFF0000f, 10f, "???");
    }

    private State WardensProminence(uint id, float delay)
    {
        return Cast(id, (uint)AID.WardensProminence, delay, 6f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WardensWarmth(uint id, float delay)
    {
        Cast(id, (uint)AID.WardensWarmth, delay, 5f, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SolarWings(uint id, float delay)
    {
        Cast(id, (uint)AID.SolarWings, delay, 4f, "Sides cleave");
        ComponentCondition<SolarFlair>(id + 0x10u, 7.7f, comp => comp.NumCasts >= 6, "Circles")
            .ExecOnExit<SolarFlair>(comp => comp.NumCasts = 0);
    }

    private void SolarWingsSunShine(uint id, float delay)
    {
        Cast(id, (uint)AID.SolarWings, delay, 4f, "Sides cleave");
        Cast(id + 0x10, (uint)AID.SunShine, 2.5f, 3);
        ComponentCondition<SolarFlair>(id + 0x20u, 13.5f, comp => comp.NumCasts >= 6, "Circles")
            .ExecOnExit<SolarFlair>(comp => comp.NumCasts = 0);
    }

    private void SolarFans(uint id, float delay)
    {
        Cast(id, (uint)AID.SolarFans, delay, 4, "Fans start");
        // +0.5s: charge cast end
        CastStart(id + 0x10u, (uint)AID.RadiantRhythmFirst, 3.2f);
        CastEnd(id + 0x11u, 5f);
        ComponentCondition<RadiantRhythm>(id + 0x12u, 0.1f, comp => comp.NumCasts >= 2); // first cast; after that there are 3 or 4 rhythm casts, 1.4s apart
        CastStart(id + 0x20, (uint)AID.RadiantFinish, 5.4f) // or 6.8, depending on number of rhythm casts
            .ResetComp<RadiantRhythm>();
        CastEnd(id + 0x21u, 3f, "Fans resolve");
    }

    private void FleetingSpark(uint id, float delay)
    {
        Cast(id, (uint)AID.FleetingSpark, delay, 5.5f, "Front/side cleave");
    }

    private void SolarFold(uint id, float delay, bool first)
    {
        Cast(id, (uint)AID.SolarFold, delay, 2.6f); // TODO: this is very weird, need more recent data...
        ComponentCondition<SolarFold>(id + 2u, 1.4f, comp => comp.NumCasts != 0, "Cross")
            .ResetComp<SolarFold>();
        Cast(id + 0x10u, (uint)AID.SunShine, 2.1f, 3f);
        ComponentCondition<DancingFlame>(id + 0x20u, 5.8f, comp => comp.AOEs.Count < 4);
        ComponentCondition<DancingFlame>(id + 0x30u, 13.1f, comp => comp.NumCasts != 0, "Diagonals start");
        WardensProminence(id + 0x100u, first ? 2.3f : 0.3f)
            .ResetComp<DancingFlame>();
    }

    private void WildfireWard(uint id, float delay)
    {
        Cast(id, (uint)AID.WildfireWard, delay, 5f);
        ComponentCondition<WildfireWard>(id + 0x10u, 16.2f, comp => comp.NumCasts != 0, "Knockback 1");
        ComponentCondition<WildfireWard>(id + 0x11u, 4f, comp => comp.NumCasts > 1, "Knockback 2");
        ComponentCondition<WildfireWard>(id + 0x12u, 4f, comp => comp.NumCasts > 2, "Knockback 3")
            .ResetComp<WildfireWard>();
    }

    private void NobleDawn(uint id, float delay)
    {
        Cast(id, (uint)AID.NobleDawn, delay, 4f);
        ComponentCondition<Sunbeam>(id + 0x10u, 4.9f, comp => comp.Casters.Count != 0);
        ComponentCondition<Sunbeam>(id + 0x20u, 6f, comp => comp.NumCasts != 0, "Puddles 1");
        ComponentCondition<Sunbeam>(id + 0x21u, 2f, comp => comp.NumCasts > 7, "Puddles 2");
        ComponentCondition<Sunbeam>(id + 0x22u, 2f, comp => comp.NumCasts > 14, "Puddles 3")
            .ResetComp<Sunbeam>();
    }

    private void SublimeSunset(uint id, float delay)
    {
        Cast(id, (uint)AID.SublimeSunset, delay, 9f);
        ComponentCondition<SublimeSunset>(id + 2u, 0.5f, comp => comp.NumCasts != 0, "Large AOE")
            .ResetComp<SublimeSunset>();
    }
}
