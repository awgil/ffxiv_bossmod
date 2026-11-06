using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision.Math;
using System.Diagnostics;
using VisibilityPolygonCSharp;

namespace BossMod.Debug;

sealed unsafe class DebugRaycast(WorldState ws, Camera camera)
{
    private readonly (Vector3, Vector3)[] ShadowPoints = new (Vector3, Vector3)[25600];

    const float PlayerReach = 25;
    const float PlayerReachSq = PlayerReach * PlayerReach;

    class WPosAdapter : PointAdapter<WPos>
    {
        public override WPos Create(double x, double y) => new((float)x, (float)y);
        public override double GetX(WPos point) => point.X;
        public override double GetY(WPos point) => point.Z;
    }

    private readonly VisibilityPolygonCSharp<WPos> Vis = new(new WPosAdapter());

    struct DrawStats
    {
        public TimeSpan ScanTime;
        public TimeSpan ComputeTime;

        public int TotalCells;
        public int ProcessedCells;
        public int NumTests;
        public int NumTestsPruned;
        public int NumPoints;
    }

    DrawStats _stats;
    Vector3 _origin;
    Vector3 _lookNormal; // perpendicular to look dir (along Y axis)
    Vector3 _lookDir;

    public void Draw()
    {
        var watch = Stopwatch.StartNew();

        DoIt();

        _stats.ScanTime = watch.Elapsed;

        var originXZ = new WPos(_origin.X, _origin.Z);
        List<Segment<WPos>> segments = [];

        void addBlock(WPos center, IEnumerable<WDir> corners)
        {
            var c = corners.ToList();
            for (var i = 0; i < c.Count - 1; i++)
                segments.Add(new(c[i] + center, c[i + 1] + center));
            segments.Add(new(c[^1] + center, c[0] + center));
        }

        addBlock(originXZ, CurveApprox.Circle(30, 0.1f));

        for (var i = 0; i < _stats.NumPoints; i++)
            segments.Add(new(new(ShadowPoints[i].Item1.X, ShadowPoints[i].Item1.Z), new(ShadowPoints[i].Item2.X, ShadowPoints[i].Item2.Z)));

        var seg = Vis.Compute(new WPos(_origin.X, _origin.Z), Vis.BreakIntersections(segments));

        _stats.ComputeTime = watch.Elapsed - _stats.ScanTime;

        for (int i = 0, j = seg.Count - 1; i < seg.Count; j = i++)
        {
            var oj = (seg[j] - originXZ).ToVec3();
            var projj = oj - Vector3.Dot(oj, _lookNormal) * _lookNormal;
            var oi = (seg[i] - originXZ).ToVec3();
            var proji = oi - Vector3.Dot(oi, _lookNormal) * _lookNormal;
            camera.AddTriangle(_origin, _origin + projj, _origin + proji);
        }

        Dalamud.Utility.Util.ShowObject(_stats);
    }

    void DoIt()
    {
        Array.Fill(ShadowPoints, default);
        _stats = default;

        foreach (var scene in Framework.Instance()->BGCollisionModule->SceneManager->Scenes)
            DrawCollision(scene->Scene->Quadtree);
    }

    void DrawCollision(Quadtree* tree)
    {
        if (ws.Party.Player() is not { } player)
            return;

        if (ws.Actors.Find(player.TargetID) is { } target)
        {
            _origin = target.PosRot.XYZ();
            _lookDir = player.PosRot.XYZ() - target.PosRot.XYZ();
            _lookDir /= _lookDir.Length();
        }
        else
        {
            _origin = player.PosRot.XYZ();
            _lookDir = player.Rotation.ToDirection().ToVec3(0);
        }

        _origin.Y += 2;

        var rayWorld = Matrix4x4.CreateWorld(default, _lookDir, new(0, 1, 0));

        _lookNormal = Vector3.Transform(new(0, 1, 0), rayWorld);

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

                var playerXZ = new Vector2(_origin.X, _origin.Z);

                if (OverlapAARect(playerXZ, new(cellX, cellZ), new Vector2(cellX, cellZ) + new Vector2(cellSizeX, cellSizeZ)))
                {
                    _stats.ProcessedCells++;
                    foreach (var coll in node.Colliders)
                        if (coll != null)
                            DrawSolids(coll);
                }
            }
        }
    }

    private void DrawSolids(Collider* coll)
    {
        var type = coll->GetColliderType();

        if (type == ColliderType.Mesh)
        {
            var collM = (ColliderMesh*)coll;
            var mesh = (MeshPCB*)collM->Mesh;
            var world = collM->World;
            DrawAllTriangles(mesh->RootNode, collM->World, 0xFF00FF00);
        }
        else
        {
            //Service.Log.Debug($"unhandled collider type {type}");
        }
    }

    private void DrawAllTriangles(MeshPCB.FileNode* root, in Matrix4x3 world, uint color)
    {
        if (root == null)
            return;

        foreach (ref var prim in root->Primitives)
            DrawOneTriangle(root, prim, world, color);

        if (root->Child1 != null)
            DrawAllTriangles(root->Child1, world, color);
        if (root->Child2 != null)
            DrawAllTriangles(root->Child2, world, color);
    }

    static float DistSqXZ(in Vector3 v1, in Vector3 v2)
    {
        var dX = v2.X - v1.X;
        var dZ = v2.Z - v1.Z;
        return dX * dX + dZ * dZ;
    }

    private void DrawOneTriangle(MeshPCB.FileNode* node, in Mesh.Primitive prim, in Matrix4x3 world, uint color)
    {
        if ((prim.Material & 0x4000) == 0)
            return;

        var v1 = world.TransformCoordinate(node->Vertex(prim.V1));
        var v2 = world.TransformCoordinate(node->Vertex(prim.V2));
        var v3 = world.TransformCoordinate(node->Vertex(prim.V3));

        var ab = v2 - v1;
        var ac = v3 - v1;
        var triNormal = Vector3.Normalize(Vector3.Cross(ab, ac));
        var centroid = (v1 + v2 + v3) / 3f;

        // tri is facing away from player/source object, skip
        if (Vector3.Dot(triNormal, centroid - _origin) >= 0)
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
            ShadowPoints[_stats.NumPoints++] = (verts[0], verts[1]);
    }

    bool PlaneLine(in Vector3 pointA, in Vector3 pointB, out Vector3 intersection)
    {
        intersection = default;

        _stats.NumTests++;

        if (DistSqXZ(pointA, _origin) >= PlayerReachSq && DistSqXZ(pointB, _origin) >= PlayerReachSq)
        {
            _stats.NumTestsPruned++;
            return false;
        }

        var u = pointB - pointA;
        var dot = Vector3.Dot(_lookNormal, u);

        if (MathF.Abs(dot) >= float.Epsilon)
        {
            var w = pointA - _origin;
            var fac = -Vector3.Dot(_lookNormal, w) / dot;
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
