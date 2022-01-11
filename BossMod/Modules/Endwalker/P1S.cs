using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    public class P1S : BossModule
    {
        public enum OID : uint
        {
            Boss = 0x3522,
            Helper = 0x233C,
            FlailLR = 0x3523, // "anchor" weapon, purely visual
            FlailI = 0x3524, // "ball" weapon, also used for knockbacks
            FlailO = 0x3525, // "chakram" weapon
        };

        public enum AID : uint
        {
            GaolerFlailRL = 26102, // Boss->Boss
            GaolerFlailLR = 26103, // Boss->Boss
            GaolerFlailIO1 = 26104, // Boss->Boss
            GaolerFlailIO2 = 26105, // Boss->Boss
            GaolerFlailOI1 = 26106, // Boss->Boss
            GaolerFlailOI2 = 26107, // Boss->Boss
            Aetherflail1 = 26114, // Boss->Boss -- seen BlueRI & RedRO
            Aetherflail2 = 26115, // Boss->Boss -- seen BlueLO, RedLI, RedLO - maybe it's *L*?
            Aetherflail3 = 26118, // Boss->Boss -- seen BlueOL
            Aetherflail4 = 26119, // Boss->Boss -- seen RedOR
            KnockbackGrace = 26126, // Boss->MT
            KnockbackPurge = 26127, // Boss->MT
            ShiningCells = 26134, // Boss->Boss, raidwide aoe
            SlamShut = 26135, // Boss->Boss, raidwide aoe
            Aetherchain = 26137, // Boss->Boss
            ShacklesOfTime = 26140, // Boss->Boss
            Intemperance = 26142, // Boss->Boss
            IntemperateTormentUp = 26143, // Boss->Boss (bottom->top)
            IntemperateTormentDown = 26144, // Boss->Boss (bottom->top)
            AetherialShackles = 26149, // Boss->Boss
            FourShackles = 26150, // Boss->Boss
            HeavyHand = 26153, // Boss->MT, generic tankbuster
            WarderWrath = 26154, // Boss->Boss, generic raidwide
            GaolerFlailR1 = 28070, // Helper->Helper, first hit, right-hand cone
            GaolerFlailL1 = 28071, // Helper->Helper, first hit, left-hand cone
            GaolerFlailI1 = 28072, // Helper->Helper, first hit, point-blank
            GaolerFlailO1 = 28073, // Helper->Helper, first hit, donut
            GaolerFlailR2 = 28074, // Helper->Helper, second hit, right-hand cone
            GaolerFlailL2 = 28075, // Helper->Helper, second hit, left-hand cone
            GaolerFlailI2 = 28076, // Helper->Helper, second hit, point-blank
            GaolerFlailO2 = 28077, // Helper->Helper, second hit, donut
        };

        private WorldState.Actor? _boss;
        private string _hint = "";

        public P1S(WorldState ws)
            : base(ws)
        {
            foreach (var v in WorldState.Actors)
                ActorCreated(v.Value);

            StateMachine.State? s;
            s = BuildTankbusterState(ref InitialState, 8);

            s = CommonStates.CastStart(ref s.Next, () => _boss, AID.AetherialShackles, 6);
            s = BuildShacklesCastEndState(ref s.Next);
            s = BuildWarderWrathState(ref s.Next, 4, true);
            s = BuildShacklesResolveState(ref s.Next, 10);

            s = BuildFlailStates(ref s.Next, 4);
            s = BuildKnockbackStates(ref s.Next, 5);
            s = BuildFlailStates(ref s.Next, 3);
            s = BuildWarderWrathState(ref s.Next, 5);

            s = BuildIntemperanceExplosionStart(ref s.Next, 11);
            s = BuildWarderWrathState(ref s.Next, 1, true);
            s = BuildWarderWrathState(ref s.Next, 5, true, "Cube2"); // cube2 and aoe start happen at almost same time
            s = CommonStates.Timeout(ref s.Next, 6, "Cube3");

            s = BuildKnockbackStates(ref s.Next, 5);

            s = BuildCellsState(ref s.Next, 8);
            s = BuildAetherflailStates(ref s.Next, 8);
            s = BuildKnockbackStates(ref s.Next, 7);
            s = BuildAetherflailStates(ref s.Next, 2);
            s = CommonStates.CastStart(ref s.Next, () => _boss, AID.ShacklesOfTime, 4);
            s = BuildShacklesOfTimeCastEndState(ref s.Next);
            s = BuildTankbusterState(ref s.Next, 5, true);
            s = BuildShacklesOfTimeResolveState(ref s.Next, 5);
            s = BuildSlamShutState(ref s.Next, 1);

            s = CommonStates.Cast(ref s.Next, () => _boss, AID.FourShackles, 13, 3, "FourShackles");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 10, "Hit1");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit2");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit3");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit4");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildWarderWrathState(ref s.Next, 5);

            s = BuildIntemperanceExplosionStart(ref s.Next, 11);
            s = BuildFlailStartState(ref s.Next, 3);
            s = CommonStates.Timeout(ref s.Next, 8, "Cube2");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.CastEnd(ref s.Next, () => _boss, 3);
            s = CommonStates.Timeout(ref s.Next, 4, "Flails");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s.Exit = () => _hint = "";
            s = CommonStates.Timeout(ref s.Next, 4, "Cube3");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildWarderWrathState(ref s.Next, 3);

            s = BuildCellsState(ref s.Next, 11);

            // subsequences
            StateMachine.State? s1b = null, s1e = null;
            s1e = BuildShacklesCastEndState(ref s1b);
            s1e = CommonStates.Cast(ref s1e.Next, () => _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s1e = CommonStates.Cast(ref s1e.Next, () => _boss, AID.Aetherchain, 3, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s1e = BuildShacklesResolveState(ref s1e.Next, 0);
            s1e = BuildWarderWrathState(ref s1e.Next, 7);
            s1e = CommonStates.CastStart(ref s1e.Next, () => _boss, AID.ShacklesOfTime, 6);
            s1e = BuildShacklesOfTimeCastEndState(ref s1e.Next);
            s1e = BuildKnockbackStates(ref s1e.Next, 2, true);
            s1e = BuildShacklesOfTimeResolveState(ref s1e.Next, 3);
            s1e = BuildWarderWrathState(ref s1e.Next, 3);

            StateMachine.State? s2b = null, s2e = null;
            s2e = BuildShacklesOfTimeCastEndState(ref s2b);
            s2e = BuildKnockbackStates(ref s2e.Next, 2, true);
            s2e = BuildShacklesOfTimeResolveState(ref s2e.Next, 3);
            s2e = BuildWarderWrathState(ref s2e.Next, 3);
            s2e = CommonStates.CastStart(ref s2e.Next, () => _boss, AID.AetherialShackles, 9);
            s2e = BuildShacklesCastEndState(ref s2e.Next);
            s2e = CommonStates.Cast(ref s2e.Next, () => _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s2e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s2e = CommonStates.Cast(ref s2e.Next, () => _boss, AID.Aetherchain, 3, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s2e = BuildShacklesResolveState(ref s2e.Next, 0);
            s2e = BuildWarderWrathState(ref s2e.Next, 7);

            Dictionary<AID, (StateMachine.State?, Action)> forkDispatch = new();
            forkDispatch[AID.AetherialShackles] = new(s1b, () => { });
            forkDispatch[AID.ShacklesOfTime] = new(s2b, () => { });
            var fork = CommonStates.CastStart(ref s.Next, () => _boss, forkDispatch, 6, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback");

            // forks merge
            s = BuildAetherflailStates(ref s1e.Next, 9);
            s2e.Next = s1e.Next;
            s = BuildAetherflailStates(ref s.Next, 6);
            s = BuildAetherflailStates(ref s.Next, 6);
            s = BuildWarderWrathState(ref s.Next, 13);
            s = CommonStates.Simple(ref s.Next, 2, "?????");
        }

        public override void Draw(float cameraAzimuth)
        {
            base.Draw(cameraAzimuth);
            ImGui.Text(_hint);

            // TODO: what part of this should be done by the framework?..
            Arena.Begin(cameraAzimuth);
            Arena.BorderSquare();
            if (_boss != null)
                Arena.Actor(_boss.Position, 0xff0000ff);
            Arena.End();

            // TODO: I think framework should do this, since it should provide access to CD planners...
            if (ImGui.Button("Show timeline"))
            {
                var timeline = new StateMachineVisualizer(InitialState);
                var w = WindowManager.CreateWindow("P1S Timeline", () => timeline.Draw(StateMachine), () => { });
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
            _hint = "";
        }

        private StateMachine.State BuildTankbusterState(ref StateMachine.State? link, float delay, bool partOfGroup = false)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.HeavyHand, delay, 5, "HeavyHand");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            if (partOfGroup)
                s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        private StateMachine.State BuildWarderWrathState(ref StateMachine.State? link, float delay, bool partOfGroup = false, string startName = "")
        {
            var start = CommonStates.CastStart(ref link, () => _boss, AID.WarderWrath, delay, startName);
            start.EndHint |= StateMachine.StateHint.GroupWithNext;
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Wrath");
            end.EndHint |= StateMachine.StateHint.Raidwide;
            if (partOfGroup)
                end.EndHint |= StateMachine.StateHint.GroupWithNext;
            return end;
        }

        // note: shackles are always combined with some other following mechanic, or at very least with resolve
        private StateMachine.State BuildShacklesCastEndState(ref StateMachine.State? link)
        {
            var s = CommonStates.CastEnd(ref link, () => _boss, 3, "Shackles");
            s.EndHint |= StateMachine.StateHint.PositioningStart | StateMachine.StateHint.GroupWithNext;
            s.Exit = () => { }; // TODO: start shackles helper
            return s;
        }

        // delay from cast-end is 19 seconds, but we usually have some intermediate states
        private StateMachine.State BuildShacklesResolveState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Timeout(ref link, delay, "Shackles resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;
            s.Exit = () => { }; // TODO: end shackles helper
            return s;
        }

        private StateMachine.State BuildShacklesOfTimeCastEndState(ref StateMachine.State? link)
        {
            var s = CommonStates.CastEnd(ref link, () => _boss, 4, "ShacklesOfTime");
            s.EndHint |= StateMachine.StateHint.PositioningStart | StateMachine.StateHint.GroupWithNext;
            s.Exit = () => { }; // TODO: SOT helper?.. or activate by debuff independently from state machines?..
            return s;
        }

        // delay from cast-end is 15 seconds, but we usually have some intermediate states
        private StateMachine.State BuildShacklesOfTimeResolveState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Timeout(ref link, delay, "Shackles resolve");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;
            s.Exit = () => { }; // TODO: end SOT helper
            return s;
        }

        private StateMachine.State BuildFlailStartState(ref StateMachine.State? link, float delay)
        {
            // TODO: better helper...
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.GaolerFlailRL] = new(null, () => _hint = "Order: right->left");
            dispatch[AID.GaolerFlailLR] = new(null, () => _hint = "Order: left->right");
            dispatch[AID.GaolerFlailIO1] = new(null, () => _hint = "Order: in->out");
            dispatch[AID.GaolerFlailIO2] = new(null, () => _hint = "Order: in->out");
            dispatch[AID.GaolerFlailOI1] = new(null, () => _hint = "Order: out->in");
            dispatch[AID.GaolerFlailOI2] = new(null, () => _hint = "Order: out->in");
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            return start;
        }

        private StateMachine.State BuildFlailStates(ref StateMachine.State? link, float delay)
        {
            var start = BuildFlailStartState(ref link, delay);
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 12);
            var resolve = CommonStates.Timeout(ref end.Next, 4, "Flails");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        private StateMachine.State BuildAetherflailStates(ref StateMachine.State? link, float delay)
        {
            // TODO: better helper...
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.Aetherflail1] = new(null, () => _hint = "Order: unknown");
            dispatch[AID.Aetherflail2] = new(null, () => _hint = "Order: unknown");
            dispatch[AID.Aetherflail3] = new(null, () => _hint = "Order: unknown");
            dispatch[AID.Aetherflail4] = new(null, () => _hint = "Order: unknown");
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 12);
            var resolve = CommonStates.Timeout(ref end.Next, 4, "Aetherflail");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        // part of group => group-with-next hint + no positioning hints
        private StateMachine.State BuildKnockbackStates(ref StateMachine.State? link, float delay, bool partOfGroup = false)
        {
            // TODO: better helper...
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.KnockbackGrace] = new(null, () => _hint = "What to do: stack!");
            dispatch[AID.KnockbackPurge] = new(null, () => _hint = "What to do: gtfo!");
            var start = CommonStates.CastStart(ref link, () => _boss, dispatch, delay);
            if (!partOfGroup)
                start.EndHint |= StateMachine.StateHint.PositioningStart;
            var end = CommonStates.CastEnd(ref start.Next, () => _boss, 5, "Knockback");
            end.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Tankbuster;
            var resolve = CommonStates.Timeout(ref end.Next, 5, "Explode");
            resolve.EndHint |= partOfGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        // intemperance cast start/end + explosion start/end + first resolve
        private StateMachine.State BuildIntemperanceExplosionStart(ref StateMachine.State? link, float delay)
        {
            var intemp = CommonStates.Cast(ref link, () => _boss, AID.Intemperance, delay, 2, "Intemperance");
            intemp.EndHint |= StateMachine.StateHint.GroupWithNext;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.IntemperateTormentUp] = new(null, () => _hint = "Explosion order: bottom->top");
            dispatch[AID.IntemperateTormentDown] = new(null, () => _hint = "Explosion order: top->bottom");
            var explosion = CommonStates.CastStart(ref intemp.Next, () => _boss, dispatch, 6);
            var end = CommonStates.CastEnd(ref explosion.Next, () => _boss, 10);
            var resolve = CommonStates.Timeout(ref end.Next, 1, "Cube1");
            resolve.EndHint |= StateMachine.StateHint.GroupWithNext;
            return resolve;
        }

        private StateMachine.State BuildCellsState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.ShiningCells, delay, 7, "Cells");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => { }; // TODO: begin cells
            return s;
        }

        private StateMachine.State BuildSlamShutState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.SlamShut, delay, 6, "SlamShut");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => { }; // TODO: end cells
            return s;
        }
    }
}
