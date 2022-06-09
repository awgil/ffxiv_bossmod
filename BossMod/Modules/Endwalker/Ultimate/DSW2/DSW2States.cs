namespace BossMod.Endwalker.Ultimate.DSW2
{
    class DSW2States : StateMachineBuilder
    {
        private DSW2 _module;

        public DSW2States(DSW2 module) : base(module)
        {
            _module = module;
            SimplePhase(1, Phase2Thordan, "P2: Thordan") // TODO: auto-attack cleave component
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsDead;
            SimplePhase(2, Phase3Nidhogg, "P3: Nidhogg")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.BossP3()?.IsDestroyed ?? true);
        }

        private void Phase2Thordan(uint id)
        {
            P2AscalonMercyMight(id, 8.4f);
            P2StrengthOfTheWard(id + 0x10000, 7);
            P2AncientQuaga(id + 0x20000, 0.1f);
            P2HeavenlyHeelAscalonMight(id + 0x30000, 6.2f);
            P2SanctityOfTheWard(id + 0x40000, 7);
            P2UltimateEnd(id + 0x50000, 13.5f);
            P2BroadSwing(id + 0x60000, 9.5f);
            P2BroadSwing(id + 0x70000, 2.6f);
            P2AethericBurst(id + 0x80000, 2.4f);
        }

        private void Phase3Nidhogg(uint id)
        {
            P3FinalChorus(id);
            SimpleState(id + 0xF0000, 100, "???");
        }

        private void P2AscalonMercyMight(uint id, float delay)
        {
            CastStart(id, AID.AscalonsMercyConcealed, delay);
            CastEnd(id + 1, 3)
                .SetHint(StateMachine.StateHint.PositioningStart);
            ComponentCondition<P2AscalonMercy>(id + 2, 1.6f, comp => comp.NumCasts > 0, "Dropped cones")
                .ActivateOnEnter<P2AscalonMercy>()
                .DeactivateOnExit<P2AscalonMercy>();
            ComponentCondition<P2AscalonMight>(id + 0x1000, 5, comp => comp.NumCasts > 2, "3x tankbuster cones")
                .ActivateOnEnter<P2AscalonMight>()
                .DeactivateOnExit<P2AscalonMight>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void P2StrengthOfTheWard(uint id, float delay)
        {
            Cast(id, AID.StrengthOfTheWard, delay, 4);
            Targetable(id + 0x10, false, 3.1f, "Trio 1");
            CastStart(id + 0x20, AID.LightningStorm, 3.6f)
                .ActivateOnEnter<P2StrengthOfTheWard1>();
            CastEnd(id + 0x21, 5.7f);
            ComponentCondition<P2StrengthOfTheWard1>(id + 0x30, 0.3f, comp => comp.NumImpactHits > 0, "Charges + Ring 1");
            ComponentCondition<P2StrengthOfTheWard1>(id + 0x40, 1.9f, comp => comp.NumImpactHits > 1, "Ring 2");
            ComponentCondition<P2StrengthOfTheWard1>(id + 0x60, 1.9f, comp => comp.NumImpactHits > 2, "Ring 3");
            ComponentCondition<P2StrengthOfTheWard1>(id + 0x80, 1.9f, comp => comp.NumImpactHits > 3, "Ring 4")
                .ActivateOnEnter<P2AscalonMercy>();
            ComponentCondition<P2AscalonMercy>(id + 0x90, 1, comp => comp.NumCasts > 0, "Dropped cones")
                .DeactivateOnExit<P2AscalonMercy>();
            ComponentCondition<P2StrengthOfTheWard1>(id + 0xA0, 0.9f, comp => comp.NumImpactHits > 4, "Ring 5")
                .ActivateOnEnter<P2StrengthOfTheWard2>()
                .DeactivateOnExit<P2StrengthOfTheWard1>();
            ComponentCondition<P2StrengthOfTheWard2>(id + 0x100, 9, comp => comp.LeapsDone && comp.ChargeDone && comp.RageDone, "Void zones + Leaps + Charges + Stacks")
                .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster);
            ComponentCondition<P2StrengthOfTheWard2>(id + 0x110, 3.3f, comp => comp.TowersDone, "Towers")
                .DeactivateOnExit<P2StrengthOfTheWard2>();
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
            Targetable(id + 0x10, false, 3, "Trio 2");
            CastStart(id + 0x20, AID.DragonsGaze, 5.6f)
                .ActivateOnEnter<P2SanctityOfTheWard1Gaze>()
                .ActivateOnEnter<P2SanctityOfTheWard1>();
            CastEnd(id + 0x21, 4);
            ComponentCondition<P2SanctityOfTheWard1Gaze>(id + 0x30, 1.1f, comp => comp.NumCasts > 0, "Gazes")
                .DeactivateOnExit<P2SanctityOfTheWard1Gaze>();
            ComponentCondition<P2SanctityOfTheWard1>(id + 0x40, 6.1f, comp => comp.NumFlareCasts >= 18, "Charges")
                .DeactivateOnExit<P2SanctityOfTheWard1>();
            ComponentCondition<P2SanctityOfTheWard2>(id + 0x100, 11.8f, comp => comp.StormDone, "Storms")
                .ActivateOnEnter<P2SanctityOfTheWard2HeavensStakeCircles>()
                .ActivateOnEnter<P2SanctityOfTheWard2HeavensStakeDonut>()
                .ActivateOnEnter<P2SanctityOfTheWard2>()
                .DeactivateOnExit<P2SanctityOfTheWard2HeavensStakeCircles>()
                .DeactivateOnExit<P2SanctityOfTheWard2HeavensStakeDonut>();
            ComponentCondition<P2SanctityOfTheWard2>(id + 0x110, 4.2f, comp => comp.Towers1Done > 0, "Towers 1");
            ComponentCondition<P2SanctityOfTheWard2Knockback>(id + 0x120, 10.3f, comp => comp.NumCasts > 0, "Knockback")
                .ActivateOnEnter<P2SanctityOfTheWard2Knockback>()
                .DeactivateOnExit<P2SanctityOfTheWard2Knockback>();
            ComponentCondition<P2SanctityOfTheWard2>(id + 0x130, 3, comp => comp.Towers2Done > 0, "Towers 2")
                .DeactivateOnExit<P2SanctityOfTheWard2>();
            Targetable(id + 0x200, true, 4.5f, "Reappear");
        }

        private void P2UltimateEnd(uint id, float delay)
        {
            ComponentCondition<P2UltimateEnd>(id, delay, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<P2UltimateEnd>()
                .DeactivateOnExit<P2UltimateEnd>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P2BroadSwing(uint id, float delay)
        {
            CastMulti(id, new AID[] { AID.BroadSwingLR, AID.BroadSwingRL }, delay, 3)
                .ActivateOnEnter<P2BroadSwing>();
            ComponentCondition<P2BroadSwing>(id + 2, 2.8f, comp => comp.NumCasts >= 3, "Swings")
                .DeactivateOnExit<P2BroadSwing>();
        }

        private void P2AethericBurst(uint id, float delay)
        {
            Cast(id, AID.AethericBurst, delay, 6, "Enrage");
        }

        private void P3FinalChorus(uint id)
        {
            Timeout(id, 0)
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ActorTargetable(id + 1, _module.BossP3, true, 9.2f)
                .SetHint(StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide); // final chorus raidwide happens together with boss becoming targetable
        }
    }
}
