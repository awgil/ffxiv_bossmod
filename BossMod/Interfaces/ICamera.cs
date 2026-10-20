namespace BossMod.Interfaces;

public interface ICamera
{
    void Update();
    void DrawWorldPrimitives();
    void DrawWorldLine(Vector3 start, Vector3 end, uint color, float thickness = 1);
    void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1);
    void DrawWorldCircle(Vector3 center, float radius, uint color, float thickness = 1);
}

public sealed class MockCamera : ICamera
{
    public void Update() { }
    public void DrawWorldPrimitives() { }
    public void DrawWorldLine(Vector3 start, Vector3 end, uint color, float thickness = 1) { }
    public void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1) { }
    public void DrawWorldCircle(Vector3 center, float radius, uint color, float thickness = 1) { }
}
