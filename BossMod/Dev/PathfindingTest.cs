using BossMod.Pathfinding;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using Vector2 = System.Numerics.Vector2;

namespace BossMod.Dev;

class PathfindingTest : TestWindow
{
    private MapVisualizer _visu;

    private bool _useNewRasterizer;

    private float _mapResolution = 1;
    private float _mapThreshold = 0.5f;
    private Vector2 _mapCenter;
    private Vector2 _mapHalfSize = new(20, 20);
    private float _mapRotationDeg;
    private float _cushion;
    private float _ping;

    private Vector2 _startingPos = new(15, 0);
    private Vector2 _targetPos = new(-15, 0);
    private float _targetRadius = 5;
    private float _targetFacingDeg;

    private bool _blockCone = true;
    private Vector2 _blockConeCenter = new(0, 0);
    private Vector2 _blockConeRadius = new(11, 15);
    private float _blockConeRotationDeg = 90;
    private float _blockConeHalfAngle = 90;
    private float _blockConeG = 20;

    private bool _blockRect = true;
    private Vector2 _blockRectCenter = new(0, 0);
    private Vector2 _blockRectLen = new(20, 20);
    private float _blockRectHalfWidth = 5;
    private float _blockRectRotationDeg;
    private float _blockRectG = 4;

    bool _antialiased;

    private readonly D2DDraw _draw = new();
    private IDalamudTextureWrap _result = Service.Texture.CreateEmpty(RawImageSpecification.Bgra32(1, 1), false, false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _draw.Dispose();
            _result.Dispose();
        }

