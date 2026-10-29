using BossMod.Interfaces;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Pictomancy;

namespace BossMod;

public sealed class Camera(IDalamudPluginInterface dalamud) : ICamera, IDisposable
{
    public Vector3 Origin;
    public Matrix4x4 View;
    public Matrix4x4 Proj;
    public Matrix4x4 ViewProj;
    public Vector4 NearPlane;
    public float CameraAzimuth { get; private set; } // facing north = 0, facing west = pi/4, facing south = +-pi/2, facing east = -pi/4
    public float CameraAltitude; // facing horizontally = 0, facing down = pi/4, facing up = -pi/4
    public Vector2 ViewportSize;
    private readonly DtrConfig _uiConfig = Service.Config.Get<DtrConfig>();

    record struct Line(Vector3 Start, Vector3 End, uint Color, float Thickness = 1, bool Occluded = true);
    record struct Cone(Vector3 Center, float Radius, Angle Direction, Angle HalfAngle, uint Color, float Thickness = 1, bool Occluded = true);
    record struct Circle(Vector3 Center, float Radius, uint Color, float Thickness = 1, bool Occluded = true);
    record struct CircleFilled(Vector3 Center, float Radius, uint Color, uint? OutlineColor, bool Occluded = true, float ProjectionHeight = 0);

    readonly List<Line> _lines = [];
    readonly List<Cone> _cones = [];
    readonly List<Circle> _circles = [];
    readonly List<CircleFilled> _circlesFilled = [];

    private readonly PctContext picto = PctService.Initialize(dalamud);

    public unsafe void Update()
    {
        var controlCamera = CameraManager.Instance()->GetActiveCamera();
        var renderCamera = controlCamera != null ? controlCamera->SceneCamera.RenderCamera : null;
        if (renderCamera == null)
            return;

        Origin = renderCamera->Origin;
        View = renderCamera->ViewMatrix;
        View.M44 = 1; // for whatever reason, game doesn't initialize it...
        Proj = renderCamera->ProjectionMatrix;
        ViewProj = View * Proj;

        // note that game uses reverse-z by default, so we can't just get full plane equation by reading column 3 of vp matrix
        // so just calculate it manually: column 3 of view matrix is plane equation for a plane equation going through origin
        // proof:
        // plane equation p is such that p.dot(Q, 1) = 0 if Q lines on the plane => pw = -Q.dot(n); for view matrix, V43 is -origin.dot(forward)
        // plane equation for near plane has Q.dot(n) = O.dot(n) - near => pw = V43 + near
        NearPlane = new(View.M13, View.M23, View.M33, View.M43 + renderCamera->NearPlane);

        CameraAzimuth = MathF.Atan2(View.M13, View.M33);
        CameraAltitude = MathF.Asin(View.M23);
        var device = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device.Instance();
        ViewportSize = new(device->Width, device->Height);

        using var drawList = PctService.Draw();
        if (drawList == null)
            return;

        var occludeAlpha = _uiConfig.Occlude ? _uiConfig.OcclusionAlpha : 1;

        foreach (var line in _lines)
            drawList.AddLine(line.Start, line.End, 0, line.Color, line.Thickness, new PctDxParams()
            {
                OccludedAlpha = line.Occluded ? occludeAlpha : 1
            });
        foreach (var cone in _cones)
            drawList.AddFan(cone.Center, 0, cone.Radius, (-cone.Direction - cone.HalfAngle).Rad, (-cone.Direction + cone.HalfAngle).Rad, cone.Color, thickness: cone.Thickness, p: new PctDxParams()
            {
                OccludedAlpha = cone.Occluded ? occludeAlpha : 1
            });
        foreach (var circle in _circles)
            drawList.AddCircle(circle.Center, circle.Radius, circle.Color, thickness: circle.Thickness, p: new PctDxParams()
            {
                OccludedAlpha = circle.Occluded ? occludeAlpha : 1
            });
        foreach (var cf in _circlesFilled)
            drawList.AddCircleFilled(cf.Center, cf.Radius, cf.Color, cf.OutlineColor, p: new PctDxParams()
            {
                OccludedAlpha = cf.Occluded ? occludeAlpha : 1,
                ProjectionHeight = cf.ProjectionHeight
            });

        _lines.Clear();
        _cones.Clear();
        _circles.Clear();
        _circlesFilled.Clear();
    }

    public void DrawWorldLine(Vector3 start, Vector3 end, uint color, float thickness = 1, bool occluded = true)
    {
        if ((end.X, end.Z) == (start.X, start.Z) && end.Y != start.Y)
            Service.PluginLog.Warning($"Line {start} -> {end} is purely vertical, pictomancy doesn't like this");

        _lines.Add(new(start, end, color, thickness, occluded));
    }

    public void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1, bool occluded = true)
    {
        _cones.Add(new(center, radius, direction, halfWidth, color, thickness, occluded));
    }

    public void DrawWorldCircle(Vector3 center, float radius, uint color, float thickness = 1, bool occluded = true)
    {
        _circles.Add(new(center, radius, color, thickness, occluded));
    }

    public void DrawWorldCircleFilled(Vector3 center, float radius, uint color, uint? outlineColor = null, bool occluded = true, float projectionHeight = 0)
    {
        _circlesFilled.Add(new(center, radius, color, outlineColor, occluded, projectionHeight));
    }

    public void DrawWorldSphere(Vector3 center, float radius, uint color, float thickness = 1)
    {
        var numSegments = CurveApprox.CalculateCircleSegments(radius, 360.Degrees(), 1 / 90f);
        var prev1 = center + new Vector3(0, 0, radius);
        var prev2 = center + new Vector3(0, radius, 0);
        var prev3 = center + new Vector3(radius, 0, 0);
        for (var i = 1; i <= numSegments; ++i)
        {
            var dir = (i * 360.0f / numSegments).Degrees().ToDirection();
            var curr1 = center + radius * new Vector3(dir.X, 0, dir.Z);
            var curr2 = center + radius * new Vector3(0, dir.Z, dir.X);
            var curr3 = center + radius * new Vector3(dir.Z, dir.X, 0);
            DrawWorldLine(curr1, prev1, color, thickness);
            DrawWorldLine(curr2, prev2, color, thickness);
            DrawWorldLine(curr3, prev3, color, thickness);
            prev1 = curr1;
            prev2 = curr2;
            prev3 = curr3;
        }
    }

    public void Dispose()
    {
        picto.Dispose();
    }
}
