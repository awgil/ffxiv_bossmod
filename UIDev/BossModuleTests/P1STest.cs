using ImGuiNET;
using BossMod;
using System;

namespace UIDev
{
    class P1STest : ITest
    {
        private float _azimuth;
        private WorldState _ws;
        private P1S _o;

        public P1STest()
        {
            _ws = new();
            _ws.AddActor(1, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Tank, new(105, 0, 100), 0, 1, true);
            _ws.AddActor(2, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Tank, new(100, 0, 105), 0, 1, true);
            _ws.AddActor(3, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Healer, new(100, 0,  95), 0, 1, true);
            _ws.AddActor(4, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Healer, new( 95, 0, 100), 0, 1, true);
            _ws.AddActor(5, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Ranged, new(110, 0, 110), 0, 1, true);
            _ws.AddActor(6, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Ranged, new( 90, 0, 110), 0, 1, true);
            _ws.AddActor(7, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Melee, new(110, 0,  90), 0, 1, true);
            _ws.AddActor(8, 0, WorldState.ActorType.Player, 0, WorldState.ActorRole.Melee, new( 90, 0,  90), 0, 1, true);
            _ws.AddActor(9, (uint)P1S.OID.Boss, WorldState.ActorType.Enemy, 0, WorldState.ActorRole.None, new(100, 0, 100), 0, 1, true);
            _ws.PlayerActorID = 1;
            _o = new P1S(_ws);
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

            if (ImGui.Button(!_ws.PlayerInCombat ? "Pull" : "Wipe"))
            {
                _ws.UpdateCastInfo(_ws.FindActor(1)!, null);
                _ws.PlayerInCombat = !_ws.PlayerInCombat;
            }

            var boss = _ws.FindActor(9)!;
            int cnt = 0;
            foreach (var e in Enum.GetValues<P1S.AID>())
            {
                if (ImGui.Button(e.ToString()))
                    _ws.UpdateCastInfo(boss, boss.CastInfo == null ? new WorldState.CastInfo { ActionID = (uint)e, TargetID = boss.TargetID } : null);
                if (++cnt % 5 != 0)
                    ImGui.SameLine();
            }
            ImGui.NewLine();

            foreach (var actor in _ws.Actors)
            {
                var pos = actor.Value.Position;
                var rot = actor.Value.Rotation / MathF.PI * 180;
                ImGui.SetNextItemWidth(100);
                ImGui.DragFloat($"X##{actor.Value.InstanceID}", ref pos.X, 1, 80, 120);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.DragFloat($"Z##{actor.Value.InstanceID}", ref pos.Z, 1, 80, 120);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.DragFloat($"Rot##{actor.Value.InstanceID}", ref rot, 1, -180, 180);
                _ws.MoveActor(actor.Value, pos, rot / 180 * MathF.PI);

                if (actor.Value.Type == WorldState.ActorType.Player)
                {
                    ImGui.SameLine();
                    bool isMT = boss.TargetID == actor.Value.InstanceID;
                    if (ImGui.Checkbox($"MT##{actor.Value.InstanceID}", ref isMT))
                    {
                        _ws.ChangeActorTarget(boss, isMT ? actor.Value.InstanceID : 0);
                    }

                    ImGui.SameLine();
                    bool blue = actor.Value.Statuses[0].ID != 0;
                    if (ImGui.Checkbox($"Blue shackle##{actor.Value.InstanceID}", ref blue))
                    {
                        if (blue)
                        {
                            SetStatus(actor.Value, 0, (uint)P1S.SID.ShacklesOfCompanionship0, 0);
                        }
                        else
                        {
                            SetStatus(actor.Value, 0, (uint)P1S.SID.InescapableCompanionship, 0);
                            SetStatus(actor.Value, 0, 0, 0);
                        }
                    }

                    ImGui.SameLine();
                    bool redShackle = actor.Value.Statuses[1].ID != 0;
                    if (ImGui.Checkbox($"Red shackle##{actor.Value.InstanceID}", ref redShackle))
                    {
                        if (redShackle)
                        {
                            SetStatus(actor.Value, 1, (uint)P1S.SID.ShacklesOfLoneliness0, 0);
                        }
                        else
                        {
                            SetStatus(actor.Value, 1, (uint)P1S.SID.InescapableLoneliness, 0);
                            SetStatus(actor.Value, 1, 0, 0);
                        }
                    }

                    ImGui.SameLine();
                    bool sot = actor.Value.Statuses[2].ID != 0;
                    if (ImGui.Checkbox($"Shackle of time##{actor.Value.InstanceID}", ref sot))
                    {
                        SetStatus(actor.Value, 2, sot ? (uint)P1S.SID.ShacklesOfTime : 0, 0);
                    }
                }
                else
                {
                    ImGui.SameLine();
                    bool red = actor.Value.Statuses[0].ID != 0 && actor.Value.Statuses[0].StackCount == 0x4C;
                    if (ImGui.Checkbox($"Red aether##{actor.Value.InstanceID}", ref red))
                    {
                        SetStatus(actor.Value, 0, red ? (uint)P1S.SID.AetherExplosion : 0, 0x4C);
                    }

                    ImGui.SameLine();
                    bool blue = actor.Value.Statuses[0].ID != 0 && actor.Value.Statuses[0].StackCount == 0x4D;
                    if (ImGui.Checkbox($"Blue aether##{actor.Value.InstanceID}", ref blue))
                    {
                        SetStatus(actor.Value, 0, blue ? (uint)P1S.SID.AetherExplosion : 0, 0x4D);
                    }
                }
            }
        }

        private void SetStatus(WorldState.Actor actor, int index, uint statusID, byte param)
        {
            var newStatuses = (WorldState.Status[])actor.Statuses.Clone();
            newStatuses[index].ID = statusID;
            newStatuses[index].StackCount = param;
            _ws.UpdateStatuses(actor, newStatuses);
        }
    }
}
