namespace BossMod.Endwalker.Alliance.A14Naldthal;

public class A14NaldthalStates : StateMachineBuilder
{
    public A14NaldthalStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        AsAboveSoBelow(id, 6.7f);
        HeatAboveFlamesBelow(id + 0x10000, 2.2f);
        AsAboveSoBelow(id + 0x20000, 4.1f);
        HeatAboveFlamesBelow(id + 0x30000, 2.2f);
        HeavensTrial(id + 0x40000, 6.2f);
        GoldenTenet(id + 0x50000, 2.3f);
        AsAboveSoBelow(id + 0x60000, 6.0f);
        FarAboveDeepBelow(id + 0x70000, 2.2f);
        OnceAboveEverBelow(id + 0x80000, 2.2f);
        HellOfFire(id + 0x90000, 7); // note: large variance (5 to 9)
        WaywardSoul(id + 0xA0000, 7); // note: large variance (6.5 to 7.5)
        HellOfFire(id + 0xB0000, 6.2f);
        FiredUp(id + 0xC0000, 11.9f, false);
        FiredUp(id + 0xD0000, 2.1f, true);
        SoulMeasure(id + 0xE0000, 4.4f);

        // note: mechanics below have many variations...
        AsAboveSoBelow(id + 0x100000, 6.4f);
        OnceAboveEverBelowHeavensTrialOrStygianTenet(id + 0x110000, 2.2f);
        AsAboveSoBelow(id + 0x120000, 4.7f);
        HearthAboveFlightBelow(id + 0x130000, 2.2f);
        HellOfFire(id + 0x140000, 8.4f); // note: 5.4 if previous was hell's trial
        WaywardSoulHellOfFire(id + 0x150000, 9.5f); // TODO: sometimes we can get fired up here instead?..
        StygianTenet(id + 0x160000, 4.2f);
        HellsTrial(id + 0x170000, 9.7f);
        AsAboveSoBelow(id + 0x180000, 5.5f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private State AsAboveSoBelow(uint id, float delay)
    {
        return CastMulti(id, new[] { AID.AsAboveSoBelowNald, AID.AsAboveSoBelowThal }, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HellsTrial(uint id, float delay)
    {
        Cast(id, AID.HellsTrial, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HeavensTrial(uint id, float delay)
    {
        CastStart(id, AID.HeavensTrial, delay)
            .ActivateOnEnter<HeavensTrialCone>();
        CastEnd(id + 1, 5)
            .ActivateOnEnter<HeavensTrialStack>();
        ComponentCondition<HeavensTrialStack>(id + 2, 0.5f, comp => !comp.Active, "Stack")
            .DeactivateOnExit<HeavensTrialStack>();
        ComponentCondition<HeavensTrialCone>(id + 3, 0.4f, comp => comp.NumCasts > 0, "Baited cones")
            .DeactivateOnExit<HeavensTrialCone>();
    }

    private State GoldenTenet(uint id, float delay)
    {
        Cast(id, AID.GoldenTenet, delay, 5)
            .ActivateOnEnter<GoldenTenet>();
        return ComponentCondition<GoldenTenet>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Shared tankbuster")
            .DeactivateOnExit<GoldenTenet>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void StygianTenet(uint id, float delay)
    {
        Cast(id, AID.StygianTenet, delay, 5)
            .ActivateOnEnter<StygianTenet>();
        ComponentCondition<StygianTenet>(id + 0x10, 0.5f, comp => comp.NumFinishedSpreads > 0, "Tankbusters")
            .DeactivateOnExit<StygianTenet>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HeatAboveFlamesBelow(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, new[] { AID.HeatAboveFlamesBelowNald, AID.HeatAboveFlamesBelowThal }, delay);
        ComponentCondition<HeatAboveFlamesBelow>(id + 1, 12, comp => comp.NumCasts > 0, "In or out")
            .ActivateOnEnter<HeatAboveFlamesBelow>()
            .DeactivateOnExit<HeatAboveFlamesBelow>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
    }

    private void FarAboveDeepBelow(uint id, float delay)
    {
        CastStartMulti(id, new[] { AID.FarAboveDeepBelowThal, AID.FarAboveDeepBelowNald }, delay)
            .ActivateOnEnter<FarFlungFire>()
            .ActivateOnEnter<DeepestPit>();
        CastEnd(id + 1, 12);
        Condition(id + 0x10, 0.9f, () => Module.FindComponent<FarFlungFire>()!.NumCasts > 0 || Module.FindComponent<DeepestPit>()!.Active, "Line stack or baited puddles start") // note: deepest pit start is 1.4s instead
            .DeactivateOnExit<FarFlungFire>();
        AsAboveSoBelow(id + 0x100, 5.3f) // note: 5.8s for deepest pit
            .DeactivateOnExit<DeepestPit>();
    }

    private void OnceAboveEverBelowStart(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, new[] { AID.OnceAboveEverBelowThalNald, AID.OnceAboveEverBelowThal, AID.OnceAboveEverBelowNaldThal, AID.OnceAboveEverBelowNald }, delay);
        ComponentCondition<OnceAboveEverBelow>(id + 2, 12.6f, comp => comp.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<OnceAboveEverBelow>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
    }

    private void OnceAboveEverBelow(uint id, float delay)
    {
        OnceAboveEverBelowStart(id, delay);
        ComponentCondition<OnceAboveEverBelow>(id + 0x10, 6, comp => comp.NumCasts > 30, "Exaflares end")
            .DeactivateOnExit<OnceAboveEverBelow>();
    }

    private void OnceAboveEverBelowHeavensTrialOrStygianTenet(uint id, float delay)
    {
        OnceAboveEverBelowStart(id, delay);
        CastStartMulti(id + 0x10, new[] { AID.HeavensTrial, AID.StygianTenet }, 5.6f)
            .ActivateOnEnter<HeavensTrialCone>();
        ComponentCondition<OnceAboveEverBelow>(id + 0x20, 0.4f, comp => comp.NumCasts > 30)
            .ActivateOnEnter<HeavensTrialStack>()
            .ActivateOnEnter<StygianTenet>()
            .DeactivateOnExit<OnceAboveEverBelow>();
        CastEnd(id + 0x30, 4.6f);
        Condition(id + 0x40, 0.5f, () => Module.FindComponent<HeavensTrialStack>()!.NumFinishedStacks > 0 || Module.FindComponent<StygianTenet>()!.NumFinishedSpreads > 0, "Tankbusters -or- Stack & baited cones")
            .DeactivateOnExit<StygianTenet>()
            .DeactivateOnExit<HeavensTrialStack>()
            .DeactivateOnExit<HeavensTrialCone>(); // note: cone resolves ~0.4s later, but we don't care
    }

    private void HearthAboveFlightBelow(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, new[] { AID.HearthAboveFlightBelowThalNald, AID.HearthAboveFlightBelowThal, AID.HearthAboveFlightBelowNald, AID.HearthAboveFlightBelowNaldThal }, delay)
            .ActivateOnEnter<FarFlungFire>()
            .ActivateOnEnter<DeepestPit>();
        ComponentCondition<HeatAboveFlamesBelow>(id + 1, 12, comp => comp.NumCasts > 0, "In or out")
            .ActivateOnEnter<HeatAboveFlamesBelow>()
            .DeactivateOnExit<HeatAboveFlamesBelow>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
        Condition(id + 0x10, 0.9f, () => Module.FindComponent<FarFlungFire>()!.NumCasts > 0 || Module.FindComponent<DeepestPit>()!.Active, "Line stack or baited puddles start") // note: deepest pit start is 1.4s instead; sometimes we get 0.1 delay instead
            .DeactivateOnExit<FarFlungFire>();
        // orange => golden tenet, blue => hell's trial
        CastMulti(id + 0x100, new[] { AID.GoldenTenet, AID.HellsTrial }, 5.3f, 5, "Shared tankbuster -or- Raidwide")
            .ActivateOnEnter<GoldenTenet>()
            .DeactivateOnExit<DeepestPit>() // last puddle ends ~3s into cast
            .DeactivateOnExit<GoldenTenet>(); // note: actual aoe happens ~0.5s later, but that would complicate the condition...
    }

    private State HellOfFire(uint id, float delay)
    {
        CastMulti(id, new[] { AID.HellOfFireFront, AID.HellOfFireBack }, delay, 8)
            .ActivateOnEnter<HellOfFireFront>()
            .ActivateOnEnter<HellOfFireBack>();
        return Condition(id + 2, 1, () => Module.FindComponent<HellOfFireFront>()!.NumCasts + Module.FindComponent<HellOfFireBack>()!.NumCasts > 0, "Half-arena cleave")
            .DeactivateOnExit<HellOfFireFront>()
            .DeactivateOnExit<HellOfFireBack>();
    }

    private void WaywardSoulStart(uint id, float delay)
    {
        Cast(id, AID.WaywardSoul, delay, 3);
        ComponentCondition<WaywardSoul>(id + 0x10, 0.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<WaywardSoul>();
        ComponentCondition<WaywardSoul>(id + 0x20, 8, comp => comp.NumCasts > 0, "Circles start");
        // +5.5s: second set of 3
        // +11.0s: third set of 3
    }

    private void WaywardSoul(uint id, float delay)
    {
        WaywardSoulStart(id, delay);
        ComponentCondition<WaywardSoul>(id + 0x100, 32.2f, comp => comp.Casters.Count == 0, "Circles resolve")
            .DeactivateOnExit<WaywardSoul>();
    }

    private void WaywardSoulHellOfFire(uint id, float delay)
    {
        WaywardSoulStart(id, delay);
        HellOfFire(id + 0x100, 14.1f) // sometimes it's 9.2s instead...
            .DeactivateOnExit<WaywardSoul>(); // last aoe ends ~2.5s into cast
    }

    private void FiredUp(uint id, float delay, bool three)
    {
        CastMulti(id, new[] { AID.FiredUp1Knockback, AID.FiredUp1AOE }, delay, 4)
            .ActivateOnEnter<FortuneFluxOrder>()
            .ActivateOnEnter<FortuneFluxAOE>()
            .ActivateOnEnter<FortuneFluxKnockback>();
        CastMulti(id + 0x10, new[] { AID.FiredUp2Knockback, AID.FiredUp2AOE }, 2.1f, 4);
        if (three)
            CastMulti(id + 0x20, new[] { AID.FiredUp3Knockback, AID.FiredUp3AOE }, 2.1f, 4);

        Cast(id + 0x100, AID.FortuneFlux, 2.1f, 8);
        ComponentCondition<FortuneFluxOrder>(id + 0x110, 2.5f, comp => comp.NumComplete > 0, "AOE/Knockback 1");
        var resolve = ComponentCondition<FortuneFluxOrder>(id + 0x120, 2.0f, comp => comp.NumComplete > 1, "AOE/Knockback 2");
        if (three)
            resolve = ComponentCondition<FortuneFluxOrder>(id + 0x130, 1.5f, comp => comp.NumComplete > 2, "AOE/Knockback 3");
        resolve
            .DeactivateOnExit<FortuneFluxAOE>()
            .DeactivateOnExit<FortuneFluxOrder>()
            .DeactivateOnExit<FortuneFluxKnockback>();
    }

    private void SoulMeasure(uint id, float delay)
    {
        Cast(id, AID.SoulsMeasure, delay, 6);
        Targetable(id + 0x10, false, 1.1f, "Boss disappears");
        ComponentCondition<SoulVessel>(id + 0x20, 20.6f, comp => comp.ActiveActors.Any(), "Adds appear")
            .ActivateOnEnter<SoulVessel>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SoulVessel>(id + 0x30, 100, comp => !comp.ActiveActors.Any(), "Adds enrage")
            .ActivateOnEnter<Twingaze>()
            .ActivateOnEnter<MagmaticSpell>()
            .DeactivateOnExit<Twingaze>()
            .DeactivateOnExit<MagmaticSpell>()
            .DeactivateOnExit<SoulVessel>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Cast(id + 0x100, AID.Balance, 5.2f, 12.5f, "Balance check");
        ComponentCondition<TippedScales>(id + 0x110, 38.2f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<TippedScales>()
            .DeactivateOnExit<TippedScales>();
        Targetable(id + 0x120, true, 8.1f, "Boss reappears");
    }
}
