namespace BossMod.Endwalker.Unreal.Un4Zurvan;

class Un4ZurvanStates : StateMachineBuilder
{
    readonly Un4Zurvan _module;

    public Un4ZurvanStates(Un4Zurvan module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1 + P2 until 89%")
            .ActivateOnEnter<P2Eidos>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (Module.FindComponent<P2Eidos>()?.PhaseIndex ?? 0) >= 1;
        SimplePhase(1, Phase2, "P2 until 74%")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (Module.FindComponent<P2Eidos>()?.PhaseIndex ?? 0) >= 2;
        SimplePhase(2, Phase3, "P2 until adds die")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (Module.FindComponent<P2Eidos>()?.PhaseIndex ?? 0) >= 3;
        DeathPhase(3, Phase4);
    }

    private void Phase1(uint id)
    {
        Phase1Platforms(id, 9.2f);
        Phase2EarlyWaveCannon(id + 0x10000, 9); // note: repeats until phase end
    }

    private void Phase2(uint id)
    {
        Phase2EarlySoar(id, 6.2f);
        Phase2EarlySoar(id + 0x100000, 6.2f); // note: repeats until phase end
    }

    private void Phase3(uint id)
    {
        MetalCutter(id, 7.3f);
        MetalCutter(id + 0x10000, 4.1f);
        MetalCutter(id + 0x20000, 4.1f);
        Phase2Icy(id + 0x30000, 3.3f);
        Phase2Adds(id + 0x40000, 6.8f);
    }

    private void Phase4(uint id)
    {
        ActorTargetable(id, _module.BossP2, true, 27.9f, "Reappear");
        Phase2BrokenSeals(id + 0x100000, 6.2f, 5);
        Phase2BrokenSeals(id + 0x200000, 6.2f, 7); // and continues?...
    }

