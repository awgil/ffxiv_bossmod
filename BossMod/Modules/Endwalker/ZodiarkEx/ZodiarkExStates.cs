using System.Linq;

namespace BossMod.Endwalker.ZodiarkEx
{
    class ZodiarkExStates : StateMachineBuilder
    {
        public ZodiarkExStates(BossModule module) : base(module)
        {
            Cast(0x00000000, AID.Kokytos, 6.1f, 4, "Kokytos");
            Paradeigma1(0x00010000, 7.2f);
            Ania(0x00020000, 2.7f);
            Exoterikos1(0x00030000, 4.2f);
            Paradeigma2(0x00040000, 9.2f);
            Phobos(0x00050000, 7.2f);
            Paradeigma3(0x00060000, 7.2f);
            Ania(0x00070000, 3);
            Paradeigma4(0x00080000, 3.2f);

            Intermission(0x00100000, 9.5f);
            AstralEclipse(0x00110000, 6.1f, true);

            Paradeigma5(0x00200000, 10.1f);
            Ania(0x00210000, 9);
            Exoterikos4(0x00220000, 6.2f);
            Paradeigma6(0x00230000, 10.2f);
            TrimorphosExoterikos(0x00240000, 0.6f, true);
            AstralEclipse(0x00250000, 8.5f, false);

            Ania(0x00300000, 7.2f);
            Paradeigma7(0x00310000, 6.2f);
            // below are from legacy module, timings need verification
            Exoterikos6(0x00320000, 2.2f);
            Paradeigma8(0x00330000, 8);
            Phobos(0x00340000, 4.5f);
            TrimorphosExoterikos(0x00350000, 10, false);
            Styx(0x00360000, 3, 9);
            Paradeigma9(0x00370000, 0.1f);
            Cast(0x00380000, AID.Enrage, 1, 1, "Enrage"); // TODO: timings..
        }

        private void Ania(uint id, float delay)
        {
            var cast = Cast(id, AID.Ania, delay, 4);
            cast.Enter.Add(Module.ActivateComponent<Ania>);

            var resolve = ComponentCondition<Ania>(id + 2, 1, comp => comp.Done, "Tankbuster");
            resolve.Exit.Add(Module.DeactivateComponent<Ania>);
            resolve.EndHint |= StateMachine.StateHint.Tankbuster;
        }

