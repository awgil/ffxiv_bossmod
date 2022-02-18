using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.P2S
{
    // TODO: improve this somehow...
    class OminousBubbling : CommonComponents.CastCounter
    {
        public OminousBubbling() : base(ActionID.MakeSpell(AID.OminousBubblingAOE)) { }
    }

    public class P2S : BossModule
    {
        private List<WorldState.Actor> _boss;
        private List<WorldState.Actor> _cataractHead;
        private List<WorldState.Actor> _dissociatedHead;
        public WorldState.Actor? Boss() => _boss.FirstOrDefault();
        public WorldState.Actor? CataractHead() => _cataractHead.FirstOrDefault();
        public WorldState.Actor? DissociatedHead() => _dissociatedHead.FirstOrDefault();

        public P2S(WorldState ws)
            : base(ws, 8)
        {
            _boss = Enemies(OID.Boss);
            _cataractHead = Enemies(OID.CataractHead);
            _dissociatedHead = Enemies(OID.DissociatedHead);

            StateMachine.State? s;
            s = MurkyDepths(ref InitialState, 10);
            s = DoubledImpact(ref s.Next, 5.2f);

            // deluge 1
            s = SewageDeluge(ref s.Next, 7.8f);
            s = Cataract(ref s.Next, 14.9f);
            s = Coherence(ref s.Next, 8.1f);
            s = MurkyDepths(ref s.Next, 8.2f);
            s = OminousBubblingShockwave(ref s.Next, 3.7f);
            s = AvariceCataract(ref s.Next, 10.9f);
            // note: deluge 1 ends here...

            s = Flow1(ref s.Next, 8.6f);
            s = DoubledImpact(ref s.Next, 8.2f);
            s = MurkyDepths(ref s.Next, 5.2f);

            // deluge 2
            s = SewageDeluge(ref s.Next, 11.7f);
            s = Shockwave(ref s.Next, 9.6f);
            s = KampeosHarma(ref s.Next, 4.4f);
            s = DoubledImpact(ref s.Next, 9.9f);
            s = MurkyDepths(ref s.Next, 4.2f);
            s = Flow2(ref s.Next, 8.6f);
            s = Cataract(ref s.Next, 1.2f);
            // note: deluge 2 ends here...

            s = AvariceDissociationCataract(ref s.Next, 15.2f);
            s = DissociationEruptionFloodCoherence(ref s.Next, 9.8f);
            s = DoubledImpact(ref s.Next, 7.2f);
            s = MurkyDepths(ref s.Next, 3.2f);

            // deluge 3
            s = SewageDeluge(ref s.Next, 12.8f);
            s = Flow3(ref s.Next, 11.7f);
            s = DissociationEruption(ref s.Next, 8.3f);
            s = OminousBubblingShockwave(ref s.Next, 3.4f);
            s = DoubledImpact(ref s.Next, 5.4f);
            s = MurkyDepths(ref s.Next, 7.2f);
            s = MurkyDepths(ref s.Next, 6.2f);

            s = CommonStates.Cast(ref s.Next, Boss, AID.Enrage, 5.3f, 10, "Enrage");

            // TODO: reconsider...
            InitialState!.Enter.Add(() => ActivateComponent(new SewageDeluge(this)));
        }

        protected override void DrawArenaForegroundPost()
        {
            Arena.Actor(Boss(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
        }

        private StateMachine.State MurkyDepths(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.MurkyDepths, delay, 5, "MurkyDepths");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State DoubledImpact(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.DoubledImpact, delay, 5, "DoubledImpact");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State SewageDeluge(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.SewageDeluge, delay, 5, "Deluge");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State Cataract(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.SpokenCataract] = new(null, () => ActivateComponent(new Cataract(this, false)));
            dispatch[AID.WingedCataract] = new(null, () => ActivateComponent(new Cataract(this, true)));
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 8, "Cataract");
            end.Exit.Add(DeactivateComponent<Cataract>);
            end.EndHint |= StateMachine.StateHint.PositioningEnd;
            return end;
        }

        private StateMachine.State Coherence(ref StateMachine.State? link, float delay)
        {
            var start = CommonStates.CastStart(ref link, Boss, AID.Coherence, delay);
            start.Exit.Add(() => ActivateComponent(new Coherence(this)));

            var end = CommonStates.CastEnd(ref start.Next, Boss, 12);

            var resolve = CommonStates.ComponentCondition<Coherence>(ref end.Next, 3.1f, this, comp => comp.NumCasts > 0, "Coherence");
            resolve.Exit.Add(DeactivateComponent<Coherence>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
            return resolve;
        }

        // note: this activates component, which has to be deactivated later manually - and this is automatically grouped with next cast
        private StateMachine.State PredatoryAvarice(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.PredatoryAvarice, delay, 4, "Avarice");
            s.Exit.Add(() => ActivateComponent(new PredatoryAvarice(this)));
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        // note: this activates component, which has to be deactivated later manually - and this is automatically grouped with next cast
        private StateMachine.State Dissociation(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.Dissociation, delay, 4, "Dissociation");
            s.Exit.Add(() => ActivateComponent(new Dissociation(this)));
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        private StateMachine.State AvariceCataract(ref StateMachine.State? link, float delay)
        {
            var avarice = PredatoryAvarice(ref link, delay);

            var cataract = Cataract(ref avarice.Next, 9.8f);
            cataract.EndHint &= ~StateMachine.StateHint.PositioningEnd;
            cataract.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.ComponentCondition<PredatoryAvarice>(ref cataract.Next, 6.2f, this, comp => !comp.Active, "Avarice resolve");
            resolve.Exit.Add(DeactivateComponent<PredatoryAvarice>);
            resolve.EndHint = StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.Raidwide;
            return resolve;
        }

        private StateMachine.State AvariceDissociationCataract(ref StateMachine.State? link, float delay)
        {
            var avarice = PredatoryAvarice(ref link, delay);
            var dissociation = Dissociation(ref avarice.Next, 2.4f);

            // note: we don't create separate avarice/dissociation resolve states here, since they all resolve at almost the same time
            var cataract = Cataract(ref dissociation.Next, 10.1f);
            cataract.Exit.Add(DeactivateComponent<PredatoryAvarice>);
            cataract.Exit.Add(DeactivateComponent<Dissociation>);
            cataract.EndHint |= StateMachine.StateHint.Raidwide; // avarice resolve
            return cataract;
        }

        private StateMachine.State DissociationEruptionFloodCoherence(ref StateMachine.State? link, float delay)
        {
            var dissociation = Dissociation(ref link, delay);

            var eruption = CommonStates.Cast(ref dissociation.Next, Boss, AID.SewageEruption, 8.2f, 5, "Eruption");
            eruption.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var flood = CommonStates.Cast(ref eruption.Next, Boss, AID.TaintedFlood, 2.3f, 3, "Flood");
            flood.Exit.Add(DeactivateComponent<Dissociation>);
            flood.EndHint |= StateMachine.StateHint.GroupWithNext;

            var coherence = Coherence(ref flood.Next, 4.7f);
            coherence.EndHint |= StateMachine.StateHint.PositioningEnd;
            return coherence;
        }

        private StateMachine.State DissociationEruption(ref StateMachine.State? link, float delay)
        {
            // TODO: get rid of timeout!
            var dissociation = Dissociation(ref link, delay);

            var eruption = CommonStates.Cast(ref dissociation.Next, Boss, AID.SewageEruption, 8.3f, 5, "Eruption");
            eruption.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var resolve = CommonStates.Timeout(ref eruption.Next, 5, "Resolve"); // should be last eruption...
            resolve.Exit.Add(DeactivateComponent<Dissociation>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State Flow1(ref StateMachine.State? link, float delay)
        {
            // TODO: get rid of timeouts!
            var cast = CommonStates.Cast(ref link, Boss, AID.ChannelingFlow, delay, 5, "Flow 1");
            cast.Exit.Add(() => ActivateComponent(new ChannelingFlow(this)));
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var stuns = CommonStates.Timeout(ref cast.Next, 14);
            stuns.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.DowntimeStart;

            var resolve = CommonStates.Timeout(ref stuns.Next, 3, "Flow resolve");
            resolve.Exit.Add(DeactivateComponent<ChannelingFlow>);
            resolve.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide;
            return resolve;
        }

        private StateMachine.State Flow2(ref StateMachine.State? link, float delay)
        {
            // TODO: get rid of timeouts!
            // flow 2: same statuses as first flow, different durations
            var cast = CommonStates.Cast(ref link, Boss, AID.ChannelingOverflow, delay, 5, "Flow 2");
            cast.Exit.Add(() => ActivateComponent(new ChannelingFlow(this)));
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var flood1 = CommonStates.Cast(ref cast.Next, Boss, AID.TaintedFlood, 4.2f, 3);

            var hit1 = CommonStates.Timeout(ref flood1.Next, 9, "Hit 1");
            hit1.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var flood2 = CommonStates.Cast(ref hit1.Next, Boss, AID.TaintedFlood, 3.4f, 3);

            var hit2 = CommonStates.Timeout(ref flood2.Next, 9, "Hit 2");
            hit2.Exit.Add(DeactivateComponent<ChannelingFlow>);
            hit2.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return hit2;
        }

        private StateMachine.State Flow3(ref StateMachine.State? link, float delay)
        {
            // TODO: get rid of timeouts!
            // flow 3: same as flow 2, but with coherence instead of floods
            var cast = CommonStates.Cast(ref link, Boss, AID.ChannelingOverflow, delay, 5, "Flow 3");
            cast.Exit.Add(() => ActivateComponent(new ChannelingFlow(this)));
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var coherence = Coherence(ref cast.Next, 5.5f); // first hit is around coherence cast end
            coherence.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.Timeout(ref coherence.Next, 10, "Flow resolve"); // second hit
            resolve.Exit.Add(DeactivateComponent<ChannelingFlow>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State Shockwave(ref StateMachine.State? link, float delay)
        {
            // TODO: some component (knockback? or just make sure autorot uses arms length?)
            var s = CommonStates.Cast(ref link, Boss, AID.Shockwave, delay, 8, "Shockwave");
            s.EndHint |= StateMachine.StateHint.Knockback;
            return s;
        }

        private StateMachine.State OminousBubblingShockwave(ref StateMachine.State? link, float delay)
        {
            // note: can determine bubbling targets by watching 233Cs cast OminousBubblingAOE on two targets
            // TODO: some component...
            var bubbling = CommonStates.Cast(ref link, Boss, AID.OminousBubbling, delay, 3, "TwoStacks");
            bubbling.Exit.Add(() => ActivateComponent(new OminousBubbling()));
            bubbling.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var shockwave = Shockwave(ref bubbling.Next, 2.8f);
            shockwave.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.ComponentCondition<OminousBubbling>(ref shockwave.Next, 3.8f, this, comp => comp.NumCasts > 0, "AOE resolve");
            resolve.Exit.Add(DeactivateComponent<OminousBubbling>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State KampeosHarma(ref StateMachine.State? link, float delay)
        {
            var start = CommonStates.CastStart(ref link, Boss, AID.KampeosHarma, delay);
            start.Enter.Add(() => ActivateComponent(new KampeosHarma(this)));// note: icons appear right before harma cast start...
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 8.4f, "Harma");
            end.EndHint |= StateMachine.StateHint.DowntimeStart | StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.Condition(ref end.Next, 7.5f, () => Boss()?.IsTargetable ?? true, "Harma resolve", 1, 1); // protection for boss becoming untargetable slightly later than cast end
            resolve.Exit.Add(DeactivateComponent<KampeosHarma>);
            resolve.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }
    }
}
