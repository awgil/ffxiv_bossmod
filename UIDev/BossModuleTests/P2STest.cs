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
            _ws.AddActor(1, 0, "T1", WorldState.ActorType.Player, Class.WAR, new(100, 0, 90, 0));
            _ws.AddActor(2, 0, "T2", WorldState.ActorType.Player, Class.PLD, new(100, 0, 110, 0));
            _ws.AddActor(3, 0, "H1", WorldState.ActorType.Player, Class.WHM, new(90, 0, 90, 0));
            _ws.AddActor(4, 0, "H2", WorldState.ActorType.Player, Class.SGE, new(92, 0, 90, 0));
            _ws.AddActor(5, 0, "R1", WorldState.ActorType.Player, Class.BLM, new(94, 0, 90, 0));
            _ws.AddActor(6, 0, "R2", WorldState.ActorType.Player, Class.MCH, new(90, 0, 92, 0));
            _ws.AddActor(7, 0, "M1", WorldState.ActorType.Player, Class.MNK, new(92, 0, 92, 0));
            _ws.AddActor(8, 0, "M2", WorldState.ActorType.Player, Class.RPR, new(94, 0, 92, 0));
            _ws.AddActor(9, (uint)OID.Boss, "Boss", WorldState.ActorType.Enemy, Class.None, new(100, 0, 100, -MathF.PI / 2));
            _ws.AddActor(10, (uint)OID.CataractHead, "CHead", WorldState.ActorType.Enemy, Class.None, new(100, 0, 100, MathF.PI));
            _ws.AddActor(11, (uint)OID.DissociatedHead, "DHead", WorldState.ActorType.Enemy, Class.None, new(90, 0, 75, 0));
            _ws.PlayerActorID = 1;
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

            _o.Draw(_azimuth / 180 * MathF.PI, null);

            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);

            var boss = _ws.FindActor(9)!;
            if (ImGui.Button(!_ws.PlayerInCombat ? "Pull" : "Wipe"))
            {
                _ws.UpdateCastInfo(boss, null);
                _ws.PlayerInCombat = !_ws.PlayerInCombat;
            }

            ImGui.SameLine();
            if (ImGui.Button(_paused ? "Resume" : "Pause"))
                _paused = !_paused;

            ImGui.SameLine();
            ImGui.Text($"Downtime in: {_o.StateMachine.EstimateTimeToNextDowntime():f2}, Positioning in: {_o.StateMachine.EstimateTimeToNextPositioning():f2}");

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
                _ws.MoveActor(actor, new(pos, rot / 180 * MathF.PI));

                if (actor.Type == WorldState.ActorType.Player)
                {
                    ImGui.SameLine();
                    bool isMT = boss.TargetID == actor.InstanceID;
                    if (ImGui.Checkbox($"MT##{actor.InstanceID}", ref isMT))
                    {
                        _ws.ChangeActorTarget(boss, isMT ? actor.InstanceID : 0);
                    }
                }
                else if (actor.OID == (uint)OID.Boss)
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
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = new(ActionType.Spell, 1) });
                        ImGui.SameLine();
                        if (ImGui.Button("SpokenCata"))
                        {
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(AID.SpokenCataract) });
                            _ws.UpdateCastInfo(_ws.FindActor(10)!, new WorldState.CastInfo { Action = ActionID.MakeSpell(AID.SpokenCataractSecondary) });
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("WingedCata"))
                        {
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(AID.WingedCataract) });
                            _ws.UpdateCastInfo(_ws.FindActor(10)!, new WorldState.CastInfo { Action = ActionID.MakeSpell(AID.WingedCataractSecondary) });
                        }
                    }
                }
                else if (actor.OID == (uint)OID.DissociatedHead)
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
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(AID.DissociationAOE) });
                    }
                }
            }

            if (ImGui.Button("Deluge stop"))
                _ws.DispatchEventEnvControl((0x800375A2, 1, 0x00080004));
            ImGui.SameLine();
            if (ImGui.Button("Deluge NW"))
                _ws.DispatchEventEnvControl((0x800375A2, 1, 0x00200010));
            ImGui.SameLine();
            if (ImGui.Button("Deluge NE"))
                _ws.DispatchEventEnvControl((0x800375A2, 2, 0x00200010));
            ImGui.SameLine();
            if (ImGui.Button("Deluge SW"))
                _ws.DispatchEventEnvControl((0x800375A2, 3, 0x00200010));
            ImGui.SameLine();
            if (ImGui.Button("Deluge SE"))
                _ws.DispatchEventEnvControl((0x800375A2, 4, 0x00200010));
        }
    }
}
