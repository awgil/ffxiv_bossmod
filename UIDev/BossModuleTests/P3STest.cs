using BossMod;
using ImGuiNET;
using System;

namespace UIDev
{
    class P3STest : ITest
    {
        private float _azimuth;
        private WorldState _ws;
        private P3S _o;

        public P3STest()
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
            _ws.AddActor(9, (uint)P3S.OID.Boss, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), 0, 1, true);
            _ws.AddActor(10, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(115, 0, 100), 0, 1, true);
            _ws.AddActor(11, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(85, 0, 100), MathF.PI / 3, 1, true);
            _ws.AddActor(12, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(110, 0, 110), 2 * MathF.PI / 3, 1, true);
            _ws.AddActor(13, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(90, 0, 90), MathF.PI, 1, true);
            _ws.AddActor(14, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 115), -MathF.PI / 3, 1, true);
            _ws.AddActor(15, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 85), -2 * MathF.PI / 3, 1, true);
            _ws.AddActor(16, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(90, 0, 110), 0, 1, true);
            _ws.AddActor(17, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(110, 0, 90), 0, 1, true);
            _ws.AddActor(18, (uint)P3S.OID.Helper, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), 0, 1, true);
            _ws.AddActor(19, (uint)P3S.OID.DarkblazeTwister, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(114, 0, 108), -2 * MathF.PI / 3, 1, false);
            _ws.AddActor(20, (uint)P3S.OID.DarkblazeTwister, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 84), 0, 1, false);
            _ws.AddActor(21, (uint)P3S.OID.DarkblazeTwister, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(86, 0, 108), 2 * MathF.PI / 3, 1, false);
            _ws.PlayerActorID = 1;
            _o = new P3S(_ws);
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

                    ImGui.SameLine();
                    bool heatTether = actor.Tether.ID == (uint)P3S.TetherID.HeatOfCondemnation;
                    if (ImGui.Checkbox($"Heat tether##{actor.InstanceID}", ref heatTether))
                    {
                        _ws.UpdateTether(actor, heatTether ? new WorldState.TetherInfo { ID = (uint)P3S.TetherID.HeatOfCondemnation, Target = 9 } : new());
                    }
                }
                else if (actor.OID == (uint)P3S.OID.Boss)
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
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = 1 });
                        ImGui.SameLine();
                        if (ImGui.Button("WingL"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.LeftCinderwing });
                        ImGui.SameLine();
                        if (ImGui.Button("WingR"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.RightCinderwing });
                        ImGui.SameLine();
                        if (ImGui.Button("AshSpread"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.ExperimentalAshplumeSpread });
                        ImGui.SameLine();
                        if (ImGui.Button("AshStack"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.ExperimentalAshplumeStack });
                        ImGui.SameLine();
                        if (ImGui.Button("TrailC"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.TrailOfCondemnationCenter });
                        ImGui.SameLine();
                        if (ImGui.Button("TrailS"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.TrailOfCondemnationSides });
                    }
                }
                else if (actor.OID == (uint)P3S.OID.Helper)
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
                        if (ImGui.Button($"PlumeMulti##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.ExperimentalFireplumeMultiAOE });
                        ImGui.SameLine();
                        if (ImGui.Button($"PlumeSingle##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.ExperimentalFireplumeSingleAOE });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone1##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.FlamesOfAsphodelosAOE1 });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone2##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.FlamesOfAsphodelosAOE2 });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone3##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { ActionID = (uint)P3S.AID.FlamesOfAsphodelosAOE3 });
                    }
                }
            }
        }
    }
}
