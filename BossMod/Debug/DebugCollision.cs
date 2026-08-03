using Clipper2Lib;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision.Math;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace BossMod;

public static unsafe class CollisionOutlinesExtractor
{
    public sealed class PolygonWithHoles
    {
        public List<Vector3> Outer = [];
        public List<List<Vector3>> Holes = [];
    }

    public enum ClipboardVectorFormat { Vector2XZ, Vector3XYZ }

    public enum MaterialMatchMode
    {
        EffectiveMasked, // current behavior
        EffectiveExact, // effective == wantedId
        PrimMasked, // (raw prim) masked compare
        PrimExact // (raw prim) exact id match
    }

    public static List<PolygonWithHoles> ExtractPolygonsUnion(ColliderMesh* coll, ulong wantedMaterialId, ulong wantedMask,
      float snapEpsXZ = 1e-4f, Vector2 centerXZ = default, float radius = 0f, bool strictRadius = true,
      MaterialMatchMode matchMode = MaterialMatchMode.PrimExact, long scale = 1024 * 1024, float minAreaMeters2 = 1e-6f)
    {
        var res = new List<PolygonWithHoles>();
        if (coll == null || coll->MeshIsSimple || coll->Mesh == null)
        {
            return res;
        }

        // precompute snapping parameters
        var snapInt = ComputeSnapInt(snapEpsXZ, scale);
        var subjects = new Paths64();
        var yLUT = new Dictionary<Point64, float>(1 << 13);
        var edges = new List<EdgeY>(1 << 15);

        CollectSubjectTriangles(coll, wantedMaterialId, wantedMask, centerXZ, radius, strictRadius, matchMode, scale, snapInt, subjects, yLUT, edges);

        if (subjects.Count == 0)
        {
            return res;
        }

        var union = Clipper.Union(subjects, FillRule.NonZero);
        return Paths64ToPolys(union, yLUT, edges, scale, snapInt, minAreaMeters2);
    }

    public static List<PolygonWithHoles> ExtractPolygonsUnionStreamed(ColliderStreamed* streamed, ulong wantedMaterialId,
        ulong wantedMask, float snapEpsXZ = 1e-4f, Vector2 centerXZ = default, float radius = 0f, bool strictRadius = true,
        MaterialMatchMode matchMode = MaterialMatchMode.PrimExact, long scale = 1024 * 1024, float minAreaMeters2 = 1e-6f)
    {
        var res = new List<PolygonWithHoles>();
        if (streamed == null || streamed->Header == null || streamed->Elements == null)
        {
            return res;
        }

        var snapInt = ComputeSnapInt(snapEpsXZ, scale);
        var subjects = new Paths64();
        var yLUT = new Dictionary<Point64, float>(1 << 15);
        var edges = new List<EdgeY>(1 << 17);

        var n = streamed->Header->NumMeshes;
        for (var i = 0; i < n; ++i)
        {
            var elem = streamed->Elements + i;
            var cm = elem->Mesh;
            if (cm == null || cm->MeshIsSimple || cm->Mesh == null)
            {
                continue;
            }

            CollectSubjectTriangles(cm, wantedMaterialId, wantedMask, centerXZ, radius, strictRadius, matchMode, scale, snapInt, subjects, yLUT, edges);
        }

        if (subjects.Count == 0)
        {
            return res;
        }

        var union = Clipper.Union(subjects, FillRule.NonZero);
        return Paths64ToPolys(union, yLUT, edges, scale, snapInt, minAreaMeters2);
    }

    public static List<PolygonWithHoles> ExtractPolygonsUnionMany(
        IReadOnlyList<nint> meshPtrs, ulong wantedMaterialId, ulong wantedMask,
        float snapEpsXZ = 1e-4f, Vector2 centerXZ = default, float radius = 0f, bool strictRadius = true,
        MaterialMatchMode matchMode = MaterialMatchMode.PrimExact, long scale = 1024 * 1024, float minAreaMeters2 = 1e-6f)
    {
        var res = new List<PolygonWithHoles>();
        var countM = meshPtrs.Count;
        if (meshPtrs == null || countM == 0)
        {
            return res;
        }

        var snapInt = ComputeSnapInt(snapEpsXZ, scale);
        var subjects = new Paths64();
        var yLUT = new Dictionary<Point64, float>(1 << 16);
        var edges = new List<EdgeY>(1 << 18);

        for (var i = 0; i < countM; ++i)
        {
            var cm = (ColliderMesh*)meshPtrs[i];
            if (cm == null || cm->MeshIsSimple || cm->Mesh == null)
            {
                continue;
            }

            CollectSubjectTriangles(cm, wantedMaterialId, wantedMask, centerXZ, radius, strictRadius, matchMode, scale, snapInt, subjects, yLUT, edges);
        }

        if (subjects.Count == 0)
        {
            return res;
        }

        var union = Clipper.Union(subjects, FillRule.NonZero);
        return Paths64ToPolys(union, yLUT, edges, scale, snapInt, minAreaMeters2);
    }

    private static void CollectSubjectTriangles(ColliderMesh* coll, ulong wantedId, ulong wantedMask, Vector2 centerXZ, float radius, bool strictRadius, MaterialMatchMode matchMode,
       long scale, long snapInt, Paths64 subjects, Dictionary<Point64, float> yLUT, List<EdgeY> edges)
    {
        var mesh = (MeshPCB*)coll->Mesh;
        var world = coll->World;
        var objMask = coll->Collider.ObjectMaterialMask;
        var objId = coll->Collider.ObjectMaterialValue & objMask;

        CollectNode(mesh->RootNode, ref world, objId, objMask, wantedId, wantedMask, centerXZ, radius, strictRadius, matchMode, scale, snapInt, subjects, yLUT, edges);
    }

    private static void CollectNode(MeshPCB.FileNode* node, ref Matrix4x3 world, ulong objMatId, ulong objMatMask, ulong wantedId, ulong wantedMask,
        Vector2 centerXZ, float radius, bool strictRadius, MaterialMatchMode matchMode, long scale, long snapInt, Paths64 subjects, Dictionary<Point64, float> yLUT, List<EdgeY> edges)
    {
        if (node == null)
        {
            return;
        }

        if (radius > 0f && !OBBXZIntersectsCircle(node->LocalBounds, ref world, centerXZ, radius))
        {
            return;
        }

        var nv = node->NumVertsRaw + node->NumVertsCompressed;
        int np = node->NumPrims;

        if (nv > 0 && np > 0)
        {
            var r2 = radius * radius;
            for (var i = 0; i < np; ++i)
            {
                var prim = node->Primitives[i];
                if (!MatchesMaterial(prim.Material, objMatId, objMatMask, wantedId, wantedMask, matchMode))
                {
                    continue;
                }

                var a = world.TransformCoordinate(node->Vertex(prim.V1));
                var b = world.TransformCoordinate(node->Vertex(prim.V2));
                var c = world.TransformCoordinate(node->Vertex(prim.V3));

                if (radius > 0f)
                {
                    if (strictRadius)
                    {
                        if (!(WithinR2XZ(a, centerXZ, r2) && WithinR2XZ(b, centerXZ, r2) && WithinR2XZ(c, centerXZ, r2)))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!TriPointDistance2LessEqR2(a, b, c, centerXZ, r2))
                        {
                            continue;
                        }
                    }
                }

                // quantize & SNAP to grid to fuse seams
                var pa = Snap(ToP64(a, scale), snapInt);
                var pb = Snap(ToP64(b, scale), snapInt);
                var pc = Snap(ToP64(c, scale), snapInt);

                // drop degenerate after snapping (any duplicates)
                if (pa == pb || pb == pc || pc == pa)
                {
                    continue;
                }

                // enforce CCW in XZ after snap (consistent winding -> no NonZero cancellations)
                EnsureCCW(ref pa, ref pb, ref pc);

                // add triangle
                subjects.Add([pa, pb, pc]);

                // Y lifting LUT + edges for interpolation
                AccumY(yLUT, pa, a.Y);
                AccumY(yLUT, pb, b.Y);
                AccumY(yLUT, pc, c.Y);

                edges.Add(new(pa, pb, a.Y, b.Y));
                edges.Add(new(pb, pc, b.Y, c.Y));
                edges.Add(new(pc, pa, c.Y, a.Y));
            }
        }

