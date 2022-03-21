namespace BossMod.Endwalker.P4S1
{
    class P4S1States : StateMachineBuilder
    {
        public P4S1States(BossModule module) : base(module)
        {
            Decollation(0x00000000, 9.3f);
            BloodrakeBelone(0x00010000, 4.2f);
            Decollation(0x00020000, 3.4f);
            ElegantEvisceration(0x00030000, 4.2f);

            Pinax(0x00100000, 11.3f, true);
            ElegantEvisceration(0x00110000, 4.4f);

            VengefulElementalBelone(0x00200000, 4.2f);

            BeloneCoils(0x00300000, 8.2f);
            Decollation(0x00310000, 3.4f);
            ElegantEvisceration(0x00320000, 4.2f);

            Pinax(0x00400000, 11.3f, false);
            Decollation(0x00410000, 0); // note: cast starts ~0.2s before pinax resolve, whatever...
            Decollation(0x00420000, 4.2f);
            Decollation(0x00430000, 4.2f);
            Targetable(0x00440000, false, 10, "Enrage"); // checkpoint is triggered by boss becoming untargetable...
        }

        private void Decollation(uint id, float delay)
        {
            var s = Cast(id, AID.Decollation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void ElegantEvisceration(uint id, float delay)
        {
            var cast = Cast(id, AID.ElegantEvisceration, delay, 5, "Tankbuster");
            cast.Exit.Add(Module.ActivateComponent<ElegantEvisceration>);
            cast.EndHint |= StateMachine.StateHint.Tankbuster;

            var second = ComponentCondition<ElegantEvisceration>(id + 2, 3.2f, comp => comp.NumCasts > 0, "Tankbuster");
            second.Exit.Add(Module.DeactivateComponent<ElegantEvisceration>);
            second.EndHint |= StateMachine.StateHint.Tankbuster;
        }

        private StateMachine.State InversiveChlamys(uint id, float delay)
        {
            Cast(id, AID.InversiveChlamys, delay, 7);
            var resolve = ComponentCondition<InversiveChlamys>(id + 2, 0.8f, comp => !comp.TethersActive, "Chlamys"); // explosion happens when tethers disappear, shortly after cast end
            return resolve;
        }

        private void BloodrakeBelone(uint id, float delay)
        {
            // note: just before (~0.1s) every bloodrake cast start, its targets are tethered to boss
            // targets of first bloodrake will be killed if they are targets of chlamys tethers later
            var bloodrake1 = Cast(id, AID.Bloodrake, delay, 4, "Bloodrake 1");
            bloodrake1.Enter.Add(Module.ActivateComponent<InversiveChlamys>);

            // this cast is pure flavour and does nothing (replaces status 2799 'Aethersucker' with status 2800 'Casting Chlamys' on boss)
            Cast(id + 0x1000, AID.AethericChlamys, 3.2f, 4);

            // targets of second bloodrake will be killed if they are targets of 'Cursed Casting' (which targets players with 'Role Call')
            var bloodrake2 = Cast(id + 0x2000, AID.Bloodrake, 4.2f, 4, "Bloodrake 2");
            bloodrake2.Enter.Add(Module.ActivateComponent<DirectorsBelone>);

            // this cast removes status 2799 'Aethersucker' from boss
            // right after it ends, instant cast 27111 applies 'Role Call' debuffs - corresponding component handles that
            var beloneStart = CastStart(id + 0x3000, AID.DirectorsBelone, 4.2f);
            beloneStart.EndHint |= StateMachine.StateHint.PositioningStart;

            CastEnd(id + 0x3001, 5);

            // Cursed Casting happens right before (0.5s) chlamys resolve
            var inv = InversiveChlamys(id + 0x4000, 9.2f);
            inv.Exit.Add(Module.DeactivateComponent<InversiveChlamys>);
            inv.Exit.Add(Module.DeactivateComponent<DirectorsBelone>);
            inv.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void Pinax(uint id, float delay, bool keepScene)
        {
            var setting = Cast(id, AID.SettingTheScene, delay, 4, "Scene");
            setting.Exit.Add(Module.ActivateComponent<SettingTheScene>);
            setting.Exit.Add(Module.ActivateComponent<PinaxUptime>);
            setting.EndHint |= StateMachine.StateHint.PositioningStart;
            // ~1s after cast end, we get a bunch of env controls

            var pinaxStart = CastStart(id + 0x1000, AID.Pinax, 8.2f);
            pinaxStart.Exit.Add(Module.DeactivateComponent<PinaxUptime>);
            pinaxStart.EndHint |= StateMachine.StateHint.PositioningEnd;

            var pinaxEnd = CastEnd(id + 0x1001, 5, "Pinax");
            pinaxEnd.Exit.Add(Module.ActivateComponent<Pinax>);
            pinaxEnd.EndHint |= StateMachine.StateHint.PositioningStart;
            // timeline:
            //  0.0s pinax cast end
            //  1.0s square 1 activation: env control (.10 = 00800040), helper starts casting 27095
            //  4.0s square 2 activation: env control (.15 = 00800040), helper starts casting 27092
            //  7.0s square 1 env control (.10 = 02000001)
            // 10.0s square 2 env control (.15 = 02000001)
            //       square 1 cast finish (+ instant 27091)
            // 13.0s square 2 cast finish (+ instant 27088)
            // 14.0s square 3 activation: env control (.20 = 00800040), helper starts casting 27094
            // 20.0s square 3 env control (.20 = 02000001)
            // 23.0s square 3 cast finish (+ instant 27090)
            // 25.0s square 4 activation: env control (.05 = 00800040), helper starts casting 27093
            // 31.0s square 4 env control (.05 = 02000001)
            // 34.0s square 4 cast finish (+ instant 27089)

            var p1 = ComponentCondition<Pinax>(id + 0x2000, 10, comp => comp.NumFinished == 1, "Corner1");

            var p2 = ComponentCondition<Pinax>(id + 0x3000, 3, comp => comp.NumFinished == 2, "Corner2");
            p2.EndHint |= StateMachine.StateHint.PositioningEnd;

            var shiftStart = CastStartMulti(id + 0x4000, new AID[] { AID.NortherlyShiftCloak, AID.SoutherlyShiftCloak, AID.EasterlyShiftCloak, AID.WesterlyShiftCloak, AID.NortherlyShiftSword, AID.SoutherlyShiftSword, AID.EasterlyShiftSword, AID.WesterlyShiftSword }, 3.6f);
            shiftStart.Exit.Add(Module.ActivateComponent<Shift>); // together with this, one of the helpers starts casting 27142 or 27137
            shiftStart.EndHint |= StateMachine.StateHint.PositioningStart;

            var p3 = ComponentCondition<Pinax>(id + 0x5000, 6.4f, comp => comp.NumFinished == 3, "Corner3");

            var shiftEnd = CastEnd(id + 0x6000, 1.6f, "Shift");

            var p4 = ComponentCondition<Pinax>(id + 0x7000, 9.4f, comp => comp.NumFinished == 4, "Pinax resolve");
            if (!keepScene)
                p4.Exit.Add(Module.DeactivateComponent<SettingTheScene>);
            p4.Exit.Add(Module.DeactivateComponent<Pinax>);
            p4.Exit.Add(Module.DeactivateComponent<Shift>);
            p4.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void VengefulElementalBelone(uint id, float delay)
        {
            // all other bloodrakes target all players
            // third bloodrake in addition 'targets' three of the four corner helpers - untethered one is safe during later mechanic
            var bloodrake3 = Cast(id, AID.Bloodrake, delay, 4, "Bloodrake 3");
            bloodrake3.Enter.Add(Module.ActivateComponent<ElementalBelone>);
            bloodrake3.Exit.Add(Module.DeactivateComponent<SettingTheScene>);
            bloodrake3.EndHint |= StateMachine.StateHint.Raidwide;

            var setting = Cast(id + 0x1000, AID.SettingTheScene, 7.2f, 4, "Scene");
            setting.Exit.Add(Module.ActivateComponent<SettingTheScene>);
            setting.Exit.Add(() => Module.FindComponent<ElementalBelone>()!.Visible = true);

            var vengeful = Cast(id + 0x2000, AID.VengefulBelone, 8.2f, 4, "Roles"); // acting X applied after cast end
            vengeful.Exit.Add(Module.ActivateComponent<VengefulBelone>);

            Cast(id + 0x3000, AID.ElementalBelone, 4.2f, 4); // 'elemental resistance down' applied after cast end

            var bloodrake4 = Cast(id + 0x4000, AID.Bloodrake, 4.2f, 4, "Bloodrake 4");
            bloodrake4.EndHint |= StateMachine.StateHint.Raidwide;

            var bursts = Cast(id + 0x5000, AID.BeloneBursts, 4.2f, 5, "Orbs"); // orbs appear at cast start, tether and start moving at cast end
            bursts.EndHint |= StateMachine.StateHint.PositioningStart;

            var periaktoi = Cast(id + 0x6000, AID.Periaktoi, 9.2f, 5, "Square explode");
            periaktoi.Exit.Add(Module.DeactivateComponent<SettingTheScene>);
            periaktoi.Exit.Add(Module.DeactivateComponent<ElementalBelone>);
            periaktoi.Exit.Add(Module.DeactivateComponent<VengefulBelone>); // TODO: reconsider deactivation time, debuffs fade ~12s later, but I think vengeful needs to be handled before explosion?
            periaktoi.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void BeloneCoils(uint id, float delay)
        {
            var bloodrake5 = Cast(id, AID.Bloodrake, delay, 4, "Bloodrake 5");
            bloodrake5.EndHint |= StateMachine.StateHint.Raidwide;

            var coils1 = Cast(id + 0x1000, AID.BeloneCoils, 3.2f, 4, "Coils 1");
            coils1.Exit.Add(Module.ActivateComponent<BeloneCoils>);
            coils1.Exit.Add(Module.ActivateComponent<InversiveChlamys>);
            coils1.EndHint |= StateMachine.StateHint.PositioningStart;

            var inv1 = InversiveChlamys(id + 0x2000, 3.2f);
            inv1.EndHint |= StateMachine.StateHint.PositioningEnd;

            Cast(id + 0x3000, AID.AethericChlamys, 2.4f, 4);

            var bloodrake6 = Cast(id + 0x4000, AID.Bloodrake, 4.2f, 4, "Bloodrake 6");
            bloodrake6.EndHint |= StateMachine.StateHint.Raidwide;

            var coils2 = Cast(id + 0x5000, AID.BeloneCoils, 4.2f, 4, "Coils 2");
            coils2.Exit.Add(Module.ActivateComponent<DirectorsBelone>);
            coils2.EndHint |= StateMachine.StateHint.PositioningStart;

            Cast(id + 0x6000, AID.DirectorsBelone, 9.2f, 5);

            var inv2 = InversiveChlamys(id + 0x7000, 9.2f);
            inv2.Exit.Add(Module.DeactivateComponent<BeloneCoils>);
            inv2.Exit.Add(Module.DeactivateComponent<InversiveChlamys>);
            inv2.Exit.Add(Module.DeactivateComponent<DirectorsBelone>);
            inv2.EndHint |= StateMachine.StateHint.PositioningEnd;
        }
    }
}
