namespace BossMod.Dawntrail.Ultimate.FRU;

class FRUStates : StateMachineBuilder
{
    private readonly FRU _module;

    private static bool IsActorDead(Actor? a, bool valueIfNull) => a == null ? valueIfNull : (a.IsDeadOrDestroyed || a.HPMP.CurHP <= 1);

    public FRUStates(FRU module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1: Fatebreaker")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed;
        SimplePhase(1, Phase2, "P2: Usurper of Frost")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => !Module.PrimaryActor.IsDead || (_module.BossP2()?.IsDestroyed ?? false) || (_module.IceVeil()?.IsDeadOrDestroyed ?? false);
        SimplePhase(2, Phase34, "P3/4: Oracle of Darkness & Both")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => !Module.PrimaryActor.IsDead || (_module.BossP2()?.IsDestroyed ?? false) && (_module.BossP3()?.IsDestroyed ?? true) && IsActorDead(_module.BossP4Oracle(), true) && IsActorDead(_module.BossP4Usurper(), true);
        SimplePhase(3, Phase5, "P5: Pandora")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => !Module.PrimaryActor.IsDead || (_module.BossP4Oracle()?.IsDeadOrDestroyed ?? true) && (_module.BossP5()?.IsDeadOrDestroyed ?? true);
    }

    private void Phase1(uint id)
    {
        P1CyclonicBreakPowderMarkTrail(id, 7.2f);
        P1UtopianSky(id + 0x10000, 6.7f);
        P1CyclonicBreakImage(id + 0x20000, 6.3f);
        P1TurnOfTheHeavensBoundOfFaith(id + 0x30000, 2.2f);
        P1BurnishedGlory(id + 0x40000, 1.1f);
        P1FallOfFaith(id + 0x50000, 6.6f);
        P1BurnishedGlory(id + 0x60000, 2.7f);
        P1PowderMarkTrailExplosions(id + 0x70000, 3.5f);
        ActorCast(id + 0x80000, _module.BossP1, AID.EnrageP1, 4.6f, 10, true, "Enrage");
    }

