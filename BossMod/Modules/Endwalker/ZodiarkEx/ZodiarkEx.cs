using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.ZodiarkEx
{
    // simple component tracking raidwide cast at the end of intermission
    public class Apomnemoneumata : CommonComponents.CastCounter
    {
        public Apomnemoneumata() : base(ActionID.MakeSpell(AID.ApomnemoneumataNormal)) { }
    }

    public class ZodiarkEx : BossModule
    {
        public ZodiarkEx(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            StateMachine.State? s;
            s = CommonStates.Cast(ref InitialState, this, AID.Kokytos, 6.1f, 4, "Kokytos");
            s = Paradeigma1(ref s.Next, 7.2f);
            s = Ania(ref s.Next, 2.8f);
            s = Exoterikos1(ref s.Next, 4.2f);
            s = Paradeigma2(ref s.Next, 9.4f);
            s = Phobos(ref s.Next, 7.4f);
            s = Paradeigma3(ref s.Next, 7.2f);
            s = Ania(ref s.Next, 2.5f);
            s = Paradeigma4(ref s.Next, 3.2f);
            s = Intermission(ref s.Next, 9.5f);
            s = AstralEclipse(ref s.Next, 6.2f, true);
            s = Paradeigma5(ref s.Next, 10.2f);
            s = Ania(ref s.Next, 8.5f);
            s = Exoterikos4(ref s.Next, 6.2f);
            s = Paradeigma6(ref s.Next, 10.2f);
            s = TrimorphosExoterikos(ref s.Next, 0.6f, true);
            s = AstralEclipse(ref s.Next, 8.5f, false);
            s = Ania(ref s.Next, 7.2f);
            s = Paradeigma7(ref s.Next, 6.2f);
            // below are from legacy module, timings need verification
            s = Exoterikos6(ref s.Next, 2.2f);
            s = Paradeigma8(ref s.Next, 8);
            s = Phobos(ref s.Next, 4.5f);
            s = TrimorphosExoterikos(ref s.Next, 10, false);
            s = Styx(ref s.Next, 3, 9);
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
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AlgedonTL] = new(null, () => { });
            dispatch[AID.AlgedonTR] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, this, dispatch, delay);
            start.Exit.Add(() => ActivateComponent(new Algedon()));

            var end = CommonStates.CastEnd(ref start.Next, this, 7);

            var resolve = CommonStates.ComponentCondition<Algedon>(ref end.Next, 1, this, comp => comp.Done, "Diagonal");
            resolve.Exit.Add(DeactivateComponent<Algedon>);
            return resolve;
        }

        private StateMachine.State Adikia(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, this, AID.Adikia, delay, 6);
            cast.Enter.Add(() => ActivateComponent(new Adikia()));

            var resolve = CommonStates.ComponentCondition<Adikia>(ref cast.Next, 1.7f, this, comp => comp.Done, "SideSmash");
            resolve.Exit.Add(DeactivateComponent<Adikia>);
            return resolve;
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

        // note that exoterikos component is optionally activated, but unconditionally deactivated
        private StateMachine.State TripleEsotericRay(ref StateMachine.State? link, float delay, bool startExo)
        {
            var cast = CommonStates.Cast(ref link, this, AID.TripleEsotericRay, delay, 7, "TripleRay");
            if (startExo)
                cast.Enter.Add(() => ActivateComponent(new Exoterikos(this)));
            cast.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.ComponentCondition<Exoterikos>(ref cast.Next, 3.1f, this, comp => comp.Done, "TripleRay resolve");
            resolve.Exit.Add(DeactivateComponent<Exoterikos>);
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

            var resolve = CommonStates.Condition(ref end.Next, 6.7f, () => WorldState.Party.WithoutSlot().All(a => (a.FindStatus(SID.TenebrousGrasp) == null)), "Rotate resolve", 5, 1);
            resolve.Exit.Add(DeactivateComponent<Paradeigma>);
            return resolve;
        }

        // this is used by various exoterikos states; the state activates component and is automatically grouped with next
        private StateMachine.State ExoterikosStart(ref StateMachine.State? link, float delay, string name)
        {
            var s = CommonStates.Cast(ref link, this, AID.ExoterikosGeneric, delay, 5, name);
            s.Enter.Add(() => ActivateComponent(new Exoterikos(this)));
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        private StateMachine.State Paradeigma1(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para1 (4 birds)");
            var styx = Styx(ref para.Next, 11.2f, 6);
            styx.Enter.Add(DeactivateComponent<Paradeigma>);
            return styx;
        }

        private StateMachine.State Exoterikos1(ref StateMachine.State? link, float delay)
        {
            var exo1 = ExoterikosStart(ref link, delay, "Exo1 (side tri)");
            var exo2 = CommonStates.Cast(ref exo1.Next, this, AID.ExoterikosFront, 2.2f, 7, "Exo2 (front)");
            exo2.Exit.Add(DeactivateComponent<Exoterikos>);
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
            var exo = ExoterikosStart(ref para.Next, 2.2f, "Exo3 (side)");
            var flow = AstralFlow(ref exo.Next, 2.2f);
            flow.Exit.Add(DeactivateComponent<Exoterikos>);
            return flow;
        }

        private StateMachine.State Paradeigma4(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para4 (snakes side)");
            var adikia = Adikia(ref para.Next, 4.2f);
            adikia.Exit.Add(DeactivateComponent<Paradeigma>);
            return adikia;
        }

        private StateMachine.State Paradeigma5(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para5 (birds/behemoths)");
            var flow = AstralFlow(ref para.Next, 5.2f);
            return flow;
        }

        private StateMachine.State Exoterikos4(ref StateMachine.State? link, float delay)
        {
            var exo = ExoterikosStart(ref link, delay, "Exo4 (side sq)");
            var diag = Algedon(ref exo.Next, 2.2f);
            diag.Exit.Add(DeactivateComponent<Exoterikos>);
            return diag;
        }

        private StateMachine.State Paradeigma6(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para6 (4 birds + snakes)");
            var flow = AstralFlow(ref para.Next, 5.2f);
            flow.EndHint |= StateMachine.StateHint.GroupWithNext;

            var styx = Styx(ref flow.Next, 0, 7); // note: cast starts slightly before flow resolve
            return styx;
        }

        private StateMachine.State TrimorphosExoterikos(ref StateMachine.State? link, float delay, bool first)
        {
            var exo = CommonStates.Cast(ref link, this, AID.TrimorphosExoterikos, delay, 13, "TriExo");
            exo.Enter.Add(() => ActivateComponent(new Exoterikos(this)));
            exo.EndHint |= StateMachine.StateHint.GroupWithNext;

            var followup = first ? Adikia(ref exo.Next, 6.2f) : Algedon(ref exo.Next, 5);
            followup.Enter.Add(DeactivateComponent<Exoterikos>);
            return followup;
        }

        private StateMachine.State Paradeigma7(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para7 (snakes)");
            var exo = ExoterikosStart(ref para.Next, 2.2f, "Exo5 (side)");
            var flow = AstralFlow(ref exo.Next, 2);
            flow.Exit.Add(DeactivateComponent<Exoterikos>);
            flow.EndHint |= StateMachine.StateHint.GroupWithNext;

            // TODO: component
            var puddles = CommonStates.Cast(ref flow.Next, this, AID.Phlegeton, 0, 3, "Puddles"); // TODO: cast starts slightly before flow resolve...
            puddles.EndHint |= StateMachine.StateHint.GroupWithNext;
            // TODO: resolve; it overlaps with styx cast start

            var styx = Styx(ref puddles.Next, 2.2f, 8);
            return styx;
        }

        private StateMachine.State Exoterikos6(ref StateMachine.State? link, float delay)
        {
            var exo = ExoterikosStart(ref link, delay, "Exo6 (side)");
            var tri = TripleEsotericRay(ref exo.Next, 2, false);
            return tri;
        }

        private StateMachine.State Paradeigma8(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para8 (birds/behemoths)");
            var exo = ExoterikosStart(ref para.Next, 2, "Exo7 (back sq)");
            var flow = AstralFlow(ref exo.Next, 2);
            flow.Exit.Add(DeactivateComponent<Exoterikos>);
            return flow;
        }

        private StateMachine.State Paradeigma9(ref StateMachine.State? link, float delay)
        {
            var para = ParadeigmaStart(ref link, delay, "Para9 (4 birds + snakes)");
            var exo = ExoterikosStart(ref para.Next, 2, "Exo8 (side/back?)");
            var flow = AstralFlow(ref exo.Next, 2);
            flow.Exit.Add(DeactivateComponent<Exoterikos>);
            flow.EndHint |= StateMachine.StateHint.GroupWithNext;

            var styx = Styx(ref flow.Next, 0, 9); // TODO: how many hits?..; cast starts slightly before flow resolve...
            return styx;
        }

        private StateMachine.State Intermission(ref StateMachine.State? link, float delay)
        {
            var disappear = CommonStates.Targetable(ref link, this, false, delay, "Intermission start");
            disappear.Exit.Add(() => ActivateComponent(new Exoterikos(this)));
            disappear.EndHint |= StateMachine.StateHint.GroupWithNext;
            disappear.EndHint &= ~StateMachine.StateHint.DowntimeStart; // adds appear almost immediately, so there is no downtime

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AddsEndFail] = new(null, () => { });
            dispatch[AID.AddsEndSuccess] = new(null, () => { });
            var addsEndStart = CommonStates.CastStart(ref disappear.Next, this, dispatch, 40, "Add enrage");
            addsEndStart.Exit.Add(DeactivateComponent<Exoterikos>);
            addsEndStart.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeStart;

            var addsEndEnd = CommonStates.CastEnd(ref addsEndStart.Next, this, 1.1f);
            addsEndEnd.Exit.Add(() => ActivateComponent(new Apomnemoneumata()));

            var raidwide = CommonStates.ComponentCondition<Apomnemoneumata>(ref addsEndEnd.Next, 11.5f, this, comp => comp.NumCasts > 0, "Raidwide");
            raidwide.Exit.Add(DeactivateComponent<Apomnemoneumata>);
            raidwide.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var reappear = CommonStates.Targetable(ref raidwide.Next, this, true, 10.5f, "Intermission end");
            return reappear;
        }

        private StateMachine.State AstralEclipse(ref StateMachine.State? link, float delay, bool first)
        {
            var eclipse = CommonStates.Cast(ref link, this, AID.AstralEclipse, delay, 5, "Eclipse");
            eclipse.Exit.Add(() => ActivateComponent(new AstralEclipse(this)));
            eclipse.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeStart;

            var reappear = CommonStates.Targetable(ref eclipse.Next, this, true, 12, "Boss reappear", 1);
            reappear.EndHint |= StateMachine.StateHint.GroupWithNext;

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
            var followup = first ? TripleEsotericRay(ref reappear.Next, 8.2f, true) : Algedon(ref reappear.Next, 10.5f);
            followup.Exit.Add(DeactivateComponent<AstralEclipse>);
            return followup;
        }
    }
}
