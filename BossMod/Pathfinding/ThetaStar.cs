namespace BossMod.Pathfinding;

public class ThetaStar
{
    public struct Node
    {
        public float GScore;
        public float HScore;
        public int ParentX;
        public int ParentY;
        public int OpenHeapIndex; // -1 if in closed list, 0 if not in any lists, otherwise (index+1)
        public float PathLeeway; // min diff along path between node's g-value and cell's g-value
        public float PathMinG; // minimum 'max g' value along path
        public Vector2 EnterOffset; // from cell center; up to +-0.5

        public readonly float FScore => GScore + HScore;
    }

    private Map _map = new();
    private WPos _goalPos;
    private float _goalRadius;
    private Node[] _nodes = [];
    private readonly List<int> _openList = [];
    private float _deltaGSide;
    private float _startMaxG;

    public int BestIndex { get; private set; }

    private const float BorderCushion = 0.1f;
    private const float MaxNeighbourOffset = 0.5f - BorderCushion;
    private const float CenterToNeighbour = 0.5f + BorderCushion;

    public ref Node NodeByIndex(int index) => ref _nodes[index];
    public WPos CellCenter(int index) => _map.GridToWorld(index % _map.Width, index / _map.Width, 0.5f, 0.5f);

    // gMultiplier is typically inverse speed, which turns g-values into time
    public void Start(Map map, WPos startPos, WPos goalPos, float goalRadius, float gMultiplier)
    {
        _map = map;
        _goalPos = goalPos;
        _goalRadius = goalRadius;
        var numPixels = map.Width * map.Height;
        if (_nodes.Length < numPixels)
            _nodes = new Node[numPixels];
        else
            Array.Fill(_nodes, default, 0, numPixels);
        _openList.Clear();
        _deltaGSide = map.Resolution * gMultiplier;

        var startFrac = map.WorldToGridFrac(startPos);
        var start = map.ClampToGrid(map.FracToGrid(startFrac));
        int startIndex = BestIndex = _map.GridToIndex(start.x, start.y);
        startFrac.X -= start.x + 0.5f;
        startFrac.Y -= start.y + 0.5f;
        _startMaxG = _map.Pixels[startIndex].MaxG;
        _nodes[startIndex] = new()
        {
            GScore = 0,
            HScore = HeuristicDistance(start.x, start.y),
            ParentX = start.x, // start's parent is self
            ParentY = start.y,
            PathLeeway = _startMaxG,
            PathMinG = _startMaxG,
            EnterOffset = startFrac,
        };
        AddToOpen(startIndex);
    }

