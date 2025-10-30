using Dalamud.Bindings.ImGui;

namespace BossMod;

// utility for drawing 2d plots
public class UIPlot
{
    public float PointRadius = 2;
    public Vector2 Margin = new(40, 20);
    public Vector2 MainAreaSize = new(500, 300);
    public Vector2 DataMin = new(0, 0);
    public Vector2 DataMax = new(1, 1);
    public Vector2 VisMin = new(float.MinValue, float.MinValue);
    public Vector2 VisMax = new(float.MaxValue, float.MaxValue);
    public Vector2 TickOrigin = new(0, 0);
    public Vector2 TickAdvance = new(0.1f, 0.2f);

    private readonly List<string> _tooltips = [];
    private Vector2 _blScreen;
    private readonly uint _colCursor = 0x80808080;
    private readonly uint _colAxis = 0xffffffff;

    public void Begin()
    {
        var cursor = ImGui.GetCursorScreenPos();
        _blScreen = cursor + Margin;
        _blScreen.Y += MainAreaSize.Y;
        ImGui.InvisibleButton("canvas", MainAreaSize + 2 * Margin, ImGuiButtonFlags.MouseButtonLeft);

        var dl = ImGui.GetWindowDrawList();
        dl.AddLine(_blScreen, _blScreen + new Vector2(MainAreaSize.X, 0), _colAxis);
        dl.AddLine(_blScreen, _blScreen - new Vector2(0, MainAreaSize.Y), _colAxis);

        VisMin.X = Math.Max(VisMin.X, DataMin.X);
        VisMin.Y = Math.Max(VisMin.Y, DataMin.Y);
        VisMax.X = Math.Min(VisMax.X, DataMax.X);
        VisMax.Y = Math.Min(VisMax.Y, DataMax.Y);

        var mousePos = ImGui.GetIO().MousePos;
        if (mousePos.X >= _blScreen.X && mousePos.X <= _blScreen.X + MainAreaSize.X && mousePos.Y <= _blScreen.Y && mousePos.Y >= _blScreen.Y - MainAreaSize.Y)
        {
            dl.AddLine(new(_blScreen.X, mousePos.Y), new(_blScreen.X + MainAreaSize.X, mousePos.Y), _colCursor);
            dl.AddLine(new(mousePos.X, _blScreen.Y), new(mousePos.X, _blScreen.Y - MainAreaSize.Y), _colCursor);

            var wheel = ImGui.GetIO().MouseWheel;
            if (wheel != 0)
            {
                var k = MathF.Pow(1.05f, -wheel);
                var visRange = VisMax - VisMin;
                var center = FromScreen(mousePos);
                var newMin = k * VisMin + (1 - k) * center;
                var newMax = newMin + k * visRange;
                if (ImGui.GetIO().KeyShift)
                {
                    VisMin.X = Math.Max(newMin.X, DataMin.X);
                    VisMax.X = Math.Min(newMax.X, DataMax.X);
                }
                else
                {
                    VisMin.Y = Math.Max(newMin.Y, DataMin.Y);
                    VisMax.Y = Math.Min(newMax.Y, DataMax.Y);
                }
                ImGui.GetIO().MouseWheel = 0;
            }

            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                var visRange = VisMax - VisMin;
                var delta = ImGui.GetIO().MouseDelta / MainAreaSize * visRange;
                if (delta.X > 0)
                {
                    VisMin.X = Math.Max(VisMin.X - delta.X, DataMin.X);
                    VisMax.X = VisMin.X + visRange.X;
                }
                else if (delta.X < 0)
                {
                    VisMax.X = Math.Min(VisMax.X - delta.X, DataMax.X);
                    VisMin.X = VisMax.X - visRange.X;
                }
                if (delta.Y > 0)
                {
                    VisMax.Y = Math.Min(VisMax.Y + delta.Y, DataMax.Y);
                    VisMin.Y = VisMax.Y - visRange.Y;
                }
                else if (delta.Y < 0)
                {
                    VisMin.Y = Math.Max(VisMin.Y + delta.Y, DataMin.Y);
                    VisMax.Y = VisMin.Y + visRange.Y;
                }
            }
        }

        var minTick = (VisMin - TickOrigin) / TickAdvance;
        var maxTick = (VisMax - TickOrigin) / TickAdvance;
        for (float x = MathF.Ceiling(minTick.X), xmax = MathF.Floor(maxTick.X); x <= xmax; ++x)
            DrawTickX(dl, TickOrigin.X + x * TickAdvance.X);
        for (float y = MathF.Ceiling(minTick.Y), ymax = MathF.Floor(maxTick.Y); y <= ymax; ++y)
            DrawTickY(dl, TickOrigin.Y + y * TickAdvance.Y);

        _tooltips.Clear();
    }

    public void End()
    {
        if (_tooltips.Count > 0)
        {
            ImGui.BeginTooltip();
            foreach (var s in _tooltips)
                ImGui.TextUnformatted(s);
            ImGui.EndTooltip();
        }
    }

    public void Point(Vector2 p, uint color, Func<string> tooltip)
    {
        if (p.X < VisMin.X || p.Y < VisMin.Y || p.X > VisMax.X || p.Y > VisMax.Y)
            return;

        var screen = ToScreen(p);
        ImGui.GetWindowDrawList().AddCircleFilled(screen, PointRadius, color);
        if (ImGui.IsMouseHoveringRect(screen - new Vector2(PointRadius), screen + new Vector2(PointRadius)))
            _tooltips.Add($"[{p.X:f2}, {p.Y:f2}]: {tooltip()}");
    }

    private Vector2 ToScreen(Vector2 p)
    {
        var rel = (p - VisMin) / (VisMax - VisMin);
        rel.Y = -rel.Y;
        return _blScreen + rel * MainAreaSize;
    }

    private Vector2 FromScreen(Vector2 p)
    {
        var rel = (p - _blScreen) / MainAreaSize;
        rel.Y = -rel.Y;
        return VisMin + rel * (VisMax - VisMin);
    }

    private void DrawTickX(ImDrawListPtr dl, float x)
    {
        var s = ToScreen(new(x, VisMin.Y));
        dl.AddLine(s, s + new Vector2(0, 7), _colAxis);

        var text = $"{x:f1}";
        var size = ImGui.CalcTextSize(text);
        dl.AddText(s + new Vector2(-size.X / 2, Margin.Y - size.Y), _colAxis, text);
    }

    private void DrawTickY(ImDrawListPtr dl, float y)
    {
        var s = ToScreen(new(VisMin.X, y));
        dl.AddLine(s, s - new Vector2(5, 0), _colAxis);

        var text = $"{y:f1}";
        var size = ImGui.CalcTextSize(text);
        dl.AddText(s - new Vector2(5 + size.X, size.Y / 2), _colAxis, text);
    }
}
