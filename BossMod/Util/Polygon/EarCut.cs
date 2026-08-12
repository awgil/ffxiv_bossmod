using System.Buffers;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BossMod;

[SkipLocalsInit]
internal sealed class EarCut
{
    // Earcut-style triangulation. The hot path is allocation-free apart from the returned array:
    // clean simplified contours are consumed in-place and topology lives in a contiguous index arena.
    private const float Eps = 1e-7f;
    private const float Eps2 = Eps * Eps;

    public static RelTriangle[] Triangulate(RelSimplifiedComplexPolygon polygon)
    {
        var parts = CollectionsMarshal.AsSpan(polygon.Parts);
        var estimatedTriangles = 0;
        var lenP = parts.Length;
        for (var i = 0; i < lenP; ++i)
        {
            var part = parts[i];
            estimatedTriangles += Math.Max(0, part.Vertices.Count + part.HoleStarts.Count * 2 - 2);
        }

        if (estimatedTriangles == 0)
        {
            return [];
        }

        var result = GC.AllocateUninitializedArray<RelTriangle>(estimatedTriangles);
        var writer = new TriangulationWriter(result);
        for (var partIndex = 0; partIndex < lenP; ++partIndex)
        {
            var part = parts[partIndex];
            var outerLength = part.Exterior.Length;
            if (outerLength < 3)
            {
                continue;
            }

            var vertices = CollectionsMarshal.AsSpan(part.Vertices);
            var holeStarts = CollectionsMarshal.AsSpan(part.HoleStarts);

            // Most arena geometry is tiny or convex. Bypass every topology allocation, Morton setup, and ear search for triangles, simple quads, and clean convex exteriors
            if (holeStarts.Length == 0 && TryTriangulateSmallOrConvex(vertices[..outerLength], ref writer))
            {
                continue;
            }

            // RelSimplifiedComplexPolygon is already flattened as exterior followed by holes. The linked-list filter handles repeated, duplicate, collinear, and degenerate vertices, so
            // copying contours to a second buffer is both redundant and slower
            Earcut(vertices, outerLength, holeStarts, ref writer);
        }
        var countW = writer.Count;
        if (countW == 0)
        {
            return [];
        }
        return countW == result.Length ? result : result.AsSpan(0, countW).ToArray();
    }

    private const int NoTriangulationNode = -1;
    private const int StackTriangulationNodes = 256;
    private const int StackTriangulationScratchWords = 1024; // keep dynamic stack use below roughly 14 KiB
    private const int StackTriangulationHoles = 64;

    // 24 bytes. Morton ordering is stored in contiguous scratch buffers instead of topology links, so three fields disappear compared with the linked-Z implementation
    private struct TriangulationNode
    {
        public int SourceIndex;
        public int Prev;
        public int Next;
        public int MortonSlot;
        public float X;
        public float Y;
    }

