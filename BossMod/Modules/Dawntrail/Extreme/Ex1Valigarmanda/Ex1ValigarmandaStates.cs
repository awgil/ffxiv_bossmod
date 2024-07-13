namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class Ex1ValigarmandaStates : StateMachineBuilder
{
    public Ex1ValigarmandaStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SpikesicleFireSkyruin(id, 9.3f);
        Triscourge(id + 0x10000, 5.7f);
        FireVolcanicDrop(id + 0x20000, 0.9f);
        FireStance(id + 0x30000, 1.5f);
        FireScourgeOfIce(id + 0x40000, 0.7f);
        MountainFire(id + 0x50000, 1.4f);
        FireScourgeOfFireIceVolcanicDropStance(id + 0x60000, 1.5f);
        FireIceScourgeOfThunder(id + 0x70000, 3.0f);
        FireDisasterZone(id + 0x80000, 5.2f);
        RuinForetold(id + 0x90000, 3.3f);
        Tulidisaster(id + 0xA0000, 7.7f);

        Dictionary<AID, (uint seqID, Action<uint> buildState)> fork = new()
        {
            [AID.SkyruinIce] = ((id >> 24) + 1, ForkIceThunder),
            [AID.SkyruinThunder] = ((id >> 24) + 2, ForkThunderIce)
        };
        CastStartFork(id + 0xB0000, fork, 3.5f, "Ice -or- Thunder");
    }

    private void ForkIceThunder(uint id)
    {
        SubphaseIce(id, 0);
        SubphaseThunder(id + 0x100000, 3.1f);
        SubphaseEnrage(id + 0x200000, 0.7f);
    }

    private void ForkThunderIce(uint id)
    {
        SubphaseThunder(id, 0);
        SubphaseIce(id + 0x100000, 0.6f);
        SubphaseEnrage(id + 0x200000, 3.2f);
    }

    private void SubphaseIce(uint id, float delay)
    {
        IceSkyruin(id, delay);
        Triscourge(id + 0x10000, 5.7f);
        IceScourgeOfFireIce(id + 0x20000, 4.9f);
        IceNorthernCrossStance(id + 0x30000, 7.2f);
        FireIceScourgeOfThunder(id + 0x40000, 1.2f);
        IceSpikesicleNorthernCross(id + 0x50000, 5.6f);
        IceStanceNorthernCrossFreezingDust(id + 0x60000, 10.0f);
        IceDisasterZone(id + 0x70000, 3.7f);
        IceTalon(id + 0x80000, 4.3f);
    }

    private void SubphaseThunder(uint id, float delay)
    {
        ThunderSkyruin(id, delay);
        Triscourge(id + 0x10000, 5.7f);
        ThunderScourgeOfFire(id + 0x20000, 4.9f);
        ThunderHailOfFeathers(id + 0x30000, 7.3f);
        ThunderScourgeOfIceThunder(id + 0x40000, 2.6f);
        ThunderStance(id + 0x50000, 7.7f);
        ThunderousBreath(id + 0x60000, 3.0f);
        ThunderScourgeOfIceThunder(id + 0x70000, 3.0f);
        ThunderStance(id + 0x80000, 8.1f);
        ThunderDisasterZone(id + 0x90000, 2.6f);
        ThunderRuinfall(id + 0xA0000, 6.3f);
    }

    private void SubphaseEnrage(uint id, float delay)
    {
        WrathUnfurled(id, delay);
        MountainFire(id + 0x10000, 7.9f);
        Enrage(id + 0x20000, 6.3f);
    }

    private State FireSkyruin(uint id, float delay)
    {
        Cast(id, AID.SkyruinFire, delay, 6);
        return ComponentCondition<SkyruinFire>(id + 0x10, 5.5f, comp => comp.NumCasts > 0, "Raidwide + Fire start")
            .ActivateOnEnter<SkyruinFire>()
            .DeactivateOnExit<SkyruinFire>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void IceSkyruin(uint id, float delay)
    {
        Cast(id, AID.SkyruinIce, delay, 6);
        ComponentCondition<SkyruinIce>(id + 0x10, 5.5f, comp => comp.NumCasts > 0, "Raidwide + Ice start")
            .ActivateOnEnter<SkyruinIce>()
            .DeactivateOnExit<SkyruinIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ThunderSkyruin(uint id, float delay)
    {
        Cast(id, AID.SkyruinThunder, delay, 6);
        ComponentCondition<SkyruinThunder>(id + 0x10, 5.5f, comp => comp.NumCasts > 0, "Raidwide + Thunder start")
            .ActivateOnEnter<SkyruinThunder>()
            .DeactivateOnExit<SkyruinThunder>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SpikesicleFireSkyruin(uint id, float delay)
    {
        CastStart(id, AID.Spikesicle, delay)
            .ActivateOnEnter<Spikesicle>(); // first envcontrol happens right before cast start
        CastEnd(id + 1, 10);
        ComponentCondition<Spikesicle>(id + 0x10, 1.3f, comp => comp.NumCasts > 0, "Curves start"); // every 1.2s after
        ComponentCondition<SphereShatter>(id + 0x20, 6.7f, comp => comp.NumCasts > 0, "Circles start") // after 6th curve; every 1.2s after
            .ActivateOnEnter<SphereShatter>();
        ComponentCondition<Spikesicle>(id + 0x30, 4.1f, comp => comp.NumCasts >= 10)
            .DeactivateOnExit<Spikesicle>();

        FireSkyruin(id + 0x1000, 4.6f)
            .DeactivateOnExit<SphereShatter>(); // last sphere explodes ~2.1s into cast
    }

    private void Triscourge(uint id, float delay)
    {
        Cast(id, AID.Triscourge, delay, 3, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    // voidzones remain active
    private void FireScourgeOfFire(uint id, float delay)
    {
        ComponentCondition<FireScourgeOfFire>(id, delay, comp => comp.Stacks.Count > 0)
            .ActivateOnEnter<FireScourgeOfFire>();
        ComponentCondition<FireScourgeOfFire>(id + 0x10, 7.1f, comp => comp.RemainingCasts() < 3, "Party stack 1", checkDelay: 7)
            .ActivateOnEnter<FireScourgeOfFireVoidzone>();
        ComponentCondition<FireScourgeOfFire>(id + 0x20, 3.1f, comp => comp.RemainingCasts() < 2, "Party stack 2", checkDelay: 3);
        ComponentCondition<FireScourgeOfFire>(id + 0x30, 3.1f, comp => comp.RemainingCasts() < 1, "Party stack 3", checkDelay: 3)
            .DeactivateOnExit<FireScourgeOfFire>();
    }

    private State FireScourgeOfIce(uint id, float delay)
    {
        ComponentCondition<FireScourgeOfIce>(id, delay, comp => comp.NumImminent > 0)
            .ActivateOnEnter<FireScourgeOfIce>();
        ComponentCondition<FireScourgeOfIce>(id + 1, 7.0f, comp => comp.NumActiveFreezes > 0, "Start moving");
        return ComponentCondition<FireScourgeOfIce>(id + 2, 2.0f, comp => comp.NumActiveFreezes == 0, "Chill resolve")
            .DeactivateOnExit<FireScourgeOfIce>();
    }

    private void IceScourgeOfFireIce(uint id, float delay)
    {
        ComponentCondition<IceScourgeOfFireIce>(id, delay, comp => comp.Active)
            .ActivateOnEnter<IceScourgeOfFireIce>();
        ComponentCondition<IceScourgeOfFireIce>(id + 1, 7.1f, comp => comp.NumFinishedSpreads > 0, "Party stack + defamations")
            .DeactivateOnExit<IceScourgeOfFireIce>();
    }

    private void FireIceScourgeOfThunder(uint id, float delay)
    {
        ComponentCondition<FireIceScourgeOfThunder>(id, delay, comp => comp.Spreads.Count > 0)
            .ActivateOnEnter<FireIceScourgeOfThunder>();
        ComponentCondition<FireIceScourgeOfThunder>(id + 1, 7.1f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<FireIceScourgeOfThunder>();
    }

    private void ThunderScourgeOfFire(uint id, float delay)
    {
        ComponentCondition<ThunderScourgeOfFire>(id, delay, comp => comp.Stacks.Count > 0)
            .ActivateOnEnter<ThunderScourgeOfFire>();
        ComponentCondition<ThunderScourgeOfFire>(id + 0x10, 7.1f, comp => comp.NumFinishedStacks > 0, "Party stack")
            .DeactivateOnExit<ThunderScourgeOfFire>();
    }

    private void ThunderScourgeOfIceThunder(uint id, float delay)
    {
        ComponentCondition<ThunderScourgeOfIceThunder>(id, delay, comp => comp.Spreads.Count > 0)
            .ActivateOnEnter<ThunderPlatform>()
            .ActivateOnEnter<ThunderScourgeOfIceThunder>();
        ComponentCondition<ThunderScourgeOfIceThunder>(id + 0x10, 7.1f, comp => comp.NumCasts > 0, "Spread")
            .DeactivateOnExit<ThunderScourgeOfIceThunder>()
            .DeactivateOnExit<ThunderPlatform>();
    }

    private void FireVolcanicDropMid(uint id, float delay)
    {
        ComponentCondition<VolcanicDropPuddle>(id, delay, comp => comp.Casters.Count > 0) // 1s between sets
            .ActivateOnEnter<VolcanicDropPuddle>();
        ComponentCondition<VolcanicDrop>(id + 1, 0.6f, comp => comp.NumCasts > 0, "Volcano");
        ComponentCondition<VolcanicDrop>(id + 0x10, 2.3f, comp => comp.NumCasts >= 5)
            .DeactivateOnExit<VolcanicDrop>();
    }

    private void FireVolcanicDrop(uint id, float delay)
    {
        ComponentCondition<VolcanicDrop>(id, 0.9f, comp => comp.AOE != null)
            .ActivateOnEnter<VolcanicDrop>();
        FireVolcanicDropMid(id + 0x100, 7.2f);
        ComponentCondition<VolcanicDropPuddle>(id + 0x200, 1.6f, comp => comp.Casters.Count == 0, "Puddles resolve")
            .DeactivateOnExit<VolcanicDropPuddle>();
    }

    private State FireStance(uint id, float delay)
    {
        CastMulti(id, [AID.SusurrantBreathFire, AID.SlitheringStrikeFire, AID.StranglingCoilFire], delay, 6.5f)
            .ActivateOnEnter<Stance>()
            .ActivateOnEnter<CharringCataclysm>();
        ComponentCondition<Stance>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Cone/out/in")
            .DeactivateOnExit<Stance>();
        return ComponentCondition<CharringCataclysm>(id + 3, 0.7f, comp => comp.Stacks.Count == 0, "Pairs")
            .DeactivateOnExit<CharringCataclysm>();
    }

    private State IceStance(uint id, float delay, string castEndName = "")
    {
        CastMulti(id, [AID.SusurrantBreathIce, AID.SlitheringStrikeIce, AID.StranglingCoilIce], delay, 6.5f, castEndName)
            .ActivateOnEnter<Stance>();
        ComponentCondition<Stance>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Cone/out/in")
            .DeactivateOnExit<Stance>();
        return ComponentCondition<ChillingCataclysm>(id + 3, 6.0f, comp => comp.NumCasts > 0, "Stars")
            .ActivateOnEnter<ChillingCataclysm>()
            .DeactivateOnExit<ChillingCataclysm>();
    }

    private void ThunderStance(uint id, float delay)
    {
        CastMulti(id, [AID.SusurrantBreathThunder, AID.SlitheringStrikeThunder, AID.StranglingCoilThunder], delay, 6.5f)
            .ActivateOnEnter<Stance>();
        ComponentCondition<Stance>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Cone/out/in")
            .DeactivateOnExit<Stance>();
        ComponentCondition<CracklingCataclysm>(id + 3, 0.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<CracklingCataclysm>();
        ComponentCondition<CracklingCataclysm>(id + 4, 3, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<CracklingCataclysm>();
    }

    private void MountainFire(uint id, float delay)
    {
        Cast(id, AID.MountainFire, delay, 4);
        ComponentCondition<MountainFire>(id + 0x10, 5.6f, comp => comp.NumCasts >= 1, "Towers/cones 1")
            .ActivateOnEnter<MountainFire>()
            .ActivateOnEnter<MountainFireCone>();
        ComponentCondition<MountainFireCone>(id + 0x11, 0.4f, comp => comp.NumCasts >= 1);
        ComponentCondition<MountainFire>(id + 0x20, 4.6f, comp => comp.NumCasts >= 2);
        ComponentCondition<MountainFireCone>(id + 0x21, 0.4f, comp => comp.NumCasts >= 2);
        ComponentCondition<MountainFire>(id + 0x30, 4.6f, comp => comp.NumCasts >= 3);
        ComponentCondition<MountainFireCone>(id + 0x31, 0.4f, comp => comp.NumCasts >= 3);
        ComponentCondition<MountainFire>(id + 0x40, 4.6f, comp => comp.NumCasts >= 4);
        ComponentCondition<MountainFireCone>(id + 0x41, 0.4f, comp => comp.NumCasts >= 4);
        ComponentCondition<MountainFire>(id + 0x50, 4.6f, comp => comp.NumCasts >= 5);
        ComponentCondition<MountainFireCone>(id + 0x51, 0.4f, comp => comp.NumCasts >= 5);
        ComponentCondition<MountainFire>(id + 0x60, 4.6f, comp => comp.NumCasts >= 6);
        ComponentCondition<MountainFireCone>(id + 0x61, 0.4f, comp => comp.NumCasts >= 6, "Towers/cones 6")
            .DeactivateOnExit<MountainFireCone>()
            .DeactivateOnExit<MountainFire>();
    }

    private void FireScourgeOfFireIceVolcanicDropStance(uint id, float delay)
    {
        FireScourgeOfFire(id, delay);
        FireScourgeOfIce(id + 0x1000, 1.8f)
            .ActivateOnEnter<VolcanicDrop>() // envcontrol happens ~0.3s before mechanic resolve
            .DeactivateOnExit<FireScourgeOfFireVoidzone>();
        FireVolcanicDropMid(id + 0x1100, 7.0f);
        FireStance(id + 0x2000, 1.1f)
            .DeactivateOnExit<VolcanicDropPuddle>(); // puddles resolve 0.5s into cast
    }

    private void FireDisasterZone(uint id, float delay)
    {
        Cast(id, AID.DisasterZoneFire, delay, 3);
        ComponentCondition<DisasterZoneFire>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<DisasterZoneFire>()
            .DeactivateOnExit<DisasterZoneFire>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void IceDisasterZone(uint id, float delay)
    {
        Cast(id, AID.DisasterZoneIce, delay, 3);
        ComponentCondition<DisasterZoneIce>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<DisasterZoneIce>()
            .DeactivateOnExit<DisasterZoneIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ThunderDisasterZone(uint id, float delay)
    {
        Cast(id, AID.DisasterZoneThunder, delay, 3);
        ComponentCondition<DisasterZoneThunder>(id + 2, 0.8f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<DisasterZoneThunder>()
            .DeactivateOnExit<DisasterZoneThunder>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void IceNorthernCrossStance(uint id, float delay)
    {
        ComponentCondition<NorthernCross>(id, delay, comp => comp.AOE != null)
            .ActivateOnEnter<NorthernCross>();

        IceStance(id + 0x1000, 3.2f, "Avalanche")
            .DeactivateOnExit<NorthernCross>(); // avalanche happens right before cast end
    }

    private void IceSpikesicleNorthernCross(uint id, float delay)
    {
        CastStart(id, AID.Spikesicle, delay)
            .ActivateOnEnter<Spikesicle>(); // first envcontrol happens right before cast start
        CastEnd(id + 1, 10);
        ComponentCondition<Spikesicle>(id + 0x10, 1.3f, comp => comp.NumCasts > 0, "Curves start"); // every 1.2s after
        ComponentCondition<NorthernCross>(id + 0x20, 2.7f, comp => comp.AOE != null)
            .ActivateOnEnter<NorthernCross>();
        ComponentCondition<SphereShatter>(id + 0x30, 4.1f, comp => comp.NumCasts > 0, "Circles start") // after 6th curve; every 1.2s after
            .ActivateOnEnter<SphereShatter>();
        ComponentCondition<Spikesicle>(id + 0x40, 4.1f, comp => comp.NumCasts >= 10)
            .DeactivateOnExit<Spikesicle>();
        ComponentCondition<NorthernCross>(id + 0x50, 0.9f, comp => comp.NumCasts > 0, "Avalanche")
            .DeactivateOnExit<NorthernCross>();
        ComponentCondition<SphereShatter>(id + 0x60, 5.8f, comp => comp.NumCasts >= 10, "Circles resolve")
            .DeactivateOnExit<SphereShatter>();
    }

    private void IceStanceNorthernCrossFreezingDust(uint id, float delay)
    {
        IceStance(id, delay)
            .ActivateOnEnter<NorthernCross>(); // env control happens ~1.7s after stance resolve
        CastStart(id + 0x10, AID.FreezingDust, 3.8f);
        ComponentCondition<NorthernCross>(id + 0x11, 0.9f, comp => comp.NumCasts > 0, "Avalanche")
            .DeactivateOnExit<NorthernCross>();
        CastEnd(id + 0x12, 4.0f, "Start moving")
            .ActivateOnEnter<FreezingDust>();
        ComponentCondition<FreezingDust>(id + 0x13, 1.0f, comp => comp.NumActiveFreezes > 0);
        ComponentCondition<FreezingDust>(id + 0x14, 2.0f, comp => comp.NumActiveFreezes == 0, "Chill resolve")
            .ActivateOnEnter<FireIceScourgeOfThunder>() // icons can appear slightly before chill resolve
            .DeactivateOnExit<FreezingDust>();

        ComponentCondition<FireIceScourgeOfThunder>(id + 0x1000, 0.3f, comp => comp.Spreads.Count > 0);
        ComponentCondition<FireIceScourgeOfThunder>(id + 0x1001, 7.1f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<FireIceScourgeOfThunder>();
    }

    private void IceTalon(uint id, float delay)
    {
        CastStart(id, AID.IceTalon, delay)
            .ActivateOnEnter<IceTalon>(); // icons appear ~0.1s before cast start
        CastEnd(id + 1, 4);
        ComponentCondition<IceTalon>(id + 2, 1, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<IceTalon>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ThunderHailOfFeathers(uint id, float delay)
    {
        Cast(id, AID.HailOfFeathers, delay, 4)
            .ActivateOnEnter<HailOfFeathers>();
        ComponentCondition<HailOfFeathers>(id + 0x10, 2, comp => comp.NumCasts >= 1, "Feather 1");
        ComponentCondition<HailOfFeathers>(id + 0x20, 3, comp => comp.NumCasts >= 2, "Feather 2")
            .ActivateOnEnter<FeatherOfRuin>();
        ComponentCondition<HailOfFeathers>(id + 0x30, 3, comp => comp.NumCasts >= 3, "Feather 3");
        ComponentCondition<HailOfFeathers>(id + 0x40, 3, comp => comp.NumCasts >= 4, "Feather 4");
        ComponentCondition<HailOfFeathers>(id + 0x50, 3, comp => comp.NumCasts >= 5, "Feather 5");
        ComponentCondition<HailOfFeathers>(id + 0x60, 3, comp => comp.NumCasts >= 6, "Feather 6")
            .DeactivateOnExit<HailOfFeathers>();

        Cast(id + 0x100, AID.BlightedBolt, 4.3f, 5)
            .ActivateOnEnter<ThunderPlatform>()
            .ActivateOnEnter<BlightedBolt>();
        ComponentCondition<BlightedBolt>(id + 0x110, 0.8f, comp => comp.NumCasts > 0, "Feathers explode")
            .DeactivateOnExit<BlightedBolt>()
            .DeactivateOnExit<FeatherOfRuin>()
            .DeactivateOnExit<ThunderPlatform>();
    }

    private void ThunderousBreath(uint id, float delay)
    {
        ComponentCondition<ArcaneLighning>(id, delay, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<ThunderPlatform>()
            .ActivateOnEnter<ArcaneLighning>();
        Cast(id + 0x10, AID.ThunderousBreath, 0.7f, 7)
            .ActivateOnEnter<ThunderousBreath>();
        ComponentCondition<ThunderousBreath>(id + 0x20, 0.9f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<ThunderousBreath>()
            .DeactivateOnExit<ThunderPlatform>();
        ComponentCondition<ArcaneLighning>(id + 0x21, 0.1f, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<ArcaneLighning>();
    }

    private void ThunderRuinfall(uint id, float delay)
    {
        Cast(id, AID.Ruinfall, delay, 4)
            .ActivateOnEnter<RuinfallTower>()
            .ActivateOnEnter<RuinfallKnockback>()
            .ActivateOnEnter<RuinfallAOE>();
        ComponentCondition<RuinfallTower>(id + 0x10, 1.6f, comp => comp.NumCasts > 0, "Tankbuster tower")
            .DeactivateOnExit<RuinfallTower>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<RuinfallKnockback>(id + 0x20, 2.4f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<RuinfallKnockback>();
        ComponentCondition<RuinfallAOE>(id + 0x30, 1.5f, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<RuinfallAOE>();
    }

    private void RuinForetold(uint id, float delay)
    {
        Cast(id, AID.RuinForetold, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x10, false, 0.9f, "Boss disappears");
        ComponentCondition<Beacons>(id + 0x11, 1.0f, comp => comp.ActiveActors.Any(), "Adds appear")
            .ActivateOnEnter<Beacons>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        Targetable(id + 0x100, true, 50, "Adds enrage") // boss becomes targetable immediately when last adds becomes untargetable
            .ActivateOnEnter<CalamitousCry>() // 7.2s, then every 6s
            .ActivateOnEnter<CalamitousEcho>() // 7.2s, then every 6s
            .DeactivateOnExit<CalamitousCry>()
            .DeactivateOnExit<CalamitousEcho>()
            .DeactivateOnExit<Beacons>();
    }

    private void Tulidisaster(uint id, float delay)
    {
        Cast(id, AID.Tulidisaster, delay, 7);
        ComponentCondition<Tulidisaster1>(id + 0x10, 3.2f, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<Tulidisaster1>()
            .DeactivateOnExit<Tulidisaster1>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Tulidisaster2>(id + 0x20, 8.5f, comp => comp.NumCasts > 0, "Raidwide 2")
            .ActivateOnEnter<Tulidisaster2>()
            .DeactivateOnExit<Tulidisaster2>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Tulidisaster3>(id + 0x30, 8.0f, comp => comp.NumCasts > 0, "Raidwide 3 (dot)")
            .ActivateOnEnter<Tulidisaster3>()
            .DeactivateOnExit<Tulidisaster3>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WrathUnfurled(uint id, float delay)
    {
        Cast(id, AID.WrathUnfurled, delay, 4);
        ComponentCondition<WrathUnfurled>(id + 2, 3.3f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<WrathUnfurled>()
            .DeactivateOnExit<WrathUnfurled>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.TulidisasterEnrage, delay, 7);
        ComponentCondition<TulidisasterEnrage1>(id + 0x10, 3.2f, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<TulidisasterEnrage1>()
            .DeactivateOnExit<TulidisasterEnrage1>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<TulidisasterEnrage2>(id + 0x20, 8.5f, comp => comp.NumCasts > 0, "Raidwide 2")
            .ActivateOnEnter<TulidisasterEnrage2>()
            .DeactivateOnExit<TulidisasterEnrage2>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<TulidisasterEnrage3>(id + 0x30, 8.1f, comp => comp.NumCasts > 0, "Enrage")
            .ActivateOnEnter<TulidisasterEnrage3>()
            .DeactivateOnExit<TulidisasterEnrage3>();
    }
}
