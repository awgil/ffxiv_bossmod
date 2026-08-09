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
                k = (bx - ax) / (by - ay);
            }
            else
            {
                y0 = by;
                y1 = ay;
                x0 = bx;
                k = (ax - bx) / (ay - by);
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

    private static readonly IComparer<H> HorizontalComparer = Comparer<H>.Create(static (a, b) => a.y.CompareTo(b.y));

    private readonly ref struct SectorGeometry
    {
        public readonly float ox, oz, fx, fz;
        public readonly float radius, radiusSq, cosHalfAngle;
        public readonly float leftX, leftZ, rightX, rightZ;
        public readonly float minX, minZ, maxX, maxZ;

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

            // Exact AABB of the circular sector: origin + arc endpoints + any cardinal arc extrema contained by the angular interval.
            var loX = Math.Min(ox, Math.Min(leftX, rightX));
            var hiX = Math.Max(ox, Math.Max(leftX, rightX));
            var loZ = Math.Min(oz, Math.Min(leftZ, rightZ));
            var hiZ = Math.Max(oz, Math.Max(leftZ, rightZ));
            if (fx >= cos)
            {
                hiX = ox + this.radius;
            }
            if (-fx >= cos)
            {
                loX = ox - this.radius;
            }
            if (fz >= cos)
            {
                hiZ = oz + this.radius;
            }
            if (-fz >= cos)
            {
                loZ = oz - this.radius;
            }
            minX = loX;
            minZ = loZ;
            maxX = hiX;
            maxZ = hiZ;
        }
    }

    private readonly ref struct AnnularSectorGeometry
    {
        public readonly float cx, cz, fx, fz;
        public readonly float inner, outer, innerSq, outerSq, cosHalfAngle;
        public readonly float leftInnerX, leftInnerZ, leftOuterX, leftOuterZ;
        public readonly float rightInnerX, rightInnerZ, rightOuterX, rightOuterZ;
        public readonly float minX, minZ, maxX, maxZ;

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

            var loX = Math.Min(cx, Math.Min(leftOuterX, rightOuterX));
            var hiX = Math.Max(cx, Math.Max(leftOuterX, rightOuterX));
            var loZ = Math.Min(cz, Math.Min(leftOuterZ, rightOuterZ));
            var hiZ = Math.Max(cz, Math.Max(leftOuterZ, rightOuterZ));
            if (fx >= cos)
            {
                hiX = cx + outer;
            }
            if (-fx >= cos)
            {
                loX = cx - outer;
            }
            if (fz >= cos)
            {
                hiZ = cz + outer;
            }
            if (-fz >= cos)
            {
                loZ = cz - outer;
            }
            minX = loX;
            minZ = loZ;
            maxX = hiX;
            maxZ = hiZ;
        }
    }

    private readonly ref struct TriangleGeometry
    {
        public readonly float ax, ay, bx, by, cx, cy;
        public readonly float minX, minY, maxX, maxY;
        public readonly float e0x, e0y, e0InvLen2; // c -> a
        public readonly float e1x, e1y, e1InvLen2; // a -> b
        public readonly float e2x, e2y, e2InvLen2; // b -> c
        public readonly float area2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TriangleGeometry(in WDir a, in WDir b, in WDir c)
        {
            ax = a.X;
            ay = a.Z;
            bx = b.X;
            by = b.Z;
            cx = c.X;
            cy = c.Z;

            minX = Math.Min(ax, Math.Min(bx, cx));
            minY = Math.Min(ay, Math.Min(by, cy));
            maxX = Math.Max(ax, Math.Max(bx, cx));
            maxY = Math.Max(ay, Math.Max(by, cy));

            e0x = ax - cx;
            e0y = ay - cy;
            e0InvLen2 = 1f / Math.Max(e0x * e0x + e0y * e0y, TinyLen2);
            e1x = bx - ax;
            e1y = by - ay;
            e1InvLen2 = 1f / Math.Max(e1x * e1x + e1y * e1y, TinyLen2);
            e2x = cx - bx;
            e2y = cy - by;
            e2InvLen2 = 1f / Math.Max(e2x * e2x + e2y * e2y, TinyLen2);
            area2 = (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
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
    private readonly H[] _hEdges; // grouped contiguously by row
    private readonly int[] _hRowOffsets; // rows+1
    private readonly int[] _rowOffsets; // rows+1 (into SoA)
    private readonly int[] _rowEndingStarts; // first carry-in edge whose source ends in this row
    private readonly int[] _rowNewStarts; // first edge whose source starts in this row
    private readonly int[] _rowSingleStarts; // first edge fully contained in this row
    private readonly int[] _rowEnds; // actual (unpadded) end of every row

    private readonly float[] _rowMinX;
    private readonly float[] _rowMaxX;

    private readonly int _rows;
    private readonly int _sourceEdgeCount;
    private readonly float _minY, _cellH, _invCellH;
    private readonly float _bbMinX, _bbMinY, _bbMaxX, _bbMaxY;

    // One point on every source contour (exteriors and holes). These are used to
    // detect source contours completely enclosed by a query polygon without any edge crossing.
    private readonly WDir[] _contourSamples;

    private bool _disposed;

    private PolygonBoundaryIndex2D(float* y0, float* y1, float* x0, float* k, float* b, float* minX, float* maxX, float* dx, float* dy, float* invLen2, int total,
        int[] rowOffsets, int[] rowEndingStarts, int[] rowNewStarts, int[] rowSingleStarts, int[] rowEnds, H[] hEdges, int[] hRowOffsets, int rows, int sourceEdgeCount,
        float minY, float cellH, float invCellH, float bbMinX, float bbMinY, float bbMaxX, float bbMaxY, float[] rowMinX, float[] rowMaxX, WDir[] contourSamples, void* rawBlock)
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
        _rowEndingStarts = rowEndingStarts;
        _rowNewStarts = rowNewStarts;
        _rowSingleStarts = rowSingleStarts;
        _rowEnds = rowEnds;
        _hEdges = hEdges;
        _hRowOffsets = hRowOffsets;
        _rows = rows;
        _sourceEdgeCount = sourceEdgeCount;
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

    public static PolygonBoundaryIndex2D Build(RelSimplifiedComplexPolygon complex)
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
            ProcessExteriorContour(ext, eList, hList, ref bbMinX, ref bbMinY, ref bbMaxX, ref bbMaxY);
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
        var hEdges = CollectionsMarshal.AsSpan(hList);

        // Rowing
        var lenEdges = edges.Length;
        var lenH = hEdges.Length;
        var nEdges = Math.Max(lenEdges + lenH, 1);
        const int MaxRows = 512;
        const int MinRows = 4;
        var rows = nEdges switch
        {
            <= 4 => 1,
            <= 8 => 2,
            <= 16 => 4,
            _ => Math.Clamp((int)MathF.Round(MathF.Sqrt(nEdges) * 0.9f) + 8, MinRows, MaxRows)
        };

        var height = Math.Max(bbMaxY - bbMinY, Eps);
        var cellH = height / rows;
        var invCellH = 1f / cellH;

        Span<int> counts = stackalloc int[rows];
        Span<int> countDeltas = stackalloc int[rows + 1];
        Span<int> newCounts = stackalloc int[rows];
        Span<int> endingCarryCounts = stackalloc int[rows];
        Span<int> singleCounts = stackalloc int[rows];
        Span<int> hCounts = stackalloc int[rows];
        counts.Clear();
        countDeltas.Clear();
        newCounts.Clear();
        endingCarryCounts.Clear();
        singleCounts.Clear();
        hCounts.Clear();

        const int MaxStackEdges = 4096;
        var shouldstackalloc = lenEdges <= MaxStackEdges;
        var edgeRowStarts = shouldstackalloc ? stackalloc int[lenEdges] : new int[lenEdges];
        var edgeRowEnds = shouldstackalloc ? stackalloc int[lenEdges] : new int[lenEdges];

        // Count row copies with a difference array. This avoids walking every spanned row once here and then again while materializing the SoA.
        for (var idx = 0; idx < lenEdges; ++idx)
        {
            ref readonly var e = ref edges[idx];
            var y1 = MathF.BitDecrement(e.y1); // top-exclusive
            var r0 = (int)((e.y0 - bbMinY) * invCellH);
            var r1 = (int)((y1 - bbMinY) * invCellH);
            if (r0 < 0)
            {
                r0 = 0;
            }
            if (r1 >= rows)
            {
                r1 = rows - 1;
            }

            edgeRowStarts[idx] = r0;
            edgeRowEnds[idx] = r1;
            ++countDeltas[r0];
            --countDeltas[r1 + 1];
            ++newCounts[r0];

            if (r0 == r1)
            {
                ++singleCounts[r0];
            }
            else
            {
                ++endingCarryCounts[r1];
            }
        }

        var activeCount = 0;
        for (var r = 0; r < rows; ++r)
        {
            activeCount += countDeltas[r];
            counts[r] = activeCount;
        }

        // horizontal counts
        for (int idx = 0, hN = lenH; idx < hN; ++idx)
        {
            ref readonly var hEdge = ref hEdges[idx];
            var r = (int)((hEdge.y - bbMinY) * invCellH);
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

        // Use JIT-recognized hardware predicates directly so unsupported paths are removed
        var padWidth = Avx512F.IsSupported ? 16 : Avx2.IsSupported ? 8 : 1;

        // prefix sums with padding
        var rowOffsets = new int[rows + 1]; // padded offsets
        var rowEndingStarts = new int[rows]; // continuing carry-in edges precede ending carry-in edges
        var rowNewStarts = new int[rows]; // carry-in edges precede edges starting in this row
        var rowSingleStarts = new int[rows]; // multi-row starts precede single-row edges
        var rowEnds = new int[rows]; // actual ends, excluding SIMD padding
        var total = 0;
        for (var r = 0; r < rows; ++r)
        {
            rowOffsets[r] = total;
            var c = counts[r];
            var carry = c - newCounts[r];
            var pc = padWidth == 1 ? c : RoundUp(c, padWidth);
            rowEndingStarts[r] = total + carry - endingCarryCounts[r];
            rowNewStarts[r] = total + carry;
            rowSingleStarts[r] = total + c - singleCounts[r];
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
        var hEdgesByRow = new H[hTotal];

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

        // Row order is:
        // continuing carry-in, ending carry-in, multi-row starts, single-row edges.
        // This keeps starts contiguous for ascending scans and gives descending
        // closest-point scans two compact ranges containing only edges ending here.
        Span<int> carryWpos = stackalloc int[rows];
        Span<int> endingWpos = stackalloc int[rows];
        Span<int> newWpos = stackalloc int[rows];
        Span<int> singleWpos = stackalloc int[rows];
        rowOffsets.AsSpan(0, rows).CopyTo(carryWpos);
        rowEndingStarts.AsSpan().CopyTo(endingWpos);
        rowNewStarts.AsSpan().CopyTo(newWpos);
        rowSingleStarts.AsSpan().CopyTo(singleWpos);

        // per-row conservative X bounds (init here, update while filling)
        var rowMinX = new float[rows];
        var rowMaxX = new float[rows];
        Array.Fill(rowMinX, float.PositiveInfinity);
        Array.Fill(rowMaxX, float.NegativeInfinity);

        for (var idx = 0; idx < lenEdges; ++idx)
        {
            ref readonly var e = ref edges[idx];
            var r0 = edgeRowStarts[idx];
            var r1 = edgeRowEnds[idx];

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

                var w = r == r0
                    ? (r == r1 ? singleWpos[r]++ : newWpos[r]++)
                    : (r == r1 ? endingWpos[r]++ : carryWpos[r]++);
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
            var endPad = rowOffsets[r + 1];
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

        // horizontals: fill row-contiguous storage + update row bounds
        Span<int> hwpos = stackalloc int[rows];
        hRowOffsets.AsSpan(0, rows).CopyTo(hwpos);
        for (int idx = 0, hN = lenH; idx < hN; ++idx)
        {
            ref readonly var hEdge = ref hEdges[idx];
            var r = (int)((hEdge.y - bbMinY) * invCellH);
            if (r < 0)
            {
                r = 0;
            }
            else if (r >= rows)
            {
                r = rows - 1;
            }
            hEdgesByRow[hwpos[r]++] = hEdge;

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

        // Point and horizontal-ray queries only need edges at one Y. Keep each row slice ordered so dense horizontal rows can be narrowed by binary search.
        for (var r = 0; r < rows; ++r)
        {
            var start = hRowOffsets[r];
            var count = hRowOffsets[r + 1] - start;
            if (count > 1)
            {
                Array.Sort(hEdgesByRow, start, count, HorizontalComparer);
            }
        }

        return new PolygonBoundaryIndex2D(y0Ptr, y1Ptr, x0Ptr, kPtr, bPtr, minXPtr, maxXPtr, dxPtr, dyPtr, invL2Ptr, total,
            rowOffsets, rowEndingStarts, rowNewStarts, rowSingleStarts, rowEnds, hEdgesByRow, hRowOffsets, rows, lenEdges + lenH, bbMinY, cellH, invCellH,
            bbMinX, bbMinY, bbMaxX, bbMaxY, rowMinX, rowMaxX, [.. contourSamples], rawBlock);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int RoundUp(int v, int m) => (v + m - 1) & -m;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ProcessExteriorContour(ReadOnlySpan<WDir> contour, List<E> eList, List<H> hList,
            ref float bbMinX, ref float bbMinY, ref float bbMaxX, ref float bbMaxY)
        {
            var count = contour.Length;
            if (count == 0)
            {
                return;
            }

            if (count == 1)
            {
                var point = contour[0];
                var pointX = point.X;
                var pointZ = point.Z;
                bbMinX = Math.Min(bbMinX, pointX);
                bbMaxX = Math.Max(bbMaxX, pointX);
                bbMinY = Math.Min(bbMinY, pointZ);
                bbMaxY = Math.Max(bbMaxY, pointZ);
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

                if (bx < bbMinX)
                {
                    bbMinX = bx;
                }
                if (bx > bbMaxX)
                {
                    bbMaxX = bx;
                }
                if (by < bbMinY)
                {
                    bbMinY = by;
                }
                if (by > bbMaxY)
                {
                    bbMaxY = by;
                }

                if (MathF.Abs(ay - by) <= Eps)
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

                if (MathF.Abs(ay - by) <= Eps)
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
        var r = (int)((y - _minY) * _invCellH);
        return r < 0 ? 0 : r >= _rows ? _rows - 1 : r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FirstHorizontalAtOrAbove(int start, int end, float y)
    {
        // Binary search only when it can amortize its branchy setup
        if (end - start < 8)
        {
            return start;
        }

        while (start < end)
        {
            var mid = (int)(((uint)start + (uint)end) >> 1);
            if (_hEdges[mid].y < y)
            {
                start = mid + 1;
            }
            else
            {
                end = mid;
            }
        }
        return start;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FirstHorizontalAbove(int start, int end, float y)
    {
        if (end - start < 8)
        {
            return end;
        }

        while (start < end)
        {
            var mid = (int)(((uint)start + (uint)end) >> 1);
            if (_hEdges[mid].y <= y)
            {
                start = mid + 1;
            }
            else
            {
                end = mid;
            }
        }
        return start;
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
        if (px < _rowMinX[row] - Eps || px > _rowMaxX[row] + Eps)
        {
            return false;
        }

        int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
        var minHorizontalY = py - Eps;
        var maxHorizontalY = py + Eps;
        hs = FirstHorizontalAtOrAbove(hs, he, minHorizontalY);
        for (var i = hs; i < he; ++i)
        {
            ref readonly var h = ref _hEdges[i];
            var hy = h.y;
            if (hy > maxHorizontalY)
            {
                break;
            }
            if (hy >= minHorizontalY && px >= h.minX - Eps && px <= h.maxX + Eps)
            {
                return true;
            }
        }

        int es = _rowOffsets[row], ee = _rowOffsets[row + 1];
        if (ee - es == 0)
        {
            return false;
        }

        var parity = 0;

        if (Avx512F.IsSupported)
        {
            var v_py = Vector512.Create(py);
            var v_px = Vector512.Create(px);
            var v_eps = Vector512.Create(Eps);

            var i0 = es;
            for (; i0 + 16 <= ee; i0 += 16)
            {
                parity ^= ContainsBlock512(i0, v_py, v_px, v_eps);
                if ((parity & 2) != 0)
                {
                    return true;
                }
                parity &= 1;
            }
            return (parity & 1) != 0;
        }
        else if (Avx2.IsSupported)
        {
            var v_py = Vector256.Create(py);
            var v_px = Vector256.Create(px);
            var eps = Vector256.Create(Eps);

            var i0 = es;
            for (; i0 + 8 <= ee; i0 += 8)
            {
                parity ^= ContainsBlock256(i0, v_py, v_px, eps);
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
                if (Math.Abs(dx) <= Eps)
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
            if (Avx512F.IsSupported)
            {
                KernelHorizontalRay512(es, ee, ox, oz, invDx, t0, tmax, ref best);
            }
            else if (Avx2.IsSupported)
            {
                KernelHorizontalRay256(es, ee, ox, oz, invDx, t0, tmax, ref best);
            }
            else
            {
                KernelHorizontalRayScalar(es, _rowEnds[row], ox, oz, invDx, t0, tmax, ref best);
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            var minHorizontalY = oz - Eps;
            var maxHorizontalY = oz + Eps;
            hs = FirstHorizontalAtOrAbove(hs, he, minHorizontalY);
            for (var k = hs; k < he; ++k)
            {
                ref readonly var h = ref _hEdges[k];
                if (h.y > maxHorizontalY)
                {
                    break;
                }
                if (h.y < minHorizontalY)
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

        // Set up a DDA in parameter space. Row boundaries are uniformly spaced, so avoid recomputing y-to-t and both endpoint x values in every row
        var rowEdge = (int)((yStart - _minY) * _invCellH);
        if (rowEdge < 0)
        {
            rowEdge = 0;
        }
        else if (rowEdge >= _rows)
        {
            rowEdge = _rows - 1;
        }
        var nextY = dz > 0f ? (_minY + (rowEdge + 1) * cellH) : (_minY + rowEdge * cellH);
        var xEnter = ox + dx * t0m;

        var bestT = float.MaxValue;
        var hit = false;

        // Once a row has passed the X-hull cull and all of its active edges have been tested, subsequent contiguous rows only need edges first encountered
        // in the traversal direction. Reset after a culled row: an edge may have started inside that gap and must be picked up by the next full active-set scan
        var activeSetComplete = false;
        var rowTMin = t0m;
        while ((uint)rowCur < (uint)_rows)
        {
            var tBoundary = (nextY - oz) * invDz;
            var xExit = ox + dx * tBoundary;
            var rowTMax = Math.Min(tmax, bestT);

            // row-level x-range cull
            var rxMin = Math.Min(xEnter, xExit) - 2e-6f; // expand a touch to be conservative
            var rxMax = Math.Max(xEnter, xExit) + 2e-6f;

            // row polygon x-extent
            var pxMin = _rowMinX[rowCur];
            var pxMax = _rowMaxX[rowCur];

            // if disjoint, skip this row entirely
            if (!(rxMax < pxMin || rxMin > pxMax))
            {
                var prevBest = bestT;
                if (!activeSetComplete)
                {
                    KernelRayDispatch(_rowOffsets[rowCur], _rowOffsets[rowCur + 1], ox, oz, dx, dz, t0m, rowTMax, ref bestT);
                }
                else if (step > 0)
                {
                    KernelRayDispatch(_rowNewStarts[rowCur], _rowEnds[rowCur], ox, oz, dx, dz, t0m, rowTMax, ref bestT);
                }
                else
                {
                    KernelRayDispatch(_rowEndingStarts[rowCur], _rowNewStarts[rowCur], ox, oz, dx, dz, t0m, rowTMax, ref bestT);
                    KernelRayDispatch(_rowSingleStarts[rowCur], _rowEnds[rowCur], ox, oz, dx, dz, t0m, Math.Min(rowTMax, bestT), ref bestT);
                }
                activeSetComplete = true;

                if (bestT < prevBest)
                {
                    hit = true;
                    rowTMax = Math.Min(rowTMax, bestT);
                }

                // Horizontals are sorted by Y inside each row. Narrow the scan to the ray's active parameter interval instead of walking the whole row
                int hs = _hRowOffsets[rowCur], he = _hRowOffsets[rowCur + 1];
                var yAtLimit = oz + dz * rowTMax;
                var horizontalMinY = Math.Min(oz + dz * rowTMin, yAtLimit) - Eps;
                var horizontalMaxY = Math.Max(oz + dz * rowTMin, yAtLimit) + Eps;
                hs = FirstHorizontalAtOrAbove(hs, he, horizontalMinY);
                for (var k = hs; k < he; ++k)
                {
                    ref readonly var h = ref _hEdges[k];
                    if (h.y > horizontalMaxY)
                    {
                        break;
                    }

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
            else
            {
                activeSetComplete = false;
            }

            // stop after row that reaches AABB exit
            if (tBoundary >= tmax - 1e-6f)
            {
                break;
            }

            // advance
            rowCur += step;
            rowTMin = tBoundary;
            xEnter = xExit;
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

        void ProcessEdges(int es, int ee) => KernelClosestDispatch(es, ee, px, py, ref bestSq, ref bestX, ref bestY);

        // direction: 0 = initial row, +1 = ascending, -1 = descending
        // After the initial row, only edges first encountered in that direction are evaluated; long edges are not projected once per copied row
        void ProcessRow(int row, float vDistSq, int direction)
        {
            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            var rownewstart = _rowNewStarts[row];
            var rowend = _rowEnds[row];

            var hasEdges = direction switch
            {
                > 0 => rownewstart < rowend,
                < 0 => _rowEndingStarts[row] < rownewstart || _rowSingleStarts[row] < rowend,
                _ => _rowOffsets[row] < rowend
            };
            if (!hasEdges && hs == he)
            {
                return;
            }

            var rMinX = _rowMinX[row];
            var rMaxX = _rowMaxX[row];
            var hDist = Math.Max(0f, Math.Max(rMinX - px, px - rMaxX));
            if (vDistSq + hDist * hDist >= bestSq)
            {
                return;
            }

            if (direction > 0)
            {
                ProcessEdges(rownewstart, rowend);
            }
            else if (direction < 0)
            {
                ProcessEdges(_rowEndingStarts[row], rownewstart);
                if (bestSq == 0f)
                {
                    return;
                }
                ProcessEdges(_rowSingleStarts[row], rowend);
            }
            else
            {
                // Preserve the padded, aligned initial-row fast path
                ProcessEdges(_rowOffsets[row], _rowOffsets[row + 1]);
            }

            if (bestSq == 0f)
            {
                return;
            }

            // Horizontals are Y-sorted. Search outward from py so the vertical-distance lower bound tightens quickly, without repeatedly taking square roots as bestSq improves
            if (he - hs >= 8)
            {
                var above = FirstHorizontalAtOrAbove(hs, he, py);
                var below = above - 1;
                while (below >= hs || above < he)
                {
                    var belowDY = below >= hs ? py - _hEdges[below].y : float.PositiveInfinity;
                    var aboveDY = above < he ? _hEdges[above].y - py : float.PositiveInfinity;

                    int h;
                    float dyAbs;
                    if (belowDY <= aboveDY)
                    {
                        h = below--;
                        dyAbs = belowDY;
                    }
                    else
                    {
                        h = above++;
                        dyAbs = aboveDY;
                    }
                    var dySq = dyAbs * dyAbs;
                    if (dySq >= bestSq)
                    {
                        // the selected side is the nearer remaining side; all later horizontals are farther in Y
                        break;
                    }

                    ref readonly var edge = ref _hEdges[h];
                    var cx = Math.Min(Math.Max(px, edge.minX), edge.maxX);
                    var dxp = cx - px;
                    var d2 = dxp * dxp + dySq;
                    if (d2 < bestSq)
                    {
                        bestSq = d2;
                        bestX = cx;
                        bestY = edge.y;
                        if (d2 == 0f)
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                for (var h = hs; h < he; ++h)
                {
                    ref readonly var edge = ref _hEdges[h];
                    var dyp = edge.y - py;
                    var dySq = dyp * dyp;
                    if (dySq >= bestSq)
                    {
                        continue;
                    }
                    var cx = Math.Min(Math.Max(px, edge.minX), edge.maxX);
                    var dxp = cx - px;
                    var d2 = dxp * dxp + dySq;
                    if (d2 < bestSq)
                    {
                        bestSq = d2;
                        bestX = cx;
                        bestY = edge.y;
                        if (d2 == 0f)
                        {
                            return;
                        }
                    }
                }
            }
        }

        var minY0 = _minY;
        if ((uint)row0 < (uint)_rows)
        {
            var minY = minY0 + row0 * cellH;
            var maxY = minY + cellH;
            ProcessRow(row0, VDistSq(py, minY, maxY), 0);
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
                    ProcessRow(rn, vDistSq, -1);
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
                    ProcessRow(rp, vDistSq, +1);
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
        var (angs, countA) = CollectUniqueAngles(origin, polygon);
        if (countA == 0)
        {
            return [];
        }

        // At most two hits are emitted for every angular interval
        var res = new (WDir pt, float t)[countA * 2];
        var countR = 0;
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
                    res[countR++] = (tL >= tR) ? (pL, tL) : (pR, tR);
                }
                else
                {
                    // Keep both (in CCW order)
                    res[countR++] = (pL, tL);
                    res[countR++] = (pR, tR);
                }
            }
            else if (gotL)
            {
                res[countR++] = (pL, tL);
            }
            else if (gotR)
            {
                res[countR++] = (pR, tR);
            }
            else
            {
                // Midpoint fallback
                var mid = a0 + 0.5d * d;
                if (RayAt(origin, mid, out var pM, out var tM))
                {
                    res[countR++] = (pM, tM);
                }
            }
        }

        if (countR == 0)
        {
            return [];
        }

        // fan-wide close-pair merge: keep the farther (bigger fan for safety)
        MergeConsecutivePreferOuter(res, ref countR);

        // close loop if endpoints coincide (prefer farther across seam)
        var first = res[0];
        var lastIndex = countR - 1;
        var last = res[lastIndex];
        if ((first.pt - last.pt).LengthSq() <= sqMerge)
        {
            res[lastIndex] = (first.t >= last.t) ? first : last;
        }

        // remove near-collinear points
        SimplifyCollinear(res, ref countR);

        // materialize
        var outPts = new WDir[countR];
        for (var i = 0; i < countR; ++i)
        {
            outPts[i] = res[i].pt;
        }
        return outPts;
    }

    public void AddForbiddenDirections(in WDir centerOffset, Angle offset, AIHints hints, DateTime act, float forbiddenDist)
    {
        var radius = Math.Abs(forbiddenDist + Eps);
        var searchRadius = Math.Max(radius, 1e-5f);
        var originX = centerOffset.X;
        var originY = centerOffset.Z;
        var minX = originX - searchRadius;
        var maxX = originX + searchRadius;
        var minY = originY - searchRadius;
        var maxY = originY + searchRadius;

        if (!TryGetRowRange(minY, maxY, out var row0, out var row1) || maxX < _bbMinX || minX > _bbMaxX)
        {
            return;
        }

        var initialCapacity = Math.Clamp(_sourceEdgeCount >> 2, 8, 256);
        var blocked = new List<(double start, double end)>(initialCapacity);
        var activeSetComplete = false;

        for (var row = row0; row <= row1; ++row)
        {
            if (maxX < _rowMinX[row] || minX > _rowMaxX[row])
            {
                // An edge can start while rows are skipped, so the next accepted row must scan its full active set
                activeSetComplete = false;
                continue;
            }

            int es = activeSetComplete ? _rowNewStarts[row] : _rowOffsets[row], ee = _rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var y0 = _y0[i];
                if (_maxX[i] < minX || _minX[i] > maxX || _y1[i] < minY || y0 > maxY)
                {
                    continue;
                }

                var ax = _x0[i] - originX;
                var ay = y0 - originY;
                if (CollectForbiddenDirectionInterval(ax, ay, ax + _dx[i], ay + _dy[i], radius, blocked))
                {
                    // the origin lies on the boundary
                    return;
                }
            }
            activeSetComplete = true;

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            hs = FirstHorizontalAtOrAbove(hs, he, minY);
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
                var edgeY = edge.y;
                if (edgeY > maxY)
                {
                    break;
                }
                var edgeMaxX = edge.maxX;
                var edgeMinX = edge.minX;
                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }

                if (CollectForbiddenDirectionInterval(edgeMinX - originX, edgeY - originY, edgeMaxX - originX, edgeY - originY, radius, blocked))
                {
                    return;
                }
            }
        }

        if (blocked.Count == 0)
        {
            return;
        }

        MergeAngleIntervals(blocked);
        MergeCircularAngleSeam(blocked);
        var countB = blocked.Count;
        for (var i = 0; i < countB; ++i)
        {
            var interval = blocked[i];
            var iStart = interval.start;
            var width = interval.end - iStart;
            if (width <= 1e-9)
            {
                continue;
            }

            var centerMath = iStart + 0.5d * width;
            var (sin, cos) = Math.SinCos(centerMath);
            var centerDir = new WDir((float)cos, (float)sin);

            hints.ForbiddenDirections.Add((Angle.FromDirection(centerDir) + offset, new(0.5f * (float)Math.Min(width, Math.Tau)), act));
        }
    }

    // Returns true when the segment passes through the ray origin, in which case every direction intersects at t ~= 0
    private static bool CollectForbiddenDirectionInterval(float ax, float ay, float bx, float by, float radius, List<(double start, double end)> blocked)
    {
        const double originTolerance = 1e-5d;
        const double angularTolerance = 1e-12d;

        var dax = (double)ax;
        var day = (double)ay;
        var dbx = (double)bx;
        var dby = (double)by;
        var sdx = dbx - dax;
        var sdy = dby - day;
        var segmentLenSq = sdx * sdx + sdy * sdy;

        double distanceSq;
        if (segmentLenSq <= 1e-24d)
        {
            distanceSq = dax * dax + day * day;
        }
        else
        {
            var t = Math.Clamp(-(dax * sdx + day * sdy) / segmentLenSq, 0d, 1d);
            var closestX = dax + t * sdx;
            var closestY = day + t * sdy;
            distanceSq = closestX * closestX + closestY * closestY;
        }

        if (distanceSq <= originTolerance * originTolerance)
        {
            return true;
        }

        if (!ClipSegmentToDisk(dax, day, dbx, dby, radius, out var x0, out var y0, out var x1, out var y1))
        {
            return false;
        }

        // Along a segment that does not contain the origin, polar angle is monotonic and its extrema
        // are the clipped endpoints. There are two circular arcs between those endpoint angles; choose
        // the one containing an interior point of the clipped segment. This remains stable when the
        // endpoint directions are almost opposite, where cross-product sign alone can flip.
        var a0 = NormalizeAngle(Math.Atan2(y0, x0));
        var a1 = NormalizeAngle(Math.Atan2(y1, x1));
        var ccwWidth = AngleDiffCCW(a0, a1);
        if (ccwWidth <= angularTolerance || Math.Tau - ccwWidth <= angularTolerance)
        {
            return false; // radial/tangent point: zero-measure direction range

        }

        // Use the farther quarter-point rather than the midpoint: for a segment passing close to the
        // origin, the midpoint can be almost zero and therefore have a numerically unstable angle.
        var q0x = 0.75d * x0 + 0.25d * x1;
        var q0y = 0.75d * y0 + 0.25d * y1;
        var q1x = 0.25d * x0 + 0.75d * x1;
        var q1y = 0.25d * y0 + 0.75d * y1;
        var q0LenSq = q0x * q0x + q0y * q0y;
        var q1LenSq = q1x * q1x + q1y * q1y;
        var sampleAngle = q0LenSq >= q1LenSq ? Math.Atan2(q0y, q0x) : Math.Atan2(q1y, q1x);
        sampleAngle = NormalizeAngle(sampleAngle);

        var sampleFromA0 = AngleDiffCCW(a0, sampleAngle);
        if (sampleFromA0 <= ccwWidth + angularTolerance)
        {
            AddCircularInterval(blocked, a0, ccwWidth);
        }
        else
        {
            AddCircularInterval(blocked, a1, Math.Tau - ccwWidth);
        }
        return false;
    }

    private static bool ClipSegmentToDisk(double ax, double ay, double bx, double by, double radius, out double x0, out double y0, out double x1, out double y1)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var qa = dx * dx + dy * dy;
        var radiusSq = radius * radius;

        if (qa <= 1e-24)
        {
            if (ax * ax + ay * ay > radiusSq)
            {
                x0 = y0 = x1 = y1 = 0d;
                return false;
            }
            x0 = x1 = ax;
            y0 = y1 = ay;
            return true;
        }

        var qb = 2d * (ax * dx + ay * dy);
        var qc = ax * ax + ay * ay - radiusSq;
        var discriminant = qb * qb - 4d * qa * qc;
        if (discriminant < 0d)
        {
            x0 = y0 = x1 = y1 = 0d;
            return false;
        }

        var sqrtDiscriminant = Math.Sqrt(Math.Max(0d, discriminant));
        var inv2A = 0.5d / qa;
        var root0 = (-qb - sqrtDiscriminant) * inv2A;
        var root1 = (-qb + sqrtDiscriminant) * inv2A;
        var t0 = Math.Max(0d, Math.Min(root0, root1));
        var t1 = Math.Min(1d, Math.Max(root0, root1));
        if (t1 < t0)
        {
            x0 = y0 = x1 = y1 = 0d;
            return false;
        }

        x0 = ax + t0 * dx;
        y0 = ay + t0 * dy;
        x1 = ax + t1 * dx;
        y1 = ay + t1 * dy;
        return true;
    }

    // Add a CCW interval represented by start + non-negative width, splitting it at the 0/2pi seam.
    private static void AddCircularInterval(List<(double start, double end)> intervals, double start, double width)
    {
        if (width <= 1e-12)
        {
            return;
        }
        if (width >= Math.Tau - 1e-12)
        {
            intervals.Clear();
            intervals.Add((0d, Math.Tau));
            return;
        }

        start = NormalizeAngle(start);
        var end = start + width;
        if (end <= Math.Tau)
        {
            intervals.Add((start, end));
        }
        else
        {
            intervals.Add((start, Math.Tau));
            intervals.Add((0d, end - Math.Tau));
        }
    }

    private static void MergeCircularAngleSeam(List<(double start, double end)> intervals)
    {
        var count = intervals.Count;
        if (count < 2)
        {
            return;
        }

        const double mergeTolerance = 1e-7d;
        var first = intervals[0];
        var last = intervals[count - 1];
        if (first.start <= mergeTolerance && last.end >= Math.Tau - mergeTolerance)
        {
            // Keep a single unwrapped interval crossing the seam; center conversion uses sin/cos, so >2pi is fine
            intervals[0] = (last.start, first.end + Math.Tau);
            intervals.RemoveAt(count - 1);
        }
    }

    public static void MergeAngleIntervals(List<(double start, double end)> intervals)
    {
        var count = intervals.Count;
        if (count <= 1)
        {
            return;
        }

        intervals.Sort(static (x, y) => x.start.CompareTo(y.start));

        var write = 0;
        var cur = intervals[0];
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NormalizeAngle(double a)
    {
        if (a is >= Math.Tau or < 0d)
        {
            a %= Math.Tau;
            if (a < 0d)
            {
                a += Math.Tau;
            }
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

    private static (double[] angles, int count) CollectUniqueAngles(in WDir origin, RelSimplifiedComplexPolygon polygon)
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
        if (vertsCount == 0)
        {
            return ([], 0);
        }

        var angles = new double[vertsCount];
        var countA = 0;
        for (int i = 0, n = countP; i < n; ++i)
        {
            var vs = parts[i].Vertices;
            var countVS = vs.Count;
            for (int j = 0, m = countVS; j < m; ++j)
            {
                var v = vs[j];
                var a = Math.Atan2(v.Z - oz, v.X - ox);
                angles[countA++] = a < 0d ? a + Math.Tau : a;
            }
        }

        Array.Sort(angles, 0, countA);
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
        return (angles, write);
    }

    private static void MergeConsecutivePreferOuter((WDir pt, float t)[] pts, ref int countP)
    {
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

        countP = write;
    }

    private static void SimplifyCollinear((WDir pt, float t)[] pts, ref int countP)
    {
        if (countP <= 2)
        {
            return;
        }

        const float eps = 1e-6f;
        var first = pts[0].pt;
        var a = pts[countP - 1].pt;
        var b = first;
        var write = 0;

        for (var i = 0; i < countP; ++i)
        {
            var current = pts[i];
            var c = i + 1 < countP ? pts[i + 1].pt : first;
            var abx = b.X - a.X;
            var abz = b.Z - a.Z;
            var bcx = c.X - b.X;
            var bcz = c.Z - b.Z;
            var cross = Math.Abs(abx * bcz - abz * bcx);
            var scale = Math.Abs(abx) + Math.Abs(abz) + Math.Abs(bcx) + Math.Abs(bcz);
            if (cross > eps * scale)
            {
                pts[write++] = current;
            }

            a = b;
            b = c;
        }

        countP = write;
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
    private static float HorizontalMin(Vector256<float> value)
    {
        var min = Sse.Min(value.GetLower(), value.GetUpper());
        min = Sse.Min(min, Sse.Shuffle(min, min, 0b_01_00_11_10));
        min = Sse.Min(min, Sse.Shuffle(min, min, 0b_10_11_00_01));
        return min.ToScalar();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float HorizontalMin(Vector512<float> value) => HorizontalMin(Avx.Min(value.GetLower(), value.GetUpper()));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateClosest(Vector256<float> d2, Vector256<float> nx, Vector256<float> ny, Vector256<float> improving,
        ref float bestSq, ref float bestX, ref float bestY)
    {
        var candidates = Vector256.ConditionalSelect(improving, d2, Vector256.Create(float.PositiveInfinity));
        var blockBest = HorizontalMin(candidates);
        if (blockBest >= bestSq)
        {
            return;
        }

        var laneMask = (Vector256.Equals(d2, Vector256.Create(blockBest)) & improving).ExtractMostSignificantBits();
        var lane = BitOperations.TrailingZeroCount(laneMask);
        bestSq = blockBest;
        bestX = nx.GetElement(lane);
        bestY = ny.GetElement(lane);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateClosest(Vector512<float> d2, Vector512<float> nx, Vector512<float> ny, Vector512<float> improving,
        ref float bestSq, ref float bestX, ref float bestY)
    {
        var candidates = Vector512.ConditionalSelect(improving, d2, Vector512.Create(float.PositiveInfinity));
        var blockBest = HorizontalMin(candidates);
        if (blockBest >= bestSq)
        {
            return;
        }

        var laneMask = (Vector512.Equals(d2, Vector512.Create(blockBest)) & improving).ExtractMostSignificantBits();
        var lane = BitOperations.TrailingZeroCount(laneMask);
        bestSq = blockBest;
        bestX = nx.GetElement(lane);
        bestY = ny.GetElement(lane);
    }

    // SIMD kernels
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ContainsBlock512(int idx, Vector512<float> v_py, Vector512<float> v_px, Vector512<float> v_eps)
    {
        var y0 = Load512(_y0, idx);
        var y1 = Load512(_y1, idx);
        var k = Load512(_k, idx);
        var b = Load512(_b, idx);

        var span = Vector512.GreaterThanOrEqual(v_py, y0) & Vector512.LessThan(v_py, y1);

        var x = Avx512F.FusedMultiplyAdd(k, v_py, b);

        var dx = Avx512F.Subtract(v_px, x);
        var near = Vector512.LessThanOrEqual(Vector512.Abs(dx), v_eps) & span;

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
    private int ContainsBlock256(int idx, Vector256<float> v_py, Vector256<float> v_px, Vector256<float> v_eps)
    {
        var y0 = Load256(_y0, idx);
        var y1 = Load256(_y1, idx);
        var k = Load256(_k, idx);
        var b = Load256(_b, idx);

        var span = Vector256.BitwiseAnd(Vector256.GreaterThanOrEqual(v_py, y0), Vector256.LessThan(v_py, y1));

        var x = Fma.IsSupported ? Fma.MultiplyAdd(k, v_py, b) : Avx.Add(Avx.Multiply(k, v_py), b); // k*py + b

        var dx = Avx.Subtract(v_px, x);
        var near = Vector256.BitwiseAnd(Vector256.LessThanOrEqual(Vector256.Abs(dx), v_eps), span);

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
    private void KernelHorizontalRay512(int es, int ee, float ox, float oz, float invDx, float tMin, float tMax, ref float best)
    {
        var v_ox = Vector512.Create(ox);
        var v_oz = Vector512.Create(oz);
        var v_invDx = Vector512.Create(invDx);
        var v_eps = Vector512.Create(Eps);
        var v_tMin = Vector512.Create(tMin);
        var v_tMax = Vector512.Create(Math.Min(tMax, best));
        var v_inf = Vector512.Create(float.PositiveInfinity);

        for (var i = es; i + 16 <= ee; i += 16)
        {
            var y0 = Load512(_y0, i);
            var y1 = Load512(_y1, i);
            var span = Vector512.GreaterThanOrEqual(v_oz, y0 - v_eps)
                & Vector512.LessThan(v_oz, y1 - v_eps);

            var x = Avx512F.FusedMultiplyAdd(Load512(_k, i), v_oz, Load512(_b, i));
            var t = Avx512F.Multiply(Avx512F.Subtract(x, v_ox), v_invDx);
            var candidate = span
                & Vector512.GreaterThanOrEqual(t, v_tMin)
                & Vector512.LessThanOrEqual(t, v_tMax);
            if (candidate.ExtractMostSignificantBits() == 0ul)
            {
                continue;
            }

            var valid = candidate
                & Vector512.GreaterThanOrEqual(x, Load512(_minX, i) - v_eps)
                & Vector512.LessThanOrEqual(x, Load512(_maxX, i) + v_eps);
            if (valid.ExtractMostSignificantBits() != 0ul)
            {
                var blockBest = HorizontalMin(Vector512.ConditionalSelect(valid, t, v_inf));
                if (blockBest < best)
                {
                    best = blockBest;
                    v_tMax = Vector512.Create(blockBest);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelHorizontalRay256(int es, int ee, float ox, float oz, float invDx, float tMin, float tMax, ref float best)
    {
        var v_ox = Vector256.Create(ox);
        var v_oz = Vector256.Create(oz);
        var v_invDx = Vector256.Create(invDx);
        var v_eps = Vector256.Create(Eps);
        var v_tMin = Vector256.Create(tMin);
        var v_tMax = Vector256.Create(Math.Min(tMax, best));
        var v_inf = Vector256.Create(float.PositiveInfinity);

        for (var i = es; i + 8 <= ee; i += 8)
        {
            var y0 = Load256(_y0, i);
            var y1 = Load256(_y1, i);
            var span = Vector256.GreaterThanOrEqual(v_oz, y0 - v_eps)
                & Vector256.LessThan(v_oz, y1 - v_eps);

            var k = Load256(_k, i);
            var b = Load256(_b, i);
            var x = Fma.IsSupported ? Fma.MultiplyAdd(k, v_oz, b) : Avx.Add(Avx.Multiply(k, v_oz), b);
            var t = Avx.Multiply(Avx.Subtract(x, v_ox), v_invDx);
            var candidate = span
                & Vector256.GreaterThanOrEqual(t, v_tMin)
                & Vector256.LessThanOrEqual(t, v_tMax);
            if (candidate.ExtractMostSignificantBits() == 0u)
            {
                continue;
            }

            var valid = candidate
                & Vector256.GreaterThanOrEqual(x, Avx.Subtract(Load256(_minX, i), v_eps))
                & Vector256.LessThanOrEqual(x, Avx.Add(Load256(_maxX, i), v_eps));
            if (valid.ExtractMostSignificantBits() != 0u)
            {
                var blockBest = HorizontalMin(Vector256.ConditionalSelect(valid, t, v_inf));
                if (blockBest < best)
                {
                    best = blockBest;
                    v_tMax = Vector256.Create(blockBest);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelHorizontalRayScalar(int es, int ee, float ox, float oz, float invDx, float tMin, float tMax, ref float best)
    {
        for (var i = es; i < ee; ++i)
        {
            var y0 = _y0[i];
            if (oz < y0 - Eps || oz >= _y1[i] - Eps)
            {
                continue;
            }

            var x = _k[i] * oz + _b[i];
            var t = (x - ox) * invDx;
            if (t >= tMin && t <= tMax && t < best && x >= _minX[i] - Eps && x <= _maxX[i] + Eps)
            {
                best = t;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelRayDispatch(int es, int ee, float ox, float oz, float dx, float dz, float tMin, float tMax, ref float best)
    {
        if (es >= ee || tMin > tMax || best <= tMin)
        {
            return;
        }

        var count = ee - es;
        if (count >= 16 && Avx512F.IsSupported)
        {
            KernelRay512(es, ee, ox, oz, dx, dz, tMin, tMax, ref best);
        }
        else if (count >= 8 && Avx2.IsSupported)
        {
            KernelRay256(es, ee, ox, oz, dx, dz, tMin, tMax, ref best);
        }
        else
        {
            KernelRayScalar(es, ee, ox, oz, dx, dz, tMin, tMax, ref best);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelRay512(int es, int ee, float ox, float oz, float dx, float dz, float tMin, float tMax, ref float best)
    {
        var v_dx = Vector512.Create(dx);
        var v_dz = Vector512.Create(dz);
        var v_ox = Vector512.Create(ox);
        var v_oz = Vector512.Create(oz);
        var v_tiny = Vector512.Create(TinyDen);
        var v_eps = Vector512.Create(Eps);
        var v_invDirLenSq = Vector512.Create(1f / (dx * dx + dz * dz + 1e-20f));
        var v_zero = Vector512<float>.Zero;
        var v_tMin = Vector512.Create(tMin);
        var v_tMax = Vector512.Create(Math.Min(tMax, best));
        var v_inf = Vector512.Create(float.PositiveInfinity);

        for (var i = es; i + 16 <= ee; i += 16)
        {
            var y0 = Load512(_y0, i);
            var dy = Load512(_dy, i);
            var x0 = Load512(_x0, i);
            var dxE = Load512(_dx, i);

            var wox = Avx512F.Subtract(x0, v_ox);
            var woz = Avx512F.Subtract(y0, v_oz);
            var den = Avx512F.FusedMultiplySubtract(v_dx, dy, Avx512F.Multiply(v_dz, dxE));
            var absDen = Vector512.Abs(den);
            var validDen = Vector512.GreaterThan(absDen, v_tiny);
            var denNegative = Vector512.LessThan(den, v_zero);
            var blockBest = float.PositiveInfinity;

            // Non-parallel segment intersections. Test numerator ranges before issuing the expensive vector divide
            var rawTNum = Avx512F.FusedMultiplySubtract(wox, dy, Avx512F.Multiply(woz, dxE));
            var tNum = Vector512.ConditionalSelect(denNegative, Avx512F.Subtract(v_zero, rawTNum), rawTNum);
            var tCandidate = validDen
                & Vector512.GreaterThanOrEqual(tNum, Avx512F.Multiply(v_tMin, absDen))
                & Vector512.LessThanOrEqual(tNum, Avx512F.Multiply(v_tMax, absDen));
            if (tCandidate.ExtractMostSignificantBits() != 0ul)
            {
                var rawUNum = Avx512F.FusedMultiplySubtract(wox, v_dz, Avx512F.Multiply(woz, v_dx));
                var uNum = Vector512.ConditionalSelect(denNegative, Avx512F.Subtract(v_zero, rawUNum), rawUNum);
                var valid = tCandidate
                    & Vector512.GreaterThanOrEqual(uNum, v_zero)
                    & Vector512.LessThan(uNum, absDen);
                if (valid.ExtractMostSignificantBits() != 0ul)
                {
                    var t = Avx512F.Divide(tNum, absDen);
                    blockBest = HorizontalMin(Vector512.ConditionalSelect(valid, t, v_inf));
                }
            }

            // Parallel lanes used to be discarded by SIMD. Handle collinear overlap directly and return the first parameter in the overlap
            var parallel = Vector512.LessThanOrEqual(absDen, v_tiny);
            if (parallel.ExtractMostSignificantBits() != 0ul)
            {
                var col = Avx512F.FusedMultiplySubtract(wox, v_dz, Avx512F.Multiply(woz, v_dx));
                var collinear = parallel & Vector512.LessThanOrEqual(Vector512.Abs(col), v_eps);
                if (collinear.ExtractMostSignificantBits() != 0ul)
                {
                    var tA = Avx512F.Multiply(Avx512F.FusedMultiplyAdd(wox, v_dx, Avx512F.Multiply(woz, v_dz)), v_invDirLenSq);
                    var wbx = Avx512F.Add(wox, dxE);
                    var wbz = Avx512F.Add(woz, dy);
                    var tB = Avx512F.Multiply(Avx512F.FusedMultiplyAdd(wbx, v_dx, Avx512F.Multiply(wbz, v_dz)), v_invDirLenSq);
                    var tLo = Avx512F.Min(tA, tB);
                    var tHi = Avx512F.Max(tA, tB);
                    var tOverlap = Avx512F.Max(tLo, v_tMin);
                    var validOverlap = collinear
                        & Vector512.LessThanOrEqual(tOverlap, tHi)
                        & Vector512.LessThanOrEqual(tOverlap, v_tMax);
                    if (validOverlap.ExtractMostSignificantBits() != 0ul)
                    {
                        var parallelBest = HorizontalMin(Vector512.ConditionalSelect(validOverlap, tOverlap, v_inf));
                        blockBest = Math.Min(blockBest, parallelBest);
                    }
                }
            }

            if (blockBest < best)
            {
                best = blockBest;
                v_tMax = Vector512.Create(blockBest);
            }
        }

        var tail = es + ((ee - es) & ~15);
        if (tail + 8 <= ee && Avx2.IsSupported)
        {
            KernelRay256(tail, ee, ox, oz, dx, dz, tMin, Math.Min(tMax, best), ref best);
        }
        else if (tail < ee)
        {
            KernelRayScalar(tail, ee, ox, oz, dx, dz, tMin, Math.Min(tMax, best), ref best);
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
        var v_eps = Vector256.Create(Eps);
        var v_invDirLenSq = Vector256.Create(1f / (dx * dx + dz * dz + 1e-20f));
        var v_zero = Vector256<float>.Zero;
        var v_tMin = Vector256.Create(tMin);
        var v_tMax = Vector256.Create(Math.Min(tMax, best));
        var v_inf = Vector256.Create(float.PositiveInfinity);

        for (var i = es; i + 8 <= ee; i += 8)
        {
            var y0 = Load256(_y0, i);
            var dy = Load256(_dy, i);
            var x0 = Load256(_x0, i);
            var dxE = Load256(_dx, i);

            var wox = Avx.Subtract(x0, v_ox);
            var woz = Avx.Subtract(y0, v_oz);
            var den = Avx.Subtract(Avx.Multiply(v_dx, dy), Avx.Multiply(v_dz, dxE));
            var absDen = Vector256.Abs(den);
            var validDen = Vector256.GreaterThan(absDen, v_tiny);
            var denNegative = Vector256.LessThan(den, v_zero);
            var blockBest = float.PositiveInfinity;

            var rawTNum = Fma.IsSupported
                ? Fma.MultiplySubtract(wox, dy, Avx.Multiply(woz, dxE))
                : Avx.Subtract(Avx.Multiply(wox, dy), Avx.Multiply(woz, dxE));
            var tNum = Vector256.ConditionalSelect(denNegative, Avx.Subtract(v_zero, rawTNum), rawTNum);
            var tCandidate = validDen
                & Vector256.GreaterThanOrEqual(tNum, Avx.Multiply(v_tMin, absDen))
                & Vector256.LessThanOrEqual(tNum, Avx.Multiply(v_tMax, absDen));
            if (tCandidate.ExtractMostSignificantBits() != 0u)
            {
                var rawUNum = Fma.IsSupported
                    ? Fma.MultiplySubtract(wox, v_dz, Avx.Multiply(woz, v_dx))
                    : Avx.Subtract(Avx.Multiply(wox, v_dz), Avx.Multiply(woz, v_dx));
                var uNum = Vector256.ConditionalSelect(denNegative, Avx.Subtract(v_zero, rawUNum), rawUNum);
                var valid = tCandidate
                    & Vector256.GreaterThanOrEqual(uNum, v_zero)
                    & Vector256.LessThan(uNum, absDen);
                if (valid.ExtractMostSignificantBits() != 0u)
                {
                    var t = Avx.Divide(tNum, absDen);
                    blockBest = HorizontalMin(Vector256.ConditionalSelect(valid, t, v_inf));
                }
            }

            var parallel = Vector256.LessThanOrEqual(absDen, v_tiny);
            if (parallel.ExtractMostSignificantBits() != 0u)
            {
                var col = Fma.IsSupported ? Fma.MultiplySubtract(wox, v_dz, Avx.Multiply(woz, v_dx)) : Avx.Subtract(Avx.Multiply(wox, v_dz), Avx.Multiply(woz, v_dx));
                var collinear = parallel & Vector256.LessThanOrEqual(Vector256.Abs(col), v_eps);
                if (collinear.ExtractMostSignificantBits() != 0u)
                {
                    var dotA = Fma.IsSupported ? Fma.MultiplyAdd(wox, v_dx, Avx.Multiply(woz, v_dz)) : Avx.Add(Avx.Multiply(wox, v_dx), Avx.Multiply(woz, v_dz));
                    var wbx = Avx.Add(wox, dxE);
                    var wbz = Avx.Add(woz, dy);
                    var dotB = Fma.IsSupported
                        ? Fma.MultiplyAdd(wbx, v_dx, Avx.Multiply(wbz, v_dz))
                        : Avx.Add(Avx.Multiply(wbx, v_dx), Avx.Multiply(wbz, v_dz));
                    var tA = Avx.Multiply(dotA, v_invDirLenSq);
                    var tB = Avx.Multiply(dotB, v_invDirLenSq);
                    var tLo = Avx.Min(tA, tB);
                    var tHi = Avx.Max(tA, tB);
                    var tOverlap = Avx.Max(tLo, v_tMin);
                    var validOverlap = collinear & Vector256.LessThanOrEqual(tOverlap, tHi) & Vector256.LessThanOrEqual(tOverlap, v_tMax);
                    if (validOverlap.ExtractMostSignificantBits() != 0u)
                    {
                        var parallelBest = HorizontalMin(Vector256.ConditionalSelect(validOverlap, tOverlap, v_inf));
                        blockBest = Math.Min(blockBest, parallelBest);
                    }
                }
            }

            if (blockBest < best)
            {
                best = blockBest;
                v_tMax = Vector256.Create(blockBest);
            }
        }

        var tail = es + ((ee - es) & ~7);
        if (tail < ee)
        {
            KernelRayScalar(tail, ee, ox, oz, dx, dz, tMin, Math.Min(tMax, best), ref best);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelRayScalar(int es, int ee, float ox, float oz, float dx, float dz, float tMin, float tMax, ref float best)
    {
        var invDirLenSq = 1f / (dx * dx + dz * dz + 1e-20f);
        for (var i = es; i < ee; ++i)
        {
            float y0s = _y0[i], eys = _dy[i], x0s = _x0[i], exs = _dx[i];
            float woxs = x0s - ox, wozs = y0s - oz;
            var den = dx * eys - dz * exs;
            var absDen = Math.Abs(den);

            if (absDen > TinyDen)
            {
                var tNum = woxs * eys - wozs * exs;
                var uNum = woxs * dz - wozs * dx;
                if (den < 0f)
                {
                    tNum = -tNum;
                    uNum = -uNum;
                }

                var activeTMax = Math.Min(tMax, best);
                if (tNum < tMin * absDen || tNum > activeTMax * absDen
                    || uNum < 0f || uNum >= (1f - 1e-6f) * absDen)
                {
                    continue;
                }

                best = tNum / absDen;
            }
            else
            {
                var col = woxs * dz - wozs * dx;
                if (Math.Abs(col) <= Eps)
                {
                    var tA = (woxs * dx + wozs * dz) * invDirLenSq;
                    var tB = ((x0s + exs - ox) * dx + (y0s + eys - oz) * dz) * invDirLenSq;
                    if (tA > tB)
                    {
                        (tA, tB) = (tB, tA);
                    }

                    var cand = Math.Max(tA, tMin);
                    if (cand <= tB && cand <= tMax && cand < best)
                    {
                        best = cand;
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelClosestDispatch(int es, int ee, float px, float py, ref float bestSq, ref float bestX, ref float bestY)
    {
        var count = ee - es;
        if (count >= 16 && Avx512F.IsSupported)
        {
            KernelClosest512(es, ee, px, py, ref bestSq, ref bestX, ref bestY);
        }
        else if (count >= 8 && Avx2.IsSupported)
        {
            KernelClosest256(es, ee, px, py, ref bestSq, ref bestX, ref bestY);
        }
        else if (count > 0)
        {
            KernelClosestScalar(es, ee, px, py, ref bestSq, ref bestX, ref bestY);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelClosest512(int es, int ee, float px, float py, ref float bestSq, ref float bestX, ref float bestY)
    {
        var v_px = Vector512.Create(px);
        var v_py = Vector512.Create(py);
        var v_one = Vector512<float>.One;
        var v_zero = Vector512<float>.Zero;
        var v_bestSq = Vector512.Create(bestSq);

        var i = es;
        for (; i + 16 <= ee; i += 16)
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

            var improving = Vector512.LessThan(d2, v_bestSq);
            if (improving.ExtractMostSignificantBits() != 0ul)
            {
                UpdateClosest(d2, nx, ny, improving, ref bestSq, ref bestX, ref bestY);
                v_bestSq = Vector512.Create(bestSq);
            }
        }

        if (i + 8 <= ee && Avx2.IsSupported)
        {
            KernelClosest256(i, ee, px, py, ref bestSq, ref bestX, ref bestY);
        }
        else if (i < ee)
        {
            KernelClosestScalar(i, ee, px, py, ref bestSq, ref bestX, ref bestY);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KernelClosest256(int es, int ee, float px, float py, ref float bestSq, ref float bestX, ref float bestY)
    {
        var v_px = Vector256.Create(px);
        var v_py = Vector256.Create(py);
        var v_one = Vector256<float>.One;
        var v_zero = Vector256<float>.Zero;
        var v_bestSq = Vector256.Create(bestSq);

        var i = es;
        for (; i + 8 <= ee; i += 8)
        {
            var y0 = Load256(_y0, i);
            var dy = Load256(_dy, i);
            var x0 = Load256(_x0, i);
            var dx = Load256(_dx, i);
            var invL2 = Load256(_invLen2, i);

            var relx = Avx.Subtract(v_px, x0);
            var rely = Avx.Subtract(v_py, y0);

            // t = clamp(((relx*dx) + (rely*dy)) * invL2, 0, 1)
            var tDot = Fma.IsSupported ? Fma.MultiplyAdd(relx, dx, Avx.Multiply(rely, dy)) : Avx.Add(Avx.Multiply(relx, dx), Avx.Multiply(rely, dy));
            var t = Avx.Multiply(tDot, invL2);
            t = Avx.Min(Avx.Max(t, v_zero), v_one);

            var nx = Avx.Add(x0, Avx.Multiply(t, dx));
            var ny = Avx.Add(y0, Avx.Multiply(t, dy));

            var dxp = Avx.Subtract(nx, v_px);
            var dyp = Avx.Subtract(ny, v_py);
            var d2 = Fma.IsSupported ? Fma.MultiplyAdd(dxp, dxp, Avx.Multiply(dyp, dyp)) : Avx.Add(Avx.Multiply(dxp, dxp), Avx.Multiply(dyp, dyp));

            var improving = Vector256.LessThan(d2, v_bestSq);
            if (improving.ExtractMostSignificantBits() != 0u)
            {
                UpdateClosest(d2, nx, ny, improving, ref bestSq, ref bestX, ref bestY);
                v_bestSq = Vector256.Create(bestSq);
            }
        }

        if (i < ee)
        {
            KernelClosestScalar(i, ee, px, py, ref bestSq, ref bestX, ref bestY);
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
        var idx = polygon.VerifyPolygonIndexExistance();
        var minX = idx._bbMinX;
        var minY = idx._bbMinY;
        var maxX = idx._bbMaxX;
        var maxY = idx._bbMaxY;

        if (maxX < _bbMinX - Eps || minX > _bbMaxX + Eps || maxY < _bbMinY - Eps || minY > _bbMaxY + Eps)
        {
            return PolygonShapeRelation.Outside;
        }

        if (BoundaryIntersectsIndex(idx))
        {
            return PolygonShapeRelation.Intersecting;
        }

        var anyInside = false;
        var anyOutside = false;

        var lenP = parts.Length;
        for (var i = 0; i < lenP; ++i)
        {
            var exterior = parts[i].Exterior;

            if (Contains(exterior[0]))
            {
                anyInside = true;
            }
            else
            {
                anyOutside = true;
            }

            // A disconnected complex polygon can have one part inside and another outside without either boundary crossing. It is not wholly inside or wholly outside
            if (anyInside && anyOutside)
            {
                return PolygonShapeRelation.Intersecting;
            }
        }

        // No boundaries cross, but this polygon (or one of its hole contours) can still be completely enclosed by the filled area of the query polygon
        var lenS = _contourSamples.Length;
        for (var i = 0; i < lenS; ++i)
        {
            if (idx.Contains(_contourSamples[i]))
            {
                return PolygonShapeRelation.Intersecting;
            }
        }

        return anyInside ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    private bool BoundaryIntersectsIndex(PolygonBoundaryIndex2D other)
    {
        if (_bbMaxX < other._bbMinX - Eps || _bbMinX > other._bbMaxX + Eps || _bbMaxY < other._bbMinY - Eps || _bbMinY > other._bbMaxY + Eps)
        {
            return false;
        }

        // Probe the smaller boundary against the larger index to minimize indexed segment queries
        var probe = this;
        var target = other;
        if (probe._sourceEdgeCount > target._sourceEdgeCount)
        {
            (probe, target) = (target, probe);
        }
        return target.BoundaryIntersectsIndexedEdges(probe);
    }

    private bool BoundaryIntersectsIndexedEdges(PolygonBoundaryIndex2D source)
    {
        // Every non-horizontal source edge appears exactly once in the range of edges starting in its first row
        for (var row = 0; row < source._rows; ++row)
        {
            int es = source._rowNewStarts[row], ee = source._rowEnds[row];
            for (var i = es; i < ee; ++i)
            {
                var edgeMinX = source._minX[i];
                var edgeMaxX = source._maxX[i];
                var edgeMinY = source._y0[i];
                var edgeMaxY = source._y1[i];
                if (edgeMaxX < _bbMinX - Eps || edgeMinX > _bbMaxX + Eps || edgeMaxY < _bbMinY - Eps || edgeMinY > _bbMaxY + Eps)
                {
                    continue;
                }

                var ax = source._x0[i];
                var ay = edgeMinY;
                if (BoundaryIntersectsSegment(ax, ay, ax + source._dx[i], ay + source._dy[i]))
                {
                    return true;
                }
            }
        }

        // horizontal edges are not copied across rows and can be traversed directly
        var horizontalCount = source._hEdges.Length;
        for (var i = 0; i < horizontalCount; ++i)
        {
            ref readonly var edge = ref source._hEdges[i];
            var maxX = edge.maxX;
            var minX = edge.minX;
            var edgeY = edge.y;
            if (maxX < _bbMinX - Eps || minX > _bbMaxX + Eps || edgeY < _bbMinY - Eps || edgeY > _bbMaxY + Eps)
            {
                continue;
            }
            if (BoundaryIntersectsSegment(minX, edgeY, maxX, edgeY))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyCircle(in WDir center, float radius)
    {
        var centerX = center.X;
        var centerZ = center.Z;
        var limit = radius + Eps;
        if (limit >= 0f && (centerX + limit < _bbMinX || centerX - limit > _bbMaxX || centerZ + limit < _bbMinY || centerZ - limit > _bbMaxY))
        {
            return PolygonShapeRelation.Outside;
        }

        if (BoundaryIntersectsCircle(centerX, centerZ, limit))
        {
            return PolygonShapeRelation.Intersecting;
        }

        return Contains(center) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyRectangle(in WDir center, in WDir forward, float halfWidth, float halfLength)
    {
        return ClassifyRectangleNormalized(center.X, center.Z, forward.X, forward.Z, halfWidth, halfLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyAABBRect(in WDir center, float halfWidth, float halfLength)
    {
        var cx = center.X;
        var cz = center.Z;
        var minX = cx - halfWidth - Eps;
        var maxX = cx + halfWidth + Eps;
        var minY = cz - halfLength - Eps;
        var maxY = cz + halfLength + Eps;

        if (maxX < _bbMinX || minX > _bbMaxX || maxY < _bbMinY || minY > _bbMaxY)
        {
            return PolygonShapeRelation.Outside;
        }

        if (BoundaryIntersectsAABBRect(minX, minY, maxX, maxY))
        {
            return PolygonShapeRelation.Intersecting;
        }

        return Contains(center) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolygonShapeRelation ClassifyDirectionalRectangle(in WDir origin, in WDir forward, float lengthFront, float lengthBack, float halfWidth)
    {
        var fx = forward.X;
        var fz = forward.Z;
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
        var ha = halfAngle.Rad;
        if (ha >= MathF.PI)
        {
            return ClassifyCircle(origin, radius);
        }
        var originX = origin.X;
        var originZ = origin.Z;
        var fx = forward.X;
        var fz = forward.Z;
        if (BoundaryIntersectsCone(originX, originZ, fx, fz, radius, ha))
        {
            return PolygonShapeRelation.Intersecting;
        }

        // A sector is connected. With no polygon boundary in it, one interior sample classifies the whole shape.
        var sampleDistance = 0.5f * radius;
        var sample = new WDir(originX + fx * sampleDistance, originZ + fz * sampleDistance);
        return Contains(sample) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        var fx = forward.X;
        var fz = forward.Z;
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
        var fx = forward.X;
        var fz = forward.Z;
        var end = new WDir(origin.X + fx * length, origin.Z + fz * length);
        return ClassifyCapsule(origin, end, radius);
    }

    public PolygonShapeRelation ClassifyArcCapsule(in WDir origin, in WDir toOrbitCenter, Angle angularLength, float radius)
    {
        var cxOffset = toOrbitCenter.X;
        var czOffset = toOrbitCenter.Z;
        var orbitRadiusSq = cxOffset * cxOffset + czOffset * czOffset;

        var sweep = angularLength.Rad;
        if (orbitRadiusSq <= TinyLen2)
        {
            return ClassifyCircle(origin, radius);
        }

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
        var triangle = new TriangleGeometry(a, b, c);
        if (triangle.maxX < _bbMinX - Eps || triangle.minX > _bbMaxX + Eps || triangle.maxY < _bbMinY - Eps || triangle.minY > _bbMaxY + Eps)
        {
            return PolygonShapeRelation.Outside;
        }

        if (BoundaryIntersectsTriangle(triangle))
        {
            return PolygonShapeRelation.Intersecting;
        }

        const float OneThird = 1f / 3f;
        var sample = new WDir((a.X + b.X + c.X) * OneThird, (a.Z + b.Z + c.Z) * OneThird);
        return Contains(sample) ? PolygonShapeRelation.Inside : PolygonShapeRelation.Outside;
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
        var fx = dir.X;
        var fz = dir.Z;
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

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelSegmentIntersectsDispatch(es, ee, ax, ay, bx, by, minX, minY - Eps, maxX, maxY + Eps))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            hs = FirstHorizontalAtOrAbove(hs, he, minY - Eps);
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
                if (edge.y > maxY + Eps)
                {
                    break;
                }

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
            if (worldMaxX < _rowMinX[row] || worldMinX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelSimplePolygonIntersectsDispatch(es, ee, vertices, worldMinX, minY - Eps, worldMaxX, maxY + Eps))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, minY - Eps);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, maxY + Eps);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
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

    private bool BoundaryIntersectsTriangle(in TriangleGeometry triangle)
    {
        var worldMinX = triangle.minX - Eps;
        var worldMaxX = triangle.maxX + Eps;
        var minY = triangle.minY;
        var maxY = triangle.maxY;
        if (!TryGetRowRange(minY, maxY, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            if (worldMaxX < _rowMinX[row] || worldMinX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelTriangleIntersectsDispatch(es, ee, triangle, worldMinX, minY - Eps, worldMaxX, maxY + Eps))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, minY - Eps);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, maxY + Eps);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;

                if (edgeMaxX < worldMinX || edgeMinX > worldMaxX)
                {
                    continue;
                }
                var edgeY = edge.y;

                if (SegmentIntersectsFilledTriangle(edgeMinX, edgeY, edgeMaxX, edgeY, triangle))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SegmentIntersectsFilledTriangle(float ax, float ay, float bx, float by, in TriangleGeometry triangle)
    {
        if (Math.Max(ax, bx) + Eps < triangle.minX || triangle.maxX + Eps < Math.Min(ax, bx)
         || Math.Max(ay, by) + Eps < triangle.minY || triangle.maxY + Eps < Math.Min(ay, by))
        {
            return false;
        }

        if (PointInTriangle(ax, ay, triangle) || PointInTriangle(bx, by, triangle))
        {
            return true;
        }

        // same boundary-inclusive semantics as the generic simple-polygon path, but fully unrolled for the fixed three-edge case
        return SegmentsIntersect(ax, ay, bx, by, triangle.cx, triangle.cy, triangle.ax, triangle.ay)
           || SegmentsIntersect(ax, ay, bx, by, triangle.ax, triangle.ay, triangle.bx, triangle.by)
            || SegmentsIntersect(ax, ay, bx, by, triangle.bx, triangle.by, triangle.cx, triangle.cy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointInTriangle(float px, float py, in TriangleGeometry triangle)
    {
        var cx = triangle.cx;
        var cy = triangle.cy;
        var e0x = triangle.e0x;
        var e0y = triangle.e0y;
        var ax = triangle.ax;
        var ay = triangle.ay;
        var e1x = triangle.e1x;
        var e1y = triangle.e1y;
        var bx = triangle.bx;
        var by = triangle.by;
        var e2x = triangle.e2x;
        var e2y = triangle.e2y;

        // For a non-degenerate triangle, three same-sign edge crosses prove inclusion
        var c0 = Cross(e0x, e0y, px - cx, py - cy);
        var c1 = Cross(e1x, e1y, px - ax, py - ay);
        var c2 = Cross(e2x, e2y, px - bx, py - by);
        if (Math.Abs(triangle.area2) > TinyDen)
        {
            var hasNegative = c0 < 0f || c1 < 0f || c2 < 0f;
            var hasPositive = c0 > 0f || c1 > 0f || c2 > 0f;
            if (!(hasNegative && hasPositive))
            {
                return true;
            }
        }

        return PointSegmentDistanceSqKnownInv(px, py, cx, cy, e0x, e0y, triangle.e0InvLen2) <= Eps2
            || PointSegmentDistanceSqKnownInv(px, py, ax, ay, e1x, e1y, triangle.e1InvLen2) <= Eps2
            || PointSegmentDistanceSqKnownInv(px, py, bx, by, e2x, e2y, triangle.e2InvLen2) <= Eps2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float PointSegmentDistanceSqKnownInv(float px, float py, float ax, float ay, float dx, float dy, float invLenSq)
    {
        var lenSq = dx * dx + dy * dy;
        if (lenSq <= TinyLen2)
        {
            var ex = px - ax;
            var ey = py - ay;
            return ex * ex + ey * ey;
        }

        return PointSegmentDistanceSqKnownInvNonDegenerate(px, py, ax, ay, dx, dy, invLenSq);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float PointSegmentDistanceSqKnownInvNonDegenerate(float px, float py, float ax, float ay, float dx, float dy, float invLenSq)
    {
        var rx = px - ax;
        var ry = py - ay;
        var dot = rx * dx + ry * dy;
        if (dot <= 0f)
        {
            return rx * rx + ry * ry;
        }
        if (dot * invLenSq >= 1f)
        {
            rx -= dx;
            ry -= dy;
            return rx * rx + ry * ry;
        }

        var cross = rx * dy - ry * dx;
        return cross * cross * invLenSq;
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
            if (PointWithinDistanceSqOfSegment(px, py, prevX, prevZ, curX, curZ, Eps2))
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
        var sector = new AnnularSectorGeometry(cx, cz, fx, fz, innerRadius, outerRadius, halfAngle);
        var minX = sector.minX;
        var maxX = sector.maxX;
        var minZ = sector.minZ;
        var maxZ = sector.maxZ;
        if (!TryGetRowRange(minZ, maxZ, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            if (maxX < _rowMinX[row] || minX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelAnnularSectorIntersectsDispatch(es, ee, sector))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, minZ);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, maxZ);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelAnnularSectorIntersectsDispatch(int es, int ee, in AnnularSectorGeometry sector)
    {
        // The annulus-distance test is vectorized; only geometrically plausible lanes run the exact angular/arc predicate
        var sMinX = sector.minX;
        var sMaxX = sector.maxX;
        var sCX = sector.cx;
        var sCZ = sector.cz;
        var sISQ = sector.innerSq;
        var sOSQ = sector.outerSq;
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var vCX = Vector512.Create(sCX);
            var vCZ = Vector512.Create(sCZ);
            var vInnerSq = Vector512.Create(sISQ);
            var vOuterSq = Vector512.Create(sOSQ);
            var vMinX = Vector512.Create(sMinX);
            var vMaxX = Vector512.Create(sMaxX);
            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                var mask = AnnulusCandidateMask512(i, vCX, vCZ, vInnerSq, vOuterSq, vMinX, vMaxX);
                while (mask != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1ul;
                    var j = i + lane;
                    var x0 = _x0[j];
                    var y0 = _y0[j];
                    if (SegmentIntersectsAnnularSector(x0, y0, x0 + _dx[j], y0 + _dy[j], sector))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        if (Avx2.IsSupported && ee - es >= 8)
        {
            var vCX = Vector256.Create(sCX);
            var vCZ = Vector256.Create(sCZ);
            var vInnerSq = Vector256.Create(sISQ);
            var vOuterSq = Vector256.Create(sOSQ);
            var vMinX = Vector256.Create(sMinX);
            var vMaxX = Vector256.Create(sMaxX);
            var i = es;

            for (; i + 8 <= ee; i += 8)
            {
                var mask = AnnulusCandidateMask256(i, vCX, vCZ, vInnerSq, vOuterSq, vMinX, vMaxX);
                while (mask != 0u)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    var j = i + lane;
                    var x0 = _x0[j];
                    var y0 = _y0[j];
                    if (SegmentIntersectsAnnularSector(x0, y0, x0 + _dx[j], y0 + _dy[j], sector))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        for (var i = es; i < ee; ++i)
        {
            var x0 = _x0[i];
            var y0 = _y0[i];
            var dx = _dx[i];
            var dy = _dy[i];
            if (_maxX[i] >= sMinX && _minX[i] <= sMaxX
                && SegmentDistanceRangeOverlapsAnnulusKnownInv(x0, y0, dx, dy, _invLen2[i], sCX, sCZ, sISQ, sOSQ)
                && SegmentIntersectsAnnularSector(x0, y0, x0 + dx, y0 + dy, sector))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong AnnulusCandidateMask512(int i, Vector512<float> cx, Vector512<float> cz, Vector512<float> innerSq, Vector512<float> outerSq,
        Vector512<float> queryMinX, Vector512<float> queryMaxX)
    {
        var x0 = Load512(_x0, i);
        var y0 = Load512(_y0, i);
        var dx = Load512(_dx, i);
        var dy = Load512(_dy, i);
        var apx = Avx512F.Subtract(cx, x0);
        var apy = Avx512F.Subtract(cz, y0);
        var dot = Avx512F.FusedMultiplyAdd(apx, dx, Avx512F.Multiply(apy, dy));
        var t = Avx512F.Min(Vector512<float>.One, Avx512F.Max(Vector512<float>.Zero, Avx512F.Multiply(dot, Load512(_invLen2, i))));
        var qx = Avx512F.FusedMultiplyAdd(t, dx, x0);
        var qy = Avx512F.FusedMultiplyAdd(t, dy, y0);
        var mdx = Avx512F.Subtract(cx, qx);
        var mdy = Avx512F.Subtract(cz, qy);
        var minD = Avx512F.FusedMultiplyAdd(mdx, mdx, Avx512F.Multiply(mdy, mdy));
        var ad = Avx512F.FusedMultiplyAdd(apx, apx, Avx512F.Multiply(apy, apy));
        var bpx = Avx512F.Subtract(apx, dx);
        var bpy = Avx512F.Subtract(apy, dy);
        var bd = Avx512F.FusedMultiplyAdd(bpx, bpx, Avx512F.Multiply(bpy, bpy));
        var hit = Vector512.GreaterThanOrEqual(Load512(_maxX, i), queryMinX) & Vector512.LessThanOrEqual(Load512(_minX, i), queryMaxX)
            & Vector512.LessThanOrEqual(minD, outerSq) & Vector512.GreaterThanOrEqual(Avx512F.Max(ad, bd), innerSq);
        return hit.ExtractMostSignificantBits();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint AnnulusCandidateMask256(int i, Vector256<float> cx, Vector256<float> cz, Vector256<float> innerSq, Vector256<float> outerSq,
        Vector256<float> queryMinX, Vector256<float> queryMaxX)
    {
        var x0 = Load256(_x0, i);
        var y0 = Load256(_y0, i);
        var dx = Load256(_dx, i);
        var dy = Load256(_dy, i);
        var apx = Avx.Subtract(cx, x0);
        var apy = Avx.Subtract(cz, y0);
        var dot = Fma.IsSupported ? Fma.MultiplyAdd(apx, dx, Avx.Multiply(apy, dy)) : Avx.Add(Avx.Multiply(apx, dx), Avx.Multiply(apy, dy));
        var t = Avx.Min(Vector256<float>.One, Avx.Max(Vector256<float>.Zero, Avx.Multiply(dot, Load256(_invLen2, i))));
        var qx = Fma.IsSupported ? Fma.MultiplyAdd(t, dx, x0) : Avx.Add(Avx.Multiply(t, dx), x0);
        var qy = Fma.IsSupported ? Fma.MultiplyAdd(t, dy, y0) : Avx.Add(Avx.Multiply(t, dy), y0);
        var mdx = Avx.Subtract(cx, qx);
        var mdy = Avx.Subtract(cz, qy);
        var minD = Fma.IsSupported ? Fma.MultiplyAdd(mdx, mdx, Avx.Multiply(mdy, mdy)) : Avx.Add(Avx.Multiply(mdx, mdx), Avx.Multiply(mdy, mdy));
        var ad = Fma.IsSupported ? Fma.MultiplyAdd(apx, apx, Avx.Multiply(apy, apy)) : Avx.Add(Avx.Multiply(apx, apx), Avx.Multiply(apy, apy));
        var bpx = Avx.Subtract(apx, dx);
        var bpy = Avx.Subtract(apy, dy);
        var bd = Fma.IsSupported ? Fma.MultiplyAdd(bpx, bpx, Avx.Multiply(bpy, bpy)) : Avx.Add(Avx.Multiply(bpx, bpx), Avx.Multiply(bpy, bpy));
        var hit = Vector256.GreaterThanOrEqual(Load256(_maxX, i), queryMinX) & Vector256.LessThanOrEqual(Load256(_minX, i), queryMaxX)
            & Vector256.LessThanOrEqual(minD, outerSq) & Vector256.GreaterThanOrEqual(Avx.Max(ad, bd), innerSq);
        return hit.ExtractMostSignificantBits();
    }

    private bool BoundaryIntersectsCapsule(float ax, float az, float bx, float bz, float radius)
    {
        var reach = radius + Eps;
        var minX = Math.Min(ax, bx) - reach;
        var maxX = Math.Max(ax, bx) + reach;
        var minZ = Math.Min(az, bz) - reach;
        var maxZ = Math.Max(az, bz) + reach;
        var reachSq = reach * reach;
        var capsuleDx = bx - ax;
        var capsuleDy = bz - az;
        var capsuleLen2 = capsuleDx * capsuleDx + capsuleDy * capsuleDy;
        var capsuleNonDegenerate = capsuleLen2 > TinyLen2;
        var capsuleInvLen2 = 1f / Math.Max(capsuleLen2, TinyLen2);

        if (!TryGetRowRange(minZ, maxZ, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            if (maxX < _rowMinX[row] || minX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelCapsuleIntersectsDispatch(es, ee, ax, az, capsuleDx, capsuleDy, capsuleInvLen2, capsuleNonDegenerate, reachSq, minX, maxX, minZ, maxZ))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, minZ);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, maxZ);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;
                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }

                var edgeY = edge.y;
                var edgeDx = edgeMaxX - edgeMinX;
                var edgeInvLen2 = 1f / Math.Max(edgeDx * edgeDx, TinyLen2);

                if (SegmentSegmentDistanceSqKnownInv(ax, az, capsuleDx, capsuleDy, capsuleInvLen2, edgeMinX, edgeY, edgeDx, 0f, edgeInvLen2) <= reachSq)
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
        var capReach = capsuleRadius + Eps;
        var capReachSq = capReach * capReach;
        var sector = new AnnularSectorGeometry(cx, cz, midX, midZ, innerRadius, outerRadius, halfAngle);
        var minX = Math.Min(sector.minX, Math.Min(startX, endX) - capReach);
        var maxX = Math.Max(sector.maxX, Math.Max(startX, endX) + capReach);
        var minZ = Math.Min(sector.minZ, Math.Min(startZ, endZ) - capReach);
        var maxZ = Math.Max(sector.maxZ, Math.Max(startZ, endZ) + capReach);

        if (!TryGetRowRange(minZ, maxZ, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            if (maxX < _rowMinX[row] || minX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelArcCapsuleIntersectsDispatch(es, ee, startX, startZ, endX, endZ, capReachSq, sector, minX, maxX))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, minZ);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, maxZ);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;
                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }

                var edgeY = edge.y;
                if (PointHorizontalSegmentDistanceSq(startX, startZ, edgeMinX, edgeY, edgeMaxX) <= capReachSq
                    || PointHorizontalSegmentDistanceSq(endX, endZ, edgeMinX, edgeY, edgeMaxX) <= capReachSq
                    || SegmentIntersectsAnnularSector(edgeMinX, edgeY, edgeMaxX, edgeY, sector))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool BoundaryIntersectsAABBRect(float minX, float minY, float maxX, float maxY)
    {
        if (!TryGetRowRange(minY, maxY, out var row0, out var row1))
        {
            return false;
        }

        var activeSetComplete = false;
        for (var row = row0; row <= row1; ++row)
        {
            if (maxX < _rowMinX[row] || minX > _rowMaxX[row])
            {
                // An edge may start in a skipped row and enter the query X range later
                activeSetComplete = false;
                continue;
            }

            // Once a full active set has been tested, later contiguous rows only need newly
            // starting edges. A row-level cull resets this optimization (see above)
            int es = activeSetComplete ? _rowNewStarts[row] : _rowOffsets[row], ee = _rowEnds[row];
            if (KernelAABBRectIntersectsDispatch(es, ee, minX, minY, maxX, maxY))
            {
                return true;
            }
            activeSetComplete = true;

            // Horizontal edges need no clipping: Y inclusion plus X interval overlap is exact
            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, minY);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, maxY);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
                var edgeY = edge.y;
                // FirstHorizontalAtOrAbove/FirstHorizontalAbove deliberately skip their binary search for small row slices, so the returned range is only a
                // coarse range in that case. Keep the exact Y test here
                if (edgeY < minY || edgeY > maxY)
                {
                    continue;
                }
                if (edge.maxX >= minX && edge.minX <= maxX)
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
            if (worldMaxX < _rowMinX[row] || worldMinX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelRectangleIntersectsDispatch(es, ee, cx, cz, rx, rz, fx, fz, halfWidth, halfLength, worldMinX, worldMaxX))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, cz - extentZ - Eps);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, cz + extentZ + Eps);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelCapsuleIntersectsDispatch(int es, int ee, float ax, float ay, float capsuleDx, float capsuleDy, float capsuleInvLen2, bool capsuleNonDegenerate,
        float reachSq, float queryMinX, float queryMaxX, float queryMinY, float queryMaxY)
    {
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var i = es;
            var qMinX = Vector512.Create(queryMinX);
            var qMaxX = Vector512.Create(queryMaxX);
            var qMinY = Vector512.Create(queryMinY);
            var qMaxY = Vector512.Create(queryMaxY);
            for (; i + 16 <= ee; i += 16)
            {
                var ey0 = Load512(_y0, i);
                var ey1 = Load512(_y1, i);
                var overlap = Vector512.GreaterThanOrEqual(Load512(_maxX, i), qMinX) & Vector512.LessThanOrEqual(Load512(_minX, i), qMaxX)
                    & Vector512.GreaterThanOrEqual(ey1, qMinY) & Vector512.LessThanOrEqual(ey0, qMaxY);
                var mask = overlap.ExtractMostSignificantBits();
                while (mask != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1ul;
                    if (CapsuleIntersectsEdgeScalar(i + lane, ax, ay, capsuleDx, capsuleDy, capsuleInvLen2, capsuleNonDegenerate, reachSq))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        if (Avx2.IsSupported && ee - es >= 8)
        {
            var i = es;
            var qMinX = Vector256.Create(queryMinX);
            var qMaxX = Vector256.Create(queryMaxX);
            var qMinY = Vector256.Create(queryMinY);
            var qMaxY = Vector256.Create(queryMaxY);
            for (; i + 8 <= ee; i += 8)
            {
                var ey0 = Load256(_y0, i);
                var ey1 = Load256(_y1, i);
                var overlap = Vector256.GreaterThanOrEqual(Load256(_maxX, i), qMinX) & Vector256.LessThanOrEqual(Load256(_minX, i), qMaxX)
                    & Vector256.GreaterThanOrEqual(ey1, qMinY) & Vector256.LessThanOrEqual(ey0, qMaxY);
                var mask = overlap.ExtractMostSignificantBits();
                while (mask != 0u)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    if (CapsuleIntersectsEdgeScalar(i + lane, ax, ay, capsuleDx, capsuleDy, capsuleInvLen2, capsuleNonDegenerate, reachSq))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        for (var i = es; i < ee; ++i)
        {
            if (_maxX[i] >= queryMinX && _minX[i] <= queryMaxX && _y1[i] >= queryMinY && _y0[i] <= queryMaxY
                && CapsuleIntersectsEdgeScalar(i, ax, ay, capsuleDx, capsuleDy, capsuleInvLen2, capsuleNonDegenerate, reachSq))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CapsuleIntersectsEdgeScalar(int i, float ax, float ay, float capsuleDx, float capsuleDy, float capsuleInvLen2, bool capsuleNonDegenerate, float reachSq)
    {
        var x0 = _x0[i];
        var y0 = _y0[i];
        var dx = _dx[i];
        var dy = _dy[i];
        var invLen = _invLen2[i];
        var distanceSq = capsuleNonDegenerate
            ? SegmentSegmentDistanceSqKnownInvNonDegenerate(ax, ay, capsuleDx, capsuleDy, capsuleInvLen2, x0, y0, _dx[i], dy, invLen)
            : PointSegmentDistanceSqKnownInvNonDegenerate(ax, ay, x0, y0, dx, dy, invLen);
        return distanceSq <= reachSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelConeIntersectsDispatch(int es, int ee, in SectorGeometry sector)
    {
        // A segment can touch the sector only if it touches the containing circle. SIMD performs that radial rejection; exact angular tests remain scalar
        var sOX = sector.ox;
        var sOZ = sector.oz;
        var sRSQ = sector.radiusSq;
        var sMinX = sector.minX;
        var sMaxX = sector.maxX;
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var vOX = Vector512.Create(sOX);
            var vOZ = Vector512.Create(sOZ);
            var vRadiusSq = Vector512.Create(sRSQ);
            var vMinX = Vector512.Create(sMinX);
            var vMaxX = Vector512.Create(sMaxX);
            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                var mask = AnnulusCandidateMask512(i, vOX, vOZ, Vector512<float>.Zero, vRadiusSq, vMinX, vMaxX);
                while (mask != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1ul;
                    var j = i + lane;
                    var x0 = _x0[j];
                    var y0 = _y0[j];
                    if (SegmentIntersectsSector(x0, y0, x0 + _dx[j], y0 + _dy[j], sector))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        if (Avx2.IsSupported && ee - es >= 8)
        {
            var vOX = Vector256.Create(sOX);
            var vOZ = Vector256.Create(sOZ);
            var vRadiusSq = Vector256.Create(sRSQ);
            var vMinX = Vector256.Create(sMinX);
            var vMaxX = Vector256.Create(sMaxX);
            var i = es;
            for (; i + 8 <= ee; i += 8)
            {
                var mask = AnnulusCandidateMask256(i, vOX, vOZ, Vector256<float>.Zero, vRadiusSq, vMinX, vMaxX);
                while (mask != 0u)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    var j = i + lane;
                    var x0 = _x0[j];
                    var y0 = _y0[j];
                    if (SegmentIntersectsSector(x0, y0, x0 + _dx[j], y0 + _dy[j], sector))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        for (var i = es; i < ee; ++i)
        {
            var x0 = _x0[i];
            var y0 = _y0[i];
            var dx = _dx[i];
            var dy = _dy[i];
            if (_maxX[i] >= sMinX && _minX[i] <= sMaxX
                && PointSegmentDistanceSqKnownInvNonDegenerate(sOX, sOZ, x0, y0, dx, dy, _invLen2[i]) <= sRSQ
                && SegmentIntersectsSector(x0, y0, x0 + dx, y0 + dy, sector))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelArcCapsuleIntersectsDispatch(int es, int ee, float startX, float startZ, float endX, float endZ, float capReachSq,
        in AnnularSectorGeometry sector, float queryMinX, float queryMaxX)
    {
        // The swept shape is the union of an annular sector and two endpoint circles. Fuse all three SIMD broad-phase masks before exact testing.
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var vQueryMinX = Vector512.Create(queryMinX);
            var vQueryMaxX = Vector512.Create(queryMaxX);
            var vCapReachSq = Vector512.Create(capReachSq);
            var vSectorCX = Vector512.Create(sector.cx);
            var vSectorCZ = Vector512.Create(sector.cz);
            var vSectorInnerSq = Vector512.Create(sector.innerSq);
            var vSectorOuterSq = Vector512.Create(sector.outerSq);
            var vStartX = Vector512.Create(startX);
            var vStartZ = Vector512.Create(startZ);
            var vEndX = Vector512.Create(endX);
            var vEndZ = Vector512.Create(endZ);
            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                var mask = AnnulusCandidateMask512(i, vSectorCX, vSectorCZ, vSectorInnerSq, vSectorOuterSq, vQueryMinX, vQueryMaxX)
                    | AnnulusCandidateMask512(i, vStartX, vStartZ, Vector512<float>.Zero, vCapReachSq, vQueryMinX, vQueryMaxX)
                    | AnnulusCandidateMask512(i, vEndX, vEndZ, Vector512<float>.Zero, vCapReachSq, vQueryMinX, vQueryMaxX);
                while (mask != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1ul;
                    var j = i + lane;
                    var ax = _x0[j];
                    var ay = _y0[j];
                    var dx = _dx[j];
                    var dy = _dy[j];
                    var inv = _invLen2[j];
                    if (PointSegmentDistanceSqKnownInvNonDegenerate(startX, startZ, ax, ay, dx, dy, inv) <= capReachSq
                        || PointSegmentDistanceSqKnownInvNonDegenerate(endX, endZ, ax, ay, dx, dy, inv) <= capReachSq
                        || SegmentIntersectsAnnularSector(ax, ay, ax + dx, ay + dy, sector))
                        return true;
                }
            }
            es = i;
        }
        if (Avx2.IsSupported && ee - es >= 8)
        {
            var vQueryMinX = Vector256.Create(queryMinX);
            var vQueryMaxX = Vector256.Create(queryMaxX);
            var vCapReachSq = Vector256.Create(capReachSq);
            var vSectorCX = Vector256.Create(sector.cx);
            var vSectorCZ = Vector256.Create(sector.cz);
            var vSectorInnerSq = Vector256.Create(sector.innerSq);
            var vSectorOuterSq = Vector256.Create(sector.outerSq);
            var vStartX = Vector256.Create(startX);
            var vStartZ = Vector256.Create(startZ);
            var vEndX = Vector256.Create(endX);
            var vEndZ = Vector256.Create(endZ);
            var i = es;
            for (; i + 8 <= ee; i += 8)
            {
                var mask = AnnulusCandidateMask256(i, vSectorCX, vSectorCZ, vSectorInnerSq, vSectorOuterSq, vQueryMinX, vQueryMaxX)
                    | AnnulusCandidateMask256(i, vStartX, vStartZ, Vector256<float>.Zero, vCapReachSq, vQueryMinX, vQueryMaxX)
                    | AnnulusCandidateMask256(i, vEndX, vEndZ, Vector256<float>.Zero, vCapReachSq, vQueryMinX, vQueryMaxX);
                while (mask != 0u)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    var j = i + lane;
                    var ax = _x0[j];
                    var ay = _y0[j];
                    var dx = _dx[j];
                    var dy = _dy[j];
                    var inv = _invLen2[j];
                    if (PointSegmentDistanceSqKnownInvNonDegenerate(startX, startZ, ax, ay, dx, dy, inv) <= capReachSq
                        || PointSegmentDistanceSqKnownInvNonDegenerate(endX, endZ, ax, ay, dx, dy, inv) <= capReachSq
                        || SegmentIntersectsAnnularSector(ax, ay, ax + dx, ay + dy, sector))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        for (var i = es; i < ee; ++i)
        {
            if (_maxX[i] < queryMinX || _minX[i] > queryMaxX)
            {
                continue;
            }
            var ax = _x0[i];
            var ay = _y0[i];
            var dx = _dx[i];
            var dy = _dy[i];
            var inv = _invLen2[i];
            if (PointSegmentDistanceSqKnownInvNonDegenerate(startX, startZ, ax, ay, dx, dy, inv) <= capReachSq
                || PointSegmentDistanceSqKnownInvNonDegenerate(endX, endZ, ax, ay, dx, dy, inv) <= capReachSq
                || SegmentIntersectsAnnularSector(ax, ay, ax + dx, ay + dy, sector))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelSegmentIntersectsDispatch(int es, int ee, float ax, float ay, float bx, float by, float minX, float minY, float maxX, float maxY)
    {
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var vMinX = Vector512.Create(minX);
            var vMinY = Vector512.Create(minY);
            var vMaxX = Vector512.Create(maxX);
            var vMaxY = Vector512.Create(maxY);
            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                var mask = SegmentAabbCandidateMask512(i, vMinX, vMinY, vMaxX, vMaxY);
                while (mask != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1ul;
                    var j = i + lane;
                    var cx = _x0[j];
                    var cy = _y0[j];
                    if (SegmentsIntersect(ax, ay, bx, by, cx, cy, cx + _dx[j], cy + _dy[j]))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }

        if (Avx2.IsSupported && ee - es >= 8)
        {
            var vMinX = Vector256.Create(minX);
            var vMinY = Vector256.Create(minY);
            var vMaxX = Vector256.Create(maxX);
            var vMaxY = Vector256.Create(maxY);
            var i = es;
            for (; i + 8 <= ee; i += 8)
            {
                var mask = SegmentAabbCandidateMask256(i, vMinX, vMinY, vMaxX, vMaxY);
                while (mask != 0u)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    var j = i + lane;
                    var cx = _x0[j];
                    var cy = _y0[j];
                    if (SegmentsIntersect(ax, ay, bx, by, cx, cy, cx + _dx[j], cy + _dy[j]))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }

        for (var i = es; i < ee; ++i)
        {
            var cy = _y0[i];
            var dy = _dy[i];
            if (_maxX[i] < minX || _minX[i] > maxX || Math.Max(cy, cy + dy) < minY || Math.Min(cy, cy + dy) > maxY)
            {
                continue;
            }

            var cx = _x0[i];
            if (SegmentsIntersect(ax, ay, bx, by, cx, cy, cx + _dx[i], cy + dy))
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelTriangleIntersectsDispatch(int es, int ee, in TriangleGeometry triangle, float minX, float minY, float maxX, float maxY)
    {
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var vMinX = Vector512.Create(minX);
            var vMinY = Vector512.Create(minY);
            var vMaxX = Vector512.Create(maxX);
            var vMaxY = Vector512.Create(maxY);
            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                var mask = SegmentAabbCandidateMask512(i, vMinX, vMinY, vMaxX, vMaxY);
                while (mask != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1ul;
                    var j = i + lane;
                    var y0 = _y0[j];
                    var x0 = _x0[j];
                    if (SegmentIntersectsFilledTriangle(x0, y0, x0 + _dx[j], y0 + _dy[j], triangle))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        if (Avx2.IsSupported && ee - es >= 8)
        {
            var vMinX = Vector256.Create(minX);
            var vMinY = Vector256.Create(minY);
            var vMaxX = Vector256.Create(maxX);
            var vMaxY = Vector256.Create(maxY);
            var i = es;
            for (; i + 8 <= ee; i += 8)
            {
                var mask = SegmentAabbCandidateMask256(i, vMinX, vMinY, vMaxX, vMaxY);
                while (mask != 0u)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    var j = i + lane;
                    var y0 = _y0[j];
                    var x0 = _x0[j];
                    if (SegmentIntersectsFilledTriangle(x0, y0, x0 + _dx[j], y0 + _dy[j], triangle))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        for (var i = es; i < ee; ++i)
        {
            var y0 = _y0[i];
            var x0 = _x0[i];
            var dx = _dx[i];
            var dy = _dy[i];
            if (_maxX[i] >= minX && _minX[i] <= maxX && Math.Max(y0, y0 + dy) >= minY && Math.Min(y0, y0 + dy) <= maxY
                && SegmentIntersectsFilledTriangle(x0, y0, x0 + dx, y0 + dy, triangle))
            {
                return true;
            }
        }
        return false;
    }

    private bool KernelSimplePolygonIntersectsDispatch(int es, int ee, ReadOnlySpan<WDir> vertices, float minX, float minY, float maxX, float maxY)
    {
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var vMinX = Vector512.Create(minX);
            var vMinY = Vector512.Create(minY);
            var vMaxX = Vector512.Create(maxX);
            var vMaxY = Vector512.Create(maxY);
            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                var mask = SegmentAabbCandidateMask512(i, vMinX, vMinY, vMaxX, vMaxY);
                while (mask != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    var j = i + lane;
                    var y0 = _y0[j];
                    var x0 = _x0[j];
                    if (SegmentIntersectsFilledSimplePolygon(x0, y0, x0 + _dx[j], y0 + _dy[j], vertices))
                        return true;
                }
            }
            es = i;
        }
        if (Avx2.IsSupported && ee - es >= 8)
        {
            var vMinX = Vector256.Create(minX);
            var vMinY = Vector256.Create(minY);
            var vMaxX = Vector256.Create(maxX);
            var vMaxY = Vector256.Create(maxY);
            var i = es;
            for (; i + 8 <= ee; i += 8)
            {
                var mask = SegmentAabbCandidateMask256(i, vMinX, vMinY, vMaxX, vMaxY);
                while (mask != 0u)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    var j = i + lane;
                    var y0 = _y0[j];
                    var x0 = _x0[j];
                    if (SegmentIntersectsFilledSimplePolygon(x0, y0, x0 + _dx[j], y0 + _dy[j], vertices))
                        return true;
                }
            }
            es = i;
        }
        for (var i = es; i < ee; ++i)
        {
            var y0 = _y0[i];
            var x0 = _x0[i];
            var dx = _dx[i];
            var dy = _dy[i];
            if (_maxX[i] >= minX && _minX[i] <= maxX && Math.Max(y0, y0 + dy) >= minY && Math.Min(y0, y0 + dy) <= maxY
                && SegmentIntersectsFilledSimplePolygon(x0, y0, x0 + dx, y0 + dy, vertices))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong SegmentAabbCandidateMask512(int i, Vector512<float> minX, Vector512<float> minY, Vector512<float> maxX, Vector512<float> maxY)
    {
        var y0 = Load512(_y0, i);
        var y1 = Avx512F.Add(y0, Load512(_dy, i));
        var hit = Vector512.GreaterThanOrEqual(Load512(_maxX, i), minX) & Vector512.LessThanOrEqual(Load512(_minX, i), maxX)
            & Vector512.GreaterThanOrEqual(Avx512F.Max(y0, y1), minY) & Vector512.LessThanOrEqual(Avx512F.Min(y0, y1), maxY);
        return hit.ExtractMostSignificantBits();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint SegmentAabbCandidateMask256(int i, Vector256<float> minX, Vector256<float> minY, Vector256<float> maxX, Vector256<float> maxY)
    {
        var y0 = Load256(_y0, i);
        var y1 = Avx.Add(y0, Load256(_dy, i));
        var hit = Vector256.GreaterThanOrEqual(Load256(_maxX, i), minX) & Vector256.LessThanOrEqual(Load256(_minX, i), maxX)
            & Vector256.GreaterThanOrEqual(Avx.Max(y0, y1), minY) & Vector256.LessThanOrEqual(Avx.Min(y0, y1), maxY);
        return hit.ExtractMostSignificantBits();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelAABBRectIntersectsDispatch(int es, int ee, float minX, float minY, float maxX, float maxY)
    {
        // Exact test for a non-horizontal segment against an AABB:
        // intersect its Y interval with the box, evaluate X at both clipped Y endpoints,
        // then test whether that X interval overlaps the box. Since X(y)=k*y+b is linear,
        // no slab division or scalar lane fallback is required
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var vMinX = Vector512.Create(minX);
            var vMinY = Vector512.Create(minY);
            var vMaxX = Vector512.Create(maxX);
            var vMaxY = Vector512.Create(maxY);
            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                var y0 = Load512(_y0, i);
                var y1 = Load512(_y1, i);
                var loY = Avx512F.Max(y0, vMinY);
                var hiY = Avx512F.Min(y1, vMaxY);
                var active = Vector512.LessThanOrEqual(loY, hiY)
                    & Vector512.GreaterThanOrEqual(Load512(_maxX, i), vMinX)
                    & Vector512.LessThanOrEqual(Load512(_minX, i), vMaxX);
                if (active.ExtractMostSignificantBits() == 0ul)
                {
                    continue;
                }

                var k = Load512(_k, i);
                var b = Load512(_b, i);
                var x0 = Avx512F.Add(Avx512F.Multiply(k, loY), b);
                var x1 = Avx512F.Add(Avx512F.Multiply(k, hiY), b);
                var segMinX = Avx512F.Min(x0, x1);
                var segMaxX = Avx512F.Max(x0, x1);
                var hit = active & Vector512.GreaterThanOrEqual(segMaxX, vMinX) & Vector512.LessThanOrEqual(segMinX, vMaxX);
                if (hit.ExtractMostSignificantBits() != 0ul)
                {
                    return true;
                }
            }
            es = i;
        }

        if (Avx2.IsSupported && ee - es >= 8)
        {
            var vMinX = Vector256.Create(minX);
            var vMinY = Vector256.Create(minY);
            var vMaxX = Vector256.Create(maxX);
            var vMaxY = Vector256.Create(maxY);
            var i = es;
            for (; i + 8 <= ee; i += 8)
            {
                var y0 = Load256(_y0, i);
                var y1 = Load256(_y1, i);
                var loY = Avx.Max(y0, vMinY);
                var hiY = Avx.Min(y1, vMaxY);
                var active = Vector256.LessThanOrEqual(loY, hiY)
                    & Vector256.GreaterThanOrEqual(Load256(_maxX, i), vMinX)
                    & Vector256.LessThanOrEqual(Load256(_minX, i), vMaxX);
                if (active.ExtractMostSignificantBits() == 0u)
                {
                    continue;
                }

                var k = Load256(_k, i);
                var b = Load256(_b, i);
                var x0 = Avx.Add(Avx.Multiply(k, loY), b);
                var x1 = Avx.Add(Avx.Multiply(k, hiY), b);
                var segMinX = Avx.Min(x0, x1);
                var segMaxX = Avx.Max(x0, x1);
                var hit = active & Vector256.GreaterThanOrEqual(segMaxX, vMinX) & Vector256.LessThanOrEqual(segMinX, vMaxX);
                if (hit.ExtractMostSignificantBits() != 0u)
                {
                    return true;
                }
            }
            es = i;
        }

        for (var i = es; i < ee; ++i)
        {
            if (_maxX[i] < minX || _minX[i] > maxX)
            {
                continue;
            }

            var loY = Math.Max(_y0[i], minY);
            var hiY = Math.Min(_y1[i], maxY);
            if (loY > hiY)
            {
                continue;
            }

            var k = _k[i];
            var b = _b[i];
            var x0 = k * loY + b;
            var x1 = k * hiY + b;
            if (Math.Max(x0, x1) >= minX && Math.Min(x0, x1) <= maxX)
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelRectangleIntersectsDispatch(int es, int ee, float cx, float cz, float rx, float rz, float fx, float fz, float halfWidth, float halfLength, float worldMinX, float worldMaxX)
    {
        // Vectorize the local-space segment AABB rejection; exact clipping remains scalar for surviving lanes
        if (Avx512F.IsSupported && ee - es >= 16)
        {
            var vCX = Vector512.Create(cx);
            var vCZ = Vector512.Create(cz);
            var vRX = Vector512.Create(rx);
            var vRZ = Vector512.Create(rz);
            var vFX = Vector512.Create(fx);
            var vFZ = Vector512.Create(fz);
            var vHalfWidth = Vector512.Create(halfWidth + Eps);
            var vHalfLength = Vector512.Create(halfLength + Eps);
            var vNegHalfWidth = Avx512F.Subtract(Vector512<float>.Zero, vHalfWidth);
            var vNegHalfLength = Avx512F.Subtract(Vector512<float>.Zero, vHalfLength);
            var vWorldMinX = Vector512.Create(worldMinX);
            var vWorldMaxX = Vector512.Create(worldMaxX);
            var i = es;
            for (; i + 16 <= ee; i += 16)
            {
                var mask = RectangleCandidateMask512(i, vCX, vCZ, vRX, vRZ, vFX, vFZ, vHalfWidth, vNegHalfWidth, vHalfLength, vNegHalfLength, vWorldMinX, vWorldMaxX);
                while (mask != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1ul;
                    var j = i + lane;
                    var x0 = _x0[j];
                    var y0 = _y0[j];
                    if (SegmentIntersectsOrientedRectangle(x0, y0, x0 + _dx[j], y0 + _dy[j], cx, cz, rx, rz, fx, fz, halfWidth, halfLength))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        if (Avx2.IsSupported && ee - es >= 8)
        {
            var vCX = Vector256.Create(cx);
            var vCZ = Vector256.Create(cz);
            var vRX = Vector256.Create(rx);
            var vRZ = Vector256.Create(rz);
            var vFX = Vector256.Create(fx);
            var vFZ = Vector256.Create(fz);
            var vHalfWidth = Vector256.Create(halfWidth + Eps);
            var vHalfLength = Vector256.Create(halfLength + Eps);
            var vNegHalfWidth = Avx.Subtract(Vector256<float>.Zero, vHalfWidth);
            var vNegHalfLength = Avx.Subtract(Vector256<float>.Zero, vHalfLength);
            var vWorldMinX = Vector256.Create(worldMinX);
            var vWorldMaxX = Vector256.Create(worldMaxX);
            var i = es;
            for (; i + 8 <= ee; i += 8)
            {
                var mask = RectangleCandidateMask256(i, vCX, vCZ, vRX, vRZ, vFX, vFZ, vHalfWidth, vNegHalfWidth, vHalfLength, vNegHalfLength, vWorldMinX, vWorldMaxX);
                while (mask != 0u)
                {
                    var lane = BitOperations.TrailingZeroCount(mask);
                    mask &= mask - 1u;
                    var j = i + lane;
                    var x0 = _x0[j];
                    var y0 = _y0[j];
                    if (SegmentIntersectsOrientedRectangle(x0, y0, x0 + _dx[j], y0 + _dy[j], cx, cz, rx, rz, fx, fz, halfWidth, halfLength))
                    {
                        return true;
                    }
                }
            }
            es = i;
        }
        for (var i = es; i < ee; ++i)
        {
            var x0 = _x0[i];
            var y0 = _y0[i];
            if (_maxX[i] >= worldMinX && _minX[i] <= worldMaxX && SegmentIntersectsOrientedRectangle(x0, y0, x0 + _dx[i], y0 + _dy[i], cx, cz, rx, rz, fx, fz, halfWidth, halfLength))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong RectangleCandidateMask512(int i, Vector512<float> cx, Vector512<float> cz, Vector512<float> rx, Vector512<float> rz,
        Vector512<float> fx, Vector512<float> fz, Vector512<float> halfWidth, Vector512<float> negHalfWidth,
        Vector512<float> halfLength, Vector512<float> negHalfLength, Vector512<float> worldMinX, Vector512<float> worldMaxX)
    {
        var x0 = Load512(_x0, i);
        var y0 = Load512(_y0, i);
        var x1 = Avx512F.Add(x0, Load512(_dx, i));
        var y1 = Avx512F.Add(y0, Load512(_dy, i));
        var dcx0 = Avx512F.Subtract(x0, cx);
        var dcy0 = Avx512F.Subtract(y0, cz);
        var dcx1 = Avx512F.Subtract(x1, cx);
        var dcy1 = Avx512F.Subtract(y1, cz);
        var lx0 = Avx512F.FusedMultiplyAdd(dcx0, rx, Avx512F.Multiply(dcy0, rz));
        var ly0 = Avx512F.FusedMultiplyAdd(dcx0, fx, Avx512F.Multiply(dcy0, fz));
        var lx1 = Avx512F.FusedMultiplyAdd(dcx1, rx, Avx512F.Multiply(dcy1, rz));
        var ly1 = Avx512F.FusedMultiplyAdd(dcx1, fx, Avx512F.Multiply(dcy1, fz));
        var local = Vector512.LessThanOrEqual(Avx512F.Min(lx0, lx1), halfWidth) & Vector512.GreaterThanOrEqual(Avx512F.Max(lx0, lx1), negHalfWidth)
            & Vector512.LessThanOrEqual(Avx512F.Min(ly0, ly1), halfLength) & Vector512.GreaterThanOrEqual(Avx512F.Max(ly0, ly1), negHalfLength);
        var world = Vector512.GreaterThanOrEqual(Load512(_maxX, i), worldMinX) & Vector512.LessThanOrEqual(Load512(_minX, i), worldMaxX);
        return (local & world).ExtractMostSignificantBits();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint RectangleCandidateMask256(int i, Vector256<float> cx, Vector256<float> cz, Vector256<float> rx, Vector256<float> rz,
        Vector256<float> fx, Vector256<float> fz, Vector256<float> halfWidth, Vector256<float> negHalfWidth,
        Vector256<float> halfLength, Vector256<float> negHalfLength, Vector256<float> worldMinX, Vector256<float> worldMaxX)
    {
        var x0 = Load256(_x0, i);
        var y0 = Load256(_y0, i);
        var x1 = Avx.Add(x0, Load256(_dx, i));
        var y1 = Avx.Add(y0, Load256(_dy, i));
        var dcx0 = Avx.Subtract(x0, cx);
        var dcy0 = Avx.Subtract(y0, cz);
        var dcx1 = Avx.Subtract(x1, cx);
        var dcy1 = Avx.Subtract(y1, cz);
        var lx0 = Fma.IsSupported ? Fma.MultiplyAdd(dcx0, rx, Avx.Multiply(dcy0, rz)) : Avx.Add(Avx.Multiply(dcx0, rx), Avx.Multiply(dcy0, rz));
        var ly0 = Fma.IsSupported ? Fma.MultiplyAdd(dcx0, fx, Avx.Multiply(dcy0, fz)) : Avx.Add(Avx.Multiply(dcx0, fx), Avx.Multiply(dcy0, fz));
        var lx1 = Fma.IsSupported ? Fma.MultiplyAdd(dcx1, rx, Avx.Multiply(dcy1, rz)) : Avx.Add(Avx.Multiply(dcx1, rx), Avx.Multiply(dcy1, rz));
        var ly1 = Fma.IsSupported ? Fma.MultiplyAdd(dcx1, fx, Avx.Multiply(dcy1, fz)) : Avx.Add(Avx.Multiply(dcx1, fx), Avx.Multiply(dcy1, fz));
        var local = Vector256.LessThanOrEqual(Avx.Min(lx0, lx1), halfWidth) & Vector256.GreaterThanOrEqual(Avx.Max(lx0, lx1), negHalfWidth)
            & Vector256.LessThanOrEqual(Avx.Min(ly0, ly1), halfLength) & Vector256.GreaterThanOrEqual(Avx.Max(ly0, ly1), negHalfLength);
        var world = Vector256.GreaterThanOrEqual(Load256(_maxX, i), worldMinX) & Vector256.LessThanOrEqual(Load256(_minX, i), worldMaxX);
        return (local & world).ExtractMostSignificantBits();
    }

    private bool BoundaryIntersectsCircle(float cx, float cz, float radius)
    {
        var radiusSq = radius * radius;
        var minX = cx - radius;
        var maxX = cx + radius;

        if (!TryGetRowRange(cz - radius, cz + radius, out var row0, out var row1))
        {
            return false;
        }

        for (var row = row0; row <= row1; ++row)
        {
            if (maxX < _rowMinX[row] || minX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelCircleIntersectsDispatch(es, ee, cx, cz, radiusSq, minX, maxX))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, cz - radius);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, cz + radius);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;
                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }

                var nearestX = Math.Min(Math.Max(cx, edgeMinX), edgeMaxX);
                var dx = nearestX - cx;
                var dy = edge.y - cz;
                if (dx * dx + dy * dy <= radiusSq)
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
            if (maxX < _rowMinX[row] || minX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelAnnulusIntersectsDispatch(es, ee, cx, cz, innerSq, outerSq, minX, maxX))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, cz - outerExpanded);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, cz + outerExpanded);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
                var edgeMinX = edge.minX;
                var edgeMaxX = edge.maxX;

                if (edgeMaxX < minX || edgeMinX > maxX)
                {
                    continue;
                }
                var edgeY = edge.y;
                if (HorizontalSegmentDistanceRangeOverlapsAnnulus(edgeMinX, edgeY, edgeMaxX, cx, cz, innerSq, outerSq))
                {
                    return true;
                }
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelCircleIntersectsDispatch(int es, int ee, float cx, float cz, float radiusSq, float queryMinX, float queryMaxX)
    {
        var count = ee - es;
        if (count >= 16 && Avx512F.IsSupported)
        {
            return KernelCircleIntersects512(es, ee, cx, cz, radiusSq, queryMinX, queryMaxX);
        }
        if (count >= 8 && Avx2.IsSupported)
        {
            return KernelCircleIntersects256(es, ee, cx, cz, radiusSq, queryMinX, queryMaxX);
        }
        return KernelCircleIntersectsScalar(es, ee, cx, cz, radiusSq, queryMinX, queryMaxX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelAnnulusIntersectsDispatch(int es, int ee, float cx, float cz, float innerSq, float outerSq, float queryMinX, float queryMaxX)
    {
        var count = ee - es;
        if (count >= 16 && Avx512F.IsSupported)
        {
            return KernelAnnulusIntersects512(es, ee, cx, cz, innerSq, outerSq, queryMinX, queryMaxX);
        }
        if (count >= 8 && Avx2.IsSupported)
        {
            return KernelAnnulusIntersects256(es, ee, cx, cz, innerSq, outerSq, queryMinX, queryMaxX);
        }
        return KernelAnnulusIntersectsScalar(es, ee, cx, cz, innerSq, outerSq, queryMinX, queryMaxX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelCircleIntersectsScalar(int es, int ee, float cx, float cz, float radiusSq, float queryMinX, float queryMaxX)
    {
        for (var i = es; i < ee; ++i)
        {
            if (_maxX[i] < queryMinX || _minX[i] > queryMaxX)
            {
                continue;
            }
            if (PointSegmentDistanceSqKnownInvNonDegenerate(cx, cz, _x0[i], _y0[i], _dx[i], _dy[i], _invLen2[i]) <= radiusSq)
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelAnnulusIntersectsScalar(int es, int ee, float cx, float cz, float innerSq, float outerSq, float queryMinX, float queryMaxX)
    {
        for (var i = es; i < ee; ++i)
        {
            if (_maxX[i] < queryMinX || _minX[i] > queryMaxX)
            {
                continue;
            }
            if (SegmentDistanceRangeOverlapsAnnulusKnownInv(_x0[i], _y0[i], _dx[i], _dy[i], _invLen2[i], cx, cz, innerSq, outerSq))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelCircleIntersects512(int es, int ee, float cx, float cz, float radiusSq, float queryMinX, float queryMaxX)
    {
        var vCx = Vector512.Create(cx);
        var vCz = Vector512.Create(cz);
        var vRadiusSq = Vector512.Create(radiusSq);
        var vMinX = Vector512.Create(queryMinX);
        var vMaxX = Vector512.Create(queryMaxX);
        var zero = Vector512<float>.Zero;
        var one = Vector512.Create(1f);
        var i = es;
        for (; i + 16 <= ee; i += 16)
        {
            var xOverlap = Vector512.GreaterThanOrEqual(Load512(_maxX, i), vMinX) & Vector512.LessThanOrEqual(Load512(_minX, i), vMaxX);
            if (xOverlap.ExtractMostSignificantBits() == 0ul)
            {
                continue;
            }

            var x0 = Load512(_x0, i);
            var y0 = Load512(_y0, i);
            var dx = Load512(_dx, i);
            var dy = Load512(_dy, i);
            var apx = Avx512F.Subtract(vCx, x0);
            var apy = Avx512F.Subtract(vCz, y0);
            var dot = Avx512F.FusedMultiplyAdd(apx, dx, Avx512F.Multiply(apy, dy));
            var t = Avx512F.Min(one, Avx512F.Max(zero, Avx512F.Multiply(dot, Load512(_invLen2, i))));
            var qx = Avx512F.FusedMultiplyAdd(t, dx, x0);
            var qy = Avx512F.FusedMultiplyAdd(t, dy, y0);
            var ddx = Avx512F.Subtract(vCx, qx);
            var ddy = Avx512F.Subtract(vCz, qy);
            var distSq = Avx512F.FusedMultiplyAdd(ddx, ddx, Avx512F.Multiply(ddy, ddy));
            if ((xOverlap & Vector512.LessThanOrEqual(distSq, vRadiusSq)).ExtractMostSignificantBits() != 0ul)
            {
                return true;
            }
        }
        return i < ee && KernelCircleIntersects256OrScalar(i, ee, cx, cz, radiusSq, queryMinX, queryMaxX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelCircleIntersects256OrScalar(int es, int ee, float cx, float cz, float radiusSq, float queryMinX, float queryMaxX)
        => Avx2.IsSupported && ee - es >= 8
            ? KernelCircleIntersects256(es, ee, cx, cz, radiusSq, queryMinX, queryMaxX)
            : KernelCircleIntersectsScalar(es, ee, cx, cz, radiusSq, queryMinX, queryMaxX);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelCircleIntersects256(int es, int ee, float cx, float cz, float radiusSq, float queryMinX, float queryMaxX)
    {
        var vCx = Vector256.Create(cx);
        var vCz = Vector256.Create(cz);
        var vRadiusSq = Vector256.Create(radiusSq);
        var vMinX = Vector256.Create(queryMinX);
        var vMaxX = Vector256.Create(queryMaxX);
        var zero = Vector256<float>.Zero;
        var one = Vector256.Create(1f);
        var i = es;
        for (; i + 8 <= ee; i += 8)
        {
            var xOverlap = Vector256.GreaterThanOrEqual(Load256(_maxX, i), vMinX) & Vector256.LessThanOrEqual(Load256(_minX, i), vMaxX);
            if (xOverlap.ExtractMostSignificantBits() == 0u)
            {
                continue;
            }

            var x0 = Load256(_x0, i);
            var y0 = Load256(_y0, i);
            var dx = Load256(_dx, i);
            var dy = Load256(_dy, i);
            var apx = Avx.Subtract(vCx, x0);
            var apy = Avx.Subtract(vCz, y0);
            var dot = Fma.IsSupported ? Fma.MultiplyAdd(apx, dx, Avx.Multiply(apy, dy)) : Avx.Add(Avx.Multiply(apx, dx), Avx.Multiply(apy, dy));
            var t = Avx.Min(one, Avx.Max(zero, Avx.Multiply(dot, Load256(_invLen2, i))));
            var qx = Fma.IsSupported ? Fma.MultiplyAdd(t, dx, x0) : Avx.Add(Avx.Multiply(t, dx), x0);
            var qy = Fma.IsSupported ? Fma.MultiplyAdd(t, dy, y0) : Avx.Add(Avx.Multiply(t, dy), y0);
            var ddx = Avx.Subtract(vCx, qx);
            var ddy = Avx.Subtract(vCz, qy);
            var distSq = Fma.IsSupported ? Fma.MultiplyAdd(ddx, ddx, Avx.Multiply(ddy, ddy)) : Avx.Add(Avx.Multiply(ddx, ddx), Avx.Multiply(ddy, ddy));
            if ((xOverlap & Vector256.LessThanOrEqual(distSq, vRadiusSq)).ExtractMostSignificantBits() != 0u)
            {
                return true;
            }
        }
        return i < ee && KernelCircleIntersectsScalar(i, ee, cx, cz, radiusSq, queryMinX, queryMaxX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelAnnulusIntersects512(int es, int ee, float cx, float cz, float innerSq, float outerSq, float queryMinX, float queryMaxX)
    {
        var vCx = Vector512.Create(cx);
        var vCz = Vector512.Create(cz);
        var vInnerSq = Vector512.Create(innerSq);
        var vOuterSq = Vector512.Create(outerSq);
        var vMinX = Vector512.Create(queryMinX);
        var vMaxX = Vector512.Create(queryMaxX);
        var zero = Vector512<float>.Zero;
        var one = Vector512.Create(1f);
        var i = es;
        for (; i + 16 <= ee; i += 16)
        {
            var xOverlap = Vector512.GreaterThanOrEqual(Load512(_maxX, i), vMinX) & Vector512.LessThanOrEqual(Load512(_minX, i), vMaxX);
            if (xOverlap.ExtractMostSignificantBits() == 0ul)
            {
                continue;
            }

            var x0 = Load512(_x0, i);
            var y0 = Load512(_y0, i);
            var dx = Load512(_dx, i);
            var dy = Load512(_dy, i);
            var apx = Avx512F.Subtract(vCx, x0);
            var apy = Avx512F.Subtract(vCz, y0);
            var dot = Avx512F.FusedMultiplyAdd(apx, dx, Avx512F.Multiply(apy, dy));
            var t = Avx512F.Min(one, Avx512F.Max(zero, Avx512F.Multiply(dot, Load512(_invLen2, i))));
            var qx = Avx512F.FusedMultiplyAdd(t, dx, x0);
            var qy = Avx512F.FusedMultiplyAdd(t, dy, y0);
            var minDx = Avx512F.Subtract(vCx, qx);
            var minDy = Avx512F.Subtract(vCz, qy);
            var minDistSq = Avx512F.FusedMultiplyAdd(minDx, minDx, Avx512F.Multiply(minDy, minDy));

            var aDistSq = Avx512F.FusedMultiplyAdd(apx, apx, Avx512F.Multiply(apy, apy));
            var bpx = Avx512F.Subtract(apx, dx);
            var bpy = Avx512F.Subtract(apy, dy);
            var bDistSq = Avx512F.FusedMultiplyAdd(bpx, bpx, Avx512F.Multiply(bpy, bpy));
            var maxDistSq = Avx512F.Max(aDistSq, bDistSq);
            var hit = xOverlap & Vector512.LessThanOrEqual(minDistSq, vOuterSq) & Vector512.GreaterThanOrEqual(maxDistSq, vInnerSq);
            if (hit.ExtractMostSignificantBits() != 0ul)
            {
                return true;
            }
        }
        return i < ee && KernelAnnulusIntersects256OrScalar(i, ee, cx, cz, innerSq, outerSq, queryMinX, queryMaxX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelAnnulusIntersects256OrScalar(int es, int ee, float cx, float cz, float innerSq, float outerSq, float queryMinX, float queryMaxX)
        => Avx2.IsSupported && ee - es >= 8
            ? KernelAnnulusIntersects256(es, ee, cx, cz, innerSq, outerSq, queryMinX, queryMaxX)
            : KernelAnnulusIntersectsScalar(es, ee, cx, cz, innerSq, outerSq, queryMinX, queryMaxX);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool KernelAnnulusIntersects256(int es, int ee, float cx, float cz, float innerSq, float outerSq, float queryMinX, float queryMaxX)
    {
        var vCx = Vector256.Create(cx);
        var vCz = Vector256.Create(cz);
        var vInnerSq = Vector256.Create(innerSq);
        var vOuterSq = Vector256.Create(outerSq);
        var vMinX = Vector256.Create(queryMinX);
        var vMaxX = Vector256.Create(queryMaxX);
        var zero = Vector256<float>.Zero;
        var one = Vector256.Create(1f);
        var i = es;
        for (; i + 8 <= ee; i += 8)
        {
            var xOverlap = Vector256.GreaterThanOrEqual(Load256(_maxX, i), vMinX) & Vector256.LessThanOrEqual(Load256(_minX, i), vMaxX);
            if (xOverlap.ExtractMostSignificantBits() == 0u)
            {
                continue;
            }

            var x0 = Load256(_x0, i);
            var y0 = Load256(_y0, i);
            var dx = Load256(_dx, i);
            var dy = Load256(_dy, i);
            var apx = Avx.Subtract(vCx, x0);
            var apy = Avx.Subtract(vCz, y0);
            var dot = Fma.IsSupported ? Fma.MultiplyAdd(apx, dx, Avx.Multiply(apy, dy)) : Avx.Add(Avx.Multiply(apx, dx), Avx.Multiply(apy, dy));
            var t = Avx.Min(one, Avx.Max(zero, Avx.Multiply(dot, Load256(_invLen2, i))));
            var qx = Fma.IsSupported ? Fma.MultiplyAdd(t, dx, x0) : Avx.Add(Avx.Multiply(t, dx), x0);
            var qy = Fma.IsSupported ? Fma.MultiplyAdd(t, dy, y0) : Avx.Add(Avx.Multiply(t, dy), y0);
            var minDx = Avx.Subtract(vCx, qx);
            var minDy = Avx.Subtract(vCz, qy);
            var minDistSq = Fma.IsSupported ? Fma.MultiplyAdd(minDx, minDx, Avx.Multiply(minDy, minDy)) : Avx.Add(Avx.Multiply(minDx, minDx), Avx.Multiply(minDy, minDy));

            var aDistSq = Fma.IsSupported ? Fma.MultiplyAdd(apx, apx, Avx.Multiply(apy, apy)) : Avx.Add(Avx.Multiply(apx, apx), Avx.Multiply(apy, apy));
            var bpx = Avx.Subtract(apx, dx);
            var bpy = Avx.Subtract(apy, dy);
            var bDistSq = Fma.IsSupported ? Fma.MultiplyAdd(bpx, bpx, Avx.Multiply(bpy, bpy)) : Avx.Add(Avx.Multiply(bpx, bpx), Avx.Multiply(bpy, bpy));
            var maxDistSq = Avx.Max(aDistSq, bDistSq);
            var hit = xOverlap & Vector256.LessThanOrEqual(minDistSq, vOuterSq) & Vector256.GreaterThanOrEqual(maxDistSq, vInnerSq);
            if (hit.ExtractMostSignificantBits() != 0u)
            {
                return true;
            }
        }
        return i < ee && KernelAnnulusIntersectsScalar(i, ee, cx, cz, innerSq, outerSq, queryMinX, queryMaxX);
    }

    private bool BoundaryIntersectsCone(float ox, float oz, float fx, float fz, float radius, float halfAngle)
    {
        var sector = new SectorGeometry(ox, oz, fx, fz, radius, halfAngle);
        var minX = sector.minX;
        var maxX = sector.maxX;
        var maxZ = sector.maxZ;

        if (!TryGetRowRange(sector.minZ, maxZ, out var row0, out var row1))
        {
            return false;
        }
        var minZ = sector.minZ;
        for (var row = row0; row <= row1; ++row)
        {
            if (maxX < _rowMinX[row] || minX > _rowMaxX[row])
            {
                continue;
            }

            int es = row == row0 ? _rowOffsets[row] : _rowNewStarts[row], ee = _rowEnds[row];
            if (KernelConeIntersectsDispatch(es, ee, sector))
            {
                return true;
            }

            int hs = _hRowOffsets[row], he = _hRowOffsets[row + 1];
            if (row == row0)
            {
                hs = FirstHorizontalAtOrAbove(hs, he, minZ);
            }
            if (row == row1)
            {
                he = FirstHorizontalAbove(hs, he, maxZ);
            }
            for (var h = hs; h < he; ++h)
            {
                ref readonly var edge = ref _hEdges[h];
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
    private static float PointHorizontalSegmentDistanceSq(float px, float py, float minX, float y, float maxX)
    {
        var nearestX = Math.Min(Math.Max(px, minX), maxX);
        var dx = nearestX - px;
        var dy = y - py;
        return dx * dx + dy * dy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HorizontalSegmentDistanceRangeOverlapsAnnulus(float minX, float y, float maxX, float cx, float cz, float innerSq, float outerSq)
    {
        var dx0 = minX - cx;
        var dx1 = maxX - cx;
        var dy = y - cz;
        var dySq = dy * dy;
        var maxSq = Math.Max(dx0 * dx0, dx1 * dx1) + dySq;
        if (maxSq < innerSq)
        {
            return false;
        }

        var nearestX = Math.Min(Math.Max(cx, minX), maxX);
        var nearestDx = nearestX - cx;
        return nearestDx * nearestDx + dySq <= outerSq;
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
    private static bool SegmentDistanceRangeOverlapsAnnulusKnownInv(float ax, float ay, float dx, float dy, float invLenSq, float cx, float cz, float innerSq, float outerSq)
    {
        var daX = ax - cx;
        var daY = ay - cz;
        var dbX = daX + dx;
        var dbY = daY + dy;
        var maxSq = Math.Max(daX * daX + daY * daY, dbX * dbX + dbY * dbY);
        if (maxSq < innerSq)
        {
            return false;
        }

        var minSq = PointSegmentDistanceSqKnownInvNonDegenerate(cx, cz, ax, ay, dx, dy, invLenSq);
        return minSq <= outerSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SegmentIntersectsSector(float ax, float ay, float bx, float by, in SectorGeometry sector)
    {
        if (PointInSector(ax, ay, sector) || PointInSector(bx, by, sector))
        {
            return true;
        }
        var sOX = sector.ox;
        var sOZ = sector.oz;
        if (SegmentsIntersect(ax, ay, bx, by, sOX, sOZ, sector.leftX, sector.leftZ)
         || SegmentsIntersect(ax, ay, bx, by, sOX, sOZ, sector.rightX, sector.rightZ))
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

        // projection >= sqrt(distanceSq) * cosHalfAngle - Eps, handle obtuse sectors separately because their cosine is
        // negative and blindly squaring would reverse part of the predicate
        var adjustedProjection = dx * fx + dy * fz + Eps;
        var angularLimitSq = distanceSq * cosHalfAngle * cosHalfAngle;
        if (cosHalfAngle >= 0f)
        {
            return adjustedProjection >= 0f && adjustedProjection * adjustedProjection >= angularLimitSq;
        }

        return adjustedProjection >= 0f || adjustedProjection * adjustedProjection <= angularLimitSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SegmentBoundsOverlap(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy)
        => Math.Max(ax, bx) + Eps >= Math.Min(cx, dx) && Math.Max(cx, dx) + Eps >= Math.Min(ax, bx)
        && Math.Max(ay, by) + Eps >= Math.Min(cy, dy) && Math.Max(cy, dy) + Eps >= Math.Min(ay, by);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SegmentsProperlyIntersect(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy)
    {
        var abX = bx - ax;
        var abY = by - ay;
        var cdX = dx - cx;
        var cdY = dy - cy;
        var c1 = Cross(abX, abY, cx - ax, cy - ay);
        var c2 = Cross(abX, abY, dx - ax, dy - ay);
        var c3 = Cross(cdX, cdY, ax - cx, ay - cy);
        var c4 = Cross(cdX, cdY, bx - cx, by - cy);
        return (c1 > 0f && c2 < 0f || c1 < 0f && c2 > 0f) && (c3 > 0f && c4 < 0f || c3 < 0f && c4 > 0f);
    }

    private static bool SegmentsIntersect(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy)
    {
        if (!SegmentBoundsOverlap(ax, ay, bx, by, cx, cy, dx, dy))
        {
            return false;
        }
        if (SegmentsProperlyIntersect(ax, ay, bx, by, cx, cy, dx, dy))
        {
            return true;
        }

        return PointWithinDistanceSqOfSegment(ax, ay, cx, cy, dx, dy, Eps2) || PointWithinDistanceSqOfSegment(bx, by, cx, cy, dx, dy, Eps2)
            || PointWithinDistanceSqOfSegment(cx, cy, ax, ay, bx, by, Eps2) || PointWithinDistanceSqOfSegment(dx, dy, ax, ay, bx, by, Eps2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float PointSegmentDistanceSq(float px, float py, float ax, float ay, float bx, float by)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var rx = px - ax;
        var ry = py - ay;
        var lenSq = dx * dx + dy * dy;
        if (lenSq <= TinyLen2)
        {
            return rx * rx + ry * ry;
        }

        var dot = rx * dx + ry * dy;
        if (dot <= 0f)
        {
            return rx * rx + ry * ry;
        }
        if (dot >= lenSq)
        {
            rx -= dx;
            ry -= dy;
            return rx * rx + ry * ry;
        }

        var cross = rx * dy - ry * dx;
        return cross * cross / lenSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointWithinDistanceSqOfSegment(float px, float py, float ax, float ay, float bx, float by, float maxDistanceSq)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var rx = px - ax;
        var ry = py - ay;
        var lenSq = dx * dx + dy * dy;
        if (lenSq <= TinyLen2)
        {
            return rx * rx + ry * ry <= maxDistanceSq;
        }

        var dot = rx * dx + ry * dy;
        if (dot <= 0f)
        {
            return rx * rx + ry * ry <= maxDistanceSq;
        }
        if (dot >= lenSq)
        {
            rx -= dx;
            ry -= dy;
            return rx * rx + ry * ry <= maxDistanceSq;
        }

        var cross = rx * dy - ry * dx;
        return cross * cross <= maxDistanceSq * lenSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float SegmentSegmentDistanceSqKnownInvNonDegenerate(float ax, float ay, float adx, float ady, float aInvLenSq,
        float bx, float by, float bdx, float bdy, float bInvLenSq)
    {
        var aex = ax + adx;
        var aey = ay + ady;
        var bex = bx + bdx;
        var bey = by + bdy;
        if (SegmentBoundsOverlap(ax, ay, aex, aey, bx, by, bex, bey)
            && SegmentsProperlyIntersect(ax, ay, aex, aey, bx, by, bex, bey))
        {
            return 0f;
        }

        var distanceSq = Math.Min(
            Math.Min(PointSegmentDistanceSqKnownInvNonDegenerate(ax, ay, bx, by, bdx, bdy, bInvLenSq), PointSegmentDistanceSqKnownInvNonDegenerate(aex, aey, bx, by, bdx, bdy, bInvLenSq)),
            Math.Min(PointSegmentDistanceSqKnownInvNonDegenerate(bx, by, ax, ay, adx, ady, aInvLenSq), PointSegmentDistanceSqKnownInvNonDegenerate(bex, bey, ax, ay, adx, ady, aInvLenSq)));
        return distanceSq <= Eps2 ? 0f : distanceSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float SegmentSegmentDistanceSqKnownInv(float ax, float ay, float adx, float ady, float aInvLenSq,
        float bx, float by, float bdx, float bdy, float bInvLenSq)
    {
        var aex = ax + adx;
        var aey = ay + ady;
        var bex = bx + bdx;
        var bey = by + bdy;
        if (SegmentBoundsOverlap(ax, ay, aex, aey, bx, by, bex, bey)
            && SegmentsProperlyIntersect(ax, ay, aex, aey, bx, by, bex, bey))
        {
            return 0f;
        }

        var distanceSq = Math.Min(
            Math.Min(PointSegmentDistanceSqKnownInv(ax, ay, bx, by, bdx, bdy, bInvLenSq), PointSegmentDistanceSqKnownInv(aex, aey, bx, by, bdx, bdy, bInvLenSq)),
            Math.Min(PointSegmentDistanceSqKnownInv(bx, by, ax, ay, adx, ady, aInvLenSq), PointSegmentDistanceSqKnownInv(bex, bey, ax, ay, adx, ady, aInvLenSq)));
        return distanceSq <= Eps2 ? 0f : distanceSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Rotate(float x, float z, float angle, out float rotatedX, out float rotatedZ)
    {
        var (sin, cos) = MathF.SinCos(angle);
        rotatedX = x * cos - z * sin;
        rotatedZ = x * sin + z * cos;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Cross(float ax, float ay, float bx, float by) => ax * by - ay * bx;
}
