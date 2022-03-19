using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.ZodiarkEx
{
    public class ZodiarkEx : BossModule
    {
        public ZodiarkEx(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            StateMachine.State? s;
            s = CommonStates.Cast(ref InitialState, this, AID.Kokytos, 6.1f, 4, "Kokytos");
            s = Paradeigma1(ref s.Next, 7.2f);
            s = Ania(ref s.Next, 2.8f);
            s = Exoterikos1(ref s.Next, 4);
            s = Paradeigma2(ref s.Next, 9.4f);
            s = Phobos(ref s.Next, 8.2f);
            s = Paradeigma3(ref s.Next, 7.2f);
            s = Ania(ref s.Next, 2.5f);
            s = Paradeigma4(ref s.Next, 3.2f);
            s = Intermission(ref s.Next, 11.2f);
            s = AstralEclipse(ref s.Next, 6, true);
            s = Paradeigma5(ref s.Next, 13.1f);
            s = Ania(ref s.Next, 8.5f);
            s = Exoterikos4(ref s.Next, 6.2f);
            s = Paradeigma6(ref s.Next, 11.3f);
            s = TrimorphosExoterikos(ref s.Next, 1.5f);
            s = AstralEclipse(ref s.Next, 10.2f, false);
            s = Ania(ref s.Next, 8.2f);
            s = Paradeigma7(ref s.Next, 6.3f);
            // below are from legacy module, timings need verification
            s = Exoterikos6(ref s.Next, 2.2f);
            s = Paradeigma8(ref s.Next, 8);
            s = Phobos(ref s.Next, 4.5f);
            s = CommonStates.Cast(ref s.Next, this, AID.TrimorphosExoterikos, 10, 13, "TriExo"); // TODO: always back + side + side?
            s = Algedon(ref s.Next, 5);
            s = Styx(ref s.Next, 4, 9);
            s = Paradeigma9(ref s.Next, 0.1f);
            s = CommonStates.Cast(ref s.Next, this, AID.Enrage, 1, 1, "Enrage"); // TODO: timings..

            InitialState!.Enter.Add(() => ActivateComponent(new StyxTargetTracker(this)));
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }

        private StateMachine.State Ania(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, this, AID.Ania, delay, 4);
            cast.Enter.Add(() => ActivateComponent(new Ania(this)));

            var resolve = CommonStates.ComponentCondition<Ania>(ref cast.Next, 1, this, comp => comp.Done, "Tankbuster");
            resolve.Exit.Add(DeactivateComponent<Ania>);
            resolve.EndHint |= StateMachine.StateHint.Tankbuster;
            return resolve;
        }

        private StateMachine.State Phobos(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, this, AID.Phobos, delay, 4, "Raidwide");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State Algedon(ref StateMachine.State? link, float delay)
        {
            // TODO: component
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AlgedonTL] = new(null, () => { });
            dispatch[AID.AlgedonTR] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, this, dispatch, delay);

            var end = CommonStates.CastEnd(ref start.Next, this, 7, "Diagonal");
            // TODO: resolve +1s
            return end;
        }

        private StateMachine.State Styx(ref StateMachine.State? link, float delay, int numHits)
        {
            var cast = CommonStates.Cast(ref link, this, AID.Styx, delay, 5, "Stack");
            cast.Enter.Add(() => ActivateComponent(new Styx(this)));
            cast.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.ComponentCondition<Styx>(ref cast.Next, 1.1f * numHits, this, comp => comp.NumCasts >= numHits, "Stack resolve", 2);
            resolve.Exit.Add(DeactivateComponent<Styx>);
            return resolve;
        }

        // this is used by various paradeigma states; the state activates component and is automatically grouped with next
        private StateMachine.State ParadeigmaStart(ref StateMachine.State? link, float delay, string name)
        {
            var s = CommonStates.Cast(ref link, this, AID.Paradeigma, delay, 3, name);
            s.Exit.Add(() => ActivateComponent(new Paradeigma(this)));
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        // this is used by various paradeigma states; automatically deactivates paradeigma component
        private StateMachine.State AstralFlow(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AstralFlowCW] = new(null, () => { });
            dispatch[AID.AstralFlowCCW] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, this, dispatch, delay);

            var end = CommonStates.CastEnd(ref start.Next, this, 10, "Rotate");
            end.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.Condition(ref end.Next, 6.7f, () => WorldState.Party.WithoutSlot().All(a => a.FindStatus(SID.TenebrousGrasp) == null), "Rotate resolve", 1, 1);
            resolve.Exit.Add(DeactivateComponent<Paradeigma>);
            return resolve;
        }

        private StateMachine.State Paradeigma1(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para1 (4 birds)");

            var styx = Styx(ref para.Next, 11, 6);
            styx.Enter.Add(DeactivateComponent<Paradeigma>);
            return styx;
        }

        private StateMachine.State Exoterikos1(ref StateMachine.State? link, float delay)
        {
            // TODO: exo component
            var exo1 = CommonStates.Cast(ref link, this, AID.ExoterikosGeneric, delay, 5, "Exo1 (side tri)");
            exo1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var exo2 = CommonStates.Cast(ref exo1.Next, this, AID.ExoterikosFront, 2, 7, "Exo2 (front)");
            return exo2;
        }

        private StateMachine.State Paradeigma2(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para2 (birds/behemoths)");

            var diag = Algedon(ref para.Next, 5.2f);
            diag.Exit.Add(DeactivateComponent<Paradeigma>);
            return diag;
        }

        private StateMachine.State Paradeigma3(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para3 (snakes)");

            // TODO: exo component
            var exo = CommonStates.Cast(ref para.Next, this, AID.ExoterikosGeneric, 2.2f, 5, "Exo3 (side)");
            exo.EndHint |= StateMachine.StateHint.GroupWithNext;

            var flow = AstralFlow(ref exo.Next, 2.2f);
            return flow;
        }

        private StateMachine.State Paradeigma4(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para4 (snakes side)");

            // TODO: component
            var adikia = CommonStates.Cast(ref para.Next, this, AID.Adikia, 4.2f, 6, "SideSmash");
            // TODO: +0.9 AdikiaL, +1.4 snakes, +1.7 AdikiaR resolve...
            adikia.Exit.Add(DeactivateComponent<Paradeigma>);
            return adikia;
        }

        private StateMachine.State Paradeigma5(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para5 (birds/behemoths)");
            para.EndHint |= StateMachine.StateHint.GroupWithNext;

            var flow = AstralFlow(ref para.Next, 5.2f);
            return flow;
        }

        private StateMachine.State Exoterikos4(ref StateMachine.State? link, float delay)
        {
            // TODO: exo component
            var exo = CommonStates.Cast(ref link, this, AID.ExoterikosGeneric, delay, 5, "Exo4 (side sq)");
            exo.EndHint |= StateMachine.StateHint.GroupWithNext;

            var diag = Algedon(ref exo.Next, 2.2f);
            return diag;
        }

        private StateMachine.State Paradeigma6(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para6 (4 birds + snakes)");
            para.EndHint |= StateMachine.StateHint.GroupWithNext;

            var flow = AstralFlow(ref para.Next, 5.2f);
            flow.EndHint |= StateMachine.StateHint.GroupWithNext;

            var styx = Styx(ref flow.Next, 0, 7); // note: cast starts slightly before flow resolve
            return styx;
        }

        private StateMachine.State TrimorphosExoterikos(ref StateMachine.State? link, float delay)
        {
            // TODO: component
            var exo = CommonStates.Cast(ref link, this, AID.TrimorphosExoterikos, delay, 13, "TriExo"); // TODO: always side + side + back?
            exo.EndHint |= StateMachine.StateHint.GroupWithNext;

            // TODO: component
            var adikia = CommonStates.Cast(ref exo.Next, this, AID.Adikia, 6.1f, 6, "SideSmash");
            // TODO: +0.9 AdikiaL, +1.7 AdikiaR resolve...
            return adikia;
        }

        private StateMachine.State Paradeigma7(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para7 (snakes)");
            para.EndHint |= StateMachine.StateHint.GroupWithNext;

            // TODO: exo component
            var exo = CommonStates.Cast(ref para.Next, this, AID.ExoterikosGeneric, 2.2f, 5, "Exo5 (side)");
            exo.EndHint |= StateMachine.StateHint.GroupWithNext;

            var flow = AstralFlow(ref exo.Next, 2);
            flow.EndHint |= StateMachine.StateHint.GroupWithNext;

            // TODO: component
            var puddles = CommonStates.Cast(ref flow.Next, this, AID.Phlegeton, 0, 3, "Puddles"); // TODO: cast starts slightly before flow resolve...
            puddles.EndHint |= StateMachine.StateHint.GroupWithNext;
            // TODO: resolve; it overlaps with styx cast start

            var styx = Styx(ref puddles.Next, 2, 8);
            return styx;
        }

        private StateMachine.State Exoterikos6(ref StateMachine.State? link, float delay)
        {
            // TODO: exo component
            var exo = CommonStates.Cast(ref link, this, AID.ExoterikosGeneric, delay, 5, "Exo6 (side)");
            exo.EndHint |= StateMachine.StateHint.GroupWithNext;

            // TODO: component
            var tri = CommonStates.Cast(ref exo.Next, this, AID.TripleEsotericRay, 2, 7, "TriExo");
            return tri;
        }

        private StateMachine.State Paradeigma8(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para8 (birds/behemoths)");
            para.EndHint |= StateMachine.StateHint.GroupWithNext;

            // TODO: exo component
            var exo = CommonStates.Cast(ref para.Next, this, AID.ExoterikosGeneric, 2, 5, "Exo7 (back sq)");
            exo.EndHint |= StateMachine.StateHint.GroupWithNext;

            var flow = AstralFlow(ref exo.Next, 2);
            return flow;
        }

        private StateMachine.State Paradeigma9(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para9 (4 birds + snakes)");
            para.EndHint |= StateMachine.StateHint.GroupWithNext;

            // TODO: exo component
            var exo = CommonStates.Cast(ref para.Next, this, AID.ExoterikosGeneric, 2, 5, "Exo8 (side/back?)");
            exo.EndHint |= StateMachine.StateHint.GroupWithNext;

            var flow = AstralFlow(ref exo.Next, 2);
            flow.EndHint |= StateMachine.StateHint.GroupWithNext;

            var styx = Styx(ref flow.Next, 0, 9); // TODO: how many hits?..; cast starts slightly before flow resolve...
            return styx;
        }

        private StateMachine.State Intermission(ref StateMachine.State? link, float delay)
        {
            // TODO: component
            var disappear = CommonStates.Targetable(ref link, this, false, delay, "Intermission start");
            disappear.EndHint |= StateMachine.StateHint.GroupWithNext;
            disappear.EndHint &= ~StateMachine.StateHint.DowntimeStart; // adds appear almost immediately, so there is no downtime

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AddsEndFail] = new(null, () => { });
            dispatch[AID.AddsEndSuccess] = new(null, () => { });
            var addsEndStart = CommonStates.CastStart(ref disappear.Next, this, dispatch, 40, "Add enrage");
            addsEndStart.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeStart;

            var addsEndEnd = CommonStates.CastEnd(ref addsEndStart.Next, this, 1.1f);

            var reappear = CommonStates.Targetable(ref addsEndEnd.Next, this, true, 22.1f, "Intermission end");
            reappear.EndHint |= StateMachine.StateHint.Raidwide;
            return reappear;
        }

        private StateMachine.State AstralEclipse(ref StateMachine.State? link, float delay, bool first)
        {
            // TODO: component
            var eclipse = CommonStates.Cast(ref link, this, AID.AstralEclipse, delay, 5, "Eclipse");
            eclipse.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeStart;

            var reappear = CommonStates.Targetable(ref eclipse.Next, this, true, 12, "Boss reappear", 1);
            eclipse.EndHint |= StateMachine.StateHint.GroupWithNext;

            if (first)
            {
                //  5.1s first explosion
                //  8.2s triple ray cast start
                //  9.2s second explosion
                // 13.2s third explosion
                // 15.2s triple ray cast end
                // 15.3s ray 1
                // 18.3s ray 2

                // TODO: component
                var rays = CommonStates.Cast(ref reappear.Next, this, AID.TripleEsotericRay, 8.2f, 7, "TripleRay");
                // TODO: +3.1s resolve second hit
                return rays;
            }
            else
            {
                // similar timeline...
                var diag = Algedon(ref reappear.Next, 10.7f);
                return diag;
            }
        }
    }
}
