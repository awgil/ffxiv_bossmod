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
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<Vodoriga>()
            .ActivateOnEnter<TerrorEye>();
    }

    private void SinglePhase(uint id)
    {
        CastStartMulti(id, [AID.ScourgingBlazeHorizontalFirst, AID.ScourgingBlazeVerticalFirst], 9.2f)
            .ActivateOnEnter<ScourgingBlaze>()
            .ActivateOnEnter<Neutralize>();

        BoundsOfSin(id + 0x10, 20, 6.8f);

        ComponentCondition<Neutralize>(id + 0x30, 1.3f, n => n.NumIcons == 0, "Light/dark")
            .ActivateOnEnter<AbyssalSunTower>()
            .DeactivateOnExit<Neutralize>()
            .ExecOnExit<AbyssalSunTower>(t => t.EnableHints = true);

        ComponentCondition<AbyssalSunTower>(id + 0x40, 4.2f, t => t.Towers.Count == 0, "Towers")
            .DeactivateOnExit<AbyssalSunTower>();

        ComponentCondition<ScourgingBlaze>(id + 0x100, 7.1f, b => b.NumCasts > 0, "Exaflares start")
            .ExecOnEnter<ScourgingBlaze>(b => b.Draw = true);

        BladeMechanic(id + 0x10000, 4);

        ComponentCondition<SearingChain>(id + 0x10100, 9.2f, s => s.TethersAssigned, "Chains appear")
            .ActivateOnEnter<SearingChain>();

        // TODO: this delay is actually based on when the chains break, not a fixed timer
        ComponentCondition<SearingChainCross>(id + 0x10101, 6.2f, s => s.NumCasts > 0, "Crosses")
            .ActivateOnEnter<SearingChainCross>()
            .ActivateOnEnter<Spinelash>()
            .DeactivateOnExit<SearingChain>()
            .DeactivateOnExit<SearingChainCross>();

        ComponentCondition<Spinelash>(id + 0x20000, 2.6f, s => s.Source != null, "Target select");
        ComponentCondition<Spinelash>(id + 0x20001, 10.4f, s => s.NumCasts > 0, "Wild charge");

        ComponentCondition<Vodoriga>(id + 0x20010, 6.2f, v => v.ActiveActors.Any(), "Add appears");

        ActorCastStart(id + 0x30000, _module.Eater, AID.ShacklesOfGreaterSanctity, 7)
            .ActivateOnEnter<ShackleSpreadHint>()
            .ActivateOnEnter<ShackleHint>();

        ComponentCondition<ShackleSpreadHint>(id + 0x30010, 4.3f, s => s.Shackles, "Shackles appear")
            .DeactivateOnExit<ShackleSpreadHint>();

        CastStart(id + 0x30100, AID.HellishEarthCast, 3.7f)
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<HellishEarthPull>();

        ComponentCondition<HellishEarthPull>(id + 0x30200, 6, p => p.NumCasts > 0, "Attract to middle")
            .DeactivateOnExit<HellishEarthPull>();

        ManifoldLashingsCast(id + 0x30300, 3);
        ComponentCondition<Eruption>(id + 0x30301, 1.1f, e => e.Casters.Count > 0, "Baited puddles start");
        ManifoldLashings(id + 0x30310, 5.2f);

        ActorCast(id + 0x30500, _module.Eater, AID.UnholyDarknessVisual, 5.5f, 6)
            .ActivateOnEnter<UnholyDarkness>();

        ComponentCondition<UnholyDarkness>(id + 0x30502, 0.7f, u => u.NumCasts > 0, "Raidwide (bleed)")
            .DeactivateOnExit<UnholyDarkness>();

        ManifoldLashingsCast(id + 0x30600, 2.3f);
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

        ActorCast(id + 0x50000, _module.Eater, AID.CrimeAndPunishmentCast, 11.9f, 6, false, "Apply rot")
            .ActivateOnEnter<CrimeAndPunishmentHint>()
            .ActivateOnEnter<SinBearer>()
            .ActivateOnEnter<Doom>()
            .DeactivateOnExit<CrimeAndPunishmentHint>();

        BladeMechanic(id + 0x50010, 5);

        ComponentCondition<Spinelash>(id + 0x50100, 13.9f, s => s.Source != null, "Target select");
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

        ActorCast(id + 0x60200, _module.Eater, AID.UnholyDarknessVisual, 2.1f, 6)
            .ActivateOnEnter<UnholyDarkness>();

        ComponentCondition<UnholyDarkness>(id + 0x60210, 0.7f, u => u.NumCasts > 0, "Raidwide (bleed)")
            .DeactivateOnExit<UnholyDarkness>();

        Cast(id + 0x70000, AID.FeveredFlame, 15.4f, 4)
            .ActivateOnEnter<Flameborn>()
            .ActivateOnEnter<FlamebornAura>()
            .ActivateOnEnter<SelfDestruct>();

        ComponentCondition<Flameborn>(id + 0x70010, 3.2f, f => f.ActiveActors.Any(), "Adds appear");

        CastMulti(id + 0x80000, [AID.ScourgingBlazeHorizontalFirst, AID.ScourgingBlazeVerticalFirst], 6, 3);

        BladeMechanic(id + 0x80010, 24)
            .ActivateOnEnter<AbyssalSunTower>()
            .DeactivateOnExit<Flameborn>()
            .DeactivateOnExit<FlamebornAura>()
            .DeactivateOnExit<SelfDestruct>();

        BoundsOfSinStart(id + 0x80020, 5);

        ComponentCondition<AbyssalSunTower>(id + 0x80030, 2, t => t.Towers.Count == 0, "Towers")
            .DeactivateOnExit<AbyssalSunTower>();

        BoundsOfSinFinish(id + 0x80100, 2.1f, 7.8f);

        ComponentCondition<Spinelash>(id + 0x80200, 0.1f, s => s.Source != null, "Target select")
            .ActivateOnEnter<Neutralize>();
        ComponentCondition<Spinelash>(id + 0x80201, 10.4f, s => s.NumCasts > 2, "Wild charge");
        ComponentCondition<Neutralize>(id + 0x80202, 0.9f, n => n.NumIcons == 0, "Light/dark")
            .DeactivateOnExit<Neutralize>()
            .ExecOnExit<ScourgingBlaze>(b => b.Draw = true);

        ComponentCondition<Vodoriga>(id + 0x80300, 5.3f, v => v.ActiveActors.Any(), "Add appears");
        ComponentCondition<ScourgingBlaze>(id + 0x80301, 1.6f, s => s.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<SearingChain>()
            .ActivateOnEnter<SearingChainCross>();

        ComponentCondition<SearingChain>(id + 0x80400, 10.3f, s => s.TethersAssigned, "Chains appear");
        ComponentCondition<SearingChainCross>(id + 0x80410, 5.5f, s => s.NumCasts > 0, "Crosses")
            .DeactivateOnExit<SearingChain>()
            .DeactivateOnExit<SearingChainCross>();

        ManifoldLashingsCast(id + 0x80500, 3.5f);
        ManifoldLashings(id + 0x80510, 6.2f);

        ComponentCondition<Eruption>(id + 0x80600, 4.4f, e => e.Casters.Count > 0, "Baited puddles start")
            .ActivateOnEnter<DrainAether>();

        ComponentCondition<DrainAether>(id + 0x80610, 11, d => d.NumCasts > 0, "Drain 1");
        ComponentCondition<DrainAether>(id + 0x80611, 5, d => d.NumCasts > 1, "Drain 2")
            .DeactivateOnExit<DrainAether>();

        ComponentCondition<Eruption>(id + 0x80700, 9.1f, e => e.Casters.Count > 0, "Baited puddles start");

        BoundsOfSin(id + 0x80800, 5.9f, 7.8f);

        BladeMechanic(id + 0x80900, 0);

        ActorCast(id + 0x90000, _module.Eater, AID.UnholyDarknessEnrageCast, 9.2f, 9)
            .ActivateOnEnter<UnholyDarknessEnrage>();
        ComponentCondition<UnholyDarknessEnrage>(id + 0x90010, 0.7f, d => d.NumCasts > 0, "Raidwide (bleed)");

        CastEnd(id + 0x90100, 17.3f, "Enrage");
    }

    private State BoundsOfSin(uint id, float delay, float jailDelay, string tag = "In/out")
    {
        BoundsOfSinStart(id, delay);
        return BoundsOfSinFinish(id + 1, 4, jailDelay, tag);
    }

    private void BoundsOfSinStart(uint id, float delay)
    {
        ActorCastStart(id, _module.Eater, AID.BoundsOfSinBossCast, delay)
            .ActivateOnEnter<BoundsOfSinBind>()
            .ActivateOnEnter<BoundsOfSinInOut>()
            .ActivateOnEnter<BoundsOfSinIcicle>();
    }

    private State BoundsOfSinFinish(uint id, float delay, float jailDelay, string tag = "In/out")
    {
        ComponentCondition<BoundsOfSinBind>(id + 1, delay, b => b.NumCasts > 0, "Bind")
            .DeactivateOnExit<BoundsOfSinBind>();

        return ComponentCondition<BoundsOfSinInOut>(id + 2, jailDelay, b => b.NumCasts > 0, tag)
            .DeactivateOnExit<BoundsOfSinInOut>()
            .DeactivateOnExit<BoundsOfSinIcicle>();
    }

    private State BladeMechanic(uint id, float delay)
    {
        CastStartMulti(id, [AID.ChainsOfCondemnationCastFast, AID.ChainsOfCondemnationCastSlow, AID.BallOfFireCastFast, AID.BallOfFireCastSlow], delay)
            .ActivateOnEnter<BallOfFireBait>()
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<BladeStillnessFireCounter>();

        var st = ComponentCondition<BladeStillnessFireCounter>(id + 1, 5, b => b.NumCasts > 0, "In/out/fireball/stay 1");
        ComponentCondition<BladeStillnessFireCounter>(id + 2, 3, b => b.NumCasts > 1, "In/out/fireball/stay 2")
            .DeactivateOnExit<BallOfFireBait>()
            .DeactivateOnExit<BladeOfFirstLight>()
            .DeactivateOnExit<ChainsOfCondemnation>()
            .DeactivateOnExit<BladeStillnessFireCounter>();

        return st;
    }

    private void ManifoldLashingsCast(uint id, float delay)
    {
        CastStartMulti(id, [AID.ManifoldLashingsCast1, AID.ManifoldLashingsCast2], delay)
            .ActivateOnEnter<ManifoldLashingsTower>()
            .ActivateOnEnter<ManifoldLashingsTail>();
    }

    private void ManifoldLashings(uint id, float delay)
    {
        ComponentCondition<ManifoldLashingsTower>(id, delay, m => m.NumCasts > 0, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<ManifoldLashingsTower>(id + 1, 1.4f, m => m.NumCasts == 3, "Tankbuster 3")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<ManifoldLashingsTower>();

        ComponentCondition<ManifoldLashingsTail>(id + 2, 2.9f, m => m.NumCasts > 0, "Tail")
            .DeactivateOnExit<ManifoldLashingsTail>();
    }
}
