using BossMod;
using Dalamud.Bindings.ImGui;

namespace UIDev;

class CanvasTest : TestWindow
{
    private Vector2 _screenSize = new(300, 300);
    private WPos _start = new(5, 150);
    private WPos _goal = new(295, 150);
    private readonly List<List<WPos>> _contours = [];

    List<WPos>? _mouseoverContour;
    int _mouseoverIndex;
    bool _mouseoverPoint;

    public CanvasTest() : base("Canvas test", new(400, 400), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        _contours.Add([
            new(30, 100),
            new(10, 50),
            new(200, 50),
            new(220, 250),
            new(170, 250),
            new(150, 130),
            new(130, 130),
            new(100, 250),
            new(10, 250),
        ]);
        _contours.Add([new(140, 170), new(160, 280), new(120, 280)]);
        _contours.Add([new(240, 140), new(260, 140), new(260, 160), new(240, 160)]);
    }

    public override void Draw()
    {
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton("canvas", _screenSize, ImGuiButtonFlags.MouseButtonLeft);

        var dl = ImGui.GetWindowDrawList();
        var restoreFlags = dl.Flags;
        dl.Flags &= ~ImDrawListFlags.AntiAliasedLines;
        dl.AddRect(cursor, cursor + _screenSize, 0xffffffff);

        var mousePos = new WPos(ImGui.GetIO().MousePos - cursor);
        bool dragging = ImGui.IsMouseDragging(ImGuiMouseButton.Left);
        if (!dragging)
        {
            _mouseoverContour = null;
            _mouseoverIndex = 0;
            _mouseoverPoint = false;
        }

        foreach (var c in _contours)
        {
            var prev = c[^1];
            int i = 0;
            foreach (var next in c)
            {
                dl.PathLineTo(cursor + next.ToVec2());

                if (!dragging && next.AlmostEqual(mousePos, 4))
                {
                    _mouseoverContour = c;
                    _mouseoverIndex = i;
                    _mouseoverPoint = true;
                }

                if (!dragging && !_mouseoverPoint && DistancePointToEdge(prev, next, mousePos) < 4)
                {
                    _mouseoverContour = c;
                    _mouseoverIndex = i;
                }

                prev = next;
                ++i;
            }
            dl.PathStroke(0xffffffff, ImDrawFlags.Closed);
        }

        if (_mouseoverContour != null)
        {
            if (_mouseoverPoint)
            {
                dl.AddCircle(cursor + _mouseoverContour[_mouseoverIndex].ToVec2(), 5, 0xffffffff);
                if (dragging)
                {
                    _mouseoverContour[_mouseoverIndex] += new WDir(ImGui.GetIO().MouseDelta);
                }
            }
            else
            {
                dl.AddLine(cursor + _mouseoverContour[_mouseoverIndex > 0 ? _mouseoverIndex - 1 : _mouseoverContour.Count - 1].ToVec2(), cursor + _mouseoverContour[_mouseoverIndex].ToVec2(), 0xffffffff, 5);
                if (dragging)
                {
                    _mouseoverContour.Insert(_mouseoverIndex, mousePos);
                    _mouseoverPoint = true;
                }
            }
        }

        //var clip = new Clip2D();
        //var tree = clip.Simplify(_contours);
        //var path = Pathfind(tree, _start, _goal);

        dl.AddLine(cursor + _start.ToVec2(), cursor + _goal.ToVec2(), 0xff00ff00);
        dl.Flags = restoreFlags;
    }

    //private List<WPos> Pathfind(ClipperLib.PolyTree tree, WPos from, WPos to)
    //{
    //}

    private float DistancePointToEdge(WPos a, WPos b, WPos p)
    {
        var l = b - a;
        var len = (b - a).Length();
        l /= len;

        var proj = (p - a).Dot(l);
        var m = a + Math.Clamp(proj, 0, len) * l;
        return (p - m).Length();
    }
}
