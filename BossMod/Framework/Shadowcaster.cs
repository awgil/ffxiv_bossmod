using BossMod.Interfaces;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision.Math;
using System.Diagnostics;
using VisibilityPolygonCSharp;

using Seg3 = VisibilityPolygonCSharp.Segment<System.Numerics.Vector3>;

namespace BossMod;

internal unsafe class Shadowcaster : IShadowcaster
{
    public DrawStats Stats { get; private set; } = new();

    class VecAdapter : PointAdapter<Vector3>
    {
        public override Vector3 Create(double x, double y) => new((float)x, 0, (float)y);
        public override double GetX(Vector3 point) => point.X;
        public override double GetY(Vector3 point) => point.Z;
    }

    private readonly VisibilityPolygonCSharp<Vector3> Vis = new(new VecAdapter());

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
    }

    public List<Tri> ComputeVisibility2D(Vector3 origin, Vector3 lookDirection, float radius)
    {
        var (poly, _) = ComputeShared(origin, lookDirection, radius);
        var output = new List<Tri>();

        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            output.Add(new(new(origin.XZ()), new(poly[j].XZ()), new(poly[i].XZ())));

        return output;
    }

    public List<IShadowcaster.Triangle> ComputeVisibility3D(Vector3 origin, Vector3 lookDirection, float radius)
    {
        var (poly, normal) = ComputeShared(origin, lookDirection, radius);
        var output = new List<IShadowcaster.Triangle>();

        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            var oj = (poly[j] - origin) with { Y = 0 };
            var projj = oj - Vector3.Dot(oj, normal) * normal;
            var oi = (poly[i] - origin) with { Y = 0 };
            var proji = oi - Vector3.Dot(oi, normal) * normal;
            output.Add(new(origin, origin + projj, origin + proji));
        }

        return output;
    }

    (List<Vector3>, Vector3) ComputeShared(Vector3 origin, Vector3 lookDirection, float radius)
    {
        Stats = new();
        var watch = Stopwatch.StartNew();

        var (segments, normal) = Scan(origin, lookDirection, radius);

        Stats.ScanTime = watch.Elapsed;

        var poly = Vis.Compute(origin, Vis.BreakIntersections(segments));

        Stats.ComputeTime = watch.Elapsed - Stats.ScanTime;

        return (poly, normal);
    }

    (List<Seg3>, Vector3) Scan(Vector3 origin, Vector3 lookDirection, float radius)
    {
        var lookWorld = Matrix4x4.CreateWorld(new(0), lookDirection, new(0, 1, 0));
        var lookNormal = Vector3.Transform(new(0, 1, 0), lookWorld);

        List<Seg3> segments = [];

        var perimeter = CurveApprox.Circle(radius, 0.1f).ToList();
        for (var i = 1; i < perimeter.Count; i++)
            segments.Add(new(perimeter[i - 1].ToVec3() + origin, perimeter[i].ToVec3() + origin));
        segments.Add(new(perimeter[^1].ToVec3() + origin, perimeter[0].ToVec3() + origin));

        foreach (var scene in Framework.Instance()->BGCollisionModule->SceneManager->Scenes)
            ScanTree(scene->Scene->Quadtree, origin, lookNormal, radius, segments);

        return (segments, lookNormal);
    }

    void ScanTree(Quadtree* tree, Vector3 origin, Vector3 lookNormal, float radius, List<Seg3> segments)
    {
        origin.Y += 2;

        var scanner = new Scanner(origin, radius, lookNormal, segments, Stats);

        // TODO: level 0 contains the streaming colliders, which might be duplicated in other layers...
        // figure out if using just streaming suffices since it would likely speed up the scan a lot
        for (var level = 1; level < tree->NumLevels; level++)
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
                            scanner.ProcessColider(coll);
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

    class Scanner(Vector3 origin, float radius, Vector3 lookNormal, List<Seg3> segments, DrawStats stats)
    {
        readonly float Rsq = radius * radius;

        public void ProcessColider(Collider* coll)
        {
            switch (coll->GetColliderType())
            {
                case ColliderType.Mesh:
                    stats.CollMesh++;
                    var collM = (ColliderMesh*)coll;
                    var mesh = (MeshPCB*)collM->Mesh;
                    var world = collM->World;
                    ProcessFileMesh(mesh->RootNode, world);
                    break;
            }
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

        static float DistSqXZ(in Vector3 v1, in Vector3 v2)
        {
            var dX = v2.X - v1.X;
            var dZ = v2.Z - v1.Z;
            return dX * dX + dZ * dZ;
        }
    }
}
