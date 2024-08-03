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
        BiscuitMarker(id + 0x10000, 4.5f);
        NineLives1(id + 0x20000, 12.1f);
        BloodyScratch(id + 0x30000, 5.6f);
    }

    private void BiscuitMarker(uint id, float delay)
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
        Cast(id + 0x110, AID.QuadrupleCrossingMid, 1.2f, 1);
        ComponentCondition<QuadrupleCrossingProtean>(id + 0x112, 0.8f, comp => comp.NumCasts > 4, "Proteans 2")
            .DeactivateOnExit<QuadrupleCrossingProtean>();
        Cast(id + 0x120, AID.QuadrupleCrossingMid, 1.2f, 1)
            .ActivateOnEnter<QuadrupleSwipeShade>() // TODO: consider activating earlier?..
            .ActivateOnEnter<DoubleSwipeShade>();
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x122, 0.7f, comp => comp.NumCasts > 0, "Cones 1");
        Cast(id + 0x130, AID.QuadrupleCrossingLast, 1.3f, 1);
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x132, 0.7f, comp => comp.NumCasts > 4, "Cones 2")
            .DeactivateOnExit<QuadrupleCrossingAOE>();
        Condition(id + 0x140, 0.8f, () => Module.FindComponent<QuadrupleSwipeShade>()?.NumFinishedStacks > 0 || Module.FindComponent<DoubleSwipeShade>()?.NumFinishedStacks > 0, "Pair/party stacks")
            .DeactivateOnExit<QuadrupleSwipeShade>()
            .DeactivateOnExit<DoubleSwipeShade>();
    }
}