        CollectNode(node->Child1, ref world, objMatId, objMatMask, wantedId, wantedMask, centerXZ, radius, strictRadius, matchMode, scale, snapInt, subjects, yLUT, edges);
        CollectNode(node->Child2, ref world, objMatId, objMatMask, wantedId, wantedMask, centerXZ, radius, strictRadius, matchMode, scale, snapInt, subjects, yLUT, edges);
    }

    private static List<PolygonWithHoles> Paths64ToPolys(Paths64 paths, Dictionary<Point64, float> yLUT, List<EdgeY> edges, long scale, long snapInt, float minAreaMeters2)
    {
        // filter slivers (post-union) using area threshold
        var minAreaInt = Math.Max(1.0, minAreaMeters2 * (double)scale * scale);

        var outers = new List<(Path64 path, double area, AABB2 bb)>(paths.Count);
        var holes = new List<(Path64 path, double area, AABB2 bb)>();

        foreach (var p in paths)
        {
            if (p.Count < 3)
            {
                continue;
            }

            // small clean-up: remove consecutive duplicates after union
            var cleaned = RemoveConsecutiveDuplicates(p);
            if (cleaned.Count < 3)
            {
                continue;
            }

            var aInt = Math.Abs(AreaSigned(cleaned));
            if (aInt < minAreaInt)
            {
                continue; // drop tiny fragments
            }

            var aSigned = AreaSigned(cleaned); // signed for orientation
            var bb = BoundsXZ(cleaned);

            if (aSigned > 0d)
            {
                outers.Add((cleaned, aSigned, bb));
            }
            else
            {
                holes.Add((cleaned, aSigned, bb));
            }
        }

        outers.Sort(static (A, B) => B.area.CompareTo(A.area));
        holes.Sort(static (A, B) => Math.Abs(B.area).CompareTo(Math.Abs(A.area)));

        var countO = outers.Count;
        var countH = holes.Count;
        var res = new List<PolygonWithHoles>(countO + countH);

        for (var i = 0; i < countO; ++i)
        {
            var poly = new PolygonWithHoles();
            var outer = outers[i];
            PolyToVectorsCCW(outer.path, yLUT, edges, scale, poly.Outer);

            for (var h = 0; h < countH; ++h)
            {
                var hol = holes[h];
                if (hol.path == null)
                {
                    continue;
                }
                if (!outer.bb.Contains(hol.bb))
                {
                    continue;
                }

                var centroid = Centroid(hol.path, scale);
                if (PointInPolygonXZ(centroid, outer.path))
                {
                    var hole = new List<Vector3>();
                    PolyToVectorsCCW(hol.path, yLUT, edges, scale, hole);
                    poly.Holes.Add(hole);
                    holes[h] = (null!, 0, default);
                }
            }
            res.Add(poly);
        }

        for (var i = 0; i < countH; ++i)
        {
            var h = holes[i];
            if (h.path == null)
            {
                continue;
            }
            var poly = new PolygonWithHoles();
            PolyToVectorsCCW(h.path, yLUT, edges, scale, poly.Outer);
            res.Add(poly);
        }
        return res;
    }

    private static Path64 RemoveConsecutiveDuplicates(Path64 p)
    {
        if (p.Count <= 2)
        {
            return p;
        }
        var count = p.Count;
        var outp = new Path64(count);
        Point64 prev = new(long.MinValue, long.MinValue);
        for (var i = 0; i < count; ++i)
        {
            var pi = p[i];
            if (i == 0 || pi.X != prev.X || pi.Y != prev.Y)
            {
                outp.Add(pi);
            }
            prev = pi;
        }
        // also check last==first
        var countO = outp.Count;
        if (countO >= 2 && outp[0] == outp[^1])
        {
            outp.RemoveAt(countO - 1);
        }
        return outp;
    }

    private static void PolyToVectorsCCW(Path64 path, Dictionary<Point64, float> yLUT, List<EdgeY> edges, long scale, List<Vector3> dst)
    {
        if (AreaSigned(path) < 0d)
        {
            path.Reverse();
        }
        var count = path.Count;
        dst.Capacity = Math.Max(dst.Capacity, count);
        for (var i = 0; i < count; ++i)
        {
            var p = path[i];
            var y = SampleY(p, yLUT, edges);
            dst.Add(new Vector3(p.X / (float)scale, y, p.Y / (float)scale));
        }
    }

    public static string FormatForClipboard(List<PolygonWithHoles> polys, ClipboardVectorFormat fmt, int decimals = 5)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var f = "F" + decimals;

        string Vec2(Vector3 v) => $"new({v.X.ToString(f, ci)}f, {v.Z.ToString(f, ci)}f)";
        string Vec3(Vector3 v) => $"new({v.X.ToString(f, ci)}f, {v.Y.ToString(f, ci)}f, {v.Z.ToString(f, ci)}f)";

        string V(Vector3 v) => fmt == ClipboardVectorFormat.Vector2XZ ? Vec2(v) : Vec3(v);

        var sb = new StringBuilder();
        sb.AppendLine(fmt == ClipboardVectorFormat.Vector2XZ ? "var outers = new WPos[][] {" : "var outers = new System.Numerics.Vector3[][] {");

        var countP = polys.Count;
        for (var i = 0; i < countP; ++i)
        {
            sb.Append("    new[] { ");
            var poly = polys[i];
            var countO = poly.Outer.Count;
            for (var j = 0; j < countO; ++j)
            {
                sb.Append(V(poly.Outer[j]));
                if (j + 1 < countO)
                {
                    sb.Append(", ");
                }
            }
            sb.AppendLine(i + 1 < countP ? " }," : " }");
        }
        sb.AppendLine("};");

        sb.AppendLine(fmt == ClipboardVectorFormat.Vector2XZ ? "var holes = new WPos[][][] {" : "var holes = new System.Numerics.Vector3[][][] {");

        for (var i = 0; i < countP; ++i)
        {
            sb.Append("    new[] { ");
            var poly = polys[i];
            var countH = poly.Holes.Count;
            for (var h = 0; h < countH; ++h)
            {
                var hole = poly.Holes[h];
                sb.Append("new[] { ");
                var countH2 = hole.Count;
                for (var j = 0; j < countH2; j++)
                {
                    sb.Append(V(hole[j]));
                    if (j + 1 < hole.Count)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(" }");
                if (h + 1 < countH)
                {
                    sb.Append(", ");
                }
            }
            sb.AppendLine(i + 1 < countP ? " }," : " }");
        }
        sb.AppendLine("};");
        return sb.ToString();
    }

    private static bool MatchesMaterial(ulong primMaterial, ulong objMatId, ulong objMatMask, ulong wantedId, ulong wantedMask, MaterialMatchMode mode)
    {
        var prim = primMaterial;
        var effective = (prim & ~objMatMask) | (objMatId & objMatMask);

        return mode switch
        {
            MaterialMatchMode.PrimExact => prim == wantedId,
            MaterialMatchMode.PrimMasked => ((prim ^ wantedId) & wantedMask) == 0UL,
            MaterialMatchMode.EffectiveExact => effective == wantedId,
            MaterialMatchMode.EffectiveMasked => ((effective ^ wantedId) & wantedMask) == 0UL,
            _ => false
        };
    }
    private static bool WithinR2XZ(in Vector3 v, in Vector2 c, float r2)
    {
        float dx = v.X - c.X, dz = v.Z - c.Y;
        return dx * dx + dz * dz <= r2;
    }

    private static Point64 ToP64(in Vector3 v, long s) => new((long)Math.Round(v.X * s), (long)Math.Round(v.Z * s));

    private static void AccumY(Dictionary<Point64, float> lut, Point64 p, float y) => lut[p] = lut.TryGetValue(p, out var cur) ? (cur + y) * 0.5f : y;

    private readonly struct EdgeY
    {
        public readonly Point64 A, B; public readonly float YA, YB;
        public EdgeY(Point64 a, Point64 b, float ya, float yb)
        {
            // normalize key order to make on-seg checks stable
            if (a.X > b.X || a.X == b.X && a.Y > b.Y)
            {
                (a, b) = (b, a);
                (ya, yb) = (yb, ya);
            }
            A = a;
            B = b;
            YA = ya;
            YB = yb;
        }
    }

    private static float SampleY(Point64 p, Dictionary<Point64, float> lut, List<EdgeY> edges)
    {
        if (lut.TryGetValue(p, out var y))
        {
            return y;
        }

        // check if lies on any recorded edge (exact integer colinearity)
        var count = edges.Count;
        for (int i = 0, n = count; i < n; ++i)
        {
            var e = edges[i];
            if (!OnSegment(p, e.A, e.B))
            {
                continue;
            }

            // param t along the dominant axis
            long dx = e.B.X - e.A.X, dz = e.B.Y - e.A.Y;
            var t = Math.Abs(dx) >= Math.Abs(dz) ? dx == 0 ? 0 : (p.X - e.A.X) / (double)dx : dz == 0 ? 0 : (p.Y - e.A.Y) / (double)dz;
            return (float)(e.YA + t * (e.YB - e.YA));
        }

        // fallback: nearest known vertex
        var bestY = 0f;
        var bestD2 = double.MaxValue;
        foreach (var kv in lut)
        {
            double ddx = kv.Key.X - p.X, ddz = kv.Key.Y - p.Y;
            var d2 = ddx * ddx + ddz * ddz;
            if (d2 < bestD2)
            {
                bestD2 = d2;
                bestY = kv.Value;
            }
        }
        return bestY;
    }

    private static long ComputeSnapInt(float snapEpsXZ, long scale)
    {
        if (!(snapEpsXZ > 0f))
        {
            return 1L;
        }
        var k = (long)Math.Round(snapEpsXZ * scale);
        return Math.Max(1, k);
    }

    private static Point64 Snap(Point64 p, long snapInt)
    {
        if (snapInt <= 1)
        {
            return p;
        }
        static long RoundToMultiple(long v, long m)
        {
            // nearest multiple of m
            var half = m >> 1;
            return v >= 0L ? (v + half) / m * m : (v - half) / m * m;
        }
        return new Point64(RoundToMultiple(p.X, snapInt), RoundToMultiple(p.Y, snapInt));
    }

    private static void EnsureCCW(ref Point64 a, ref Point64 b, ref Point64 c)
    {
        var cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        if (cross < 0L)
        {
            (b, c) = (c, b);
        }
    }

    private static bool OnSegment(in Point64 p, in Point64 a, in Point64 b)
    {
        var cross = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
        if (cross != 0L)
        {
            return false;
        }
        long minX = Math.Min(a.X, b.X), maxX = Math.Max(a.X, b.X);
        long minY = Math.Min(a.Y, b.Y), maxY = Math.Max(a.Y, b.Y);
        return p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY;
    }

    private static double AreaSigned(Path64 p)
    {
        long a = 0;
        var n = p.Count;
        for (int i = 0, j = n - 1; i < n; j = ++i)
        {
            a += p[j].X * p[i].Y - p[i].X * p[j].Y;
        }
        return 0.5 * a;
    }

    private static AABB2 BoundsXZ(Path64 p)
    {
        long minX = long.MaxValue, minZ = long.MaxValue, maxX = long.MinValue, maxZ = long.MinValue;
        var count = p.Count;
        for (var i = 0; i < count; ++i)
        {
            var pt = p[i];
            if (pt.X < minX)
            {
                minX = pt.X;
            }
            if (pt.X > maxX)
            {
                maxX = pt.X;
            }
            if (pt.Y < minZ)
            {
                minZ = pt.Y;
            }
            if (pt.Y > maxZ)
            {
                maxZ = pt.Y;
            }
        }
        return new AABB2(minX, minZ, maxX, maxZ);
    }

    private static Vector2 Centroid(Path64 p, long scale)
    {
        double cx = 0, cz = 0;
        var n = p.Count;
        for (var i = 0; i < n; ++i)
        {
            cx += p[i].X;
            cz += p[i].Y;
        }
        return new Vector2((float)(cx / n / scale), (float)(cz / n / scale));
    }

    private static bool PointInPolygonXZ(Vector2 p, Path64 path)
    {
        var inside = false;
        var n = path.Count;
        double px = p.X, pz = p.Y;
        for (int i = 0, j = n - 1; i < n; j = ++i)
        {
            var vi = path[i];
            var vj = path[j];
            double xi = vi.X, zi = vi.Y, xj = vj.X, zj = vj.Y;
            var inter = (zi > pz) != (zj > pz) && px < (xj - xi) * (pz - zi) / ((zj - zi) == 0 ? double.Epsilon : (zj - zi)) + xi;
            if (inter)
            {
                inside = !inside;
            }
        }
        return inside;
    }

    // true if min distance (in XZ) from center to triangle <= R
    private static bool TriPointDistance2LessEqR2(in Vector3 v1, in Vector3 v2, in Vector3 v3, in Vector2 c, float r2)
    {
        var a = new Vector2(v1.X, v1.Z);
        var b = new Vector2(v2.X, v2.Z);
        var d = new Vector2(v3.X, v3.Z);

        // inside → distance 0
        if (PointInTri2(c, a, b, d))
        {
            return true;
        }

        // else min distance to edges
        var d2 = Math.Min(Dist2PointSeg(c, a, b), Math.Min(Dist2PointSeg(c, b, d), Dist2PointSeg(c, d, a)));
        return d2 <= r2;
    }

    private static float Dist2PointSeg(in Vector2 p, in Vector2 a, in Vector2 b)
    {
        var ab = b - a;
        var len2 = ab.X * ab.X + ab.Y * ab.Y;
        if (len2 <= float.Epsilon)
        {
            var dx = p.X - a.X;
            var dy = p.Y - a.Y;
            return dx * dx + dy * dy;
        }
        var t = ((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / len2;
        t = MathF.Max(0, MathF.Min(1, t));
        var q = new Vector2(a.X + t * ab.X, a.Y + t * ab.Y);
        float dx2 = p.X - q.X, dy2 = p.Y - q.Y;
        return dx2 * dx2 + dy2 * dy2;
    }

    private static bool PointInTri2(in Vector2 p, in Vector2 a, in Vector2 b, in Vector2 c)
    {
        // barycentric sign test
        var s1 = Sign(p, a, b) >= 0;
        var s2 = Sign(p, b, c) >= 0;
        var s3 = Sign(p, c, a) >= 0;
        return s1 == s2 && s2 == s3;
    }

    private static float Sign(in Vector2 p1, in Vector2 p2, in Vector2 p3) => (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);

    private static bool OBBXZIntersectsCircle(in AABB local, ref Matrix4x3 world, in Vector2 c, float r)
    {
        var localMinX = local.Min.X;
        var localMinY = local.Min.Y;
        var localMinZ = local.Min.Z;
        var localMaxX = local.Max.X;
        var localMaxY = local.Max.Y;
        var localMaxZ = local.Max.Z;

        // Project the 8 OBB corners to XZ, build an AABB in XZ, then circle-test
        var aaa = world.TransformCoordinate(new(localMinX, localMinY, localMinZ));
        var aab = world.TransformCoordinate(new(localMinX, localMinY, localMaxZ));
        var aba = world.TransformCoordinate(new(localMinX, localMaxY, localMinZ));
        var abb = world.TransformCoordinate(new(localMinX, localMaxY, localMaxZ));
        var baa = world.TransformCoordinate(new(localMaxX, localMinY, localMinZ));
        var bab = world.TransformCoordinate(new(localMaxX, localMinY, localMaxZ));
        var bba = world.TransformCoordinate(new(localMaxX, localMaxY, localMinZ));
        var bbb = world.TransformCoordinate(new(localMaxX, localMaxY, localMaxZ));

        var minX = Math.Min(Math.Min(Math.Min(aaa.X, aab.X), Math.Min(aba.X, abb.X)), Math.Min(Math.Min(baa.X, bab.X), Math.Min(bba.X, bbb.X)));
        var maxX = Math.Max(Math.Max(Math.Max(aaa.X, aab.X), Math.Max(aba.X, abb.X)), Math.Max(Math.Max(baa.X, bab.X), Math.Max(bba.X, bbb.X)));
        var minZ = Math.Min(Math.Min(Math.Min(aaa.Z, aab.Z), Math.Min(aba.Z, abb.Z)), Math.Min(Math.Min(baa.Z, bab.Z), Math.Min(bba.Z, bbb.Z)));
        var maxZ = Math.Max(Math.Max(Math.Max(aaa.Z, aab.Z), Math.Max(aba.Z, abb.Z)), Math.Max(Math.Max(baa.Z, bab.Z), Math.Max(bba.Z, bbb.Z)));

        return RectXZIntersectsCircle(minX, minZ, maxX, maxZ, c, r);
    }

    private static bool RectXZIntersectsCircle(float minX, float minZ, float maxX, float maxZ, in Vector2 c, float r)
    {
        var dx = 0f;
        if (c.X < minX)
        {
            dx = minX - c.X;
        }
        else if (c.X > maxX)
        {
            dx = c.X - maxX;
        }
        var dz = 0f;
        if (c.Y < minZ)
        {
            dz = minZ - c.Y;
        }
        else if (c.Y > maxZ)
        {
            dz = c.Y - maxZ;
        }
        return (dx * dx + dz * dz) <= r * r;
    }

    private readonly struct AABB2(float minX, float minZ, float maxX, float maxZ)
    {
        public readonly float MinX = minX, MinZ = minZ, MaxX = maxX, MaxZ = maxZ;

        public bool Contains(AABB2 o) => o.MinX >= MinX && o.MaxX <= MaxX && o.MinZ >= MinZ && o.MaxZ <= MaxZ;
    }
}

public sealed unsafe class DebugCollision() : IDisposable
{
    private readonly UITree _tree = new();
    private BitMask _shownLayers = new(1);
    private BitMask _materialMask;
    private BitMask _materialId;
    private bool _showZeroLayer = true;
    private bool _showOnlyFlagRaycast;
    private bool _showOnlyFlagVisit;

    private readonly HashSet<nint> _streamedMeshes = [];
    private BitMask _availableLayers;
    private BitMask _availableMaterials;

    private CollisionOutlinesExtractor.MaterialMatchMode _exportMatchMode = CollisionOutlinesExtractor.MaterialMatchMode.EffectiveMasked;
    private float _exportRadiusXZ = 0f; // 0 => whole mesh/streamed
    private bool _exportStrictRadius = true;
    private float _exportSnapEpsXZ = 1e-5f;
    private float _exportMinArea = 1e-6f;

    private string _exportMeshIdListHex = "";
    private bool _selectionMode = false;
    private readonly HashSet<ulong> _meshSelection = [];

    private static readonly (int, int)[] _boxEdges =
    [
        (0, 1), (1, 3), (3, 2), (2, 0),
        (4, 5), (5, 7), (7, 6), (6, 4),
        (0, 4), (1, 5), (2, 6), (3, 7)
    ];

    private static readonly Vector3[] _boxCorners =
    [
        new(-1, -1, -1),
        new(-1, -1,  1),
        new(-1,  1, -1),
        new(-1,  1,  1),
        new( 1, -1, -1),
        new( 1, -1,  1),
        new( 1,  1, -1),
        new( 1,  1,  1),
    ];

    private float _maxDrawDistance = 10;

    public void Dispose() { }

    public void Draw()
    {
        var module = Framework.Instance()->BGCollisionModule;
        ImGui.TextUnformatted($"Module: {(nint)module:X}->{(nint)module->SceneManager:X} ({module->SceneManager->NumScenes} scenes, {module->LoadInProgressCounter} loads)");
        ImGui.TextUnformatted($"Streaming: {SphereStr(module->ForcedStreamingSphere)} / {SphereStr(module->SceneManager->StreamingSphere)}");
        module->ForcedStreamingSphere.W = _maxDrawDistance;

        GatherInfo();
        DrawSettings();

        var i = 0;
        foreach (var s in module->SceneManager->Scenes)
        {
            DrawSceneColliders(s->Scene, i);
            DrawSceneQuadtree(s->Scene->Quadtree, i);
            ++i;
        }
    }

    private void GatherInfo()
    {
        _streamedMeshes.Clear();
        _availableLayers.Reset();
        _availableMaterials.Reset();
        foreach (var s in Framework.Instance()->BGCollisionModule->SceneManager->Scenes)
        {
            foreach (var coll in s->Scene->Colliders)
            {
                _availableLayers |= new BitMask(coll->LayerMask);
                _availableMaterials |= new BitMask(coll->ObjectMaterialValue);

                var collType = coll->GetColliderType();
                if (collType == ColliderType.Streamed)
                {
                    var cast = (ColliderStreamed*)coll;
                    if (cast->Header != null && cast->Elements != null)
                    {
                        for (var i = 0; i < cast->Header->NumMeshes; ++i)
                        {
                            var m = cast->Elements[i].Mesh;
                            if (m != null)
                            {
                                _streamedMeshes.Add((nint)m);
                            }
                        }
                    }
                }
                else if (collType == ColliderType.Mesh)
                {
                    var cast = (ColliderMesh*)coll;
                    if (!cast->MeshIsSimple && cast->Mesh != null)
                    {
                        var mesh = (MeshPCB*)cast->Mesh;
                        var mask = new BitMask(coll->ObjectMaterialMask);
                        GatherMeshNodeMaterials(mesh->RootNode, ~mask);
                    }
                }
            }
        }
    }

    private bool FilterCollider(Collider* coll)
    {
        // mayer & flag filters
        if (coll->LayerMask == 0 ? !_showZeroLayer : (_shownLayers.Raw & coll->LayerMask) == 0)
        {
            return false;
        }
        if (_showOnlyFlagRaycast && (coll->VisibilityFlags & 1) == 0)
        {
            return false;
        }
        if (_showOnlyFlagVisit && (coll->VisibilityFlags & 2) == 0)
        {
            return false;
        }

        // material filter currently selected in the UI
        var maskActiveBits = (_availableMaterials & _materialMask).Raw;
        if (maskActiveBits == 0ul)
        {
            return true; // no active material filter
        }

        var type = coll->GetColliderType();

        // mesh & streamed are containers; material filtering happens per-triangle when drawing/exporting
        // Never drop them here just because their object-level material doesn't match
        if (type is ColliderType.Mesh or ColliderType.Streamed)
        {
            return true;
        }

        // for simple/primitive colliders, compare the object's material to the UI selection
        // keep the ones whose (objectMaterial ^ wantedId) has no differences inside the active mask bits
        var wantedId = _materialId.Raw;
        var objValue = coll->ObjectMaterialValue; // primitives only have object-level material
        return ((objValue ^ wantedId) & maskActiveBits) == 0UL;
    }

    private readonly CollisionOutlinesExtractor.MaterialMatchMode[] modes = Enum.GetValues<CollisionOutlinesExtractor.MaterialMatchMode>();

    private void DrawSettings()
    {
        using var n = _tree.Node2("Settings");
        if (!n.Opened)
        {
            return;
        }

        ImGui.Checkbox("Show objects with zero layer", ref _showZeroLayer);
        {
            var shownLayers = _availableLayers & _shownLayers;
            using var layers = ImRaii.Combo("Shown layers", shownLayers == _availableLayers ? "All" : shownLayers.None() ? "None" : string.Join(", ", shownLayers.SetBits()));
            if (layers)
            {
                foreach (var i in _availableLayers.SetBits())
                {
                    var shown = _shownLayers[i];
                    if (ImGui.Checkbox($"Layer {i}", ref shown))
                    {
                        _shownLayers[i] = shown;
                    }
                }
            }
        }

        {
            var matMask = _materialMask & _availableMaterials;
            using var materials = ImRaii.Combo("Material mask", matMask.None() ? "None" : matMask.Raw.ToString("X"));
            if (materials)
            {
                foreach (var i in _availableMaterials.SetBits())
                {
                    var filter = _materialMask[i];
                    if (ImGui.Checkbox($"Material {1u << i:X16}", ref filter))
                    {
                        _materialMask[i] = filter;
                    }
                }
            }
        }

        {
            var matId = _materialId & _availableMaterials;
            using var materials = ImRaii.Combo("Material id", matId.None() ? "None" : matId.Raw.ToString("X"));
            if (materials)
            {
                foreach (var i in _availableMaterials.SetBits())
                {
                    var filter = _materialId[i];
                    if (ImGui.Checkbox($"Material {1u << i:X16}", ref filter))
                    {
                        _materialId[i] = filter;
                    }
                }
            }
        }

        {
            using var flags = ImRaii.Combo("Flag filter", _showOnlyFlagRaycast ? _showOnlyFlagVisit ? "Only when both flags are set" : "Only if raycast flag is set" : _showOnlyFlagVisit ? "Only if global visit flag is set" : "Show everything");
            if (flags)
            {
                ImGui.Checkbox("Hide objects without raycast flag (0x1)", ref _showOnlyFlagRaycast);
                ImGui.Checkbox("Hide objects without global viist flag (0x2)", ref _showOnlyFlagVisit);
            }
        }

        ImGui.SliderFloat("Max Draw Distance", ref _maxDrawDistance, 10f, 1000f, "%.0f");

        using var ex = _tree.Node2("Export / Union settings");
        if (ex.Opened)
        {
            // Material match
            var label = _exportMatchMode.ToString();
            if (ImGui.BeginCombo("Material match", label))
            {
                for (var i = 0; i < 4; ++i)
                {
                    var mode = modes[i];
                    var sel = mode == _exportMatchMode;
                    if (ImGui.Selectable(mode.ToString(), sel))
                    {
                        _exportMatchMode = mode;
                    }
                    if (sel)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }

            // radius & strict mode
            ImGui.SliderFloat("Radius XZ (yalm, 0 = whole)", ref _exportRadiusXZ, 0f, 200f, "%.1f");
            ImGui.Checkbox("Strict radius (all tri verts inside)", ref _exportStrictRadius);

            // snapping epsilon (pre-union)
            ImGui.SliderFloat("Snap Eps XZ (yalm)", ref _exportSnapEpsXZ, 0f, 0.05f, "%.6f");

            // drop tiny fragments after union
            ImGui.SliderFloat("Min area (yalm^2)", ref _exportMinArea, 0f, 0.01f, "%.6f");

            ImGui.Separator();
            ImGui.Checkbox("Selection mode (multi-pick)", ref _selectionMode);
            ImGui.SameLine();
            ImGui.TextDisabled($"Selected: {_meshSelection.Count}");

            ImGui.BeginDisabled(_meshSelection.Count == 0);
            if (ImGui.Button("Append selected → mesh-id list"))
            {
                AppendIdsToList(ref _exportMeshIdListHex, _meshSelection);
            }
            ImGui.SameLine();
            if (ImGui.Button("Clear selection"))
            {
                _meshSelection.Clear();
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Select all visible"))
            {
                SelectAllVisibleMeshes(add: true);
            }
            ImGui.SameLine();
            if (ImGui.Button("Deselect all visible"))
            {
                SelectAllVisibleMeshes(add: false);
            }

            ImGui.Separator();
            ImGui.TextDisabled("Merge by mesh-id list (hex pointers, any separators: , ; space tab newline)");
            ImGui.InputTextMultiline("MeshId list (hex)", ref _exportMeshIdListHex, 4096, new Vector2(-1, ImGui.GetTextLineHeight() * 6));
            var (okList, meshIds) = TryParseHexU64List(_exportMeshIdListHex);
            ImGui.Text(okList ? $"Parsed {meshIds.Count} unique mesh ids" : "Enter hex values like 0x7FFB1234ABCD0000");

            ImGui.BeginDisabled(!okList || meshIds.Count == 0);
            if (ImGui.Button("Copy polygons (WPos, mesh-id list)"))
            {
                ExportPolysByMeshIdList(meshIds, CollisionOutlinesExtractor.ClipboardVectorFormat.Vector2XZ);
            }

            ImGui.SameLine();
            if (ImGui.Button("Copy polygons (Vector3, mesh-id list)"))
            {
                ExportPolysByMeshIdList(meshIds, CollisionOutlinesExtractor.ClipboardVectorFormat.Vector3XYZ);
            }

            ImGui.EndDisabled();
        }
    }

    private void DrawSceneColliders(Scene* s, int index)
    {
        using var n = _tree.Node2($"Scene {index}: {s->NumColliders} colliders, {s->NumLoading} loading, streaming={SphereStr(s->StreamingSphere)}###scene_{index}");
        if (n.SelectedOrHovered)
        {
            foreach (var coll in s->Colliders)
            {
                if (FilterCollider(coll))
                {
                    VisualizeCollider(coll, _materialId, _materialMask);
                }
            }
        }

        if (n.Opened)
        {
            foreach (var coll in s->Colliders)
            {
                DrawCollider(coll);
            }
        }
    }

    private void DrawSceneQuadtree(Quadtree* tree, int index)
    {
        using var n = _tree.Node2($"Quadtree {index}: {tree->NumLevels} levels ([{tree->MinX}, {tree->MaxX}]x[{tree->MinZ}, {tree->MaxZ}], leaf {tree->LeafSizeX}x{tree->LeafSizeZ}), {tree->NumNodes} nodes###tree_{index}");
        if (!n.Opened)
        {
            return;
        }

        for (var level = 0; level < tree->NumLevels; ++level)
        {
            var cellSizeX = (tree->MaxX - tree->MinX + 1) / (1 << level);
            var cellSizeZ = (tree->MaxZ - tree->MinZ + 1) / (1 << level);
            using var ln = _tree.Node2($"Level {level}, {cellSizeX}x{cellSizeZ} cells ({Quadtree.NumNodesAtLevel(level)} nodes starting at {Quadtree.StartingNodeForLevel(level)})");
            if (!ln.Opened)
            {
                continue;
            }

            var nodes = tree->NodesAtLevel(level);
            for (var i = 0; i < nodes.Length; ++i)
            {
                ref var node = ref nodes[i];
                if (node.Node.NodeLink.Next == null)
                {
                    continue;
                }

                var coord = Quadtree.CellCoords((uint)i);
                var cellX = tree->MinX + coord.x * cellSizeX;
                var cellZ = tree->MinZ + coord.z * cellSizeZ;
                using var cn = _tree.Node2($"[{coord.x}, {coord.z}] ([{cellX}x{cellZ}]-[{cellX + cellSizeX}x{cellZ + cellSizeZ}])###node_{level}_{i}", node.Node.NodeLink.Next == null);

                if (cn.Opened)
                {
                    foreach (var coll in node.Colliders)
                    {
                        DrawCollider(coll);
                    }
                }

                if (cn.SelectedOrHovered)
                {
                    // TODO: visualize cell bounds?
                    foreach (var coll in node.Colliders)
                    {
                        VisualizeCollider(coll, _materialId, _materialMask);
                    }
                }
            }
        }
    }

    private void DrawCollider(Collider* coll)
    {
        if (!FilterCollider(coll))
        {
            return;
        }

        var raycastFlag = (coll->VisibilityFlags & 1) != 0;
        var globalVisitFlag = (coll->VisibilityFlags & 2) != 0;
        var flagsText = raycastFlag ? globalVisitFlag ? "raycast, global visit" : "raycast" : globalVisitFlag ? "global visit" : "none";

        var type = coll->GetColliderType();
        var color = Colors.TextColor1;
        if (type == ColliderType.Mesh)
        {
            var collMesh = (ColliderMesh*)coll;
            if (_streamedMeshes.Contains((nint)coll))
            {
                color = Colors.TextColor4;
            }
            else if (collMesh->MeshIsSimple)
            {
                color = Colors.TextColor3;
            }
        }
        using var n = _tree.Node2($"{type} {(nint)coll:X}, layers={coll->LayerMask:X8}, layout-id={coll->LayoutObjectId:X16}, refs={coll->NumRefs}, material={coll->ObjectMaterialValue:X}/{coll->ObjectMaterialMask:X}, flags={flagsText}###{(nint)coll:X}", false, color);
        if (_selectionMode && type == ColliderType.Mesh)
        {
            var cm = (ColliderMesh*)coll;
            if (cm->Mesh != null && !cm->MeshIsSimple)
            {
                var ptr = (ulong)(nuint)cm;
                var sel = _meshSelection.Contains(ptr);
                ImGui.SameLine();
                if (ImGui.Checkbox($"##sel_mesh_{(nint)coll:X}", ref sel))
                {
                    if (sel)
                    {
                        _meshSelection.Add(ptr);
                    }
                    else
                    {
                        _meshSelection.Remove(ptr);
                    }
                }
                ImGui.SameLine();
                ImGui.TextDisabled("pick");
            }
        }
        if (ImGui.BeginPopupContextItem($"###{(nint)coll:X}"))
        {
            ContextCollider(coll);
            if (type == ColliderType.Mesh)
            {
                ImGui.TextDisabled("Mesh Id (pointer)");
                var cm = (ColliderMesh*)coll;
                var ptr = (ulong)(nuint)cm;
                if (ImGui.MenuItem("Copy mesh id (hex)"))
                {
                    ImGui.SetClipboardText(FormatHexU64(ptr));
                }

                if (ImGui.MenuItem("Append mesh id to list"))
                {
                    AppendIdToList(ref _exportMeshIdListHex, ptr.ToString("X16"));
                }
            }
            ImGui.EndPopup();
        }
        if (n.SelectedOrHovered)
        {
            VisualizeCollider(coll, _materialId, _materialMask);
        }

        if (!n.Opened)
        {
            return;
        }

        _tree.LeafNode2($"Raw flags: {coll->VisibilityFlags:X}");
        switch (type)
        {
            case ColliderType.Streamed:
                {
                    var cast = (ColliderStreamed*)coll;
                    DrawResource(cast->Resource);
                    var path = cast->PathBaseString;
                    _tree.LeafNode2($"Path: {path}/{Encoding.UTF8.GetString(cast->PathBase[(path.Length + 1)..])}");
                    _tree.LeafNode2($"Streamed: [{cast->StreamedMinX:f3}x{cast->StreamedMinZ:f3}] - [{cast->StreamedMaxX:f3}x{cast->StreamedMaxZ:f3}]");
                    _tree.LeafNode2($"Loaded: {cast->Loaded} ({cast->NumMeshesLoading} meshes load in progress)");
                    if (cast->Header != null && cast->Entries != null && cast->Elements != null)
                    {
                        var headerRaw = (float*)cast->Header;
                        _tree.LeafNode2($"Header: meshes={cast->Header->NumMeshes}, u={headerRaw[1]:f3} {headerRaw[2]:f3} {headerRaw[3]:f3} {headerRaw[4]:f3} {headerRaw[5]:f3} {headerRaw[6]:f3} {headerRaw[7]:f3}");
                        for (var i = 0; i < cast->Header->NumMeshes; ++i)
                        {
                            var entry = cast->Entries + i;
                            var elem = cast->Elements + i;
                            var entryRaw = (uint*)entry;
                            using var mn = _tree.Node2($"Mesh {i}: file=tr{entry->MeshId:d4}.pcb, bounds={AABBStr(entry->Bounds)} == {(nint)elem->Mesh:X}###mesh_{i}", elem->Mesh == null);

                            if (mn.SelectedOrHovered && elem->Mesh != null)
                            {
                                VisualizeCollider(&elem->Mesh->Collider, _materialId, _materialMask);
                            }

                            if (mn.Opened)
                            {
                                DrawColliderMesh(elem->Mesh);
                            }
                        }
                    }
                }
                break;
            case ColliderType.Mesh:
                DrawColliderMesh((ColliderMesh*)coll);
                break;
            case ColliderType.Box:
                {
                    var cast = (ColliderBox*)coll;
                    _tree.LeafNode2($"Translation: {Vec3Str(cast->Translation)}");
                    var rotation = cast->Rotation;
                    _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref cast->World)}°)");
                    _tree.LeafNode2($"Scale: {Vec3Str(cast->Scale)}");
                    DrawMat4x3("World", ref cast->World);
                    DrawMat4x3("InvWorld", ref cast->InvWorld);
                }
                break;
            case ColliderType.Cylinder:
                {
                    var cast = (ColliderCylinder*)coll;
                    _tree.LeafNode2($"Translation: {Vec3Str(cast->Translation)}");
                    var rotation = cast->Rotation;
                    _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref cast->World)}°)");
                    _tree.LeafNode2($"Scale: {Vec3Str(cast->Scale)}");
                    _tree.LeafNode2($"Radius: {cast->Radius:f3}");
                    DrawMat4x3("World", ref cast->World);
                    DrawMat4x3("InvWorld", ref cast->InvWorld);
                }
                break;
            case ColliderType.Sphere:
                {
                    var cast = (ColliderSphere*)coll;
                    _tree.LeafNode2($"Translation: {Vec3Str(cast->Translation)}");
                    var rotation = cast->Rotation;
                    _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref cast->World)}°)");
                    _tree.LeafNode2($"Scale: {Vec3Str(cast->Scale)}");
                    DrawMat4x3("World", ref cast->World);
                    DrawMat4x3("InvWorld", ref cast->InvWorld);
                }
                break;
            case ColliderType.Plane:
            case ColliderType.PlaneTwoSided:
                {
                    var cast = (ColliderPlane*)coll;
                    _tree.LeafNode2($"Normal: {cast->World.Row2 / cast->Scale.Z:f3}");
                    _tree.LeafNode2($"Translation: {Vec3Str(cast->Translation)}");
                    var rotation = cast->Rotation;
                    _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref cast->World)}°)");
                    _tree.LeafNode2($"Scale: {Vec3Str(cast->Scale)}");
                    DrawMat4x3("World", ref cast->World);
                    DrawMat4x3("InvWorld", ref cast->InvWorld);
                }
                break;
        }
    }

    private void DrawColliderMesh(ColliderMesh* coll)
    {
        DrawResource(coll->Resource);
        _tree.LeafNode2($"Translation: {Vec3Str(coll->Translation)}");
        var rotation = coll->Rotation;
        _tree.LeafNode2($"Rotation: {Vec3Str(rotation)} (Yaw: {HeadingDegFromWorld(ref coll->World)}°)");
        _tree.LeafNode2($"Scale: {Vec3Str(coll->Scale)}");
        DrawMat4x3("World", ref coll->World);
        DrawMat4x3("InvWorld", ref coll->InvWorld);
        if (_tree.LeafNode2($"Bounding sphere: {SphereStr(coll->BoundingSphere)}").SelectedOrHovered)
        {
            VisualizeSphere(coll->BoundingSphere, Colors.CollisionColor1);
        }

        if (_tree.LeafNode2($"Bounding box: {AABBStr(coll->WorldBoundingBox)}").SelectedOrHovered)
        {
            VisualizeOBB(ref coll->WorldBoundingBox, ref Matrix4x3.Identity, Colors.CollisionColor1);
        }

        _tree.LeafNode2($"Total size: {coll->TotalPrimitives} prims, {coll->TotalChildren} nodes");
        _tree.LeafNode2($"Mesh type: {(coll->MeshIsSimple ? "simple" : coll->MemoryData != null ? "PCB in-memory" : "PCB from file")} {(coll->Loaded ? "" : "(loading)")}");
        if (coll->Mesh == null || coll->MeshIsSimple)
        {
            return;
        }

        var mesh = (MeshPCB*)coll->Mesh;
        DrawColliderMeshPCBNode("Root", mesh->RootNode, ref coll->World, coll->Collider.ObjectMaterialValue & coll->Collider.ObjectMaterialMask, ~coll->Collider.ObjectMaterialMask, coll);
    }

    private void DrawColliderMeshPCBNode(string tag, MeshPCB.FileNode* node, ref Matrix4x3 world, ulong objMatId, ulong objMatInvMask, ColliderMesh* coll)
    {
        if (node == null)
        {
            return;
        }

        using var n = _tree.Node2(tag);
        if (n.SelectedOrHovered)
        {
            VisualizeColliderMeshPCBNode(node, ref world, Colors.CollisionColor1, objMatId, objMatId, _materialId, _materialMask);
        }

        if (!n.Opened)
        {
            return;
        }

        _tree.LeafNode2($"Header: {node->Header:X16}");

        if (_tree.LeafNode2($"AABB: {AABBStr(node->LocalBounds)}").SelectedOrHovered)
        {
            VisualizeOBB(ref node->LocalBounds, ref world, Colors.CollisionColor1);
        }

        using var nv = _tree.Node2($"Vertices: {node->NumVertsRaw}+{node->NumVertsCompressed}", node->NumVertsRaw + node->NumVertsCompressed == 0);
        if (nv.Opened)
        {
            // Collect all vertices
            var translation = coll->Translation;
            var rotation = coll->Rotation;

            List<(Vector3 vertex, int index, char type)> vertices = [];

            for (var i = 0; i < node->NumVertsRaw + node->NumVertsCompressed; ++i)
            {
                var v = node->Vertex(i);
                var transformedVertex = ApplyTransformation(v, translation, rotation);
                vertices.Add((transformedVertex, i, i < node->NumVertsRaw ? 'r' : 'c'));
            }

            var playerPos = Service.ObjectTable.LocalPlayer!.Position;
            // Sort vertices by distance to player position, ignore height

            vertices.Sort((a, b) =>
            {
                var distA = (playerPos.X - a.vertex.X) * (playerPos.X - a.vertex.X) +
                              (playerPos.Z - a.vertex.Z) * (playerPos.Z - a.vertex.Z);

                var distB = (playerPos.X - b.vertex.X) * (playerPos.X - b.vertex.X) +
                              (playerPos.Z - b.vertex.Z) * (playerPos.Z - b.vertex.Z);

                return distA.CompareTo(distB);
            });

            // Render vertices in sorted order
            foreach (var (vertex, index, type) in vertices)
            {
                var vertexStr = $"new({vertex.X.ToString("F5", System.Globalization.CultureInfo.InvariantCulture)}f, {vertex.Z.ToString("F5", System.Globalization.CultureInfo.InvariantCulture)}f)";
                using var node2 = _tree.Node2($"[{index}] ({type}): {Vec3Str(vertex)}");
                if (node2.SelectedOrHovered)
                {
                    VisualizeVertex(vertex, Colors.CollisionColor2);
                }

                if (ImGui.BeginPopupContextItem($"##popup_vertex_{index}"))
                {
                    if (ImGui.MenuItem("Copy to Clipboard"))
                    {
                        ImGui.SetClipboardText(vertexStr);
                    }
                    ImGui.EndPopup();
                }
            }
        }
        {
            using var np = _tree.Node2($"Primitives: {node->NumPrims}", node->NumPrims == 0);
            if (np.Opened)
            {
                var i = 0;
                foreach (ref var prim in node->Primitives)
                {
                    if (_tree.LeafNode2($"[{++i}]: {prim.V1}x{prim.V2}x{prim.V3}, material={prim.Material:X8}").SelectedOrHovered)
                    {
                        VisualizeTriangle(node, ref prim, ref world, Colors.CollisionColor2);
                    }
                }
            }
        }
        DrawColliderMeshPCBNode($"Child 1 (+{node->Child1Offset})", node->Child1, ref world, objMatId, objMatId, coll);
        DrawColliderMeshPCBNode($"Child 2 (+{node->Child2Offset})", node->Child2, ref world, objMatId, objMatId, coll);
    }

    private void DrawResource(Resource* res)
    {
        if (res != null)
        {
            _tree.LeafNode2($"Resource: {(nint)res:X} '{res->PathString}'");
        }
        else
        {
            _tree.LeafNode2($"Resource: null");
        }
    }

    public void VisualizeCollider(Collider* coll, BitMask filterId, BitMask filterMask)
    {
        switch (coll->GetColliderType())
        {
            case ColliderType.Streamed:
                {
                    var cast = (ColliderStreamed*)coll;
                    if (cast->Header != null && cast->Elements != null)
                    {
                        for (var i = 0; i < cast->Header->NumMeshes; ++i)
                        {
                            var elem = cast->Elements + i;
                            VisualizeColliderMesh(elem->Mesh, Colors.CollisionColor1, _materialId, _materialMask);
                        }
                    }
                }
                break;
            case ColliderType.Mesh:
                VisualizeColliderMesh((ColliderMesh*)coll, _streamedMeshes.Contains((nint)coll) ? Colors.CollisionColor1 : Colors.CollisionColor2, _materialId, _materialMask);
                break;
            case ColliderType.Box:
                {
                    var cast = (ColliderBox*)coll;
                    Span<Vector3> corners = stackalloc Vector3[8];
                    for (var i = 0; i < 8; ++i)
                    {
                        corners[i] = cast->World.TransformCoordinate(_boxCorners[i]);
                    }

                    foreach (var (start, end) in _boxEdges)
                    {
                        Camera.Instance?.DrawWorldLine(corners[start], corners[end], Colors.CollisionColor3);
                    }
                }
                break;
            case ColliderType.Cylinder:
                {
                    var cast = (ColliderCylinder*)coll;
                    VisualizeCylinder(ref cast->World, Colors.CollisionColor3);
                }
                break;
            case ColliderType.Sphere:
                {
                    var cast = (ColliderSphere*)coll;
                    Camera.Instance?.DrawWorldSphere(cast->Translation, cast->Scale.X, Colors.CollisionColor3);
                }
                break;
            case ColliderType.Plane:
            case ColliderType.PlaneTwoSided:
                {
                    var cast = (ColliderPlane*)coll;
                    var a = cast->World.TransformCoordinate(new(-1, +1, 0));
                    var b = cast->World.TransformCoordinate(new(-1, -1, 0));
                    var c = cast->World.TransformCoordinate(new(+1, -1, 0));
                    var d = cast->World.TransformCoordinate(new(+1, +1, 0));
                    Camera.Instance?.DrawWorldLine(a, b, Colors.CollisionColor3);
                    Camera.Instance?.DrawWorldLine(b, c, Colors.CollisionColor3);
                    Camera.Instance?.DrawWorldLine(c, d, Colors.CollisionColor3);
                    Camera.Instance?.DrawWorldLine(d, a, Colors.CollisionColor3);
                }
                break;
        }
    }

    private void VisualizeColliderMesh(ColliderMesh* coll, uint color, BitMask filterId, BitMask filterMask)
    {
        if (coll != null && !coll->MeshIsSimple && coll->Mesh != null)
        {
            var mesh = (MeshPCB*)coll->Mesh;
            VisualizeColliderMeshPCBNode(mesh->RootNode, ref coll->World, color, coll->Collider.ObjectMaterialValue & coll->Collider.ObjectMaterialMask, ~coll->Collider.ObjectMaterialMask, filterId, filterMask);
        }
    }

    private void VisualizeColliderMeshPCBNode(MeshPCB.FileNode* node, ref Matrix4x3 world, uint color, ulong objMatId, ulong objMatInvMask, BitMask filterId, BitMask filterMask)
    {
        if (node == null)
        {
            return;
        }

        if (node->NumVertsRaw + node->NumVertsCompressed != 0)
        {
            for (var i = 0; i < node->NumPrims; ++i)
            {
                var prim = node->Primitives[i];

                var v1 = world.TransformCoordinate(node->Vertex(prim.V1));
                var v2 = world.TransformCoordinate(node->Vertex(prim.V2));
                var v3 = world.TransformCoordinate(node->Vertex(prim.V3));

                Camera.Instance?.DrawWorldLine(v1, v2, color);
                Camera.Instance?.DrawWorldLine(v2, v3, color);
                Camera.Instance?.DrawWorldLine(v3, v1, color);
            }
        }

        VisualizeColliderMeshPCBNode(node->Child1, ref world, color, objMatId, objMatInvMask, filterId, filterMask);
        VisualizeColliderMeshPCBNode(node->Child2, ref world, color, objMatId, objMatInvMask, filterId, filterMask);
    }

    private void VisualizeOBB(ref AABB localBB, ref Matrix4x3 world, uint color)
    {
        var localBBMinX = localBB.Min.X;
        var localBBMinY = localBB.Min.Y;
        var localBBMinZ = localBB.Min.Z;
        var localBBMaxX = localBB.Max.X;
        var localBBMaxY = localBB.Max.Y;
        var localBBMaxZ = localBB.Max.Z;
        var aaa = world.TransformCoordinate(new(localBBMinX, localBBMinY, localBBMinZ));
        var aab = world.TransformCoordinate(new(localBBMinX, localBBMinY, localBBMaxZ));
        var aba = world.TransformCoordinate(new(localBBMinX, localBBMaxY, localBBMinZ));
        var abb = world.TransformCoordinate(new(localBBMinX, localBBMaxY, localBBMaxZ));
        var baa = world.TransformCoordinate(new(localBBMaxX, localBBMinY, localBBMinZ));
        var bab = world.TransformCoordinate(new(localBBMaxX, localBBMinY, localBBMaxZ));
        var bba = world.TransformCoordinate(new(localBBMaxX, localBBMaxY, localBBMinZ));
        var bbb = world.TransformCoordinate(new(localBBMaxX, localBBMaxY, localBBMaxZ));
        Camera.Instance?.DrawWorldLine(aaa, aab, color);
        Camera.Instance?.DrawWorldLine(aab, bab, color);
        Camera.Instance?.DrawWorldLine(bab, baa, color);
        Camera.Instance?.DrawWorldLine(baa, aaa, color);
        Camera.Instance?.DrawWorldLine(aba, abb, color);
        Camera.Instance?.DrawWorldLine(abb, bbb, color);
        Camera.Instance?.DrawWorldLine(bbb, bba, color);
        Camera.Instance?.DrawWorldLine(bba, aba, color);
        Camera.Instance?.DrawWorldLine(aaa, aba, color);
        Camera.Instance?.DrawWorldLine(aab, abb, color);
        Camera.Instance?.DrawWorldLine(baa, bba, color);
        Camera.Instance?.DrawWorldLine(bab, bbb, color);
    }

    private void VisualizeCylinder(ref Matrix4x3 world, uint color)
    {
        var numSegments = CurveApprox.CalculateCircleSegments(world.Row0.Length(), 360f.Degrees(), 0.1f);
        var prev1 = world.TransformCoordinate(new(0, +1, 1));
        var prev2 = world.TransformCoordinate(new(0, -1, 1));
        for (var i = 1; i <= numSegments; ++i)
        {
            var dir = (i * 360.0f / numSegments).Degrees().ToDirection().ToVec2();
            var curr1 = world.TransformCoordinate(new(dir.X, +1, dir.Y));
            var curr2 = world.TransformCoordinate(new(dir.X, -1, dir.Y));
            Camera.Instance?.DrawWorldLine(curr1, prev1, color);
            Camera.Instance?.DrawWorldLine(curr2, prev2, color);
            Camera.Instance?.DrawWorldLine(curr1, curr2, color);
            prev1 = curr1;
            prev2 = curr2;
        }
    }

    private void VisualizeSphere(Vector4 sphere, uint color) => Camera.Instance?.DrawWorldSphere(new(sphere.X, sphere.Y, sphere.Z), sphere.W, color);

    private void VisualizeVertex(Vector3 worldPos, uint color) => Camera.Instance?.DrawWorldSphere(worldPos, 0.1f, color);

    private void VisualizeTriangle(MeshPCB.FileNode* node, ref Mesh.Primitive prim, ref Matrix4x3 world, uint color)
    {
        var v1 = world.TransformCoordinate(node->Vertex(prim.V1));
        var v2 = world.TransformCoordinate(node->Vertex(prim.V2));
        var v3 = world.TransformCoordinate(node->Vertex(prim.V3));
        Camera.Instance?.DrawWorldLine(v1, v2, color);
        Camera.Instance?.DrawWorldLine(v2, v3, color);
        Camera.Instance?.DrawWorldLine(v3, v1, color);
    }

    private void GatherMeshNodeMaterials(MeshPCB.FileNode* node, BitMask invMask)
    {
        if (node == null)
        {
            return;
        }

        foreach (ref var prim in node->Primitives)
        {
            _availableMaterials |= invMask & new BitMask(prim.Material);
        }

        GatherMeshNodeMaterials(node->Child1, invMask);
        GatherMeshNodeMaterials(node->Child2, invMask);
    }

    private string SphereStr(Vector4 s) => $"[{s.X:f3}, {s.Y:f3}, {s.Z:f3}] R{s.W:f3}";
    private string Vec3Str(Vector3 v) => $"[{v.X:f5}, {v.Y:f3}, {v.Z:f5}]";
    private string AABBStr(AABB bb) => $"{Vec3Str(bb.Min)} - {Vec3Str(bb.Max)}";

    private void DrawMat4x3(string tag, ref Matrix4x3 mat)
    {
        _tree.LeafNode2($"{tag} R0: {Vec3Str(mat.Row0)}");
        _tree.LeafNode2($"{tag} R1: {Vec3Str(mat.Row1)}");
        _tree.LeafNode2($"{tag} R2: {Vec3Str(mat.Row2)}");
        _tree.LeafNode2($"{tag} R3: {Vec3Str(mat.Row3)}");
    }

    private void ContextCollider(Collider* coll)
    {
        var activeLayers = new BitMask(coll->LayerMask);
        foreach (var i in _availableLayers.SetBits())
        {
            var active = activeLayers[i];
            if (ImGui.Checkbox($"Layer {i}", ref active))
            {
                activeLayers[i] = active;
                coll->LayerMask = activeLayers.Raw;
            }
        }

        var raycast = (coll->VisibilityFlags & 1) != 0;
        if (ImGui.Checkbox("Flag: raycast", ref raycast))
        {
            coll->VisibilityFlags ^= 1;
        }

        var globalVisit = (coll->VisibilityFlags & 2) != 0;
        if (ImGui.Checkbox("Flag: global visit", ref globalVisit))
        {
            coll->VisibilityFlags ^= 2;
        }

        // export (Clipper2 union) using settings
        if (coll->GetColliderType() == ColliderType.Mesh)
        {
            var cm = (ColliderMesh*)coll;
            if (cm->Mesh != null && !cm->MeshIsSimple)
            {
                ImGui.Separator();
                ImGui.TextDisabled("Export outlines (Clipper2 union)");

                var wantedId = _materialId.Raw;
                var wantedMask = _materialMask.Raw;

                // center: player if radius > 0 else ignored
                var p = Service.ObjectTable.LocalPlayer!.Position;
                var centerXZ = new Vector2(p.X, p.Z);
                var useRadius = _exportRadiusXZ > 0f;

                if (ImGui.MenuItem("Copy polygons (WPos)"))
                {
                    var polys = CollisionOutlinesExtractor.ExtractPolygonsUnion(cm, wantedId, wantedMask,
                        _exportSnapEpsXZ, useRadius ? centerXZ : default, useRadius ? _exportRadiusXZ : 0f,
                        _exportStrictRadius, _exportMatchMode, minAreaMeters2: _exportMinArea);
                    var text = CollisionOutlinesExtractor.FormatForClipboard(polys, CollisionOutlinesExtractor.ClipboardVectorFormat.Vector2XZ, 5);
                    ImGui.SetClipboardText(text);
                }
                if (ImGui.MenuItem("Copy polygons (Vector3)"))
                {
                    var polys = CollisionOutlinesExtractor.ExtractPolygonsUnion(cm, wantedId, wantedMask,
                        _exportSnapEpsXZ, useRadius ? centerXZ : default, useRadius ? _exportRadiusXZ : 0f,
                        _exportStrictRadius, _exportMatchMode, minAreaMeters2: _exportMinArea);
                    var text = CollisionOutlinesExtractor.FormatForClipboard(polys, CollisionOutlinesExtractor.ClipboardVectorFormat.Vector3XYZ, 5);
                    ImGui.SetClipboardText(text);
                }
            }
        }
        else if (coll->GetColliderType() == ColliderType.Streamed)
        {
            var cs = (ColliderStreamed*)coll;
            if (cs->Header != null && cs->Elements != null)
            {
                ImGui.Separator();
                ImGui.TextDisabled("Export outlines (Clipper2 union, merged)");

                var wantedId = _materialId.Raw;
                var wantedMask = _materialMask.Raw;

                var p = Service.ObjectTable.LocalPlayer!.Position;
                var centerXZ = new Vector2(p.X, p.Z);
                var useRadius = _exportRadiusXZ > 0f;

                if (ImGui.MenuItem("Copy polygons (WPos, merged)"))
                {
                    var polys = CollisionOutlinesExtractor.ExtractPolygonsUnionStreamed(cs, wantedId, wantedMask,
                        _exportSnapEpsXZ, useRadius ? centerXZ : default,
                        useRadius ? _exportRadiusXZ : 0f, _exportStrictRadius,
                        _exportMatchMode, minAreaMeters2: _exportMinArea);
                    var text = CollisionOutlinesExtractor.FormatForClipboard(polys, CollisionOutlinesExtractor.ClipboardVectorFormat.Vector2XZ, 5);
                    ImGui.SetClipboardText(text);
                }
                if (ImGui.MenuItem("Copy polygons (Vector3, merged)"))
                {
                    var polys = CollisionOutlinesExtractor.ExtractPolygonsUnionStreamed(cs, wantedId, wantedMask, _exportSnapEpsXZ,
                        useRadius ? centerXZ : default, useRadius ? _exportRadiusXZ : 0f,
                        _exportStrictRadius, _exportMatchMode, minAreaMeters2: _exportMinArea);
                    var text = CollisionOutlinesExtractor.FormatForClipboard(polys, CollisionOutlinesExtractor.ClipboardVectorFormat.Vector3XYZ, 5);
                    ImGui.SetClipboardText(text);
                }
            }
        }
    }

    private static Vector3 ApplyTransformation(Vector3 vertex, Vector3 translation, Vector3 rotation)
    {
        var rotX = rotation.X;
        var rotY = rotation.Y;
        var rotZ = rotation.Z;
        var rotMatrix = Matrix4x4.CreateRotationX(rotX) * Matrix4x4.CreateRotationY(rotY) * Matrix4x4.CreateRotationZ(rotZ);
        var rotatedVertex = Vector3.Transform(vertex, rotMatrix);
        return rotatedVertex + translation;
    }

    private static Angle HeadingDegFromWorld(ref Matrix4x3 world)
    {
        var f = world.Row2;
        var x = f.X;
        var z = f.Z;
        if (x == 0f && z == 0f)
        {
            return default; // degenerate/zero scale
        }

        var rad = MathF.Atan2(x, z);
        return new Angle(rad).Normalized();
    }

    private static bool TryParseHexU64(string s, out ulong value)
    {
        s = (s ?? string.Empty).Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            s = s[2..];
        }
        return ulong.TryParse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static (bool ok, HashSet<ulong> ids) TryParseHexU64List(string s)
    {
        HashSet<ulong> set = [];
        if (string.IsNullOrWhiteSpace(s))
        {
            return (true, set);
        }

        var span = s.AsSpan();
        int i = 0, n = span.Length;

        while (i < n)
        {
            // skip separators
            while (i < n && IsSep(span[i]))
            {
                ++i;
            }
            if (i >= n)
            {
                break;
            }

            var start = i;
            var has0x = false;
            if (i + 1 < n && span[i] == '0' && (span[i + 1] == 'x' || span[i + 1] == 'X'))
            {
                has0x = true;
                i += 2;
            }

            var hexStart = i;
            while (i < n && IsHex(span[i]))
            {
                ++i;
            }
            var hexLen = i - hexStart;

            if (hexLen == 0)
            {
                return (false, set); // invalid token
            }

            var token = span.Slice(has0x ? start + 2 : start, has0x ? 2 + hexLen - 2 : (i - start)).ToString();
            token = token.TrimStart('0', 'x', 'X'); // normalize leading
            if (token.Length == 0)
            {
                token = "0";
            }

            if (!ulong.TryParse(token, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var val))
            {
                return (false, set);
            }

            set.Add(val);
        }

        return (true, set);

        static bool IsSep(char c) => c == ',' || c == ';' || char.IsWhiteSpace(c);
        static bool IsHex(char c)
            => c is >= '0' and <= '9' or
            >= 'a' and <= 'f' or
            >= 'A' and <= 'F';
    }

    private void ExportPolysByMeshIdList(HashSet<ulong> ids, CollisionOutlinesExtractor.ClipboardVectorFormat fmt)
    {
        var module = Framework.Instance()->BGCollisionModule;
        List<nint> meshPtrs = [];

        foreach (var s in module->SceneManager->Scenes)
        {
            foreach (var coll in s->Scene->Colliders)
            {
                switch (coll->GetColliderType())
                {
                    case ColliderType.Mesh:
                        {
                            var cm = (ColliderMesh*)coll;
                            if (cm->Mesh != null && !cm->MeshIsSimple)
                            {
                                var addr = (ulong)(nuint)cm;
                                if (ids.Contains(addr))
                                {
                                    meshPtrs.Add((nint)cm);
                                }
                            }
                            break;
                        }
                    case ColliderType.Streamed:
                        {
                            var cs = (ColliderStreamed*)coll;
                            if (cs->Header != null && cs->Elements != null)
                            {
                                for (var i = 0; i < cs->Header->NumMeshes; ++i)
                                {
                                    var cm = (cs->Elements + i)->Mesh;
                                    if (cm == null || cm->MeshIsSimple || cm->Mesh == null)
                                    {
                                        continue;
                                    }
                                    var addr = (ulong)(nuint)cm;
                                    if (ids.Contains(addr))
                                    {
                                        meshPtrs.Add((nint)cm);
                                    }
                                }
                            }
                            break;
                        }
                }
            }
        }

        ExportPolys(meshPtrs, fmt);
    }

    private void ExportPolys(List<nint> meshPtrs, CollisionOutlinesExtractor.ClipboardVectorFormat fmt)
    {
        var wantedId = _materialId.Raw;
        var wantedMask = _materialMask.Raw;

        var p = Service.ObjectTable.LocalPlayer!.Position;
        var centerXZ = new Vector2(p.X, p.Z);
        var useRadius = _exportRadiusXZ > 0f;

        var polys = CollisionOutlinesExtractor.ExtractPolygonsUnionMany(
            meshPtrs, wantedId, wantedMask,
            _exportSnapEpsXZ,
            useRadius ? centerXZ : default,
            useRadius ? _exportRadiusXZ : 0f,
            _exportStrictRadius,
            _exportMatchMode,
            minAreaMeters2: _exportMinArea);

        var text = CollisionOutlinesExtractor.FormatForClipboard(polys, fmt, 5);
        ImGui.SetClipboardText(text);
    }

    private void SelectAllVisibleMeshes(bool add)
    {
        var module = Framework.Instance()->BGCollisionModule;
        foreach (var s in module->SceneManager->Scenes)
        {
            foreach (var coll in s->Scene->Colliders)
            {
                if (!FilterCollider(coll))
                {
                    continue;
                }

                switch (coll->GetColliderType())
                {
                    case ColliderType.Mesh:
                        {
                            var cm = (ColliderMesh*)coll;
                            if (cm->Mesh != null && !cm->MeshIsSimple)
                            {
                                var addr = (ulong)(nuint)cm;
                                if (add)
                                {
                                    _meshSelection.Add(addr);
                                }
                                else
                                {
                                    _meshSelection.Remove(addr);
                                }
                            }
                            break;
                        }
                    case ColliderType.Streamed:
                        {
                            SelectAllSubMeshes((ColliderStreamed*)coll, add);
                            break;
                        }
                }
            }
        }
    }

    private void SelectAllSubMeshes(ColliderStreamed* cs, bool add)
    {
        if (cs == null || cs->Header == null || cs->Elements == null)
        {
            return;
        }
        for (var i = 0; i < cs->Header->NumMeshes; ++i)
        {
            var cm = (cs->Elements + i)->Mesh;
            if (cm == null || cm->MeshIsSimple || cm->Mesh == null)
            {
                continue;
            }
            var addr = (ulong)(nuint)cm;
            if (add)
            {
                _meshSelection.Add(addr);
            }
            else
            {
                _meshSelection.Remove(addr);
            }
        }
    }

    private static void AppendIdToList(ref string dst, string idHexNoPrefix)
    {
        var tok = $"0x{idHexNoPrefix}";
        if (string.IsNullOrWhiteSpace(dst))
        {
            dst = tok;
            return;
        }
        if (dst.IndexOf(tok, StringComparison.OrdinalIgnoreCase) < 0)
        {
            dst = dst.TrimEnd() + Environment.NewLine + tok;
        }
    }

    private static void AppendIdsToList(ref string dst, HashSet<ulong> ids)
    {
        var sb = new StringBuilder(dst?.TrimEnd() ?? "");
        var first = sb.Length == 0;
        foreach (var v in ids)
        {
            var tok = $"0x{v:X16}";
            if (sb.ToString().Contains(tok, StringComparison.OrdinalIgnoreCase))
            {
                continue; // avoid visual dups
            }
            if (!first)
            {
                sb.AppendLine();
            }
            sb.Append(tok);
            first = false;
        }
        dst = sb.ToString();
    }

    private static string FormatHexU64(ulong v) => $"0x{v:X16}";
}
