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
        BanishStormHoly(id + 0x40000, 4.6f);
        CrystallineThornsAuroralUppercut(id + 0x50000, 7.0f);
        BanishgaIV(id + 0x60000, 5.2f);
        CrystallineThornsAuroralUppercut(id + 0x70000, 2.4f);
        AsuranFists(id + 0x80000, 4.5f);
        // from now on, we have a loop with following mechanics in random order:
        // - banishga iv (raidwide + orbs) _or_ banish storm (exaflares) overlapping with knuckle sandwich _or_ thorns+uppercut (4 options total, each iteration of loop will have 2 combos with all mechanics seen once)
        // - holy
        // - nullifying dropkick
        // - asuran fists
        // hypothesis: actually it's one branch and loop repeats exactly:
        // 1. banish storm + knuckle sandwich > dropkick > banishga + uppercut > holy > fists
        // 2. banishga + knuckle sandwich > holy > banish storm + uppercut > dropkick > fists (seen in video - however, on second iteration dropkick happened before banish storm...)
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

    private void BanishStormHoly(uint id, float delay)
    {
        Cast(id, AID.BanishStorm, delay, 4);
        ComponentCondition<BanishStorm>(id + 0x10, 2.7f, comp => comp.Active)
            .ActivateOnEnter<BanishStorm>();
        ComponentCondition<BanishStorm>(id + 0x20, 9.1f, comp => comp.NumCasts > 0, "Exaflares start");

        Cast(id + 0x100, AID.Holy, 4.4f, 4)
            .ActivateOnEnter<Holy>();
        ComponentCondition<Holy>(id + 0x110, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Holy>();

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
}
