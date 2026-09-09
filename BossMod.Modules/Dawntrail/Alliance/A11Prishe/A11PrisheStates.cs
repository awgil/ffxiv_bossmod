namespace BossMod.Dawntrail.Alliance.A11Prishe;

class A11PrisheStates : StateMachineBuilder
{
    public A11PrisheStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Banishga(id, 6.2f);
        KnuckleSandwich(id + 0x10000, 9.5f);
        KnuckleSandwich(id + 0x20000, 5.4f);
        NullifyingDropkick(id + 0x30000, 3.2f);
        BanishStormHoly(id + 0x40000, 4.7f);
        CrystallineThornsAuroralUppercut(id + 0x50000, 7.0f);
        BanishgaIV(id + 0x60000, 5.2f);
        CrystallineThornsAuroralUppercut(id + 0x70000, 2.4f);
        AsuranFists(id + 0x80000, 4.6f);

        Dictionary<AID, (uint seqID, Action<uint> buildState)> fork = new()
        {
            [AID.BanishStorm] = ((id >> 24) + 1, ForkBanishStormFirst),
            [AID.BanishgaIV] = ((id >> 24) + 2, ForkBanishgaFirst)
        };
        CastStartFork(id + 0xC0000, fork, 9.9f, "Exaflares -or- Orbs");
    }

    private void ForkBanishStormFirst(uint id)
    {
        ForkBanishStormFirstRepeat(id, 0, true);
        ForkBanishStormFirstRepeat(id + 0x100000, 6.8f, false);
        ForkBanishStormFirstRepeat(id + 0x200000, 5.9f, false);

        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<BanishStorm>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<KnuckleSandwich>()
            .ActivateOnEnter<CrystallineThorns>()
            .ActivateOnEnter<AuroralUppercut>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<NullifyingDropkick>()
            .ActivateOnEnter<AsuranFists>();
    }

    private void ForkBanishStormFirstRepeat(uint id, float delay, bool firstTime)
    {
        BanishStormKnuckleSandwich(id, delay);
        NullifyingDropkick(id + 0x10000, 2.2f);
        BanishgaIVCrystallineThornsAuroralUppercut(id + 0x20000, firstTime ? 6.7f : 2.6f);
        Holy(id + 0x30000, 3.2f);
        AsuranFists(id + 0x40000, 2.2f);
    }

    private void ForkBanishgaFirst(uint id)
    {
        // first loop has slightly different mechanic order
        BanishgaIVKnuckleSandwichHoly(id, 0);
        BanishStormCrystallineThornsAuroralUppercut(id + 0x10000, 5.2f);
        NullifyingDropkick(id + 0x20000, 8.2f);
        AsuranFists(id + 0x30000, 4.7f);

        ForkBanishgaFirstRepeat(id + 0x100000, 6.8f);

        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<BanishStorm>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<KnuckleSandwich>()
            .ActivateOnEnter<CrystallineThorns>()
            .ActivateOnEnter<AuroralUppercut>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<NullifyingDropkick>()
            .ActivateOnEnter<AsuranFists>();
    }

    private void ForkBanishgaFirstRepeat(uint id, float delay)
    {
        BanishgaIVKnuckleSandwichHoly(id, delay);
        NullifyingDropkick(id + 0x10000, 2.1f);
        BanishStormCrystallineThornsAuroralUppercut(id + 0x20000, 2.6f);
        AsuranFists(id + 0x30000, 4.2f);
    }

    private void Banishga(uint id, float delay)
    {
        Cast(id, AID.Banishga, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void KnuckleSandwich(uint id, float delay)
    {
        CastMulti(id, [AID.KnuckleSandwich1, AID.KnuckleSandwich2, AID.KnuckleSandwich3], delay, 12)
            .ActivateOnEnter<KnuckleSandwich>();
        ComponentCondition<KnuckleSandwich>(id + 0x10, 1, comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x11, 1.5f, comp => comp.NumCasts > 1, "In")
            .DeactivateOnExit<KnuckleSandwich>();
    }

    private void NullifyingDropkick(uint id, float delay)
    {
        Cast(id, AID.NullifyingDropkick, delay, 5, "Tankbuster 1")
            .ActivateOnEnter<NullifyingDropkick>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<NullifyingDropkick>(id + 2, 1.5f, comp => comp.NumCasts > 0, "Tankbuster 2")
            .DeactivateOnExit<NullifyingDropkick>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Holy(uint id, float delay)
    {
        Cast(id, AID.Holy, delay, 4)
            .ActivateOnEnter<Holy>();
        ComponentCondition<Holy>(id + 0x10, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Holy>();
    }

    private void BanishStormHoly(uint id, float delay)
    {
        Cast(id, AID.BanishStorm, delay, 4);
        ComponentCondition<BanishStorm>(id + 0x10, 2.7f, comp => comp.Active)
            .ActivateOnEnter<BanishStorm>();
        ComponentCondition<BanishStorm>(id + 0x20, 9.1f, comp => comp.NumCasts > 0, "Exaflares start");

        Holy(id + 0x100, 4.4f);

        ComponentCondition<BanishStorm>(id + 0x200, 2, comp => comp.Done, "Exaflares end")
            .DeactivateOnExit<BanishStorm>();
    }

    private void CrystallineThornsAuroralUppercut(uint id, float delay)
    {
        CastStart(id, AID.CrystallineThorns, delay)
            .ActivateOnEnter<CrystallineThorns>();
        CastEnd(id + 1, 4);
        ComponentCondition<CrystallineThorns>(id + 2, 1.1f, comp => comp.NumCasts > 0, "Spikes");
        CastMulti(id + 0x10, [AID.AuroralUppercut1, AID.AuroralUppercut2, AID.AuroralUppercut3], 3.1f, 11.4f)
            .ActivateOnEnter<AuroralUppercut>();
        ComponentCondition<AuroralUppercut>(id + 0x12, 1.4f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<AuroralUppercut>();
        ComponentCondition<CrystallineThorns>(id + 0x20, 5.1f, comp => !comp.Active, "Spikes end")
            .DeactivateOnExit<CrystallineThorns>();
    }

    private void BanishgaIV(uint id, float delay)
    {
        Cast(id, AID.BanishgaIV, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Explosion>(id + 0x10, 7.8f, comp => comp.NumCasts > 0, "Explosions start")
            .ActivateOnEnter<Explosion>();
        ComponentCondition<Explosion>(id + 0x20, 12, comp => comp.NumCasts >= 41, "Explosions end")
            .DeactivateOnExit<Explosion>();
    }

    private void AsuranFists(uint id, float delay)
    {
        Cast(id, AID.AsuranFists, delay, 6.5f)
            .ActivateOnEnter<AsuranFists>();
        ComponentCondition<AsuranFists>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, "Tower start");
        ComponentCondition<AsuranFists>(id + 0x20, 7.8f, comp => comp.NumCasts >= 8, "Tower resolve")
            .DeactivateOnExit<AsuranFists>();
    }

    private void BanishStormKnuckleSandwich(uint id, float delay)
    {
        Cast(id, AID.BanishStorm, delay, 4);
        ComponentCondition<BanishStorm>(id + 0x10, 2.7f, comp => comp.Active)
            .ActivateOnEnter<BanishStorm>();
        CastStartMulti(id + 0x20, [AID.KnuckleSandwich1, AID.KnuckleSandwich2, AID.KnuckleSandwich3], 5.7f);
        ComponentCondition<BanishStorm>(id + 0x21, 3.4f, comp => comp.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<KnuckleSandwich>();
        CastEnd(id + 0x22, 8.6f);
        ComponentCondition<KnuckleSandwich>(id + 0x30, 1, comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x31, 1.5f, comp => comp.NumCasts > 1, "In")
            .DeactivateOnExit<KnuckleSandwich>()
            .DeactivateOnExit<BanishStorm>();
    }

    private void BanishgaIVCrystallineThornsAuroralUppercut(uint id, float delay)
    {
        Cast(id, AID.BanishgaIV, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Explosion>(id + 0x10, 7.8f, comp => comp.NumCasts > 0, "Explosions start")
            .ActivateOnEnter<Explosion>();
        CastStart(id + 0x20, AID.CrystallineThorns, 1.6f)
            .ActivateOnEnter<CrystallineThorns>();
        CastEnd(id + 0x21, 4);
        ComponentCondition<CrystallineThorns>(id + 0x22, 1.1f, comp => comp.NumCasts > 0, "Spikes");
        CastStartMulti(id + 0x30, [AID.AuroralUppercut1, AID.AuroralUppercut2, AID.AuroralUppercut3], 3.1f);
        ComponentCondition<Explosion>(id + 0x31, 2.2f, comp => comp.NumCasts >= 41, "Explosions end")
            .ActivateOnEnter<AuroralUppercut>()
            .DeactivateOnExit<Explosion>();
        CastEnd(id + 0x32, 9.2f);
        ComponentCondition<AuroralUppercut>(id + 0x33, 1.4f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<AuroralUppercut>();
        ComponentCondition<CrystallineThorns>(id + 0x40, 5.1f, comp => !comp.Active, "Spikes end")
            .DeactivateOnExit<CrystallineThorns>();
    }

    private void BanishgaIVKnuckleSandwichHoly(uint id, float delay)
    {
        Cast(id, AID.BanishgaIV, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x10, [AID.KnuckleSandwich1, AID.KnuckleSandwich2, AID.KnuckleSandwich3], 4.4f, 12)
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<KnuckleSandwich>();
        ComponentCondition<Explosion>(id + 0x20, 0.5f, comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<KnuckleSandwich>(id + 0x21, 0.5f, comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x22, 1.5f, comp => comp.NumCasts > 1, "In")
            .DeactivateOnExit<KnuckleSandwich>();

        CastStart(id + 0x100, AID.Holy, 8.2f);
        ComponentCondition<Explosion>(id + 0x101, 1.8f, comp => comp.NumCasts >= 41, "Explosions end")
            .ActivateOnEnter<Holy>()
            .DeactivateOnExit<Explosion>();
        CastEnd(id + 0x102, 2.2f);
        ComponentCondition<Holy>(id + 0x110, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Holy>();
    }

    private void BanishStormCrystallineThornsAuroralUppercut(uint id, float delay)
    {
        Cast(id, AID.BanishStorm, delay, 4);
        ComponentCondition<BanishStorm>(id + 0x10, 2.7f, comp => comp.Active)
            .ActivateOnEnter<BanishStorm>();
        CastStart(id + 0x20, AID.CrystallineThorns, 1.7f)
            .ActivateOnEnter<CrystallineThorns>();
        CastEnd(id + 0x21, 4);
        ComponentCondition<CrystallineThorns>(id + 0x22, 1.1f, comp => comp.NumCasts > 0, "Spikes");
        ComponentCondition<BanishStorm>(id + 0x30, 2.4f, comp => comp.NumCasts > 0, "Exaflares start");
        CastMulti(id + 0x40, [AID.AuroralUppercut1, AID.AuroralUppercut2, AID.AuroralUppercut3], 0.7f, 11.4f)
            .ActivateOnEnter<AuroralUppercut>()
            .DeactivateOnExit<BanishStorm>();
        ComponentCondition<AuroralUppercut>(id + 0x50, 1.4f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<AuroralUppercut>();
        ComponentCondition<CrystallineThorns>(id + 0x60, 5.1f, comp => !comp.Active, "Spikes end")
            .DeactivateOnExit<CrystallineThorns>();
    }
}
