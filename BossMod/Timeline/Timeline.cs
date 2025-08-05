using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace BossMod;

// utility for drawing time-related data
public class Timeline
{
    // definition of a timeline column
    public class Column(Timeline timeline)
    {
        public float Width;
        public string Name = "";
        public Timeline Timeline { get; private init; } = timeline;

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
    public class ColumnGroup(Timeline timeline) : Column(timeline)
    {
        public List<Column> Columns = [];

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
            return Columns[^1];
        }
    }

    public readonly ColorConfig Colors = Service.Config.Get<ColorConfig>();
    public float MinTime;
    public float MaxTime;
    public float? CurrentTime;
    public float PixelsPerSecond = 10 * ImGuiHelpers.GlobalScale;
    public float TopMargin = 20 * ImGuiHelpers.GlobalScale;
    public float BottomMargin = 5 * ImGuiHelpers.GlobalScale;
    public ColumnGroup Columns;

    private float _tickFrequency = 5;
    private readonly float _timeAxisWidth = 35 * ImGuiHelpers.GlobalScale;

    // these fields are transient and reinitialized on each draw
    private float _curColumnOffset;
    private Vector2 _screenClientTL;
    private readonly List<List<string>> _tooltip = [];
    private readonly List<(float t, uint color)> _highlightTime = [];
    public float MinVisibleTime { get; private set; }
    public float MaxVisibleTime => MinVisibleTime + Height / PixelsPerSecond;

    public float Height { get; private set; }
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

        Height = MathF.Max(10, ImGui.GetWindowPos().Y + ImGui.GetWindowHeight() - _screenClientTL.Y - TopMargin - BottomMargin - 8);
        ImGui.InvisibleButton("canvas", new(_timeAxisWidth + Columns.Width, Height), ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);
        HandleScrollZoom();
        DrawTimeAxis();
        ImGui.PushClipRect(_screenClientTL, _screenClientTL + new Vector2(Columns.Width, Height), true);

        _curColumnOffset = 0;
        Columns.DrawAdvance(ref _curColumnOffset);

        // cursor lines
        foreach (var h in _highlightTime)
            ImGui.GetWindowDrawList().AddLine(CanvasCoordsToScreenCoords(0, h.t), CanvasCoordsToScreenCoords(Columns.Width, h.t), h.color);
        _highlightTime.Clear();

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
    public void AddTooltip(List<string> strings) => _tooltip.Add(strings);
    public void HighlightTime(float t, uint color = 0xffffffff) => _highlightTime.Add((t, color));

    public float TimeDeltaToScreenDelta(float dt) => dt * PixelsPerSecond;
    public float ScreenDeltaToTimeDelta(float dy) => dy / PixelsPerSecond;
    public float TimeToScreenCoord(float t) => _screenClientTL.Y + TimeDeltaToScreenDelta(t - MinVisibleTime);
    public float ScreenCoordToTime(float y) => MinVisibleTime + ScreenDeltaToTimeDelta(y - _screenClientTL.Y);
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
                MinVisibleTime += cursorT - ScreenCoordToTime(ImGui.GetMousePos().Y);

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
                MinVisibleTime -= ScreenDeltaToTimeDelta(70 * ImGui.GetIO().MouseWheel);
            }
        }

        // clamp to data range
        MinVisibleTime = MathF.Min(MinVisibleTime, MaxTime - Height / PixelsPerSecond);
        MinVisibleTime = MathF.Max(MinVisibleTime, MinTime);
    }

    private void DrawTimeAxis()
    {
        var maxT = Math.Min(MaxTime, MinVisibleTime + Height / PixelsPerSecond);
        var drawlist = ImGui.GetWindowDrawList();
        drawlist.AddLine(_screenClientTL, CanvasCoordsToScreenCoords(0, maxT), 0xffffffff);
        for (float t = MathF.Ceiling(MinVisibleTime / _tickFrequency) * _tickFrequency; t <= maxT; t += _tickFrequency)
        {
            string tickText = $"{t:f1}";
            var tickTextSize = ImGui.CalcTextSize(tickText);

            var p = CanvasCoordsToScreenCoords(0, t);
            drawlist.AddLine(p, p - new Vector2(3, 0), 0xffffffff);
            drawlist.AddText(p - new Vector2(tickTextSize.X + 5, tickTextSize.Y / 2), 0xffffffff, tickText);
        }

        if (CurrentTime != null)
        {
            var p = CanvasCoordsToScreenCoords(0, CurrentTime.Value);
            if (CurrentTime.Value >= MinVisibleTime && CurrentTime.Value <= maxT)
            {
                // draw timeline mark
                drawlist.AddTriangleFilled(p, p - new Vector2(4, 2), p - new Vector2(4, -2), 0xffffffff);
            }

            if (ImGui.IsWindowFocused() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                // change current time, so that listeners could react
                var pos = ImGui.GetMousePos();
                if (Math.Abs(pos.X - p.X) <= 3 && pos.Y >= _screenClientTL.Y && pos.Y <= _screenClientTL.Y + Height)
                {
                    CurrentTime = ScreenCoordToTime(pos.Y);
                }
            }
        }
    }
}
