using BossMod;
using BossMod.P2S;
using ImGuiNET;
using System;

namespace UIDev
{
    class P2STest : ITest
    {
        private float _azimuth;
        private WorldState _ws;
        private P2S _o;
        private DateTime _prevFrame;
        private bool _paused;

        public P2STest()
        {
            _ws = new();
            _ws.CurrentTime = _prevFrame = DateTime.Now;
            _ws.Actors.Add(1, 0, "T1", ActorType.Player, Class.WAR, new(100, 0, 90, 0));
            _ws.Actors.Add(2, 0, "T2", ActorType.Player, Class.PLD, new(100, 0, 110, 0));
            _ws.Actors.Add(3, 0, "H1", ActorType.Player, Class.WHM, new(90, 0, 90, 0));
            _ws.Actors.Add(4, 0, "H2", ActorType.Player, Class.SGE, new(92, 0, 90, 0));
            _ws.Actors.Add(5, 0, "R1", ActorType.Player, Class.BLM, new(94, 0, 90, 0));
            _ws.Actors.Add(6, 0, "R2", ActorType.Player, Class.MCH, new(90, 0, 92, 0));
            _ws.Actors.Add(7, 0, "M1", ActorType.Player, Class.MNK, new(92, 0, 92, 0));
            _ws.Actors.Add(8, 0, "M2", ActorType.Player, Class.RPR, new(94, 0, 92, 0));
            _ws.Actors.Add(9, (uint)OID.Boss, "Boss", ActorType.Enemy, Class.None, new(100, 0, 100, -MathF.PI / 2));
            _ws.Actors.Add(10, (uint)OID.CataractHead, "CHead", ActorType.Enemy, Class.None, new(100, 0, 100, MathF.PI));
            _ws.Actors.Add(11, (uint)OID.DissociatedHead, "DHead", ActorType.Enemy, Class.None, new(90, 0, 75, 0));
            for (int i = 1; i <= 8; ++i)
                _ws.Party.Add((ulong)i, _ws.Actors.Find((uint)i), i == 1);
            _o = new P2S(_ws);
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

            var boss = _ws.Actors.Find(9)!;
            var pc = _ws.Party.Player()!;
            if (ImGui.Button(!pc.InCombat ? "Pull" : "Wipe"))
            {
                _ws.Actors.UpdateCastInfo(boss, null);
                _ws.Actors.ChangeInCombat(pc, !pc.InCombat);
            }

            ImGui.SameLine();
            if (ImGui.Button(_paused ? "Resume" : "Pause"))
                _paused = !_paused;

            ImGui.SameLine();
            ImGui.Text($"Downtime in: {_o.StateMachine.EstimateTimeToNextDowntime():f2}, Positioning in: {_o.StateMachine.EstimateTimeToNextPositioning():f2}");

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
                }
                else if (actor.OID == (uint)OID.Boss)
                {
                    if (actor.CastInfo != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Finish cast##{actor.InstanceID}"))
                        {
                            _ws.Actors.UpdateCastInfo(actor, null);
                            _ws.Actors.UpdateCastInfo(_ws.Actors.Find(10)!, null);
                        }
                    }
                    else
                    {
                        ImGui.SameLine();
                        if (ImGui.Button("Generic"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = new(ActionType.Spell, 1) });
                        ImGui.SameLine();
                        if (ImGui.Button("SpokenCata"))
                        {
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.SpokenCataract) });
                            _ws.Actors.UpdateCastInfo(_ws.Actors.Find(10)!, new ActorCastInfo { Action = ActionID.MakeSpell(AID.SpokenCataractSecondary) });
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("WingedCata"))
                        {
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.WingedCataract) });
                            _ws.Actors.UpdateCastInfo(_ws.Actors.Find(10)!, new ActorCastInfo { Action = ActionID.MakeSpell(AID.WingedCataractSecondary) });
                        }
                    }
                }
                else if (actor.OID == (uint)OID.DissociatedHead)
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
                        if (ImGui.Button($"Start cast##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.DissociationAOE) });
                    }
                }
            }

            if (ImGui.Button("Deluge stop"))
                _ws.Events.DispatchEnvControl((0x800375A2, 1, 0x00080004));
            ImGui.SameLine();
            if (ImGui.Button("Deluge NW"))
                _ws.Events.DispatchEnvControl((0x800375A2, 1, 0x00200010));
            ImGui.SameLine();
            if (ImGui.Button("Deluge NE"))
                _ws.Events.DispatchEnvControl((0x800375A2, 2, 0x00200010));
            ImGui.SameLine();
            if (ImGui.Button("Deluge SW"))
                _ws.Events.DispatchEnvControl((0x800375A2, 3, 0x00200010));
            ImGui.SameLine();
            if (ImGui.Button("Deluge SE"))
                _ws.Events.DispatchEnvControl((0x800375A2, 4, 0x00200010));
        }
    }
}
