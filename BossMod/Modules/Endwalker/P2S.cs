using ImGuiNET;
using System;
using System.Collections.Generic;

namespace BossMod
{
    public class P2S : BossModule
    {
        public enum OID : uint
        {
            Boss = 0x359B,
            CataractHead = 0x359C,
            DissociatedHead = 0x386A,
            Helper = 0x233C, // x24
        };

        public enum AID : uint
        {
            SewageDeluge = 26640, // Boss->Boss
            SpokenCataractSecondary = 26641, // CHead->CHead, half behind is safe
            WingedCataractSecondaryDissoc = 26644, // CHead->CHead, half in front is safe
            WingedCataractSecondary = 26645, // CHead->CHead, half in front is safe
            SpokenCataract = 26647, // Boss->Boss
            WingedCataract = 26648, // Boss->Boss
            Coherence = 26651, // Boss->Boss
            ChannelingFlow = 26654, // Boss->Boss
            Crash = 26657, // Helper->Helper, attack after arrows resolve
            KampeosHarma = 26659, // Boss->Boss
            PredatoryAvarice = 26663, // Boss->Boss
            OminousBubbling = 26666, // Boss->Boss
            OminousBubblingAOE = 26667, // Helper->targets
            Dissociation = 26668, // Boss->Boss
            DissociationAOE = 26670, // DHead->DHead
            Shockwave = 26671, // Boss->impact location
            SewageEruption = 26672, // Boss->Boss
            SewageEruptionAOE = 26673, // Helper->Helper
            DoubledImpact = 26674, // Boss->MT
            MurkyDepths = 26675, // Boss->Boss
            Enrage = 26676, // Boss->Boss
            TaintedFlood = 26679, // Boss->Boss
            TaintedFloodAOE = 26680, // Helper->targets
            ChannelingOverflow = 28098, // Boss->Boss (both 2nd and 3rd arrows)
        };

        private WorldState.Actor? _boss;

        public P2S(WorldState ws)
            : base(ws)
        {
            foreach (var v in WorldState.Actors)
                ActorCreated(v.Value);

            StateMachine.State? s;
            s = BuildMurkyDepthsState(ref InitialState, 10);
            s = BuildDoubledImpactState(ref s.Next, 5);
            s = BuildSewageDelugeState(ref s.Next, 8);
            s = BuildCataractState(ref s.Next, 15);
            s = BuildCoherenceState(ref s.Next, 8);
            s = BuildMurkyDepthsState(ref s.Next, 7);
            s = BuildOminousBubblingState(ref s.Next, 4);

            // avarice + cataract
            // status: 2768 Mark of the Tides - tank+dd, should gtfo
            // status: 2769 Mark of the Depths - healer, stack
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.PredatoryAvarice, 12, 4, "Avarice");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildCataractState(ref s.Next, 10, true);
            s = CommonStates.Timeout(ref s.Next, 7, "Avarice resolve"); // triggered by debuff expiration...
            s.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.Raidwide;
            s.Exit = () => { }; // TODO: end deluge 1 marker mode

