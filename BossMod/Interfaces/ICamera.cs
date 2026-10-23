namespace BossMod.Interfaces;

public interface ICamera
{
    float CameraAzimuth { get; }
    void Update();
    void DrawWorldLine(Vector3 start, Vector3 end, uint color, float thickness = 1);
    void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1);
    void DrawWorldCircle(Vector3 center, float radius, uint color, float thickness = 1);
}

public class MockCamera : ICamera
{
    public float CameraAzimuth => 0;

    public void DrawWorldCircle(Vector3 center, float radius, uint color, float thickness = 1) { }
    public void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1) { }
    public void DrawWorldLine(Vector3 start, Vector3 end, uint color, float thickness = 1) { }

    public void Update() { }
}
