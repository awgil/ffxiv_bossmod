using BossMod.Interfaces;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision.Math;
using System.Diagnostics;

using Vis = BossMod.VisibilityPolygon<System.Numerics.Vector3>;

namespace BossMod;

internal unsafe class Shadowcast : IShadowcast
{
    public DrawStats Stats { get; private set; } = new();

    private readonly Vis Vis = new(VisibilityAdapters.Vec3);

    public class DrawStats
    {
        public TimeSpan ScanTime;
        public TimeSpan ComputeTime;

        public int TotalCells;
        public int ProcessedCells;
        public int NumTests;
        public int NumTestsPruned;
        public int NumPoints;

        public int CollMesh;
        public int CollBox;
        public int CollTube;
        public int CollPlane;
    }

    public List<WPos> ComputeVisibility2D(Vector3 origin, Vector3 lookDirection, float radius)
    {
        var (poly, _) = ComputeShared(origin, lookDirection, radius);
        return [.. poly.Select(p => new WPos(p.XZ()))];
    }

    public List<Vector3> ComputeVisibility3D(Vector3 origin, Vector3 lookDirection, float radius)
    {
        var (poly, normal) = ComputeShared(origin, lookDirection, radius);
        var output = new List<Vector3>();

        for (var i = 0; i < poly.Count; i++)
        {
            var pi = poly[i] - origin;
            var oi = pi with { Y = (-pi.X * normal.X - pi.Z * normal.Z) / normal.Y };
            output.Add(oi + origin);
        }

        return output;
    }

    (List<Vector3>, Vector3) ComputeShared(Vector3 origin, Vector3 lookDirection, float radius)
    {
        Stats = new();
        var watch = Stopwatch.StartNew();

        var (segments, normal) = Scan(origin, lookDirection, radius);

        Stats.ScanTime = watch.Elapsed;

        // FIXME: BreakIntersections is O(n^2) and extremely noticeable in busy environments, should be replaced with balaban's algorithm
        var poly = Vis.Compute(origin, Vis.BreakIntersections(segments));

        Stats.ComputeTime = watch.Elapsed - Stats.ScanTime;

        return (poly, normal);
    }

    (List<Vis.Segment>, Vector3) Scan(Vector3 origin, Vector3 lookDirection, float radius)
    {
        var lookWorld = Matrix4x4.CreateWorld(new(0), lookDirection, new(0, 1, 0));
        var lookNormal = Vector3.Transform(new(0, 1, 0), lookWorld);

        List<Vis.Segment> segments = [];

        var perimeter = CurveApprox.Circle(radius, 0.1f).ToList();
        for (var i = 1; i < perimeter.Count; i++)
            segments.Add(new(perimeter[i - 1].ToVec3() + origin, perimeter[i].ToVec3() + origin));
        segments.Add(new(perimeter[^1].ToVec3() + origin, perimeter[0].ToVec3() + origin));

        foreach (var scene in Framework.Instance()->BGCollisionModule->SceneManager->Scenes)
            ScanTree(scene->Scene->Quadtree, origin, lookNormal, radius, segments);

        return (segments, lookNormal);
    }

    void ScanTree(Quadtree* tree, Vector3 origin, Vector3 lookNormal, float radius, List<Vis.Segment> segments)
    {
        var scanner = new Scanner(origin, radius, lookNormal, segments, Stats);

        // TODO: level 0 contains the streaming colliders, which might be duplicated in other layers...
        // figure out if using just streaming suffices since it would likely speed up the scan a lot
        for (var level = 0; level < tree->NumLevels; level++)
        {
            var cellSizeX = (tree->MaxX - tree->MinX + 1) / (1 << level);
            var cellSizeZ = (tree->MaxZ - tree->MinZ + 1) / (1 << level);

            var nodes = tree->NodesAtLevel(level);
            for (var i = 0; i < nodes.Length; i++)
            {
                Stats.TotalCells++;
                ref var node = ref nodes[i];
                if (node.Node.NodeLink.Next == null)
                    continue;

                var (x, z) = Quadtree.CellCoords((uint)i);
                var cellX = tree->MinX + x * cellSizeX;
                var cellZ = tree->MinZ + z * cellSizeZ;

                var playerXZ = new Vector2(origin.X, origin.Z);

                if (OverlapAARect(playerXZ, new(cellX, cellZ), new Vector2(cellX, cellZ) + new Vector2(cellSizeX, cellSizeZ), radius))
                {
                    Stats.ProcessedCells++;
                    foreach (var coll in node.Colliders)
                        if (coll != null)
                            scanner.ProcessCollider(coll);
                }
            }
        }
    }

