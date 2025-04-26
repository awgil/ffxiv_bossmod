namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class RM08SHowlingBladeStates : StateMachineBuilder
{
    public RM08SHowlingBladeStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ExtraplanarPursuit(id, 10.2f);
        WindStonefang(id + 0x10000, 8.9f);
        RevolutionaryReign(id + 0x20000, 5.1f);
        ExtraplanarPursuit(id + 0x30000, 2.2f);
        MillennialDecay(id + 0x40000, 8.5f);
        TrackingTremors(id + 0x50000, 0.8f);
        ExtraplanarPursuit(id + 0x60000, 1.8f);
        TerrestrialTitans(id + 0x70000, 3.8f);
        RevolutionaryReign(id + 0x80000, 0.3f);
        TacticalPack(id + 0x90000, 9.2f);
        TerrestrialRage(id + 0xA0000, 14.5f);
        RevolutionaryReign(id + 0xB0000, 0);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void ExtraplanarPursuit(uint id, float delay)
    {
        CastStart(id, AID.ExtraplanarPursuitVisual, delay)
            .ActivateOnEnter<ExtraplanarPursuit>();
        ComponentCondition<ExtraplanarPursuit>(id + 1, 4, e => e.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<ExtraplanarPursuit>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WindStonefang(uint id, float delay)
    {
        CastMulti(id, [AID.WindfangIntercards, AID.WindfangCards, AID.StonefangCards, AID.StonefangIntercards], delay, 6, "In/out")
            .ActivateOnEnter<WindfangStonefangCross>()
            .ActivateOnEnter<WindfangDonut>()
            .ActivateOnEnter<StonefangCircle>()
            .ActivateOnEnter<WindfangStonefang>();

        ComponentCondition<WindfangStonefang>(id + 2, 0.1f, w => w.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<WindfangStonefangCross>()
            .DeactivateOnExit<WindfangDonut>()
            .DeactivateOnExit<StonefangCircle>()
            .DeactivateOnExit<WindfangStonefang>();
    }

    private void RevolutionaryReign(uint id, float delay)
    {
        CastMulti(id, [AID.EminentReignVisual1, AID.RevolutionaryReignVisual1, AID.EminentReignVisual2, AID.RevolutionaryReignVisual2], delay, 5.1f)
            .ActivateOnEnter<ReignJumpCounter>()
            .ActivateOnEnter<WolvesReign>()
            .ActivateOnEnter<ReignInout>()
            .ActivateOnEnter<WolvesReignRect>();

        ComponentCondition<ReignJumpCounter>(id + 2, 1.9f, e => e.NumCasts > 0, "Boss jump")
            .DeactivateOnExit<ReignJumpCounter>()
            .DeactivateOnExit<WolvesReign>();
        ComponentCondition<WolvesReignRect>(id + 3, 2.5f, w => w.NumCasts > 0, "Line AOE")
            .DeactivateOnExit<WolvesReignRect>();
        ComponentCondition<ReignInout>(id + 4, 3.1f, r => r.NumCasts > 0, "In/out")
            .ActivateOnEnter<ReignsEnd>()
            .ActivateOnEnter<SovereignScar>()
            .ExecOnEnter<ReignInout>(r => r.Risky = true)
            .DeactivateOnExit<ReignInout>()
            .DeactivateOnExit<SovereignScar>()
            .DeactivateOnExit<ReignsEnd>();
    }

    private void MillennialDecay(uint id, float delay)
    {
        Cast(id, AID.MillennialDecay, delay, 5)
            .ActivateOnEnter<MillennialDecay>()
            .ActivateOnEnter<BreathOfDecay>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<AeroIII>()
            .ActivateOnEnter<ProwlingGale>()
            .DeactivateOnExit<MillennialDecay>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<AeroIII>(id + 0x10, 10.7f, e => e.NumCasts > 0, "Knockback");

        ComponentCondition<BreathOfDecay>(id + 0x11, 1.5f, b => b.NumCasts > 0, "Line AOE 1");

        ComponentCondition<Gust>(id + 0x12, 0.4f, g => g.NumFinishedSpreads > 0, "Spreads 1");
        Timeout(id + 0x13, 5.1f, "Spreads 2")
            .DeactivateOnExit<Gust>();
        ComponentCondition<BreathOfDecay>(id + 0x14, 2.5f, b => b.NumCasts > 4, "Line AOE 5")
            .ActivateOnEnter<WindsOfDecay>()
            .ActivateOnEnter<WindsOfDecayTether>()
            .DeactivateOnExit<BreathOfDecay>();

        ComponentCondition<AeroIII>(id + 0x20, 6.2f, a => a.NumCasts > 1, "Knockback")
            .DeactivateOnExit<AeroIII>()
            .ExecOnExit<WindsOfDecay>(w => w.EnableHints = true)
            .ExecOnExit<WindsOfDecayTether>(w => w.EnableHints = true);

        ComponentCondition<ProwlingGale>(id + 0x22, 2.2f, p => p.NumCasts > 0, "Towers")
            .DeactivateOnExit<ProwlingGale>();
        ComponentCondition<WindsOfDecay>(id + 0x23, 0.2f, w => w.NumCasts > 0, "Baits")
            .DeactivateOnExit<WindsOfDecay>()
            .DeactivateOnExit<WindsOfDecayTether>()
            .ActivateOnEnter<TrackingTremors>()
            .ActivateOnEnter<TrackingTremorsStack>();
    }

    private void TrackingTremors(uint id, float delay)
    {
        Cast(id, AID.TrackingTremorsVisual, delay, 5);

        ComponentCondition<TrackingTremors>(id + 2, 0.9f, t => t.NumCasts > 0, "Stack 1");

        ComponentCondition<TrackingTremors>(id + 5, 7.5f, t => t.NumCasts == 8, "Stack 8")
            .DeactivateOnExit<TrackingTremors>()
            .DeactivateOnExit<TrackingTremorsStack>();
    }

    private void TerrestrialTitans(uint id, float delay)
    {
        Cast(id, AID.GreatDivide, delay, 5, "Tankbuster")
            .ActivateOnEnter<TerrestrialTitans>()
            .DeactivateOnExit<TerrestrialTitans>()
            .SetHint(StateMachine.StateHint.Tankbuster);

        Cast(id + 0x10, AID.TerrestrialTitansVisual, 11, 4, "Pillars appear")
            .ActivateOnEnter<TerrestrialTitans>()
            .DeactivateOnExit<TerrestrialTitans>();

        CastStart(id + 0x20, AID.TitanicPursuitVisual, 3.2f)
            .ActivateOnEnter<TitanicPursuit>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<FangedCrossing>();

        ComponentCondition<TitanicPursuit>(id + 0x21, 4, t => t.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<TitanicPursuit>();

        ComponentCondition<FangedCrossing>(id + 0x30, 7.9f, f => f.NumCasts > 0, "Safe spot")
            .DeactivateOnExit<FangedCrossing>()
            .DeactivateOnExit<Towerfall>();
    }

    private void TacticalPack(uint id, float delay)
    {
        Cast(id, AID.TacticalPack, delay, 3)
            .ActivateOnEnter<HowlingHavoc>()
            .ActivateOnEnter<AddsVoidzone>()
            .ActivateOnEnter<WolfOfWindStone>()
            .ActivateOnEnter<StalkingWindStone>()
            .ActivateOnEnter<AlphaWindStone>()
            .ActivateOnEnter<ForlornWindStone>();

        Targetable(id + 0x10, false, 2, "Boss disappears");

        id += 0x20;

        ComponentCondition<HowlingHavoc>(id, 7.2f, h => h.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<HowlingHavoc>();

        ComponentCondition<WolfOfWindStone>(id + 1, 2, w => w.WolfOfStone != null, "Adds appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<StalkingWindStone>(id + 0x10, 3, s => s.Baits.Count > 0)
            .ActivateOnEnter<EarthyWindborneEnd>();

        ComponentCondition<StalkingWindStone>(id + 0x11, 5.1f, s => s.NumCasts > 0, "Baits 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<StalkingWindStone>(id + 0x12, 9.1f, s => s.Baits.Count > 0);
        ComponentCondition<StalkingWindStone>(id + 0x13, 5.1f, s => s.NumCasts > 2, "Baits 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<StalkingWindStone>(id + 0x14, 9.1f, s => s.Baits.Count > 0);
        ComponentCondition<StalkingWindStone>(id + 0x15, 5.1f, s => s.NumCasts > 4, "Baits 3")
            .ActivateOnEnter<RavenousSaber>()
            .SetHint(StateMachine.StateHint.Tankbuster);

        Targetable(id + 0x20, true, 18.1f, "Boss reappears");

        Timeout(id + 0x30, 5, "Adds enrage")
            .DeactivateOnExit<AddsVoidzone>()
            .DeactivateOnExit<WolfOfWindStone>()
            .DeactivateOnExit<StalkingWindStone>()
            .DeactivateOnExit<AlphaWindStone>()
            .DeactivateOnExit<ForlornWindStone>()
            .DeactivateOnExit<EarthyWindborneEnd>();

        ComponentCondition<RavenousSaber>(id + 0x40, 8.1f, r => r.NumCasts > 0, "Raidwide 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RavenousSaber>(id + 0x42, 3.9f, r => r.NumCasts > 4, "Raidwide 5")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<RavenousSaber>();
    }

    private void TerrestrialRage(uint id, float delay)
    {
        Cast(id, AID.TerrestrialRage, delay, 3, "TR start")
            .ActivateOnEnter<FangedCharge>()
            .ActivateOnEnter<Heavensearth>()
            .ActivateOnEnter<SuspendedStone>()
            .ActivateOnEnter<Shadowchase>()
            .ActivateOnEnter<RoaringWind>();

        ComponentCondition<FangedCharge>(id + 0x10, 7, f => f.NumCasts > 0, "Lines 1");
        ComponentCondition<Heavensearth>(id + 0x11, 1.4f, s => s.NumFinishedStacks > 0, "Stack/spread 1");
        ComponentCondition<FangedCharge>(id + 0x12, 1.2f, f => f.NumCasts > 2, "Lines 2")
            .DeactivateOnExit<FangedCharge>();
        ComponentCondition<Heavensearth>(id + 0x13, 5.4f, h => h.NumFinishedStacks > 1, "Stack/spread 2")
            .DeactivateOnExit<Heavensearth>();
        ComponentCondition<Shadowchase>(id + 0x14, 0.3f, s => s.NumCasts > 0, "Lines 3")
            .DeactivateOnExit<Shadowchase>()
            .ExecOnExit<RoaringWind>(r => r.Enabled = true);
        ComponentCondition<RoaringWind>(id + 0x15, 4.4f, r => r.NumCasts > 0, "Lines 4");
    }
}
