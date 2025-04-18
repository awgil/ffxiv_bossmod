namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class RM07SBruteAbombinatorStates : StateMachineBuilder
{
    public RM07SBruteAbombinatorStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Phase1(id, 5.15f);
        Phase2(id + 0x100000, 15);
        Phase3(id + 0x200000, 17.4f);
    }

    private void Phase1(uint id, float delay)
    {
        BrutalImpact(id, delay, 6, 5.6f);

        CastMulti(id + 0x10, [AID.StoneringerClub, AID.StoneringerSword], 5.2f, 2)
            .ActivateOnEnter<P1Stoneringer>()
            .ActivateOnEnter<P1Smash>();
        CastStartMulti(id + 0x20, [AID._Weaponskill_SmashHere, AID._Weaponskill_SmashThere], 5.9f);

        ComponentCondition<P1Stoneringer>(id + 0x22, 3.9f, p => p.NumCasts > 0, "In/out");
        ComponentCondition<P1Smash>(id + 0x23, 1.1f, p => p.NumCasts > 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P1Stoneringer>()
            .DeactivateOnExit<P1Smash>();

        id += 0x10000;
        Cast(id, AID._Weaponskill_SporeSac, 6.2f, 3)
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<SinisterSeedsSpread>()
            .ActivateOnEnter<SinisterSeedsChase>()
            .ActivateOnEnter<SinisterSeedsStored>()
            .ActivateOnEnter<BloomingAbomination>()
            .ActivateOnEnter<CrossingCrosswinds>()
            .ActivateOnEnter<WindingWildwinds>()
            .ActivateOnEnter<HurricaneForce>();

        ComponentCondition<SporeSac>(id + 2, 5.1f, s => s.NumCasts > 0, "Seed AOEs 1")
            .DeactivateOnExit<SporeSac>();
        ComponentCondition<Pollen>(id + 3, 5.6f, s => s.NumCasts > 0, "Seed AOEs 2")
            .ActivateOnEnter<Pollen>()
            .DeactivateOnExit<Pollen>();

        ComponentCondition<TendrilsOfTerror>(id + 0x10, 11.5f, t => t.NumCasts > 0, "Safe spot + stacks")
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<RootsOfEvil>()
            .ActivateOnEnter<TendrilsOfTerror>()
            .ExecOnExit<SinisterSeedsStored>(s => s.Activate())
            .DeactivateOnExit<Impact>()
            .DeactivateOnExit<SinisterSeedsSpread>()
            .DeactivateOnExit<SinisterSeedsChase>();

        id += 0x10000;

        ComponentCondition<RootsOfEvil>(id + 0x20, 5.4f, r => r.NumCasts > 0, "Stored AOEs")
            .DeactivateOnExit<RootsOfEvil>()
            .DeactivateOnExit<SinisterSeedsStored>();

        ComponentCondition<WindingWildwinds>(id + 0x30, 0.7f, w => w.Casters.Count > 0);

        Timeout(id + 0x32, 7, "Interruptible casts");

        CastStart(id + 0x40, AID._Weaponskill_QuarrySwamp, 11)
            .ActivateOnEnter<QuarrySwamp>();
        Timeout(id + 0x41, 0.5f, "Adds enrage");
        CastEnd(id + 0x42, 3.5f, "LoS")
            .DeactivateOnExit<BloomingAbomination>()
            .DeactivateOnExit<CrossingCrosswinds>()
            .DeactivateOnExit<WindingWildwinds>()
            .DeactivateOnExit<HurricaneForce>()
            .DeactivateOnExit<QuarrySwamp>();

        id += 0x10000;

        ComponentCondition<Explosion>(id, 12, e => e.NumCasts > 0, "Gigaflare 1")
            .ActivateOnEnter<Explosion>();
        ComponentCondition<Explosion>(id + 1, 2.5f, e => e.NumCasts > 1, "Gigaflare 2");
        ComponentCondition<Explosion>(id + 2, 2.5f, e => e.NumCasts > 2, "Gigaflare 3")
            .ActivateOnEnter<P1Stoneringer>()
            .ActivateOnEnter<P1Smash>()
            .DeactivateOnExit<Explosion>();

        id += 0x10000;

        CastStartMulti(id + 0x10, [AID._Weaponskill_SmashHere, AID._Weaponskill_SmashThere], 3.9f);

        ComponentCondition<P1Stoneringer>(id + 0x11, 3.9f, p => p.NumCasts > 0, "In/out");
        ComponentCondition<P1Smash>(id + 0x12, 1.1f, p => p.NumCasts > 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P1Stoneringer>()
            .DeactivateOnExit<P1Smash>();

        ComponentCondition<PulpSmash>(id + 0x20, 7.8f, p => p.NumFinishedStacks > 0, "Stack")
            .ActivateOnEnter<PulpSmash>()
            .ActivateOnEnter<PulpSmashProtean>()
            .ActivateOnEnter<ItCameFromTheDirt>();
        ComponentCondition<PulpSmashProtean>(id + 0x21, 2, p => p.NumCasts > 0, "Proteans")
            .DeactivateOnExit<PulpSmash>()
            .DeactivateOnExit<PulpSmashProtean>()
            .DeactivateOnExit<ItCameFromTheDirt>();

        id += 0x10000;
        Cast(id, AID._Weaponskill_NeoBombarianSpecial, 10.8f, 8, "Arena change")
            .ActivateOnEnter<NeoBombarianSpecial>()
            .SetHint(StateMachine.StateHint.DowntimeStart | StateMachine.StateHint.Raidwide)
            .Raw.Exit = () =>
            {
                Module.DeactivateComponent<NeoBombarianSpecial>();
                Module.Arena.Center = new(100, 5);
                Module.Arena.Bounds = new ArenaBoundsRect(12.5f, 25);
            };
    }

    private void Phase2(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss appears")
            .ActivateOnEnter<P2BrutishSwing>()
            .ActivateOnEnter<RevengeOfTheVines>();

        CastMulti(id + 0x10, [AID.P2StoneringerClub, AID.P2StoneringerSword], 5.25f, 2);

        CastStartMulti(id + 0x20, [AID._Weaponskill_BrutishSwing7, AID._Weaponskill_BrutishSwing41], 5.9f);

        ComponentCondition<P2BrutishSwing>(id + 0x21, 8, s => s.NumCasts > 0, "In/out");

        P2GlowerPower(id + 0x30, 0.9f);
        RevengeOfTheVines(id + 0x40, 0.9f);

        id += 0x10000;

        Sporesplosion(id, 8.2f);

        id += 0x10000;

        ComponentCondition<P2BrutishSwing>(id + 0x98, 5.1f, s => s.NumCasts > 1, "In/out")
            .ExecOnEnter<P2BrutishSwing>(p => p.Risky = true);

        P2GlowerPower(id + 0xA0, 0.9f);
        RevengeOfTheVines(id + 0xB0, 1);

        id += 0x10000;

        StrangeSeeds(id, 6.2f);

        id += 0x10000;

        ComponentCondition<P2BrutishSwing>(id + 0x24, 5.5f, s => s.NumCasts > 2, "In/out")
            .ExecOnEnter<P2BrutishSwing>(p => p.Risky = true)
            .DeactivateOnExit<P2BrutishSwing>();

        P2GlowerPower(id + 0x30, 0.9f);
        RevengeOfTheVines(id + 0x40, 0.9f);

        Cast(id + 0x50, AID._Weaponskill_Powerslam, 6.2f, 6, "Arena change")
            .ActivateOnEnter<Powerslam>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart)
            .Raw.Exit = () =>
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

        BrutalImpact(id + 0x10, 7.1f, 7, 6.7f);

        CastMulti(id + 0x20, [AID._Weaponskill_Stoneringer2Stoneringers, AID._Weaponskill_Stoneringer2Stoneringers1], 5.3f, 2)
            .ActivateOnEnter<Lariat>()
            .ActivateOnEnter<P3Glower>()
            .ActivateOnEnter<Slaminator>();

        Cast(id + 0x30, AID._Weaponskill_BrutishSwing12, 6.3f, 3);

        ComponentCondition<P3BrutishSwing>(id + 0x32, 3.8f, b => b.NumCasts > 0, "In/out");

        CastMulti(id + 0x40, [AID._Weaponskill_LashingLariat, AID._Weaponskill_LashingLariat2], 3.1f, 3.5f);

        ComponentCondition<Lariat>(id + 0x42, 0.5f, l => l.NumCasts > 0, "Cleave");

        CastMulti(id + 0x50, [AID._Weaponskill_BrutishSwing18, AID._Weaponskill_BrutishSwing15], 1.6f, 3)
            .ActivateOnEnter<P3ElectrogeneticForce>();

        ComponentCondition<P3BrutishSwing>(id + 0x52, 3.7f, b => b.NumCasts > 1, "In/out")
            .ExecOnExit<P3ElectrogeneticForce>(p => p.Risky = true);

        Cast(id + 0x60, AID._Weaponskill_GlowerPower2, 1.1f, 0.7f);
        ComponentCondition<P3ElectrogeneticForce>(id + 0x62, 1.2f, f => f.NumCasts > 0, "Spreads")
            .DeactivateOnExit<P3ElectrogeneticForce>();
        ComponentCondition<P3Glower>(id + 0x63, 0.1f, g => g.NumCasts > 0, "Line AOE")
            .DeactivateOnExit<P3Glower>();

        Cast(id + 0x70, AID._Weaponskill_Slaminator, 0.8f, 4);

        ComponentCondition<Slaminator>(id + 0x72, 1.1f, s => s.NumCasts > 0, "Tower");

        BrutalImpact(id + 0x80, 4.1f, 8, 7.7f);

        id += 0x10000;

        CastMulti(id + 0x90, [AID.StoneringerSword, AID.StoneringerClub], 3.2f, 2)
            .ActivateOnEnter<P1Stoneringer>()
            .ActivateOnEnter<P1Smash>();
        ComponentCondition<P1Stoneringer>(id + 0x92, 9.9f, p => p.NumCasts > 0, "In/out");
        ComponentCondition<P1Smash>(id + 0x93, 1.1f, p => p.NumCasts > 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P1Stoneringer>()
            .DeactivateOnExit<P1Smash>();

        Cast(id + 0xB0, AID._Spell_DebrisDeathmatch, 8.2f, 3);

        ComponentCondition<ThornsOfDeath>(id + 0xB2, 1.1f, t => t.Tethers.Count > 0, "Tethers appear");

        Cast(id + 0xC0, AID._Weaponskill_SporeSac, 1, 3)
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<P3BloomingAbomination>();

        ComponentCondition<SporeSac>(id + 0xC2, 5.1f, s => s.NumCasts > 0, "Seed AOEs 1")
            .DeactivateOnExit<SporeSac>();
        ComponentCondition<Pollen>(id + 0xC3, 5.7f, s => s.NumCasts > 0, "Seed AOEs 2")
            .ActivateOnEnter<KillerSeeds>()
            .DeactivateOnExit<Pollen>();

        Cast(id + 0xD0, AID._Weaponskill_QuarrySwamp, 16.5f, 4, "Petrify adds")
            .ActivateOnEnter<QuarrySwamp>()
            .ActivateOnEnter<SinisterSeedsSpread>()
            .ActivateOnEnter<SinisterSeedsChase>()
            .ActivateOnEnter<RootsOfEvil>()
            .DeactivateOnExit<QuarrySwamp>()
            .DeactivateOnExit<P3BloomingAbomination>()
            .DeactivateOnExit<KillerSeeds>();

        ComponentCondition<SinisterSeedsChase>(id + 0xD2, 12.3f, s => s.Casters.Count > 0, "Puddles start");

        ComponentCondition<TendrilsOfTerror>(id + 0xD4, 13.6f, t => t.NumCasts > 0, "Safe spot")
            .ExecOnEnter<TendrilsOfTerror>(t => t.ResetCount())
            .ActivateOnEnter<PulpSmash>()
            .ActivateOnEnter<PulpSmashProtean>()
            .ActivateOnEnter<ItCameFromTheDirt>()
            .DeactivateOnExit<SinisterSeedsChase>()
            .DeactivateOnExit<SinisterSeedsSpread>()
            .DeactivateOnExit<RootsOfEvil>();

        ComponentCondition<PulpSmash>(id + 0xE0, 3.5f, p => p.NumFinishedStacks > 0, "Stack");
        ComponentCondition<PulpSmashProtean>(id + 0xE1, 2.1f, p => p.NumCasts > 0, "Proteans")
            .DeactivateOnExit<PulpSmash>()
            .DeactivateOnExit<PulpSmashProtean>()
            .DeactivateOnExit<ItCameFromTheDirt>()
            .DeactivateOnExit<ThornsOfDeath>();

        BrutalImpact(id + 0xF0, 1.4f, 8, 7.7f);

        id += 0x10000;

        CastMulti(id, [AID._Weaponskill_Stoneringer2Stoneringers, AID._Weaponskill_Stoneringer2Stoneringers1], 8.2f, 2, "Weapon select");

        Cast(id + 0x10, AID._Weaponskill_StrangeSeeds1, 8, 4, "Strange seeds")
            .ActivateOnEnter<StrangeSeeds>()
            .ActivateOnEnter<StrangeSeedsCounter>()
            .ExecOnEnter<P3BrutishSwing>(p => p.Risky = false);

        ComponentCondition<StrangeSeedsCounter>(id + 0x12, 6.1f, s => s.NumCasts > 0, "Spreads");

        ComponentCondition<P3BrutishSwing>(id + 0x14, 4.8f, b => b.NumCasts > 2, "In/out")
            .ExecOnEnter<P3BrutishSwing>(p => p.Risky = true)
            .ExecOnEnter<StrangeSeeds>(s => s.Risky = false);

        ComponentCondition<Lariat>(id + 0x20, 5.1f, l => l.NumCasts > 1, "Cleave")
            .DeactivateOnExit<Lariat>();

        ComponentCondition<StrangeSeedsCounter>(id + 0x22, 4.2f, s => s.NumCasts > 4, "Spreads")
            .ExecOnEnter<StrangeSeeds>(s => s.Risky = true)
            .DeactivateOnExit<StrangeSeeds>()
            .DeactivateOnExit<StrangeSeedsCounter>();

        ComponentCondition<P3BrutishSwing>(id + 0x24, 5.1f, b => b.NumCasts > 3, "In/out")
            .DeactivateOnExit<P3BrutishSwing>();

        ComponentCondition<Slaminator>(id + 0x26, 6.6f, s => s.NumCasts > 1, "Tower")
            .DeactivateOnExit<Slaminator>();

        id += 0x10000;

        CastMulti(id + 0x90, [AID.StoneringerSword, AID.StoneringerClub], 4.1f, 2)
            .ActivateOnEnter<P1Stoneringer>()
            .ActivateOnEnter<P1Smash>();
        ComponentCondition<P1Stoneringer>(id + 0x92, 9.9f, p => p.NumCasts > 0, "In/out");
        ComponentCondition<P1Smash>(id + 0x93, 1.2f, p => p.NumCasts > 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P1Stoneringer>()
            .DeactivateOnExit<P1Smash>();

        BrutalImpact(id + 0x100, 4.1f, 8, 7.7f);

        Cast(id + 0x200, AID._Weaponskill_SpecialBombarianSpecial, 9.7f, 10, "Enrage")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void BrutalImpact(uint id, float delay, int numCasts, float duration)
    {
        Cast(id, AID._Weaponskill_BrutalImpact, delay, 5, "Raidwide 1")
            .ActivateOnEnter<BrutalImpact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpact>(id + 2, duration, b => b.NumCasts == numCasts, $"Raidwide {numCasts}")
            .DeactivateOnExit<BrutalImpact>();
    }

    private void RevengeOfTheVines(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_RevengeOfTheVines, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2GlowerPower(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_GlowerPower, delay, 2.7f)
            .ActivateOnEnter<P2ElectrogeneticForce>()
            .ActivateOnEnter<P2GlowerPower>();
        ComponentCondition<P2ElectrogeneticForce>(id + 2, 1.2f, e => e.NumCasts > 0, "Spreads")
            .DeactivateOnExit<P2ElectrogeneticForce>()
            .DeactivateOnExit<P2GlowerPower>();
    }

    private void Sporesplosion(uint id, float delay)
    {
        Cast(id, AID._Spell_ThornyDeathmatch, delay, 3)
            .ActivateOnEnter<ThornsOfDeath>()
            .ActivateOnEnter<AbominableBlink>();

        ComponentCondition<ThornsOfDeath>(id + 2, 1.1f, t => t.Tethers.Count > 0, "Tank tether appear");

        CastMulti(id + 0x10, [AID.P2StoneringerClub, AID.P2StoneringerSword], 1, 2);

        Cast(id + 0x20, AID._Weaponskill_AbominableBlink, 5.9f, 5.3f);

        ComponentCondition<AbominableBlink>(id + 0x22, 1, b => b.NumCasts > 0, "Flare");

        Cast(id + 0x30, AID._Weaponskill_Sporesplosion, 5.1f, 4)
            .ActivateOnEnter<Sporesplosion>();

        ComponentCondition<Sporesplosion>(id + 0x32, 6.1f, s => s.NumCasts >= 6, "Puddles 1");

        CastStartMulti(id + 0x40, [AID._Weaponskill_BrutishSwing10, AID._Weaponskill_BrutishSwing4], 1)
            .ExecOnEnter<P2BrutishSwing>(p => p.Risky = false);

        ComponentCondition<Sporesplosion>(id + 0x42, 1, s => s.NumCasts >= 12, "Puddles 2");

        ComponentCondition<Sporesplosion>(id + 0x44, 2, s => s.NumCasts >= 18, "Puddles 3")
            .DeactivateOnExit<Sporesplosion>();
    }

    private void StrangeSeeds(uint id, float delay)
    {
        Cast(id, AID._Spell_DemolitionDeathmatch, delay, 3);

        ComponentCondition<ThornsOfDeath>(id + 2, 0.9f, t => t.Tethers.Count > 0, "Tethers appear");

        Cast(id + 0x10, AID._Weaponskill_AbominableBlink, 7.2f, 5.3f);

        ComponentCondition<AbominableBlink>(id + 0x12, 1, b => b.NumCasts > 1, "Flare")
            .DeactivateOnExit<AbominableBlink>();

        Cast(id + 0x20, AID._Weaponskill_StrangeSeeds, 3.1f, 4)
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

        CastStartMulti(id + 0x40, [AID._Weaponskill_BrutishSwing10, AID._Weaponskill_BrutishSwing4, AID._Weaponskill_BrutishSwing11, AID._Weaponskill_BrutishSwing21], 2)
            .ExecOnEnter<P2BrutishSwing>(p => p.Risky = false);

        ComponentCondition<TendrilsOfTerror>(id + 0x42, 2.6f, t => t.Casters.Count == 0, "Star AOEs");
    }
}
