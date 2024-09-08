using System.Reflection.Metadata;

namespace BossMod.Pathfinding;

public class ThetaStar
{
    public enum Score
    {
        JustBad, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe and has same or lower max-g than starting cell
        UltimatelyBetter, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe but has larger max-g than starting cell
        UltimatelySafe, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), however destination is safe
        UnsafeAsStart, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is <= 0)
        SemiSafeAsStart, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is > 0)
        UnsafeImprove, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is at least better than start
        SemiSafeImprove, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe but better than start
        Safe, // the path reaches safe cell and is fully safe (no cell along the path has negative leeway) (starting cell will have this score if it's safe)
        SafeMaxPrio, // the path reaches safe cell with max goal priority and is fully safe (no cell along the path has negative leeway)
    }

    public struct Node
    {
        public float GScore;
        public float HScore;
        public int ParentX;
        public int ParentY;
        public int OpenHeapIndex; // -1 if in closed list, 0 if not in any lists, otherwise (index+1)
        public float PathLeeway; // min diff along path between node's g-value and cell's g-value
        public float PathMinG; // minimum 'max g' value along path
        public Score Score;
        public Vector2 EnterOffset; // from cell center; up to +-0.5

        public readonly float FScore => GScore + HScore;
    }

    private Map _map = new();
    private WPos _goalPos;
    private float _goalRadius;
    private Node[] _nodes = [];
    private readonly List<int> _openList = [];
    private float _deltaGSide;
    private int _startNodeIndex;
    private float _startMaxG;
    private Score _startScore;

    private int _bestIndex; // node with best score
    private int _fallbackIndex; // best 'fallback' node: node that we don't necessarily want to go to, but might want to move closer to it (to the parent)

    // statistics
    public int NumSteps { get; private set; }
    public int NumReopens { get; private set; }

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
        _startNodeIndex = _bestIndex = _fallbackIndex = _map.GridToIndex(start.x, start.y);
        ref var startPix = ref _map.Pixels[_startNodeIndex];
        _startMaxG = startPix.MaxG;
        _startScore = CalculateScore(ref startPix, _startMaxG, _startMaxG);
        NumSteps = NumReopens = 0;

        startFrac.X -= start.x + 0.5f;
        startFrac.Y -= start.y + 0.5f;
        ref var startNode = ref _nodes[_startNodeIndex];
        startNode = new()
        {
            GScore = 0,
            HScore = HeuristicDistance(start.x, start.y),
            ParentX = start.x, // start's parent is self
            ParentY = start.y,
            PathLeeway = _startMaxG,
            PathMinG = _startMaxG,
            Score = _startScore,
            EnterOffset = startFrac,
        };
        AddToOpen(_startNodeIndex);
    }

    // returns whether search is to be terminated; on success, first node of the open list would contain found goal
    public bool ExecuteStep()
    {
        if (_openList.Count == 0 /*|| _nodes[_openList[0]].HScore <= 0*/)
            return false;

        ++NumSteps;
        int nextNodeIndex = PopMinOpen();
        var (nextNodeX, nextNodeY) = _map.IndexToGrid(nextNodeIndex);
        ref var nextNode = ref _nodes[nextNodeIndex];

        // update our best indices
        if (HeapLess(nextNodeIndex, _bestIndex))
            _bestIndex = nextNodeIndex;
        if (nextNode.Score == Score.UltimatelySafe && (_fallbackIndex == _startNodeIndex || HeapLess(nextNodeIndex, _fallbackIndex)))
            _fallbackIndex = nextNodeIndex;

        if (nextNodeY > 0)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY - 1, nextNodeIndex - _map.Width, CenterToNeighbour + nextNode.EnterOffset.Y);
        if (nextNodeX > 0)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY, nextNodeIndex - 1, CenterToNeighbour + nextNode.EnterOffset.X);
        if (nextNodeX < _map.Width - 1)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY, nextNodeIndex + 1, CenterToNeighbour - nextNode.EnterOffset.X);
        if (nextNodeY < _map.Height - 1)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY + 1, nextNodeIndex + _map.Width, CenterToNeighbour - nextNode.EnterOffset.Y);
        return true;
    }

    public int Execute()
    {
        while (_nodes[_bestIndex].Score < Score.SafeMaxPrio && ExecuteStep())
            ;
        return BestIndex();
    }

    public int BestIndex()
    {
        if (_nodes[_bestIndex].Score > _startScore)
            return _bestIndex; // we've found something better than start

        if (_fallbackIndex != _startNodeIndex)
        {
            // find first parent of best-among-worst that is at least as good as start
            var destIndex = _fallbackIndex;
            var parentIndex = _map.GridToIndex(_nodes[destIndex].ParentX, _nodes[destIndex].ParentY);
            while (_nodes[parentIndex].Score < _startScore)
            {
                destIndex = parentIndex;
                parentIndex = _map.GridToIndex(_nodes[destIndex].ParentX, _nodes[destIndex].ParentY);
            }

            // TODO: this is very similar to LineOfSight, try to unify implementations...
            ref var startNode = ref _nodes[parentIndex];
            ref var destNode = ref _nodes[destIndex];
            var (x2, y2) = _map.IndexToGrid(destIndex);
            var (x1, y1) = (destNode.ParentX, destNode.ParentY);
            int dx = x2 - x1;
            int dy = y2 - y1;
            int sx = dx > 0 ? 1 : -1;
            int sy = dy > 0 ? 1 : -1;
            var hsx = 0.5f * sx;
            var hsy = 0.5f * sy;

            var ab = new Vector2(dx + destNode.EnterOffset.X - startNode.EnterOffset.X, dy + destNode.EnterOffset.Y - startNode.EnterOffset.Y);
            ab /= ab.Length();
            var invx = ab.X != 0 ? 1 / ab.X : float.MaxValue; // either can be infinite, but not both; we want to avoid actual infinities here, because 0*inf = NaN (and we'd rather have it be 0 in this case)
            var invy = ab.Y != 0 ? 1 / ab.Y : float.MaxValue;
            var off1 = startNode.EnterOffset;
            while (x1 != x2 || y1 != y2)
            {
                var tx = (hsx - off1.X) * invx; // if negative, we'll never intersect it
                var ty = (hsy - off1.Y) * invy;
                if (tx < 0 || x1 == x2)
                    tx = float.MaxValue;
                if (ty < 0 || y1 == y2)
                    ty = float.MaxValue;

                if (tx < ty)
                {
                    x1 += sx;
                    off1.X = -hsx;
                    off1.Y = Math.Clamp(off1.Y + tx * ab.Y, -0.5f, +0.5f);
                }
                else
                {
                    y1 += sy;
                    off1.Y = -hsy;
                    off1.X = Math.Clamp(off1.X + ty * ab.X, -0.5f, +0.5f);
                }

                if (_nodes[_map.GridToIndex(x1, y1)].Score < _startScore)
                {
                    var (x, y) = tx < ty ? (x1 - sx, y1) : (x1, y1 - sy);
                    return _map.GridToIndex(x, y);
                }
            }
        }

        return _bestIndex;
    }

    public Score CalculateScore(ref Map.Pixel pix, float pathMinG, float pathLeeway)
    {
        var destSafe = pix.MaxG == float.MaxValue;
        var pathSafe = pathLeeway > 0;
        var destBetter = pix.MaxG > _startMaxG;
        if (destSafe && pathSafe)
            return pix.Priority == _map.MaxPriority ? Score.SafeMaxPrio : Score.Safe;

        if (pathMinG == _startMaxG) // TODO: some small threshold? should be solved by preprocessing...
            return pathSafe
                ? (destBetter ? Score.SemiSafeImprove : Score.SemiSafeAsStart) // note: if pix.MaxG is < _startMaxG, then PathMinG will be < too
                : (destBetter ? Score.UnsafeImprove : Score.UnsafeAsStart);

        return destSafe ? Score.UltimatelySafe : destBetter ? Score.UltimatelyBetter : Score.JustBad;
    }

    // return a 'score' difference: 0 if identical, -1 if left is somewhat better, -2 if left is significantly better, +1/+2 when right is better
    public int CompareNodeScores(ref Node nodeL, ref Node nodeR)
    {
        if (nodeL.Score != nodeR.Score)
            return nodeL.Score > nodeR.Score ? -2 : +2;
        // TODO: should we use leeway here or distance?..
        //return nodeL.PathLeeway > nodeR.PathLeeway;
        return CompareFScores(ref nodeL, ref nodeR);
    }

    public Vector2 CalculateEnterOffset(int fromX, int fromY, Vector2 fromOff, int toX, int toY)
    {
        var x = fromX == toX ? fromOff.X : fromX < toX ? -MaxNeighbourOffset : +MaxNeighbourOffset;
        var y = fromY == toY ? fromOff.Y : fromY < toY ? -MaxNeighbourOffset : +MaxNeighbourOffset;
        return new(x, y);
    }

    public float LineOfSight(int x1, int y1, Vector2 off1, int x2, int y2, out Vector2 off2, out float length, out float minG)
    {
        var startNodeIndex = _map.GridToIndex(x1, y1);
        ref var startNode = ref _nodes[startNodeIndex];
        float minLeeway = startNode.PathLeeway;
        minG = startNode.PathMinG;

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

        var curG = startNode.GScore;
        var prevPixMaxG = _map.Pixels[startNodeIndex].MaxG;
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

            var pixG = _map.Pixels[_map.GridToIndex(x1, y1)].MaxG;
            minLeeway = Math.Min(minLeeway, Math.Min(pixG, prevPixMaxG) - curG);
            minG = Math.Min(minG, pixG);
            prevPixMaxG = pixG;
        }
        return minLeeway;
    }

    private void VisitNeighbour(int parentX, int parentY, int parentIndex, int nodeX, int nodeY, int nodeIndex, float deltaGrid)
    {
        ref var destNode = ref _nodes[nodeIndex];
        if (destNode.OpenHeapIndex < 0 && destNode.Score >= Score.SemiSafeAsStart)
            return; // in closed list already, and the previous path was decent - TODO: is it possible to visit again with lower cost?..

        if (destNode.OpenHeapIndex == 0)
            destNode.HScore = HeuristicDistance(nodeX, nodeY); // if this is the first time we visit this node, initialize h-score, we're going to add it to open list

        // note: we may visit the node even if it's blocked (eg we might be moving outside imminent aoe)
        ref var destPix = ref _map.Pixels[nodeIndex];
        var deltaG = _deltaGSide * deltaGrid;
        var destGScore = _nodes[parentIndex].GScore + deltaG;
        var destLeeway = MathF.Min(_nodes[parentIndex].PathLeeway, Math.Min(destPix.MaxG, _map.Pixels[parentIndex].MaxG) - destGScore);
        var destMinG = MathF.Min(_nodes[parentIndex].PathMinG, destPix.MaxG);
        var altNode = new Node()
        {
            GScore = destGScore,
            HScore = destNode.HScore,
            ParentX = parentX,
            ParentY = parentY,
            OpenHeapIndex = destNode.OpenHeapIndex,
            PathLeeway = destLeeway,
            PathMinG = destMinG,
            Score = CalculateScore(ref destPix, destMinG, destLeeway),
            EnterOffset = CalculateEnterOffset(parentX, parentY, _nodes[parentIndex].EnterOffset, nodeX, nodeY),
        };
        //if (_nodes[parentIndex].PathLeeway > 0 && nodeLeeway <= 0)
        //    return; // don't try to enter danger from safety

        // check LoS from grandparent
        int grandParentX = _nodes[parentIndex].ParentX;
        int grandParentY = _nodes[parentIndex].ParentY;
        var grandParentIndex = _map.GridToIndex(grandParentX, grandParentY);
        if (_nodes[grandParentIndex].PathMinG >= _nodes[parentIndex].PathMinG)
        {
            var losLeeway = LineOfSight(grandParentX, grandParentY, _nodes[grandParentIndex].EnterOffset, nodeX, nodeY, out var grandParentOffset, out var grandParentDist, out var losMinG);
            var losScore = CalculateScore(ref destPix, losMinG, losLeeway);
            // accept direct route either if score is better, score is same and leeway is better, or score is safe and leeway is good enough
            if (losScore > altNode.Score || losScore == altNode.Score && losLeeway >= (losScore >= Score.Safe ? 0 : altNode.PathLeeway))
            {
                parentIndex = grandParentIndex;
                altNode.GScore = _nodes[parentIndex].GScore + _deltaGSide * grandParentDist;
                altNode.ParentX = grandParentX;
                altNode.ParentY = grandParentY;
                altNode.PathLeeway = losLeeway;
                altNode.PathMinG = losMinG;
                altNode.Score = losScore;
                altNode.EnterOffset = grandParentOffset;
            }
        }

        //if (destNode.OpenHeapIndex < 0 && destNode.PathLeeway >= nodeLeeway)
        //    return; // node was already visited before, old path was bad, but new path is even worse
        //if (destNode.OpenHeapIndex > 0 && destNode.PathLeeway > 0 && nodeLeeway <= 0)
        //    return; // node is already in scheduled for visit with a reasonable path, and new path is bad (even if it's shorter)

        // - always visit nodes that were never visited before
        // - revisit nodes only if new path is much better
        // - update nodes scheduled for visit if new path is even somewhat better
        var visit = destNode.OpenHeapIndex == 0 || CompareNodeScores(ref altNode, ref destNode) < (destNode.OpenHeapIndex < 0 ? -1 : 0);
        if (visit)
        {
            if (destNode.OpenHeapIndex < 0)
                ++NumReopens;
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

    private int CompareFScores(ref Node nodeL, ref Node nodeR)
    {
        var fl = nodeL.FScore;
        var fr = nodeR.FScore;
        if (fl + 0.00001f < fr)
            return -1;
        else if (fr + 0.00001f < fl)
            return +1;
        else if (nodeL.GScore != nodeR.GScore)
            return nodeL.GScore > nodeR.GScore ? -1 : 1; // tie-break towards larger g-values
        else
            return 0;
    }

    private bool HeapLess(int nodeIndexLeft, int nodeIndexRight) => CompareNodeScores(ref _nodes[nodeIndexLeft], ref _nodes[nodeIndexRight]) < 0;
}
