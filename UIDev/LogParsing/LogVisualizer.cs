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
    class LogVisualizer : IDisposable
    {
        private WorldStateLogParser _data;
        private WorldState _ws = new();
        private DateTime _cur;
        private int _cursor = 0;
        private List<(DateTime, bool)> _checkpoints = new();
        private DateTime _first;
        private DateTime _last;
        private DateTime _prevFrame;
        private float _playSpeed = 0;
        private BossModule? _bossmod;
        private float _azimuth;

        public LogVisualizer(WorldStateLogParser data)
        {
            _data = data;
            _cur = data.Ops.First().Timestamp;

            foreach (var op in data.Ops.OfType<WorldStateLogParser.OpEnterExitCombat>())
                _checkpoints.Add((op.Timestamp, op.Value));
            _first = data.Ops.First().Timestamp;
            _last = data.Ops.Last().Timestamp;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            var curFrame = DateTime.Now;
            MoveTo(_cur + (curFrame - _prevFrame) * _playSpeed);
            _prevFrame = curFrame;

            DrawControlRow();
            DrawTimelineRow();
            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);
            if (_bossmod != null)
            {
                _bossmod.Update();
                _bossmod.Draw(_azimuth / 180 * MathF.PI, null);
            }

            var riskColor = ImGui.ColorConvertU32ToFloat4(0xff00ffff);
            var safeColor = ImGui.ColorConvertU32ToFloat4(0xff00ff00);
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
                ImGui.SameLine();
                ImGui.Text($"{actor.Name} ({actor.OID:X})");
                if (actor.CastInfo != null)
                {
                    ImGui.SameLine();
                    ImGui.Text($"Casting {actor.CastInfo.Action}");
                }

                foreach (var s in actor.Statuses.Where(s => s.ID != 0))
                {
                    var src = _ws.FindActor(s.SourceID);
                    if (src?.Type == WorldState.ActorType.Player || src?.Type == WorldState.ActorType.Pet)
                        continue;
                    ImGui.SameLine();
                    ImGui.Text($"Status {s.ID} ({s.Extra})");
                }

                if (_bossmod != null && actor.Type == WorldState.ActorType.Player)
                {
                    int slot = _bossmod.FindRaidMemberSlot(actor.InstanceID);
                    if (slot >= 0)
                    {
                        var hints = _bossmod.CalculateHintsForRaidMember(slot, actor);
                        foreach ((var hint, bool risk) in hints)
                        {
                            ImGui.SameLine();
                            ImGui.TextColored(risk ? riskColor : safeColor, hint);
                        }
                    }
                }
            }
        }

        private void DrawControlRow()
        {
            if (ImGui.Button("<<<"))
                _playSpeed = -5;
            ImGui.SameLine();
            if (ImGui.Button("<<"))
                _playSpeed = -1;
            ImGui.SameLine();
            if (ImGui.Button("<"))
                _playSpeed = -0.2f;
            ImGui.SameLine();
            if (ImGui.Button("||"))
            {
                if (_playSpeed == 0)
                {
                    _playSpeed = 1;
                    if (_bossmod != null)
                        _bossmod.StateMachine.Paused = false;
                }
                else
                {
                    _playSpeed = 0;
                    if (_bossmod != null)
                        _bossmod.StateMachine.Paused = true;
                }
            }
            ImGui.SameLine();
            if (ImGui.Button(">"))
                _playSpeed = 0.2f;
            ImGui.SameLine();
            if (ImGui.Button(">>"))
                _playSpeed = 1;
            ImGui.SameLine();
            if (ImGui.Button(">>>"))
                _playSpeed = 5;

            ImGui.SameLine();
            if (ImGui.Button("P4S"))
                ActivateBossMod<P4S>();
        }

        private void DrawTimelineRow()
        {
            var dl = ImGui.GetWindowDrawList();
            var cursor = ImGui.GetCursorScreenPos();
            var w = ImGui.GetWindowWidth() - 2 * ImGui.GetCursorPosX() - 15;
            cursor.Y += 4;
            dl.AddLine(cursor, cursor + new Vector2(w, 0), 0xff00ffff);

            var curp = cursor + new Vector2(w * (float)((_cur - _first) / (_last - _first)), 0);
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

        private void MoveTo(DateTime t)
        {
            if (t > _cur)
            {
                while (_cursor < _data.Ops.Count && t > _data.Ops[_cursor].Timestamp)
                    _data.Ops[_cursor++].Redo(_ws);
            }
            else if (t < _cur)
            {
                while (_cursor > 0 && t <= _data.Ops[_cursor - 1].Timestamp)
                    _data.Ops[--_cursor].Undo(_ws);
            }
            _cur = t;
        }

        private void ActivateBossMod<T>() where T : BossModule
        {
            _bossmod?.Dispose();
            _bossmod = (BossModule?)Activator.CreateInstance(typeof(T), _ws);
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
