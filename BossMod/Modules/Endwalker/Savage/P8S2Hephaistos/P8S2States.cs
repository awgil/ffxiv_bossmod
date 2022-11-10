namespace BossMod.Endwalker.Savage.P8S2
{
    class P8S2States : StateMachineBuilder
    {
        public P8S2States(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            // TODO: 3 autoattacks
            Aioniopyr(id, 10.2f);
            // TODO: 2 autoattacks
            TyrantsUnholyDarkness(id + 0x10000, 6.2f);
            // TODO: 2 autoattacks
            NaturalAlignment1(id + 0x20000, 7.1f);
            // TODO: 2 autoattacks
            TyrantsUnholyDarkness(id + 0x30000, 8.2f);
            // TODO: 1 autoattack
            HighConcept1(id + 0x40000, 6.1f);
            // TODO: 3 autoattacks
            LimitlessDesolation(id + 0x50000, 9.4f);
            // TODO: 2 autoattacks
            TyrantsUnholyDarkness(id + 0x60000, 8.2f);
            // TODO: 2 autoattacks
            NaturalAlignment2(id + 0x70000, 9.1f);
            // TODO: 2 autoattacks
            TyrantsUnholyDarkness(id + 0x80000, 8.2f);
            // TODO: 1 autoattack
            HighConcept2(id + 0x90000, 6.1f);
            EgoDeath(id + 0xA0000, 6.1f);
            Aionagonia(id + 0xB0000, 9.2f);
            DominionAionagonia(id + 0xC0000, 3.2f);
            DominionAionagonia(id + 0xD0000, 3.2f);
            Cast(id + 0xE0000, AID.Enrage, 7.1f, 16, "Enrage");
        }

        private void Aioniopyr(uint id, float delay)
        {
            Cast(id, AID.Aioniopyr, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void TyrantsUnholyDarkness(uint id, float delay)
        {
            Cast(id, AID.TyrantsUnholyDarkness, delay, 5)
                .ActivateOnEnter<TyrantsUnholyDarkness>();
            ComponentCondition<TyrantsUnholyDarkness>(id + 2, 1.2f, comp => comp.NumCasts > 0, "Tankbuster")
                .DeactivateOnExit<TyrantsUnholyDarkness>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void NaturalAlignment1(uint id, float delay)
        {
            Cast(id, AID.NaturalAlignment, delay, 5)
                .ActivateOnEnter<NaturalAlignment>();
            // +1.1s: each target gets status 3412 (debuff) and 2552 0x209 (circle under player)

            // ~0.1s before next cast start, first NA activates (209 replaced with 1E1/1E3)
            Cast(id + 0x10, AID.TwistNature, 6.2f, 3);

            // ~0.1s before next cast start, NA progress bars start filling (1E1/1E3 replaced with 1E0/1E2)
            Cast(id + 0x20, AID.TyrantsFlare, 3.2f, 3, "Puddle bait");

            ComponentCondition<NaturalAlignment>(id + 0x30, 3, comp => comp.CurMechanicProgress > 0, "Stack/spread")
                .ActivateOnEnter<TyrantsFlare>() // AOE casts start right after visual cast end
                .DeactivateOnExit<TyrantsFlare>(); // AOE casts end together with first NA proc

            CastMulti(id + 0x40, new AID[] { AID.AshingBlazeL, AID.AshingBlazeR }, 0.2f, 6, "Spread/stack") // second mechanic happens together with ashing blaze end
                .ActivateOnEnter<AshingBlaze>()
                .DeactivateOnExit<AshingBlaze>();

            ComponentCondition<NaturalAlignment>(id + 0x50, 3.7f, comp => comp.CurMechanicProgress == 0);
            ComponentCondition<EndOfDays>(id + 0x51, 0.5f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<EndOfDays>();
            // +1.5s: target 2 replaces 2552 0x1DE (ice first filling progress bars) or 0x1DC (fire first filling progress bars)
            // +2.0s: EndOfDays cast-start on 3 bad lanes
            // +6.0s: PATE on second 3 bad lanes

            ComponentCondition<NaturalAlignment>(id + 0x60, 7.6f, comp => comp.CurMechanicProgress > 0);
            ComponentCondition<EndOfDays>(id + 0x61, 0.5f, comp => comp.NumCasts > 0, "Fire/ice"); // second set of cast-starts immediately after

            ComponentCondition<NaturalAlignment>(id + 0x70, 5.6f, comp => comp.CurMechanicProgress > 1)
                .DeactivateOnExit<NaturalAlignment>();
            ComponentCondition<EndOfDays>(id + 0x71, 0.4f, comp => comp.Casters.Count == 0, "Ice/fire")
                .DeactivateOnExit<EndOfDays>();

            Aioniopyr(id + 0x1000, 0.9f);
        }

        private void NaturalAlignment2(uint id, float delay)
        {
            Cast(id, AID.NaturalAlignment, delay, 5)
                .ActivateOnEnter<NaturalAlignment>();
            // +1.1s: each target gets status 3412 (debuff) and 2552 0x209 (circle under player)

            Cast(id + 0x10, AID.InverseMagicks, 3.2f, 3);
            // +1.1s: 1 or 2 targets get status 3349

            // ~0.1s before next cast start, first NA activates (209 replaced with 1E1/1E3)
            Cast(id + 0x20, AID.TwistNature, 3.2f, 3);

            ComponentCondition<EndOfDays>(id + 0x30, 1, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<EndOfDays>();
            // +2.0s: NA progress bars start filling, EndOfDays cast start
            // +6.0s: PATE on second set of lanes
            ComponentCondition<NaturalAlignment>(id + 0x40, 8.1f, comp => comp.CurMechanicProgress > 0, "Stack/spread");

            ComponentCondition<NaturalAlignment>(id + 0x50, 6.1f, comp => comp.CurMechanicProgress > 1, "Spread/stack")
                .DeactivateOnExit<EndOfDays>(); // second set of cast-ends happens at the same time as second mechanic

            ComponentCondition<NaturalAlignment>(id + 0x60, 4.7f, comp => comp.CurMechanicProgress == 0);
            ComponentCondition<EndOfDays>(id + 0x61, 0.2f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<EndOfDays>();
            // +1.8s: NA progress bars start filling
            // +2.0s: EndOfDays cast-start on 3 bad lanes
            // +6.0s: PATE on second 3 bad lanes

            ComponentCondition<NaturalAlignment>(id + 0x70, 7.9f, comp => comp.CurMechanicProgress > 0);
            ComponentCondition<EndOfDays>(id + 0x71, 0.2f, comp => comp.NumCasts > 0, "Fire/ice"); // second set of cast-starts immediately after

            ComponentCondition<NaturalAlignment>(id + 0x80, 5.9f, comp => comp.CurMechanicProgress > 1, "Ice/fire")
                .DeactivateOnExit<NaturalAlignment>()
                .DeactivateOnExit<EndOfDays>();

            // end-of-days ends right after mechanic proc & ashing blaze start
            CastMulti(id + 0x90, new AID[] { AID.AshingBlazeL, AID.AshingBlazeR }, 0.1f, 6)
                .ActivateOnEnter<AshingBlaze>()
                .DeactivateOnExit<AshingBlaze>();

            Aioniopyr(id + 0x1000, 3.2f);
        }

        private void HighConcept1(uint id, float delay)
        {
            Cast(id, AID.HighConcept, delay, 5); // this is really weird...
            Targetable(id + 2, false, 4.4f, "High Concept 1")
                .ActivateOnEnter<HighConcept1>() // buffs appear right as boss becomes untargetable
                .SetHint(StateMachine.StateHint.Raidwide);

            Cast(id + 0x10, AID.ArcaneControl, 4.8f, 3, "Explosion 1");
            // +0.3s: first explosions (conceptual shift)
            // +1.0s: perfection status gains
            // +1.1s: first towers appear

            CastMulti(id + 0x30, new AID[] { AID.AshingBlazeL, AID.AshingBlazeR }, 6.2f, 6, "Towers 1") // tower explosions happen at the same time as cast-end
                .ActivateOnEnter<AshingBlaze>()
                .DeactivateOnExit<AshingBlaze>();

            Cast(id + 0x100, AID.ArcaneControl, 3.2f, 3, "Explosion 2");
            // +0.1s: second explosions (conceptual shift)
            // +0.7s: perfection status gains
            // +1.1s: second towers appear

            CastMulti(id + 0x120, new AID[] { AID.AshingBlazeL, AID.AshingBlazeR }, 6.2f, 6, "Towers 2") // tower explosions happen at the same time as cast-end
                .ActivateOnEnter<AshingBlaze>()
                .DeactivateOnExit<AshingBlaze>();

            Targetable(id + 0x200, true, 3.1f, "Reappear");
            Cast(id + 0x210, AID.Deconceptualize, 0.1f, 3)
                .DeactivateOnExit<HighConcept1>();

            Aioniopyr(id + 0x1000, 3.2f);
        }

        private void HighConcept2(uint id, float delay)
        {
            Cast(id, AID.HighConcept, delay, 5); // this is really weird...
            Targetable(id + 2, false, 4.4f, "High Concept 2")
                .ActivateOnEnter<HighConcept2>() // buffs appear right as boss becomes untargetable
                .SetHint(StateMachine.StateHint.Raidwide);

            Cast(id + 0x10, AID.ArcaneControl, 4.7f, 3, "Explosion 1");
            // +0.3s: first explosions (conceptual shift)
            // +1.0s: perfection status gains
            // +1.1s: first towers appear (always blue or purple)

            CastMulti(id + 0x30, new AID[] { AID.AshingBlazeL, AID.AshingBlazeR }, 6.2f, 6, "Towers 1") // tower explosions happen at the same time as cast-end
                .ActivateOnEnter<AshingBlaze>()
                .DeactivateOnExit<AshingBlaze>();

            Cast(id + 0x100, AID.ArcaneControl, 3.2f, 3, "Explosion 2");
            // +0.0s: PATE 11D3 on IllusoryHephaistosMovable (appear)
            // +0.1s: second explosions (conceptual shift)
            // +0.7s: perfection status gains
            // +1.1s: second towers appear (ENVC 00020001 index 34/2B/36/2D/ ???)

            ComponentCondition<EndOfDaysTethered>(id + 0x110, 7.1f, comp => comp.Active, "Tethers")
                .ActivateOnEnter<EndOfDaysTethered>();
            ComponentCondition<EndOfDaysTethered>(id + 0x111, 5.1f, comp => !comp.Active, "Towers 2") // tower explosions happen at the same time as end-of-days
                .DeactivateOnExit<EndOfDaysTethered>();

            Targetable(id + 0x200, true, 3.9f, "Reappear")
                .DeactivateOnExit<HighConcept2>(); // not sure, current implementation can be deactivated earlier, but maybe we want to show phoenix merge hints?..
        }

        private void LimitlessDesolation(uint id, float delay)
        {
            Cast(id, AID.LimitlessDesolation, delay, 5)
                .ActivateOnEnter<LimitlessDesolation>(); // show spreads slightly in advance

            ComponentCondition<LimitlessDesolation>(id + 0x10, 1.1f, comp => comp.NumAOEs > 0, "Limitless desolation start");
            ComponentCondition<LimitlessDesolation>(id + 0x11, 0.9f, comp => comp.NumTowers > 0);
            ComponentCondition<LimitlessDesolation>(id + 0x12, 2.1f, comp => comp.NumAOEs > 2);
            ComponentCondition<LimitlessDesolation>(id + 0x13, 0.9f, comp => comp.NumTowers > 2);
            ComponentCondition<LimitlessDesolation>(id + 0x14, 2.1f, comp => comp.NumAOEs > 4);
            ComponentCondition<LimitlessDesolation>(id + 0x15, 0.9f, comp => comp.NumTowers > 4);
            ComponentCondition<LimitlessDesolation>(id + 0x16, 2.1f, comp => comp.NumAOEs > 6)
                .ActivateOnEnter<LimitlessDesolationTyrantsFlare>();
            ComponentCondition<LimitlessDesolation>(id + 0x17, 0.9f, comp => comp.NumTowers > 6);
            ComponentCondition<LimitlessDesolation>(id + 0x20, 1.0f, comp => comp.NumBursts > 0);
            ComponentCondition<LimitlessDesolation>(id + 0x21, 3.0f, comp => comp.NumBursts > 2);
            ComponentCondition<LimitlessDesolation>(id + 0x22, 3.0f, comp => comp.NumBursts > 4);
            ComponentCondition<LimitlessDesolation>(id + 0x23, 3.0f, comp => comp.NumBursts > 6)
                .DeactivateOnExit<LimitlessDesolationTyrantsFlare>()
                .DeactivateOnExit<LimitlessDesolation>();

            Aioniopyr(id + 0x1000, 1.9f);
        }

        private void EgoDeath(uint id, float delay)
        {
            Cast(id, AID.EgoDeath, delay, 10);
            ComponentCondition<EgoDeath>(id + 0x10, 2.2f, comp => comp.InEventMask.Any(), "Cutscene start")
                .ActivateOnEnter<EgoDeath>()
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<EgoDeath>(id + 0x11, 15.1f, comp => comp.InEventMask.None(), "Cutscene end")
                .DeactivateOnExit<EgoDeath>()
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void Aionagonia(uint id, float delay)
        {
            Cast(id, AID.Aionagonia, delay, 8, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void DominionAionagonia(uint id, float delay)
        {
            Cast(id, AID.Dominion, delay, 7, "Raidwide")
                .ActivateOnEnter<Dominion>() // activate early to show spreads
                .SetHint(StateMachine.StateHint.Raidwide);

            ComponentCondition<Dominion>(id + 0x10, 1.1f, comp => comp.NumDeformations > 0, "Spread");
            // +6.0s: cast-start for second set of shifts
            ComponentCondition<Dominion>(id + 0x20, 7.0f, comp => comp.NumShifts > 0, "Towers 1");

            CastStart(id + 0x30, AID.Aionagonia, 3.1f);
            ComponentCondition<Dominion>(id + 0x31, 2.9f, comp => comp.NumShifts > 4, "Towers 2")
                .DeactivateOnExit<Dominion>();
            CastEnd(id + 0x32, 5.1f, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }
    }
}