    private void Phase1Platforms(uint id, float delay)
    {
        ComponentCondition<P1Platforms>(id, delay, comp => comp.ForbiddenPlatforms.Count >= 1)
            .ActivateOnEnter<P1MetalCutter>()
            .ActivateOnEnter<P1Platforms>();
        ActorCastStart(id + 0x10, _module.BossP1, AID.FlareStar, 3.2f, true);
        ComponentCondition<P1Platforms>(id + 0x20, 1.8f, comp => comp.NumCasts >= 1, "Platform E")
            .ActivateOnEnter<P1FlareStar>();
        ActorCastEnd(id + 0x30, _module.BossP1, 1.2f, true);
        ComponentCondition<P1FlareStar>(id + 0x40, 0.6f, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P1FlareStar>();

        ComponentCondition<P1Platforms>(id + 0x100, 4.5f, comp => comp.ForbiddenPlatforms.Count >= 2);
        ComponentCondition<P1Platforms>(id + 0x110, 5.0f, comp => comp.NumCasts >= 2, "Platform N");

        ComponentCondition<P1Platforms>(id + 0x200, 5.2f, comp => comp.ForbiddenPlatforms.Count >= 3)
            .DeactivateOnExit<P1MetalCutter>(); // no more cleaves after third platform becomes unsafe
        ActorCast(id + 0x210, _module.BossP1, AID.FlareStar, 0.1f, 3, true)
            .ActivateOnEnter<P1FlareStar>();
        ComponentCondition<P1FlareStar>(id + 0x220, 0.6f, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P1FlareStar>();
        ComponentCondition<P1Platforms>(id + 0x230, 1.3f, comp => comp.NumCasts >= 3, "Platform W");

        ComponentCondition<P1Purge>(id + 0x300, 3.6f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P1Purge>()
            .DeactivateOnExit<P1Purge>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x310, _module.BossP1, false, 9, "Disappear")
            .DeactivateOnExit<P1Platforms>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void Phase2EarlyWaveCannon(uint id, float delay)
    {
        ActorTargetable(id, _module.BossP2, true, delay, "Reappear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        SimpleState(id + 0x100, 3.2f, "??? (until 89%)") // TODO: wave cannons...
            .ActivateOnEnter<P2MetalCutter>();
    }

    private void Phase2EarlySoar(uint id, float delay)
    {
        MetalCutter(id, delay);
        MetalCutter(id + 0x10000, 5.1f);
        Soar(id + 0x20000, 5.1f);
        MetalCutter(id + 0x30000, 3.1f);
        MetalCutter(id + 0x40000, 5.1f);
        DemonsClaw(id + 0x50000, 5.2f);
    }

    private void Phase2Icy(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.BitingHalberd, delay, true)
            .ActivateOnEnter<P2IcyVoidzone>();
        ActorCastEnd(id + 1, _module.BossP2, 5, true, "Frontal cone")
            .ActivateOnEnter<P2BitingHalberd>()
            .DeactivateOnExit<P2BitingHalberd>();
        SouthernCross(id + 0x100, 3.2f);
        ActorCastStartMulti(id + 0x200, _module.BossP2, new[] { AID.BitingHalberd, AID.TailEnd, AID.Ciclicle }, 6.8f, true)
            .ActivateOnEnter<P2SouthernCrossVoidzone>()
            .ActivateOnEnter<P2MetalCutter>() // 1 metal cutter before cast start
            .DeactivateOnExit<P2MetalCutter>();
        ActorCastEnd(id + 0x201, _module.BossP2, 5, true, "Cone/in/out")
            .ActivateOnEnter<P2BitingHalberd>()
            .ActivateOnEnter<P2TailEnd>()
            .ActivateOnEnter<P2Ciclicle>()
            .DeactivateOnExit<P2SouthernCrossVoidzone>()
            .DeactivateOnExit<P2BitingHalberd>()
            .DeactivateOnExit<P2TailEnd>()
            .DeactivateOnExit<P2Ciclicle>();
        ActorTargetable(id + 0x300, _module.BossP2, false, 3.2f, "Disappear")
            .DeactivateOnExit<P2IcyVoidzone>();
    }

    private void Phase2Adds(uint id, float delay)
    {
        ComponentCondition<P2ExecratedWill>(id, delay, comp => comp.ActiveActors.Any(), "Add wave 1")
            .ActivateOnEnter<P2ExecratedWill>();
        SimpleState(id + 0x100, 100, "Wave 1: tank+aoe N; wave 2: tank W, caster E, gaze S; wave 3: tank S, aoe E, caster E, gaze N") // TODO: enrage timer
            .ActivateOnEnter<P2ExecratedWit>()
            .ActivateOnEnter<P2ExecratedWile>()
            .ActivateOnEnter<P2ExecratedThew>()
            .ActivateOnEnter<P2Comet>()
            .ActivateOnEnter<P2MeracydianFear>();
    }

    private void Phase2BrokenSeals(uint id, float delay, int firstTyrfings)
    {
        BrokenSeal(id, delay, firstTyrfings);
        MetalCutter(id + 0x10000, 6.2f);
        BrokenSeal(id + 0x20000, 5.1f, firstTyrfings + 1);
        MetalCutter(id + 0x30000, 6.2f);
        Soar(id + 0x40000, 5.1f);
        DemonsClaw(id + 0x50000, 3.1f);
    }

    private State MetalCutter(uint id, float delay)
    {
        return ComponentCondition<P2MetalCutter>(id, delay, comp => comp.NumCasts > 0, "Cleave")
            .ActivateOnEnter<P2MetalCutter>()
            .DeactivateOnExit<P2MetalCutter>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Soar(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.Soar, delay, true);
        ActorCastEnd(id + 1, _module.BossP2, 5, true, "Soar")
            .ActivateOnEnter<P2SoarTwinSpirit>();
        ActorTargetable(id + 0x10, _module.BossP2, false, 0.2f);
        ComponentCondition<P2SoarTwinSpirit>(id + 0x20, 9.0f, comp => comp.NumCasts > 0, "Charges")
            .ActivateOnEnter<P2SoarFlamingHalberd>() // 3.9s after untargetable
            .ActivateOnEnter<P2SoarDemonicDiveCoolFlame>() // stack marker appears together with charge cast, with extremely slight overlap with halberd
            .DeactivateOnExit<P2SoarTwinSpirit>();
        ComponentCondition<P2SoarFlamingHalberd>(id + 0x21, 0.1f, comp => !comp.Active)
            .DeactivateOnExit<P2SoarFlamingHalberd>();
        ComponentCondition<P2SoarDemonicDiveCoolFlame>(id + 0x30, 5.1f, comp => comp.Stacks.Count == 0, "Stack")
            .ActivateOnEnter<P2SoarFlamingHalberdVoidzone>();
        ComponentCondition<P2SoarDemonicDiveCoolFlame>(id + 0x31, 0.9f, comp => !comp.Active, "Spread")
            .DeactivateOnExit<P2SoarDemonicDiveCoolFlame>();
        ActorTargetable(id + 0x40, _module.BossP2, true, 2.2f, "Reappear")
            .DeactivateOnExit<P2SoarFlamingHalberdVoidzone>(); // note: they actually disappear ~0.8s later, but who cares
    }

    private void DemonsClaw(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.DemonsClaw, delay, true);
        ActorCastEnd(id + 1, _module.BossP2, 3, true, "Knockback tankbuster")
            .ActivateOnEnter<P2DemonsClawKnockback>()
            .ActivateOnEnter<P2DemonsClawWaveCannon>()
            .DeactivateOnExit<P2DemonsClawKnockback>();
        // next cast is skipped if knockback target dies
        ComponentCondition<P2DemonsClawWaveCannon>(id + 0x10, 2.1f, comp => comp.Source != null || comp.Target == null || comp.Target.IsDead)
            .SetHint(StateMachine.StateHint.BossCastStart);
        ActorCastEnd(id + 0x11, _module.BossP2, 5, true, "Shared aoe")
            .DeactivateOnExit<P2DemonsClawWaveCannon>();
    }

    private void SouthernCross(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.SouthernCross, delay, 3, true, "Puddles bait");
        ComponentCondition<P2SouthernCross>(id + 0x10, 3.6f, comp => comp.NumCasts > 0, "Puddles resolve")
            .ActivateOnEnter<P2SouthernCross>()
            .DeactivateOnExit<P2SouthernCross>();
    }

    private void BrokenSeal(uint id, float delay, int tyrfings)
    {
        ComponentCondition<P2BrokenSeal>(id, delay, comp => comp.NumAssigned > 0)
            .ActivateOnEnter<P2BrokenSeal>();
        ActorCast(id + 0x100, _module.BossP2, AID.WaveCannonSolo, 3.0f, 5, true, "Wave cannon")
            .ActivateOnEnter<P2WaveCannon>()
            .DeactivateOnExit<P2WaveCannon>();

        ActorCast(id + 0x200, _module.BossP2, AID.Tyrfing, 4.4f, 3, true, $"Multi tankbuster ({tyrfings})")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P2TyrfingFire>(id + 0x210, tyrfings * 1.0f + 0.3f, comp => comp.NumCasts > 0, "AOE tankbuster", 10) // 1: 4 hits, 5.3; 2: 5 hits, 6.3; 3: 6 hits, 7.3
            .ActivateOnEnter<P2TyrfingFire>()
            .DeactivateOnExit<P2TyrfingFire>()
            .SetHint(StateMachine.StateHint.Tankbuster);

        SouthernCross(id + 0x300, 3.2f);
        MetalCutter(id + 0x400, 1.6f)
            .ActivateOnEnter<P2SouthernCrossVoidzone>();
        ActorCast(id + 0x500, _module.BossP2, AID.BrokenSeal, 6.2f, 3, true)
            .DeactivateOnExit<P2SouthernCrossVoidzone>();
        // note: timings below have significant variance
        ActorCastStartMulti(id + 0x510, _module.BossP2, new[] { AID.BitingHalberd, AID.TailEnd, AID.Ciclicle }, 10.5f, true);
        ComponentCondition<P2BrokenSeal>(id + 0x520, 1.2f, comp => comp.NumCasts > 0, "Towers", 2)
            .ActivateOnEnter<P2BitingHalberd>()
            .ActivateOnEnter<P2TailEnd>()
            .ActivateOnEnter<P2Ciclicle>()
            .DeactivateOnExit<P2BrokenSeal>();
        ActorCastEnd(id + 0x530, _module.BossP2, 3.8f, true, "Cone/in/out")
            .DeactivateOnExit<P2BitingHalberd>()
            .DeactivateOnExit<P2TailEnd>()
            .DeactivateOnExit<P2Ciclicle>();
    }
}
