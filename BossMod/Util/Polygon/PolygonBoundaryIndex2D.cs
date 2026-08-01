using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BossMod;

public enum PolygonShapeRelation : byte
{
    Outside,
    Inside,
    Intersecting
}

[SkipLocalsInit]
internal sealed unsafe class PolygonBoundaryIndex2D : IDisposable
{
    private const float Eps = 1e-7f;
    private const float Eps2 = Eps * Eps;
    private const float TinyDen = 1e-9f;
    private const float TinyLen2 = 1e-12f;

    private static readonly bool HasAVX512 = Vector512.IsHardwareAccelerated && Avx512F.IsSupported;
    private static readonly bool HasAVX2 = Vector256.IsHardwareAccelerated && Avx2.IsSupported;
    private static readonly bool HasFMA = Fma.IsSupported;

    private readonly struct E
    {
        public readonly float y0, y1; // inclusive bottom, exclusive top
        public readonly float x0; // x at y0
        public readonly float k; // (x1 - x0) / (y1 - y0)
        public readonly float minX, maxX; // for boundary hit

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public E(float ax, float ay, float bx, float by)
        {
            if (ay <= by)
            {
                y0 = ay;
                y1 = by;
                x0 = ax;
                k = (bx - ax) / Math.Max(by - ay, Eps);
            }
            else
            {
                y0 = by;
                y1 = ay;
                x0 = bx;
                k = (ax - bx) / Math.Max(ay - by, Eps);
            }

            minX = Math.Min(ax, bx);
            maxX = Math.Max(ax, bx);
        }
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly struct H(float ax, float ay, float bx)
    {
        public readonly float y = ay, minX = Math.Min(ax, bx), maxX = Math.Max(ax, bx);
    }

    private readonly struct SectorGeometry
    {
        public readonly float ox, oz, fx, fz;
        public readonly float radius, radiusSq, cosHalfAngle;
        public readonly float leftX, leftZ, rightX, rightZ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SectorGeometry(float ox, float oz, float fx, float fz, float radius, float halfAngle)
        {
            this.ox = ox;
            this.oz = oz;
            this.fx = fx;
            this.fz = fz;
            this.radius = radius + Eps;
            radiusSq = this.radius * this.radius;
            var (sin, cos) = MathF.SinCos(halfAngle);
            cosHalfAngle = cos;
            leftX = ox + (fx * cos - fz * sin) * this.radius;
            leftZ = oz + (fx * sin + fz * cos) * this.radius;
            rightX = ox + (fx * cos + fz * sin) * this.radius;
            rightZ = oz + (-fx * sin + fz * cos) * this.radius;
        }
    }

    private readonly struct AnnularSectorGeometry
    {
        public readonly float cx, cz, fx, fz;
        public readonly float inner, outer, innerSq, outerSq, cosHalfAngle;
        public readonly float leftInnerX, leftInnerZ, leftOuterX, leftOuterZ;
        public readonly float rightInnerX, rightInnerZ, rightOuterX, rightOuterZ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AnnularSectorGeometry(float cx, float cz, float fx, float fz, float innerRadius, float outerRadius, float halfAngle)
        {
            this.cx = cx;
            this.cz = cz;
            this.fx = fx;
            this.fz = fz;
            inner = Math.Max(0f, innerRadius - Eps);
            outer = outerRadius + Eps;
            innerSq = inner * inner;
            outerSq = outer * outer;

            var (sin, cos) = MathF.SinCos(halfAngle);
            cosHalfAngle = cos;
            var leftDirX = fx * cos - fz * sin;
            var leftDirZ = fx * sin + fz * cos;
            var rightDirX = fx * cos + fz * sin;
            var rightDirZ = -fx * sin + fz * cos;
            leftInnerX = cx + leftDirX * inner;
            leftInnerZ = cz + leftDirZ * inner;
            leftOuterX = cx + leftDirX * outer;
            leftOuterZ = cz + leftDirZ * outer;
            rightInnerX = cx + rightDirX * inner;
            rightInnerZ = cz + rightDirZ * inner;
            rightOuterX = cx + rightDirX * outer;
            rightOuterZ = cz + rightDirZ * outer;
        }
    }

    // Unmanaged, aligned SoA buffers (length = _total)
    private float* _y0; // edge y0
    private float* _y1; // edge y1
    private float* _x0; // edge x0 at y0
    private float* _k; // slope
    private float* _b; // intercept for contains: b = x0 - k*y0
    private float* _minX; // bbox
    private float* _maxX; // bbox
    private float* _dx; // precomputed x delta (k*(y1-y0))
    private float* _dy; // precomputed y delta
    private float* _invLen2; // 1 / max(dx*dx + dy*dy, TinyLen2)
    private void* _rawBlock;
    private int _total;

    // horizontals & row indexing
    private readonly H[] _hEdges;
    private readonly int[] _hRowOffsets; // rows+1
    private readonly int[] _hRowIdx; // indices into _hEdges
    private readonly int[] _rowOffsets; // rows+1 (into SoA)
    private readonly int[] _rowEnds; // actual (unpadded) end of every row

    private readonly float[] _rowMinX;
    private readonly float[] _rowMaxX;

    private readonly int _rows;
    private readonly float _minY, _cellH, _invCellH;
    private readonly float _bbMinX, _bbMinY, _bbMaxX, _bbMaxY;

    // One point on every source contour (exteriors and holes). These are used to
    // detect source contours completely enclosed by a query polygon without any edge crossing.
    private readonly WDir[] _contourSamples;

    private bool _disposed;

    private PolygonBoundaryIndex2D(float* y0, float* y1, float* x0, float* k, float* b, float* minX, float* maxX, float* dx, float* dy, float* invLen2, int total,
        int[] rowOffsets, int[] rowEnds, H[] hEdges, int[] hRowOffsets, int[] hRowIdx, int rows, float minY, float cellH, float invCellH,
        float bbMinX, float bbMinY, float bbMaxX, float bbMaxY, float[] rowMinX, float[] rowMaxX, WDir[] contourSamples, void* rawBlock)
    {
        _y0 = y0;
        _y1 = y1;
        _x0 = x0;
        _k = k;
        _b = b;
        _minX = minX;
        _maxX = maxX;
        _dx = dx;
        _dy = dy;
        _invLen2 = invLen2;
        _total = total;
        _rowOffsets = rowOffsets;
        _rowEnds = rowEnds;
        _hEdges = hEdges;
        _hRowOffsets = hRowOffsets;
        _hRowIdx = hRowIdx;
        _rows = rows;
        _minY = minY;
        _cellH = cellH;
        _invCellH = invCellH;
        _bbMinX = bbMinX;
        _bbMinY = bbMinY;
        _bbMaxX = bbMaxX;
        _bbMaxY = bbMaxY;
        _rowMinX = rowMinX;
        _rowMaxX = rowMaxX;
        _rawBlock = rawBlock;
        _contourSamples = contourSamples;
    }

    ~PolygonBoundaryIndex2D() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool _)
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        if (_rawBlock != null)
        {
            FreeAligned(_rawBlock);
            _rawBlock = null;
        }

