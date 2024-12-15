namespace BossMod.Dawntrail.Ultimate.FRU;

class FRUStates : StateMachineBuilder
{
    private readonly FRU _module;

    public FRUStates(FRU module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1: Fatebreaker")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed;
        SimplePhase(1, Phase2, "P2: Usurper of Frost")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => !Module.PrimaryActor.IsDead || (_module.BossP2()?.IsDeadOrDestroyed ?? false) || (_module.IceVeil()?.IsDeadOrDestroyed ?? false);
        SimplePhase(2, Phase3, "P3: Oracle of Darkness")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => !Module.PrimaryActor.IsDead || (_module.BossP2()?.IsDeadOrDestroyed ?? false) && (_module.BossP3()?.IsDeadOrDestroyed ?? true);
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

    private void Phase3(uint id)
    {
        P3JunctionHellsJudgment(id, 13.3f);
        P3UltimateRelativity(id + 0x10000, 4.3f);
        P3BlackHalo(id + 0x20000, 3.2f);
        P3Apocalypse(id + 0x30000, 7.2f);
        ActorCast(id + 0x40000, _module.BossP3, AID.MemorysEnd, 3.7f, 10, true, "Enrage");
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
            .DeactivateOnExit<P1TurnOfHeavensBurntStrikeLightning>();
        ComponentCondition<P1TurnOfHeavensBurnout>(id + 0x10, 1.7f, comp => comp.NumCasts > 0, "Line 2")
            .DeactivateOnExit<P1TurnOfHeavensBurnout>();
        ComponentCondition<P1BrightfireSmall>(id + 0x11, 0.4f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P1TurnOfHeavensBurntStrikeFire>()
            .ActivateOnEnter<P1Blastburn>()
            .ActivateOnEnter<P1BrightfireSmall>()
            .ActivateOnEnter<P1BrightfireLarge>();
        ComponentCondition<P1TurnOfHeavensBurntStrikeFire>(id + 0x12, 3.9f, comp => comp.NumCasts > 0, "Line 3")
            .DeactivateOnExit<P1TurnOfHeavensBurntStrikeFire>();
        ComponentCondition<P1Blastburn>(id + 0x13, 2, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<P1Blastburn>();
        ComponentCondition<P1BrightfireSmall>(id + 0x20, 2.1f, comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<P1BrightfireSmall>()
            .DeactivateOnExit<P1BrightfireLarge>();
        ComponentCondition<P1BoundOfFaith>(id + 0x30, 4, comp => !comp.Active, "Stacks") // note: won't happen if both targets die, but that's a wipe anyway
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
            .ActivateOnEnter<P1PowderMarkTrail>()
            .ActivateOnEnter<P1ExplosionBurntStrikeFire>()
            .ActivateOnEnter<P1ExplosionBurntStrikeLightning>()
            .ActivateOnEnter<P1ExplosionBurnout>()
            .ActivateOnEnter<P1Blastburn>()
            .DeactivateOnExit<P1ExplosionBurntStrikeFire>()
            .DeactivateOnExit<P1ExplosionBurntStrikeLightning>();
        Condition(id + 0x102, 2, () => Module.FindComponent<P1ExplosionBurnout>()?.NumCasts > 0 || Module.FindComponent<P1Blastburn>()?.NumCasts > 0, "Line/Knockback", checkDelay: 2) // note: kb and wide line have slightly different cast time...
            .DeactivateOnExit<P1ExplosionBurnout>()
            .DeactivateOnExit<P1Blastburn>();
        ComponentCondition<P1Explosion>(id + 0x103, 2, comp => comp.NumCasts > 0, "Towers")
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
            .DeactivateOnExit<P2DiamondDustHouseOfLight>();
        ComponentCondition<P2FrigidStone>(id + 0x32, 1.6f, comp => comp.NumCasts > 0, "Ice baits")
            .DeactivateOnExit<P2FrigidStone>()
            .DeactivateOnExit<P2DiamondDustSafespots>();
        ComponentCondition<P2IcicleImpact>(id + 0x33, 0.4f, comp => comp.NumCasts > 0, "Ice circle 1");
        ComponentCondition<P2HeavenlyStrike>(id + 0x40, 3.9f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<P2FrigidNeedleCircle>()
            .ActivateOnEnter<P2FrigidNeedleCross>()
            .ActivateOnEnter<P2SinboundHoly>()
            .ActivateOnEnter<P2HeavenlyStrike>()
            .ActivateOnEnter<P2TwinStillnessSilence>() // show the cone caster early, to simplify finding movement direction...
            .DeactivateOnExit<P2HeavenlyStrike>();
        ComponentCondition<P2FrigidNeedleCross>(id + 0x50, 2.8f, comp => comp.NumCasts > 0, "Stars")
            .DeactivateOnExit<P2FrigidNeedleCircle>()
            .DeactivateOnExit<P2FrigidNeedleCross>();
        ComponentCondition<P2SinboundHoly>(id + 0x60, 1.3f, comp => comp.NumCasts > 0);
        ComponentCondition<P2SinboundHoly>(id + 0x70, 4.7f, comp => comp.NumCasts >= 4)
            .ActivateOnEnter<P2SinboundHolyVoidzone>()
            .ActivateOnEnter<P2ShiningArmor>()
            .DeactivateOnExit<P2IcicleImpact>() // last icicle explodes together with first stack
            .DeactivateOnExit<P2SinboundHoly>();
        ComponentCondition<P2ShiningArmor>(id + 0x80, 3.7f, comp => comp.NumCasts > 0, "Gaze")
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
        ComponentCondition<P2MirrorMirrorHouseOfLight>(id + 0x21, 0.6f, comp => comp.NumCasts > 1, "Mirror 2")
            .DeactivateOnExit<P2MirrorMirrorHouseOfLight>();

        ActorCastMulti(id + 0x100, _module.BossP2, [AID.BanishStack, AID.BanishSpread], 0.5f, 5, true)
            .ActivateOnEnter<P2Banish>();
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
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P2LuminousHammer>(id + 0x20, 4.8f, comp => comp.NumCasts > 0, "Puddle bait");
        ComponentCondition<P2BrightHunger1>(id + 0x30, 3.3f, comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<P2SinboundHolyVoidzone>()
            .DeactivateOnExit<P2BrightHunger1>();
        ComponentCondition<P2PowerfulLight>(id + 0x40, 5.7f, comp => !comp.Active, "Stack")
            .ActivateOnEnter<P2HolyLightBurst>()
            .ActivateOnEnter<P2PowerfulLight>()
            .DeactivateOnExit<P2LuminousHammer>() // last puddle is baited right before holy light burst casts start
            .DeactivateOnExit<P2PowerfulLight>();
        ComponentCondition<P2HolyLightBurst>(id + 0x50, 2.4f, comp => comp.NumCasts > 0, "Orbs 1")
            .ActivateOnEnter<P2BrightHunger2>();
        ComponentCondition<P2HolyLightBurst>(id + 0x60, 3, comp => comp.NumCasts > 3, "Orbs 2")
            .DeactivateOnExit<P2HolyLightBurst>()
            .DeactivateOnExit<P2LightRampant>(); // tethers resolve right after first orbs

        ActorCastStartMulti(id + 0x70, _module.BossP2, [AID.BanishStack, AID.BanishSpread], 1.7f, true);
        ComponentCondition<P2BrightHunger2>(id + 0x71, 1.9f, comp => comp.NumCasts > 0, "Central tower")
            .ActivateOnEnter<P2Banish>()
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

        ComponentCondition<P2CrystalOfLight>(id + 0x1000, 18.9f, comp => comp.ActiveActors.Any(), "Crystals appear")
            .ActivateOnEnter<P2CrystalOfLight>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCast(id + 0x1010, _module.IceVeil, AID.EndlessIceAge, 4.7f, 40, true, "Enrage")
            .ActivateOnEnter<P2SinboundBlizzard>()
            .ActivateOnEnter<P2HiemalStorm>()
            .ActivateOnEnter<P2HiemalRay>()
            .DeactivateOnExit<P2CrystalOfLight>()
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
            .ExecOnExit<P3ApocalypseDarkWater>(comp => comp.ShowOrder(1));
        ActorCast(id + 0x20, _module.BossP3, AID.Apocalypse, 2.6f, 4, true);
        ActorCastStart(id + 0x30, _module.BossP3, AID.SpiritTaker, 2.2f, true);
        ComponentCondition<P3ApocalypseDarkWater>(id + 0x31, 1.3f, comp => comp.Stacks.Count == 0, "Stack 1");
        ActorCastEnd(id + 0x32, _module.BossP3, 1.7f, true)
            .ActivateOnEnter<P3SpiritTaker>();
        ComponentCondition<P3SpiritTaker>(id + 0x33, 0.3f, comp => comp.Spreads.Count == 0, "Jump")
            .DeactivateOnExit<P3SpiritTaker>();
        ActorCastStart(id + 0x40, _module.BossP3, AID.ApocalypseDarkEruption, 6.2f, true)
            .ExecOnEnter<P3Apocalypse>(comp => comp.Show(8.5f))
            .ActivateOnEnter<P3ApocalypseDarkEruption>();
        ComponentCondition<P3Apocalypse>(id + 0x41, 2.4f, comp => comp.NumCasts > 0, "Apocalypse start");
        ActorCastEnd(id + 0x42, _module.BossP3, 1.6f, true);
        ComponentCondition<P3Apocalypse>(id + 0x43, 0.4f, comp => comp.NumCasts > 4);
        ComponentCondition<P3ApocalypseDarkEruption>(id + 0x44, 0.7f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<P3ApocalypseDarkEruption>()
            .ExecOnExit<P3ApocalypseDarkWater>(comp => comp.ShowOrder(2));
        ComponentCondition<P3Apocalypse>(id + 0x45, 1.3f, comp => comp.NumCasts > 10);
        ActorCastStart(id + 0x50, _module.BossP3, AID.DarkestDance, 1.3f, true);
        ComponentCondition<P3Apocalypse>(id + 0x51, 0.7f, comp => comp.NumCasts > 16);
        ComponentCondition<P3Apocalypse>(id + 0x52, 2.0f, comp => comp.NumCasts > 22);
        ComponentCondition<P3ApocalypseDarkWater>(id + 0x53, 0.5f, comp => comp.Stacks.Count == 0, "Stack 2");
        ComponentCondition<P3Apocalypse>(id + 0x54, 1.5f, comp => comp.NumCasts > 28)
            .ActivateOnEnter<P3DarkestDanceBait>()
            .DeactivateOnExit<P3Apocalypse>();
        ActorCastEnd(id + 0x55, _module.BossP3, 0.3f, true);
        ComponentCondition<P3DarkestDanceBait>(id + 0x56, 0.4f, comp => comp.NumCasts > 0, "Tankbuster")
            .ActivateOnEnter<P3DarkestDanceKnockback>()
            .DeactivateOnExit<P3DarkestDanceBait>()
            .ExecOnExit<P3ApocalypseDarkWater>(comp => comp.ShowOrder(3))
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P3DarkestDanceKnockback>(id + 0x57, 2.8f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<P3DarkestDanceKnockback>();
        ComponentCondition<P3ApocalypseDarkWater>(id + 0x60, 4.1f, comp => comp.Stacks.Count == 0, "Stack 3")
            .DeactivateOnExit<P3ApocalypseDarkWater>();

        P3ShockwavePulsar(id + 0x1000, 0.3f);
    }
}
