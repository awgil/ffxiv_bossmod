namespace BossMod.Endwalker.Unreal.Un5Thordan;

class Un5ThordanStates : StateMachineBuilder
{
    public Un5ThordanStates(Un5Thordan module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        // first 'phase'
        AscalonsMight(id, 6.1f);
        Meteorain(id + 0x10000, 7.1f);
        AscalonsMercy(id + 0x20000, 2.0f);
        AscalonsMight(id + 0x30000, 2.3f);
        DragonsEyeGaze(id + 0x40000, 2.1f);
        AscalonsMight(id + 0x50000, 5.2f);
        LightningStorm(id + 0x60000, 5.1f);
        DragonsRage(id + 0x70000, 1.8f);
        AncientQuaga(id + 0x80000, 2.2f);
        AscalonsMight(id + 0x90000, 6.1f);
        HeavenlyHeel(id + 0xA0000, 2.1f);
        AscalonsMight(id + 0xB0000, 2.1f);
        Targetable(id + 0xC0000, false, 2, "Boss disappears");

        // intermission
        HeavensflameChainsConviction(id + 0x100000, 16.2f); // note: quite large variance
        SacredCrossSpiralThrust(id + 0x110000, 3);
        AdelphelJanlenoux(id + 0x120000, 7);
        SpiralPierceDimensionalCollapseHiemalStormComets(id + 0x130000, 6.1f);
        LightOfAscalonUltimateEnd(id + 0x140000, 14.1f);
        Targetable(id + 0x150000, true, 4.5f, "Boss reappears");

        // second 'phase'
        KnightsOfTheRound1(id + 0x200000, 3.2f);
        AncientQuaga(id + 0x210000, 9.2f);
        KnightsOfTheRound2(id + 0x230000, 2.2f);
        AscalonsMight(id + 0x240000, 5.2f); // note: sometimes it's 3.2s instead
        KnightsOfTheRound3(id + 0x250000, 6.2f);
        HeavenlyHeel(id + 0x260000, 7.2f);
        AscalonsMight(id + 0x270000, 2.1f);
        KnightsOfTheRound4(id + 0x280000, 7.2f);
        KnightsOfTheRound5(id + 0x290000, 2.2f);
        AncientQuaga(id + 0x2A0000, 4.3f);
        HeavenlyHeel(id + 0x2B0000, 5.2f);
        AscalonsMightEnrage(id + 0x2C0000, 2.1f);
        Cast(id + 0x2D0000, AID.Enrage, 2.1f, 10, "Enrage");
    }

