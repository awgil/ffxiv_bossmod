using BossMod;
using Dalamud.Bindings.ImGui;

namespace UIDev;

class IntersectionTest : TestWindow
{
    private float _circleOffset = 5.5f;
    private float _circleDir = 0;
    private float _circleRadius = 0.5f;
    private float _shapeExtentPrimary = 5; // z/r
    private float _shapeExtentSecondary = 30; // x/phi (deg)
    private float _shapeDirDeg = 90;
    private float _coneInner;
    private bool _shapeIsRect;
    private readonly MiniArena _arena = new(new(), default, new ArenaBoundsSquare(10));

    public IntersectionTest() : base("Intersection test", new(400, 400), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
    }

    public override void Draw()
    {
        ImGui.DragFloat("Circle offset", ref _circleOffset, 0.1f, 0, 10);
        ImGui.DragFloat("Circle dir", ref _circleDir, 1, -180, 180);
        ImGui.DragFloat("Circle radius", ref _circleRadius, 0.1f, 0.1f, 5);
        ImGui.Checkbox("Intersect with rect", ref _shapeIsRect);
        if (_shapeIsRect)
        {
            ImGui.DragFloat("Rect half-Z", ref _shapeExtentPrimary, 0.1f, 0.1f, 10);
            ImGui.DragFloat("Rect half-X", ref _shapeExtentSecondary, 0.1f, 0.1f, 30);
            ImGui.DragFloat("Rect dir", ref _shapeDirDeg, 1, -180, 180);
        }
        else
        {
            ImGui.DragFloat("Cone radius (inner)", ref _coneInner, 0.1f, 0, 9.9f);
            ImGui.DragFloat("Cone radius (outer)", ref _shapeExtentPrimary, 0.1f, 0.1f, 10);
            ImGui.DragFloat("Cone half-angle", ref _shapeExtentSecondary, 1, 0, 180);
            ImGui.DragFloat("Cone dir", ref _shapeDirDeg, 1, -180, 180);
        }
        var circleCenter = _circleOffset * _circleDir.Degrees().ToDirection();
        var intersect = _shapeIsRect
            ? Intersect.CircleRect(circleCenter, _circleRadius, _shapeDirDeg.Degrees().ToDirection(), _shapeExtentSecondary, _shapeExtentPrimary)
            : Intersect.CircleDonutSector(circleCenter, _circleRadius, _coneInner, _shapeExtentPrimary, _shapeDirDeg.Degrees().ToDirection(), _shapeExtentSecondary.Degrees());
        ImGui.TextUnformatted($"Intersect: {intersect}");

        _arena.Begin(default);
        if (_shapeIsRect)
            _arena.AddRect(default, _shapeDirDeg.Degrees().ToDirection(), _shapeExtentPrimary, _shapeExtentPrimary, _shapeExtentSecondary, ArenaColor.Safe);
        else
            _arena.AddDonutCone(default, _coneInner, _shapeExtentPrimary, _shapeDirDeg.Degrees(), _shapeExtentSecondary.Degrees(), ArenaColor.Safe);
        _arena.AddCircle(circleCenter.ToWPos(), _circleRadius, ArenaColor.Danger);
        _arena.End();
    }
}
