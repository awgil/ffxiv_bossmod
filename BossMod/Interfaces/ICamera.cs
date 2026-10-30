namespace BossMod.Interfaces;

public interface ICamera
{
    void Update();
    void AddLine(Vector3 start, Vector3 end, uint color, float thickness = 1, bool occluded = true);
    void AddCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1, bool occluded = true);
    void AddConeFilled(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float projectionHeight = 0, bool occluded = true);
    void AddCircle(Vector3 center, float radius, uint color, float thickness = 1, bool occluded = true);
    void AddCircleFilled(Vector3 center, float radius, uint color, float projectionHeight = 0, bool occluded = true);
}

public class MockCamera : ICamera
{
    public void AddLine(Vector3 start, Vector3 end, uint color, float thickness = 1, bool occluded = true) { }
    public void AddCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1, bool occluded = true) { }
    public void AddConeFilled(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float projectionHeight = 0, bool occluded = true) { }
    public void AddCircle(Vector3 center, float radius, uint color, float thickness = 1, bool occluded = true) { }
    public void AddCircleFilled(Vector3 center, float radius, uint color, float projectionHeight = 0, bool occluded = true) { }

    public void Update() { }
}
