namespace BossMod.Debug;

sealed class DebugRaycast(WorldState ws, Camera camera, Shadowcaster shadows)
{
    public void Draw()
    {
        if (ws.Party.Player() is not { } player)
            return;

        Vector3 origin;
        Vector3 dir;
        if (ws.Actors.Find(player.TargetID) is { } target)
        {
            origin = target.PosRot.XYZ();
            dir = player.PosRot.XYZ() - origin;
            dir /= dir.Length();
        }
        else
        {
            origin = player.PosRot.XYZ();
            dir = player.Rotation.ToDirection().ToVec3(0);
        }

        origin.Y += 2;

        var world = Matrix4x4.CreateWorld(origin, dir, new(0, 1, 0));

        var guideColor = 0xFFFF00FF;

        camera.AddLine(Vector3.Transform(new(0), world), Vector3.Transform(new(0, 0, -2.5f), world), guideColor, 10, false);
        camera.AddLine(Vector3.Transform(new(0, 0, -2.5f), world), Vector3.Transform(new(0.5f, 0, -2), world), guideColor, 10, false);
        camera.AddLine(Vector3.Transform(new(0, 0, -2.5f), world), Vector3.Transform(new(-0.5f, 0, -2), world), guideColor, 10, false);

        foreach (var tri in shadows.ComputeVisibility3D(origin, dir, 30))
            camera.AddTriangle(tri.A, tri.B, tri.C);

        Dalamud.Utility.Util.ShowObject(shadows.Stats);
    }
}
