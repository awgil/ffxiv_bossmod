using BossMod;
using ImGuiNET;

namespace UIDev;

class ComplexBoundsTest : TestWindow
{
    private MiniArena _arena;
    private float _azimuth = -72;
    private float _altitude = 90;
    private float _boundsApproxError = 0.5f;
    private float _innerPlatformRadius = 21;
    private float _outerPlatformRadius = 28;
    private float _platformCenterOffset = 15;
    private float _aoeRadius = 20;
    private float _aoeOffset = 10;
    private float _aoeRotDeg;
    private bool _evenOdd;
    private bool _fillBG;

    public ComplexBoundsTest() : base("Complex bounds test", new(800, 800), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) => _arena = RebuildArena();

    public override void Draw()
    {
        ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, +180);
        ImGui.DragFloat("Camera altitude", ref _altitude, 1, -90, +90);

        _arena.Begin((float)(Math.PI * _azimuth / 180));
        if (_fillBG)
            _arena.Zone(_arena.Bounds.ShapeTriangulation, 0xff402010);
        _arena.Border(ArenaColor.Border);

        var t1 = DateTime.Now;
        _arena.ZoneCircle(_arena.Center + _aoeOffset * _aoeRotDeg.Degrees().ToDirection(), _aoeRadius, ArenaColor.AOE);
        var dt = DateTime.Now - t1;

        _arena.End();

        ImGui.TextUnformatted($"AOE draw perf: {dt.TotalMilliseconds:f3}ms");

        // arena config
        if (ImGui.DragFloat("Bounds approximation error", ref _boundsApproxError, 0.01f, 0.001f, 10, "%.4f", ImGuiSliderFlags.Logarithmic))
            _arena = RebuildArena();
        if (ImGui.DragFloat("Platform inner radius", ref _innerPlatformRadius, 0.1f))
            _arena = RebuildArena();
        if (ImGui.DragFloat("Platform outer radius", ref _outerPlatformRadius, 0.1f))
            _arena = RebuildArena();
        if (ImGui.DragFloat("Platform center offset", ref _platformCenterOffset, 0.1f))
            _arena = RebuildArena();
        if (ImGui.Checkbox("Even-odd fill", ref _evenOdd))
            _arena = RebuildArena();

        // aoe config
        ImGui.Checkbox("Fill background", ref _fillBG);
        ImGui.DragFloat("AOE radius", ref _aoeRadius, 0.1f);
        ImGui.DragFloat("AOE offset", ref _aoeOffset, 0.1f);
        ImGui.DragFloat("AOE rotation", ref _aoeRotDeg, 1, -180, +180);
    }

    private MiniArena RebuildArena()
    {
        var shape = new PolygonClipper.Operand();
        AddPlatform(shape, _platformCenterOffset * 0.Degrees().ToDirection());
        AddPlatform(shape, _platformCenterOffset * 120.Degrees().ToDirection());
        AddPlatform(shape, _platformCenterOffset * 240.Degrees().ToDirection());
        var bounds = new PolygonClipper().Simplify(shape, _evenOdd ? Clipper2Lib.FillRule.EvenOdd : Clipper2Lib.FillRule.NonZero);
        return new(new(), new(100, 100), new ArenaBoundsCustom(40, bounds));
    }

    private void AddPlatform(PolygonClipper.Operand poly, WDir centerOffset)
    {
        if (_innerPlatformRadius > 0 && _innerPlatformRadius < _outerPlatformRadius)
            poly.AddContour(CurveApprox.Donut(_innerPlatformRadius, _outerPlatformRadius, _boundsApproxError).Select(p => p + centerOffset));
        else
            poly.AddContour(CurveApprox.Circle(_outerPlatformRadius, _boundsApproxError).Select(p => p + centerOffset));
    }
}
