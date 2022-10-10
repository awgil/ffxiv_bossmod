using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    // utility for drawing time-related data
    public class Timeline
    {
        // definition of a timeline column
        public abstract class Column
        {
            public bool Visible = true;
            public float Width;
            public string Name = "";
            protected Timeline Timeline;

            protected Column(Timeline timeline)
            {
                Timeline = timeline;
            }

            // called before layouting and drawing, good chance to update e.g. width and column name
            public virtual void Update() { }

            public abstract void Draw();
        }

        public float MaxTime = 0;
        public float? CurrentTime = null;
        public float PixelsPerSecond = 10;
        public float TopMargin = 20;
        public float BottomMargin = 5;

        private List<Column> _columns = new();
        private float _tScroll = 0;
        private float _tickFrequency = 5;
        private float _timeAxisWidth = 35;

        // these fields are transient and reinitialized on each draw
        private float _height = 0;
        private float _clientWidth = 0;
        private float _curColumnOffset = 0;
        private Vector2 _screenClientTL;
        private List<List<string>> _tooltip = new();
        public float MinVisibleTime => _tScroll;
        public float MaxVisibleTime => _tScroll + _height / PixelsPerSecond;

        public float Height => _height;
        public Vector2 ScreenClientTL => _screenClientTL;

        public T AddColumn<T>(T col) where T : Column
        {
            _columns.Add(col);
            return col;
        }

        public T AddColumnBefore<T>(T col, Column next) where T : Column
        {
            _columns.Insert(_columns.IndexOf(next), col);
            return col;
        }

        public void Draw()
        {
            _screenClientTL = ImGui.GetCursorScreenPos();
            _clientWidth = 0;
            foreach (var c in _columns)
            {
                c.Update();
                if (c.Visible)
                {
                    var s = ImGui.CalcTextSize(c.Name);
                    ImGui.GetWindowDrawList().AddText(_screenClientTL + new Vector2(_timeAxisWidth + _clientWidth + c.Width / 2 - s.X / 2, 0), 0xffffffff, c.Name);
                    _clientWidth += c.Width;
                }
            }

            _screenClientTL.Y += TopMargin;
            ImGui.SetCursorScreenPos(_screenClientTL);
            _screenClientTL.X += _timeAxisWidth;

            _height = MathF.Max(10, ImGui.GetWindowPos().Y + ImGui.GetWindowHeight() - _screenClientTL.Y - TopMargin - BottomMargin - 8);
            ImGui.InvisibleButton("canvas", new(_timeAxisWidth + _clientWidth, _height), ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);
            HandleScrollZoom();
            DrawTimeAxis();
            ImGui.PushClipRect(_screenClientTL, _screenClientTL + new Vector2(_clientWidth, _height), true);

            _curColumnOffset = 0;
            foreach (var c in _columns.Where(c => c.Visible))
            {
                c.Draw();
                _curColumnOffset += c.Width;
            }

            ImGui.PopClipRect();

            if (_tooltip.Count > 0)
            {
                ImGui.BeginTooltip();
                bool first = true;
                foreach (var strings in _tooltip)
                {
                    if (!first)
                        ImGui.Separator();
                    first = false;
                    foreach (var s in strings)
                        ImGui.TextUnformatted(s);
                }
                ImGui.EndTooltip();
                _tooltip.Clear();
            }
        }

        // API below is supposed to be called during column's Draw() function
        public void AddTooltip(List<string> strings)
        {
            _tooltip.Add(strings);
        }

        public void HighlightTime(float t)
        {
            ImGui.GetWindowDrawList().AddLine(CanvasCoordsToScreenCoords(0, t), CanvasCoordsToScreenCoords(_clientWidth, t), 0xffffffff);
        }

        public float TimeDeltaToScreenDelta(float dt) => dt * PixelsPerSecond;
        public float ScreenDeltaToTimeDelta(float dy) => dy / PixelsPerSecond;
        public float TimeToScreenCoord(float t) => _screenClientTL.Y + TimeDeltaToScreenDelta(t - _tScroll);
        public float ScreenCoordToTime(float y) => _tScroll + ScreenDeltaToTimeDelta(y - _screenClientTL.Y);
        public float ColumnOffsetToScreenCoord(float o) => _screenClientTL.X + _curColumnOffset + o;
        public float ScreenCoordToColumnOffset(float x) => x - _screenClientTL.X - _curColumnOffset;
        public float CanvasOffsetToScreenCoord(float o) => _screenClientTL.X + o;
        public float ScreenCoordToCanvasOffset(float x) => x - _screenClientTL.X;
        public Vector2 ColumnCoordsToScreenCoords(float offset, float t) => new(ColumnOffsetToScreenCoord(offset), TimeToScreenCoord(t));
        public Vector2 CanvasCoordsToScreenCoords(float offset, float t) => new(CanvasOffsetToScreenCoord(offset), TimeToScreenCoord(t));

        private void HandleScrollZoom()
        {
            // scroll by wheel, zoom by shift+wheel
            if (ImGui.IsItemHovered() && ImGui.GetIO().MouseWheel != 0)
            {
                if (ImGui.GetIO().KeyShift)
                {
                    var cursorT = ScreenCoordToTime(ImGui.GetMousePos().Y);
                    PixelsPerSecond *= MathF.Pow(1.05f, ImGui.GetIO().MouseWheel);
                    _tScroll += cursorT - ScreenCoordToTime(ImGui.GetMousePos().Y);

                    _tickFrequency = 5;
                    while (_tickFrequency < 60 && PixelsPerSecond * _tickFrequency < 30)
                        _tickFrequency *= 2;
                    while (_tickFrequency > 1 && PixelsPerSecond * _tickFrequency > 55)
                        _tickFrequency = MathF.Floor(_tickFrequency * 0.5f);
                    while (_tickFrequency > 0.1f && PixelsPerSecond * _tickFrequency > 55)
                        _tickFrequency = MathF.Floor(_tickFrequency * 5) * 0.1f;
                }
                else
                {
                    _tScroll -= ScreenDeltaToTimeDelta(70 * ImGui.GetIO().MouseWheel);
                }
            }

            // clamp to data range
            _tScroll = MathF.Min(_tScroll, MaxTime - _height / PixelsPerSecond);
            _tScroll = MathF.Max(_tScroll, 0);
        }

        private void DrawTimeAxis()
        {
            var maxT = Math.Min(MaxTime, _tScroll + _height / PixelsPerSecond);
            var drawlist = ImGui.GetWindowDrawList();
            drawlist.AddLine(_screenClientTL, CanvasCoordsToScreenCoords(0, maxT), 0xffffffff);
            for (float t = MathF.Ceiling(_tScroll / _tickFrequency) * _tickFrequency; t <= maxT; t += _tickFrequency)
            {
                string tickText = $"{t:f1}";
                var tickTextSize = ImGui.CalcTextSize(tickText);

                var p = CanvasCoordsToScreenCoords(0, t);
                drawlist.AddLine(p, p - new Vector2(3, 0), 0xffffffff);
                drawlist.AddText(p - new Vector2(tickTextSize.X + 5, tickTextSize.Y / 2), 0xffffffff, tickText);
            }

            if (CurrentTime != null && CurrentTime.Value >= _tScroll && CurrentTime.Value <= maxT)
            {
                // draw timeline mark
                var p = CanvasCoordsToScreenCoords(0, CurrentTime.Value);
                drawlist.AddTriangleFilled(p, p - new Vector2(4, 2), p - new Vector2(4, -2), 0xffffffff);
            }
        }
    }
}
