using Dalamud.Interface.Utility;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ImGuiNET;

namespace BossMod;

class Camera
{
    public static Camera? Instance;

    public Vector3 Origin;
    public Matrix4x4 View;
    public Matrix4x4 Proj;
    public Matrix4x4 ViewProj;
    public Vector4 NearPlane;
    public float CameraAzimuth; // facing north = 0, facing west = pi/4, facing south = +-pi/2, facing east = -pi/4
    public float CameraAltitude; // facing horizontally = 0, facing down = pi/4, facing up = -pi/4
    public Vector2 ViewportSize;

    private readonly List<(Vector2 from, Vector2 to, uint col)> _worldDrawLines = [];

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
    }

    public void DrawWorldPrimitives()
    {
        if (_worldDrawLines.Count == 0)
            return;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
        ImGui.Begin("world_overlay", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
        ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

        var dl = ImGui.GetWindowDrawList();
        foreach (var l in _worldDrawLines)
            dl.AddLine(l.from, l.to, l.col);
        _worldDrawLines.Clear();

        ImGui.End();
        ImGui.PopStyleVar();
    }

    public void DrawWorldLine(Vector3 start, Vector3 end, uint color)
    {
        var p1w = start;
        var p2w = end;
        if (!ClipLineToNearPlane(ref p1w, ref p2w))
            return;

        var p1p = Vector4.Transform(p1w, ViewProj);
        var p2p = Vector4.Transform(p2w, ViewProj);
        var p1c = p1p.XY() * (1 / p1p.W);
        var p2c = p2p.XY() * (1 / p2p.W);
        var p1screen = new Vector2(0.5f * ViewportSize.X * (1 + p1c.X), 0.5f * ViewportSize.Y * (1 - p1c.Y)) + ImGuiHelpers.MainViewport.Pos;
        var p2screen = new Vector2(0.5f * ViewportSize.X * (1 + p2c.X), 0.5f * ViewportSize.Y * (1 - p2c.Y)) + ImGuiHelpers.MainViewport.Pos;
        _worldDrawLines.Add((p1screen, p2screen, color));
    }

    public void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color)
    {
        int numSegments = CurveApprox.CalculateCircleSegments(radius, halfWidth, 1 / 90f);
        var delta = halfWidth / numSegments;

        var prev = center + radius * (direction - delta * numSegments).ToDirection().ToVec3();
        DrawWorldLine(center, prev, color);
        for (int i = -numSegments + 1; i <= numSegments; ++i)
        {
            var curr = center + radius * (direction + delta * i).ToDirection().ToVec3();
            DrawWorldLine(prev, curr, color);
            prev = curr;
        }
        DrawWorldLine(prev, center, color);
    }

    public void DrawWorldCircle(Vector3 center, float radius, uint color)
    {
        int numSegments = CurveApprox.CalculateCircleSegments(radius, 360.Degrees(), 1 / 90f);
        var prev = center + new Vector3(0, 0, radius);
        for (int i = 1; i <= numSegments; ++i)
        {
            var curr = center + radius * (i * 360.0f / numSegments).Degrees().ToDirection().ToVec3();
            DrawWorldLine(curr, prev, color);
            prev = curr;
        }
    }

    public void DrawWorldSphere(Vector3 center, float radius, uint color)
    {
        int numSegments = CurveApprox.CalculateCircleSegments(radius, 360.Degrees(), 1 / 90f);
        var prev1 = center + new Vector3(0, 0, radius);
        var prev2 = center + new Vector3(0, radius, 0);
        var prev3 = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= numSegments; ++i)
        {
            var dir = (i * 360.0f / numSegments).Degrees().ToDirection();
            var curr1 = center + radius * new Vector3(dir.X, 0, dir.Z);
            var curr2 = center + radius * new Vector3(0, dir.Z, dir.X);
            var curr3 = center + radius * new Vector3(dir.Z, dir.X, 0);
            DrawWorldLine(curr1, prev1, color);
            DrawWorldLine(curr2, prev2, color);
            DrawWorldLine(curr3, prev3, color);
            prev1 = curr1;
            prev2 = curr2;
            prev3 = curr3;
        }
    }

    public void DrawWorldUnitCylinder(SharpDX.Matrix transform, uint color)
    {
        int numSegments = CurveApprox.CalculateCircleSegments(transform.Row1.Length(), 360.Degrees(), 1 / 90f);
        var prev1 = SharpDX.Vector3.TransformCoordinate(new(0, +1, 1), transform).ToSystem();
        var prev2 = SharpDX.Vector3.TransformCoordinate(new(0, -1, 1), transform).ToSystem();
        for (int i = 1; i <= numSegments; ++i)
        {
            var dir = (i * 360.0f / numSegments).Degrees().ToDirection();
            var curr1 = SharpDX.Vector3.TransformCoordinate(new(dir.X, +1, dir.Z), transform).ToSystem();
            var curr2 = SharpDX.Vector3.TransformCoordinate(new(dir.X, -1, dir.Z), transform).ToSystem();
            DrawWorldLine(curr1, prev1, color);
            DrawWorldLine(curr2, prev2, color);
            DrawWorldLine(curr1, curr2, color);
            prev1 = curr1;
            prev2 = curr2;
        }
    }

    public void DrawWorldOBB(Vector3 min, Vector3 max, SharpDX.Matrix transform, uint color)
    {
        var aaa = SharpDX.Vector3.TransformCoordinate(new(min.X, min.Y, min.Z), transform).ToSystem();
        var aab = SharpDX.Vector3.TransformCoordinate(new(min.X, min.Y, max.Z), transform).ToSystem();
        var aba = SharpDX.Vector3.TransformCoordinate(new(min.X, max.Y, min.Z), transform).ToSystem();
        var abb = SharpDX.Vector3.TransformCoordinate(new(min.X, max.Y, max.Z), transform).ToSystem();
        var baa = SharpDX.Vector3.TransformCoordinate(new(max.X, min.Y, min.Z), transform).ToSystem();
        var bab = SharpDX.Vector3.TransformCoordinate(new(max.X, min.Y, max.Z), transform).ToSystem();
        var bba = SharpDX.Vector3.TransformCoordinate(new(max.X, max.Y, min.Z), transform).ToSystem();
        var bbb = SharpDX.Vector3.TransformCoordinate(new(max.X, max.Y, max.Z), transform).ToSystem();
        DrawWorldLine(aaa, aab, color);
        DrawWorldLine(aab, bab, color);
        DrawWorldLine(bab, baa, color);
        DrawWorldLine(baa, aaa, color);
        DrawWorldLine(aba, abb, color);
        DrawWorldLine(abb, bbb, color);
        DrawWorldLine(bbb, bba, color);
        DrawWorldLine(bba, aba, color);
        DrawWorldLine(aaa, aba, color);
        DrawWorldLine(aab, abb, color);
        DrawWorldLine(baa, bba, color);
        DrawWorldLine(bab, bbb, color);
    }

    private bool ClipLineToNearPlane(ref Vector3 a, ref Vector3 b)
    {
        var an = Vector4.Dot(new(a, 1), NearPlane);
        var bn = Vector4.Dot(new(b, 1), NearPlane);
        if (an >= 0 && bn >= 0)
            return false; // line fully behind near plane

        if (an > 0 || bn > 0)
        {
            var ab = b - a;
            var abn = Vector3.Dot(ab, NearPlane.XYZ());
            var t = -an / abn;
            var p = a + t * ab;
            if (an > 0)
                a = p;
            else
                b = p;
        }
        return true;
    }
}
