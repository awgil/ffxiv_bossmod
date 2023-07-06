using BossMod.Endwalker.Savage.P5SProtoCarbuncle;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class DSW2States : StateMachineBuilder
    {
        private DSW2 _module;

        private bool IsReset => _module.PrimaryActor.IsDestroyed && (_module.ArenaFeatures?.IsDestroyed ?? true);
        private bool IsResetOrRewindFailed => IsReset || Module.Enemies(OID.BossP2).Any();
        private bool IsDead(Actor? actor) => actor != null && (actor.IsDestroyed || actor.IsDead);

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
                .Raw.Update = () => IsResetOrRewindFailed;
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
            P2BroadSwing(id + 0x70000, 2.7f);
            Cast(id + 0x80000, AID.AethericBurst, 2.4f, 6, "Enrage");
        }

        private void Phase3Nidhogg(uint id)
        {
            P3FinalChorus(id);
            P3Dives(id + 0x10000, 13.2f);
            P3Drachenlance(id + 0x20000, 1.9f);
            P3SoulTether(id + 0x30000, 1.4f);
            P3Drachenlance(id + 0x40000, 20.9f);
            ActorCast(id + 0x50000, _module.BossP3, AID.RevengeOfTheHordeP3, 1.4f, 11, true, "Enrage");
        }

        private void Phase4Eyes(uint id)
        {
            P4SoulOfFriendshipDevotion(id);
            P4Hatebound(id + 0x10000, 5.5f);
            P4SteepInRage(id + 0x20000, 3.8f);
        }

        private void Phase4Intermission(uint id)
        {
            P4IntermissionCharibert(id);
            ActorCast(id + 0x10000, _module.Spear, AID.Pierce, 2.6f, 11, true, "Enrage");
        }

        private void Phase5KingThordan(uint id)
        {
            P5Start(id);
            P5WrathOfHeavens(id + 0x10000, 0.1f);
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void P2AscalonsMercyConcealedMight(uint id, float delay)
        {
            CastStart(id, AID.AscalonsMercyConcealed, delay);
            CastEnd(id + 1, 3)
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

        private State P2AncientQuaga(uint id, float delay)
        {
            return Cast(id, AID.AncientQuaga, delay, 6, "Raidwide")
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
            ComponentCondition<P2SanctityOfTheWard1Gaze>(id + 0x30, 1.1f, comp => comp.NumCasts > 0, "Gazes")
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
            ComponentCondition<P3GnashAndLash>(id + 0xA0, 3.1f, comp => comp.NumCasts >= 4, "Towers 3 + In/out 4"); // note: towers happen ~0.6s earlier
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
            ComponentCondition<P3SoulTether>(id + 0x10, 7, comp => comp.NumCasts > 0, "Tethers")
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
            ComponentCondition<P4Hatebound>(id + 0x10, 6.0f, comp => comp.YellowReady, "Pop yellow orbs");
            ComponentCondition<P4Hatebound>(id + 0x20, 6.0f, comp => comp.BlueReady, "Pop blue orbs");

            ActorCast(id + 0x1000, _module.LeftEyeP4, AID.MirageDive, 10.2f, 3) // both eyes cast it at the same time
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
            ActorCast(id, _module.LeftEyeP4, AID.SteepInRage, delay, 6, false, "Raidwide") // note: while both eyes cast it at the same time, typically right eye is killed before or during cast
                .SetHint(StateMachine.StateHint.Raidwide);
            Condition(id + 0x1000, 7.2f, () => !(_module.LeftEyeP4()?.IsTargetable ?? false) && !(_module.RightEyeP4()?.IsTargetable ?? false), "Enrage");
        }

        private void P4IntermissionCharibert(uint id)
        {
            Timeout(id, 0)
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ActorTargetable(id + 1, _module.SerCharibert, true, 20.1f, "Boss appears")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            ActorCastStart(id + 2, _module.SerCharibert, AID.PureOfHeart, 0.1f, true)
                .ActivateOnEnter<P4IntermissionBrightwing>()
                .ActivateOnEnter<P4IntermissionSkyblindBait>()
                .ActivateOnEnter<P4IntermissionSkyblind>();
            ComponentCondition<P4Haurchefant>(id + 3, 6.5f, comp => comp.Appear, "Tank LB3")
                .ActivateOnEnter<P4Haurchefant>()
                .DeactivateOnExit<P4Haurchefant>();
            ComponentCondition<P4IntermissionBrightwing>(id + 0x10, 8.8f, comp => comp.NumCasts > 0, "Cone 1");
            ComponentCondition<P4IntermissionBrightwing>(id + 0x20, 5, comp => comp.NumCasts > 2, "Cone 2");
            ComponentCondition<P4IntermissionBrightwing>(id + 0x30, 5, comp => comp.NumCasts > 4, "Cone 3");
            ComponentCondition<P4IntermissionBrightwing>(id + 0x40, 5, comp => comp.NumCasts > 6, "Cone 4")
                .DeactivateOnExit<P4IntermissionBrightwing>();
            ActorCastEnd(id + 0x50, _module.SerCharibert, 5, true, "Raidwide");
            ActorTargetable(id + 0x60, _module.SerCharibert, false, 2.1f, "Disappear");
        }

        private void P5Start(uint id)
        {
            Timeout(id, 0)
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ActorTargetable(id + 1, _module.BossP5, true, 15.2f, "Reappear")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void P5WrathOfHeavens(uint id, float delay)
        {
            ActorCast(id, _module.BossP5, AID.Incarnation, delay, 4, true);
            ActorCast(id + 0x10, _module.BossP5, AID.DragonsEye, 3.1f, 3, true);
            ActorCast(id + 0x20, _module.BossP5, AID.WrathOfTheHeavens, 14.7f, 4, true);
            ActorTargetable(id + 0x30, _module.BossP5, false, 3.1f, "Trio 1");

            ComponentCondition<P5WrathOfTheHeavensSkywardLeap>(id + 0x100, 2.7f, comp => comp.Active) // icons + tethers
                .ActivateOnEnter<P5WrathOfTheHeavensSkywardLeap>();
            ComponentCondition<P5WrathOfTheHeavensChainLightning>(id + 0x110, 3.6f, comp => comp.Targets.Any()) // lighting debuffs
                .ActivateOnEnter<P5WrathOfTheHeavensSpiralPierce>() // tethers appear together with skyward leap icon
                .ActivateOnEnter<P5WrathOfTheHeavensTwistingDive>() // cast starts right after skyward leap icon / tethers
                .ActivateOnEnter<P5WrathOfTheHeavensChainLightning>();
            ComponentCondition<P5WrathOfTheHeavensEmptyDimension>(id + 0x120, 1.5f, comp => comp.KnowPosition)
                .ActivateOnEnter<P5WrathOfTheHeavensEmptyDimension>()
                .ActivateOnEnter<P5WrathOfTheHeavensCauterizeBait>(); // icon appears right as spiral pierces complete
            ComponentCondition<P5WrathOfTheHeavensSpiralPierce>(id + 0x130, 1.0f, comp => comp.NumCasts > 0, "Charges")
                .DeactivateOnExit<P5WrathOfTheHeavensTwistingDive>() // note: this happens ~0.1s before
                .DeactivateOnExit<P5WrathOfTheHeavensSpiralPierce>();
            ComponentCondition<P5WrathOfTheHeavensSkywardLeap>(id + 0x131, 0.2f, comp => !comp.Active, "Blue spread resolve")
                .ActivateOnEnter<P5WrathOfTheHeavensTwister>() // TODO: reconsider activation point
                .DeactivateOnExit<P5WrathOfTheHeavensSkywardLeap>();
            ComponentCondition<P5WrathOfTheHeavensTwister>(id + 0x140, 1.2f, comp => comp.Active, "Twisters");

            ActorCast(id + 0x200, _module.BossP5, AID.AscalonsMercyRevealed, 0.8f, 3.3f, true)
                .ActivateOnEnter<P5WrathOfTheHeavensAscalonsMercyRevealed>();
            ComponentCondition<P5WrathOfTheHeavensAscalonsMercyRevealed>(id + 0x202, 0.8f, comp => comp.NumCasts > 0, "Proteans")
                .DeactivateOnExit<P5WrathOfTheHeavensAscalonsMercyRevealed>()
                .DeactivateOnExit<P5WrathOfTheHeavensTwister>(); // twisters disappear together with protean hits
            ComponentCondition<P5WrathOfTheHeavensCauterize1>(id + 0x210, 0.8f, comp => comp.Casters.Count > 0, "Green marker bait")
                .ActivateOnEnter<P5WrathOfTheHeavensCauterize1>()
                .ActivateOnEnter<P5WrathOfTheHeavensCauterize2>()
                .ActivateOnEnter<P5WrathOfTheHeavensAltarFlare>() // first cast starts right as proteans resolve
                .ActivateOnEnter<P5WrathOfTheHeavensLiquidHeaven>() // first cast happens right after proteans, then every 1.1s
                .DeactivateOnExit<P5WrathOfTheHeavensCauterizeBait>();
            ComponentCondition<P5WrathOfTheHeavensEmptyDimension>(id + 0x220, 1.2f, comp => comp.Casters.Count > 0);
            ComponentCondition<P5WrathOfTheHeavensEmptyDimension>(id + 0x230, 5.0f, comp => comp.NumCasts > 0, "Trio 1 resolve")
                .OnEnter(() => Module.FindComponent<P5WrathOfTheHeavensChainLightning>()?.ShowSpreads(Module, 5.2f))
                .DeactivateOnExit<P5WrathOfTheHeavensCauterize1>() // these casts resolve ~0.2s before donut
                .DeactivateOnExit<P5WrathOfTheHeavensCauterize2>()
                .DeactivateOnExit<P5WrathOfTheHeavensEmptyDimension>();

            P2AncientQuaga(id + 0x1000, 1.0f)
                .DeactivateOnExit<P5WrathOfTheHeavensChainLightning>() // lightings resolve right after donuts
                .DeactivateOnExit<P5WrathOfTheHeavensAltarFlare>() // last cast finishes ~0.3s into cast
                .DeactivateOnExit<P5WrathOfTheHeavensLiquidHeaven>(); // voidzones disappear ~4.2s into cast
        }
    }
}
