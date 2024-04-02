namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class C031KetudukeStates : StateMachineBuilder
{
    private bool _savage;

    public C031KetudukeStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        TidalRoar(id, 9.2f);
        FlukeGale(id + 0x10000, 8.5f);
        BlowingBubbles(id + 0x20000, 5.0f);
        StrewnBubbles(id + 0x30000, 1.9f);
        Roar(id + 0x40000, 5.6f);
        AngrySeas(id + 0x50000, 10.2f);
        FlukeGale(id + 0x60000, 5.7f);
        StrewnBubbles(id + 0x70000, 4.9f);
        TidalRoar(id + 0x80000, 5.5f);
        Cast(id + 0x90000, AID.Enrage, 7, 10, "Enrage");
    }

    private void TidalRoar(uint id, float delay)
    {
        Cast(id, AID.TidalRoar, delay, 5);
        ComponentCondition<TidalRoar>(id + 0x10, 1, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<NTidalRoar>(!_savage)
            .ActivateOnEnter<STidalRoar>(_savage)
            .DeactivateOnExit<TidalRoar>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State BubbleNet(uint id, float delay, bool variant2)
    {
        Cast(id, variant2 ? AID.BubbleNet2 : AID.BubbleNet1, delay, 4.1f);
        return ComponentCondition<BubbleNet>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<NBubbleNet1>(!variant2 && !_savage)
            .ActivateOnEnter<SBubbleNet1>(!variant2 && _savage)
            .ActivateOnEnter<NBubbleNet2>(variant2 && !_savage)
            .ActivateOnEnter<SBubbleNet2>(variant2 && _savage)
            .DeactivateOnExit<BubbleNet>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FlukeGale(uint id, float delay)
    {
        Cast(id, AID.SpringCrystals, delay, 2.2f)
            .ActivateOnEnter<SpringCrystalsRectMove>();
        BubbleNet(id + 0x10, 3, false)
            .ActivateOnEnter<FlukeGale>();
        CastMulti(id + 0x20, [AID.Hydrofall, AID.Hydrobullet], 2.2f, 4)
            .ActivateOnEnter<HydrofallHydrobullet>();
        Cast(id + 0x30, AID.FlukeGale, 6.2f, 3);
        ComponentCondition<FlukeGale>(id + 0x40, 2.1f, comp => comp.Gales.Count > 0)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<FlukeGale>(id + 0x50, 8, comp => comp.NumCasts >= 2, "Knockbacks 1");
        ComponentCondition<FlukeGale>(id + 0x51, 2, comp => comp.NumCasts >= 4, "Knockbacks 2")
            .ExecOnEnter<HydrofallHydrobullet>(comp => comp.Activate(Module, 0)) // TODO: consider activating earlier?..
            .DeactivateOnExit<FlukeGale>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        ComponentCondition<SpringCrystalsRect>(id + 0x60, 3.1f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<SpringCrystalsRect>();
        ComponentCondition<HydrofallHydrobullet>(id + 0x61, 0.2f, comp => comp.ActiveMechanic > 0, "Stack/spread")
            .DeactivateOnExit<HydrofallHydrobullet>();
    }

    private void BlowingBubbles(uint id, float delay)
    {
        CastMulti(id, [AID.Hydrofall, AID.Hydrobullet], delay, 4)
            .ActivateOnEnter<HydrofallHydrobullet>()
            .ExecOnEnter<HydrofallHydrobullet>(comp => comp.Activate(Module, 0));
        ComponentCondition<HydrofallHydrobullet>(id + 0x10, 3.1f, comp => comp.Mechanics.Count > 1);
        Cast(id + 0x20, AID.BlowingBubbles, 3.1f, 4.2f)
            .ActivateOnEnter<BlowingBubbles>();
        Cast(id + 0x30, AID.Hydrobomb, 5.0f, 2.2f);
        ComponentCondition<Hydrobomb>(id + 0x40, 1.1f, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<NHydrobomb>(!_savage)
            .ActivateOnEnter<SHydrobomb>(_savage)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<HydrofallHydrobullet>(id + 0x50, 3.8f, comp => comp.ActiveMechanic > 0, "Spread/stack");
        ComponentCondition<HydrofallHydrobullet>(id + 0x60, 6.1f, comp => comp.ActiveMechanic > 1, "Stack/spread")
            .DeactivateOnExit<HydrofallHydrobullet>();
        ComponentCondition<Hydrobomb>(id + 0x70, 5.2f, comp => comp.Casters.Count == 0, "Puddles/exaflares resolve")
            .DeactivateOnExit<Hydrobomb>()
            .DeactivateOnExit<BlowingBubbles>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void StrewnBubbles(uint id, float delay)
    {
        Cast(id, AID.Hydrofall, delay, 4)
            .ActivateOnEnter<HydrofallHydrobullet>()
            .ExecOnEnter<HydrofallHydrobullet>(comp => comp.Activate(Module, 0));
        Cast(id + 0x10, AID.StrewnBubbles, 3.2f, 2.2f)
            .ActivateOnEnter<StrewnBubbles>(); // first set appears ~1.4s after cast end
        CastStartMulti(id + 0x20, [_savage ? AID.SRecedingTwintides : AID.NRecedingTwintides, _savage ? AID.SEncroachingTwintides : AID.NEncroachingTwintides], 6.5f)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x21, 5, "In/out")
            .ActivateOnEnter<RecedingEncroachingTwintides>();
        ComponentCondition<StrewnBubbles>(id + 0x22, 0.6f, comp => comp.NumCasts > 0);
        ComponentCondition<RecedingEncroachingTwintides>(id + 0x30, 2.5f, comp => comp.NumCasts > 1, "Out/in")
            .DeactivateOnExit<RecedingEncroachingTwintides>();
        ComponentCondition<StrewnBubbles>(id + 0x31, 0.5f, comp => comp.NumCasts > 4)
            .DeactivateOnExit<StrewnBubbles>();
        ComponentCondition<HydrofallHydrobullet>(id + 0x32, 0.1f, comp => comp.ActiveMechanic > 0, "Stack")
            .DeactivateOnExit<HydrofallHydrobullet>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Roar(uint id, float delay)
    {
        Cast(id, AID.Hydrobullet, delay, 4)
            .ActivateOnEnter<HydrofallHydrobullet>()
            .ExecOnEnter<HydrofallHydrobullet>(comp => comp.Activate(Module, 0));
        Cast(id + 0x10, AID.Roar, 3.2f, 3)
            .ActivateOnEnter<Roar>(); // zaratans spawn ~1.2s after cast ends
        Cast(id + 0x20, AID.SpringCrystals, 2.6f, 2.2f)
            .ActivateOnEnter<SpringCrystalsRectStay>();
        BubbleNet(id + 0x30, 6.0f, true)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<HydrofallHydrobullet>(id + 0x40, 11.5f, comp => comp.ActiveMechanic > 0, "Spread")
            .DeactivateOnExit<HydrofallHydrobullet>();
        ComponentCondition<SpringCrystalsRect>(id + 0x41, 0.6f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<SpringCrystalsRect>();
        Cast(id + 0x50, AID.Updraft, 1.0f, 4.2f)
            .ExecOnEnter<Roar>(comp => comp.Active = true);
        ComponentCondition<Roar>(id + 0x60, 2.6f, comp => comp.NumCasts > 0, "Bait resolve")
            .DeactivateOnExit<Roar>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void AngrySeas(uint id, float delay)
    {
        CastMulti(id, [AID.Hydrofall, AID.Hydrobullet], delay, 4)
            .ActivateOnEnter<HydrofallHydrobullet>()
            .ExecOnEnter<HydrofallHydrobullet>(comp => comp.Activate(Module, 0));
        ComponentCondition<HydrofallHydrobullet>(id + 0x10, 3.1f, comp => comp.Mechanics.Count > 1);
        Cast(id + 0x20, AID.AngrySeas, 3.1f, 4.2f)
            .ActivateOnEnter<AngrySeasAOE>()
            .ActivateOnEnter<AngrySeasKnockback>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<AngrySeasKnockback>(id + 0x22, 0.8f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<AngrySeasKnockback>();
        ComponentCondition<HydrofallHydrobullet>(id + 0x30, 1.2f, comp => comp.ActiveMechanic > 0, "Stack/spread");

        Cast(id + 0x40, AID.SpringCrystals, 0.9f, 2.2f)
            .ActivateOnEnter<SpringCrystalsSphere>();
        ComponentCondition<HydrofallHydrobullet>(id + 0x50, 2.0f, comp => comp.ActiveMechanic > 1, "Spread/stack")
            .DeactivateOnExit<HydrofallHydrobullet>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        BubbleNet(id + 0x60, 0.9f, false)
            .ActivateOnEnter<FlukeTyphoonBurst>();

        Cast(id + 0x100, AID.FlukeTyphoon, 2.2f, 3, "Bubbles");
        ComponentCondition<FlukeTyphoon>(id + 0x110, 6.1f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<FlukeTyphoon>()
            .DeactivateOnExit<FlukeTyphoon>();
        ComponentCondition<SpringCrystalsSphere>(id + 0x120, 2.6f, comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<SpringCrystalsSphere>();
        ComponentCondition<FlukeTyphoonBurst>(id + 0x130, 2.4f, comp => comp.NumCasts > 0, "Towers")
            .DeactivateOnExit<FlukeTyphoonBurst>()
            .DeactivateOnExit<AngrySeasAOE>();
    }
}
class C031NKetudukeStates : C031KetudukeStates { public C031NKetudukeStates(BossModule module) : base(module, false) { } }
class C031SKetudukeStates : C031KetudukeStates { public C031SKetudukeStates(BossModule module) : base(module, true) { } }
