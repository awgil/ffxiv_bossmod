namespace BossMod.Endwalker.Savage.P10SPandaemonium
{
    class P10SPandaemoniumStates : StateMachineBuilder
    {
        public P10SPandaemoniumStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase)
                .ActivateOnEnter<Border>(); // keep active throught the fight, since it overlaps with subsequent mechanics
        }

        private void SinglePhase(uint id)
        {
            Ultima(id, 8.4f);
            SoulGrasp(id + 0x10000, 4.2f);
            DividingWings1(id + 0x20000, 2.8f);
            WickedStepPandaemoniacPillars(id + 0x30000, 2.7f);
            Silkspit(id + 0x40000, 2.0f);
            DaemoniacBondsPandaemoniacMeltdownTouchdown(id + 0x50000, 1.4f);
            Ultima(id + 0x60000, 5.9f);
            SoulGrasp(id + 0x70000, 4.2f);
            DaemoniacBondsPandaemoniacTurrets(id + 0x80000, 7.5f);
            Ultima(id + 0x90000, 6.6f);
            SoulGrasp(id + 0xA0000, 4.2f);
            WickedStepSilkspit(id + 0xB0000, 7.5f);
            DividingWings2(id + 0xC0000, 1.4f);
            SoulGrasp(id + 0xD0000, 3.6f);
            DividingWings3(id + 0xE0000, 11.6f);
            Ultima(id + 0xF0000, 13.2f);
            SoulGrasp(id + 0x100000, 5.2f);
            WickedStepEntanglingWeb(id + 0x110000, 10.7f);
            PartedPlumesPandaemoniacRay(id + 0x120000, 6.7f);
            Silkspit(id + 0x130000, 3.2f);
            PandaemoniacPillarsTurrets(id + 0x140000, 2.4f, AID.PandaemoniacPillars);
            CirclesHolyPillars(id + 0x150000, 1.5f);
            PandaemoniacMeltdown(id + 0x160000, 2.0f);
            HarrowingHell(id + 0x170000, 17.1f, true);
        }

        private void Ultima(uint id, float delay)
        {
            Cast(id, AID.Ultima, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void SoulGrasp(uint id, float delay)
        {
            CastStart(id, AID.SoulGrasp, delay)
                .ActivateOnEnter<SoulGrasp>(); // icon appears right before cast start
            CastEnd(id + 1, 5);
            ComponentCondition<SoulGrasp>(id + 0x10, 0.8f, comp => comp.NumCasts >= 1, "Tankbuster hit 1")
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<SoulGrasp>(id + 0x11, 1.6f, comp => comp.NumCasts >= 2)
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<SoulGrasp>(id + 0x12, 1.6f, comp => comp.NumCasts >= 3)
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<SoulGrasp>(id + 0x13, 1.6f, comp => comp.NumCasts >= 4, "Tankbuster hit 4")
                .DeactivateOnExit<SoulGrasp>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private State CirclesHoly(uint id, float delay)
        {
            return CastMulti(id, new[] { AID.PandaemonsHoly, AID.CirclesOfPandaemonium }, delay, 4, "In/out")
                .ActivateOnEnter<PandaemonsHoly>()
                .ActivateOnEnter<CirclesOfPandaemonium>()
                .DeactivateOnExit<PandaemonsHoly>()
                .DeactivateOnExit<CirclesOfPandaemonium>();
        }

        private void CirclesHolyPillars(uint id, float delay)
        {
            CirclesHoly(id, delay)
                .ActivateOnEnter<Imprisonment>() // note: these start ~1.1s later, theoretically we can predict that by looking at previous bury casts...
                .ActivateOnEnter<Cannonspawn>()
                .ActivateOnEnter<PealOfDamnation>();
            ComponentCondition<Imprisonment>(id + 0x10, 0.7f, comp => comp.NumCasts > 0, "Pillars")
                .DeactivateOnExit<PealOfDamnation>() // this ends ~0.5s earlier, but who cares
                .DeactivateOnExit<Imprisonment>()
                .DeactivateOnExit<Cannonspawn>();
        }

        private void WickedStep(uint id, float delay)
        {
            Cast(id, AID.WickedStep, delay, 6)
                .ActivateOnEnter<WickedStep>();
            ComponentCondition<WickedStep>(id + 0x10, 1.1f, comp => comp.NumCasts >= 1, "Tower L")
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<WickedStep>(id + 0x20, 2.4f, comp => comp.NumCasts >= 2, "Tower R")
                .DeactivateOnExit<WickedStep>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void EntanglingWeb(uint id, float delay)
        {
            Cast(id, AID.EntanglingWeb, delay, 3);
            ComponentCondition<EntanglingWebAOE>(id + 0x10, 8, comp => comp.Casters.Count > 0, "Web bait")
                .ActivateOnEnter<EntanglingWebHints>()
                .ActivateOnEnter<EntanglingWebAOE>()
                .DeactivateOnExit<EntanglingWebHints>();
            ComponentCondition<EntanglingWebAOE>(id + 0x20, 3, comp => comp.NumCasts > 0, "Web resolve")
                .DeactivateOnExit<EntanglingWebAOE>();
        }

        private void PandaemoniacPillarsTurrets(uint id, float delay, AID aid)
        {
            Cast(id, aid, delay, 5)
                .ActivateOnEnter<PandaemoniacPillars>();
            ComponentCondition<PandaemoniacPillars>(id + 0x10, 1.2f, comp => comp.NumCasts > 0, "Towers")
                .DeactivateOnExit<PandaemoniacPillars>();
        }

        private State PandaemoniacMeltdown(uint id, float delay)
        {
            CastStart(id, AID.PandaemoniacMeltdown, delay)
                .ActivateOnEnter<PandaemoniacMeltdown>(); // icons appear right before cast start
            CastEnd(id + 1, 5);
            return ComponentCondition<PandaemoniacMeltdown>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Line stack/spread")
                .DeactivateOnExit<PandaemoniacMeltdown>();
        }

        private void Touchdown(uint id, float delay)
        {
            Cast(id, AID.Touchdown, delay, 8)
                .ActivateOnEnter<Touchdown>();
            ComponentCondition<Touchdown>(id + 2, 1, comp => comp.NumCasts > 0)
                .DeactivateOnExit<Touchdown>();
        }

        private void Silkspit(uint id, float delay)
        {
            Cast(id, AID.Silkspit, delay, 3);
            ComponentCondition<Silkspit>(id + 0x10, 1.9f, comp => comp.Active)
                .ActivateOnEnter<Silkspit>();
            ComponentCondition<Silkspit>(id + 0x20, 8.1f, comp => !comp.Active, "Silkspit")
                .DeactivateOnExit<Silkspit>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void DaemoniacBondsCast(uint id, float delay)
        {
            Cast(id, AID.DaemoniacBonds, delay, 3)
                .ActivateOnEnter<DaemoniacBonds>(); // statuses appear ~0.7s after cast end
        }

        private void DaemoniacBondsResolve(uint id, float delay)
        {
            ComponentCondition<DaemoniacBonds>(id, delay, comp => comp.NumMechanics >= 1, "Stack/spread");
            ComponentCondition<DaemoniacBonds>(id + 1, 3, comp => comp.NumMechanics >= 2, "Spread/stack")
                .DeactivateOnExit<DaemoniacBonds>();
        }

        private void PandaemoniacRay(uint id, float delay)
        {
            CastMulti(id, new[] { AID.PandaemoniacRayL, AID.PandaemoniacRayR }, delay, 5)
                .ActivateOnEnter<PandaemoniacRay>();
            ComponentCondition<PandaemoniacRay>(id + 2, 0.2f, comp => comp.NumCasts > 0, "Half-room cleave")
                .DeactivateOnExit<PandaemoniacRay>();
            ComponentCondition<JadePassage>(id + 0x10, 3.6f, comp => comp.NumCasts > 0, "Lines")
                .ActivateOnEnter<JadePassage>()
                .DeactivateOnExit<JadePassage>();
        }

        private void PartedPlumesPandaemoniacRay(uint id, float delay)
        {
            // overlaps with ray & circle/holy
            Cast(id, AID.PartedPlumes, delay, 3);
            CastStartMulti(id + 0x10, new[] { AID.PandaemoniacRayL, AID.PandaemoniacRayR }, 7.3f)
                .ActivateOnEnter<PartedPlumes>(); // first aoe cast start 3.8s after previous cast end, individual aoes are 0.3s apart
            ComponentCondition<PartedPlumes>(id + 0x20, 0.5f, comp => comp.NumCasts > 0, "Plumes start")
                .ActivateOnEnter<PandaemoniacRay>();
            ComponentCondition<PartedPlumes>(id + 0x30, 2.4f, comp => comp.Casters.Count == 0, "Plumes end")
                .DeactivateOnExit<PartedPlumes>();
            CastEnd(id + 0x40, 2.1f);
            ComponentCondition<PandaemoniacRay>(id + 0x41, 0.2f, comp => comp.NumCasts > 0, "Half-room cleave")
                .DeactivateOnExit<PandaemoniacRay>();
            CastStartMulti(id + 0x50, new[] { AID.PandaemonsHoly, AID.CirclesOfPandaemonium }, 3)
                .ActivateOnEnter<JadePassage>();
            ComponentCondition<JadePassage>(id + 0x60, 0.6f, comp => comp.NumCasts > 0, "Lines")
                .ActivateOnEnter<PandaemonsHoly>()
                .ActivateOnEnter<CirclesOfPandaemonium>()
                .DeactivateOnExit<JadePassage>();
            CastEnd(id + 0x70, 3.3f, "In/out")
                .DeactivateOnExit<PandaemonsHoly>()
                .DeactivateOnExit<CirclesOfPandaemonium>();
        }

        private void WickedStepEntanglingWeb(uint id, float delay)
        {
            WickedStep(id, delay);
            EntanglingWeb(id + 0x1000, 3.6f);
        }

        private void WickedStepPandaemoniacPillars(uint id, float delay)
        {
            // these happen in quick succession, makes sense to group them together
            WickedStep(id, delay);
            EntanglingWeb(id + 0x1000, 4.7f);
            PandaemoniacPillarsTurrets(id + 0x2000, 1.4f, AID.PandaemoniacPillars);
            CirclesHolyPillars(id + 0x3000, 1.5f);
        }

        private void WickedStepSilkspit(uint id, float delay)
        {
            WickedStep(id, delay);

            // overlap of entangling web, silkspit and daemoniac bonds mechanics
            Cast(id + 0x1000, AID.EntanglingWeb, 4.7f, 3);
            CastStart(id + 0x1010, AID.Silkspit, 7.3f)
                .ActivateOnEnter<EntanglingWebHints>()
                .ActivateOnEnter<EntanglingWebAOE>();
            ComponentCondition<EntanglingWebAOE>(id + 0x1011, 0.7f, comp => comp.Casters.Count > 0, "Web bait")
                .DeactivateOnExit<EntanglingWebHints>();
            CastEnd(id + 0x1012, 2.3f);
            ComponentCondition<EntanglingWebAOE>(id + 0x1013, 0.7f, comp => comp.NumCasts > 0, "Web resolve")
                .DeactivateOnExit<EntanglingWebAOE>();
            ComponentCondition<Silkspit>(id + 0x1020, 1.2f, comp => comp.Active)
                .ActivateOnEnter<Silkspit>();
            DaemoniacBondsCast(id + 0x1030, 4.2f);
            ComponentCondition<Silkspit>(id + 0x1040, 0.8f, comp => !comp.Active, "Silkspit")
                .DeactivateOnExit<Silkspit>()
                .SetHint(StateMachine.StateHint.Raidwide);

            PandaemoniacPillarsTurrets(id + 0x2000, 2.3f, AID.PandaemoniacPillars);
            CirclesHolyPillars(id + 0x3000, 1.5f);

            // overlap of pandaemoniac ray and daemoniac bonds resolve
            CastStartMulti(id + 0x4000, new[] { AID.PandaemoniacRayL, AID.PandaemoniacRayR }, 2.0f)
                .OnEnter(() => Module.FindComponent<DaemoniacBonds>()?.Show());
            ComponentCondition<DaemoniacBonds>(id + 0x4010, 4.4f, comp => comp.NumMechanics >= 1, "Stack/spread")
                .ActivateOnEnter<PandaemoniacRay>();
            CastEnd(id + 0x4020, 0.6f);
            ComponentCondition<PandaemoniacRay>(id + 0x4030, 0.2f, comp => comp.NumCasts > 0, "Half-room cleave")
                .DeactivateOnExit<PandaemoniacRay>();
            ComponentCondition<DaemoniacBonds>(id + 0x4040, 3.2f, comp => comp.NumMechanics >= 2, "Spread/stack")
                .ActivateOnEnter<JadePassage>()
                .DeactivateOnExit<DaemoniacBonds>();
            ComponentCondition<JadePassage>(id + 0x4050, 0.4f, comp => comp.NumCasts > 0, "Lines")
                .DeactivateOnExit<JadePassage>();
        }

        private State HarrowingHell(uint id, float delay, bool isEnrage)
        {
            Cast(id, AID.HarrowingHell, delay, 5);
            ComponentCondition<HarrowingHell>(id + 0x11, 1.0f, comp => comp.NumCasts >= 1, "Hell start")
                .ActivateOnEnter<HarrowingHell>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<HarrowingHell>(id + 0x12, 1.9f, comp => comp.NumCasts >= 2)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<HarrowingHell>(id + 0x13, 1.7f, comp => comp.NumCasts >= 3)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<HarrowingHell>(id + 0x14, 1.7f, comp => comp.NumCasts >= 4)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<HarrowingHell>(id + 0x15, 1.7f, comp => comp.NumCasts >= 5)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<HarrowingHell>(id + 0x16, 1.5f, comp => comp.NumCasts >= 6)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<HarrowingHell>(id + 0x17, 1.4f, comp => comp.NumCasts >= 7)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<HarrowingHell>(id + 0x18, 1.3f, comp => comp.NumCasts >= 8)
                .SetHint(StateMachine.StateHint.Raidwide);
            return ComponentCondition<HarrowingHell>(id + 0x20, 3.9f, comp => comp.NumCasts >= 9, isEnrage ? "Enrage" : "Hell resolve")
                .DeactivateOnExit<HarrowingHell>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void DividingWings1(uint id, float delay)
        {
            Cast(id, AID.DividingWings, delay, 3);
            Cast(id + 0x10, AID.SteelWeb, 3.2f, 3);
            ComponentCondition<DividingWings>(id + 0x20, 7.7f, comp => comp.NumCasts > 0, "Tethers")
                .ActivateOnEnter<DividingWings>()
                .ActivateOnEnter<SteelWebStack>()
                .DeactivateOnExit<DividingWings>();
            ComponentCondition<SteelWebStack>(id + 0x30, 0.3f, comp => !comp.Active, "Web")
                .ActivateOnEnter<SteelWebTethers>()
                .DeactivateOnExit<SteelWebStack>();
            CirclesHoly(id + 0x1000, 6.1f)
                .DeactivateOnExit<SteelWebTethers>();
        }

        private void DividingWings2(uint id, float delay)
        {
            Cast(id, AID.DividingWings, delay, 3)
                .ActivateOnEnter<DividingWings>();
            Cast(id + 0x10, AID.SteelWeb, 3.2f, 3)
                .ActivateOnEnter<SteelWebStack>();
            Cast(id + 0x20, AID.Touchdown, 4.1f, 8)
                .ActivateOnEnter<Touchdown>();
            ComponentCondition<DividingWings>(id + 0x30, 0.6f, comp => comp.NumCasts > 0, "Tethers")
                .DeactivateOnExit<DividingWings>();
            ComponentCondition<SteelWebStack>(id + 0x40, 0.3f, comp => !comp.Active, "Web")
                .ActivateOnEnter<SteelWebTethers>()
                .DeactivateOnExit<SteelWebStack>();
            ComponentCondition<Touchdown>(id + 0x50, 0.1f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<Touchdown>();
            PandaemoniacMeltdown(id + 0x1000, 12.5f)
                .DeactivateOnExit<SteelWebTethers>();
        }

        private void DividingWings3(uint id, float delay)
        {
            Cast(id, AID.DividingWings, delay, 3);
            Cast(id + 0x10, AID.PandaemoniacWeb, 3.2f, 3);

            CastStart(id + 0x100, AID.PandaemonsHoly, 4.1f)
                .ActivateOnEnter<DividingWings>()
                .ActivateOnEnter<SteelWebStack>()
                .ActivateOnEnter<EntanglingWebHints>()
                .ActivateOnEnter<EntanglingWebAOE>();
            ComponentCondition<DividingWings>(id + 0x110, 3.5f, comp => comp.NumCasts > 0, "Tethers")
                .ActivateOnEnter<PandaemonsHoly>()
                .DeactivateOnExit<DividingWings>();
            ComponentCondition<SteelWebStack>(id + 0x120, 0.4f, comp => !comp.Active, "Web stack & bait 1") // steel web resolve + entangling web 1 baits
                .ActivateOnEnter<SteelWebTethers>()
                .DeactivateOnExit<SteelWebStack>();
            CastEnd(id + 0x130, 0.1f)
                .DeactivateOnExit<PandaemonsHoly>();
            ComponentCondition<EntanglingWebAOE>(id + 0x140, 2.8f, comp => comp.NumCasts > 0);

            CastStart(id + 0x200, AID.DaemoniacBonds, 1.3f);
            ComponentCondition<EntanglingWebAOE>(id + 0x210, 1.7f, comp => comp.Casters.Count > 0, "Web bait 2")
                .DeactivateOnExit<EntanglingWebHints>();
            CastEnd(id + 0x220, 1.3f);
            ComponentCondition<EntanglingWebAOE>(id + 0x230, 1.7f, comp => comp.Casters.Count == 0)
                .ActivateOnEnter<DaemoniacBonds>()
                .DeactivateOnExit<EntanglingWebAOE>();

            HarrowingHell(id + 0x300, 4.6f, false)
                .DeactivateOnExit<SteelWebTethers>() // TODO: this can be deactivated much earlier?
                .OnExit(() => Module.FindComponent<DaemoniacBonds>()?.Show());

            DaemoniacBondsResolve(id + 0x400, 5.4f);
        }

        private void DaemoniacBondsPandaemoniacMeltdownTouchdown(uint id, float delay)
        {
            DaemoniacBondsCast(id, delay);
            PandaemoniacMeltdown(id + 0x100, 4.2f)
                .OnExit(() => Module.FindComponent<DaemoniacBonds>()?.Show());
            Touchdown(id + 0x200, 3.6f);
            DaemoniacBondsResolve(id + 0x300, 0.5f);
        }

        private void DaemoniacBondsPandaemoniacTurrets(uint id, float delay)
        {
            DaemoniacBondsCast(id, delay);
            PandaemoniacPillarsTurrets(id + 0x100, 4.2f, AID.PandaemoniacTurrets);
            ComponentCondition<Turrets>(id + 0x200, 10.7f, comp => comp.NumCasts > 0, "Knockback 1")
                .ActivateOnEnter<Turrets>();
            ComponentCondition<Turrets>(id + 0x201, 4.5f, comp => comp.NumCasts > 2, "Knockback 2");
            ComponentCondition<Turrets>(id + 0x202, 4.5f, comp => comp.NumCasts > 4, "Knockback 3");
            ComponentCondition<Turrets>(id + 0x203, 4.5f, comp => comp.NumCasts > 6, "Knockback 4")
                .DeactivateOnExit<Turrets>()
                .OnExit(() => Module.FindComponent<DaemoniacBonds>()?.Show());
            DaemoniacBondsResolve(id + 0x300, 4.3f);
            PandaemoniacRay(id + 0x400, 1.8f);
        }
    }
}
