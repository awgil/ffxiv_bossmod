using BossMod;
using BossMod.P4S;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIDev
{
    class P4STest : ITest
    {
        private float _azimuth;
        private WorldState _ws;
        private P4S _o;
        private DateTime _prevFrame;
        private bool _paused;

        public P4STest()
        {
            _ws = new();
            _ws.CurrentTime = _prevFrame = DateTime.Now;
            _ws.Actors.Add(1, 0, "T1", ActorType.Player, Class.WAR, new(105, 0, 100, 0));
            _ws.Actors.Add(2, 0, "T2", ActorType.Player, Class.PLD, new(100, 0, 105, 0));
            _ws.Actors.Add(3, 0, "H1", ActorType.Player, Class.WHM, new(100, 0, 95, 0));
            _ws.Actors.Add(4, 0, "H2", ActorType.Player, Class.SGE, new(95, 0, 100, 0));
            _ws.Actors.Add(5, 0, "R1", ActorType.Player, Class.BLM, new(110, 0, 110, 0));
            _ws.Actors.Add(6, 0, "R2", ActorType.Player, Class.MCH, new(90, 0, 110, 0));
            _ws.Actors.Add(7, 0, "M1", ActorType.Player, Class.MNK, new(110, 0, 90, 0));
            _ws.Actors.Add(8, 0, "M2", ActorType.Player, Class.RPR, new(90, 0, 90, 0));
            _ws.Actors.Add(9, (uint)OID.Boss1, "Boss1", ActorType.Enemy, Class.None, new(100, 0, 100, 0));
            _ws.Actors.Add(10, (uint)OID.Boss2, "Boss2", ActorType.Enemy, Class.None, new(100, 0, 100, 0));
            _ws.Actors.Add(11, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(90, 0, 90, 0));
            _ws.Actors.Add(12, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(110, 0, 90, 0));
            _ws.Actors.Add(13, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(90, 0, 110, 0));
            _ws.Actors.Add(14, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(110, 0, 110, 0));
            _ws.Actors.Add(15, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(94.34f, 0, 94.34f, 0));
            _ws.Actors.Add(16, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(94.34f, 0, 105.66f, 0));
            _ws.Actors.Add(17, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(105.66f, 0, 94.34f, 0));
            _ws.Actors.Add(18, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(105.66f, 0, 105.66f, 0));
            for (int i = 1; i <= 8; ++i)
                _ws.Party.Add((ulong)i, _ws.Actors.Find((uint)i), i == 1);
            _o = new P4S(_ws);
        }

        public void Dispose()
        {
            _o.Dispose();
        }

        public void Draw()
        {
            var now = DateTime.Now;
            if (!_paused)
                _ws.CurrentTime += now - _prevFrame;
            _prevFrame = now;

            _o.Update();

            _o.Draw(_azimuth / 180 * MathF.PI, PartyState.PlayerSlot, null);

            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);

            var boss1 = _ws.Actors.Find(9);
            var boss2 = _ws.Actors.Find(10)!;
            if (ImGui.Button(!_ws.PlayerInCombat ? "Pull" : "Wipe"))
            {
                if (boss1 != null)
                    _ws.Actors.UpdateCastInfo(boss1, null);
                _ws.Actors.UpdateCastInfo(boss2, null);
                _ws.PlayerInCombat = !_ws.PlayerInCombat;
            }

            ImGui.SameLine();
            if (ImGui.Button(_paused ? "Resume" : "Pause"))
                _paused = !_paused;

            ImGui.SameLine();
            bool phase1 = boss1 != null;
            if (ImGui.Checkbox("Phase 1", ref phase1))
            {
                if (phase1)
                {
                    boss1 = _ws.Actors.Add(9, (uint)OID.Boss1, "Boss1", ActorType.Enemy, Class.None, new(100, 0, 100, 0));
                }
                else
                {
                    _ws.Actors.Remove(9);
                    boss1 = null;
                }
            }

            ImGui.SameLine();
            ImGui.Text($"Downtime in: {_o.StateMachine.EstimateTimeToNextDowntime():f2}, Positioning in: {_o.StateMachine.EstimateTimeToNextPositioning():f2}");

            var boss = boss1 ?? boss2;

            foreach (var actor in _ws.Actors)
            {
                var pos = actor.Position;
                var rot = actor.Rotation / MathF.PI * 180;
                ImGui.SetNextItemWidth(100);
                ImGui.DragFloat($"X##{actor.InstanceID}", ref pos.X, 0.25f, 80, 120);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.DragFloat($"Z##{actor.InstanceID}", ref pos.Z, 0.25f, 80, 120);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.DragFloat($"Rot##{actor.InstanceID}", ref rot, 1, -180, 180);
                _ws.Actors.Move(actor, new(pos, rot / 180 * MathF.PI));
                ImGui.SameLine();
                ImGui.Text(actor.Name);

                if (actor.Type == ActorType.Player)
                {
                    ImGui.SameLine();
                    bool isMT = boss.TargetID == actor.InstanceID;
                    if (ImGui.Checkbox($"MT##{actor.InstanceID}", ref isMT))
                        _ws.Actors.ChangeTarget(boss, isMT ? actor.InstanceID : 0);

                    ImGui.SameLine();
                    bool bloodrakeTether = actor.Tether.ID == (uint)TetherID.Bloodrake;
                    if (ImGui.Checkbox($"BloodrakeTether##{actor.InstanceID}", ref bloodrakeTether))
                        _ws.Actors.UpdateTether(actor, new() { ID = bloodrakeTether ? (uint)TetherID.Bloodrake : 0, Target = boss.InstanceID });

                    ImGui.SameLine();
                    bool chlamysTether = actor.Tether.ID == (uint)TetherID.Chlamys;
                    if (ImGui.Checkbox($"ChlamysTether##{actor.InstanceID}", ref chlamysTether))
                        _ws.Actors.UpdateTether(actor, new() { ID = chlamysTether ? (uint)TetherID.Chlamys : 0, Target = boss.InstanceID });

                    ImGui.SameLine();
                    bool roleCall = actor.Statuses[0].ID == (uint)SID.RoleCall;
                    if (ImGui.Checkbox($"RoleCall##{actor.InstanceID}", ref roleCall))
                        _ws.Actors.UpdateStatus(actor, 0, new() { ID = roleCall ? (uint)SID.RoleCall : 0 });
                }
                else if (actor.OID == (uint)OID.Boss1)
                {
                    if (actor.CastInfo != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Finish cast##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, null);
                    }
                    else
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Generic##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = new(ActionType.Spell, 1) });
                    }
                }
                else if (actor.OID == (uint)OID.Helper)
                {
                    if (actor.CastInfo != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Finish cast##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, null);
                    }
                    else
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"PinaxA##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.PinaxAcid) });
                        ImGui.SameLine();
                        if (ImGui.Button($"PinaxF##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.PinaxLava) });
                        ImGui.SameLine();
                        if (ImGui.Button($"PinaxW##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.PinaxWell) });
                        ImGui.SameLine();
                        if (ImGui.Button($"PinaxL##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.PinaxLevinstrike) });
                        ImGui.SameLine();
                        if (ImGui.Button($"BeloneCoilDPS##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.BeloneCoilsDPS) });
                        ImGui.SameLine();
                        if (ImGui.Button($"BeloneCoilTH##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.BeloneCoilsTH) });
                    }
                }
            }
        }
    }
}