    private void Phase2(uint id)
    {
        ActorTargetable(id, _module.BossP2, true, 4.4f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        P2QuadrupleSlap(id + 0x10000, 6.1f);
        P2DiamondDust(id + 0x20000, 6.5f);
        P2HallowedRay(id + 0x30000, 2.1f);
        P2MirrorMirror(id + 0x40000, 6.1f);
        P2LightRampant(id + 0x50000, 4.4f);
        P2AbsoluteZero(id + 0x60000, 8.4f);
    }

    private void Phase34(uint id)
    {
        P3JunctionHellsJudgment(id, 13.3f);
        P3UltimateRelativity(id + 0x10000, 4.3f);
        P3BlackHalo(id + 0x20000, 3.2f);
        P3Apocalypse(id + 0x30000, 7.2f);
        P3Enrage(id + 0x40000, 3.7f);

        P4AkhRhai(id + 0x100000, 5.5f);
        P4DarklitDragonsong(id + 0x110000, 1.9f);
        P4AkhMornMornAfah(id + 0x120000, 5.8f);
        P4CrystallizeTime(id + 0x130000, 4.6f);
        P4AkhMornMornAfah(id + 0x140000, 0.1f);
        P4Enrage(id + 0x150000, 2.3f);
    }

    private void Phase5(uint id)
    {
        P5Start(id, 77);
        P5FulgentBlade(id + 0x10000, 5.3f);
        P5ParadiseRegained(id + 0x20000, 4.2f);
        P5PolarizingStrikes(id + 0x30000, 7.6f);
        P5PandorasBox(id + 0x40000, 5.8f);
        P5FulgentBlade(id + 0x50000, 6.2f);
        P5ParadiseRegained(id + 0x60000, 8.4f);
        P5PolarizingStrikes(id + 0x70000, 2.3f);
        P5FulgentBlade(id + 0x80000, 2.6f);
        P5Enrage(id + 0x90000, 8.4f);
    }

    private void P1CyclonicBreakPowderMarkTrail(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP1, [AID.CyclonicBreakBossStack, AID.CyclonicBreakBossSpread], delay, 6.5f, true)
            .ActivateOnEnter<P1CyclonicBreakSpreadStack>()
            .ActivateOnEnter<P1CyclonicBreakProtean>()
            .ActivateOnEnter<P1CyclonicBreakAIBait>();
        ComponentCondition<P1CyclonicBreakProtean>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Protean 1")
            .ActivateOnEnter<P1CyclonicBreakCone>()
            .DeactivateOnExit<P1CyclonicBreakProtean>()
            .DeactivateOnExit<P1CyclonicBreakAIBait>();
        ComponentCondition<P1CyclonicBreakCone>(id + 0x11, 2.1f, comp => comp.NumCasts > 0, "Protean 2 + Spread/Stack") // both happen at the same time
            .ActivateOnEnter<P1CyclonicBreakAIDodgeSpreadStack>()
            .DeactivateOnExit<P1CyclonicBreakAIDodgeSpreadStack>()
            .DeactivateOnExit<P1CyclonicBreakSpreadStack>();
        ComponentCondition<P1CyclonicBreakCone>(id + 0x12, 2.1f, comp => comp.NumCasts > 1, "Protean 3")
            .ActivateOnEnter<P1CyclonicBreakAIDodgeRest>();

        ActorCastStart(id + 0x100, _module.BossP1, AID.PowderMarkTrail, 0.8f, true);
        ComponentCondition<P1CyclonicBreakCone>(id + 0x101, 1.3f, comp => comp.NumCasts > 2, "Protean 4")
            .DeactivateOnExit<P1CyclonicBreakCone>()
            .DeactivateOnExit<P1CyclonicBreakAIDodgeRest>();
        ActorCastEnd(id + 0x102, _module.BossP1, 3.7f, true, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P1UtopianSky(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP1, [AID.UtopianSkyStack, AID.UtopianSkySpread], delay, 4, true, "Boss disappears")
            .ActivateOnEnter<P1UtopianSkySpreadStack>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P1UtopianSkyBlastingZone>(id + 0x10, 5.2f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<P1PowderMarkTrail>()
            .ActivateOnEnter<P1UtopianSkyBlastingZone>()
            .ActivateOnEnter<P1UtopianSkyAIInitial>();
        ComponentCondition<P1PowderMarkTrail>(id + 0x11, 0.2f, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<P1PowderMarkTrail>()
            .DeactivateOnExit<P1UtopianSkyAIInitial>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P1UtopianSkyBlastingZone>(id + 0x20, 8.9f, comp => comp.NumCasts > 0, "Lines")
            .ExecOnEnter<P1UtopianSkySpreadStack>(comp => comp.Show(Module.WorldState.FutureTime(9.7f)))
            .ActivateOnEnter<P1UtopianSkyAIResolve>()
            .DeactivateOnExit<P1UtopianSkyBlastingZone>();
        ComponentCondition<P1UtopianSkySpreadStack>(id + 0x21, 0.8f, comp => !comp.Active, "Spread/stack")
            .DeactivateOnExit<P1UtopianSkyAIResolve>()
            .DeactivateOnExit<P1UtopianSkySpreadStack>();
    }

    private void P1CyclonicBreakImage(uint id, float delay)
    {
        ComponentCondition<P1CyclonicBreakProtean>(id, delay, comp => comp.NumCasts > 0, "Protean 1")
            .ActivateOnEnter<P1CyclonicBreakSpreadStack>()
            .ActivateOnEnter<P1CyclonicBreakProtean>()
            .ActivateOnEnter<P1CyclonicBreakCone>()
            .ActivateOnEnter<P1CyclonicBreakAIBait>()
            .DeactivateOnExit<P1CyclonicBreakAIBait>()
            .DeactivateOnExit<P1CyclonicBreakProtean>();
        ComponentCondition<P1CyclonicBreakCone>(id + 1, 2.1f, comp => comp.NumCasts > 0, "Protean 2 + Spread/Stack") // both happen at the same time
            .ActivateOnEnter<P1CyclonicBreakAIDodgeSpreadStack>()
            .DeactivateOnExit<P1CyclonicBreakAIDodgeSpreadStack>()
            .DeactivateOnExit<P1CyclonicBreakSpreadStack>();
        ComponentCondition<P1CyclonicBreakCone>(id + 2, 2.1f, comp => comp.NumCasts > 1, "Protean 3")
            .ActivateOnEnter<P1CyclonicBreakAIDodgeRest>();
        ComponentCondition<P1CyclonicBreakCone>(id + 3, 2.1f, comp => comp.NumCasts > 2, "Protean 4")
            .DeactivateOnExit<P1CyclonicBreakAIDodgeRest>()
            .DeactivateOnExit<P1CyclonicBreakCone>();
    }

    private void P1TurnOfTheHeavensBoundOfFaith(uint id, float delay)
    {
        ComponentCondition<P1TurnOfHeavensBurntStrikeLightning>(id, delay, comp => comp.NumCasts > 0, "Line 1")
            .ActivateOnEnter<P1BoundOfFaith>()
            .ActivateOnEnter<P1TurnOfHeavensBurntStrikeLightning>()
            .ActivateOnEnter<P1TurnOfHeavensBurnout>()
            .ActivateOnEnter<P1BoundOfFaithAIKnockback>()
            .DeactivateOnExit<P1TurnOfHeavensBurntStrikeLightning>();
        ComponentCondition<P1TurnOfHeavensBurnout>(id + 0x10, 1.7f, comp => comp.NumCasts > 0, "Line 2")
            .DeactivateOnExit<P1TurnOfHeavensBurnout>();
        ComponentCondition<P1BrightfireSmall>(id + 0x11, 0.4f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P1TurnOfHeavensBurntStrikeFire>()
            .ActivateOnEnter<P1Blastburn>()
            .ActivateOnEnter<P1BrightfireSmall>()
            .ActivateOnEnter<P1BrightfireLarge>()
            .ExecOnEnter<P1BrightfireLarge>(comp => comp.Risky = false); // it's fine to stay in aoes before kb
        ComponentCondition<P1TurnOfHeavensBurntStrikeFire>(id + 0x12, 3.9f, comp => comp.NumCasts > 0, "Line 3")
            .DeactivateOnExit<P1TurnOfHeavensBurntStrikeFire>();
        ComponentCondition<P1Blastburn>(id + 0x13, 2, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<P1BoundOfFaithAIKnockback>()
            .DeactivateOnExit<P1Blastburn>();
        ComponentCondition<P1BrightfireSmall>(id + 0x20, 2.1f, comp => comp.NumCasts > 0, "Circles")
            .ActivateOnEnter<P1BoundOfFaithAIStack>()
            .ExecOnEnter<P1BrightfireLarge>(comp => comp.Risky = true)
            .ExecOnEnter<P1BoundOfFaith>(comp => comp.EnableHints = true)
            .DeactivateOnExit<P1BrightfireSmall>()
            .DeactivateOnExit<P1BrightfireLarge>();
        ComponentCondition<P1BoundOfFaith>(id + 0x30, 4, comp => !comp.Active, "Stacks") // note: won't happen if both targets die, but that's a wipe anyway
            .DeactivateOnExit<P1BoundOfFaithAIStack>()
            .DeactivateOnExit<P1BoundOfFaith>();
        ActorTargetable(id + 0x100, _module.BossP1, true, 1.4f, "Boss reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P1BurnishedGlory(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.BurnishedGlory, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1FallOfFaith(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP1, [AID.FallOfFaithFire, AID.FallOfFaithLightning], delay, 9, true)
            .ActivateOnEnter<P1FallOfFaith>();
        ComponentCondition<P1FallOfFaith>(id + 0x10, 4.1f, comp => comp.NumCasts >= 1, "Baits 1");
        ComponentCondition<P1FallOfFaith>(id + 0x20, 3.1f, comp => comp.NumCasts >= 2, "Baits 2");
        ComponentCondition<P1FallOfFaith>(id + 0x30, 2.5f, comp => comp.NumCasts >= 3, "Baits 3");
        ComponentCondition<P1FallOfFaith>(id + 0x40, 2.5f, comp => comp.NumCasts >= 4, "Baits 4")
            .DeactivateOnExit<P1FallOfFaith>();
    }

    private void P1PowderMarkTrailExplosions(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.PowderMarkTrail, delay, 5, true, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P1Explosion>(id + 0x100, 5.1f, comp => comp.Towers.Count > 0)
            .ActivateOnEnter<P1Explosion>();
        Condition(id + 0x101, 6.5f, () => Module.FindComponent<P1ExplosionBurntStrikeFire>()?.NumCasts > 0 || Module.FindComponent<P1ExplosionBurntStrikeLightning>()?.NumCasts > 0, "Narrow line")
            .ActivateOnEnter<P1ExplosionBurntStrikeFire>()
            .ActivateOnEnter<P1ExplosionBurntStrikeLightning>()
            .ActivateOnEnter<P1ExplosionBurnout>()
            .ActivateOnEnter<P1Blastburn>()
            .ExecOnEnter<P1ExplosionBurnout>(comp => comp.Risky = false) // fine to greed
            .DeactivateOnExit<P1ExplosionBurntStrikeFire>()
            .DeactivateOnExit<P1ExplosionBurntStrikeLightning>();
        Condition(id + 0x102, 2, () => Module.FindComponent<P1ExplosionBurnout>()?.NumCasts > 0 || Module.FindComponent<P1Blastburn>()?.NumCasts > 0, "Line/Knockback", checkDelay: 2) // note: kb and wide line have slightly different cast time...
            .ExecOnEnter<P1ExplosionBurnout>(comp => comp.Risky = true)
            .DeactivateOnExit<P1ExplosionBurnout>()
            .DeactivateOnExit<P1Blastburn>();
        ComponentCondition<P1Explosion>(id + 0x103, 2, comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<P1PowderMarkTrail>()
            .ExecOnEnter<P1PowderMarkTrail>(comp => comp.AllowTankStacking = Service.Config.Get<FRUConfig>().P1ExplosionsTankbusterCheese)
            .DeactivateOnExit<P1Explosion>();
        ComponentCondition<P1PowderMarkTrail>(id + 0x104, 0.5f, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<P1PowderMarkTrail>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2QuadrupleSlap(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.QuadrupleSlapFirst, delay, 5, true, "Tankbuster 1")
            .ActivateOnEnter<P2QuadrupleSlap>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorCast(id + 0x10, _module.BossP2, AID.QuadrupleSlapSecond, 1.7f, 2.5f, true, "Tankbuster 2")
            .DeactivateOnExit<P2QuadrupleSlap>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2DiamondDust(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.MirrorImage, delay, 3, true);
        ActorCast(id + 0x10, _module.BossP2, AID.DiamondDust, 2.1f, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x20, _module.BossP2, false, 3.1f, "Boss disappears")
            .ActivateOnEnter<P2AxeKick>()
            .ActivateOnEnter<P2ScytheKick>()
            .ActivateOnEnter<P2IcicleImpact>()
            .ActivateOnEnter<P2FrigidStone>()
            .ActivateOnEnter<P2DiamondDustHouseOfLight>()
            .ActivateOnEnter<P2DiamondDustSafespots>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Condition(id + 0x30, 5.7f, () => Module.FindComponent<P2AxeKick>()?.NumCasts > 0 || Module.FindComponent<P2ScytheKick>()?.NumCasts > 0, "In/out")
            .DeactivateOnExit<P2AxeKick>()
            .DeactivateOnExit<P2ScytheKick>();
        ComponentCondition<P2DiamondDustHouseOfLight>(id + 0x31, 0.8f, comp => comp.NumCasts > 0, "Proteans")
            .ExecOnEnter<P2FrigidStone>(comp => comp.EnableHints = true)
            .DeactivateOnExit<P2DiamondDustHouseOfLight>();
        ComponentCondition<P2FrigidStone>(id + 0x32, 1.6f, comp => comp.NumCasts > 0, "Ice baits")
            .DeactivateOnExit<P2FrigidStone>()
            .DeactivateOnExit<P2DiamondDustSafespots>();
        ComponentCondition<P2IcicleImpact>(id + 0x33, 0.4f, comp => comp.NumCasts > 0, "Ice circle 1");
        ComponentCondition<P2HeavenlyStrike>(id + 0x40, 3.9f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<P2HeavenlyStrike>()
            .ActivateOnEnter<P2FrigidNeedleCircle>()
            .ActivateOnEnter<P2FrigidNeedleCross>()
            .ActivateOnEnter<P2TwinStillnessSilence>() // show the cone caster early, to simplify finding movement direction...
            .ExecOnEnter<P2FrigidNeedleCircle>(comp => comp.Risky = false)
            .ExecOnEnter<P2FrigidNeedleCross>(comp => comp.Risky = false)
            .DeactivateOnExit<P2HeavenlyStrike>();
        ComponentCondition<P2FrigidNeedleCross>(id + 0x50, 2.8f, comp => comp.NumCasts > 0, "Stars")
            .ActivateOnEnter<P2SinboundHoly>()
            .ExecOnEnter<P2FrigidNeedleCircle>(comp => comp.Risky = true)
            .ExecOnEnter<P2FrigidNeedleCross>(comp => comp.Risky = true)
            .DeactivateOnExit<P2FrigidNeedleCircle>()
            .DeactivateOnExit<P2FrigidNeedleCross>();
        ComponentCondition<P2SinboundHoly>(id + 0x60, 1.3f, comp => comp.NumCasts > 0);
        ComponentCondition<P2SinboundHoly>(id + 0x70, 4.7f, comp => comp.NumCasts >= 4)
            .ActivateOnEnter<P2SinboundHolyVoidzone>()
            .ActivateOnEnter<P2ShiningArmor>()
            .DeactivateOnExit<P2IcicleImpact>() // last icicle explodes together with first stack
            .DeactivateOnExit<P2SinboundHoly>();
        ComponentCondition<P2ShiningArmor>(id + 0x80, 3.7f, comp => comp.NumCasts > 0, "Gaze")
            .ExecOnEnter<P2TwinStillnessSilence>(comp => comp.EnableAIHints())
            .ExecOnEnter<P2SinboundHolyVoidzone>(comp => comp.AIHintsEnabled = false)
            .DeactivateOnExit<P2ShiningArmor>();
        ComponentCondition<P2TwinStillnessSilence>(id + 0x90, 3.0f, comp => comp.AOEs.Count > 0);
        ComponentCondition<P2TwinStillnessSilence>(id + 0x91, 3.5f, comp => comp.NumCasts > 0, "Front/back");
        ComponentCondition<P2TwinStillnessSilence>(id + 0x92, 2.1f, comp => comp.NumCasts > 1, "Back/front")
            .DeactivateOnExit<P2TwinStillnessSilence>()
            .DeactivateOnExit<P2SinboundHolyVoidzone>();
        ActorTargetable(id + 0xA0, _module.BossP2, true, 3.6f, "Boss reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P2HallowedRay(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.HallowedRay, delay, true)
            .ActivateOnEnter<P2HallowedRay>();
        ActorCastEnd(id + 1, _module.BossP2, 5, true);
        ComponentCondition<P2HallowedRay>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Line stack")
            .DeactivateOnExit<P2HallowedRay>();
    }

    private void P2MirrorMirror(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.MirrorMirror, delay, 3, true)
            .ActivateOnEnter<P2MirrorMirrorReflectedScytheKickBlue>()
            .ActivateOnEnter<P2MirrorMirrorHouseOfLight>();
        ActorCast(id + 0x10, _module.BossP2, AID.ScytheKick, 8.2f, 6, true)
            .ActivateOnEnter<P2ScytheKick>()
            .DeactivateOnExit<P2ScytheKick>()
            .DeactivateOnExit<P2MirrorMirrorReflectedScytheKickBlue>();
        ComponentCondition<P2MirrorMirrorHouseOfLight>(id + 0x12, 0.7f, comp => comp.NumCasts > 0, "Mirror 1");
        ComponentCondition<P2MirrorMirrorReflectedScytheKickRed>(id + 0x20, 9.3f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<P2MirrorMirrorReflectedScytheKickRed>()
            .DeactivateOnExit<P2MirrorMirrorReflectedScytheKickRed>();
        ComponentCondition<P2MirrorMirrorHouseOfLight>(id + 0x21, 0.6f, comp => comp.NumCasts > 8, "Mirror 2")
            .ActivateOnEnter<P2MirrorMirrorBanish>() // activate a bit early, so that it can read state gathered by house of light component
            .DeactivateOnExit<P2MirrorMirrorHouseOfLight>();

        ActorCastMulti(id + 0x100, _module.BossP2, [AID.BanishStack, AID.BanishSpread], 0.5f, 5, true);
        ComponentCondition<P2Banish>(id + 0x102, 0.1f, comp => !comp.Active, "Spread/Stack")
            .DeactivateOnExit<P2Banish>();
    }

    private void P2LightRampant(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.LightRampant, delay, 5, true, "Raidwide (light rampant)")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x10, _module.BossP2, false, 3.1f, "Boss disappears")
            .ActivateOnEnter<P2LightRampant>()
            .ActivateOnEnter<P2LuminousHammer>()
            .ActivateOnEnter<P2BrightHunger1>()
            .ActivateOnEnter<P2LightRampantAITowers>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P2LuminousHammer>(id + 0x20, 4.8f, comp => comp.NumCasts > 0, "Puddle bait");
        ComponentCondition<P2BrightHunger1>(id + 0x30, 3.3f, comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<P2SinboundHolyVoidzone>()
            .DeactivateOnExit<P2LightRampantAITowers>()
            .DeactivateOnExit<P2BrightHunger1>();
        ComponentCondition<P2HolyLightBurst>(id + 0x38, 3.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P2HolyLightBurst>()
            .ActivateOnEnter<P2PowerfulLight>()
            .ActivateOnEnter<P2LightRampantAIStackPrepos>()
            .DeactivateOnExit<P2LightRampantAIStackPrepos>()
            .DeactivateOnExit<P2LuminousHammer>(); // last puddle is baited right before holy light burst casts start
        ComponentCondition<P2PowerfulLight>(id + 0x40, 2.5f, comp => !comp.Active, "Stack")
            .ActivateOnEnter<P2LightRampantAIStackResolve>()
            .DeactivateOnExit<P2LightRampantAIStackResolve>()
            .DeactivateOnExit<P2PowerfulLight>();
        ComponentCondition<P2HolyLightBurst>(id + 0x50, 2.4f, comp => comp.NumCasts > 0, "Orbs 1")
            .ActivateOnEnter<P2LightRampantAIOrbs>()
            .ActivateOnEnter<P2BrightHunger2>();
        ComponentCondition<P2HolyLightBurst>(id + 0x60, 3, comp => comp.NumCasts > 3, "Orbs 2")
            .DeactivateOnExit<P2LightRampantAIOrbs>()
            .DeactivateOnExit<P2HolyLightBurst>()
            .DeactivateOnExit<P2LightRampant>(); // tethers resolve right after first orbs

        ActorCastStartMulti(id + 0x70, _module.BossP2, [AID.BanishStack, AID.BanishSpread], 1.7f, true)
            .ActivateOnEnter<P2LightRampantBanish>();
        ComponentCondition<P2BrightHunger2>(id + 0x71, 1.9f, comp => comp.NumCasts > 0, "Central tower")
            .DeactivateOnExit<P2BrightHunger2>()
            .DeactivateOnExit<P2SinboundHolyVoidzone>();
        ActorCastEnd(id + 0x72, _module.BossP2, 3.1f, true);
        ComponentCondition<P2Banish>(id + 0x73, 0.1f, comp => !comp.Active, "Spread/Stack")
            .DeactivateOnExit<P2Banish>();
        ActorTargetable(id + 0x80, _module.BossP2, true, 3, "Boss reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x1000, _module.BossP2, AID.HouseOfLightBoss, 0.1f, 5, true)
            .ActivateOnEnter<P2HouseOfLightBoss>();
        ComponentCondition<P2HouseOfLightBoss>(id + 0x1002, 0.9f, comp => comp.NumCasts > 0, "Proteans")
            .DeactivateOnExit<P2HouseOfLightBoss>();
    }

    private void P2AbsoluteZero(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.AbsoluteZero, delay, 10, true, "Intermission start")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P2AbsoluteZero>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P2AbsoluteZero>()
            .ActivateOnEnter<P2SwellingFrost>()
            .DeactivateOnExit<P2AbsoluteZero>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2SwellingFrost>(id + 3, 2.3f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<P2SwellingFrost>();

        ComponentCondition<P2Intermission>(id + 0x1000, 18.9f, comp => comp.CrystalsActive, "Crystals appear")
            .ActivateOnEnter<P2SinboundBlizzard>()
            .ActivateOnEnter<P2Intermission>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCast(id + 0x1010, _module.IceVeil, AID.EndlessIceAge, 4.7f, 40, true, "Enrage")
            .ActivateOnEnter<P2HiemalStorm>()
            .ActivateOnEnter<P2HiemalRay>()
            .DeactivateOnExit<P2Intermission>()
            .DeactivateOnExit<P2SinboundBlizzard>()
            .DeactivateOnExit<P2HiemalStorm>()
            .DeactivateOnExit<P2HiemalRay>();
    }

    private void P3JunctionHellsJudgment(uint id, float delay)
    {
        ComponentCondition<P3Junction>(id, delay, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P3Junction>()
            .DeactivateOnExit<P3Junction>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x10, _module.BossP3, true, 14.2f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCast(id + 0x20, _module.BossP3, AID.HellsJudgment, 0.1f, 4, true, "1hp")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P3UltimateRelativity(uint id, float delay)
    {
        ActorCast(id, _module.BossP3, AID.UltimateRelativity, delay, 10, true, "Raidwide (relativity)")
            .ActivateOnEnter<P3UltimateRelativity>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCast(id + 0x10, _module.BossP3, AID.UltimateRelativitySpeed, 4.1f, 5.5f, true)
            .ActivateOnEnter<P3UltimateRelativityDarkFireUnholyDarkness>();
        ComponentCondition<P3UltimateRelativity>(id + 0x20, 2.3f, comp => comp.NumCasts >= 1, "Spread/stack 1")
            .DeactivateOnExit<P3UltimateRelativityDarkFireUnholyDarkness>();
        ComponentCondition<P3UltimateRelativity>(id + 0x30, 5.1f, comp => comp.NumCasts >= 2, "Lasers 1")
            .ActivateOnEnter<P3UltimateRelativitySinboundMeltdownBait>()
            .ActivateOnEnter<P3UltimateRelativitySinboundMeltdownAOE>();
        ComponentCondition<P3UltimateRelativity>(id + 0x40, 4.9f, comp => comp.NumCasts >= 3, "Spread/stack 2")
            .ActivateOnEnter<P3UltimateRelativityDarkBlizzard>()
            .ActivateOnEnter<P3UltimateRelativityDarkFireUnholyDarkness>()
            .DeactivateOnExit<P3UltimateRelativityDarkBlizzard>()
            .DeactivateOnExit<P3UltimateRelativityDarkFireUnholyDarkness>();
        ComponentCondition<P3UltimateRelativity>(id + 0x50, 5.1f, comp => comp.NumCasts >= 4, "Lasers 2");
        ComponentCondition<P3UltimateRelativity>(id + 0x60, 4.9f, comp => comp.NumCasts >= 5, "Spread/stack 3")
            .ActivateOnEnter<P3UltimateRelativityDarkFireUnholyDarkness>()
            .DeactivateOnExit<P3UltimateRelativityDarkFireUnholyDarkness>();
        ComponentCondition<P3UltimateRelativity>(id + 0x70, 6.1f, comp => comp.NumCasts >= 6, "Lasers 3")
            .DeactivateOnExit<P3UltimateRelativitySinboundMeltdownBait>();
        ComponentCondition<P3UltimateRelativity>(id + 0x80, 2.8f, comp => comp.NumReturnStuns > 0, "Return")
            .ActivateOnEnter<P3UltimateRelativityShadoweye>() // note: there are no hints for stack or eruption, as they should have been resolved earlier...
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ActorCastStart(id + 0x90, _module.BossP3, AID.ShellCrusher, 3.9f, true, "Relativity resolve")
            .DeactivateOnExit<P3UltimateRelativityShadoweye>()
            .DeactivateOnExit<P3UltimateRelativity>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCastEnd(id + 0x91, _module.BossP3, 3, true)
            .ActivateOnEnter<P3ShellCrusher>();
        ComponentCondition<P3ShellCrusher>(id + 0x92, 0.4f, comp => comp.Stacks.Count == 0, "Stack")
            .DeactivateOnExit<P3ShellCrusher>();

        P3ShockwavePulsar(id + 0x1000, 3.5f)
            .DeactivateOnExit<P3UltimateRelativitySinboundMeltdownAOE>();
    }

    private State P3ShockwavePulsar(uint id, float delay)
    {
        return ActorCast(id, _module.BossP3, AID.ShockwavePulsar, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P3BlackHalo(uint id, float delay)
    {
        ActorCast(id, _module.BossP3, AID.BlackHalo, delay, 5, true)
            .ActivateOnEnter<P3BlackHalo>();
        ComponentCondition<P3BlackHalo>(id + 2, 0.2f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<P3BlackHalo>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P3Apocalypse(uint id, float delay)
    {
        ActorCast(id, _module.BossP3, AID.SpellInWaitingRefrain, delay, 2, true);
        ActorCast(id + 0x10, _module.BossP3, AID.ApocalypseDarkWater, 3.2f, 5, true);
        ComponentCondition<P3ApocalypseDarkWater>(id + 0x12, 0.6f, comp => comp.NumStatuses >= 6)
            .ActivateOnEnter<P3Apocalypse>()
            .ActivateOnEnter<P3ApocalypseDarkWater>()
            .ActivateOnEnter<P3ApocalypseAIWater1>()
            .ExecOnExit<P3ApocalypseDarkWater>(comp => comp.ShowOrder(1));
        ActorCast(id + 0x20, _module.BossP3, AID.Apocalypse, 2.6f, 4, true);
        ActorCastStart(id + 0x30, _module.BossP3, AID.SpiritTaker, 2.2f, true);
        ComponentCondition<P3ApocalypseDarkWater>(id + 0x31, 1.3f, comp => comp.Stacks.Count == 0, "Stack 1")
            .DeactivateOnExit<P3ApocalypseAIWater1>();
        ActorCastEnd(id + 0x32, _module.BossP3, 1.7f, true)
            .ActivateOnEnter<P3ApocalypseSpiritTaker>();
        ComponentCondition<SpiritTaker>(id + 0x33, 0.3f, comp => comp.Spreads.Count == 0, "Jump")
            .DeactivateOnExit<SpiritTaker>();
        ActorCastStart(id + 0x40, _module.BossP3, AID.ApocalypseDarkEruption, 6.2f, true)
            .ExecOnEnter<P3Apocalypse>(comp => comp.Show(8.5f))
            .ActivateOnEnter<P3ApocalypseDarkEruption>();
        ComponentCondition<P3Apocalypse>(id + 0x41, 2.4f, comp => comp.NumCasts >= 4, "Apocalypse start");
        ActorCastEnd(id + 0x42, _module.BossP3, 1.6f, true);
        ComponentCondition<P3Apocalypse>(id + 0x43, 0.4f, comp => comp.NumCasts >= 10);
        ComponentCondition<P3ApocalypseDarkEruption>(id + 0x44, 0.7f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<P3ApocalypseDarkEruption>()
            .ExecOnExit<P3ApocalypseDarkWater>(comp => comp.ShowOrder(2));
        ComponentCondition<P3Apocalypse>(id + 0x45, 1.3f, comp => comp.NumCasts >= 16)
            .ActivateOnEnter<P3ApocalypseAIWater2>();
        ActorCastStart(id + 0x50, _module.BossP3, AID.DarkestDance, 1.3f, true);
        ComponentCondition<P3Apocalypse>(id + 0x51, 0.7f, comp => comp.NumCasts >= 22);
        ComponentCondition<P3Apocalypse>(id + 0x52, 2.0f, comp => comp.NumCasts >= 28);
        ComponentCondition<P3ApocalypseDarkWater>(id + 0x53, 0.5f, comp => comp.Stacks.Count == 0, "Stack 2");
        ComponentCondition<P3Apocalypse>(id + 0x54, 1.5f, comp => comp.NumCasts >= 34)
            .ActivateOnEnter<P3DarkestDanceBait>()
            .DeactivateOnExit<P3Apocalypse>();
        ActorCastEnd(id + 0x55, _module.BossP3, 0.3f, true);
        ComponentCondition<P3DarkestDanceBait>(id + 0x56, 0.4f, comp => comp.NumCasts > 0, "Tankbuster")
            .ActivateOnEnter<P3DarkestDanceKnockback>()
            .DeactivateOnExit<P3ApocalypseAIWater2>()
            .DeactivateOnExit<P3DarkestDanceBait>()
            .ExecOnExit<P3ApocalypseDarkWater>(comp => comp.ShowOrder(3))
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P3DarkestDanceKnockback>(id + 0x57, 2.8f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<P3ApocalypseAIWater3>()
            .DeactivateOnExit<P3DarkestDanceKnockback>();
        ComponentCondition<P3ApocalypseDarkWater>(id + 0x60, 4.1f, comp => comp.Stacks.Count == 0, "Stack 3")
            .DeactivateOnExit<P3ApocalypseAIWater3>()
            .DeactivateOnExit<P3ApocalypseDarkWater>();

        P3ShockwavePulsar(id + 0x1000, 0.3f);
    }

    private void P3Enrage(uint id, float delay)
    {
        ActorCast(id, _module.BossP3, AID.MemorysEnd, delay, 10, true, "Enrage");
        ActorTargetable(id + 0x10, _module.BossP3, false, 3.5f, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void P4AkhRhai(uint id, float delay)
    {
        ActorTargetable(id, _module.BossP4Usurper, true, delay, "Usurper appears")
            .ActivateOnEnter<P4Preposition>()
            .ActivateOnEnter<P4FragmentOfFate>()
            .DeactivateOnExit<P4Preposition>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCastStart(id + 0x10, _module.BossP4Usurper, AID.Materialization, 5.1f, true)
            .ActivateOnEnter<P4AkhRhai>();
        ActorCastEnd(id + 0x11, _module.BossP4Usurper, 3, true);
        ComponentCondition<P4AkhRhai>(id + 0x20, 11.2f, comp => comp.AOEs.Count > 0, "Puddle baits");
        ComponentCondition<P4AkhRhai>(id + 0x30, 2.6f, comp => comp.NumCasts > 0);
        ActorTargetable(id + 0x50, _module.BossP4Oracle, true, 3.6f, "Oracle appears");
        ComponentCondition<P4AkhRhai>(id + 0x60, 1.6f, comp => comp.NumCasts >= 10 * comp.AOEs.Count, "Puddle resolve")
            .ActivateOnEnter<P4MornAfahHPCheck>()
            .DeactivateOnExit<P4AkhRhai>();
    }

    private void P4DarklitDragonsong(uint id, float delay)
    {
        ActorCast(id, _module.BossP4Usurper, AID.DarklitDragonsongUsurper, delay, 5, true, "Raidwide (darklit)")
            .ActivateOnEnter<P4DarklitDragonsong>()
            .ActivateOnEnter<P4DarklitDragonsongBrightHunger>()
            .ActivateOnEnter<P4DarklitDragonsongPathOfLight>()
            .ActivateOnEnter<P4DarklitDragonsongDarkWater>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCast(id + 0x10, _module.BossP4Usurper, AID.PathOfLight, 3.2f, 8);
        ActorCastStart(id + 0x20, _module.BossP4Oracle, AID.SpiritTaker, 0.1f, true, "Towers") // towers resolve right as cast starts
            .DeactivateOnExit<P4DarklitDragonsongBrightHunger>();
        ComponentCondition<P4DarklitDragonsongPathOfLight>(id + 0x21, 0.8f, comp => comp.NumCasts > 0, "Proteans")
            .DeactivateOnExit<P4DarklitDragonsongPathOfLight>();
        ActorCastEnd(id + 0x22, _module.BossP4Oracle, 2.2f, true)
            .ActivateOnEnter<P4DarklitDragonsongSpiritTaker>();
        ActorCastStartMulti(id + 0x23, _module.BossP4Usurper, [AID.HallowedWingsL, AID.HallowedWingsR], 0.1f, true);
        ComponentCondition<SpiritTaker>(id + 0x24, 0.3f, comp => comp.Spreads.Count == 0, "Jump")
            .DeactivateOnExit<SpiritTaker>();
        ActorCastStart(id + 0x25, _module.BossP4Oracle, AID.SomberDance, 2.8f)
            .ActivateOnEnter<P4HallowedWingsL>()
            .ActivateOnEnter<P4HallowedWingsR>()
            .ExecOnEnter<P4DarklitDragonsongDarkWater>(comp => comp.Show());
        ComponentCondition<P4DarklitDragonsongDarkWater>(id + 0x26, 1.7f, comp => comp.Stacks.Count == 0, "Stacks")
            .DeactivateOnExit<P4DarklitDragonsongDarkWater>();
        ActorCastEnd(id + 0x27, _module.BossP4Usurper, 0.2f, false, "Side cleave")
            .ActivateOnEnter<P4SomberDance>()
            .DeactivateOnExit<P4HallowedWingsL>()
            .DeactivateOnExit<P4HallowedWingsR>();
        ActorCastEnd(id + 0x28, _module.BossP4Oracle, 3.1f, true)
            .DeactivateOnExit<P4DarklitDragonsong>(); // tethers deactivate ~0.5s before cast end
        ComponentCondition<P4SomberDance>(id + 0x29, 0.4f, comp => comp.NumCasts > 0, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P4SomberDance>(id + 0x2A, 3.2f, comp => comp.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<P4SomberDance>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P4AkhMornMornAfah(uint id, float delay)
    {
        ActorCast(id, _module.BossP4Usurper, AID.AkhMornUsurper, delay, 4, true)
            .ActivateOnEnter<P4AkhMorn>();
        ComponentCondition<P4AkhMorn>(id + 0x10, 0.9f, comp => comp.NumCasts >= 1, "Party stack 1");
        ComponentCondition<P4AkhMorn>(id + 0x11, 1.1f, comp => comp.NumCasts >= 2, "Party stack 2");
        ComponentCondition<P4AkhMorn>(id + 0x12, 1.1f, comp => comp.NumCasts >= 3, "Party stack 3");
        ComponentCondition<P4AkhMorn>(id + 0x13, 1.1f, comp => comp.NumCasts >= 4, "Party stack 4")
            .DeactivateOnExit<P4AkhMorn>();

        ActorCast(id + 0x1000, _module.BossP4Usurper, AID.MornAfahUsurper, 0.1f, 6, true)
            .ActivateOnEnter<P4MornAfah>();
        ComponentCondition<P4MornAfah>(id + 0x1010, 0.9f, comp => comp.Stacks.Count == 0, "Raid stack + HP check")
            .DeactivateOnExit<P4MornAfah>();
    }

    private void P4CrystallizeTime(uint id, float delay)
    {
        ActorCast(id, _module.BossP4Oracle, AID.CrystallizeTimeOracle, delay, 10, true, "Raidwide (crystallize)")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x10, _module.BossP4Usurper, false, 3.1f, "Usurper disappears")
            .ActivateOnEnter<P4CrystallizeTime>()
            .ActivateOnEnter<P4CrystallizeTimeDragonHead>();
        ActorTargetable(id + 0x11, _module.BossP4Oracle, false, 1.1f, "Oracle disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorCast(id + 0x20, _module.BossP4Oracle, AID.UltimateRelativitySpeed, 0.1f, 5.5f, true)
            .ActivateOnEnter<P4CrystallizeTimeMaelstrom>()
            .ActivateOnEnter<P4CrystallizeTimeDarkWater>()
            .ActivateOnEnter<P4CrystallizeTimeHints>();
        ComponentCondition<P4CrystallizeTimeMaelstrom>(id + 0x30, 2.2f, comp => comp.NumCasts > 0, "Hourglass 1");
        ComponentCondition<P4CrystallizeTimeDarkWater>(id + 0x31, 1.0f, comp => comp.Stacks.Count == 0)
            .ActivateOnEnter<P4CrystallizeTimeDarkEruption>()
            .ActivateOnEnter<P3UltimateRelativityDarkBlizzard>()
            .ActivateOnEnter<P4CrystallizeTimeDarkAero>()
            .DeactivateOnExit<P4CrystallizeTimeDarkWater>();
        ComponentCondition<P4CrystallizeTimeDarkEruption>(id + 0x32, 2.0f, comp => comp.NumCasts > 0, "Knockbacks") // aero + eruption + blizzard donuts resolve at the same time
            .DeactivateOnExit<P4CrystallizeTimeDarkEruption>()
            .DeactivateOnExit<P3UltimateRelativityDarkBlizzard>()
            .DeactivateOnExit<P4CrystallizeTimeDarkAero>();
        ComponentCondition<P4CrystallizeTimeMaelstrom>(id + 0x33, 2.5f, comp => comp.NumCasts > 2, "Hourglass 2")
            .ActivateOnEnter<P4CrystallizeTimeUnholyDarkness>();
        ComponentCondition<P4CrystallizeTimeUnholyDarkness>(id + 0x34, 0.5f, comp => comp.Stacks.Count == 0)
            .DeactivateOnExit<P4CrystallizeTimeUnholyDarkness>();
        ActorCast(id + 0x40, _module.BossP4Usurper, AID.TidalLight, 0.5f, 3, true, "Exaline EW start")
            .ActivateOnEnter<P4CrystallizeTimeTidalLight>();
        ComponentCondition<P4CrystallizeTimeMaelstrom>(id + 0x50, 1.1f, comp => comp.NumCasts > 4, "Hourglass 3")
            .DeactivateOnExit<P4CrystallizeTimeMaelstrom>()
            .DeactivateOnExit<P4CrystallizeTimeHints>();
        ActorCast(id + 0x60, _module.BossP4Usurper, AID.TidalLight, 2.3f, 3, true, "Exaline NS start")
            .ActivateOnEnter<P4CrystallizeTimeRewind>();
        ComponentCondition<P4CrystallizeTimeQuietus>(id + 0x70, 4.1f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<P4CrystallizeTimeQuietus>()
            .DeactivateOnExit<P4CrystallizeTimeQuietus>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P4CrystallizeTimeRewind>(id + 0x80, 1.9f, comp => comp.RewindDone, "Rewind place")
            .DeactivateOnExit<P4CrystallizeTimeTidalLight>()
            .DeactivateOnExit<P4CrystallizeTimeDragonHead>()
            .DeactivateOnExit<P4CrystallizeTime>();
        ActorCastStart(id + 0x90, _module.BossP4Oracle, AID.SpiritTaker, 0.4f);
        ActorCastStart(id + 0x91, _module.BossP4Usurper, AID.CrystallizeTimeHallowedWings1, 2.2f)
            .ActivateOnEnter<P4CrystallizeTimeSpiritTaker>();
        ActorCastEnd(id + 0x92, _module.BossP4Oracle, 0.8f);
        ComponentCondition<SpiritTaker>(id + 0x93, 0.3f, comp => comp.Spreads.Count == 0, "Jump")
            .DeactivateOnExit<SpiritTaker>();
        ComponentCondition<P4CrystallizeTimeRewind>(id + 0x94, 3.3f, comp => comp.ReturnDone, "Rewind return")
            .DeactivateOnExit<P4CrystallizeTimeRewind>();
        ActorCastEnd(id + 0x95, _module.BossP4Usurper, 0.3f);
        ActorCast(id + 0xA0, _module.BossP4Usurper, AID.CrystallizeTimeHallowedWingsAOE, 1.4f, 0.5f, true);
        ActorCast(id + 0xB0, _module.BossP4Usurper, AID.CrystallizeTimeHallowedWings2, 2.1f, 0.5f, true);
        ActorCast(id + 0xC0, _module.BossP4Usurper, AID.CrystallizeTimeHallowedWingsAOE, 1.4f, 0.5f, true);
        ActorTargetable(id + 0xD0, _module.BossP4Usurper, true, 5.3f, "Bosses reappear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P4Enrage(uint id, float delay)
    {
        ActorCast(id, _module.BossP4Usurper, AID.AbsoluteZeroP4, delay, 10, true, "Enrage");
    }

    private void P5Start(uint id, float delay)
    {
        ActorTargetable(id, _module.BossP5, true, delay, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P5FulgentBlade(uint id, float delay)
    {
        ActorCast(id, _module.BossP5, AID.FulgentBlade, delay, 6, true, "Raidwide")
            .ActivateOnEnter<P5FulgentBlade>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P5FulgentBlade>(id + 0x10, 4.1f, comp => comp.Active);
        ComponentCondition<P5FulgentBlade>(id + 0x20, 7, comp => comp.NumCasts > 0, "Exaline 1");
        ComponentCondition<P5FulgentBlade>(id + 0x21, 2, comp => comp.NumCasts > 1, "Exaline 2");
        ComponentCondition<P5FulgentBlade>(id + 0x22, 2, comp => comp.NumCasts > 2, "Exaline 3");
        ComponentCondition<P5FulgentBlade>(id + 0x23, 2, comp => comp.NumCasts > 3, "Exaline 4");
        ActorCastStart(id + 0x30, _module.BossP5, AID.AkhMornPandora, 1.8f, true);
        ComponentCondition<P5FulgentBlade>(id + 0x31, 0.2f, comp => comp.NumCasts > 4, "Exaline 5")
            .ActivateOnEnter<P5AkhMorn>();
        ComponentCondition<P5FulgentBlade>(id + 0x32, 2, comp => comp.NumCasts > 5, "Exaline 6");
        ActorCastEnd(id + 0x33, _module.BossP5, 5.8f, true);
        ComponentCondition<P5AkhMorn>(id + 0x34, 0.1f, comp => comp.Source == null, "Left/right stack")
            .DeactivateOnExit<P5AkhMorn>()
            .DeactivateOnExit<P5FulgentBlade>(); // TODO: there are still lines going, but they are far...
    }

    private void P5ParadiseRegained(uint id, float delay)
    {
        ActorCast(id, _module.BossP5, AID.ParadiseRegained, delay, 4, true)
            .ActivateOnEnter<P5ParadiseRegainedTowers>(); // first tower appears ~0.9s after cast end, then every 3.5s
        ActorCastMulti(id + 0x10, _module.BossP5, [AID.WingsDarkAndLightDL, AID.WingsDarkAndLightLD], 3.2f, 6.9f, true)
            .ActivateOnEnter<P5ParadiseRegainedBaits>();
        ComponentCondition<P5ParadiseRegainedBaits>(id + 0x20, 0.3f, comp => comp.NumCasts > 0, "Light/dark"); // first tower resolves at the same time
        ComponentCondition<P5ParadiseRegainedBaits>(id + 0x30, 3.7f, comp => comp.NumCasts > 1, "Dark/light") // second tower resolves at the same time
            .DeactivateOnExit<P5ParadiseRegainedBaits>();
        // note: tethers resolve ~0.8s after cleave, but they won't happen if tether target dies to cleave
        ComponentCondition<P5ParadiseRegainedTowers>(id + 0x40, 3.4f, comp => comp.NumCasts > 2, "Towers resolve")
            .DeactivateOnExit<P5ParadiseRegainedTowers>();
    }

    private void P5PolarizingStrikes(uint id, float delay)
    {
        ActorCast(id, _module.BossP5, AID.PolarizingStrikes, delay, 6.5f, true)
            .ActivateOnEnter<P5PolarizingStrikes>();
        ComponentCondition<P5PolarizingStrikes>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Polarizing bait 1");
        ActorCastStart(id + 0x20, _module.BossP5, AID.PolarizingPaths, 1.5f, true);
        ComponentCondition<P5PolarizingStrikes>(id + 0x21, 0.5f, comp => comp.NumCasts > 2, "Polarizing AOE 1");
        ActorCastEnd(id + 0x22, _module.BossP5, 2, true);
        ComponentCondition<P5PolarizingStrikes>(id + 0x30, 0.6f, comp => comp.NumCasts > 4, "Polarizing bait 2");
        ActorCastStart(id + 0x40, _module.BossP5, AID.PolarizingPaths, 1.5f, true);
        ComponentCondition<P5PolarizingStrikes>(id + 0x41, 0.5f, comp => comp.NumCasts > 6, "Polarizing AOE 2");
        ActorCastEnd(id + 0x42, _module.BossP5, 2, true);
        ComponentCondition<P5PolarizingStrikes>(id + 0x50, 0.6f, comp => comp.NumCasts > 8, "Polarizing bait 3");
        ActorCastStart(id + 0x60, _module.BossP5, AID.PolarizingPaths, 1.5f, true);
        ComponentCondition<P5PolarizingStrikes>(id + 0x61, 0.5f, comp => comp.NumCasts > 10, "Polarizing AOE 3");
        ActorCastEnd(id + 0x62, _module.BossP5, 2, true);
        ComponentCondition<P5PolarizingStrikes>(id + 0x70, 0.6f, comp => comp.NumCasts > 12, "Polarizing bait 4");
        ComponentCondition<P5PolarizingStrikes>(id + 0x80, 2.0f, comp => comp.NumCasts > 14, "Polarizing AOE 4")
            .DeactivateOnExit<P5PolarizingStrikes>();
    }

    private void P5PandorasBox(uint id, float delay)
    {
        ActorCast(id, _module.BossP5, AID.PandorasBox, delay, 12, true, "Tank LB")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P5Enrage(uint id, float delay)
    {
        ActorCast(id, _module.BossP5, AID.ParadiseLostP5, delay, 12, true);
        ComponentCondition<P5ParadiseLost>(id + 2, 9.5f, comp => comp.NumCasts > 0, "Enrage")
            .ActivateOnEnter<P5ParadiseLost>()
            .DeactivateOnExit<P5ParadiseLost>();
    }
}
