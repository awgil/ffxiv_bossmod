namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

class C033StaticeStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C033StaticeStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Aero(id, 8.2f);
        TrickReload1(id + 0x10000, 5.7f);
        TrickReload2(id + 0x20000, 8.6f);
        Aero(id + 0x30000, 3.1f);
        Intermission(id + 0x40000, 7.2f);
        ShockingAbandon(id + 0x50000, 1.2f);
        PinwheelingDartboard(id + 0x60000, 7.2f);
        Aero(id + 0x70000, 3.1f);
        TrickReload3(id + 0x80000, 8.8f);
        Aero(id + 0x90000, 2.0f);
        Aero(id + 0xA0000, 3.2f);
        Cast(id + 0xB0000, AID.Enrage, 3.2f, 10, "Enrage");
    }

    private void Aero(uint id, float delay)
    {
        Cast(id, _savage ? AID.SAero : AID.NAero, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ShockingAbandon(uint id, float delay)
    {
        Cast(id, _savage ? AID.SShockingAbandon : AID.NShockingAbandon, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TrickReload1(uint id, float delay)
    {
        Cast(id, _savage ? AID.STrickReload : AID.NTrickReload, delay, 4)
            .ActivateOnEnter<TrickReload>();
        Cast(id + 0x10, _savage ? AID.STrapshooting1 : AID.NTrapshooting1, 10.7f, 4)
            .ActivateOnEnter<Trapshooting>();
        ComponentCondition<Trapshooting>(id + 0x12, 4.1f, comp => comp.NumResolves > 0, "Stack/spread");
        Cast(id + 0x20, _savage ? AID.STriggerHappy : AID.NTriggerHappy, 2.1f, 4.3f)
            .ActivateOnEnter<NTriggerHappy>(!_savage)
            .ActivateOnEnter<STriggerHappy>(_savage);
        ComponentCondition<TriggerHappy>(id + 0x22, 0.7f, comp => comp.NumCasts > 0, "Pizza")
            .DeactivateOnExit<TriggerHappy>();
        Cast(id + 0x30, _savage ? AID.SRingARingOExplosions : AID.NRingARingOExplosions, 4.2f, 3)
            .ActivateOnEnter<RingARingOExplosions>();
        Cast(id + 0x40, _savage ? AID.STrapshooting2 : AID.NTrapshooting2, 10.2f, 4);
        ComponentCondition<RingARingOExplosions>(id + 0x50, 4.0f, comp => comp.NumCasts > 0, "Bombs")
            .DeactivateOnExit<RingARingOExplosions>();
        ComponentCondition<Trapshooting>(id + 0x51, 0.1f, comp => comp.NumResolves > 1, "Spread/stack")
            .DeactivateOnExit<Trapshooting>()
            .DeactivateOnExit<TrickReload>();
    }

    private void TrickReload2(uint id, float delay)
    {
        Cast(id, _savage ? AID.STrickReload : AID.NTrickReload, delay, 4)
            .ActivateOnEnter<TrickReload>();
        Cast(id + 0x10, _savage ? AID.SRingARingOExplosions : AID.NRingARingOExplosions, 10.7f, 3)
            .ActivateOnEnter<RingARingOExplosions>();
        Cast(id + 0x20, _savage ? AID.SDartboardOfDancingExplosives : AID.NDartboardOfDancingExplosives, 2.2f, 3)
            .ActivateOnEnter<Dartboard>(); // bullseye statuses are applied on 3 players ~0.6s after cast end
        Cast(id + 0x30, _savage ? AID.STrapshooting2 : AID.NTrapshooting2, 9.7f, 4)
            .ActivateOnEnter<Trapshooting>();
        ComponentCondition<RingARingOExplosions>(id + 0x40, 1.9f, comp => comp.NumCasts > 0, "Bombs")
            .DeactivateOnExit<RingARingOExplosions>();
        ComponentCondition<Trapshooting>(id + 0x41, 2.2f, comp => comp.NumResolves > 0, "Stack/spread");
        ComponentCondition<Dartboard>(id + 0x42, 1.1f, comp => comp.NumCasts > 0, "Colors")
            .DeactivateOnExit<Dartboard>();

        Cast(id + 0x100, _savage ? AID.SSurpriseBalloon : AID.NSurpriseBalloon, 8.5f, 4);
        Cast(id + 0x110, _savage ? AID.SBeguilingGlitter : AID.NBeguilingGlitter, 3.2f, 4);
        CastStart(id + 0x120, _savage ? AID.STriggerHappy : AID.NTriggerHappy, 3.2f);
        ComponentCondition<SurpriseBalloon>(id + 0x121, 2.7f, comp => comp.NumCasts > 0, "Knockback 1")
            .ActivateOnEnter<NTriggerHappy>(!_savage)
            .ActivateOnEnter<STriggerHappy>(_savage)
            .ActivateOnEnter<NSurpriseBalloon>(!_savage)
            .ActivateOnEnter<SSurpriseBalloon>(_savage);
        CastEnd(id + 0x122, 1.6f);
        ComponentCondition<TriggerHappy>(id + 0x123, 0.7f, comp => comp.NumCasts > 0, "Pizza")
            .DeactivateOnExit<TriggerHappy>();
        ComponentCondition<SurpriseBalloon>(id + 0x124, 2.5f, comp => comp.NumCasts > 1, "Knockback 2")
            .DeactivateOnExit<SurpriseBalloon>();

        Cast(id + 0x130, _savage ? AID.STrapshooting2 : AID.NTrapshooting2, 0.7f, 4)
            .ActivateOnEnter<BeguilingGlitter>();
        ComponentCondition<BeguilingGlitter>(id + 0x132, 1.2f, comp => comp.NumActiveForcedMarches > 0, "Forced march");
        ComponentCondition<Trapshooting>(id + 0x133, 2.9f, comp => comp.NumResolves > 1, "Spread/stack")
            .DeactivateOnExit<BeguilingGlitter>() // forced marches end ~0.9s before resolve
            .DeactivateOnExit<Trapshooting>()
            .DeactivateOnExit<TrickReload>();
    }

    private void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Boss disappears");
        Cast(id + 0x10, _savage ? AID.SRingARingOExplosions : AID.NRingARingOExplosions, 1.5f, 3)
            .ActivateOnEnter<RingARingOExplosions>();
        Cast(id + 0x20, _savage ? AID.SPresentBox : AID.NPresentBox, 2.1f, 3)
            .ActivateOnEnter<Fireworks>()
            .ActivateOnEnter<Fireworks1Hints>();
        // +0.9s: spawn 4x staffs, 2x missiles/claws
        // +1.6s: missiles/claws tether to players
        Cast(id + 0x30, _savage ? AID.SFireworks : AID.NFireworks, 2.1f, 3)
            .ActivateOnEnter<NFaerieRing>(!_savage) // casts start ~2.2s into cast
            .ActivateOnEnter<SFaerieRing>(_savage);
        ComponentCondition<BurningChains>(id + 0x40, 4.6f, comp => comp.Active, "Chains")
            .ActivateOnEnter<BurningChains>();
        ComponentCondition<Fireworks>(id + 0x50, 5.1f, comp => comp.Spreads.Count == 0, "Stack/spread")
            .DeactivateOnExit<BurningChains>();
        ComponentCondition<Fireworks>(id + 0x51, 0.1f, comp => !comp.Active)
            .DeactivateOnExit<Fireworks1Hints>()
            .DeactivateOnExit<Fireworks>();
        ComponentCondition<RingARingOExplosions>(id + 0x52, 0.1f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<RingARingOExplosions>();
        ComponentCondition<FaerieRing>(id + 0x53, 0.2f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<FaerieRing>();
        Targetable(id + 0x60, true, 2.0f, "Boss reappears");
    }

    private void PinwheelingDartboard(uint id, float delay)
    {
        Cast(id, _savage ? AID.SPinwheelingDartboard : AID.NPinwheelingDartboard, delay, 3)
            .ActivateOnEnter<Dartboard>()
            .ActivateOnEnter<FireSpread>() // first cast starts ~2.6s after cast-end
            .ActivateOnEnter<Fireworks>()
            .ActivateOnEnter<Fireworks2Hints>();
        Cast(id + 0x10, _savage ? AID.SFireworks : AID.NFireworks, 5.7f, 3);
        ComponentCondition<FireSpread>(id + 0x20, 1.9f, comp => comp.NumCasts > 0);
        ComponentCondition<BurningChains>(id + 0x30, 2.7f, comp => comp.Active, "Chains")
            .ActivateOnEnter<BurningChains>();
        ComponentCondition<Fireworks>(id + 0x40, 5.1f, comp => !comp.Active, "Stack/spread")
            .DeactivateOnExit<Fireworks2Hints>()
            .DeactivateOnExit<BurningChains>()
            .DeactivateOnExit<Fireworks>();
        ComponentCondition<Dartboard>(id + 0x50, 0.4f, comp => comp.NumCasts > 0, "Colors")
            .DeactivateOnExit<Dartboard>();
        ComponentCondition<FireSpread>(id + 0x60, 3.9f, comp => comp.NumCasts >= 36, "Fire wall resolve")
            .DeactivateOnExit<FireSpread>();
    }

    private void TrickReload3(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBeguilingGlitter : AID.NBeguilingGlitter, delay, 4);
        Cast(id + 0x10, _savage ? AID.STrickReload : AID.NTrickReload, 3.2f, 4)
            .ActivateOnEnter<TrickReload>();
        Cast(id + 0x20, _savage ? AID.STrapshooting1 : AID.NTrapshooting1, 10.7f, 4)
            .ActivateOnEnter<Trapshooting>();
        ComponentCondition<Trapshooting>(id + 0x22, 4.1f, comp => comp.NumResolves > 0, "Stack/spread");

        Cast(id + 0x30, _savage ? AID.SPresentBox : AID.NPresentBox, 0.1f, 3);
        // +1.0s: spawn 4x staffs
        Cast(id + 0x40, _savage ? AID.SRingARingOExplosions : AID.NRingARingOExplosions, 3.6f, 3)
            .ActivateOnEnter<BeguilingGlitter>()
            .ActivateOnEnter<NFaerieRing>(!_savage)
            .ActivateOnEnter<SFaerieRing>(_savage);
        Cast(id + 0x50, _savage ? AID.STriggerHappy : AID.NTriggerHappy, 3.1f, 4.3f)
            .ActivateOnEnter<NTriggerHappy>(!_savage) // TODO: ideally we'd like to show this earlier...
            .ActivateOnEnter<STriggerHappy>(_savage);
        ComponentCondition<FaerieRing>(id + 0x52, 0.6f, comp => comp.NumCasts > 0, "Donuts")
            .DeactivateOnExit<FaerieRing>();
        ComponentCondition<TriggerHappy>(id + 0x53, 0.1f, comp => comp.NumCasts > 0, "Pizza")
            .DeactivateOnExit<TriggerHappy>();

        CastStart(id + 0x60, _savage ? AID.STrapshooting2 : AID.NTrapshooting2, 2.1f)
            .ActivateOnEnter<RingARingOExplosions>();
        CastEnd(id + 0x61, 4);
        ComponentCondition<RingARingOExplosions>(id + 0x62, 4.0f, comp => comp.NumCasts > 0, "Bombs")
            .DeactivateOnExit<RingARingOExplosions>();
        ComponentCondition<Trapshooting>(id + 0x63, 0.1f, comp => comp.NumResolves > 1, "Spread/stack")
            .DeactivateOnExit<BeguilingGlitter>()
            .DeactivateOnExit<Trapshooting>()
            .DeactivateOnExit<TrickReload>();
    }
}
class C033NStaticeStates(BossModule module) : C033StaticeStates(module, false);
class C033SStaticeStates(BossModule module) : C033StaticeStates(module, true);
