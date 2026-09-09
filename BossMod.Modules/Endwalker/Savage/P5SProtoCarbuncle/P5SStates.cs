namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class P5SStates : StateMachineBuilder
{
    public P5SStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SonicHowl(id, 9.2f);
        RubyGlowTopazStones(id + 0x10000, 2.1f);
        VenomousMassToxicCrunch(id + 0x20000, 8.2f, true);
        VenomTowers(id + 0x30000, 8.1f);
        VenomousMassToxicCrunch(id + 0x40000, 4.1f);
        RubyGlowTopazStonesDoubleRush(id + 0x50000, 9.1f);
        SonicHowl(id + 0x60000, 4.7f, true);
        RubyGlowTopazCluster(id + 0x70000, 8.2f);
        VenomousMassToxicCrunch(id + 0x80000, 3.6f);
        VenomSquallSurge(id + 0x90000, 6.1f);
        ClawTail(id + 0xA0000, 2.4f);
        StarvingStampede(id + 0xB0000, 19.6f); // this delay is sometimes 0.6s less (claw/tail?)
        SonicHowl(id + 0xC0000, 8.3f);
        RubyGlowTopazStonesVenomPoolRayClaw(id + 0xD0000, 2.1f);
        VenomousMassToxicCrunch(id + 0xE0000, 9.3f, true);
        RubyGlowTopazStonesVenomSquall(id + 0xF0000, 8.2f);
        VenomTowersClawTail(id + 0x100000, 0.3f);
        VenomousMassToxicCrunch(id + 0x110000, 2.8f); // 2.7-3.4 range (claw/tail?)
        SonicHowl(id + 0x120000, 6.1f);
        RubyGlowTopazStonesVenomPoolDoubleRush(id + 0x130000, 2.2f);
        VenomousMassToxicCrunch(id + 0x140000, 3.2f, true);
        SonicHowl(id + 0x150000, 8.2f);
        VenomSquallSurge(id + 0x160000, 3.2f);
        ClawTail(id + 0x170000, 0.5f);
        VenomousMassToxicCrunch(id + 0x180000, 4.4f); // this delay is sometimes 0.6s less (claw/tail?)
        SonicShatter(id + 0x190000, 9.1f);
        Cast(id + 0x1A0000, AID.AcidicSlaver, 15.3f, 5, "Enrage");
    }

    private void VenomousMassToxicCrunch(uint id, float delay, bool endRubyGlow = false)
    {
        Cast(id, AID.VenomousMass, delay, 5)
            .ActivateOnEnter<VenomousMass>()
            .DeactivateOnExit<RubyGlowCommon>(endRubyGlow);
        ComponentCondition<VenomousMass>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Tankbuster 1")
            .DeactivateOnExit<VenomousMass>();

        Cast(id + 0x1000, AID.ToxicCrunch, 1.3f, 5)
            .ActivateOnEnter<ToxicCrunch>();
        ComponentCondition<ToxicCrunch>(id + 0x1002, 0.3f, comp => comp.NumCasts > 0, "Tankbuster 2")
            .DeactivateOnExit<ToxicCrunch>();
    }

    private void SonicHowl(uint id, float delay, bool endRubyGlow = false)
    {
        Cast(id, AID.SonicHowl, delay, 5, "Raidwide")
            .DeactivateOnExit<RubyGlowCommon>(endRubyGlow)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State RubyGlow(uint id, float delay)
    {
        return Cast(id, AID.RubyGlow, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RubyGlowTopazStones(uint id, float delay)
    {
        // ruby glow 1: 2x2 cells, 2 magic, 2 poison - need to find a safespot
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow1>();
        Cast(id + 0x1000, AID.TopazStones, 3.2f, 4);
        ComponentCondition<RubyGlow1>(id + 0x1010, 0.5f, comp => comp.MagicStones.Any());
        ComponentCondition<RubyGlow1>(id + 0x1020, 13.5f, comp => !comp.MagicStones.Any(), "Cells");
        // note: poison disappears later, during next mechanic...
    }

    private void RubyGlowTopazStonesDoubleRush(uint id, float delay)
    {
        // ruby glow 2: diagonal line, 1 magic, 1 poison, charge - need to avoid first charge and then avoid magic
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow2>();
        Cast(id + 0x1000, AID.TopazStones, 3.2f, 4);
        DoubleRush(id + 0x1010, 4.5f);
        ComponentCondition<RubyGlow2>(id + 0x1020, 1.5f, comp => !comp.MagicStones.Any(), "Cells");
        // note: poison disappears later, during next mechanic...
    }

    private void RubyGlowTopazCluster(uint id, float delay)
    {
        // ruby glow 3: 2x2 cells, 2+2+3+3 magic - need to move between safespots
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow3>();
        Cast(id + 0x1000, AID.TopazCluster, 2.1f, 4);
        ComponentCondition<RubyGlow3>(id + 0x1010, 11.1f, comp => comp.NumCasts >= 2, "Cells 1");
        ComponentCondition<RubyGlow3>(id + 0x1011, 2.5f, comp => comp.NumCasts >= 4, "Cells 2");
        ComponentCondition<RubyGlow3>(id + 0x1012, 2.5f, comp => comp.NumCasts >= 7, "Cells 3");
        ComponentCondition<RubyGlow3>(id + 0x1013, 2.5f, comp => comp.NumCasts >= 10, "Cells 4")
            .DeactivateOnExit<RubyGlow3>();
    }

    private void RubyGlowTopazStonesVenomPoolRayClaw(uint id, float delay)
    {
        // ruby glow 4: diagonal line, 2+3 magic, 2 venom pools - need to recolor 2 magic to poison and then avoid ray/claw while avoiding poison
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow4>();
        Cast(id + 0x1000, AID.TopazStones, 2.1f, 4);
        Cast(id + 0x1010, AID.VenomPoolRecolor, 2.1f, 5);
        ComponentCondition<RubyGlow4>(id + 0x1020, 3.9f, comp => comp.NumCasts > 0, "Recolor");
        ComponentCondition<RubyGlow4>(id + 0x1030, 3, comp => !comp.MagicStones.Any(), "Cells");
        CastMulti(id + 0x2000, [AID.SearingRay, AID.RagingClaw], 0.2f, 5, "Searing ray / Raging claw");
        // note: poison disappears later, during next mechanic...
        // note: raging claw continues hitting for ~2.5s, searing ray resolves immediately - next mechanic is fixed relative to cast end
    }

    private void RubyGlowTopazStonesVenomSquall(uint id, float delay)
    {
        // ruby glow 5: 2x2 cells, 2 magic, 2 poison, spread - need to avoid magic and then spread while avoiding poison
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow5>();
        Cast(id + 0x1000, AID.TopazStones, 3.1f, 4);
        ComponentCondition<RubyGlow5>(id + 0x1010, 0.5f, comp => comp.MagicStones.Any());
        // note: we next part is same as venom squall/surge, except that order is fixed; magic explosion happens ~0.1s before squall cast end
        CastStart(id + 0x1020, AID.VenomSquall, 8.6f);
        ComponentCondition<RubyGlow5>(id + 0x1021, 4.9f, comp => !comp.MagicStones.Any(), "Cells");
        CastEnd(id + 0x1022, 0.1f)
            .ActivateOnEnter<VenomSquallSurge>(); // note: activating only after cells resolve to reduce visual clutter
        ComponentCondition<VenomSquallSurge>(id + 0x1023, 3.8f, comp => comp.Progress > 0, "Spread");
        ComponentCondition<VenomSquallSurge>(id + 0x1024, 3, comp => comp.Progress > 1, "Mid bait");
        ComponentCondition<VenomDrops>(id + 0x1025, 3, comp => comp.NumCasts > 0)
            .ActivateOnEnter<VenomDrops>()
            .DeactivateOnExit<VenomDrops>()
            .DeactivateOnExit<RubyGlow5>(); // poison disappears ~2.5s into cast
        ComponentCondition<VenomSquallSurge>(id + 0x1026, 3, comp => comp.Progress > 2, "Stack")
            .DeactivateOnExit<VenomSquallSurge>();
    }

    private void RubyGlowTopazStonesVenomPoolDoubleRush(uint id, float delay)
    {
        // ruby glow 6: 2x2 cells, 3+2+2+2 magic, venom pools - need to recolor 2 magic to poison and then avoid charges while avoiding poison
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow6>();
        Cast(id + 0x1000, AID.TopazStones, 2.1f, 4);
        Cast(id + 0x1010, AID.VenomPoolRecolor, 2.1f, 5);
        ComponentCondition<RubyGlow6>(id + 0x1020, 3.9f, comp => comp.NumCasts > 0, "Recolor");
        ComponentCondition<RubyGlow6>(id + 0x1030, 3, comp => !comp.MagicStones.Any(), "Cells");
        DoubleRush(id + 0x1040, 4);
        // note: poison disappears later, during next mechanic...
    }

    private void DoubleRush(uint id, float delay)
    {
        Cast(id, AID.DoubleRush, delay, 6, "Charge 1")
            .ActivateOnEnter<DoubleRush>()
            .DeactivateOnExit<DoubleRush>();
        ComponentCondition<DoubleRushReturn>(id + 2, 2.1f, comp => comp.NumCasts > 0, "Charge 2")
            .ActivateOnEnter<DoubleRushReturn>()
            .DeactivateOnExit<DoubleRushReturn>();
    }

    private void VenomSquallSurge(uint id, float delay)
    {
        CastMulti(id, [AID.VenomSquall, AID.VenomSurge], delay, 5)
            .ActivateOnEnter<VenomSquallSurge>();
        ComponentCondition<VenomSquallSurge>(id + 2, 3.8f, comp => comp.Progress > 0, "Spread/stack");
        ComponentCondition<VenomSquallSurge>(id + 3, 3, comp => comp.Progress > 1, "Mid bait");
        ComponentCondition<VenomDrops>(id + 4, 3, comp => comp.NumCasts > 0)
            .ActivateOnEnter<VenomDrops>()
            .DeactivateOnExit<VenomDrops>();
        ComponentCondition<VenomSquallSurge>(id + 5, 3, comp => comp.Progress > 2, "Stack/spread")
            .DeactivateOnExit<VenomSquallSurge>();
    }

    private void VenomTowers(uint id, float delay)
    {
        ComponentCondition<VenomTowers>(id, delay, comp => comp.Active)
            .ActivateOnEnter<VenomTowers>();
        ComponentCondition<VenomTowers>(id + 0x10, 13, comp => !comp.Active, "Towers")
            .DeactivateOnExit<VenomTowers>();
    }

    private void ClawTail(uint id, float delay)
    {
        // note: tail to claw is ~0.5s shorter (and next state is longer), but other timings are unaffected
        CastStartMulti(id, [AID.ClawToTail, AID.TailToClaw], delay);
        ComponentCondition<ClawTail>(id + 1, 6, comp => comp.Progress > 0, "Claw/tail hit 1")
            .ActivateOnEnter<ClawTail>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
        ComponentCondition<ClawTail>(id + 0x100, 4.2f, comp => comp.Progress >= 8, "Claw/tail end")
            .DeactivateOnExit<ClawTail>();
    }

    private void VenomTowersClawTail(uint id, float delay)
    {
        ComponentCondition<VenomTowers>(id, delay, comp => comp.Active)
            .ActivateOnEnter<VenomTowers>();
        CastStartMulti(id + 1, [AID.ClawToTail, AID.TailToClaw], 12.2f);
        ComponentCondition<VenomTowers>(id + 2, 0.8f, comp => !comp.Active, "Towers")
            .DeactivateOnExit<VenomTowers>();
        ComponentCondition<ClawTail>(id + 3, 5.2f, comp => comp.Progress > 0, "Claw/tail hit 1")
            .ActivateOnEnter<ClawTail>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
        ComponentCondition<ClawTail>(id + 0x100, 4.2f, comp => comp.Progress >= 8, "Claw/tail end")
            .DeactivateOnExit<ClawTail>();
    }

    private void StarvingStampede(uint id, float delay)
    {
        Targetable(id, false, delay, "Jumps disappear")
            .ActivateOnEnter<StarvingStampede>();
        ComponentCondition<VenomTowers>(id + 1, 0.9f, comp => comp.Active)
            .ActivateOnEnter<VenomTowers>();
        Targetable(id + 2, true, 9.7f, "Jumps reappear")
            .DeactivateOnExit<StarvingStampede>();
        ComponentCondition<VenomTowers>(id + 3, 3.3f, comp => !comp.Active, "Towers")
            .DeactivateOnExit<VenomTowers>();
        // note: if any players have failed the mechanic, this will be extended while he is eating people...
        ComponentCondition<DevourBait>(id + 0x10, 2.3f, comp => comp.NumCasts > 0, "Devour", 20)
            .ActivateOnEnter<DevourBait>()
            .DeactivateOnExit<DevourBait>();
    }

    private void SonicShatter(uint id, float delay)
    {
        Cast(id, AID.SonicShatter, delay, 5, "Raidwide hit 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SonicShatter>(id + 2, 3.1f, comp => comp.NumCasts >= 1, "Hit 2")
            .ActivateOnEnter<SonicShatter>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SonicShatter>(id + 3, 3.1f, comp => comp.NumCasts >= 2, "Hit 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SonicShatter>(id + 4, 3.1f, comp => comp.NumCasts >= 3, "Hit 4")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SonicShatter>(id + 5, 3.1f, comp => comp.NumCasts >= 4, "Hit 5")
            .DeactivateOnExit<SonicShatter>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
