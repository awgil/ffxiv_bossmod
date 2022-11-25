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
        public class Column
        {
            public float Width;
            public string Name = "";
            public Timeline Timeline { get; private init; }

            public Column(Timeline timeline)
            {
                Timeline = timeline;
            }

            // called before layouting and drawing, good chance to update e.g. width
            public virtual void Update() { }

            public virtual void DrawHeader(Vector2 topLeft)
            {
                var s = ImGui.CalcTextSize(Name);
                ImGui.GetWindowDrawList().AddText(topLeft + new Vector2((Width - s.X) * 0.5f, 0), 0xffffffff, Name);
            }

            public virtual void Draw() { }

            // this version is used by column group to advance cursor
            public virtual void DrawAdvance(ref float x)
            {
                Draw();
                x += Width;
            }
        }

        // a number of consecutive columns grouped together
        // if a column has a name, it won't draw subcolumn names
        public class ColumnGroup : Column
        {
            public List<Column> Columns = new();

            public ColumnGroup(Timeline timeline) : base(timeline) { }

            public override void Update()
            {
                Width = 0;
                foreach (var c in Columns)
                {
                    c.Update();
                    Width += c.Width;
                }
            }

            public override void DrawHeader(Vector2 topLeft)
            {
                if (Name.Length > 0)
                {
                    base.DrawHeader(topLeft);
                }
                else
                {
                    foreach (var c in Columns.Where(c => c.Width > 0))
                    {
                        c.DrawHeader(topLeft);
                        topLeft.X += c.Width;
                    }
                }
            }

            public override void DrawAdvance(ref float x)
            {
                Draw(); // in case someone overrides this...
                foreach (var c in Columns.Where(c => c.Width > 0))
                    c.DrawAdvance(ref x);
            }

            public T Add<T>(T col) where T : Column
            {
                Columns.Add(col);
                return col;
            }

            public T AddBefore<T>(T col, Column next) where T : Column
            {
                Columns.Insert(Columns.IndexOf(next), col);
                return col;
            }

            public Column AddDummy()
            {
                Columns.Add(new(Timeline));
                return Columns.Last();
            }
        }

        public float MinTime = 0;
        public float MaxTime = 0;
        public float? CurrentTime = null;
        public float PixelsPerSecond = 10;
        public float TopMargin = 20;
        public float BottomMargin = 5;
        public ColumnGroup Columns;

        private float _tScroll = 0;
        private float _tickFrequency = 5;
        private float _timeAxisWidth = 35;

        // these fields are transient and reinitialized on each draw
        private float _height = 0;
        private float _curColumnOffset = 0;
        private Vector2 _screenClientTL;
        private List<List<string>> _tooltip = new();
        public float MinVisibleTime => _tScroll;
        public float MaxVisibleTime => _tScroll + _height / PixelsPerSecond;

        public float Height => _height;
        public Vector2 ScreenClientTL => _screenClientTL;

        public Timeline()
        {
            Columns = new(this);
        }

        public void Draw()
        {
            Columns.Update();

            _screenClientTL = ImGui.GetCursorScreenPos();
            Columns.DrawHeader(_screenClientTL + new Vector2(_timeAxisWidth, 0));

            _screenClientTL.Y += TopMargin;
            ImGui.SetCursorScreenPos(_screenClientTL);
            _screenClientTL.X += _timeAxisWidth;

            _height = MathF.Max(10, ImGui.GetWindowPos().Y + ImGui.GetWindowHeight() - _screenClientTL.Y - TopMargin - BottomMargin - 8);
            ImGui.InvisibleButton("canvas", new(_timeAxisWidth + Columns.Width, _height), ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);
            HandleScrollZoom();
            DrawTimeAxis();
            ImGui.PushClipRect(_screenClientTL, _screenClientTL + new Vector2(Columns.Width, _height), true);

            _curColumnOffset = 0;
            Columns.DrawAdvance(ref _curColumnOffset);

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
            ImGui.GetWindowDrawList().AddLine(CanvasCoordsToScreenCoords(0, t), CanvasCoordsToScreenCoords(Columns.Width, t), 0xffffffff);
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
            _tScroll = MathF.Max(_tScroll, MinTime);
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
