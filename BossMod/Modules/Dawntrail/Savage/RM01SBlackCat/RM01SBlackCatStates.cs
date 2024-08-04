namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class RM01SBlackCatStates : StateMachineBuilder
{
    public RM01SBlackCatStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        QuadrupleCrossing(id, 8.5f);
        BiscuitMaker(id + 0x10000, 4.5f);
        NineLives1(id + 0x20000, 12.2f);
        BloodyScratch(id + 0x30000, 5.6f);
        Mouser1(id + 0x40000, 13.4f);
        NineLives2Prepare(id + 0x50000, 12.5f);
    }

    private void ForkCleavesFirst(uint id)
    {
        NineLives2Cleaves(id, 0);
        NineLives2Proteans(id + 0x10000, 6.2f);
        ForkMerge(id + 0x100000, 6.4f);
    }

    private void ForkProteansFirst(uint id)
    {
        NineLives2Proteans(id, 0);
        NineLives2Cleaves(id + 0x10000, 5.4f);
        ForkMerge(id + 0x100000, 7.2f);
    }

    private void ForkMerge(uint id, float delay)
    {
        BloodyScratch(id, delay);
        Mouser2(id + 0x10000, 13.4f);
        RainingCats(id + 0x20000, 13.6f);
        PredaceousPounce(id + 0x30000, 8.4f);
        Mouser3(id + 0x40000, 14.2f);
    }

    private State BiscuitMaker(uint id, float delay)
    {
        Cast(id, AID.BiscuitMaker, delay, 5, "Tankbuster 1")
            .ActivateOnEnter<BiscuitMaker>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        return ComponentCondition<BiscuitMaker>(id + 0x10, 2, comp => comp.NumCasts > 1, "Tankbuster 2")
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
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x22, 0.6f, comp => comp.NumCasts > 0, "Cones 1");
        Cast(id + 0x30, AID.QuadrupleCrossingLast, 1.4f, 1);
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x32, 0.6f, comp => comp.NumCasts > 4, "Cones 2")
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
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x122, 0.6f, comp => comp.NumCasts > 0, "Cones 1");
        Cast(id + 0x130, AID.LeapingQuadrupleCrossingBossLast, 1.4f, 1);
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x132, 0.6f, comp => comp.NumCasts > 4, "Cones 2")
            .DeactivateOnExit<QuadrupleCrossingAOE>();
        Condition(id + 0x140, 0.8f, () => Module.FindComponent<QuadrupleSwipeShade>()?.NumFinishedStacks > 0 || Module.FindComponent<DoubleSwipeShade>()?.NumFinishedStacks > 0, "Pair/party stacks")
            .DeactivateOnExit<QuadrupleSwipeShade>()
            .DeactivateOnExit<DoubleSwipeShade>();
    }

    private void NineLives2Prepare(uint id, float delay)
    {
        Cast(id, AID.NineLives, delay, 3);
        CastMulti(id + 0x10, [AID.LeapingOneTwoPawBossLRL, AID.LeapingOneTwoPawBossLLR, AID.LeapingOneTwoPawBossRRL, AID.LeapingOneTwoPawBossRLR], 2.1f, 5)
            .ActivateOnEnter<LeapingOneTwoPaw>();
        ComponentCondition<LeapingOneTwoPaw>(id + 0x20, 1.8f, comp => comp.NumCasts > 0, "Cleave 1");
        ComponentCondition<LeapingOneTwoPaw>(id + 0x21, 2, comp => comp.NumCasts > 1, "Cleave 2");
        Cast(id + 0x30, AID.Soulshade, 2.2f, 3);
        Cast(id + 0x40, AID.NineLives, 2.1f, 3);

        CastMulti(id + 0x100, [AID.LeapingQuadrupleCrossingBossR, AID.LeapingQuadrupleCrossingBossL], 2.3f, 5)
            .ActivateOnEnter<QuadrupleCrossingProtean>();
        ComponentCondition<QuadrupleCrossingProtean>(id + 0x102, 1.8f, comp => comp.NumCasts > 0, "Jump + Proteans 1")
            .ActivateOnEnter<QuadrupleCrossingAOE>();
        Cast(id + 0x110, AID.LeapingQuadrupleCrossingBossMid, 1.2f, 1);
        ComponentCondition<QuadrupleCrossingProtean>(id + 0x112, 0.8f, comp => comp.NumCasts > 4, "Proteans 2");
        Cast(id + 0x120, AID.LeapingQuadrupleCrossingBossMid, 1.2f, 1);
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x122, 0.6f, comp => comp.NumCasts > 0, "Cones 1");
        Cast(id + 0x130, AID.LeapingQuadrupleCrossingBossLast, 1.4f, 1);
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x132, 0.6f, comp => comp.NumCasts > 4, "Cones 2")
            .DeactivateOnExit<QuadrupleCrossingAOE>();
        Cast(id + 0x140, AID.Soulshade, 1.4f, 3);

        Dictionary<bool, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [false] = (1, ForkCleavesFirst),
            [true] = (2, ForkProteansFirst),
        };
        ConditionFork(id + 0x200, 10.0f, () => Module.FindComponent<LeapingOneTwoPaw>()?.AOEs.Count > 2 || Module.FindComponent<QuadrupleCrossingProtean>()?.Origin != null, () => Module.FindComponent<QuadrupleCrossingProtean>()?.Origin != null, dispatch, "Clone tether");
    }

    private void NineLives2Cleaves(uint id, float delay)
    {
        ComponentCondition<LeapingOneTwoPaw>(id, delay, comp => comp.AOEs.Count > 2)
            .ActivateOnEnter<TempestuousTear>(); // target select happens together with cast start
        Cast(id + 0x10, AID.TempestuousTear, 10.3f, 5);
        ComponentCondition<LeapingOneTwoPaw>(id + 0x20, 0.8f, comp => comp.NumCasts > 2, "Cleave 1 + Line stacks")
            .DeactivateOnExit<TempestuousTear>(); // resolves at the same time
        ComponentCondition<LeapingOneTwoPaw>(id + 0x30, 2.0f, comp => comp.NumCasts > 3, "Cleave 2")
            .DeactivateOnExit<LeapingOneTwoPaw>();
    }

    private void NineLives2Proteans(uint id, float delay)
    {
        ComponentCondition<QuadrupleCrossingProtean>(id, delay, comp => comp.Origin != null);
        Cast(id + 0x10, AID.Nailchipper, 9.2f, 7)
            .ActivateOnEnter<Nailchipper>();
        ComponentCondition<QuadrupleCrossingProtean>(id + 0x20, 0.8f, comp => comp.NumCasts > 8, "Protean 1")
            .ActivateOnEnter<QuadrupleCrossingAOE>();
        ComponentCondition<Nailchipper>(id + 0x21, 0.2f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Nailchipper>();
        ComponentCondition<QuadrupleCrossingProtean>(id + 0x30, 2.7f, comp => comp.NumCasts > 12, "Protean 2")
            .ActivateOnEnter<Nailchipper>()
            .DeactivateOnExit<QuadrupleCrossingProtean>();
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x40, 2.9f, comp => comp.NumCasts > 0, "Cones 1");
        ComponentCondition<QuadrupleCrossingAOE>(id + 0x50, 3.0f, comp => comp.NumCasts > 4, "Cones 2 + Spread")
            .DeactivateOnExit<Nailchipper>() // resolves at the same time
            .DeactivateOnExit<QuadrupleCrossingAOE>();
    }

    private void Mouser(uint id, float delay)
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
    }

    private void GrimalkinGale(uint id, float delay)
    {
        Cast(id, AID.GrimalkinGaleShockwave, delay, 6)
            .ActivateOnEnter<GrimalkinGaleShockwave>();
        ComponentCondition<GrimalkinGaleShockwave>(id + 0x10, 1, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<GrimalkinGaleShockwave>();
        ComponentCondition<GrimalkinGaleSpread>(id + 0x20, 3.8f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .ActivateOnEnter<GrimalkinGaleSpread>() // note: casts start ~1.2s earlier, but no point showing hints before kb
            .DeactivateOnExit<GrimalkinGaleSpread>()
            .DeactivateOnExit<Mouser>(); // full repairs happen ~0.2s after spreads resolve
    }

    private void Mouser1(uint id, float delay)
    {
        Mouser(id, delay);

        ComponentCondition<ElevateAndEviscerate>(id + 0x100, 13.1f, comp => comp.NumCasts >= 1, "Jump 1")
            .ActivateOnEnter<ElevateAndEviscerate>();
        ComponentCondition<ElevateAndEviscerate>(id + 0x110, 11.2f, comp => comp.NumCasts >= 2, "Jump 2");
        ComponentCondition<ElevateAndEviscerate>(id + 0x120, 11.2f, comp => comp.NumCasts >= 3, "Jump 3");
        ComponentCondition<ElevateAndEviscerate>(id + 0x130, 11.2f, comp => comp.NumCasts >= 4, "Jump 4")
            .DeactivateOnExit<ElevateAndEviscerate>();

        BiscuitMaker(id + 0x200, 5.5f);
        GrimalkinGale(id + 0x300, 4);
    }

    private void Mouser2(uint id, float delay)
    {
        Mouser(id, delay);

        CastStartMulti(id + 0x100, [AID.Overshadow, AID.SplinteringNails], 8.4f)
            .ActivateOnEnter<ElevateAndEviscerate>();
        ComponentCondition<ElevateAndEviscerate>(id + 0x101, 4.7f, comp => comp.NumCasts >= 1, "Jump 1")
            .ActivateOnEnter<Overshadow>()
            .ActivateOnEnter<SplinteringNails>();
        CastEnd(id + 0x102, 0.3f); // note: splintering nails resolve ~0.8s later, overshadow ~0.3s later
        CastStartMulti(id + 0x110, [AID.Overshadow, AID.SplinteringNails], 7.4f);
        ComponentCondition<ElevateAndEviscerate>(id + 0x111, 4.7f, comp => comp.NumCasts >= 2, "Jump 2");
        CastEnd(id + 0x112, 0.3f);
        CastStartMulti(id + 0x120, [AID.Overshadow, AID.SplinteringNails], 7.4f);
        ComponentCondition<ElevateAndEviscerate>(id + 0x121, 4.7f, comp => comp.NumCasts >= 3, "Jump 3");
        CastEnd(id + 0x122, 0.3f);
        CastStartMulti(id + 0x130, [AID.Overshadow, AID.SplinteringNails], 7.4f);
        ComponentCondition<ElevateAndEviscerate>(id + 0x131, 4.7f, comp => comp.NumCasts >= 4, "Jump 4");
        CastEnd(id + 0x132, 0.3f);

        BiscuitMaker(id + 0x200, 5.3f)
            .DeactivateOnExit<Overshadow>()
            .DeactivateOnExit<SplinteringNails>()
            .DeactivateOnExit<ElevateAndEviscerate>();
        GrimalkinGale(id + 0x300, 5.1f);
    }

    private void Mouser3(uint id, float delay)
    {
        Cast(id, AID.MouserEnrage, delay, 8)
            .ActivateOnEnter<Mouser>();
        ComponentCondition<Mouser>(id + 0x10, 1.5f, comp => comp.NumCasts >= 4, "Tiles start");
        ComponentCondition<Mouser>(id + 0x11, 1, comp => comp.NumCasts >= 8);
        ComponentCondition<Mouser>(id + 0x12, 1, comp => comp.NumCasts >= 12);
        ComponentCondition<Mouser>(id + 0x13, 1, comp => comp.NumCasts >= 16);
        ComponentCondition<Mouser>(id + 0x14, 1, comp => comp.NumCasts >= 20);
        ComponentCondition<Mouser>(id + 0x15, 1, comp => comp.NumCasts >= 24);
        ComponentCondition<Mouser>(id + 0x16, 1, comp => comp.NumCasts >= 28);
        ComponentCondition<Mouser>(id + 0x17, 1, comp => comp.NumCasts >= 32, "Enrage");
    }

    private void RainingCats(uint id, float delay)
    {
        Cast(id, AID.RainingCatsFirst, delay, 6)
            .ActivateOnEnter<RainingCatsTether>()
            .ActivateOnEnter<RainingCatsStack>();
        ComponentCondition<RainingCatsTether>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Limit cut 1");
        Cast(id + 0x10, AID.RainingCatsMid, 0.3f, 5);
        ComponentCondition<RainingCatsTether>(id + 0x12, 0.8f, comp => comp.NumCasts > 2, "Limit cut 2");
        Cast(id + 0x20, AID.RainingCatsMid, 0.3f, 5);
        ComponentCondition<RainingCatsTether>(id + 0x22, 0.8f, comp => comp.NumCasts > 4, "Limit cut 3");
        Cast(id + 0x30, AID.RainingCatsLast, 0.3f, 5);
        ComponentCondition<RainingCatsTether>(id + 0x32, 0.8f, comp => comp.NumCasts > 6, "Limit cut 4")
            .DeactivateOnExit<RainingCatsStack>()
            .DeactivateOnExit<RainingCatsTether>();
    }

    private void PredaceousPounce(uint id, float delay)
    {
        Cast(id, AID.Copycat, delay, 3);
        ComponentCondition<PredaceousPounce>(id + 0x10, 4.8f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<PredaceousPounce>();
        ComponentCondition<PredaceousPounce>(id + 0x20, 13.5f, comp => comp.NumCasts > 0, "Jumps start"); // 12 aoes happen every ~0.5s
        CastMulti(id + 0x30, [AID.OneTwoPawBossRL, AID.OneTwoPawBossLR], 0.2f, 5)
            .ActivateOnEnter<OneTwoPawBoss>();
        ComponentCondition<OneTwoPawBoss>(id + 0x40, 1, comp => comp.NumCasts > 0, "Cleave 1")
            .DeactivateOnExit<PredaceousPounce>(); // resolves right before first cleave
        ComponentCondition<OneTwoPawBoss>(id + 0x41, 3, comp => comp.NumCasts > 1, "Cleave 2")
            .DeactivateOnExit<OneTwoPawBoss>();
    }
}
