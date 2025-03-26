namespace BossMod.Endwalker.Savage.P1SErichthonios;

class P1SStates : StateMachineBuilder
{
    public P1SStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        HeavyHand(id, 8.2f);
        AetherialShackles(id + 0x010000, 6, false);
        GaolerFlail(id + 0x020000, 4.6f);
        Knockback(id + 0x030000, 5.7f);
        GaolerFlail(id + 0x040000, 3.3f);
        WarderWrath(id + 0x050000, 5.6f);
        IntemperancePhase(id + 0x060000, 11.2f, true);
        Knockback(id + 0x070000, 5.4f);

        ShiningCells(id + 0x100000, 9.3f);
        Aetherflail(id + 0x110000, 8);
        Knockback(id + 0x120000, 7.7f);
        Aetherflail(id + 0x130000, 3);
        ShacklesOfTime(id + 0x140000, 4.6f, false);
        SlamShut(id + 0x150000, 1.6f);

        FourfoldShackles(id + 0x200000, 12.8f);
        WarderWrath(id + 0x210000, 5.4f);
        IntemperancePhase(id + 0x220000, 11.2f, false);
        WarderWrath(id + 0x230000, 3.7f);

        ShiningCells(id + 0x300000, 11.2f);

        Dictionary<AID, (uint seqID, Action<uint> buildState)> fork = new()
        {
            [AID.AetherialShackles] = (1, Fork1),
            [AID.ShacklesOfTime] = (2, Fork2)
        };
        CastStartFork(id + 0x310000, fork, 6.2f, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback"); // first branch delay = 7.8
    }

    // if delay is >0, build cast-start + cast-end states, otherwise build only cast-end state (used for first cast after fork)
    private State CastMaybeOmitStart(uint id, AID aid, float delay, float castTime, string name)
    {
        if (delay > 0)
            return Cast(id, aid, delay, castTime, name);
        else
            return CastEnd(id, castTime, name);
    }

