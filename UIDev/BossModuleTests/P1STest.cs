using ImGuiNET;
using BossMod;
using System;
using BossMod.P1S;

namespace UIDev
{
    class P1STest : ITest
    {
        private float _azimuth;
        private WorldState _ws;
        private P1S _o;
        private DateTime _prevFrame;
        private bool _paused;

        public P1STest()
        {
            _ws = new();
            _ws.CurrentTime = _prevFrame = DateTime.Now;
            _ws.Actors.Add(1, 0, "T1", ActorType.Player, Class.WAR, new(105, 0, 100, 0));
            _ws.Actors.Add(2, 0, "T2", ActorType.Player, Class.PLD, new(100, 0, 105, 0));
            _ws.Actors.Add(3, 0, "H1", ActorType.Player, Class.WHM, new(100, 0,  95, 0));
            _ws.Actors.Add(4, 0, "H2", ActorType.Player, Class.SGE, new( 95, 0, 100, 0));
            _ws.Actors.Add(5, 0, "R1", ActorType.Player, Class.BLM, new(110, 0, 110, 0));
            _ws.Actors.Add(6, 0, "R2", ActorType.Player, Class.MCH, new( 90, 0, 110, 0));
            _ws.Actors.Add(7, 0, "M1", ActorType.Player, Class.MNK, new(110, 0,  90, 0));
            _ws.Actors.Add(8, 0, "M2", ActorType.Player, Class.RPR, new( 90, 0,  90, 0));
            _ws.Actors.Add(9, (uint)OID.Boss, "Boss", ActorType.Enemy, Class.None, new(100, 0, 100, 0));
            for (int i = 1; i <= 8; ++i)
                _ws.Party.Add((ulong)i, _ws.Actors.Find((uint)i), i == 1);
            _o = new P1S(_ws);
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

            _o.Draw(_azimuth / 180 * MathF.PI, null);

            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);

            var boss = _ws.Actors.Find(9)!;
            if (ImGui.Button(!_ws.PlayerInCombat ? "Pull" : "Wipe"))
            {
                _ws.Actors.UpdateCastInfo(boss, null);
                _ws.PlayerInCombat = !_ws.PlayerInCombat;
            }

            ImGui.SameLine();
            if (ImGui.Button(_paused ? "Resume" : "Pause"))
                _paused = !_paused;

            ImGui.SameLine();
            ImGui.Text($"Downtime in: {_o.StateMachine.EstimateTimeToNextDowntime():f2}, Positioning in: {_o.StateMachine.EstimateTimeToNextPositioning():f2}");

            int cnt = 0;
            foreach (var e in Enum.GetValues<AID>())
            {
                if (ImGui.Button(e.ToString()))
                    _ws.Actors.UpdateCastInfo(boss, boss.CastInfo == null ? new ActorCastInfo { Action = new(ActionType.Spell, (uint)e), TargetID = boss.TargetID } : null);
                if (++cnt % 5 != 0)
                    ImGui.SameLine();
            }
            ImGui.NewLine();

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

                if (actor.Type == ActorType.Player)
                {
                    ImGui.SameLine();
                    bool isMT = boss.TargetID == actor.InstanceID;
                    if (ImGui.Checkbox($"MT##{actor.InstanceID}", ref isMT))
                    {
                        _ws.Actors.ChangeTarget(boss, isMT ? actor.InstanceID : 0);
                    }

                    ImGui.SameLine();
                    bool blue = actor.Statuses[0].ID != 0;
                    if (ImGui.Checkbox($"Blue shackle##{actor.InstanceID}", ref blue))
                    {
                        if (blue)
                        {
                            _ws.Actors.UpdateStatus(actor, 0, new() { ID = (uint)SID.ShacklesOfCompanionship0 });
                        }
                        else
                        {
                            _ws.Actors.UpdateStatus(actor, 0, new() { ID = (uint)SID.InescapableCompanionship });
                            _ws.Actors.UpdateStatus(actor, 0, new());
                        }
                    }

                    ImGui.SameLine();
                    bool redShackle = actor.Statuses[1].ID != 0;
                    if (ImGui.Checkbox($"Red shackle##{actor.InstanceID}", ref redShackle))
                    {
                        if (redShackle)
                        {
                            _ws.Actors.UpdateStatus(actor, 1, new() { ID = (uint)SID.ShacklesOfLoneliness0 });
                        }
                        else
                        {
                            _ws.Actors.UpdateStatus(actor, 1, new() { ID = (uint)SID.InescapableLoneliness });
                            _ws.Actors.UpdateStatus(actor, 1, new());
                        }
                    }

                    ImGui.SameLine();
                    bool sot = actor.Statuses[2].ID != 0;
                    if (ImGui.Checkbox($"Shackle of time##{actor.InstanceID}", ref sot))
                    {
                        _ws.Actors.UpdateStatus(actor, 2, new() { ID = sot ? (uint)SID.ShacklesOfTime : 0 });
                    }
                }
                else
                {
                    ImGui.SameLine();
                    bool red = actor.Statuses[0].ID != 0 && actor.Statuses[0].Extra == 0x4C;
                    if (ImGui.Checkbox($"Red aether##{actor.InstanceID}", ref red))
                    {
                        _ws.Actors.UpdateStatus(actor, 0, new() { ID = red ? (uint)SID.AetherExplosion : 0, Extra = 0x4C });
                    }

                    ImGui.SameLine();
                    bool blue = actor.Statuses[0].ID != 0 && actor.Statuses[0].Extra == 0x4D;
                    if (ImGui.Checkbox($"Blue aether##{actor.InstanceID}", ref blue))
                    {
                        _ws.Actors.UpdateStatus(actor, 0, new() { ID = blue ? (uint)SID.AetherExplosion : 0, Extra = 0x4D });
                    }
                }
            }
        }
    }
}
