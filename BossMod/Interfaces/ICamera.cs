namespace BossMod.Interfaces;

public interface ICamera
{
    float CameraAzimuth { get; }
    void Update();
    void DrawWorldLine(Vector3 start, Vector3 end, uint color, float thickness = 1, bool occluded = true);
    void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1, bool occluded = true);
    void DrawWorldCircle(Vector3 center, float radius, uint color, float thickness = 1, bool occluded = true);
    void DrawWorldCircleFilled(Vector3 center, float radius, uint color, uint? outlineColor = null, bool occluded = true, float projectionHeight = 0);
}

public class MockCamera : ICamera
{
    public float CameraAzimuth => 0;

    public void DrawWorldCircle(Vector3 center, float radius, uint color, float thickness = 1, bool occluded = true) { }
    public void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1, bool occluded = true) { }
    public void DrawWorldLine(Vector3 start, Vector3 end, uint color, float thickness = 1, bool occluded = true) { }
    public void DrawWorldCircleFilled(Vector3 center, float radius, uint color, uint? outlineColor = null, bool occluded = true, float projectionHeight = 0) { }

    public void Update() { }
}
