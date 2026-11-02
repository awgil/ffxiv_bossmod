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
    record struct Arc(Vector3 Center, float Radius, Angle Direction, Angle HalfAngle, uint Color, float Thickness = 1, bool Occluded = true);
    // dimension = projection height (filled), thickness (stroked)
    record struct Cone(Vector3 Center, float Radius, Angle Direction, Angle HalfAngle, uint Color, bool Filled, float Dimension = 1, bool Occluded = true);
    record struct Circle(Vector3 Center, float Radius, uint Color, bool Filled, float Dimension, bool Occluded);

    private readonly PctContext picto = PctService.Initialize(dalamud);

    private readonly List<Line> _lines = [];
    private readonly List<Cone> _cones = [];
    private readonly List<Arc> _arcs = [];
    private readonly List<Circle> _circles = [];

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

        PctDxParams p(bool isLine, bool occluded) => new() { OcclusionTolerance = isLine ? 0.02f : 0.02f, OccludedAlpha = occluded && _uiConfig.Occlude ? _uiConfig.OcclusionAlpha : 1 };

        foreach (var line in _lines)
            drawList.AddLine(line.Start, line.End, 0, line.Color, line.Thickness, p(true, line.Occluded));
        foreach (var arc in _arcs)
            drawList.AddArc(arc.Center, arc.Radius, (-arc.Direction - arc.HalfAngle).Rad, (-arc.Direction + arc.HalfAngle).Rad, arc.Color, thickness: arc.Thickness, p: p(true, arc.Occluded));

        foreach (var cone in _cones)
        {
            if (cone.Filled)
                drawList.AddFanFilled(cone.Center, 0, cone.Radius, (-cone.Direction - cone.HalfAngle).Rad, (-cone.Direction + cone.HalfAngle).Rad, cone.Color, p: p(false, cone.Occluded) with { ProjectionHeight = cone.Dimension });
            else
                drawList.AddFan(cone.Center, 0, cone.Radius, (-cone.Direction - cone.HalfAngle).Rad, (-cone.Direction + cone.HalfAngle).Rad, cone.Color, thickness: cone.Dimension, p: p(true, cone.Occluded));
        }

        foreach (var circle in _circles)
        {
            if (circle.Filled)
                drawList.AddCircleFilled(circle.Center, circle.Radius, circle.Color, p: p(isLine: false, occluded: circle.Occluded) with { ProjectionHeight = circle.Dimension });
            else
                drawList.AddCircle(circle.Center, circle.Radius, circle.Color, thickness: circle.Dimension, p: p(false, circle.Occluded));
        }
        //foreach (var cf in _circlesFilled)
        //drawList.AddCircleFilled(cf.Center, cf.Radius, cf.Color, cf.OutlineColor, p: p(false, cf.Occluded) with { ProjectionHeight = cf.ProjectionHeight });

        _lines.Clear();
        _cones.Clear();
        _arcs.Clear();
        _circles.Clear();
        //_circlesFilled.Clear();
    }

    public void AddLine(Vector3 start, Vector3 end, uint color, float thickness = 1, bool occluded = true)
    {
        if ((end.X, end.Z) == (start.X, start.Z) && end.Y != start.Y)
            Service.PluginLog.Warning($"Line {start} -> {end} is purely vertical, pictomancy doesn't like this");

        _lines.Add(new(start, end, color, thickness, occluded));
    }

    public void AddArc(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1, bool occluded = true)
    {
        _arcs.Add(new(center, radius, direction, halfWidth, color, thickness, occluded));
    }

    public void AddCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1, bool occluded = true)
    {
        _cones.Add(new(center, radius, direction, halfWidth, color, false, thickness, occluded));
    }

    public void AddConeFilled(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float projectionHeight = 0, bool occluded = true)
    {
        _cones.Add(new(center, radius, direction, halfWidth, color, true, projectionHeight, occluded));
    }

    public void AddCircle(Vector3 center, float radius, uint color, float thickness = 1, bool occluded = true)
    {
        _circles.Add(new(center, radius, color, false, thickness, occluded));
    }

    public void AddCircleFilled(Vector3 center, float radius, uint color, float projectionHeight = 0, bool occluded = true)
    {
        _circles.Add(new(center, radius, color, true, projectionHeight, occluded));
    }

    public void AddSphere(Vector3 center, float radius, uint color, float thickness = 1)
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
            AddLine(curr1, prev1, color, thickness);
            AddLine(curr2, prev2, color, thickness);
            AddLine(curr3, prev3, color, thickness);
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
