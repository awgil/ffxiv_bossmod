using BossMod;
using ImGuiNET;

namespace UIDev;

class ShrinkTest : TestWindow
{
    private readonly List<WDir> _points = [];
    private readonly List<WDir> _innerPoints = [];

    private Vector2 _canvasOrigin = new();

    public ShrinkTest() : base("Poly shrink test", new(800, 800), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        _points = [new(155, 297), new(326, 297), new(381, 297)];
    }

    public override void Draw()
    {
        if (ImGui.Button("Reset"))
        {
            _points.Clear();
            _innerPoints.Clear();
        }

        _canvasOrigin = ImGui.GetCursorScreenPos();

        ImGui.Dummy(new Vector2(600, 600));

        if (ImGui.IsItemClicked())
        {
            _points.Add(new(ImGui.GetMousePos() - _canvasOrigin));
            _innerPoints.Clear();
            if (_points.Count > 2)
            {
                try
                {
                    _innerPoints.AddRange(ShrinkPoly.Shrink(_points, 10));
                }
                catch (InvalidOperationException ex)
                {
                    Service.Log($"malformed polygon: {ex}");
                }
            }
        }

        switch (_points.Count)
        {
            case 0:
                return;
            case 1:
                DrawCircle(_points[0]);
                break;
            case 2:
                DrawLine(_points[0], _points[1]);
                break;
            default:
                for (var i = 0; i < _points.Count - 1; i++)
                    DrawLine(_points[i], _points[i + 1]);
                DrawLine(_points[^1], _points[0]);

                break;
        }

        foreach (var p in _points)
            ImGui.TextUnformatted(p.ToString());
        foreach (var i in _innerPoints)
            ImGui.TextUnformatted(i.ToString());

        if (_innerPoints.Count > 2)
        {
            for (var i = 0; i < _innerPoints.Count - 1; i++)
                DrawLine(_innerPoints[i], _innerPoints[i + 1], 0xFF00FFFF);
            DrawLine(_innerPoints[^1], _innerPoints[0], 0xFF00FFFF);
        }
    }

    private void DrawLine(WDir start, WDir end, uint color = 0xFF00FF00)
    {
        ImGui.GetWindowDrawList().AddLine(_canvasOrigin + start.ToVec2(), _canvasOrigin + end.ToVec2(), color);
    }
    private void DrawCircle(WDir start) => ImGui.GetWindowDrawList().AddCircleFilled(_canvasOrigin + start.ToVec2(), 10, 0xFF00FF00);
}
