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

        Timeout(id + 0x2FF000, 9999, "Enrage");
    }

    private void Phase1(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_BrutalImpact, delay, 5, "Raidwide 1")
            .ActivateOnEnter<BrutalImpact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpact>(id + 2, 5.6f, b => b.NumCasts == 6, "Raidwide 6")
            .DeactivateOnExit<BrutalImpact>();

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
            .ActivateOnEnter<P2BrutishSwingIn>()
            .ActivateOnEnter<P2BrutishSwingOut>()
            .ActivateOnEnter<P2BrutishSwingCounter>()
            .ActivateOnEnter<P2GlowerPower>()
            .ActivateOnEnter<RevengeOfTheVines>();

        CastMulti(id + 0x10, [AID.P2StoneringerClub, AID.P2StoneringerSword], 5.25f, 2);

        CastStartMulti(id + 0x20, [AID._Weaponskill_BrutishSwing7, AID._Weaponskill_BrutishSwing41], 5.9f);

        ComponentCondition<P2BrutishSwingCounter>(id + 0x21, 8, s => s.NumCasts > 0, "In/out");

        Cast(id + 0x30, AID._Weaponskill_GlowerPower, 0.9f, 2.6f)
            .ActivateOnEnter<P2ElectrogeneticForce>();

        ComponentCondition<P2ElectrogeneticForce>(id + 0x32, 1.2f, e => e.NumCasts > 0, "Spreads")
            .DeactivateOnExit<P2ElectrogeneticForce>();

        Cast(id + 0x40, AID._Weaponskill_RevengeOfTheVines, 0.9f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;

        Cast(id + 0x50, AID._Spell_ThornyDeathmatch, 8.2f, 3)
            .ActivateOnEnter<ThornsOfDeath>()
            .ActivateOnEnter<AbominableBlink>();

        ComponentCondition<ThornsOfDeath>(id + 0x52, 1.1f, t => t.Tethers.Count > 0, "Tank tether appear");

        CastMulti(id + 0x60, [AID.P2StoneringerClub, AID.P2StoneringerSword], 1, 2);

        Cast(id + 0x70, AID._Weaponskill_AbominableBlink, 5.9f, 5.3f);

        ComponentCondition<AbominableBlink>(id + 0x72, 1, b => b.NumCasts > 0, "Flare");

        Cast(id + 0x80, AID._Weaponskill_Sporesplosion, 5.1f, 4)
            .ActivateOnEnter<Sporesplosion>();

        ComponentCondition<Sporesplosion>(id + 0x82, 6.1f, s => s.NumCasts >= 6, "Puddles 1");

        CastStartMulti(id + 0x90, [AID._Weaponskill_BrutishSwing10, AID._Weaponskill_BrutishSwing4], 1)
            .ExecOnEnter<P2BrutishSwingIn>(p => p.Risky = false)
            .ExecOnEnter<P2BrutishSwingOut>(p => p.Risky = false);

        ComponentCondition<Sporesplosion>(id + 0x92, 1, s => s.NumCasts >= 12, "Puddles 2");

        ComponentCondition<Sporesplosion>(id + 0x94, 2, s => s.NumCasts >= 18, "Puddles 3")
            .DeactivateOnExit<Sporesplosion>()
            .ExecOnExit<P2BrutishSwingIn>(p => p.Risky = true)
            .ExecOnExit<P2BrutishSwingOut>(p => p.Risky = true);

        id += 0x10000;

        ComponentCondition<P2BrutishSwingCounter>(id + 0x98, 5.1f, s => s.NumCasts > 1, "In/out");

        Cast(id + 0xA0, AID._Weaponskill_GlowerPower, 0.9f, 2.7f)
            .ActivateOnEnter<P2ElectrogeneticForce>();

        ComponentCondition<P2ElectrogeneticForce>(id + 0xA2, 1.2f, e => e.NumCasts > 0, "Spreads")
            .DeactivateOnExit<P2ElectrogeneticForce>();

        Cast(id + 0xB0, AID._Weaponskill_RevengeOfTheVines, 1, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;

        Cast(id + 0xC0, AID._Spell_DemolitionDeathmatch, 6.2f, 3);

        ComponentCondition<ThornsOfDeath>(id + 0xC2, 0.9f, t => t.Tethers.Count > 0, "Tethers appear");

        Cast(id + 0xD0, AID._Weaponskill_AbominableBlink, 7.2f, 5.3f);

        ComponentCondition<AbominableBlink>(id + 0xD2, 1, b => b.NumCasts > 1, "Flare")
            .DeactivateOnExit<AbominableBlink>();

        id += 0x100;
        Cast(id, AID._Weaponskill_StrangeSeeds, 3.1f, 4)
            .ActivateOnEnter<StrangeSeeds>()
            .ActivateOnEnter<StrangeSeedsCounter>();

        ComponentCondition<StrangeSeeds>(id + 2, 1.2f, s => s.ActiveSpreads.Any(), "Seeds start");

        ComponentCondition<StrangeSeedsCounter>(id + 4, 5, s => s.NumCasts == 2, "Seeds 1");

        CastStartMulti(id + 0x10, [AID.P2StoneringerClub, AID.P2StoneringerSword], 5.25f);

        ComponentCondition<StrangeSeedsCounter>(id + 0x12, 5, s => s.NumCasts == 4, "Seeds 2");
        ComponentCondition<StrangeSeedsCounter>(id + 0x13, 5, s => s.NumCasts == 6, "Seeds 3");
        ComponentCondition<StrangeSeedsCounter>(id + 0x14, 5, s => s.NumCasts == 8, "Seeds 4")
            .DeactivateOnExit<StrangeSeeds>()
            .DeactivateOnExit<StrangeSeedsCounter>();

        ComponentCondition<KillerSeeds>(id + 0x18, 9.8f, k => k.NumFinishedStacks > 0, "Pairs")
            .ActivateOnEnter<KillerSeeds>()
            .DeactivateOnExit<KillerSeeds>();

        id += 0x10000;

        CastStartMulti(id + 0x20, [AID._Weaponskill_BrutishSwing10, AID._Weaponskill_BrutishSwing4, AID._Weaponskill_BrutishSwing11, AID._Weaponskill_BrutishSwing21], 2)
            .ExecOnEnter<P2BrutishSwingIn>(p => p.Risky = false)
            .ExecOnEnter<P2BrutishSwingOut>(p => p.Risky = false);

        ComponentCondition<TendrilsOfTerror>(id + 0x22, 2.6f, t => t.Casters.Count == 0, "Star AOEs")
            .ExecOnExit<P2BrutishSwingIn>(p => p.Risky = true)
            .ExecOnExit<P2BrutishSwingOut>(p => p.Risky = true);

        ComponentCondition<P2BrutishSwingCounter>(id + 0x24, 5.5f, s => s.NumCasts > 2, "In/out")
            .DeactivateOnExit<P2BrutishSwingCounter>()
            .DeactivateOnExit<P2BrutishSwingIn>()
            .DeactivateOnExit<P2BrutishSwingOut>();

        Cast(id + 0x30, AID._Weaponskill_GlowerPower, 0.9f, 2.7f)
            .ActivateOnEnter<P2ElectrogeneticForce>();
        ComponentCondition<P2ElectrogeneticForce>(id + 0x32, 1.2f, e => e.NumCasts > 0, "Spreads")
            .DeactivateOnExit<P2ElectrogeneticForce>()
            .DeactivateOnExit<P2GlowerPower>();

        Cast(id + 0x40, AID._Weaponskill_RevengeOfTheVines, 0.9f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

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
            .ActivateOnEnter<P3BrutishSwingIn>()
            .ActivateOnEnter<P3BrutishSwingOut>()
            .ActivateOnEnter<P3BrutishSwingCounter>();

        Cast(id + 0x10, AID._Weaponskill_BrutalImpact, 7.1f, 5, "Raidwide 1")
            .ActivateOnEnter<BrutalImpact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpact>(id + 0x12, 6.7f, b => b.NumCasts == 7, "Raidwide 7")
            .DeactivateOnExit<BrutalImpact>();

        CastMulti(id + 0x20, [AID._Weaponskill_Stoneringer2Stoneringers, AID._Weaponskill_Stoneringer2Stoneringers1], 5.3f, 2)
            .ActivateOnEnter<Lariat>()
            .ActivateOnEnter<GlowerP3>()
            .ActivateOnEnter<Slaminator>();

        Cast(id + 0x30, AID._Weaponskill_BrutishSwing12, 6.3f, 3);

        ComponentCondition<P3BrutishSwingCounter>(id + 0x32, 3.8f, b => b.NumCasts > 0, "In/out");

        CastMulti(id + 0x40, [AID._Weaponskill_LashingLariat, AID._Weaponskill_LashingLariat2], 3.1f, 3.5f);

        ComponentCondition<Lariat>(id + 0x42, 0.5f, l => l.NumCasts > 0, "Cleave");

        CastMulti(id + 0x50, [AID._Weaponskill_BrutishSwing18, AID._Weaponskill_BrutishSwing15], 1.6f, 3)
            .ActivateOnEnter<P3ElectrogeneticForce>();

        ComponentCondition<P3BrutishSwingCounter>(id + 0x52, 3.7f, b => b.NumCasts > 1, "In/out")
            .ExecOnExit<P3ElectrogeneticForce>(p => p.Risky = true);

        Cast(id + 0x60, AID._Weaponskill_GlowerPower2, 1.1f, 0.7f);
        ComponentCondition<P3ElectrogeneticForce>(id + 0x62, 1.2f, f => f.NumCasts > 0, "Spreads");
        ComponentCondition<GlowerP3>(id + 0x63, 0.1f, g => g.NumCasts > 0, "Line AOE");

        Cast(id + 0x70, AID._Weaponskill_Slaminator, 0.8f, 4);

        ComponentCondition<Slaminator>(id + 0x72, 1.1f, s => s.NumCasts > 0, "Tower");

        Cast(id + 0x80, AID._Weaponskill_BrutalImpact, 4.1f, 5, "Raidwide 1")
            .ActivateOnEnter<BrutalImpact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpact>(id + 0x82, 7.7f, b => b.NumCasts == 8, "Raidwide 8")
            .DeactivateOnExit<BrutalImpact>();

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

        Cast(id + 0xC0, AID._Weaponskill_SporeSac, 2.2f, 3)
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<Pollen>();

        ComponentCondition<SporeSac>(id + 0xC2, 5.1f, s => s.NumCasts > 0, "Seed AOEs 1")
            .DeactivateOnExit<SporeSac>();
        ComponentCondition<Pollen>(id + 0xC3, 5.7f, s => s.NumCasts > 0, "Seed AOEs 2")
            .DeactivateOnExit<Pollen>();

        Cast(id + 0xD0, AID._Weaponskill_QuarrySwamp, 27.3f, 4, "Petrify");
    }
}