            // first flow
            // status: 2770 Fore Mark of the Tides - points North
            // status: 2771 Rear Mark of the Tides - points South
            // status: 2772 Left Mark of the Tides - points East
            // status: 2773 Right Mark of the Tides - points West
            // status: 2656 Stun - 14 sec after cast end
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.ChannelingFlow, 8, 5, "Flow 1");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 14);
            s.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.DowntimeStart;
            s = CommonStates.Timeout(ref s.Next, 3, "Flow resolve");
            s.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide;

            s = BuildDoubledImpactState(ref s.Next, 8);
            s = BuildMurkyDepthsState(ref s.Next, 5);
            s = BuildSewageDelugeState(ref s.Next, 12);
            s = BuildShockwaveState(ref s.Next, 10);
            s = BuildKampeosHarmaState(ref s.Next, 4);
            s = BuildDoubledImpactState(ref s.Next, 9);
            s = BuildMurkyDepthsState(ref s.Next, 4);

            // second flow (same statuses, different durations)
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.ChannelingOverflow, 9, 5, "Flow 2");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.TaintedFlood, 4, 3);
            s = CommonStates.Timeout(ref s.Next, 9, "Hit 1");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.TaintedFlood, 3, 3);
            s = CommonStates.Timeout(ref s.Next, 9, "Hit 2");
            s.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;

            s = BuildCataractState(ref s.Next, 2);
            s.Exit = () => { }; // TODO: end deluge 2 marker mode

            // avarice + dissociation + cataract
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.PredatoryAvarice, 15, 4, "Avarice");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildDissociationState(ref s.Next, 2);
            s = BuildCataractState(ref s.Next, 10);

            // dissociation + eruption + flood + coherence
            s = BuildDissociationState(ref s.Next, 9);
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.SewageEruption, 8, 5, "Eruption");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.TaintedFlood, 2, 3, "Flood");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = BuildCoherenceState(ref s.Next, 5);
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildDoubledImpactState(ref s.Next, 6);
            s = BuildMurkyDepthsState(ref s.Next, 3);
            s = BuildSewageDelugeState(ref s.Next, 13);

            // flow 3 (with coherence)
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.ChannelingOverflow, 12, 5, "Flow 3");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = BuildCoherenceState(ref s.Next, 5); // first hit is around coherence cast end
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 10, "Flow resolve"); // second hit
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            // dissociation + eruption
            s = BuildDissociationState(ref s.Next, 11);
            s = CommonStates.Cast(ref s.Next, () => _boss, AID.SewageEruption, 8, 5, "Eruption");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 5, "Resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildOminousBubblingState(ref s.Next, 3);
            s = BuildDoubledImpactState(ref s.Next, 6);
            s = BuildMurkyDepthsState(ref s.Next, 7);
            s = BuildMurkyDepthsState(ref s.Next, 6);

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.Enrage, 6, 10, "Enrage");
        }

        public override void Draw(float cameraAzimuth)
        {
            base.Draw(cameraAzimuth);

            // TODO: what part of this should be done by the framework?..
            Arena.Begin(cameraAzimuth);
            Arena.Border();
            if (_boss != null)
                Arena.Actor(_boss.Position, 0xff0000ff);
            Arena.End();

            // TODO: I think framework should do this, since it should provide access to CD planners...
            if (ImGui.Button("Show timeline"))
            {
                var timeline = new StateMachineVisualizer(InitialState);
                var w = WindowManager.CreateWindow("P2S Timeline", () => timeline.Draw(StateMachine), () => { });
                w.SizeHint = new(600, 600);
                w.MinSize = new(100, 100);
            }
        }

        protected override void ActorCreated(WorldState.Actor actor)
        {
            if ((OID)actor.OID == OID.Boss)
            {
                _boss = actor;
            }
        }

        protected override void ActorDestroyed(WorldState.Actor actor)
        {
            if (_boss == actor)
            {
                _boss = null;
            }
        }

        protected override void Reset()
        {
        }

        private StateMachine.State BuildMurkyDepthsState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.MurkyDepths, delay, 5, "MurkyDepths");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildDoubledImpactState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.DoubledImpact, delay, 5, "DoubledImpact");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State BuildSewageDelugeState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.SewageDeluge, delay, 5, "Deluge");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => { }; // TODO: start showing deluge markers on arena, allow selecting oneshot corner
            return s;
        }

        // if group continues, last state won't clear positioning flag
        private StateMachine.State BuildCataractState(ref StateMachine.State? link, float delay, bool continueGroup = false)
        {
            // TODO: helpers!!!
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.SpokenCataract] = new(null, () => { });
            dispatch[AID.WingedCataract] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 8, "Cataract");
            end.EndHint |= continueGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            return end;
        }

        // dissociation is always part of the group
        private StateMachine.State BuildDissociationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.Dissociation, delay, 4, "Dissociation");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s.Exit = () => { }; // TODO: start dissociation mode
            return s;
        }

        private StateMachine.State BuildCoherenceState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.Coherence, delay, 12, 4, "Coherence");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildShockwaveState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.Shockwave, delay, 8, "Shockwave");
            s.EndHint |= StateMachine.StateHint.Knockback;
            return s;
        }

        // includes shockwave
        private StateMachine.State BuildOminousBubblingState(ref StateMachine.State? link, float delay)
        {
            // note: can determine bubbling targets by watching 233Cs cast OminousBubblingAOE on two targets
            var bubbling = CommonStates.Cast(ref link, () => _boss, AID.OminousBubbling, delay, 3, "TwoStacks");
            bubbling.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            var shockwave = BuildShockwaveState(ref bubbling.Next, 3);
            shockwave.EndHint |= StateMachine.StateHint.GroupWithNext;
            var resolve = CommonStates.Timeout(ref shockwave.Next, 3, "AOE resolve");
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State BuildKampeosHarmaState(ref StateMachine.State? link, float delay)
        {
            var start = CommonStates.CastStart(ref link, () => _boss, delay);
            start.Enter = () => { }; // TODO: start harma helper ui...
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 8, "Harma");
            end.EndHint |= StateMachine.StateHint.DowntimeStart | StateMachine.StateHint.GroupWithNext;
            var resolve = CommonStates.Timeout(ref end.Next, 8, "Harma resolve");
            resolve.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }
    }
}
