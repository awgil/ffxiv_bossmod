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
            _ws.AddActor(1, 0, WorldState.ActorType.Player, new(105, 0, 100), 0, 1, 0);
            _ws.AddActor(2, 0, WorldState.ActorType.Player, new(100, 0, 105), 0, 1, 0);
            _ws.AddActor(3, 0, WorldState.ActorType.Player, new(100, 0,  95), 0, 1, 0);
            _ws.AddActor(4, 0, WorldState.ActorType.Player, new( 95, 0, 100), 0, 1, 0);
            _ws.AddActor(5, 0, WorldState.ActorType.Player, new(110, 0, 110), 0, 1, 0);
            _ws.AddActor(6, 0, WorldState.ActorType.Player, new( 90, 0, 110), 0, 1, 0);
            _ws.AddActor(7, 0, WorldState.ActorType.Player, new(110, 0,  90), 0, 1, 0);
            _ws.AddActor(8, 0, WorldState.ActorType.Player, new( 90, 0,  90), 0, 1, 0);
            _ws.AddActor(9, (uint)P1S.OID.Boss, WorldState.ActorType.Enemy, new(100, 0, 100), 0, 1, 0);
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

            _o.Draw(_azimuth / 180 * MathF.PI);

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
                    _ws.UpdateCastInfo(boss, boss.CastInfo == null ? new WorldState.CastInfo { ActionID = (uint)e } : null);
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
                    bool blue = actor.Value.Statuses[0].ID != 0;
                    if (ImGui.Checkbox($"Blue shackle##{actor.Value.InstanceID}", ref blue))
                    {
                        var newStatuses = (WorldState.Status[])actor.Value.Statuses.Clone();
                        if (blue)
                        {
                            newStatuses[0].ID = (uint)P1S.SID.ShacklesOfCompanionship;
                            _ws.UpdateStatuses(actor.Value, newStatuses);
                        }
                        else
                        {
                            newStatuses[0].ID = (uint)P1S.SID.InescapableCompanionship;
                            _ws.UpdateStatuses(actor.Value, newStatuses);
                            newStatuses[0].ID = 0;
                            _ws.UpdateStatuses(actor.Value, newStatuses);
                        }
                    }

                    ImGui.SameLine();
                    bool redShackle = actor.Value.Statuses[1].ID != 0;
                    if (ImGui.Checkbox($"Red shackle##{actor.Value.InstanceID}", ref redShackle))
                    {
                        var newStatuses = (WorldState.Status[])actor.Value.Statuses.Clone();
                        if (redShackle)
                        {
                            newStatuses[1].ID = (uint)P1S.SID.ShacklesOfLoneliness;
                            _ws.UpdateStatuses(actor.Value, newStatuses);
                        }
                        else
                        {
                            newStatuses[1].ID = (uint)P1S.SID.InescapableLoneliness;
                            _ws.UpdateStatuses(actor.Value, newStatuses);
                            newStatuses[1].ID = 0;
                            _ws.UpdateStatuses(actor.Value, newStatuses);
                        }
                    }
                }
            }
        }
    }
}
