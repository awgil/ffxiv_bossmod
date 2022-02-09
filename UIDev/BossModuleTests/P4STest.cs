using BossMod;
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

        public P4STest()
        {
            _ws = new();
            _ws.AddActor(1, 0, "T1", WorldState.ActorType.Player, 0, WorldState.ActorRole.Tank, new(105, 0, 100), 0, 1, true);
            _ws.AddActor(2, 0, "T2", WorldState.ActorType.Player, 0, WorldState.ActorRole.Tank, new(100, 0, 105), 0, 1, true);
            _ws.AddActor(3, 0, "H1", WorldState.ActorType.Player, 0, WorldState.ActorRole.Healer, new(100, 0, 95), 0, 1, true);
            _ws.AddActor(4, 0, "H2", WorldState.ActorType.Player, 0, WorldState.ActorRole.Healer, new(95, 0, 100), 0, 1, true);
            _ws.AddActor(5, 0, "R1", WorldState.ActorType.Player, 0, WorldState.ActorRole.Ranged, new(110, 0, 110), 0, 1, true);
            _ws.AddActor(6, 0, "R2", WorldState.ActorType.Player, 0, WorldState.ActorRole.Ranged, new(90, 0, 110), 0, 1, true);
            _ws.AddActor(7, 0, "M1", WorldState.ActorType.Player, 0, WorldState.ActorRole.Melee, new(110, 0, 90), 0, 1, true);
            _ws.AddActor(8, 0, "M2", WorldState.ActorType.Player, 0, WorldState.ActorRole.Melee, new(90, 0, 90), 0, 1, true);
            _ws.AddActor(9, (uint)P4S.OID.Boss1, "Boss1", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), 0, 1, true);
            _ws.AddActor(10, (uint)P4S.OID.Boss2, "Boss2", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), 0, 1, true);
            _ws.AddActor(11, (uint)P4S.OID.Helper, "Helper", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(90, 0, 90), 0, 1, true);
            _ws.AddActor(12, (uint)P4S.OID.Helper, "Helper", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(110, 0, 90), 0, 1, true);
            _ws.AddActor(13, (uint)P4S.OID.Helper, "Helper", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(90, 0, 110), 0, 1, true);
            _ws.AddActor(14, (uint)P4S.OID.Helper, "Helper", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(110, 0, 110), 0, 1, true);
            _ws.AddActor(15, (uint)P4S.OID.Helper, "Helper", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(94.34f, 0, 94.34f), 0, 1, true);
            _ws.AddActor(16, (uint)P4S.OID.Helper, "Helper", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(94.34f, 0, 105.66f), 0, 1, true);
            _ws.AddActor(17, (uint)P4S.OID.Helper, "Helper", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(105.66f, 0, 94.34f), 0, 1, true);
            _ws.AddActor(18, (uint)P4S.OID.Helper, "Helper", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(105.66f, 0, 105.66f), 0, 1, true);
            _ws.PlayerActorID = 1;
            _o = new P4S(_ws);
        }

        public void Dispose()
        {
            _o.Dispose();
        }

        public void Draw()
        {
            _o.Update();

            _o.Draw(_azimuth / 180 * MathF.PI, null);

            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);

            var boss1 = _ws.FindActor(9);
            var boss2 = _ws.FindActor(10)!;
            if (ImGui.Button(!_ws.PlayerInCombat ? "Pull" : "Wipe"))
            {
                if (boss1 != null)
                    _ws.UpdateCastInfo(boss1, null);
                _ws.UpdateCastInfo(boss2, null);
                _ws.PlayerInCombat = !_ws.PlayerInCombat;
            }

            ImGui.SameLine();
            bool phase1 = boss1 != null;
            if (ImGui.Checkbox("Phase 1", ref phase1))
            {
                if (phase1)
                {
                    boss1 = _ws.AddActor(9, (uint)P4S.OID.Boss1, "Boss1", WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), 0, 1, true);
                }
                else
                {
                    _ws.RemoveActor(9);
                    boss1 = null;
                }
            }

            var boss = boss1 ?? boss2;

            foreach (var actor in _ws.Actors.Values)
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
                _ws.MoveActor(actor, pos, rot / 180 * MathF.PI);

                if (actor.Type == WorldState.ActorType.Player)
                {
                    ImGui.SameLine();
                    bool isMT = boss.TargetID == actor.InstanceID;
                    if (ImGui.Checkbox($"MT##{actor.InstanceID}", ref isMT))
                        _ws.ChangeActorTarget(boss, isMT ? actor.InstanceID : 0);

                    ImGui.SameLine();
                    bool bloodrakeTether = actor.Tether.ID == (uint)P4S.TetherID.Bloodrake;
                    if (ImGui.Checkbox($"BloodrakeTether##{actor.InstanceID}", ref bloodrakeTether))
                        _ws.UpdateTether(actor, new() { ID = bloodrakeTether ? (uint)P4S.TetherID.Bloodrake : 0, Target = boss.InstanceID });

                    ImGui.SameLine();
                    bool chlamysTether = actor.Tether.ID == (uint)P4S.TetherID.Chlamys;
                    if (ImGui.Checkbox($"ChlamysTether##{actor.InstanceID}", ref chlamysTether))
                        _ws.UpdateTether(actor, new() { ID = chlamysTether ? (uint)P4S.TetherID.Chlamys : 0, Target = boss.InstanceID });

                    ImGui.SameLine();
                    bool roleCall = actor.Statuses[0].ID == (uint)P4S.SID.RoleCall;
                    if (ImGui.Checkbox($"RoleCall##{actor.InstanceID}", ref roleCall))
                        SetStatus(actor, 0, roleCall ? (uint)P4S.SID.RoleCall : 0);
                }
                else if (actor.OID == (uint)P4S.OID.Boss1)
                {
                    if (actor.CastInfo != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Finish cast##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, null);
                    }
                    else
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Generic##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionType = WorldState.ActionType.Spell, ActionID = 1 });
                    }
                }
                else if (actor.OID == (uint)P4S.OID.Helper)
                {
                    if (actor.CastInfo != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Finish cast##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, null);
                    }
                    else
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"PinaxA##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionType = WorldState.ActionType.Spell, ActionID = (uint)P4S.AID.PinaxAcid });
                        ImGui.SameLine();
                        if (ImGui.Button($"PinaxF##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionType = WorldState.ActionType.Spell, ActionID = (uint)P4S.AID.PinaxLava });
                        ImGui.SameLine();
                        if (ImGui.Button($"PinaxW##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionType = WorldState.ActionType.Spell, ActionID = (uint)P4S.AID.PinaxWell });
                        ImGui.SameLine();
                        if (ImGui.Button($"PinaxL##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionType = WorldState.ActionType.Spell, ActionID = (uint)P4S.AID.PinaxLevinstrike });
                        ImGui.SameLine();
                        if (ImGui.Button($"BeloneCoilDPS##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionType = WorldState.ActionType.Spell, ActionID = (uint)P4S.AID.BeloneCoilsDPS });
                        ImGui.SameLine();
                        if (ImGui.Button($"BeloneCoilTH##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionType = WorldState.ActionType.Spell, ActionID = (uint)P4S.AID.BeloneCoilsTH });
                    }
                }
            }
        }

        private void SetStatus(WorldState.Actor actor, int index, uint statusID)
        {
            var newStatuses = (WorldState.Status[])actor.Statuses.Clone();
            newStatuses[index].ID = statusID;
            _ws.UpdateStatuses(actor, newStatuses);
        }
    }
}
