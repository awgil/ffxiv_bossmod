namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class RM07SBruteAbombinatorStates : StateMachineBuilder
{
    public RM07SBruteAbombinatorStates(BossModule module) : base(module)
    {
        DeathPhase(0, id =>
        {
            Phase1(id, 5.15f);
            Phase2(id + 0x100000, 15);
            Phase3(id + 0x200000, 17.4f);
        });
    }

    private void Phase1(uint id, float delay)
    {
        BrutalImpact(id, delay, 6, 5.6f);
        SmashSomewhere(id + 0x10000, 5.2f);
        P1Seeds(id + 0x20000, 6.2f);
        P1Adds(id + 0x30000, 5.4f);
        Explosion(id + 0x40000, 12);
        SmashSomewhere(id + 0x50000, 3.9f, false);
        PulpSmash(id + 0x60000, 7.8f);
        Cast(id + 0x70000, AID.NeoBombarianSpecial, 10.8f, 8, "Arena change")
            .ActivateOnEnter<NeoBombarianSpecial>()
            .DeactivateOnExit<NeoBombarianSpecial>()
            .SetHint(StateMachine.StateHint.DowntimeStart | StateMachine.StateHint.Raidwide)
            .Raw.Exit += () =>
            {
                Module.Arena.Center = new(100, 5);
                Module.Arena.Bounds = new ArenaBoundsRect(12.5f, 25);
            };
    }

    private void Phase2(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss appears")
            .ActivateOnEnter<P2BrutishSwing>()
            .ActivateOnEnter<RevengeOfTheVines>();

        CastMulti(id + 0x10000, [AID.P2StoneringerClub, AID.P2StoneringerSword], 5.25f, 2);

        CastStartMulti(id + 0x10002, [AID.P2BrutishSwingJump2, AID.P2BrutishSwingJump1], 5.9f);

        ComponentCondition<P2BrutishSwing>(id + 0x10003, 8, s => s.NumCasts > 0, "In/out");
        P2GlowerPower(id + 0x10100, 0.9f);
        RevengeOfTheVines(id + 0x10200, 0.9f);

        Sporesplosion(id + 0x20000, 8.2f);

        ComponentCondition<P2BrutishSwing>(id + 0x30000, 5.1f, s => s.NumCasts > 1, "In/out")
            .ExecOnEnter<P2BrutishSwing>(p => p.Risky = true);
        P2GlowerPower(id + 0x30100, 0.9f);
        RevengeOfTheVines(id + 0x30200, 1);

        StrangeSeeds(id + 0x40000, 6.2f);

        ComponentCondition<P2BrutishSwing>(id + 0x50000, 5.5f, s => s.NumCasts > 2, "In/out")
            .ExecOnEnter<P2BrutishSwing>(p => p.Risky = true)
            .DeactivateOnExit<P2BrutishSwing>();
        P2GlowerPower(id + 0x50100, 0.9f);
        RevengeOfTheVines(id + 0x50200, 0.9f);

        Cast(id + 0x70000, AID.Powerslam, 6.2f, 6, "Arena change")
            .ActivateOnEnter<Powerslam>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart)
            .Raw.Exit += () =>
            {
                Module.DeactivateComponent<RevengeOfTheVines>();
                Module.DeactivateComponent<Powerslam>();
                Module.Arena.Bounds = new ArenaBoundsSquare(20);
            };
    }

    private void Phase3(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss appears")
            .ActivateOnEnter<P3BrutishSwing>();

        BrutalImpact(id + 0x10000, 7.1f, 7, 6.7f);
        Stoneringers1(id + 0x20000, 5.3f);
        BrutalImpact(id + 0x30000, 4.1f, 8, 7.7f);
        SmashSomewhere(id + 0x40000, 3.2f);
        DebrisDeathmatch(id + 0x50000, 8.2f);
        P3Seeds(id + 0x60000, 12.3f);
        PulpSmash(id + 0x70000, 3.5f, false);
        BrutalImpact(id + 0x80000, 1.4f, 8, 7.7f);
        Stoneringers2(id + 0x90000, 8.2f);
        SmashSomewhere(id + 0xA0000, 4.1f);
        BrutalImpact(id + 0xB0000, 4.1f, 8, 7.7f);

        Cast(id + 0xC0000, AID.SpecialBombarianSpecialVisual, 10.4f, 10, "Enrage")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void BrutalImpact(uint id, float delay, int numCasts, float duration)
    {
        Cast(id, AID.BrutalImpact, delay, 5, "Raidwide 1")
            .ActivateOnEnter<BrutalImpact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpact>(id + 2, duration, b => b.NumCasts == numCasts, $"Raidwide {numCasts}")
            .DeactivateOnExit<BrutalImpact>();
    }

    private void SmashSomewhere(uint id, float delay, bool includeInitialCast = true)
    {
        if (includeInitialCast)
            CastMulti(id, [AID.P1StoneringerClub, AID.P1StoneringerSword], delay, 2)
                .ActivateOnEnter<P1Stoneringer>()
                .ActivateOnEnter<P1Smash>();
        CastStartMulti(id + 0x10, [AID.SmashHere, AID.SmashThere], includeInitialCast ? 5.9f : delay);

        ComponentCondition<P1Stoneringer>(id + 0x12, 3.9f, p => p.NumCasts > 0, "In/out");
        ComponentCondition<P1Smash>(id + 0x13, 1.1f, p => p.NumCasts > 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P1Stoneringer>()
            .DeactivateOnExit<P1Smash>();
    }

    private void P1Seeds(uint id, float delay)
    {
        Cast(id, AID.SporeSacVisual, delay, 3)
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<SinisterSeedsSpread>()
            .ActivateOnEnter<SinisterSeedsChase>()
            .ActivateOnEnter<SinisterSeedsStored>()
            .ActivateOnEnter<P1BloomingAbomination>()
            .ActivateOnEnter<CrossingCrosswinds>()
            .ActivateOnEnter<WindingWildwinds>()
            .ActivateOnEnter<HurricaneForce>();

        ComponentCondition<SporeSac>(id + 2, 5.1f, s => s.NumCasts > 0, "Seed AOEs 1")
            .DeactivateOnExit<SporeSac>();
        ComponentCondition<Pollen>(id + 3, 5.6f, s => s.NumCasts > 0, "Seed AOEs 2")
            .ActivateOnEnter<Pollen>()
            .DeactivateOnExit<Pollen>();

        ComponentCondition<SinisterSeedsSpread>(id + 4, 7, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<SinisterSeedsSpread>();

        ComponentCondition<TendrilsOfTerror>(id + 0x10, 4.6f, t => t.NumCasts > 0, "Safe spot + stacks")
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<RootsOfEvil>()
            .ActivateOnEnter<TendrilsOfTerror>()
            .ExecOnExit<SinisterSeedsStored>(s => s.Activate())
            .DeactivateOnExit<Impact>()
            .DeactivateOnExit<SinisterSeedsChase>();
    }

    private void P1Adds(uint id, float delay)
    {
        ComponentCondition<RootsOfEvil>(id, delay, r => r.NumCasts > 0, "Stored AOEs")
            .DeactivateOnExit<RootsOfEvil>()
            .DeactivateOnExit<SinisterSeedsStored>();

        ComponentCondition<WindingWildwinds>(id + 0x10, 0.7f, w => w.Casters.Count > 0);

        Timeout(id + 0x12, 7, "Interruptible casts");

        CastStart(id + 0x20, AID.QuarrySwamp, 11)
            .ActivateOnEnter<QuarrySwamp>();
        Timeout(id + 0x21, 0.5f, "Adds enrage");
        CastEnd(id + 0x22, 3.5f, "LoS")
            .DeactivateOnExit<P1BloomingAbomination>()
            .DeactivateOnExit<CrossingCrosswinds>()
            .DeactivateOnExit<WindingWildwinds>()
            .DeactivateOnExit<HurricaneForce>()
            .DeactivateOnExit<QuarrySwamp>();
    }

    private void Explosion(uint id, float delay)
    {
        ComponentCondition<Explosion>(id, delay, e => e.NumCasts > 0, "Gigaflare 1")
            .ActivateOnEnter<Explosion>();
        ComponentCondition<Explosion>(id + 1, 2.5f, e => e.NumCasts > 1, "Gigaflare 2");
        ComponentCondition<Explosion>(id + 2, 2.5f, e => e.NumCasts > 2, "Gigaflare 3")
            .ActivateOnEnter<P1Stoneringer>()
            .ActivateOnEnter<P1Smash>()
            .DeactivateOnExit<Explosion>();
    }

    private void PulpSmash(uint id, float delay, bool activate = true)
    {
        var st = ComponentCondition<PulpSmash>(id, delay, p => p.NumFinishedStacks > 0, "Stack");

        if (activate)
            st
                .ActivateOnEnter<PulpSmash>()
                .ActivateOnEnter<PulpSmashProtean>()
                .ActivateOnEnter<ItCameFromTheDirt>();
        ComponentCondition<PulpSmashProtean>(id + 1, 2, p => p.NumCasts > 0, "Proteans")
            .DeactivateOnExit<PulpSmash>()
            .DeactivateOnExit<PulpSmashProtean>()
            .DeactivateOnExit<ItCameFromTheDirt>();
    }

    private void RevengeOfTheVines(uint id, float delay)
    {
        Cast(id, AID.RevengeOfTheVines, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2GlowerPower(uint id, float delay)
    {
        Cast(id, AID.P2GlowerPowerVisual, delay, 2.7f)
            .ActivateOnEnter<P2ElectrogeneticForce>()
            .ActivateOnEnter<P2GlowerPower>();
        ComponentCondition<P2ElectrogeneticForce>(id + 2, 1.2f, e => e.NumCasts > 0, "Spreads")
            .DeactivateOnExit<P2ElectrogeneticForce>()
            .DeactivateOnExit<P2GlowerPower>();
    }

    private void Sporesplosion(uint id, float delay)
    {
        Cast(id, AID.ThornyDeathmatch, delay, 3)
            .ActivateOnEnter<ThornsOfDeath>()
            .ActivateOnEnter<AbominableBlink>();

        ComponentCondition<ThornsOfDeath>(id + 2, 1.1f, t => t.Tethers.Count > 0, "Tank tether appear");

        CastMulti(id + 0x10, [AID.P2StoneringerClub, AID.P2StoneringerSword], 1, 2);

        Cast(id + 0x20, AID.AbominableBlinkVisual, 5.9f, 5.3f);

        ComponentCondition<AbominableBlink>(id + 0x22, 1, b => b.NumCasts > 0, "Flare");

        Cast(id + 0x30, AID.SporesplosionVisual, 5.1f, 4)
            .ActivateOnEnter<Sporesplosion>();

        ComponentCondition<Sporesplosion>(id + 0x32, 6.1f, s => s.NumCasts >= 6, "Puddles 1");

        CastStartMulti(id + 0x40, [AID.P2BrutishSwingJump4, AID.P2BrutishSwingJump6], 1)
            .ExecOnEnter<P2BrutishSwing>(p => p.Risky = false);

        ComponentCondition<Sporesplosion>(id + 0x42, 1, s => s.NumCasts >= 12, "Puddles 2");

        ComponentCondition<Sporesplosion>(id + 0x44, 2, s => s.NumCasts >= 18, "Puddles 3")
            .DeactivateOnExit<Sporesplosion>();
    }

    private void StrangeSeeds(uint id, float delay)
    {
        Cast(id, AID.DemolitionDeathmatch, delay, 3);

        ComponentCondition<ThornsOfDeath>(id + 2, 0.9f, t => t.Tethers.Count > 0, "Tethers appear");

        Cast(id + 0x10, AID.AbominableBlinkVisual, 7.2f, 5.3f);

        ComponentCondition<AbominableBlink>(id + 0x12, 1, b => b.NumCasts > 1, "Flare")
            .DeactivateOnExit<AbominableBlink>();

        Cast(id + 0x20, AID.P2StrangeSeedsVisual, 3.1f, 4)
            .ActivateOnEnter<StrangeSeeds>()
            .ActivateOnEnter<StrangeSeedsCounter>();

        ComponentCondition<StrangeSeeds>(id + 0x22, 1.2f, s => s.ActiveSpreads.Any(), "Seeds start");

        ComponentCondition<StrangeSeedsCounter>(id + 0x24, 5, s => s.NumCasts == 2, "Seeds 1");

        CastStartMulti(id + 0x30, [AID.P2StoneringerClub, AID.P2StoneringerSword], 5.25f);

        ComponentCondition<StrangeSeedsCounter>(id + 0x32, 5, s => s.NumCasts == 4, "Seeds 2");
        ComponentCondition<StrangeSeedsCounter>(id + 0x33, 5, s => s.NumCasts == 6, "Seeds 3");
        ComponentCondition<StrangeSeedsCounter>(id + 0x34, 5, s => s.NumCasts == 8, "Seeds 4")
            .DeactivateOnExit<StrangeSeeds>()
            .DeactivateOnExit<StrangeSeedsCounter>();

        ComponentCondition<KillerSeeds>(id + 0x38, 9.8f, k => k.NumFinishedStacks > 0, "Pairs")
            .ActivateOnEnter<KillerSeeds>()
            .DeactivateOnExit<KillerSeeds>();

        CastStartMulti(id + 0x40, [AID.P2BrutishSwingJump3, AID.P2BrutishSwingJump4, AID.P2BrutishSwingJump5, AID.P2BrutishSwingJump6], 2)
            .ExecOnEnter<P2BrutishSwing>(p => p.Risky = false);

        ComponentCondition<TendrilsOfTerror>(id + 0x42, 2.6f, t => t.Casters.Count == 0, "Star AOEs");
    }

    private void Stoneringers1(uint id, float delay)
    {
        CastMulti(id, [AID.Stoneringer2Stoneringers1, AID.Stoneringer2Stoneringers2], delay, 2)
            .ActivateOnEnter<Lariat>()
            .ActivateOnEnter<P3GlowerPower>()
            .ActivateOnEnter<Slaminator>();

        Cast(id + 0x10, AID.P3BrutishSwingJump1, 6.3f, 3);

        ComponentCondition<P3BrutishSwing>(id + 0x12, 3.8f, b => b.NumCasts > 0, "In/out");

        CastMulti(id + 0x20, [AID.LashingLariatJump1, AID.LashingLariatJump2], 3.2f, 3.5f);

        ComponentCondition<Lariat>(id + 0x22, 0.5f, l => l.NumCasts > 0, "Cleave")
            .DeactivateOnExit<Lariat>();

        CastMulti(id + 0x30, [AID.P3BrutishSwingJump2, AID.P3BrutishSwingJump3], 1.6f, 3)
            .ActivateOnEnter<P3ElectrogeneticForce>();

        ComponentCondition<P3BrutishSwing>(id + 0x32, 3.7f, b => b.NumCasts > 1, "In/out")
            .ExecOnExit<P3ElectrogeneticForce>(p => p.Risky = true);

        Cast(id + 0x40, AID.P3GlowerPowerVisual, 1.1f, 0.7f);
        ComponentCondition<P3ElectrogeneticForce>(id + 0x42, 1.2f, f => f.NumCasts > 0, "Spreads")
            .DeactivateOnExit<P3ElectrogeneticForce>();
        ComponentCondition<P3GlowerPower>(id + 0x43, 0.1f, g => g.NumCasts > 0, "Line AOE")
            .DeactivateOnExit<P3GlowerPower>();

        Cast(id + 0x50, AID.SlaminatorVisual, 0.8f, 4);

        ComponentCondition<Slaminator>(id + 0x52, 1.1f, s => s.NumCasts > 0, "Tower");
    }

    private void DebrisDeathmatch(uint id, float delay)
    {
        Cast(id, AID.DebrisDeathmatch, delay, 3);

        ComponentCondition<ThornsOfDeath>(id + 2, 1.1f, t => t.Tethers.Count > 0, "Tethers appear");

        Cast(id + 0x10, AID.SporeSacVisual, 1, 3)
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<P3BloomingAbomination>();

        ComponentCondition<SporeSac>(id + 0x12, 5.1f, s => s.NumCasts > 0, "Seed AOEs 1")
            .DeactivateOnExit<SporeSac>();
        ComponentCondition<Pollen>(id + 0x13, 5.7f, s => s.NumCasts > 0, "Seed AOEs 2")
            .ActivateOnEnter<KillerSeeds>()
            .DeactivateOnExit<Pollen>();

        ComponentCondition<KillerSeeds>(id + 0x14, 3.5f, k => k.NumFinishedStacks > 0, "Stacks");

        Cast(id + 0x20, AID.QuarrySwamp, 13, 4, "Petrify adds")
            .ActivateOnEnter<QuarrySwamp>()
            .ActivateOnEnter<SinisterSeedsSpread>()
            .ActivateOnEnter<SinisterSeedsChase>()
            .ActivateOnEnter<RootsOfEvil>()
            .DeactivateOnExit<QuarrySwamp>()
            .DeactivateOnExit<KillerSeeds>();
    }

    private void P3Seeds(uint id, float delay)
    {
        ComponentCondition<SinisterSeedsChase>(id, delay, s => s.Casters.Count > 0, "Puddles start");

        ComponentCondition<SinisterSeedsSpread>(id + 1, 7, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<SinisterSeedsSpread>();

        ComponentCondition<TendrilsOfTerror>(id + 2, 6.6f, t => t.NumCasts > 0, "Safe spot")
            .ExecOnEnter<TendrilsOfTerror>(t => t.ResetCount())
            .ActivateOnEnter<PulpSmash>()
            .ActivateOnEnter<PulpSmashProtean>()
            .ActivateOnEnter<ItCameFromTheDirt>()
            .DeactivateOnExit<SinisterSeedsChase>()
            .DeactivateOnExit<RootsOfEvil>();
    }

    private void Stoneringers2(uint id, float delay)
    {
        CastMulti(id, [AID.Stoneringer2Stoneringers1, AID.Stoneringer2Stoneringers2], delay, 2);

        Cast(id + 0x10, AID.P3StrangeSeedsVisual, 8, 4)
            .ActivateOnEnter<StrangeSeeds>()
            .ActivateOnEnter<StrangeSeedsCounter>()
            .ActivateOnEnter<Lariat>()
            .ExecOnEnter<P3BrutishSwing>(p => p.Risky = false);

        ComponentCondition<StrangeSeedsCounter>(id + 0x12, 6.1f, s => s.NumCasts > 0, "Spreads");

        ComponentCondition<P3BrutishSwing>(id + 0x14, 4.8f, b => b.NumCasts > 2, "In/out")
            .ExecOnEnter<P3BrutishSwing>(p => p.Risky = true)
            .ExecOnEnter<StrangeSeeds>(s => s.Risky = false);

        ComponentCondition<Lariat>(id + 0x20, 5.1f, l => l.NumCasts > 0, "Cleave")
            .DeactivateOnExit<Lariat>();

        ComponentCondition<StrangeSeedsCounter>(id + 0x22, 4.2f, s => s.NumCasts > 4, "Spreads")
            .ExecOnEnter<StrangeSeeds>(s => s.Risky = true)
            .DeactivateOnExit<StrangeSeeds>()
            .DeactivateOnExit<StrangeSeedsCounter>();

        ComponentCondition<P3BrutishSwing>(id + 0x24, 5.1f, b => b.NumCasts > 3, "In/out")
            .DeactivateOnExit<P3BrutishSwing>();

        ComponentCondition<Slaminator>(id + 0x26, 6.6f, s => s.NumCasts > 1, "Tower")
            .DeactivateOnExit<Slaminator>();
    }
}
