using BossMod;
using ImGuiNET;

namespace UIDev;

class IntersectionTest : TestWindow
{
    private float _cirleOffset = 5.5f;
    private float _circleDir = 0;
    private float _circleRadius = 0.5f;
    private float _coneRadius = 5;
    private float _coneHalfAngleDeg = 30;
    private float _coneDirDeg = 90;
    private readonly MiniArena _arena = new(new(), default, new ArenaBoundsSquare(10));

    public IntersectionTest() : base("Intersection test", new(400, 400), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
    }

    public override void Draw()
    {
        ImGui.DragFloat("Circle offset", ref _cirleOffset, 0.1f, 0, 10);
        ImGui.DragFloat("Circle dir", ref _circleDir, 1, -180, 180);
        ImGui.DragFloat("Circle radius", ref _circleRadius, 0.1f, 0.1f, 5);
        ImGui.DragFloat("Cone radius", ref _coneRadius, 0.1f, 0.1f, 10);
        ImGui.DragFloat("Cone half-angle", ref _coneHalfAngleDeg, 1, 0, 180);
        ImGui.DragFloat("Cone dir", ref _coneDirDeg, 1, -180, 180);
        var circleCenter = _cirleOffset * _circleDir.Degrees().ToDirection();
        var intersect = Intersect.CircleCone(circleCenter, _circleRadius, _coneRadius, _coneDirDeg.Degrees().ToDirection(), _coneHalfAngleDeg.Degrees());
        ImGui.TextUnformatted($"Intersect: {intersect}");

        _arena.Begin(default);
        _arena.AddCone(default, _coneRadius, _coneDirDeg.Degrees(), _coneHalfAngleDeg.Degrees(), ArenaColor.Safe);
        _arena.AddCircle(circleCenter.ToWPos(), _circleRadius, ArenaColor.Danger);
        _arena.End();
    }
}
