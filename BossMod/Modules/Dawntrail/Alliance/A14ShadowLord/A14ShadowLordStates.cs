namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class A14ShadowLordStates : StateMachineBuilder
{
    public A14ShadowLordStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<UmbraSmash>()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<GigaSlash>()
            .ActivateOnEnter<DamningStrikes>()
            .ActivateOnEnter<CthonicFury>()
            .ActivateOnEnter<BurningCourtMoatKeepBattlements>()
            .ActivateOnEnter<DarkNebula>()
            .ActivateOnEnter<EchoesOfAgony>()
            .ActivateOnEnter<BindingSigil>()
            .ActivateOnEnter<UnbridledRage>()
            .ActivateOnEnter<DoomArc>()
            .ActivateOnEnter<DarkNova>();
    }

    private void SinglePhase(uint id)
    {
        // note: this is a very weird fight, if you wipe, it uses a slightly different script (no initial giga slash and slightly different cthonic fury 1)
        Dictionary<bool, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [true] = (1u, SinglePhaseInitial),
            [false] = (2u, SinglePhaseAfterWipe),
        };
        ConditionFork(id, 10.2f, () => Module.PrimaryActor.CastInfo != null || Module.FindComponent<Teleport>()?.NumCasts > 0, () => (Module.PrimaryActor.CastInfo?.Action.ID ?? 0) is (uint)AID.GigaSlashL or (uint)AID.GigaSlashR, dispatch, "First mechanic...")
            .ActivateOnEnter<Teleport>()
            .DeactivateOnExit<Teleport>();
    }

    private void SinglePhaseInitial(uint id)
    {
        GigaSlash(id, default);
        PhaseInitial(id + 0x100000, 6.5f, true);
        PhaseRepeats(id + 0x200000, 12.2f);
        PhaseRepeats(id + 0x300000, 10.2f);
        PhaseRepeats(id + 0x400000, 10.2f);
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void SinglePhaseAfterWipe(uint id)
    {
        PhaseInitial(id + 0x100000, 2.4f, false);
        PhaseRepeats(id + 0x200000, 12.2f);
        PhaseRepeats(id + 0x300000, 10.2f);
        PhaseRepeats(id + 0x400000, 10.2f);
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void PhaseInitial(uint id, float delay, bool initialPull)
    {
        UmbraSmashGigaSlash(id, delay);
        FlamesOfHatred(id + 0x10000, 4.1f);
        Implosion(id + 0x20000, 3.2f);
        CthonicFury1(id + 0x30000, 3.6f, initialPull);
        NightfallTeraSlash(id + 0x40000, 4.8f);
    }

    private void PhaseRepeats(uint id, float delay)
    {
        GigaSlashNightfall(id, delay);
        ShadowSpawnGigaSlashNightfallImplosion(id + 0x10000, 5.4f);
        UnbridledRage(id + 0x20000, 2.6f);
        EchoesOfAgony(id + 0x30000, 1.2f, 7);
        BindingSigil(id + 0x40000, 2.6f);
        DamningStrikes(id + 0x50000, 3.5f);
        CthonicFury2(id + 0x60000, 9.0f);
        ShadowSpawnUmbraSmashGigaSlashNightfall(id + 0x70000, 4.6f);
        DoomArc(id + 0x80000, 3);
    }

    private State GigaSlash(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.GigaSlashL, (uint)AID.GigaSlashR], delay, 11);
        ComponentCondition<GigaSlash>(id + 2, 1, comp => comp.NumCasts > 0, "Cleave 1");
        return ComponentCondition<GigaSlash>(id + 0x10, 2.1f, comp => comp.NumCasts > 1, "Cleave 2")
            .ResetComp<GigaSlash>();
    }

    private void UmbraSmashGigaSlash(uint id, float delay)
    {
        Cast(id, (uint)AID.UmbraSmashBoss, delay, 4);
        ComponentCondition<UmbraSmash>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, "Exalines");
        GigaSlash(id + 0x100, 9.1f)
            .ResetComp<UmbraSmash>();
    }

    private void FlamesOfHatred(uint id, float delay)
    {
        Cast(id, (uint)AID.FlamesOfHatred, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Implosion(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.ImplosionL, (uint)AID.ImplosionR], delay, 8);
        ComponentCondition<Implosion>(id + 2, 1, comp => comp.NumCasts > 0, "Circle + cleave")
            .ResetComp<Implosion>();
    }

    private void BurningCourtMoat(uint id, float delay)
    {
        ComponentCondition<BurningCourtMoatKeepBattlements>(id, delay, comp => comp.AOEs.Count > 0);
        ComponentCondition<BurningCourtMoatKeepBattlements>(id + 1, 7, comp => comp.AOEs.Count == 0, "Platform in/out");
    }

    private void EchoesOfAgony(uint id, float delay, int numCasts)
    {
        CastStart(id, (uint)AID.EchoesOfAgony, delay);
        CastEnd(id + 1, 8);
        ComponentCondition<EchoesOfAgony>(id + 2, 1.1f, comp => comp.NumFinishedStacks > 0, "Stack 1");
        ComponentCondition<EchoesOfAgony>(id + 0x10, numCasts == 7 ? 6.4f : 4.3f, comp => comp.NumFinishedStacks >= numCasts, $"Stack {numCasts}")
            .ResetComp<EchoesOfAgony>();
    }

    private void CthonicFuryStart(uint id, float delay)
    {
        Cast(id, (uint)AID.CthonicFuryStart, delay, 7, "Raidwide + platforms start")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CthonicFuryEnd(uint id, float delay)
    {
        Cast(id, (uint)AID.CthonicFuryEnd, delay, 7, "Raidwide + platforms end")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DarkNebula(uint id, float delay)
    {
        Cast(id, (uint)AID.DarkNebula, delay, 3);
        ComponentCondition<DarkNebula>(id + 0x10, 1.2f, comp => comp.KBs.Count > 0);
        ComponentCondition<DarkNebula>(id + 0x11, 5, comp => comp.NumCasts > 0, "Knockback")
            .ResetComp<DarkNebula>();
    }

    private void CthonicFury1(uint id, float delay, bool initialPull)
    {
        CthonicFuryStart(id, delay);
        if (initialPull)
        {
            BurningCourtMoat(id + 0x1000, 8.2f);
            BurningCourtMoat(id + 0x2000, 3.1f);
            DarkNebula(id + 0x3000, 6);
        }
        else
        {
            BurningCourtMoat(id + 0x1000, 3.2f);
            DarkNebula(id + 0x3000, 3);
        }
        Implosion(id + 0x4000, 4.1f);
        BurningCourtMoat(id + 0x5000, 2.2f);
        EchoesOfAgony(id + 0x6000, 4, 5);
        CthonicFuryEnd(id + 0x7000, 1.7f);
    }

    private void NightfallTeraSlash(uint id, float delay)
    {
        Cast(id, (uint)AID.Nightfall, delay, 5);
        ComponentCondition<TeraSlash>(id + 0x10, 34.1f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<TeraSlash>()
            .DeactivateOnExit<TeraSlash>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State GigaSlashNightfall(uint id, float delay, bool removeComponent = true)
    {
        CastMulti(id, [(uint)AID.GigaSlashNightfallLRF, (uint)AID.GigaSlashNightfallLRB, (uint)AID.GigaSlashNightfallRLF, (uint)AID.GigaSlashNightfallRLB], delay, 14);
        ComponentCondition<GigaSlash>(id + 2, 1, comp => comp.NumCasts > 0, "Cleave 1");
        ComponentCondition<GigaSlash>(id + 0x10, 2.1f, comp => comp.NumCasts > 1, "Cleave 2");
        return ComponentCondition<GigaSlash>(id + 0x11, 2.1f, comp => comp.NumCasts > 2, "Cleave 3")
            .ResetComp<GigaSlash>(removeComponent);
    }

    private void ShadowSpawnGigaSlashNightfallImplosion(uint id, float delay)
    {
        Cast(id, (uint)AID.ShadowSpawn, delay, 3);
        GigaSlashNightfall(id + 0x100, 3.1f, false);

        CastStartMulti(id + 0x200, [(uint)AID.ImplosionL, (uint)AID.ImplosionR], 7.5f);
        ComponentCondition<GigaSlash>(id + 0x201, 1.5f, comp => comp.NumCasts > 3, "Cleave 4");
        ComponentCondition<GigaSlash>(id + 0x202, 2.1f, comp => comp.NumCasts > 4, "Cleave 5")
            .ResetComp<GigaSlash>();
        CastEnd(id + 0x203, 4.4f);
        ComponentCondition<Implosion>(id + 0x204, 1, comp => comp.NumCasts > 0, "Circle + cleave 1");
        ComponentCondition<Implosion>(id + 0x210, 4.6f, comp => comp.NumCasts > 2, "Circle + cleave 2")
            .ResetComp<Implosion>();
    }

    private void DarkNova(uint id, float delay)
    {
        ComponentCondition<DarkNova>(id, delay, comp => comp.Active);
        ComponentCondition<DarkNova>(id + 1, 6, comp => comp.NumFinishedSpreads > 0, "Spread")
            .ResetComp<DarkNova>();
    }

    private void UnbridledRage(uint id, float delay)
    {
        CastStart(id, (uint)AID.UnbridledRage, delay);
        CastEnd(id + 1, 5);
        ComponentCondition<UnbridledRage>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Tankbusters")
            .ResetComp<UnbridledRage>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        DarkNova(id + 0x1000, 0.2f);
    }

    private void BindingSigil(uint id, float delay)
    {
        Cast(id, (uint)AID.BindingSigil, delay, 12);
        ComponentCondition<BindingSigil>(id + 2, 2.1f, comp => comp.NumCasts > 0, "Puddles 1"); // 8 or 9
        ComponentCondition<BindingSigil>(id + 3, 2.5f, comp => comp.NumCasts > 9, "Puddles 2"); // 16 or 17
        ComponentCondition<BindingSigil>(id + 4, 2.5f, comp => comp.NumCasts > 17, "Puddles 3") // 25
            .ResetComp<BindingSigil>();
    }

    private void DamningStrikes(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.DamningStrikes1, (uint)AID.DamningStrikes2], delay, 8); // note: alt cast is longer by 0.7s, whatever...
        ComponentCondition<DamningStrikes>(id + 2, 2.5f, comp => comp.NumCasts >= 1, "Tower 1");
        ComponentCondition<DamningStrikes>(id + 3, 2.5f, comp => comp.NumCasts >= 2, "Tower 2");
        ComponentCondition<DamningStrikes>(id + 4, 2.7f, comp => comp.NumCasts >= 3, "Tower 3")
            .ResetComp<DamningStrikes>();
    }

    private void DarkNebulaGigaSlashNightfall(uint id, float delay)
    {
        Cast(id, (uint)AID.DarkNebula, delay, 3);
        ComponentCondition<DarkNebula>(id + 0x10, 1.2f, comp => comp.KBs.Count > 0);
        ComponentCondition<DarkNebula>(id + 0x20, 13, comp => comp.NumCasts > 0, "Knockback 1");
        ComponentCondition<DarkNebula>(id + 0x21, 3, comp => comp.NumCasts > 1, "Knockback 2");
        CastStartMulti(id + 0x23, [(uint)AID.GigaSlashNightfallLRF, (uint)AID.GigaSlashNightfallLRB, (uint)AID.GigaSlashNightfallRLF, (uint)AID.GigaSlashNightfallRLB], 1.4f);
        ComponentCondition<DarkNebula>(id + 0x24, 1.6f, comp => comp.NumCasts > 2, "Knockback 3");
        ComponentCondition<DarkNebula>(id + 0x25, 3, comp => comp.NumCasts > 3, "Knockback 4")
            .ResetComp<DarkNebula>();
        CastEnd(id + 0x26, 9.4f);
        ComponentCondition<GigaSlash>(id + 0x30, 1, comp => comp.NumCasts > 0, "Cleave 1");
        ComponentCondition<GigaSlash>(id + 0x31, 2.1f, comp => comp.NumCasts > 1, "Cleave 2");
        ComponentCondition<GigaSlash>(id + 0x32, 2.1f, comp => comp.NumCasts > 2, "Cleave 3")
            .ResetComp<GigaSlash>();
    }

    private void CthonicFury2(uint id, float delay)
    {
        CthonicFuryStart(id, delay);
        DarkNebulaGigaSlashNightfall(id + 0x1000, 3.2f);
        BurningCourtMoat(id + 0x2000, 3);
        DarkNova(id + 0x3000, 5.4f);
        CthonicFuryEnd(id + 0x4000, 2.1f);
    }

    private void ShadowSpawnUmbraSmashGigaSlashNightfall(uint id, float delay)
    {
        Cast(id, (uint)AID.ShadowSpawn, delay, 3);
        Cast(id + 0x10, (uint)AID.UmbraSmashBoss, 4.2f, 4);
        ComponentCondition<UmbraSmash>(id + 0x20, 0.5f, comp => comp.NumCasts > 0, "Exalines");
        GigaSlashNightfall(id + 0x100, 12.2f)
            .ResetComp<UmbraSmash>();
    }

    private void DoomArc(uint id, float delay)
    {
        Cast(id, (uint)AID.DoomArc, delay, 15, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
