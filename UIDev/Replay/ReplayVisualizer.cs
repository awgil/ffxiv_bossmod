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
        private ReplayPlayer _player;
        private BossModuleManager _mgr;
        private DateTime _first;
        private DateTime _last;
        private DateTime _prevFrame;
        private float _playSpeed = 0;
        private float _azimuth;
        private int _povSlot = PartyState.PlayerSlot;
        private bool _showConfig = false;
        private Tree _encountersTree = new();

        public ReplayVisualizer(Replay data)
        {
            _player = new(data);
            _mgr = new(_player.WorldState, new());
            _first = data.Ops.First().Timestamp;
            _last = data.Ops.Last().Timestamp;
            _player.AdvanceTo(_first, _mgr.Update);
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            var curFrame = DateTime.Now;
            MoveTo(_player.WorldState.CurrentTime + (curFrame - _prevFrame) * _playSpeed);
            _prevFrame = curFrame;

            DrawControlRow();
            DrawTimelineRow();
            ImGui.Text($"Num loaded modules: {_mgr.LoadedModules.Count}, num active modules: {_mgr.ActiveModules.Count}, active module: {_mgr.ActiveModule?.GetType()}");
            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);
            if (_mgr.ActiveModule != null)
            {
                _mgr.ActiveModule.Draw(_azimuth / 180 * MathF.PI, _povSlot, null);

                ImGui.Text($"Downtime in: {_mgr.ActiveModule.PlanExecution?.EstimateTimeToNextDowntime(_mgr.ActiveModule.StateMachine):f2}, Positioning in: {_mgr.ActiveModule.PlanExecution?.EstimateTimeToNextPositioning(_mgr.ActiveModule.StateMachine):f2}, Components:");
                foreach (var comp in _mgr.ActiveModule.Components)
                {
                    ImGui.SameLine();
                    ImGui.Text(comp.GetType().Name);
                }

                if (ImGui.CollapsingHeader("Plan execution"))
                {
                    _mgr.ActiveModule.PlanExecution?.Draw(_mgr.ActiveModule.StateMachine);
                }
            }

            DrawPartyTable();
            DrawEnemyTables();
            DrawAllActorsTable();
            DrawEncounters();
        }

        private void DrawControlRow()
        {
            ImGui.Text($"{_player.WorldState.CurrentTime:O}");
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

            ImGui.SameLine();
            ImGui.Checkbox("Show config", ref _showConfig);
            if (_showConfig)
            {
                _mgr.WindowConfig.Draw();
                _mgr.EncounterConfig.Draw();
            }
        }

        private void DrawTimelineRow()
        {
            var dl = ImGui.GetWindowDrawList();
            var cursor = ImGui.GetCursorScreenPos();
            var w = ImGui.GetWindowWidth() - 2 * ImGui.GetCursorPosX() - 15;
            cursor.Y += 4;
            dl.AddLine(cursor, cursor + new Vector2(w, 0), 0xff00ffff);

            var curp = cursor + new Vector2(w * (float)((_player.WorldState.CurrentTime - _first) / (_last - _first)), 0);
            dl.AddTriangleFilled(curp, curp + new Vector2(3, 5), curp + new Vector2(-3, 5), 0xff00ffff);
            foreach (var e in _player.Replay.Encounters)
            {
                DrawCheckpoint(e.Time.Start, 0xff00ff00, cursor, w);
                DrawCheckpoint(e.Time.End, 0xff0000ff, cursor, w);
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
            _player.WorldState.Actors.Move(actor, new(pos, rot / 180 * MathF.PI));

            ImGui.TableNextColumn();
            if (actor.IsDead)
            {
                ImGui.Text("(Dead)");
                ImGui.SameLine();
            }
            ImGui.Text(actor.Name);

            ImGui.TableNextColumn();
            if (actor.CastInfo != null)
                ImGui.Text($"{actor.CastInfo.Action}: {Utils.CastTimeString(actor.CastInfo, _player.WorldState.CurrentTime)}");

            ImGui.TableNextColumn();
            foreach (var s in actor.Statuses.Where(s => s.ID != 0))
            {
                var src = _player.WorldState.Actors.Find(s.SourceID);
                if (src?.Type == ActorType.Player || src?.Type == ActorType.Pet)
                    continue;
                ImGui.Text($"{Utils.StatusString(s.ID)} ({s.Extra}): {Utils.StatusTimeString(s.ExpireAt, _player.WorldState.CurrentTime)}");
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
            foreach ((int slot, var player) in _player.WorldState.Party.WithSlot(true))
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
            foreach (var actor in _player.WorldState.Actors)
            {
                ImGui.PushID((int)actor.InstanceID);
                ImGui.TableNextRow();
                DrawCommonColumns(actor);
                ImGui.PopID();
            }
            ImGui.EndTable();
        }

        private string ActionString(Replay.Action a, DateTime start, Type? aidType)
        {
            return $"{new Replay.TimeRange(start, a.Timestamp)}: {a.ID} ({aidType?.GetEnumName(a.ID.ID)}): {ReplayUtils.ParticipantPosRotString(a.Source, a.SourcePosRot)} -> {ReplayUtils.ParticipantPosRotString(a.MainTarget, a.MainTargetPosRot)} ({a.Targets.Count} affected)";
        }

        private string StatusString(Replay.Status s, DateTime start, Type? sidType)
        {
            return $"{new Replay.TimeRange(start, s.Time.Start)} + {s.InitialDuration:f2} / {s.Time}: {Utils.StatusString(s.ID)} ({sidType?.GetEnumName(s.ID)}) ({s.StartingExtra:X}) @ {ReplayUtils.ParticipantString(s.Target)} from {ReplayUtils.ParticipantString(s.Source)}";
        }

        private void DrawActionNodes(IEnumerable<Replay.Action> actions, DateTime start, Type? aidType)
        {
            foreach (var a in _encountersTree.Nodes(actions, a => (ActionString(a, start, aidType), a.Targets.Count == 0)))
            {
                foreach (var t in _encountersTree.Nodes(a.Targets, t => (ReplayUtils.ParticipantPosRotString(t.Target, t.PosRot), false)))
                {
                    _encountersTree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
                }
            }
        }

        private void DrawStatusNodes(IEnumerable<Replay.Status> statuses, DateTime start, Type? sidType)
        {
            _encountersTree.LeafNodes(statuses, s => StatusString(s, start, sidType));
        }

        private void OpenCooldownPlanner(Replay.Encounter enc, Class pcClass, Replay.Participant? pc = null)
        {
            var planner = new ReplayVisPlayer(_player.Replay, enc, pcClass, pc);
            var w = WindowManager.CreateWindow($"Cooldown planner", planner.Draw, () => { }, () => true);
            w.SizeHint = new(600, 600);
            w.MinSize = new(100, 100);
        }

        private void DrawEncounters()
        {
            if (!ImGui.CollapsingHeader("Encounters"))
                return;

            foreach (var e in _encountersTree.Nodes(_player.Replay.Encounters, e => ($"{ModuleRegistry.TypeForOID(e.OID)}: {e.InstanceID:X}, zone={e.Zone}, start={e.Time.Start:O}, duration={e.Time}", false)))
            {
                var moduleType = ModuleRegistry.TypeForOID(e.OID)!;
                var oidType = moduleType.Module.GetType($"{moduleType.Namespace}.OID");
                var aidType = moduleType.Module.GetType($"{moduleType.Namespace}.AID");
                var sidType = moduleType.Module.GetType($"{moduleType.Namespace}.SID");
                var iidType = moduleType.Module.GetType($"{moduleType.Namespace}.IconID");
                var tidType = moduleType.Module.GetType($"{moduleType.Namespace}.TetherID");
                foreach (var en in _encountersTree.Node("Participants", e.Participants.Count == 0))
                {
                    foreach (var (oid, list) in _encountersTree.Nodes(e.Participants, kv => ($"{kv.Key:X} '{oidType?.GetEnumName(kv.Key)}' ({kv.Value.Count} objects)", false)))
                    {
                        foreach (var p in _encountersTree.Nodes(list, p => ($"{ReplayUtils.ParticipantString(p)}: spawn at {new Replay.TimeRange(e.Time.Start, p.Existence.Start)}, despawn at {new Replay.TimeRange(e.Time.Start, p.Existence.End)}", p.Casts.Count == 0 && !p.HasAnyActions && !p.HasAnyStatuses && !p.IsTargetOfAnyActions && p.Targetable.Count == 0)))
                        {
                            if (p.Casts.Count > 0)
                            {
                                foreach (var cn in _encountersTree.Node("Casts"))
                                {
                                    var prev = e.Time.Start;
                                    foreach (var c in _encountersTree.Nodes(p.Casts, c => ($"{new Replay.TimeRange(e.Time.Start, c.Time.Start)} ({new Replay.TimeRange(prev, c.Time.Start)}) + {c.ExpectedCastTime + 0.3f:f2} ({c.Time}): {c.ID} ({aidType?.GetEnumName(c.ID.ID)}) @ {ReplayUtils.ParticipantString(c.Target)} / {Utils.Vec3String(c.Location)}", true)))
                                    {
                                        prev = c.Time.End;
                                    }
                                }
                            }
                            if (p.HasAnyActions)
                            {
                                foreach (var an in _encountersTree.Node("Actions"))
                                {
                                    DrawActionNodes(_player.Replay.EncounterActions(e).Where(a => a.Source == p), e.Time.Start, aidType);
                                }
                            }
                            if (p.IsTargetOfAnyActions)
                            {
                                foreach (var an in _encountersTree.Node("Affected by actions"))
                                {
                                    DrawActionNodes(_player.Replay.EncounterActions(e).Where(a => a.Targets.Any(t => t.Target == p)), e.Time.Start, aidType);
                                }
                            }
                            if (p.HasAnyStatuses)
                            {
                                foreach (var an in _encountersTree.Node("Statuses"))
                                {
                                    DrawStatusNodes(_player.Replay.EncounterStatuses(e).Where(s => s.Target == p), e.Time.Start, sidType);
                                }
                            }
                            if (p.Targetable.Count > 0)
                            {
                                foreach (var an in _encountersTree.Node("Targetable"))
                                {
                                    _encountersTree.LeafNodes(p.Targetable, r => $"{new Replay.TimeRange(e.Time.Start, r.Start)} - {new Replay.TimeRange(e.Time.Start, r.End)}");
                                }
                            }
                        }
                    }
                }

                bool haveActions = _player.Replay.EncounterActions(e).Any();
                Func<Replay.Action, bool> actionIsCrap = a => a.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo;
                foreach (var n in _encountersTree.Node("Interesting actions", !haveActions))
                {
                    DrawActionNodes(_player.Replay.EncounterActions(e).Where(a => !actionIsCrap(a)), e.Time.Start, aidType);
                }
                foreach (var n in _encountersTree.Node("Other actions", !haveActions))
                {
                    DrawActionNodes(_player.Replay.EncounterActions(e).Where(actionIsCrap), e.Time.Start, aidType);
                }

                bool haveStatuses = _player.Replay.EncounterStatuses(e).Any();
                Func<Replay.Status, bool> statusIsCrap = s => (s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo) || (s.Target?.Type is ActorType.Pet or ActorType.Chocobo);
                foreach (var n in _encountersTree.Node("Interesting statuses", !haveStatuses))
                {
                    DrawStatusNodes(_player.Replay.EncounterStatuses(e).Where(s => !statusIsCrap(s)), e.Time.Start, sidType);
                }
                foreach (var n in _encountersTree.Node("Other statuses", !haveStatuses))
                {
                    DrawStatusNodes(_player.Replay.EncounterStatuses(e).Where(statusIsCrap), e.Time.Start, sidType);
                }

                foreach (var n in _encountersTree.Node("Tethers", !_player.Replay.EncounterTethers(e).Any()))
                {
                    _encountersTree.LeafNodes(_player.Replay.EncounterTethers(e), t => $"{new Replay.TimeRange(e.Time.Start, t.Time.Start)} + {t.Time}: {t.ID} ({tidType?.GetEnumName(t.ID)}) @ {ReplayUtils.ParticipantString(t.Source)} -> {ReplayUtils.ParticipantString(t.Target)}");
                }

                foreach (var n in _encountersTree.Node("Icons", !_player.Replay.EncounterIcons(e).Any()))
                {
                    _encountersTree.LeafNodes(_player.Replay.EncounterIcons(e), i => $"{new Replay.TimeRange(e.Time.Start, i.Timestamp)}: {i.ID} ({iidType?.GetEnumName(i.ID)}) @ {ReplayUtils.ParticipantString(i.Target)}");
                }

                foreach (var n in _encountersTree.Node("EnvControls", !_player.Replay.EncounterEnvControls(e).Any()))
                {
                    _encountersTree.LeafNodes(_player.Replay.EncounterEnvControls(e), ec => $"{new Replay.TimeRange(e.Time.Start, ec.Timestamp)}: {ec.Feature:X8}.{ec.Index:X2} = {ec.State:X8}");
                }

                foreach (var n in _encountersTree.Node("Player actions timeline", false))
                {
                    foreach (var c in AbilityDefinitions.Classes.Keys)
                    {
                        if (ImGui.Button(c.ToString()))
                        {
                            OpenCooldownPlanner(e, c);
                        }
                        foreach (var (p, _) in e.PartyMembers.Where(pc => pc.Item2 == c))
                        {
                            ImGui.SameLine();
                            if (ImGui.Button($"{p.Name}##{p.InstanceID:X}"))
                            {
                                OpenCooldownPlanner(e, c, p);
                            }
                        }
                    }
                }
            }
        }

        private void MoveTo(DateTime t)
        {
            if (t > _player.WorldState.CurrentTime)
            {
                _player.AdvanceTo(t, _mgr.Update);
            }
            else if (t < _player.WorldState.CurrentTime)
            {
                _player.ReverseTo(t, _mgr.Update);
            }
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
