using Dalamud.Plugin.Services;

namespace BossMod.Interfaces;

public interface IShadowcaster
{
    public record struct Triangle(Vector3 A, Vector3 B, Vector3 C);

    List<Tri> ComputeVisibility2D(Vector3 origin, Vector3 lookDirection, float radius);

    List<Triangle> ComputeVisibility3D(Vector3 origin, Vector3 lookDirection, float radius);
}

public class MockShadowcaster(IPluginLog logger) : IShadowcaster
{
    public List<Tri> ComputeVisibility2D(Vector3 origin, Vector3 lookDirection, float radius)
    {
        logger.Debug($"ComputeVisibility2D({origin}, {lookDirection}, {radius})");
        return [];
    }

    public List<IShadowcaster.Triangle> ComputeVisibility3D(Vector3 origin, Vector3 lookDirection, float radius)
    {
        logger.Debug($"ComputeVisibility3D({origin}, {lookDirection}, {radius})");
        return [];
    }
}