        private void Phobos(uint id, float delay)
        {
            var s = Cast(id, AID.Phobos, delay, 4, "Raidwide");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private StateMachine.State Algedon(uint id, float delay)
        {
            var start = CastStartMulti(id, new AID[] { AID.AlgedonTL, AID.AlgedonTR }, delay);
            start.Exit.Add(Module.ActivateComponent<Algedon>);

            CastEnd(id + 1, 7);

            var resolve = ComponentCondition<Algedon>(id + 2, 1, comp => comp.Done, "Diagonal");
            resolve.Exit.Add(Module.DeactivateComponent<Algedon>);
            return resolve;
        }

        private StateMachine.State Adikia(uint id, float delay)
        {
            var cast = Cast(id, AID.Adikia, delay, 6);
            cast.Enter.Add(Module.ActivateComponent<Adikia>);

            var resolve = ComponentCondition<Adikia>(id + 0x10, 1.7f, comp => comp.Done, "SideSmash");
            resolve.Exit.Add(Module.DeactivateComponent<Adikia>);
            return resolve;
        }

        private StateMachine.State Styx(uint id, float delay, int numHits)
        {
            var cast = Cast(id, AID.Styx, delay, 5, "Stack");
            cast.Enter.Add(Module.ActivateComponent<Styx>);

            var resolve = ComponentCondition<Styx>(id + 0x10, 1.1f * numHits - 0.1f, comp => comp.NumCasts >= numHits, "Stack resolve", 2);
            resolve.Exit.Add(Module.DeactivateComponent<Styx>);
            return resolve;
        }

        // note that exoterikos component is optionally activated, but unconditionally deactivated
        private StateMachine.State TripleEsotericRay(uint id, float delay, bool startExo)
        {
            var cast = Cast(id, AID.TripleEsotericRay, delay, 7, "TripleRay");
            if (startExo)
                cast.Enter.Add(Module.ActivateComponent<Exoterikos>);

            var resolve = ComponentCondition<Exoterikos>(id + 0x10, 3.1f, comp => comp.Done, "TripleRay resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Exoterikos>);
            return resolve;
        }

        // this is used by various paradeigma states; the state activates component
        private void ParadeigmaStart(uint id, float delay, string name)
        {
            var s = Cast(id, AID.Paradeigma, delay, 3, name);
            s.Exit.Add(Module.ActivateComponent<Paradeigma>);
        }

        // this is used by various paradeigma states; automatically deactivates paradeigma component
        private StateMachine.State AstralFlow(uint id, float delay)
        {
            CastStartMulti(id, new AID[] { AID.AstralFlowCW, AID.AstralFlowCCW }, delay);
            CastEnd(id + 1, 10, "Rotate");

            var resolve = Condition(id + 0x10, 6.2f, () => Module.WorldState.Party.WithoutSlot().All(a => (a.FindStatus(SID.TenebrousGrasp) == null)), "Rotate resolve", 5, 1);
            resolve.Exit.Add(Module.DeactivateComponent<Paradeigma>);
            return resolve;
        }

        // this is used by various exoterikos states; the state activates component
        private void ExoterikosStart(uint id, float delay, string name)
        {
            var s = Cast(id, AID.ExoterikosGeneric, delay, 5, name);
            s.Enter.Add(Module.ActivateComponent<Exoterikos>);
        }

        private void Paradeigma1(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para1 (4 birds)");
            var styx = Styx(id + 0x1000, 11.2f, 6);
            styx.Enter.Add(Module.DeactivateComponent<Paradeigma>);
        }

        private void Exoterikos1(uint id, float delay)
        {
            ExoterikosStart(id, delay, "Exo1 (side tri)");
            var exo2 = Cast(id + 0x1000, AID.ExoterikosFront, 2.2f, 7, "Exo2 (front)");
            exo2.Exit.Add(Module.DeactivateComponent<Exoterikos>);
        }

        private void Paradeigma2(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para2 (birds/behemoths)");
            var diag = Algedon(id + 0x1000, 5.2f);
            diag.Exit.Add(Module.DeactivateComponent<Paradeigma>);
        }

        private void Paradeigma3(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para3 (snakes)");
            ExoterikosStart(id + 0x1000, 2.2f, "Exo3 (side)");
            var flow = AstralFlow(id + 0x2000, 2.2f);
            flow.Exit.Add(Module.DeactivateComponent<Exoterikos>);
        }

        private void Paradeigma4(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para4 (snakes side)");
            var adikia = Adikia(id + 0x1000, 4.2f);
            adikia.Exit.Add(Module.DeactivateComponent<Paradeigma>);
        }

        private void Paradeigma5(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para5 (birds/behemoths)");
            AstralFlow(id + 0x1000, 5.2f);
        }

        private void Exoterikos4(uint id, float delay)
        {
            ExoterikosStart(id, delay, "Exo4 (side sq)");
            var diag = Algedon(id + 0x1000, 2.2f);
            diag.Exit.Add(Module.DeactivateComponent<Exoterikos>);
        }

        private void Paradeigma6(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para6 (4 birds + snakes)");
            AstralFlow(id + 0x1000, 5.2f);
            Styx(id + 0x2000, 0, 7); // note: cast starts slightly before flow resolve
        }

        private void TrimorphosExoterikos(uint id, float delay, bool first)
        {
            var exo = Cast(id, AID.TrimorphosExoterikos, delay, 13, "TriExo");
            exo.Enter.Add(Module.ActivateComponent<Exoterikos>);

            var followup = first ? Adikia(id + 0x1000, 6.2f) : Algedon(id + 0x1000, 5);
            followup.Enter.Add(Module.DeactivateComponent<Exoterikos>);
        }

        private void Paradeigma7(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para7 (snakes)");
            ExoterikosStart(id + 0x1000, 2.1f, "Exo5 (side)");
            var flow = AstralFlow(id + 0x2000, 2.2f);
            flow.Exit.Add(Module.DeactivateComponent<Exoterikos>);

            // TODO: component
            Cast(id + 0x3000, AID.Phlegeton, 0, 3, "Puddles"); // TODO: cast starts slightly before flow resolve...
            // TODO: resolve; it overlaps with styx cast start

            Styx(id + 0x4000, 2.2f, 8);
        }

        private void Exoterikos6(uint id, float delay)
        {
            ExoterikosStart(id, delay, "Exo6 (side)");
            TripleEsotericRay(id + 0x1000, 2, false);
        }

        private void Paradeigma8(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para8 (birds/behemoths)");
            ExoterikosStart(id + 0x1000, 2, "Exo7 (back sq)");
            var flow = AstralFlow(id + 0x2000, 2);
            flow.Exit.Add(Module.DeactivateComponent<Exoterikos>);
        }

        private void Paradeigma9(uint id, float delay)
        {
            ParadeigmaStart(id, delay, "Para9 (4 birds + snakes)");
            ExoterikosStart(id + 0x1000, 2, "Exo8 (side/back?)");
            var flow = AstralFlow(id + 0x2000, 2);
            flow.Exit.Add(Module.DeactivateComponent<Exoterikos>);

            Styx(id + 0x3000, 0, 9); // TODO: how many hits?..; cast starts slightly before flow resolve...
        }

        private void Intermission(uint id, float delay)
        {
            var disappear = Targetable(id, false, delay, "Intermission start");
            disappear.Exit.Add(Module.ActivateComponent<Exoterikos>);
            disappear.EndHint &= ~StateMachine.StateHint.DowntimeStart; // adds appear almost immediately, so there is no downtime

            var addsEndStart = CastStartMulti(id + 0x1000, new AID[] { AID.AddsEndFail, AID.AddsEndSuccess }, 40, "Add enrage");
            addsEndStart.Exit.Add(Module.DeactivateComponent<Exoterikos>);
            addsEndStart.EndHint |= StateMachine.StateHint.DowntimeStart;

            var addsEndEnd = CastEnd(id + 0x2000, 1.1f);
            addsEndEnd.Exit.Add(Module.ActivateComponent<Apomnemoneumata>);

            var raidwide = ComponentCondition<Apomnemoneumata>(id + 0x3000, 11.5f, comp => comp.NumCasts > 0, "Raidwide");
            raidwide.Exit.Add(Module.DeactivateComponent<Apomnemoneumata>);
            raidwide.EndHint |= StateMachine.StateHint.Raidwide;

            Targetable(id + 0x4000, true, 10.6f, "Intermission end");
        }

        private void AstralEclipse(uint id, float delay, bool first)
        {
            var eclipse = Cast(id, AID.AstralEclipse, delay, 5, "Eclipse");
            eclipse.Exit.Add(Module.ActivateComponent<AstralEclipse>);
            eclipse.EndHint |= StateMachine.StateHint.DowntimeStart;

            var reappear = Targetable(id + 0x1000, true, 12, "Boss reappear", 1);
            reappear.EndHint |= StateMachine.StateHint.PositioningStart;

            //  5.1s first explosion
            //  8.2s triple ray cast start
            //  9.2s second explosion
            // 13.2s third explosion
            // 15.2s triple ray cast end
            // 15.3s ray 1
            // 18.3s ray 2
            // -or-
            //  5.1s first explosion
            //  9.2s second explosion
            // 10.7s algedon cast start
            // 13.2s third explosion
            // 17.7s algedon cast end
            // 18.7s algedon aoe
            var followup = first ? TripleEsotericRay(id + 0x2000, 8.2f, true) : Algedon(id + 0x2000, 10.6f);
            followup.Exit.Add(Module.DeactivateComponent<AstralEclipse>);
            followup.EndHint |= StateMachine.StateHint.PositioningEnd;
        }
    }
}