    private void AscalonsMight(uint id, float delay)
    {
        ComponentCondition<AscalonsMight>(id, delay, comp => comp.NumCasts > 0, "Cleave")
            .ActivateOnEnter<AscalonsMight>()
            .DeactivateOnExit<AscalonsMight>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void AscalonsMightEnrage(uint id, float delay)
    {
        ComponentCondition<AscalonsMight>(id, delay, comp => comp.NumCasts >= 1, "Cleave 1")
            .ActivateOnEnter<AscalonsMight>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 1, 5.2f, comp => comp.NumCasts >= 2, "Cleave 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 2, 3.1f, comp => comp.NumCasts >= 3, "Cleave 3")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 3, 3.1f, comp => comp.NumCasts >= 4, "Cleave 4")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 4, 3.1f, comp => comp.NumCasts >= 5, "Cleave 5")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 5, 3.1f, comp => comp.NumCasts >= 6, "Cleave 6")
            .DeactivateOnExit<AscalonsMight>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Meteorain(uint id, float delay)
    {
        CastStart(id, AID.Meteorain, delay, "Puddles bait");
        CastEnd(id + 1, 2.7f)
            .ActivateOnEnter<Meteorain>();
        ComponentCondition<Meteorain>(id + 2, 0.3f, comp => comp.NumCasts > 0, "Puddles resolve")
            .DeactivateOnExit<Meteorain>();
    }

    private State AscalonsMercy(uint id, float delay)
    {
        return Cast(id, AID.AscalonsMercy, delay, 3, "Cones fan")
            .ActivateOnEnter<AscalonsMercy>()
            .ActivateOnEnter<AscalonsMercyHelper>()
            .DeactivateOnExit<AscalonsMercy>()
            .DeactivateOnExit<AscalonsMercyHelper>();
    }

    private void DragonsEyeGaze(uint id, float delay)
    {
        Cast(id, AID.DragonsEye, delay, 3);
        Cast(id + 0x10, AID.DragonsGaze, 7.2f, 3, "Gaze")
            .ActivateOnEnter<DragonsGaze>()
            .DeactivateOnExit<DragonsGaze>();
    }

    private void LightningStorm(uint id, float delay)
    {
        CastStart(id, AID.LightningStorm, delay)
            .ActivateOnEnter<LightningStorm>();
        CastEnd(id + 1, 3.5f);
        ComponentCondition<LightningStorm>(id + 2, 0.5f, comp => !comp.Active, "Spread")
            .DeactivateOnExit<LightningStorm>();
    }

    private void DragonsRage(uint id, float delay)
    {
        Cast(id, AID.DragonsRage, delay, 5, "Stack")
            .ActivateOnEnter<DragonsRage>()
            .DeactivateOnExit<DragonsRage>();
    }

    private State AncientQuaga(uint id, float delay)
    {
        return Cast(id, AID.AncientQuaga, delay, 3, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HeavenlyHeel(uint id, float delay)
    {
        Cast(id, AID.HeavenlyHeel, delay, 4, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HeavensflameChainsConviction(uint id, float delay)
    {
        ComponentCondition<Heavensflame>(id, delay, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<Heavensflame>();
        ComponentCondition<Conviction>(id + 0x10, 2, comp => comp.Towers.Count > 0)
            .ActivateOnEnter<BurningChains>()
            .ActivateOnEnter<Conviction>();
        // +0.1s: holy chain cast?..
        ComponentCondition<Heavensflame>(id + 0x20, 4, comp => comp.Casters.Count == 0, "Puddles resolve")
            .DeactivateOnExit<Heavensflame>()
            .DeactivateOnExit<BurningChains>(); // note: this resolves much earlier...
        ComponentCondition<Conviction>(id + 0x30, 3, comp => comp.Towers.Count == 0, "Towers resolve")
            .DeactivateOnExit<Conviction>();
    }

    private void SacredCrossSpiralThrust(uint id, float delay)
    {
        ComponentCondition<SerZephirin>(id, delay, comp => comp.ActiveActors.Any(), "Add appears")
            .ActivateOnEnter<SerZephirin>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SerZephirin>(id + 0x10, 20.1f, comp => !comp.ActiveActors.Any(), "DPS check")
            .ActivateOnEnter<SpiralThrust1>()
            .DeactivateOnExit<SerZephirin>()
            .SetHint(StateMachine.StateHint.DowntimeStart | StateMachine.StateHint.Raidwide);
        ComponentCondition<SpiralThrust>(id + 0x20, 6, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<SpiralThrust>();
    }

    private void AdelphelJanlenoux(uint id, float delay)
    {
        ComponentCondition<SwordShieldOfTheHeavens>(id, delay, comp => comp.Active, "Adds appear")
            .ActivateOnEnter<SwordShieldOfTheHeavens>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        // note: kill times vary wildly, so mechanics could be skipped - to avoid creating phases, we just activate all components at once and don't create intermediate states
        // +7.2s: divine right 1 start
        // +10.2s: divine right 1 finish, buffs appear - adds should be split
        // +15.3s: holy bladedance 1 start
        // +19.3s: holy bladedance 1 finish
        // +29.2s: leap icon 1
        // +32.2s: leap icon 2
        // +34.3s: divine right 2 start
        // +35.2s: leap icon 3, leap cast 1
        // +37.3s: divine right 2 finish
        // +38.2s: leap cast 2
        // +42.3s: leap cast 3, cleaves
        // +49.5s: holiest of holy 1 start
        // +52.5s: holiest of holy 1 finish
        // ...: then holy bladedance 2 > divine right 3 > holiest of holy 2 > cleaves > divine right 4 > holy bladedance 3
        ComponentCondition<SwordShieldOfTheHeavens>(id + 0x1000, 100, comp => !comp.Active, "Adds enrage", 10000)
            .ActivateOnEnter<HoliestOfHoly>()
            .ActivateOnEnter<SkywardLeap>()
            .DeactivateOnExit<HoliestOfHoly>()
            .DeactivateOnExit<SkywardLeap>()
            .DeactivateOnExit<SwordShieldOfTheHeavens>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void SpiralPierceDimensionalCollapseHiemalStormComets(uint id, float delay)
    {
        ComponentCondition<SpiralPierce>(id, delay, comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<SpiralPierce>();
        ComponentCondition<DimensionalCollapse>(id + 0x10, 2.1f, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<DimensionalCollapse>();
        ComponentCondition<HiemalStormSpread>(id + 0x20, 0.9f, comp => comp.Active)
            .ActivateOnEnter<HiemalStormSpread>();
        ComponentCondition<SpiralPierce>(id + 0x30, 3.2f, comp => comp.NumCasts > 0, "Spreads + Charges")
            .ActivateOnEnter<HiemalStormVoidzone>()
            .DeactivateOnExit<HiemalStormSpread>() // spreads resolve slightly before charges, with large variance
            .DeactivateOnExit<SpiralPierce>();
        ComponentCondition<DimensionalCollapse>(id + 0x40, 1.8f, comp => comp.NumCasts > 0, "Puddles resolve")
            .DeactivateOnExit<DimensionalCollapse>();
        ComponentCondition<FaithUnmoving>(id + 0x50, 3.1f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<FaithUnmoving>()
            .DeactivateOnExit<FaithUnmoving>();

        ComponentCondition<MeteorCircle>(id + 0x1000, 3.4f, comp => comp.ActiveActors.Any(), "Comets appear") // note: quite large variance
            .ActivateOnEnter<CometCircle>()
            .ActivateOnEnter<MeteorCircle>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        // +3.4s: prey icons, first aoe after 4.1s, then every 1.1s
        // +29.9s: all live comets cast raidwides
        // TODO: proper small/large enrage deadlines
        ComponentCondition<MeteorCircle>(id + 0x1010, 50, comp => !comp.ActiveActors.Any(), "Large comet enrage", 10000) // TODO: proper time
            .ActivateOnEnter<HeavyImpact>()
            .DeactivateOnExit<HeavyImpact>()
            .DeactivateOnExit<CometCircle>()
            .DeactivateOnExit<MeteorCircle>()
            .DeactivateOnExit<HiemalStormVoidzone>() // voidzones disappear slightly after comets appear
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void LightOfAscalonUltimateEnd(uint id, float delay)
    {
        ComponentCondition<LightOfAscalon>(id, delay, comp => comp.NumCasts > 0, "Short knockback 1")
            .ActivateOnEnter<LightOfAscalon>();
        ComponentCondition<LightOfAscalon>(id + 0x10, 8.3f, comp => comp.NumCasts >= 7, "Short knockback 7") // note: i've seen 6 casts in one of the logs...
            .DeactivateOnExit<LightOfAscalon>();
        ComponentCondition<UltimateEnd>(id + 0x20, 9.6f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<UltimateEnd>()
            .DeactivateOnExit<UltimateEnd>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void KnightsOfTheRound1(uint id, float delay)
    {
        Cast(id, AID.DragonsEye, delay, 3, "Trio 1 start (eye N)")
            .ActivateOnEnter<DragonsGaze>(); // objanim happens ~0.8s after cast end
        Cast(id + 0x10, AID.KnightsOfTheRound, 7.2f, 3);
        AscalonsMight(id + 0x20, 5.2f);
        ComponentCondition<HolyShieldBash>(id + 0x30, 6.0f, comp => comp.NumCasts > 0, "Stun healer")
            .ActivateOnEnter<HolyShieldBash>();
        ComponentCondition<HolyShieldBash>(id + 0x40, 3.1f, comp => comp.Source != null);
        CastStart(id + 0x41, AID.HeavenlyHeel, 5);
        ComponentCondition<HolyShieldBash>(id + 0x42, 1, comp => comp.NumCasts > 1, "Wild charge")
            .DeactivateOnExit<HolyShieldBash>();
        CastEnd(id + 0x43, 3, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        Cast(id + 0x50, AID.DragonsGaze, 2.1f, 3, "Gaze")
            .DeactivateOnExit<DragonsGaze>();
    }

    private void KnightsOfTheRound2(uint id, float delay)
    {
        Cast(id, AID.DragonsEye, delay, 3, "Trio 2 start")
            .ActivateOnEnter<DragonsGaze>(); // objanim happens ~0.8s after cast end
        Cast(id + 0x10, AID.KnightsOfTheRound, 7.2f, 3);
        ComponentCondition<Conviction>(id + 0x20, 7.2f, comp => comp.Towers.Count > 0)
            .ActivateOnEnter<Conviction>();
        Cast(id + 0x30, AID.DragonsGaze, 1, 3, "Gaze")
            .ActivateOnEnter<HeavyImpact>() // starts ~1s into cast
            .DeactivateOnExit<DragonsGaze>();
        ComponentCondition<HeavyImpact>(id + 0x40, 1.1f, comp => comp.NumCasts > 0, "Ring 1");
        ComponentCondition<HeavyImpact>(id + 0x50, 2, comp => comp.NumCasts > 1, "Towers + Ring 2")
            .DeactivateOnExit<Conviction>(); // happen at the same time
        ComponentCondition<DimensionalCollapse>(id + 0x60, 1, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<DimensionalCollapse>();
        ComponentCondition<HeavyImpact>(id + 0x70, 1, comp => comp.NumCasts > 2, "Ring 3");
        ComponentCondition<HeavyImpact>(id + 0x80, 2, comp => comp.NumCasts > 3, "Ring 4")
            .DeactivateOnExit<HeavyImpact>();
        ComponentCondition<DimensionalCollapse>(id + 0x90, 3, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<DimensionalCollapse>();

        CastStart(id + 0xA0, AID.DragonsRage, 0.2f)
            .ActivateOnEnter<FaithUnmoving>();
        ComponentCondition<FaithUnmoving>(id + 0xA1, 3, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<DragonsRage>()
            .DeactivateOnExit<FaithUnmoving>();
        CastEnd(id + 0xA2, 2, "Stack")
            .DeactivateOnExit<DragonsRage>();
    }

    private void KnightsOfTheRound3(uint id, float delay)
    {
        Cast(id, AID.DragonsEye, delay, 3, "Trio 3 start (eye ignore)");
        Cast(id + 0x10, AID.KnightsOfTheRound, 7.2f, 3)
            .ActivateOnEnter<SpiralThrust2>();
        LightningStorm(id + 0x20, 3.2f);
        ComponentCondition<SpiralPierce>(id + 0x30, 1.9f, comp => comp.CurrentBaits.Count > 0) // pierce & thrust start at the same time
            .ActivateOnEnter<SpiralPierce>()
            .ActivateOnEnter<SkywardLeap>(); // starts slightly later than pierce
        CastStart(id + 0x31, AID.DragonsRage, 2.9f);
        ComponentCondition<SpiralPierce>(id + 0x32, 3.1f, comp => comp.NumCasts > 0, "Baits")
            .ActivateOnEnter<DragonsRage>()
            .DeactivateOnExit<SpiralThrust2>()
            .DeactivateOnExit<SpiralPierce>()
            .DeactivateOnExit<SkywardLeap>(); // resolves slightly later, but whatever
        CastEnd(id + 0x33, 1.9f, "Stack")
            .DeactivateOnExit<DragonsRage>();
    }

    private void KnightsOfTheRound4(uint id, float delay)
    {
        Cast(id, AID.DragonsEye, delay, 3, "Trio 4 start")
            .ActivateOnEnter<DragonsGaze>(); // objanim happens ~0.8s after cast end
        Cast(id + 0x10, AID.KnightsOfTheRound, 7.2f, 3);
        AscalonsMight(id + 0x20, 6.2f);
        CastStart(id + 0x30, AID.DragonsGaze, 2.1f);
        ComponentCondition<Heavensflame>(id + 0x31, 1, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<Heavensflame>();
        CastEnd(id + 0x32, 2, "Gaze")
            .ActivateOnEnter<BurningChains>() // chains appear together with first puddles
            .DeactivateOnExit<DragonsGaze>();
        // +0.1s: chains resolve?..
        ComponentCondition<HiemalStormSpread>(id + 0x40, 0.9f, comp => comp.Active)
            .ActivateOnEnter<HiemalStormSpread>()
            .DeactivateOnExit<BurningChains>(); // TODO: i think it resolves earlier...
        AscalonsMercy(id + 0x50, 1.4f)
            .ActivateOnEnter<HiemalStormVoidzone>()
            .DeactivateOnExit<Heavensflame>() // last puddle ends ~0.5s into cast
            .DeactivateOnExit<HiemalStormSpread>(); // note: this happens mid cast, with staggers...

        AncientQuaga(id + 0x1000, 5.1f);
        HeavenlyHeel(id + 0x2000, 2.1f);
        AncientQuaga(id + 0x3000, 2.1f)
            .DeactivateOnExit<HiemalStormVoidzone>();
    }

    private void KnightsOfTheRound5(uint id, float delay)
    {
        Cast(id, AID.DragonsEye, delay, 3, "Trio 5 start")
            .ActivateOnEnter<DragonsGaze>(); // objanim happens ~0.8s after cast end
        Cast(id + 0x10, AID.KnightsOfTheRound, 7.2f, 3);
        AscalonsMight(id + 0x20, 5.2f);
        ComponentCondition<HoliestOfHoly>(id + 0x30, 5, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<HoliestOfHoly>()
            .DeactivateOnExit<HoliestOfHoly>()
            .SetHint(StateMachine.StateHint.Raidwide);
        AscalonsMight(id + 0x40, 5.2f);
        ComponentCondition<HeavenswardLeap>(id + 0x50, 6.1f, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<HeavenswardLeap>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HeavenswardLeap>(id + 0x51, 3, comp => comp.NumCasts > 1, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HeavenswardLeap>(id + 0x52, 3, comp => comp.NumCasts > 2, "Raidwide 3")
            .DeactivateOnExit<HeavenswardLeap>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<SerZephirin>(id + 0x1000, 7.7f, comp => comp.ActiveActors.Any(), "Boss invuln")
            .ActivateOnEnter<SerZephirin>();
        // +0.1s: zephirin starts 25s cast
        ComponentCondition<PureOfSoul>(id + 0x1010, 6.1f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<PureOfSoul>()
            .DeactivateOnExit<PureOfSoul>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<AbsoluteConviction>(id + 0x1020, 10.9f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<AbsoluteConviction>()
            .DeactivateOnExit<AbsoluteConviction>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x1030, AID.DragonsGaze, 2.3f, 3, "Gaze")
            .DeactivateOnExit<DragonsGaze>();
        ComponentCondition<SerZephirin>(id + 0x1040, 2.8f, comp => !comp.ActiveActors.Any(), "Add enrage")
            .DeactivateOnExit<SerZephirin>();
    }
}
