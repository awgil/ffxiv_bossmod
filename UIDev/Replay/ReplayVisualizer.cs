using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UIDev
{
    class ReplayVisualizer : IDisposable
    {
        private Replay _data;
        private WorldState _ws = new();
        private BossModuleManager _mgr;
        private int _cursor = 0;
        private DateTime _first;
        private DateTime _last;
        private DateTime _prevFrame;
        private float _playSpeed = 0;
        private float _azimuth;
        private int _povSlot = PartyState.PlayerSlot;

        public ReplayVisualizer(Replay data)
        {
            _data = data;
            _mgr = new(_ws, new());
            _ws.CurrentTime = _first = data.Ops.First().Timestamp;
            _last = data.Ops.Last().Timestamp;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            var curFrame = DateTime.Now;
            MoveTo(_ws.CurrentTime + (curFrame - _prevFrame) * _playSpeed);
            _prevFrame = curFrame;

            _mgr.Update();

            DrawControlRow();
            DrawTimelineRow();
            ImGui.Text($"Num loaded modules: {_mgr.LoadedModules.Count}, num active modules: {_mgr.ActiveModules.Count}, active module: {_mgr.ActiveModule?.GetType()}");
            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);
            if (_mgr.ActiveModule != null)
            {
                _mgr.ActiveModule.Draw(_azimuth / 180 * MathF.PI, _povSlot, null);

                ImGui.Text($"Downtime in: {_mgr.ActiveModule.StateMachine.EstimateTimeToNextDowntime():f2}, Positioning in: {_mgr.ActiveModule.StateMachine.EstimateTimeToNextPositioning():f2}, Components:");
                foreach (var comp in _mgr.ActiveModule.Components)
                {
                    ImGui.SameLine();
                    ImGui.Text(comp.GetType().Name);
                }
            }

            DrawPartyTable();
            DrawEnemyTables();
            DrawAllActorsTable();
            DrawEncounters();
        }

        private void DrawControlRow()
        {
            ImGui.Text($"{_ws.CurrentTime:O}");
            ImGui.SameLine();
            if (ImGui.Button("<<<"))
                _playSpeed = -10;
            ImGui.SameLine();
            if (ImGui.Button("<<"))
                _playSpeed = -1;
            ImGui.SameLine();
            if (ImGui.Button("<"))
                _playSpeed = -0.2f;
            ImGui.SameLine();
            if (ImGui.Button("||"))
                _playSpeed = _playSpeed == 0 ? 1 : 0;
            ImGui.SameLine();
            if (ImGui.Button(">"))
                _playSpeed = 0.2f;
            ImGui.SameLine();
            if (ImGui.Button(">>"))
                _playSpeed = 1;
            ImGui.SameLine();
            if (ImGui.Button(">>>"))
                _playSpeed = 10;
        }

        private void DrawTimelineRow()
        {
            var dl = ImGui.GetWindowDrawList();
            var cursor = ImGui.GetCursorScreenPos();
            var w = ImGui.GetWindowWidth() - 2 * ImGui.GetCursorPosX() - 15;
            cursor.Y += 4;
            dl.AddLine(cursor, cursor + new Vector2(w, 0), 0xff00ffff);

            var curp = cursor + new Vector2(w * (float)((_ws.CurrentTime - _first) / (_last - _first)), 0);
            dl.AddTriangleFilled(curp, curp + new Vector2(3, 5), curp + new Vector2(-3, 5), 0xff00ffff);
            foreach (var e in _data.Encounters)
            {
                DrawCheckpoint(e.Start, 0xff00ff00, cursor, w);
                DrawCheckpoint(e.End, 0xff0000ff, cursor, w);
            }
            ImGui.Dummy(new(w, 8));
        }

        private void DrawCheckpoint(DateTime timestamp, uint color, Vector2 cursor, float w)
        {
            var off = (float)((timestamp - _first) / (_last - _first));
            var center = cursor + new Vector2(w * off, 0);
            ImGui.GetWindowDrawList().AddCircleFilled(center, 3, color);
            if (ClickedAt(center, 3))
            {
                MoveTo(timestamp);
            }
        }

        // x, z, rot, name, cast, statuses
        private void DrawCommonColumns(Actor actor)
        {
            var pos = actor.Position;
            var rot = actor.Rotation / MathF.PI * 180;
            ImGui.TableNextColumn(); ImGui.DragFloat("###X", ref pos.X, 0.25f, 80, 120);
            ImGui.TableNextColumn(); ImGui.DragFloat("###Z", ref pos.Z, 0.25f, 80, 120);
            ImGui.TableNextColumn(); ImGui.DragFloat("###Rot", ref rot, 1, -180, 180);
            _ws.Actors.Move(actor, new(pos, rot / 180 * MathF.PI));

            ImGui.TableNextColumn();
            if (actor.IsDead)
            {
                ImGui.Text("(Dead)");
                ImGui.SameLine();
            }
            ImGui.Text(actor.Name);

            ImGui.TableNextColumn();
            if (actor.CastInfo != null)
                ImGui.Text($"{actor.CastInfo.Action}: {Utils.CastTimeString(actor.CastInfo, _ws.CurrentTime)}");

            ImGui.TableNextColumn();
            foreach (var s in actor.Statuses.Where(s => s.ID != 0))
            {
                var src = _ws.Actors.Find(s.SourceID);
                if (src?.Type == ActorType.Player || src?.Type == ActorType.Pet)
                    continue;
                ImGui.Text($"{Utils.StatusString(s.ID)} ({s.Extra}): {Utils.StatusTimeString(s.ExpireAt, _ws.CurrentTime)}");
                ImGui.SameLine();
            }
        }

        private void DrawPartyTable()
        {
            if (!ImGui.CollapsingHeader("Party"))
                return;

            var riskColor = ImGui.ColorConvertU32ToFloat4(0xff00ffff);
            var safeColor = ImGui.ColorConvertU32ToFloat4(0xff00ff00);

            ImGui.BeginTable("party", 9, ImGuiTableFlags.Resizable);
            ImGui.TableSetupColumn("POV", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 25);
            ImGui.TableSetupColumn("Class", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 30);
            ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Rot", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 100);
            ImGui.TableSetupColumn("Cast", ImGuiTableColumnFlags.None, 100);
            ImGui.TableSetupColumn("Statuses", ImGuiTableColumnFlags.None, 100);
            ImGui.TableSetupColumn("Hints", ImGuiTableColumnFlags.None, 250);
            ImGui.TableHeadersRow();
            foreach ((int slot, var player) in _ws.Party.WithSlot(true))
            {
                ImGui.PushID((int)player.InstanceID);
                ImGui.TableNextRow();

                bool isPOV = _povSlot == slot;
                ImGui.TableNextColumn();
                ImGui.Checkbox("###POV", ref isPOV);
                if (isPOV)
                    _povSlot = slot;

                ImGui.TableNextColumn();
                ImGui.Text(player.Class.ToString());

                DrawCommonColumns(player);

                ImGui.TableNextColumn();
                if (_mgr.ActiveModule != null)
                {
                    var hints = _mgr.ActiveModule.CalculateHintsForRaidMember(slot, player);
                    foreach ((var hint, bool risk) in hints)
                    {
                        ImGui.TextColored(risk ? riskColor : safeColor, hint);
                        ImGui.SameLine();
                    }
                }

                ImGui.PopID();
            }
            ImGui.EndTable();
        }

        private void DrawEnemyTables()
        {
            if (_mgr.ActiveModule == null)
                return;

            DrawEnemyTable(_mgr.ActiveModule.PrimaryActor.OID, new Actor[] { _mgr.ActiveModule.PrimaryActor });
            foreach ((var oid, var list) in _mgr.ActiveModule.RelevantEnemies)
            {
                DrawEnemyTable(oid, list);
            }
        }

        private void DrawEnemyTable(uint oid, ICollection<Actor> actors)
        {
            var oidType = _mgr.ActiveModule != null ? _mgr.ActiveModule.GetType().Module.GetType($"{_mgr.ActiveModule.GetType().Namespace}.OID") : null;
            var oidName = oidType?.GetEnumName(oid);
            if (!ImGui.CollapsingHeader($"Enemy {oid:X} {oidName ?? ""}") || actors.Count == 0)
                return;

            ImGui.BeginTable($"enemy_{oid}", 6, ImGuiTableFlags.Resizable);
            ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Rot", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 100);
            ImGui.TableSetupColumn("Cast");
            ImGui.TableSetupColumn("Statuses");
            ImGui.TableHeadersRow();
            foreach (var enemy in actors)
            {
                ImGui.PushID((int)enemy.InstanceID);
                ImGui.TableNextRow();
                DrawCommonColumns(enemy);
                ImGui.PopID();
            }
            ImGui.EndTable();
        }

        private void DrawAllActorsTable()
        {
            if (!ImGui.CollapsingHeader("All actors"))
                return;

            ImGui.BeginTable($"actors", 6, ImGuiTableFlags.Resizable);
            ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Rot", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 100);
            ImGui.TableSetupColumn("Cast");
            ImGui.TableSetupColumn("Statuses");
            ImGui.TableHeadersRow();
            foreach (var actor in _ws.Actors)
            {
                ImGui.PushID((int)actor.InstanceID);
                ImGui.TableNextRow();
                DrawCommonColumns(actor);
                ImGui.PopID();
            }
            ImGui.EndTable();
        }

        private string ParticipantString(Replay.Participant? p)
        {
            return p != null ? $"{p.Type} {p.InstanceID:X} ({p.OID:X}) '{p.Name}'" : "<none>";
        }

        private void DrawActionNode(Replay.Action a, DateTime start, Type? aidType)
        {
            if (ImGui.TreeNodeEx($"{(a.Time - start).TotalSeconds:f2}: {a.ID} ({aidType?.GetEnumName(a.ID.ID)}): {ParticipantString(a.Source)} -> {ParticipantString(a.MainTarget)} ({a.Targets.Count} affected)", a.Targets.Count > 0 ? ImGuiTreeNodeFlags.None : ImGuiTreeNodeFlags.Leaf))
            {
                foreach (var t in a.Targets)
                {
                    if (ImGui.TreeNodeEx($"{ParticipantString(t.Target)}", ImGuiTreeNodeFlags.Leaf))
                        ImGui.TreePop();
                }
                ImGui.TreePop();
            }
        }

        private void DrawEncounters()
        {
            if (!ImGui.CollapsingHeader("Encounters"))
                return;

            foreach (var e in _data.Encounters)
            {
                var moduleType = ModuleRegistry.TypeForOID(e.OID)!;
                var oidType = moduleType.Module.GetType($"{moduleType.Namespace}.OID");
                var aidType = moduleType.Module.GetType($"{moduleType.Namespace}.AID");
                var sidType = moduleType.Module.GetType($"{moduleType.Namespace}.SID");
                var iidType = moduleType.Module.GetType($"{moduleType.Namespace}.IconID");
                var tidType = moduleType.Module.GetType($"{moduleType.Namespace}.TetherID");
                if (ImGui.TreeNode($"{moduleType}: {e.InstanceID:X}, zone={e.Zone}, start={e.Start:O}, duration={(e.End - e.Start).TotalSeconds:f2}"))
                {
                    foreach ((var oid, var list) in e.Enemies)
                    {
                        if (ImGui.TreeNode($"{oid:X} '{oidType?.GetEnumName(oid)}'"))
                        {
                            foreach (var p in list)
                            {
                                var pflags = (p.Casts.Count > 0 || p.Actions.Count > 0) ? ImGuiTreeNodeFlags.None : ImGuiTreeNodeFlags.Leaf;
                                if (ImGui.TreeNodeEx($"{ParticipantString(p)}: spawn at {(p.Spawn - e.Start).TotalSeconds:f2}, despawn at {(p.Despawn - e.Start).TotalSeconds:f2}", pflags))
                                {
                                    if (p.Casts.Count > 0 && ImGui.TreeNode("Casts"))
                                    {
                                        for (int i = 0; i < p.Casts.Count; ++i)
                                        {
                                            var c = p.Casts[i];
                                            if (ImGui.TreeNodeEx($"{(c.Start - e.Start).TotalSeconds:f2} ({(c.Start - (i == 0 ? e.Start : p.Casts[i - 1].End)).TotalSeconds:f2}) + {(c.End - c.Start).TotalSeconds:f2}: {c.ID} ({aidType?.GetEnumName(c.ID.ID)}) @ {ParticipantString(c.Target)}", ImGuiTreeNodeFlags.Leaf))
                                                ImGui.TreePop();
                                        }
                                        ImGui.TreePop();
                                    }
                                    if (p.Actions.Count > 0 && ImGui.TreeNode("Actions"))
                                    {
                                        foreach (var a in p.Actions)
                                        {
                                            DrawActionNode(a, e.Start, aidType);
                                        }
                                        ImGui.TreePop();
                                    }
                                    ImGui.TreePop();
                                }
                            }
                            ImGui.TreePop();
                        }
                    }
                    if (ImGui.TreeNode("Actions"))
                    {
                        foreach (var a in e.Actions)
                        {
                            DrawActionNode(a, e.Start, aidType);
                        }
                        ImGui.TreePop();
                    }
                    if (ImGui.TreeNode("Statuses"))
                    {
                        foreach (var s in e.Statuses)
                        {
                            if (ImGui.TreeNodeEx($"{(s.Apply - e.Start).TotalSeconds:f2} + {(s.Expire - s.Apply).TotalSeconds:f2} / {(s.Fade - s.Apply).TotalSeconds:f2}: {Utils.StatusString(s.ID)} ({sidType?.GetEnumName(s.ID)}) ({s.StartingExtra:X}) @ {ParticipantString(s.Target)} from {ParticipantString(s.Source)}", ImGuiTreeNodeFlags.Leaf))
                                ImGui.TreePop();
                        }
                        ImGui.TreePop();
                    }
                    if (ImGui.TreeNode("Tethers"))
                    {
                        foreach (var t in e.Tethers)
                        {
                            if (ImGui.TreeNodeEx($"{(t.Appear - e.Start).TotalSeconds:f2} + {(t.Disappear - t.Appear).TotalSeconds:f2}: {t.ID} ({tidType?.GetEnumName(t.ID)}) @ {ParticipantString(t.Source)} -> {ParticipantString(t.Target)}", ImGuiTreeNodeFlags.Leaf))
                                ImGui.TreePop();
                        }
                        ImGui.TreePop();
                    }
                    if (ImGui.TreeNode("Icons"))
                    {
                        foreach (var i in e.Icons)
                        {
                            if (ImGui.TreeNodeEx($"{(i.Timestamp - e.Start).TotalSeconds:f2}: {i.ID} ({iidType?.GetEnumName(i.ID)}) @ {ParticipantString(i.Target)}", ImGuiTreeNodeFlags.Leaf))
                                ImGui.TreePop();
                        }
                        ImGui.TreePop();
                    }
                    if (ImGui.TreeNode("EnvControls"))
                    {
                        foreach (var ec in e.EnvControls)
                        {
                            if (ImGui.TreeNodeEx($"{(ec.Timestamp - e.Start).TotalSeconds:f2}: {ec.Feature:X8}.{ec.Index:X2} = {ec.State:X8}", ImGuiTreeNodeFlags.Leaf))
                                ImGui.TreePop();
                        }
                        ImGui.TreePop();
                    }
                    ImGui.TreePop();
                }
            }
        }

        private void MoveTo(DateTime t)
        {
            if (t > _ws.CurrentTime)
            {
                while (_cursor < _data.Ops.Count && t > _data.Ops[_cursor].Timestamp)
                {
                    _ws.CurrentTime = _data.Ops[_cursor].Timestamp;
                    _data.Ops[_cursor++].Redo(_ws);
                }
            }
            else if (t < _ws.CurrentTime)
            {
                while (_cursor > 0 && t <= _data.Ops[_cursor - 1].Timestamp)
                {
                    _ws.CurrentTime = _data.Ops[_cursor - 1].Timestamp;
                    _data.Ops[--_cursor].Undo(_ws);
                }
            }
            _ws.CurrentTime = t;
        }

        private bool ClickedAt(Vector2 centerPos, float halfSize)
        {
            if (!ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                return false;
            var pos = ImGui.GetMousePos();
            return pos.X >= centerPos.X - halfSize && pos.X <= centerPos.X + halfSize && pos.Y >= centerPos.Y - halfSize && pos.Y <= centerPos.Y + halfSize;
        }
    }
}
