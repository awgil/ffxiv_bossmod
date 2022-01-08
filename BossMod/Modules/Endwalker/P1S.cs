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

        public bool Paused
        {
            get => _sm.Paused;
            set => _sm.Paused = value;
        }

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
            s = CommonStates.Cast(ref _stateInitial, _boss, AID.HeavyHand, 8, 5, "Tankbuster");

            s = CommonStates.CastStart(ref s.Next, _boss, AID.AetherialShackles, 6);
            s.Enter = () => { }; // TODO: start shackles helper
            s = CommonStates.CastEnd(ref s.Next, _boss, 3, "Shackles");
            s.Substate = true;
            s = CommonStates.Cast(ref s.Next, _boss, AID.WarderWrath, 4, 5, "AOE");
            s.Substate = true;
            s = CommonStates.Timeout(ref s.Next, 10, "Shackles resolve");
            s.Exit = () => { }; // TODO: end shackles helper

            s = BuildFlailStates(ref s.Next, _boss, 4);
            s = BuildKnockbackStates(ref s.Next, _boss, 5);
            s = BuildFlailStates(ref s.Next, _boss, 3);
            s = CommonStates.Cast(ref s.Next, _boss, AID.WarderWrath, 5, 5, "AOE");

            s = BuildIntemperanceExplosionStart(ref s.Next, _boss, 11);
            s = CommonStates.Cast(ref s.Next, _boss, AID.WarderWrath, 1, 5, "AOE");
            s.Substate = true;
            s = CommonStates.CastStart(ref s.Next, _boss, AID.WarderWrath, 5, "Cube2"); // cube2 and aoe start happen at almost same time
            s.Substate = true;
            s = CommonStates.CastEnd(ref s.Next, _boss, 5, "AOE");
            s.Substate = true;
            s = CommonStates.Timeout(ref s.Next, 6, "Cube3");

            s = BuildKnockbackStates(ref s.Next, _boss, 5);

            s = CommonStates.CastStart(ref s.Next, _boss, AID.ShiningCells, 8);
            s.Enter = () => { }; // TODO: begin cells
            s = CommonStates.CastEnd(ref s.Next, _boss, 7, "Cells");
            s = BuildAetherflailStates(ref s.Next, _boss, 8);
            s = BuildKnockbackStates(ref s.Next, _boss, 7);
            s = BuildAetherflailStates(ref s.Next, _boss, 2);
            s = CommonStates.Cast(ref s.Next, _boss, AID.ShacklesOfTime, 4, 4, "ShacklesOfTime"); // TODO: exit => SOT helper?.. or activate by debuff independently from state machines?..
            s.Substate = true;
            s = CommonStates.Cast(ref s.Next, _boss, AID.HeavyHand, 5, 5, "Tankbuster");
            s.Substate = true;
            s = CommonStates.Timeout(ref s.Next, 5, "Shackles resolve"); // TODO: exit => clear SOT helper
            s = CommonStates.Cast(ref s.Next, _boss, AID.SlamShut, 1, 6, "SlamShut");
            s.Exit = () => { }; // TODO: end cells

            s = CommonStates.Cast(ref s.Next, _boss, AID.FourShackles, 13, 3, "FourShackles");
            s.Substate = true;
            s = CommonStates.Timeout(ref s.Next, 10, "Hit1");
            s.Substate = true;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit2");
            s.Substate = true;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit3");
            s.Substate = true;
            s = CommonStates.Timeout(ref s.Next, 5, "Hit4");

            s = CommonStates.Cast(ref s.Next, _boss, AID.WarderWrath, 5, 5, "AOE");

            s = BuildIntemperanceExplosionStart(ref s.Next, _boss, 11);
            s = BuildFlailStartState(ref s.Next, _boss, 3);
            s = CommonStates.Timeout(ref s.Next, 8, "Cube2");
            s.Substate = true;
            s = CommonStates.CastEnd(ref s.Next, _boss, 3);
            s = CommonStates.Timeout(ref s.Next, 4, "Flails");
            s.Substate = true;
            s.Exit = () => _hint = "";
            s = CommonStates.Timeout(ref s.Next, 4, "Cube3");

            s = CommonStates.Cast(ref s.Next, _boss, AID.WarderWrath, 3, 5, "AOE");

            s = CommonStates.CastStart(ref s.Next, _boss, AID.ShiningCells, 11);
            s.Enter = () => { }; // TODO: begin cells
            s = CommonStates.CastEnd(ref s.Next, _boss, 7, "Cells");

            // subsequences
            StateMachine.State? s1b = null, s1e = null;
            s1e = CommonStates.CastEnd(ref s1b, _boss, 3, "Shackles");
            s1e.Substate = true;
            s1e = CommonStates.Cast(ref s1e.Next, _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s1e.Substate = true;
            s1e = CommonStates.Cast(ref s1e.Next, _boss, AID.Aetherchain, 3, 5, "Aetherchain + Shackles resolve");
            s1e.Exit = () => { }; // TODO: end shackles helper
            s1e = CommonStates.Cast(ref s1e.Next, _boss, AID.WarderWrath, 7, 5, "AOE");
            s1e = CommonStates.Cast(ref s1e.Next, _boss, AID.ShacklesOfTime, 6, 4, "ShacklesOfTime"); // TODO: exit => SOT helper?.. or activate by debuff independently from state machines?..
            s1e.Substate = true;
            s1e = BuildKnockbackStates(ref s1e.Next, _boss, 2);
            s1e.Substate = true;
            s1e = CommonStates.Timeout(ref s1e.Next, 3, "Shackles resolve"); // TODO: exit => clear SOT helper
            s1e = CommonStates.Cast(ref s1e.Next, _boss, AID.WarderWrath, 3, 5, "AOE");

            StateMachine.State? s2b = null, s2e = null;
            s2e = CommonStates.CastEnd(ref s2b, _boss, 4, "ShacklesOfTime"); // TODO: exit => SOT helper?.. or activate by debuff independently from state machines?..
            s2e.Substate = true;
            s2e = BuildKnockbackStates(ref s2e.Next, _boss, 2);
            s2e.Substate = true;
            s2e = CommonStates.Timeout(ref s2e.Next, 3, "Shackles resolve"); // TODO: exit => clear SOT helper
            s2e = CommonStates.Cast(ref s2e.Next, _boss, AID.WarderWrath, 3, 5, "AOE");
            s2e = CommonStates.CastStart(ref s2e.Next, _boss, AID.AetherialShackles, 9);
            s2e.Enter = () => { }; // TODO: start shackles helper
            s2e = CommonStates.CastEnd(ref s2e.Next, _boss, 3, "Shackles");
            s2e.Substate = true;
            s2e = CommonStates.Cast(ref s2e.Next, _boss, AID.Aetherchain, 6, 5, "Aetherchain");
            s2e.Substate = true;
            s2e = CommonStates.Cast(ref s2e.Next, _boss, AID.Aetherchain, 3, 5, "Aetherchain + Shackles resolve");
            s2e.Exit = () => { }; // TODO: end shackles helper
            s2e = CommonStates.Cast(ref s2e.Next, _boss, AID.WarderWrath, 7, 5, "AOE");

            var fork = CommonStates.Simple(ref s.Next, 6, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback");
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
            s = CommonStates.Cast(ref s.Next, _boss, AID.WarderWrath, 13, 5, "AOE");
            s = CommonStates.Simple(ref s.Next, 0, "?????");

            _stateInitial = fork;
        }

        private StateMachine.State BuildFlailStartState(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            // TODO: better helper...
            var start = CommonStates.CastStart(ref link, boss, delay);
            start.Exit = () =>
            {
                switch ((AID)boss.CastInfo!.ActionID)
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
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        private StateMachine.State BuildAetherflailStates(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            // TODO: better helper...
            var start = CommonStates.CastStart(ref link, boss, delay);
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
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        private StateMachine.State BuildKnockbackStates(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            // TODO: better helper...
            var start = CommonStates.CastStart(ref link, boss, delay);
            start.Exit = () =>
            {
                switch ((AID)boss.CastInfo!.ActionID)
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
            end.Substate = true;
            var resolve = CommonStates.Timeout(ref end.Next, 5, "Explode");
            resolve.Exit = () => _hint = "";
            return resolve;
        }

        // intemperance cast start/end + explosion start/end + first resolve
        private StateMachine.State BuildIntemperanceExplosionStart(ref StateMachine.State? link, WorldState.Actor boss, float delay)
        {
            var intemp = CommonStates.Cast(ref link, boss, AID.Intemperance, delay, 2, "Intemperance");
            intemp.Substate = true;
            var explosion = CommonStates.CastStart(ref intemp.Next, boss, 6);
            explosion.Exit = () =>
            {
                switch ((AID)boss.CastInfo!.ActionID)
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
            resolve.Substate = true;
            return resolve;
        }
    }
}