        _y0 = _y1 = _x0 = _k = _b = _minX = _maxX = _dx = _dy = _invLen2 = null;
        _total = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void FreeAligned(void* aligned)
        {
            if (aligned == null)
            {
                return;
            }
            var raw = (void*)((nuint*)aligned)[-1];
            NativeMemory.Free(raw);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* AllocAlignedBlock(nuint bytes, nuint alignment)
    {
        // alloc alignment padding + store original pointer just before aligned
        var mask = alignment - 1;
        var raw = (byte*)NativeMemory.Alloc(bytes + alignment + (nuint)sizeof(nuint));
        var aligned = (byte*)(((nuint)(raw + sizeof(nuint) + mask)) & ~mask);
        ((nuint*)aligned)[-1] = (nuint)raw;
        return aligned;
    }

    public static PolygonBoundaryIndex2D Build(RelSimplifiedComplexPolygon complex, int minRows = 32, int maxRows = 512)
    {
        // Collect edges and global bbox
        var parts = CollectionsMarshal.AsSpan(complex.Parts);
        var lenP = parts.Length;

        float bbMinX = float.MaxValue, bbMinY = float.MaxValue;
        float bbMaxX = float.MinValue, bbMaxY = float.MinValue;

        var vertsCount = 0;
        for (var i = 0; i < lenP; ++i)
        {
            vertsCount += parts[i].Vertices.Count;
        }

        var eList = new List<E>(vertsCount);
        var hList = new List<H>(Math.Max(8, vertsCount / 2));
        var contourSamples = new List<WDir>(Math.Max(lenP, 4));

        for (var i = 0; i < lenP; ++i)
        {
            var part = parts[i];
            var ext = part.Exterior;
            AccumulateContourBounds(ext, ref bbMinX, ref bbMinY, ref bbMaxX, ref bbMaxY);
            ProcessContour(ext, eList, hList);
            if (ext.Length >= 2)
            {
                contourSamples.Add(ext[0]);
            }
            var countHoles = part.HoleStarts.Count;
            for (var h = 0; h < countHoles; ++h)
            {
                var interior = part.Interior(h);
                ProcessContour(interior, eList, hList);
                if (interior.Length >= 2)
                {
                    contourSamples.Add(interior[0]);
                }
            }
        }

        var edges = CollectionsMarshal.AsSpan(eList);
        H[] hEdgesArr = [.. hList];

        // Rowing
        var lenEdges = edges.Length;
        var nEdges = Math.Max(lenEdges, 1);
        var rows = Math.Clamp((int)MathF.Round(MathF.Sqrt(nEdges) * 0.9f) + 8, minRows, maxRows);

        var height = Math.Max(bbMaxY - bbMinY, Eps);
        var cellH = height / rows;
        var invCellH = 1f / cellH;

        var counts = new int[rows];
        var hCounts = new int[rows];

        // non-horiz counts
        for (var idx = 0; idx < lenEdges; ++idx)
        {
            ref readonly var e = ref edges[idx];
            var y0 = e.y0;
            var y1 = MathF.BitDecrement(e.y1); // top-exclusive
            var r0 = (int)MathF.Floor((y0 - bbMinY) * invCellH);
            var r1 = (int)MathF.Floor((y1 - bbMinY) * invCellH);
            if (r0 < 0)
            {
                r0 = 0;
            }
            if (r1 >= rows)
            {
                r1 = rows - 1;
            }
            for (var r = r0; r <= r1; ++r)
            {
                ++counts[r];
            }
        }

        // horizontal counts
        var hEdges = hEdgesArr;
        var lenH = hEdges.Length;
        for (int idx = 0, hN = lenH; idx < hN; ++idx)
        {
            ref readonly var hEdge = ref hEdges[idx];
            var y = hEdge.y;
            var r = (int)MathF.Floor((y - bbMinY) * invCellH);
            if (r < 0)
            {
                r = 0;
            }
            else if (r >= rows)
            {
                r = rows - 1;
            }
            ++hCounts[r];
        }
        // decide SIMD pad width
        var padWidth = HasAVX512 ? 16 : HasAVX2 ? 8 : 1;

        // prefix sums with padding
        var rowOffsets = new int[rows + 1]; // padded offsets
        var rowEnds = new int[rows]; // actual ends, excluding SIMD padding
        var paddedCounts = new int[rows];
        var total = 0;
        for (var r = 0; r < rows; ++r)
        {
            rowOffsets[r] = total;
            var c = counts[r];
            var pc = padWidth == 1 ? c : RoundUp(c, padWidth);
            paddedCounts[r] = pc;
            rowEnds[r] = total + c;
            total += pc;
        }
        rowOffsets[rows] = total;

        // horizontals offsets (no padding needed)
        var hRowOffsets = new int[rows + 1];
        var hTotal = 0;
        for (var r = 0; r < rows; ++r)
        {
            hRowOffsets[r] = hTotal;
            hTotal += hCounts[r];
        }
        hRowOffsets[rows] = hTotal;
        var hRowIdx = new int[hTotal];

        // single aligned block allocation for all arrays
        const int Fields = 10; // y0, y1, x0, k, b, minX, maxX, dx, dy, invLen2
        var bytes = Fields * (nuint)total * sizeof(float);
        // AVX2 needs 32byte allignments, AVX512F needs 64byte allignments
        var rawBlock = AllocAlignedBlock(bytes, (nuint)(padWidth * sizeof(float)));

        // Slice pointers
        float* y0Ptr, y1Ptr, x0Ptr, kPtr, bPtr, minXPtr, maxXPtr, dxPtr, dyPtr, invL2Ptr;
        {
            var basePtr = (byte*)rawBlock;
            var stride = (nuint)(total * sizeof(float));
            y0Ptr = (float*)(basePtr + stride * 0);
            y1Ptr = (float*)(basePtr + stride * 1);
            x0Ptr = (float*)(basePtr + stride * 2);
            kPtr = (float*)(basePtr + stride * 3);
            bPtr = (float*)(basePtr + stride * 4);
            minXPtr = (float*)(basePtr + stride * 5);
            maxXPtr = (float*)(basePtr + stride * 6);
            dxPtr = (float*)(basePtr + stride * 7);
            dyPtr = (float*)(basePtr + stride * 8);
            invL2Ptr = (float*)(basePtr + stride * 9);
        }

        // fill rows
        var wpos = rows <= 2048 ? stackalloc int[rows] : new int[rows];
        rowOffsets.AsSpan(0, rows).CopyTo(wpos);

        // per-row conservative X bounds (init here, update while filling)
        var rowMinX = new float[rows];
        var rowMaxX = new float[rows];
        Array.Fill(rowMinX, float.PositiveInfinity);
        Array.Fill(rowMaxX, float.NegativeInfinity);

        for (var idx = 0; idx < lenEdges; ++idx)
        {
            ref readonly var e = ref edges[idx];
            var y0 = e.y0;
            var y1TopExcl = MathF.BitDecrement(e.y1);
            var r0 = (int)MathF.Floor((y0 - bbMinY) * invCellH);
            var r1 = (int)MathF.Floor((y1TopExcl - bbMinY) * invCellH);
            if (r0 < 0)
            {
                r0 = 0;
            }
            if (r1 >= rows)
            {
                r1 = rows - 1;
            }

            var dy = e.y1 - e.y0;
            var dx = e.k * dy;
            var b = e.x0 - e.k * e.y0;
            var len2 = dx * dx + dy * dy;
            var invLen2Local = 1f / Math.Max(len2, TinyLen2);

            // row range min/max update
            var rMinY = bbMinY + r0 * cellH;
            for (var r = r0; r <= r1; ++r, rMinY += cellH)
            {
                var rMaxY = rMinY + cellH;
                var ey0 = e.y0;
                var ex0 = e.x0;
                var ek = e.k;
                var ey1 = e.y1;

                var w = wpos[r]++;
                y0Ptr[w] = ey0;
                y1Ptr[w] = ey1;
                x0Ptr[w] = ex0;
                kPtr[w] = ek;
                bPtr[w] = b;
                minXPtr[w] = e.minX;
                maxXPtr[w] = e.maxX;
                dyPtr[w] = dy;
                dxPtr[w] = dx;
                invL2Ptr[w] = invLen2Local;

                // compute x-range of this edge clipped to row band
                var ys = Math.Max(ey0, rMinY);
                var ye = Math.Min(ey1, rMaxY);
                ye = MathF.BitDecrement(ye);
                var xs = ex0 + ek * (ys - ey0);
                var xe = ex0 + ek * (ye - ey0);
                var lo = Math.Min(xs, xe);
                var hi = Math.Max(xs, xe);
                if (lo < rowMinX[r])
                {
                    rowMinX[r] = lo;
                }
                if (hi > rowMaxX[r])
                {
                    rowMaxX[r] = hi;
                }
            }
        }

        // fill rows (padding as NaN sentinels)
        for (var r = 0; r < rows; ++r)
        {
            var start = rowOffsets[r];
            var endActual = start + counts[r];
            var endPad = start + paddedCounts[r];
            for (var i = endActual; i < endPad; ++i)
            {
                y0Ptr[i] = float.NaN; // makes span/den comparisons false
                y1Ptr[i] = float.NaN;
                x0Ptr[i] = float.NaN;
                kPtr[i] = 0f;
                bPtr[i] = float.NaN;
                dyPtr[i] = 0f;
                dxPtr[i] = 0f;
                invL2Ptr[i] = 0f;
            }
        }

        // horizontals: fill row indices + update row bounds
        var hwpos = rows <= 2048 ? stackalloc int[rows] : new int[rows];
        hRowOffsets.AsSpan(0, rows).CopyTo(hwpos);
        for (int idx = 0, hN = lenH; idx < hN; ++idx)
        {
            ref readonly var hEdge = ref hEdges[idx];
            var r = (int)MathF.Floor((hEdge.y - bbMinY) * invCellH);
            if (r < 0)
            {
                r = 0;
            }
            else if (r >= rows)
            {
                r = rows - 1;
            }
            hRowIdx[hwpos[r]++] = idx;

            // include horizontals into per-row X hull
            if (hEdge.minX < rowMinX[r])
            {
                rowMinX[r] = hEdge.minX;
            }
            if (hEdge.maxX > rowMaxX[r])
            {
                rowMaxX[r] = hEdge.maxX;
            }
        }

        return new PolygonBoundaryIndex2D(y0Ptr, y1Ptr, x0Ptr, kPtr, bPtr, minXPtr, maxXPtr, dxPtr, dyPtr, invL2Ptr, total,
            rowOffsets, rowEnds, hEdges, hRowOffsets, hRowIdx, rows, bbMinY, cellH, invCellH,
            bbMinX, bbMinY, bbMaxX, bbMaxY, rowMinX, rowMaxX, [.. contourSamples], rawBlock);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int RoundUp(int v, int m) => (v + (m - 1)) / m * m;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AccumulateContourBounds(ReadOnlySpan<WDir> contour, ref float bbMinX, ref float bbMinY, ref float bbMaxX, ref float bbMaxY)
        {
            var count = contour.Length;
            for (var i = 0; i < count; ++i)
            {
                var p = contour[i];
                var pX = p.X;
                var pZ = p.Z;
                if (pX < bbMinX)
                {
                    bbMinX = pX;
                }
                if (pX > bbMaxX)
                {
                    bbMaxX = pX;
                }
                if (pZ < bbMinY)
                {
                    bbMinY = pZ;
                }
                if (pZ > bbMaxY)
                {
                    bbMaxY = pZ;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ProcessContour(ReadOnlySpan<WDir> contour, List<E> eList, List<H> hList)
        {
            var count = contour.Length;
            if (count < 2)
            {
                return;
            }

            var prev = contour[count - 1];

            for (var i = 0; i < count; ++i)
            {
                var curr = contour[i];

                var ax = prev.X;
                var ay = prev.Z;
                var bx = curr.X;
                var by = curr.Z;

                if (Math.Abs(ay - by) <= Eps)
                {
                    hList.Add(new(ax, ay, bx));
                }
                else
                {
                    eList.Add(new(ax, ay, bx, by));
                }

                prev = curr;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ClampRow(float y)
    {
        var r = (int)MathF.Floor((y - _minY) * _invCellH);
        return r < 0 ? 0 : r >= _rows ? _rows - 1 : r;
    }

    public bool Contains(in WDir p)
    {
        var px = p.X;
        var py = p.Z;
        if (px < _bbMinX - Eps || px > _bbMaxX + Eps || py < _bbMinY - Eps || py > _bbMaxY + Eps)
        {
            return false;
        }

        var row = ClampRow(py);

        int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
        for (var i = hs; i < he; ++i)
        {
            ref readonly var h = ref _hEdges[_hRowIdx[i]];
            if (Math.Abs(py - h.y) <= Eps && px >= h.minX - Eps && px <= h.maxX + Eps)
            {
                return true;
            }
        }

        if (px < _rowMinX[row] - Eps || px > _rowMaxX[row] + Eps)
        {
            return false;
        }

        int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
        if (ee - es == 0)
        {
            return false;
        }

        var parity = 0;

        if (HasAVX512)
        {
            var v_py = Vector512.Create(py);
            var v_px = Vector512.Create(px);
            var v_eps = Vector512.Create(Eps);
            var v_eps2 = Vector512.Create(Eps2);

            var i0 = es;
            for (; i0 + 16 <= ee; i0 += 16)
            {
                parity ^= ContainsBlock512(i0, v_py, v_px, v_eps, v_eps2);
                if ((parity & 2) != 0)
                {
                    return true;
                }
                parity &= 1;
            }
            return (parity & 1) != 0;
        }
        else if (HasAVX2)
        {
            var v_py = Vector256.Create(py);
            var v_px = Vector256.Create(px);
            var eps = Vector256.Create(Eps);
            var eps2 = Vector256.Create(Eps2);

            var i0 = es;
            for (; i0 + 8 <= ee; i0 += 8)
            {
                parity ^= ContainsBlock256(i0, v_py, v_px, eps, eps2);
                if ((parity & 2) != 0)
                {
                    return true;
                }
                parity &= 1;
            }
            return (parity & 1) != 0;
        }
        else
        {
            for (var i = es; i < ee; ++i)
            {
                var y0s = _y0[i];
                if (py < y0s - Eps)
                {
                    continue;
                }
                var y1s = _y1[i];
                if (py >= y1s - Eps)
                {
                    continue;
                }

                var x = _k[i] * py + _b[i];

                var dx = px - x;
                if (dx * dx <= Eps2)
                {
                    if (px >= _minX[i] - Eps && px <= _maxX[i] + Eps)
                    {
                        return true;
                    }
                }
                if (x > px)
                {
                    parity ^= 1;
                }
            }
            return (parity & 1) != 0;
        }
    }

    public float Raycast(in WDir o, in WDir d)
    {
        float ox = o.X, oz = o.Z;
        float dx = d.X, dz = d.Z;

        float tmin = -float.Epsilon, tmax = float.MaxValue;
        var invDx = 0f; // only valid if hasDx
        var hasDx = Math.Abs(dx) > Eps;
        if (hasDx)
        {
            invDx = 1f / dx;
            var tx1 = (_bbMinX - ox) * invDx;
            var tx2 = (_bbMaxX - ox) * invDx;
            if (tx1 > tx2)
            {
                (tx1, tx2) = (tx2, tx1);
            }
            tmin = Math.Max(tmin, tx1);
            tmax = Math.Min(tmax, tx2);
        }
        else if (ox < _bbMinX - Eps || ox > _bbMaxX + Eps)
        {
            return float.MaxValue;
        }

        var invDz = 0f; // only valid if hasDz
        var hasDz = Math.Abs(dz) > Eps;
        if (hasDz)
        {
            invDz = 1f / dz;
            var ty1 = (_bbMinY - oz) * invDz;
            var ty2 = (_bbMaxY - oz) * invDz;
            if (ty1 > ty2)
            {
                (ty1, ty2) = (ty2, ty1);
            }
            tmin = Math.Max(tmin, ty1);
            tmax = Math.Min(tmax, ty2);
        }
        else if (oz < _bbMinY - Eps || oz > _bbMaxY + Eps)
        {
            return float.MaxValue;
        }

        if (tmax < 0f || tmin > tmax)
        {
            return float.MaxValue;
        }

        // Horizontal ray
        if (!hasDz)
        {
            if (!hasDx)
            {
                return float.MaxValue;
            }
            var row = ClampRow(oz);
            var best = float.MaxValue;
            var t0 = Math.Max(0f, tmin);

            int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
            for (var i = es; i < ee; ++i)
            {
                float y0 = _y0[i], y1 = _y1[i];
                if (oz < y0 - Eps || oz >= y1 - Eps)
                {
                    continue;
                }
                var x = _k[i] * oz + _b[i];
                var t = (x - ox) * invDx;
                if (t >= t0 && t <= tmax && t < best && x >= _minX[i] - Eps && x <= _maxX[i] + Eps)
                {
                    best = t;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var k = hs; k < he; ++k)
            {
                ref readonly var h = ref _hEdges[_hRowIdx[k]];
                if (Math.Abs(oz - h.y) > Eps)
                {
                    continue;
                }

                if (dx > 0f)
                {
                    var x0 = ox <= h.minX ? h.minX : (ox <= h.maxX ? ox : float.PositiveInfinity);
                    var t = (x0 - ox) * invDx;
                    if (t >= t0 && t <= tmax && t < best)
                    {
                        best = t;
                    }
                }
                else
                {
                    var x0 = ox >= h.maxX ? h.maxX : (ox >= h.minX ? ox : float.NegativeInfinity);
                    var t = (x0 - ox) * invDx;
                    if (t >= t0 && t <= tmax && t < best)
                    {
                        best = t;
                    }
                }
            }
            return best;
        }
        var cellH = _cellH;
        // General ray (|dz| > Eps)
        var t0m = Math.Max(0f, tmin);
        var yStart = oz + dz * t0m;
        var rowCur = ClampRow(yStart);
        var step = dz > 0f ? 1 : -1;

        // Next grid boundary from yStart
        var rowEdge = (int)MathF.Floor((yStart - _minY) * _invCellH);
        if (rowEdge < 0)
        {
            rowEdge = 0;
        }
        else if (rowEdge >= _rows)
        {
            rowEdge = _rows - 1;
        }
        var nextY = dz > 0f ? (_minY + (rowEdge + 1) * cellH) : (_minY + rowEdge * cellH);

        var bestT = float.MaxValue;
        var hit = false;
        var curYEnter = yStart;
        while ((uint)rowCur < (uint)_rows)
        {
            var tBoundary = (nextY - oz) * invDz;
            var rowTMax = Math.Min(tmax, bestT);

            // row-level x-range cull
            // ray x at row enter/exit
            var xEnter = ox + dx * ((curYEnter - oz) * invDz);
            var xExit = ox + dx * ((nextY - oz) * invDz);
            var rxMin = Math.Min(xEnter, xExit) - 2e-6f; // expand a touch to be conservative
            var rxMax = Math.Max(xEnter, xExit) + 2e-6f;

            // row polygon x-extent
            var pxMin = _rowMinX[rowCur];
            var pxMax = _rowMaxX[rowCur];

            // if disjoint, skip this row entirely
            if (!(rxMax < pxMin || rxMin > pxMax))
            {
                int es = _rowOffsets[rowCur], ee = _rowOffsets[rowCur + 1];
                var prevBest = bestT;
                if (HasAVX512)
                {
                    KernelRay512(es, ee, ox, oz, dx, dz, t0m, rowTMax, ref bestT);
                }
                else if (HasAVX2)
                {
                    KernelRay256(es, ee, ox, oz, dx, dz, t0m, rowTMax, ref bestT);
                }
                else
                {
                    KernelRayScalar(es, ee, ox, oz, dx, dz, t0m, rowTMax, ref bestT);
                }
                if (bestT < prevBest)
                {
                    hit = true;
                    rowTMax = Math.Min(rowTMax, bestT);
                }

                // horizontals (use t from y only)
                int hs = _hRowOffsets[rowCur], he = _hRowOffsets[rowCur + 1];
                for (var k = hs; k < he; ++k)
                {
                    ref readonly var h = ref _hEdges[_hRowIdx[k]];
                    var t = (h.y - oz) * invDz;
                    if (t < t0m || t > rowTMax)
                    {
                        continue;
                    }
                    var xAtT = ox + t * dx;
                    if (xAtT >= h.minX - 1e-6f && xAtT <= h.maxX + 1e-6f)
                    {
                        bestT = t;
                        hit = true;
                        rowTMax = t;
                    }
                }

                // early-out: found hit before crossing far boundary
                if (hit && bestT <= tBoundary + 1e-6f)
                {
                    break;
                }
            }

            // stop after row that reaches AABB exit
            if (tBoundary >= tmax - 1e-6f)
            {
                break;
            }

            // advance
            rowCur += step;
            curYEnter = nextY;
            nextY += step * cellH;
        }

        return hit ? bestT : float.MaxValue;
    }

    public WDir ClosestPointOnBoundary(in WDir p)
    {
        float px = p.X, py = p.Z;

        var row0 = ClampRow(py);
        int rNeg = row0, rPos = row0 + 1;

        var bestSq = float.PositiveInfinity;
        float bestX = px, bestY = py;

        var cellH = _cellH;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float VDistSq(float py, float minY, float maxY)
        {
            var vDist = Math.Max(0f, Math.Max(minY - py, py - maxY));
            return vDist * vDist;
        }

        void ProcessRow(int row, float vDistSq)
        {
            int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
            if (es == ee && _hRowOffsets[row] == _hRowOffsets[row + 1])
            {
                return;
            }

            var rMinX = _rowMinX[row];
            var rMaxX = _rowMaxX[row];
            var hDist = Math.Max(0f, Math.Max(rMinX - px, px - rMaxX));

            if (vDistSq + hDist * hDist < bestSq)
            {
                if (HasAVX512)
                {
                    KernelClosest512(es, ee, px, py, ref bestSq, ref bestX, ref bestY);
                }
                else if (HasAVX2)
                {
                    KernelClosest256(es, ee, px, py, ref bestSq, ref bestX, ref bestY);
                }
                else
                {
                    KernelClosestScalar(es, ee, px, py, ref bestSq, ref bestX, ref bestY);
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var e = ref _hEdges[_hRowIdx[h]];
                var cx = Math.Min(Math.Max(px, e.minX), e.maxX);
                var dxp = cx - px;
                var dyp = e.y - py;
                var d2 = dxp * dxp + dyp * dyp;
                if (d2 < bestSq)
                {
                    bestSq = d2;
                    bestX = cx;
                    bestY = e.y;
                }
            }
        }

        var minY0 = _minY;
        if ((uint)row0 < (uint)_rows)
        {
            var minY = minY0 + row0 * cellH;
            var maxY = minY + cellH;
            ProcessRow(row0, VDistSq(py, minY, maxY));
        }

        var maxRow = _rows - 1;
        while (true)
        {
            var progressed = false;

            if (rNeg - 1 >= 0)
            {
                var rn = --rNeg;
                var rMinY = minY0 + rn * cellH;
                var rMaxY = rMinY + cellH;
                var vDistSq = VDistSq(py, rMinY, rMaxY);
                if (vDistSq < bestSq)
                {
                    ProcessRow(rn, vDistSq);
                    progressed = true;
                }
            }

            if (rPos <= maxRow)
            {
                var rp = rPos++;
                var rMinY = minY0 + rp * cellH;
                var rMaxY = rMinY + cellH;
                var vDistSq = VDistSq(py, rMinY, rMaxY);
                if (vDistSq < bestSq)
                {
                    ProcessRow(rp, vDistSq);
                    progressed = true;
                }
            }

            if (!progressed || bestSq == 0f || rNeg <= 0 && rPos > maxRow)
            {
                break;
            }
        }

        return new(bestX, bestY);
    }

    public WDir[] VisibilityFrom(in WDir origin, RelSimplifiedComplexPolygon polygon)
    {
        var angs = CollectUniqueAngles(origin, polygon);
        var countA = angs.Count;
        if (countA == 0)
        {
            return [];
        }

        var res = new List<(WDir pt, float t)>(countA * 2 + 2);
        const float baseEps = 1e-6f;
        const float sqMerge = baseEps * baseEps;
        const double baseJ = 1e-4d; // jitter

        for (var i = 0; i < countA; ++i)
        {
            var a0 = angs[i];
            var a1 = (i + 1 < countA) ? angs[i + 1] : angs[0];
            var d = AngleDiffCCW(a0, a1);
            if (d <= 1e-14)
            {
                continue;
            }

            var eps = Math.Min(baseJ, 0.49d * d);
            var left = a0 + eps;
            var right = a1 - eps;

            var gotL = RayAt(origin, left, out var pL, out var tL);
            var gotR = RayAt(origin, right, out var pR, out var tR);

            if (gotL && gotR)
            {
                if ((pL - pR).LengthSq() <= sqMerge)
                {
                    // Keep farther for safety
                    res.Add((tL >= tR) ? (pL, tL) : (pR, tR));
                }
                else
                {
                    // Keep both (in CCW order)
                    res.Add((pL, tL));
                    res.Add((pR, tR));
                }
            }
            else if (gotL)
            {
                res.Add((pL, tL));
            }
            else if (gotR)
            {
                res.Add((pR, tR));
            }
            else
            {
                // Midpoint fallback
                var mid = a0 + 0.5d * d;
                if (RayAt(origin, mid, out var pM, out var tM))
                {
                    res.Add((pM, tM));
                }
            }
        }

        if (res.Count == 0)
        {
            return [];
        }

        // fan-wide close-pair merge: keep the farther (bigger fan for safety)
        MergeConsecutivePreferOuter(res);

        // close loop if endpoints coincide (prefer farther across seam)
        var first = res[0];
        var last = res[^1];
        if ((first.pt - last.pt).LengthSq() <= sqMerge)
        {
            res[^1] = (first.t >= last.t) ? first : last;
        }

        // remove near-collinear points
        SimplifyCollinear(res);

        var countR = res.Count;
        // materialize
        var outPts = new WDir[countR];
        for (var i = 0; i < countR; ++i)
        {
            outPts[i] = res[i].pt;
        }
        return outPts;
    }

    public void AddForbiddenDirections(Actor actor, WPos center, RelSimplifiedComplexPolygon polygon, AIHints hints, DateTime act, float forbiddenDist)
    {
        var origin = actor.Position - center;
        var angs = CollectUniqueAngles(origin, polygon);
        var count = angs.Count;
        if (count == 0)
        {
            return;
        }

        var blocked = new List<(double start, double end)>(count * 2);

        for (var i = 0; i < count; ++i)
        {
            var a0 = angs[i];
            var a1 = (i + 1 < count) ? angs[i + 1] : angs[0] + Math.Tau;
            CollectBlockedIntervals(origin, a0, a1, forbiddenDist, blocked, 0);
        }

        var countB = blocked.Count;

        if (countB == 0)
        {
            return;
        }

        MergeAngleIntervals(blocked);
        countB = blocked.Count;
        for (var i = 0; i < countB; ++i)
        {
            var block = blocked[i];
            var start = block.start;
            var end = block.end;
            var width = AngleDiffCCW(start, end);
            if (width <= 1e-7)
            {
                continue;
            }

            var centerA = NormalizeAngle(start + 0.5d * width);

            hints.ForbiddenDirections.Add(new(new((float)centerA), new(0.5f * (float)width), act));
        }
    }

    private void CollectBlockedIntervals(in WDir origin, double a0, double a1, float forbiddenDist, List<(double start, double end)> blocked, int depth)
    {
        var d = AngleDiffCCW(a0, a1);
        if (d <= 1e-9)
            return;

        var eps = Math.Min(1e-4, 0.2d * d);
        var left = NormalizeAngle(a0 + eps);
        var right = NormalizeAngle(a1 - eps);
        var mid = NormalizeAngle(a0 + 0.5d * d);

        var sL = IsBlocked(origin, left, forbiddenDist);
        var sM = IsBlocked(origin, mid, forbiddenDist);
        var sR = IsBlocked(origin, right, forbiddenDist);

        if (sL == sM && sM == sR)
        {
            if (sM)
            {
                blocked.Add((a0, a1));
            }
            return;
        }

        if (depth >= 10 || d <= 0.25d * (Math.PI / 180d))
        {
            if (sL || sM || sR)
            {
                blocked.Add((a0, a1));
            }
            return;
        }

        var am = a0 + 0.5 * d;
        CollectBlockedIntervals(origin, a0, am, forbiddenDist, blocked, depth + 1);
        CollectBlockedIntervals(origin, am, a1, forbiddenDist, blocked, depth + 1);
    }

    private bool IsBlocked(in WDir origin, double angle, float forbiddenDist)
    {
        return RayAt(origin, angle, out _, out var t) && t <= forbiddenDist;
    }

    public static void MergeAngleIntervals(List<(double start, double end)> intervals)
    {
        var count = intervals.Count;
        if (count <= 1)
        {
            return;
        }

        intervals.Sort(static (x, y) => x.start.CompareTo(y.start));

        var cur = intervals[0];
        var write = 0;

        for (var i = 1; i < count; ++i)
        {
            var next = intervals[i];

            if (next.start <= cur.end + 1e-7)
            {
                if (next.end > cur.end)
                {
                    cur.end = next.end;
                }
            }
            else
            {
                intervals[write++] = cur;
                cur = next;
            }
        }

        intervals[write++] = cur;
        if (write < count)
        {
            intervals.RemoveRange(write, count - write);
        }
    }

    public static double NormalizeAngle(double a)
    {
        a %= Math.Tau;
        if (a < 0d)
        {
            a += Math.Tau;
        }
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool RayAt(in WDir origin, double angle, out WDir hit, out float t)
    {
        var (sd, cd) = Math.SinCos(angle);
        var dir = new WDir((float)cd, (float)sd);
        t = Raycast(origin, dir);
        if (t == float.MaxValue)
        {
            hit = default;
            return false;
        }
        hit = origin + t * dir;
        return true;
    }

    private static List<double> CollectUniqueAngles(in WDir origin, RelSimplifiedComplexPolygon polygon)
    {
        var oz = origin.Z;
        var ox = origin.X;
        var vertsCount = 0;
        var parts = polygon.Parts;
        var countP = parts.Count;
        for (var i = 0; i < countP; ++i)
        {
            vertsCount += parts[i].Vertices.Count;
        }
        var angles = new List<double>(vertsCount);

        for (int i = 0, n = countP; i < n; ++i)
        {
            var vs = parts[i].Vertices;
            var countVS = vs.Count;
            for (int j = 0, m = countVS; j < m; ++j)
            {
                var v = vs[j];
                var a = Math.Atan2(v.Z - oz, v.X - ox);
                angles.Add(a < 0d ? a + Math.Tau : a);
            }
        }
        var countA = angles.Count;
        if (countA == 0)
        {
            return angles;
        }

        angles.Sort();
        const double Aeps = 1e-12d;
        var write = 1;
        var last = angles[0];
        for (var i = 1; i < countA; ++i)
        {
            var a = angles[i];
            if (a - last > Aeps)
            {
                angles[write++] = a;
                last = a;
            }
        }
        if (write >= 2 && AngleDiffCCW(angles[write - 1], angles[0]) < Aeps)
        {
            --write;
        }
        if (write < countA)
        {
            angles.RemoveRange(write, countA - write);
        }
        return angles;
    }

    private static void MergeConsecutivePreferOuter(List<(WDir pt, float t)> pts)
    {
        var countP = pts.Count;
        if (countP <= 2)
        {
            return;
        }
        const float baseEps = 1e-6f;
        const float sqMerge = baseEps * baseEps;
        var write = 1;

        for (var i = 1; i < countP; ++i)
        {
            var prev = pts[write - 1];
            var cur = pts[i];
            if ((prev.pt - cur.pt).LengthSq() <= sqMerge)
            {
                // keep farther from origin for safety
                pts[write - 1] = (cur.t >= prev.t) ? cur : prev;
            }
            else
            {
                pts[write++] = cur;
            }
        }

        if (write < countP)
        {
            pts.RemoveRange(write, countP - write);
        }
    }

    private static void SimplifyCollinear(List<(WDir pt, float t)> pts)
    {
        var countP = pts.Count;
        if (countP <= 2)
        {
            return;
        }
        const float eps = 1e-6f;
        var tmp = new List<(WDir pt, float t)>(countP);

        for (var i = 0; i < countP; ++i)
        {
            var a = pts[(i - 1 + countP) % countP].pt;
            var b = pts[i].pt;
            var c = pts[(i + 1) % countP].pt;
            var abx = b.X - a.X;
            var abz = b.Z - a.Z;
            var bcx = c.X - b.X;
            var bcz = c.Z - b.Z;
            var cross = Math.Abs(abx * bcz - abz * bcx);
            var scale = Math.Abs(abx) + Math.Abs(abz) + Math.Abs(bcx) + Math.Abs(bcz);
            if (cross > eps * scale)
            {
                tmp.Add(pts[i]); // keep only if not near-collinear
            }
        }

        pts.Clear();
        pts.AddRange(tmp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double AngleDiffCCW(double a, double b)
    {
        var d = b - a;
        if (d < 0d)
        {
            d += Math.Tau;
        }
        return d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateBestFromMask(Vector256<float> vals, uint mask, ref float best)
    {
        while (mask != 0u)
        {
            var i = BitOperations.TrailingZeroCount(mask);
            var v = vals.GetElement(i);
            if (v < best)
            {
                best = v;
            }
            mask &= mask - 1u;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateBestFromMask(Vector512<float> vals, ulong mask, ref float best)
    {
        while (mask != 0ul)
        {
            var i = BitOperations.TrailingZeroCount(mask);
            var v = vals.GetElement(i);
            if (v < best)
            {
                best = v;
            }
            mask &= mask - 1ul;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateClosestFromMask(Vector256<float> d2, Vector256<float> nx, Vector256<float> ny, uint mask,
        ref float bestSq, ref float bestX, ref float bestY)
    {
        while (mask != 0u)
        {
            var i = BitOperations.TrailingZeroCount(mask);
            var di = d2.GetElement(i);
            if (di < bestSq)
            {
                bestSq = di;
                bestX = nx.GetElement(i);
                bestY = ny.GetElement(i);
            }
            mask &= mask - 1u;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateClosestFromMask(Vector512<float> d2, Vector512<float> nx, Vector512<float> ny, ulong mask,
        ref float bestSq, ref float bestX, ref float bestY)
    {
        while (mask != 0ul)
        {
            var i = BitOperations.TrailingZeroCount(mask);
            var di = d2.GetElement(i);
            if (di < bestSq)
            {
                bestSq = di;
                bestX = nx.GetElement(i);
                bestY = ny.GetElement(i);
            }
            mask &= mask - 1ul;
        }
    }

    // SIMD kernels
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ContainsBlock512(int idx, Vector512<float> v_py, Vector512<float> v_px, Vector512<float> v_eps, Vector512<float> v_eps2)
    {
        var y0 = Load512(_y0, idx);
        var y1 = Load512(_y1, idx);
        var k = Load512(_k, idx);
        var b = Load512(_b, idx);

        var span = Vector512.GreaterThanOrEqual(v_py, y0) & Vector512.LessThan(v_py, y1);

        var x = Avx512F.FusedMultiplyAdd(k, v_py, b);

        var dx = Avx512F.Subtract(v_px, x);
        var near = Vector512.LessThanOrEqual(dx * dx, v_eps2) & span;

        // only load min/max if any near-bit is set.
        if (near.ExtractMostSignificantBits() != 0ul)
        {
            var minX = Load512(_minX, idx);
            var maxX = Load512(_maxX, idx);
            var geMin = Vector512.GreaterThanOrEqual(v_px, minX - v_eps);
            var leMax = Vector512.LessThanOrEqual(v_px, maxX + v_eps);

            var boundary = near & geMin & leMax;
            if (boundary.ExtractMostSignificantBits() != 0ul)
            {
                return 2; // bit1: boundary
            }
        }

        var cross = Vector512.GreaterThan(x, v_px) & span;
        return BitOperations.PopCount(cross.ExtractMostSignificantBits()) & 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ContainsBlock256(int idx, Vector256<float> v_py, Vector256<float> v_px, Vector256<float> v_eps, Vector256<float> v_eps2)
    {
        var y0 = Load256(_y0, idx);
        var y1 = Load256(_y1, idx);
        var k = Load256(_k, idx);
        var b = Load256(_b, idx);

        var span = Vector256.BitwiseAnd(Vector256.GreaterThanOrEqual(v_py, y0), Vector256.LessThan(v_py, y1));

        var x = HasFMA ? Fma.MultiplyAdd(k, v_py, b) : Avx.Add(Avx.Multiply(k, v_py), b); // k*py + b

        var dx = Avx.Subtract(v_px, x);
        var near = Vector256.BitwiseAnd(Vector256.LessThanOrEqual(Avx.Multiply(dx, dx), v_eps2), span);

        // Lazy-load min/max on near bits.
        if (near.ExtractMostSignificantBits() != 0u)
        {
            var minX = Load256(_minX, idx);
            var maxX = Load256(_maxX, idx);
            var geMin = Vector256.GreaterThanOrEqual(v_px, Avx.Subtract(minX, v_eps));
            var leMax = Vector256.LessThanOrEqual(v_px, Avx.Add(maxX, v_eps));
            var boundary = Vector256.BitwiseAnd(near, Vector256.BitwiseAnd(geMin, leMax));
            if (boundary.ExtractMostSignificantBits() != 0u)
            {
                return 2;
            }
        }

        var cross = Vector256.BitwiseAnd(Vector256.GreaterThan(x, v_px), span);
        return BitOperations.PopCount(cross.ExtractMostSignificantBits()) & 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelRay512(int es, int ee, float ox, float oz, float dx, float dz, float tMin, float tMax, ref float best)
    {
        var v_dx = Vector512.Create(dx);
        var v_dz = Vector512.Create(dz);
        var v_ox = Vector512.Create(ox);
        var v_oz = Vector512.Create(oz);
        var v_tiny = Vector512.Create(TinyDen);
        var v_one = Vector512<float>.One;
        var v_zero = Vector512<float>.Zero;
        var v_tMin = Vector512.Create(tMin);
        var v_tMax = Vector512.Create(tMax);

        for (var i = es; i + 16 <= ee; i += 16)
        {
            var y0 = Load512(_y0, i);
            var dy = Load512(_dy, i);
            var x0 = Load512(_x0, i);
            var dxE = Load512(_dx, i);

            var wox = Avx512F.Subtract(x0, v_ox);
            var woz = Avx512F.Subtract(y0, v_oz);

            // den = dx*dy - dz*dxE
            var den = Avx512F.FusedMultiplySubtract(v_dx, dy, Avx512F.Multiply(v_dz, dxE));
            var validDen = Vector512.GreaterThan(Vector512.Abs(den), v_tiny);
            var invDen = Avx512F.Divide(v_one, den);

            // t_num = wox*dy - woz*dxE,  u_num = wox*dz - woz*dx
            var t_num = Avx512F.FusedMultiplySubtract(wox, dy, Avx512F.Multiply(woz, dxE));
            var u_num = Avx512F.FusedMultiplySubtract(wox, v_dz, Avx512F.Multiply(woz, v_dx));
            var t = Avx512F.Multiply(t_num, invDen);
            var u = Avx512F.Multiply(u_num, invDen);

            var valid = validDen
                & Vector512.GreaterThanOrEqual(t, v_tMin)
                & Vector512.LessThanOrEqual(t, v_tMax)
                & Vector512.GreaterThanOrEqual(u, v_zero)
                & Vector512.LessThan(u, v_one);

            var mask = valid.ExtractMostSignificantBits();
            if (mask != 0ul)
            {
                // only touch lanes that hit and satisfy ranges
                UpdateBestFromMask(t, mask, ref best);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelRay256(int es, int ee, float ox, float oz, float dx, float dz, float tMin, float tMax, ref float best)
    {
        var v_dx = Vector256.Create(dx);
        var v_dz = Vector256.Create(dz);
        var v_ox = Vector256.Create(ox);
        var v_oz = Vector256.Create(oz);
        var v_tiny = Vector256.Create(TinyDen);
        var v_one = Vector256<float>.One;
        var v_zero = Vector256<float>.Zero;
        var v_tMin = Vector256.Create(tMin);
        var v_tMax = Vector256.Create(tMax);

        for (var i = es; i + 8 <= ee; i += 8)
        {
            var y0 = Load256(_y0, i);
            var dy = Load256(_dy, i);
            var x0 = Load256(_x0, i);
            var dxE = Load256(_dx, i);

            var wox = Avx.Subtract(x0, v_ox);
            var woz = Avx.Subtract(y0, v_oz);

            var den = Avx.Subtract(Avx.Multiply(v_dx, dy), Avx.Multiply(v_dz, dxE));
            var validDen = Vector256.GreaterThan(Vector256.Abs(den), v_tiny);
            var invDen = Avx.Divide(v_one, den);

            // t = (wox*dy - woz*dxE) / den
            var t = HasFMA ? Fma.MultiplySubtract(wox, dy, Avx.Multiply(woz, dxE)) : Avx.Subtract(Avx.Multiply(wox, dy), Avx.Multiply(woz, dxE));
            t = Avx.Multiply(t, invDen);

            // early range gate on t to avoid useless u work
            var tValid = Vector256.BitwiseAnd(
                Vector256.GreaterThanOrEqual(t, v_tMin),
                Vector256.LessThanOrEqual(t, v_tMax));

            // u = (wox*dz - woz*dx) / den
            var u = HasFMA ? Fma.MultiplySubtract(wox, v_dz, Avx.Multiply(woz, v_dx)) : Avx.Subtract(Avx.Multiply(wox, v_dz), Avx.Multiply(woz, v_dx));
            u = Avx.Multiply(u, invDen);

            var valid = validDen & tValid & Vector256.GreaterThanOrEqual(u, v_zero) & Vector256.LessThan(u, v_one);

            var mask = valid.ExtractMostSignificantBits();
            if (mask != 0u)
            {
                // only inspect lanes that actually hit
                UpdateBestFromMask(t, mask, ref best);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelRayScalar(int es, int ee, float ox, float oz, float dx, float dz, float tMin, float tMax, ref float best)
    {
        for (var i = es; i < ee; ++i)
        {
            float y0s = _y0[i], eys = _dy[i], x0s = _x0[i], exs = _dx[i];
            float woxs = x0s - ox, wozs = y0s - oz;
            var den = dx * eys - dz * exs;

            if (Math.Abs(den) > TinyDen)
            {
                var invDen = 1f / den;
                var t = (woxs * eys - wozs * exs) * invDen;
                if (t < tMin || t > tMax || t >= best)
                {
                    continue;
                }

                var u = (woxs * dz - wozs * dx) * invDen;
                if (u is < 0f or >= (1f - 1e-6f))
                {
                    continue;
                }

                best = t;
            }
            else
            {
                var col = woxs * dz - wozs * dx;
                if (Math.Abs(col) <= Eps)
                {
                    var iddd = 1f / (dx * dx + dz * dz + 1e-20f);
                    var tA = (woxs * dx + wozs * dz) * iddd;
                    var tB = ((x0s + exs - ox) * dx + (y0s + eys - oz) * dz) * iddd;

                    if (tA > tB)
                    {
                        (tA, tB) = (tB, tA);
                    }
                    var cand = float.MaxValue;
                    if (tA >= tMin && tA <= tMax)
                    {
                        cand = tA;
                    }
                    else if (tB >= tMin && tB <= tMax)
                    {
                        cand = tB;
                    }

                    if (cand < best)
                    {
                        best = cand;
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelClosest512(int es, int ee, float px, float py, ref float bestSq, ref float bestX, ref float bestY)
    {
        var v_px = Vector512.Create(px);
        var v_py = Vector512.Create(py);
        var v_one = Vector512<float>.One;
        var v_zero = Vector512<float>.Zero;

        for (var i = es; i + 16 <= ee; i += 16)
        {
            var y0 = Load512(_y0, i);
            var dy = Load512(_dy, i);
            var x0 = Load512(_x0, i);
            var dx = Load512(_dx, i);
            var invL2 = Load512(_invLen2, i);

            var relx = Avx512F.Subtract(v_px, x0);
            var rely = Avx512F.Subtract(v_py, y0);

            // t = ((relx*dx + rely*dy) * invL2) clamped
            var t = Avx512F.Multiply(Avx512F.FusedMultiplyAdd(relx, dx, Avx512F.Multiply(rely, dy)), invL2);
            t = Avx512F.Min(Avx512F.Max(t, v_zero), v_one);

            // nx = x0 + t*dx ; ny = y0 + t*dy
            var nx = Avx512F.FusedMultiplyAdd(t, dx, x0);
            var ny = Avx512F.FusedMultiplyAdd(t, dy, y0);

            var dxp = Avx512F.Subtract(nx, v_px);
            var dyp = Avx512F.Subtract(ny, v_py);
            // d2 = dxp*dxp + dyp*dyp
            var d2 = Avx512F.FusedMultiplyAdd(dxp, dxp, Avx512F.Multiply(dyp, dyp));

            var mask = Vector512.LessThan(d2, Vector512.Create(bestSq)).ExtractMostSignificantBits();
            if (mask != 0ul)
            {
                // update best and carry back the lane's (nx, ny)
                UpdateClosestFromMask(d2, nx, ny, mask, ref bestSq, ref bestX, ref bestY);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelClosest256(int es, int ee, float px, float py, ref float bestSq, ref float bestX, ref float bestY)
    {
        var v_px = Vector256.Create(px);
        var v_py = Vector256.Create(py);
        var v_one = Vector256<float>.One;
        var v_zero = Vector256<float>.Zero;

        for (var i = es; i + 8 <= ee; i += 8)
        {
            var y0 = Load256(_y0, i);
            var dy = Load256(_dy, i);
            var x0 = Load256(_x0, i);
            var dx = Load256(_dx, i);
            var invL2 = Load256(_invLen2, i);

            var relx = Avx.Subtract(v_px, x0);
            var rely = Avx.Subtract(v_py, y0);

            // t = clamp(((relx*dx) + (rely*dy)) * invL2, 0, 1)
            var tDot = HasFMA ? Fma.MultiplyAdd(relx, dx, Avx.Multiply(rely, dy)) : Avx.Add(Avx.Multiply(relx, dx), Avx.Multiply(rely, dy));
            var t = Avx.Multiply(tDot, invL2);
            t = Avx.Min(Avx.Max(t, v_zero), v_one);

            var nx = Avx.Add(x0, Avx.Multiply(t, dx));
            var ny = Avx.Add(y0, Avx.Multiply(t, dy));

            var dxp = Avx.Subtract(nx, v_px);
            var dyp = Avx.Subtract(ny, v_py);
            var d2 = HasFMA ? Fma.MultiplyAdd(dxp, dxp, Avx.Multiply(dyp, dyp)) : Avx.Add(Avx.Multiply(dxp, dxp), Avx.Multiply(dyp, dyp));

            var mask = Vector256.LessThan(d2, Vector256.Create(bestSq)).ExtractMostSignificantBits();
            if (mask != 0u)
            {
                // update best and carry back the lane's (nx, ny)
                UpdateClosestFromMask(d2, nx, ny, mask, ref bestSq, ref bestX, ref bestY);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelClosestScalar(int es, int ee, float px, float py, ref float bestSq, ref float bestX, ref float bestY)
    {
        for (var i = es; i < ee; ++i)
        {
            var y0s = _y0[i];
            var x0s = _x0[i];
            var dys = _dy[i];
            var dxs = _dx[i];
            var invL2 = _invLen2[i];

            var rx = px - x0s;
            var ry = py - y0s;

            var t = (rx * dxs + ry * dys) * invL2;
            if (t < 0f)
            {
                t = 0f;
            }
            else if (t > 1f)
            {
                t = 1f;
            }

            var nx = x0s + t * dxs;
            var ny = y0s + t * dys;

            var dxp = nx - px;
            var dyp = ny - py;
            var d2 = dxp * dxp + dyp * dyp;

            if (d2 < bestSq)
            {
                bestSq = d2;
                bestX = nx;
                bestY = ny;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<float> Load512(float* p, int index)
    {
        ref var first = ref Unsafe.AsRef<float>(p);
        return Vector512.LoadUnsafe(ref first, (nuint)index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<float> Load256(float* p, int index)
    {
        ref var first = ref Unsafe.AsRef<float>(p);
        return Vector256.LoadUnsafe(ref first, (nuint)index);
    }

    public PolygonShapeRelation ClassifyComplexPolygon(RelSimplifiedComplexPolygon polygon)
    {

        var parts = CollectionsMarshal.AsSpan(polygon.Parts);
        var idx = polygon.GetPolygonIndex(polygon);
        var hasFilledPart = false;
        var minX = idx._bbMinX;
        var minY = idx._bbMinY;
        var maxX = idx._bbMaxX;
        var maxY = idx._bbMaxY;

        var lenP = parts.Length;
        for (var i = 0; i < lenP; ++i)
        {
            var part = parts[i];
            var exterior = part.Exterior;
            if (exterior.Length < 3)
            {
                continue;
            }

            hasFilledPart = true;
            break;
        }

        // Treat an empty/degenerate query as having no overlap.
        if (!hasFilledPart)
        {
            return PolygonShapeRelation.Outside;
        }

        if (maxX < _bbMinX - Eps || minX > _bbMaxX + Eps || maxY < _bbMinY - Eps || minY > _bbMaxY + Eps)
        {
            return PolygonShapeRelation.Outside;
        }

        if (BoundaryIntersectsComplexPolygon(polygon))
        {
            return PolygonShapeRelation.Intersecting;
        }

        var anyInside = false;
        var anyOutside = false;

        for (var i = 0; i < lenP; ++i)
        {
            var exterior = parts[i].Exterior;
            if (exterior.Length < 3)
            {
                continue;
            }

            if (Contains(exterior[0]))
            {
                anyInside = true;
            }
            else
            {
                anyOutside = true;
            }

            // A disconnected complex polygon can have one part inside and another outside
            // without either boundary crossing. It is not wholly inside or wholly outside.
            if (anyInside && anyOutside)
            {
                return PolygonShapeRelation.Intersecting;
            }
        }

        // No boundaries cross, but this polygon (or one of its hole contours) can still be
        // completely enclosed by the filled area of the query polygon.
        var lenS = _contourSamples.Length;
        for (var i = 0; i < lenS; ++i)
        {
            if (polygon.Contains(_contourSamples[i]))
            {
                return PolygonShapeRelation.Intersecting;
            }
        }

        return anyInside ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    public PolygonShapeRelation ClassifyCircle(in WDir center, float radius)
    {
        var closest = ClosestPointOnBoundary(center);
        var dx = closest.X - center.X;
        var dz = closest.Z - center.Z;
        var limit = radius + Eps;
        if (dx * dx + dz * dz <= limit * limit)
        {
            return PolygonShapeRelation.Intersecting;
        }

        return Contains(center) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    public PolygonShapeRelation ClassifyRectangle(in WDir center, in WDir forward, float halfWidth, float halfLength)
    {
        NormalizeDirection(forward, out var fx, out var fz);
        return ClassifyRectangleNormalized(center.X, center.Z, fx, fz, halfWidth, halfLength);
    }

    public PolygonShapeRelation ClassifyDirectionalRectangle(in WDir origin, in WDir forward, float lengthFront, float lengthBack, float halfWidth)
    {
        NormalizeDirection(forward, out var fx, out var fz);

        var halfLength = 0.5f * (lengthFront + lengthBack);
        var centerOffset = 0.5f * (lengthFront - lengthBack);
        var cx = origin.X + fx * centerOffset;
        var cz = origin.Z + fz * centerOffset;
        return ClassifyRectangleNormalized(cx, cz, fx, fz, halfWidth, halfLength);
    }

    public PolygonShapeRelation ClassifyDonut(in WDir center, float innerRadius, float outerRadius)
    {
        if (innerRadius == 0f)
        {
            return ClassifyCircle(center, outerRadius);
        }
        var centerX = center.X;
        var centerZ = center.Z;
        if (BoundaryIntersectsDonut(centerX, centerZ, innerRadius, outerRadius))
        {
            return PolygonShapeRelation.Intersecting;
        }

        // An annulus is connected. If no polygon boundary enters it, every point in it has the same classification.

        var sampleRadius = 0.5f * (innerRadius + outerRadius);
        var sample = new WDir(centerX + sampleRadius, centerZ);
        return Contains(sample) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    public PolygonShapeRelation ClassifyCone(in WDir origin, in WDir forward, float radius, Angle halfAngle)
    {
        NormalizeDirection(forward, out var fx, out var fz);
        var ha = halfAngle.Rad;
        if (ha >= MathF.PI)
        {
            return ClassifyCircle(origin, radius);
        }
        var originX = origin.X;
        var originZ = origin.Z;
        if (BoundaryIntersectsCone(originX, originZ, fx, fz, radius, ha))
        {
            return PolygonShapeRelation.Intersecting;
        }

        // A sector is connected. With no polygon boundary in it, one interior sample classifies the whole shape.

        var sampleDistance = 0.5f * radius;
        var sample = new WDir(originX + fx * sampleDistance, originZ + fz * sampleDistance);
        return Contains(sample) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    private PolygonShapeRelation ClassifyRectangleNormalized(float cx, float cz, float fx, float fz, float halfWidth, float halfLength)
    {
        if (BoundaryIntersectsRectangle(cx, cz, fx, fz, halfWidth, halfLength))
        {
            return PolygonShapeRelation.Intersecting;
        }

        var center = new WDir(cx, cz);
        return Contains(center) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    public PolygonShapeRelation ClassifyDonutSector(in WDir center, in WDir forward, float innerRadius, float outerRadius, Angle halfAngle)
    {
        NormalizeDirection(forward, out var fx, out var fz);
        var hA = halfAngle.Rad;
        if (hA >= MathF.PI)
        {
            return ClassifyDonut(center, innerRadius, outerRadius);
        }
        if (innerRadius == 0f)
        {
            return ClassifyCone(center, forward, outerRadius, halfAngle);
        }
        var centerX = center.X;
        var centerZ = center.Z;
        if (BoundaryIntersectsDonutSector(centerX, centerZ, fx, fz, innerRadius, outerRadius, hA))
        {
            return PolygonShapeRelation.Intersecting;
        }

        // An annular sector is connected. A point on its angular bisector classifies the whole shape.
        var sampleRadius = 0.5f * (innerRadius + outerRadius);
        var sample = new WDir(centerX + fx * sampleRadius, centerZ + fz * sampleRadius);
        return Contains(sample) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    public PolygonShapeRelation ClassifyDonutSector(in WDir center, float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd)
    {
        var angularLength = angleEnd - angleStart;
        var halfAngle = 0.5f * MathF.Abs(angularLength.Rad);
        var forward = (angleStart + angularLength * 0.5f).ToDirection();
        return ClassifyDonutSector(center, forward, innerRadius, outerRadius, new(halfAngle));
    }

    public PolygonShapeRelation ClassifyCapsule(in WDir start, in WDir end, float radius)
    {
        var startX = start.X;
        var startZ = start.Z;
        var endX = end.X;
        var endZ = end.Z;
        var dx = end.X - start.X;
        var dz = end.Z - start.Z;
        if (dx * dx + dz * dz <= TinyLen2)
        {
            return ClassifyCircle(start, radius);
        }

        if (BoundaryIntersectsCapsule(startX, startZ, endX, endZ, radius))
        {
            return PolygonShapeRelation.Intersecting;
        }

        var sample = new WDir(0.5f * (startX + endX), 0.5f * (startZ + endZ));
        return Contains(sample) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    public PolygonShapeRelation ClassifyDirectionalCapsule(in WDir origin, in WDir forward, float length, float radius)
    {
        NormalizeDirection(forward, out var fx, out var fz);

        var end = new WDir(origin.X + fx * length, origin.Z + fz * length);
        return ClassifyCapsule(origin, end, radius);
    }

    public PolygonShapeRelation ClassifyArcCapsule(in WDir origin, in WDir toOrbitCenter, Angle angularLength, float radius)
    {
        var cxOffset = toOrbitCenter.X;
        var czOffset = toOrbitCenter.Z;
        var orbitRadiusSq = cxOffset * cxOffset + czOffset * czOffset;

        var sweep = angularLength.Rad;

        var orbitRadius = MathF.Sqrt(orbitRadiusSq);
        var originX = origin.X;
        var originZ = origin.Z;
        var orbitCenter = new WDir(originX + cxOffset, originZ + czOffset);
        var innerRadius = orbitRadius - radius;
        var outerRadius = orbitRadius + radius;
        var absSweep = Math.Abs(sweep);
        if (absSweep >= MathF.Tau - Eps)
        {
            return ClassifyDonut(orbitCenter, innerRadius, outerRadius);
        }

        var invOrbitRadius = 1f / orbitRadius;
        var startRadialX = -cxOffset * invOrbitRadius;
        var startRadialZ = -czOffset * invOrbitRadius;
        Rotate(startRadialX, startRadialZ, 0.5f * sweep, out var midX, out var midZ);
        Rotate(startRadialX, startRadialZ, sweep, out var endRadialX, out var endRadialZ);
        var orbitCenterX = orbitCenter.X;
        var orbitCenterZ = orbitCenter.Z;
        var endX = orbitCenterX + endRadialX * orbitRadius;
        var endZ = orbitCenterZ + endRadialZ * orbitRadius;
        var halfAngle = 0.5f * absSweep;

        if (BoundaryIntersectsArcCapsule(originX, originZ, endX, endZ, orbitCenterX, orbitCenterZ, midX, midZ, innerRadius, outerRadius, halfAngle, radius))
        {
            return PolygonShapeRelation.Intersecting;
        }

        // The swept capsule is connected and the arc start is on its center line.
        return Contains(origin) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    private PolygonShapeRelation ClassifySimplePolygon(ReadOnlySpan<WDir> vertices, in WDir interiorSample)
    {
        if (BoundaryIntersectsSimplePolygon(vertices))
        {
            return PolygonShapeRelation.Intersecting;
        }

        return Contains(interiorSample) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyTriangle(in WDir a, in WDir b, in WDir c)
    {

        Span<WDir> vertices = [a, b, c];
        var sample = new WDir((a.X + b.X + c.X) / 3f, (a.Z + b.Z + c.Z) / 3f);
        return ClassifySimplePolygon(vertices, sample);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyTri(in WDir a, in WDir b, in WDir c) => ClassifyTriangle(a, b, c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyIsoscelesTriangle(in WDir apexOffset, in WDir height, in WDir halfBase)
        => ClassifyTriangle(apexOffset, apexOffset + height + halfBase, apexOffset + height - halfBase);

    public PolygonShapeRelation ClassifyIsoscelesTriangle(in WDir apexOffset, Angle direction, Angle halfAngle, float height)
    {
        var halfAngleRad = halfAngle.Rad;
        var dir = direction.ToDirection();
        NormalizeDirection(dir, out var fx, out var fz);
        var normal = new WDir(-fz, fx);
        var heightOffset = new WDir(fx * height, fz * height);
        var halfBase = height * MathF.Tan(halfAngleRad) * normal;
        return ClassifyIsoscelesTriangle(apexOffset, heightOffset, halfBase);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyIsoscelesTri(in WDir apexOffset, in WDir height, in WDir halfBase) => ClassifyIsoscelesTriangle(apexOffset, height, halfBase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyIsoscelesTri(in WDir apexOffset, Angle direction, Angle halfAngle, float height)
        => ClassifyIsoscelesTriangle(apexOffset, direction, halfAngle, height);

    private static void AccumulateBounds(ReadOnlySpan<WDir> contour, ref float minX, ref float minY, ref float maxX, ref float maxY)
    {
        var len = contour.Length;
        for (var i = 0; i < len; ++i)
        {
            var point = contour[i];
            var pointX = point.X;
            var pointZ = point.Z;
            minX = Math.Min(minX, pointX);
            minY = Math.Min(minY, pointZ);
            maxX = Math.Max(maxX, pointX);
            maxY = Math.Max(maxY, pointZ);
        }
    }

    private bool BoundaryIntersectsComplexPolygon(RelSimplifiedComplexPolygon polygon)
    {
        var parts = CollectionsMarshal.AsSpan(polygon.Parts);
        for (var i = 0; i < parts.Length; ++i)
        {
            var part = parts[i];
            var exterior = part.Exterior;
            if (exterior.Length < 3)
            {
                continue;
            }

            if (BoundaryIntersectsContour(exterior))
            {
                return true;
            }

            var holeCount = part.HoleStarts.Count;
            for (var h = 0; h < holeCount; ++h)
            {
                var interior = part.Interior(h);
                if (interior.Length >= 2 && BoundaryIntersectsContour(interior))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool BoundaryIntersectsContour(ReadOnlySpan<WDir> contour)
    {
        if (contour.Length < 2)
        {
            return false;
        }

        var previous = contour[^1];
        var len = contour.Length;
        for (var i = 0; i < len; ++i)
        {
            var current = contour[i];
            if (BoundaryIntersectsSegment(previous.X, previous.Z, current.X, current.Z))
            {
                return true;
            }
            previous = current;
        }

        return false;
    }

    private bool BoundaryIntersectsSegment(float ax, float ay, float bx, float by)
    {
        var minX = Math.Min(ax, bx) - Eps;
        var maxX = Math.Max(ax, bx) + Eps;
        var minY = Math.Min(ay, by);
        var maxY = Math.Max(ay, by);

        if (maxX < _bbMinX - Eps || minX > _bbMaxX + Eps || maxY < _bbMinY - Eps || minY > _bbMaxY + Eps || !TryGetRowRange(minY, maxY, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            if (maxX < _rowMinX[row] - Eps || minX > _rowMaxX[row] + Eps)
            {
                continue;
            }

            int es = _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var cy = _y0[i];
                if (_maxX[i] < minX || _minX[i] > maxX)
                {
                    continue;
                }

                var cx = _x0[i];
                if (SegmentsIntersect(ax, ay, bx, by, cx, cy, cx + _dx[i], cy + _dy[i]))
                {
                    return true;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[_hRowIdx[h]];

                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;

                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }

                var edgeY = edge.y;

                if (SegmentsIntersect(ax, ay, bx, by, edgeMinX, edgeY, edgeMaxX, edgeY))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool BoundaryIntersectsSimplePolygon(ReadOnlySpan<WDir> vertices)
    {
        if (vertices.Length < 2)
        {
            return false;
        }

        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;
        var len = vertices.Length;
        for (var i = 0; i < len; ++i)
        {
            var v = vertices[i];
            minX = Math.Min(minX, v.X);
            minY = Math.Min(minY, v.Z);
            maxX = Math.Max(maxX, v.X);
            maxY = Math.Max(maxY, v.Z);
        }

        var worldMinX = minX - Eps;
        var worldMaxX = maxX + Eps;
        if (!TryGetRowRange(minY, maxY, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            int es = _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var ay = _y0[i];
                if (!IsFirstOverlappingCopy(row, row0, ay))
                {
                    continue;
                }
                if (_maxX[i] < worldMinX || _minX[i] > worldMaxX)
                {
                    continue;
                }

                var ax = _x0[i];
                var bx = ax + _dx[i];
                var by = ay + _dy[i];
                if (SegmentIntersectsFilledSimplePolygon(ax, ay, bx, by, vertices))
                {
                    return true;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[_hRowIdx[h]];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;

                if (edgeMaxX < worldMinX || edgeMinX > worldMaxX)
                {
                    continue;
                }
                var edgeY = edge.y;
                if (SegmentIntersectsFilledSimplePolygon(edgeMinX, edgeY, edgeMaxX, edgeY, vertices))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool SegmentIntersectsFilledSimplePolygon(float ax, float ay, float bx, float by, ReadOnlySpan<WDir> vertices)
    {
        if (PointInSimplePolygon(ax, ay, vertices) || PointInSimplePolygon(bx, by, vertices))
        {
            return true;
        }

        var prev = vertices[^1];
        var len = vertices.Length;
        for (var i = 0; i < len; ++i)
        {
            var cur = vertices[i];
            if (SegmentsIntersect(ax, ay, bx, by, prev.X, prev.Z, cur.X, cur.Z))
            {
                return true;
            }
            prev = cur;
        }

        return false;
    }

    private static bool PointInSimplePolygon(float px, float py, ReadOnlySpan<WDir> vertices)
    {
        var inside = false;
        var prev = vertices[^1];
        var len = vertices.Length;
        for (var i = 0; i < len; ++i)
        {
            var cur = vertices[i];
            var prevX = prev.X;
            var prevZ = prev.Z;
            var curX = cur.X;
            var curZ = cur.Z;
            if (PointSegmentDistanceSq(px, py, prevX, prevZ, curX, curZ) <= Eps2)
            {
                return true;
            }

            if ((prevZ > py) != (curZ > py))
            {
                var crossingX = prevX + (py - prevZ) * (curX - prevX) / (curZ - prevZ);
                if (crossingX > px)
                {
                    inside = !inside;
                }
            }
            prev = cur;
        }

        return inside;
    }

    private bool BoundaryIntersectsDonutSector(float cx, float cz, float fx, float fz, float innerRadius, float outerRadius, float halfAngle)
    {
        var reach = outerRadius + Eps;
        var minX = cx - reach;
        var maxX = cx + reach;
        var sector = new AnnularSectorGeometry(cx, cz, fx, fz, innerRadius, outerRadius, halfAngle);

        if (!TryGetRowRange(cz - reach, cz + reach, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            int es = _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var ay = _y0[i];
                if (!IsFirstOverlappingCopy(row, row0, ay))
                {
                    continue;
                }
                if (_maxX[i] < minX || _minX[i] > maxX)
                {
                    continue;
                }

                var ax = _x0[i];
                var bx = ax + _dx[i];
                var by = ay + _dy[i];
                if (SegmentIntersectsAnnularSector(ax, ay, bx, by, sector))
                {
                    return true;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[_hRowIdx[h]];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;
                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }

                var edgeY = edge.y;
                if (SegmentIntersectsAnnularSector(edgeMinX, edgeY, edgeMaxX, edgeY, sector))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool BoundaryIntersectsCapsule(float ax, float az, float bx, float bz, float radius)
    {
        var reach = radius + Eps;
        var minX = Math.Min(ax, bx) - reach;
        var maxX = Math.Max(ax, bx) + reach;
        var minZ = Math.Min(az, bz) - reach;
        var maxZ = Math.Max(az, bz) + reach;
        var reachSq = reach * reach;

        if (!TryGetRowRange(minZ, maxZ, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            int es = _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var ey0 = _y0[i];
                if (!IsFirstOverlappingCopy(row, row0, ey0))
                {
                    continue;
                }

                if (_maxX[i] < minX || _minX[i] > maxX)
                {
                    continue;
                }

                var ex0 = _x0[i];
                var ex1 = ex0 + _dx[i];
                var ey1 = ey0 + _dy[i];
                if (SegmentSegmentDistanceSq(ax, az, bx, bz, ex0, ey0, ex1, ey1) <= reachSq)
                {
                    return true;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[_hRowIdx[h]];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;
                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }

                var edgeY = edge.y;

                if (SegmentSegmentDistanceSq(ax, az, bx, bz, edgeMinX, edgeY, edgeMaxX, edgeY) <= reachSq)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool BoundaryIntersectsArcCapsule(float startX, float startZ, float endX, float endZ, float cx, float cz, float midX, float midZ, float innerRadius, float outerRadius,
        float halfAngle, float capsuleRadius)
    {
        var reach = outerRadius + Eps;
        var minX = cx - reach;
        var maxX = cx + reach;
        var capReach = capsuleRadius + Eps;
        var capReachSq = capReach * capReach;
        var sector = new AnnularSectorGeometry(cx, cz, midX, midZ, innerRadius, outerRadius, halfAngle);

        if (!TryGetRowRange(cz - reach, cz + reach, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            int es = _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var ay = _y0[i];
                if (!IsFirstOverlappingCopy(row, row0, ay))
                {
                    continue;
                }

                if (_maxX[i] < minX || _minX[i] > maxX)
                {
                    continue;
                }

                var ax = _x0[i];
                var bx = ax + _dx[i];
                var by = ay + _dy[i];
                if (PointSegmentDistanceSq(startX, startZ, ax, ay, bx, by) <= capReachSq || PointSegmentDistanceSq(endX, endZ, ax, ay, bx, by) <= capReachSq
                    || SegmentIntersectsAnnularSector(ax, ay, bx, by, sector))
                {
                    return true;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[_hRowIdx[h]];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;
                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }

                var edgeY = edge.y;
                if (PointSegmentDistanceSq(startX, startZ, edgeMinX, edgeY, edgeMaxX, edgeY) <= capReachSq
                    || PointSegmentDistanceSq(endX, endZ, edgeMinX, edgeY, edgeMaxX, edgeY) <= capReachSq
                    || SegmentIntersectsAnnularSector(edgeMinX, edgeY, edgeMaxX, edgeY, sector))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool BoundaryIntersectsRectangle(float cx, float cz, float fx, float fz, float halfWidth, float halfLength)
    {
        // right is forward rotated clockwise by 90 degrees

        var rx = -fz;
        var rz = fx;
        var extentX = Math.Abs(rx) * halfWidth + Math.Abs(fx) * halfLength;
        var extentZ = Math.Abs(rz) * halfWidth + Math.Abs(fz) * halfLength;
        var worldMinX = cx - extentX - Eps;
        var worldMaxX = cx + extentX + Eps;

        if (!TryGetRowRange(cz - extentZ, cz + extentZ, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            int es = _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var ay = _y0[i];
                if (!IsFirstOverlappingCopy(row, row0, ay))
                {
                    continue;
                }

                if (_maxX[i] < worldMinX || _minX[i] > worldMaxX)
                {
                    continue;
                }

                var ax = _x0[i];
                var bx = ax + _dx[i];
                var by = ay + _dy[i];
                if (SegmentIntersectsOrientedRectangle(ax, ay, bx, by, cx, cz, rx, rz, fx, fz, halfWidth, halfLength))
                {
                    return true;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[_hRowIdx[h]];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;
                if (edgeMaxX < worldMinX || edgeMinX > worldMaxX)
                {
                    continue;
                }
                var edgeY = edge.y;
                if (SegmentIntersectsOrientedRectangle(edgeMinX, edgeY, edgeMaxX, edgeY, cx, cz, rx, rz, fx, fz, halfWidth, halfLength))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool BoundaryIntersectsDonut(float cx, float cz, float innerRadius, float outerRadius)
    {
        var outerExpanded = outerRadius + Eps;
        var innerShrunk = Math.Max(0f, innerRadius - Eps);
        var outerSq = outerExpanded * outerExpanded;
        var innerSq = innerShrunk * innerShrunk;
        var minX = cx - outerExpanded;
        var maxX = cx + outerExpanded;

        if (!TryGetRowRange(cz - outerExpanded, cz + outerExpanded, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            int es = _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var ay = _y0[i];
                if (!IsFirstOverlappingCopy(row, row0, ay))
                {
                    continue;
                }
                if (_maxX[i] < minX || _minX[i] > maxX)
                {
                    continue;
                }

                var ax = _x0[i];
                var bx = ax + _dx[i];
                var by = ay + _dy[i];
                if (SegmentDistanceRangeOverlapsAnnulus(ax, ay, bx, by, cx, cz, innerSq, outerSq))
                {
                    return true;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[_hRowIdx[h]];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;

                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }
                var edgeY = edge.y;
                if (SegmentDistanceRangeOverlapsAnnulus(edgeMinX, edgeY, edgeMaxX, edgeY, cx, cz, innerSq, outerSq))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool BoundaryIntersectsCone(float ox, float oz, float fx, float fz, float radius, float halfAngle)
    {
        var reach = radius + Eps;
        var minX = ox - reach;
        var maxX = ox + reach;
        var sector = new SectorGeometry(ox, oz, fx, fz, radius, halfAngle);

        if (!TryGetRowRange(oz - reach, oz + reach, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            int es = _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var ay = _y0[i];
                if (!IsFirstOverlappingCopy(row, row0, ay))
                {
                    continue;
                }
                if (_maxX[i] < minX || _minX[i] > maxX)
                {
                    continue;
                }

                var ax = _x0[i];
                var bx = ax + _dx[i];
                var by = ay + _dy[i];
                if (SegmentIntersectsSector(ax, ay, bx, by, sector))
                {
                    return true;
                }
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[_hRowIdx[h]];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;

                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }
                var edgeY = edge.y;
                if (SegmentIntersectsSector(edgeMinX, edgeY, edgeMaxX, edgeY, sector))
                {
                    return true;
                }
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsFirstOverlappingCopy(int row, int queryRow0, float edgeMinY)
    {
        // Non-horizontal edges are duplicated into every row they span. Expensive
        // shape tests only need one copy: the first row shared by edge and query.
        var edgeRow0 = (int)((edgeMinY - _minY) * _invCellH);
        if (edgeRow0 >= _rows)
        {
            edgeRow0 = _rows - 1;
        }
        return row == Math.Max(queryRow0, edgeRow0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetRowRange(float minY, float maxY, out int row0, out int row1)
    {
        // Rows use top-exclusive edge storage, so always include a tiny neighboring band
        minY -= Eps;
        maxY += Eps;
        if (maxY < _bbMinY || minY > _bbMaxY)
        {
            row0 = row1 = 0;
            return false;
        }

        row0 = ClampRow(minY);
        row1 = ClampRow(maxY);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SegmentIntersectsOrientedRectangle(float ax, float ay, float bx, float by, float cx, float cz, float rx, float rz, float fx, float fz, float halfWidth, float halfLength)
    {
        var adx = ax - cx;
        var adz = ay - cz;
        var bdx = bx - cx;
        var bdz = by - cz;

        var localAX = adx * rx + adz * rz;
        var localAY = adx * fx + adz * fz;
        var localBX = bdx * rx + bdz * rz;
        var localBY = bdx * fx + bdz * fz;
        return SegmentIntersectsAABB(localAX, localAY, localBX, localBY, halfWidth + Eps, halfLength + Eps);
    }

    private static bool SegmentIntersectsAABB(float ax, float ay, float bx, float by, float halfX, float halfY)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var tMin = 0f;
        var tMax = 1f;

        if (!ClipSlab(ax, dx, halfX, ref tMin, ref tMax))
        {
            return false;
        }
        return ClipSlab(ay, dy, halfY, ref tMin, ref tMax);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool ClipSlab(float origin, float delta, float halfExtent, ref float t0, ref float t1)
        {
            if (Math.Abs(delta) <= TinyDen)
            {
                return origin >= -halfExtent && origin <= halfExtent;
            }

            var inv = 1f / delta;
            var near = (-halfExtent - origin) * inv;
            var far = (halfExtent - origin) * inv;
            if (near > far)
            {
                (near, far) = (far, near);
            }

            t0 = Math.Max(t0, near);
            t1 = Math.Min(t1, far);
            return t0 <= t1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SegmentDistanceRangeOverlapsAnnulus(float ax, float ay, float bx, float by, float cx, float cz, float innerSq, float outerSq)
    {
        var daX = ax - cx;
        var daY = ay - cz;
        var dbX = bx - cx;
        var dbY = by - cz;
        var maxSq = Math.Max(daX * daX + daY * daY, dbX * dbX + dbY * dbY);
        if (maxSq < innerSq)
        {
            return false;
        }

        var minSq = PointSegmentDistanceSq(cx, cz, ax, ay, bx, by);
        return minSq <= outerSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SegmentIntersectsSector(float ax, float ay, float bx, float by, in SectorGeometry sector)
    {
        if (PointInSector(ax, ay, sector) || PointInSector(bx, by, sector))
        {
            return true;
        }

        if (SegmentsIntersect(ax, ay, bx, by, sector.ox, sector.oz, sector.leftX, sector.leftZ)
         || SegmentsIntersect(ax, ay, bx, by, sector.ox, sector.oz, sector.rightX, sector.rightZ))
        {
            return true;
        }

        return SegmentIntersectsSectorArc(ax, ay, bx, by, sector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SegmentIntersectsAnnularSector(float ax, float ay, float bx, float by, in AnnularSectorGeometry sector)
    {
        if (PointInAnnularSector(ax, ay, sector) || PointInAnnularSector(bx, by, sector))
        {
            return true;
        }

        if (SegmentsIntersect(ax, ay, bx, by, sector.leftInnerX, sector.leftInnerZ, sector.leftOuterX, sector.leftOuterZ)
         || SegmentsIntersect(ax, ay, bx, by, sector.rightInnerX, sector.rightInnerZ, sector.rightOuterX, sector.rightOuterZ))
        {
            return true;
        }

        if (SegmentIntersectsCircleArcAtRadius(ax, ay, bx, by, sector, sector.outer))
        {
            return true;
        }
        return sector.inner > 0f && SegmentIntersectsCircleArcAtRadius(ax, ay, bx, by, sector, sector.inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointInAnnularSector(float px, float py, in AnnularSectorGeometry sector)
    {
        var dx = px - sector.cx;
        var dy = py - sector.cz;
        var distanceSq = dx * dx + dy * dy;
        return distanceSq >= sector.innerSq && distanceSq <= sector.outerSq
            && PointWithinConeAngle(dx, dy, sector.fx, sector.fz, sector.cosHalfAngle);
    }

    private static bool SegmentIntersectsSectorArc(float ax, float ay, float bx, float by, in SectorGeometry sector)
    {
        var r = sector.radius;
        var ox = sector.ox;
        var oz = sector.oz;
        var fx = sector.fx;
        var fz = sector.fz;
        var cosHalfAngle = sector.cosHalfAngle;
        if (r <= 0f)
        {
            return PointSegmentDistanceSq(ox, oz, ax, ay, bx, by) <= Eps2;
        }

        var dx = bx - ax;
        var dy = by - ay;
        var relX = ax - ox;
        var relY = ay - oz;
        var qa = dx * dx + dy * dy;
        if (qa <= TinyLen2)
        {
            return false;
        }

        var qb = 2f * (relX * dx + relY * dy);
        var qc = relX * relX + relY * relY - r * r;
        var discriminant = qb * qb - 4f * qa * qc;
        if (discriminant < 0f)
        {
            return false;
        }

        var sqrtD = MathF.Sqrt(Math.Max(0f, discriminant));
        var inv2A = 0.5f / qa;
        var t0 = (-qb - sqrtD) * inv2A;
        var t1 = (-qb + sqrtD) * inv2A;
        return RootOnArc(t0) || RootOnArc(t1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool RootOnArc(float t)
        {
            if (t is < 0f or > 1f)
            {
                return false;
            }
            var px = ax + dx * t - ox;
            var py = ay + dy * t - oz;
            return PointWithinConeAngle(px, py, fx, fz, cosHalfAngle);
        }
    }

    private static bool SegmentIntersectsCircleArcAtRadius(float ax, float ay, float bx, float by, in AnnularSectorGeometry sector, float radius)
    {
        var cx = sector.cx;
        var cz = sector.cz;
        var fx = sector.fx;
        var fz = sector.fz;
        var cosHalfAngle = sector.cosHalfAngle;
        if (radius <= 0f)
        {
            return PointSegmentDistanceSq(cx, cz, ax, ay, bx, by) <= Eps2;
        }

        var dx = bx - ax;
        var dy = by - ay;
        var relX = ax - cx;
        var relY = ay - cz;
        var qa = dx * dx + dy * dy;
        if (qa <= TinyLen2)
        {
            return false;
        }

        var qb = 2f * (relX * dx + relY * dy);
        var qc = relX * relX + relY * relY - radius * radius;
        var discriminant = qb * qb - 4f * qa * qc;
        if (discriminant < 0f)
        {
            return false;
        }

        var sqrtD = MathF.Sqrt(Math.Max(0f, discriminant));
        var inv2A = 0.5f / qa;
        var t0 = (-qb - sqrtD) * inv2A;
        var t1 = (-qb + sqrtD) * inv2A;
        return RootOnArc(t0) || RootOnArc(t1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool RootOnArc(float t)
        {
            if (t is < 0f or > 1f)
            {
                return false;
            }
            var px = ax + dx * t - cx;
            var py = ay + dy * t - cz;
            return PointWithinConeAngle(px, py, fx, fz, cosHalfAngle);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointInSector(float px, float py, in SectorGeometry sector)
    {
        var dx = px - sector.ox;
        var dy = py - sector.oz;
        var distanceSq = dx * dx + dy * dy;
        if (distanceSq > sector.radiusSq)
        {
            return false;
        }
        if (distanceSq <= Eps2)
        {
            return true;
        }

        return PointWithinConeAngle(dx, dy, sector.fx, sector.fz, sector.cosHalfAngle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointWithinConeAngle(float dx, float dy, float fx, float fz, float cosHalfAngle)
    {
        var distanceSq = dx * dx + dy * dy;
        if (distanceSq <= TinyLen2)
        {
            return true;
        }

        var projection = dx * fx + dy * fz;
        var angularLimit = MathF.Sqrt(distanceSq) * cosHalfAngle;
        return projection >= angularLimit - Eps;
    }

    private static bool SegmentsIntersect(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy)
    {
        if (Math.Max(ax, bx) + Eps < Math.Min(cx, dx) || Math.Max(cx, dx) + Eps < Math.Min(ax, bx) || Math.Max(ay, by) + Eps < Math.Min(cy, dy) || Math.Max(cy, dy) + Eps < Math.Min(ay, by))
        {
            return false;
        }

        var abX = bx - ax;
        var abY = by - ay;
        var cdX = dx - cx;
        var cdY = dy - cy;
        var acX = cx - ax;
        var acY = cy - ay;
        var adX = dx - ax;
        var adY = dy - ay;
        var caX = ax - cx;
        var caY = ay - cy;
        var cbX = bx - cx;
        var cbY = by - cy;

        var c1 = Cross(abX, abY, acX, acY);
        var c2 = Cross(abX, abY, adX, adY);
        var c3 = Cross(cdX, cdY, caX, caY);
        var c4 = Cross(cdX, cdY, cbX, cbY);
        if ((c1 > 0f && c2 < 0f || c1 < 0f && c2 > 0f) && (c3 > 0f && c4 < 0f || c3 < 0f && c4 > 0f))
        {
            return true;
        }

        return PointSegmentDistanceSq(ax, ay, cx, cy, dx, dy) <= Eps2 || PointSegmentDistanceSq(bx, by, cx, cy, dx, dy) <= Eps2
        || PointSegmentDistanceSq(cx, cy, ax, ay, bx, by) <= Eps2 || PointSegmentDistanceSq(dx, dy, ax, ay, bx, by) <= Eps2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float PointSegmentDistanceSq(float px, float py, float ax, float ay, float bx, float by)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var lenSq = dx * dx + dy * dy;
        if (lenSq <= TinyLen2)
        {
            var ex = px - ax;
            var ey = py - ay;
            return ex * ex + ey * ey;
        }

        var t = ((px - ax) * dx + (py - ay) * dy) / lenSq;
        t = Math.Clamp(t, 0f, 1f);
        var nx = ax + t * dx;
        var ny = ay + t * dy;
        var rx = px - nx;
        var ry = py - ny;
        return rx * rx + ry * ry;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float SegmentSegmentDistanceSq(float ax, float ay, float bx, float by,
        float cx, float cy, float dx, float dy)
    {
        if (SegmentsIntersect(ax, ay, bx, by, cx, cy, dx, dy))
        {
            return 0f;
        }

        return Math.Min(
            Math.Min(PointSegmentDistanceSq(ax, ay, cx, cy, dx, dy), PointSegmentDistanceSq(bx, by, cx, cy, dx, dy)),
            Math.Min(PointSegmentDistanceSq(cx, cy, ax, ay, bx, by), PointSegmentDistanceSq(dx, dy, ax, ay, bx, by)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Rotate(float x, float z, float angle, out float rotatedX, out float rotatedZ)
    {
        var (sin, cos) = ((float, float))Math.SinCos(angle);
        rotatedX = x * cos - z * sin;
        rotatedZ = x * sin + z * cos;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Cross(float ax, float ay, float bx, float by) => ax * by - ay * bx;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NormalizeDirection(in WDir direction, out float x, out float z)
    {
        var dirX = direction.X;
        var dirZ = direction.Z;
        var lenSq = dirX * dirX + dirZ * dirZ;
        if (lenSq <= TinyLen2)
        {
            x = 1f;
            z = 0f;
            return;
        }
        var invLen = 1f / MathF.Sqrt(lenSq);
        x = dirX * invLen;
        z = dirZ * invLen;
    }
}
