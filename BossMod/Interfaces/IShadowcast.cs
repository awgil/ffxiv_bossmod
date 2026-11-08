using Dalamud.Plugin.Services;

namespace BossMod.Interfaces;

public interface IShadowcast
{
    List<WPos> ComputeVisibility2D(Vector3 origin, Vector3 lookDirection, float radius);

    List<Vector3> ComputeVisibility3D(Vector3 origin, Vector3 lookDirection, float radius);

    public List<WPos> ComputeVisibility2D(Actor target, float radius) => ComputeVisibility2D(target.PosRot.XYZ() + new Vector3(0, 2, 0), new(1, 0, 0), radius);
}

public class MockShadowcaster(IPluginLog logger) : IShadowcast
{
    public List<WPos> ComputeVisibility2D(Vector3 origin, Vector3 lookDirection, float radius)
    {
        logger.Debug($"ComputeVisibility2D({origin}, {lookDirection}, {radius})");
        return [];
    }

    public List<Vector3> ComputeVisibility3D(Vector3 origin, Vector3 lookDirection, float radius)
    {
        logger.Debug($"ComputeVisibility3D({origin}, {lookDirection}, {radius})");
        return [];
    }
}
