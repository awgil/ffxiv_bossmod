namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class Q01TheFinalVerseStates : StateMachineBuilder
{
    private readonly Q01TheFinalVerse _module;

    public Q01TheFinalVerseStates(Q01TheFinalVerse module) : base(module)
    {
        _module = module;

        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<LightDark>()
            .ActivateOnEnter<BossLightDark>()
            .ActivateOnEnter<ArcaneFont>()
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<BallOfFireBait>()
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<Vodoriga>()
            .ActivateOnEnter<TerrorEye>()
            .ActivateOnEnter<Flameborn>();
    }

    private void SinglePhase(uint id)
    {
        CastStartMulti(id, [AID.ScourgingBlazeHorizontalFirst, AID.ScourgingBlazeVerticalFirst], 9.2f)
            .ActivateOnEnter<ScourgingBlaze>()
            .ActivateOnEnter<Neutralize>();

        BoundsOfSin(id + 0x10, 20, 6.8f);

        ComponentCondition<Neutralize>(id + 0x30, 1.3f, n => n.NumIcons == 0, "Light/dark")
            .ActivateOnEnter<BleedTower>()
            .DeactivateOnExit<Neutralize>()
            .ExecOnExit<BleedTower>(t => t.EnableHints = true);

        ComponentCondition<BleedTower>(id + 0x40, 4.2f, t => t.Towers.Count == 0, "Towers")
            .DeactivateOnExit<BleedTower>();

        ComponentCondition<ScourgingBlaze>(id + 0x100, 7.1f, b => b.NumCasts > 0, "Exaflares start")
            .ExecOnEnter<ScourgingBlaze>(b => b.Draw = true);

        // TODO: figure out better hints for slow/fast blade of light, chains, fireball
        // it's kind of a mess atm but i really don't want to introduce forks since that will make cdplan complicated
        CastStartMulti(id + 0x10000, [AID.ChainsOfCondemnationCastFast, AID.ChainsOfCondemnationCastSlow, AID.BallOfFireCastFast, AID.BallOfFireCastSlow], 4);

        ComponentCondition<SearingChain>(id + 0x10001, 17, s => s.TethersAssigned, "Chains appear")
            .ActivateOnEnter<SearingChain>();

        // TODO: this delay is actually based on when the chains break, not a fixed timer
        ComponentCondition<SearingChainCross>(id + 0x10002, 6.2f, s => s.NumCasts > 0, "Crosses")
            .ActivateOnEnter<SearingChainCross>()
            .ActivateOnEnter<Spinelash>()
            .DeactivateOnExit<SearingChain>()
            .DeactivateOnExit<SearingChainCross>();

        ComponentCondition<Spinelash>(id + 0x20000, 2.6f, s => s.Source != null, "Target select");
        ComponentCondition<Spinelash>(id + 0x20001, 10.4f, s => s.NumCasts > 0, "Wild charge");

        ComponentCondition<Vodoriga>(id + 0x20010, 6.2f, v => v.ActiveActors.Any(), "Add appears");

        ActorCastStart(id + 0x30000, _module.Eater, AID._Spell_ShacklesOfGreaterSanctity, 7)
            .ActivateOnEnter<ShackleSpreadHint>()
            .ActivateOnEnter<ShackleHint>();

        ComponentCondition<ShackleSpreadHint>(id + 0x30010, 4.3f, s => s.Shackles, "Shackles appear")
            .DeactivateOnExit<ShackleSpreadHint>();

        CastStart(id + 0x30100, AID.HellishEarthCast, 3.7f)
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<HellishEarthPull>();

        ComponentCondition<HellishEarthPull>(id + 0x30200, 6, p => p.NumCasts > 0, "Attract to middle")
            .DeactivateOnExit<HellishEarthPull>();

        CastStartMulti(id + 0x30300, [AID._Weaponskill_ManifoldLashings, AID._Weaponskill_ManifoldLashings4], 3)
            .ActivateOnEnter<ManifoldLashingsTower>()
            .ActivateOnEnter<ManifoldLashingsTail>();

        ComponentCondition<Eruption>(id + 0x30301, 1.1f, e => e.Casters.Count > 0, "Baited puddles start");

        ManifoldLashings(id + 0x30310, 5.2f);

        ActorCast(id + 0x30500, _module.Eater, AID._Spell_UnholyDarkness, 5.5f, 6)
            .ActivateOnEnter<UnholyDarkness>();

        ComponentCondition<UnholyDarkness>(id + 0x30502, 0.7f, u => u.NumCasts > 0, "Raidwide (bleed)")
            .DeactivateOnExit<UnholyDarkness>();

        CastStartMulti(id + 0x30600, [AID._Weaponskill_ManifoldLashings, AID._Weaponskill_ManifoldLashings4], 2.3f)
            .ActivateOnEnter<ManifoldLashingsTower>()
            .ActivateOnEnter<ManifoldLashingsTail>();

        ManifoldLashings(id + 0x30610, 6.3f);

        ComponentCondition<Eruption>(id + 0x30620, 13.4f, e => e.Casters.Count > 0, "Baited puddles start");

        ComponentCondition<ShackleHint>(id + 0x30700, 3.2f, s => s.Expired, "Shackles disappear")
            .DeactivateOnExit<ShackleHint>()
            .DeactivateOnExit<ArcaneFont>();

        BoundsOfSin(id + 0x40000, 15.3f, 7.8f);

        ComponentCondition<ScourgingBlaze>(id + 0x40200, 3.3f, b => b.NumCasts > 0, "Exaflares start")
            .ExecOnEnter<ScourgingBlaze>(b => b.Draw = true)
            .ActivateOnEnter<Neutralize>();

        ComponentCondition<Neutralize>(id + 0x40100, 3.8f, n => n.NumIcons > 0);

        ComponentCondition<Neutralize>(id + 0x40101, 5.1f, n => n.NumIcons == 0, "Light/dark")
            .DeactivateOnExit<Neutralize>();

        ActorCast(id + 0x50000, _module.Eater, AID._Spell_CrimeAndPunishment, 11.9f, 6, false, "Apply rot")
            .ActivateOnEnter<CrimeAndPunishmentHint>()
            .ActivateOnEnter<SinBearer>()
            .ActivateOnEnter<Doom>()
            .DeactivateOnExit<CrimeAndPunishmentHint>();

        ComponentCondition<Spinelash>(id + 0x50100, 26.9f, s => s.Source != null, "Target select");
        ComponentCondition<Spinelash>(id + 0x50101, 10.4f, s => s.NumCasts > 1, "Wild charge");
        ComponentCondition<Vodoriga>(id + 0x50200, 6.1f, v => v.ActiveActors.Any(), "Add appears")
            .ActivateOnEnter<SearingChain>()
            .ActivateOnEnter<SearingChainCross>();

        BoundsOfSin(id + 0x50300, 7, 6.7f, "Out");

        ComponentCondition<SearingChainCross>(id + 0x50400, 1.8f, s => s.NumCasts > 0, "Crosses")
            .DeactivateOnExit<SearingChain>()
            .DeactivateOnExit<SearingChainCross>();

        ComponentCondition<Neutralize>(id + 0x50500, 10.3f, n => n.NumIcons > 0)
            .ActivateOnEnter<Neutralize>();
        ComponentCondition<Neutralize>(id + 0x50501, 5.1f, n => n.NumIcons == 0, "Light/dark")
            .DeactivateOnExit<Neutralize>();

        CastStartMulti(id + 0x60000, [AID.DrainAetherLightFast, AID.DrainAetherLightSlow], 3)
            .ActivateOnEnter<DrainAether>();

        ComponentCondition<DrainAether>(id + 0x60010, 7, d => d.NumCasts > 0, "Drain 1");
        ComponentCondition<DrainAether>(id + 0x60011, 5, d => d.NumCasts > 1, "Drain 2")
            .DeactivateOnExit<SinBearer>()
            .DeactivateOnExit<Doom>()
            .DeactivateOnExit<DrainAether>();

        ComponentCondition<ScourgingBlaze>(id + 0x60100, 7, s => s.NumCasts > 0, "Exaflares start")
            .ExecOnEnter<ScourgingBlaze>(b => b.Draw = true);

        ActorCast(id + 0x60200, _module.Eater, AID._Spell_UnholyDarkness, 2.1f, 6)
            .ActivateOnEnter<UnholyDarkness>();

        ComponentCondition<UnholyDarkness>(id + 0x60210, 0.7f, u => u.NumCasts > 0, "Raidwide (bleed)")
            .DeactivateOnExit<UnholyDarkness>();

        Timeout(id + 0xFF0000, 10000, "???");
    }

    private State BoundsOfSin(uint id, float delay, float jailDelay, string tag = "In/out")
    {
        ActorCastStart(id, _module.Eater, AID.BoundsOfSinBossCast, delay)
            .ActivateOnEnter<BoundsOfSinBind>()
            .ActivateOnEnter<BoundsOfSinInOut>()
            .ActivateOnEnter<BoundsOfSinIcicle>();

        ComponentCondition<BoundsOfSinBind>(id + 1, 4, b => b.NumCasts > 0, "Bind")
            .DeactivateOnExit<BoundsOfSinBind>();

        return ComponentCondition<BoundsOfSinInOut>(id + 2, jailDelay, b => b.NumCasts > 0, tag)
            .DeactivateOnExit<BoundsOfSinInOut>()
            .DeactivateOnExit<BoundsOfSinIcicle>();
    }

    private void ManifoldLashings(uint id, float delay)
    {
        // TODO: tail direction determined by this cast, we should show the hint early
        //CastStartMulti(id, [AID._Weaponskill_ManifoldLashings, AID._Weaponskill_ManifoldLashings4], delay)
        //    .ActivateOnEnter<ManifoldLashingsTower>()
        //    .ActivateOnEnter<ManifoldLashingsTail>();

        ComponentCondition<ManifoldLashingsTower>(id, delay, m => m.NumCasts > 0, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<ManifoldLashingsTower>(id + 1, 1.4f, m => m.NumCasts == 3, "Tankbuster 3")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<ManifoldLashingsTower>();

        ComponentCondition<ManifoldLashingsTail>(id + 2, 2.9f, m => m.NumCasts > 0, "Tail")
            .DeactivateOnExit<ManifoldLashingsTail>();
    }
}
