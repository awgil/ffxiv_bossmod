using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision.Math;
using System.Text.Json;

namespace BossMod.Debug;

sealed unsafe class DebugRaycast(WorldState ws)
{
    private readonly (Vector3, Vector3)[] ShadowPoints = new (Vector3, Vector3)[25600];

    const float PlayerReach = 25;
    const float PlayerReachSq = PlayerReach * PlayerReach;

    public static Shadowcast.LineSegment FailedSegment;
    public static Shadowcast.Ray FailedRay;

    struct DrawStats
    {
        public int TotalCells;
        public int ProcessedCells;
        public int NumIntersections;
        public int NumPrunedIntersections;
    }

    int _numPoints;
    DrawStats _stats;
    Vector3 origin;
    readonly List<WPos> seg = [];

    public void Draw()
    {
        Dalamud.Utility.Util.ShowObject(_stats);

        //if (ImGui.Button("Calculate shadowcast"))
        origin = DoIt();

        var originXZ = new WPos(origin.X, origin.Z);
        List<Shadowcast.LineSegment> segments = [];

        void addBlock(WPos center, IEnumerable<WDir> corners)
        {
            var c = corners.ToList();
            for (var i = 0; i < c.Count - 1; i++)
                segments.Add(new(c[i] + center, c[i + 1] + center));
            segments.Add(new(c[^1] + center, c[0] + center));
        }

        addBlock(originXZ, CurveApprox.Rect(new(100, 0), new(0, 100)));

        for (var i = 0; i < _numPoints; i++)
        {
            segments.Add(new(new(ShadowPoints[i].Item1.X, ShadowPoints[i].Item1.Z), new(ShadowPoints[i].Item2.X, ShadowPoints[i].Item2.Z)));
            Service.Camera.AddLine(ShadowPoints[i].Item1, ShadowPoints[i].Item2, 0xFFFF00FF, 1, false);
        }

        ImGui.Text($"Total number of world segments: {_numPoints}");

        if (FailedSegment != default)
            Service.Camera.AddLine(FailedSegment.A.ToVec3(origin.Y), FailedSegment.B.ToVec3(origin.Y), 0xFFFFFF00, 1, false);

        if (FailedRay != default)
            Service.Camera.AddLine(FailedRay.Origin.ToVec3(origin.Y), FailedRay.Origin.ToVec3(origin.Y) + FailedRay.Direction.ToVec3(0), 0xFF0000FF, 1, false);

        using (ImRaii.Disabled(_numPoints == 0))
        {
            if (ImGui.Button("Calculate visibility polygon"))
            {
                ImGui.SetClipboardText(JsonSerializer.SerializeToUtf8Bytes(segments));

                seg.Clear();
                List<Shadowcast.LineSegment> l = [];
                try
                {
                    Shadowcast.Uncross(segments, l);
                    seg.AddRange([.. Shadowcast.VisibilityPolygon(new WPos(origin.X, origin.Z), l)]);
                }
                catch (ArgumentException ex)
                {
                    Service.PluginLog.Error(ex, "Unable to calculate polygon");
                }
            }
        }

        for (var i = 1; i < seg.Count; i++)
            Service.Camera.AddLine(seg[i - 1].ToVec3(origin.Y), seg[i].ToVec3(origin.Y), 0xFFFF00FF, 3, false);
    }

    Vector3 DoIt()
    {
        Array.Fill(ShadowPoints, default);
        seg.Clear();
        _stats = default;
        _numPoints = 0;

        Vector3 orig = default;

        foreach (var scene in Framework.Instance()->BGCollisionModule->SceneManager->Scenes)
            orig = DrawCollision(scene->Scene->Quadtree) ?? default;

        return orig;
    }

    Vector3? DrawCollision(Quadtree* tree)
    {
        if (ws.Party.Player() is not { } player)
            return null;

        Vector3 rayOrigin;
        Vector3 rayDir;

        if (ws.Actors.Find(player.TargetID) is { } target)
        {
            rayOrigin = target.PosRot.XYZ();
            rayDir = (player.Position - target.Position).Normalized().ToVec3(0);
        }
        else
        {
            rayOrigin = player.PosRot.XYZ();
            rayDir = player.Rotation.ToDirection().ToVec3(0);
        }

        rayOrigin.Y += 2;

        var rayWorld = Matrix4x4.CreateWorld(default, rayDir, new(0, 1, 0));

        for (var level = 1; level < tree->NumLevels; level++)
        {
            var cellSizeX = (tree->MaxX - tree->MinX + 1) / (1 << level);
            var cellSizeZ = (tree->MaxZ - tree->MinZ + 1) / (1 << level);

            var nodes = tree->NodesAtLevel(level);
            for (var i = 0; i < nodes.Length; i++)
            {
                _stats.TotalCells++;
                ref var node = ref nodes[i];
                if (node.Node.NodeLink.Next == null)
                    continue;

                var (x, z) = Quadtree.CellCoords((uint)i);
                var cellX = tree->MinX + x * cellSizeX;
                var cellZ = tree->MinZ + z * cellSizeZ;

                var playerXZ = new Vector2(rayOrigin.X, rayOrigin.Z);

                if (OverlapAARect(playerXZ, new(cellX, cellZ), new Vector2(cellX, cellZ) + new Vector2(cellSizeX, cellSizeZ)))
                {
                    _stats.ProcessedCells++;
                    foreach (var coll in node.Colliders)
                        if (coll != null)
                            DrawSolids(coll, rayOrigin, Vector3.Transform(new(0, 1, 0), rayWorld));
                }
            }
        }

        return rayOrigin;
    }

