using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    public class P1S : IBossModule
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

        private WorldState _ws;
        private WorldState.Actor? _boss;
        private string _hint = "";
        private MiniArena _arena = new();
        private StateMachine _sm = new();
        private StateMachine.State? _stateInitial;

        // debug fields...
        public StateMachine StateMachine => _sm;
        public StateMachine.State? InitialState => _stateInitial;

        public P1S(WorldState ws)
        {
            _ws = ws;
            _ws.PlayerInCombatChanged += EnterExitCombat;
            _ws.ActorCreated += ActorCreated;
            _ws.ActorDestroyed += ActorDestroyed;
            foreach (var v in _ws.Actors)
                ActorCreated(null, v.Value);
        }

        public void Dispose()
        {
            _ws.PlayerInCombatChanged -= EnterExitCombat;
            _ws.ActorCreated -= ActorCreated;
            _ws.ActorDestroyed -= ActorDestroyed;
        }

        public void Draw(float cameraAzimuth)
        {
            _sm.Update();
            _sm.Draw();
            ImGui.Text(_hint);

            _arena.Begin(cameraAzimuth);
            _arena.BorderSquare();
            if (_boss != null)
                _arena.Actor(_boss.Position, 0xff0000ff);
            _arena.End();
        }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            _sm.ActiveState = inCombat ? _stateInitial : null;
        }

        private void ActorCreated(object? sender, WorldState.Actor actor)
        {
            if ((OID)actor.OID == OID.Boss)
            {
                _boss = actor;
                RebuildStateMachine();
            }
        }

        private void ActorDestroyed(object? sender, WorldState.Actor actor)
        {
            if (_boss == actor)
            {
                _boss = null;
                RebuildStateMachine();
            }
        }

        private void RebuildStateMachine()
        {
            _hint = "";
            if (_boss == null)
            {
                _sm.ActiveState = _stateInitial = null;
                return;
            }

            StateMachine.State? s;
            s = BuildTankbusterState(ref _stateInitial, _boss, 8);

            s = CommonStates.CastStart(ref s.Next, _boss, AID.AetherialShackles, 6);
            s = BuildShacklesCastEndState(ref s.Next, _boss);
            s = BuildWarderWrathState(ref s.Next, _boss, 4, true);
            s = BuildShacklesResolveState(ref s.Next, 10);

            s = BuildFlailStates(ref s.Next, _boss, 4);
            s = BuildKnockbackStates(ref s.Next, _boss, 5);
            s = BuildFlailStates(ref s.Next, _boss, 3);
            s = BuildWarderWrathState(ref s.Next, _boss, 5);

            s = BuildIntemperanceExplosionStart(ref s.Next, _boss, 11);
            s = BuildWarderWrathState(ref s.Next, _boss, 1, true);
            s = BuildWarderWrathState(ref s.Next, _boss, 5, true, "Cube2"); // cube2 and aoe start happen at almost same time
            s = CommonStates.Timeout(ref s.Next, 6, "Cube3");

            s = BuildKnockbackStates(ref s.Next, _boss, 5);

            s = BuildCellsState(ref s.Next, _boss, 8);
            s = BuildAetherflailStates(ref s.Next, _boss, 8);
            s = BuildKnockbackStates(ref s.Next, _boss, 7);
            s = BuildAetherflailStates(ref s.Next, _boss, 2);
            s = CommonStates.CastStart(ref s.Next, _boss, AID.ShacklesOfTime, 4);
            s = BuildShacklesOfTimeCastEndState(ref s.Next, _boss);
            s = BuildTankbusterState(ref s.Next, _boss, 5, true);
            s = BuildShacklesOfTimeResolveState(ref s.Next, 5);
            s = BuildSlamShutState(ref s.Next, _boss, 1);

            s = CommonStates.Cast(ref s.Next, _boss, AID.FourShackles, 13, 3, "FourShackles");
            s.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            s = CommonStates.Timeout(ref s.Next, 10, "Hit1");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit2");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit3");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit4");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildWarderWrathState(ref s.Next, _boss, 5);

            s = BuildIntemperanceExplosionStart(ref s.Next, _boss, 11);
            s = BuildFlailStartState(ref s.Next, _boss, 3);
            s = CommonStates.Timeout(ref s.Next, 8, "Cube2");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s = CommonStates.CastEnd(ref s.Next, _boss, 3);
            s = CommonStates.Timeout(ref s.Next, 4, "Flails");
            s.EndHint |= StateMachine.StateHint.GroupWithNext;
            s.Exit = () => _hint = "";
            s = CommonStates.Timeout(ref s.Next, 4, "Cube3");
            s.EndHint |= StateMachine.StateHint.PositioningEnd;

            s = BuildWarderWrathState(ref s.Next, _boss, 3);

            s = BuildCellsState(ref s.Next, _boss, 11);
            var fork = CommonStates.Simple(ref s.Next, 6, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback");

            // subsequences
            StateMachine.State? s1b = null, s1e = null;
            s1e = BuildShacklesCastEndState(ref s1b, _boss);
            s1e = CommonStates.Cast(ref s1e.Next, _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s1e = CommonStates.Cast(ref s1e.Next, _boss, AID.Aetherchain, 3, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s1e = BuildShacklesResolveState(ref s1e.Next, 0);
            s1e = BuildWarderWrathState(ref s1e.Next, _boss, 7);
            s1e = CommonStates.CastStart(ref s1e.Next, _boss, AID.ShacklesOfTime, 6);
            s1e = BuildShacklesOfTimeCastEndState(ref s1e.Next, _boss);
            s1e = BuildKnockbackStates(ref s1e.Next, _boss, 2, true);
            s1e = BuildShacklesOfTimeResolveState(ref s1e.Next, 3);
            s1e = BuildWarderWrathState(ref s1e.Next, _boss, 3);

            StateMachine.State? s2b = null, s2e = null;
            s2e = BuildShacklesOfTimeCastEndState(ref s2b, _boss);
            s2e = BuildKnockbackStates(ref s2e.Next, _boss, 2, true);
            s2e = BuildShacklesOfTimeResolveState(ref s2e.Next, 3);
            s2e = BuildWarderWrathState(ref s2e.Next, _boss, 3);
            s2e = CommonStates.CastStart(ref s2e.Next, _boss, AID.AetherialShackles, 9);
            s2e = BuildShacklesCastEndState(ref s2e.Next, _boss);
            s2e = CommonStates.Cast(ref s2e.Next, _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s2e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s2e = CommonStates.Cast(ref s2e.Next, _boss, AID.Aetherchain, 3, 5, "Aetherchain");
            s1e.EndHint |= StateMachine.StateHint.GroupWithNext;
            s2e = BuildShacklesResolveState(ref s2e.Next, 0);
            s2e = BuildWarderWrathState(ref s2e.Next, _boss, 7);

            fork.PotentialSuccessors = new[] { s1b!, s2b! };
            fork.EndHint |= StateMachine.StateHint.BossCastStart;
            fork.Update = (float timeSinceTransition) =>
            {
                if (_boss.CastInfo == null)
                    return;
                fork.Done = true;
                switch ((AID)_boss.CastInfo.ActionID)
                {
                    case AID.AetherialShackles:
                        fork.Next = s1b;
                        break;
                    case AID.ShacklesOfTime:
                        fork.Next = s2b;
                        break;
                    default:
                        Service.Log($"Unexpected spell cast start: {_boss.CastInfo.ActionID}");
                        break;
                }
            };

            // forks merge
            s = BuildAetherflailStates(ref s1e.Next, _boss, 9);
            s2e.Next = s1e.Next;
            s = BuildAetherflailStates(ref s.Next, _boss, 6);
            s = BuildAetherflailStates(ref s.Next, _boss, 6);
            s = BuildWarderWrathState(ref s.Next, _boss, 13);
            s = CommonStates.Simple(ref s.Next, 2, "?????");
        }

        private StateMachine.State BuildTankbusterState(ref StateMachine.State? link, WorldState.Actor boss, float delay, bool partOfGroup = false)
        {
            var s = CommonStates.Cast(ref link, boss, AID.HeavyHand, delay, 5, "HeavyHand");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            if (partOfGroup)
                s.EndHint |= StateMachine.StateHint.GroupWithNext;
            return s;
        }

        private StateMachine.State BuildWarderWrathState(ref StateMachine.State? link, WorldState.Actor boss, float delay, bool partOfGroup = false, string startName = "")
        {
            var start = CommonStates.CastStart(ref link, boss, AID.WarderWrath, delay, startName);
            start.EndHint |= StateMachine.StateHint.GroupWithNext;
            var end = CommonStates.CastEnd(ref start.Next, boss, 5, "Wrath");
            end.EndHint |= StateMachine.StateHint.Raidwide;
            if (partOfGroup)
                end.EndHint |= StateMachine.StateHint.GroupWithNext;
            return end;
        }

        // note: shackles are always combined with some other following mechanic, or at very least with resolve
        private StateMachine.State BuildShacklesCastEndState(ref StateMachine.State? link, WorldState.Actor boss)
        {
            var s = CommonStates.CastEnd(ref link, boss, 3, "Shackles");
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

        private StateMachine.State BuildShacklesOfTimeCastEndState(ref StateMachine.State? link, WorldState.Actor boss)
        {
            var s = CommonStates.CastEnd(ref link, boss, 4, "ShacklesOfTime");
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

        private StateMachine.State BuildFlailStartState(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            // TODO: better helper...
            var start = CommonStates.CastStart(ref link, boss, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            start.Exit = () =>
            {
                if (boss.CastInfo == null)
                    return;
                switch ((AID)boss.CastInfo.ActionID)
                {
                    case AID.GaolerFlailRL:
                        _hint = "Order: right->left";
                        break;
                    case AID.GaolerFlailLR:
                        _hint = "Order: left->right";
                        break;
                    case AID.GaolerFlailIO1:
                    case AID.GaolerFlailIO2:
                        _hint = "Order: in->out";
                        break;
                    case AID.GaolerFlailOI1:
                    case AID.GaolerFlailOI2:
                        _hint = "Order: out->in";
                        break;
                    default:
                        Service.Log($"Unexpected flails action: {boss.CastInfo.ActionID}");
                        _hint = "Order: ???";
                        break;
                }
            };
            return start;
        }

        private StateMachine.State BuildFlailStates(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            var start = BuildFlailStartState(ref link, boss, delay);
            var end = CommonStates.CastEnd(ref start.Next, boss, 12);
            var resolve = CommonStates.Timeout(ref end.Next, 4, "Flails");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        private StateMachine.State BuildAetherflailStates(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            // TODO: better helper...
            var start = CommonStates.CastStart(ref link, boss, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            start.Exit = () =>
            {
                switch ((AID)boss.CastInfo!.ActionID)
                {
                    case AID.Aetherflail1:
                    case AID.Aetherflail2:
                    case AID.Aetherflail3:
                    case AID.Aetherflail4:
                        _hint = "Order: unknown";
                        break;
                    default:
                        Service.Log($"Unexpected flails action: {boss.CastInfo.ActionID}");
                        _hint = "Order: ???";
                        break;
                }
            };
            var end = CommonStates.CastEnd(ref start.Next, boss, 12);
            var resolve = CommonStates.Timeout(ref end.Next, 4, "Aetherflail");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        // part of group => group-with-next hint + no positioning hints
        private StateMachine.State BuildKnockbackStates(ref StateMachine.State? link, WorldState.Actor boss, float delay, bool partOfGroup = false)
        {
            // TODO: better helper...
            var start = CommonStates.CastStart(ref link, boss, delay);
            if (!partOfGroup)
                start.EndHint |= StateMachine.StateHint.PositioningStart;
            start.Exit = () =>
            {
                if (boss.CastInfo == null)
                    return;
                switch ((AID)boss.CastInfo.ActionID)
                {
                    case AID.KnockbackGrace:
                        _hint = "What to do: stack!";
                        break;
                    case AID.KnockbackPurge:
                        _hint = "What to do: gtfo!";
                        break;
                    default:
                        Service.Log($"Unexpected knockback cast: {boss.CastInfo.ActionID}");
                        _hint = "What to do: ???";
                        break;
                }
            };
            var end = CommonStates.CastEnd(ref start.Next, boss, 5, "Knockback");
            end.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Tankbuster;
            var resolve = CommonStates.Timeout(ref end.Next, 5, "Explode");
            resolve.EndHint |= partOfGroup ? StateMachine.StateHint.GroupWithNext : StateMachine.StateHint.PositioningEnd;
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        // intemperance cast start/end + explosion start/end + first resolve
        private StateMachine.State BuildIntemperanceExplosionStart(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            var intemp = CommonStates.Cast(ref link, boss, AID.Intemperance, delay, 2, "Intemperance");
            intemp.EndHint |= StateMachine.StateHint.GroupWithNext;
            var explosion = CommonStates.CastStart(ref intemp.Next, boss, 6);
            explosion.Exit = () =>
            {
                if (boss.CastInfo == null)
                    return;
                switch ((AID)boss.CastInfo.ActionID)
                {
                    case AID.IntemperateTormentUp:
                        _hint = "Explosion order: bottom->top";
                        break;
                    case AID.IntemperateTormentDown:
                        _hint = "Explosion order: top->bottom";
                        break;
                    default:
                        Service.Log($"Unexpected intemperance explosion cast: {boss.CastInfo.ActionID}");
                        _hint = "Explosion order: ???";
                        break;
                }
            };
            var end = CommonStates.CastEnd(ref explosion.Next, boss, 10);
            var resolve = CommonStates.Timeout(ref end.Next, 1, "Cube1");
            resolve.EndHint |= StateMachine.StateHint.GroupWithNext;
            return resolve;
        }

        private StateMachine.State BuildCellsState(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            var s = CommonStates.Cast(ref link, boss, AID.ShiningCells, delay, 7, "Cells");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => { }; // TODO: begin cells
            return s;
        }

        private StateMachine.State BuildSlamShutState(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            var s = CommonStates.Cast(ref link, boss, AID.SlamShut, delay, 6, "SlamShut");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit = () => { }; // TODO: end cells
            return s;
        }
    }
}