    static bool OverlapAARect(Vector2 pos, Vector2 min, Vector2 max, float radius)
    {
        var (aleft, aright) = (pos.X - radius, pos.X + radius);
        var (abottom, atop) = (pos.Y - radius, pos.Y + radius);
        var (bleft, bright) = (min.X, max.X);
        var (bbottom, btop) = (min.Y, max.Y);

        return !(aright <= bleft || bright <= aleft || atop <= bbottom || btop <= abottom);
    }

    class Scanner(Vector3 origin, float radius, Vector3 lookNormal, List<Vis.Segment> segments, DrawStats stats)
    {
        readonly float Rsq = radius * radius;

        public void ProcessCollider(Collider* coll)
        {
            // "is active" flag
            if ((coll->VisibilityFlags & 1) == 0)
                return;

            switch (coll->GetColliderType())
            {
                case ColliderType.Streamed:
                    // intentionally skipped
                    /*
                    {
                        var cast = (ColliderStreamed*)coll;
                        if (cast->Header != null && cast->Elements != null)
                        {
                            for (var i = 0; i < cast->Header->NumMeshes; i++)
                            {
                                var elem = cast->Elements + i;
                                ProcessColliderMesh(elem->Mesh);
                            }
                        }
                    }
                    */
                    break;
                case ColliderType.Mesh:
                    stats.CollMesh++;
                    ProcessColliderMesh((ColliderMesh*)coll);
                    break;
                case ColliderType.Cylinder:
                    stats.CollTube++;
                    ProcessCylinder((ColliderCylinder*)coll);
                    break;
                case ColliderType.Box:
                    stats.CollBox++;
                    ProcessBox((ColliderBox*)coll);
                    break;
                case ColliderType.Plane:
                case ColliderType.PlaneTwoSided:
                    stats.CollPlane++;
                    ProcessPlane((ColliderPlane*)coll);
                    break;
                default:
                    Service.PluginLog.Warning($"Unimplemented: collider type {coll->GetColliderType()}");
                    break;
            }
        }

        void ProcessColliderMesh(ColliderMesh* coll)
        {
            if (coll == null || coll->MeshIsSimple || coll->Mesh == null)
                return;

            var pcb = (MeshPCB*)coll->Mesh;
            ProcessFileMesh(pcb->RootNode, coll->World);
        }

        void ProcessFileMesh(MeshPCB.FileNode* root, in Matrix4x3 world)
        {
            if (root == null)
                return;

            foreach (ref var prim in root->Primitives)
                ProcessMeshTriangle(root, prim, world);

            if (root->Child1 != null)
                ProcessFileMesh(root->Child1, world);
            if (root->Child2 != null)
                ProcessFileMesh(root->Child2, world);
        }

        void ProcessPlane(ColliderPlane* coll)
        {
            if (!BlocksRaycast(&coll->Collider))
                return;

            var a = coll->World.TransformCoordinate(new(-1, +1, 0));
            var b = coll->World.TransformCoordinate(new(-1, -1, 0));
            var c = coll->World.TransformCoordinate(new(+1, -1, 0));
            var d = coll->World.TransformCoordinate(new(+1, +1, 0));

            ProcessTriangle(a, c, b);
            ProcessTriangle(a, c, d);
        }