    // returns whether search is to be terminated; on success, first node of the open list would contain found goal
    public bool ExecuteStep()
    {
        if (_openList.Count == 0 /*|| _nodes[_openList[0]].HScore <= 0*/)
            return false;

        int nextNodeIndex = PopMinOpen();
        var (nextNodeX, nextNodeY) = _map.IndexToGrid(nextNodeIndex);
        if (HeapLess(nextNodeIndex, BestIndex))
            BestIndex = nextNodeIndex;

        var startOff = _nodes[nextNodeIndex].EnterOffset;
        if (nextNodeY > 0)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY - 1, nextNodeIndex - _map.Width, CenterToNeighbour + startOff.Y);
        if (nextNodeX > 0)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY, nextNodeIndex - 1, CenterToNeighbour + startOff.X);
        if (nextNodeX < _map.Width - 1)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY, nextNodeIndex + 1, CenterToNeighbour - startOff.X);
        if (nextNodeY < _map.Height - 1)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY + 1, nextNodeIndex + _map.Width, CenterToNeighbour - startOff.Y);
        return true;
    }

    public int Execute()
    {
        while (!IsVeryGood(BestIndex) && ExecuteStep())
            ;
        return BestIndex;
    }

    // if node is the best we can hope to reach
    public bool IsVeryGood(int nodeIndex) => _map.Pixels[nodeIndex] == new Map.Pixel(float.MaxValue, _map.MaxPriority);

    public bool IsLeftBetter(ref Node nodeL, ref Map.Pixel pixL, ref Node nodeR, ref Map.Pixel pixR)
    {
        var safeL = pixL.MaxG == float.MaxValue && nodeL.PathLeeway > 0;
        var safeR = pixR.MaxG == float.MaxValue && nodeR.PathLeeway > 0;
        if (safeL != safeR)
            return safeL; // safe is always better than unsafe

        if (safeL)
        {
            if (pixL.Priority != pixR.Priority)
                return pixL.Priority > pixR.Priority; // higher prio is better

            // TODO: use max-leeway instead?..
            return IsLeftCloserToTarget(ref nodeL, ref nodeR);
        }

        var improveL = pixL.MaxG > _startMaxG && nodeL.PathMinG <= _startMaxG;
        var improveR = pixR.MaxG > _startMaxG && nodeR.PathMinG <= _startMaxG;
        if (improveL != improveR)
            return improveL; // node that improves initial state (leaves some danger zone without entering even more dangerous ones) is better than a node that doesn't

        var semiSafeL = nodeL.PathLeeway > 0;
        var semiSafeR = nodeR.PathLeeway > 0;
        if (semiSafeL != semiSafeR)
            return semiSafeL; // semi-safe (no immediate risk) is better than path going through danger

        var ultimatelySafeL = pixL.MaxG == float.MaxValue;
        var ultimatelySafeR = pixR.MaxG == float.MaxValue;
        if (ultimatelySafeL != ultimatelySafeR)
            return ultimatelySafeL; // unsafe but ultimately leading to safety is better than dying

        // TODO: should we use leeway here or distance?..
        //return nodeL.PathLeeway > nodeR.PathLeeway;
        return IsLeftCloserToTarget(ref nodeL, ref nodeR);
    }

    public Vector2 CalculateEnterOffset(int fromX, int fromY, Vector2 fromOff, int toX, int toY)
    {
        var x = fromX == toX ? fromOff.X : fromX < toX ? -MaxNeighbourOffset : +MaxNeighbourOffset;
        var y = fromY == toY ? fromOff.Y : fromY < toY ? -MaxNeighbourOffset : +MaxNeighbourOffset;
        return new(x, y);
    }

    public float LineOfSight(int x1, int y1, Vector2 off1, int x2, int y2, out Vector2 off2, out float length)
    {
        float minLeeway = float.MaxValue;
        int dx = x2 - x1;
        int dy = y2 - y1;
        int sx = dx > 0 ? 1 : -1;
        int sy = dy > 0 ? 1 : -1;
        var hsx = 0.5f * sx;
        var hsy = 0.5f * sy;

        off2 = CalculateEnterOffset(x1, y1, off1, x2, y2);
        var ab = new Vector2(dx + off2.X - off1.X, dy + off2.Y - off1.Y);
        length = ab.Length();
        if (length < 0.01f)
            return minLeeway; // zero length would create NaN's

        ab /= length; // note that ab.X == 0 does not imply dx == 0 (could be crossing the border) or vice versa (could have a small movement along axis in any direction without crossing cell boundary)
        var invx = ab.X != 0 ? 1 / ab.X : float.MaxValue; // either can be infinite, but not both; we want to avoid actual infinities here, because 0*inf = NaN (and we'd rather have it be 0 in this case)
        var invy = ab.Y != 0 ? 1 / ab.Y : float.MaxValue;

        var curG = _nodes[_map.GridToIndex(x1, y1)].GScore;
        while (x1 != x2 || y1 != y2)
        {
            var tx = (hsx - off1.X) * invx; // if negative, we'll never intersect it
            var ty = (hsy - off1.Y) * invy;
            if (tx < 0 || x1 == x2)
                tx = float.MaxValue;
            if (ty < 0 || y1 == y2)
                ty = float.MaxValue;

            // note: we need the clamp to handle corners properly
            if (tx < ty)
            {
                x1 += sx;
                off1.X = -hsx;
                off1.Y = Math.Clamp(off1.Y + tx * ab.Y, -0.5f, +0.5f);
                curG += tx * _deltaGSide;
            }
            else
            {
                y1 += sy;
                off1.Y = -hsy;
                off1.X = Math.Clamp(off1.X + ty * ab.X, -0.5f, +0.5f);
                curG += ty * _deltaGSide;
            }

            var curLeeway = _map[x1, y1].MaxG - curG;
            if (curLeeway < 0)
                return curLeeway;
            else if (curLeeway < minLeeway)
                minLeeway = curLeeway;
        }
        return minLeeway;
    }

    private void VisitNeighbour(int parentX, int parentY, int parentIndex, int nodeX, int nodeY, int nodeIndex, float deltaGrid)
    {
        ref var destNode = ref _nodes[nodeIndex];
        if (destNode.OpenHeapIndex < 0 && destNode.PathLeeway > 0)
            return; // in closed list already, and the previous path was decent - TODO: is it possible to visit again with lower cost?..

        if (destNode.OpenHeapIndex == 0)
            destNode.HScore = HeuristicDistance(nodeX, nodeY); // if this is the first time we visit this node, initialize h-score, we're going to add it to open list

        // note: we may visit the node even if it's blocked (eg we might be moving outside imminent aoe)
        ref var destPix = ref _map.Pixels[nodeIndex];
        var altNode = new Node()
        {
            GScore = _nodes[parentIndex].GScore + _deltaGSide * deltaGrid,
            HScore = destNode.HScore,
            ParentX = parentX,
            ParentY = parentY,
            OpenHeapIndex = destNode.OpenHeapIndex,
            EnterOffset = CalculateEnterOffset(parentX, parentY, _nodes[parentIndex].EnterOffset, nodeX, nodeY),
        };
        altNode.PathLeeway = MathF.Min(_nodes[parentIndex].PathLeeway, destPix.MaxG - altNode.GScore);
        //if (_nodes[parentIndex].PathLeeway > 0 && nodeLeeway <= 0)
        //    return; // don't try to enter danger from safety

        // check LoS from grandparent
        int grandParentX = _nodes[parentIndex].ParentX;
        int grandParentY = _nodes[parentIndex].ParentY;
        var grandParentIndex = _map.GridToIndex(grandParentX, grandParentY);
        if (_nodes[grandParentIndex].PathMinG >= _nodes[parentIndex].PathMinG)
        {
            var losLeeway = LineOfSight(grandParentX, grandParentY, _nodes[grandParentIndex].EnterOffset, nodeX, nodeY, out var grandParentOffset, out var grandParentDist);
            losLeeway = MathF.Min(_nodes[grandParentIndex].PathLeeway, losLeeway);
            if (losLeeway >= altNode.PathLeeway)
            {
                parentIndex = grandParentIndex;
                altNode.GScore = _nodes[parentIndex].GScore + _deltaGSide * grandParentDist;
                altNode.ParentX = grandParentX;
                altNode.ParentY = grandParentY;
                altNode.PathLeeway = losLeeway;
                altNode.EnterOffset = grandParentOffset;
            }
        }
        altNode.PathMinG = MathF.Min(_nodes[parentIndex].PathMinG, destPix.MaxG);

        //if (destNode.OpenHeapIndex < 0 && destNode.PathLeeway >= nodeLeeway)
        //    return; // node was already visited before, old path was bad, but new path is even worse
        //if (destNode.OpenHeapIndex > 0 && destNode.PathLeeway > 0 && nodeLeeway <= 0)
        //    return; // node is already in scheduled for visit with a reasonable path, and new path is bad (even if it's shorter)

        if (destNode.OpenHeapIndex == 0 || IsLeftBetter(ref altNode, ref destPix, ref destNode, ref destPix))
        {
            destNode = altNode;
            AddToOpen(nodeIndex);
        }
    }

    private float HeuristicDistance(int x, int y) => Math.Max(0, (_map.GridToWorld(x, y, 0.5f, 0.5f) - _goalPos).Length() - _goalRadius);

    private void AddToOpen(int nodeIndex)
    {
        if (_nodes[nodeIndex].OpenHeapIndex <= 0)
        {
            _openList.Add(nodeIndex);
            _nodes[nodeIndex].OpenHeapIndex = _openList.Count;
        }
        // update location
        PercolateUp(_nodes[nodeIndex].OpenHeapIndex - 1);
    }

    // remove first (minimal) node from open heap and mark as closed
    private int PopMinOpen()
    {
        int nodeIndex = _openList[0];
        _openList[0] = _openList[^1];
        _nodes[nodeIndex].OpenHeapIndex = -1;
        _openList.RemoveAt(_openList.Count - 1);
        if (_openList.Count > 0)
        {
            _nodes[_openList[0]].OpenHeapIndex = 1;
            PercolateDown(0);
        }
        return nodeIndex;
    }

    private void PercolateUp(int heapIndex)
    {
        int nodeIndex = _openList[heapIndex];
        int parent = (heapIndex - 1) >> 1;
        while (heapIndex > 0 && HeapLess(nodeIndex, _openList[parent]))
        {
            _openList[heapIndex] = _openList[parent];
            _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
            heapIndex = parent;
            parent = (heapIndex - 1) >> 1;
        }
        _openList[heapIndex] = nodeIndex;
        _nodes[nodeIndex].OpenHeapIndex = heapIndex + 1;
    }

    private void PercolateDown(int heapIndex)
    {
        int nodeIndex = _openList[heapIndex];
        int maxSize = _openList.Count;
        while (true)
        {
            int child1 = (heapIndex << 1) + 1;
            if (child1 >= maxSize)
                break;
            int child2 = child1 + 1;
            if (child2 == maxSize || HeapLess(_openList[child1], _openList[child2]))
            {
                if (HeapLess(_openList[child1], nodeIndex))
                {
                    _openList[heapIndex] = _openList[child1];
                    _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
                    heapIndex = child1;
                }
                else
                {
                    break;
                }
            }
            else if (HeapLess(_openList[child2], nodeIndex))
            {
                _openList[heapIndex] = _openList[child2];
                _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
                heapIndex = child2;
            }
            else
            {
                break;
            }
        }
        _openList[heapIndex] = nodeIndex;
        _nodes[nodeIndex].OpenHeapIndex = heapIndex + 1;
    }

    private bool IsLeftCloserToTarget(ref Node nodeL, ref Node nodeR)
    {
        var fl = nodeL.FScore;
        var fr = nodeR.FScore;
        if (fl + 0.00001f < fr)
            return true;
        else if (fr + 0.00001f < fl)
            return false;
        else
            return nodeL.GScore > nodeR.GScore; // tie-break towards larger g-values
    }

    private bool HeapLess(int nodeIndexLeft, int nodeIndexRight) => IsLeftBetter(ref _nodes[nodeIndexLeft], ref _map.Pixels[nodeIndexLeft], ref _nodes[nodeIndexRight], ref _map.Pixels[nodeIndexRight]);
}