    private void HeavyHand(uint id, float delay)
    {
        Cast(id, AID.HeavyHand, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void WarderWrath(uint id, float delay)
    {
        Cast(id, AID.WarderWrath, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Aetherchain(uint id, float delay)
    {
        Cast(id, AID.Aetherchain, delay, 5, "Aetherchain")
            .ActivateOnEnter<AetherExplosion>()
            .DeactivateOnExit<AetherExplosion>();
    }

    // aetherial shackles is paired either with wrath (first time) or two aetherchains (second time)
    private void AetherialShackles(uint id, float delay, bool withAetherchains)
    {
        CastMaybeOmitStart(id, AID.AetherialShackles, delay, 3, "Shackles")
            .ActivateOnEnter<Shackles>()
            .SetHint(StateMachine.StateHint.PositioningStart);

        if (withAetherchains)
        {
            Aetherchain(id + 0x1000, 6.1f);
            Aetherchain(id + 0x1100, 3.2f);
        }
        else
        {
            WarderWrath(id + 0x1000, 4.1f);
        }

        // ~19sec after cast end
        // technically, resolve happens ~0.4sec before second aetherchain cast end, but that's irrelevant
        ComponentCondition<Shackles>(id + 0x2000, withAetherchains ? 0 : 9.7f, comp => comp.NumExpiredDebuffs >= 2, "Shackles resolve")
            .DeactivateOnExit<Shackles>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void FourfoldShackles(uint id, float delay)
    {
        Cast(id, AID.FourShackles, delay, 3, "FourShackles")
            .ActivateOnEnter<Shackles>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        // note that it takes almost a second for debuffs to be applied
        ComponentCondition<Shackles>(id + 0x10, 8.9f, comp => comp.NumExpiredDebuffs >= 2, "Hit1", 1, 3);
        ComponentCondition<Shackles>(id + 0x20, 5, comp => comp.NumExpiredDebuffs >= 4, "Hit2");
        ComponentCondition<Shackles>(id + 0x30, 5, comp => comp.NumExpiredDebuffs >= 6, "Hit3");
        ComponentCondition<Shackles>(id + 0x40, 5, comp => comp.NumExpiredDebuffs >= 8, "Hit4")
            .DeactivateOnExit<Shackles>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    // shackles of time is paired either with heavy hand or knockback mechanics; also cast-start sometimes is omitted if delay is 0, since it is used to determine fork path
    private void ShacklesOfTime(uint id, float delay, bool withKnockback)
    {
        var cast = CastMaybeOmitStart(id, AID.ShacklesOfTime, delay, 4, "ShacklesOfTime")
            .ActivateOnEnter<AetherExplosion>()
            .SetHint(StateMachine.StateHint.PositioningStart);

        if (withKnockback)
        {
            Knockback(id + 0x1000, 2.2f, false);
        }
        else
        {
            HeavyHand(id + 0x1000, 5.2f);
        }

        // ~15s from cast end
        ComponentCondition<AetherExplosion>(id + 0x2000, withKnockback ? 3.4f : 4.7f, comp => !comp.SOTActive, "Shackles resolve")
            .DeactivateOnExit<AetherExplosion>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void GaolerFlail(uint id, float delay)
    {
        CastStartMulti(id, [AID.GaolerFlailRL, AID.GaolerFlailLR, AID.GaolerFlailIO1, AID.GaolerFlailIO2, AID.GaolerFlailOI1, AID.GaolerFlailOI2], delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 1, 11.5f)
            .ActivateOnEnter<Flails>();
        ComponentCondition<Flails>(id + 2, 3.6f, comp => comp.NumCasts == 2, "Flails")
            .DeactivateOnExit<Flails>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Aetherflail(uint id, float delay)
    {
        CastStartMulti(id, [AID.AetherflailRX, AID.AetherflailLX, AID.AetherflailIL, AID.AetherflailIR, AID.AetherflailOL, AID.AetherflailOR], delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 1, 11.5f)
            .ActivateOnEnter<Flails>()
            .ActivateOnEnter<AetherExplosion>();
        ComponentCondition<Flails>(id + 2, 3.6f, comp => comp.NumCasts == 2, "Aetherflail")
            .DeactivateOnExit<Flails>()
            .DeactivateOnExit<AetherExplosion>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Knockback(uint id, float delay, bool positioningHints = true)
    {
        CastStartMulti(id, [AID.KnockbackGrace, AID.KnockbackPurge], delay)
            .SetHint(StateMachine.StateHint.PositioningStart, positioningHints);
        CastEnd(id + 1, 5, "Knockback")
            .ActivateOnEnter<Knockback>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<Knockback>(id + 2, 4.3f, comp => comp.AOEDone, "Explode")
            .DeactivateOnExit<Knockback>()
            .SetHint(StateMachine.StateHint.PositioningEnd, positioningHints);
    }

    // full intemperance phases (overlap either with 2 wraths or with flails)
    private void IntemperancePhase(uint id, float delay, bool withWraths)
    {
        Cast(id, AID.Intemperance, delay, 2, "Intemperance");
        CastStartMulti(id + 0x1000, [AID.IntemperateTormentUp, AID.IntemperateTormentDown], 5.9f)
            .ActivateOnEnter<Intemperance>();
        CastEnd(id + 0x1001, 10);
        ComponentCondition<Intemperance>(id + 0x2000, 1.2f, comp => comp.NumExplosions > 0, "Cube1", 0.2f)
            .SetHint(StateMachine.StateHint.PositioningStart);

        if (withWraths)
        {
            WarderWrath(id + 0x3000, 1);
            ComponentCondition<Intemperance>(id + 0x4000, 5, comp => comp.NumExplosions > 1, "Cube2", 0.2f);
            WarderWrath(id + 0x5000, 0.2f);
        }
        else
        {
            CastStartMulti(id + 0x3000, [AID.GaolerFlailRL, AID.GaolerFlailLR, AID.GaolerFlailIO1, AID.GaolerFlailIO2, AID.GaolerFlailOI1, AID.GaolerFlailOI2], 3);
            ComponentCondition<Intemperance>(id + 0x4000, 8, comp => comp.NumExplosions > 1, "Cube2")
                .ActivateOnEnter<Flails>();
            CastEnd(id + 0x5000, 3.5f);
            ComponentCondition<Flails>(id + 0x5001, 3.6f, comp => comp.NumCasts == 2, "Flails")
                .DeactivateOnExit<Flails>();
        }

        ComponentCondition<Intemperance>(id + 0x6000, withWraths ? 5.8f : 3.9f, comp => comp.NumExplosions > 2, "Cube3")
            .DeactivateOnExit<Intemperance>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ShiningCells(uint id, float delay)
    {
        Cast(id, AID.ShiningCells, delay, 7, "Cells")
            .OnExit(() => Module.Arena.Bounds = new ArenaBoundsCircle(Module.Bounds.Radius))
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SlamShut(uint id, float delay)
    {
        Cast(id, AID.SlamShut, delay, 7, "SlamShut")
            .OnExit(() => Module.Arena.Bounds = new ArenaBoundsSquare(Module.Bounds.Radius))
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Fork1(uint id)
    {
        AetherialShackles(id, 0, true);
        WarderWrath(id + 0x10000, 5.2f);
        ShacklesOfTime(id + 0x20000, 7.2f, true);
        WarderWrath(id + 0x30000, 5.9f);
        ForkMerge(id + 0x40000, 9);
    }

    private void Fork2(uint id)
    {
        ShacklesOfTime(id, 0, true);
        WarderWrath(id + 0x10000, 3.9f);
        AetherialShackles(id + 0x20000, 9, true);
        WarderWrath(id + 0x30000, 7.2f);
        ForkMerge(id + 0x40000, 8.8f);
    }

    // there are two possible orderings for last mechanics of the fight
    private void ForkMerge(uint id, float delay)
    {
        Aetherflail(id, delay);
        Aetherflail(id + 0x10000, 2.7f);
        Aetherflail(id + 0x20000, 2.7f);
        WarderWrath(id + 0x30000, 10.7f);
        WarderWrath(id + 0x40000, 4.2f);
        WarderWrath(id + 0x50000, 4.2f);
        Cast(id + 0x60000, AID.Enrage, 8.2f, 12, "Enrage");
    }
}
