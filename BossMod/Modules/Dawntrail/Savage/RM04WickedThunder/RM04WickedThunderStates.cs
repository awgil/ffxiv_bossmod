namespace BossMod.Dawntrail.Savage.RM04WickedThunder;

class RM04WickedThunderStates : StateMachineBuilder
{
    private readonly RM04WickedThunder _module;

    public RM04WickedThunderStates(RM04WickedThunder module) : base(module)
    {
        _module = module;
        SimplePhase(0, SinglePhase, "Single phase")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDeadOrDestroyed ?? true);
    }

    private void SinglePhase(uint id)
    {
        WrathOfZeus(id, 10.2f);
        BewitchingFlight(id + 0x10000, 16.7f);
        ElectrifyingWitchHunt(id + 0x20000, 4.6f);
        WideningNarrowingWitchHunt(id + 0x30000, 2.7f);
        WrathOfZeus(id + 0x40000, 7.7f);
        ElectropeEdge(id + 0x50000, 8.4f);
        WickedJolt(id + 0x60000, 3.2f);
        LightningCage(id + 0x70000, 8.4f);
        WickedBolt(id + 0x80000, 2.0f);
        IonCluster(id + 0x90000, 4.2f);
        WickedJolt(id + 0xA0000, 0.7f);
        ElectropeTransplant(id + 0xB0000, 17.8f);
        PhaseTransition(id + 0xC0000, 10.4f);

        Sabertail(id + 0x100000, 1.0f);
        WickedSpecial(id + 0x110000, 0.6f);
        MustardBomb(id + 0x120000, 7.2f);
        AetherialConversion(id + 0x130000, 0.6f);
        AzureThunder(id + 0x140000, 12.2f);
        TwilightSabbath(id + 0x150000, 3.2f);
        MidnightSabbath(id + 0x160000, 7.3f);
        WickedThunder(id + 0x170000, 2.2f);
        FlameSlash(id + 0x180000, 4.2f);
        MustardBomb(id + 0x190000, 3.1f);
        SunriseSabbath(id + 0x1A0000, 0.7f);
        SwordQuiver(id + 0x1B0000, 6.2f);
        SwordQuiver(id + 0x1C0000, 3.1f);
        SwordQuiver(id + 0x1D0000, 3.1f);
        ActorCast(id + 0x1E0000, _module.BossP2, AID.Enrage, 9, 10, true, "Enrage");
    }

    private void WrathOfZeus(uint id, float delay)
    {
        Cast(id, AID.WrathOfZeus, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WickedJolt(uint id, float delay)
    {
        Cast(id, AID.WickedJolt, delay, 5)
            .ActivateOnEnter<WickedJolt>();
        ComponentCondition<WickedJolt>(id + 2, 0.2f, comp => comp.NumCasts > 0, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<WickedJolt>(id + 3, 3.2f, comp => comp.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<WickedJolt>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void BewitchingFlight(uint id, float delay)
    {
        CastMulti(id, [AID.BewitchingFlightR, AID.BewitchingFlightL], delay, 6)
            .ActivateOnEnter<BewitchingFlight>()
            .ActivateOnEnter<Electray>();
        ComponentCondition<BewitchingFlight>(id + 2, 1, comp => comp.NumCasts > 0, "Criss-cross")
            .DeactivateOnExit<BewitchingFlight>()
            .DeactivateOnExit<Electray>();
    }

    private void ElectrifyingWitchHunt(uint id, float delay)
    {
        ComponentCondition<ElectrifyingWitchHuntBurst>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<ElectrifyingWitchHuntBurst>();
        Cast(id + 0x10, AID.ElectrifyingWitchHunt, 1, 5, "Center/sides")
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<ElectrifyingWitchHuntSpread>()
            .DeactivateOnExit<ElectrifyingWitchHuntBurst>()
            .DeactivateOnExit<Electray>();
        ComponentCondition<ElectrifyingWitchHuntSpread>(id + 0x20, 0.1f, comp => comp.Spreads.Count == 0, "Spread")
            .DeactivateOnExit<ElectrifyingWitchHuntSpread>();

        ComponentCondition<ElectrifyingWitchHuntBurst>(id + 0x100, 2.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<ElectrifyingWitchHuntBurst>();
        Cast(id + 0x110, AID.WitchHunt, 0.9f, 5)
            .ActivateOnEnter<ElectrifyingWitchHuntResolve>();
        ComponentCondition<ElectrifyingWitchHuntBurst>(id + 0x120, 0.1f, comp => comp.NumCasts > 0, "Sides/center")
            .DeactivateOnExit<ElectrifyingWitchHuntBurst>();
        ComponentCondition<ElectrifyingWitchHuntResolve>(id + 0x121, 0.4f, comp => comp.CurMechanic == ElectrifyingWitchHuntResolve.Mechanic.None && comp.ForbidBait.None(), "Spread")
            .DeactivateOnExit<ElectrifyingWitchHuntResolve>();
    }

    private void WideningNarrowingWitchHunt(uint id, float delay)
    {
        CastMulti(id, [AID.WideningWitchHunt, AID.NarrowingWitchHunt], delay, 14)
            .ActivateOnEnter<WideningNarrowingWitchHunt>()
            .ActivateOnEnter<WideningNarrowingWitchHuntBait>();
        ComponentCondition<WideningNarrowingWitchHunt>(id + 0x10, 1.1f, comp => comp.NumCasts >= 1, "In/out + Bait 1"); // note: bait happens ~0.1s after in/out, but it's unreliable when people are dead
        ComponentCondition<WideningNarrowingWitchHunt>(id + 0x20, 3.6f, comp => comp.NumCasts >= 2, "Out/in + Bait 2");
        ComponentCondition<WideningNarrowingWitchHunt>(id + 0x30, 3.6f, comp => comp.NumCasts >= 3, "In/out + Bait 3");
        ComponentCondition<WideningNarrowingWitchHunt>(id + 0x40, 3.6f, comp => comp.NumCasts >= 4, "Out/in + Bait 4")
            .DeactivateOnExit<WideningNarrowingWitchHunt>()
            .DeactivateOnExit<WideningNarrowingWitchHuntBait>();
    }

    private void ElectropeEdge(uint id, float delay)
    {
        Cast(id, AID.ElectropeEdge, delay, 3);
        Cast(id + 0x10, AID.ElectropeEdgeWitchgleam, 4.2f, 3)
            .ActivateOnEnter<ElectropeEdgeWitchgleam>();
        ComponentCondition<ElectropeEdgeWitchgleam>(id + 0x12, 1.2f, comp => comp.NumCasts >= 2, "Lines");
        ComponentCondition<ElectropeEdgeWitchgleam>(id + 0x13, 1.6f, comp => comp.NumCasts >= 4);
        ComponentCondition<ElectropeEdgeWitchgleam>(id + 0x14, 1.6f, comp => comp.NumCasts >= 6)
            .DeactivateOnExit<ElectropeEdgeWitchgleam>();
        Cast(id + 0x20, AID.SymphonyFantastique, 0.4f, 3);
        CastMulti(id + 0x30, [AID.ElectropeEdgeSidewiseSparkR, AID.ElectropeEdgeSidewiseSparkL], 3.2f, 7, "Corner + Pairs/Spread") // everything resolves within 0.1s of each other
            .ActivateOnEnter<ElectropeEdgeSpark1>()
            .ActivateOnEnter<ElectropeEdgeSpark2>()
            .ActivateOnEnter<ElectropeEdgeSidewiseSparkR>()
            .ActivateOnEnter<ElectropeEdgeSidewiseSparkL>()
            .ActivateOnEnter<ElectropeEdgeStar>()
            .DeactivateOnExit<ElectropeEdgeSpark1>()
            .DeactivateOnExit<ElectropeEdgeSpark2>()
            .DeactivateOnExit<ElectropeEdgeSidewiseSparkR>()
            .DeactivateOnExit<ElectropeEdgeSidewiseSparkL>()
            .DeactivateOnExit<ElectropeEdgeStar>();
    }

    private void LightningCage(uint id, float delay)
    {
        Cast(id, AID.ElectropeEdge, delay, 3)
            .ActivateOnEnter<LightningCage>(); // statuses appear ~1.1s after cast ends
        Cast(id + 0x10, AID.LightningCageWitchgleam, 3.2f, 3)
            .ActivateOnEnter<LightningCageWitchgleam>();
        ComponentCondition<LightningCageWitchgleam>(id + 0x12, 1.2f, comp => comp.NumCasts > 0, "Proteans");
        ComponentCondition<LightningCageWitchgleam>(id + 0x13, 1.6f, comp => comp.NumCasts > 4);
        ComponentCondition<LightningCageWitchgleam>(id + 0x14, 1.6f, comp => comp.NumCasts > 8);
        ComponentCondition<LightningCageWitchgleam>(id + 0x15, 1.6f, comp => comp.NumCasts > 12)
            .DeactivateOnExit<LightningCageWitchgleam>();
        Cast(id + 0x20, AID.LightningCage, 0.4f, 3);
        ComponentCondition<LightningCage>(id + 0x30, 1.0f, comp => comp.Active);
        ComponentCondition<LightningCage>(id + 0x31, 6.7f, comp => comp.NumSparks > 0);
        ComponentCondition<LightningCage>(id + 0x32, 0.3f, comp => comp.NumCasts > 0, "Anchor 1");
        CastMulti(id + 0x40, [AID.ElectropeEdgeSidewiseSparkR, AID.ElectropeEdgeSidewiseSparkL], 2.2f, 7, "Side + Pairs/Spread") // everything resolves within 0.1s of each other
            .ActivateOnEnter<ElectropeEdgeSidewiseSparkR>()
            .ActivateOnEnter<ElectropeEdgeSidewiseSparkL>()
            .ActivateOnEnter<ElectropeEdgeStar>()
            .DeactivateOnExit<ElectropeEdgeSidewiseSparkR>()
            .DeactivateOnExit<ElectropeEdgeSidewiseSparkL>()
            .DeactivateOnExit<ElectropeEdgeStar>();
        ComponentCondition<LightningCage>(id + 0x50, 4.2f, comp => comp.Active);
        ComponentCondition<LightningCage>(id + 0x51, 6.4f, comp => comp.NumSparks > 4);
        ComponentCondition<LightningCage>(id + 0x52, 0.6f, comp => comp.NumCasts > 12, "Anchor 2")
            .DeactivateOnExit<LightningCage>();
    }

    private void WickedBolt(uint id, float delay)
    {
        CastStart(id, AID.WickedBolt, delay)
            .ActivateOnEnter<WickedBolt>(); // icon appears ~0.1s before cast start
        CastEnd(id + 1, 4);
        ComponentCondition<WickedBolt>(id + 0x10, 1.1f, comp => comp.NumFinishedStacks >= 1, "Stack 1");
        ComponentCondition<WickedBolt>(id + 0x11, 1, comp => comp.NumFinishedStacks >= 2);
        ComponentCondition<WickedBolt>(id + 0x12, 1, comp => comp.NumFinishedStacks >= 3);
        ComponentCondition<WickedBolt>(id + 0x13, 1, comp => comp.NumFinishedStacks >= 4);
        ComponentCondition<WickedBolt>(id + 0x14, 1, comp => comp.NumFinishedStacks >= 5, "Stack 5")
            .DeactivateOnExit<WickedBolt>();
    }

    private void ElectronStream(uint id, float delay, int count)
    {
        CastMulti(id, [AID.ElectronStream1, AID.ElectronStream2], delay, 6, $"Side {count}")
            .ActivateOnEnter<ElectronStream>()
            .DeactivateOnExit<ElectronStream>();
        ComponentCondition<ElectronStreamCurrent>(id + 2, 5.1f, comp => comp.NumCasts > 0, $"Debuffs {count}", checkDelay: 5) // if proximity debuff holder dies, everything explodes early
            .ActivateOnEnter<ElectronStreamCurrent>()
            .DeactivateOnExit<ElectronStreamCurrent>();
    }

    private void IonCluster(uint id, float delay)
    {
        Cast(id, AID.IonCluster, delay, 3);
        ComponentCondition<StampedingThunder>(id + 0x10, 11.7f, comp => comp.AOE != null)
            .ActivateOnEnter<StampedingThunder>();
        ComponentCondition<StampedingThunder>(id + 0x11, 2.4f, comp => comp.NumCasts >= 1, "Cannon start");
        ComponentCondition<StampedingThunder>(id + 0x12, 1.1f, comp => comp.NumCasts >= 2);
        ComponentCondition<StampedingThunder>(id + 0x13, 1.1f, comp => comp.NumCasts >= 3);
        ComponentCondition<StampedingThunder>(id + 0x14, 1.1f, comp => comp.NumCasts >= 4);
        ComponentCondition<StampedingThunder>(id + 0x15, 1.1f, comp => comp.NumCasts >= 5);
        ComponentCondition<StampedingThunder>(id + 0x20, 2.7f, comp => comp.SmallArena, "Destroy platform");

        ElectronStream(id + 0x100, 4.2f, 1);
        ElectronStream(id + 0x200, 2.1f, 2);
        ElectronStream(id + 0x300, 2.1f, 3);

        ComponentCondition<StampedingThunder>(id + 0x400, 2.5f, comp => !comp.SmallArena, "Restore platform")
            .DeactivateOnExit<StampedingThunder>();
    }

    private void FulminousField(uint id, float delay)
    {
        ComponentCondition<FulminousField>(id, delay, comp => comp.Active)
            .ActivateOnEnter<FulminousField>();
        ComponentCondition<FulminousField>(id + 1, 3, comp => comp.NumCasts > 0, "Cones start");
        ComponentCondition<FulminousField>(id + 2, 3, comp => comp.NumCasts > 8)
            .ActivateOnEnter<ConductionPoint>();
        ComponentCondition<FulminousField>(id + 3, 3, comp => comp.NumCasts > 16);
        ComponentCondition<FulminousField>(id + 4, 3, comp => comp.NumCasts > 24);
        ComponentCondition<FulminousField>(id + 5, 3, comp => comp.NumCasts > 32, "Cones 5 + Spread")
            .ActivateOnEnter<ForkedFissures>()
            .DeactivateOnExit<ConductionPoint>();
        ComponentCondition<FulminousField>(id + 6, 3, comp => comp.NumCasts > 40, "Cones 6 + Charges")
            .DeactivateOnExit<ForkedFissures>();
        ComponentCondition<FulminousField>(id + 7, 3, comp => comp.NumCasts > 48, "Cones end")
            .DeactivateOnExit<FulminousField>();
    }

    private void ElectropeTransplant(uint id, float delay)
    {
        Cast(id, AID.ElectropeTransplant, delay, 4);
        FulminousField(id + 0x100, 4.3f);
        FulminousField(id + 0x200, 5);
    }

    private void PhaseTransition(uint id, float delay)
    {
        ComponentCondition<Soulshock>(id, delay, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<Soulshock>()
            .DeactivateOnExit<Soulshock>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Impact>(id + 1, 3.3f, comp => comp.NumCasts > 0, "Raidwide 2")
            .ActivateOnEnter<Impact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Impact>(id + 2, 2.6f, comp => comp.NumCasts > 1, "Raidwide 3")
            .DeactivateOnExit<Impact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Cannonbolt>(id + 3, 2.6f, comp => comp.NumCasts > 0, "Raidwide 4")
            .ActivateOnEnter<Cannonbolt>()
            .DeactivateOnExit<Cannonbolt>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Targetable(id + 0x10, false, 0.1f, "Boss disappears");
        ActorTargetable(id + 0x20, _module.BossP2, true, 11.9f, "Boss reappears")
            .OnEnter(() => Module.Arena.Center = RM04WickedThunder.P2Center)
            .OnEnter(() => Module.Arena.Bounds = RM04WickedThunder.P2Bounds)
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x30, _module.BossP2, AID.CrossTailSwitch, 7.2f, 5, true);
        ComponentCondition<CrossTailSwitch>(id + 0x40, 1.2f, comp => comp.NumCasts > 0, "Multi-hit raidwide 1")
            .ActivateOnEnter<CrossTailSwitch>()
            .DeactivateOnExit<CrossTailSwitch>() // 8 hits every second, then different hit
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CrossTailSwitchLast>(id + 0x50, 8.2f, comp => comp.NumCasts > 0, "Multi-hit raidwide 9")
            .ActivateOnEnter<CrossTailSwitchLast>()
            .DeactivateOnExit<CrossTailSwitchLast>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void AzureThunder(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.AzureThunder, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WickedThunder(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.WickedThunder, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Sabertail(uint id, float delay)
    {
        ComponentCondition<Sabertail>(id, delay, comp => comp.Active)
            .ActivateOnEnter<Sabertail>()
            .ActivateOnEnter<WickedBlaze>();
        ActorCast(id + 0x10, _module.BossP2, AID.WickedBlaze, 2.8f, 5, true);
        ComponentCondition<Sabertail>(id + 0x20, 0.2f, comp => comp.NumCasts > 0, "Exaflares + Stacks"); // first stacks resolve ~0.1s earlier
        ComponentCondition<Sabertail>(id + 0x30, 3.4f, comp => comp.NumCasts > 60, "Exaflares resolve") // 16+14+10+10+10+6 hits
            .DeactivateOnExit<Sabertail>();
        ComponentCondition<WickedBlaze>(id + 0x40, 1.0f, comp => comp.NumFinishedStacks > 3, "Stacks resolve")
            .DeactivateOnExit<WickedBlaze>();
    }

    private void WickedSpecial(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.WickedSpecialCenter, AID.WickedSpecialSides], delay, 5, true)
            .ActivateOnEnter<WickedSpecialCenter>()
            .ActivateOnEnter<WickedSpecialSides>();
        Condition(id + 2, 1, () => Module.FindComponent<WickedSpecialCenter>()?.NumCasts > 0 || Module.FindComponent<WickedSpecialSides>()?.NumCasts > 0, "Center/Sides")
            .DeactivateOnExit<WickedSpecialCenter>()
            .DeactivateOnExit<WickedSpecialSides>();
    }

    private void MustardBomb(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.MustardBomb, delay, 8, true)
            .ActivateOnEnter<MustardBomb>();
        ComponentCondition<MustardBomb>(id + 2, 0.8f, comp => comp.CurMechanic > Savage.RM04WickedThunder.MustardBomb.Mechanic.Tethers, "Spread");
        ComponentCondition<MustardBomb>(id + 3, 9.7f, comp => comp.CurMechanic > Savage.RM04WickedThunder.MustardBomb.Mechanic.Nisi, "Nisi")
            .DeactivateOnExit<MustardBomb>();
    }

    private void AetherialConversionResolve(uint id, float delay, bool activate)
    {
        ActorCastStartMulti(id, _module.BossP2, [AID.TailThrust1HitL, AID.TailThrust1KnockbackL, AID.TailThrust1HitR, AID.TailThrust1KnockbackR], delay, true)
            .ActivateOnEnter<AetherialConversionSwitchOfTides>(activate)
            .ActivateOnEnter<AetherialConversionTailThrust>(activate);
        ActorCastEnd(id + 1, _module.BossP2, 5, true);
        ComponentCondition<AetherialConversion>(id + 2, 1.1f, comp => comp.NumCasts > 0, "AOE/Knockback L/R");
        ComponentCondition<AetherialConversion>(id + 3, 4.1f, comp => comp.NumCasts > 1, "AOE/Knockback R/L")
            .DeactivateOnExit<AetherialConversionSwitchOfTides>()
            .DeactivateOnExit<AetherialConversionTailThrust>()
            .DeactivateOnExit<AetherialConversion>();
    }

    private void AetherialConversion(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.AetherialConversionHitLR, AID.AetherialConversionKnockbackLR, AID.AetherialConversionHitRL, AID.AetherialConversionKnockbackRL], delay, 7, true)
            .ActivateOnEnter<AetherialConversion>()
            .ActivateOnEnter<AetherialConversionTailThrust>()
            .ActivateOnEnter<AetherialConversionSwitchOfTides>();
        AetherialConversionResolve(id + 0x10, 3.2f, false);
    }

    private void TwilightSabbath(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.TwilightSabbath, delay, 3, true);
        ComponentCondition<TwilightSabbath>(id + 2, 3.2f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<TwilightSabbath>();
        ActorCast(id + 0x10, _module.BossP2, AID.WickedFire, 1.0f, 4, true);
        ComponentCondition<WickedFire>(id + 0x12, 0.1f, comp => comp.Casters.Count > 0, "Puddle bait")
            .ActivateOnEnter<WickedFire>();
        ComponentCondition<TwilightSabbath>(id + 0x20, 3.0f, comp => comp.NumCasts > 0, "Cleaves 1");
        ComponentCondition<WickedFire>(id + 0x30, 1.0f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<WickedFire>();
        ActorCastMulti(id + 0x40, _module.BossP2, [AID.WickedSpecialCenter, AID.WickedSpecialSides], 1.1f, 5, true)
            .ActivateOnEnter<WickedSpecialCenter>()
            .ActivateOnEnter<WickedSpecialSides>();
        ComponentCondition<TwilightSabbath>(id + 0x50, 1, comp => comp.NumCasts > 2, "Cleaves 1 + Center/Sides")
            .DeactivateOnExit<WickedSpecialCenter>() // resolves at the same time
            .DeactivateOnExit<WickedSpecialSides>()
            .DeactivateOnExit<TwilightSabbath>();
    }

    private void MidnightSabbath(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.MidnightSabbath, delay, 3, true);
        ComponentCondition<MidnightSabbath>(id + 2, 3.2f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<MidnightSabbath>();
        ActorCastMulti(id + 0x10, _module.BossP2, [AID.ConcentratedBurst, AID.ScatteredBurst], 1.0f, 7, true)
            .ActivateOnEnter<ConcentratedScatteredBurst>();
        ComponentCondition<MidnightSabbath>(id + 0x20, 0.1f, comp => comp.NumCasts > 0, "Lines/Donuts + Spread/pairs 1"); // spread/stack resolves almost at the same time
        ActorCastStartMulti(id + 0x30, _module.BossP2, [AID.WickedSpecialCenter, AID.WickedSpecialSides], 3.1f, true);
        ComponentCondition<MidnightSabbath>(id + 0x31, 0.9f, comp => comp.NumCasts > 4, "Lines/Donuts + Pairs/spread 2") // spread/stack resolves almost at the same time
            .DeactivateOnExit<ConcentratedScatteredBurst>()
            .DeactivateOnExit<MidnightSabbath>();
        ActorCastEnd(id + 0x32, _module.BossP2, 4.1f, true)
            .ActivateOnEnter<WickedSpecialCenter>()
            .ActivateOnEnter<WickedSpecialSides>();
        Condition(id + 0x33, 1, () => Module.FindComponent<WickedSpecialCenter>()?.NumCasts > 0 || Module.FindComponent<WickedSpecialSides>()?.NumCasts > 0, "Center/Sides")
            .DeactivateOnExit<WickedSpecialCenter>()
            .DeactivateOnExit<WickedSpecialSides>();
    }

    private void FlameSlash(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.AetherialConversionHitLR, AID.AetherialConversionKnockbackLR, AID.AetherialConversionHitRL, AID.AetherialConversionKnockbackRL], delay, 7, true)
            .ActivateOnEnter<AetherialConversion>();
        ActorCast(id + 0x10, _module.BossP2, AID.FlameSlash, 3.1f, 6, true)
            .ActivateOnEnter<FlameSlash>();
        ComponentCondition<FlameSlash>(id + 0x12, 1, comp => comp.NumCasts > 0, "Destroy center");
        ActorCast(id + 0x20, _module.BossP2, AID.RainingSwords, 2.2f, 2, true)
            .ActivateOnEnter<RainingSwords>();
        ComponentCondition<RainingSwords>(id + 0x22, 1, comp => comp.NumCasts > 0, "Towers")
            .DeactivateOnExit<RainingSwords>();
        ActorCast(id + 0x30, _module.BossP2, AID.ChainLightning, 4.2f, 16, true)
            .ActivateOnEnter<ChainLightning>();
        ComponentCondition<ChainLightning>(id + 0x40, 0.8f, comp => comp.NumCasts >= 3, "Lightning start");
        ComponentCondition<ChainLightning>(id + 0x41, 2.7f, comp => comp.NumCasts >= 6);
        ComponentCondition<ChainLightning>(id + 0x42, 2.7f, comp => comp.NumCasts >= 9);
        ComponentCondition<ChainLightning>(id + 0x43, 2.7f, comp => comp.NumCasts >= 12);
        ComponentCondition<ChainLightning>(id + 0x44, 2.7f, comp => comp.NumCasts >= 15);
        ComponentCondition<ChainLightning>(id + 0x45, 2.7f, comp => comp.NumCasts >= 18);
        ComponentCondition<ChainLightning>(id + 0x46, 2.7f, comp => comp.NumCasts >= 21);
        ComponentCondition<ChainLightning>(id + 0x47, 2.7f, comp => comp.NumCasts >= 24)
            .DeactivateOnExit<ChainLightning>();
        ComponentCondition<FlameSlash>(id + 0x50, 1, comp => comp.AOE == null, "Restore center")
            .DeactivateOnExit<FlameSlash>();
        AetherialConversionResolve(id + 0x60, 0.4f, true);
    }

    private void SunriseSabbath(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.AetherialConversionHitLR, AID.AetherialConversionKnockbackLR, AID.AetherialConversionHitRL, AID.AetherialConversionKnockbackRL], delay, 7, true)
            .ActivateOnEnter<AetherialConversion>();
        AzureThunder(id + 0x10, 3.2f);
        ActorCast(id + 0x20, _module.BossP2, AID.SunriseSabbathIonCluster, 3.2f, 3, true)
            .ActivateOnEnter<SunriseSabbath>(); // buffs appear ~0.8s after cast end
        ActorCast(id + 0x30, _module.BossP2, AID.SunriseSabbath, 3.2f, 3, true);
        ComponentCondition<SunriseSabbathSoaringSoulpress>(id + 0x40, 3.2f, comp => comp.Towers.Count > 0)
            .ActivateOnEnter<SunriseSabbathSoaringSoulpress>();
        ComponentCondition<SunriseSabbathElectronStream>(id + 0x41, 3.1f, comp => comp.Cannons.Count > 0)
            .ActivateOnEnter<SunriseSabbathElectronStream>();
        ComponentCondition<SunriseSabbathSoaringSoulpress>(id + 0x50, 7.1f, comp => comp.NumCasts > 0, "Towers 1");
        ComponentCondition<SunriseSabbathElectronStream>(id + 0x51, 0.5f, comp => comp.NumCasts > 0, "Baits 1");
        WickedSpecial(id + 0x60, 1.4f);
        ComponentCondition<SunriseSabbathElectronStream>(id + 0x70, 1.7f, comp => comp.Cannons.Count > 0);
        ComponentCondition<SunriseSabbathSoaringSoulpress>(id + 0x80, 7.1f, comp => comp.NumCasts > 2, "Towers 2")
            .DeactivateOnExit<SunriseSabbathSoaringSoulpress>();
        ComponentCondition<SunriseSabbathElectronStream>(id + 0x81, 0.5f, comp => comp.NumCasts > 4, "Baits 2")
            .DeactivateOnExit<SunriseSabbathElectronStream>()
            .DeactivateOnExit<SunriseSabbath>();
        AetherialConversionResolve(id + 0x90, 0.9f, true);
    }

    private void SwordQuiver(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.SwordQuiverN, AID.SwordQuiverC, AID.SwordQuiverS], delay, 5, true)
            .ActivateOnEnter<SwordQuiverBurst>()
            .ActivateOnEnter<SwordQuiverLaceration>();
        ComponentCondition<SwordQuiverRaidwide>(id + 0x10, 1.4f, comp => comp.NumCasts >= 1)
            .ActivateOnEnter<SwordQuiverRaidwide>();
        ComponentCondition<SwordQuiverRaidwide>(id + 0x11, 1.0f, comp => comp.NumCasts >= 2);
        ComponentCondition<SwordQuiverRaidwide>(id + 0x12, 1.0f, comp => comp.NumCasts >= 3);
        ComponentCondition<SwordQuiverRaidwide>(id + 0x13, 1.2f, comp => comp.NumCasts >= 4)
            .DeactivateOnExit<SwordQuiverRaidwide>();
        ComponentCondition<SwordQuiverBurst>(id + 0x20, 4.3f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<SwordQuiverBurst>();
        ComponentCondition<SwordQuiverLaceration>(id + 0x21, 0.2f, comp => comp.NumCasts > 0, "Swords")
            .DeactivateOnExit<SwordQuiverLaceration>();
    }
}
