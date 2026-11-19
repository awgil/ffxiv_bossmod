namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class Un2ShinryuStates : StateMachineBuilder
{
    public Un2ShinryuStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1")
            .ActivateOnEnter<ArenaChanges>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.PrimaryActor.CastInfo?.IsSpell(AID.DarkMatter) == true;
        SimplePhase(1, Phase2, "P2")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.FindComponent<ArenaChanges>()?.Shattered == true;
        SimplePhase(2, Phase3, "P3")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.PrimaryActor.CastInfo?.IsSpell(AID.TidalWaveEnrage) == true;
        DeathPhase(3, Phase4);
    }

    private void Phase1(uint id)
    {
        ActorCast(id, () => Module.PrimaryActor, AID.EarthenFuryVisual, 10.2f, 10, true, "Raidwide + tiles")
            .ActivateOnEnter<EarthenFury>()
            .ActivateOnEnter<EarthenFuryTile>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<WaterPuddles>()
            .ActivateOnEnter<Paralysis>()
            .ActivateOnEnter<BurningChains>()
            .ActivateOnEnter<Dragonflight>()
            .ActivateOnEnter<TailMarker>()
            .ActivateOnEnter<TailSlap>()
            .ActivateOnEnter<IcicleAdds>()
            .ActivateOnEnter<Spikesicle>()
            .ActivateOnEnter<SpikesicleKnockback>()
            .ActivateOnEnter<Levinbolt>()
            .ActivateOnEnter<Hypernova>()
            .ActivateOnEnter<IceStorm>()
            .ActivateOnEnter<Dragonfist>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<AkhRhai>()
            .ActivateOnEnter<AkhRhaiBait>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<JudgmentBolt>()
            .ActivateOnEnter<EarthBreath>()
            .ActivateOnEnter<DiamondDust>()
            .ActivateOnEnter<ThinIce>()
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<SuperCyclone>()
            .ActivateOnEnter<GyreCharge>()
            .ActivateOnEnter<TargetPriority>()
            .ActivateOnEnter<DarkMatterPush>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10, AID.TidalWaveVisual, 13.2f, 10, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        ActorCast(id + 0x20, LeftWing, AID.SummonIcicle, 13.1f, 4, false, "Icicles");
        ActorCastMulti(id + 0x30, RightWing, [AID.LevinboltVisual, AID.HypernovaVisual], 10.1f, 8, false, "Spread/stack");
        ComponentCondition<IceStorm>(id + 0x40, 15.4f, comp => comp.NumCasts > 0, "Raidwide", 4)
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x50, AID.AkhMorn, 8.9f, 4, "Shared tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        CastMulti(id + 0x60, [AID.HellfireVisual, AID.JudgmentBoltVisual], 5.3f, 10, "Raidwide (water)")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCast(id + 0x70, RightWing, AID.LevinboltVisual, 18.2f, 8, false, "Spread");
        ActorCast(id + 0x80, LeftWing, AID.IceStormVisual, 9.0f, 6, false);
        ComponentCondition<IceStorm>(id + 0x82, 0.6f, comp => comp.NumCasts > 1, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x90, AID.EarthBreath, 9.5f, 6, "Baits");
        Cast(id + 0xA0, AID.AkhMorn, 3.4f, 4, "Shared tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorCast(id + 0xB0, LeftWing, AID.IceStormVisual, 0, 6, false);
        ComponentCondition<IceStorm>(id + 0xB2, 0.6f, comp => comp.NumCasts > 2, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0xC0, AID.DiamondDustVisual, 3.7f, 10, "Raidwide + thin ice")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Dragonflight>(id + 0xD0, 17.3f, comp => comp.NumWaves > 0, "Reiryu (heal)");
        ActorCast(id + 0xE0, LeftWing, AID.SummonIcicle, 39.1f, 4, false, "Icicles");
        Cast(id + 0xF0, AID.AkhMorn, 0, 4, "Shared tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        Cast(id + 0x100, AID.AerialBlastVisual, 9.3f, 10, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        Cast(id + 0x110, AID.EarthBreath, 21.1f, 6, "Baits");
        ComponentCondition<IceStorm>(id + 0x120, 3.7f, comp => comp.NumCasts > 3, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<GyreCharge>(id + 0x130, 7.0f, comp => comp.Casters.Count > 0, "Gyre charge", 3);
        ActorCast(id + 0x140, RightWing, AID.HypernovaVisual, 13.3f, 8, false, "Stack");
        Cast(id + 0x150, AID.AkhMorn, 8.4f, 4, "Shared tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorCastStart(id + 0x160, RightWing, AID.LevinboltVisual, 7.6f);
        CastStart(id + 0x170, AID.TidalWaveVisual, 4.0f);
        ActorCastEnd(id + 0x171, RightWing, 3.7f, false, "Spread");
        CastEnd(id + 0x172, 6.0f, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        SimpleState(id + 0xFF0000, 100, "???");
    }

    private void Phase2(uint id)
    {
        CastEnd(id, 3, "Raidwide")
            .ActivateOnEnter<DarkMatter>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<TouchdownSpot>()
            .ActivateOnEnter<TailSpitKnockback>()
            .ActivateOnEnter<Cocoons>()
            .ActivateOnEnter<DragonAdds>()
            .ActivateOnEnter<MeteorImpactMarker>()
            .ActivateOnEnter<CocoonMarker>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<BlazingTrail>()
            .ActivateOnEnter<SpikedTail>()
            .ActivateOnEnter<DeathSentence>()
            .ActivateOnEnter<Protostar>()
            .ActivateOnEnter<TailSpit>()
            .ActivateOnEnter<TailSpitCenter>()
            .ActivateOnEnter<AetherialRay>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Touchdown>(id + 0x20, 38.4f, comp => comp.NumCasts > 0, "Proximity", 2)
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<CocoonMarker>(id + 0x30, 46.5f, comp => comp.Active, "Cocoon markers", 3);
        Cast(id + 0x40, AID.Protostar, 31.0f, 6, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<TailSpit>(id + 0x50, 9.4f, comp => comp.NumCasts > 0, "Raidwide", 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        Timeout(id + 0x60, 22.2f, "Shatter");
    }

    private void Phase3(uint id)
    {
        CastStart(id, AID.TeraSlash, 18.2f)
            .ActivateOnEnter<TeraSlash>()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<Wormwail>()
            .ActivateOnEnter<DoomPuddle>()
            .ActivateOnEnter<BenightingBreath>()
            .ActivateOnEnter<IceStormP3>()
            .ActivateOnEnter<IceStormFreeze>()
            .ActivateOnEnter<HypernovaP3>()
            .ActivateOnEnter<LevinboltP3Hint>()
            .ActivateOnEnter<LevinboltP3>();
        CastEnd(id + 1, 4, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AtomicRay>(id + 0x10, 6.2f, comp => comp.Active, "", 2);
        ComponentCondition<AtomicRay>(id + 0x11, 7.2f, comp => !comp.Active, "Stacks", 2);
        ActorCast(id + 0x20, LeftWingP3, AID.IceStormVisualP3, 7.0f, 6, false);
        ComponentCondition<IceStormP3>(id + 0x22, 0.6f, comp => comp.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x30, [AID.Wormwail, AID.BenightingBreathVisual], 6.4f, 4.5f, "Donut/cone");
        Cast(id + 0x40, AID.TeraSlash, 5.2f, 4, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<Dragonflight>(id + 0x50, 4.3f, comp => comp.WaveJustSpawned, "Reiryu (heal)", 3);
        Cast(id + 0x60, AID.TeraSlash, 42.0f, 4, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AtomicRay>(id + 0x70, 6.2f, comp => comp.Active, "", 2);
        ComponentCondition<AtomicRay>(id + 0x71, 7.2f, comp => !comp.Active, "Stacks", 2);
        ActorCast(id + 0x80, LeftWingP3, AID.IceStormVisualP3, 7.0f, 6, false);
        ComponentCondition<IceStormP3>(id + 0x82, 0.6f, comp => comp.NumCasts > 1, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x90, [AID.Wormwail, AID.BenightingBreathVisual], 6.4f, 4.5f, "Donut/cone");
        SimpleState(id + 0xFF0000, 100, "???");
    }

    private void Phase4(uint id)
    {
        CastEnd(id, 70, "Enrage")
            .ActivateOnEnter<Enrage>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private Actor? LeftWing() => Module.Enemies(OID.LeftWing).FirstOrDefault();
    private Actor? RightWing() => Module.Enemies(OID.RightWing).FirstOrDefault();
    private Actor? LeftWingP3() => Module.Enemies(OID.LeftWingP3).FirstOrDefault();
}
