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
        public float PathLeeway;
        public Vector2 EnterOffset; // from cell center; up to +-0.5
    }

    private Map _map = new();
    private WPos _goalPos;
    private float _goalRadius;
    private Node[] _nodes = [];
    private readonly List<int> _openList = [];
    private float _deltaGSide;

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
        _nodes[startIndex] = new()
        {
            GScore = 0,
            HScore = HeuristicDistance(start.x, start.y),
            ParentX = start.x, // start's parent is self
            ParentY = start.y,
            PathLeeway = _map.Pixels[startIndex].MaxG, // min diff along path between node's g-value and cell's g-value
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
        if (IsBetter(nextNodeIndex, BestIndex))
            BestIndex = nextNodeIndex;

        var startOff = _nodes[nextNodeIndex].EnterOffset;
        if (nextNodeY > 0)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY - 1, nextNodeIndex - _map.Width, new(startOff.X, +MaxNeighbourOffset), CenterToNeighbour + startOff.Y);
        if (nextNodeX > 0)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY, nextNodeIndex - 1, new(+MaxNeighbourOffset, startOff.Y), CenterToNeighbour + startOff.X);
        if (nextNodeX < _map.Width - 1)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY, nextNodeIndex + 1, new(-MaxNeighbourOffset, startOff.Y), CenterToNeighbour - startOff.X);
        if (nextNodeY < _map.Height - 1)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY + 1, nextNodeIndex + _map.Width, new(startOff.X, -MaxNeighbourOffset), CenterToNeighbour - startOff.Y);
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

    public bool IsBetter(int nodeIndex, int compIndex)
    {
        ref var testNode = ref _nodes[nodeIndex];
        ref var compNode = ref _nodes[compIndex];
        ref var testPix = ref _map.Pixels[nodeIndex];
        ref var compPix = ref _map.Pixels[compIndex];

        var testSafe = testPix.MaxG == float.MaxValue && testNode.PathLeeway > 0;
        var compSafe = compPix.MaxG == float.MaxValue && compNode.PathLeeway > 0;
        if (testSafe != compSafe)
            return testSafe; // safe is always better than unsafe

        if (testSafe)
        {
            return testPix.Priority != compPix.Priority
                ? testPix.Priority > compPix.Priority // higher prio is better
                : testNode.GScore + testNode.HScore < compNode.GScore + compNode.HScore; // TODO: use max-leeway as tiebreaker instead?..
        }

        var testSemiSafe = testNode.PathLeeway > 0;
        var compSemiSafe = compNode.PathLeeway > 0;
        if (testSemiSafe != compSemiSafe)
            return testSemiSafe; // semi-safe (no immediate risk) is always better than path going through danger

        var testGtfo = testPix.MaxG == float.MaxValue;
        var compGtfo = compPix.MaxG == float.MaxValue;
        if (testGtfo != compGtfo)
            return testGtfo; // unsafe but ultimately leading to safety is better than dying

        // both for semi-safe and dangerous paths max leeway is most important
        return testNode.PathLeeway > compNode.PathLeeway;
    }

    public float LineOfSight(int x1, int y1, Vector2 off1, int x2, int y2, Vector2 off2, out float length)
    {
        float minLeeway = float.MaxValue;
        int dx = x2 - x1;
        int dy = y2 - y1;
        int sx = dx > 0 ? 1 : -1;
        int sy = dy > 0 ? 1 : -1;
        var hsx = 0.5f * sx;
        var hsy = 0.5f * sy;

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

    private void VisitNeighbour(int parentX, int parentY, int parentIndex, int nodeX, int nodeY, int nodeIndex, Vector2 enterOffset, float deltaGrid)
    {
        ref var destNode = ref _nodes[nodeIndex];
        if (destNode.OpenHeapIndex < 0 && destNode.PathLeeway > 0)
            return; // in closed list already, and the previous path was decent - TODO: is it possible to visit again with lower cost?..

        var nodeG = _nodes[parentIndex].GScore + _deltaGSide * deltaGrid;
        var nodeLeeway = _map.Pixels[nodeIndex].MaxG - nodeG; // note: we visit the node even if it's blocked (eg we might be moving outside imminent aoe)
        nodeLeeway = MathF.Min(_nodes[parentIndex].PathLeeway, nodeLeeway);
        if (_nodes[parentIndex].PathLeeway > 0 && nodeLeeway <= 0)
            return; // don't try to enter danger from safety

        // check LoS from grandparent
        int grandParentX = _nodes[parentIndex].ParentX;
        int grandParentY = _nodes[parentIndex].ParentY;
        var grandParentIndex = _map.GridToIndex(grandParentX, grandParentY);
        var losLeeway = LineOfSight(grandParentX, grandParentY, _nodes[grandParentIndex].EnterOffset, nodeX, nodeY, enterOffset, out var grandParentDist);
        losLeeway = MathF.Min(_nodes[grandParentIndex].PathLeeway, losLeeway);
        if (losLeeway >= nodeLeeway)
        {
            parentX = grandParentX;
            parentY = grandParentY;
            parentIndex = grandParentIndex;
            nodeG = _nodes[parentIndex].GScore + _deltaGSide * grandParentDist;
            nodeLeeway = losLeeway;
        }

        if (destNode.OpenHeapIndex < 0 && destNode.PathLeeway >= nodeLeeway)
            return; // node was already visited before, old path was bad, but new path is even worse
        if (destNode.OpenHeapIndex > 0 && destNode.PathLeeway > 0 && nodeLeeway <= 0)
            return; // node is already in scheduled for visit with a reasonable path, and new path is bad (even if it's shorter)

        if (destNode.OpenHeapIndex <= 0 || nodeG + 0.00001f < destNode.GScore)
        {
            destNode.GScore = nodeG;
            destNode.HScore = HeuristicDistance(nodeX, nodeY);
            destNode.ParentX = parentX;
            destNode.ParentY = parentY;
            destNode.PathLeeway = nodeLeeway;
            destNode.EnterOffset = enterOffset;
            AddToOpen(nodeIndex);
        }
        else if (destNode.OpenHeapIndex > 0 && destNode.PathLeeway <= 0 && nodeLeeway > destNode.PathLeeway)
        {
            // we have a worse path, but with better leeway
            destNode.GScore = nodeG;
            destNode.HScore = HeuristicDistance(nodeX, nodeY);
            destNode.ParentX = parentX;
            destNode.ParentY = parentY;
            destNode.PathLeeway = nodeLeeway;
            destNode.EnterOffset = enterOffset;
            PercolateDown(destNode.OpenHeapIndex - 1);
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

    private bool HeapLess(int nodeIndexLeft, int nodeIndexRight)
    {
        ref var nodeL = ref _nodes[nodeIndexLeft];
        ref var nodeR = ref _nodes[nodeIndexRight];
        var fl = nodeL.GScore + nodeL.HScore;
        var fr = nodeR.GScore + nodeR.HScore;
        if (fl + 0.00001f < fr)
            return true;
        else if (fr + 0.00001f < fl)
            return false;
        else
            return nodeL.GScore > nodeR.GScore; // tie-break towards larger g-values
    }
}