        base.Dispose(disposing);
    }

    public PathfindingTest() : base("Pathfinding test", new(400, 400), ImGuiWindowFlags.None)
    {
        _visu = RebuildMap();
    }

    public override void Draw()
    {
        _visu.Draw();

        var rebuild = false;
        if (ImGui.CollapsingHeader("Map setup"))
        {
            rebuild |= ImGui.Checkbox("Use new rasterizer", ref _useNewRasterizer);

            rebuild |= ImGui.DragFloat("Resolution", ref _mapResolution, 0.1f, 0.1f, 10, "%.1f", ImGuiSliderFlags.Logarithmic);
            rebuild |= ImGui.DragFloat("Block zone threshold", ref _mapThreshold, 0.1f, -5, 5);
            rebuild |= ImGui.DragFloat2("Center", ref _mapCenter);
            rebuild |= ImGui.DragFloat2("Half-size", ref _mapHalfSize, 1, 0, 30);
            rebuild |= ImGui.DragFloat("Rotation", ref _mapRotationDeg, 5, -180, 180);
            rebuild |= ImGui.DragFloat("Cushion", ref _cushion, 0.1f, 0, 5);
            rebuild |= ImGui.DragFloat("Ping", ref _ping, 0.01f, 0, 0.5f);

            rebuild |= ImGui.DragFloat2("Starting position", ref _startingPos, 1, -30, 30);
            rebuild |= ImGui.DragFloat2("Target position", ref _targetPos, 1, -30, 30);
            rebuild |= ImGui.DragFloat("Target radius", ref _targetRadius, 1, 0, 30);
            rebuild |= ImGui.DragFloat("Target direction", ref _targetFacingDeg, 5, -180, 180);

            rebuild |= ImGui.Checkbox("Block cone", ref _blockCone);
            if (_blockCone)
            {
                rebuild |= ImGui.DragFloat2("Block cone: center", ref _blockConeCenter, 0.25f);
                rebuild |= ImGui.DragFloat2("Block cone: radius", ref _blockConeRadius, 0.25f, 0, 30);
                rebuild |= ImGui.DragFloat("Block cone: direction", ref _blockConeRotationDeg, 5, -180, 180);
                rebuild |= ImGui.DragFloat("Block cone: half-angle", ref _blockConeHalfAngle, 5, 0, 180);
                rebuild |= ImGui.DragFloat("Block cone: max-g", ref _blockConeG, 1, 0, 100);
            }

            rebuild |= ImGui.Checkbox("Block rect", ref _blockRect);
            if (_blockRect)
            {
                rebuild |= ImGui.DragFloat2("Block rect: center", ref _blockRectCenter, 0.25f);
                rebuild |= ImGui.DragFloat2("Block rect: length", ref _blockRectLen, 0.25f, 0, 30);
                rebuild |= ImGui.DragFloat("Block rect: half width", ref _blockRectHalfWidth, 1, 0, 30);
                rebuild |= ImGui.DragFloat("Block rect: direction", ref _blockRectRotationDeg, 5, -180, 180);
                rebuild |= ImGui.DragFloat("Block rect: max-g", ref _blockRectG, 1, 0, 100);
            }
        }

        rebuild |= ImGui.Checkbox("Antialiased", ref _antialiased);

        if (rebuild)
            _visu = RebuildMap();

        //ImGui.Text($"{_visu.Map.Width} x {_visu.Map.Height}");
        //var cursor = ImGui.GetCursorScreenPos();
        //ImGui.GetWindowDrawList().AddRect(cursor, cursor + new Vector2(400), 0xFFFFFFFF);
        //ImGui.Image(_result.Handle, new(400));
    }

    private MapVisualizer RebuildMap()
    {
        Map map = new(_mapResolution, new(_mapCenter), _mapHalfSize.X, _mapHalfSize.Y, _mapRotationDeg.Degrees());

        var sg = Array.Empty<float>();
        var sd = Array.Empty<uint>();
        var now = DateTime.MinValue;
        List<(Sdf containsFn, DateTime activation, ulong source)> zones = [];
        if (_blockCone)
            zones.Add((Sdf.Continuous(ShapeDistance.DonutSector(new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees())), now.AddSeconds(_blockConeG), 0));
        if (_blockRect)
            zones.Add((Sdf.Continuous(ShapeDistance.Rect(new(_blockRectCenter), _blockRectRotationDeg.Degrees(), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth)), now.AddSeconds(_blockRectG), 0));
        zones.SortBy(z => z.activation);

        var gCopy = new List<float>();

        if (_useNewRasterizer)
            NavigationDecision.RasterizeForbiddenZones(map, zones, now, _ping, _cushion, gCopy);
        else
            NavigationDecision.RasterizeForbiddenZonesOld(map, zones, now, _ping, _cushion);

        NavigationDecision.RasterizeGoalZones(map, [AIHints.GoalSingleTarget(new(_targetPos), _targetFacingDeg.Degrees(), Positional.Rear, _targetRadius)]);

        var visu = new MapVisualizer(map, new(_startingPos), [.. gCopy]);

        if (_blockCone)
            visu.Sectors.Add((new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees()));
        if (_blockRect)
            visu.Rects.Add((new(_blockRectCenter), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth, _blockRectRotationDeg.Degrees()));

        _result.Dispose();
        _result = _draw.Draw(map.Width + 1, map.Height + 1, _antialiased, ctx =>
        {
            if (_blockCone)
            {
                var geometry = new PathGeometry(ctx.Factory);

                using (var sink = geometry.Open())
                {
                    var p1 = _blockConeCenter + _mapHalfSize / _mapResolution + (_blockConeRotationDeg + _blockConeHalfAngle).Degrees().ToDirection().ToVec2() * _blockConeRadius.Y / _mapResolution;
                    var p2 = _blockConeCenter + _mapHalfSize / _mapResolution + (_blockConeRotationDeg - _blockConeHalfAngle).Degrees().ToDirection().ToVec2() * _blockConeRadius.Y / _mapResolution;
                    var p3 = _blockConeCenter + _mapHalfSize / _mapResolution + (_blockConeRotationDeg - _blockConeHalfAngle).Degrees().ToDirection().ToVec2() * _blockConeRadius.X / _mapResolution;
                    var p4 = _blockConeCenter + _mapHalfSize / _mapResolution + (_blockConeRotationDeg + _blockConeHalfAngle).Degrees().ToDirection().ToVec2() * _blockConeRadius.X / _mapResolution;

                    sink.BeginFigure(V(p1), FigureBegin.Filled);
                    sink.AddArc(new()
                    {
                        Point = V(p2),
                        Size = new(_blockConeRadius.Y / _mapResolution, _blockConeRadius.Y / _mapResolution),
                        ArcSize = _blockConeHalfAngle >= 90 ? ArcSize.Large : ArcSize.Small,
                        SweepDirection = SweepDirection.Clockwise
                    });
                    sink.AddLine(V(p3));
                    sink.AddArc(new()
                    {
                        Point = V(p4),
                        Size = new(_blockConeRadius.X / _mapResolution, _blockConeRadius.X / _mapResolution),
                        ArcSize = _blockConeHalfAngle >= 90 ? ArcSize.Large : ArcSize.Small,
                        SweepDirection = SweepDirection.CounterClockwise
                    });
                    sink.EndFigure(FigureEnd.Closed);
                    sink.Close();
                }

                ctx.FillGeometry(geometry, new SolidColorBrush(ctx, new SharpDX.Color4(1, 0, 0, 1)));
            }
        });

        return visu;
    }
    static RawVector2 V(Vector2 a) => new(a.X, a.Y);
}
