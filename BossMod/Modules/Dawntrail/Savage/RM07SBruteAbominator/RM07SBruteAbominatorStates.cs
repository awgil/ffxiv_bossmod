namespace BossMod.Dawntrail.Savage.RM07BruteAbominator;

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
        ComponentCondition<BrutalImpact>(id + 2, 5.6f, b => b.NumCasts >= 6, "Raidwide 6")
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
        Cast(id, AID._Weaponskill_SporeSac, 6.3f, 3)
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<SinisterSeedsSpread>()
            .ActivateOnEnter<SinisterSeedsChase>()
            .ActivateOnEnter<SinisterSeedsStored>()
            .ActivateOnEnter<TendrilsOfTerror>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<BloomingAbomination>()
            .ActivateOnEnter<CrossingCrosswinds>()
            .ActivateOnEnter<WindingWildwinds>()
            .ActivateOnEnter<HurricaneForce>();

        ComponentCondition<SporeSac>(id + 2, 5.05f, s => s.NumCasts > 0, "Seed AOEs 1")
            .DeactivateOnExit<SporeSac>();
        ComponentCondition<Pollen>(id + 3, 5.75f, s => s.NumCasts > 0, "Seed AOEs 2")
            .DeactivateOnExit<Pollen>();

        ComponentCondition<TendrilsOfTerror>(id + 0x10, 11.5f, t => t.NumCasts > 0, "Safe spot + stacks")
            .ActivateOnEnter<RootsOfEvil>()
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
            .ActivateOnEnter<P2Stoneringer>()
            .ActivateOnEnter<GlowerPowerP2>()
            .ActivateOnEnter<RevengeOfTheVines>();

        CastMulti(id + 0x10, [AID.StoneringerClubP2, AID.StoneringerSwordP2], 5.25f, 2);

        CastStartMulti(id + 0x20, [AID._Weaponskill_BrutishSwing7, AID._Weaponskill_BrutishSwing41], 5.9f);

        ComponentCondition<P2Stoneringer>(id + 0x21, 8, s => s.NumCasts > 0, "In/out");

        Cast(id + 0x30, AID._Weaponskill_GlowerPower, 0.9f, 2.6f)
            .ActivateOnEnter<ElectrogeneticForceP2>();

        ComponentCondition<ElectrogeneticForceP2>(id + 0x32, 1.2f, e => e.NumCasts > 0, "Spreads")
            .DeactivateOnExit<ElectrogeneticForceP2>();

        Cast(id + 0x40, AID._Weaponskill_RevengeOfTheVines, 0.9f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;

        Cast(id + 0x50, AID._Spell_ThornyDeathmatch, 8.2f, 3)
            .ActivateOnEnter<ThornsOfDeath>()
            .ActivateOnEnter<AbominableBlink>();

        ComponentCondition<ThornsOfDeath>(id + 0x52, 1.1f, t => t.Tethers.Count > 0, "Tank tether appear");

        CastMulti(id + 0x60, [AID.StoneringerClubP2, AID.StoneringerSwordP2], 1, 2);

        Cast(id + 0x70, AID._Weaponskill_AbominableBlink, 5.9f, 5.3f);

        ComponentCondition<AbominableBlink>(id + 0x72, 1, b => b.NumCasts > 0, "Flare");

        Cast(id + 0x80, AID._Weaponskill_Sporesplosion, 5.1f, 4)
            .ActivateOnEnter<Sporesplosion>();

        ComponentCondition<Sporesplosion>(id + 0x82, 6.1f, s => s.NumCasts >= 6, "Puddles 1");

        CastStartMulti(id + 0x90, [AID._Weaponskill_BrutishSwing10, AID._Weaponskill_BrutishSwing4], 1);

        ComponentCondition<Sporesplosion>(id + 0x92, 1, s => s.NumCasts >= 12, "Puddles 2");

        ComponentCondition<Sporesplosion>(id + 0x94, 2, s => s.NumCasts >= 18, "Puddles 3")
            .DeactivateOnExit<Sporesplosion>();

        id += 0x10000;

        ComponentCondition<P2Stoneringer>(id + 0x98, 5.1f, s => s.NumCasts > 1, "In/out");

        Cast(id + 0xA0, AID._Weaponskill_GlowerPower, 0.9f, 2.7f)
            .ActivateOnEnter<ElectrogeneticForceP2>();

        ComponentCondition<ElectrogeneticForceP2>(id + 0xA2, 1.2f, e => e.NumCasts > 0, "Spreads")
            .DeactivateOnExit<ElectrogeneticForceP2>();

        Cast(id + 0xB0, AID._Weaponskill_RevengeOfTheVines, 1, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;

        Cast(id + 0xC0, AID._Spell_DemolitionDeathmatch, 6.2f, 3);

        ComponentCondition<ThornsOfDeath>(id + 0xC2, 0.9f, t => t.Tethers.Count > 0, "Tethers appear");

        Cast(id + 0xD0, AID._Weaponskill_AbominableBlink, 8.1f, 5.3f);

        ComponentCondition<AbominableBlink>(id + 0xD2, 1, b => b.NumCasts > 1, "Flare")
            .DeactivateOnExit<AbominableBlink>();

        id += 0x100;
        Cast(id, AID._Weaponskill_StrangeSeeds, 4.1f, 4)
            .ActivateOnEnter<StrangeSeeds>();

        ComponentCondition<StrangeSeeds>(id + 2, 1.2f, s => s.ActiveSpreads.Any(), "Seeds start");

        ComponentCondition<StrangeSeeds>(id + 4, 5, s => s.NumFinishedSpreads == 2, "Seeds 1");

        CastStartMulti(id + 0x10, [AID.StoneringerClubP2, AID.StoneringerSwordP2], 5.25f);

        ComponentCondition<StrangeSeeds>(id + 0x12, 5.1f, s => s.NumFinishedSpreads == 4, "Seeds 2");

        ComponentCondition<StrangeSeeds>(id + 0x14, 5.1f, s => s.NumFinishedSpreads == 6, "Seeds 3");

        ComponentCondition<StrangeSeeds>(id + 0x16, 5.1f, s => s.NumFinishedSpreads == 8, "Seeds 4")
            .DeactivateOnExit<StrangeSeeds>();

        ComponentCondition<KillerSeeds>(id + 0x18, 9.8f, k => k.NumFinishedStacks > 0, "Pairs")
            .ActivateOnEnter<KillerSeeds>()
            .DeactivateOnExit<KillerSeeds>();

        id += 0x10000;

        CastMulti(id + 0x20, [AID._Weaponskill_BrutishSwing10, AID._Weaponskill_BrutishSwing4, AID._Weaponskill_BrutishSwing11, AID._Weaponskill_BrutishSwing21], 2.1f, 4);

        ComponentCondition<P2Stoneringer>(id + 0x22, 4.1f, s => s.NumCasts > 2, "In/out")
            .DeactivateOnExit<P2Stoneringer>();

        Cast(id + 0x30, AID._Weaponskill_GlowerPower, 0.9f, 2.7f)
            .ActivateOnEnter<ElectrogeneticForceP2>();
        ComponentCondition<ElectrogeneticForceP2>(id + 0x32, 1.2f, e => e.NumCasts > 0, "Spreads")
            .DeactivateOnExit<ElectrogeneticForceP2>()
            .DeactivateOnExit<GlowerPowerP2>();

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
        Targetable(id, true, delay, "Boss appears");

        Cast(id + 0x10, AID._Weaponskill_BrutalImpact, 7.1f, 5, "Raidwide");
        CastMulti(id + 0x20, [AID._Weaponskill_Stoneringer2Stoneringers, AID._Weaponskill_Stoneringer2Stoneringers1], 12, 2, "Weapon select");

        Cast(id + 0x30, AID._Weaponskill_BrutishSwing12, 6.3f, 3, "Jump");

        Cast(id + 0x40, AID._Weaponskill_LashingLariat, 6.9f, 3.5f, "Lariat");

        Cast(id + 0x50, AID._Weaponskill_BrutishSwing18, 2.1f, 3, "Jump");
        Cast(id + 0x60, AID._Weaponskill_GlowerPower2, 4.8f, 0.7f, "Glower");
        Cast(id + 0x70, AID._Weaponskill_Slaminator, 2.1f, 4, "Tower");
        Cast(id + 0x80, AID._Weaponskill_BrutalImpact, 5.1f, 5, "Raidwide");
        CastMulti(id + 0x90, [AID.StoneringerSword, AID.StoneringerClub], 10.9f, 2, "Select weapon");
        CastMulti(id + 0xA0, [AID._Weaponskill_SmashHere, AID._Weaponskill_SmashThere], 5.9f, 3, "Tankbuster");
        Cast(id + 0xB0, AID._Spell_DebrisDeathmatch, 10.3f, 3, "Debris deathmatch");
        Cast(id + 0xC0, AID._Weaponskill_SporeSac, 2.2f, 3, "Spores");
        Cast(id + 0xD0, AID._Weaponskill_QuarrySwamp, 27.3f, 4, "Petrify");
    }
}
