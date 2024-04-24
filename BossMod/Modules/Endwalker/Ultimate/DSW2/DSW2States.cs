namespace BossMod.Endwalker.Ultimate.DSW2;

class DSW2States : StateMachineBuilder
{
    private readonly DSW2 _module;

    private bool IsReset => Module.PrimaryActor.IsDestroyed && (_module.ArenaFeatures?.IsDestroyed ?? true);
    private bool IsResetOrRewindFailed => IsReset || Module.Enemies(OID.BossP2).Any();
    private bool IsDead(Actor? actor) => actor != null && actor.IsDeadOrDestroyed;
    private bool IsEffectivelyDead(Actor? actor) => actor != null && (actor.IsDeadOrDestroyed || !actor.IsTargetable && actor.HPMP.CurHP <= 1);

    public DSW2States(DSW2 module) : base(module)
    {
        _module = module;
        SimplePhase(1, Phase2Thordan, "P2: Thordan") // TODO: auto-attack cleave component
            .Raw.Update = () => IsReset || Module.PrimaryActor.IsDead;
        SimplePhase(2, Phase3Nidhogg, "P3: Nidhogg") // TODO: auto-attack cleave component
            .OnEnter(() => Module.Arena.Bounds = DSW2.BoundsSquare)
            .Raw.Update = () => IsReset || IsDead(_module.BossP3());
        SimplePhase(3, Phase4Eyes, "P4: Eyes")
            .Raw.Update = () => IsReset || IsDead(_module.LeftEyeP4()) && IsDead(_module.RightEyeP4()) && IsDead(_module.NidhoggP4());
        SimplePhase(4, Phase4Intermission, "P4: Intermission")
            .OnEnter(() => Module.Arena.Bounds = DSW2.BoundsCircle)
            .Raw.Update = () => IsResetOrRewindFailed || IsDead(_module.Spear());
        SimplePhase(5, Phase5KingThordan, "P5: King Thordan") // TODO: auto-attack cleave component
            .ActivateOnEnter<P5Surrender>()
            .Raw.Update = () => IsResetOrRewindFailed || _module.FindComponent<P5Surrender>()?.NumCasts > 0;
        SimplePhase(6, Phase6Dragons, "P6: Nidhogg + Hraesvelgr")
            .OnEnter(() => Module.Arena.Bounds = DSW2.BoundsSquare)
            .Raw.Update = () => IsResetOrRewindFailed || IsEffectivelyDead(_module.NidhoggP6()) && IsEffectivelyDead(_module.HraesvelgrP6());
        SimplePhase(7, Phase7DragonKingThordan, "P7: DKT")
            .Raw.Update = () => IsResetOrRewindFailed || IsDead(_module.BossP7());
    }

    private void Phase2Thordan(uint id)
    {
        P2AscalonsMercyConcealedMight(id, 8.4f);
        P2StrengthOfTheWard(id + 0x10000, 7.1f);
        P2AncientQuaga(id + 0x20000, 0.1f);
        P2HeavenlyHeelAscalonMight(id + 0x30000, 6.2f);
        P2SanctityOfTheWard(id + 0x40000, 7.1f);
        P2UltimateEnd(id + 0x50000, 13.5f);
        P2BroadSwing(id + 0x60000, 6.0f);
        P2BroadSwing(id + 0x70000, 2.8f);
        Cast(id + 0x80000, AID.AethericBurstP2, 2.4f, 6, "Enrage");
    }

    private void Phase3Nidhogg(uint id)
    {
        P3FinalChorus(id);
        P3Dives(id + 0x10000, 13.2f);
        P3Drachenlance(id + 0x20000, 2.1f);
        P3SoulTether(id + 0x30000, 1.5f);
        P3Drachenlance(id + 0x40000, 21.1f);
        ActorCast(id + 0x50000, _module.BossP3, AID.RevengeOfTheHordeP3, 1.5f, 11, true, "Enrage");
    }

    private void Phase4Eyes(uint id)
    {
        P4SoulOfFriendshipDevotion(id);
        P4Hatebound(id + 0x10000, 5.4f);
        P4SteepInRage(id + 0x20000, 3.8f);
    }

    private void Phase4Intermission(uint id)
    {
        P4IntermissionCharibert(id);
        ActorCast(id + 0x10000, _module.Spear, AID.Pierce, 2.7f, 11, true, "Enrage");
    }

    private void Phase5KingThordan(uint id)
    {
        P5Start(id);
        P5WrathOfHeavens(id + 0x10000, 0.1f);
        P5HeavenlyHeelAscalonMight(id + 0x20000, 6.4f);
        P5DeathOfTheHeavens(id + 0x30000, 7.1f);
        P5AncientQuaga(id + 0x40000, 2.1f);
        P5HeavenlyHeelAscalonMight(id + 0x50000, 6.2f);
        ActorCast(id + 0x60000, _module.BossP5, AID.AethericBurstP5, 4.8f, 6, true, "Enrage");
    }

    private void Phase6Dragons(uint id)
    {
        P6Start(id);
        P6Wyrmsbreath1(id + 0x10000, 11.4f);
        P6MortalVow(id + 0x20000, 7.2f);
        P6AkhAfah(id + 0x30000, 3.1f);
        P6HallowedWingsPlume1(id + 0x40000, 3.8f);
        P6WrothFlames(id + 0x50000, 3.0f);
        P6AkhAfah(id + 0x60000, 4.2f);
        P6HallowedWingsPlume2(id + 0x70000, 4.5f);
        P6Wyrmsbreath2(id + 0x80000, 3.9f);
        P6Touchdown(id + 0x90000, 5.0f);
    }

    private void Phase7DragonKingThordan(uint id)
    {
        P7Start(id);
        P7ExaflareEdge(id + 0x10000, 4.2f);
        P7AkhMornsEdge(id + 0x20000, 2.0f, 5);
        P7GigaflaresEdge(id + 0x30000, 1.9f);
        P7ExaflareEdge(id + 0x40000, 2.0f);
        P7AkhMornsEdge(id + 0x50000, 2.0f, 6);
        P7GigaflaresEdge(id + 0x60000, 1.9f);
        P7ExaflareEdge(id + 0x70000, 2.0f);
        P7AkhMornsEdge(id + 0x80000, 2.0f, 7);
        P7MornAfahsEdge(id + 0x90000, 1.9f);
    }

