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
            AionagoniaDominion(id + 0xB0000, 9.2f);
            Cast(id + 0xC0000, AID.Enrage, 7.1f, 16, "Enrage");
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

        // TODO: component...
        private void HighConcept1(uint id, float delay)
        {
            Cast(id, AID.HighConcept, delay, 5); // this is really weird...
            Targetable(id + 2, false, 4.4f, "High Concept 1")
                .SetHint(StateMachine.StateHint.Raidwide);
            // buffs appear right as boss becomes untargetable

            Cast(id + 0x10, AID.ArcaneControl, 4.8f, 3);
            // +0.3s: first explosions (conceptual shift)
            // +1.0s: perfection status gains
            // +1.1s: first towers appear (ENVC 00020001 index 1E/1F for purple, 28/29 for blue, 32/33 for green)

            CastMulti(id + 0x30, new AID[] { AID.AshingBlazeL, AID.AshingBlazeR }, 6.2f, 6, "Towers 1") // tower explosions happen at the same time as cast-end
                .ActivateOnEnter<AshingBlaze>()
                .DeactivateOnExit<AshingBlaze>();

            Cast(id + 0x100, AID.ArcaneControl, 3.2f, 3);
            // +0.1s: second explosions (conceptual shift)
            // +0.7s: perfection status gains
            // +1.1s: second towers appear (ENVC 00020001 index 1A-1D for purple, 24-27 for blue, 2E-31 for green)

            CastMulti(id + 0x120, new AID[] { AID.AshingBlazeL, AID.AshingBlazeR }, 6.2f, 6, "Towers 2") // tower explosions happen at the same time as cast-end
                .ActivateOnEnter<AshingBlaze>()
                .DeactivateOnExit<AshingBlaze>();

            Targetable(id + 0x200, true, 3.1f, "Reappear");
            Cast(id + 0x210, AID.Deconceptualize, 0.1f, 3);

            Aioniopyr(id + 0x1000, 3.2f);
        }

        // TODO: component...
        private void HighConcept2(uint id, float delay)
        {
            // second tower ENVC
            // 34/2B/36/2D = (95,105) green, (95, 95) green, (105, 105) blue, (105, 95) blue

            Cast(id, AID.HighConcept, delay, 5); // this is really weird...
            Targetable(id + 2, false, 4.4f, "High Concept 2")
                .SetHint(StateMachine.StateHint.Raidwide);
            // buffs appear right as boss becomes untargetable

            Cast(id + 0x10, AID.ArcaneControl, 4.7f, 3);
            // +0.3s: first explosions (conceptual shift)
            // +1.0s: perfection status gains
            // +1.1s: first towers appear (ENVC 00020001 index 1E/1F for purple - always same?)

            CastMulti(id + 0x30, new AID[] { AID.AshingBlazeL, AID.AshingBlazeR }, 6.2f, 6, "Towers 1") // tower explosions happen at the same time as cast-end
                .ActivateOnEnter<AshingBlaze>()
                .DeactivateOnExit<AshingBlaze>();

            Cast(id + 0x100, AID.ArcaneControl, 3.2f, 3);
            // +0.0s: PATE 11D3 on IllusoryHephaistosMovable (appear)
            // +0.1s: second explosions (conceptual shift)
            // +0.7s: perfection status gains
            // +1.1s: second towers appear (ENVC 00020001 index 34/2B/36/2D/ ???)
            // +7.1s: targeting
            // +12.2s: towers 2 + end-of-days

            Targetable(id + 0x200, true, 16.1f, "Reappear");
        }

        // TODO: component...
        private void LimitlessDesolation(uint id, float delay)
        {
            // tower ENVC:
            // 00020001 = appear
            // 00200010 = occupied
            // 00400001 = unoccupied
            // 00080004 = explode
            // 4C = (85, 85) (extra 46)
            // 4D = (95, 85) (extra 47)
            // 4E = (105, 85) (extra 48)
            // 4F = (115, 85) (extra 49)
            // 50 = (85, 95) (extra 4A)
            // ?? = (95, 95) (extra ??)
            // 0A = (105, 95) (extra 06)
            // 51 = (115, 95) (extra 4B)
            // 54 = (85, 105) (extra 52)
            // 0B = (95, 105) (extra 07)
            // 0C = (105, 105)(extra 08)
            // 55 = (115, 105) (extra 53)

            Cast(id, AID.LimitlessDesolation, delay, 5);
            // +1.1s: aoe 1
            // +2.1s: towers 1 appear (ENVC 00020001 index 46/4C/48/4E -> (105, 85), (85, 85); ???)
            // +4.2s: aoe 2
            // +5.2s: towers 2 appear (ENVC 00020001 index 4B/51/4A/50 -> (115, 95), (85, 95); ???)
            // +7.2s: aoe 3
            // +8.2s: towers 3 appear (ENVC 00020001 index 47/4D/06/0A/???)
            // +9.2s: puddles 1 start
            // +10.2s: aoe 4
            // +11.2s: towers 4 appear (ENVC 00020001 index 52/54/53/55/???)
            // +12.2s: puddles 1 end, towers 1 explode, puddles 2 start
            // +15.2s: puddles 2 end, towers 2 explode, puddles 3 start
            // +18.2s: puddles 3 end, towers 3 explode, puddles 4 start
            // +21.2s: puddles 4 end, towers 4 explode

            Aioniopyr(id + 0x1000, 23.2f);
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

        private void Dominion(uint id, float delay)
        {
            Cast(id, AID.Dominion, delay, 7, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        // TODO: component...
        private void AionagoniaDominion(uint id, float delay)
        {
            Aionagonia(id, delay);

            Dominion(id + 0x1000, 3.2f);
            // +1.1s: 4x deformation + 4x shift cast-start
            // +7.1s: 4x shift cast-start (second set)
            // +8.1s: first set of towers
            // +14.1s: second set of towers (during next raidwide)
            Aionagonia(id + 0x2000, 11.2f);

            Dominion(id + 0x3000, 3.2f);
            // repeat...
            Aionagonia(id + 0x4000, 11.2f);
        }
    }
}