    private ref struct TriangulationArena
    {
        private ref TriangulationNode _base;
        private ref int _mortonAliveBase;
        private readonly int _mortonCapacity;
        private readonly int _capacity;
        public int Count;
        public bool Overflowed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TriangulationArena(Span<TriangulationNode> nodes, Span<int> mortonAlive)
        {
            _base = ref MemoryMarshal.GetReference(nodes);
            _mortonAliveBase = ref MemoryMarshal.GetReference(mortonAlive);
            _mortonCapacity = mortonAlive.Length;
            _capacity = nodes.Length;
            Count = 0;
            Overflowed = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TriangulationNode Node(int index) => ref Unsafe.Add(ref _base, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReserve(int count)
        {
            if (Count <= _capacity - count)
            {
                return true;
            }
            Overflowed = true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(int sourceIndex, float x, float y)
        {
            var index = Count++;
            ref var node = ref Node(index);
            node.SourceIndex = sourceIndex;
            node.Prev = NoTriangulationNode;
            node.Next = NoTriangulationNode;
            node.MortonSlot = NoTriangulationNode;
            node.X = x;
            node.Y = y;
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeactivateMorton(ref TriangulationNode node)
        {
            var slot = node.MortonSlot;
            if ((uint)slot < (uint)_mortonCapacity)
            {
                Unsafe.Add(ref _mortonAliveBase, slot) = 0;
                node.MortonSlot = NoTriangulationNode;
            }
        }
    }

    // Dynamic acceleration structure used only while holes are bridged into the exterior. Directed downward edges are stored once in a centered Y interval tree, so a horizontal
    // bridge ray visits only interval lists on one root-to-leaf path. Active exterior vertices are stored in uniform X buckets for the visibility refinement phase. Stale records are
    // intentionally left in place and rejected through current Prev/Next links; bridge insertion and point filtering only append replacement records
    private ref struct TriangulationBridgeIndex
    {
        private readonly Span<int> _yHeads;
        private readonly Span<int> _xHeads;
        private readonly Span<int> _edgeFrom;
        private readonly Span<int> _edgeTo;
        private readonly Span<int> _edgeNext;
        private readonly Span<int> _vertexNode;
        private readonly Span<int> _vertexNext;
        private int _edgeCount;
        private int _vertexCount;
        private readonly int _yBuckets;
        private readonly int _xBuckets;
        private readonly float _minX;
        private readonly float _minY;
        private readonly float _xScale;
        private readonly float _yScale;
        private bool _valid;

        public readonly bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _valid && !_edgeFrom.IsEmpty;
        }

        public static int ChooseBucketCount(int vertexCount)
        {
            var target = Math.Clamp((int)MathF.Ceiling(MathF.Sqrt(Math.Max(vertexCount, 1)) * 2f), 16, 256);
            var buckets = 16;
            while (buckets < target)
            {
                buckets <<= 1;
            }
            return buckets;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RequiredWords(int edgeCapacity, int vertexCapacity, int yBuckets, int xBuckets)
            => 2 * yBuckets + xBuckets + 3 * edgeCapacity + 2 * vertexCapacity;

        public TriangulationBridgeIndex(Span<int> storage, int edgeCapacity, int vertexCapacity, int yBuckets, int xBuckets,
            float minX, float minY, float maxX, float maxY)
        {
            var offset = 0;
            _yHeads = storage.Slice(offset, 2 * yBuckets);
            offset += _yHeads.Length;
            _xHeads = storage.Slice(offset, xBuckets);
            offset += _xHeads.Length;
            _edgeFrom = storage.Slice(offset, edgeCapacity);
            offset += edgeCapacity;
            _edgeTo = storage.Slice(offset, edgeCapacity);
            offset += edgeCapacity;
            _edgeNext = storage.Slice(offset, edgeCapacity);
            offset += edgeCapacity;
            _vertexNode = storage.Slice(offset, vertexCapacity);
            offset += vertexCapacity;
            _vertexNext = storage.Slice(offset, vertexCapacity);

            _yHeads.Fill(NoTriangulationNode);
            _xHeads.Fill(NoTriangulationNode);
            _edgeCount = 0;
            _vertexCount = 0;
            _yBuckets = yBuckets;
            _xBuckets = xBuckets;
            _minX = minX;
            _minY = minY;
            _xScale = maxX - minX > Eps ? (xBuckets - 1) / (maxX - minX) : 0f;
            _yScale = maxY - minY > Eps ? (yBuckets - 1) / (maxY - minY) : 0f;
            _valid = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int XBucket(float x) => Math.Clamp((int)((x - _minX) * _xScale), 0, _xBuckets - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int YBucket(float y) => Math.Clamp((int)((y - _minY) * _yScale), 0, _yBuckets - 1);

        public void AddRing(int start, ref TriangulationArena arena)
        {
            if (!Enabled || start == NoTriangulationNode)
            {
                return;
            }

            var point = start;
            do
            {
                var next = arena.Node(point).Next;
                AddVertex(point, ref arena);
                AddEdge(point, next, ref arena);
                point = next;
            }
            while (point != start && Enabled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVertex(int nodeIndex, ref TriangulationArena arena)
        {
            if (!Enabled)
            {
                return;
            }
            var vertexCount = _vertexCount;
            if ((uint)vertexCount >= (uint)_vertexNode.Length)
            {
                _valid = false;
                return;
            }
            ref var node = ref arena.Node(nodeIndex);
            var bucket = XBucket(node.X);
            ref var vertexNode = ref MemoryMarshal.GetReference(_vertexNode);
            ref var vertexNext = ref MemoryMarshal.GetReference(_vertexNext);
            ref var xHeads = ref MemoryMarshal.GetReference(_xHeads);
            Unsafe.Add(ref vertexNode, vertexCount) = nodeIndex;
            Unsafe.Add(ref vertexNext, vertexCount) = Unsafe.Add(ref xHeads, bucket);
            Unsafe.Add(ref xHeads, bucket) = vertexCount;
            _vertexCount = vertexCount + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddEdge(int from, int to, ref TriangulationArena arena)
        {
            if (!Enabled)
            {
                return;
            }

            ref var a = ref arena.Node(from);
            ref var b = ref arena.Node(to);
            if (a.Y - b.Y <= Eps)
            {
                return; // Earcut's ray crossing rule only admits downward, non-horizontal edges.
            }
            if ((uint)_edgeCount >= (uint)_edgeFrom.Length)
            {
                _valid = false;
                return;
            }

            var lowBucket = YBucket(b.Y);
            var highBucket = YBucket(a.Y);
            var treeNode = 1;
            var lo = 0;
            var hi = _yBuckets - 1;
            while (lo < hi)
            {
                var mid = (lo + hi) >> 1;
                if (highBucket <= mid)
                {
                    treeNode <<= 1;
                    hi = mid;
                }
                else if (lowBucket > mid)
                {
                    treeNode = treeNode << 1 | 1;
                    lo = mid + 1;
                }
                else
                {
                    break;
                }
            }

            var edgeCount = _edgeCount;
            ref var edgeFrom = ref MemoryMarshal.GetReference(_edgeFrom);
            ref var edgeTo = ref MemoryMarshal.GetReference(_edgeTo);
            ref var edgeNext = ref MemoryMarshal.GetReference(_edgeNext);
            ref var yHeads = ref MemoryMarshal.GetReference(_yHeads);
            Unsafe.Add(ref edgeFrom, edgeCount) = from;
            Unsafe.Add(ref edgeTo, edgeCount) = to;
            Unsafe.Add(ref edgeNext, edgeCount) = Unsafe.Add(ref yHeads, treeNode);
            Unsafe.Add(ref yHeads, treeNode) = edgeCount;
            _edgeCount = edgeCount + 1;
        }

        // Returns false only when equal-X candidates make original ring order observable; the
        // caller then takes the exact scalar path. A unique endpoint hit is returned directly.
        public readonly bool TryFindRayBridge(int hole, ref TriangulationArena arena, out int bridge, out float qx, out bool exactHit)
        {
            bridge = NoTriangulationNode;
            qx = float.NegativeInfinity;
            exactHit = false;
            if (!Enabled)
            {
                return false;
            }

            ref var holeNode = ref arena.Node(hole);
            var hx = holeNode.X;
            var hy = holeNode.Y;
            var bucket = YBucket(hy);
            var treeNode = 1;
            var lo = 0;
            var hi = _yBuckets - 1;
            var exactNode = NoTriangulationNode;
            var ambiguous = false;

            ref var yHeads = ref MemoryMarshal.GetReference(_yHeads);
            ref var edgeFrom = ref MemoryMarshal.GetReference(_edgeFrom);
            ref var edgeTo = ref MemoryMarshal.GetReference(_edgeTo);
            ref var edgeNext = ref MemoryMarshal.GetReference(_edgeNext);

            while (true)
            {
                for (var entry = Unsafe.Add(ref yHeads, treeNode); entry != NoTriangulationNode; entry = Unsafe.Add(ref edgeNext, entry))
                {
                    var from = Unsafe.Add(ref edgeFrom, entry);
                    var to = Unsafe.Add(ref edgeTo, entry);
                    ref var a = ref arena.Node(from);
                    ref var b = ref arena.Node(to);
                    var aY = a.Y;
                    var bY = b.Y;
                    if (a.Next != to || b.Prev != from || hy > aY || hy < bY || aY - bY <= Eps)
                    {
                        continue; // stale topology record or quantization false positive
                    }
                    var aX = a.X;
                    var bX = b.X;
                    var x = aX + (hy - aY) * (bX - aX) / (bY - aY);
                    if (x > hx + Eps)
                    {
                        continue;
                    }

                    if (Math.Abs(x - hx) <= Eps)
                    {
                        var candidate = Math.Abs(hy - aY) <= Eps ? from : Math.Abs(hy - bY) <= Eps ? to : NoTriangulationNode;
                        if (candidate != NoTriangulationNode)
                        {
                            if (exactNode == NoTriangulationNode)
                            {
                                exactNode = candidate;
                            }
                            else if (exactNode != candidate)
                            {
                                ambiguous = true;
                            }
                        }
                    }

                    if (x > qx)
                    {
                        qx = x;
                        bridge = aX < bX ? from : to;
                    }
                    else if (x == qx)
                    {
                        var candidate = aX < bX ? from : to;
                        if (candidate != bridge)
                        {
                            ambiguous = true;
                        }
                    }
                }

                if (lo == hi)
                {
                    break;
                }
                var mid = (lo + hi) >> 1;
                if (bucket <= mid)
                {
                    treeNode <<= 1;
                    hi = mid;
                }
                else
                {
                    treeNode = treeNode << 1 | 1;
                    lo = mid + 1;
                }
            }

            if (ambiguous)
            {
                return false;
            }
            if (exactNode != NoTriangulationNode)
            {
                bridge = exactNode;
                qx = hx;
                exactHit = true;
            }
            return true;
        }

        // Returns false when candidates have effectively equal tangents. That rare case falls back to ring-order traversal so output remains byte-for-byte compatible with the scalar rule
        public readonly bool TryRefineBridge(int hole, int initialBridge, float qx, ref TriangulationArena arena, out int bridge)
        {
            bridge = initialBridge;
            if (!Enabled)
            {
                return false;
            }

            ref var holeNode = ref arena.Node(hole);
            var hx = holeNode.X;
            var hy = holeNode.Y;
            ref var initial = ref arena.Node(initialBridge);
            var mx = initial.X;
            var my = initial.Y;
            var firstBucket = XBucket(mx);
            var lastBucket = XBucket(hx);
            var tanMin = float.PositiveInfinity;

            ref var xHeads = ref MemoryMarshal.GetReference(_xHeads);
            ref var vertexNode = ref MemoryMarshal.GetReference(_vertexNode);
            ref var vertexNext = ref MemoryMarshal.GetReference(_vertexNext);

            for (var bucket = firstBucket; bucket <= lastBucket; ++bucket)
            {
                for (var entry = Unsafe.Add(ref xHeads, bucket); entry != NoTriangulationNode; entry = Unsafe.Add(ref vertexNext, entry))
                {
                    var point = Unsafe.Add(ref vertexNode, entry);
                    if (point == initialBridge || !IsTriangulationNodeLinked(point, ref arena))
                    {
                        continue;
                    }

                    ref var node = ref arena.Node(point);
                    var nodeX = node.X;
                    if (hx < nodeX || nodeX < mx || Math.Abs(hx - nodeX) <= Eps || !PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, nodeX, node.Y)
                    || !LocallyInsideTriangulation(point, hole, ref arena))
                    {
                        continue;
                    }

                    var tan = Math.Abs(hy - node.Y) / (hx - node.X);
                    if (float.IsFinite(tanMin) && Math.Abs(tan - tanMin) <= Eps)
                    {
                        return false;
                    }
                    if (tan < tanMin)
                    {
                        bridge = point;
                        tanMin = tan;
                    }
                }
            }
            return true;
        }
    }

    // One 20-byte scratch record per possible node, laid out as SoA:
    // 8-byte sort key/node id + X + Y + active mask. The sorted coordinates make the blocker test streamable and allow AVX2/AVX-512 loads without gathers
    private ref struct TriangulationMortonIndex
    {
        private ref ulong _keys;
        private ref ulong _tempKeys;
        private ref float _x;
        private ref float _y;
        private ref int _alive;
        private int _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TriangulationMortonIndex(Span<ulong> keys, Span<ulong> tempKeys, Span<float> x, Span<float> y, Span<int> alive)
        {
            _keys = ref MemoryMarshal.GetReference(keys);
            _tempKeys = ref MemoryMarshal.GetReference(tempKeys);
            _x = ref MemoryMarshal.GetReference(x);
            _y = ref MemoryMarshal.GetReference(y);
            _alive = ref MemoryMarshal.GetReference(alive);
            _count = 0;
        }

        public void Build(int start, float minX, float minY, float invSize, ref TriangulationArena arena)
        {
            var count = 0;
            var point = start;
            do
            {
                ref var node = ref arena.Node(point);
                var z = TriangulationZOrder(node.X, node.Y, minX, minY, invSize);
                Unsafe.Add(ref _keys, count++) = ((ulong)(uint)z << 32) | (uint)point;
                point = node.Next;
            }
            while (point != start);

            var keys = MemoryMarshal.CreateSpan(ref _keys, count);
            if (count >= 256)
            {
                RadixSortTriangulationMortonKeys(keys, MemoryMarshal.CreateSpan(ref _tempKeys, count));
            }
            else
            {
                keys.Sort();
            }
            for (var i = 0; i < count; ++i)
            {
                var key = Unsafe.Add(ref _keys, i);
                var nodeIndex = (int)(uint)key;
                ref var node = ref arena.Node(nodeIndex);
                Unsafe.Add(ref _x, i) = node.X;
                Unsafe.Add(ref _y, i) = node.Y;
                Unsafe.Add(ref _alive, i) = -1;
                node.MortonSlot = i;
            }
            _count = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int LowerBound(int z)
        {
            var key = (ulong)(uint)z << 32;
            var lo = 0;
            var hi = _count;
            while (lo < hi)
            {
                var mid = (int)(((uint)lo + (uint)hi) >> 1);
                if (Unsafe.Add(ref _keys, mid) < key)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }
            return lo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int UpperBound(int z, int lo)
        {
            var key = ((ulong)(uint)z << 32) | uint.MaxValue;
            var hi = _count;
            while (lo < hi)
            {
                var mid = (int)(((uint)lo + (uint)hi) >> 1);
                if (Unsafe.Add(ref _keys, mid) <= key)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }
            return lo;
        }

        public bool HasBlocker(int minZ, int maxZ, float minTX, float minTY, float maxTX, float maxTY, int a, int b, int c, float ax, float ay, float bx, float by, float cx, float cy, ref TriangulationArena arena)
        {
            var start = LowerBound(minZ);
            var end = UpperBound(maxZ, start);
            if (start >= end)
            {
                return false;
            }

            var scalarStart = start;
            if (Avx512F.IsSupported && end - scalarStart >= 16)
            {
                var blockEnd = scalarStart + ((end - scalarStart) & -16);
                if (HasBlocker512(scalarStart, blockEnd, minTX, minTY, maxTX, maxTY, a, b, c, ax, ay, bx, by, cx, cy, ref arena))
                {
                    return true;
                }
                scalarStart = blockEnd;
            }
            if (Avx2.IsSupported && end - scalarStart >= 8)
            {
                var blockEnd = scalarStart + ((end - scalarStart) & -8);
                if (HasBlocker256(scalarStart, blockEnd, minTX, minTY, maxTX, maxTY, a, b, c, ax, ay, bx, by, cx, cy, ref arena))
                {
                    return true;
                }
                scalarStart = blockEnd;
            }

            for (var i = scalarStart; i < end; ++i)
            {
                if (Unsafe.Add(ref _alive, i) == 0)
                {
                    continue;
                }
                var point = (int)(uint)Unsafe.Add(ref _keys, i);
                if (point == a || point == b || point == c)
                {
                    continue;
                }
                var px = Unsafe.Add(ref _x, i);
                var py = Unsafe.Add(ref _y, i);
                if (px >= minTX && px <= maxTX && py >= minTY && py <= maxTY
                    && PointInTriangulationEarCCW(ax, ay, bx, by, cx, cy, px, py)
                    && IsTriangulationReflexOrFlat(point, ref arena))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasBlocker512(int start, int end, float minTX, float minTY, float maxTX, float maxTY,
            int a, int b, int c, float ax, float ay, float bx, float by, float cx, float cy, ref TriangulationArena arena)
        {
            var vMinX = Vector512.Create(minTX);
            var vMinY = Vector512.Create(minTY);
            var vMaxX = Vector512.Create(maxTX);
            var vMaxY = Vector512.Create(maxTY);
            var vNegEps = Vector512.Create(-Eps);
            var vAx = Vector512.Create(ax);
            var vAy = Vector512.Create(ay);
            var vBx = Vector512.Create(bx);
            var vBy = Vector512.Create(by);
            var vCx = Vector512.Create(cx);
            var vCy = Vector512.Create(cy);
            var vAbX = Vector512.Create(bx - ax);
            var vAbY = Vector512.Create(by - ay);
            var vBcX = Vector512.Create(cx - bx);
            var vBcY = Vector512.Create(cy - by);
            var vCaX = Vector512.Create(ax - cx);
            var vCaY = Vector512.Create(ay - cy);
            var vAlive = Vector512.Create(-1);

            var i = start;
            for (; i + 16 <= end; i += 16)
            {
                var px = Vector512.LoadUnsafe(ref _x, (nuint)i);
                var py = Vector512.LoadUnsafe(ref _y, (nuint)i);
                var mask = Vector512.Equals(Vector512.LoadUnsafe(ref _alive, (nuint)i), vAlive).AsSingle()
                    & Vector512.GreaterThanOrEqual(px, vMinX) & Vector512.LessThanOrEqual(px, vMaxX)
                    & Vector512.GreaterThanOrEqual(py, vMinY) & Vector512.LessThanOrEqual(py, vMaxY);
                var ab = vAbX * (py - vAy) - vAbY * (px - vAx);
                var bc = vBcX * (py - vBy) - vBcY * (px - vBx);
                var ca = vCaX * (py - vCy) - vCaY * (px - vCx);
                mask &= Vector512.GreaterThanOrEqual(ab, vNegEps) & Vector512.GreaterThanOrEqual(bc, vNegEps) & Vector512.GreaterThanOrEqual(ca, vNegEps);

                var bits = mask.ExtractMostSignificantBits();
                while (bits != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(bits);
                    bits &= bits - 1;
                    var point = (int)(uint)Unsafe.Add(ref _keys, i + lane);
                    if (point != a && point != b && point != c && IsTriangulationReflexOrFlat(point, ref arena))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasBlocker256(int start, int end, float minTX, float minTY, float maxTX, float maxTY,
            int a, int b, int c, float ax, float ay, float bx, float by, float cx, float cy, ref TriangulationArena arena)
        {
            var vMinX = Vector256.Create(minTX);
            var vMinY = Vector256.Create(minTY);
            var vMaxX = Vector256.Create(maxTX);
            var vMaxY = Vector256.Create(maxTY);
            var vNegEps = Vector256.Create(-Eps);
            var vAx = Vector256.Create(ax);
            var vAy = Vector256.Create(ay);
            var vBx = Vector256.Create(bx);
            var vBy = Vector256.Create(by);
            var vCx = Vector256.Create(cx);
            var vCy = Vector256.Create(cy);
            var vAbX = Vector256.Create(bx - ax);
            var vAbY = Vector256.Create(by - ay);
            var vBcX = Vector256.Create(cx - bx);
            var vBcY = Vector256.Create(cy - by);
            var vCaX = Vector256.Create(ax - cx);
            var vCaY = Vector256.Create(ay - cy);
            var vAlive = Vector256.Create(-1);

            var i = start;
            for (; i + 8 <= end; i += 8)
            {
                var px = Vector256.LoadUnsafe(ref _x, (nuint)i);
                var py = Vector256.LoadUnsafe(ref _y, (nuint)i);
                var mask = Vector256.Equals(Vector256.LoadUnsafe(ref _alive, (nuint)i), vAlive).AsSingle()
                    & Vector256.GreaterThanOrEqual(px, vMinX) & Vector256.LessThanOrEqual(px, vMaxX)
                    & Vector256.GreaterThanOrEqual(py, vMinY) & Vector256.LessThanOrEqual(py, vMaxY);
                var ab = vAbX * (py - vAy) - vAbY * (px - vAx);
                var bc = vBcX * (py - vBy) - vBcY * (px - vBx);
                var ca = vCaX * (py - vCy) - vCaY * (px - vCx);
                mask &= Vector256.GreaterThanOrEqual(ab, vNegEps) & Vector256.GreaterThanOrEqual(bc, vNegEps) & Vector256.GreaterThanOrEqual(ca, vNegEps);

                var bits = mask.ExtractMostSignificantBits();
                while (bits != 0ul)
                {
                    var lane = BitOperations.TrailingZeroCount(bits);
                    bits &= bits - 1;
                    var point = (int)(uint)Unsafe.Add(ref _keys, i + lane);
                    if (point != a && point != b && point != c && IsTriangulationReflexOrFlat(point, ref arena))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private struct TriangulationWriter(RelTriangle[] triangles)
    {
        private readonly RelTriangle[] _triangles = triangles;
        public int Count = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref TriangulationArena arena, int a, int b, int c)
        {
            ref var na = ref arena.Node(a);
            ref var nb = ref arena.Node(b);
            ref var nc = ref arena.Node(c);
            Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_triangles), Count++) = new(new(na.X, na.Y), new(nb.X, nb.Y), new(nc.X, nc.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in WDir a, in WDir b, in WDir c) => Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_triangles), Count++) = new(a, b, c);
    }

    private static bool TryTriangulateSmallOrConvex(ReadOnlySpan<WDir> points, ref TriangulationWriter writer)
    {
        var count = points.Length;
        if (count < 3)
        {
            return true;
        }
        ref readonly var p0 = ref points[0];
        if (count == 3)
        {
            ref readonly var p1 = ref points[1];
            ref readonly var p2 = ref points[2];
            var cross = TriangulationCross(p0, p1, p2);
            if (Math.Abs(cross) <= Eps)
            {
                return true;
            }
            if (cross > 0d)
            {
                writer.Add(p0, p1, p2);
            }
            else
            {
                writer.Add(p0, p2, p1);
            }
            return true;
        }
        else if (count == 4)
        {
            // A simple quad is triangulated directly even when concave. Pick the diagonal whose two triangles agree with the contour orientation; ambiguous/degenerate quads fall
            // through to the fully robust linked-list path
            var area = TriangulationPolygonArea2(points);
            if (Math.Abs(area) <= Eps)
            {
                return false;
            }
            ref readonly var p1 = ref points[1];
            ref readonly var p2 = ref points[2];
            ref readonly var p3 = ref points[3];
            var sign = area > 0d ? 1d : -1d;
            var c012 = TriangulationCross(p0, p1, p2) * sign;
            var c023 = TriangulationCross(p0, p2, p3) * sign;
            if (c012 > Eps && c023 > Eps)
            {
                if (sign > 0d)
                {
                    writer.Add(p0, p1, p2);
                    writer.Add(p0, p2, p3);
                }
                else
                {
                    writer.Add(p0, p2, p1);
                    writer.Add(p0, p3, p2);
                }
                return true;
            }

            var c123 = TriangulationCross(p1, p2, p3) * sign;
            var c130 = TriangulationCross(p1, p3, p0) * sign;
            if (c123 > Eps && c130 > Eps)
            {
                if (sign > 0d)
                {
                    writer.Add(p1, p2, p3);
                    writer.Add(p1, p3, p0);
                }
                else
                {
                    writer.Add(p1, p3, p2);
                    writer.Add(p1, p0, p3);
                }
                return true;
            }
            return false;
        }

        var orientation = 0d;
        var a = points[count - 2];
        var b = points[count - 1];
        for (var i = 0; i < count; ++i)
        {
            var c = points[i];
            var cross = TriangulationCross(a, b, c);
            if (Math.Abs(cross) <= Eps)
            {
                return false; // let the general path remove duplicates/collinear vertices
            }
            if (orientation == 0d)
            {
                orientation = cross;
            }
            else if ((cross > 0d) != (orientation > 0d))
            {
                return false;
            }
            a = b;
            b = c;
        }

        if (orientation > 0d)
        {
            for (var i = 1; i + 1 < count; ++i)
            {
                writer.Add(p0, points[i], points[i + 1]);
            }
        }
        else
        {
            for (var i = 1; i + 1 < count; ++i)
            {
                writer.Add(p0, points[i + 1], points[i]);
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double TriangulationCross(in WDir a, in WDir b, in WDir c) => ((double)b.X - a.X) * ((double)c.Z - a.Z) - ((double)b.Z - a.Z) * ((double)c.X - a.X);

    private static double TriangulationPolygonArea2(ReadOnlySpan<WDir> points)
    {
        var area = 0d;
        var previous = points[^1];
        var len = points.Length;
        for (var i = 0; i < len; ++i)
        {
            var current = points[i];
            area += (double)previous.X * current.Z - (double)previous.Z * current.X;
            previous = current;
        }
        return area;
    }

    private static void Earcut(ReadOnlySpan<WDir> points, int outerLength, ReadOnlySpan<int> holeStarts, ref TriangulationWriter writer)
    {
        // The common path needs one node per input vertex plus exactly two duplicate bridge nodes
        // per successful hole. Only the last-resort legacy splitter can exceed this bound, so start
        // with the exact capacity and retry with the old strict upper bound only on that rare path.
        var commonNodes = points.Length + 2 * holeStarts.Length;
        var writerStart = writer.Count;
        if (EarcutWithCapacity(points, outerLength, holeStarts, commonNodes, ref writer))
        {
            return;
        }

        writer.Count = writerStart;
        var fallbackNodes = 3 * commonNodes + 8;
        if (!EarcutWithCapacity(points, outerLength, holeStarts, fallbackNodes, ref writer))
        {
            Service.Log("Triangulation node upper bound was exceeded.");
        }
    }

    private static bool EarcutWithCapacity(ReadOnlySpan<WDir> points, int outerLength, ReadOnlySpan<int> holeStarts,
        int requiredNodes, ref TriangulationWriter writer)
    {
        var scratchWords = TriangulationScratchWords(points.Length, holeStarts.Length, requiredNodes);
        if (requiredNodes <= StackTriangulationNodes && scratchWords <= StackTriangulationScratchWords)
        {
            Span<TriangulationNode> nodes = stackalloc TriangulationNode[requiredNodes];
            Span<ulong> mortonStorage = stackalloc ulong[scratchWords];
            return EarcutWithStorage(points, outerLength, holeStarts, nodes, mortonStorage, ref writer);
        }

        var rentedNodes = ArrayPool<TriangulationNode>.Shared.Rent(requiredNodes);
        var rentedMorton = ArrayPool<ulong>.Shared.Rent(scratchWords);
        try
        {
            return EarcutWithStorage(points, outerLength, holeStarts, rentedNodes.AsSpan(0, requiredNodes),
                rentedMorton.AsSpan(0, scratchWords), ref writer);
        }
        finally
        {
            ArrayPool<ulong>.Shared.Return(rentedMorton, clearArray: false);
            ArrayPool<TriangulationNode>.Shared.Return(rentedNodes, clearArray: false);
        }
    }

    private static int TriangulationScratchWords(int pointCount, int holeCount, int nodeCapacity)
    {
        var words = nodeCapacity + ((3 * nodeCapacity + 1) >> 1); // 20 Morton bytes per possible node
        if (holeCount >= 2 && (long)holeCount * pointCount >= 768)
        {
            var buckets = TriangulationBridgeIndex.ChooseBucketCount(pointCount + 2 * holeCount);
            var edgeCapacity = 2 * pointCount + 8 * holeCount + 8;
            var vertexCapacity = pointCount + 2 * holeCount;
            var bridgeInts = TriangulationBridgeIndex.RequiredWords(edgeCapacity, vertexCapacity, buckets, buckets);
            words = Math.Max(words, (bridgeInts + 1) >> 1);
        }
        return words;
    }

    private static bool EarcutWithStorage(ReadOnlySpan<WDir> points, int outerLength, ReadOnlySpan<int> holeStarts,
        Span<TriangulationNode> nodeStorage, Span<ulong> mortonStorage, ref TriangulationWriter writer)
    {
        var capacity = nodeStorage.Length;
        var keys = mortonStorage[..capacity];
        var fields = MemoryMarshal.Cast<ulong, int>(mortonStorage[capacity..]);
        var mortonTempKeys = MemoryMarshal.Cast<int, ulong>(fields[..(2 * capacity)]);
        var mortonX = MemoryMarshal.Cast<int, float>(fields[..capacity]);
        var mortonY = MemoryMarshal.Cast<int, float>(fields.Slice(capacity, capacity));
        var mortonAlive = fields.Slice(2 * capacity, capacity);

        var arena = new TriangulationArena(nodeStorage, mortonAlive);
        var morton = new TriangulationMortonIndex(keys, mortonTempKeys, mortonX, mortonY, mortonAlive);

        // Hole acceleration reuses Morton scratch before the Morton index is built. The heuristic avoids paying setup costs when a handful of scalar scans are cheaper
        TriangulationBridgeIndex bridgeIndex = default;
        var lenHoleS = holeStarts.Length;
        var lenP = points.Length;
        if (lenHoleS >= 2 && (long)lenHoleS * lenP >= 768)
        {
            var minX = float.PositiveInfinity;
            var minY = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var maxY = float.NegativeInfinity;
            for (var i = 0; i < lenP; ++i)
            {
                var point = points[i];
                var pX = point.X;
                var pZ = point.Z;
                minX = Math.Min(minX, pX);
                minY = Math.Min(minY, pZ);
                maxX = Math.Max(maxX, pX);
                maxY = Math.Max(maxY, pZ);
            }

            var buckets = TriangulationBridgeIndex.ChooseBucketCount(lenP + 2 * lenHoleS);
            // Every original contour edge is inserted once. A successful bridge appends four replacement edges, and each filtered node can append at most one more replacement
            var edgeCapacity = 2 * lenP + 8 * lenHoleS + 8;
            var vertexCapacity = lenP + 2 * lenHoleS;
            var bridgeWords = TriangulationBridgeIndex.RequiredWords(edgeCapacity, vertexCapacity, buckets, buckets);
            var bridgeStorage = MemoryMarshal.Cast<ulong, int>(mortonStorage);
            if (bridgeWords <= bridgeStorage.Length)
            {
                bridgeIndex = new(bridgeStorage[..bridgeWords], edgeCapacity, vertexCapacity, buckets, buckets,
                    minX, minY, maxX, maxY);
            }
        }

        if (holeStarts.Length <= StackTriangulationHoles)
        {
            Span<int> holeQueue = stackalloc int[holeStarts.Length];
            return EarcutCore(points, outerLength, holeStarts, holeQueue, ref arena, ref morton, ref bridgeIndex, ref writer);
        }

        var rented = ArrayPool<int>.Shared.Rent(holeStarts.Length);
        try
        {
            return EarcutCore(points, outerLength, holeStarts, rented.AsSpan(0, holeStarts.Length), ref arena, ref morton,
                ref bridgeIndex, ref writer);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rented, clearArray: false);
        }
    }

    private static bool EarcutCore(ReadOnlySpan<WDir> points, int outerLength, ReadOnlySpan<int> holeStarts, scoped Span<int> holeQueue,
        ref TriangulationArena arena, ref TriangulationMortonIndex morton, ref TriangulationBridgeIndex bridgeIndex, ref TriangulationWriter writer)
    {
        var outerNode = TriangulationLinkedList(points, 0, outerLength, true, ref arena);
        ref var nodeO = ref arena.Node(outerNode);
        if (outerNode == NoTriangulationNode || nodeO.Next == nodeO.Prev)
        {
            return true;
        }

        if (holeStarts.Length != 0)
        {
            if (bridgeIndex.Enabled)
            {
                bridgeIndex.AddRing(outerNode, ref arena);
            }
            outerNode = EliminateTriangulationHoles(points, holeStarts, holeQueue, outerNode, ref arena, ref bridgeIndex);
            if (arena.Overflowed)
            {
                return false;
            }
        }

        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;
        var count = 0;
        var point = outerNode;
        do
        {
            ref var node = ref arena.Node(point);
            var nodeX = node.X;
            var nodeY = node.Y;
            minX = Math.Min(minX, nodeX);
            minY = Math.Min(minY, nodeY);
            maxX = Math.Max(maxX, nodeX);
            maxY = Math.Max(maxY, nodeY);
            ++count;
            point = node.Next;
        }
        while (point != outerNode);

        var invSize = 0f;
        // The linear scan is faster for small rings; contiguous Morton indexing starts paying off earlier than the old linked index because both binary search and candidate scans are linear-memory
        if (count >= 64)
        {
            var size = Math.Max(maxX - minX, maxY - minY);
            if (size > Eps)
            {
                invSize = 32767f / size;
            }
        }

        EarcutLinked(outerNode, ref arena, ref morton, ref writer, minX, minY, invSize, 0);
        return !arena.Overflowed;
    }

    private static int TriangulationLinkedList(ReadOnlySpan<WDir> points, int start, int end, bool positiveArea, ref TriangulationArena arena)
    {
        if (end - start < 3)
        {
            return NoTriangulationNode;
        }

        var last = NoTriangulationNode;
        if (positiveArea == (TriangulationSignedArea(points[start..end]) > 0d))
        {
            for (var i = start; i < end; ++i)
            {
                last = InsertTriangulationNode(i, points[i], last, ref arena);
            }
        }
        else
        {
            for (var i = end - 1; i >= start; --i)
            {
                last = InsertTriangulationNode(i, points[i], last, ref arena);
            }
        }
        ref var node = ref arena.Node(last);
        if (last != NoTriangulationNode && TriangulationNodesEqual(last, node.Next, ref arena))
        {
            var next = node.Next;
            RemoveTriangulationNode(last, ref arena);
            last = next;
        }
        return FilterTriangulationPoints(last, NoTriangulationNode, ref arena);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int InsertTriangulationNode(int sourceIndex, in WDir point, int last, ref TriangulationArena arena)
    {
        var nodeIndex = arena.Add(sourceIndex, point.X, point.Z);
        ref var node = ref arena.Node(nodeIndex);
        if (last == NoTriangulationNode)
        {
            node.Prev = nodeIndex;
            node.Next = nodeIndex;
        }
        else
        {
            ref var nodeL = ref arena.Node(last);
            var first = nodeL.Next;
            node.Next = first;
            node.Prev = last;
            arena.Node(first).Prev = nodeIndex;
            nodeL.Next = nodeIndex;
        }
        return nodeIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RemoveTriangulationNode(int nodeIndex, ref TriangulationArena arena)
    {
        ref var node = ref arena.Node(nodeIndex);
        var prev = node.Prev;
        var next = node.Next;

        arena.Node(next).Prev = prev;
        arena.Node(prev).Next = next;
        arena.DeactivateMorton(ref node);
    }

    private static int FilterTriangulationPoints(int start, int end, ref TriangulationArena arena)
    {
        if (start == NoTriangulationNode)
        {
            return NoTriangulationNode;
        }

        if (end == NoTriangulationNode)
        {
            end = start;
        }
        var point = start;
        var again = true;
        while (again || point != end)
        {
            again = false;
            ref var node = ref arena.Node(point);
            var prev = node.Prev;
            var next = node.Next;
            if (node.SourceIndex >= 0 && (TriangulationNodesEqual(point, next, ref arena) || Math.Abs(TriangulationArea(prev, point, next, ref arena)) <= Eps))
            {
                RemoveTriangulationNode(point, ref arena);
                point = end = prev;
                if (node.Next == point)
                {
                    return NoTriangulationNode;
                }
                again = true;
            }
            else
            {
                point = next;
            }
        }
        return end;
    }

    private static int FilterTriangulationPointsIndexed(int start, int end, ref TriangulationArena arena, ref TriangulationBridgeIndex bridgeIndex)
    {
        if (start == NoTriangulationNode)
        {
            return NoTriangulationNode;
        }

        if (end == NoTriangulationNode)
        {
            end = start;
        }
        var point = start;
        var again = true;
        while (again || point != end)
        {
            again = false;
            ref var node = ref arena.Node(point);
            var prev = node.Prev;
            var next = node.Next;
            if (node.SourceIndex >= 0 && (TriangulationNodesEqual(point, next, ref arena) || Math.Abs(TriangulationArea(prev, point, next, ref arena)) <= Eps))
            {
                RemoveTriangulationNode(point, ref arena);
                bridgeIndex.AddEdge(prev, next, ref arena);
                point = end = prev;
                if (node.Next == point)
                {
                    return NoTriangulationNode;
                }
                again = true;
            }
            else
            {
                point = next;
            }
        }
        return end;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTriangulationNodeLinked(int nodeIndex, ref TriangulationArena arena)
    {
        ref var node = ref arena.Node(nodeIndex);
        var prev = node.Prev;
        var next = node.Next;
        return prev != NoTriangulationNode && next != NoTriangulationNode && arena.Node(prev).Next == nodeIndex && arena.Node(next).Prev == nodeIndex;
    }

    private static void EarcutLinked(int ear, ref TriangulationArena arena, ref TriangulationMortonIndex morton, ref TriangulationWriter writer, float minX, float minY, float invSize, int pass)
    {
        if (ear == NoTriangulationNode)
        {
            return;
        }

        if (pass == 0 && invSize != 0f)
        {
            morton.Build(ear, minX, minY, invSize, ref arena);
        }

        var stop = ear;
        while (true)
        {
            ref var earNode = ref arena.Node(ear);
            var prev = earNode.Prev;
            var next = earNode.Next;
            if (prev == next)
            {
                return;
            }
            if (invSize != 0f ? IsTriangulationEarHashed(ear, minX, minY, invSize, ref arena, ref morton) : IsTriangulationEar(ear, ref arena))
            {
                writer.Add(ref arena, prev, ear, next);
                RemoveTriangulationNode(ear, ref arena);

                // Skipping one node reduces sliver formation and mirrors the original earcut traversal
                ear = arena.Node(next).Next;
                stop = ear;
                continue;
            }

            ear = next;
            if (ear == stop)
            {
                if (pass == 0)
                {
                    EarcutLinked(FilterTriangulationPoints(ear, NoTriangulationNode, ref arena), ref arena, ref morton, ref writer, minX, minY, invSize, 1);
                }
                else if (pass == 1)
                {
                    ear = CureTriangulationLocalIntersections(FilterTriangulationPoints(ear, NoTriangulationNode, ref arena), ref arena, ref writer);
                    EarcutLinked(ear, ref arena, ref morton, ref writer, minX, minY, invSize, 2);
                }
                else
                {
                    SplitTriangulationEarcut(ear, ref arena, ref morton, ref writer, minX, minY, invSize);
                }
                return;
            }
        }
    }

    private static bool IsTriangulationEar(int ear, ref TriangulationArena arena)
    {
        ref var nb = ref arena.Node(ear);
        var a = nb.Prev;
        var c = nb.Next;
        if (TriangulationArea(a, ear, c, ref arena) >= -Eps)
        {
            return false;
        }

        ref var na = ref arena.Node(a);
        ref var nc = ref arena.Node(c);
        var naX = na.X;
        var naY = na.Y;
        var nbX = nb.X;
        var nbY = nb.Y;
        var ncX = nc.X;
        var ncY = nc.Y;
        var minX = Math.Min(naX, Math.Min(nbX, ncX));
        var minY = Math.Min(naY, Math.Min(nbY, ncY));
        var maxX = Math.Max(naX, Math.Max(nbX, ncX));
        var maxY = Math.Max(naY, Math.Max(nbY, ncY));

        var point = nc.Next;
        while (point != a)
        {
            ref var np = ref arena.Node(point);
            var npX = np.X;
            var npY = np.Y;
            if (npX >= minX && npX <= maxX && npY >= minY && npY <= maxY && PointInTriangulationEarCCW(naX, naY, nbX, nbY, ncX, ncY, npX, npY) && IsTriangulationReflexOrFlat(point, ref arena))
            {
                return false;
            }
            point = np.Next;
        }
        return true;
    }

    private static bool IsTriangulationEarHashed(int ear, float minX, float minY, float invSize, ref TriangulationArena arena, ref TriangulationMortonIndex morton)
    {
        ref var nb = ref arena.Node(ear);
        var a = nb.Prev;
        var c = nb.Next;
        if (TriangulationArea(a, ear, c, ref arena) >= -Eps)
        {
            return false;
        }

        ref var na = ref arena.Node(a);
        ref var nc = ref arena.Node(c);
        var naX = na.X;
        var naY = na.Y;
        var nbX = nb.X;
        var nbY = nb.Y;
        var ncX = nc.X;
        var ncY = nc.Y;
        var minTX = Math.Min(naX, Math.Min(nbX, ncX));
        var minTY = Math.Min(naY, Math.Min(nbY, ncY));
        var maxTX = Math.Max(naX, Math.Max(nbX, ncX));
        var maxTY = Math.Max(naY, Math.Max(nbY, ncY));
        var minZ = TriangulationZOrder(minTX, minTY, minX, minY, invSize);
        var maxZ = TriangulationZOrder(maxTX, maxTY, minX, minY, invSize);

        return !morton.HasBlocker(minZ, maxZ, minTX, minTY, maxTX, maxTY, a, ear, c, naX, naY, nbX, nbY, ncX, ncY, ref arena);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointInTriangulationEarCCW(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
    {
        // EarcutLinked only calls this after proving that a-b-c is a CCW ear in standard cross-product convention, so the opposite-orientation half of the generic test is dead
        var ab = (bx - ax) * (py - ay) - (by - ay) * (px - ax);
        var bc = (cx - bx) * (py - by) - (cy - by) * (px - bx);
        var ca = (ax - cx) * (py - cy) - (ay - cy) * (px - cx);
        return ab >= -Eps && bc >= -Eps && ca >= -Eps;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTriangulationReflexOrFlat(int point, ref TriangulationArena arena)
    {
        ref var node = ref arena.Node(point);
        return TriangulationArea(node.Prev, point, node.Next, ref arena) >= -Eps;
    }

    private static void RadixSortTriangulationMortonKeys(Span<ulong> keys, Span<ulong> temp)
    {
        Span<int> counts = stackalloc int[1024];
        RadixPass(keys, temp, 32, counts);
        RadixPass(temp, keys, 42, counts);
        RadixPass(keys, temp, 52, counts);
        temp.CopyTo(keys);

        static void RadixPass(Span<ulong> source, Span<ulong> destination, int shift, Span<int> counts)
        {
            counts.Clear();
            var lenS = source.Length;
            ref var sourceBase = ref MemoryMarshal.GetReference(source);
            ref var destinationBase = ref MemoryMarshal.GetReference(destination);
            ref var countsBase = ref MemoryMarshal.GetReference(counts);
            for (var i = 0; i < lenS; ++i)
            {
                var bucket = (int)(Unsafe.Add(ref sourceBase, i) >> shift) & 1023;
                ++Unsafe.Add(ref countsBase, bucket);
            }

            var offset = 0;
            var lenC = counts.Length;
            for (var i = 0; i < lenC; ++i)
            {
                ref var count = ref Unsafe.Add(ref countsBase, i);
                var nextOffset = offset + count;
                count = offset;
                offset = nextOffset;
            }

            for (var i = 0; i < lenS; ++i)
            {
                var key = Unsafe.Add(ref sourceBase, i);
                var bucket = (int)(key >> shift) & 1023;
                var position = Unsafe.Add(ref countsBase, bucket);
                Unsafe.Add(ref destinationBase, position) = key;
                Unsafe.Add(ref countsBase, bucket) = position + 1;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int TriangulationZOrder(float x, float y, float minX, float minY, float invSize)
    {
        var ix = Math.Clamp((int)((x - minX) * invSize), 0, 32767);
        var iy = Math.Clamp((int)((y - minY) * invSize), 0, 32767);

        if (Bmi2.IsSupported)
        {
            return (int)(Bmi2.ParallelBitDeposit((uint)ix, 0x55555555u) | Bmi2.ParallelBitDeposit((uint)iy, 0xAAAAAAAAu));
        }

        ix = (ix | ix << 8) & 0x00FF00FF;
        ix = (ix | ix << 4) & 0x0F0F0F0F;
        ix = (ix | ix << 2) & 0x33333333;
        ix = (ix | ix << 1) & 0x55555555;
        iy = (iy | iy << 8) & 0x00FF00FF;
        iy = (iy | iy << 4) & 0x0F0F0F0F;
        iy = (iy | iy << 2) & 0x33333333;
        iy = (iy | iy << 1) & 0x55555555;
        return ix | iy << 1;
    }

    private static int EliminateTriangulationHoles(ReadOnlySpan<WDir> points, scoped ReadOnlySpan<int> holeStarts, scoped Span<int> queue, int outerNode, ref TriangulationArena arena, ref TriangulationBridgeIndex bridgeIndex)
    {
        var queueCount = 0;
        var lenHoleS = holeStarts.Length;
        var lenP = points.Length;
        for (var i = 0; i < lenHoleS; ++i)
        {
            var start = holeStarts[i];
            var end = i + 1 < lenHoleS ? holeStarts[i + 1] : lenP;
            var list = TriangulationLinkedList(points, start, end, false, ref arena);
            if (list == NoTriangulationNode)
            {
                continue;
            }
            ref var node = ref arena.Node(list);
            if (list == node.Next)
            {
                node.SourceIndex |= int.MinValue;
            }
            queue[queueCount++] = GetTriangulationLeftmost(list, ref arena);
        }

        SortTriangulationHoleQueue(queue[..queueCount], ref arena);
        for (var i = 0; i < queueCount; ++i)
        {
            outerNode = EliminateTriangulationHole(queue[i], outerNode, ref arena, ref bridgeIndex);
            if (arena.Overflowed)
            {
                break;
            }
        }
        return outerNode;
    }

    private static void SortTriangulationHoleQueue(scoped Span<int> queue, ref TriangulationArena arena)
    {
        var len = queue.Length;
        if (len < 2)
        {
            return;
        }
        QuickSortTriangulationHoleQueue(queue, 0, len - 1, ref arena);
    }

    private static void QuickSortTriangulationHoleQueue(scoped Span<int> queue, int lo, int hi, ref TriangulationArena arena)
    {
        while (lo < hi)
        {
            if (hi - lo <= 12)
            {
                for (var i = lo + 1; i <= hi; ++i)
                {
                    var value = queue[i];
                    var j = i - 1;
                    while (j >= lo && CompareTriangulationHoleNodes(queue[j], value, ref arena) > 0)
                    {
                        queue[j + 1] = queue[j];
                        --j;
                    }
                    queue[j + 1] = value;
                }
                return;
            }

            var pivot = queue[(lo + hi) >> 1];
            var left = lo;
            var right = hi;
            while (left <= right)
            {
                while (CompareTriangulationHoleNodes(queue[left], pivot, ref arena) < 0)
                {
                    ++left;
                }
                while (CompareTriangulationHoleNodes(queue[right], pivot, ref arena) > 0)
                {
                    --right;
                }
                if (left <= right)
                {
                    (queue[right], queue[left]) = (queue[left], queue[right]);
                    ++left;
                    --right;
                }
            }

            // Recurse into the smaller partition and iterate over the larger one, bounding stack depth
            if (right - lo < hi - left)
            {
                if (lo < right)
                {
                    QuickSortTriangulationHoleQueue(queue, lo, right, ref arena);
                }
                lo = left;
            }
            else
            {
                if (left < hi)
                {
                    QuickSortTriangulationHoleQueue(queue, left, hi, ref arena);
                }
                hi = right;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompareTriangulationHoleNodes(int a, int b, ref TriangulationArena arena)
    {
        ref var na = ref arena.Node(a);
        ref var nb = ref arena.Node(b);
        var naX = na.X;
        var nbX = nb.X;
        if (naX < nbX)
        {
            return -1;
        }
        if (naX > nbX)
        {
            return 1;
        }
        var naY = na.Y;
        var nbY = nb.Y;
        return naY < nbY ? -1 : naY > nbY ? 1 : 0;
    }

    private static int EliminateTriangulationHole(int hole, int outerNode, ref TriangulationArena arena, ref TriangulationBridgeIndex bridgeIndex)
    {
        var bridge = FindTriangulationHoleBridge(hole, outerNode, ref arena, ref bridgeIndex);
        if (bridge == NoTriangulationNode)
        {
            return outerNode;
        }

        ref var nodeB = ref arena.Node(bridge);
        var oldBridgeNext = nodeB.Next;
        var oldHolePrev = arena.Node(hole).Prev;
        if (bridgeIndex.Enabled)
        {
            // Activate the hole ring before rewiring. The old closing edge becomes stale and is
            // rejected by endpoint validation; all unchanged hole edges remain usable.
            bridgeIndex.AddRing(hole, ref arena);
        }

        var bridgeReverse = SplitTriangulationPolygon(bridge, hole, ref arena, out var bridgeCopy);
        if (bridgeReverse == NoTriangulationNode)
        {
            return outerNode;
        }
        if (bridgeIndex.Enabled)
        {
            bridgeIndex.AddVertex(bridgeCopy, ref arena);
            bridgeIndex.AddVertex(bridgeReverse, ref arena);
            bridgeIndex.AddEdge(bridge, hole, ref arena);
            bridgeIndex.AddEdge(bridgeCopy, oldBridgeNext, ref arena);
            bridgeIndex.AddEdge(oldHolePrev, bridgeReverse, ref arena);
            bridgeIndex.AddEdge(bridgeReverse, bridgeCopy, ref arena);
        }

        FilterTriangulationPointsIndexed(bridge, nodeB.Next, ref arena, ref bridgeIndex);
        FilterTriangulationPointsIndexed(bridgeReverse, arena.Node(bridgeReverse).Next, ref arena, ref bridgeIndex);
        return outerNode;
    }

    private static int FindTriangulationHoleBridge(int hole, int outerNode, ref TriangulationArena arena, ref TriangulationBridgeIndex bridgeIndex)
    {
        if (!bridgeIndex.Enabled || !bridgeIndex.TryFindRayBridge(hole, ref arena, out var bridge, out var qx, out var exactHit))
        {
            return FindTriangulationHoleBridgeScalar(hole, outerNode, ref arena);
        }
        if (exactHit || bridge == NoTriangulationNode)
        {
            return bridge;
        }

        var hx = arena.Node(hole).X;
        if (Math.Abs(hx - qx) <= Eps)
        {
            return arena.Node(bridge).Prev;
        }

        if (bridgeIndex.TryRefineBridge(hole, bridge, qx, ref arena, out var refined))
        {
            return refined;
        }
        return RefineTriangulationHoleBridgeScalar(hole, bridge, qx, ref arena);
    }

    private static int FindTriangulationHoleBridgeScalar(int hole, int outerNode, ref TriangulationArena arena)
    {
        var point = outerNode;
        ref var nodeH = ref arena.Node(hole);
        var hx = nodeH.X;
        var hy = nodeH.Y;
        var qx = float.NegativeInfinity;
        var bridge = NoTriangulationNode;

        do
        {
            ref var node = ref arena.Node(point);
            var next = node.Next;
            ref var nextNode = ref arena.Node(next);
            var nodeY = node.Y;
            var nodeX = node.X;
            var nextNodeY = nextNode.Y;
            var nextNodeX = nextNode.X;
            if (hy <= nodeY && hy >= nextNodeY && Math.Abs(nextNodeY - nodeY) > Eps)
            {
                var x = nodeX + (hy - nodeY) * (nextNodeX - nodeX) / (nextNodeY - nodeY);
                if (x <= hx + Eps && x > qx)
                {
                    qx = x;
                    if (Math.Abs(x - hx) <= Eps)
                    {
                        if (Math.Abs(hy - nodeY) <= Eps)
                        {
                            return point;
                        }
                        if (Math.Abs(hy - nextNodeY) <= Eps)
                        {
                            return next;
                        }
                    }
                    bridge = nodeX < nextNodeX ? point : next;
                }
            }
            point = next;
        }
        while (point != outerNode);

        if (bridge == NoTriangulationNode)
        {
            return NoTriangulationNode;
        }
        if (Math.Abs(hx - qx) <= Eps)
        {
            return arena.Node(bridge).Prev;
        }
        return RefineTriangulationHoleBridgeScalar(hole, bridge, qx, ref arena);
    }

    private static int RefineTriangulationHoleBridgeScalar(int hole, int bridge, float qx, ref TriangulationArena arena)
    {
        ref var holeNode = ref arena.Node(hole);
        ref var bridgeNode = ref arena.Node(bridge);
        var hx = holeNode.X;
        var hy = holeNode.Y;
        var stop = bridge;
        var mx = bridgeNode.X;
        var my = bridgeNode.Y;
        var tanMin = float.PositiveInfinity;
        var point = bridgeNode.Next;
        while (point != stop)
        {
            ref var node = ref arena.Node(point);
            var nodeX = node.X;
            var nodeY = node.Y;
            if (hx >= nodeX && nodeX >= mx && Math.Abs(hx - nodeX) > Eps && PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, nodeX, nodeY))
            {
                var tan = Math.Abs(hy - nodeY) / (hx - nodeX);
                var bNX = arena.Node(bridge).X;
                if (LocallyInsideTriangulation(point, hole, ref arena) && (tan < tanMin || Math.Abs(tan - tanMin) <= Eps
                        && (nodeX > bNX || Math.Abs(node.X - bNX) <= Eps && SectorContainsTriangulationSector(bridge, point, ref arena))))
                {
                    bridge = point;
                    tanMin = tan;
                }
            }
            point = node.Next;
        }
        return bridge;
    }

    private static int GetTriangulationLeftmost(int start, ref TriangulationArena arena)
    {
        var point = start;
        var leftmost = start;
        do
        {
            ref var node = ref arena.Node(point);
            ref var left = ref arena.Node(leftmost);
            var nodeX = node.X;
            var leftX = left.X;
            if (nodeX < leftX || nodeX == leftX && node.Y < left.Y)
            {
                leftmost = point;
            }
            point = node.Next;
        }
        while (point != start);
        return leftmost;
    }

    private static int CureTriangulationLocalIntersections(int start, ref TriangulationArena arena, ref TriangulationWriter writer)
    {
        if (start == NoTriangulationNode)
        {
            return NoTriangulationNode;
        }

        var point = start;
        do
        {
            ref var nodePoint = ref arena.Node(point);
            var pointNext = nodePoint.Next;
            var a = nodePoint.Prev;
            var b = arena.Node(pointNext).Next;
            if (!TriangulationNodesEqual(a, b, ref arena) && TriangulationSegmentsIntersect(a, point, pointNext, b, ref arena)
                && LocallyInsideTriangulation(a, b, ref arena) && LocallyInsideTriangulation(b, a, ref arena))
            {
                writer.Add(ref arena, a, point, b);
                RemoveTriangulationNode(point, ref arena);
                RemoveTriangulationNode(pointNext, ref arena);
                point = start = b;
            }
            point = arena.Node(point).Next;
        }
        while (point != start);
        return FilterTriangulationPoints(point, NoTriangulationNode, ref arena);
    }

    private const int TriangulationVertexRegular = 0;
    private const int TriangulationVertexStart = 1;
    private const int TriangulationVertexEnd = 2;
    private const int TriangulationVertexSplit = 3;
    private const int TriangulationVertexMerge = 4;
    private const int StackTriangulationMonotoneVertices = 64;

    // Pathological fallback: partition the remaining simple ring into Y-monotone pieces with a sweep line, extract the bounded faces of the resulting planar graph, then triangulate every
    // face in linear time. The original diagonal-search fallback is retained for numerically degenerate weak polygons and is only reached when this routine declines the input
    private static void SplitTriangulationEarcut(int start, ref TriangulationArena arena, ref TriangulationMortonIndex morton,
        ref TriangulationWriter writer, float minX, float minY, float invSize)
    {
        if (!TryTriangulateMonotonePartition(start, ref arena, ref writer))
        {
            SplitTriangulationEarcutLegacy(start, ref arena, ref morton, ref writer, minX, minY, invSize);
        }
    }

    private ref struct TriangulationSweepStatus
    {
        private Span<int> _left;
        private Span<int> _right;
        private Span<int> _parent;
        private Span<int> _helper;
        private readonly ReadOnlySpan<double> _x;
        private readonly ReadOnlySpan<double> _y;
        private readonly int _count;
        private int _root;
        private double _sweepY;

        public TriangulationSweepStatus(Span<int> left, Span<int> right, Span<int> parent, Span<int> helper,
            ReadOnlySpan<double> x, ReadOnlySpan<double> y, int count)
        {
            _left = left;
            _right = right;
            _parent = parent;
            _helper = helper;
            _x = x;
            _y = y;
            _count = count;
            _root = NoTriangulationNode;
            _sweepY = 0d;
            _left.Fill(NoTriangulationNode);
            _right.Fill(NoTriangulationNode);
            _parent.Fill(int.MinValue);
            _helper.Fill(NoTriangulationNode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSweepY(double y) => _sweepY = y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Helper(int edge) => _helper[edge];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetHelper(int edge, int vertex) => _helper[edge] = vertex;

        public bool Insert(int edge)
        {
            if ((uint)edge >= (uint)_count || _parent[edge] != int.MinValue)
            {
                return false;
            }

            _left[edge] = NoTriangulationNode;
            _right[edge] = NoTriangulationNode;
            if (_root == NoTriangulationNode)
            {
                _root = edge;
                _parent[edge] = NoTriangulationNode;
                return true;
            }

            var node = _root;
            while (true)
            {
                if (CompareEdges(edge, node) < 0)
                {
                    var child = _left[node];
                    if (child == NoTriangulationNode)
                    {
                        _left[node] = edge;
                        _parent[edge] = node;
                        break;
                    }
                    node = child;
                }
                else
                {
                    var child = _right[node];
                    if (child == NoTriangulationNode)
                    {
                        _right[node] = edge;
                        _parent[edge] = node;
                        break;
                    }
                    node = child;
                }
            }

            while (_parent[edge] != NoTriangulationNode && Priority(edge) < Priority(_parent[edge]))
            {
                var parent = _parent[edge];
                if (_left[parent] == edge)
                {
                    RotateRight(parent);
                }
                else
                {
                    RotateLeft(parent);
                }
            }
            return true;
        }

        public bool Remove(int edge)
        {
            if ((uint)edge >= (uint)_count || _parent[edge] == int.MinValue)
            {
                return false;
            }

            while (_left[edge] != NoTriangulationNode || _right[edge] != NoTriangulationNode)
            {
                var left = _left[edge];
                var right = _right[edge];
                if (left == NoTriangulationNode)
                {
                    RotateLeft(edge);
                }
                else if (right == NoTriangulationNode || Priority(left) < Priority(right))
                {
                    RotateRight(edge);
                }
                else
                {
                    RotateLeft(edge);
                }
            }

            var parent = _parent[edge];
            if (parent == NoTriangulationNode)
            {
                _root = NoTriangulationNode;
            }
            else if (_left[parent] == edge)
            {
                _left[parent] = NoTriangulationNode;
            }
            else
            {
                _right[parent] = NoTriangulationNode;
            }

            _parent[edge] = int.MinValue;
            _helper[edge] = NoTriangulationNode;
            return true;
        }

        public readonly int FindLeft(int vertex)
        {
            var vx = _x[vertex];
            var best = NoTriangulationNode;
            var node = _root;
            while (node != NoTriangulationNode)
            {
                var next = node + 1 == _count ? 0 : node + 1;
                var edgeX = EdgeX(node);
                if (node == vertex || next == vertex)
                {
                    // Exclude the incident edge itself, but retain the half of the search tree that can still contain a closer nonincident predecessor
                    node = edgeX < vx ? _right[node] : _left[node];
                    continue;
                }

                if (edgeX < vx)
                {
                    best = node;
                    node = _right[node];
                }
                else
                {
                    node = _left[node];
                }
            }
            return best;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly double EdgeX(int edge)
        {
            var next = edge + 1 == _count ? 0 : edge + 1;
            var y0 = _y[edge];
            var y1 = _y[next];
            var x0 = _x[edge];
            return x0 + (_x[next] - x0) * ((_sweepY - y0) / (y1 - y0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int CompareEdges(int a, int b)
        {
            var ax = EdgeX(a);
            var bx = EdgeX(b);
            if (ax < bx)
            {
                return -1;
            }
            if (ax > bx)
            {
                return 1;
            }

            var an = a + 1 == _count ? 0 : a + 1;
            var bn = b + 1 == _count ? 0 : b + 1;
            var aslope = (_x[an] - _x[a]) / (_y[an] - _y[a]);
            var bslope = (_x[bn] - _x[b]) / (_y[bn] - _y[b]);
            if (aslope > bslope)
            {
                return -1;
            }
            if (aslope < bslope)
            {
                return 1;
            }
            return a.CompareTo(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Priority(int edge)
        {
            var x = (uint)edge + 0x9E3779B9u;
            x ^= x >> 16;
            x *= 0x7FEB352Du;
            x ^= x >> 15;
            x *= 0x846CA68Bu;
            return x ^ (x >> 16);
        }

        private void RotateLeft(int node)
        {
            var pivot = _right[node];
            var parent = _parent[node];
            var middle = _left[pivot];
            _right[node] = middle;
            if (middle != NoTriangulationNode)
            {
                _parent[middle] = node;
            }
            _left[pivot] = node;
            _parent[node] = pivot;
            _parent[pivot] = parent;
            if (parent == NoTriangulationNode)
            {
                _root = pivot;
            }
            else if (_left[parent] == node)
            {
                _left[parent] = pivot;
            }
            else
            {
                _right[parent] = pivot;
            }
        }

        private void RotateRight(int node)
        {
            var pivot = _left[node];
            var parent = _parent[node];
            var middle = _right[pivot];
            _left[node] = middle;
            if (middle != NoTriangulationNode)
            {
                _parent[middle] = node;
            }
            _right[pivot] = node;
            _parent[node] = pivot;
            _parent[pivot] = parent;
            if (parent == NoTriangulationNode)
            {
                _root = pivot;
            }
            else if (_left[parent] == node)
            {
                _left[parent] = pivot;
            }
            else
            {
                _right[parent] = pivot;
            }
        }
    }

    private static bool TryTriangulateMonotonePartition(int start, ref TriangulationArena arena, ref TriangulationWriter writer)
    {
        if (start == NoTriangulationNode)
        {
            return true;
        }

        var count = 0;
        var point = start;
        do
        {
            if (++count > arena.Count)
            {
                return false;
            }
            point = arena.Node(point).Next;
        }
        while (point != start);

        if (count < 3)
        {
            return true;
        }

        var intWords = 28 * count + 8;
        var doubleWords = 2 * count;
        var ulongWords = 4 * count;
        if (count <= StackTriangulationMonotoneVertices)
        {
            scoped Span<int> ints = stackalloc int[intWords];
            scoped Span<double> doubles = stackalloc double[doubleWords];
            scoped Span<ulong> ulongs = stackalloc ulong[ulongWords];
            return TryTriangulateMonotonePartitionWithStorage(start, count, ref arena, ref writer, ints, doubles, ulongs);
        }

        var rentedInts = ArrayPool<int>.Shared.Rent(intWords);
        var rentedDoubles = ArrayPool<double>.Shared.Rent(doubleWords);
        var rentedUlongs = ArrayPool<ulong>.Shared.Rent(ulongWords);
        try
        {
            return TryTriangulateMonotonePartitionWithStorage(start, count, ref arena, ref writer,
                rentedInts.AsSpan(0, intWords), rentedDoubles.AsSpan(0, doubleWords), rentedUlongs.AsSpan(0, ulongWords));
        }
        finally
        {
            ArrayPool<ulong>.Shared.Return(rentedUlongs, clearArray: false);
            ArrayPool<double>.Shared.Return(rentedDoubles, clearArray: false);
            ArrayPool<int>.Shared.Return(rentedInts, clearArray: false);
        }
    }

    private static bool TryTriangulateMonotonePartitionWithStorage(int start, int count, ref TriangulationArena arena,
        ref TriangulationWriter writer, scoped Span<int> ints, scoped Span<double> doubles, scoped Span<ulong> adjacencyStorage)
    {
        var io = 0;
        var nodeIds = ints.Slice(io, count);
        io += count;
        var eventOrder = ints.Slice(io, count);
        io += count;
        var vertexTypes = ints.Slice(io, count);
        io += count;
        var helpers = ints.Slice(io, count);
        io += count;
        var treapLeft = ints.Slice(io, count);
        io += count;
        var treapRight = ints.Slice(io, count);
        io += count;
        var treapParent = ints.Slice(io, count);
        io += count;
        var diagonalA = ints.Slice(io, count);
        io += count;
        var diagonalB = ints.Slice(io, count);
        io += count;
        var degree = ints.Slice(io, count);
        io += count;
        var offsets = ints.Slice(io, count + 1);
        io += count + 1;
        var cursor = ints.Slice(io, count);
        io += count;
        var halfCapacity = 4 * count;
        var halfPosition = ints.Slice(io, halfCapacity);
        io += halfCapacity;
        var visited = ints.Slice(io, halfCapacity);
        io += halfCapacity;
        var face = ints.Slice(io, halfCapacity);
        io += halfCapacity;
        var chain = ints.Slice(io, count);
        io += count;
        var faceOrder = ints.Slice(io, count);
        io += count;
        var stack = ints.Slice(io, count);

        var transformedX = doubles[..count];
        var transformedY = doubles.Slice(count, count);

        var point = start;
        for (var i = 0; i < count; ++i)
        {
            nodeIds[i] = point;
            eventOrder[i] = i;
            point = arena.Node(point).Next;
        }

        if (!BuildTriangulationSweepCoordinates(nodeIds, eventOrder, transformedX, transformedY, ref arena))
        {
            return false;
        }

        var polygonArea2 = 0d;
        for (var i = 0; i < count; ++i)
        {
            var next = i + 1 == count ? 0 : i + 1;
            polygonArea2 += transformedX[i] * transformedY[next] - transformedY[i] * transformedX[next];
        }
        if (!(polygonArea2 > 0d))
        {
            return false;
        }

        for (var i = 0; i < count; ++i)
        {
            var prev = i == 0 ? count - 1 : i - 1;
            var next = i + 1 == count ? 0 : i + 1;
            var tY = transformedY[i];
            var prevAbove = transformedY[prev] > tY;
            var nextAbove = transformedY[next] > tY;
            var cross = TriangulationCross(prev, i, next, transformedX, transformedY);
            if (cross == 0d)
            {
                return false;
            }

            var convex = cross > 0d;
            vertexTypes[i] = !prevAbove && !nextAbove ? convex ? TriangulationVertexStart : TriangulationVertexSplit
            : prevAbove && nextAbove ? convex ? TriangulationVertexEnd : TriangulationVertexMerge : TriangulationVertexRegular;
        }

        var status = new TriangulationSweepStatus(treapLeft, treapRight, treapParent, helpers, transformedX, transformedY, count);
        var diagonalCount = 0;
        for (var oi = 0; oi < count; ++oi)
        {
            var vertex = eventOrder[oi];
            var prev = vertex == 0 ? count - 1 : vertex - 1;
            var next = vertex + 1 == count ? 0 : vertex + 1;
            status.SetSweepY(Math.BitDecrement(transformedY[vertex]));

            switch (vertexTypes[vertex])
            {
                case TriangulationVertexStart:
                    if (!status.Insert(vertex))
                    {
                        return false;
                    }
                    status.SetHelper(vertex, vertex);
                    break;

                case TriangulationVertexEnd:
                    {
                        var helper = status.Helper(prev);
                        if (helper == NoTriangulationNode || vertexTypes[helper] == TriangulationVertexMerge
                            && !AddTriangulationMonotoneDiagonal(vertex, helper, diagonalA, diagonalB, ref diagonalCount, count))
                        {
                            return false;
                        }
                        if (!status.Remove(prev))
                        {
                            return false;
                        }
                        break;
                    }

                case TriangulationVertexSplit:
                    {
                        var leftEdge = status.FindLeft(vertex);
                        if (leftEdge == NoTriangulationNode)
                        {
                            return false;
                        }
                        var helper = status.Helper(leftEdge);
                        if (helper == NoTriangulationNode
                            || !AddTriangulationMonotoneDiagonal(vertex, helper, diagonalA, diagonalB, ref diagonalCount, count))
                        {
                            return false;
                        }
                        status.SetHelper(leftEdge, vertex);
                        if (!status.Insert(vertex))
                        {
                            return false;
                        }
                        status.SetHelper(vertex, vertex);
                        break;
                    }

                case TriangulationVertexMerge:
                    {
                        var helper = status.Helper(prev);
                        if (helper == NoTriangulationNode || vertexTypes[helper] == TriangulationVertexMerge
                            && !AddTriangulationMonotoneDiagonal(vertex, helper, diagonalA, diagonalB, ref diagonalCount, count))
                        {
                            return false;
                        }
                        if (!status.Remove(prev))
                        {
                            return false;
                        }
                        var leftEdge = status.FindLeft(vertex);
                        if (leftEdge == NoTriangulationNode)
                        {
                            return false;
                        }
                        helper = status.Helper(leftEdge);
                        if (helper == NoTriangulationNode || vertexTypes[helper] == TriangulationVertexMerge
                            && !AddTriangulationMonotoneDiagonal(vertex, helper, diagonalA, diagonalB, ref diagonalCount, count))
                        {
                            return false;
                        }
                        status.SetHelper(leftEdge, vertex);
                        break;
                    }

                default:
                    if (transformedY[next] < transformedY[vertex])
                    {
                        var helper = status.Helper(prev);
                        if (helper == NoTriangulationNode || vertexTypes[helper] == TriangulationVertexMerge
                            && !AddTriangulationMonotoneDiagonal(vertex, helper, diagonalA, diagonalB, ref diagonalCount, count))
                        {
                            return false;
                        }
                        if (!status.Remove(prev) || !status.Insert(vertex))
                        {
                            return false;
                        }
                        status.SetHelper(vertex, vertex);
                    }
                    else
                    {
                        var leftEdge = status.FindLeft(vertex);
                        if (leftEdge == NoTriangulationNode)
                        {
                            return false;
                        }
                        var helper = status.Helper(leftEdge);
                        if (helper == NoTriangulationNode || vertexTypes[helper] == TriangulationVertexMerge
                            && !AddTriangulationMonotoneDiagonal(vertex, helper, diagonalA, diagonalB, ref diagonalCount, count))
                        {
                            return false;
                        }
                        status.SetHelper(leftEdge, vertex);
                    }
                    break;
            }
        }

        var edgeCount = count + diagonalCount;
        var halfCount = 2 * edgeCount;
        if (halfCount > halfCapacity)
        {
            return false;
        }

        degree.Clear();
        for (var i = 0; i < count; ++i)
        {
            degree[i] = 2;
        }
        for (var i = 0; i < diagonalCount; ++i)
        {
            ++degree[diagonalA[i]];
            ++degree[diagonalB[i]];
        }

        offsets[0] = 0;
        for (var i = 0; i < count; ++i)
        {
            offsets[i + 1] = offsets[i] + degree[i];
            cursor[i] = offsets[i];
        }
        if (offsets[count] != halfCount)
        {
            return false;
        }

        var adjacency = adjacencyStorage[..halfCount];
        for (var edge = 0; edge < count; ++edge)
        {
            var next = edge + 1 == count ? 0 : edge + 1;
            AddTriangulationGraphEdge(edge, edge, next, adjacency, cursor);
        }
        for (var i = 0; i < diagonalCount; ++i)
        {
            AddTriangulationGraphEdge(count + i, diagonalA[i], diagonalB[i], adjacency, cursor);
        }

        for (var vertex = 0; vertex < count; ++vertex)
        {
            SortTriangulationAdjacency(adjacency.Slice(offsets[vertex], degree[vertex]), vertex, transformedX, transformedY);
        }
        for (var i = 0; i < halfCount; ++i)
        {
            halfPosition[TriangulationAdjacencyHalf(adjacency[i])] = i;
        }

        visited[..halfCount].Clear();
        var writerStart = writer.Count;
        var positiveFaces = 0;
        for (var firstHalf = 0; firstHalf < halfCount; ++firstHalf)
        {
            if (visited[firstHalf] != 0)
            {
                continue;
            }

            var currentHalf = firstHalf;
            var faceCount = 0;
            GetTriangulationHalfEndpoints(currentHalf, count, diagonalA, diagonalB, out var from, out _);
            do
            {
                if ((uint)currentHalf >= (uint)halfCount || visited[currentHalf] != 0 && currentHalf != firstHalf
                    || faceCount >= halfCount)
                {
                    writer.Count = writerStart;
                    return false;
                }

                visited[currentHalf] = 1;
                face[faceCount++] = from;
                GetTriangulationHalfEndpoints(currentHalf, count, diagonalA, diagonalB, out _, out var to);
                var reversePosition = halfPosition[currentHalf ^ 1];
                var local = reversePosition - offsets[to];
                if ((uint)local >= (uint)degree[to])
                {
                    writer.Count = writerStart;
                    return false;
                }
                var previousPosition = local == 0 ? offsets[to + 1] - 1 : reversePosition - 1;
                currentHalf = TriangulationAdjacencyHalf(adjacency[previousPosition]);
                from = to;
            }
            while (currentHalf != firstHalf);

            if (faceCount < 3)
            {
                continue;
            }

            var area2 = 0d;
            for (var i = 0; i < faceCount; ++i)
            {
                var a = face[i];
                var b = face[i + 1 == faceCount ? 0 : i + 1];
                area2 += transformedX[a] * transformedY[b] - transformedY[a] * transformedX[b];
            }
            if (area2 <= 0d)
            {
                continue;
            }

            ++positiveFaces;
            if (!TriangulateMonotoneFace(face[..faceCount], nodeIds, transformedX, transformedY, chain[..faceCount], faceOrder[..faceCount], stack[..faceCount], ref arena, ref writer))
            {
                writer.Count = writerStart;
                return false;
            }
        }

        if (positiveFaces != diagonalCount + 1 || writer.Count - writerStart != count - 2)
        {
            writer.Count = writerStart;
            return false;
        }
        return true;
    }

    private static bool BuildTriangulationSweepCoordinates(scoped ReadOnlySpan<int> nodeIds, scoped Span<int> eventOrder, scoped Span<double> transformedX, scoped Span<double> transformedY, ref TriangulationArena arena)
    {
        ReadOnlySpan<double> slopes = [0.6180339887498948d, -0.4142135623730950d, 1.7320508075688772d, -2.4142135623730950d];
        var lenNode = nodeIds.Length;
        var lenEOrder = eventOrder.Length;
        for (var attempt = 0; attempt < 4; ++attempt)
        {
            var slope = slopes[attempt];
            var maxAbsY = 1d;
            for (var i = 0; i < lenNode; ++i)
            {
                ref var node = ref arena.Node(nodeIds[i]);
                var x = (double)node.X;
                var y = (double)node.Y;
                transformedX[i] = x + slope * y;
                transformedY[i] = y - slope * x;
                maxAbsY = Math.Max(maxAbsY, Math.Abs(transformedY[i]));
                eventOrder[i] = i;
            }

            SortTriangulationSweepEvents(eventOrder, transformedX, transformedY);
            var tolerance = 32d * 2.2204460492503131e-16d * maxAbsY;
            var valid = true;
            for (var i = 1; i < lenEOrder; ++i)
            {
                if (Math.Abs(transformedY[eventOrder[i - 1]] - transformedY[eventOrder[i]]) <= tolerance)
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AddTriangulationMonotoneDiagonal(int a, int b, Span<int> diagonalA, Span<int> diagonalB, ref int count, int vertexCount)
    {
        if (a == b || (a + 1 == vertexCount ? 0 : a + 1) == b || (b + 1 == vertexCount ? 0 : b + 1) == a || count >= diagonalA.Length)
        {
            return a != b;
        }
        diagonalA[count] = a;
        diagonalB[count] = b;
        ++count;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double TriangulationCross(int a, int b, int c, ReadOnlySpan<double> x, ReadOnlySpan<double> y)
    {
        var yb = y[b];
        var xb = x[b];
        return (xb - x[a]) * (y[c] - yb) - (yb - y[a]) * (x[c] - xb);
    }

    private static void AddTriangulationGraphEdge(int edge, int a, int b, Span<ulong> adjacency, Span<int> cursor)
    {
        adjacency[cursor[a]++] = ((ulong)(uint)(2 * edge) << 32) | (uint)b;
        adjacency[cursor[b]++] = ((ulong)(uint)(2 * edge + 1) << 32) | (uint)a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int TriangulationAdjacencyHalf(ulong entry) => (int)(entry >> 32);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int TriangulationAdjacencyTo(ulong entry) => (int)(uint)entry;

    private static void GetTriangulationHalfEndpoints(int half, int boundaryEdges, ReadOnlySpan<int> diagonalA, ReadOnlySpan<int> diagonalB, out int from, out int to)
    {
        var edge = half >> 1;
        if (edge < boundaryEdges)
        {
            from = edge;
            to = edge + 1 == boundaryEdges ? 0 : edge + 1;
        }
        else
        {
            var diagonal = edge - boundaryEdges;
            from = diagonalA[diagonal];
            to = diagonalB[diagonal];
        }
        if ((half & 1) != 0)
        {
            (from, to) = (to, from);
        }
    }

    private static bool TriangulateMonotoneFace(scoped ReadOnlySpan<int> face, scoped ReadOnlySpan<int> nodeIds, scoped ReadOnlySpan<double> x, scoped ReadOnlySpan<double> y, scoped Span<int> chain, scoped Span<int> order, scoped Span<int> stack,
    ref TriangulationArena arena, ref TriangulationWriter writer)
    {
        var count = face.Length;
        if (count == 3)
        {
            return AddTriangulationMonotoneTriangle(nodeIds[face[0]], nodeIds[face[1]], nodeIds[face[2]], ref arena, ref writer);
        }

        var top = 0;
        var bottom = 0;
        for (var i = 1; i < count; ++i)
        {
            var vertex = face[i];
            var yV = y[vertex];
            if (yV > y[face[top]])
            {
                top = i;
            }
            if (yV < y[face[bottom]])
            {
                bottom = i;
            }
        }
        if (top == bottom)
        {
            return false;
        }

        chain.Fill(NoTriangulationNode);
        var index = top;
        chain[index] = 0; // Forward traversal of a CCW face is its left chain.
        while (index != bottom)
        {
            index = index + 1 == count ? 0 : index + 1;
            chain[index] = index == bottom ? 2 : 0;
        }
        index = top;
        while (index != bottom)
        {
            index = index == 0 ? count - 1 : index - 1;
            if (index != bottom)
            {
                chain[index] = 1;
            }
        }
        chain[top] = 2;

        for (var i = 0; i < count; ++i)
        {
            order[i] = i;
        }
        SortTriangulationFaceEvents(order, face, x, y);
        if (order[0] != top || order[count - 1] != bottom)
        {
            return false;
        }

        var writerStart = writer.Count;
        var stackCount = 2;
        stack[0] = order[0];
        stack[1] = order[1];
        for (var oi = 2; oi < count - 1; ++oi)
        {
            var current = order[oi];
            if (chain[current] != chain[stack[stackCount - 1]])
            {
                while (stackCount > 1)
                {
                    var u = stack[--stackCount];
                    var v = stack[stackCount - 1];
                    if (!AddTriangulationMonotoneTriangle(nodeIds[face[current]], nodeIds[face[u]], nodeIds[face[v]], ref arena, ref writer))
                    {
                        writer.Count = writerStart;
                        return false;
                    }
                }
                --stackCount;
                stack[stackCount++] = order[oi - 1];
                stack[stackCount++] = current;
            }
            else
            {
                var u = stack[--stackCount];
                while (stackCount > 0)
                {
                    var v = stack[stackCount - 1];
                    var cross = TriangulationCross(face[current], face[u], face[v], x, y);
                    var inside = chain[current] == 0 ? cross < 0d : cross > 0d;
                    if (!inside)
                    {
                        break;
                    }
                    if (!AddTriangulationMonotoneTriangle(nodeIds[face[current]], nodeIds[face[u]], nodeIds[face[v]], ref arena, ref writer))
                    {
                        writer.Count = writerStart;
                        return false;
                    }
                    u = stack[--stackCount];
                }
                stack[stackCount++] = u;
                stack[stackCount++] = current;
            }
        }

        var last = order[count - 1];
        while (stackCount > 1)
        {
            var u = stack[--stackCount];
            var v = stack[stackCount - 1];
            if (!AddTriangulationMonotoneTriangle(nodeIds[face[last]], nodeIds[face[u]], nodeIds[face[v]], ref arena, ref writer))
            {
                writer.Count = writerStart;
                return false;
            }
        }
        return writer.Count - writerStart == count - 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AddTriangulationMonotoneTriangle(int a, int b, int c, ref TriangulationArena arena, ref TriangulationWriter writer)
    {
        ref var na = ref arena.Node(a);
        ref var nb = ref arena.Node(b);
        ref var nc = ref arena.Node(c);
        var naX = na.X;
        var cross = ((double)nb.X - naX) * ((double)nc.Y - na.Y) - ((double)nb.Y - na.Y) * ((double)nc.X - naX);
        if (Math.Abs(cross) <= Eps)
        {
            return false;
        }
        if (cross > 0d)
        {
            writer.Add(ref arena, a, b, c);
        }
        else
        {
            writer.Add(ref arena, a, c, b);
        }
        return true;
    }

    private static void SortTriangulationSweepEvents(Span<int> values, ReadOnlySpan<double> x, ReadOnlySpan<double> y)
    {
        SortTriangulationIndices(values, x, y, descendingY: true);
    }

    private static void SortTriangulationFaceEvents(Span<int> values, ReadOnlySpan<int> face, ReadOnlySpan<double> x, ReadOnlySpan<double> y)
    {
        var len = values.Length;
        if (len < 2)
        {
            return;
        }
        SortTriangulationFaceEvents(values, face, x, y, 0, len - 1);
    }

    private static void SortTriangulationFaceEvents(Span<int> values, ReadOnlySpan<int> face, ReadOnlySpan<double> x, ReadOnlySpan<double> y, int lo, int hi)
    {
        while (hi - lo > 16)
        {
            var pivot = values[(lo + hi) >> 1];
            var i = lo;
            var j = hi;
            while (i <= j)
            {
                while (CompareTriangulationFaceEvent(values[i], pivot, face, x, y) < 0)
                {
                    ++i;
                }
                while (CompareTriangulationFaceEvent(values[j], pivot, face, x, y) > 0)
                {
                    --j;
                }
                if (i <= j)
                {
                    (values[i], values[j]) = (values[j], values[i]);
                    ++i;
                    --j;
                }
            }
            if (j - lo < hi - i)
            {
                if (lo < j)
                {
                    SortTriangulationFaceEvents(values, face, x, y, lo, j);
                }
                lo = i;
            }
            else
            {
                if (i < hi)
                {
                    SortTriangulationFaceEvents(values, face, x, y, i, hi);
                }
                hi = j;
            }
        }
        for (var i = lo + 1; i <= hi; ++i)
        {
            var value = values[i];
            var j = i - 1;
            while (j >= lo && CompareTriangulationFaceEvent(values[j], value, face, x, y) > 0)
            {
                values[j + 1] = values[j--];
            }
            values[j + 1] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompareTriangulationFaceEvent(int a, int b, ReadOnlySpan<int> face, ReadOnlySpan<double> x, ReadOnlySpan<double> y)
        => CompareTriangulationEventVertices(face[a], face[b], x, y);

    private static void SortTriangulationIndices(Span<int> values, ReadOnlySpan<double> x, ReadOnlySpan<double> y, bool descendingY)
    {
        var len = values.Length;
        if (len < 2)
        {
            return;
        }
        SortTriangulationIndices(values, x, y, descendingY, 0, len - 1);
    }

    private static void SortTriangulationIndices(Span<int> values, ReadOnlySpan<double> x, ReadOnlySpan<double> y, bool descendingY, int lo, int hi)
    {
        while (hi - lo > 16)
        {
            var pivot = values[(lo + hi) >> 1];
            var i = lo;
            var j = hi;
            while (i <= j)
            {
                while (CompareTriangulationEventVertices(values[i], pivot, x, y, descendingY) < 0)
                {
                    ++i;
                }
                while (CompareTriangulationEventVertices(values[j], pivot, x, y, descendingY) > 0)
                {
                    --j;
                }
                if (i <= j)
                {
                    (values[i], values[j]) = (values[j], values[i]);
                    ++i;
                    --j;
                }
            }
            if (j - lo < hi - i)
            {
                if (lo < j)
                {
                    SortTriangulationIndices(values, x, y, descendingY, lo, j);
                }
                lo = i;
            }
            else
            {
                if (i < hi)
                {
                    SortTriangulationIndices(values, x, y, descendingY, i, hi);
                }
                hi = j;
            }
        }
        for (var i = lo + 1; i <= hi; ++i)
        {
            var value = values[i];
            var j = i - 1;
            while (j >= lo && CompareTriangulationEventVertices(values[j], value, x, y, descendingY) > 0)
            {
                values[j + 1] = values[j--];
            }
            values[j + 1] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompareTriangulationEventVertices(int a, int b, ReadOnlySpan<double> x, ReadOnlySpan<double> y, bool descendingY = true)
    {
        var ya = y[a];
        var yb = y[b];
        if (ya != y[b])
        {
            return descendingY ? (ya > yb ? -1 : 1) : (ya < yb ? -1 : 1);
        }
        var xa = x[a];
        var xb = x[b];
        if (xa != xb)
        {
            return xa < xb ? -1 : 1;
        }
        return a.CompareTo(b);
    }

    private static void SortTriangulationAdjacency(Span<ulong> values, int origin, ReadOnlySpan<double> x, ReadOnlySpan<double> y)
    {
        var len = values.Length;
        if (len < 2)
        {
            return;
        }
        SortTriangulationAdjacency(values, origin, x, y, 0, len - 1);
    }

    private static void SortTriangulationAdjacency(Span<ulong> values, int origin, ReadOnlySpan<double> x, ReadOnlySpan<double> y, int lo, int hi)
    {
        while (hi - lo > 12)
        {
            var pivot = values[(lo + hi) >> 1];
            var i = lo;
            var j = hi;
            while (i <= j)
            {
                while (CompareTriangulationAdjacency(values[i], pivot, origin, x, y) < 0)
                {
                    ++i;
                }
                while (CompareTriangulationAdjacency(values[j], pivot, origin, x, y) > 0)
                {
                    --j;
                }
                if (i <= j)
                {
                    (values[i], values[j]) = (values[j], values[i]);
                    ++i;
                    --j;
                }
            }
            if (j - lo < hi - i)
            {
                if (lo < j)
                {
                    SortTriangulationAdjacency(values, origin, x, y, lo, j);
                }
                lo = i;
            }
            else
            {
                if (i < hi)
                {
                    SortTriangulationAdjacency(values, origin, x, y, i, hi);
                }
                hi = j;
            }
        }
        for (var i = lo + 1; i <= hi; ++i)
        {
            var value = values[i];
            var j = i - 1;
            while (j >= lo && CompareTriangulationAdjacency(values[j], value, origin, x, y) > 0)
            {
                values[j + 1] = values[j--];
            }
            values[j + 1] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompareTriangulationAdjacency(ulong a, ulong b, int origin,
        ReadOnlySpan<double> x, ReadOnlySpan<double> y)
    {
        var av = TriangulationAdjacencyTo(a);
        var bv = TriangulationAdjacencyTo(b);
        var adx = x[av] - x[origin];
        var ady = y[av] - y[origin];
        var bdx = x[bv] - x[origin];
        var bdy = y[bv] - y[origin];
        var ah = ady > 0d || ady == 0d && adx >= 0d ? 0 : 1;
        var bh = bdy > 0d || bdy == 0d && bdx >= 0d ? 0 : 1;
        if (ah != bh)
        {
            return ah - bh;
        }
        var cross = adx * bdy - ady * bdx;
        if (cross != 0d)
        {
            return cross > 0d ? -1 : 1;
        }
        var alen = adx * adx + ady * ady;
        var blen = bdx * bdx + bdy * bdy;
        if (alen != blen)
        {
            return alen < blen ? -1 : 1;
        }
        return TriangulationAdjacencyHalf(a).CompareTo(TriangulationAdjacencyHalf(b));
    }

    private static void SplitTriangulationEarcutLegacy(int start, ref TriangulationArena arena, ref TriangulationMortonIndex morton, ref TriangulationWriter writer, float minX, float minY, float invSize)
    {
        if (start == NoTriangulationNode)
        {
            return;
        }

        var a = start;
        do
        {
            ref var nodeA = ref arena.Node(a);
            var b = arena.Node(nodeA.Next).Next;
            while (b != nodeA.Prev)
            {
                ref var nodeB = ref arena.Node(b);
                if ((nodeA.SourceIndex & int.MaxValue) != (nodeB.SourceIndex & int.MaxValue) && IsValidTriangulationDiagonal(a, b, ref arena))
                {
                    var c = SplitTriangulationPolygon(a, b, ref arena);
                    if (c == NoTriangulationNode)
                    {
                        return;
                    }

                    var aFiltered = FilterTriangulationPoints(a, nodeA.Next, ref arena);
                    var cFiltered = FilterTriangulationPoints(c, arena.Node(c).Next, ref arena);
                    EarcutLinked(aFiltered, ref arena, ref morton, ref writer, minX, minY, invSize, 0);
                    EarcutLinked(cFiltered, ref arena, ref morton, ref writer, minX, minY, invSize, 0);
                    return;
                }
                b = nodeB.Next;
            }
            a = nodeA.Next;
        }
        while (a != start);
    }

    private static bool IsValidTriangulationDiagonal(int a, int b, ref TriangulationArena arena)
    {
        ref var nodeA = ref arena.Node(a);
        ref var nodeB = ref arena.Node(b);
        var nodeAPrev = nodeA.Prev;
        var nodeANext = nodeA.Next;
        var nodeBPrev = nodeB.Prev;
        return nodeANext != b && nodeAPrev != b && !IntersectsTriangulationPolygon(a, b, ref arena)
            && (LocallyInsideTriangulation(a, b, ref arena) && LocallyInsideTriangulation(b, a, ref arena) && MiddleInsideTriangulation(a, b, ref arena)
                && (Math.Abs(TriangulationArea(nodeAPrev, a, nodeBPrev, ref arena)) > Eps || Math.Abs(TriangulationArea(a, nodeBPrev, b, ref arena)) > Eps)
                || TriangulationNodesEqual(a, b, ref arena) && TriangulationArea(nodeAPrev, a, nodeANext, ref arena) > Eps && TriangulationArea(nodeBPrev, b, nodeB.Next, ref arena) > Eps);
    }

    private static bool IntersectsTriangulationPolygon(int a, int b, ref TriangulationArena arena)
    {
        var point = a;
        do
        {
            var next = arena.Node(point).Next;
            if (point != a && next != a && point != b && next != b && TriangulationSegmentsIntersect(point, next, a, b, ref arena))
            {
                return true;
            }
            point = next;
        }
        while (point != a);
        return false;
    }

    private static bool LocallyInsideTriangulation(int a, int b, ref TriangulationArena arena)
    {
        ref var node = ref arena.Node(a);
        var prev = node.Prev;
        var next = node.Next;
        return TriangulationArea(prev, a, next, ref arena) < 0f
            ? TriangulationArea(a, b, next, ref arena) >= -Eps && TriangulationArea(a, prev, b, ref arena) >= -Eps
            : TriangulationArea(a, b, prev, ref arena) < Eps || TriangulationArea(a, next, b, ref arena) < Eps;
    }

    private static bool MiddleInsideTriangulation(int a, int b, ref TriangulationArena arena)
    {
        var point = a;
        var inside = false;
        ref var nodeA = ref arena.Node(a);
        ref var nodeB = ref arena.Node(b);
        var px = 0.5f * (nodeA.X + nodeB.X);
        var py = 0.5f * (nodeA.Y + nodeB.Y);
        do
        {
            ref var node = ref arena.Node(point);
            var next = node.Next;
            ref var nextNode = ref arena.Node(next);
            var nodeY = node.Y;
            var nodeX = node.X;
            var nextNodeY = nextNode.Y;
            if ((nodeY > py) != (nextNodeY > py) && px < (nextNode.X - nodeX) * (py - nodeY) / (nextNodeY - nodeY) + nodeX)
            {
                inside = !inside;
            }
            point = next;
        }
        while (point != a);
        return inside;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SplitTriangulationPolygon(int a, int b, ref TriangulationArena arena) => SplitTriangulationPolygon(a, b, ref arena, out _);

    private static int SplitTriangulationPolygon(int a, int b, ref TriangulationArena arena, out int a2)
    {
        if (!arena.TryReserve(2))
        {
            a2 = NoTriangulationNode;
            return NoTriangulationNode;
        }

        ref var aNode = ref arena.Node(a);
        ref var bNode = ref arena.Node(b);
        var aSourceIndex = aNode.SourceIndex;
        var ax = aNode.X;
        var ay = aNode.Y;
        var an = aNode.Next;
        var bSourceIndex = bNode.SourceIndex;
        var bx = bNode.X;
        var by = bNode.Y;
        var bp = bNode.Prev;
        a2 = arena.Add(aSourceIndex, ax, ay);
        var b2 = arena.Add(bSourceIndex, bx, by);

        aNode.Next = b;
        bNode.Prev = a;

        ref var a2Node = ref arena.Node(a2);
        ref var b2Node = ref arena.Node(b2);

        a2Node.Next = an;
        arena.Node(an).Prev = a2;

        b2Node.Next = a2;
        a2Node.Prev = b2;

        arena.Node(bp).Next = b2;
        b2Node.Prev = bp;

        return b2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SectorContainsTriangulationSector(int m, int p, ref TriangulationArena arena)
    {
        ref var nodeM = ref arena.Node(m);
        ref var nodeP = ref arena.Node(p);
        return TriangulationArea(nodeM.Prev, m, nodeP.Prev, ref arena) < 0f && TriangulationArea(nodeP.Next, m, nodeM.Next, ref arena) < 0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TriangulationSegmentsIntersect(int p1, int q1, int p2, int q2, ref TriangulationArena arena)
    {
        if (TriangulationNodesEqual(p1, q1, ref arena) && TriangulationNodesEqual(p2, q2, ref arena))
        {
            return true;
        }
        return (TriangulationArea(p1, q1, p2, ref arena) > 0f) != (TriangulationArea(p1, q1, q2, ref arena) > 0f)
            && (TriangulationArea(p2, q2, p1, ref arena) > 0f) != (TriangulationArea(p2, q2, q1, ref arena) > 0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointInTriangle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
    {
        var ab = (cx - px) * (ay - py) - (ax - px) * (cy - py);
        if (ab < Eps)
        {
            return false;
        }

        var bc = (ax - px) * (by - py) - (bx - px) * (ay - py);

        if (bc < Eps)
        {
            return false;
        }
        return (bx - px) * (cy - py) - (cx - px) * (by - py) >= Eps;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float TriangulationArea(int p, int q, int r, ref TriangulationArena arena)
    {
        ref var np = ref arena.Node(p);
        ref var nq = ref arena.Node(q);
        ref var nr = ref arena.Node(r);
        return (nq.Y - np.Y) * (nr.X - nq.X) - (nq.X - np.X) * (nr.Y - nq.Y);
    }

    private static double TriangulationSignedArea(ReadOnlySpan<WDir> points)
    {
        var sum = 0d;
        var len = points.Length;
        for (int i = 0, j = len - 1; i < len; j = i++)
        {
            sum += ((double)points[j].X - points[i].X) * ((double)points[i].Z + points[j].Z);
        }
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TriangulationNodesEqual(int a, int b, ref TriangulationArena arena)
    {
        ref var na = ref arena.Node(a);
        ref var nb = ref arena.Node(b);
        var dx = na.X - nb.X;
        var dy = na.Y - nb.Y;
        return dx * dx + dy * dy <= Eps2;
    }
}