    private void P2AscalonsMercyConcealedMight(uint id, float delay)
    {
        Cast(id, AID.AscalonsMercyConcealed, delay, 3)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<P2AscalonsMercyConcealed>(id + 2, 1.6f, comp => comp.NumCasts > 0, "Baited cones")
            .ActivateOnEnter<P2AscalonsMercyConcealed>()
            .DeactivateOnExit<P2AscalonsMercyConcealed>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        ComponentCondition<P2AscalonMight>(id + 0x1000, 4.9f, comp => comp.NumCasts > 2, "3x tankbuster cones")
            .ActivateOnEnter<P2AscalonMight>()
            .DeactivateOnExit<P2AscalonMight>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2StrengthOfTheWard(uint id, float delay)
    {
        Cast(id, AID.StrengthOfTheWard, delay, 4);
        Targetable(id + 0x10, false, 3.1f, "Trio 1");
        CastStart(id + 0x20, AID.LightningStorm, 3.6f)
            .ActivateOnEnter<P2StrengthOfTheWard1LightningStorm>()
            .ActivateOnEnter<P2StrengthOfTheWard1SpiralThrust>()
            .ActivateOnEnter<P2StrengthOfTheWard1HeavyImpact>();
        CastEnd(id + 0x21, 5.7f);
        ComponentCondition<P2StrengthOfTheWard1HeavyImpact>(id + 0x30, 0.3f, comp => comp.NumCasts > 0, "Charges + Ring 1")
            .DeactivateOnExit<P2StrengthOfTheWard1SpiralThrust>();
        ComponentCondition<P2StrengthOfTheWard1HeavyImpact>(id + 0x40, 1.9f, comp => comp.NumCasts > 1, "Ring 2")
            .DeactivateOnExit<P2StrengthOfTheWard1LightningStorm>(); // event happens ~0.2s after previous state
        ComponentCondition<P2StrengthOfTheWard1HeavyImpact>(id + 0x60, 1.9f, comp => comp.NumCasts > 2, "Ring 3");
        ComponentCondition<P2StrengthOfTheWard1HeavyImpact>(id + 0x80, 1.9f, comp => comp.NumCasts > 3, "Ring 4")
            .ActivateOnEnter<P2AscalonsMercyConcealed>();
        ComponentCondition<P2AscalonsMercyConcealed>(id + 0x90, 1, comp => comp.NumCasts > 0, "Baited cones")
            .DeactivateOnExit<P2AscalonsMercyConcealed>();
        ComponentCondition<P2StrengthOfTheWard1HeavyImpact>(id + 0xA0, 0.9f, comp => comp.NumCasts > 4, "Ring 5")
            .ActivateOnEnter<P2StrengthOfTheWard2SpreadStack>() // note: PATE 1E43 happens right after ring-2, could start showing something (boss/charging mobs/?) much earlier
            .ActivateOnEnter<P2StrengthOfTheWard2Voidzones>()
            .ActivateOnEnter<P2StrengthOfTheWard2Charges>()
            .DeactivateOnExit<P2StrengthOfTheWard1HeavyImpact>();
        ComponentCondition<P2StrengthOfTheWard2SpreadStack>(id + 0x100, 9, comp => comp.LeapsDone && comp.RageDone, "Void zones + Leaps + Charges + Stacks")
            .DeactivateOnExit<P2StrengthOfTheWard2SpreadStack>()
            .DeactivateOnExit<P2StrengthOfTheWard2Voidzones>()
            .DeactivateOnExit<P2StrengthOfTheWard2Charges>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster);
        ComponentCondition<P2StrengthOfTheWard2Towers>(id + 0x110, 3.3f, comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<P2StrengthOfTheWard2Towers>()
            .DeactivateOnExit<P2StrengthOfTheWard2Towers>();
        Targetable(id + 0x120, true, 1.7f, "Reappear");
    }

    private void P2AncientQuaga(uint id, float delay)
    {
        Cast(id, AID.AncientQuaga, delay, 6, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2HeavenlyHeelAscalonMight(uint id, float delay)
    {
        Cast(id, AID.HeavenlyHeel, delay, 4, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P2AscalonMight>(id + 0x1000, 6.5f, comp => comp.NumCasts > 2, "3x tankbuster cones")
            .ActivateOnEnter<P2AscalonMight>()
            .DeactivateOnExit<P2AscalonMight>();
    }

    private void P2SanctityOfTheWard(uint id, float delay)
    {
        Cast(id, AID.SanctityofTheWard, delay, 4);
        Targetable(id + 0x10, false, 3.1f, "Trio 2");
        CastStart(id + 0x20, AID.DragonsGaze, 5.6f)
            .ActivateOnEnter<P2SanctityOfTheWard1Gaze>()
            .ActivateOnEnter<P2SanctityOfTheWard1Sever>()
            .ActivateOnEnter<P2SanctityOfTheWard1Flares>()
            .ActivateOnEnter<P2SanctityOfTheWard1Hints>();
        CastEnd(id + 0x21, 4);
        ComponentCondition<P2SanctityOfTheWard1Gaze>(id + 0x30, 1.2f, comp => comp.NumCasts > 0, "Gazes")
            .DeactivateOnExit<P2SanctityOfTheWard1Gaze>();
        ComponentCondition<P2SanctityOfTheWard1Flares>(id + 0x40, 6.1f, comp => comp.NumCasts >= 18, "Charges")
            .DeactivateOnExit<P2SanctityOfTheWard1Hints>()
            .DeactivateOnExit<P2SanctityOfTheWard1Flares>()
            .DeactivateOnExit<P2SanctityOfTheWard1Sever>();

        ComponentCondition<P2SanctityOfTheWard2HiemalStorm>(id + 0x100, 11.9f, comp => comp.NumCasts > 0, "Storms")
            .ActivateOnEnter<P2SanctityOfTheWard2HeavensStakeCircles>()
            .ActivateOnEnter<P2SanctityOfTheWard2HeavensStakeDonut>()
            .ActivateOnEnter<P2SanctityOfTheWard2HiemalStorm>()
            .ActivateOnEnter<P2SanctityOfTheWard2Towers1>()
            .DeactivateOnExit<P2SanctityOfTheWard2HeavensStakeCircles>()
            .DeactivateOnExit<P2SanctityOfTheWard2HeavensStakeDonut>()
            .DeactivateOnExit<P2SanctityOfTheWard2HiemalStorm>();
        ComponentCondition<P2SanctityOfTheWard2Towers1>(id + 0x110, 4.2f, comp => comp.NumCasts > 0, "Towers 1")
            .ActivateOnEnter<P2SanctityOfTheWard2VoidzoneFire>()
            .ActivateOnEnter<P2SanctityOfTheWard2VoidzoneIce>()
            .ActivateOnEnter<P2SanctityOfTheWard2Towers2>();
        ComponentCondition<P2SanctityOfTheWard2Knockback>(id + 0x120, 10.4f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<P2SanctityOfTheWard2Knockback>()
            .DeactivateOnExit<P2SanctityOfTheWard2Knockback>()
            .DeactivateOnExit<P2SanctityOfTheWard2VoidzoneFire>()
            .DeactivateOnExit<P2SanctityOfTheWard2VoidzoneIce>();
        ComponentCondition<P2SanctityOfTheWard2Towers2>(id + 0x130, 3, comp => comp.NumCasts > 0, "Towers 2")
            .DeactivateOnExit<P2SanctityOfTheWard2Towers1>() // TODO: reconsider...
            .DeactivateOnExit<P2SanctityOfTheWard2Towers2>();

        Targetable(id + 0x200, true, 4.5f, "Reappear");
    }

    private void P2UltimateEnd(uint id, float delay)
    {
        ComponentCondition<P2UltimateEnd>(id, delay, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P2UltimateEnd>()
            .DeactivateOnExit<P2UltimateEnd>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2Discomposed>(id + 1, 3.5f, comp => comp.Applied, "Vulnerable")
            .ActivateOnEnter<P2Discomposed>()
            .DeactivateOnExit<P2Discomposed>()
            .SetHint(StateMachine.StateHint.VulnerableStart);
    }

    private void P2BroadSwing(uint id, float delay)
    {
        CastMulti(id, new AID[] { AID.BroadSwingLR, AID.BroadSwingRL }, delay, 3)
            .ActivateOnEnter<P2BroadSwing>();
        ComponentCondition<P2BroadSwing>(id + 2, 2.8f, comp => comp.NumCasts >= 3, "Swings")
            .DeactivateOnExit<P2BroadSwing>();
    }

    private void P3FinalChorus(uint id)
    {
        Timeout(id, 0)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorTargetable(id + 1, _module.BossP3, true, 9.2f, "Raidwide + Reappear")
            .SetHint(StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide); // final chorus raidwide happens together with boss becoming targetable
    }

    private void P3Dives(uint id, float delay)
    {
        ActorCast(id, _module.BossP3, AID.DiveFromGrace, delay, 5, true, "Dive start")
            .ActivateOnEnter<P3DiveFromGrace>()
            .ActivateOnEnter<P3Geirskogul>();

        ActorCastMulti(id + 0x10, _module.BossP3, new AID[] { AID.GnashAndLash, AID.LashAndGnash }, 2.1f, 7.6f, true, "Stack + Jump 1")
            .ActivateOnEnter<P3GnashAndLash>()
            .SetHint(StateMachine.StateHint.Raidwide);
        // TODO: consider adding a state 0.3s later when stack happens; first jumps happen +/- 0.1s around it

        ComponentCondition<P3GnashAndLash>(id + 0x20, 3.7f, comp => comp.NumCasts >= 1, "In/out 1");
        ComponentCondition<P3GnashAndLash>(id + 0x30, 3.1f, comp => comp.NumCasts >= 2, "Towers 1 + In/out 2"); // note: towers happen ~0.1s earlier
        ComponentCondition<P3Geirskogul>(id + 0x40, 2.5f, comp => comp.Casters.Count > 0);
        ComponentCondition<P3DiveFromGrace>(id + 0x50, 0.8f, comp => comp.NumJumps > 3, "Jump 2");
        ActorCastStartMulti(id + 0x58, _module.BossP3, new AID[] { AID.GnashAndLash, AID.LashAndGnash }, 3.8f, true);
        ComponentCondition<P3DiveFromGrace>(id + 0x60, 2.8f, comp => comp.NumCasts > 3, "Towers 2");
        ComponentCondition<P3Geirskogul>(id + 0x70, 2.6f, comp => comp.Casters.Count > 0);
        ComponentCondition<P3DiveFromGrace>(id + 0x80, 1.8f, comp => comp.NumJumps > 5, "Stack + Jump 3");
        ActorCastEnd(id + 0x88, _module.BossP3, 0.4f, true)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P3GnashAndLash>(id + 0x90, 3.7f, comp => comp.NumCasts >= 3, "In/out 3");
        ComponentCondition<P3GnashAndLash>(id + 0xA0, 3.1f, comp => comp.NumCasts >= 4, "Towers 3 + In/out 4") // note: towers happen ~0.6s earlier
            .DeactivateOnExit<P3GnashAndLash>();
        ComponentCondition<P3Geirskogul>(id + 0xB0, 2.0f, comp => comp.Casters.Count > 0);
        ComponentCondition<P3Geirskogul>(id + 0xC0, 4.5f, comp => comp.Casters.Count == 0, "Dive resolve")
            .DeactivateOnExit<P3DiveFromGrace>()
            .DeactivateOnExit<P3Geirskogul>();
    }

    private void P3Drachenlance(uint id, float delay)
    {
        ActorCast(id, _module.BossP3, AID.Drachenlance, delay, 2.9f, true)
            .ActivateOnEnter<P3Drachenlance>();
        ComponentCondition<P3Drachenlance>(id + 2, 0.7f, comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<P3Drachenlance>();
    }

    private void P3SoulTether(uint id, float delay)
    {
        ComponentCondition<P3DarkdragonDiveCounter>(id, delay, comp => comp.Towers.Count > 0)
            .ActivateOnEnter<P3DarkdragonDiveCounter>();
        ComponentCondition<P3DarkdragonDiveCounter>(id + 1, 5, comp => comp.Towers.Count == 0, "Towers")
            .DeactivateOnExit<P3DarkdragonDiveCounter>();
        ComponentCondition<P3SoulTether>(id + 0x10, 7.1f, comp => comp.NumCasts > 0, "Tethers")
            .ActivateOnEnter<P3SoulTether>()
            .ActivateOnEnter<P3Geirskogul>()
            .DeactivateOnExit<P3SoulTether>()
            .DeactivateOnExit<P3Geirskogul>();
    }

    private void P4SoulOfFriendshipDevotion(uint id)
    {
        Timeout(id, 0)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorTargetable(id + 1, _module.LeftEyeP4, true, 17.3f, "Buffs + eyes appear") // note: soul of x casts happen ~0.2s later
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<P4Resentment>(id + 0x10, 7.3f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P4Resentment>()
            .DeactivateOnExit<P4Resentment>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P4Hatebound(uint id, float delay)
    {
        ActorCast(id, _module.LeftEyeP4, AID.Hatebound, delay, 3); // both eyes cast it at the same time
        ComponentCondition<P4Hatebound>(id + 2, 0.8f, comp => comp.ColorsAssigned, "Colors")
            .ActivateOnEnter<P4Hatebound>();
        ComponentCondition<P4Hatebound>(id + 0x10, 6.1f, comp => comp.YellowReady, "Pop yellow orbs");
        ComponentCondition<P4Hatebound>(id + 0x20, 6.0f, comp => comp.BlueReady, "Pop blue orbs");

        ActorCast(id + 0x1000, _module.LeftEyeP4, AID.MirageDive, 10.3f, 3) // both eyes cast it at the same time
            .ActivateOnEnter<P4MirageDive>();
        ComponentCondition<P4MirageDive>(id + 0x1010, 0.8f, comp => comp.NumCasts >= 2, "Dive 1");
        ComponentCondition<P4MirageDive>(id + 0x1020, 5.1f, comp => comp.NumCasts >= 4, "Dive 2");
        ComponentCondition<P4MirageDive>(id + 0x1030, 5.1f, comp => comp.NumCasts >= 6, "Dive 3");
        ComponentCondition<P4MirageDive>(id + 0x1040, 5.1f, comp => comp.NumCasts >= 8, "Dive 4")
            .DeactivateOnExit<P4MirageDive>()
            .DeactivateOnExit<P4Hatebound>();
    }

    private void P4SteepInRage(uint id, float delay)
    {
        Condition(id, delay, () => (_module.LeftEyeP4()?.CastInfo?.IsSpell(AID.SteepInRage) ?? false) || (_module.RightEyeP4()?.CastInfo?.IsSpell(AID.SteepInRage) ?? false));
        Condition(id + 1, 6, () => _module.LeftEyeP4()?.CastInfo == null && _module.RightEyeP4()?.CastInfo == null, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Condition(id + 0x1000, 7.2f, () => !(_module.LeftEyeP4()?.IsTargetable ?? false) && !(_module.RightEyeP4()?.IsTargetable ?? false), "Enrage");
    }

    private void P4IntermissionCharibert(uint id)
    {
        Timeout(id, 0)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorTargetable(id + 1, _module.SerCharibert, true, 19.9f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCastStart(id + 2, _module.SerCharibert, AID.PureOfHeart, 0.1f, true)
            .ActivateOnEnter<P4IntermissionBrightwing>()
            .ActivateOnEnter<P4IntermissionSkyblindBait>()
            .ActivateOnEnter<P4IntermissionSkyblind>();
        ComponentCondition<P4Haurchefant>(id + 3, 6.6f, comp => comp.Appear, "Tank LB3")
            .ActivateOnEnter<P4Haurchefant>()
            .DeactivateOnExit<P4Haurchefant>();
        ComponentCondition<P4IntermissionBrightwing>(id + 0x10, 8.7f, comp => comp.NumCasts > 0, "Cone 1");
        ComponentCondition<P4IntermissionBrightwing>(id + 0x20, 5, comp => comp.NumCasts > 2, "Cone 2");
        ComponentCondition<P4IntermissionBrightwing>(id + 0x30, 5, comp => comp.NumCasts > 4, "Cone 3");
        ComponentCondition<P4IntermissionBrightwing>(id + 0x40, 5, comp => comp.NumCasts > 6, "Cone 4")
            .DeactivateOnExit<P4IntermissionBrightwing>();
        ActorCastEnd(id + 0x50, _module.SerCharibert, 5, true, "Raidwide");
        ActorTargetable(id + 0x60, _module.Spear, true, 2.0f, "Spear appears");
    }

    private void P5Start(uint id)
    {
        Timeout(id, 0)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorTargetable(id + 1, _module.BossP5, true, 15.2f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private State P5AncientQuaga(uint id, float delay)
    {
        return ActorCast(id, _module.BossP5, AID.AncientQuaga, delay, 6, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P5HeavenlyHeelAscalonMight(uint id, float delay)
    {
        ActorCast(id, _module.BossP5, AID.HeavenlyHeel, delay, 4, true, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P5AscalonMight>(id + 0x1000, 6.5f, comp => comp.NumCasts > 2, "3x tankbuster cones")
            .ActivateOnEnter<P5AscalonMight>()
            .DeactivateOnExit<P5AscalonMight>();
    }

    private void P5WrathOfHeavens(uint id, float delay)
    {
        ActorCast(id, _module.BossP5, AID.Incarnation, delay, 4, true);
        ActorCast(id + 0x10, _module.BossP5, AID.DragonsEye, 3.2f, 3, true);
        ActorCast(id + 0x20, _module.BossP5, AID.WrathOfTheHeavens, 14.7f, 4, true);
        ActorTargetable(id + 0x30, _module.BossP5, false, 3.1f, "Trio 1")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<P5WrathOfTheHeavensSkywardLeap>(id + 0x100, 2.7f, comp => comp.Active) // icons + tethers
            .ActivateOnEnter<P5WrathOfTheHeavensSkywardLeap>();
        ComponentCondition<P5WrathOfTheHeavensChainLightning>(id + 0x110, 3.7f, comp => comp.Targets.Any()) // lighting debuffs
            .ActivateOnEnter<P5WrathOfTheHeavensSpiralPierce>() // tethers appear together with skyward leap icon
            .ActivateOnEnter<P5TwistingDive>() // cast starts right after skyward leap icon / tethers
            .ActivateOnEnter<P5WrathOfTheHeavensChainLightning>();
        ComponentCondition<P5WrathOfTheHeavensEmptyDimension>(id + 0x120, 1.5f, comp => comp.KnowPosition)
            .ActivateOnEnter<P5WrathOfTheHeavensEmptyDimension>()
            .ActivateOnEnter<P5WrathOfTheHeavensCauterizeBait>(); // icon appears right as spiral pierces complete
        ComponentCondition<P5WrathOfTheHeavensSpiralPierce>(id + 0x130, 1.0f, comp => comp.NumCasts > 0, "Charges + blue marker")
            .DeactivateOnExit<P5TwistingDive>() // note: this happens ~0.1s before
            .DeactivateOnExit<P5WrathOfTheHeavensSkywardLeap>() // note: this happens within ~0.1s, either slightly before or slightly later
            .DeactivateOnExit<P5WrathOfTheHeavensSpiralPierce>();
        ComponentCondition<P5WrathOfTheHeavensTwister>(id + 0x140, 1.2f, comp => comp.Active, "Twisters")
            .ActivateOnEnter<P5WrathOfTheHeavensTwister>(); // note: positions are determined slightly later, but regardless this is a good activation point

        ActorCast(id + 0x200, _module.BossP5, AID.AscalonsMercyRevealed, 1.0f, 3.3f, true)
            .ActivateOnEnter<P5WrathOfTheHeavensAscalonsMercyRevealed>();
        ComponentCondition<P5WrathOfTheHeavensAscalonsMercyRevealed>(id + 0x202, 0.8f, comp => comp.NumCasts > 0, "Proteans")
            .DeactivateOnExit<P5WrathOfTheHeavensAscalonsMercyRevealed>()
            .DeactivateOnExit<P5WrathOfTheHeavensTwister>(); // twisters disappear together with protean hits
        ComponentCondition<P5Cauterize1>(id + 0x210, 0.9f, comp => comp.Casters.Count > 0, "Green marker bait")
            .ExecOnEnter<P5WrathOfTheHeavensChainLightning>(comp => comp.ShowSpreads(5.2f))
            .ActivateOnEnter<P5Cauterize1>()
            .ActivateOnEnter<P5Cauterize2>()
            .ActivateOnEnter<P5WrathOfTheHeavensAltarFlare>() // first cast starts right as proteans resolve
            .ActivateOnEnter<P5WrathOfTheHeavensLiquidHeaven>() // first cast happens right after proteans, then every 1.1s
            .DeactivateOnExit<P5WrathOfTheHeavensCauterizeBait>();
        ComponentCondition<P5WrathOfTheHeavensEmptyDimension>(id + 0x220, 1.2f, comp => comp.Casters.Count > 0);
        ComponentCondition<P5WrathOfTheHeavensEmptyDimension>(id + 0x230, 5.0f, comp => comp.NumCasts > 0, "Trio 1 resolve")
            .DeactivateOnExit<P5Cauterize1>() // these casts resolve ~0.2s before donut
            .DeactivateOnExit<P5Cauterize2>()
            .DeactivateOnExit<P5WrathOfTheHeavensEmptyDimension>();

        ActorTargetable(id + 0x300, _module.BossP5, true, 1.0f, "Boss reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        P5AncientQuaga(id + 0x1000, 0.1f)
            .DeactivateOnExit<P5WrathOfTheHeavensChainLightning>() // lightings resolve right after donuts
            .DeactivateOnExit<P5WrathOfTheHeavensAltarFlare>(); // last cast finishes ~0.3s into cast
        // note: liquid heaven voidzoned disappear after cast; keep component alive
    }

    private void P5DeathOfTheHeavens(uint id, float delay)
    {
        ActorCast(id, _module.BossP5, AID.DeathOfTheHeavens, delay, 4, true);
        ActorTargetable(id + 0x10, _module.BossP5, false, 3.1f, "Trio 2")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<P5DeathOfTheHeavensHeavyImpact>(id + 0x100, 1.6f, comp => comp.Active)
            .ActivateOnEnter<P5DeathOfTheHeavensHeavyImpact>();
        ComponentCondition<P5DeathOfTheHeavensGaze>(id + 0x110, 2.2f, comp => comp.Active)
            .ActivateOnEnter<P5DeathOfTheHeavensGaze>()
            .ActivateOnEnter<P5DeathOfTheHeavensDooms>();
        ComponentCondition<P5DeathOfTheHeavensDooms>(id + 0x111, 0.1f, comp => comp.Dooms.Any());
        ActorCastStart(id + 0x120, _module.BossP5, AID.LightningStorm, 4.1f, true);
        ComponentCondition<P5DeathOfTheHeavensHeavyImpact>(id + 0x130, 4.1f, comp => comp.NumCasts >= 1)
            .ActivateOnEnter<P5TwistingDive>() // TODO: consider activating earlier, when PATE's happen
            .ActivateOnEnter<P5Cauterize1>()
            .ActivateOnEnter<P5SpearOfTheFury>()
            .ActivateOnEnter<P5DeathOfTheHeavensLightningStorm>();
        ActorCastEnd(id + 0x140, _module.BossP5, 1.6f, true);
        ComponentCondition<P5TwistingDive>(id + 0x150, 0.3f, comp => comp.NumCasts > 0, "Charges")
            .DeactivateOnExit<P5TwistingDive>()
            .DeactivateOnExit<P5Cauterize1>() // these all three happen at the same time
            .DeactivateOnExit<P5SpearOfTheFury>();
        ComponentCondition<P5DeathOfTheHeavensHeavyImpact>(id + 0x151, 0.1f, comp => comp.NumCasts >= 2, "Ring 2");
        ComponentCondition<P5DeathOfTheHeavensLightningStorm>(id + 0x152, 0.1f, comp => !comp.Active, "Spreads")
            .DeactivateOnExit<P5DeathOfTheHeavensLightningStorm>();
        ComponentCondition<P5WrathOfTheHeavensTwister>(id + 0x160, 1.1f, comp => comp.Active, "Twisters")
            .ActivateOnEnter<P5WrathOfTheHeavensTwister>();
        ComponentCondition<P5DeathOfTheHeavensHeavyImpact>(id + 0x170, 0.6f, comp => comp.NumCasts >= 3, "Ring 3");
        ComponentCondition<P5DeathOfTheHeavensHeavyImpact>(id + 0x180, 1.9f, comp => comp.NumCasts >= 4, "Ring 4");
        ComponentCondition<P5DeathOfTheHeavensHeavyImpact>(id + 0x190, 1.9f, comp => comp.NumCasts >= 5)
            .DeactivateOnExit<P5DeathOfTheHeavensHeavyImpact>();
        ComponentCondition<P5WrathOfTheHeavensTwister>(id + 0x1A0, 0.6f, comp => !comp.Active)
            .DeactivateOnExit<P5WrathOfTheHeavensTwister>();

        ActorCastStart(id + 0x200, _module.BossP5, AID.DragonsGaze, 1.9f, true)
            .ActivateOnEnter<P5DeathOfTheHeavensHeavensflame>(); // heavensflame cast starts at the same time, icons appear ~0.1s before cast start
        // +1.1s: faith unmoving and holy meteor casts start
        // +1.9s: wings of salvation aoes end
        // +2.6s: wings of salvation voidzones appear
        ActorCastEnd(id + 0x201, _module.BossP5, 4, true) // chains appear just as cast ends
            .ExecOnEnter<P5DeathOfTheHeavensGaze>(comp => comp.EnableHints = true);
        ComponentCondition<P5DeathOfTheHeavensHeavensflame>(id + 0x210, 1.1f, comp => comp.KnockbackDone, "Knockback");
        ComponentCondition<P5DeathOfTheHeavensGaze>(id + 0x220, 0.1f, comp => comp.NumCasts > 0, "Gaze")
            .DeactivateOnExit<P5DeathOfTheHeavensGaze>();
        ComponentCondition<P5DeathOfTheHeavensHeavensflame>(id + 0x230, 2.5f, comp => comp.NumCasts > 0, "Playstation resolve")
            .DeactivateOnExit<P5DeathOfTheHeavensHeavensflame>()
            .DeactivateOnExit<P5DeathOfTheHeavensDooms>();

        ComponentCondition<P5DeathOfTheHeavensMeteorCircle>(id + 0x300, 2.4f, comp => comp.ActiveActors.Any(), "Meteors spawn")
            .ActivateOnEnter<P5DeathOfTheHeavensMeteorCircle>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorTargetable(id + 0x310, _module.BossP5, true, 14.8f, "Meteors enrage") // boss will reappear as soon as final meteor dies
            .DeactivateOnExit<P5DeathOfTheHeavensMeteorCircle>();
    }

    private void P6Start(uint id)
    {
        Timeout(id, 0)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorTargetable(id + 1, _module.BossP5, false, 6.6f, "Boss disappears");
        ActorTargetable(id + 2, _module.NidhoggP6, true, 10.4f, "Dragons appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P6MortalVow(uint id, float delay)
    {
        ComponentCondition<P6MortalVow>(id, delay, comp => comp.Progress > 0, "Mortal vow apply")
            .ActivateOnEnter<P6MortalVow>();
    }

    private void P6AkhAfah(uint id, float delay)
    {
        ActorCast(id, _module.NidhoggP6, AID.AkhAfahN, delay, 8, true)
            .ActivateOnEnter<P6AkhAfah>();
        ComponentCondition<P6AkhAfah>(id + 0x10, 0.2f, comp => comp.Done, "Party stacks + HP check")
            .DeactivateOnExit<P6AkhAfah>();
    }

    private void P6Wyrmsbreath1(uint id, float delay)
    {
        ActorCastMulti(id, _module.NidhoggP6, new[] { AID.DreadWyrmsbreathNormal, AID.DreadWyrmsbreathGlow }, delay, 6.3f, true)
            .ActivateOnEnter<P6HPCheck>()
            .ActivateOnEnter<P6Wyrmsbreath1>()
            .ActivateOnEnter<P6WyrmsbreathTankbusterShared>()
            .ActivateOnEnter<P6WyrmsbreathTankbusterSolo>()
            .ActivateOnEnter<P6WyrmsbreathCone>()
            .ActivateOnEnter<P6SwirlingBlizzard>();
        ComponentCondition<P6SwirlingBlizzard>(id + 0x10, 0.7f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<P6SwirlingBlizzard>();
        ComponentCondition<P6Wyrmsbreath>(id + 0x20, 0.1f, comp => comp.NumCasts > 0, "Wyrmsbreath 1")
            .DeactivateOnExit<P6WyrmsbreathTankbusterShared>()
            .DeactivateOnExit<P6WyrmsbreathTankbusterSolo>()
            .DeactivateOnExit<P6WyrmsbreathCone>()
            .DeactivateOnExit<P6Wyrmsbreath1>();
    }

    private void P6Wyrmsbreath2(uint id, float delay)
    {
        ActorCastMulti(id, _module.NidhoggP6, new[] { AID.DreadWyrmsbreathNormal, AID.DreadWyrmsbreathGlow }, delay, 6.3f, true)
            .ActivateOnEnter<P6Wyrmsbreath2>()
            .ActivateOnEnter<P6WyrmsbreathTankbusterShared>()
            .ActivateOnEnter<P6WyrmsbreathTankbusterSolo>()
            .ActivateOnEnter<P6WyrmsbreathCone>()
            .ActivateOnEnter<P6SwirlingBlizzard>();
        ComponentCondition<P6SwirlingBlizzard>(id + 0x10, 0.7f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<P6SwirlingBlizzard>();
        ComponentCondition<P6Wyrmsbreath>(id + 0x20, 0.1f, comp => comp.NumCasts > 0, "Wyrmsbreath 2")
            .DeactivateOnExit<P6WyrmsbreathTankbusterShared>()
            .DeactivateOnExit<P6WyrmsbreathTankbusterSolo>()
            .DeactivateOnExit<P6WyrmsbreathCone>()
            .DeactivateOnExit<P6Wyrmsbreath2>();
    }

    private void P6HallowedWingsPlume1(uint id, float delay)
    {
        ActorTargetable(id, _module.NidhoggP6, false, delay, "Nidhogg disappears");
        ActorCastMulti(id + 0x10, _module.HraesvelgrP6, new[] { AID.HallowedWingsLN, AID.HallowedWingsLF, AID.HallowedWingsRN, AID.HallowedWingsRF }, 1.8f, 7.5f, true)
            .ActivateOnEnter<P6HallowedWings>()
            .ActivateOnEnter<P6CauterizeN>() // cauterize cast starts later, but nidhogg is already in position, so we start showing hints immediately
            .ActivateOnEnter<P6HallowedPlume1>();
        ComponentCondition<P6HallowedWings>(id + 0x20, 1.0f, comp => comp.NumCasts > 0, "Safe quarter + near/far tankbusters")
            .DeactivateOnExit<P6HallowedWings>()
            .DeactivateOnExit<P6CauterizeN>() // cauterize happens +-0.3s
            .DeactivateOnExit<P6HallowedPlume1>() // tankbusters happen at the same time as wings
            .SetHint(StateMachine.StateHint.Tankbuster);

        ComponentCondition<P6MortalVow>(id + 0x100, 8.7f, comp => comp.Progress > 1, "Mortal vow pass 1")
            .ExecOnEnter<P6MortalVow>(comp => comp.ShowNextPass());
    }

    private void P6HallowedWingsPlume2(uint id, float delay)
    {
        ActorCastMulti(id, _module.HraesvelgrP6, new[] { AID.HallowedWingsLN, AID.HallowedWingsLF, AID.HallowedWingsRN, AID.HallowedWingsRF }, delay, 7.5f, true)
            .ActivateOnEnter<P6HallowedWings>()
            .ActivateOnEnter<P6HotWingTail>() // this cast starts ~2s later
            .ActivateOnEnter<P6HallowedPlume2>();
        ComponentCondition<P6HotWingTail>(id + 0x10, 1.0f, comp => comp.NumCasts > 0, "Safe line + near/far tankbusters")
            .DeactivateOnExit<P6HallowedWings>() // this typically happens slightly earlier than wing/tail
            .DeactivateOnExit<P6HotWingTail>()
            .DeactivateOnExit<P6HallowedPlume2>(); // tankbusters happen at the same time as wings

        ComponentCondition<P6MortalVow>(id + 0x100, 8.7f, comp => comp.Progress > 3, "Mortal vow pass 3")
            .ExecOnEnter<P6MortalVow>(comp => comp.ShowNextPass());
    }

    private void P6WrothFlames(uint id, float delay)
    {
        ActorCast(id, _module.NidhoggP6, AID.WrothFlames, delay, 2.5f, true);
        ActorTargetable(id + 0x10, _module.HraesvelgrP6, false, 0.5f, "Hraesvelgr disappears");
        // +1.0s: 4x spreading flames, 2x entangling flames
        ActorTargetable(id + 0x20, _module.HraesvelgrP6, true, 1.3f);
        ActorCastStart(id + 0x30, _module.NidhoggP6, AID.AkhMornFirst, 1.0f, true)
            .ActivateOnEnter<P6WrothFlames>(); // first set spawns soon after hraesvelgr reappears, then next set spawns in 2s, then in 3s
        // +3.1s: hraesvelgr starts cauterize
        ActorCastEnd(id + 0x31, _module.NidhoggP6, 8, true, "Stack start") // stacks repeat every 1.6s
            .ActivateOnEnter<P6AkhMorn>();
        ComponentCondition<P6WrothFlames>(id + 0x32, 0.1f, comp => comp.NumCasts > 0, "Line")
            .ActivateOnEnter<P6AkhMornVoidzone>();
        ComponentCondition<P6AkhMorn>(id + 0x33, 1.6f, comp => comp.NumFinishedStacks >= 2);
        ComponentCondition<P6AkhMorn>(id + 0x34, 1.5f, comp => comp.NumFinishedStacks >= 3);
        ComponentCondition<P6WrothFlames>(id + 0x40, 0.1f, comp => comp.NumCasts > 1, "Cross 1");
        ComponentCondition<P6AkhMorn>(id + 0x41, 1.5f, comp => comp.NumFinishedStacks >= 4)
            .DeactivateOnExit<P6AkhMorn>();
        ComponentCondition<P6WrothFlames>(id + 0x50, 0.5f, comp => comp.NumCasts > 4, "Cross 2")
            .ActivateOnEnter<P6HotWingTail>() // note: activating early, so that spreading/entangled flames can use it
            .ActivateOnEnter<P6SpreadingEntangledFlames>();
        ActorCastStartMulti(id + 0x60, _module.NidhoggP6, new[] { AID.HotWing, AID.HotTail }, 1.1f);
        ComponentCondition<P6WrothFlames>(id + 0x70, 1.9f, comp => comp.NumCasts > 7, "Cross 3")
            .DeactivateOnExit<P6WrothFlames>();
        ActorCastEnd(id + 0x80, _module.NidhoggP6, 3.6f);
        ComponentCondition<P6HotWingTail>(id + 0x90, 1.0f, comp => comp.NumCasts > 0, "Sides/center");
        ComponentCondition<P6SpreadingEntangledFlames>(id + 0xA0, 0.9f, comp => !comp.Active, "Stack/spread")
            .DeactivateOnExit<P6SpreadingEntangledFlames>()
            .DeactivateOnExit<P6HotWingTail>();

        ComponentCondition<P6MortalVow>(id + 0x100, 4.0f, comp => comp.Progress > 2, "Mortal vow pass 2")
            .ExecOnEnter<P6MortalVow>(comp => comp.ShowNextPass());
        // note: voidzones disappear slightly later...
    }

    private void P6Touchdown(uint id, float delay)
    {
        ActorTargetable(id, _module.NidhoggP6, false, delay, "Bosses disappear"); // both
        ActorTargetable(id + 1, _module.NidhoggP6, true, 1.3f); // both
        ActorCast(id + 0x10, _module.NidhoggP6, AID.CauterizeN, 1.2f, 5, true, "Wild charges")
            .ActivateOnEnter<P6TouchdownCauterize>()
            .DeactivateOnExit<P6TouchdownCauterize>();
        ComponentCondition<P6Touchdown>(id + 0x20, 7.0f, comp => comp.NumCasts > 0, "Proximity")
            .ActivateOnEnter<P6Touchdown>()
            .DeactivateOnExit<P6Touchdown>();

        ActorCastStart(id + 0x100, _module.NidhoggP6, AID.RevengeOfTheHordeP6, 1.2f, true)
            .ExecOnEnter<P6MortalVow>(comp => comp.ShowNextPass());
        ComponentCondition<P6MortalVow>(id + 0x101, 2.3f, comp => comp.Progress > 4, "Mortal vow pass 4")
            .DeactivateOnExit<P6MortalVow>();
        ActorCastEnd(id + 0x102, _module.NidhoggP6, 22.7f, true, "Enrage");
    }

    private void P7Start(uint id)
    {
        Timeout(id, 0)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P4Resentment>(id + 0x10, 10.3f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P4Resentment>()
            .DeactivateOnExit<P4Resentment>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P7Shockwave>(id + 0x20, 17.0f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P7Shockwave>()
            .DeactivateOnExit<P7Shockwave>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P7AlternativeEnd>(id + 0x30, 15.7f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P7AlternativeEnd>()
            .DeactivateOnExit<P7AlternativeEnd>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x40, _module.BossP7, true, 9.1f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private State P7Trinity(uint id, float delay)
    {
        ComponentCondition<P7Trinity>(id, delay, comp => comp.NumCasts > 0, "Trinity 1")
            .ActivateOnEnter<P7Trinity>();
        return ComponentCondition<P7Trinity>(id + 0x10, 4.0f, comp => comp.NumCasts > 3, "Trinity 2")
            .DeactivateOnExit<P7Trinity>();
    }

    private void P7ExaflareEdge(uint id, float delay)
    {
        ActorCast(id, _module.BossP7, AID.ExaflaresEdge, delay, 6, true)
            .ActivateOnEnter<P7ExaflaresEdge>()
            .ActivateOnEnter<P7FlamesIceOfAscalon>();
        ComponentCondition<P7ExaflaresEdge>(id + 0x10, 0.9f, comp => comp.NumCasts > 0, "Exaflares 1");
        ComponentCondition<P7FlamesIceOfAscalon>(id + 0x11, 0.3f, comp => comp.NumCasts > 0, "In/out")
            .DeactivateOnExit<P7FlamesIceOfAscalon>();
        ComponentCondition<P7ExaflaresEdge>(id + 0x20, 1.6f, comp => comp.NumCasts > 3, "Exaflares 2");
        ComponentCondition<P7ExaflaresEdge>(id + 0x30, 1.9f, comp => comp.NumCasts > 12, "Exaflares 3");
        // don't really care about remaining exaflares...
        P7Trinity(id + 0x100, 4.5f)
            .DeactivateOnExit<P7ExaflaresEdge>();
    }

    private void P7AkhMornsEdge(uint id, float delay, int count)
    {
        ActorCast(id, _module.BossP7, AID.AkhMornsEdge, delay, 6, true)
            .ActivateOnEnter<P7AkhMornsEdge>()
            .ActivateOnEnter<P7FlamesIceOfAscalon>();
        ComponentCondition<P7AkhMornsEdge>(id + 0x10, 0.7f, comp => comp.NumCasts >= 1, "Towers 1");
        ComponentCondition<P7FlamesIceOfAscalon>(id + 0x11, 0.1f, comp => comp.NumCasts > 0, "In/out")
            .DeactivateOnExit<P7FlamesIceOfAscalon>();
        ComponentCondition<P7AkhMornsEdge>(id + 0x20, 2.1f, comp => comp.NumCasts >= 2);
        ComponentCondition<P7AkhMornsEdge>(id + 0x30, 1.1f * (count - 2) - 0.1f, comp => comp.NumCasts >= count, $"Towers {count}")
            .DeactivateOnExit<P7AkhMornsEdge>();
        P7Trinity(id + 0x100, 6.4f);
    }

    private void P7GigaflaresEdge(uint id, float delay)
    {
        ActorCast(id, _module.BossP7, AID.GigaflaresEdge, delay, 8, true)
            .ActivateOnEnter<P7GigaflaresEdge>()
            .ActivateOnEnter<P7FlamesIceOfAscalon>();
        ComponentCondition<P7GigaflaresEdge>(id + 0x10, 1.0f, comp => comp.NumCasts >= 1, "Gigaflare 1");
        ComponentCondition<P7FlamesIceOfAscalon>(id + 0x11, 0.2f, comp => comp.NumCasts > 0, "In/out")
            .DeactivateOnExit<P7FlamesIceOfAscalon>();
        ComponentCondition<P7GigaflaresEdge>(id + 0x20, 3.8f, comp => comp.NumCasts >= 2, "Gigaflare 2");
        ComponentCondition<P7GigaflaresEdge>(id + 0x30, 4.0f, comp => comp.NumCasts >= 3, "Gigaflare 3")
            .DeactivateOnExit<P7GigaflaresEdge>();
        P7Trinity(id + 0x100, 10.2f);
    }

    private void P7MornAfahsEdge(uint id, float delay)
    {
        ActorCast(id, _module.BossP7, AID.MornAfahsEdge, delay, 10, true)
            .ActivateOnEnter<P7MornAfahsEdge>();
        ComponentCondition<P7MornAfahsEdge>(id + 0x10, 0.7f, comp => comp.NumCasts > 0, "Enrage 1");
        ComponentCondition<P7MornAfahsEdge>(id + 0x20, 3.2f, comp => comp.NumCasts > 3, "Enrage 2");
        ComponentCondition<P7MornAfahsEdge>(id + 0x30, 3.2f, comp => comp.NumCasts > 6, "Enrage 3");
    }
}