        void ProcessBox(ColliderBox* coll)
        {
            if (!BlocksRaycast(&coll->Collider))
                return;

            var v0 = coll->World.TransformCoordinate(new(-1, -1, -1));
            var v1 = coll->World.TransformCoordinate(new(-1, -1, +1));
            var v2 = coll->World.TransformCoordinate(new(+1, -1, -1));
            var v3 = coll->World.TransformCoordinate(new(+1, -1, +1));
            var v4 = coll->World.TransformCoordinate(new(-1, +1, -1));
            var v5 = coll->World.TransformCoordinate(new(-1, +1, +1));
            var v6 = coll->World.TransformCoordinate(new(+1, +1, -1));
            var v7 = coll->World.TransformCoordinate(new(+1, +1, +1));

            ProcessTriangle(v0, v1, v2);
            ProcessTriangle(v2, v1, v3);
            ProcessTriangle(v5, v4, v7);
            ProcessTriangle(v7, v4, v6);
            ProcessTriangle(v0, v4, v1);
            ProcessTriangle(v1, v4, v5);
            ProcessTriangle(v2, v3, v6);
            ProcessTriangle(v6, v3, v7);
            ProcessTriangle(v0, v2, v4);
            ProcessTriangle(v4, v2, v6);
            ProcessTriangle(v1, v5, v3);
            ProcessTriangle(v3, v5, v7);
        }

        void ProcessCylinder(ColliderCylinder* coll)
        {
            if (!BlocksRaycast(&coll->Collider))
                return;

            var numSegments = CurveApprox.CalculateCircleSegments(coll->World.Row0.Length(), 360.Degrees(), 0.1f);
            var prev1 = coll->World.TransformCoordinate(new(0, +1, 1));
            var prev2 = coll->World.TransformCoordinate(new(0, -1, 1));

            for (var i = 1; i <= numSegments; i++)
            {
                var dir = (i * 360f / numSegments).Degrees().ToDirection();
                var curr1 = coll->World.TransformCoordinate(new(dir.X, +1, dir.Z));
                var curr2 = coll->World.TransformCoordinate(new(dir.X, -1, dir.Z));

                ProcessTriangle(prev1, prev2, curr2);
                ProcessTriangle(prev1, curr2, curr1);

                prev1 = curr1;
                prev2 = curr2;
            }
        }

        void ProcessMeshTriangle(MeshPCB.FileNode* node, in Mesh.Primitive prim, in Matrix4x3 world)
        {
            if ((prim.Material & 0x4000) != 0)
                ProcessTriangle(world.TransformCoordinate(node->Vertex(prim.V1)), world.TransformCoordinate(node->Vertex(prim.V2)), world.TransformCoordinate(node->Vertex(prim.V3)));
        }

        void ProcessTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var ab = v2 - v1;
            var ac = v3 - v1;
            var triNormal = Vector3.Normalize(Vector3.Cross(ab, ac));
            var centroid = (v1 + v2 + v3) / 3f;

            // tri is facing away from player/source object, skip
            if (Vector3.Dot(triNormal, centroid - origin) >= 0)
                return;

            var verts = new Vector3[3];
            var i = 0;
            if (PlaneLine(v1, v2, out var i12))
                verts[i++] = i12;
            if (PlaneLine(v2, v3, out var i23))
                verts[i++] = i23;
            if (i < 2 && PlaneLine(v1, v3, out var i13))
                verts[i++] = i13;

            if (i == 2)
            {
                stats.NumPoints++;
                segments.Add(new(verts[0], verts[1]));
            }
        }

        bool PlaneLine(in Vector3 pointA, in Vector3 pointB, out Vector3 intersection)
        {
            intersection = default;

            stats.NumTests++;

            if (DistSqXZ(pointA, origin) >= Rsq && DistSqXZ(pointB, origin) >= Rsq)
            {
                stats.NumTestsPruned++;
                return false;
            }

            var u = pointB - pointA;
            var dot = Vector3.Dot(lookNormal, u);

            if (MathF.Abs(dot) >= float.Epsilon)
            {
                var w = pointA - origin;
                var fac = -Vector3.Dot(lookNormal, w) / dot;
                u *= fac;
                intersection = pointA + u;
                return fac is >= 0 and <= 1;
            }

            return false;
        }

        static bool BlocksRaycast(Collider* c)
        {
            ulong id = 0;
            ulong mask = 0;
            c->GetMaterial(&id, &mask);
            return (id & 0x4000) != 0 && (mask & 0x4000) != 0;
        }

        static float DistSqXZ(in Vector3 v1, in Vector3 v2)
        {
            var dX = v2.X - v1.X;
            var dZ = v2.Z - v1.Z;
            return dX * dX + dZ * dZ;
        }
    }
}