    private void DrawSolids(Collider* coll, in Vector3 rayOrigin, in Vector3 rayDir)
    {
        var type = coll->GetColliderType();

        if (type == ColliderType.Mesh)
        {
            var collM = (ColliderMesh*)coll;
            var mesh = (MeshPCB*)collM->Mesh;
            var world = collM->World;
            DrawAllTriangles(mesh->RootNode, collM->World, 0xFF00FF00, rayOrigin, rayDir);
        }
        else
        {
            //Service.Log.Debug($"unhandled collider type {type}");
        }
    }

    private void DrawAllTriangles(MeshPCB.FileNode* root, in Matrix4x3 world, uint color, in Vector3 rayOrigin, in Vector3 rayDir)
    {
        if (root == null)
            return;

        foreach (ref var prim in root->Primitives)
            DrawOneTriangle(root, prim, world, color, rayOrigin, rayDir);

        if (root->Child1 != null)
            DrawAllTriangles(root->Child1, world, color, rayOrigin, rayDir);
        if (root->Child2 != null)
            DrawAllTriangles(root->Child2, world, color, rayOrigin, rayDir);
    }

    static float DistSqXZ(in Vector3 v1, in Vector3 v2)
    {
        var dX = v2.X - v1.X;
        var dZ = v2.Z - v1.Z;
        return dX * dX + dZ * dZ;
    }

    private void DrawOneTriangle(MeshPCB.FileNode* node, in Mesh.Primitive prim, in Matrix4x3 world, uint color, in Vector3 rayOrigin, in Vector3 rayDir)
    {
        if ((prim.Material & 0x4000) == 0)
            return;

        var v1 = world.TransformCoordinate(node->Vertex(prim.V1));
        var v2 = world.TransformCoordinate(node->Vertex(prim.V2));
        var v3 = world.TransformCoordinate(node->Vertex(prim.V3));

        var ab = v2 - v1;
        var ac = v3 - v1;
        var normal = Vector3.Normalize(Vector3.Cross(ab, ac));
        var centroid = (v1 + v2 + v3) / 3f;

        // tri is facing away from player/source object, skip
        if (Vector3.Dot(normal, centroid - rayOrigin) >= 0)
            return;

        var verts = new Vector3[2];
        var i = 0;
        if (PlaneLine(rayOrigin, rayDir, v1, v2, out var i12))
            verts[i++] = i12;
        if (PlaneLine(rayOrigin, rayDir, v2, v3, out var i23))
            verts[i++] = i23;
        if (i < 2 && PlaneLine(rayOrigin, rayDir, v1, v3, out var i13))
            verts[i++] = i13;

        if (i >= 2)
            ShadowPoints[_numPoints++] = (verts[0], verts[1]);
    }

    bool PlaneLine(in Vector3 rayOrigin, in Vector3 rayDir, in Vector3 pointA, in Vector3 pointB, out Vector3 intersection)
    {
        intersection = default;

        _stats.NumIntersections++;

        if (DistSqXZ(pointA, rayOrigin) >= PlayerReachSq && DistSqXZ(pointB, rayOrigin) >= PlayerReachSq)
        {
            _stats.NumPrunedIntersections++;
            return false;
        }

        var u = pointB - pointA;
        var dot = Vector3.Dot(rayDir, u);

        if (MathF.Abs(dot) >= float.Epsilon)
        {
            var w = pointA - rayOrigin;
            var fac = -Vector3.Dot(rayDir, w) / dot;
            u *= fac;
            intersection = pointA + u;
            return fac is >= 0 and <= 1;
        }

        return false;
    }

    static bool OverlapAARect(Vector2 pos, Vector2 min, Vector2 max)
    {
        var (aleft, aright) = (pos.X - PlayerReach, pos.X + PlayerReach);
        var (abottom, atop) = (pos.Y - PlayerReach, pos.Y + PlayerReach);
        var (bleft, bright) = (min.X, max.X);
        var (bbottom, btop) = (min.Y, max.Y);

        return !(aright <= bleft || bright <= aleft || atop <= bbottom || btop <= abottom);
    }
}
