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
            _ws.AddActor(1, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Tank, new(105, 0, 100), 0, 1, true);
            _ws.AddActor(2, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Tank, new(100, 0, 105), 0, 1, true);
            _ws.AddActor(3, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Healer, new(100, 0, 95), 0, 1, true);
            _ws.AddActor(4, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Healer, new(95, 0, 100), 0, 1, true);
            _ws.AddActor(5, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Ranged, new(110, 0, 110), 0, 1, true);
            _ws.AddActor(6, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Ranged, new(90, 0, 110), 0, 1, true);
            _ws.AddActor(7, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Melee, new(110, 0, 90), 0, 1, true);
            _ws.AddActor(8, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Melee, new(90, 0, 90), 0, 1, true);
            _ws.AddActor(9, (uint)P4S.OID.Boss, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), 0, 1, true);
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

            var boss = _ws.FindActor(9)!;
            if (ImGui.Button(!_ws.PlayerInCombat ? "Pull" : "Wipe"))
            {
                _ws.UpdateCastInfo(boss, null);
                _ws.PlayerInCombat = !_ws.PlayerInCombat;
            }

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
                }
                else if (actor.OID == (uint)P4S.OID.Boss)
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
                        if (ImGui.Button("Generic"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionType = WorldState.ActionType.Spell, ActionID = 1 });
                    }
                }
            }
        }
    }
}
