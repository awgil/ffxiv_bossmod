using BossMod.Pathfinding;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;
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

    private IDalamudTextureWrap Rendered = Service.Texture.CreateEmpty(new Dalamud.Interface.Textures.RawImageSpecification() { Width = 40, Height = 40 }, true, false);
    private readonly Device _d3dDevice;
    private Texture2DDescription _desc;
    private readonly SharpDX.Direct2D1.DeviceContext _d2dContext;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _d3dDevice.Dispose();
            _d2dContext.Dispose();
        }
    }

    public PathfindingTest() : base("Pathfinding test", new(400, 400), ImGuiWindowFlags.None)
    {
        _d3dDevice = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
        _desc = new Texture2DDescription()
        {
            ArraySize = 1,
            MipLevels = 1,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            Width = 40,
            Height = 40,
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0)
        };

        var dxgi = _d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
        var d2dFactory = new SharpDX.Direct2D1.Factory1();
        var d2dDevice = new SharpDX.Direct2D1.Device(d2dFactory, dxgi);
        _d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, DeviceContextOptions.None)
        {
            AntialiasMode = AntialiasMode.Aliased
        };

        _visu = RebuildMap();
    }

    public override void Draw()
    {
        if (_visu == null)
        {
            ImGui.Text($"_visu is null, despite being set in the constructor. That's kinda cool, right?");
            return;
        }
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

        if (rebuild)
            _visu = RebuildMap();

        ImGui.Text($"{_desc.Width} x {_desc.Height}");

        ImGui.Image(Rendered.Handle, new Vector2(_desc.Width, _desc.Height) * 8);
    }

    private MapVisualizer RebuildMap()
    {
        Map map = new(_mapResolution, new(_mapCenter), _mapHalfSize.X, _mapHalfSize.Y, _mapRotationDeg.Degrees());

        _desc.Width = map.Width + 2;
        _desc.Height = map.Height + 2;

        using (var tex = new Texture2D(_d3dDevice, _desc))
        {
            var surface = tex.QueryInterface<Surface>();

            var bitmap = new Bitmap1(_d2dContext, surface, new BitmapProperties1(new PixelFormat(_desc.Format, SharpDX.Direct2D1.AlphaMode.Premultiplied), 96, 96, BitmapOptions.Target));

            _d2dContext.Target = bitmap;
            _d2dContext.BeginDraw();
            _d2dContext.Clear(new Color4(0, 0, 0, 1));

            var geometry = new PathGeometry(_d2dContext.Factory);

            RawVector2 V(Vector2 v2)
            {
                var v3 = v2 + _mapHalfSize / _mapResolution;
                return new(v3.X, v3.Y);
            }

            if (_blockCone)
            {
                using var sink = geometry.Open();
                sink.BeginFigure(V(_blockConeCenter), FigureBegin.Filled);

                // left outer point
                var mag = _blockConeRadius.Y / _mapResolution;
                sink.AddLine(V(_blockConeCenter + (_blockConeRotationDeg + _blockConeHalfAngle).Degrees().ToDirection().ToVec2() * mag));
                sink.AddArc(new ArcSegment()
                {
                    Point = V(_blockConeCenter + (_blockConeRotationDeg - _blockConeHalfAngle).Degrees().ToDirection().ToVec2() * mag),
                    Size = new Size2F(1, 1),
                    ArcSize = ArcSize.Large,
                    SweepDirection = SweepDirection.Clockwise
                });
                sink.AddLine(V(_blockConeCenter + (_blockConeRotationDeg - _blockConeHalfAngle).Degrees().ToDirection().ToVec2() * _blockConeRadius.X / _mapResolution));
                sink.AddArc(new ArcSegment()
                {
                    Point = V(_blockConeCenter + (_blockConeRotationDeg + _blockConeHalfAngle).Degrees().ToDirection().ToVec2() * _blockConeRadius.X / _mapResolution),
                    Size = new Size2F(1, 1),
                    ArcSize = ArcSize.Large,
                    SweepDirection = SweepDirection.CounterClockwise
                });
                sink.EndFigure(FigureEnd.Closed);

                sink.Close();
            }

            _d2dContext.FillGeometry(geometry, new SolidColorBrush(_d2dContext, new Color4(1, 0, 0, 1)));
            _d2dContext.EndDraw();

            var staging = tex.Description;
            staging.Usage = ResourceUsage.Staging;
            staging.BindFlags = BindFlags.None;
            staging.CpuAccessFlags = CpuAccessFlags.Read;
            staging.OptionFlags = ResourceOptionFlags.None;

            using (var stx = new Texture2D(_d3dDevice, staging))
            {
                _d3dDevice.ImmediateContext.CopyResource(tex, stx);

                var data = _d3dDevice.ImmediateContext.MapSubresource(stx, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                var width = tex.Description.Width;
                var height = tex.Description.Height;
                var stride = width * 4;

                var px = new byte[stride * height];

                for (var y = 0; y < height; y++)
                    Marshal.Copy(data.DataPointer + y * data.RowPitch, px, y * stride, stride);

                unsafe
                {
                    Rendered = Service.Texture.CreateFromRaw(RawImageSpecification.Bgra32(_desc.Width, _desc.Height), px);
                }
            }
        }

        List<float> sg = [];
        var sb = new BitArray(0);
        var now = DateTime.MinValue;
        List<(Sdf containsFn, DateTime activation, ulong source)> zones = [];
        if (_blockCone)
            zones.Add((Sdf.Continuous(ShapeDistance.DonutSector(new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees())), now.AddSeconds(_blockConeG), 0));
        if (_blockRect)
            zones.Add((Sdf.Continuous(ShapeDistance.Rect(new(_blockRectCenter), _blockRectRotationDeg.Degrees(), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth)), now.AddSeconds(_blockRectG), 0));
        zones.SortBy(z => z.activation);

        var sw = Stopwatch.StartNew();

        if (_useNewRasterizer)
            NavigationDecision.RasterizeForbiddenZones(map, new(_startingPos), zones, now, sg, sb, _ping, _cushion);
        else
            NavigationDecision.RasterizeForbiddenZonesOld(map, zones, now, sg, sb, _ping, _cushion);

        Service.Log($"rasterizer took {sw.ElapsedMilliseconds}ms");

        List<Func<WPos, float>> goals = [];
        goals.Add(AIHints.GoalSingleTarget(new(_targetPos), _targetFacingDeg.Degrees(), Positional.Rear, _targetRadius));
        NavigationDecision.RasterizeGoalZones(map, goals, _cushion > 0);

        var visu = new MapVisualizer(map, new(_startingPos)/*, sg*/);

        if (_blockCone)
            visu.Sectors.Add((new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees()));
        if (_blockRect)
            visu.Rects.Add((new(_blockRectCenter), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth, _blockRectRotationDeg.Degrees()));

        return visu;
    }
}
