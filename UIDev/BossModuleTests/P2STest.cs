using BossMod;
using ImGuiNET;
using System;

namespace UIDev
{
    class P2STest : ITest
    {
        private float _azimuth;
        private WorldState _ws;
        private P2S _o;

        public P2STest()
        {
            _ws = new();
            _ws.AddActor(1, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Tank, new(100, 0, 90), 0, 1, true);
            _ws.AddActor(2, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Tank, new(100, 0, 110), 0, 1, true);
            _ws.AddActor(3, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Healer, new(90, 0, 90), 0, 1, true);
            _ws.AddActor(4, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Healer, new(92, 0, 90), 0, 1, true);
            _ws.AddActor(5, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Ranged, new(94, 0, 90), 0, 1, true);
            _ws.AddActor(6, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Ranged, new(90, 0, 92), 0, 1, true);
            _ws.AddActor(7, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Melee, new(92, 0, 92), 0, 1, true);
            _ws.AddActor(8, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Melee, new(94, 0, 92), 0, 1, true);
            _ws.AddActor(9, (uint)P2S.OID.Boss, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), -MathF.PI / 2, 1, true);
            _ws.AddActor(10, (uint)P2S.OID.CataractHead, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), MathF.PI, 1, true);
            _ws.AddActor(11, (uint)P2S.OID.DissociatedHead, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(90, 0, 75), 0, 1, true);
            _ws.PlayerActorID = 1;
            _o = new P2S(_ws);
        }

        public void Dispose()
        {
            _o.Dispose();
        }

        public void Draw()
        {
            _o.Update();

            _o.Draw(_azimuth / 180 * MathF.PI);

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
                ImGui.DragFloat($"X##{actor.InstanceID}", ref pos.X, 1, 80, 120);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.DragFloat($"Z##{actor.InstanceID}", ref pos.Z, 1, 80, 120);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.DragFloat($"Rot##{actor.InstanceID}", ref rot, 1, -180, 180);
                _ws.MoveActor(actor, pos, rot / 180 * MathF.PI);

                if (actor.Type == WorldState.ActorType.Player)
                {
                    ImGui.SameLine();
                    bool isMT = boss.TargetID == actor.InstanceID;
                    if (ImGui.Checkbox($"MT##{actor.InstanceID}", ref isMT))
                    {
                        _ws.ChangeActorTarget(boss, isMT ? actor.InstanceID : 0);
                    }

                    //ImGui.SameLine();
                    //bool heatTether = actor.Tether.ID == (uint)P3S.TetherID.HeatOfCondemnation;
                    //if (ImGui.Checkbox($"Heat tether##{actor.InstanceID}", ref heatTether))
                    //{
                    //    _ws.UpdateTether(actor, heatTether ? new WorldState.TetherInfo { ID = (uint)P3S.TetherID.HeatOfCondemnation, Target = 9 } : new());
                    //}
                }
                else if (actor.OID == (uint)P2S.OID.Boss)
                {
                    if (actor.CastInfo != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Finish cast##{actor.InstanceID}"))
                        {
                            _ws.UpdateCastInfo(actor, null);
                            _ws.UpdateCastInfo(_ws.FindActor(10)!, null);
                        }
                    }
                    else
                    {
                        ImGui.SameLine();
                        if (ImGui.Button("Generic"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = 1 });
                        ImGui.SameLine();
                        if (ImGui.Button("SpokenCata"))
                        {
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P2S.AID.SpokenCataract });
                            _ws.UpdateCastInfo(_ws.FindActor(10)!, new WorldState.CastInfo { ActionID = (uint)P2S.AID.SpokenCataractSecondary });
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("WingedCata"))
                        {
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P2S.AID.WingedCataract });
                            _ws.UpdateCastInfo(_ws.FindActor(10)!, new WorldState.CastInfo { ActionID = (uint)P2S.AID.WingedCataractSecondary });
                        }
                    }
                }
                else if (actor.OID == (uint)P2S.OID.DissociatedHead)
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
                        if (ImGui.Button($"Start cast##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P2S.AID.DissociationAOE });
                    }
                }
            }

            if (ImGui.Button("Deluge stop"))
                _ws.DispatchEventEnvControl(0x800375A2, 1, 0x00080004);
            ImGui.SameLine();
            if (ImGui.Button("Deluge NW"))
                _ws.DispatchEventEnvControl(0x800375A2, 1, 0x00200010);
            ImGui.SameLine();
            if (ImGui.Button("Deluge NE"))
                _ws.DispatchEventEnvControl(0x800375A2, 2, 0x00200010);
            ImGui.SameLine();
            if (ImGui.Button("Deluge SW"))
                _ws.DispatchEventEnvControl(0x800375A2, 3, 0x00200010);
            ImGui.SameLine();
            if (ImGui.Button("Deluge SE"))
                _ws.DispatchEventEnvControl(0x800375A2, 4, 0x00200010);
        }
    }
}
