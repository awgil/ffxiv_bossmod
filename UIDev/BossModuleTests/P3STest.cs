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
        private DateTime _prevFrame;
        private bool _paused;
        private uint _numAssignedIcons = 0;

        public P3STest()
        {
            _ws = new();
            _ws.CurrentTime = _prevFrame = DateTime.Now;
            _ws.AddActor(1, 0, "T1", WorldState.ActorType.Player, Class.WAR, new(100, 0,  90, 0), 1, true);
            _ws.AddActor(2, 0, "T2", WorldState.ActorType.Player, Class.PLD, new(100, 0, 110, 0), 1, true);
            _ws.AddActor(3, 0, "H1", WorldState.ActorType.Player, Class.WHM, new( 90, 0,  90, 0), 1, true);
            _ws.AddActor(4, 0, "H2", WorldState.ActorType.Player, Class.SGE, new( 92, 0,  90, 0), 1, true);
            _ws.AddActor(5, 0, "R1", WorldState.ActorType.Player, Class.BLM, new( 94, 0,  90, 0), 1, true);
            _ws.AddActor(6, 0, "R2", WorldState.ActorType.Player, Class.MCH, new( 90, 0,  92, 0), 1, true);
            _ws.AddActor(7, 0, "M1", WorldState.ActorType.Player, Class.MNK, new( 92, 0,  92, 0), 1, true);
            _ws.AddActor(8, 0, "M2", WorldState.ActorType.Player, Class.RPR, new( 94, 0,  92, 0), 1, true);
            _ws.AddActor(9, (uint)P3S.OID.Boss, "Boss", WorldState.ActorType.Enemy, Class.None, new(100, 0, 100, 0), 1, true);
            _ws.AddActor(10, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(115, 0, 100, 0), 1, true);
            _ws.AddActor(11, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(85, 0, 100, MathF.PI / 3), 1, true);
            _ws.AddActor(12, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(110, 0, 110, 2 * MathF.PI / 3), 1, true);
            _ws.AddActor(13, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(90, 0, 90, MathF.PI), 1, true);
            _ws.AddActor(14, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(100, 0, 115, -MathF.PI / 3), 1, true);
            _ws.AddActor(15, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(100, 0, 85, -2 * MathF.PI / 3), 1, true);
            _ws.AddActor(16, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(90, 0, 110, 0), 1, true);
            _ws.AddActor(17, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(110, 0, 90, 0), 1, true);
            _ws.AddActor(18, (uint)P3S.OID.Helper, "Helper", WorldState.ActorType.Enemy, Class.None, new(100, 0, 100, 0), 1, true);
            _ws.AddActor(19, (uint)P3S.OID.DarkblazeTwister, "Twister", WorldState.ActorType.Enemy, Class.None, new(114, 0, 108, -2 * MathF.PI / 3), 1, false);
            _ws.AddActor(20, (uint)P3S.OID.DarkblazeTwister, "Twister", WorldState.ActorType.Enemy, Class.None, new(100, 0, 84, 0), 1, false);
            _ws.AddActor(21, (uint)P3S.OID.DarkblazeTwister, "Twister", WorldState.ActorType.Enemy, Class.None, new(86, 0, 108, 2 * MathF.PI / 3), 1, false);
            _ws.AddActor(22, (uint)P3S.OID.SunbirdLarge, "BirdLarge", WorldState.ActorType.Enemy, Class.None, new(110, 0, 100, 0), 1, true);
            _ws.AddActor(23, (uint)P3S.OID.SunbirdLarge, "BirdLarge", WorldState.ActorType.Enemy, Class.None, new(90, 0, 100, 0), 1, true);
            _ws.AddActor(24, (uint)P3S.OID.SunbirdLarge, "BirdLarge", WorldState.ActorType.Enemy, Class.None, new(100, 0, 110, 0), 1, true);
            _ws.AddActor(25, (uint)P3S.OID.SunbirdLarge, "BirdLarge", WorldState.ActorType.Enemy, Class.None, new(100, 0, 90, 0), 1, true);
            _ws.AddActor(26, (uint)P3S.OID.DarkenedFire, "Fire", WorldState.ActorType.Enemy, Class.None, new(94, 0, 94, 0), 1, true);
            _ws.AddActor(27, (uint)P3S.OID.DarkenedFire, "Fire", WorldState.ActorType.Enemy, Class.None, new(106, 0, 94, 0), 1, true);
            _ws.AddActor(28, (uint)P3S.OID.DarkenedFire, "Fire", WorldState.ActorType.Enemy, Class.None, new(94, 0, 106, 0), 1, true);
            _ws.AddActor(29, (uint)P3S.OID.DarkenedFire, "Fire", WorldState.ActorType.Enemy, Class.None, new(106, 0, 106, 0), 1, true);
            _ws.SetWaymark(WorldState.Waymark.N1, new(100, 0, 90));
            _ws.SetWaymark(WorldState.Waymark.N2, new(110, 0, 100));
            _ws.SetWaymark(WorldState.Waymark.N3, new(100, 0, 110));
            _ws.SetWaymark(WorldState.Waymark.N4, new(90, 0, 100));
            _ws.PlayerActorID = 1;
            _o = new P3S(_ws);
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
            if (ImGui.Button("Reset icons"))
            {
                _numAssignedIcons = 0;
            }

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

                    ImGui.SameLine();
                    bool heatTether = actor.Tether.ID == (uint)P3S.TetherID.HeatOfCondemnation;
                    if (ImGui.Checkbox($"Heat tether##{actor.InstanceID}", ref heatTether))
                    {
                        _ws.UpdateTether(actor, heatTether ? new WorldState.TetherInfo { ID = (uint)P3S.TetherID.HeatOfCondemnation, Target = 9 } : new());
                    }

                    ImGui.SameLine();
                    if (ImGui.Button($"Assign icon {_numAssignedIcons + 1}##{actor.InstanceID}"))
                    {
                        _ws.DispatchEventIcon((actor.InstanceID, 268 + _numAssignedIcons));
                        ++_numAssignedIcons;
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
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = new(ActionType.Spell, 1) });
                        ImGui.SameLine();
                        if (ImGui.Button("WingL"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.LeftCinderwing) });
                        ImGui.SameLine();
                        if (ImGui.Button("WingR"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.RightCinderwing) });
                        ImGui.SameLine();
                        if (ImGui.Button("AshSpread"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.ExperimentalAshplumeSpread) });
                        ImGui.SameLine();
                        if (ImGui.Button("AshStack"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.ExperimentalAshplumeStack) });
                        ImGui.SameLine();
                        if (ImGui.Button("TrailC"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.TrailOfCondemnationCenter) });
                        ImGui.SameLine();
                        if (ImGui.Button("TrailS"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.TrailOfCondemnationSides) });
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
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.ExperimentalFireplumeMultiAOE) });
                        ImGui.SameLine();
                        if (ImGui.Button($"PlumeSingle##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.ExperimentalFireplumeSingleAOE) });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone1##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.FlamesOfAsphodelosAOE1) });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone2##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.FlamesOfAsphodelosAOE2) });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone3##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.FlamesOfAsphodelosAOE3) });
                    }
                }
                else if (actor.OID == (uint)P3S.OID.DarkblazeTwister)
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
                        if (ImGui.Button($"Knockback##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.DarkTwister) });
                        ImGui.SameLine();
                        if (ImGui.Button($"AOE##{actor.InstanceID}"))
                            _ws.UpdateCastInfo(actor, new WorldState.CastInfo { Action = ActionID.MakeSpell(P3S.AID.BurningTwister) });
                    }
                }
                else if (actor.OID == (uint)P3S.OID.SunbirdLarge)
                {
                    ImGui.SameLine();
                    if (ImGui.Button($"Tether 1->2##{actor.InstanceID}"))
                    {
                        _ws.UpdateTether(actor, new WorldState.TetherInfo { ID = 1, Target = 1 });
                        _ws.UpdateTether(_ws.FindActor(1)!, new WorldState.TetherInfo { ID = 1, Target = 2 });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Tether 2->1##{actor.InstanceID}"))
                    {
                        _ws.UpdateTether(actor, new WorldState.TetherInfo { ID = 1, Target = 2 });
                        _ws.UpdateTether(_ws.FindActor(2)!, new WorldState.TetherInfo { ID = 1, Target = 1 });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Tether 3->4##{actor.InstanceID}"))
                    {
                        _ws.UpdateTether(actor, new WorldState.TetherInfo { ID = 1, Target = 3 });
                        _ws.UpdateTether(_ws.FindActor(3)!, new WorldState.TetherInfo { ID = 1, Target = 4 });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Charge##{actor.InstanceID}"))
                    {
                        _ws.UpdateTether(actor, new());
                    }
                }
            }
        }
    }
}
