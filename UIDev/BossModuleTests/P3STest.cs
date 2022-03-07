using BossMod;
using BossMod.P3S;
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
            _ws.Actors.Add(1, 0, "T1", ActorType.Player, Class.WAR, new(100, 0, 90, 0));
            _ws.Actors.Add(2, 0, "T2", ActorType.Player, Class.PLD, new(100, 0, 110, 0));
            _ws.Actors.Add(3, 0, "H1", ActorType.Player, Class.WHM, new(90, 0, 90, 0));
            _ws.Actors.Add(4, 0, "H2", ActorType.Player, Class.SGE, new(92, 0, 90, 0));
            _ws.Actors.Add(5, 0, "R1", ActorType.Player, Class.BLM, new(94, 0, 90, 0));
            _ws.Actors.Add(6, 0, "R2", ActorType.Player, Class.MCH, new(90, 0, 92, 0));
            _ws.Actors.Add(7, 0, "M1", ActorType.Player, Class.MNK, new(92, 0, 92, 0));
            _ws.Actors.Add(8, 0, "M2", ActorType.Player, Class.RPR, new(94, 0, 92, 0));
            _ws.Actors.Add(9, (uint)OID.Boss, "Boss", ActorType.Enemy, Class.None, new(100, 0, 100, 0));
            _ws.Actors.Add(10, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(115, 0, 100, 0));
            _ws.Actors.Add(11, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(85, 0, 100, MathF.PI / 3));
            _ws.Actors.Add(12, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(110, 0, 110, 2 * MathF.PI / 3));
            _ws.Actors.Add(13, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(90, 0, 90, MathF.PI));
            _ws.Actors.Add(14, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(100, 0, 115, -MathF.PI / 3));
            _ws.Actors.Add(15, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(100, 0, 85, -2 * MathF.PI / 3));
            _ws.Actors.Add(16, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(90, 0, 110, 0));
            _ws.Actors.Add(17, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(110, 0, 90, 0));
            _ws.Actors.Add(18, (uint)OID.Helper, "Helper", ActorType.Enemy, Class.None, new(100, 0, 100, 0));
            _ws.Actors.Add(19, (uint)OID.DarkblazeTwister, "Twister", ActorType.Enemy, Class.None, new(114, 0, 108, -2 * MathF.PI / 3), 1, false);
            _ws.Actors.Add(20, (uint)OID.DarkblazeTwister, "Twister", ActorType.Enemy, Class.None, new(100, 0, 84, 0), 1, false);
            _ws.Actors.Add(21, (uint)OID.DarkblazeTwister, "Twister", ActorType.Enemy, Class.None, new(86, 0, 108, 2 * MathF.PI / 3), 1, false);
            _ws.Actors.Add(22, (uint)OID.SunbirdLarge, "BirdLarge", ActorType.Enemy, Class.None, new(110, 0, 100, 0));
            _ws.Actors.Add(23, (uint)OID.SunbirdLarge, "BirdLarge", ActorType.Enemy, Class.None, new(90, 0, 100, 0));
            _ws.Actors.Add(24, (uint)OID.SunbirdLarge, "BirdLarge", ActorType.Enemy, Class.None, new(100, 0, 110, 0));
            _ws.Actors.Add(25, (uint)OID.SunbirdLarge, "BirdLarge", ActorType.Enemy, Class.None, new(100, 0, 90, 0));
            _ws.Actors.Add(26, (uint)OID.DarkenedFire, "Fire", ActorType.Enemy, Class.None, new(94, 0, 94, 0));
            _ws.Actors.Add(27, (uint)OID.DarkenedFire, "Fire", ActorType.Enemy, Class.None, new(106, 0, 94, 0));
            _ws.Actors.Add(28, (uint)OID.DarkenedFire, "Fire", ActorType.Enemy, Class.None, new(94, 0, 106, 0));
            _ws.Actors.Add(29, (uint)OID.DarkenedFire, "Fire", ActorType.Enemy, Class.None, new(106, 0, 106, 0));
            _ws.Waymarks[Waymark.N1] = new(100, 0, 90);
            _ws.Waymarks[Waymark.N2] = new(110, 0, 100);
            _ws.Waymarks[Waymark.N3] = new(100, 0, 110);
            _ws.Waymarks[Waymark.N4] = new(90, 0, 100);
            for (int i = 1; i <= 8; ++i)
                _ws.Party.Add((ulong)i, _ws.Actors.Find((uint)i), i == 1);
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

            _o.Draw(_azimuth / 180 * MathF.PI, PartyState.PlayerSlot, null);

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
            if (ImGui.Button("Reset icons"))
            {
                _numAssignedIcons = 0;
            }

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

                    ImGui.SameLine();
                    bool heatTether = actor.Tether.ID == (uint)TetherID.HeatOfCondemnation;
                    if (ImGui.Checkbox($"Heat tether##{actor.InstanceID}", ref heatTether))
                    {
                        _ws.Actors.UpdateTether(actor, heatTether ? new ActorTetherInfo { ID = (uint)TetherID.HeatOfCondemnation, Target = 9 } : new());
                    }

                    ImGui.SameLine();
                    if (ImGui.Button($"Assign icon {_numAssignedIcons + 1}##{actor.InstanceID}"))
                    {
                        _ws.Events.DispatchIcon((actor.InstanceID, 268 + _numAssignedIcons));
                        ++_numAssignedIcons;
                    }
                }
                else if (actor.OID == (uint)OID.Boss)
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
                        if (ImGui.Button("Generic"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = new(ActionType.Spell, 1) });
                        ImGui.SameLine();
                        if (ImGui.Button("WingL"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.LeftCinderwing) });
                        ImGui.SameLine();
                        if (ImGui.Button("WingR"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.RightCinderwing) });
                        ImGui.SameLine();
                        if (ImGui.Button("AshSpread"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.ExperimentalAshplumeSpread) });
                        ImGui.SameLine();
                        if (ImGui.Button("AshStack"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.ExperimentalAshplumeStack) });
                        ImGui.SameLine();
                        if (ImGui.Button("TrailC"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.TrailOfCondemnationCenter) });
                        ImGui.SameLine();
                        if (ImGui.Button("TrailS"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.TrailOfCondemnationSides) });
                    }
                }
                else if (actor.OID == (uint)OID.Helper)
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
                        if (ImGui.Button($"PlumeMulti##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.ExperimentalFireplumeMultiAOE) });
                        ImGui.SameLine();
                        if (ImGui.Button($"PlumeSingle##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.ExperimentalFireplumeSingleAOE) });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone1##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.FlamesOfAsphodelosAOE1) });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone2##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.FlamesOfAsphodelosAOE2) });
                        ImGui.SameLine();
                        if (ImGui.Button($"Cone3##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.FlamesOfAsphodelosAOE3) });
                    }
                }
                else if (actor.OID == (uint)OID.DarkblazeTwister)
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
                        if (ImGui.Button($"Knockback##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.DarkTwister) });
                        ImGui.SameLine();
                        if (ImGui.Button($"AOE##{actor.InstanceID}"))
                            _ws.Actors.UpdateCastInfo(actor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.BurningTwister) });
                    }
                }
                else if (actor.OID == (uint)OID.SunbirdLarge)
                {
                    ImGui.SameLine();
                    if (ImGui.Button($"Tether 1->2##{actor.InstanceID}"))
                    {
                        _ws.Actors.UpdateTether(actor, new ActorTetherInfo { ID = 1, Target = 1 });
                        _ws.Actors.UpdateTether(_ws.Actors.Find(1)!, new ActorTetherInfo { ID = 1, Target = 2 });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Tether 2->1##{actor.InstanceID}"))
                    {
                        _ws.Actors.UpdateTether(actor, new ActorTetherInfo { ID = 1, Target = 2 });
                        _ws.Actors.UpdateTether(_ws.Actors.Find(2)!, new ActorTetherInfo { ID = 1, Target = 1 });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Tether 3->4##{actor.InstanceID}"))
                    {
                        _ws.Actors.UpdateTether(actor, new ActorTetherInfo { ID = 1, Target = 3 });
                        _ws.Actors.UpdateTether(_ws.Actors.Find(3)!, new ActorTetherInfo { ID = 1, Target = 4 });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Charge##{actor.InstanceID}"))
                    {
                        _ws.Actors.UpdateTether(actor, new());
                    }
                }
            }
        }
    }
}
