using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UIDev
{
    class ReplayVisualizer : IDisposable
    {
        private Replay _data;
        private WorldState _ws = new();
        private int _cursor = 0;
        private List<(DateTime, bool)> _checkpoints = new();
        private DateTime _first;
        private DateTime _last;
        private DateTime _prevFrame;
        private float _playSpeed = 0;
        private BossModule? _bossmod;
        private float _azimuth;
        private int _povSlot = PartyState.PlayerSlot;

        public ReplayVisualizer(Replay data)
        {
            _data = data;
            _ws.CurrentTime = data.Ops.First().Timestamp;
            _ws.CurrentZoneChanged += OnZoneChanged;

            foreach (var op in data.Ops.OfType<Replay.OpActorCombat>())
                if (_checkpoints.Count == 0 || (op.Timestamp - _checkpoints.Last().Item1).TotalSeconds > 5)
                    _checkpoints.Add((op.Timestamp, op.Value));
            _first = data.Ops.First().Timestamp;
            _last = data.Ops.Last().Timestamp;
        }

        public void Dispose()
        {
            _ws.CurrentZoneChanged -= OnZoneChanged;
        }

        public void Draw()
        {
            var curFrame = DateTime.Now;
            MoveTo(_ws.CurrentTime + (curFrame - _prevFrame) * _playSpeed);
            _prevFrame = curFrame;

            DrawControlRow();
            DrawTimelineRow();
            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);
            if (_bossmod != null)
            {
                _bossmod.Update();
                _bossmod.Draw(_azimuth / 180 * MathF.PI, _povSlot, null);

                ImGui.Text($"Downtime in: {_bossmod.StateMachine.EstimateTimeToNextDowntime():f2}, Positioning in: {_bossmod.StateMachine.EstimateTimeToNextPositioning():f2}, Components:");
                foreach (var comp in _bossmod.Components)
                {
                    ImGui.SameLine();
                    ImGui.Text(comp.GetType().Name);
                }
            }

            DrawPartyTable();
            DrawEnemyTables();
            DrawAllActorsTable();
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
            foreach ((var checkpoint, bool type) in _checkpoints)
            {
                var off = (float)((checkpoint - _first) / (_last - _first));
                var center = cursor + new Vector2(w * off, 0);
                dl.AddCircleFilled(center, 3, type ? 0xff00ff00 : 0xff0000ff);
                if (ClickedAt(center, 3))
                {
                    MoveTo(checkpoint);
                }
            }
            ImGui.Dummy(new(w, 8));
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
            if (_bossmod == null || !ImGui.CollapsingHeader("Party"))
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
            foreach ((int slot, var player) in _bossmod.Raid.WithSlot(true))
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
                var hints = _bossmod.CalculateHintsForRaidMember(slot, player);
                foreach ((var hint, bool risk) in hints)
                {
                    ImGui.TextColored(risk ? riskColor : safeColor, hint);
                    ImGui.SameLine();
                }

                ImGui.PopID();
            }
            ImGui.EndTable();
        }

        private void DrawEnemyTables()
        {
            if (_bossmod == null)
                return;

            var oidType = _bossmod.GetType().Module.GetType($"{_bossmod.GetType().Namespace}.OID");
            foreach ((var oid, var list) in _bossmod.RelevantEnemies)
            {
                var oidName = oidType?.GetEnumName(oid);
                if (!ImGui.CollapsingHeader($"Enemy {oid:X} {oidName ?? ""}") || list.Count == 0)
                    continue;

                ImGui.BeginTable($"enemy_{oid}", 6, ImGuiTableFlags.Resizable);
                ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
                ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
                ImGui.TableSetupColumn("Rot", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 100);
                ImGui.TableSetupColumn("Cast");
                ImGui.TableSetupColumn("Statuses");
                ImGui.TableHeadersRow();
                foreach (var enemy in list)
                {
                    ImGui.PushID((int)enemy.InstanceID);
                    ImGui.TableNextRow();
                    DrawCommonColumns(enemy);
                    ImGui.PopID();
                }
                ImGui.EndTable();
            }
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

        private void OnZoneChanged(object? sender, ushort zone)
        {
            _bossmod?.Dispose();
            _bossmod = ZoneModule.CreateModule(zone, _ws);
        }
    }
}
