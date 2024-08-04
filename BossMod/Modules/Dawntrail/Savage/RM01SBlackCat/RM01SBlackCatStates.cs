namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class RM01SBlackCatStates : StateMachineBuilder
{
    public RM01SBlackCatStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        QuadrupleCrossing(id, 8.3f);
        BiscuitMaker(id + 0x10000, 4.5f);
        NineLives1(id + 0x20000, 12.1f);
        BloodyScratch(id + 0x30000, 5.6f);
        Mouser1(id + 0x40000, 13.4f);
        NineLives2(id + 0x50000, 12.4f);
    }

    private void BiscuitMaker(uint id, float delay)
    {
        Cast(id, AID.BiscuitMaker, delay, 5, "Tankbuster 1")
            .ActivateOnEnter<BiscuitMaker>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<BiscuitMaker>(id + 0x10, 2, comp => comp.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<BiscuitMaker>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void BloodyScratch(uint id, float delay)
    {
        Cast(id, AID.BloodyScratch, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void QuadrupleCrossing(uint id, float delay)
    {
        Cast(id, AID.QuadrupleCrossingFirst, delay, 5)
            .ActivateOnEnter<QuadrupleCrossingProtean>();
        ComponentCondition<QuadrupleCrossingProtean>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Proteans 1")
            .ActivateOnEnter<QuadrupleCrossingAOE>();
        Cast(id + 0x10, AID.QuadrupleCrossingMid, 1.2f, 1);
        ComponentCondition<QuadrupleCrossingProtean>(id + 0x12, 0.8f, comp => comp.NumCasts > 4, "Proteans 2")
            .DeactivateOnExit<QuadrupleCrossingProtean>();
        Cast(id + 0x20, AID.QuadrupleCrossingMid, 1.2f, 1);
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x22, 0.7f, comp => comp.NumCasts > 0, "Cones 1");
        Cast(id + 0x30, AID.QuadrupleCrossingLast, 1.3f, 1);
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x32, 0.7f, comp => comp.NumCasts > 4, "Cones 2")
            .DeactivateOnExit<QuadrupleCrossingAOE>();
    }

    private void NineLives1(uint id, float delay)
    {
        Cast(id, AID.NineLives, delay, 3);
        CastMulti(id + 0x10, [AID.OneTwoPawBossRL, AID.OneTwoPawBossLR], 2, 5)
            .ActivateOnEnter<OneTwoPawBoss>()
            .ActivateOnEnter<OneTwoPawShade>();
        ComponentCondition<OneTwoPawBoss>(id + 0x20, 1, comp => comp.NumCasts > 0, "Cleave 1");
        ComponentCondition<OneTwoPawBoss>(id + 0x21, 3, comp => comp.NumCasts > 1, "Cleave 2")
            .DeactivateOnExit<OneTwoPawBoss>();
        Cast(id + 0x30, AID.Soulshade, 1.1f, 3);
        Cast(id + 0x40, AID.NineLives, 11.4f, 3);
        ComponentCondition<OneTwoPawShade>(id + 0x50, 3, comp => comp.NumCasts > 0, "Clone cleaves 1");
        CastStartMulti(id + 0x60, [AID.QuadrupleSwipeBoss, AID.DoubleSwipeBoss], 1);
        ComponentCondition<OneTwoPawShade>(id + 0x61, 2, comp => comp.NumCasts > 2, "Clone cleaves 2")
            .ActivateOnEnter<QuadrupleSwipeBoss>()
            .ActivateOnEnter<DoubleSwipeBoss>()
            .DeactivateOnExit<OneTwoPawShade>();
        CastEnd(id + 0x62, 2);
        Condition(id + 0x63, 1, () => Module.FindComponent<QuadrupleSwipeBoss>()?.NumFinishedStacks > 0 || Module.FindComponent<DoubleSwipeBoss>()?.NumFinishedStacks > 0, "Pair/party stacks")
            .DeactivateOnExit<QuadrupleSwipeBoss>()
            .DeactivateOnExit<DoubleSwipeBoss>();
        Cast(id + 0x70, AID.Soulshade, 2.1f, 3);

        CastMulti(id + 0x100, [AID.LeapingQuadrupleCrossingBossR, AID.LeapingQuadrupleCrossingBossL], 2.1f, 5)
            .ActivateOnEnter<QuadrupleCrossingProtean>();
        ComponentCondition<QuadrupleCrossingProtean>(id + 0x102, 1.8f, comp => comp.NumCasts > 0, "Jump + Proteans 1")
            .ActivateOnEnter<QuadrupleCrossingAOE>();
        Cast(id + 0x110, AID.LeapingQuadrupleCrossingBossMid, 1.2f, 1);
        ComponentCondition<QuadrupleCrossingProtean>(id + 0x112, 0.8f, comp => comp.NumCasts > 4, "Proteans 2")
            .DeactivateOnExit<QuadrupleCrossingProtean>();
        Cast(id + 0x120, AID.LeapingQuadrupleCrossingBossMid, 1.2f, 1)
            .ActivateOnEnter<QuadrupleSwipeShade>() // TODO: consider activating earlier?..
            .ActivateOnEnter<DoubleSwipeShade>();
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x122, 0.7f, comp => comp.NumCasts > 0, "Cones 1");
        Cast(id + 0x130, AID.LeapingQuadrupleCrossingBossLast, 1.3f, 1);
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x132, 0.7f, comp => comp.NumCasts > 4, "Cones 2")
            .DeactivateOnExit<QuadrupleCrossingAOE>();
        Condition(id + 0x140, 0.8f, () => Module.FindComponent<QuadrupleSwipeShade>()?.NumFinishedStacks > 0 || Module.FindComponent<DoubleSwipeShade>()?.NumFinishedStacks > 0, "Pair/party stacks")
            .DeactivateOnExit<QuadrupleSwipeShade>()
            .DeactivateOnExit<DoubleSwipeShade>();
    }

    private void NineLives2(uint id, float delay)
    {

    }

    private void Mouser1(uint id, float delay)
    {
        Cast(id, AID.Mouser, delay, 10)
            .ActivateOnEnter<Mouser>();
        ComponentCondition<Mouser>(id + 0x10, 1.5f, comp => comp.NumCasts >= 3, "Tiles start");
        ComponentCondition<Mouser>(id + 0x11, 1, comp => comp.NumCasts >= 6);
        ComponentCondition<Mouser>(id + 0x12, 1, comp => comp.NumCasts >= 9);
        ComponentCondition<Mouser>(id + 0x13, 1, comp => comp.NumCasts >= 12);
        ComponentCondition<Mouser>(id + 0x14, 1, comp => comp.NumCasts >= 15);
        ComponentCondition<Mouser>(id + 0x15, 1, comp => comp.NumCasts >= 18);
        ComponentCondition<Mouser>(id + 0x16, 1, comp => comp.NumCasts >= 21);
        ComponentCondition<Mouser>(id + 0x17, 1, comp => comp.NumCasts >= 24);
        ComponentCondition<Mouser>(id + 0x18, 1, comp => comp.NumCasts >= 26);
        ComponentCondition<Mouser>(id + 0x19, 1, comp => comp.NumCasts >= 28, "Tiles finish");
        Cast(id + 0x20, AID.Copycat, 7.6f, 3);
        ComponentCondition<ElevateAndEviscerate>(id + 0x30, 13.1f, comp => comp.NumCasts >= 1, "Jump 1")
            .ActivateOnEnter<ElevateAndEviscerate>();
        ComponentCondition<ElevateAndEviscerate>(id + 0x40, 11.1f, comp => comp.NumCasts >= 2, "Jump 2");
        ComponentCondition<ElevateAndEviscerate>(id + 0x50, 11.1f, comp => comp.NumCasts >= 3, "Jump 3");
        ComponentCondition<ElevateAndEviscerate>(id + 0x60, 11.1f, comp => comp.NumCasts >= 4, "Jump 4")
            .DeactivateOnExit<ElevateAndEviscerate>();

        BiscuitMaker(id + 0x100, 5.4f);

        Cast(id + 0x200, AID.GrimalkinGaleShockwave, 4, 6)
            .ActivateOnEnter<GrimalkinGaleShockwave>();
        ComponentCondition<GrimalkinGaleShockwave>(id + 0x210, 1, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<GrimalkinGaleShockwave>();
        ComponentCondition<GrimalkinGaleSpread>(id + 0x220, 3.8f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .ActivateOnEnter<GrimalkinGaleSpread>() // note: casts start ~1.2s earlier, but no point showing hints before kb
            .DeactivateOnExit<GrimalkinGaleSpread>()
            .DeactivateOnExit<Mouser>(); // full repairs happen ~0.2s after spreads resolve
    }
}
